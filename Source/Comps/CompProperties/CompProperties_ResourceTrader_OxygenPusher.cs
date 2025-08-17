using PipeSystem;
using RimWorld;

namespace VanillaGravshipExpanded;

public class CompProperties_ResourceTrader_OxygenPusher : CompProperties_ResourceTrader
{
    // Vanilla CompProperties_OxygenPusher properties
    public bool requiresPower = true;
    public float airPerSecondPerHundredCells = 0.1f;

    public CompProperties_ResourceTrader_OxygenPusher() => compClass = typeof(CompResourceTrader_OxygenPusher);
}