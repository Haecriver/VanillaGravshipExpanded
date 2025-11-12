using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "GetForcedMissTarget")]
    public static class Verb_LaunchProjectile_GetForcedMissTarget_Patch
    {
        public static void Prefix(Verb_LaunchProjectile __instance, ref float forcedMissRadius)
        {
            var turret = __instance.caster as Building_GravshipTurret;
            if (turret is not null)
            {
                forcedMissRadius /= turret.GravshipTargeting;
            }
        }
    }
}
