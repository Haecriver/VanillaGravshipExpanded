using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(QuestNode_Root_Gravship_Wreckage), nameof(QuestNode_Root_Gravship_Wreckage.RunInt))]
public static class QuestNode_Root_Gravship_Wreckage_RunInt_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
        => ReplaceGravEngineFieldWithMechanoidGravEngine(instr, baseMethod, 1);

    internal static IEnumerable<CodeInstruction> ReplaceGravEngineFieldWithMechanoidGravEngine(IEnumerable<CodeInstruction> instr, MethodBase baseMethod, int expectedReplacements)
    {
        var targetField = typeof(ThingDefOf).DeclaredField(nameof(ThingDefOf.GravEngine));
        var replacementField = typeof(VGEDefOf).DeclaredField(nameof(VGEDefOf.VGE_MechanoidGravEngine));

        var replacements = 0;

        foreach (var ci in instr)
        {
            if (ci.LoadsField(targetField))
            {
                ci.operand = replacementField;
                replacements++;
            }

            yield return ci;
        }

        if (replacements != expectedReplacements)
            Log.Error($"Patching {baseMethod.DeclaringType?.Name}:{baseMethod.Name} - unexpected amount of patches. Expected patches: {expectedReplacements}, actual patch amount: {replacements}. Quest may spawn grav engine rather than mechanoid grav engine in certain quests.");
    }
}