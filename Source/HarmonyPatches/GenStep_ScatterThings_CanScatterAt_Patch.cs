using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GenStep_ScatterThings), "CanScatterAt")]
    public static class GenStep_ScatterThings_CanScatterAt_Patch
    {
        public static bool Prefix(IntVec3 loc, Map map)
        {
            if (map.Tile.LayerDef?.isSpace == true && loc.GetTerrain(map) == TerrainDefOf.Space)
            {
                return false;
            }
            return true;
        }
    }
}
