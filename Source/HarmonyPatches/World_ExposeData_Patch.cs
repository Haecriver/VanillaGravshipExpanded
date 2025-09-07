using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(World), nameof(World.ExposeData))]
    public static class World_ExposeData_Patch
    {
        public static ResearchProjectDef currentGravtechProject;
        public static void Reset()
        {
            currentGravtechProject = null;
        }
        
        public static void Postfix()
        {
            Scribe_Defs.Look(ref currentGravtechProject, "currentGravtechProject");
        }
    }
}
