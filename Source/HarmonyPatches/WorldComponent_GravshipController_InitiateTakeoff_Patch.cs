using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "InitiateTakeoff")]
    public static class WorldComponent_GravshipController_InitiateTakeoff_Patch
    {
        public static void Postfix(WorldComponent_GravshipController __instance)
        {
            __instance.mapHasGravAnchor = true;
            foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
            {
                pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDef(VGEDefOf.VGE_CrewEuphoria);
            }
        }
    }
}
