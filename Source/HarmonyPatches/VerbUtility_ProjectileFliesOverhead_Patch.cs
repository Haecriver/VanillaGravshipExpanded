using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(VerbUtility), "ProjectileFliesOverhead")]
    public static class VerbUtility_ProjectileFliesOverhead_Patch
    {
        public static bool Prefix(Verb verb)
        {
            if (Building_TurretGun_TryFindNewTarget_Patch.checking is false && verb.caster is Building_GravshipTurret)
            {
                return false;
            }
            return true;
        }
    }
}
