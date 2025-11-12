using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;
using System;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    [HotSwappable]
    public static class ResearchTree_Compat_Patches
    {
        private static FieldInfo _researchOriginalField;

        static ResearchTree_Compat_Patches()
        {
            var harmony = new Harmony("VanillaGravshipExpanded.ResearchTreeCompat");
            if (ModsConfig.IsActive("Owlchemist.ResearchPowl"))
            {
                ApplyPowlPatches(harmony);
            }
            if (ModsConfig.IsActive("Mlie.ResearchTree"))
            {
                ApplyMliePatches(harmony);
            }
        }

        private static void ApplyPowlPatches(Harmony harmony)
        {
            var powlMenuType = AccessTools.TypeByName("ResearchPowl.MainTabWindow_ResearchTree");
            if (powlMenuType != null)
            {
                var powlMenuTargetMethod = AccessTools.Method(powlMenuType, "MenuOfPacks");
                if (powlMenuTargetMethod != null)
                {
                    var powlAssetsType = AccessTools.TypeByName("ResearchPowl.Assets+MainButtonDefOf");
                    if (powlAssetsType != null)
                    {
                        _researchOriginalField = AccessTools.Field(powlAssetsType, "ResearchOriginal");
                        if (_researchOriginalField != null)
                        {
                            var postfix = new HarmonyMethod(typeof(ResearchTree_Compat_Patches), nameof(MenuOfPacks_Postfix));
                            harmony.Patch(powlMenuTargetMethod, postfix: postfix);
                        }
                        else Log.Error("[VGE] ResearchPowl.Assets+MainButtonDefOf.ResearchOriginal field is not found.");
                    }
                    else Log.Error("[VGE] ResearchPowl.Assets+MainButtonDefOf is not found.");
                }
                else Log.Error("[VGE] ResearchPowl.MainTabWindow_ResearchTree.MenuOfPacks method is not found.");
            }
            else Log.Error("[VGE] ResearchPowl.MainTabWindow_ResearchTree is not found.");

            var powlTreeType = AccessTools.TypeByName("ResearchPowl.Tree");
            if (powlTreeType != null)
            {
                var powlTargetMethod = AccessTools.Method(powlTreeType, "InitializeNodesStructures");
                if (powlTargetMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(ResearchTree_Compat_Patches), nameof(Tree_Generation_Prefix));
                    var postfix = new HarmonyMethod(typeof(ResearchTree_Compat_Patches), nameof(Tree_Generation_Postfix));
                    harmony.Patch(powlTargetMethod, prefix: prefix, postfix: postfix);
                }
                else Log.Error("[VGE] ResearchPowl.Tree.InitializeNodesStructures method is not found.");
            }
            else Log.Error("[VGE] ResearchPowl.Tree type is not found.");
        }

        private static void ApplyMliePatches(Harmony harmony)
        {
            var fluffyTreeType = AccessTools.TypeByName("FluffyResearchTree.Tree");
            if (fluffyTreeType != null)
            {
                var fluffyTargetMethod = AccessTools.Method(fluffyTreeType, "populateNodes");
                if (fluffyTargetMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(ResearchTree_Compat_Patches), nameof(Tree_Generation_Prefix));
                    var postfix = new HarmonyMethod(typeof(ResearchTree_Compat_Patches), nameof(Tree_Generation_Postfix));
                    harmony.Patch(fluffyTargetMethod, prefix: prefix, postfix: postfix);
                }
                else Log.Error("[VGE] FluffyResearchTree.Tree.populateNodes method is not found.");
            }
            else Log.Error("[VGE] FluffyResearchTree.Tree type is not found.");

            var fluffyWindowType = AccessTools.TypeByName("FluffyResearchTree.MainTabWindow_ResearchTree");
            if (fluffyWindowType != null)
            {
                var fluffySearchBarMethod = AccessTools.Method(fluffyWindowType, "DrawSearchBar");
                if (fluffySearchBarMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(ResearchTree_Compat_Patches), nameof(Fluffy_DrawSearchBar_Postfix));
                    harmony.Patch(fluffySearchBarMethod, postfix: postfix);
                }
                else Log.Error("[VGE] FluffyResearchTree.MainTabWindow_ResearchTree.DrawSearchBar method is not found.");
            }
            else Log.Error("[VGE] FluffyResearchTree.MainTabWindow_ResearchTree type is not found.");

            var fluffyToggleTabPrefixMethod = AccessTools.Method(AccessTools.TypeByName("FluffyResearchTree.MainTabsRoot_ToggleTab"), "Prefix");
            if (fluffyToggleTabPrefixMethod != null)
            {
                var ourEarlyPrefix = new HarmonyMethod(typeof(ResearchTree_Compat_Patches), nameof(Fluffy_ToggleTab_EarlyPrefix));
                harmony.Patch(fluffyToggleTabPrefixMethod, prefix: ourEarlyPrefix);
            }
            else Log.Error("[VGE] FluffyResearchTree.MainTabsRoot_ToggleTab.Prefix method is not found.");
        }

        public static void MenuOfPacks_Postfix(ref List<FloatMenuOption> __result)
        {
            if (__result.Any(opt => opt.labelInt == VGEDefOf.VGE_Gravtech.LabelCap)) return;

            var vgeMod = LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId.Equals("vanillaexpanded.gravship", StringComparison.OrdinalIgnoreCase));
            if (vgeMod != null)
            {
                __result.RemoveAll(option => option.labelInt == vgeMod.Name);
            }

            var researchOriginalDef = _researchOriginalField.GetValue(null) as MainButtonDef;
            if (researchOriginalDef == null) return;

            var gravtechOption = new FloatMenuOption(VGEDefOf.VGE_Gravtech.LabelCap, () =>
            {
                var vanillaResearchWindow = (MainTabWindow_Research)researchOriginalDef.TabWindow;
                Find.MainTabsRoot.ToggleTab(researchOriginalDef);
                vanillaResearchWindow.CurTab = VGEDefOf.VGE_Gravtech;
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);

            int insertIndex = __result.FindIndex(opt => opt.labelInt == "Anomaly");
            insertIndex = (insertIndex != -1) ? insertIndex + 1 : 1;

            if (__result.Count > insertIndex) __result.Insert(insertIndex, gravtechOption);
            else __result.Add(gravtechOption);
        }

        public static void Fluffy_DrawSearchBar_Postfix(Rect canvas)
        {
            Rect buttonArea = canvas.BottomHalf();
            bool anomalyActive = ModsConfig.AnomalyActive;

            float buttonWidth = buttonArea.width;
            float buttonX = buttonArea.x;

            if (anomalyActive)
            {
                buttonX += buttonWidth + 4f;
            }

            var buttonRect = new Rect(buttonX, 0f, buttonWidth, 30f);
            var finalRect = GenUI.CenteredOnYIn(buttonRect, buttonArea);

            if (Widgets.ButtonText(finalRect, VGEDefOf.VGE_Gravtech.LabelCap))
            {
                ((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).CurTab = VGEDefOf.VGE_Gravtech;
                Find.MainTabsRoot.ToggleTab(MainButtonDefOf.Research);
            }
        }

        public static bool Fluffy_ToggleTab_EarlyPrefix()
        {
            var vanillaWindow = (MainTabWindow_Research)MainButtonDefOf.Research.TabWindow;
            if (vanillaWindow != null && vanillaWindow.CurTab == VGEDefOf.VGE_Gravtech)
            {
                return false;
            }
            return true;
        }

        public static void Tree_Generation_Prefix(out List<ResearchProjectDef> __state)
        {
            __state = new List<ResearchProjectDef>();
            var allDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;

            for (int i = allDefs.Count - 1; i >= 0; i--)
            {
                var def = allDefs[i];
                if (def.tab == VGEDefOf.VGE_Gravtech)
                {
                    __state.Add(def);
                    DefDatabase<ResearchProjectDef>.Remove(def);
                }
            }
        }

        public static void Tree_Generation_Postfix(List<ResearchProjectDef> __state)
        {
            if (__state == null) return;
            foreach (var def in __state)
            {
                DefDatabase<ResearchProjectDef>.Add(def);
            }
        }
    }
}
