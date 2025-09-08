
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public class Building_GravCapacitor:Building_Battery
    {
        private static readonly Vector2 NewBarSize = new Vector2(2f, 0.4f);

        private static readonly Material NewBatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f,0.5f));

        private static readonly Material NewBatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.5f));

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
          
            CompPowerBattery comp = GetComp<CompPowerBattery>();
            GenDraw.FillableBarRequest fillableBarRequest = default(GenDraw.FillableBarRequest);
            fillableBarRequest.center = drawLoc + Vector3.up * 0.1f;
            fillableBarRequest.size = NewBarSize;
            fillableBarRequest.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            fillableBarRequest.filledMat = NewBatteryBarFilledMat;
            fillableBarRequest.unfilledMat = NewBatteryBarUnfilledMat;
            fillableBarRequest.margin = 0.15f;
            GenDraw.FillableBarRequest r = fillableBarRequest;
            Rot4 rotation = base.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
            if (ticksToExplode > 0 && base.Spawned)
            {
                base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
            }
        }

      
    }
}