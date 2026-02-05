using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;
[HotSwappable]
[HarmonyPatch(typeof(Room), nameof(Room.PushHeat))]
public static class Room_PushHeat_Patch
{
    public static void Prefix(Room __instance, ref float energy)
    {
        if (!__instance.Map.Biome.inVacuum || energy <= 0f)
        {
            return;
        }
        energy *= 1f - __instance.Vacuum;
    }
}
