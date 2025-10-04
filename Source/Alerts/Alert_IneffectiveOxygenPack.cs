using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class Alert_IneffectiveOxygenPack : Alert
{
    private const int CheckInterval = GenTicks.TickRareInterval;
    private int nextCheckTick = -1000;
    private readonly List<Pawn> pawnsWithIneffectiveOxygenPack = [];

    private List<Pawn> PawnsWithIneffectiveOxygenPack
    {
        get
        {
            if (Find.TickManager.TicksGame < nextCheckTick)
            {
                pawnsWithIneffectiveOxygenPack.RemoveAll(x => !x.Spawned);
                return pawnsWithIneffectiveOxygenPack;
            }

            nextCheckTick = Find.TickManager.TicksGame + CheckInterval;
            pawnsWithIneffectiveOxygenPack.Clear();

            foreach (var pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                float baseVacuumResistance = -100000000;

                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    var comp = apparel.GetComp<CompApparelOxygenProvider>();
                    if (comp == null || comp.Props.minResistanceToActivate <= 0)
                        continue;

                    if (baseVacuumResistance < -10000000)
                        baseVacuumResistance = StatDefOf.VacuumResistance.Worker.GetValueUnfinalized(StatRequest.For(pawn), false);
                    if (baseVacuumResistance >= comp.Props.minResistanceToActivate)
                        continue;

                    pawnsWithIneffectiveOxygenPack.Add(pawn);
                    break;
                }
            }

            return pawnsWithIneffectiveOxygenPack;
        }
    }

    public Alert_IneffectiveOxygenPack()
    {
        defaultLabel = "VGE_IneffectiveOxygenPack".Translate();
        defaultExplanation = "VGE_IneffectiveOxygenPackDesc".Translate();
        defaultPriority = AlertPriority.High;
    }

    public override AlertReport GetReport() => AlertReport.CulpritsAre(PawnsWithIneffectiveOxygenPack);

    public override TaggedString GetExplanation()
    {
        var sb = new StringBuilder(base.GetExplanation());

        foreach (var pawn in pawnsWithIneffectiveOxygenPack)
        {
            sb.AppendLine();
            sb.Append("  - ");
            sb.Append(pawn.NameShortColored.Resolve());
        }

        return sb.ToString();
    }
}