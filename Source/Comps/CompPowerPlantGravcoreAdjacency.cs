
using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class CompPowerPlantGravcoreAdjacency : CompPowerPlantGravcore
    {
        public float adjacencyBonus = 1;

        public override float DesiredPowerOutput
        {
            get
            {
                if (!onSubstructure)
                {
                    return 0f;
                }
                return base.DesiredPowerOutput * adjacencyBonus;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(2000))
            {
                adjacencyBonus = parent.GetStatValue(VGEDefOf.VGE_GravPowerAdjacency);
            }
        }


    }
}
