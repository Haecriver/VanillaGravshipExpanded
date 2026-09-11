using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.RenamableLabel), MethodType.Setter)]
public static class Building_GravEngine_RenamableLabel_Patch
{
    private static void Postfix(string __0) => World_ExposeData_Patch.lastGravshipName = __0;
}