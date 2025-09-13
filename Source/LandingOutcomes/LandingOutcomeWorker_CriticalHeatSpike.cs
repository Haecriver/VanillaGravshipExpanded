using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_CriticalHeatSpike : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_CriticalHeatSpike(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return true;
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            var heatManager = gravship.Engine.GetComp<CompHeatManager>();
            if (LaunchInfo_ExposeData_Patch.lastCost.TryGetValue(gravship.Engine.launchInfo, out float cost))
            {
                heatManager.AddHeat(cost);
                SendStandardLetter(gravship.Engine, null, gravship.Engine);
            }
        }
    }
}
