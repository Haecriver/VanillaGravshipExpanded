using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class SubstructureGrid_DrawSubstructure_Patch
{
    private static List<Thing> TmpList = [];

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return typeof(SubstructureGrid).DeclaredMethod(nameof(SubstructureGrid.DrawSubstructureCountOnGUI));
        yield return typeof(SubstructureGrid).DeclaredMethod(nameof(SubstructureGrid.DrawSubstructureFootprint));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var gravEngineField = typeof(ThingDefOf).DeclaredField(nameof(ThingDefOf.GravEngine));

        var listerThingsMethodTarget = typeof(ListerThings).DeclaredMethod(nameof(ListerThings.ThingsOfDef));
        var listerThingsMethodReplacement = typeof(SubstructureGrid_DrawSubstructure_Patch).DeclaredMethod(nameof(ReturnAllGravEngines));

        var isGravEngineField = false;
        var replacedThingsOfDefCalls = 0;

        foreach (var ci in instr)
        {
            if (ci.LoadsField(gravEngineField))
            {
                isGravEngineField = true;
            }
            else if (isGravEngineField)
            {
                isGravEngineField = false;

                if (ci.Calls(listerThingsMethodTarget))
                {
                    // Replace the vanilla method call with our own
                    ci.opcode = OpCodes.Call;
                    ci.operand = listerThingsMethodReplacement;

                    replacedThingsOfDefCalls++;
                }
            }

            yield return ci;
        }

        const int expectedPatches = 1;
        if (replacedThingsOfDefCalls != expectedPatches)
            Log.Error($"Patching {baseMethod.DeclaringType?.Name}:{baseMethod.Name} - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {replacedThingsOfDefCalls}. Game may fail to find custom VE grav engines.");
    }

    private static List<Thing> ReturnAllGravEngines(ListerThings lister, ThingDef def)
    {
        TmpList.Clear();
        TmpList.AddRange(lister.ThingsOfDef(def));
        TmpList.AddRange(lister.ThingsOfDef(VGEDefOf.VGE_GravjumperEngine));
        TmpList.AddRange(lister.ThingsOfDef(VGEDefOf.VGE_GravhulkEngine));

        return TmpList;
    }
}