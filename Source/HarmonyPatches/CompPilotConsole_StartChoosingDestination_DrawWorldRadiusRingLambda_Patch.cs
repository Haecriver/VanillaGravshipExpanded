using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class CompPilotConsole_StartChoosingDestination_DrawWorldRadiusRingLambda_Patch
{
    private static readonly WorldRadiusCache MaxRadiusCache = new();
    private static readonly WorldRadiusCache CurrentRadiusCache = new();

    private static bool Prepare(MethodBase method)
    {
        if (method != null)
            return true;
        if (TargetMethod() != null)
            return true;

        Log.Error("[VGE] Error patching world radius ring - could not find one of the lambdas to CompPilotConsole:StartChoosingDestination.");
        return false;
    }

    private static MethodBase TargetMethod() => typeof(CompPilotConsole).FindIncludingInnerTypes<MethodBase>(t => t.FirstMethod(m => m.Name == $"<{nameof(CompPilotConsole.StartChoosingDestination_NewTemp)}>b__3"));

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var targetMethod = SymbolExtensions.GetMethodInfo(() => GenDraw.DrawWorldRadiusRing);
        var patched = 0;

        // A bit of a destructive approach - replace the built-in method with our own wrappers.
        // We could wrap the original method with a method before and after - but that would require extra work.
        // Honestly, makes you wish that Harmony had the infixes implemented, as that would be the perfect place to use those.
        foreach (var ci in instr)
        {
            if (ci.Calls(targetMethod))
            {
                switch (patched)
                {
                    case 0:
                        ci.opcode = OpCodes.Call;
                        ci.operand = SymbolExtensions.GetMethodInfo(() => DrawWorldRadiusRingMax);
                        break;
                    case 1:
                        ci.opcode = OpCodes.Call;
                        ci.operand = SymbolExtensions.GetMethodInfo(() => DrawWorldRadiusRingCurrent);
                        break;
                }

                patched++;
            }

            yield return ci;
        }

        const int expectedPatches = 2;
        if (patched != expectedPatches)
            Log.Error($"[VGE] Patching CompPilotConsole:StartChoosingDestination lambda - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {patched}.");
    }

    private static void DrawWorldRadiusRingMax(PlanetTile center, int radius, Material overrideMat) => MaxRadiusCache.Draw(center, radius, overrideMat);

    private static void DrawWorldRadiusRingCurrent(PlanetTile center, int radius, Material overrideMat) => CurrentRadiusCache.Draw(center, radius, overrideMat);

    private class WorldRadiusCache
    {
        private PlanetTile cachedEdgeTilesForCenter = PlanetTile.Invalid;
        private int cachedEdgeTilesForWorldSeed = -1;
        private int cachedEdgeTilesForRadius = -1;
        private readonly List<PlanetTile> cachedEdgeTilesSorted = [];
        private readonly HashSet<PlanetTile> cachedEdgeTiles = [];

        public void Draw(PlanetTile center, int radius, Material overrideMat)
        {
            var valid = cachedEdgeTilesForCenter == center && cachedEdgeTilesForRadius == radius && cachedEdgeTilesForWorldSeed == Find.World.info.Seed;

            // If our current cache is valid, then replace the current GenDraw cache
            if (valid)
            {
                GenDraw.cachedEdgeTilesForCenter = cachedEdgeTilesForCenter;
                GenDraw.cachedEdgeTilesForWorldSeed = cachedEdgeTilesForWorldSeed;
                GenDraw.cachedEdgeTilesForRadius = cachedEdgeTilesForRadius;

                GenDraw.cachedEdgeTilesSorted.Clear();
                GenDraw.cachedEdgeTilesSorted.AddRange(cachedEdgeTilesSorted);

                GenDraw.cachedEdgeTiles.Clear();
                GenDraw.cachedEdgeTiles.AddRange(cachedEdgeTiles);
            }

            // The original call method
            GenDraw.DrawWorldRadiusRing(center, radius, overrideMat);

            // If our cache is not valid, copy the current GenDraw cache
            if (!valid)
            {
                cachedEdgeTilesForCenter = GenDraw.cachedEdgeTilesForCenter;
                cachedEdgeTilesForWorldSeed = GenDraw.cachedEdgeTilesForWorldSeed;
                cachedEdgeTilesForRadius = GenDraw.cachedEdgeTilesForRadius;

                cachedEdgeTilesSorted.Clear();
                cachedEdgeTilesSorted.AddRange(GenDraw.cachedEdgeTilesSorted);

                cachedEdgeTiles.Clear();
                cachedEdgeTiles.AddRange(GenDraw.cachedEdgeTiles);
            }
        }
    }
}