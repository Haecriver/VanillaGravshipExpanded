using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.HasSignalJammer), MethodType.Getter)]
[HarmonyPatchCategory(GravshipsMod.HarmonyLatePatchCategory)]
public static class Building_GravEngine_HasSignalJammer_Patch
{
    public static bool isPatchActive = false;

    private static bool Prepare() => isPatchActive;

    private static void Postfix(Building_GravEngine __instane, ref bool __result)
    {
        // Don't do anything if an actual signal jammer is present
        if (__result)
            return;

        for (var i = 0; i < __instane.GravshipComponents.Count; i++)
        {
            var extension = __instane.GravshipComponents[i].parent.def.GetModExtension<AdditionalGravshipComponentTypesExtension>();
            if (extension != null && extension.additionalComponentTypeDefs.NotNullAndContains(GravshipComponentTypeDefOf.SignalJammer))
            {
                __result = true;
                return;
            }
        }
    }
}