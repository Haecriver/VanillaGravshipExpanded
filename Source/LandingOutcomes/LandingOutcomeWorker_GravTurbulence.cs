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
            var launchSourceTile = LaunchInfo_ExposeData_Patch.launchSourceTiles[launchInfo];
            int distanceTravelled = GravshipHelper.GetDistance(launchSourceTile, landingTile);
            WorldComponent_GravshipController_LandingEnded_Patch.CalculateMaintenanceLoss(gravship, distanceTravelled,0.25f);
            SendStandardLetter(gravship.Engine, null, null);
        }
    }
}
