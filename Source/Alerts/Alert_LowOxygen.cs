using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class Alert_LowOxygen : Alert
{
    public const float OxygenPerPawn = 50f;

    public Alert_LowOxygen()
    {
        defaultLabel = "VGE_Alert_LowOxygen".Translate();
        defaultPriority = AlertPriority.High;
    }

    public override TaggedString GetExplanation()
    {
        if (MapWithLowOxygen(out var oxygen, out var pawnsNeedingOxygen) == null)
            return string.Empty;

        return "VGE_Alert_LowOxygenExplanation".Translate(oxygen.ToString("F0"), pawnsNeedingOxygen.ToStringCached());
    }

    public override AlertReport GetReport() => MapWithLowOxygen(out _, out _) != null;

    private static Map MapWithLowOxygen(out float storedOxygen, out int pawnsNeedingOxygen)
    {
        foreach (var map in Find.Maps)
        {
            if (map.Biome?.inVacuum != true)
                continue;
            if (!map.mapPawns.AnyColonistSpawned)
                continue;

            pawnsNeedingOxygen = 0;
            var neededOxygen = 0f;

            foreach (var pawn in map.mapPawns.FreeColonistsAndPrisoners)
            {
                if (pawn.RaceProps.Humanlike)
                {
                    if (pawn.RaceProps.IsMechanoid)
                        continue;
                    if (pawn.IsMutant && !pawn.mutant.def.breathesAir)
                        continue;
                    var resistance = pawn.GetStatValue(StatDefOf.VacuumResistance, cacheStaleAfterTicks: 60);
                    if (resistance >= 1)
                        continue;

                    pawnsNeedingOxygen++;
                    neededOxygen += OxygenPerPawn * (1f - resistance);
                }
            }

            if (neededOxygen < OxygenPerPawn * 2)
                neededOxygen = OxygenPerPawn;

            storedOxygen = 0f;
            var anyStorage = false;

            var nets = CachedPipeNetManager.GetFor(map).pipeNets;
            for (var i = 0; i < nets.Count; i++)
            {
                var net = nets[i];
                if (net.def == VGEDefOf.VGE_OxygenNet && net.storages.Count > 0)
                {
                    anyStorage = true;
                    storedOxygen += net.Stored;
                }
            }

            if (anyStorage && storedOxygen < neededOxygen)
                return map;
        }

        storedOxygen = 0;
        pawnsNeedingOxygen = 0;
        return null;
    }
}