using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class WeatherOverlay_MicrometeorStorm : WeatherOverlayDualPanner
    {
        public WeatherOverlay_MicrometeorStorm()
        {
            Init();
        }

        public void Init()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                worldOverlayPanSpeed1 = 0.03f;
                worldPanDir1 = new Vector2(-0.25f, -1f);
                worldPanDir1.Normalize();
                worldOverlayPanSpeed2 = 0.04f;
                worldPanDir2 = new Vector2(-0.24f, -1f);
                worldPanDir2.Normalize();

                worldOverlayMat = new Material(MaterialPool.MatFrom("Weather/MicrometeorStormWorldOverlay"));
                var mat = MatLoader.LoadMat("Weather/SnowOverlayWorld");
                worldOverlayMat.CopyPropertiesFromMaterial(mat);
                worldOverlayMat.shader = mat.shader;
                Texture2D texture = ContentFinder<Texture2D>.Get("Weather/MicrometeorStormWorldOverlay");
                worldOverlayMat.SetTexture("_MainTex", texture);
                worldOverlayMat.SetTexture("_MainTex2", texture);
                worldOverlayMat.SetColor("_TuningColor", new Color(0.272f, 0.272f, 0.272f, 0.6f));
            });
        }
    }
}
