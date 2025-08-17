using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_OxygenPusher : CompResourceTrader
{
    private CompPowerTrader intPowerTrader;

    protected new CompProperties_ResourceTrader_OxygenPusher Props => (CompProperties_ResourceTrader_OxygenPusher)props;

    public CompPowerTrader PowerTrader => intPowerTrader ??= parent.GetComp<CompPowerTrader>();

    public override void CompTickRare()
    {
        // Never work in a vacuum biome
        if (parent.Map?.Biome.inVacuum != true)
            return;

        // If disabled due to no more vacuum/exposed to vacuum, skip most other checks
        // TODO: Actually do this part

        // Don't do anything if no pipe net or turned off
        if (PipeNet == null || !ResourceOn)
            return;
        // Make sure the power is on
        if (Props.requiresPower && PowerTrader.Off)
            return;
        // Don't do anything if there's not enough oxygen stored
        var available = PipeNet.TotalProductionThisTick + PipeNet.Stored - PipeNet.TotalConsumptionThisTick;
        if (available <= 0f)
            return;

        var room = parent.GetRoom();
        if (room.ExposedToSpace)
            return;

        var vacuum = room.Vacuum;
        if (vacuum <= 0)
            return;

        var maxConsumption = Props.consumptionPerTick / 100f * GenTicks.TickRareInterval;
        var consumption = maxConsumption;
        // Don't draw too much
        if (consumption > available)
            consumption = available;

        var change = 100f / room.CellCount * Props.airPerSecondPerHundredCells * CompOxygenPusher.IntervalToPerSecond * (consumption / maxConsumption);
        var vacuumAfter = room.UnsanitizedVacuum - change;
        // Scale the 
        if (vacuumAfter < 0)
            consumption *= (change + vacuumAfter) / change;

        room.Vacuum = vacuumAfter;

        PipeNet.ExtraConsumptionThisTick += consumption;
    }

    public override string CompInspectStringExtra()
    {
        if (parent.Spawned && parent.Map.Biome.inVacuum)
        {
            var room = parent.GetRoom();
            if (room.ExposedToSpace)
                return "".Translate();

            var vacuumOffset = 100f / room.CellCount * Props.airPerSecondPerHundredCells * CompOxygenPusher.IntervalToPerSecond;
            // TODO: No oxygen in network
        }

        return string.Empty;
    }
}