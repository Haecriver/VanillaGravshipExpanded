using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompConstantGravshipFacilityBonus : ThingComp
{
    // Additional bonuses from gravship facilities that only care if the facility is linked to the gravship, nothing more.
    // This one is added to the facilities itself, and it requires CompGravshipFacility to be present on its parent.

    public CompGravshipFacility facility;

    public CompProperties_ConstantGravshipFacilityBonus Props => (CompProperties_ConstantGravshipFacilityBonus)props;

    public virtual List<StatModifier> StatOffsets => Props.statOffsets;

    public virtual bool ConnectedToGravEngine => facility.LinkedBuildings.Any();

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            InitializeComps();
    }

    private void InitializeComps() => facility = parent.GetComp<CompGravshipFacility>();
}