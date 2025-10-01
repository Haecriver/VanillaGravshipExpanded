using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.SpawnSetup))]
public static class Building_GravEngine_SpawnSetup_Patch
{
    private static void Postfix() => CleanCachedGravEngine();

    public static void CleanCachedGravEngine()
    {
        // Set the last cached tick to -1, forcing the method to recalculate if the player has an engine or not.
        // Basically, if a grav engine is spawned/despawned (possibly through dev mode) make sure it forces an update
        // to the cached grav engine. This prevents a situation where we could dev-mode spawn multiple engines,
        // as well as ensuring that after it's destroyed that we can immediately place another blueprint.
        GravshipUtility.lastCachedEngineTick = -1;
        // We could clean the last cached map and grav engine themselves... But there's not really a point to that.
    }
}