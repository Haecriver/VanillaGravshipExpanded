using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.PlayerHasGravEngine), [])]
// Go before anything else, we want to fully replace the vanilla check with our own
[HarmonyPriority(int.MaxValue)]
public static class GravshipUtility_PlayerHasGravEngine_Global_Patch
{
    private static bool Prefix(ref bool __result)
    {
        // Return true if we have an engine at all
        __result = GravEngineTracker.GetPlayerGravEngine() != null;

        return false;
    }
}

[HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.PlayerHasGravEngine), typeof(Map))]
// Go before anything else, we want to fully replace the vanilla check with our own
[HarmonyPriority(int.MaxValue)]
public static class GravshipUtility_PlayerHasGravEngine_Map_Patch
{
    private static bool Prefix(Map map, ref bool __result)
    {
        // Null map, not supported. Let the vanilla code run so it errors out there, not in our patch.
        if (map == null)
            return true;

        var engine = GravEngineTracker.GetPlayerGravEngine();
        // Return true if we have an engine and it's on the specified map
        __result = engine != null && engine.MapHeld == map;

        return false;
    }
}