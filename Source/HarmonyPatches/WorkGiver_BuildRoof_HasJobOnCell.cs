using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(WorkGiver_BuildRoof), nameof(WorkGiver_BuildRoof.HasJobOnCell))]
public static class WorkGiver_BuildRoof_HasJobOnCell_Patch
{
    private static void Postfix(Pawn pawn, IntVec3 c, ref bool __result)
    {
        if (!__result)
            return;

        // If there's a vac barrier roof with a build vac barrier roof area, fail work
        if (VGEDefOf.VGE_VacBarrierRoof != null && c.GetRoof(pawn.Map) == VGEDefOf.VGE_VacBarrierRoof && pawn.Map.areaManager.BuildVacBarrierRoof()[c])
            __result = true;
    }
}