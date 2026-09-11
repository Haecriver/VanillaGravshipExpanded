using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static RimWorld.SectionLayer_SubstructureProps;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(SectionLayer_SubstructureProps), "Regenerate")]
    public static class SectionLayer_SubstructureProps_Regenerate_Patch
    {
        private static readonly CachedMaterial CustomBottom = new CachedMaterial("Things/Terrain/Substructure/Subscaffolding/SubscaffoldingProps_Loops", ShaderDatabase.Transparent);
        private static readonly CachedMaterial CustomBottomMech = new CachedMaterial("Things/Terrain/Substructure/MechanoidSubstructure/MechanoidSubstructureProps_Loops", ShaderDatabase.Transparent);
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var finalizeMeshMethod = AccessTools.Method(typeof(MapDrawLayer), nameof(MapDrawLayer.FinalizeMesh));
            var drawMethod = AccessTools.Method(typeof(SectionLayer_SubstructureProps_Regenerate_Patch), nameof(DrawCustomBottomForGravshipSubscaffold));

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

        public enum EdgeType
        {
            OShape,
            UShape,
            CornerInner,
            CornerOuter,
            Flat,
            LoopLeft,
            LoopRight,
            LoopSingle,
            Loop
        }

        private static readonly Vector2[] UVs = new Vector2[4]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        private static readonly Dictionary<EdgeDirections, (EdgeType, Rot4)[]> EdgeMats_Terrain = new Dictionary<EdgeDirections, (EdgeType, Rot4)[]>
        {
            {
                EdgeDirections.North,
                new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.South) }
            },
            {
                EdgeDirections.East,
                new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.West) }
            },
            {
                EdgeDirections.South,
                new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.North) }
            },
            {
                EdgeDirections.West,
                new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.East) }
            },
            {
                EdgeDirections.North | EdgeDirections.East,
                new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.West) }
            },
            {
                EdgeDirections.East | EdgeDirections.South,
                new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.North) }
            },
            {
                EdgeDirections.South | EdgeDirections.West,
                new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.East) }
            },
            {
                EdgeDirections.North | EdgeDirections.West,
                new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.South) }
            },
            {
                EdgeDirections.North | EdgeDirections.South,
                new(EdgeType, Rot4)[2]
                {
                    (EdgeType.Flat, Rot4.South),
                    (EdgeType.Flat, Rot4.North)
                }
            },
            {
                EdgeDirections.East | EdgeDirections.West,
                new(EdgeType, Rot4)[2]
                {
                    (EdgeType.Flat, Rot4.West),
                    (EdgeType.Flat, Rot4.East)
                }
            },
            {
                EdgeDirections.North | EdgeDirections.East | EdgeDirections.South,
                new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.West) }
            },
            {
                EdgeDirections.East | EdgeDirections.South | EdgeDirections.West,
                new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.North) }
            },
            {
                EdgeDirections.North | EdgeDirections.South | EdgeDirections.West,
                new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.East) }
            },
            {
                EdgeDirections.North | EdgeDirections.East | EdgeDirections.West,
                new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.South) }
            },
            {
                EdgeDirections.North | EdgeDirections.East | EdgeDirections.South | EdgeDirections.West,
                new(EdgeType, Rot4)[1] { (EdgeType.OShape, Rot4.North) }
            }
        };

        private static void DrawCustomBottomForGravshipSubscaffold(SectionLayer_SubstructureProps instance)
        {
            var section = instance.section;
            var map = instance.Map;
            var terrainGrid = map.terrainGrid;
            var cellRect = section.CellRect;
            float altitude = AltitudeLayer.TerrainScatter.AltitudeFor();

            foreach (IntVec3 item in cellRect)
            {
                TerrainDef foundationDef = terrainGrid.FoundationAt(item);
                TerrainDef topDef = terrainGrid.TerrainAt(item);
                if (foundationDef == VGEDefOf.VGE_GravshipSubscaffold)
                {
                    LayerSubMesh subMesh = instance.GetSubMesh(CustomBottom.Material);
                    SectionLayer_SubstructureProps_ShouldDrawPropsOn_Patch.doVanilla = true;
                    if (instance.ShouldDrawPropsOn(item, terrainGrid, out var edgeEdgeDirections, out var cornerDirections))
                    {
                        instance.DrawEdges(item, edgeEdgeDirections, altitude);
                        instance.DrawCorners(item, cornerDirections, edgeEdgeDirections, altitude);
                        var bottomCell = item + IntVec3.South;
                        if (bottomCell.InBounds(map))
                        {
                            SectionLayer_GravshipHull.ShouldDrawCornerPiece(bottomCell, map, terrainGrid, out var cornerType, out var _);
                            bool flag = cornerType == SectionLayer_GravshipHull.CornerType.Corner_NW || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NW || cornerType == SectionLayer_GravshipHull.CornerType.Corner_NE || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NE;
                            if (edgeEdgeDirections.HasFlag(EdgeDirections.South) && !flag)
                            {
                                instance.AddQuad(subMesh, item + IntVec3.South, altitude, Rot4.North, SectionLayer_GravshipMask.IsValidSubstructure(item));
                            }
                        }
                    }
                    SectionLayer_SubstructureProps_ShouldDrawPropsOn_Patch.doVanilla = false;
                }

                var useCustomEdgesFoundation = foundationDef != null && foundationDef.GetModExtension<SubstructureEdgeGraphicsExtension>() is { } foundationExt && !foundationExt.renderAsSubstructure;
                var useCustomEdgesTop = topDef != null && topDef.GetModExtension<SubstructureEdgeGraphicsExtension>() is { } topExt && !topExt.renderAsSubstructure;

                TerrainDef terrainToDraw = null;
                if (foundationDef != null && (useCustomEdgesFoundation || foundationDef == VGEDefOf.VGE_MechanoidSubstructure) && foundationDef != VGEDefOf.VGE_GravshipSubscaffold)
                {
                    terrainToDraw = foundationDef;
                }
                else if (topDef != null && (useCustomEdgesTop || topDef == VGEDefOf.VGE_MechanoidSubstructure))
                {
                    terrainToDraw = topDef;
                }

                if (terrainToDraw != null)
                {
                    if (ShouldDrawGenericProps(instance, item, terrainGrid, terrainToDraw, out var edgeEdgeDirections, out var cornerDirections))
                    {
                        TerrainDef edgeTerrain = terrainToDraw == VGEDefOf.VGE_MechanoidSubstructure ? TerrainDefOf.MechanoidPlatform : terrainToDraw;

                        DrawEdges(instance, edgeTerrain, item, edgeEdgeDirections, altitude);
                        DrawCorners(instance, edgeTerrain, item, edgeEdgeDirections, cornerDirections, altitude);

                        Material bottomMat = null;
                        if (terrainToDraw == VGEDefOf.VGE_MechanoidSubstructure)
                        {
                            bottomMat = CustomBottomMech.Material;
                        }
                        else if (edgeTerrain.spaceEdgeGraphicData != null && !edgeTerrain.spaceEdgeGraphicData.LoopTexPaths.NullOrEmpty())
                        {
                            bottomMat = MaterialPool.MatFrom(edgeTerrain.spaceEdgeGraphicData.LoopTexPaths[0], ShaderDatabase.Transparent);
                        }

                        if (bottomMat != null)
                        {
                            LayerSubMesh subMesh = instance.GetSubMesh(bottomMat);
                            var bottomCell = item + IntVec3.South;
                            if (bottomCell.InBounds(map))
                            {
                                SectionLayer_GravshipHull.ShouldDrawCornerPiece(bottomCell, map, terrainGrid, out var cornerType, out var _);
                                bool flag = cornerType == SectionLayer_GravshipHull.CornerType.Corner_NW || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NW || cornerType == SectionLayer_GravshipHull.CornerType.Corner_NE || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NE;
                                if (edgeEdgeDirections.HasFlag(EdgeDirections.South) && !flag)
                                {
                                    instance.AddQuad(subMesh, bottomCell, altitude, Rot4.North, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DrawEdges(SectionLayer_SubstructureProps instance, TerrainDef terrain, IntVec3 c, EdgeDirections edgeDirs, float altitude)
        {
            if (EdgeMats_Terrain.TryGetValue(edgeDirs, out var value))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    var (edgeType, rotation) = value[i];
                    AddQuad(instance, terrain, edgeType, c, altitude, rotation);
                }
            }
        }

        private static void DrawCorners(SectionLayer_SubstructureProps instance, TerrainDef terrain, IntVec3 c, EdgeDirections edges, CornerDirections corners, float altitude)
        {
            if (corners.HasFlag(CornerDirections.NorthWest) && !edges.HasFlag(EdgeDirections.North) && !edges.HasFlag(EdgeDirections.West))
            {
                AddQuad(instance, terrain, EdgeType.CornerInner, c, altitude, Rot4.East);
            }
            if (corners.HasFlag(CornerDirections.NorthEast) && !edges.HasFlag(EdgeDirections.North) && !edges.HasFlag(EdgeDirections.East))
            {
                AddQuad(instance, terrain, EdgeType.CornerInner, c, altitude, Rot4.South);
            }
            if (corners.HasFlag(CornerDirections.SouthEast) && !edges.HasFlag(EdgeDirections.South) && !edges.HasFlag(EdgeDirections.East))
            {
                AddQuad(instance, terrain, EdgeType.CornerInner, c, altitude, Rot4.West);
            }
            if (corners.HasFlag(CornerDirections.SouthWest) && !edges.HasFlag(EdgeDirections.South) && !edges.HasFlag(EdgeDirections.West))
            {
                AddQuad(instance, terrain, EdgeType.CornerInner, c, altitude, Rot4.North);
            }
        }

        private static void AddQuad(SectionLayer_SubstructureProps instance, TerrainDef terrain, EdgeType edgeType, IntVec3 c, float altitude, Rot4 rotation, int listIndexOffset = 0)
        {
            if (terrain?.spaceEdgeGraphicData == null) return;
            Material material = terrain.spaceEdgeGraphicData.GetMaterial(terrain, (RimWorld.SectionLayer_TerrainEdges.EdgeType)edgeType, listIndexOffset);
            if (material == null) return;
            LayerSubMesh subMesh = instance.GetSubMesh(material);
            int count = subMesh.verts.Count;
            float num = Mathf.Max((float)material.mainTexture.width / (float)material.mainTexture.height, 1f);
            float num2 = Mathf.Max((float)material.mainTexture.height / (float)material.mainTexture.width, 1f);
            int num3 = Mathf.Abs(4 - rotation.AsInt);
            for (int i = 0; i < 4; i++)
            {
                subMesh.verts.Add(new Vector3((float)c.x + UVs[i].x * num, altitude, (float)c.z + UVs[i].y * num2));
                subMesh.uvs.Add(UVs[(num3 + i) % 4]);
            }
            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 1);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count + 3);
        }

        private static bool ShouldDrawGenericProps(SectionLayer_SubstructureProps instance, IntVec3 c, TerrainGrid terrGrid, TerrainDef targetDef, out EdgeDirections edgeEdgeDirections, out CornerDirections cornerDirections)
        {
            edgeEdgeDirections = EdgeDirections.None;
            cornerDirections = CornerDirections.None;
            TerrainDef terrainDef = terrGrid.FoundationAt(c);
            if (terrainDef == null && targetDef == VGEDefOf.VGE_MechanoidSubstructure)
                terrainDef = terrGrid.TerrainAt(c);

            if (terrainDef == null || terrainDef != targetDef)
            {
                return false;
            }
            for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
            {
                IntVec3 c2 = c + GenAdj.CardinalDirections[i];
                if (!c2.InBounds(instance.Map))
                {
                    edgeEdgeDirections |= (EdgeDirections)(1 << i);
                    continue;
                }
                TerrainDef terrainDef2 = terrGrid.FoundationAt(c2);
                if (terrainDef2 == null && targetDef == VGEDefOf.VGE_MechanoidSubstructure)
                    terrainDef2 = terrGrid.TerrainAt(c2);

                if (terrainDef2 == null || terrainDef2 != targetDef)
                {
                    edgeEdgeDirections |= (EdgeDirections)(1 << i);
                }
            }
            for (int j = 0; j < GenAdj.DiagonalDirections.Length; j++)
            {
                IntVec3 c3 = c + GenAdj.DiagonalDirections[j];
                if (!c3.InBounds(instance.Map))
                {
                    cornerDirections |= (CornerDirections)(1 << j);
                    continue;
                }
                TerrainDef terrainDef3 = terrGrid.FoundationAt(c3);
                if (terrainDef3 == null && targetDef == VGEDefOf.VGE_MechanoidSubstructure)
                    terrainDef3 = terrGrid.TerrainAt(c3);

                if (terrainDef3 == null || terrainDef3 != targetDef)
                {
                    cornerDirections |= (CornerDirections)(1 << j);
                }
            }
            if (edgeEdgeDirections == EdgeDirections.None)
            {
                return cornerDirections != CornerDirections.None;
            }
            return true;
        }
    }
}
