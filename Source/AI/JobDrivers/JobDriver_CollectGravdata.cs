using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace VanillaGravshipExpanded
{
    public class JobDriver_CollectGravdata : JobDriver
    {
        private Building GravtechConsole => (Building)TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(GravtechConsole, job, 1, -1, null, errorOnFailed))
            {
                if (GravtechConsole.def.hasInteractionCell)
                {
                    return pawn.ReserveSittableOrSpot(GravtechConsole.InteractionCell, job, errorOnFailed);
                }
                return true;
            }
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil collectGravdata = ToilMaker.MakeToil();
            collectGravdata.tickIntervalAction = delegate (int delta)
            {
                Pawn actor = collectGravdata.actor;
                float gravshipResearchSpeed = actor.GetStatValue(VGEDefOf.VGE_GravshipResearch);
                float effectiveSpeed = gravshipResearchSpeed / 10f;
                effectiveSpeed *= GravtechConsole.GetStatValue(StatDefOf.ResearchSpeedFactor);
                float progressToAdd = effectiveSpeed * delta * 0.00825f;
                // Find.ResearchManager.ResearchPerformed adds progress to the current vanilla research project.
                // It also scales it by the tech level of the faction.
                // However, it does multiply the progress by 0.00825 (done above) and storyteller's research speed factor.
                GravshipResearchUtility.ResearchPerformed(progressToAdd, actor);
                actor.skills.Learn(SkillDefOf.Intellectual, 0.1f * delta);
                actor.GainComfortFromCellIfPossible(delta, chairsOnly: true);
            };
            collectGravdata.FailOn(() => !WorkGiver_CollectGravdata.CanResearchAt(pawn, TargetA.Thing));
            collectGravdata.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            collectGravdata.WithEffect(EffecterDefOf.Research, TargetIndex.A);
            collectGravdata.WithProgressBar(TargetIndex.A, () => World_ExposeData_Patch.currentGravtechProject?.ProgressPercent ?? 0f);
            collectGravdata.defaultCompleteMode = ToilCompleteMode.Delay;
            collectGravdata.defaultDuration = 4000;
            collectGravdata.activeSkill = () => SkillDefOf.Intellectual;
            yield return collectGravdata;
            yield return Toils_General.Wait(2);
        }
    }
}
