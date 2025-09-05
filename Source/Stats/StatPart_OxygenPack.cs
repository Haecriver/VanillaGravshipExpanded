using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class StatPart_OxygenPack : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (req.Thing is not Pawn pawn || val >= 1f)
            return;

        var apparel = GetRelevantApparel(pawn, val);
        if (apparel != null)
            val += 1f;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (req.Thing is not Pawn pawn)
            return null;

        var apparel = GetRelevantApparel(pawn, StatDefOf.VacuumResistance.Worker.GetValueUnfinalized(req, false));
        if (apparel == null)
            return null;

        return "".Translate();
    }

    private static CompApparelOxygenProvider GetRelevantApparel(Pawn pawn, float baseVacuumResistance)
    {
        if (pawn?.apparel?.WornApparel == null)
            return null;

        foreach (var apparel in pawn.apparel.WornApparel)
        {
            var comp = apparel.GetComp<CompApparelOxygenProvider>();
            if (comp is { RemainingCharges: > 0 } && comp.Props.minResistanceToActivate <= baseVacuumResistance)
                return comp;
        }

        return null;
    }
}