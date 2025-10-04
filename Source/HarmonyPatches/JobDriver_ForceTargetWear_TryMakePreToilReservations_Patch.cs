using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(JobDriver_ForceTargetWear), nameof(JobDriver_ForceTargetWear.TryMakePreToilReservations))]
public static class JobDriver_ForceTargetWear_TryMakePreToilReservations_Patch
{
    private static void Postfix(JobDriver_ForceTargetWear __instance, bool __result)
    {
        if (!__result)
            return;

        var apparel = __instance.Apparel;
        var comp = apparel.GetComp<CompApparelOxygenProvider>();
        if (comp == null || comp.Props.minResistanceToActivate <= 0)
            return;

        var target = __instance.TargetPawn;
        var baseVacuumResistance = StatDefOf.VacuumResistance.Worker.GetValueUnfinalized(StatRequest.For(target), false);
        if (baseVacuumResistance < comp.Props.minResistanceToActivate)
            Messages.Message("VGE_EquippingOnOtherIneffectiveOxygenPack".Translate(__instance.pawn.Named("PAWN"), target.Named("TARGET"), apparel.Named("APPAREL")), new LookTargets(__instance.pawn, target, apparel), MessageTypeDefOf.CautionInput, false);
    }
}