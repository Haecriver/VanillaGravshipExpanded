using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public class Building_GunnerSubpersonaCore : Building_TargetingTerminal
    {
        private CompPowerTrader powerComp;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public bool IsPowered => powerComp?.PowerOn ?? false;

        public override bool MannedByPlayer => IsPowered;
    
        public override float GravshipTargeting => 3f;
    }
}
