using RimWorld;
using UnityEngine.UIElements;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded
{
    public class StatPart_ToxicDustCloud : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.Map != null)
            {
                if (req.Thing.Map.weatherManager.curWeather == VGEDefOf.VGE_ToxicDustCloud && !req.Thing.Position.Roofed(req.Thing.Map))
                {
                    val *= 5f;
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.Map != null)
            {
                if (req.Thing.Map.weatherManager.curWeather == VGEDefOf.VGE_ToxicDustCloud && !req.Thing.Position.Roofed(req.Thing.Map))
                {
                    return VGEDefOf.VGE_ToxicDustCloud.LabelCap + ": x5";
                }
            }
            return null;
        }
    }
}
