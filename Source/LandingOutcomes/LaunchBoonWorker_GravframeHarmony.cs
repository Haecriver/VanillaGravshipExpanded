using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_GravframeHarmony:LaunchBoonWorker
    {

        public LaunchBoonWorker_GravframeHarmony(LaunchBoonDef def)
            : base(def)
        {
        }
        public override void ApplyBoon(Gravship gravship)
        {
            SendStandardLetter(gravship.Engine, null, gravship.Engine);
        }

   
        public override bool CanTrigger(Gravship gravship)
        {
            return true;
        }
    }
}
