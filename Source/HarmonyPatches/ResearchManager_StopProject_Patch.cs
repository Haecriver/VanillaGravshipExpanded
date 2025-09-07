using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.StopProject))]
    public static class ResearchManager_StopProject_Patch
    {
        public static void Prefix(ResearchProjectDef proj)
        {
            if (proj == World_ExposeData_Patch.currentGravtechProject)
            {
                World_ExposeData_Patch.currentGravtechProject = null;
            }
        }
    }
}