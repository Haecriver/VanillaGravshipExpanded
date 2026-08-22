using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class ScenPart_ChooseStartingGravship : ScenPart_ConfigPage
    {
        public string tag;
        public StartingGravshipDef chosenDef;
        public float startingAstrofuel;
        public float startingOxygen;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tag, "tag");
            Scribe_Defs.Look(ref chosenDef, "chosenDef");
            Scribe_Values.Look(ref startingAstrofuel, "startingAstrofuel");
            Scribe_Values.Look(ref startingOxygen, "startingOxygen");
        }
    }
}
