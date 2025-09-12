using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class StatPart_MaintenanceHub : StatPart
    {
        private float offset = -0.25f;

        public override void TransformValue(StatRequest req, ref float val)
        {

            if (req.HasThing && req.Thing.Map != null && MaintenanceHubsPresent(req.Thing.Map) > 0)
            {
                val += offset * MaintenanceHubsPresent(req.Thing.Map);
            }
        }

        public int MaintenanceHubsPresent(Map map)
        {
            int hubsInMap = map.listerThings.ThingsOfDef(VGEDefOf.VGE_MaintenanceHub).Count;
            return (hubsInMap >= 2) ? 2 : hubsInMap;

        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.Map != null && MaintenanceHubsPresent(req.Thing.Map) > 0)
            {
                return "VGE_StatsReport_MaintenanceHubs".Translate() + (": " + offset * MaintenanceHubsPresent(req.Thing.Map));

            }
            return null;
        }
    }
}