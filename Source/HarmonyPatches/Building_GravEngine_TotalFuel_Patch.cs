using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.TotalFuel), MethodType.Getter)]
public static class Building_GravEngine_TotalFuel_Patch
{
    private static void Postfix(Building_GravEngine __instance, ref float __result)
    {
        __result += GravshipFuelProviderUtility.CurrentRangeForAllProviders(__instance) * 10f;
    }
}