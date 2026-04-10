using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class Designator_AreaRoof_RemoveVacBarrierArea
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return Designator_AreaRoof_DrawVacBarrierRoof.TargetDesignatorTypes().Select(x => x.DeclaredMethod(nameof(Designator.DesignateSingleCell)));
    }

    private static void Postfix(Designator __instance, IntVec3 __0)
    {
        __instance.Map.areaManager.BuildVacBarrierRoof()[__0] = false;
    }
}