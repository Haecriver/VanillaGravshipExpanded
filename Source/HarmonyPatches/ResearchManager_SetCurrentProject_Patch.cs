using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.SetCurrentProject))]
    public static class ResearchManager_SetCurrentProject_Patch
    {
        public static bool Prefix(ResearchProjectDef proj) => !proj.SetGravshipResearch(false);
    }
}
