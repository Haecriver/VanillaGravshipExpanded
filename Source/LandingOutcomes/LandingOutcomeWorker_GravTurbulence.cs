using System.Linq;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_GravTurbulence : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_GravTurbulence(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return true;
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            var landingTile = gravship.Engine.Tile;
            var launchInfo = gravship.Engine?.launchInfo;
            var extendedInfo = launchInfo.ExtendedInfo(false);
            if (extendedInfo is { launchSourceTile.Valid: true })
            {
                int distanceTravelled = GravshipHelper.GetDistance(extendedInfo.launchSourceTile, landingTile);
                WorldComponent_GravshipController_LandingEnded_Patch.CalculateMaintenanceLoss(gravship, distanceTravelled,0.25f);
                SendStandardLetter(gravship.Engine, null, null);
            }
        }
    }
}
