using RimWorld;

namespace VanillaGravshipExpanded;

public class CompRefuelable_AstrofuelSynthesizer : CompRefuelable
{
    public CompResourceTrader_AstrofuelSynthesizer synthesizer;

    public override string CompInspectStringExtra()
    {
        var prevRate = Props.fuelConsumptionRate;
        var prevConsumeWhenUsed = Props.consumeFuelOnlyWhenUsed;

        try
        {
            // Update consumption like for ticking.
            if (synthesizer.astropurifier != null)
                Props.fuelConsumptionRate /= 4f;
            // If resource is off, disable refuelable drain display
            if (!synthesizer.ResourceOn || synthesizer.LowPowerModeOn || synthesizer.PipeNet.AvailableCapacityLastTick <= 0f)
                Props.consumeFuelOnlyWhenUsed = true;

            return base.CompInspectStringExtra();
        }
        finally
        {
            // Cleanup once done
            Props.fuelConsumptionRate = prevRate;
            Props.consumeFuelOnlyWhenUsed = prevConsumeWhenUsed;
        }
    }
}