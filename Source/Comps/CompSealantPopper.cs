using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_SealantPopper : CompProperties
    {
        public float vacuumThreshold = 0.25f;
        public int checkIntervalTicks = 60;

        public CompProperties_SealantPopper()
        {
            compClass = typeof(CompSealantPopper);
        }
    }
    public class CompSealantPopper : ThingComp
    {
        private CompProperties_SealantPopper Props => (CompProperties_SealantPopper)props;

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(Props.checkIntervalTicks) && IsRoomVacuum())
            {
                TriggerSealantPopper();
            }
        }

        private bool IsRoomVacuum()
        {
            Room room = parent.GetRoom();
            if (room == null)
                return false;
            return room.Vacuum > Props.vacuumThreshold;
        }

        private void TriggerSealantPopper()
        {
            var explosive = parent.TryGetComp<CompExplosive>();
            if (!explosive.wickStarted)
            {
                explosive.StartWick();
            }
        }
    }
}
