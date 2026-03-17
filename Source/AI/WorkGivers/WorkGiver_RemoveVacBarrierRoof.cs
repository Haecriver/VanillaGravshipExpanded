using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class WorkGiver_RemoveVacBarrierRoof : WorkGiver_Scanner
{
    public override bool Prioritized => true;

    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn) => pawn.Map.areaManager.NoVacBarrierRoof().ActiveCells;

    public override bool ShouldSkip(Pawn pawn, bool forced = false) => pawn.Map.areaManager.NoVacBarrierRoof().TrueCount == 0;

    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        return pawn.Map.areaManager.NoVacBarrierRoof()[c] && c.GetRoof(pawn.Map) == VGEDefOf.VGE_VacBarrierRoof && pawn.CanReserve(c, layer: ReservationLayerDefOf.Ceiling, ignoreOtherReservations: forced);
    }

    public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        return JobMaker.MakeJob(VGEDefOf.VGE_RemoveVacBarrierRoofs, c, c);
    }

    public override float GetPriority(Pawn pawn, TargetInfo t)
    {
        var cell = t.Cell;
        var num = 0;

        for (var i = 0; i < 8; i++)
        {
            var adjCell = cell + GenAdj.AdjacentCells[i];
            if (adjCell.InBounds(t.Map))
            {
                var edifice = adjCell.GetEdifice(t.Map);
                if (edifice != null && edifice.def.holdsRoof)
                    return -60f;
                if (adjCell.GetRoof(pawn.Map) != null)
                    num++;
            }
        }

        return -Math.Min(num, 3);
    }
}