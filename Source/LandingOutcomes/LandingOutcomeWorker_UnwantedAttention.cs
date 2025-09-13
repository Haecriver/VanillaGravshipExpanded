using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_UnwantedAttention : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_UnwantedAttention(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            var map = gravship.Engine.Map;
            return map.Tile.Layer is SurfaceLayer;
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            var map = gravship.Engine.Map;
            SpawnRaid(map);
            SendStandardLetter(gravship.Engine, null, gravship.Engine);
        }

        private void SpawnRaid(Map map)
        {
            var pirateFaction = Find.FactionManager.AllFactions
                .Where(f => f.def.pawnGroupMakers != null && f.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Combat))
                .Where(f => f.def.humanlikeFaction && f.HostileTo(Faction.OfPlayer))
                .Where(f => f.def.techLevel >= TechLevel.Industrial)
                .RandomElementWithFallback(null);
            if (pirateFaction == null)
            {
                pirateFaction = Find.FactionManager.AllFactions
                    .Where(f => f.HostileTo(Faction.OfPlayer))
                    .RandomElementWithFallback(Faction.OfMechanoids);
            }
            float points = Mathf.Max(100f, StorytellerUtility.DefaultThreatPointsNow(map));
            var parms = new IncidentParms
            {
                target = map,
                points = points,
                forced = true,
                faction = pirateFaction,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
                raidStrategy = RaidStrategyDefOf.ImmediateAttack
            };
            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        }
    }
}
