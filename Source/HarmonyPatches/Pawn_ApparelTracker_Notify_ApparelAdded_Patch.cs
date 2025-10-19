using HarmonyLib;
using RimWorld;
using UnityEngine;
using VEF.Apparels;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
public static class Pawn_ApparelTracker_Notify_ApparelAdded_Patch
{
    private static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        => UpdateReachabilityOnApparelChangeIfNeeded(__instance.pawn, apparel);

    public static void UpdateReachabilityOnApparelChangeIfNeeded(Pawn pawn, Apparel apparel)
    {
        var map = pawn.Map;
        // Not spawned, not a vacuum biome, etc., don't clear cache
        if (map?.Biome?.inVacuum != true)
            return;

        // Doesn't affect oxygen resistance, don't clear cache
        if (Mathf.Approximately(apparel.def.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.VacuumResistance), 0f) &&
            Mathf.Approximately(VanillaExpandedFramework_StatWorker_GetValueUnfinalized_Transpiler.StatFactorFromGear(apparel, StatDefOf.VacuumResistance), 1f) &&
            apparel.GetComp<CompApparelOxygenProvider>() != null)
            return;

        map.reachability.cache.ClearFor(pawn);
    }
}