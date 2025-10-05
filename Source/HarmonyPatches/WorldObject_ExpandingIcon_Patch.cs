using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.ExpandingIcon), MethodType.Getter)]
public static class WorldObject_ExpandingIcon_Patch
{
    private static bool Prefix(WorldObject __instance, ref Texture2D __result)
    {
        if (__instance is not Gravship gravship)
            return true;

        __result = GravshipHelper.GetExpandingTextureForEngine(gravship.engine?.def);
        // Allow execution if we didn't replace the texture, stop it otherwise.
        return __result == null;
    }
}