using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_RichDepositDetected : LaunchBoonWorker
    {
        public LaunchBoonWorker_RichDepositDetected(LaunchBoonDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            Map map = gravship.Engine.Map;
            return map.Biome.hasBedrock;
        }

        public override void ApplyBoon(Gravship gravship)
        {
            Map map = gravship.Engine.Map;
            if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10,
                (IntVec3 x) => CanPlaceDepositAt(x, map), map, out var result))
            {
                SendStandardLetter(gravship.Engine, null, gravship.Engine);
                return;
            }
            ThingDef resource = ChooseValuableResource();
            int numCells = Mathf.CeilToInt(resource.deepLumpSizeRange.RandomInRange * 3);
            foreach (IntVec3 item in GridShapeMaker.IrregularLump(result, map, numCells))
            {
                if (CanPlaceDepositAt(item, map) && !item.InNoBuildEdgeArea(map))
                {
                    map.deepResourceGrid.SetAt(item, resource, resource.deepCountPerCell);
                }
            }
            SendStandardLetter(gravship.Engine, null, new LookTargets(result, map));
        }

        private bool CanPlaceDepositAt(IntVec3 pos, Map map)
        {
            int index = CellIndicesUtility.CellToIndex(pos, map.Size.x);
            TerrainDef terrainDef = map.terrainGrid.BaseTerrainAt(pos);
            if ((terrainDef != null && terrainDef.IsWater && terrainDef.passability == Traversability.Impassable) || 
                !pos.GetAffordances(map).Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded))
            {
                return false;
            }
            return !map.deepResourceGrid.GetCellBool(index);
        }

        private ThingDef ChooseValuableResource()
        {
            return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight((ThingDef def) => def.deepCommonality);
        }
    }
}