using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_EnemyTerminal : CompProperties
    {
        public float cooldownFactor = 1f;
        public float cooldownFlatOffset = 0f;
        public float forcedMissRadiusOffset = 0f;
        public CompProperties_EnemyTerminal() => compClass = typeof(CompEnemyTerminal);
    }

    public class CompEnemyTerminal : ThingComp
    {
        public CompProperties_EnemyTerminal Props => (CompProperties_EnemyTerminal)props;
        public bool IsManned => parent.GetComp<CompMannable>() is CompMannable mannable && mannable.MannedNow;
    }
}
