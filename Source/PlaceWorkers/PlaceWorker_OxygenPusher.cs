using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class PlaceWorker_OxygenPusher : PlaceWorker
{
    private static readonly Color ColorSpotOxygen = new(202f / 255f, 164f / 255f, 79f / 255f, 0.6f);
    private static readonly Color ColorRoomNoVacuum = new(1f, 0f, 0f, 0.3f);
    private static readonly Color ColorRoomVacuum = new(0f, 1f, 0f, 0.3f);

    private static IntVec3 GetOffsetCell(ThingDef def, Rot4 rot) => def.GetCompProperties<CompProperties_ResourceTrader_OxygenPusher>()?.oxygenCellOffset.RotatedBy(rot) ?? IntVec3.Zero;

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        base.DrawGhost(def, center, rot, ghostCol, thing);

        var cell = center + GetOffsetCell(def, rot);
        GenDraw.DrawFieldEdges([cell], ColorSpotOxygen);

        var room = cell.GetRoom(Find.CurrentMap);
        if (room is { TouchesMapEdge: false })
            GenDraw.DrawFieldEdges(room.Cells.ToList(), room.ExposedToSpace ? ColorRoomNoVacuum : ColorRoomVacuum);
    }
}