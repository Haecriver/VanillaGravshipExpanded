using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class VehicleFramework_GenGridVehicles_ImpassableForVehicles_Patch
{
    private static bool Prepare() => ModLister.AnyModActiveNoSuffix(["SmashPhil.VehicleFramework"]);
    
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method("Vehicles.GenGridVehicles:ImpassableForVehicles");
    }

    private static void Postfix(ThingDef __0, ref bool __result)
    {
        if (__result && __0.thingClass.SameOrSubclassOf(typeof(Building_VacBarrier)))
            __result = false;
    }
}
