using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class StatPart_ToxicDustCloud : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.Map != null)
            {
                if (req.Thing.Map.weatherManager.curWeather == VGEDefOf.VGE_ToxicDustCloud)
                {
                    val *= 5f;
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.Map != null)
            {
                if (req.Thing.Map.weatherManager.curWeather == VGEDefOf.VGE_ToxicDustCloud)
                {
                    return VGEDefOf.VGE_ToxicDustCloud.LabelCap + ": x5";
                }
            }
            return null;
        }
    }
}
