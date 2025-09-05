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
        public static bool Prefix(IntVec3 c, TerrainGrid terrGrid)
        {
            if (doVanilla)
            {
                return true;
            }
            return terrGrid.FoundationAt(c) != VGEDefOf.VGE_GravshipSubscaffold;
        }
    }
}
