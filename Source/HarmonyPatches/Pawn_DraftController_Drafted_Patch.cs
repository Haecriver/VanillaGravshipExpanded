using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
public static class Pawn_DraftController_Drafted_Patch
{
    private static void Postfix(Pawn_DraftController __instance)
    {
        var map = __instance.pawn.Map;
        // Not spawned, not a vacuum biome, etc., don't clear cache
        if (map?.Biome?.inVacuum != true)
            return;

        map.reachability.cache.ClearFor(__instance.pawn);
    }
}