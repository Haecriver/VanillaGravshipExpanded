using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(SectionLayer_SubstructureProps), "ShouldDrawPropsOn")]
    public static class SectionLayer_SubstructureProps_ShouldDrawPropsOn_Patch
    {
        public static bool doVanilla = false;

        public static bool Prefix(IntVec3 c, TerrainGrid terrGrid, out SectionLayer_SubstructureProps.EdgeDirections edgeEdgeDirections, out SectionLayer_SubstructureProps.CornerDirections cornerDirections, ref bool __result)
        {
            edgeEdgeDirections = SectionLayer_SubstructureProps.EdgeDirections.None;
            cornerDirections = SectionLayer_SubstructureProps.CornerDirections.None;

            if (doVanilla)
            {
                return true;
            }

            var foundationDef = terrGrid.FoundationAt(c);
            var topDef = terrGrid.TerrainAt(c);
            var terrain = foundationDef ?? topDef;

            if (foundationDef == VGEDefOf.VGE_GravshipSubscaffold)
            {
                __result = false;
                return false;
            }

            var ext = terrain?.GetModExtension<SubstructureEdgeGraphicsExtension>();
            if (ext != null && ext.renderAsSubstructure)
            {
                CalculateSubstructureDirections(terrain, c, terrGrid, out edgeEdgeDirections, out cornerDirections);
                if (edgeEdgeDirections == SectionLayer_SubstructureProps.EdgeDirections.None)
                {
                    __result = cornerDirections != SectionLayer_SubstructureProps.CornerDirections.None;
                }
                else
                {
                    __result = true;
                }
                return false;
            }

            if (terrain == VGEDefOf.VGE_MechanoidSubstructure || ext != null)
            {
                __result = false;
                return false;
            }

            return true;
        }

        private static bool IsEquivalentSubstructure(TerrainDef terrain, TerrainDef other)
        {
            if (terrain == null || other == null) return false;
            if (terrain == other) return true;

            var isStandard1 = terrain == TerrainDefOf.Substructure || (terrain.GetModExtension<SubstructureEdgeGraphicsExtension>()?.renderAsSubstructure == true);
            var isStandard2 = other == TerrainDefOf.Substructure || (other.GetModExtension<SubstructureEdgeGraphicsExtension>()?.renderAsSubstructure == true);

            return isStandard1 && isStandard2;
        }

        private static void CalculateSubstructureDirections(TerrainDef targetDef, IntVec3 c, TerrainGrid terrGrid, out SectionLayer_SubstructureProps.EdgeDirections edgeEdgeDirections, out SectionLayer_SubstructureProps.CornerDirections cornerDirections)
        {
            edgeEdgeDirections = SectionLayer_SubstructureProps.EdgeDirections.None;
            cornerDirections = SectionLayer_SubstructureProps.CornerDirections.None;

            for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
            {
                var c2 = c + GenAdj.CardinalDirections[i];
                if (!c2.InBounds(terrGrid.map))
                {
                    edgeEdgeDirections |= (SectionLayer_SubstructureProps.EdgeDirections)(1 << i);
                    continue;
                }
                var terrainDef2 = terrGrid.FoundationAt(c2) ?? terrGrid.TerrainAt(c2);
                if (!IsEquivalentSubstructure(targetDef, terrainDef2))
                {
                    edgeEdgeDirections |= (SectionLayer_SubstructureProps.EdgeDirections)(1 << i);
                }
            }
            for (int j = 0; j < GenAdj.DiagonalDirections.Length; j++)
            {
                var c3 = c + GenAdj.DiagonalDirections[j];
                if (!c3.InBounds(terrGrid.map))
                {
                    cornerDirections |= (SectionLayer_SubstructureProps.CornerDirections)(1 << j);
                    continue;
                }
                var terrainDef3 = terrGrid.FoundationAt(c3) ?? terrGrid.TerrainAt(c3);
                if (!IsEquivalentSubstructure(targetDef, terrainDef3))
                {
                    cornerDirections |= (SectionLayer_SubstructureProps.CornerDirections)(1 << j);
                }
            }
        }
    }
}
