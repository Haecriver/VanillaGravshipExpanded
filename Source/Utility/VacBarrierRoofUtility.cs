using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using VEF.Maps;
using Verse;

namespace VanillaGravshipExpanded;

public static class VacBarrierRoofUtility
{
    public static readonly Color BaseColor = VGEDefOf.VGE_VacBarrierRoof?.GetModExtension<RoofExtension>()?.customRoofGraphic?.customRoofGraphicColor ?? Color.white;
    public static readonly HashSet<RoofDef> ReplaceableRoofDefs = [RoofDefOf.RoofConstructed, RoofDefOf.RoofRockThin];

    public static Area_BuildVacBarrierRoof BuildVacBarrierRoof(this AreaManager manager) => manager.Get<Area_BuildVacBarrierRoof>();

    public static bool CanReplaceWithVacBarrier(this RoofDef roof) => roof == null || ReplaceableRoofDefs.Contains(roof);

    public static Color? VacBarrierRoofColorAt(this Map map, IntVec3 pos)
    {
        if (map.roofGrid.RoofAt(pos)?.GetModExtension<RoofExtension>()?.customRoofGraphic is ColorableVacBarrierRoofGraphic)
            return MaintenanceAndDeterioration_MapComponent.GetCompFast(map).vacBarrierColorGrid[map.cellIndices.CellToIndex(pos)];
        return null;
    }

    public static Color? VacBarrierRoofColorAtSafe(this Map map, IntVec3 pos)
    {
        if (pos.InBounds(map))
            return map.VacBarrierRoofColorAt(pos);
        return null;
    }

    public static void SetVacBarrierRoofColorAt(this Map map, IntVec3 pos, Color color)
    {
        if (map.roofGrid.RoofAt(pos)?.GetModExtension<RoofExtension>()?.customRoofGraphic is ColorableVacBarrierRoofGraphic)
            MaintenanceAndDeterioration_MapComponent.GetCompFast(map).vacBarrierColorGrid[map.cellIndices.CellToIndex(pos)] = color;
    }

    public static void SetVacBarrierRoofColorAtSafe(this Map map, IntVec3 pos, Color color)
    {
        if (pos.InBounds(map))
            map.SetVacBarrierRoofColorAt(pos, color);
    }
    
    public static bool IsGravBarrierRoofAccessible()
    {
        // Always accessible if dev mode gizmos visible
        if (DebugSettings.ShowDevGizmos)
            return true;
        if (VGEDefOf.VacBarrier != null)
        {
            if (VGEDefOf.VacBarrier.IsResearchFinished)
                return true;
        }
        else if (VGEDefOf.OrbitalTech != null)
        {
            if (VGEDefOf.OrbitalTech.IsFinished)
                return true;
        }
        else
        {
            Log.ErrorOnce("[VGE] Both Vac Barrier building and Orbital Tech research project are missing from the game, unable to determine if vac barrier roofs should be accessible.", -680002143);
            return true;
        }

        return false;
    }
}