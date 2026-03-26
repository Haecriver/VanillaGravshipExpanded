using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class JobDriver_BuildVacBarrierRoof : JobDriver_AffectRoof
{
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(() => !Map.areaManager.BuildVacBarrierRoof()[Cell]);
        this.FailOn(() => !RoofCollapseUtility.WithinRangeOfRoofHolder(Cell, Map));
        this.FailOn(() => !RoofCollapseUtility.ConnectedToRoofHolder(Cell, Map, true));

        foreach (var makeNewToil in base.MakeNewToils())
            yield return makeNewToil;
    }

    public override void DoEffect()
    {
        for (var i = 0; i < 9; i++)
        {
            var pos = Cell + GenAdj.AdjacentCellsAndInside[i];
            if (pos.InBounds(Map) && Map.areaManager.BuildVacBarrierRoof()[pos] && pos.GetRoof(Map).CanReplaceWithVacBarrier() && RoofCollapseUtility.WithinRangeOfRoofHolder(pos, Map))
            {
                Map.roofGrid.SetRoof(pos, VGEDefOf.VGE_VacBarrierRoof);
                // MoteMaker.PlaceTempRoof(pos, Map); // Doesn't look well with vac barrier roofs

                // Wake up dormant mortars (mechanoid cluster mortars?) when building a roof over them
                var things = Map.thingGrid.ThingsListAtFast(pos);
                for (var j = 0; j < things.Count; j++)
                {
                    var thing = things[j];
                    if (thing.def.building is { IsMortar: true } && thing.TryGetComp<CompWakeUpDormant>(out var comp))
                        comp.Activate(pawn);
                }
            }
        }
    }

    public override bool DoWorkFailOn() => !Cell.GetRoof(Map).CanReplaceWithVacBarrier();
}