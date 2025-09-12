using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace VanillaGravshipExpanded
{
    public class Skyfaller_EscapePod : Skyfaller
    {
        public override void Impact()
        {
            IntVec3 position = base.Position;
            Map map = base.Map;
            
            CreateHoleInCeiling(position, map);
            DestroyImpactTile(position, map);
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Filth_BlastMark), position, map, ThingPlaceMode.Near);
            base.Impact();
        }
        
        private void CreateHoleInCeiling(IntVec3 position, Map map)
        {
            RoofDef roofDef = map.roofGrid.RoofAt(position);
            if (roofDef != null)
            {
                map.roofGrid.SetRoof(position, null);
            }
        }
        
        private void DestroyImpactTile(IntVec3 position, Map map)
        {
            foreach (Thing thing in position.GetThingList(map).OfType<Building>().ToList())
            {
                thing.Destroy(DestroyMode.KillFinalize);
            }
        }
    }
}
