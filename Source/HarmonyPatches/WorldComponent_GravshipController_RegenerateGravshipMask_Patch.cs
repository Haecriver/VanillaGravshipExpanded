using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "RegenerateGravshipMask")]
    public static class WorldComponent_GravshipController_RegenerateGravshipMask_Patch
    {
        public static void Postfix(Map ___map)
        {
            ___map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_ScaffoldMask));
            ___map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_ScaffoldClear));
        }
    }
}
