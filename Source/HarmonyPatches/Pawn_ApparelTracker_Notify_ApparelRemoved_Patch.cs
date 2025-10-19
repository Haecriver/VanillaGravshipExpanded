using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
public static class Pawn_ApparelTracker_Notify_ApparelRemoved_Patch
{
    private static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        => Pawn_ApparelTracker_Notify_ApparelAdded_Patch.UpdateReachabilityOnApparelChangeIfNeeded(__instance.pawn, apparel);
}