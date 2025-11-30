using HarmonyLib;
using Verse;
using RimWorld.Planet;
using System.Collections.Generic;
using System;
using RimWorld;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    public static class MapGenerator_GenerateMap_Patch
    {
        public static void Prefix(ref IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator)
        {
            if (mapGenerator == MapGeneratorDefOf.Space && mapSize.z < Find.World.info.initialMapSize.z && mapSize.x < Find.World.info.initialMapSize.x)
            {
                mapSize = Find.World.info.initialMapSize;
            }
        }
    }
}
