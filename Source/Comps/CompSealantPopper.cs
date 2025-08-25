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
        private int nextCheckTick = 0;
        private CompProperties_SealantPopper Props => (CompProperties_SealantPopper)props;

        public override void CompTick()
        {
            base.CompTick();

            if (Find.TickManager.TicksGame >= nextCheckTick)
            {
                nextCheckTick = Find.TickManager.TicksGame + Props.checkIntervalTicks;

                if (IsRoomVacuum())
                {
                    TriggerSealantPopper();
                }
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
            CompExplosive explosive = parent.TryGetComp<CompExplosive>();
            if (explosive != null && !explosive.wickStarted)
            {
                explosive.StartWick();
            }
        }
    }
}
