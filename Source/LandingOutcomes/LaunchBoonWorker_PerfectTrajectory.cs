using System.Linq;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_PerfectTrajectory : LaunchBoonWorker
    {
        public LaunchBoonWorker_PerfectTrajectory(LaunchBoonDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return true;
        }

        public override void ApplyBoon(Gravship gravship)
        {
            var engine = gravship.Engine;
            var spentFuelData = engine.launchInfo.ExtendedInfo(false)?.fuelSpentPerTank;
            if (spentFuelData == null || spentFuelData.fuelData.Count <= 0)
                return;

            foreach (var entry in spentFuelData.fuelData)
            {
                entry.Key.TryGetComp<CompRefuelable>()?.Refuel(entry.Value * 0.25f);
            }

            GravshipFuelProviderUtility.RefundFuelForAllProviders(gravship.engine, 0.25f, spentFuelData);

            SendStandardLetter(engine, null, engine);
        }
    }
}
