namespace VanillaGravshipExpanded
{
    using RimWorld;
    using Verse;

    public class CompProperties_GravEngineHaze : CompProperties
    {
        public HazeSettings hazeSettings = new();

        public CompProperties_GravEngineHaze()
        {
            compClass = typeof(CompGravEngineHaze);
        }
    }

    public class CompGravEngineHaze : ThingComp
    {
        public Effecter haze;

        public CompProperties_GravEngineHaze Props => props as CompProperties_GravEngineHaze;

        public Building_GravEngine Engine => parent as Building_GravEngine;

        public override void CompTick()
        {
            base.CompTick();

            if(this.parent.IsHashIntervalTick(GenTicks.TicksPerRealSecond / 2) && GenTicks.TicksGame < Engine.cooldownCompleteTick)
            {
                if (this.haze == null)
                {
                    this.haze        = VGEDefOf.VGE_HazeEffecter.Spawn(this.parent, this.parent.Map, Props.hazeSettings.scale);
                    this.haze.offset = this.parent.TrueCenter() - this.parent.DrawPos + Props.hazeSettings.offset.ToVector3();
                }

                this.haze.Trigger(this.parent, this.parent);
            }
        }
    }
}
