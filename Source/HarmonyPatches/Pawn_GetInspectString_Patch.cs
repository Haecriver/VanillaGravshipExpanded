using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
public class Pawn_GetInspectString_Patch
{
    private static void Postfix(Pawn __instance, ref string __result)
    {
        // Only display on colonists or prisoners
        if (__instance.Faction != Faction.OfPlayer && !__instance.IsPrisonerOfColony)
            return;
        // Mechanoids can't get vacuum exposure, in case a mod makes humanlike mechanoids
        if (__instance.RaceProps.IsMechanoid)
            return;
        // Can't get exposure with no hediffs
        if (__instance.health?.hediffSet == null)
            return;
        // No point showing for a dead pawn
        if (__instance.Dead)
            return;

        var parent = __instance.SpawnedParentOrMe;
        // Make sure the pawn is spawned or something is holding it. If pawn is held in something that doesn't tick it, don't show info.
        if (parent == null || (parent != __instance && parent.cachedTickable is { ShouldTickContents: false }))
            return;

        var pos = __instance.PositionHeld;
        // Check if in bounds
        if (!pos.InBounds(parent.Map))
            return;

        var vacuum = pos.GetVacuum(parent.Map);
        // Don't display anything when less than 50% vacuum
        if (vacuum < VacuumUtility.MinVacuumForDamage)
            return;

        // If a mutant that doesn't breathe air, skip any vacuum resistance checks (since they may have none)
        if (__instance.IsMutant && !__instance.mutant.Def.breathesAir)
        {
            __result += $"\n{"VGE_ResistantToVacuumExposure".Translate().CapitalizeFirst()}";
            return;
        }

        var changePerSecond = VacuumUtility.SeverityPerSecond * vacuum * Mathf.Max(1f - __instance.GetStatValue(StatDefOf.VacuumResistance, cacheStaleAfterTicks: 60), 0f);
        if (changePerSecond <= 0f)
        {
            __result += $"\n{"VGE_ResistantToVacuumExposure".Translate().CapitalizeFirst()}";
            return;
        }

        // Get current severity
        var severity = __instance.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.VacuumExposure)?.Severity ?? 0f;
        var timeUntilDeath = (1f - severity) / changePerSecond;

        __result += $"\n{"VGE_TimeUntilVacuumExposureDeath".Translate(timeUntilDeath.SecondsToTicks().ToStringTicksToPeriod()).CapitalizeFirst()}";
    }
}