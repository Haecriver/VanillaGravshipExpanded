using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class JobDriver_InspectMechanoidGravEngine : JobDriver
{
    private const TargetIndex GravEngineIndex = TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed);

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(GravEngineIndex);
        this.FailOn(() => Find.ResearchManager.gravEngineInspected);
        this.FailOn(() => TargetA.Thing.TryGetComp<CompGravEngineInspectable>() == null);

        yield return Toils_Goto.GotoThing(GravEngineIndex, PathEndMode.Touch);
        yield return Toils_General.WaitWith(GravEngineIndex, TargetA.Thing.TryGetComp<CompGravEngineInspectable>().Props.inspectDuration, true, false, false, GravEngineIndex);
        var toil = ToilMaker.MakeToil();
        toil.initAction = () =>
        {
            if (TargetA.Thing is ThingWithComps thing && thing.GetComp<CompGravEngineInspectable>() is { } comp)
                GravshipHelper.InspectGravEngine(thing, questTags: thing.questTags, setToPlayerFaction: comp.Props.setToPlayerFaction);
        };
        yield return toil;
    }
}