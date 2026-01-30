using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public static class GravdataUtility
    {
        public static float CalculateYieldMultiplier(Building_GravEngine engine)
        {
            float yieldMultiplier = 1f;
            foreach (var facility in engine.GravshipComponents)
            {
                var comp = facility.parent.GetComp<CompGravdataYield>();
                if (comp != null)
                {
                    yieldMultiplier *= comp.Multiplier;
                    yieldMultiplier += comp.Offset;
                }
            }
            return yieldMultiplier;
        }

        public static int CalculateGravdataYield(float distanceTravelled, float gravshipResearchStat, float launchRitualQuality, float gravdataYieldMultiplier)
        {
            float gravdataYield = ((distanceTravelled * gravshipResearchStat) * launchRitualQuality) * gravdataYieldMultiplier;
            return (int)Math.Ceiling(gravdataYield);
        }

        public static int CalculateGravdataYield(float distanceTravelled, float launchRitualQuality, Building_GravEngine engine, Pawn researcherPawn)
        {
            float gravshipResearchStat = 0f;
            if (researcherPawn != null)
            {
                gravshipResearchStat = researcherPawn.GetStatValue(VGEDefOf.VGE_GravshipResearch);
            }
            float yieldMultiplier = CalculateYieldMultiplier(engine);
            var gravdataYield = CalculateGravdataYield(distanceTravelled, gravshipResearchStat, launchRitualQuality, yieldMultiplier);
            return gravdataYield;
        }

        public static Pawn GetResearcher(RitualRoleAssignments assignments)
        {
            var role = GetResearcherRole(assignments);
            if (role == null)
                return null;

            return assignments.AssignedPawns(role).FirstOrDefault();
        }

        public static RitualRole GetResearcherRole(RitualRoleAssignments assignments)
        {
            // Just for safety
            if (assignments == null)
                return null;

            // If the ritual type allows for reasearcher, use that role.
            // If the ritual doesn't allow for it (like gravjump), use pilot role instead.
            return assignments.GetRole("gravtechResearcher") ?? assignments.GetRole("pilot");
        }
    }
}
