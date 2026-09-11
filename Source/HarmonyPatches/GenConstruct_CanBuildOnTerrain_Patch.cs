using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanBuildOnTerrain))]
    public static class GenConstruct_CanBuildOnTerrain_Patch
    {
        public static bool Prefix(BuildableDef entDef, IntVec3 c, Map map, ref bool __result)
        {
            if (entDef is TerrainDef terrain && terrain.IsSubstructure && map.terrainGrid.FoundationAt(c) == VGEDefOf.VGE_DamagedSubstructure)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
