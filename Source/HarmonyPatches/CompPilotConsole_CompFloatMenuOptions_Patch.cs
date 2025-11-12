using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompPilotConsole), nameof(CompPilotConsole.CompFloatMenuOptions))]
public static class CompPilotConsole_CompFloatMenuOptions_Patch
{
    private static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> options, CompPilotConsole __instance)
    {
        var pilotGravshipText = ((string)"PilotGravship".Translate()).TrimEnd();
    
        foreach (var option in options)
        {
            if (option.Label == pilotGravshipText)
            {
                if (__instance.parent.def == VGEDefOf.VGE_PilotCockpit)
                    option.Label = "VGE_PilotGravjumper".Translate();
                else if (__instance.parent.def == VGEDefOf.VGE_PilotBridge)
                    option.Label = "VGE_PilotGravhulk".Translate();
            }
    
            yield return option;
        }
    }
}