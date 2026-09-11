using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.MissingComponents), MethodType.Getter)]
[HarmonyPatchCategory(GravshipsMod.HarmonyLatePatchCategory)]
public static class Building_GravEngine_MissingComponents_Patch
{
    public static bool isPatchActive = false;

    private static bool Prepare() => isPatchActive;

    private static void Prefix(Building_GravEngine __instance, ref bool __state) => __state = __instance.missingComponentsDirty;

    private static void Postfix(Building_GravEngine __instance, List<GravshipComponentTypeDef> __result, bool __state)
    {
        // Only re-check if component list was dirty before the main method was called
        if (!__state)
            return;

        // Remove all components that vanilla code added, but we had a multipurpose component present for it
        for (var i = __result.Count - 1; i >= 0; i--)
        {
            for (var j = 0; j < __instance.GravshipComponents.Count; j++)
            {
                var extension = __instance.GravshipComponents[j].parent.def.GetModExtension<AdditionalGravshipComponentTypesExtension>();
                if (extension != null && extension.additionalComponentTypeDefs.NotNullAndContains(__result[i]))
                {
                    __result.RemoveAt(i);
                    break;
                }
            }
        }
    }
}