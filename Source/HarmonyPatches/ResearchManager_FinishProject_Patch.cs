using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
public static class ResearchManager_FinishProject_Patch
{
    private static void Postfix(ResearchProjectDef proj)
    {
        // Unselect the gravship research once 
        if (World_ExposeData_Patch.currentGravtechProject == proj)
            World_ExposeData_Patch.currentGravtechProject = null;
    }
}