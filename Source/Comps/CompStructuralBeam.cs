using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_StructuralBeam : CompProperties
    {
        public CompProperties_StructuralBeam()
        {
            compClass = typeof(CompStructuralBeam);
        }
    }
    public class CompStructuralBeam : ThingComp
    {
        private static HashSet<Thing> collapsingBeams = new HashSet<Thing>();
        public override void PostDestroy(DestroyMode mode, Map map)
        {
            base.PostDestroy(mode, map);
            if (!collapsingBeams.Contains(parent))
            {
                collapsingBeams.Clear();
                collapsingBeams.Add(parent);
                TriggerAdjacentCollapse(map, mode);
            }
        }

        private void TriggerAdjacentCollapse(Map map, DestroyMode mode)
        {
            if (map == null) return;

            List<IntVec3> adjacentCells = GenAdjFast.AdjacentCells8Way(parent.Position);
            for (int i = 0; i < adjacentCells.Count; i++)
            {
                IntVec3 cell = adjacentCells[i];
                if (!cell.InBounds(map)) continue;

                List<Thing> things = cell.GetThingList(map);
                for (int j = things.Count - 1; j >= 0; j--)
                {
                    if (things[j] is Building building && building.def == parent.def && !building.Destroyed && collapsingBeams.Add(building))
                    {
                        building.Destroy(mode);
                    }
                }
            }
        }
    }
}
