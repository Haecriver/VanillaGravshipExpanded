using System.Linq;
using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Scenario), nameof(Scenario.GetFirstConfigPage))]
public static class Scenario_GetFirstConfigPage_Patch
{
    private static void Postfix(Scenario __instance, ref Page __result)
    {
        var scenPart = __instance.AllParts.OfType<ScenPart_ChooseStartingGravship>().FirstOrDefault();
        if (scenPart == null)
            return;

        var gravshipPage = new Page_ChooseStartingGravship(scenPart)
        {
            next = __result
        };
        __result.prev = gravshipPage;
        __result = gravshipPage;
    }
}
