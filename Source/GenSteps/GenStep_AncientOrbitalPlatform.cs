
using RimWorld;
using RimWorld.Planet;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Noise;
namespace VanillaGravshipExpanded
{
    public class GenStep_AncientOrbitalPlatform : GenStep_OrbitalPlatform
    {

        private new static readonly IntRange SizeRange = new IntRange(40, 45);

        private new CellRect GeneratePlatform(Map map, Faction faction, float? threatPoints)
        {
            IntVec2 size = new IntVec2(SizeRange.RandomInRange, SizeRange.RandomInRange);
            Rot4 random = Rot4.Random;
            CellRect cellRect = map.Center.RectAbout(size, random).ClipInsideMap(map);
            StructureGenParams parms = new StructureGenParams
            {
                size = cellRect.Size
            };
            LayoutWorker worker = LayoutDef.Worker;
            LayoutStructureSketch layoutStructureSketch = worker.GenerateStructureSketch(parms);
            map.layoutStructureSketches.Add(layoutStructureSketch);
            worker.Spawn(layoutStructureSketch, map, cellRect.Min, threatPoints, null, roofs: true, canReuseSketch: false, faction);
            MapGenerator.SetVar("SpawnRect", cellRect);
            MapGenerator.UsedRects.Add(cellRect);
            return cellRect;
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!ModLister.CheckOdyssey("Orbital Platform"))
            {
                return;
            }
            float? threatPoints = null;
            if (parms.sitePart != null)
            {
                threatPoints = parms.sitePart.parms.points;
            }
            if (!threatPoints.HasValue)
            {
                Site site = map.Parent as Site;
                if (site != null)
                {
                    threatPoints = site.ActualThreatPoints;
                }
            }
            Faction faction = GetFaction(map);
            CellRect rect = GeneratePlatform(map, faction, threatPoints);
            if (Rand.Chance(0.33f))
            {
                DoRing(map, rect);
            }
            else if (Rand.Chance(0.5f))
            {
                DoLargePlatforms(map, rect);
            }
            else
            {
                DoSmallPlatforms(map, rect);
            }
            SpawnCannons(map, rect.ExpandedBy(6));
            map.FogOfWarColor = fogOfWarColor.ToColor;
            map.OrbitalDebris = orbitalDebrisDef;
            SpawnExteriorPrefabs(map, rect.ExpandedBy(6), faction);
        }



    }


}