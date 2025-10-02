using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class RitualOutcomeComp_LandingOnGravAnchor : RitualOutcomeComp_QualitySingleOffset
{
    public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data) => IsGravAnchorPresent(ritual.ritual) ? base.QualityOffset(ritual, data) : 0f;

    public override bool Applies(LordJob_Ritual ritual) => IsGravAnchorPresent(ritual.ritual);

    public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
    {
        if (!IsGravAnchorPresent(ritual))
            return null;

        return new QualityFactor
        {
            label = LabelForDesc,
            qualityChange = ExpectedOffsetDesc(qualityOffset > 0, qualityOffset),
            present = qualityOffset > 0,
            quality = qualityOffset,
            positive = qualityOffset > 0,
            priority = 1f,
            noMiddleColumnInfo = true,
        };
    }

    private static bool IsGravAnchorPresent(Precept_Ritual ritual)
    {
        var state = Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state;
        if (state == null || state.instance != ritual || !state.targetTile.Valid)
            return false;

        var map = Current.Game.FindMap(state.targetTile);
        return map != null && map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor);
    }
}