using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class WeatherOverlay_DustCloud : WeatherOverlayDualPanner
    {
        public WeatherOverlay_DustCloud()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                Init();
            });
        }

        public virtual void Init()
        {
            worldOverlayPanSpeed1 = 0.005f;
            worldPanDir1 = new Vector2(1f, 1f);
            worldPanDir1.Normalize();
            worldOverlayPanSpeed2 = 0.0004f;
            worldPanDir2 = new Vector2(1f, -1f);
            worldPanDir2.Normalize();

            worldOverlayMat = new Material(MaterialPool.MatFrom("Weather/SpaceFogWorld"));
            var mat = MatLoader.LoadMat("Weather/FogOverlayWorld");
            worldOverlayMat.CopyPropertiesFromMaterial(mat);
            worldOverlayMat.shader = mat.shader;
            Texture2D texture = ContentFinder<Texture2D>.Get("Weather/SpaceFogWorld");
            worldOverlayMat.SetTexture("_MainTex", texture);
            worldOverlayMat.SetTexture("_MainTex2", texture);
            worldOverlayMat.SetColor("_TuningColor", new Color(0.5f, 0.5f, 0.6f, 0.7f));
        }
    }
}
