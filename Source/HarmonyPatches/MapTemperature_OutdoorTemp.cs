using HarmonyLib;
using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(MapTemperature), nameof(MapTemperature.OutdoorTemp), MethodType.Getter)]
public static class VanillaGravshipExpanded_MapTemperature_OutdoorTemp_Patch
{
    private static void Postfix(MapTemperature __instance,ref float __result)
    {
        if (__instance.map.gameConditionManager.ConditionIsActive(VGEDefOf.VGE_SpaceSolarFlare))
        {
            __result = 340;
        }
    }
}