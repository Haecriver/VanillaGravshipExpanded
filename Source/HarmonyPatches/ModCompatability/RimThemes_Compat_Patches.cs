using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
[HarmonyPatchCategory(GravshipsMod.HarmonyLatePatchCategory)]
public static class RimThemes_Compat_Patches
{
    private static bool Prepare() => ModLister.AnyModActiveNoSuffix(["aRandomKiwi.RimThemes"]);

    private static MethodBase TargetMethod() => AccessTools.DeclaredMethod("aRandomKiwi.RimThemes.Utils:isNSBlacklistedWindowsType");

    private static void Postfix(Window win, ref bool __result)
    {
        if (!__result && win is Dialog_BeginGravshipLaunch)
            __result = true;
    }
}