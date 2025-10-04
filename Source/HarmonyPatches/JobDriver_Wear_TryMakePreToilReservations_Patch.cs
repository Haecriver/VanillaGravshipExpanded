using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(JobDriver_Wear), nameof(JobDriver_Wear.TryMakePreToilReservations))]
public static class JobDriver_Wear_TryMakePreToilReservations_Patch
{
    private static void Postfix(JobDriver_Wear __instance, bool __result)
    {
        if (!__result)
            return;

        var apparel = __instance.Apparel;
        var comp = apparel.GetComp<CompApparelOxygenProvider>();
        if (comp == null || comp.Props.minResistanceToActivate <= 0)
            return;

        var baseVacuumResistance = StatDefOf.VacuumResistance.Worker.GetValueUnfinalized(StatRequest.For(__instance.pawn), false);
        if (baseVacuumResistance < comp.Props.minResistanceToActivate)
            Messages.Message("VGE_EquippingIneffectiveOxygenPack".Translate(__instance.pawn.Named("PAWN"), apparel.Named("APPAREL")), new LookTargets(__instance.pawn, apparel), MessageTypeDefOf.CautionInput, false);
    }
}