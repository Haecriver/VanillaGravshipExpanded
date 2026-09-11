using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class MaintenanceAndDeterioration_MapComponent : MapComponent
    {
        private HashSet<Thing> thingsInSpace = new HashSet<Thing>();
        
        private Dictionary<Thing, int> timeInSpace = new Dictionary<Thing, int>();
        
        private const int CHECK_INTERVAL = 60;
        
        private const float DAMAGE_PER_TICK = 0.01f / 60f;

        public HashSet<Thing> maintainables_InMap = new HashSet<Thing>();

        public List<Color> vacBarrierColorGrid;

        private static Map cachedMap;
        private static MaintenanceAndDeterioration_MapComponent cachedComp;

        public MaintenanceAndDeterioration_MapComponent(Map map) : base(map)
        {
            vacBarrierColorGrid = GetDefaultList();
        }

        public static MaintenanceAndDeterioration_MapComponent GetCompFast(Map map) => map == cachedMap ? cachedComp : cachedComp = (cachedMap = map).GetComponent<MaintenanceAndDeterioration_MapComponent>();

        public override void MapComponentTick()
        {
            // Use map.IsHashIntervalTick rather than using a counter to ensure that different maps process deterioration
            // at different ticks, spreading the performance impact on multiple tics rather than doing everything in a single
            // tick, potentially causing longer stutters.
            if (map.Tile.LayerDef.isSpace && map.IsHashIntervalTick(CHECK_INTERVAL))
            {
                ProcessSpaceDeterioration();
            }
        }

        private void ProcessSpaceDeterioration()
        {
            var allThings = map.listerThings.AllThings;
            
            for (int i = 0; i < allThings.Count; i++)
            {
                Thing thing = allThings[i];
                
                if (!ShouldDeteriorateInSpace(thing))
                {
                    continue;
                }
                
                if (IsOnSpaceTerrain(thing))
                {
                    // Add returns true if the value was added, or false if it already existed
                    if (thingsInSpace.Add(thing))
                    {
                        timeInSpace[thing] = 0;
                    }
                    
                    timeInSpace[thing] = timeInSpace[thing] + CHECK_INTERVAL;
                    
                    ApplySpaceDamage(thing);
                }
                else
                {
                    // Remove returns true if value existed and was removed, or false if it didn't exist
                    if (thingsInSpace.Remove(thing))
                    {
                        timeInSpace.Remove(thing);
                    }
                }
            }
            
            var toRemove = new List<Thing>();
            foreach (Thing thing in thingsInSpace)
            {
                if (!allThings.Contains(thing))
                {
                    toRemove.Add(thing);
                }
            }
            
            foreach (Thing thing in toRemove)
            {
                thingsInSpace.Remove(thing);
                timeInSpace.Remove(thing);
            }
        }
        
        private bool IsOnSpaceTerrain(Thing thing)
        {
            var position = thing.Position;
            if (!position.InBounds(map))
            {
                return false;
            }
            
            var terrain = position.GetTerrain(map);
            return terrain == TerrainDefOf.Space;
        }
        
        private bool ShouldDeteriorateInSpace(Thing thing)
        {
            if (thing is Pawn || thing is Building)
            {
                return false;
            }
            
            if (thing.def.destroyable == false || thing.def.useHitPoints == false)
            {
                return false;
            }
            
            return true;
        }
        
        private void ApplySpaceDamage(Thing thing)
        {
            var maxHitPoints = thing.MaxHitPoints;
            var damage = Mathf.Max(1, Mathf.RoundToInt(maxHitPoints * DAMAGE_PER_TICK * CHECK_INTERVAL));
            
            thing.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, damage, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
            
            if (!thing.Destroyed && thing.HitPoints <= 0)
            {
                thing.Destroy();
            }
        }
        
        public int GetTimeInSpace(Thing thing)
        {
            if (timeInSpace.TryGetValue(thing, out var time))
            {
                return time;
            }
            return 0;
        }
        
        public bool IsThingInSpace(Thing thing)
        {
            return thingsInSpace.Contains(thing) && IsOnSpaceTerrain(thing);
        }

        public void AddMaintainableToMap(Thing thing)
        {
            maintainables_InMap.Add(thing);
        }

        public void RemoveMaintainableFromMap(Thing thing)
        {
            maintainables_InMap.Remove(thing);
        }

        public float AverageMaintenanceForEngine(Building_GravEngine engine)
        {
            var totalMaintenance = 0f;
            var totalBuildings = 0;

            foreach (var thing in maintainables_InMap)
            {
                // Only player buildings
                if (thing.Faction != Faction.OfPlayer)
                    continue;

                var comp = thing.TryGetComp<CompGravMaintainable>();
                // Not null and maintenance is falling
                if (comp is not { maintenanceFalls: true })
                    continue;

                if (!engine.LooselyConnectedToGravEngine(thing))
                    continue;

                totalMaintenance += thing.TryGetComp<CompGravMaintainable>().maintenance;
                totalBuildings++;
            }

            if (totalBuildings > 0)
                return totalMaintenance / totalBuildings;
            return 1;
        }

        public void ChangeGlobalMaintenance(float amount, float chance)
        {
            if (maintainables_InMap.Count > 0)
            {
                foreach (Thing thing in maintainables_InMap)
                {
                    if (Rand.Chance(chance))
                    {
                        CompGravMaintainable comp = thing.TryGetComp<CompGravMaintainable>();
                        comp.maintenance += amount * thing.GetStatValue(VGEDefOf.VGE_MaintenanceSensitivity);
                    }

                }

            }

        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            if (map.areaManager.BuildVacBarrierRoof() == null)
                map.areaManager.areas.Add(new Area_BuildVacBarrierRoof(map.areaManager));
        }
    
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref vacBarrierColorGrid, nameof(vacBarrierColorGrid), LookMode.Value);
            if (vacBarrierColorGrid == null || vacBarrierColorGrid.Count != map.cellIndices.NumGridCells)
                vacBarrierColorGrid = GetDefaultList();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                vacBarrierColorGrid.Capacity = map.cellIndices.NumGridCells;
        }

        private List<Color> GetDefaultList()
        {
            var list = Enumerable.Repeat(VacBarrierRoofUtility.BaseColor, map.cellIndices.NumGridCells).ToList();
            list.Capacity = map.cellIndices.NumGridCells;
            return list;
        }
    }
}
