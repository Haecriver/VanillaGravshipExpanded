using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.MaxFuel), MethodType.Getter)]
public static class Building_GravEngine_MaxFuel_Patch
{
    private static void Postfix(Building_GravEngine __instance, ref float __result)
    {
        __result += GravshipFuelProviderUtility.MaxRangeForAllProviders(__instance) * 10f;
    }
}