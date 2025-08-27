using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(SectionLayer_Terrain), "Regenerate")]
    public static class SectionLayer_Terrain_Regenerate_Patch
    {
        public static void Prefix()
        {
            VGEDefOf.VGE_GravshipSubscaffold.dontRender = false;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var finalizeMeshMethod = AccessTools.Method(typeof(MapDrawLayer), nameof(MapDrawLayer.FinalizeMesh));
            var drawMethod = AccessTools.Method(typeof(SectionLayer_Terrain_Regenerate_Patch), nameof(DrawTopGridUnderScaffold));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(finalizeMeshMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, drawMethod);
                }
                yield return instruction;
            }
        }

        private static void DrawTopGridUnderScaffold(SectionLayer_Terrain instance)
        {
            if (WorldComponent_GravshipController.CutsceneInProgress ||
                WorldComponent_GravshipController.GravshipRenderInProgess)
            {
                return;
            }

            var ColorWhite = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            var section = instance.section;
            var map = instance.Map;
            var terrainGrid = map.terrainGrid;

            foreach (IntVec3 cell in section.CellRect)
            {
                TerrainDef foundationDef = terrainGrid.FoundationAt(cell);
                if (foundationDef == VGEDefOf.VGE_GravshipSubscaffold)
                {
                    TerrainDef floorDef = terrainGrid.TopTerrainAt(cell);
                    if (floorDef == null || VGEDefOf.VGE_GravshipSubscaffold == floorDef)
                    {
                        continue;
                    }
                    CellTerrain floorCellTerrain = new CellTerrain(floorDef, cell.IsPolluted(map), map.snowGrid.GetDepth(cell), cell.GetSandDepth(map), terrainGrid.ColorAt(cell));
                    Material material = instance.GetMaterialFor(floorCellTerrain);
                    LayerSubMesh floorSubMesh = instance.GetSubMesh(floorDef.dontRender ? MatBases.ShadowMask : material);
                    float floorY = AltitudeLayer.Terrain.AltitudeFor() - 0.001f;

                    if (floorSubMesh != null && instance.AllowRenderingFor(floorCellTerrain.def))
                    {
                        int count = floorSubMesh.verts.Count;
                        floorSubMesh.verts.Add(new Vector3(cell.x, floorY, cell.z));
                        floorSubMesh.verts.Add(new Vector3(cell.x, floorY, cell.z + 1));
                        floorSubMesh.verts.Add(new Vector3(cell.x + 1, floorY, cell.z + 1));
                        floorSubMesh.verts.Add(new Vector3(cell.x + 1, floorY, cell.z));
                        floorSubMesh.colors.Add(ColorWhite);
                        floorSubMesh.colors.Add(ColorWhite);
                        floorSubMesh.colors.Add(ColorWhite);
                        floorSubMesh.colors.Add(ColorWhite);
                        floorSubMesh.tris.Add(count);
                        floorSubMesh.tris.Add(count + 1);
                        floorSubMesh.tris.Add(count + 2);
                        floorSubMesh.tris.Add(count);
                        floorSubMesh.tris.Add(count + 2);
                        floorSubMesh.tris.Add(count + 3);
                    }
                }
            }
        }

        public static void Postfix()
        {
            VGEDefOf.VGE_GravshipSubscaffold.dontRender = true;
        }
    }
}
