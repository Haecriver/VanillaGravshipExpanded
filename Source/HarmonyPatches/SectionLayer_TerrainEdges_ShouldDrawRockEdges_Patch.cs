using HarmonyLib;
using Verse;
using RimWorld;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(SectionLayer_TerrainEdges), "ShouldDrawRockEdges")]
    public static class SectionLayer_TerrainEdges_ShouldDrawRockEdges_Patch
    {
        public static void Prefix()
        {
            VGEDefOf.VGE_GravshipSubscaffold.dontRender = false;
        }
        public static void Postfix()
        {
            VGEDefOf.VGE_GravshipSubscaffold.dontRender = true;
        }
    }
}
