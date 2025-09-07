using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_GravdataYield : CompProperties
    {
        public float multiplier = 1f;

        public CompProperties_GravdataYield()
        {
            compClass = typeof(CompGravdataYield);
        }
    }

    public class CompGravdataYield : ThingComp
    {
        public CompProperties_GravdataYield Props => (CompProperties_GravdataYield)props;

        public float Multiplier => Props.multiplier;
    }
}
