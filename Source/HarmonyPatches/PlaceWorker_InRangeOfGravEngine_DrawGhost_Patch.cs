using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(PlaceWorker_InRangeOfGravEngine), nameof(PlaceWorker_InRangeOfGravEngine.DrawGhost))]
public class PlaceWorker_InRangeOfGravEngine_DrawGhost_Patch
{
    private static List<Thing> TmpWorkingList = [];

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
        => ListAllAllowedGravEngines(instr, baseMethod, 0);

    internal static IEnumerable<CodeInstruction> ListAllAllowedGravEngines(IEnumerable<CodeInstruction> instr, MethodBase baseMethod, int gravshipFacilityPropsIndex)
    {
        var targetMethod = typeof(ListerThings).DeclaredMethod(nameof(ListerThings.ThingsOfDef));
        var replacementMethod = typeof(PlaceWorker_InRangeOfGravEngine_DrawGhost_Patch).DeclaredMethod(nameof(AllLinkableBuildings));

        var patchedMethods = 0;

        foreach (var ci in instr)
        {
            if (ci.Calls(targetMethod))
            {
                yield return CodeInstruction.LoadLocal(gravshipFacilityPropsIndex);

                ci.opcode = OpCodes.Call;
                ci.operand = replacementMethod;

                patchedMethods++;
            }

            yield return ci;
        }

        const int expectedPatches = 1;
        if (patchedMethods != expectedPatches)
            Log.Error($"Patching {baseMethod.DeclaringType}:{baseMethod.Name} - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {patchedMethods}. There may be bugs with linking some buildings to non-vanilla grav engines.");
    }

    private static List<Thing> AllLinkableBuildings(ListerThings lister, ThingDef _, CompProperties_GravshipFacility comp)
    {
        TmpWorkingList.Clear();

        foreach (var building in comp.linkableBuildings)
            TmpWorkingList.AddRange(lister.ThingsOfDef(building));

        return TmpWorkingList;
    }
}