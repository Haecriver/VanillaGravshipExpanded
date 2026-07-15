using System;
using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompPipeNetGravshipFuelProvider : CompGravshipFacility, IGravshipFuelProvider
{
    protected CompResourceStorage storage;

    public Thing ParentThing => parent;

    public new CompProperties_PipeNetFuelProvider Props => (CompProperties_PipeNetFuelProvider)props;

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
        storage = parent.GetComps<CompResourceStorage>().FirstOrDefault(c => c.Props.pipeNet == Props.pipeNet);
    }

    public bool IsActive(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        if (Props.pipeNet == null)
            return true;

        if (activeThrusters.Any(t => t.parent.GetComps<CompResource>().Any(r => r.Props.pipeNet == Props.pipeNet)))
            return true;

        // Remove other providers using the same pipe net
        otherProviders?.RemoveAll(x => x is CompPipeNetGravshipFuelProvider other && Props.pipeNet == other.Props.pipeNet);
        return false;
    }

    public float CurrentFuel(Building_GravEngine engine) => storage.AmountStored;

    public float MaxFuel(Building_GravEngine engine) => storage.Props.storageCapacity;

    public float CurrentRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        var range = storage.AmountStored / Props.resourceToRangeRatio;
        if (Props.pipeNet == null)
            return range;

        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            range += other.storage.AmountStored / other.Props.resourceToRangeRatio;
            return true;
        });

        // Grab either the range provided by thrusters or max range of all thrusters
        return Mathf.Min(range, GetMaxRangeForThrusters(activeThrusters));
    }

    public float MaxRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        var maxRange = storage.Props.storageCapacity / Props.resourceToRangeRatio;
        if (Props.pipeNet == null)
            return maxRange;

        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            maxRange += other.storage.Props.storageCapacity / other.Props.resourceToRangeRatio;
            return true;
        });

        // return maxRange;
        // Grab either the range provided by thrusters or max range of all thrusters
        return Mathf.Min(maxRange, GetMaxRangeForThrusters(activeThrusters));
    }

    private float GetMaxRangeForThrusters(List<CompGravshipThruster> activeThrusters)
    {
        var maxRange = 0f;

        for (var i = 0; i < activeThrusters.Count; i++)
        {
            var thruster = activeThrusters[i];
            for (var j = 0; j < thruster.parent.AllComps.Count; j++)
            {
                if (thruster.parent.AllComps[j] is CompResource resource && Props.pipeNet == resource.Props.pipeNet)
                    maxRange += thruster.Props.statOffsets.GetStatOffsetFromList(StatDefOf.GravshipRange);
            }
        }

        return maxRange;
    }

    public float ConsumeFuelAmount(Building_GravEngine engine, float fuelAmount)
    {
        var amountStored = storage.AmountStored;
        if (amountStored >= fuelAmount)
        {
            storage.DrawResource(fuelAmount);
            return fuelAmount;
        }

        storage.Empty();
        return amountStored;
    }

    public List<(IGravshipFuelProvider, float)> ConsumeFuelRatio(Building_GravEngine engine, float fuelConsumedRatio, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        var list = new List<(IGravshipFuelProvider, float)>();

        if (Props.pipeNet == null)
        {
            var amountToConsume = storage.AmountStored * fuelConsumedRatio;
            storage.DrawResource(amountToConsume);
            list.Add((this, amountToConsume));
            return list;
        }

        var totalFuel = CurrentFuel(engine);
        // CurrentRangeProvidedByFuel would remove all from the list if we passed the list, so we'll need to manually iterate over all thrusters
        var range = CurrentRangeProvidedByFuel(engine, activeThrusters, null);

        if (otherProviders != null)
        {
            for (var i = 0; i < otherProviders.Count; i++)
            {
                if (otherProviders[i] is CompPipeNetGravshipFuelProvider other && Props.pipeNet == other.Props.pipeNet)
                {
                    totalFuel += other.CurrentFuel(engine);
                    range += other.CurrentRangeProvidedByFuel(engine, activeThrusters, null);
                }
            }
        }

        range = Mathf.Min(range, GetMaxRangeForThrusters(activeThrusters));
        var toConsume = range * fuelConsumedRatio * Props.resourceToRangeRatio;
        var toConsumeRatio = toConsume / totalFuel;

        var amount = storage.AmountStored * toConsumeRatio;
        storage.DrawResource(storage.AmountStored * toConsumeRatio);
        list.Add((this, amount));
        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            var amount = other.storage.AmountStored * toConsumeRatio;
            other.storage.DrawResource(amount);
            list.Add((x, amount));
            return true;
        });

        return list;
    }

    public float AddFuelAmount(Building_GravEngine engine, float amount)
    {
        var canAccept = storage.AmountCanAccept;
        if (canAccept >= amount)
        {
            storage.AddResource(amount);
            return amount;
        }

        storage.AddResource(canAccept);
        return canAccept;
    }

    public FuelTabEntry GetFuelTabEntry(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        if (Props.pipeNet == null)
            return null;

        var entry = new SimpleMultiLineTextEntry(engine)
        {
            title = "VGE_FuelTab_ThrustersTitle".Translate(Props.pipeNet.resource.name.UncapitalizeFirst().Named("RESOURCE")).CapitalizeFirst(),
        };
        var currentFuel = CurrentFuel(engine);
        var maxFuel = MaxFuel(engine);
        var range = storage.AmountStored / Props.resourceToRangeRatio;
        var maxRange = storage.Props.storageCapacity / Props.resourceToRangeRatio;
        entry.fuelProviders.Add(ParentThing);

        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            entry.fuelProviders.Add(other.ParentThing);
            currentFuel += other.CurrentFuel(engine);
            maxFuel += other.MaxFuel(engine);
            range += other.storage.AmountStored / other.Props.resourceToRangeRatio;
            maxRange += other.storage.Props.storageCapacity / other.Props.resourceToRangeRatio;

            return true;
        });

        for (var i = 0; i < activeThrusters.Count; i++)
        {
            var thruster = activeThrusters[i];
            if (thruster.parent.GetComp<CompResource>() is { } comp && comp.Props.pipeNet == Props.pipeNet)
                entry.thrusters.Add(thruster.parent);
        }

        var maxRangeForThrusters = GetMaxRangeForThrusters(activeThrusters);
        if (range > maxRangeForThrusters)
            range = maxRangeForThrusters;
        if (maxRange > maxRangeForThrusters)
            maxRange = maxRangeForThrusters;

        entry.text.Add($"{"VGE_FuelTab_Thrusters".Translate().CapitalizeFirst()}: {entry.thrusters.Count}");
        entry.text.Add($"{Props.pipeNet.resource.name.CapitalizeFirst()}: {currentFuel} / {maxFuel}");
        entry.text.Add($"{"VGE_FuelTab_Range".Translate().CapitalizeFirst()}: {range} / {maxRange}");
        entry.text.Add("VGE_FuelTab_UsagePerTile".Translate((Props.resourceToRangeRatio * engine.FuelUseageFactor).Named("COST"), Props.pipeNet.resource.name.UncapitalizeFirst().Named("RESOURCE")).CapitalizeFirst());

        return entry;
    }
}