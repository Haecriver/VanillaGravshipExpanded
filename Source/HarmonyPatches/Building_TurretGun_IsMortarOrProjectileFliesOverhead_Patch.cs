using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Building_TurretGun), "IsMortarOrProjectileFliesOverhead", MethodType.Getter)]
    public static class Building_TurretGun_IsMortarOrProjectileFliesOverhead_Patch
    {
        public static bool Prefix(Building_TurretGun __instance)
        {
            if (__instance is Building_GravshipTurret)
            {
                return false;
            }
            return true;
        }
    }
}
