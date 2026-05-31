using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GravshipPlacementUtility), nameof(GravshipPlacementUtility.SpawnRoofs))]
public static class GravshipPlacementUtility_SpawnRoofs
{
    private static void Postfix(Gravship gravship, Map map, IntVec3 root)
    {
        // We DON'T want to ever error on this, as this will break vanilla landing otherwise.
        // Wrap in try/catch for extra safety to prevent any exception from slipping through.
        try
        {
            var colors = gravship.GetComponent<ExtraGravshipDataComp>()?.VacBarrierRoofColors;
            if (colors != null)
            {
                foreach (var (pos, color) in colors)
                    map.SetVacBarrierRoofColorAt(root + pos, color);
            }
        }
        catch (Exception e)
        {
            Log.Error($"[VGE] Exception caught trying to apply vac barrier roof colors after landing. Exception:\n{e}");
        }
    }
}