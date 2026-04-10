using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Designator_AreaBuildVacBarrierRoof : Designator_Cells
{
    public override bool DragDrawMeasurements => true;

    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Areas;

    public override bool Visible => VacBarrierRoofUtility.IsGravBarrierRoofAccessible();

    public Designator_AreaBuildVacBarrierRoof()
    {
        defaultLabel = "VGE_DesignatorAreaBuildVacBarrierRoofExpand".Translate();
        defaultDesc = "VGE_DesignatorAreaBuildVacBarrierRoofExpandDesc".Translate();
        icon = ContentFinder<Texture2D>.Get("UI/MenuIcons/RoofVacBarrier_Designator");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd_Roof;
        useMouseIcon = true;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map))
            return false;
        if (c.Fogged(Map))
            return false;

        return !Map.areaManager.BuildVacBarrierRoof()[c];
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.BuildVacBarrierRoof()[c] = true;
        Map.areaManager.NoRoof[c] = false;
        Map.areaManager.BuildRoof[c] = false;
    }

    // public override bool ShowWarningForCell(IntVec3 c)
    // {
    //     var roofDef = Map.roofGrid.RoofAt(c);
    //     if (roofDef != null)
    //     {
    //         if (roofDef == RoofDefOf.RoofRockThick && !roofDef.CanReplaceWithVacBarrier())
    //         {
    //             Messages.Message("VGE_".Translate(), MessageTypeDefOf.CautionInput, false);
    //             return true;
    //         }
    //
    //         if (roofDef.CanReplaceWithVacBarrier())
    //         {
    //             Messages.Message("VGE_".Translate(), MessageTypeDefOf.CautionInput, false);
    //             return true;
    //         }
    //     }
    //
    //     return false;
    // }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.NoRoof.MarkForDraw();
        Map.areaManager.BuildRoof.MarkForDraw();
        Map.areaManager.BuildVacBarrierRoof().MarkForDraw();
    }
}