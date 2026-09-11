using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Map), nameof(Map.AnyBuildingBlockingMapRemoval), MethodType.Getter)]
public class Map_AnyBuildingBlockingMapRemoval_Patch
{
    private static void Postfix(Map __instance, ref bool __result)
    {
        // If false, check if there's any of our own engines on the map to prevent the map from being removed.
        if (!__result)
        {
            for (var i = 0; i < GravshipHelper.GravEngineDefs.Length; i++)
            {
                var def = GravshipHelper.GravEngineDefs[i];
                if (def != null && __instance.listerThings.AnyThingWithDef(def))
                {
                    __result = true;
                    break;
                }
            }
        }
    }
}