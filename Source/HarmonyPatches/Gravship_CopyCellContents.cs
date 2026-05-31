using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Gravship), nameof(Gravship.CopyCellContents))]
public static class Gravship_CopyCellContents
{
    // Used to verify that the current gravship matches, just in case
    public static Gravship currentGravship;
    // Actual data
    public static Dictionary<IntVec3, Color> tempVacBarrierRoofColors = null;

    private static void Postfix(Map oldMap, IntVec3 origin, Gravship __instance, Building_GravEngine ___engine, Dictionary<IntVec3, RoofDef> ___roofs)
    {
        if (___roofs.NullOrEmpty())
            return;

        // We can't write directly to the comp, as it's not initialized yet. We either need to create it ourselves, or use a workaround.
        currentGravship = __instance;
        tempVacBarrierRoofColors = new Dictionary<IntVec3, Color>();
        foreach (var (pos, _) in ___roofs)
        {
            var mapPos = pos + origin;
            var color = oldMap.VacBarrierRoofColorAt(mapPos);
            if (color != null)
                tempVacBarrierRoofColors[pos] = color.Value;
        }
    }
}