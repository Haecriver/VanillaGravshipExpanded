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
        public static bool Prefix(IntVec3 c, TerrainGrid terrGrid, ref bool __result)
        {
            if (doVanilla)
            {
                return true;
            }
            var foundationDef = terrGrid.FoundationAt(c);
            var topDef = terrGrid.TerrainAt(c);
            var terrain = foundationDef ?? topDef;
            if (foundationDef == VGEDefOf.VGE_GravshipSubscaffold || (terrain != null && (terrain.HasModExtension<SubstructureEdgeGraphicsExtension>() || terrain == VGEDefOf.VGE_MechanoidSubstructure)))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
