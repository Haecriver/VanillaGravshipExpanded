using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class Designator_AreaRoof_DrawVacBarrierRoof
{
    public static IEnumerable<Type> TargetDesignatorTypes()
    {
        yield return typeof(Designator_AreaBuildRoof);
        yield return typeof(Designator_AreaNoRoof);
        yield return typeof(Designator_AreaIgnoreRoof);
    }

    private static IEnumerable<MethodBase> TargetMethods()
    {
        return TargetDesignatorTypes().Select(x => x.DeclaredMethod(nameof(Designator.SelectedUpdate)));
    }

    private static void Postfix(Designator __instance)
    {
        __instance.Map.areaManager.BuildVacBarrierRoof().MarkForDraw();
    }
}