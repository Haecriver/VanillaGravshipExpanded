using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_CaravanEncounter : LaunchBoonWorker
    {
        public LaunchBoonWorker_CaravanEncounter(LaunchBoonDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            var map = gravship.Engine.Map;
            return map.Tile.Layer is SurfaceLayer;
        }

        public override void ApplyBoon(Gravship gravship)
        {
            var map = gravship.Engine.Map;
            SpawnTradeCaravan(map);
            SendStandardLetter(gravship.Engine, null, gravship.Engine);
        }

        private void SpawnTradeCaravan(Map map)
        {
            var traderFaction = Find.FactionManager.AllFactions
                .Where(f => f.def.caravanTraderKinds != null && f.def.caravanTraderKinds.Any())
                .Where(f => !f.HostileTo(Faction.OfPlayer))
                .RandomElementWithFallback(null);

            if (traderFaction == null)
            {
                return;
            }
            
            var traderKind = traderFaction.def.caravanTraderKinds.RandomElement();
            var parms = StorytellerUtility.DefaultParmsNow(IncidentDefOf.TraderCaravanArrival.category, map);
            parms.forced = true;
            parms.faction = traderFaction;
            parms.traderKind = traderKind;
            IncidentDefOf.TraderCaravanArrival.Worker.TryExecute(parms);
        }
    }
}
