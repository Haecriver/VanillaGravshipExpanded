using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaGravshipExpanded
{
    public class IncidentWorker_EscapePodCrash : IncidentWorker
    {
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            if (!CellFinder.TryFindRandomCell(map, c => map.roofGrid.RoofAt(c) is RoofDef roofDef && !roofDef.isNatural, out IntVec3 intVec))
            {
                return false;
            }

            Thing escapePod = ThingMaker.MakeThing(VGEDefOf.VGE_DamagedEscapePod);
            SkyfallerMaker.SpawnSkyfaller(VGEDefOf.VGE_EscapePodSkyfaller, new List<Thing> { escapePod }, intVec, map);
            SendStandardLetter("VGE_EscapePodCrash".Translate(), "VGE_EscapePodCrashDesc".Translate(), LetterDefOf.NegativeEvent, parms, new TargetInfo(intVec, map));
            return true;
        }
    }
}
