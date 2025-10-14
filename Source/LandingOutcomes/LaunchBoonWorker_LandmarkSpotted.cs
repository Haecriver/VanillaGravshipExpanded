using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_LandmarkSpotted : LaunchBoonWorker
    {
        public LaunchBoonWorker_LandmarkSpotted(LaunchBoonDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return FindValidLandmarkTile(gravship.Engine.Tile, out _);
        }

        public override void ApplyBoon(Gravship gravship)
        {
            if (FindValidLandmarkTile(gravship.Engine.Tile, out PlanetTile landmarkTile))
            {
                var validLandmarks = DefDatabase<LandmarkDef>.AllDefsListForReading
                    .Where(ld => ld.IsValidTile(landmarkTile, landmarkTile.Layer))
                    .ToList();

                if (validLandmarks.Count > 0)
                {
                    var selectedLandmark = validLandmarks.RandomElement();
                    Find.World.landmarks.AddLandmark(selectedLandmark, landmarkTile, landmarkTile.Layer);
                    var engine = gravship.Engine;
                    var text = LetterText.Formatted(engine.RenamableLabel.Named("GRAVSHIP"), engine.launchInfo.pilot.Named("PILOT"), engine.launchInfo.copilot.Named("COPILOT"), selectedLandmark.label.Named("LANDMARK"));
                    SendStandardLetter(gravship.Engine, null, new LookTargets(landmarkTile), text);
                }
            }
        }

        private bool FindValidLandmarkTile(PlanetTile originTile, out PlanetTile result)
        {
            if (originTile.Layer is not SurfaceLayer)
            {
                var surfaceLayer = originTile.Layer.connections.Keys.FirstOrDefault(l => l is SurfaceLayer);
                if (surfaceLayer == null)
                {
                    result = PlanetTile.Invalid;
                    return false;
                }
                originTile = surfaceLayer.GetClosestTile_NewTemp(originTile);
                if (originTile == PlanetTile.Invalid)
                {
                    result = PlanetTile.Invalid;
                    return false;
                }
            }
            result = PlanetTile.Invalid;
            return TileFinder.TryFindTileWithDistance(originTile, 1, 5, out result,
                (PlanetTile tile) => Find.World.landmarks[tile] == null &&
                                   tile.Layer[tile]?.PrimaryBiome != null &&
                                   !tile.Layer[tile].PrimaryBiome.impassable &&
                                   tile.Layer[tile].hilliness != Hilliness.Impassable,
                TileFinderMode.Random, exitOnFirstTileFound: false);
        }
    }
}
