using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ResourceTrader_OxygenPusher : CompProperties_ResourceTrader
{
    // Our custom properties
    public IntVec3 oxygenCellOffset = IntVec3.Zero;
    public float lowPowerConsumptionFactor = 0.1f;
    // Vanilla CompProperties_OxygenPusher properties
    public bool requiresPower = true;
    public float airPerSecondPerHundredCells = 0.1f;

    public CompProperties_ResourceTrader_OxygenPusher() => compClass = typeof(CompResourceTrader_OxygenPusher);
}