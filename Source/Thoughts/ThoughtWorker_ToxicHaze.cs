using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class ThoughtWorker_ToxicHaze : ThoughtWorker
    {
        public override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (pawn.Position.UsesOutdoorTemperature(pawn.Map) && pawn.Map.weatherManager.curWeather == VGEDefOf.VGE_ToxicDustCloud)
            {
                return ThoughtState.ActiveDefault;
            }
            return ThoughtState.Inactive;
        }
    }
}
