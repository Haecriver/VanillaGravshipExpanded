using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KCSG;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;
using RimWorld.SketchGen;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ScenPart_PlayerPawnsArriveMethod), nameof(ScenPart_PlayerPawnsArriveMethod.DoGravship))]
public static class ScenPart_PlayerPawnsArriveMethod_DoGravship_Patch
{
    public static bool Prefix(Map map, List<Thing> startingItems)
    {
        List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
        map.regionAndRoomUpdater.Enabled = true;
        IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
        var cellRect = CellRect.CenteredOn(playerStartSpot, VGEDefOf.VGE_StartingGravjumper.Sizes.x, VGEDefOf.VGE_StartingGravjumper.Sizes.z);
        var hashSet = cellRect.Cells.ToHashSet();
        if (!MapGenerator.PlayerStartSpotValid)
        {
            GenStep_ReserveGravshipArea.SetStartSpot(map, hashSet, orGenerateVar);
            playerStartSpot = MapGenerator.PlayerStartSpot;
        }
        GravshipPlacementUtility.ClearAreaForGravship(map, playerStartSpot, hashSet);
        HashSet<Thing> list = new HashSet<Thing>();
        cellRect = CellRect.CenteredOn(playerStartSpot, cellRect.Width, cellRect.Height);
        GenOption.GetAllMineableIn(cellRect, map);
        LayoutUtils.CleanRect(VGEDefOf.VGE_StartingGravjumper, map, cellRect, true);
        Log.Message("Faction.OfPlayer: " + Faction.OfPlayer);
        VGEDefOf.VGE_StartingGravjumper.Generate(cellRect, map, list, Faction.OfPlayer);

        orGenerateVar.Add(cellRect);
        foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
        {
            if (!cellRect.TryRandomElement((IntVec3 c) => c.Standable(map) && (c.GetTerrain(map)?.IsSubstructure ?? false), out var result))
            {
                Log.Error("Could not find a valid spawn location for pawn " + startingAndOptionalPawn.Name);
            }
            else
            {
                GenPlace.TryPlaceThing(startingAndOptionalPawn, result, map, ThingPlaceMode.Near);
            }
        }
        foreach (Thing startingItem in startingItems)
        {
            if (startingItem.def.CanHaveFaction)
            {
                startingItem.SetFactionDirect(Faction.OfPlayer);
            }
            int num = startingItem.stackCount;
            int num2 = 99;
            while (num > 0 && num2-- > 0)
            {
                if (list.Where((Thing t) => t.def == ThingDefOf.Shelf || t.def == ThingDefOf.ShelfSmall || t.def == VGEDefOf.VGE_GravshipShelf).TryRandomElement(out var result2))
                {
                    IntVec3 randomCell = result2.OccupiedRect().RandomCell;
                    Thing thing = startingItem.SplitOff(Math.Min(startingItem.def.stackLimit, num));
                    num -= thing.stackCount;
                    GenPlace.TryPlaceThing(thing, randomCell, map, ThingPlaceMode.Near);
                }
            }
        }
        foreach (Thing item in list)
        {
            if (item.def == ThingDefOf.Door)
            {
                MapGenerator.rootsToUnfog.AddRange(GenAdj.CellsAdjacentCardinal(item));
            }
            if (item.TryGetComp(out CompRefuelable comp))
            {
                comp.Refuel(comp.Props.fuelCapacity);
            }
            if (item is Building_GravEngine building_GravEngine)
            {
                building_GravEngine.silentlyActivate = true;
            }
            if (item.TryGetComp<CompResourceStorage>() is CompResourceStorage compResourceStorage)
            {
                compResourceStorage.AddResource(compResourceStorage.Props.storageCapacity);
            }
        }
        foreach (IntVec3 item2 in cellRect)
        {
            if (item2.GetTerrain(map) == TerrainDefOf.Substructure)
            {
                map.areaManager.Home[item2] = true;
            }
        }
        return false;
    }
}
