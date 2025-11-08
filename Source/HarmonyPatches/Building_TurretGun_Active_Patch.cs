using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(Building_TurretGun), "Active", MethodType.Getter)]
    public static class Building_TurretGun_Active_Patch
    {
        public static bool Prefix(Building_TurretGun __instance, ref bool __result)
        {
            if (__instance is Building_GravshipTurret turret && turret.CanFire is false)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
