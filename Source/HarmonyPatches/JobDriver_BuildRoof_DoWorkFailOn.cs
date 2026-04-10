using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(JobDriver_BuildRoof), nameof(JobDriver_BuildRoof.DoWorkFailOn))]
public static class JobDriver_BuildRoof_DoWorkFailOn_Patch
{
    private static void Postfix(JobDriver_BuildRoof __instance, ref bool __result)
    {
        if (__result)
            return;

        // If there's a vac barrier roof with a build vac barrier roof area, fail work
        if (VGEDefOf.VGE_VacBarrierRoof != null && __instance.Cell.GetRoof(__instance.Map) == VGEDefOf.VGE_VacBarrierRoof && __instance.Map.areaManager.BuildVacBarrierRoof()[__instance.Cell])
            __result = true;
    }
}