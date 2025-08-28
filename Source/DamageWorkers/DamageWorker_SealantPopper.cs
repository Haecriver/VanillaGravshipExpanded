using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class DamageWorker_SealantPopper : DamageWorker
    {
        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            SpawnSealantGoopInCell(explosion, c);
        }

        private void SpawnSealantGoopInCell(Explosion explosion, IntVec3 cell)
        {
            Map map = explosion.Map;
            if (map == null)
                return;

            if (cell.InBounds(map) && CanSpawnSealantGoopAt(cell, map))
            {
                var things = cell.GetThingList(map).Where(t => t.def.category == ThingCategory.Item).ToList();
                foreach (var thing in things)
                {
                    thing.DeSpawn();
                }
                Thing goop = ThingMaker.MakeThing(VGEDefOf.VGE_SealantGoop);
                GenPlace.TryPlaceThing(goop, cell, map, ThingPlaceMode.Direct);
                foreach (var thing in things)
                {
                    GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
                }
            }
        }

        private bool CanSpawnSealantGoopAt(IntVec3 c, Map map)
        {
            if (!GenSpawn.CanSpawnAt(VGEDefOf.VGE_SealantGoop, c, map))
                return false;
            List<Thing> thingList = c.GetThingList(map);
            foreach (Thing thing in thingList)
            {
                if (thing.def.Fillage == FillCategory.Full)
                    return false;
                if (thing.def.preventDroppingThingsOn)
                    return false;
                if (thing.def.IsDoor)
                    return false;
                if (thing is Building_WorkTable)
                    return false;
                if (thing.def.category == ThingCategory.Building)
                    return false;
            }

            return true;
        }
    }
}
