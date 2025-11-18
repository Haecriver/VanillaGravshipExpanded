using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Gravship), nameof(Gravship.Label), MethodType.Getter)]
public static class Gravship_Label_Patch
{
    private static void Postfix(Gravship __instance, ref string __result)
    {
        // Ship was renamed, do nothing
        if (!__instance.engine.nameHidden)
            return;

        if (__instance.engine.def == VGEDefOf.VGE_GravjumperEngine)
            __result = "VGE_Gravjumper".Translate();
        else if (__instance.engine.def == VGEDefOf.VGE_GravhulkEngine)
            __result = "VGE_Gravhulk".Translate();
    }
}