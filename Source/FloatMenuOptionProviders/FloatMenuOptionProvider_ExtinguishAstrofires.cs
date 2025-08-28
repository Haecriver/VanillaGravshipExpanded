
using RimWorld;
using Verse;
using Verse.AI;
namespace VanillaGravshipExpanded
{
    public class FloatMenuOptionProvider_ExtinguishAstrofires : FloatMenuOptionProvider
    {
        public override bool Drafted => true;

        public override bool Undrafted => true;

        public override bool Multiselect => true;

        public override bool RequiresManipulation => true;

        public override FloatMenuOption GetSingleOption(FloatMenuContext context)
        {
            if (!new TargetInfo(context.ClickedCell, context.FirstSelectedPawn.Map).IsAstroBurning())
            {
                return null;
            }
            bool flag = false;
            AcceptanceReport acceptanceReport = null;
            foreach (Pawn validSelectedPawn in context.ValidSelectedPawns)
            {
                acceptanceReport = PawnCanExtinguish(validSelectedPawn, context.ClickedCell);
                if (acceptanceReport.Accepted)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return new FloatMenuOption(string.Format("{0}: {1}", "CannotGenericWorkCustom".Translate(WorkGiverDefOf.FightFires.label), acceptanceReport.Reason), null);
            }
            return new FloatMenuOption("ExtinguishFiresNearby".Translate(), delegate
            {
                foreach (Pawn validSelectedPawn2 in context.ValidSelectedPawns)
                {
                    if (PawnCanExtinguish(validSelectedPawn2, context.ClickedCell).Accepted)
                    {
                        Job job = JobMaker.MakeJob(VGEDefOf.VGE_ExtinguishAstrofiresNearby);
                        foreach (Astrofire item in context.ClickedCell.GetAstrofiresNearCell(context.FirstSelectedPawn.Map))
                        {
                            job.AddQueuedTarget(TargetIndex.A, item);
                        }
                        validSelectedPawn2.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                }
            }, VGEDefOf.VGE_Astrofire);
        }

        private AcceptanceReport PawnCanExtinguish(Pawn pawn, IntVec3 cell)
        {
            if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Firefighter))
            {
                return "IncapableOf".Translate().CapitalizeFirst() + " " + WorkTypeDefOf.Firefighter.gerundLabel;
            }
            if (!pawn.CanReach(cell, PathEndMode.ClosestTouch, Danger.Deadly) && (!cell.TryGetFirstThing(pawn.Map, out Astrofire thing) || !pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly)))
            {
                return "NoPath".Translate().CapitalizeFirst();
            }
            if (!cell.InAllowedArea(pawn))
            {
                return "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + " ( " + pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.Label + ")";
            }
            return true;
        }
    }
}