using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class RitualOutcomeComp_MannedGravshipFacilities : RitualOutcomeComp_GravshipFacilities
{
    [NoTranslate]
    public string roleId;

    public int facilitiesPerPawn = -1;

    public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
    {
        Building_GravEngine engine = null;
        if (ritualTarget.Thing is { } thing)
        {
            var compPilotConsole = thing.TryGetComp<CompPilotConsole>();
            engine = compPilotConsole?.engine;
        }

        if (engine == null)
            return null;

        var maxConnected = 0;
        if (facilitiesPerPawn > 0)
            maxConnected = assignments.AssignedPawns(roleId).Count() * facilitiesPerPawn;
        else if (assignments.AnyPawnAssigned(roleId))
            maxConnected = int.MaxValue;

        if (maxConnected == 0)
            return null;

        var totalOffset = 0f;
        var connected = 0;

        foreach (var compGravshipFacility in engine.GravshipComponents)
        {
            var powerComp = compGravshipFacility.parent.GetComp<CompPowerTrader>();
            if (powerComp == null || powerComp.PowerOn)
            {
                var thruster = compGravshipFacility.parent.GetComp<CompGravshipThruster>();
                if (thruster == null || thruster.CanBeActive)
                {
                    var def = compGravshipFacility.parent.def;
                    if (facilityQualityOffsets.TryGetValue(def, out var offset))
                    {
                        totalOffset += offset;
                        connected++;

                        if (tmpFacilityCount.TryGetValue(def, out var currentCount))
                            tmpFacilityCount[def] = currentCount + 1;
                        else
                            tmpFacilityCount[def] = 0;

                        if (connected >= maxConnected)
                            break;
                    }
                }
            }
        }

        if (connected == 0)
            return null;

        var stringBuilder = new StringBuilder();
        foreach (var (thingDef, count) in tmpFacilityCount)
        {
            if (facilityQualityOffsets.TryGetValue(thingDef, out var offset))
                stringBuilder.AppendLine($" - {thingDef.LabelCap} x{count}: +{(offset * count).ToStringPercent()}");
        }

        tmpFacilityCount.Clear();

        var maxSimultaneous = 0;
        foreach (var (thingDef, _) in facilityQualityOffsets)
            maxSimultaneous += thingDef.GetCompProperties<CompProperties_GravshipFacility>().maxSimultaneous;

        var text = stringBuilder.ToString();

        return new QualityFactor
        {
            label = LabelForDesc.CapitalizeFirst(),
            qualityChange = "OutcomeBonusDesc_QualitySingleOffset".Translate(totalOffset.ToStringWithSign("0.#%")).Resolve(),
            count = $"{connected} / {maxSimultaneous}",
            quality = totalOffset,
            positive = true,
            priority = 4f,
            toolTip = text.NullOrEmpty() ? null : text
        };
    }
}