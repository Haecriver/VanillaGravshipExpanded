using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(SectionLayer_IndoorMask), nameof(SectionLayer_IndoorMask.GenerateSectionLayer))]
public static class SectionLayer_IndoorMask_GenerateSectionLayer_Patch
{
    // RimWorld is currently drawing fog over vac checkpoints that are indoors.
    // This is due to the check expecting the doors to have full fillage, rather than 0% like we do.
    // Another exception is thick roof.
    // Basically, we change the condition from "!RoofDef.isThickRoof" to "!(RoofDef.isThickRoof || building is VacCheckpoint)"

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instr, generator);

        // Find the location where the code lookups the building using edificeGrid directly
        // Only match the beginning of the call
        matcher.MatchEndForward(
            // The code loads the map argument
            CodeMatch.IsLdarg(),
            // The code loads the edifice grid field
            CodeMatch.LoadsField(typeof(Map).DeclaredField(nameof(Map.edificeGrid))),
            // The code loads the inner array of edifice grid
            CodeMatch.Calls(typeof(EdificeGrid).DeclaredPropertyGetter(nameof(EdificeGrid.InnerArray)))
        );
        // Skip some code, but look for the next stloc to where the building is set, so we can copy its index.
        // Match until the start of the sequence, rather than end (past it).
        matcher.MatchStartForward(CodeMatch.StoresLocal());

        // If testing with outdated Harmony (TranspilerExplorer), change this to 8 (unless the index changed due to an update).
        // This code is more future-proof, but won't work with more recent Harmony versions.
        var buildingIndex = matcher.Instruction.LocalIndex();

        // Find the !roof.isThickRoof check
        matcher.MatchEndForward(
            // The code loads the local we need (roofGrid)
            CodeMatch.IsLdloc(),
            // The code checks if it's thick roof
            CodeMatch.LoadsField(typeof(RoofDef).DeclaredField(nameof(RoofDef.isThickRoof))),
            // The code branches away after the check
            CodeMatch.Branches()
        );

        matcher.Insert(
            // Load the building
            CodeInstruction.LoadLocal(buildingIndex),
            // Call our method
            CodeInstruction.Call(() => IsVacCheckpoint),
            // Bitwise or the two instructions, so we skip over if either one is true
            new CodeInstruction(OpCodes.Or)
        );

        return matcher.Instructions();
    }

    // Technically, we could just use the ILCode from this methohd directly, rather than calling this method.
    // However, if we ever need to update this method to do more stuff - we can easily update it this way.
    private static bool IsVacCheckpoint(Building building) => building is Building_VacCheckpoint;
}