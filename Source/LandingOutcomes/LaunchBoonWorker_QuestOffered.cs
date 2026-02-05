using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_QuestOffered : LaunchBoonWorker
    {

        public LaunchBoonWorker_QuestOffered(LaunchBoonDef def)
            : base(def)
        {
        }
        public override void ApplyBoon(Gravship gravship)
        {
            Faction friendlyFaction = Find.FactionManager.AllFactions.Where(f => f.AllyOrNeutralTo(Faction.OfPlayer)).RandomElementWithFallback();
            if (friendlyFaction != null) {
                Pawn pawn = friendlyFaction.leader;
                var text = LetterText.Formatted(gravship.Engine.RenamableLabel.Named("GRAVSHIP"), friendlyFaction.Named("FACTION"), pawn.Named("LEADER"));
                SendStandardLetter(gravship.Engine, null, gravship.Engine, text);
                Slate slate = new Slate();
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(VGEDefOf.SurveySite, slate);
                QuestUtility.SendLetterQuestAvailable(quest);
            }
            
        }


        public override bool CanTrigger(Gravship gravship)
        {
            return Find.FactionManager.AllFactions.Where(f => f.AllyOrNeutralTo(Faction.OfPlayer)).Count()>0;
        }
    }
}
