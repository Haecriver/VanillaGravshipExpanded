using HarmonyLib;
using VEF.Maps;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(RoofGrid), nameof(RoofGrid.SetRoof))]
public static class RoofGrid_SetRoof_Patch
{
    private static void Postfix(IntVec3 c, RoofDef def, Map ___map)
    {
        if (def == null || def.GetModExtension<RoofExtension>()?.customRoofGraphic is ColorableVacBarrierRoofGraphic)
            ___map.SetVacBarrierRoofColorAt(c, VacBarrierRoofUtility.BaseColor);
    }
}