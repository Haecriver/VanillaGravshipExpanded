using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.CostFactor))]
public static class ResearchProjectDef_CostFactor_Patch
{
    private static bool Prefix(ResearchProjectDef __instance, ref float __result)
    {
        if (!__instance.IsGravshipResearch())
            return true;

        __result = 1f;
        return false;
    }
}