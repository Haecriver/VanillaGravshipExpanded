using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch]
    public static class GenConstruct_CanPlaceBlueprintAt_Patch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(GenConstruct).DeclaredMethod(nameof(GenConstruct.CanPlaceBlueprintAt_NewTemp))
                   ?? typeof(GenConstruct).DeclaredMethod(nameof(GenConstruct.CanPlaceBlueprintAt));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo foundationAtMethod = AccessTools.Method(
                typeof(TerrainGrid),
                nameof(TerrainGrid.FoundationAt),
                new Type[] { typeof(IntVec3) }
            );

            var code = new List<CodeInstruction>(instructions);
            bool patched = false;

            for (int i = 0; i < code.Count; i++)
            {
                var currentInstruction = code[i];
                yield return currentInstruction;
                if (!patched && code[i].opcode == OpCodes.Brfalse_S && code[i - 1].Calls(foundationAtMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(GenConstruct_CanPlaceBlueprintAt_Patch), nameof(ShouldSkip)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, code[i].operand);
                    patched = true;
                }
            }

            if (!patched)
            {
                Log.Warning("[VGE] GenConstruct.CanPlaceBlueprintAt transpiler failed to find its patch point.");
            }
        }
        private static bool ShouldSkip(IntVec3 cell, Map map, TerrainDef terrainDef)
        {
            if (map.terrainGrid.FoundationAt(cell) == VGEDefOf.VGE_DamagedSubstructure)
            {
                return true;
            }
            if (terrainDef.HasTag("VGE_Subarmor"))
            {
                return true;
            }
            return false;
        }
    }
}
