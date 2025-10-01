using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.DeSpawn))]
public static class Building_GravEngine_DeSpawn_Patch
{
    private static void Postfix() => Building_GravEngine_SpawnSetup_Patch.CleanCachedGravEngine();
}