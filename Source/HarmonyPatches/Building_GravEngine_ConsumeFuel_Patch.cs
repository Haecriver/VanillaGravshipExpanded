using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HotSwappable]
[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.ConsumeFuel))]
public static class Building_GravEngine_ConsumeFuel_Patch
{
    private static void Prefix(Building_GravEngine __instance, ref float __state) => __state = __instance.TotalFuel;

    private static void Postfix(Building_GravEngine __instance, PlanetTile tile, float __state)
    {
        // Grab cached values
        if (!GravshipUtility.TryGetPathFuelCost(__instance.Map.Tile, tile, out var cost, out _))
            return;

        if (LaunchInfo_ExposeData_Patch.isGravliftLaunch.TryGetValue(__instance.launchInfo, out bool isLiftLaunch) && isLiftLaunch)
        {
            cost = 20f;
            Log.Message("[VGE] Overriding fuel cost for gravlift launch. Using cost: " + cost);
        }

        // Divide cost by total fuel (cached before vanilla code started lowering it) to get a ratio of fuel we'll need to set each fuel tank to
        LaunchInfo_ExposeData_Patch.lastCost[__instance.launchInfo] = cost;
        var ratio = cost / __state;
        
        // Create the fuel spent data directly
        var fuelSpentData = new FuelSpentData();
        foreach (var comp in __instance.GravshipComponents)
        {
            if (comp.Props.providesFuel && comp.CanBeActive)
            {
                var storage = comp.parent.GetComp<CompResourceStorage>();
                if (storage == null)
                    continue;
                var toSpend = storage.AmountStored * ratio;
                fuelSpentData.fuelData[storage.parent] = toSpend;
                storage.DrawResource(toSpend);
            }
        }
        
        LaunchInfo_ExposeData_Patch.fuelSpentPerTank[__instance.launchInfo] = fuelSpentData;

        var heatManager = __instance.GetComp<CompHeatManager>();
        heatManager.AddHeat(cost);
        ApplyCooldownReduction(__instance);
    }

    private static void ApplyCooldownReduction(Building_GravEngine gravEngine)
    {
        float totalReduction = GetCooldownReduction(gravEngine);
        if (totalReduction > 0f)
        {
            int originalCooldownTicks = gravEngine.cooldownCompleteTick - GenTicks.TicksGame;
            int reducedCooldownTicks = Mathf.RoundToInt(originalCooldownTicks * (1f - totalReduction));
            gravEngine.cooldownCompleteTick = GenTicks.TicksGame + reducedCooldownTicks;
        }
    }

    public static float GetCooldownReduction(Building_GravEngine gravEngine)
    {
        float totalReduction = 0f;
        foreach (var comp in gravEngine.GravshipComponents)
        {
            var heatsink = comp.parent.GetComp<CompHeatsink>();
            if (heatsink != null && heatsink.IsActive)
            {
                totalReduction += heatsink.Props.cooldownReductionPercent;
            }
        }
        totalReduction = Mathf.Min(totalReduction, 0.5f);
        return totalReduction;
    }
}