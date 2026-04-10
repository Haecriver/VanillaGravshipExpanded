using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompGenepackContainer), nameof(CompGenepackContainer.PowerOn), MethodType.Getter)]
public static class CompGenepackContainer_PowerOn_Patch
{
    private static bool Prefix(CompGenepackContainer __instance, ref bool __result)
    {
        if (__instance.parent.GetComp<CompPowerTrader>() != null)
            return true;

        __result = true;
        return false;
    }
}