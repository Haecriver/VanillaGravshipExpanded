using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_AsteroidDiscovery : LaunchBoonWorker
    {
        public LaunchBoonWorker_AsteroidDiscovery(LaunchBoonDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return true;
        }

        public override void ApplyBoon(Gravship gravship)
        {
            var engine = gravship.Engine;
            var landingTile = engine.Map.Tile;
            if (TryFindAsteroidTile(landingTile, out PlanetTile asteroidTile))
            {
                var asteroid = (ResourceAsteroidMapParent)WorldObjectMaker.MakeWorldObject(VGEDefOf.AsteroidMiningSite);
                asteroid.Tile = asteroidTile;
                var mineableDefs = DefDatabase<ThingDef>.AllDefs
                    .Where(def => def.building?.isResourceRock ?? false)
                    .ToList();
                
                if (mineableDefs.Any())
                {
                    var resource = mineableDefs.RandomElement();
                    asteroid.preciousResource = resource;
                    asteroid.nameInt = "AsteroidName".Translate(resource.label.Named("RESOURCE"));
                }
                Find.WorldObjects.Add(asteroid);
                
                SendStandardLetter(engine, null, new LookTargets(asteroidTile));
            }
            else
            {
                SendStandardLetter(engine, null, engine);
            }
        }
        
        private bool TryFindAsteroidTile(PlanetTile origin, out PlanetTile result)
        {
            result = PlanetTile.Invalid;
            if (!Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(origin, PlanetLayerDefOf.Orbit, out var layer))
            {
                return false;
            }
            FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(origin, 1f, 10f);
            return layer.FastTileFinder.Query(query).TryRandomElement(out result);
        }
    }
}
