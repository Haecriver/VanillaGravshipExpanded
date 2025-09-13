using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Scenario), "PostMapGenerate")]
    public static class Scenario_PostMapGenerate_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Prefix()
        {
            TerrainDefOf.Space.passability = Traversability.Impassable;
            TerrainDefOf.Space.affordances.Remove(TerrainAffordanceDefOf.Walkable);
        }

        [HarmonyPriority(int.MaxValue)]
        public static void Postfix()
        {
            TerrainDefOf.Space.passability = Traversability.Standable;
            if (TerrainDefOf.Space.affordances.Contains(TerrainAffordanceDefOf.Walkable) is false)
            {
                TerrainDefOf.Space.affordances.Add(TerrainAffordanceDefOf.Walkable);
            }
        }
    }
}
