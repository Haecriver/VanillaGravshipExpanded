using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    public class JobGiver_ExtinguishSelfAstrofire : ThinkNode_JobGiver
    {
        private const float ActivateChance = 0.1f;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (Rand.Value < ActivateChance)
            {
                Astrofire fire = (Astrofire)pawn.GetAttachment(VGEDefOf.VGE_Astrofire);
                if (fire != null)
                {
                    return JobMaker.MakeJob(VGEDefOf.VGE_ExtinguishSelfAstrofire, fire);
                }
            }
            return null;
        }
    }
}