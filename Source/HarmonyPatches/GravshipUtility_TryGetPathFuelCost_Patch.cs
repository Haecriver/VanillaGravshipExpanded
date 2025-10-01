using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.TryGetPathFuelCost))]
public static class GravshipUtility_TryGetPathFuelCost_Patch
{
    private static void Prefix(ref float fuelPerTile)
    {
        // We're changing the cost from 10 chemfuel to 5 astrofuel (which, by default, costs 10 chemfuel).
        // The method takes 10 as the default value, and never specifies any other cost. So the simplest
        // way to handle this is just cut all the costs in half. We don't bother considering the fuel
        // savings factor in this, since that is a separate argument for this method (as opposed to
        // Building_GravEngine:FuelPerTile getter, which includes the savings already).
        fuelPerTile /= 2f;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var modifyFuelFactorMethod = typeof(GravshipUtility_TryGetPathFuelCost_Patch).DeclaredMethod(nameof(ModifyFuelFactor));

        var minCostReplacements = 0;
        var fuelFactorReplacements = 0;

        foreach (var ci in instr)
        {
            yield return ci;

            // Drop the minimum cost of the gravship launch from 50 to 25, since we're cutting everything else in half.
            if (ci.opcode == OpCodes.Ldc_R4 && ci.operand is 50f)
            {
                ci.operand = 25f;
                minCostReplacements++;
            }
            // Apply 25% extra fuel efficiency if the target tile has a grav anchor.
            // We could handle it as a prefix, but for the sake of having this value cached - adding transpiler so the check will also get cached.
            else if (ci.IsLdarg(5))
            {
                // Insert the 2nd argument (target PlanetTile)
                yield return CodeInstruction.LoadArgument(1);
                // Modify the value by surrounding it in our method
                yield return new CodeInstruction(OpCodes.Call, modifyFuelFactorMethod);
                fuelFactorReplacements++;
            }
        }

        const int expectedMinCostReplacements = 1;
        const int expectedFuelFactorReplacements = 1;

        if (minCostReplacements != expectedMinCostReplacements)
            Log.Error($"Patching GravshipUtility:TryGetPathFuelCost - unexpected amount of minimum cost patches. Expected patches: {expectedMinCostReplacements}, actual patch amount: {minCostReplacements}. Gravship launch cost may be incorrect/broken.");
        if (fuelFactorReplacements != expectedFuelFactorReplacements)
            Log.Error($"Patching GravshipUtility:TryGetPathFuelCost - unexpected amount of fuel factor patches. Expected patches: {expectedFuelFactorReplacements}, actual patch amount: {fuelFactorReplacements}. Grav anchor fuel efficiency bonus may be broken/incorrect.");
    }

    private static float ModifyFuelFactor(float fuelFactor, PlanetTile to)
    {
        if (!to.Valid)
            return fuelFactor;

        var map = Current.Game.FindMap(to);
        if (map != null && map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor))
            return Mathf.Max(fuelFactor - 0.25f, 0f);

        return fuelFactor;
    }
}