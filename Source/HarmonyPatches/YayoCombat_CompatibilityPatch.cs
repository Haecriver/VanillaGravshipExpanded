using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch]
    public static class YayoCombat_CompatibilityPatch
    {
        private static MethodBase yayoPrefix;
        public static bool Prepare()
        {
            if (!ModLister.AnyModActiveNoSuffix(["Mlie.YayosCombat3"])) return false;
            var yayoPatch = AccessTools.TypeByName("yayoCombat.HarmonyPatches.Verb_LaunchProjectile_TryCastShot");
            if (yayoPatch == null)
            {
                Log.Error("[VGE] Yayo's Combat 3 patch type not found.");
                return false;
            }
            yayoPrefix = AccessTools.Method(yayoPatch, "Prefix");
            if (yayoPrefix == null)
            {
                Log.Error("[VGE] Yayo's Combat 3 Prefix method not found.");
                return false;
            }
            return true;
        }
        
        public static MethodBase TargetMethod()
        {
            return yayoPrefix;
        }
        
        public static bool Prefix(ref bool __result, Verb_LaunchProjectile __1)
        {
            if (__1 is Verb_ShootWithWorldTargeting)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
