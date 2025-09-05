using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_Door), nameof(Building_Door.CanPhysicallyPass))]
public static class Building_Door_CanPhysicallyPass_Patch
{
    private static void Postfix(Building_Door __instance, Pawn p, ref bool __result)
    {
        if (__result && __instance is Building_VacCheckpoint checkpoint && checkpoint.CaresAboutThisVacBarrier(p))
            __result = false;
    }
}