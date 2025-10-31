using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(PlaceWorker_InRangeOfGravEngine), nameof(PlaceWorker_InRangeOfGravEngine.AllowsPlacing))]
public class PlaceWorker_InRangeOfGravEngine_AllowsPlacing_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
        => PlaceWorker_InRangeOfGravEngine_DrawGhost_Patch.ListAllAllowedGravEngines(instr, baseMethod, 1, 3);
}