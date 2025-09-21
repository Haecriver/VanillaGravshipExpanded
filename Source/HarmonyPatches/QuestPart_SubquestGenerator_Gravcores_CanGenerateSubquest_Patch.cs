using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(QuestPart_SubquestGenerator_Gravcores), nameof(QuestPart_SubquestGenerator_Gravcores.CanGenerateSubquest), MethodType.Getter)]
public static class QuestPart_SubquestGenerator_Gravcores_CanGenerateSubquest_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var colonistsHaveBuildingMethod = typeof(ListerBuildings).DeclaredMethod(nameof(ListerBuildings.ColonistsHaveBuilding), [typeof(ThingDef)]);
        var replacementColonistsHaveBuildingMethod = typeof(QuestPart_SubquestGenerator_Gravcores_CanGenerateSubquest_Patch).DeclaredMethod(nameof(ColonistsHaveAnyGravship));

        var replacedColonistsHaveBuildingCalls = 0;

        foreach (var ci in instr)
        {
            if (ci.Calls(colonistsHaveBuildingMethod))
            {
                ci.opcode = OpCodes.Call;
                ci.operand = replacementColonistsHaveBuildingMethod;

                replacedColonistsHaveBuildingCalls++;
            }

            yield return ci;
        }

        const int expectedPatches = 1;
        if (replacedColonistsHaveBuildingCalls != expectedPatches)
            Log.Error($"Patching QuestPart_SubquestGenerator_Gravcores:CanGenerateSubquest - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {replacedColonistsHaveBuildingCalls}. Game may fail to find custom VE grav engines.");
    }

    private static bool ColonistsHaveAnyGravship(ListerBuildings lister, ThingDef def) => lister.ColonistsHaveBuilding(def) || lister.ColonistsHaveBuilding(VGEDefOf.VGE_GravjumperEngine) || lister.ColonistsHaveBuilding(VGEDefOf.VGE_GravhulkEngine);
}