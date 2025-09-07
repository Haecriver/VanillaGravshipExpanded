using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(LordJob_Ritual), "ExposeData")]
    public static class LordJob_Ritual_ExposeData_Patch
    {
        public static Dictionary<LordJob_Ritual, PlanetTile> targetTile = new();
        public static void Postfix(LordJob_Ritual __instance)
        {
            PlanetTile targetTile = PlanetTile.Invalid;
            if (!LordJob_Ritual_ExposeData_Patch.targetTile.TryGetValue(__instance, out targetTile))
            {
                targetTile = PlanetTile.Invalid;
            }
            Scribe_Values.Look(ref targetTile, "targetTile", PlanetTile.Invalid);
            if (targetTile != PlanetTile.Invalid)
            {
                LordJob_Ritual_ExposeData_Patch.targetTile[__instance] = targetTile;
            }
        }
    }
}
