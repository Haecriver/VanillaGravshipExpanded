using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(AvoidGrid), nameof(AvoidGrid.PrintAvoidGridAroundTurret))]
    public static class AvoidGrid_PrintAvoidGridAroundTurret_Patch
    {
        public static bool Prefix(Building_TurretGun tur)
        {
            if (tur is Building_GravshipTurret)
            {
                return false;
            }
            return true;
        }
    }
}
