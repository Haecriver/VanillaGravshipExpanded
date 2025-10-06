using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(Dialog_BeginLordJob), "InitialSize", MethodType.Getter)]
    public static class Dialog_BeginLordJob_InitialSize_Patch
    {
        public static void Postfix(Dialog_BeginLordJob __instance, ref Vector2 __result)
        {
            if (__instance is Dialog_BeginGravshipLaunch)
            {
                __result.y += 100f;
            }
        }
    }
}
