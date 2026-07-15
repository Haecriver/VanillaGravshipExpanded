using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public static class GravshipFuelProviderUtility
{
    private static List<CompGravshipThruster> TmpThrustersList = [];
    private static List<IGravshipFuelProvider> TmpProvidersList = [];

    public static void ConsumeFuelRatioForAllProviders(Building_GravEngine engine, float ratio, FuelSpentData fuelSpentData = null)
    {
        ForEachActiveProvider(engine, ConsumeFuel);

        void ConsumeFuel(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            var consumptionList = provider.ConsumeFuelRatio(gravEngine, ratio, thrusters, otherProviders);
            if (fuelSpentData != null)
            {
                foreach (var (p, amount) in consumptionList)
                {
                    if (p.ParentThing is { } thing)
                        fuelSpentData.fuelData[thing] = amount;
                }
            }
        }
    }

    public static void RefundFuelForAllProviders(Building_GravEngine engine, float refundRatio, FuelSpentData fuelSpentData)
    {
        if (fuelSpentData == null || fuelSpentData.fuelData.NullOrEmpty())
            return;

        ForEachActiveProvider(engine, RefundFuel);

        void RefundFuel(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            if (provider.ParentThing is { } thing && fuelSpentData.fuelData.TryGetValue(thing, out var amount))
                provider.AddFuelAmount(gravEngine, amount * refundRatio);
        }
    }

    public static float CurrentRangeForAllProviders(Building_GravEngine engine)
    {
        var range = 0f;

        ForEachActiveProvider(engine, SumMaxRanges);

        return range;

        void SumMaxRanges(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            range += provider.CurrentRangeProvidedByFuel(gravEngine, thrusters, otherProviders);
        }
    }

    public static float MaxRangeForAllProviders(Building_GravEngine engine)
    {
        var range = 0f;

        ForEachActiveProvider(engine, SumMaxRanges);

        return range;

        void SumMaxRanges(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            range += provider.MaxRangeProvidedByFuel(gravEngine, thrusters, otherProviders);
        }
    }

    public static List<FuelTabEntry> GetFuelTabEntriesForAllProviders(Building_GravEngine engine)
    {
        var list = new List<FuelTabEntry>();
        var genericEntry = new GenericFuelTabEntry(engine)
        {
            title = "VGE_FuelTab_OtherFuelTitle".Translate()
        };

        foreach (var comp in engine.GravshipComponents)
        {
            if (comp.Props.providesFuel && comp.parent.Spawned && comp.CanBeActive)
            {
                var refuelable = comp.parent.GetComp<CompRefuelable>();
                if (refuelable != null)
                {
                    genericEntry.fuelProviders.Add(refuelable.parent);
                    genericEntry.currentFuel += refuelable.fuel;
                    genericEntry.maxFuel += refuelable.Props.fuelCapacity;
                }
            }
        }

        ForEachActiveProvider(engine, GetAllEntries, false, false);

        if (genericEntry.maxFuel > 0f)
            list.Add(genericEntry);

        return list;

        void GetAllEntries(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            var entry = provider.GetFuelTabEntry(gravEngine, thrusters, otherProviders);
            if (entry != null)
            {
                list.Add(entry);
            }
            else
            {
                genericEntry.fuelProviders.Add(provider.ParentThing);
                genericEntry.currentFuel += provider.CurrentFuel(gravEngine);
                genericEntry.maxFuel += provider.MaxFuel(gravEngine);
            }
        }
    }

    public static void ForEachActiveProvider(Building_GravEngine engine, Action<IGravshipFuelProvider, Building_GravEngine, List<CompGravshipThruster>, List<IGravshipFuelProvider>> action, bool includeCompActiveCheck = true, bool includeProviderActiveCheck = true)
    {
        if (engine == null || action == null)
            return;

        TmpThrustersList.Clear();
        TmpProvidersList.Clear();

        // Preparation
        for (var i = 0; i < engine.GravshipComponents.Count; i++)
        {
            var comp = engine.GravshipComponents[i];
            if (comp.parent.Spawned)
            {
                if (comp is CompGravshipThruster thruster && comp.CanBeActive)
                    TmpThrustersList.Add(thruster);
                // We don't change vanilla handling of CompRefuelable
                if (comp.Props.providesFuel && (!includeCompActiveCheck || comp.CanBeActive) && !comp.parent.HasComp<CompRefuelable>())
                {
                    if (comp.parent is IGravshipFuelProvider thingProvider)
                        TmpProvidersList.Add(thingProvider);
                    else if (comp is IGravshipFuelProvider compProvider)
                        TmpProvidersList.Add(compProvider);
                }
            }
        }

        while (TmpProvidersList.Count > 0)
        {
            // Remove the last element
            var current = TmpProvidersList[^1];
            TmpProvidersList.RemoveAt(TmpProvidersList.Count - 1);

            if (!includeProviderActiveCheck || current.IsActive(engine, TmpThrustersList, TmpProvidersList))
                action(current, engine, TmpThrustersList, TmpProvidersList);
        }

        TmpThrustersList.Clear();
        TmpProvidersList.Clear();
    }
}