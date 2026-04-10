using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class RoofGrid_Roofed_Patch
{
    public static bool countVacBarrierAsRoofed = false;

    private static IEnumerable<MethodBase> TargetMethods()
    {
        var foundMethods = 0;

        foreach (var method in typeof(RoofGrid).GetDeclaredMethods())
        {
            if (method.Name == nameof(RoofGrid.Roofed))
            {
                foundMethods++;
                yield return method;
            }
        }

        const int expectedMethods = 3;
        if (foundMethods != expectedMethods)
            Log.Error($"[VGE] Found unexpected methods with name '{nameof(RoofGrid.Roofed)}' in '{nameof(RoofGrid)}' type. Expected: {expectedMethods}, found: {foundMethods}.");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchStartForward(
            // Loads an element out of roof grid array
            new CodeMatch(OpCodes.Ldelem_Ref),
            // Loads null
            new CodeMatch(OpCodes.Ldnull),
            // Returns if the 1st value is greater than the second (basically, true if not null, false if null)
            new CodeMatch(OpCodes.Cgt_Un)
        );

        // Advance over Ldelem and ldnull calls
        matcher.Advance(2);
        // Replace the equality comparison with our call
        matcher.Opcode = OpCodes.Call;
        matcher.Operand = SymbolExtensions.GetMethodInfo(() => IsRoofedReplacement);

        return matcher.Instructions();
    }

    private static bool IsRoofedReplacement(RoofDef roof, object nullObj)
    {
        // We keep the ldnull that the code uses for comparisons in case some other mod checks for it for positioning in patches
        return roof != null && (countVacBarrierAsRoofed || roof != VGEDefOf.VGE_VacBarrierRoof);
    }
}