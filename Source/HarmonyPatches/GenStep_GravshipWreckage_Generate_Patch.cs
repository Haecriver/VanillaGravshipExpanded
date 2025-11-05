using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GenStep_GravshipWreckage), nameof(GenStep_GravshipWreckage.Generate))]
public static class GenStep_GravshipWreckage_Generate_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
        => QuestNode_Root_Gravship_Wreckage_RunInt_Patch.ReplaceGravEngineFieldWithMechanoidGravEngine(instr, baseMethod, 1);
}