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
        
        public static void Postfix(LaunchInfo __instance)
        {
            Pawn researcherPawn = null;
            PlanetTile sourceTile = PlanetTile.Invalid;
            bool isLiftLaunch = false;
            
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                gravtechResearcherPawns.TryGetValue(__instance, out researcherPawn);
                launchSourceTiles.TryGetValue(__instance, out sourceTile);
                isGravliftLaunch.TryGetValue(__instance, out isLiftLaunch);
            }
            
            Scribe_References.Look(ref researcherPawn, "gravtechResearcherPawn");
            Scribe_Values.Look(ref sourceTile, "launchSourceTile", -1);
            Scribe_Values.Look(ref isLiftLaunch, "isGravliftLaunch", false);
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                gravtechResearcherPawns[__instance] = researcherPawn;
                launchSourceTiles[__instance] = sourceTile;
                isGravliftLaunch[__instance] = isLiftLaunch;
            }
        }
    }
}
