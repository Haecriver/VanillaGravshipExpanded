using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_VacBarrier), nameof(Building_VacBarrier.BarrierOn), MethodType.Getter)]
public static class Building_VacBarrier_BarrierOn_Patch
{
    // We need to disable the barrier for multi-tile doors so it stops drawing the barrier graphic, stops equalizing temperature and start exchanging vacuum.
    private static void Postfix(Building_VacBarrier __instance, ref bool __result)
    {
        if (__result && __instance is Building_VacBarrier_Recolorable { tempStuckInactive: true })
            __result = false;
    }
}