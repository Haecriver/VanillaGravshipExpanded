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
        => ListAllAllowedGravEngines(instr, baseMethod, 0, 2);

    internal static IEnumerable<CodeInstruction> ListAllAllowedGravEngines(IEnumerable<CodeInstruction> instr, MethodBase baseMethod, int gravshipFacilityPropsIndex, int potentialLinkedThingIndex)
    {
        var targetMethod = typeof(ListerThings).DeclaredMethod(nameof(ListerThings.ThingsOfDef));
        var replacementMethod = typeof(PlaceWorker_InRangeOfGravEngine_DrawGhost_Patch).DeclaredMethod(nameof(AllLinkableBuildings));

        var targetField = typeof(CompProperties_Facility).DeclaredField(nameof(CompProperties_Facility.maxDistance));
        var fieldWrapperMethod = typeof(PlaceWorker_InRangeOfGravEngine_DrawGhost_Patch).DeclaredMethod(nameof(ModifyMaxDistance));

        var patchedMethods = 0;
        var patchedFields = 0;

        foreach (var ci in instr)
        {
            if (ci.Calls(targetMethod))
            {
                // Load the props field
                yield return CodeInstruction.LoadLocal(gravshipFacilityPropsIndex);

                // Replace the call with our method
                ci.opcode = OpCodes.Call;
                ci.operand = replacementMethod;

                patchedMethods++;
            }

            yield return ci;

            if (ci.LoadsField(targetField))
            {
                // Load the Thing local used in the loop
                yield return CodeInstruction.LoadLocal(potentialLinkedThingIndex);
                // Insert our wrapper method
                yield return new CodeInstruction(OpCodes.Call, fieldWrapperMethod);

                patchedFields++;
            }
        }

        const int expectedMethodReplacements = 1;
        const int expectedFieldPatches = 1;

        if (patchedMethods != expectedMethodReplacements)
            Log.Error($"Patching {baseMethod.DeclaringType}:{baseMethod.Name} - unexpected amount of patches. Expected patches: {expectedMethodReplacements}, actual patch amount: {patchedMethods}. There may be bugs with linking some buildings to non-vanilla grav engines.");
        if (patchedFields != expectedFieldPatches)
            Log.Error($"Patching {baseMethod.DeclaringType}:{baseMethod.Name} - unexpected amount of patches. Expected patches: {expectedFieldPatches}, actual patch amount: {patchedFields}. There may be bugs with grav field extenders/amplifiers having incorrect linking range.");
    }

    private static List<Thing> AllLinkableBuildings(ListerThings lister, ThingDef _, CompProperties_GravshipFacility comp)
    {
        TmpWorkingList.Clear();

        foreach (var building in comp.linkableBuildings)
            TmpWorkingList.AddRange(lister.ThingsOfDef(building));

        return TmpWorkingList;
    }

    private static float ModifyMaxDistance(float maxDistance, Thing thing) => thing.TryGetComp<CompSubstructureFootprint>()?.Props.radius ?? maxDistance;
}