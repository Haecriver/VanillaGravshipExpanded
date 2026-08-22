using PipeSystem;
using RimWorld;

namespace VanillaGravshipExpanded;

public class CompProperties_PipeNetFuelProvider : CompProperties_GravshipFacility
{
    public PipeNetDef pipeNet;
    public bool isGenericFuel = false;
    public float genericResourceToRangeRatio = -1f;

    public CompProperties_PipeNetFuelProvider() => compClass = typeof(CompPipeNetGravshipFuelProvider);
}