using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    public class CompMannableWithFailReason : CompMannable
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.RaceProps.ToolUser || !pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield break;
            }
            if (Props.manWorkType != 0)
            {
                if (pawn.WorkTagIsDisabled(WorkTags.Violent) && Props.manWorkType.HasFlag(WorkTags.Violent))
                {
                    yield return new FloatMenuOption("CannotManThing".Translate(parent.LabelShort, parent) + " (" + "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn) + ")", null);
                }
                else if (pawn.WorkTagIsDisabled(WorkTags.Intellectual) && Props.manWorkType.HasFlag(WorkTags.Intellectual))
                {
                    yield return new FloatMenuOption("CannotManThing".Translate(parent.LabelShort, parent) + " (" + "VGE_IsIncapableOfIntellectualLower".Translate(pawn.LabelShort, pawn) + ")", null);
                }
            }
            foreach (var floatOption in base.CompFloatMenuOptions(pawn))
            {
                yield return floatOption;
            }
        }
    }
}
