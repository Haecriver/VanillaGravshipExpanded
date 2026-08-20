using System;
using System.Collections.Generic;
using System.Globalization;

namespace VanillaGravshipExpanded;

public class FuelUsageData : IComparable<FuelUsageData>, IComparable
{
    /// <summary>
    /// Fuel usage amount for each provider (as actual, unscaled amount).
    /// </summary>
    public Dictionary<IGravshipFuelProvider, float> fuelData = [];
    /// <summary>
    /// Total amount of fuel used (as actual, unscaled amount).
    /// </summary>
    public float totalAmount = 0;
    /// <summary>
    /// The sorting order of the fuels. Used on the world map when selecting a destination tile, with entries with higher values having a higher priority.
    /// </summary>
    public float sortingOrder = 0;
    /// <summary>
    /// The report string used, used on the world map when selecting a destination tile. Won't be displayed if null. Unused if generic fuel, as those have their own special display.
    /// </summary>
    public string reportString = null;
    /// <summary>
    /// Determines if it's a generic fuel source. Rather than having separate entries, this is basically handled the same as vanilla refuelable fuel sources (which we haven't changed at all).
    /// </summary>
    public bool isGenericFuel = false;

    public int CompareTo(object value)
    {
        if (value == null)
            return 1;
        if (value is not FuelUsageData other)
            throw new ArgumentException($"Object must be of type {nameof(FuelUsageData)}.");
        return CompareTo(other);
    }

    public int CompareTo(FuelUsageData other)
    {
        var order = sortingOrder.CompareTo(other.sortingOrder);
        if (order != 0)
            return order;
        order = totalAmount.CompareTo(other.totalAmount);
        if (order != 0)
            return order;
        return CultureInfo.InvariantCulture.CompareInfo.Compare(reportString, other.reportString);
    }
}