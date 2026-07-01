using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(DamageWorker), nameof(DamageWorker.ExplosionDamageTerrain))]
    public static class DamageWorker_ExplosionDamageTerrain_Patch
    {
        public static void Postfix(DamageWorker __instance, Explosion explosion, IntVec3 c)
        {
            var modExtension = explosion.weapon?.GetModExtension<SubstructureDamageExtension>();
            if (modExtension == null)
            {
                return;
            }

            var cell = c;
            var map = explosion.Map;
            var explosionCenter = explosion.Position;

            if (cell.DistanceTo(explosionCenter) > modExtension.substructureDamageRadius)
            {
                return;
            }
            DamageTerrain(cell, map);
        }

        public static void DamageTerrain(IntVec3 cell, Map map)
        {
            var terrain = cell.GetTerrain(map);
            var ext = terrain.GetModExtension<DamagedTerrainReplacementExtension>();
            if (ext != null && ext.damagedTerrain != null)
            {
                map.terrainGrid.SetTerrain(cell, ext.damagedTerrain);
                SpawnDebrisFilth(cell, map);
                ThingUtility.CheckAutoRebuildTerrainOnDestroyed(terrain, cell, map);
            }
            else if (terrain == TerrainDefOf.Substructure)
            {
                map.terrainGrid.SetTerrain(cell, VGEDefOf.VGE_DamagedSubstructure);
                SpawnDebrisFilth(cell, map);
                ThingUtility.CheckAutoRebuildTerrainOnDestroyed(TerrainDefOf.Substructure, cell, map);
            }
            else if (terrain.HasTag("DestroyableByArtillery"))
            {
                if (terrain.isFoundation)
                {
                    map.terrainGrid.RemoveFoundation(cell, false);
                }
                else
                {
                    var top = map.terrainGrid.TopTerrainAt(cell);
                    if (top == terrain)
                    {
                        map.terrainGrid.RemoveTopLayer(cell, false);
                    }
                }
            }
        }

        public static void SpawnDebrisFilth(IntVec3 cell, Map map)
        {
            CellRect area = CellRect.FromCell(cell).ExpandedBy(1);
            int count = 0;
            foreach (IntVec3 filthCell in area.Cells.InRandomOrder())
            {
                if (filthCell.InBounds(map) && FilthMaker.TryMakeFilth(filthCell, map, VGEDefOf.VGE_Filth_DamagedSubstructure))
                {
                    count++;
                    if (count >= 3)
                    {
                        break;
                    }
                }
            }
        }
    }
}
