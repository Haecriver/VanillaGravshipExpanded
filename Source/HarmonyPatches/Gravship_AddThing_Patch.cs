using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Gravship), nameof(Gravship.AddThing))]
public static class Gravship_AddThing_Patch
{
    private static void Postfix(Gravship __instance, Thing thing)
    {
        if (thing.def == VGEDefOf.VGE_PilotCockpit || thing.def == VGEDefOf.VGE_PilotBridge)
            __instance.pilotConsole = (Building)thing;
    }
}