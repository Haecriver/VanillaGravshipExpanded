using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(DeepResourceGrid), nameof(DeepResourceGrid.AnyActiveDeepScannersOnMap))]
public class DeepResourceGrid_AnyActiveDeepScannersOnMap
{
    private static void Postfix(ref bool __result, Map ___map)
    {
        if (__result || !VGEDefOf.GroundPenetratingScanner.IsFinished)
            return;

        // TODO: Consider making this a transpiler so we don't iterate twice over everything?
        foreach (var building in ___map.listerBuildings.allBuildingsColonist)
        {
            var compDeepScanner = building.TryGetComp<CompScannerCluster>();
            if (compDeepScanner != null && compDeepScanner.ShouldShowDeepResourceOverlay())
            {
                __result = true;
                return;
            }
        }
    }
}