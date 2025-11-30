using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(TurretTop), "DrawTurret")]
    public static class TurretTop_DrawTurret_Patch
    {
        public static void Prefix(TurretTop __instance, ref Vector3 drawLoc)
        {
            if (__instance.parentTurret is Building_GravshipTurret || __instance.parentTurret.def == VGEDefOf.VGE_PointDefenseTurret)
            {
                if (WorldComponent_GravshipController.CutsceneInProgress is false)
                {
                    drawLoc.y += 6.19f;
                }
            }
        }
    }
}
