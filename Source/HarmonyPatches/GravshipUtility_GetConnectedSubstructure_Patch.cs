using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.GetConnectedSubstructure))]
    public static class GravshipUtility_GetConnectedSubstructure_Patch
    {
        public static void Prefix(Building_GravEngine engine, HashSet<IntVec3> cells, ref int maxCells, bool requireInsideFootprint = true)
        {
            if (maxCells != int.MaxValue && engine.validSubstructure == cells)
            {
                GetConnectedSubstructure(engine, ref maxCells, requireInsideFootprint);
            }
        }

        public static void GetConnectedSubstructure(Building_GravEngine engine, ref int maxCells, bool requireInsideFootprint = true)
        {
            if (!ModsConfig.OdysseyActive)
            {
                return;
            }
            if (!engine.Spawned)
            {
                Log.Error("Tried to get connected substructure for an unspawned engine.");
                return;
            }
            var oldMaxCells = maxCells;
            List<Thing> footprintMakers = engine.Map.listerThings.ThingsInGroup(ThingRequestGroup.SubstructureFootprint);
            engine.Map.floodFiller.FloodFill(engine.Position, delegate (IntVec3 x)
            {
                if (x.InBounds(engine.Map))
                {
                    TerrainDef terrainDef = engine.Map.terrainGrid.FoundationAt(x);
                    if (terrainDef != null && terrainDef.IsSubstructure)
                    {
                        if (requireInsideFootprint)
                        {
                            return GravshipUtility.InsideFootprint(x, engine.Map, footprintMakers);
                        }
                        return true;
                    }
                }
                return false;
            }, delegate (IntVec3 x)
            {
                TerrainDef terrainDef = engine.Map.terrainGrid.FoundationAt(x);
                if (terrainDef == VGEDefOf.VGE_GravshipSubscaffold)
                {
                    oldMaxCells++;
                }
            }, int.MaxValue);
            maxCells = oldMaxCells;
        }

        public static void Postfix(Building_GravEngine engine, HashSet<IntVec3> cells)
        {
            if (engine.allConnectedSubstructure == cells)
            {
                cells.RemoveWhere(x => engine.Map.terrainGrid.FoundationAt(x) == VGEDefOf.VGE_GravshipSubscaffold);
            }
        }
    }
}
