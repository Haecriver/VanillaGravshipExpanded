using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System.Reflection;
using System.Reflection.Emit;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(GravshipLandingMarker), "DrawAt")]
    public static class GravshipLandingMarker_DrawAt_Patch
    {
        private static readonly MethodInfo drawDiagonalStripesMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawDiagonalStripes));
        private static readonly MethodInfo drawCustomDiagonalStripesMethod = AccessTools.Method(typeof(GravshipLandingMarker_DrawAt_Patch), nameof(DrawCustomDiagonalStripes));
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var skipNextDrawFieldEdges = true;
            var drawFieldEdgesMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawFieldEdges), new[] { typeof(List<IntVec3>), typeof(int) });
            
            for (int i = 0; i < codes.Count; i++)
            {
                if (skipNextDrawFieldEdges && codes[i].Calls(drawFieldEdgesMethod))
                {
                    skipNextDrawFieldEdges = false;
                    yield return new CodeInstruction(OpCodes.Pop);
                    continue;
                }
                
                if (codes[i].Calls(drawDiagonalStripesMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, drawCustomDiagonalStripesMethod);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        
        private static void DrawCustomDiagonalStripes(List<IntVec3> cells, Color? color, float? altOffset, int renderQueue, GravshipLandingMarker instance)
        {
            if (!instance.Visible)
                return;

            var blockingCells = new List<IntVec3>();
            var nonBlockingCells = new List<IntVec3>();
            
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var blockingThings = GravshipMapGenUtility.GetBlockingThingsInCell(cell, instance.Map);
                if (blockingThings.Any())
                {
                    blockingCells.Add(cell);
                }
                else
                {
                    nonBlockingCells.Add(cell);
                }
            }

            if (nonBlockingCells.Any())
            {
                GenDraw.DrawFieldEdges(nonBlockingCells);
                GenDraw.DrawDiagonalStripes(nonBlockingCells, color, altOffset, renderQueue);
            }

            if (blockingCells.Any())
            {
                GenDraw.DrawFieldEdges(blockingCells, Color.red);
                GenDraw.DrawDiagonalStripes(blockingCells, Designator_MoveGravship.diagonalsColorInvalid, altOffset, renderQueue);
            }
        }
    }
}
