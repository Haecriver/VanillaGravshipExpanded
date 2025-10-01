using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompPilotConsole), nameof(CompPilotConsole.CompInspectStringExtra))]
public static class CompPilotConsole_CompInspectStringExtra_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var replacements = 0;

        foreach (var ci in instr)
        {
            if (ci.opcode == OpCodes.Ldstr && ci.operand is string s)
            {
                switch (s)
                {
                    // Replace the "Stored chemfuel" with "Stored astrofuel" text
                    case "StoredChemfuel":
                        ci.operand = "VGE_StoredAstrofuel";
                        replacements++;
                        break;
                    // Replace the "{0} chemfuel per tile" with "{0} astrofuel per tile" text
                    case "FuelPerTile":
                        ci.operand = "VGE_AstrofuelPerTile";
                        replacements++;
                        break;
                }
            }

            yield return ci;
        }

        const int expectedPatches = 2;

        if (replacements != expectedPatches)
            Log.Error($"Patching CompPilotConsole:CompInspectStringExtra - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {replacements}. Pilot consoles may incorrectly refer to astrofuel as chemfuel.");
    }
}