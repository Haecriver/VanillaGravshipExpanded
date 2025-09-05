using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "TakeoffEnded")]
    public static class WorldComponent_GravshipController_TakeoffEnded_Patch
    {
        public static void Prefix(WorldComponent_GravshipController __instance)
        {
            var map = __instance.map;
            Find.WindowStack.Add(new Dialog_MessageBox("VGE_MapDecisionText".Translate(), "VGE_KeepMap".Translate(), null, "VGE_DiscardMap".Translate(), delegate
            {
                map.Parent.Abandon(wasGravshipLaunch: true);
            }, "VGE_MapDecisionTitle".Translate(), buttonADestructive: false));
        }
    }
}
