using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(MainTabWindow_Architect), "CacheDesPanels")]
    public static class MainTabWindow_Architect_CacheDesPanels_Patch
    {
        [HarmonyPriority(int.MaxValue)]
        public static bool Prepare()
        {
            return !ModsConfig.IsActive("ferny.BetterArchitect");
        }

        public static void Postfix(MainTabWindow_Architect __instance)
        {
            var visibleCategories = DefDatabase<DesignationCategoryDef>.AllDefsListForReading
                .Where(def => def.GetModExtension<NestedCategoryExtension>()?.parentCategory == null || def == VGEDefOf.Odyssey)
                .ToList();
            __instance.desPanelsCached.RemoveAll(x => !visibleCategories.Contains(x.def));
        }
    }
}
