using System.Collections;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public abstract class LandingStructureBase : Thing
    {
        public int ticksToImpact;

        public int ticksToImpactMax;

        public SavedTexture2D capturedTexture;
        public Vector3 drawSize;
        public Vector3 captureCenter;
        public CellRect captureBounds;
        public Vector3 textureOffset;
        public HashSet<Thing> thrusters = new HashSet<Thing>();
        public List<IntVec3> gravFieldExtenderPositions = new List<IntVec3>();
        public IntVec3 enginePos;
        public Rot4 landingRotation;
        public IntVec3 launchDirection;
        public int randomSeed;
        protected static readonly int ShaderPropertyGravshipHeight = Shader.PropertyToID("_GravshipHeight");
        protected static readonly int ShaderPropertyIsTakeoff = Shader.PropertyToID("_IsTakeoff");
        protected static readonly int GravshipCaptureLayerMaskExclude = LayerMask.GetMask("UI", "GravshipExclude");
        protected static readonly int GravshipCaptureLayerMaskInclude = LayerMask.GetMask("GravshipMask");
        protected static readonly Material MatGravshipBlit = MatLoader.LoadMat("Map/Gravship/GravshipBlit");
        protected static readonly Material MatGravshipChromaKey = MatLoader.LoadMatDirect("Map/Gravship/GravshipChromaKey");
        protected static readonly Material MatGravshipDownwash = MatLoader.LoadMat("Map/Gravship/GravshipDownwash");
        protected static readonly Material MatGravshipLensFlare = MatLoader.LoadMat("Map/Gravship/GravshipLensFlare");
        protected static readonly Material MatGravFieldExtenderGlow = MatLoader.LoadMat("Map/Gravship/GravFieldExtenderGlow");
        protected static readonly Material MatGravEngineGlow = MatLoader.LoadMat("Map/Gravship/GravEngineGlow");

        protected MaterialPropertyBlock flareBlock;
        protected MaterialPropertyBlock thrusterFlameBlock;
        static LandingStructureBase()
        {
            var graphic = VGEDefOf.VGE_FakeTerrain.graphic as Graphic_Single;
            var cache = GraphicDatabase.allGraphics;
            GraphicDatabase.allGraphics = new Dictionary<GraphicRequest, Graphic>();
            graphic = graphic.GetCopy(graphic.drawSize, graphic.Shader) as Graphic_Single;
            graphic.mat = MatGravshipChromaKey;
            VGEDefOf.VGE_FakeTerrain.graphic = graphic;
            GraphicDatabase.allGraphics = cache;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            distortionBlock = new MaterialPropertyBlock();
            flareBlock = new MaterialPropertyBlock();
            thrusterFlameBlock = new MaterialPropertyBlock();
            exhaustFleckSystem = new FleckSystemThrown(map.flecks);
            {
                MatGravshipShadowFallback = new Material(MatGravship.shader);
                MatGravshipShadowFallback.mainTexture = null;
                MatGravshipShadowFallback.color = new Color(0f, 0f, 0f, 0.7f);
            }

            if (respawningAfterLoad is false)
            {
                ticksToImpact = ticksToImpactMax = 600;
                randomSeed = Rand.Int;
                Find.CameraDriver.shaker.DoShake(0.2f, 120);
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksToImpact--;
            if (ticksToImpact <= 0)
            {
                Impact();
            }
            if (exhaustFleckSystem != null)
            {
                exhaustFleckSystem.Update(1);
            }

        }

        protected bool coroutineStarted;
        protected abstract IEnumerator CaptureGravshipCoroutine();

        protected void ScanGeneratedLayout(Map tempMap, CellRect cellRect, out Building_GravEngine engine)
        {
            engine = null;
            Building pilotConsole = null;
            foreach (var pos in cellRect)
            {
                foreach (var thing in pos.GetThingList(tempMap))
                {
                    if (thing.TryGetComp<CompPilotConsole>() != null)
                    {
                        pilotConsole = (Building)thing;
                    }
                    else if (thing.TryGetComp<CompGravshipThruster>() != null || thing.def.HasModExtension<EnemyThrusterExtension>())
                    {
                        thrusters.Add(thing);
                    }
                    else if (thing.def == ThingDefOf.GravFieldExtender)
                    {
                        gravFieldExtenderPositions.Add(thing.Position);
                    }
                    else if (thing is Building_GravEngine gravEngine)
                    {
                        engine = gravEngine;
                        enginePos = thing.Position;
                    }
                }
            }
            launchDirection = IntVec3.Zero;
            foreach (var thruster in thrusters)
            {
                var comp = thruster.TryGetComp<CompGravshipThruster>();
                if (comp != null)
                {
                    launchDirection += thruster.Rotation.AsIntVec3 * comp.Props.directionInfluence;
                }
                else
                {
                    var ext = thruster.def.GetModExtension<EnemyThrusterExtension>();
                    if (ext != null)
                    {
                        launchDirection += thruster.Rotation.AsIntVec3 * ext.directionInfluence;
                    }
                }
            }
            if (launchDirection == IntVec3.Zero && pilotConsole != null)
            {
                launchDirection = pilotConsole.Rotation.AsIntVec3;
            }
            if (engine != null)
            {
                landingRotation = engine.Rotation;
            }
            else
            {
                landingRotation = Rot4.Random;
            }
        }

        protected void RenderAndSaveTexture(Map tempMap, Camera mainCamera, CellRect cellRect, Building_GravEngine engine)
        {
            captureBounds = CellRect.FromCellList(cellRect.Cells).ExpandedBy(1);
            var captureCam = GravshipCacheCameraManager.GravshipCacheCamera;
            captureCam.cullingMask = (mainCamera.cullingMask & ~GravshipCaptureLayerMaskExclude) | GravshipCaptureLayerMaskInclude;
            captureCam.Fit(captureBounds, 15f);
            drawSize = captureBounds.Size.ToVector3().WithY(1f);
            captureCenter = captureBounds.CenterVector3;

            var screenshotWidth = Mathf.RoundToInt((float)Screen.height * captureCam.aspect);
            var screenshotHeight = Screen.height;
            var screenshot = RenderTexture.GetTemporary(screenshotWidth, screenshotHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);
            captureCam.targetTexture = screenshot;
            captureCam.clearFlags = CameraClearFlags.Color;
            captureCam.backgroundColor = MatGravshipChromaKey.color;

            if (engine != null)
            {
                SectionLayer_GravshipMask.Engine = engine;
                SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.Gravship;
            }

            tempMap.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipMask));
            tempMap.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipHull));
            tempMap.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_SubstructureProps));

            if (engine != null)
            {
                SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.None;
            }

            MapUpdate(tempMap);
            captureCam.Render();

            var temporary = RenderTexture.GetTemporary(screenshotWidth, screenshotHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            Graphics.Blit(screenshot, temporary, MatGravshipBlit);
            capturedTexture = (SavedTexture2D)temporary.CreateTexture2D(TextureFormat.ARGB32, mipChain: true);
            capturedTexture.Texture.filterMode = FilterMode.Bilinear;

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(temporary);
            RenderTexture.ReleaseTemporary(screenshot);
            captureCam.targetTexture = null;
            captureCam.clearFlags = CameraClearFlags.Color;
            captureCam.backgroundColor = new Color(0, 0, 0, 0);
            var xCorrection = launchDirection.x > 0 ? -1f : 0f;
            var zCorrection = launchDirection.z > 0 ? -1f : 0f;
            textureOffset = captureCenter - tempMap.Center.ToVector3Shifted() + new Vector3(xCorrection, 0f, zCorrection);
        }

        public void MapUpdate(Map map)
        {
            map.glowGrid.GlowGridUpdate_First();
            map.waterInfo.SetTextures();
            map.mapDrawer.MapMeshDrawerUpdate_First();
            map.mapDrawer.DrawMapMesh();
            map.dynamicDrawManager.DrawDynamicThings();
        }

        public void SkyManagerUpdate(Map original, SkyManager sky)
        {
            sky.curSky = original.skyManager.CurSky;
            sky.curSkyGlowInt = original.skyManager.CurSkyGlow;
            sky.curSky.colors.sky = original.skyManager.CurSky.colors.sky;

            MatBases.LightOverlay.color = original.skyManager.CurSky.colors.sky;
            MatBases.FogOfWar.color = original.FogOfWarColor ?? SkyManager.FogOfWarBaseColor;

            var shadowColor = original.skyManager.CurSky.colors.shadow;
            var overridenShadowVector = original.skyManager.GetOverridenShadowVector();
            if (overridenShadowVector.HasValue)
            {
                sky.SetSunShadowVector(overridenShadowVector.Value);
            }
            else
            {
                sky.SetSunShadowVector(GenCelestial.GetLightSourceInfo(original, GenCelestial.LightType.Shadow).vector);
                shadowColor = Color.Lerp(Color.white, shadowColor, GenCelestial.CurShadowStrength(original));
            }
            var lightSourceInfo = GenCelestial.GetLightSourceInfo(original, GenCelestial.LightType.LightingSun);
            var lightSourceInfo2 = GenCelestial.GetLightSourceInfo(original, GenCelestial.LightType.LightingMoon);
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectSun, new Vector4(lightSourceInfo.vector.x, 0f, lightSourceInfo.vector.y, lightSourceInfo.intensity));
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectMoon, new Vector4(lightSourceInfo2.vector.x, 0f, lightSourceInfo2.vector.y, lightSourceInfo2.intensity));
            Shader.SetGlobalFloat(SkyManager.LightsourceShineSizeReduction, 20f * (1f / original.skyManager.CurSky.lightsourceShineSize));
            Shader.SetGlobalFloat(SkyManager.LightsourceShineIntensity, original.skyManager.CurSky.lightsourceShineIntensity);
            Shader.SetGlobalFloat(SkyManager.DayPercent, GenLocalDate.DayPercent(original));
            MatBases.SunShadow.color = shadowColor;
            MatBases.SunShadowFade.color = shadowColor;
        }

        public abstract void Impact();

        public static void CreateTempMap(IntVec3 size, Map source, out MapParent mapParent, out Map map)
        {
            mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(VGEDefOf.VGE_GravshipGenerationSite);
            mapParent.Tile = source.Tile;
            mapParent.SetFaction(Faction.OfPlayer);
            Find.WorldObjects.Add(mapParent);
            map = MapGenerator.GenerateMap(size, mapParent, mapParent.MapGeneratorDef);
        }

        protected static readonly Material MatGravship = MatLoader.LoadMat("Map/Gravship/Gravship");
        protected static readonly Material MatGravshipShadow = MatLoader.LoadMat("Map/Gravship/GravshipShadow");
        protected static readonly Material MatGravshipDistortion = MatLoader.LoadMat("Map/Gravship/GravshipDistortion");
        protected static Material MatGravshipShadowFallback;
        protected MaterialPropertyBlock distortionBlock;
        protected FleckSystem exhaustFleckSystem;
        protected Dictionary<Thing, EventQueue> exhaustTimers = new Dictionary<Thing, EventQueue>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref drawSize, "drawSize");
            Scribe_Values.Look(ref captureCenter, "captureCenter");
            Scribe_Values.Look(ref captureBounds, "captureBounds");
            Scribe_Collections.Look(ref thrusters, "thrusters", LookMode.Deep);
            Scribe_Collections.Look(ref gravFieldExtenderPositions, "gravFieldExtenderPositions", LookMode.Value);
            Scribe_Values.Look(ref enginePos, "enginePos");
            Scribe_Values.Look(ref landingRotation, "landingRotation");
            Scribe_Values.Look(ref launchDirection, "launchDirection");
            Scribe_Values.Look(ref textureOffset, "textureOffset");
            Scribe_Values.Look(ref randomSeed, "randomSeed");
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            DrawGravship(drawLoc + textureOffset);
        }

        private void DrawGravship(Vector3 drawLoc)
        {
            if (capturedTexture?.Texture == null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    if (coroutineStarted is false)
                    {
                        Find.CameraDriver.StartCoroutine(CaptureGravshipCoroutine());
                    }
                });
                return;
            }

            var progress = 1f - (float)ticksToImpact / (float)ticksToImpactMax;
            progress = progress.RemapClamped(0f, 0.95f, 0f, 1f);
            var height = Mathf.Pow(1f - progress, 5f);

            Vector3 vector;
            Vector3 vector2;
            if (landingRotation == Rot4.North || landingRotation == Rot4.South)
            {
                vector = new Vector3(0f, 0f, 100f * height);
                vector2 = landingRotation.AsQuat * -launchDirection.ToVector3().normalized * 200f * height;
            }
            else
            {
                vector = new Vector3(0f, 0f, 200f * height);
                vector2 = landingRotation.AsQuat * -launchDirection.ToVector3().normalized * 100f * Mathf.Pow(1f - progress, 9f);
            }

            var groundEffectsCenter = drawLoc + vector2;
            var gravshipDrawCenter = drawLoc + vector + vector2;
            gravshipDrawCenter.y = AltitudeLayer.Skyfaller.AltitudeFor();

            var vector3 = Find.Camera.WorldToViewportPoint(gravshipDrawCenter);
            distortionBlock.SetFloat(ShaderPropertyIDs.Progress, progress);
            distortionBlock.SetFloat(ShaderPropertyGravshipHeight, height);
            distortionBlock.SetVector(ShaderPropertyIDs.DrawPos, vector3);
            distortionBlock.SetFloat(ShaderPropertyIsTakeoff, 0f);
            DrawLayer(MatGravshipDistortion, Find.Camera.transform.position.SetToAltitude(AltitudeLayer.Weather).WithYOffset(0.07317074f), distortionBlock, Find.Camera);

            MatGravship.mainTexture = capturedTexture.Texture;
            MatGravship.color = Color.white;
            MatGravship.SetFloat(ShaderPropertyIDs.Progress, progress);
            MatGravship.SetFloat(ShaderPropertyGravshipHeight, height);
            MatGravship.SetFloat(ShaderPropertyIsTakeoff, 0f);
            GenDraw.DrawQuad(MatGravship, gravshipDrawCenter, landingRotation.AsQuat, this.drawSize);

            MatGravshipShadowFallback.mainTexture = capturedTexture.Texture;
            MatGravshipShadowFallback.SetFloat(ShaderPropertyIDs.Progress, 1f - progress);
            MatGravshipShadowFallback.SetFloat(ShaderPropertyGravshipHeight, height);
            MatGravshipShadowFallback.SetFloat(ShaderPropertyIsTakeoff, 0f);
            MatGravshipShadowFallback.color = MatGravshipShadow.color.WithAlpha(progress.RemapClamped(0.9f, 1f, 1f, 0f));
            var shadowAlpha = progress.RemapClamped(0.9f, 1f, 0.35f, 0f);

            var shadowPos = (drawLoc + vector2).SetToAltitude(AltitudeLayer.Gas).WithYOffset(0.03658537f);
            var blurOffset = 0.15f;
            var offsets = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(blurOffset, 0, 0),
                new Vector3(-blurOffset, 0, 0),
                new Vector3(0, 0, blurOffset),
                new Vector3(0, 0, -blurOffset),
            };

            if (progress > 0f && !base.Map.Biome.inVacuum)
            {
                MatGravshipDownwash.SetFloat(ShaderPropertyIDs.Progress, progress);
                MatGravshipDownwash.SetFloat(ShaderPropertyGravshipHeight, height);
                MatGravshipDownwash.SetVector(ShaderPropertyIDs.DrawPos, Find.Camera.WorldToViewportPoint(groundEffectsCenter));
                MatGravshipDownwash.SetFloat(ShaderPropertyIsTakeoff, 0f);
                DrawLayer(MatGravshipDownwash, Find.Camera.transform.position.SetToAltitude(AltitudeLayer.Gas).WithYOffset(0.03658537f), null, Find.Camera);

                foreach (var offset in offsets)
                {
                    MatGravshipShadowFallback.color = new Color(0.1f, 0.1f, 0.1f, shadowAlpha / offsets.Length);
                    GenDraw.DrawQuad(MatGravshipShadowFallback, shadowPos + offset, Quaternion.identity, this.drawSize * 1.08f);
                }
            }

            if (thrusters.NullOrEmpty()) return;

            var value = new Color(1f, 1f, 1f, 1f);
            value *= Mathf.Lerp(0.75f, 1f, Mathf.PerlinNoise1D(progress * 100f));
            value.a = Mathf.InverseLerp(0f, 0.1f, 1f - progress);

            foreach (var thruster in thrusters)
            {
                var comp = thruster.TryGetComp<CompGravshipThruster>();
                if (comp != null)
                {
                    var props = comp.Props;
                    DrawThrusterFlame(thruster, gravshipDrawCenter, value, props.flameSize, props.flameOffsetsPerDirection, props.FlameShaderType, props.flameShaderParameters, props.exhaustSettings);
                }
                else
                {
                    var ext = thruster.def.GetModExtension<EnemyThrusterExtension>();
                    if (ext != null)
                    {
                        DrawThrusterFlame(thruster, gravshipDrawCenter, value, ext.flameSize, ext.flameOffsetsPerDirection, ext.FlameShaderType, ext.flameShaderParameters, ext.exhaustSettings);
                    }
                }
            }

            MatGravFieldExtenderGlow.SetColor("_Color2", value);
            foreach (var gravFieldExtenderPosition in gravFieldExtenderPositions)
            {
                var vector7 = gravFieldExtenderPosition.ToVector3() + ThingDefOf.GravFieldExtender.graphicData.drawSize.ToVector3() * 0.5f;
                var position3 = (gravshipDrawCenter + (vector7 - captureCenter)).SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.07317074f);
                GenDraw.DrawQuad(MatGravFieldExtenderGlow, position3, Quaternion.identity, 8f);
            }

            MatGravEngineGlow.SetColor("_Color2", value);
            var position4 = (gravshipDrawCenter + (enginePos.ToVector3() + new Vector3(0.5f, 0, 0.5f) - captureCenter)).SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.07317074f);
            GenDraw.DrawQuad(MatGravEngineGlow, position4, Quaternion.identity, 12.5f);
        }

        private void DrawThrusterFlame(Thing thruster, Vector3 gravshipDrawCenter, Color value, float flameSize, List<Vector3> flameOffsetsPerDirection, ShaderTypeDef flameShaderType, List<ShaderParameter> flameShaderParameters, CompProperties_GravshipThruster.ExhaustSettings exhaustSettings)
        {
            var num = (float)thruster.def.size.x * flameSize;
            var vector4 = thruster.Rotation.AsQuat * flameOffsetsPerDirection[thruster.Rotation.AsInt];
            var vector5 = GenThing.TrueCenter(thruster.Position, thruster.Rotation, thruster.def.size, 0f) - thruster.Rotation.AsIntVec3.ToVector3() * ((float)thruster.def.size.z * 0.5f + num * 0.5f) + vector4;
            var position2 = (gravshipDrawCenter + (vector5 - captureCenter)).SetToAltitude(AltitudeLayer.Skyfaller).WithYOffset(0.07317074f);
            var req = new MaterialRequest(flameShaderType.Shader);
            req.renderQueue = 3201;
            var mat = MaterialPool.MatFrom(req);
            thrusterFlameBlock.Clear();
            thrusterFlameBlock.SetColor("_Color2", value);
            foreach (var flameShaderParameter in flameShaderParameters)
            {
                flameShaderParameter.Apply(thrusterFlameBlock);
            }
            GenDraw.DrawQuad(mat, position2, landingRotation.AsQuat * thruster.Rotation.AsQuat, num, thrusterFlameBlock);

            var vector6 = Find.Camera.WorldToViewportPoint(position2);
            flareBlock.SetVector(ShaderPropertyIDs.DrawPos, vector6);
            MatGravshipLensFlare.SetColor("_Color2", value);
            DrawLayer(MatGravshipLensFlare, Find.Camera.transform.position.SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.03658537f), flareBlock, Find.Camera);

            if (exhaustSettings.enabled)
            {
                if (!exhaustTimers.ContainsKey(thruster))
                {
                    exhaustFleckSystem.handledDefs.AddUnique(exhaustSettings.ExhaustFleckDef);
                    exhaustTimers.Add(thruster, new EventQueue(1f / exhaustSettings.emissionsPerSecond));
                }
                var eventQueue = exhaustTimers[thruster];
                eventQueue.Push(Time.deltaTime);
                while (eventQueue.Pop())
                {
                    EmitSmoke(exhaustSettings, position2, landingRotation.AsQuat, thruster.Rotation.AsQuat);
                }
            }
        }

        private void EmitSmoke(CompProperties_GravshipThruster.ExhaustSettings settings, Vector3 position, Quaternion gravshipRotation, Quaternion thrusterRotation)
        {
            var quaternion = Quaternion.identity;
            if (settings.inheritThrusterRotation)
            {
                quaternion = thrusterRotation * quaternion;
            }
            if (settings.inheritGravshipRotation)
            {
                quaternion = gravshipRotation * quaternion;
            }
            exhaustFleckSystem.CreateFleck(new FleckCreationData
            {
                def = settings.ExhaustFleckDef,
                spawnPosition = position + quaternion * settings.spawnOffset + UnityEngine.Random.insideUnitSphere.WithY(0f).normalized * settings.spawnRadiusRange.RandomInRange,
                velocity = quaternion * Quaternion.Euler(0f, settings.velocityRotationRange.RandomInRange, 0f) * (settings.velocity * settings.velocityMultiplierRange.RandomInRange),
                rotationRate = settings.rotationOverTimeRange.RandomInRange,
                ageTicksOverride = -1
            });
        }

        private void DrawLayer(Material mat, Vector3 position, MaterialPropertyBlock props, Camera camera)
        {
            var num = camera.orthographicSize * 2f;
            var matrix = Matrix4x4.TRS(s: new Vector3(num * camera.aspect, 1f, num), pos: position, q: Quaternion.identity);
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0, null, 0, props);
        }
    }
}
