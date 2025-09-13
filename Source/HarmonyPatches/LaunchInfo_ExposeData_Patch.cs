using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(LaunchInfo), nameof(LaunchInfo.ExposeData))]
    public static class LaunchInfo_ExposeData_Patch
    {
        public static Dictionary<LaunchInfo, Pawn> gravtechResearcherPawns = new Dictionary<LaunchInfo, Pawn>();
        public static Dictionary<LaunchInfo, PlanetTile> launchSourceTiles = new Dictionary<LaunchInfo, PlanetTile>();
        public static Dictionary<LaunchInfo, bool> isGravliftLaunch = new Dictionary<LaunchInfo, bool>();
        public static Dictionary<LaunchInfo, float> lastCost = new Dictionary<LaunchInfo, float>();
        public static Dictionary<LaunchInfo, FuelSpentData> fuelSpentPerTank = 
            new Dictionary<LaunchInfo, FuelSpentData>();
        
        public static void Postfix(LaunchInfo __instance)
        {
            Pawn researcherPawn = null;
            PlanetTile sourceTile = PlanetTile.Invalid;
            bool isLiftLaunch = false;
            float cost = 0f;
            FuelSpentData fuelSpent = null;
            
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                gravtechResearcherPawns.TryGetValue(__instance, out researcherPawn);
                launchSourceTiles.TryGetValue(__instance, out sourceTile);
                isGravliftLaunch.TryGetValue(__instance, out isLiftLaunch);
                lastCost.TryGetValue(__instance, out cost);
                fuelSpentPerTank.TryGetValue(__instance, out fuelSpent);
            }
            
            Scribe_References.Look(ref researcherPawn, "gravtechResearcherPawn");
            Scribe_Values.Look(ref sourceTile, "launchSourceTile", -1);
            Scribe_Values.Look(ref isLiftLaunch, "isGravliftLaunch", false);
            Scribe_Values.Look(ref cost, "lastCost", 0f);
            Scribe_Deep.Look(ref fuelSpent, "fuelSpentPerTank");
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                gravtechResearcherPawns[__instance] = researcherPawn;
                launchSourceTiles[__instance] = sourceTile;
                isGravliftLaunch[__instance] = isLiftLaunch;
                lastCost[__instance] = cost;
                fuelSpentPerTank[__instance] = fuelSpent ?? new FuelSpentData();
            }
        }
    }
}
