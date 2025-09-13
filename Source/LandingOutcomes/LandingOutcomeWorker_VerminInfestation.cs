using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_VerminInfestation : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_VerminInfestation(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return gravship.Things.Any(t => t is Building_Vent);
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            var map = gravship.Engine.Map;
            var vents = gravship.Things.Where(t => t is Building_Vent).ToList();
            foreach (var vent in vents)
            {
                int ratCount = Rand.RangeInclusive(1, 2);
                for (int i = 0; i < ratCount; i++)
                {
                    var rat = PawnGenerator.GeneratePawn(VGEDefOf.Rat);
                    var nearbyCell = GenRadial.RadialCellsAround(vent.Position, 1, true).Where(x => x.InBounds(map) && x.WalkableBy(map, rat) && x.GetTerrain(map) != TerrainDefOf.Space).RandomElement();
                    GenSpawn.Spawn(rat, nearbyCell, map);
                    rat.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, forced: true);
                }
            }
            SendStandardLetter(gravship.Engine, null, new LookTargets(vents));
        }
    }
}
