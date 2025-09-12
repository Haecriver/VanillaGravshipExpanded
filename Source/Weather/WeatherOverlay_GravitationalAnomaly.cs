using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class WeatherOverlay_GravitationalAnomaly : WeatherOverlayDualPanner
    {
        public WeatherOverlay_GravitationalAnomaly()
        {
            Init();
        }

        public void Init()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
              

                worldOverlayMat = new Material(MaterialPool.MatFrom("Weather/MicrometeorStormWorldOverlay"));
                var mat = MatLoader.LoadMat("Weather/GlowSporeOverlayWorld");
                worldOverlayMat.CopyPropertiesFromMaterial(mat);
                worldOverlayMat.shader = mat.shader;
                Texture2D texture = ContentFinder<Texture2D>.Get("Weather/MicrometeorStormWorldOverlay");
                worldOverlayMat.SetTexture("_MainTex", texture);
                worldOverlayMat.SetTexture("_MainTex2", texture);
                worldOverlayMat.SetColor("_TuningColor", new Color(0.4f,0.53f,1f, 0.05f));
                worldOverlayMat.SetColor("_Color", new Color(0.4f, 0.53f, 1f, 0.05f));
                worldOverlayMat.SetColor("_MainTexColor", new Color(0.4f, 0.53f, 1f, 0.05f));
                worldOverlayMat.SetColor("_MainTex2Color", new Color(0.4f, 0.53f, 1f, 0.05f));



            });
        }
    }
}
