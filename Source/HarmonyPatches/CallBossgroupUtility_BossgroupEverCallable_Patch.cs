using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CallBossgroupUtility), nameof(CallBossgroupUtility.BossgroupEverCallable))]
public static class CallBossgroupUtility_BossgroupEverCallable_Patch
{
    // Only apply this patch if we have Biotech active
    private static bool Prepare(MethodBase method) => method != null || ModsConfig.BiotechActive;

    private static void Postfix(Pawn pawn, BossgroupDef def, bool forced, ref AcceptanceReport __result)
    {
        // Do nothing if accepted
        if (__result)
            return;
        // Only check for the diabolus
        if (def != VGEDefOf.Diabolus)
            return;
        // Avoid infinite recursion, if we already have an override just skip
        if (CallBossgroupUtility_GetBossgroupCaller_Patch.temporaryBossgroupCallerOverride != null)
            return;
        // Is our console ends up being the first result, no point in doing anything
        if (CallBossgroupUtility.GetBossgroupCaller(def) == VGEDefOf.VGE_CommsTerminal)
            return;
        // Double-check that the console is still valid bossgroup caller
        if (VGEDefOf.VGE_CommsTerminal.GetCompProperties<CompProperties_Useable_CallBossgroup>() is not { } comp || comp.bossgroupDef != def)
            return;

        // Replace the bossgroup caller with our own
        CallBossgroupUtility_GetBossgroupCaller_Patch.temporaryBossgroupCallerOverride = VGEDefOf.VGE_CommsTerminal;
        // Call the method we patched again, but this time we have replaced the bossgroup caller
        var newResult = CallBossgroupUtility.BossgroupEverCallable(pawn, def, forced);
        // If accepted, replace result. Don't replace always as that would replace the original label as well.
        if (newResult)
            __result = newResult;
    }

    // Cleanup in finalizer in case an exception is ever thrown in postfix
    private static void Finalizer() => CallBossgroupUtility_GetBossgroupCaller_Patch.temporaryBossgroupCallerOverride = null;
}