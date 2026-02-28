using System.Collections.Generic;
using System.Linq;
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

        var listConstructor = typeof(List<Thing>).DeclaredConstructor([]);
        var listLocIndex = -1; // 3 in 1.6.4739-beta, 2 before that

        var replacedThingsOfDefCalls = 0;
        var insertedCallsAfterListFind = 0;

        var instrList = instr.ToList();
        for (var i = 0; i < instrList.Count; i++)
        {
            var ci = instrList[i];
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
                if (listLocIndex == -1)
                {
                    Log.Error("[VGE] Patching GravshipUtility:GetPlayerGravEngine - failed to find List<Thing> constructor index. Game may fail to find custom VE grav engines that were minified.");
                }
                else
                {
                    // Load the map argument
                    yield return CodeInstruction.LoadArgument(0);
                    // Load the List<Thing> local, so we can reuse it
                    yield return CodeInstruction.LoadLocal(listLocIndex);
                    // Call our method
                    yield return new CodeInstruction(OpCodes.Call, listThingFindWrapper);

                    insertedCallsAfterListFind++;
                }
            }
            else if (listLocIndex == -1 && ci.opcode == OpCodes.Newobj && ci.operand is ConstructorInfo ctor && ctor == listConstructor && i + 1 < instrList.Count)
            {
                listLocIndex = instrList[i + 1].LocalIndex();
            }
        }

        const int expectedThingsOfDefCalls = 2;
        const int expectedListFindCalls = 1;

        if (replacedThingsOfDefCalls != expectedThingsOfDefCalls)
            Log.Error($"[VGE] Patching GravshipUtility:GetPlayerGravEngine - unexpected amount of patches. Expected patches: {expectedThingsOfDefCalls}, actual patch amount: {replacedThingsOfDefCalls}. Game may fail to find custom VE grav engines.");
        if (insertedCallsAfterListFind != expectedListFindCalls)
            Log.Error($"[VGE] Patching GravshipUtility:GetPlayerGravEngine - unexpected amount of patches. Expected patches: {expectedListFindCalls}, actual patch amount: {insertedCallsAfterListFind}. Game may fail to find custom VE grav engines that were minified.");
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