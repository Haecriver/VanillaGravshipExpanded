using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompPilotConsole), nameof(CompPilotConsole.CompInspectStringExtra))]
public static class CompPilotConsole_CompInspectStringExtra_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var removals = 0;
        var replacedLayerWithSurface = false;
        var addedRangeForCurrentLayer = false;

        var matcher = new CodeMatcher(instr, generator);
        while (true)
        {
            // Find the string we're looking for
            matcher.MatchStartForward(
                CodeMatch.IsLdloc(),
                new CodeMatch(ci => ci.opcode == OpCodes.Ldstr && ci.operand is string and ("StoredChemfuel" or "FuelConsumption"))
            );

            // Break if not found
            if (matcher.IsInvalid)
                break;

            // Add a jump instruction to our new label (that we'll insert later)
            matcher.DefineLabel(out var label);
            matcher.Insert(new CodeInstruction(OpCodes.Br_S, label).MoveLabelsFrom(matcher.Instruction));

            // Look for the first pop instruction (since StringBuilder methods return self, if unused, it needs to be popped)
            matcher.MatchStartForward(new CodeMatch(OpCodes.Pop));
            // Advance after the pop
            matcher.Advance();
            // Insert our label, effectively jumping over the instructions we're removing - we keep it around in case other mods try to match those instructions
            matcher.AddLabels([label]);
            removals++;

            if (removals >= 10)
            {
                Log.Error($"Too many attempts patching {nameof(CompPilotConsole)}:{nameof(CompPilotConsole.CompInspectStringExtra)}");
                break;
            }
        }

        // Reset matcher - we're looking for new instructions
        matcher.Reset();
        // Match "GravshipRange" string
        matcher.MatchStartForward(
            CodeMatch.IsLdloc(),
            CodeMatch.LoadsConstant("GravshipRange")
        );

        if (matcher.IsValid)
        {
            matcher.MatchEndForward(
                CodeMatch.IsLdarg(0),
                CodeMatch.LoadsField(typeof(ThingComp).DeclaredField(nameof(ThingComp.parent))),
                CodeMatch.Calls(typeof(Thing).DeclaredPropertyGetter(nameof(Thing.Map))),
                CodeMatch.Calls(typeof(Map).DeclaredPropertyGetter(nameof(Map.Tile))),
                CodeMatch.IsStloc(),
                CodeMatch.IsLdloc(),
                CodeMatch.Calls(typeof(PlanetTile).DeclaredPropertyGetter(nameof(PlanetTile.Layer)))
            );

            if (matcher.IsValid)
            {
                // Insert our instruction to replace the layyer
                matcher.InsertAfter(CodeInstruction.Call(() => ReturnSurfaceLayer));
                replacedLayerWithSurface = true;

                // Look for the first pop instruction (since StringBuilder methods return self, if unused, it needs to be popped)
                matcher.MatchStartForward(new CodeMatch(OpCodes.Pop));
                if (matcher.IsValid)
                {
                    matcher.InsertAfter(
                        CodeInstruction.LoadArgument(0),
                        CodeInstruction.LoadLocal(0),
                        CodeInstruction.Call(() => InsertCurrentLayerRangeInfoIfNeeded)
                    );
                    addedRangeForCurrentLayer = true;
                }
            }
        }

        const int expectedPatches = 2;

        if (removals != expectedPatches)
            Log.Error($"Patching CompPilotConsole:CompInspectStringExtra - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {removals}. Pilot consoles will display chemfuel costs despite having a separate fuel tab.");
        if (!replacedLayerWithSurface)
            Log.Error("Patching CompPilotConsole:CompInspectStringExtra - failed to replace current layer range with surface layer range.");
        if (!addedRangeForCurrentLayer)
            Log.Error("Patching CompPilotConsole:CompInspectStringExtra - failed to add optional current layer range info.");

        return matcher.Instructions();
    }

    private static PlanetLayer ReturnSurfaceLayer(PlanetLayer layer) => Find.WorldGrid.Surface;

    private static void InsertCurrentLayerRangeInfoIfNeeded(CompPilotConsole instance, StringBuilder builder)
    {
        var layer = instance.parent.Map.Tile.Layer;
        if (!Mathf.Approximately(layer.Def.rangeDistanceFactor, PlanetLayerDefOf.Surface.rangeDistanceFactor))
            builder.AppendInNewLine("VGE_GravshipRangeCurrentLayer".Translate(layer.Def.label).CapitalizeFirst()).Append(": ").Append(instance.GetMaxLaunchDistance(layer).ToString("F0"));
    }
}