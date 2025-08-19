using HarmonyLib;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.ConsumeFuel))]
public static class Building_GravEngine_ConsumeFuel_Patch
{
    private static void Prefix(Building_GravEngine __instance, ref float __state) => __state = __instance.TotalFuel;

    private static void Postfix(Building_GravEngine __instance, PlanetTile tile, float __state)
    {
        // Grab cached values
        if (!GravshipUtility.TryGetPathFuelCost(__instance.Map.Tile, tile, out var cost, out _))
            return;

        // Divide cost by total fuel (cached before vanilla code started lowering it) to get a ratio of fuel we'll need to set each fuel tank to
        var ratio = cost / __state;
        foreach (var comp in __instance.GravshipComponents)
        {
            if (comp.Props.providesFuel && comp.CanBeActive)
            {
                var storage = comp.parent.GetComp<CompResourceStorage>();
                storage?.DrawResource(storage.AmountStored * ratio);
            }
        }
    }
}