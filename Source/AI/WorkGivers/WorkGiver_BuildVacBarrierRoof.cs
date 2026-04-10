using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class WorkGiver_BuildVacBarrierRoof : WorkGiver_Scanner
{
    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn) => pawn.Map.areaManager.BuildVacBarrierRoof().ActiveCells;

    public override bool ShouldSkip(Pawn pawn, bool forced = false) => pawn.Map.areaManager.BuildVacBarrierRoof().TrueCount == 0;

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool AllowUnreachable => true;

    public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        if (!pawn.Map.areaManager.BuildVacBarrierRoof()[c])
            return false;

        if (!c.GetRoof(pawn.Map).CanReplaceWithVacBarrier())
            return false;

        if (!pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Ceiling, forced))
            return false;

        if (!pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger()) && BuildingToTouchToBeAbleToBuildRoof(c, pawn) == null)
            return false;

        if (!RoofCollapseUtility.WithinRangeOfRoofHolder(c, pawn.Map))
            return false;

        if (!RoofCollapseUtility.ConnectedToRoofHolder(c, pawn.Map, true))
            return false;

        return true;
        // var thing = RoofUtility.FirstBlockingThing(c, pawn.Map);
        // return thing == null || RoofUtility.CanHandleBlockingThing(thing, pawn, forced);
    }

    private Building BuildingToTouchToBeAbleToBuildRoof(IntVec3 c, Pawn pawn)
    {
        if (c.Standable(pawn.Map))
            return null;

        var edifice = c.GetEdifice(pawn.Map);
        if (edifice == null)
            return null;

        if (!pawn.CanReach(edifice, PathEndMode.Touch, pawn.NormalMaxDanger()))
            return null;

        return edifice;
    }

    public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        // var thing = RoofUtility.FirstBlockingThing(c, pawn.Map);
        // if (thing != null)
        //     return RoofUtility.HandleBlockingThingJob(thing, pawn, forced);

        LocalTargetInfo localTargetInfo;
        if (!pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger()))
            localTargetInfo = BuildingToTouchToBeAbleToBuildRoof(c, pawn);
        else
            localTargetInfo = c;

        return JobMaker.MakeJob(VGEDefOf.VGE_BuildVacBarrierRoofs, c, localTargetInfo);
    }
}