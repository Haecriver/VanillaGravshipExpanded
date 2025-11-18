using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ScenPart_PursuingMechanoids), nameof(ScenPart_PursuingMechanoids.Tick))]
public static class ScenPart_PursuingMechanoids_Tick_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var targetListerMethod = SymbolExtensions.GetMethodInfo(() => ((ListerThings)null).ThingsOfDef);
        var replacedListerMethod = SymbolExtensions.GetMethodInfo(() => ReplacePilotConsole);

        var targetTranslateMethod = SymbolExtensions.GetMethodInfo(() => Translator.Translate(null));
        var replacedTranslateMethod = SymbolExtensions.GetMethodInfo(() => ReplaceTranslation);

        var foundRightText = false;

        var patchedConsoleCalls = 0;
        var patchedTranslationCalls = 0;

        foreach (var ci in instr)
        {
            if (foundRightText)
            {
                foundRightText = false;

                if (!ci.Calls(targetTranslateMethod))
                    Log.Error($"[VGE] Incorrect method patched. Expected a call to Translator.Trasnlate(string). Code instruction found: {ci}");

                // Load the first local (map)
                yield return CodeInstruction.LoadLocal(1);
                // Load the second local (pilot console thing)
                yield return CodeInstruction.LoadLocal(2);

                // Replace the Translate call with our own so we can easily insert extra arguments
                ci.opcode = OpCodes.Call;
                ci.operand = replacedTranslateMethod;

                patchedTranslationCalls++;
            }
            else if (ci.opcode == OpCodes.Ldstr && ci.operand is "LetterTextMechanoidThreat")
            {
                foundRightText = true;
            }
            else if (ci.Calls(targetListerMethod))
            {
                ci.opcode = OpCodes.Call;
                ci.operand = replacedListerMethod;

                patchedConsoleCalls++;
            }

            yield return ci;
        }

        const int expectedTranslationPatches = 1;
        const int expectedPilotConsolePatches = 1;

        if (patchedTranslationCalls != expectedTranslationPatches)
            Log.Error($"Patching ScenPart_PursuingMechanoids:Tick - unexpected amount of translation patches. Expected patches: {expectedTranslationPatches}, actual patch amount: {patchedTranslationCalls}. Game may fail to find custom VE grav engines.");
        if (patchedConsoleCalls != expectedPilotConsolePatches)
            Log.Error($"Patching ScenPart_PursuingMechanoids:Tick - unexpected amount of pilot console patches. Expected patches: {expectedPilotConsolePatches}, actual patch amount: {patchedConsoleCalls}. Game may fail to find custom VE pilot consoles.");
    }

    private static TaggedString ReplaceTranslation(string key, Map map, Thing pilotConsole)
    {
        if (key != "LetterTextMechanoidThreat")
        {
            Log.ErrorOnce($"[VGE] Incorrect key was replaced. Expected: \"LetterTextMechanoidThreat\", received: \"{key}\".", key.GetHashCode());
            return key.Translate();
        }

        var engine = GravshipUtility.GetPlayerGravEngine_NewTemp(map);
        string engineName;

        if (engine?.def == VGEDefOf.VGE_GravjumperEngine)
            engineName = "VGE_Gravjumper";
        else if (engine?.def == VGEDefOf.VGE_GravhulkEngine)
            engineName = "VGE_Gravhulk";
        else
            engineName = "VGE_Gravship";

        var consoleName = pilotConsole?.def.label ?? ThingDefOf.PilotConsole.label;

        return "VGE_LetterTextMechanoidThreat".Translate(engineName.Translate().Named("GRAVSHIP"), consoleName.Named("CONSOLE"));
    }

    private static List<Thing> ReplacePilotConsole(ListerThings lister, ThingDef def)
    {
        if (def != ThingDefOf.PilotConsole)
            return lister.ThingsOfDef(def);

        Thing fallback = null;

        return [GetLinkedThing(def) ?? GetLinkedThing(VGEDefOf.VGE_PilotCockpit) ?? GetLinkedThing(VGEDefOf.VGE_PilotBridge) ?? fallback];

        Thing GetLinkedThing(ThingDef targetDef)
        {
            var things = lister.ThingsOfDef(targetDef);
            // Grab a fallback if it's currently null, in case it'll be needed.
            fallback ??= things.FirstOrDefault();
            // Find a first linked thing
            return things.FirstOrDefault(x => x.TryGetComp<CompGravshipFacility>()?.engine != null);
        }
    }
}