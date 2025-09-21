using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(PlaceWorker_BuildingsValidOverSubstructure), nameof(PlaceWorker_BuildingsValidOverSubstructure.AllowsPlacing))]
public static class PlaceWorker_BuildingsValidOverSubstructure_AllowsPlacing_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var thingDefField = typeof(Thing).DeclaredField(nameof(Thing.def));
        var gravEngineField = typeof(ThingDefOf).DeclaredField(nameof(ThingDefOf.GravEngine));
        var ourMethod = typeof(PlaceWorker_BuildingsValidOverSubstructure_AllowsPlacing_Patch).DeclaredMethod(nameof(CheckIfGravEngine));

        var patchedMethods = 0;
        var instrList = instr.ToList();

        for (var i = 0; i < instrList.Count; i++)
        {
            var ci = instrList[i];

            yield return ci;

            // Look for `if (thing.def == ThingDefOf.GravEngine)`
            if (ci.LoadsField(thingDefField) && instrList[i + 1].LoadsField(gravEngineField))
            {
                // Surround `thing.def` with `CheckIfGravEngine method, which will
                // return `ThingDefOf.GravEngine` if the def is any of our grav engines.
                yield return new CodeInstruction(OpCodes.Call, ourMethod);

                patchedMethods++;
            }
        }

        const int expectedPatches = 1;
        if (patchedMethods != expectedPatches)
            Log.Error($"Patching PlaceWorker_BuildingsValidOverSubstructure:AllowsPlacing - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {patchedMethods}. There may be bugs with placing substructures under non-vanilla grav engines.");
    }

    private static ThingDef CheckIfGravEngine(ThingDef def)
    {
        if (def == VGEDefOf.VGE_GravjumperEngine || def == VGEDefOf.VGE_GravhulkEngine)
            return ThingDefOf.GravEngine;
        return def;
    }
}