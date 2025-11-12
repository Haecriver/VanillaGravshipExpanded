using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_OxygenPusher : CompResourceTrader
{
    private bool lowPowerMode = false;
    private CompPowerTrader intPowerTrader;

    protected new CompProperties_ResourceTrader_OxygenPusher Props => (CompProperties_ResourceTrader_OxygenPusher)props;

    public CompPowerTrader PowerTrader => intPowerTrader ??= parent.GetComp<CompPowerTrader>();

    public bool LowPowerMode
    {
        get => lowPowerMode;
        set
        {
            lowPowerMode = value;
            CheckUpdatePowerMode();
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        // If we don't use ExecuteWhenFinished here, we'll be in high power mode for the first rare tick after loading
        if (parent.Map?.Biome.inVacuum != true)
            LongEventHandler.ExecuteWhenFinished(() => LowPowerMode = true);
    }

    public override void CompTickRare()
    {
        base.CompTickRare();

        // Never work in a non-vacuum biome
        if (parent.Map?.Biome.inVacuum != true)
        {
            LowPowerMode = true;
            return;
        }

        // Make sure the power is on
        if (Props.requiresPower && PowerTrader.Off)
            return;

        // Don't do anything if no pipe net or turned off
        if (PipeNet == null || !ResourceOn)
        {
            LowPowerMode = true;
            return;
        }
        // Don't do anything if there's not enough oxygen stored
        var available = PipeNet.TotalProductionThisTick + PipeNet.Stored - PipeNet.TotalConsumptionThisTick;
        if (available <= 0f)
        {
            LowPowerMode = true;
            return;
        }

        var roomCell = parent.Position + Props.oxygenCellOffset.RotatedBy(parent.Rotation);
        // If disabled due to no more vacuum/exposed to vacuum, skip most other checks
        if (LowPowerMode)
        {
            var r = roomCell.GetRoom(parent.Map);
            if (r == null || r.ExposedToSpace || r.Vacuum <= 0)
                return;

            LowPowerMode = false;
        }

        var room = roomCell.GetRoom(parent.Map);
        if (room == null || room.ExposedToSpace)
        {
            LowPowerMode = true;
            return;
        }

        var vacuum = room.Vacuum;
        if (vacuum <= 0)
        {
            LowPowerMode = true;
            return;
        }

        var maxConsumption = Props.consumptionPerTick * (60f * CompOxygenPusher.IntervalToPerSecond);
        var consumption = maxConsumption;
        // Don't draw too much
        if (consumption > available)
            consumption = available;

        var change = 100f / room.CellCount * Props.airPerSecondPerHundredCells * CompOxygenPusher.IntervalToPerSecond * (consumption / maxConsumption);
        var vacuumAfter = room.UnsanitizedVacuum - change;
        // Scale the consumption with the actual amount of oxygen pushed
        if (vacuumAfter < 0)
            consumption *= (change + vacuumAfter) / change;

        room.Vacuum = vacuumAfter;

        PipeNet.ExtraConsumptionThisTick += consumption;
    }

    public override string CompInspectStringExtra()
    {
        if (!Props.requiresPower)
            return string.Empty;

        string text;
        if (PowerTrader.Off)
            text = "PowerConsumptionOff".Translate();
        else if (LowPowerMode)
            text = "PowerConsumptionLow".Translate();
        else
            text = "PowerConsumptionHigh".Translate();

        return $"{"PowerConsumptionMode".Translate()}: {text.CapitalizeFirst()}";
    }

    public override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);

        if (signal == CompPowerTrader.PowerTurnedOnSignal)
            CheckUpdatePowerMode();
    }

    private void CheckUpdatePowerMode()
    {
        if (!Props.requiresPower)
            return;
        if (PowerTrader.Off)
            return;

        if (LowPowerMode)
            PowerTrader.PowerOutput = PowerTrader.Props.PowerConsumption * Props.lowPowerConsumptionFactor;
        else
            PowerTrader.PowerOutput = PowerTrader.Props.PowerConsumption;
    }
}