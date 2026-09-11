using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using RimWorld.Planet;
using VEF.CacheClearing;
using Verse;

namespace VanillaGravshipExpanded;

public static class GravEngineTracker
{
    private const int CooldownTicksAfterFailedCheck = GenTicks.TickRareInterval;

    private static Building_GravEngine Cached = null;
    private static int LastFailedRecacheTick = -1000000;
    private static readonly List<IThingHolder> TmpMapChildHolders = [];
    private static readonly List<Thing> TmpThings = [];

    static GravEngineTracker()
    {
        ClearCaches.OnClearCache += _ =>
        {
            Cached = null;
            ResetLastFailedRecacheTimer();
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetLastFailedRecacheTimer() => LastFailedRecacheTick = -1000000;

    public static Building_GravEngine GetPlayerGravEngine()
    {
        if (Cached != null)
        {
            if (IsGravEngineValid(Cached))
                return Cached;
            Cached = null;
            ResetLastFailedRecacheTimer();
        }

        if (Find.TickManager.TicksGame < LastFailedRecacheTick)
            return null;

        if (Find.CurrentGravship?.engine != null)
            return Cached = Find.CurrentGravship.engine;

        // Fast check using ListerThings
        for (var i = 0; i < Find.Maps.Count; i++)
        {
            Cached = GetGravEngine_ListerThings(Find.Maps[i]);
            if (Cached != null)
                return Cached;
        }

        // Slow recursive check
        for (var i = 0; i < Find.Maps.Count; i++)
        {
            Cached = GetGravEngine_MapRecursive(Find.Maps[i]);
            if (Cached != null)
                return Cached;
        }

        // Recursive check on all player-owned world objects (that don't have maps)
        for (var i = 0; i < Find.WorldObjects.AllWorldObjects.Count; i++)
        {
            Cached = GetGravEngine_WorldObjectRecursive(Find.WorldObjects.AllWorldObjects[i]);
            if (Cached != null)
            {
                TmpThings.Clear();
                return Cached;
            }
        }
        TmpThings.Clear();

        // Failed to find an engine, wait a moment before re-checking for performance reasons
        LastFailedRecacheTick = Find.TickManager.TicksGame + CooldownTicksAfterFailedCheck;
        return null;
    }

    private static Building_GravEngine GetGravEngine_ListerThings(Map map)
    {
        for (var i = 0; i < GravshipHelper.GravEngineDefs.Length; i++)
        {
            var engine = GravshipHelper.GravEngineDefs[i];
            var list = map.listerThings.ThingsOfDef(engine);
            if (list.Count > 0)
                return list[0] as Building_GravEngine;

            if (engine.minifiedDef != ThingDefOf.MinifiedThing)
            {
                list = map.listerThings.ThingsOfDef(engine.minifiedDef);
                if (list.Count > 0)
                    return list[0] as Building_GravEngine;
            }
            else Log.ErrorOnce($"Grav engine with defName {engine.defName} doesn't have a custom minified def set up and is using the generic {nameof(ThingDefOf.MinifiedThing)} def for it.", Gen.HashCombineInt(engine.defNameHash, -1577006653));
        }

        return null;
    }

    private static Building_GravEngine GetGravEngine_MapRecursive(Map map)
    {
        TmpMapChildHolders.Clear();
        map.GetChildHolders(TmpMapChildHolders);
        Building_GravEngine engine = null;

        for (var i = 0; i < TmpMapChildHolders.Count; i++)
        {
            engine = GetGravEngine_IThingHolderRecursive(TmpMapChildHolders[i]);
            if (engine != null)
                break;
        }

        TmpThings.Clear();
        TmpMapChildHolders.Clear();
        return engine;
    }

    private static Building_GravEngine GetGravEngine_WorldObjectRecursive(WorldObject worldObject)
    {
        if (worldObject.Faction is { IsPlayer: true } && worldObject is not MapParent && worldObject is IThingHolder thingHolder)
        {
            var engine = GetGravEngine_IThingHolderRecursive(thingHolder);
            TmpThings.Clear();
            return engine;
        }

        return null;
    }

    private static Building_GravEngine GetGravEngine_IThingHolderRecursive(IThingHolder holder)
    {
        TmpThings.Clear();
        ThingOwnerUtility.GetAllThingsRecursively(holder, TmpThings);
        for (var j = 0; j < TmpThings.Count; j++)
        {
            if (TmpThings[j] is Building_GravEngine engine)
                return engine;
        }

        return null;
    }

    private static bool IsGravEngineValid(Building_GravEngine engine)
    {
        // Destroyed, not valid engine
        if (engine.Destroyed)
            return false;

        // Spawned or on a map, valid engine
        if (engine.SpawnedOrAnyParentSpawned)
            return true;

        // Held by a Gravship world object
        if (Find.CurrentGravship?.engine == engine)
            return true;

        // Not spawned and no parent holder, not valid engine
        if (engine.ParentHolder == null)
            return false;

        // Parent is a WorldObject of player faction that isn't a MapParent is a valid engine
        return engine.ParentHolder is WorldObject { Faction.IsPlayer: true } and not MapParent;
    }

    public static void Notify_GravEngineStateChanged(Building_GravEngine engine)
    {
        if (engine == null)
            return;

        // Cached engine is the same as the one whose state changed - either do nothing, or clear cache.
        if (engine == Cached)
        {
            if (!IsGravEngineValid(engine))
            {
                Cached = null;
                ResetLastFailedRecacheTimer();
            }
            return;
        }

        // We currently have something cache - either return early (if current Cached engine is valid), or reset cached engine
        if (Cached != null)
        {
            if (IsGravEngineValid(Cached))
                return;
            Cached = null;
            ResetLastFailedRecacheTimer();
        }

        // If the new engine is valid - cache it immediately
        if (IsGravEngineValid(engine))
            Cached = engine;
    }
}