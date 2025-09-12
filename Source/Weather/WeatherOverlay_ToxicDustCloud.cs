using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class WeatherOverlay_ToxicDustCloud : WeatherOverlay_DustCloud
    {
        public override void Init()
        {
            base.Init();
            var color = Color.green;
            worldOverlayMat.color = color;
            worldOverlayMat.SetColor("_Color", color);
            worldOverlayMat.SetColor("_TuningColor", color);
        }
    }
}
