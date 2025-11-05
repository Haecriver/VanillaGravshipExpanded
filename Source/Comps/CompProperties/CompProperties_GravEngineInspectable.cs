using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_GravEngineInspectable : CompProperties
{
    public bool setToPlayerFaction = true;
    public int inspectDuration = 240;

    public CompProperties_GravEngineInspectable() => compClass = typeof(CompGravEngineInspectable);
}