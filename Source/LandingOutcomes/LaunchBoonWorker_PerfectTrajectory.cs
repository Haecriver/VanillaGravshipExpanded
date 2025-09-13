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
            if (!LaunchInfo_ExposeData_Patch.fuelSpentPerTank.TryGetValue(engine.launchInfo, out var spentFuelData))
                return;
                
            foreach (var entry in spentFuelData.fuelData)
            {
                var thing = entry.Key;
                var amountSpent = entry.Value;
                var storageComp = thing.TryGetComp<CompResourceStorage>();
                float amountToRefund = amountSpent * 0.25f;
                storageComp.AddResource(amountToRefund);
            }

            SendStandardLetter(engine, null, engine);
        }
    }
}
