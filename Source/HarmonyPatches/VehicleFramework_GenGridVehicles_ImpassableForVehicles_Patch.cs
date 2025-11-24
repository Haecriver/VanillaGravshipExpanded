using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class VehicleFramework_GenGridVehicles_ImpassableForVehicles_Patch
{
    private static bool Prepare() => ModsConfig.IsActive("SmashPhil.VehicleFramework") || ModsConfig.IsActive("SmashPhil.VehicleFramework_steam");
    
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method("VehicleFramework.GenGridVehicles:ImpassableForVehicles");
    }

    private static void Postfix(ThingDef __0, ref bool __result)
    {
        if (__result && __0.thingClass.SameOrSubclassOf(typeof(Building_VacBarrier)))
            __result = false;
    }
}
