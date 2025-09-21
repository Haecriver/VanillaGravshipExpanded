using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.GetPlayerGravEngine_NewTemp))]
public static class GravshipUtility_GetPlayerGravEngine_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var listerThingsMethodTarget = typeof(ListerThings).DeclaredMethod(nameof(ListerThings.ThingsOfDef));
        var listerThingsMethodReplacement = typeof(GravshipUtility_GetPlayerGravEngine_Patch).DeclaredMethod(nameof(ReplacedThingsOfDef));

        var listThingFindTarget = typeof(List<Thing>).DeclaredMethod(nameof(List<Thing>.Find));
        var listThingFindWrapper = typeof(GravshipUtility_GetPlayerGravEngine_Patch).DeclaredMethod(nameof(SearchForMinifiedEngineRecursively));

        var replacedThingsOfDefCalls = 0;
        var insertedCallsAfterListFind = 0;

        foreach (var ci in instr)
        {
            yield return ci;

            if (ci.Calls(listerThingsMethodTarget))
            {
                // Replace the vanilla method call with our own
                ci.opcode = OpCodes.Call;
                ci.operand = listerThingsMethodReplacement;

                replacedThingsOfDefCalls++;
            }
            else if (ci.Calls(listThingFindTarget))
            {
                // Load the map argument
                yield return CodeInstruction.LoadArgument(0);
                // Load the List<Thing> local, so we can reuse it
                yield return CodeInstruction.LoadLocal(2);
                // Call our method
                yield return new CodeInstruction(OpCodes.Call, listThingFindWrapper);

                insertedCallsAfterListFind++;
            }
        }

        const int expectedThingsOfDefCalls = 2;
        const int expectedListFindCalls = 1;

        if (replacedThingsOfDefCalls != expectedThingsOfDefCalls)
            Log.Error($"Patching GravshipUtility:GetPlayerGravEngine - unexpected amount of patches. Expected patches: {expectedThingsOfDefCalls}, actual patch amount: {replacedThingsOfDefCalls}. Game may fail to find custom VE grav engines.");
        if (insertedCallsAfterListFind != expectedListFindCalls)
            Log.Error($"Patching GravshipUtility:GetPlayerGravEngine - unexpected amount of patches. Expected patches: {expectedListFindCalls}, actual patch amount: {insertedCallsAfterListFind}. Game may fail to find custom VE grav engines that were minified.");
    }

    private static List<Thing> ReplacedThingsOfDef(ListerThings lister, ThingDef def)
    {
        // The original call we replaced
        var list = lister.ThingsOfDef(def);

        // If the original call didn't find anything, attempt to search for our engines
        if (list.Count == 0)
        {
            // If the search was for a grav engine, search for our engines
            if (def == ThingDefOf.GravEngine)
            {
                list = lister.ThingsOfDef(VGEDefOf.VGE_GravjumperEngine);
                if (list.Count == 0)
                    list = lister.ThingsOfDef(VGEDefOf.VGE_GravhulkEngine);
            }
            // If the search was for a minified grav engine, search for our minified engines
            else if (def == ThingDefOf.GravEngine.minifiedDef)
            {
                list = lister.ThingsOfDef(VGEDefOf.VGE_GravjumperEngine.minifiedDef);
                if (list.Count == 0)
                    list = lister.ThingsOfDef(VGEDefOf.VGE_GravhulkEngine.minifiedDef);
            }
        }

        return list;
    }

    private static Thing SearchForMinifiedEngineRecursively(Thing thing, Map map, List<Thing> list)
    {
        // Vanilla found something, let it use it. Otherwise, search for minified VE grav engines.
        return thing ?? GetThing(VGEDefOf.VGE_GravjumperEngine.minifiedDef) ?? GetThing(VGEDefOf.VGE_GravhulkEngine.minifiedDef);

        Thing GetThing(ThingDef minifiedDef)
        {
            list.Clear();
            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForDef(minifiedDef), list, true, null, false);
            return list.Find(x => x.GetInnerIfMinified()?.def == minifiedDef);
        }
    }
}