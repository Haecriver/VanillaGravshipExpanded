using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingStructure : LandingStructureBase
    {
        public KCSG.StructureLayoutDef layoutDef;
        public bool forceNullFaction;

        protected override IEnumerator CaptureGravshipCoroutine()
        {
            coroutineStarted = true;
            var maxSize = Mathf.Max(layoutDef.Sizes.x, layoutDef.Sizes.z) + 3;
            CreateTempMap(new IntVec3(maxSize, 1, maxSize), Map, out var mapParent, out var tempMap);
            var originalMap = Current.Game.CurrentMap;
            var mainCamera = Find.Camera;
            var cameraDriver = mainCamera.GetComponent<CameraDriver>();

            var wasCamDriverEnabled = cameraDriver.enabled;
            var wasCamEnabled = mainCamera.enabled;
            cameraDriver.enabled = false;
            mainCamera.enabled = false;
            Current.Game.CurrentMap = tempMap;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            try
            {
                Rand.PushState(randomSeed);
                var cellRect = SpawnLayout(tempMap, tempMap.Center);
                Rand.PopState();
                ScanGeneratedLayout(tempMap, cellRect, out var engine);
                RenderAndSaveTexture(tempMap, mainCamera, cellRect, engine);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to capture " + layoutDef + ": " + ex.ToString());
            }
            Current.Game.CurrentMap = originalMap;
            mainCamera.enabled = wasCamEnabled;
            cameraDriver.enabled = wasCamDriverEnabled;
            Find.WorldObjects.Remove(mapParent);
            Find.Maps.Remove(tempMap);
            coroutineStarted = false;
        }

        private CellRect SpawnLayout(Map map, IntVec3 position)
        {
            var cellRect = CellRect.CenteredOn(position, layoutDef.Sizes.x, layoutDef.Sizes.z);
            GenOption.GetAllMineableIn(cellRect, map);
            LayoutUtils.CleanRect(layoutDef, map, cellRect, true);
            var things = new List<Thing>();
            AutoHomeAreaMaker_MarkHomeAroundThing_Patch.preventHomeArea = true;
            layoutDef.Generate(cellRect, map, things, Faction, forceNullFaction: forceNullFaction);
            var engine = things.OfType<Building_GravEngine>().FirstOrDefault();
            if (engine != null && engine.Faction != Faction.OfPlayer)
            {
                engine.SetFaction(Faction.OfPlayer);
                engine.ForceSubstructureDirty();
            }
            AutoHomeAreaMaker_MarkHomeAroundThing_Patch.preventHomeArea = false;
            return cellRect;
        }

        public override void Impact()
        {
            Rand.PushState(randomSeed);
            SpawnLayout(Map, Position);
            Rand.PopState();
            Destroy(DestroyMode.Vanish);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref layoutDef, "layoutDef");
            Scribe_Values.Look(ref forceNullFaction, "forceNullFaction");
            Scribe_Values.Look(ref ticksToImpact, "ticksToImpact");
            Scribe_Values.Look(ref ticksToImpactMax, "ticksToImpactMax");
        }
    }
}
