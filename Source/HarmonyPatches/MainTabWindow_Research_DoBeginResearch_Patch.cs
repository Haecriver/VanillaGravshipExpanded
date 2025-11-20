using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.DoBeginResearch))]
    public static class MainTabWindow_Research_DoBeginResearch_Patch
    {
        public static bool Prefix(ResearchProjectDef projectToStart) => !projectToStart.SetGravshipResearch();
    }
}
