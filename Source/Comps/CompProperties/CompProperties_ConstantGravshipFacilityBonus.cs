using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ConstantGravshipFacilityBonus : CompProperties
{
    public List<StatModifier> statOffsets;

    public CompProperties_ConstantGravshipFacilityBonus() => compClass = typeof(CompConstantGravshipFacilityBonus);
}