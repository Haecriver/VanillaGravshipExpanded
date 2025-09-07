using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.IsCurrentProject))]
    public static class ResearchManager_IsCurrentProject_Patch
    {
        public static bool Prefix(ResearchProjectDef proj, ref bool __result)
        {
            if (proj?.tab == VGEDefOf.VGE_Gravtech)
            {
                __result = World_ExposeData_Patch.currentGravtechProject == proj;
                return false;
            }
            return true;
        }
    }
}
