using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class StockGenerator_Gravcore : StockGenerator
    {
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            foreach (Thing item in StockGeneratorUtility.TryMakeForStock(ThingDefOf.Gravcore, 1, faction))
            {
                yield return item;
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef == ThingDefOf.Gravcore;
        }
    }
}
