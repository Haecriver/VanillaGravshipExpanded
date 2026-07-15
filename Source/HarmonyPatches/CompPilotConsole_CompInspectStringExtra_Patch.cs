using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompPilotConsole), nameof(CompPilotConsole.CompInspectStringExtra))]
public static class CompPilotConsole_CompInspectStringExtra_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var removals = 0;

        var matcher = new CodeMatcher(instr, generator);
        while (true)
        {
            matcher.MatchStartForward(
                CodeMatch.IsLdloc(),
                new CodeMatch(ci => ci.opcode == OpCodes.Ldstr && ci.operand is string and ("StoredChemfuel" or "FuelConsumption"))
            );

            if (matcher.IsInvalid)
                break;

            matcher.DefineLabel(out var label);
            matcher.Insert(new CodeInstruction(OpCodes.Br_S, label).MoveLabelsFrom(matcher.Instruction));

            matcher.MatchStartForward(new CodeMatch(OpCodes.Pop));
            matcher.Advance();
            matcher.AddLabels([label]);
            removals++;

            if (removals >= 10)
            {
                Log.Error($"Too many attempts patching {nameof(CompPilotConsole)}:{nameof(CompPilotConsole.CompInspectStringExtra)}");
                break;
            }
        }

        const int expectedPatches = 2;

        if (removals != expectedPatches)
            Log.Error($"Patching CompPilotConsole:CompInspectStringExtra - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {removals}. Pilot consoles will display chemfuel costs despite having a separate fuel tab.");

        return matcher.Instructions();
    }
}