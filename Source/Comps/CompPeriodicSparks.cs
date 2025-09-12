using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_PeriodicSparks : CompProperties
    {
        public int sparkInterval = 150;
        public int smokeInterval = 90;
        public CompProperties_PeriodicSparks() { this.compClass = typeof(CompPeriodicSparks); }
    }
    
    public class CompPeriodicSparks : ThingComp
    {
        public CompProperties_PeriodicSparks Props => (CompProperties_PeriodicSparks)props;
        public override void CompTick()
        {
            if (this.parent.IsHashIntervalTick(Props.sparkInterval))
            {
                FleckMaker.ThrowMicroSparks(this.parent.DrawPos, this.parent.Map);
            }
            
            if (this.parent.IsHashIntervalTick(Props.smokeInterval))
            {
                FleckMaker.ThrowSmoke(this.parent.DrawPos, this.parent.Map, 0.5f);
            }
        }
    }
}
