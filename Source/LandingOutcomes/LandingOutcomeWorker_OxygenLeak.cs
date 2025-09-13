using System.Linq;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_OxygenLeak : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_OxygenLeak(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return gravship.Things.Any(t =>
            {
                var storageComp = t.TryGetComp<CompResourceStorage>();
                return storageComp != null && storageComp.Props.pipeNet == VGEDefOf.VGE_OxygenNet;
            });
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            var oxygenTanks = gravship.Things
                .Where(t =>
                {
                    var storageComp = t.TryGetComp<CompResourceStorage>();
                    return storageComp != null && storageComp.Props.pipeNet == VGEDefOf.VGE_OxygenNet;
                }).ToList();

            if (oxygenTanks.Count == 0)
            {
                return;
            }
            var selectedTank = oxygenTanks.RandomElement();
            var breakdownableComp = selectedTank.TryGetComp<CompBreakdownable>();
            breakdownableComp.DoBreakdown();
            var storageComp = selectedTank.TryGetComp<CompResourceStorage>();
            storageComp.Empty();
            SendStandardLetter(gravship.Engine, null, new LookTargets(selectedTank));
        }
    }
}
