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

        public static int countDownSinceElectricityTickCounter = 0;

        public static float maintenanceThreshold = 0.7f;

        public static int gravcoresPurchasedCount = 0;

        public static string lastGravshipName = null;

        public static void Reset()
        {
            currentGravtechProject = null;
            gravcoresPurchasedCount = 0;
            countDownSinceElectricityTickCounter = 0;
        }

        public static void Postfix()
        {
            Scribe_Defs.Look(ref currentGravtechProject, "currentGravtechProject");
            Scribe_Values.Look(ref countDownSinceElectricityTickCounter, "countDownSinceElectricityTickCounter");
            Scribe_Values.Look(ref maintenanceThreshold, "maintenanceThreshold", 0.7f, false);
            Scribe_Values.Look(ref gravcoresPurchasedCount, "gravcoresPurchasedCount", 0, false);
            Scribe_Values.Look(ref lastGravshipName, "lastGravshipName");
        }
    }
}
