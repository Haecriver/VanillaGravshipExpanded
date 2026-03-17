using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Designator_AreaNoVacBarrierRoof : Designator_Cells
{
    private static readonly List<IntVec3> JustAddedCells = [];

    public override bool DragDrawMeasurements => true;

    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Areas;

    public Designator_AreaNoVacBarrierRoof()
    {
        defaultLabel = "VGE_DesignatorAreaNoVacBarrierRoofExpand".Translate();
        defaultDesc = "VGE_DesignatorAreaNoVacBarrierRoofExpandDesc".Translate();
        icon = ContentFinder<Texture2D>.Get("UI/MenuIcons/RemoveVacBarrierRoof_Designator");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = null;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        useMouseIcon = true;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map))
            return false;
        if (c.Fogged(Map))
            return false;
        return !Map.areaManager.NoVacBarrierRoof()[c];
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.NoVacBarrierRoof()[c] = true;
        JustAddedCells.Add(c);
    }

    public override void FinalizeDesignationSucceeded()
    {
        base.FinalizeDesignationSucceeded();

        for (var i = 0; i < JustAddedCells.Count; i++)
            Map.areaManager.BuildVacBarrierRoof()[JustAddedCells[i]] = false;

        JustAddedCells.Clear();
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.NoVacBarrierRoof().MarkForDraw();
        Map.areaManager.BuildVacBarrierRoof().MarkForDraw();
    }
}