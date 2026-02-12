using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.SetPower))]
public static class CompBiosculpterPod_SetPower_Patch
{
    private static bool Prefix(CompBiosculpterPod __instance)
    {
        // Check if power comp is present, prevent the method from running if it's missing.
        return (__instance.powerComp ??= __instance.parent.GetComp<CompPower>()) != null;
    }
}