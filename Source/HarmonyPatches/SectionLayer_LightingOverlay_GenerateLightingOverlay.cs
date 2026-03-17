using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(SectionLayer_LightingOverlay), nameof(SectionLayer_LightingOverlay.GenerateLightingOverlay))]
public static class SectionLayer_LightingOverlay_GenerateLightingOverlay_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            CodeMatch.IsLdloc(),
            CodeMatch.IsLdloc(),
            CodeMatch.Calls(typeof(RoofGrid).DeclaredMethod(nameof(RoofGrid.RoofAt), [typeof(int)])),
            CodeMatch.IsStloc()
        );

        matcher.Insert(CodeInstruction.Call(() => RoofAtWrapper));

        return matcher.Instructions();
    }

    private static RoofDef RoofAtWrapper(RoofDef def)
    {
        if (def == null || def == VGEDefOf.VGE_VacBarrierRoof)
            return null;
        return def;
    }
}