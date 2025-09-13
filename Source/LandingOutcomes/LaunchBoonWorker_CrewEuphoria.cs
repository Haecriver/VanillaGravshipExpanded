using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_CrewEuphoria : LaunchBoonWorker
    {
        public LaunchBoonWorker_CrewEuphoria(LaunchBoonDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return true;
        }

        public override void ApplyBoon(Gravship gravship)
        {
            var engine = gravship.Engine;
            var pawns = engine.Map.mapPawns.FreeColonists.ToList();
            foreach (var pawn in pawns)
            {
                if (pawn.needs?.mood?.thoughts?.memories != null && pawn.IsColonist)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(VGEDefOf.VGE_CrewEuphoria);
                }
            }
            
            SendStandardLetter(engine, null, engine);
        }
    }
}
