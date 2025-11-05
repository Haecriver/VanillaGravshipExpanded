using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KCSG;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ScenPart_PlayerPawnsArriveMethod), nameof(ScenPart_PlayerPawnsArriveMethod.DoGravship))]
public static class ScenPart_PlayerPawnsArriveMethod_DoGravship_Patch
{
    public static bool Prefix(Map map, List<Thing> startingItems)
    {
        var orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
        map.regionAndRoomUpdater.Enabled = true;
        var playerStartSpot = MapGenerator.PlayerStartSpot;
        var cellRect = CellRect.CenteredOn(playerStartSpot, VGEDefOf.VGE_StartingGravjumper.Sizes.x, VGEDefOf.VGE_StartingGravjumper.Sizes.z);
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
        LayoutUtils.CleanRect(VGEDefOf.VGE_StartingGravjumper, map, cellRect, true);
        Log.Message("Faction.OfPlayer: " + Faction.OfPlayer);
        VGEDefOf.VGE_StartingGravjumper.Generate(cellRect, map, list, Faction.OfPlayer);

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
                if (list.Where(t => t.def == ThingDefOf.Shelf || t.def == ThingDefOf.ShelfSmall || t.def == VGEDefOf.VGE_GravshipShelf).TryRandomElement(out var result2))
                {
                    var randomCell = result2.OccupiedRect().RandomCell;
                    var thing = startingItem.SplitOff(Math.Min(startingItem.def.stackLimit, countLeft));
                    countLeft -= thing.stackCount;
                    GenPlace.TryPlaceThing(thing, randomCell, map, ThingPlaceMode.Near);
                }
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
}
