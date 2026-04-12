using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.DebugSetAllProjectsFinished))]
public static class ResearchManager_DebugSetAllProjectsFinished_Patch
{
    private static void Postfix()
    {
        // Unselect the gravship research, as it's guaranteed to be finished now
        World_ExposeData_Patch.currentGravtechProject = null;

        // Clear all heatsink comp stats in case the research changes its stats
        foreach (var comp in ResearchManager_FinishProject_Patch.allHeatsinkComps)
            comp.cachedStats = null;
    }
}