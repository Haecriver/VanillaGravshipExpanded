using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using System.Collections.Generic;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class GravshipHelper
    {
        private static readonly SimpleCurve LaunchBoonChanceFromQualityCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.5f, 0.03f),
            new CurvePoint(1f, 0.3f)
        };
        public static readonly Material TileMaterial = MaterialPool.MatFrom("Things/Terrain/Substructure/SubscaffoldingTile", ShaderDatabase.Cutout);
        public static readonly Material MaskOverlayMaterial = SolidColorMaterials.NewSolidColorMaterial(new Color(1f, 1f, 0f), ShaderDatabase.TransparentPostLight);
        public static readonly Texture2D GravjumperTexture = ContentFinder<Texture2D>.Get("UI/MapIcons/Gravjumper_WorldIcon");
        public static readonly Texture2D GravshipTexture = ContentFinder<Texture2D>.Get("UI/MapIcons/Gravship_WorldIcon");
        public static readonly Texture2D GravhulkTexture = ContentFinder<Texture2D>.Get("UI/MapIcons/Gravhulk_WorldIcon");

        public static readonly List<ThingDef> GravEngineDefs = [ThingDefOf.GravEngine, VGEDefOf.VGE_GravjumperEngine, VGEDefOf.VGE_GravhulkEngine];

        public static void AddScaffoldQuad(LayerSubMesh subMesh, IntVec3 cell, float y)
        {
            int count = subMesh.verts.Count;
            subMesh.verts.Add(new Vector3(cell.x, y, cell.z));
            subMesh.verts.Add(new Vector3(cell.x, y, cell.z + 1));
            subMesh.verts.Add(new Vector3(cell.x + 1, y, cell.z + 1));
            subMesh.verts.Add(new Vector3(cell.x + 1, y, cell.z));
            subMesh.uvs.Add(new Vector2(0f, 0f));
            subMesh.uvs.Add(new Vector2(0f, 1f));
            subMesh.uvs.Add(new Vector2(1f, 1f));
            subMesh.uvs.Add(new Vector2(1f, 0f));
            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 1);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count + 3);
        }

        public static bool IsSustructureOrScaffold(this TerrainDef terrainDef)
        {
            return terrainDef.HasTag("Substructure") || terrainDef == VGEDefOf.VGE_DamagedSubstructure
            || terrainDef == VGEDefOf.VGE_GravshipSubscaffold;
        }

        public static bool IsScaffold(this TerrainDef terrainDef)
        {
            return terrainDef == VGEDefOf.VGE_GravshipSubscaffold;
        }

        public static void RegenerateScaffoldLayer(SectionLayer sectionLayer, Material material, AltitudeLayer altitudeLayer, int? renderLayer = null)
        {
            sectionLayer.ClearSubMeshes(MeshParts.All);
            TerrainGrid terrainGrid = sectionLayer.Map.terrainGrid;
            foreach (IntVec3 cell in sectionLayer.section.CellRect)
            {
                if (terrainGrid.FoundationAt(cell).IsScaffold())
                {
                    LayerSubMesh subMesh = sectionLayer.GetSubMesh(material);
                    if (subMesh != null)
                    {
                        if (renderLayer.HasValue)
                            subMesh.renderLayer = renderLayer.Value;
                        float y = altitudeLayer.AltitudeFor();
                        AddScaffoldQuad(subMesh, cell, y);
                    }
                }
            }
            sectionLayer.FinalizeMesh(MeshParts.All);
        }

        private static PlanetTile cachedOrigin;
        private static PlanetTile cachedDest;
        private static int cachedDistance;
        private static PlanetLayer cachedOriginLayer;
        private static PlanetLayer cachedDestLayer;
        private static readonly List<PlanetLayerConnection> connections = new List<PlanetLayerConnection>();

        public static int GetDistance(PlanetTile from, PlanetTile to)
        {
            if (cachedOrigin == from && cachedDest == to)
            {
                return cachedDistance;
            }
            cachedOrigin = from;
            cachedDest = to;
            cachedDistance = 0;
            if (from.Layer != to.Layer)
            {
                if (cachedOriginLayer == from.Layer && cachedDestLayer == to.Layer)
                {
                }
                else
                {
                    if (!from.Layer.TryGetPath(to.Layer, connections, out var cost))
                    {
                        connections.Clear();
                        return 0;
                    }
                    cachedOriginLayer = to.Layer;
                    cachedDestLayer = from.Layer;
                    connections.Clear();
                }
                from = to.Layer.GetClosestTile_NewTemp(from);
            }
            cachedDistance = (int)(Find.WorldGrid.TraversalDistanceBetween(from, to) * to.LayerDef.rangeDistanceFactor);
            return cachedDistance;
        }

        public static bool IsGravshipLaunch(this PreceptDef ritual)
        {
            return ritual == PreceptDefOf.GravshipLaunch ||
                   ritual == VGEDefOf.VGE_GravjumperLaunch ||
                   ritual == VGEDefOf.VGE_GravhulkLaunch;
        }

        public static float LaunchBoonChanceFromQuality(float quality)
        {
            return LaunchBoonChanceFromQualityCurve.Evaluate(quality);
        }

        public static bool PlayerHasGravEngineUnderConstruction() => Find.Maps.Any(PlayerHasGravEngineUnderConstruction);

        public static bool PlayerHasGravEngineUnderConstruction(Map map)
        {
            return ListerThingsUtility.AnyThingWithBuildBlueprintDefs(map, GravEngineDefs) ||
                   ListerThingsUtility.AnyThingWithFrameDefs(map, GravEngineDefs);
        }

        public static Texture2D GetExpandingTextureForEngine(ThingDef engineDef)
        {
            if (engineDef == ThingDefOf.GravEngine)
                return GravshipTexture;
            if (engineDef == VGEDefOf.VGE_GravjumperEngine)
                return GravjumperTexture;
            if (engineDef == VGEDefOf.VGE_GravhulkEngine)
                return GravhulkTexture;
            return null;
        }

        public static void InspectGravEngine(Thing thing = null, IIncidentTarget incidentTarget = null, List<string> questTags = null, bool silent = false, bool setToPlayerFaction = true)
        {
            // Remember to mirror any (relevant) changes in Building_GravEngine_Inspect_Patch:Postfix

            if (Find.ResearchManager.gravEngineInspected)
                return;

            incidentTarget ??= thing?.MapHeld ?? (IIncidentTarget)Find.AnyPlayerHomeMap ?? Find.World;
            if (incidentTarget == null)
            {
                Log.Error("No possible incident target found.");
                return;
            }

            Find.ResearchManager.gravEngineInspected = true;

            if (thing != null)
            {
                if (questTags != null)
                    QuestUtility.SendQuestTargetSignals(questTags, QuestUtility.QuestTargetSignalPart_Inspected, thing.Named(SignalArgsNames.Subject));
                QuestUtility.SendQuestTargetSignals(thing.Map.Parent.questTags, QuestUtility.QuestTargetSignalPart_Inspected, thing.Named(SignalArgsNames.Subject));
            }

            LessonAutoActivator.TeachOpportunity(ConceptDefOf.Gravship, OpportunityType.Important);
            Find.ResearchManager.FinishProject(ResearchProjectDefOf.BasicGravtech);

            if (setToPlayerFaction && thing != null && thing.def.CanHaveFaction && thing.Faction != Faction.OfPlayer)
                thing.SetFaction(Faction.OfPlayer);

            var quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.GravEngine, StorytellerUtility.DefaultThreatPointsNow(incidentTarget));
            if (!silent)
            {
                var diaNode = new DiaNode("GravEngineInspectedLetterContents".Translate());
                var diaOption = new DiaOption("ViewQuest".Translate())
                {
                    resolveTree = true,
                    action = () =>
                    {
                        Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
                        ((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
                    }
                };
                diaNode.options.Add(diaOption);
                diaNode.options.Add(new DiaOption("Close".Translate())
                {
                    resolveTree = true
                });
                var dialog_NodeTree = new Dialog_NodeTree(diaNode)
                {
                    forcePause = true
                };
                Find.WindowStack.Add(dialog_NodeTree);
            }
        }
    }
}
