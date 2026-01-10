using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(GenDraw), nameof(GenDraw.DrawAimPie))]
    public static class GenDraw_DrawAimPie_Patch
    {
        public static void Prefix(Thing shooter, ref float offsetDist)
        {
            if (shooter is Building_GravshipTurret)
            {
                offsetDist += 5f;
            }
        }
    }
}
