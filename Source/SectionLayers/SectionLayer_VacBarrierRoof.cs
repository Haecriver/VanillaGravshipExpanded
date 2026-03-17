using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class SectionLayer_VacBarrierRoof : SectionLayer
{
    // TODO: Move somewhere else, unhardcode the color
    private static readonly Material VacBarrierGraphic = MaterialPool.MatFrom("Things/Structures/RoofVacBarrier", ShaderDatabase.TransparentPostLight, Color.white);
    private CellRect bounds;

    public SectionLayer_VacBarrierRoof(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlagDefOf.Roofs;
    }

    public override CellRect GetBoundaryRect() => bounds;

    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);
        bounds = section.CellRect;

        foreach (var pos in section.CellRect)
        {
            var roof = Map.roofGrid.RoofAt(pos);
            if (roof == null || roof != VGEDefOf.VGE_VacBarrierRoof)
                continue;

            Print(pos.ToVector3ShiftedWithAltitude(AltitudeLayer.Skyfaller) + new Vector3(0, 0, 0.5f));
        }

        FinalizeMesh(MeshParts.All);
    }

    private void Print(Vector3 pos)
    {
        Graphic.TryGetTextureAtlasReplacementInfo(VacBarrierGraphic, TextureAtlasGroup.Misc, false, true, out var material, out var array, out var color);
        Printer_Plane.PrintPlane(this, pos, Vector2.one, material, 0, false, array, [color, color, color, color]);
    }
}