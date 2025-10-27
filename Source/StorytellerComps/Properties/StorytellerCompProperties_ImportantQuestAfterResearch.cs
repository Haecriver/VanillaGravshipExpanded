using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class StorytellerCompProperties_ImportantQuestAfterResearch:StorytellerCompProperties_ImportantQuest
    {
        public ResearchProjectDef research;

       

        public StorytellerCompProperties_ImportantQuestAfterResearch()
        {
            compClass = typeof(StorytellerComp_ImportantQuestAfterResearch);
        }
    }
}