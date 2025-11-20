using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.PlayerHasAnyAppropriateResearchBench), MethodType.Getter)]
    public static class ResearchProjectDef_PlayerHasAnyAppropriateResearchBench_Patch
    {
        public static void Postfix(ResearchProjectDef __instance, ref bool __result)
        {
            if (__instance.IsGravshipResearch())
            {
                __result = true;
            }
        }
    }
}
