using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public abstract class LandingOutcomeWorker_GravshipBase : LandingOutcomeWorker
    {
        public LandingOutcomeWorker_GravshipBase(LandingOutcomeDef def)
            : base(def)
        {
        }
        public abstract bool CanTrigger(Gravship gravship);
    }
}
