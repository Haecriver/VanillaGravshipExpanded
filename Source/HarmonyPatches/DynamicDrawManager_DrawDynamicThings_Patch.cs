using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(DynamicDrawManager), nameof(DynamicDrawManager.DrawDynamicThings))]
    public static class DynamicDrawManager_DrawDynamicThings_Patch
    {
        public static void Postfix(DynamicDrawManager __instance)
        {
            foreach (var thing in __instance.DrawThings)
            {
                if (thing is Building_TurretGun gun && (gun is Building_GravshipTurret || gun.def.IsPointDefenseTurret()))
                {
                    if (gun.Map.fogGrid.IsFogged(gun.Position))
                    {
                        gun.Top.DrawTurret(gun.DrawPos, Vector3.zero, 0f);
                    }
                }
            }
        }
    }
}
