using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(JobDriver_BuildRoof), nameof(JobDriver_BuildRoof.DoEffect))]
public static class JobDriver_BuildRoof_DoEffect_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        // Look for the check for roofed check
        matcher.MatchEndForward(
            // Loads the current cell loc
            CodeMatch.IsLdloc(),
            // Loads "this" arg
            CodeMatch.IsLdarg(0),
            // Grabs the map from the job driver
            CodeMatch.Calls(typeof(JobDriver).DeclaredPropertyGetter(nameof(JobDriver.Map))),
            // Checks if the cell is roofed
            CodeMatch.Calls(typeof(GridsUtility).DeclaredMethod(nameof(GridsUtility.Roofed), [typeof(IntVec3), typeof(Map)])),
            // 
            CodeMatch.Branches()
        );

        matcher.Insert(
            // Clone instructions to load the cell
            matcher.InstructionAt(-4).Clone(),
            // Clone instructions to call this.Map
            matcher.InstructionAt(-3).Clone(),
            matcher.InstructionAt(-2).Clone(),
            // Insert our call
            CodeInstruction.Call(() => NotVacBarrierRoofArea),
            // Bitwise and the 2 bools together
            new CodeInstruction(OpCodes.And)
            // We could have replaced the vanilla call and reproduced it in our method,
            // but if another mod relies on that method's presence - it would fail since
            // it's not present. So we just insert an extra check.
        );

        return matcher.Instructions();
    }

    private static bool NotVacBarrierRoofArea(IntVec3 cell, Map map)
    {
        // If vac barrier roof is null (something removed it?) always true.
        // Prevents bugs with roof construction.
        if (VGEDefOf.VGE_VacBarrierRoof == null)
            return true;
        // If roof is anything but vac barrier roof, allow building over it.
        // Vanilla already does an unroofed check, so we only care about vac barrier roofs.
        if (cell.GetRoof(map) != VGEDefOf.VGE_VacBarrierRoof)
            return true;
        // If the area is not a build vac barrier roof area
        return !map.areaManager.BuildVacBarrierRoof()[cell];
    }
}