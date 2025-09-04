using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ApparelOxygenProvider : CompProperties_ApparelReloadable
{
    public float minResistanceToActivate = 0.12f;
    public float consumptionPerTick = 1f / GenTicks.TickLongInterval;

    public float percentageToAutoRefuel;

    public CompProperties_ApparelOxygenProvider() => compClass = typeof(CompApparelOxygenProvider);
}