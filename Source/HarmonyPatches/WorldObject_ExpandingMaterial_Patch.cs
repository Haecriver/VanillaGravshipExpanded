using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.ExpandingMaterial), MethodType.Getter)]
public static class WorldObject_ExpandingMaterial_Patch
{
    private static void Prefix(WorldObject __instance)
    {
        if (__instance is not Gravship gravship || __instance.def.expandingShader == null || __instance.def.expandingMaterial != null)
            return;

        var texture = GravshipHelper.GetExpandingTextureForEngine(gravship.engine?.def);
        if (texture == null)
            return;

        var request = new MaterialRequest
        {
            mainTex = texture,
            shader = __instance.def.expandingShader.Shader,
            color = Color.white,
        };

        __instance.expandingMaterial = MaterialPool.MatFrom(request);
    }
}