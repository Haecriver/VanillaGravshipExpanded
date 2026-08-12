using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.GetPlayerGravEngine_NewTemp))]
// Go before anything else, we want to fully replace the vanilla check with our own
[HarmonyPriority(int.MaxValue)]
public static class GravshipUtility_GetPlayerGravEngine_Patch
{
    private static bool Prefix(Map map, ref Building_GravEngine __result)
    {
        var engine = GravEngineTracker.GetPlayerGravEngine();
        if (engine.MapHeld == map)
            __result = engine;

        return false;
    }
}