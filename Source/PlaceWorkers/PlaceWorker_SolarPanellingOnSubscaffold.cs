using Verse;
using RimWorld;

namespace VanillaGravshipExpanded
{
    public class PlaceWorker_OnSubstructureOrSubscaffold : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            TerrainDef terrain = map.terrainGrid.FoundationAt(loc);
            if (terrain?.IsSubstructure == true)
            {
                return AcceptanceReport.WasAccepted;
            }
            return "VGE_RequiresSubstructureOrScaffolding".Translate();
        }
    }
}
