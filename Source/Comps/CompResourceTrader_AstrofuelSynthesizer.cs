using PipeSystem;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_AstrofuelSynthesizer : CompResourceTrader
{
    public CompFacility_Astropurifier astropurifier;

    public void Notify_LinkAdded(CompFacility_Astropurifier facility)
    {
        if (astropurifier == facility)
            return;
        // Update display stats
        if (astropurifier == null)
            PipeNet.producersDirty = true;

        astropurifier = facility;
        BaseConsumption = Props.consumptionPerTick / 2f;
    }

    public void Notify_LinkRemoved(CompFacility_Astropurifier facility)
    {
        if (astropurifier != facility)
            return;

        PipeNet.producersDirty = true;
        astropurifier = null;
        BaseConsumption = Props.consumptionPerTick;
    }

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
        ((CompRefuelable_AstrofuelSynthesizer)compRefuelable).synthesizer = this;
    }
}