using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public interface IGravshipFuelProvider
{
    /// <summary>
    /// The Thing that is the parent of this provider, be it the Building or the parent of the comp.
    /// </summary>
    public Thing ParentThing { get; }

    /// <summary>
    /// Determines if the fuel provider is currently active and can be used for fuel. This is done on top of CompGravshipFacility:CanBeActive check.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="activeThrusters">List of thrusters linked to this gravship.</param>
    /// <param name="otherProviders">All the other providers that are currently active (excluding this one), or null. A single provider is allowed to handle other, related providers, to avoid recalculating the same shared data for every single provider. Removing other providers from the list makes them inactive.</param>
    /// <returns>True if active and can be interacted with, false otherwise.</returns>
    public bool IsActive(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders);

    /// <summary>
    /// Current amount of fuel provided by this building.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <returns>Amount of fuel provided by this building.</returns>
    public float CurrentFuel(Building_GravEngine engine);

    /// <summary>
    /// Max amount of fuel provided by this building.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <returns>Max fuel provided by this building.</returns>
    public float MaxFuel(Building_GravEngine engine);

    /// <summary>
    /// Current range provided by the fuel in the tank, before applying FuelUseageFactor.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="activeThrusters">List of thrusters linked to this gravship.</param>
    /// <param name="otherProviders">All the other providers that are currently active (excluding this one), or null. A single provider is allowed to handle other, related providers, to avoid recalculating the same shared data for every single provider. Removing other providers from the list prevents them from calling this method themselves.</param>
    /// <returns>The travel range provided by the current amount of fuel.</returns>
    public float CurrentRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders);

    /// <summary>
    /// Max amount of range this building can provide, before applying FuelUseageFactor.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="activeThrusters">List of thrusters linked to this gravship.</param>
    /// <param name="otherProviders">All the other providers that are currently active (excluding this one), or null. A single provider is allowed to handle other, related providers, to avoid recalculating the same shared data for every single provider. Removing other providers from the list prevents them from calling this method themselves.</param>
    /// <returns>The travel range provided if the fuel is full.</returns>
    public float MaxRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders);

    /// <summary>
    /// Consume fuel from this container.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="fuelAmount">The amount of fuel that should be consumed for travel.</param>
    /// <returns>Actual amount of fuel consumed.</returns>
    public float ConsumeFuelAmount(Building_GravEngine engine, float fuelAmount);

    /// <summary>
    /// Consume fuel from this container. This is treated as a ratio of the total fuel considered for CurrentRangeProvidedByFuel
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="fuelConsumedRatio">The ratio of fuel that should be consumed for travel. For example, 0.1 means that 10% of current fuel amount should be consumed.</param>
    /// <param name="activeThrusters">List of thrusters linked to this gravship.</param>
    /// <param name="otherProviders">All the other providers that are currently active (excluding this one), or null. A single provider is allowed to handle other, related providers, to avoid recalculating the same shared data for every single provider. Removing other providers from the list prevents them from calling this method themselves.</param>
    /// <returns>(Tuple will be changed into its own class before release.) Actual amount of fuel consumed (not ratio) per provider.</returns>
    public List<(IGravshipFuelProvider provider, float fuelUsed)> ConsumeFuelRatio(Building_GravEngine engine, float fuelConsumedRatio, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders);

    /// <summary>
    /// Add fuel to this container.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="amount">Amount of fuel to be added.</param>
    /// <returns>Actual amount of fuel added.</returns>
    public float AddFuelAmount(Building_GravEngine engine, float amount);

    /// <summary>
    /// An entry to the gravship fuel ITab to display fuel info, like current/max fuel or any other relevant information.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="activeThrusters">List of thrusters linked to this gravship.</param>
    /// <param name="otherProviders">All the other providers that are currently active (excluding this one), or null. A single provider is allowed to handle other, related providers, to avoid recalculating the same shared data for every single provider. Removing other providers from the list prevents them from calling this method themselves.</param>
    /// <returns>A fuel tab entry for the fuel ITab, or null if it's a generic fuel.</returns>
    public FuelTabEntry GetFuelTabEntry(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders);

    /// <summary>
    /// A report (and sorting order) for the fuel usage by the providers.
    /// </summary>
    /// <param name="engine">Grav engine that this is linked to.</param>
    /// <param name="fuelConsumedRatio">The ratio of fuel that would be consumed for travel. For example, 0.1 means that 10% of current fuel amount would be consumed.</param>
    /// <param name="activeThrusters">List of thrusters linked to this gravship.</param>
    /// <param name="otherProviders">All the other providers that are currently active (excluding this one), or null. A single provider is allowed to handle other, related providers, to avoid recalculating the same shared data for every single provider. Removing other providers from the list prevents them from calling this method themselves.</param>
    /// <returns>(Tuple will be changed into its own class before release.) Currently, it's a string report (or null if generic fuel), and sorting order when displaying the report (higher value = higher position). If generic, the sorting order is amount of fuel consumed (if positive) for the generic report.</returns>
    public (string report, float sortingOrder) GetFuelUsageReport(Building_GravEngine engine, float fuelConsumedRatio, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders);
}