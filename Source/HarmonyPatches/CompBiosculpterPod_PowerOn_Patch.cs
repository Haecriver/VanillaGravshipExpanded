using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.PowerOn), MethodType.Getter)]
public static class CompBiosculpterPod_PowerOn_Patch
{
    private static bool Prefix(CompBiosculpterPod __instance, ref bool __result)
    {
        // If power comp is present, let it run
        if (__instance.parent.GetComp<CompPowerTrader>() != null)
            return true;

        // No power comp, return early and set result to true
        __result = true;
        return false;
    }
}