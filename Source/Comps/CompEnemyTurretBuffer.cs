using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_EnemyTurretBuffer : CompProperties
    {
        public float radius;
        public float cooldownReductionTicks;
        public float maxForcedMissRadius;
        public int maxLinks = 999;
        public List<ThingDef> validTurrets;
        public CompProperties_EnemyTurretBuffer() => compClass = typeof(CompEnemyTurretBuffer);
    }
    public class CompEnemyTurretBuffer : ThingComp
    {
        public CompProperties_EnemyTurretBuffer Props => (CompProperties_EnemyTurretBuffer)props;
        private CompStunnable compStunnable;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compStunnable = parent.GetComp<CompStunnable>();
        }
        public bool Active => compStunnable.StunHandler.Stunned is false;
    }
}
