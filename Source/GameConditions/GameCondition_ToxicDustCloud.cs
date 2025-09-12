using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class GameCondition_ToxicDustCloud : GameCondition_ForceWeather
    {
        private const float CorpseRotProgressAdd = 3000f;

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            if (Find.TickManager.TicksGame % 3451 == 0)
            {
                for (int i = 0; i < affectedMaps.Count; i++)
                {
                    DoPawnsToxicDamage(affectedMaps[i]);
                }
            }
            base.GameConditionTick();
        }

        private void DoPawnsToxicDamage(Map map)
        {
            IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                if (!allPawnsSpawned[i].kindDef.immuneToGameConditionEffects && !allPawnsSpawned[i].Position.Roofed(map))
                {
                    ToxicUtility.DoAirbornePawnToxicDamage(allPawnsSpawned[i], 2f);
                }
            }
        }

        public override void DoCellSteadyEffects(IntVec3 c, Map map)
        {
            if (c.Roofed(map))
            {
                return;
            }
            List<Thing> thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing is Plant)
                {
                    if (thing.def.plant.dieFromToxicFallout && Rand.Value < 0.1f)
                    {
                        if (thing is Plant { IsCrop: not false } plant && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfPoison-" + thing.def.defName, 240f))
                        {
                            Messages.Message("MessagePlantDiedOfPoison".Translate(plant.GetCustomLabelNoCount(includeHp: false)), new TargetInfo(plant.Position, map), MessageTypeDefOf.NegativeEvent);
                        }
                        thing.Kill();
                    }
                }
                else if (thing.def.category == ThingCategory.Item)
                {
                    CompRottable compRottable = thing.TryGetComp<CompRottable>();
                    if (compRottable != null && (int)compRottable.Stage < 2)
                    {
                        compRottable.RotProgress += CorpseRotProgressAdd;
                    }
                }
            }
        }
    }
}
