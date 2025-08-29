using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class GravshipHelper
    {
        public static readonly Material TileMaterial = MaterialPool.MatFrom("Things/Terrain/Substructure/SubscaffoldingTile", ShaderDatabase.Cutout);
        public static readonly Material MaskOverlayMaterial = SolidColorMaterials.NewSolidColorMaterial(new Color(1f, 1f, 0f), ShaderDatabase.TransparentPostLight);
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
    }
}
