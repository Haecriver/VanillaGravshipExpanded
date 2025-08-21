using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_ConnectedToGravship : CompResourceTrader
{
    private CompGravshipFacility facility;

    public override bool CanBeOn() => base.CanBeOn() && (facility == null || (!facility.LinkedBuildings.NullOrEmpty() && facility.CanBeActive));

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    private void InitializeComps()
    {
        facility = parent.GetComp<CompGravshipFacility>();
        ResourceOn = false;

        facility.OnLinkRemoved += Notify_LinkRemoved;
    }

    public void Notify_LinkRemoved(CompFacility facility, Thing thing) => ResourceOn = false;
}