using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KCSG;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ScenPart_PlayerPawnsArriveMethod), nameof(ScenPart_PlayerPawnsArriveMethod.DoGravship))]
public static class ScenPart_PlayerPawnsArriveMethod_DoGravship_Patch
{
    public static bool Prefix(Map map, List<Thing> startingItems)
    {
        var choosePart = Find.Scenario.AllParts.OfType<ScenPart_ChooseStartingGravship>().FirstOrDefault();
        if (choosePart == null || choosePart.chosenDef == null)
        {
            return true;
        }
        var orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
        map.regionAndRoomUpdater.Enabled = true;
        var playerStartSpot = MapGenerator.PlayerStartSpot;
        var prefab = choosePart.chosenDef.prefab;
        var cellRect = CellRect.CenteredOn(playerStartSpot, prefab.size.x, prefab.size.z);
        var hashSet = cellRect.Cells.ToHashSet();
        if (!MapGenerator.PlayerStartSpotValid)
        {
            GenStep_ReserveGravshipArea.SetStartSpot(map, hashSet, orGenerateVar);
            playerStartSpot = MapGenerator.PlayerStartSpot;
        }
        GravshipPlacementUtility.ClearAreaForGravship(map, playerStartSpot, hashSet);
        var list = new HashSet<Thing>();
        cellRect = CellRect.CenteredOn(playerStartSpot, cellRect.Width, cellRect.Height);
        GenOption.GetAllMineableIn(cellRect, map);
        var spawned = new List<Thing>();
        PrefabUtility.SpawnPrefab(prefab, map, playerStartSpot, Rot4.North, Faction.OfPlayer, spawned);
        list.AddRange(spawned);

        DistributeIntoPipeNet(map, playerStartSpot, spawned, VGEDefOf.VGE_AstrofuelNet, choosePart.startingAstrofuel, VGEDefOf.VGE_Astrofuel);
        DistributeIntoPipeNet(map, playerStartSpot, spawned, VGEDefOf.VGE_OxygenNet, choosePart.startingOxygen, null);

        orGenerateVar.Add(cellRect);
        foreach (var startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
        {
            if (!cellRect.TryRandomElement(c => c.Standable(map) && (c.GetTerrain(map)?.IsSubstructure ?? false), out var result))
            {
                Log.Error("Could not find a valid spawn location for pawn " + startingAndOptionalPawn.Name);
            }
            else
            {
                GenPlace.TryPlaceThing(startingAndOptionalPawn, result, map, ThingPlaceMode.Near);
            }
        }

        var allShelves = list.OfType<Building_Storage>().ToList();
        var emptyShelves = new List<Building_Storage>(allShelves);
        foreach (var startingItem in startingItems)
        {
            if (startingItem.def.CanHaveFaction)
            {
                startingItem.SetFactionDirect(Faction.OfPlayer);
            }
            var countLeft = startingItem.stackCount;
            var attempts = 99;
            while (countLeft > 0 && attempts-- > 0)
            {
                // First try to use empty shelves
                if (!emptyShelves.Where(x => x.GetParentStoreSettings().AllowedToAccept(startingItem)).TryRandomElement(out var shelf))
                {
                    // If there's none, try placing around full shelves
                    allShelves.Where(x => x.GetParentStoreSettings().AllowedToAccept(startingItem)).TryRandomElement(out shelf);
                }

                IntVec3 cell;
                // Pick a shelf cell if possible
                if (shelf != null)
                {
                    cell = shelf.OccupiedRect().RandomCell;
                }
                // Try to pick any substructure tile
                else if (!cellRect.TryFindRandomCell(out cell, x => x.GetTerrain(map) == TerrainDefOf.Substructure && x.GetFirstThing<Building_Door>(map) == null))
                {
                    // Pick any tile in the rect
                    cell = cellRect.RandomCell;
                }

                var thing = startingItem.SplitOff(Math.Min(startingItem.def.stackLimit, countLeft));
                countLeft -= thing.stackCount;
                GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near, extraValidator: x => x.GetFirstThing<Building_Door>(map) == null);

                // If shelf is full after adding to it, remove it from list of empty shelves
                if (shelf != null && shelf.SpaceRemainingFor(startingItem.def) <= 0)
                    emptyShelves.Remove(shelf);
            }
        }
        foreach (var thing in list)
        {
            if (thing.def == ThingDefOf.Door)
            {
                MapGenerator.rootsToUnfog.AddRange(GenAdj.CellsAdjacentCardinal(thing));
            }
            if (thing is Building_GravEngine building_GravEngine)
            {
                building_GravEngine.silentlyActivate = true;
            }
            // Don't refuel stuff, since that will be handled through KCSG import
        }
        foreach (var cell in cellRect)
        {
            if (cell.GetTerrain(map) == TerrainDefOf.Substructure)
            {
                map.areaManager.Home[cell] = true;
            }
        }
        return false;
    }

    private static void DistributeIntoPipeNet(Map map, IntVec3 center, List<Thing> spawned, PipeNetDef pipeNet, float amount, ThingDef overflowItem)
    {
        var storages = spawned
            .OfType<ThingWithComps>()
            .SelectMany(t => t.GetComps<CompResourceStorage>())
            .Where(c => c.Props.pipeNet == pipeNet)
            .ToList();
        var remaining = amount;
        foreach (var storage in storages)
        {
            var free = storage.Props.storageCapacity - storage.AmountStored;
            if (free <= 0f)
            {
                continue;
            }
            var toAdd = Mathf.Min(free, remaining);
            storage.AddResource(toAdd);
            remaining -= toAdd;
            if (remaining <= 0f)
            {
                return;
            }
        }
        if (remaining > 0f && overflowItem != null)
        {
            while (remaining >= 1f)
            {
                var thing = ThingMaker.MakeThing(overflowItem);
                thing.stackCount = Mathf.Min(Mathf.FloorToInt(remaining), overflowItem.stackLimit);
                remaining -= thing.stackCount;
                GenPlace.TryPlaceThing(thing, center, map, ThingPlaceMode.Near);
            }
        }
    }
}
