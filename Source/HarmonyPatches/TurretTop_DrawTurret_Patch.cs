using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(TurretTop), "DrawTurret")]
    public static class TurretTop_DrawTurret_Patch
    {
        public static bool Prefix(TurretTop __instance, Vector3 drawLoc, Vector3 recoilDrawOffset, float recoilAngleOffset)
        {
            if (__instance.parentTurret is Building_GravshipTurret || __instance.parentTurret.def == VGEDefOf.VGE_PointDefenseTurret)
            {
                Building_Turret parentTurret = __instance.parentTurret;
                Vector3 v = new Vector3(parentTurret.def.building.turretTopOffset.x, 0f, parentTurret.def.building.turretTopOffset.y).RotatedBy(__instance.CurRotation);
                Vector3 pos = drawLoc + Altitudes.AltIncVect + v;
                if (WorldComponent_GravshipController.CutsceneInProgress is false)
                {
                    pos.y += 6.19f;
                }
                Map map = __instance.parentTurret.Map;
                if (map != null)
                {
                    float turretTopDrawSize = parentTurret.def.building.turretTopDrawSize;
                    v = v.RotatedBy(recoilAngleOffset);
                    v += recoilDrawOffset;
                    float num = parentTurret.CurrentEffectiveVerb?.AimAngleOverride ?? __instance.CurRotation;
                    Quaternion q = ((float)TurretTop.ArtworkRotation + num).ToQuat();
                    Matrix4x4 matrix = Matrix4x4.TRS(pos, q, new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    Color skyColor = map.skyManager.CurSky.colors.sky;
                    skyColor.a = 1f;
                    block.SetColor("_Color", skyColor);
                    Graphics.DrawMesh(MeshPool.plane10, matrix, parentTurret.TurretTopMaterial, 0, null, 0, block);
                    return false;
                }
            }
            return true;
        }
    }
}
