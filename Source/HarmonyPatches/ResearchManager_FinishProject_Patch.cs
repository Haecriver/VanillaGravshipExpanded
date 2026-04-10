using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using VEF.CacheClearing;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
public static class ResearchManager_FinishProject_Patch
{
    public static HashSet<CompProperties_Heatsink> allHeatsinkComps = [];

    static ResearchManager_FinishProject_Patch()
    {
        // Clear cached heatsink stats on new game/game load
        ClearCaches.OnClearCache += _ =>
        {
            foreach (var comp in allHeatsinkComps)
                comp.cachedStats = null;
        };
    }

    private static void Postfix(ResearchProjectDef proj)
    {
        // Unselect the gravship research once 
        if (World_ExposeData_Patch.currentGravtechProject == proj)
            World_ExposeData_Patch.currentGravtechProject = null;

        // Clear all heatsink comp stats in case the research changes its stats
        foreach (var comp in allHeatsinkComps)
            comp.cachedStats = null;
    }
}