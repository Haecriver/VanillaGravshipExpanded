using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class JobDriver_RemoveVacBarrierRoof : JobDriver_AffectRoof
{
    private static readonly List<IntVec3> RemovedRoofs = [];

    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(() => !Map.areaManager.NoVacBarrierRoof()[Cell]);

        foreach (var makeNewToil in base.MakeNewToils())
            yield return makeNewToil;
    }

    public override void DoEffect()
    {
        // If an area has a build roof area, replace vac barrier roof
        if (Map.areaManager.BuildRoof[Cell])
        {
            Map.roofGrid.SetRoof(Cell, RoofDefOf.RoofConstructed);
            MoteMaker.PlaceTempRoof(Cell, Map);
        }
        // If there's no build roof area, remove vac barrier roof
        else
        {
            RemovedRoofs.Clear();
            Map.roofGrid.SetRoof(Cell, null);
            RemovedRoofs.Add(Cell);
            RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(RemovedRoofs, Map, true);
            RemovedRoofs.Clear();
        }
    }

    public override bool DoWorkFailOn()
    {
        return Cell.GetRoof(Map) != VGEDefOf.VGE_VacBarrierRoof;
    }
}