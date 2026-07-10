using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_PipeNetFuelProvider : CompProperties_GravshipFacility
{
    public PipeNetDef pipeNet;
    public float resourceToRangeRatio = 10f;

    public CompProperties_PipeNetFuelProvider() => compClass = typeof(CompPipeNetGravshipFuelProvider);
}