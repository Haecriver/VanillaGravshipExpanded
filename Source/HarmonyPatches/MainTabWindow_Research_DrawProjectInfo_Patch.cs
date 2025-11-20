using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.DrawProjectInfo))]
public static class MainTabWindow_Research_DrawProjectInfo_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            CodeMatch.Calls(typeof(Find).DeclaredPropertyGetter(nameof(Find.ResearchManager))),
            new CodeMatch(OpCodes.Ldnull),
            CodeMatch.Calls(() => ((ResearchManager)null).GetProject)
        ).InsertAfter(
            CodeInstruction.Call(() => GetProjectWrapper)
        );

        return matcher.Instructions();
    }

    private static ResearchProjectDef GetProjectWrapper(ResearchProjectDef currentProject)
    {
        var tab = ((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).CurTab;
        // If a gravtech research tab, use the current gravtech project
        if (tab.IsGravshipResearchTab())
            return World_ExposeData_Patch.currentGravtechProject;
        // Return current project, as we're on a normal tab
        return currentProject;
    }
}