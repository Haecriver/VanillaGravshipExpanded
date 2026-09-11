using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

public class ExtendedLaunchInfo : IExposable
{
    public static Action<LaunchInfo, ExtendedLaunchInfo> onInit;

    public Pawn gravtechResearcherPawns = null;
    public PlanetTile launchSourceTile = PlanetTile.Invalid;
    public bool isGravliftLaunch = false;
    public float lastCost;
    public FuelSpentData fuelSpentPerTank = new();
    public LandingOutcomeDef forcedMishap = null;
    public LaunchBoonDef forcedBoon = null;
    public ExtendedLaunchInfoComp vge2Data = null;
    public List<ExtendedLaunchInfoComp> extraData = [];

    internal void Init(LaunchInfo info) => onInit?.Invoke(info, this);

    public void ExposeData()
    {
        Scribe_References.Look(ref gravtechResearcherPawns, "gravtechResearcherPawn");
        Scribe_Values.Look(ref launchSourceTile, "launchSourceTile", -1);
        Scribe_Values.Look(ref isGravliftLaunch, "isGravliftLaunch", false);
        Scribe_Values.Look(ref lastCost, "lastCost", 0f);
        Scribe_Deep.Look(ref fuelSpentPerTank, "fuelSpentPerTank");
        Scribe_Defs.Look(ref forcedMishap, "forcedMishap");
        Scribe_Defs.Look(ref forcedBoon, "forcedBoon");
        Scribe_Deep.Look(ref vge2Data, "vge2Data");
        Scribe_Collections.Look(ref extraData, "extraData", LookMode.Deep);

        if (Scribe.mode is LoadSaveMode.Saving or LoadSaveMode.PostLoadInit)
            fuelSpentPerTank ??= new FuelSpentData();
        if (Scribe.mode is LoadSaveMode.PostLoadInit)
            extraData ??= [];
    }

    public void PreLandingEnded(WorldComponent_GravshipController controller)
    {
        vge2Data?.PreLandingEnded(controller);
        for (var i = 0; i < extraData.Count; i++)
            extraData[i].PreLandingEnded(controller);
    }

    public void PostLandingEnded(Gravship gravship)
    {
        vge2Data?.PostLandingEnded(gravship);
        for (var i = 0; i < extraData.Count; i++)
            extraData[i].PostLandingEnded(gravship);
    }
}

public abstract class ExtendedLaunchInfoComp : IExposable
{
    public virtual void ExposeData()
    {
    }

    public virtual void PreLandingEnded(WorldComponent_GravshipController controller)
    {
    }

    public virtual void PostLandingEnded(Gravship gravship)
    {
    }
}