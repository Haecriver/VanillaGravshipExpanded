using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class SectionLayer_ScaffoldClear : SectionLayer
    {
        private static readonly int RenderLayer = LayerMask.NameToLayer("GravshipMask");
        public override bool Visible => DebugViewSettings.drawTerrain;
        public SectionLayer_ScaffoldClear(Section section) : base(section)
        {
            relevantChangeTypes = MapMeshFlagDefOf.Terrain;
        }

        public override void Regenerate()
        {
            GravshipHelper.RegenerateScaffoldLayer(this, GravshipHelper.TileMaterial, AltitudeLayer.Floor, RenderLayer);
        }
    }
}
