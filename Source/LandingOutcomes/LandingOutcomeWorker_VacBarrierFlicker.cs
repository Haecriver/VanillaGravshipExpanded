using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_VacBarrierFlicker : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_VacBarrierFlicker(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return gravship.Things.Any(t => t is Building_VacBarrier && t.HasComp<CompBreakdownable>());
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            var vacBarriers = gravship.Things
                .OfType<Building_VacBarrier>()
                .Where(x => x.HasComp<CompBreakdownable>())
                .ToList();
            if (vacBarriers.Count == 0)
            {
                return;
            }
            var selectedBarrier = vacBarriers.RandomElement();
            var breakdownableComp = selectedBarrier.GetComp<CompBreakdownable>();
            breakdownableComp.DoBreakdown();
            SendStandardLetter(gravship.Engine, null, new LookTargets(selectedBarrier));
        }
    }
}
