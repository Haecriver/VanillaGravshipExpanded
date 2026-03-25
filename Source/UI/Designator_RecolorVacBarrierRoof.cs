using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class Designator_RecolorVacBarrierRoof : DesignatorWithEyedropper
{
    private static readonly Texture2D ColorWheel = ContentFinder<Texture2D>.Get("UI/MenuIcons/ColorWheel");
    private Color color = VacBarrierRoofUtility.BaseColor;

    public override bool DragDrawMeasurements => true;

    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Paint;

    public override Color IconDrawColor => color;

    protected Texture2D IconTopTex => field ??= ContentFinder<Texture2D>.Get("UI/Designators/PaintFloor_Top");

    public override bool Visible => VacBarrierRoofUtility.IsGravBarrierRoofAccessible();

    public Designator_RecolorVacBarrierRoof()
    {
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Paint;
        eyedropper = new Designator_VacBarrierRoofEyedropper(c =>
        {
            color = c;
            if (!eyedropMode)
                Find.DesignatorManager.Select(this);
        }, "SelectAPaintedBuilding".Translate(), "DesignatorEyeDropperDesc_Paint".Translate());

        defaultLabel = "VGE_DesignatorRecolorVacBarrierRoof".Translate();
        defaultDesc = "VGE_DesignatorRecolorVacBarrierRoofDesc".Translate();
        icon = ContentFinder<Texture2D>.Get("UI/Designators/PaintFloor_Bottom");
    }

    public override void ProcessInput(Event ev)
    {
        if (!CheckCanInteract())
            return;

        var list = new List<FloatMenuGridOption>
        {
            new(Designator_Eyedropper.EyeDropperTex, () =>
            {
                base.ProcessInput(ev);
                Find.DesignatorManager.Select(eyedropper);
            }, null, "DesignatorEyeDropperDesc_Paint".Translate()),
            new(ColorWheel, () =>
            {
                Find.WindowStack.Add(new Dialog_VacBarrierRoofDesignatorColorPicker(c =>
                {
                    base.ProcessInput(ev);
                    color = c;
                }, color, VacBarrierRoofUtility.BaseColor, Dialog_NoLimitColorPicker.EditableRgb, Dialog_NoLimitColorPicker.EditableRgb));
            }, null, "VGE_ColorPickerMenu".Translate()),
        };

        foreach (var c in Dialog_GlowerColorPicker.colors)
        {
            list.Add(new FloatMenuGridOption(BaseContent.WhiteTex, () =>
            {
                base.ProcessInput(ev);
                Find.DesignatorManager.Select(this);
                color = c;
            }, c));
        }

        Find.WindowStack.Add(new FloatMenuGrid(list));
        Find.DesignatorManager.Select(this);
    }

    public override void DrawMouseAttachments()
    {
        eyedropMode = KeyBindingDefOf.ShowEyedropper.IsDown;
        if (eyedropMode)
            eyedropper.DrawMouseAttachments();
        else if (useMouseIcon)
            GenUI.DrawMouseAttachment(icon, $"{KeyBindingDefOf.ShowEyedropper.MainKeyLabel}: {"GrabExistingColor".Translate()}", iconAngle, iconOffset, iconColor: color, postDrawAction: rect => GUI.DrawTexture(rect, IconTopTex));
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
    }

    public override void RenderHighlight(List<IntVec3> dragCells)
    {
        DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
    }

    public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
    {
        base.DrawIcon(rect, buttonMat, parms);
        Widgets.DrawTextureFitted(rect, IconTopTex, iconDrawScale * 0.85f, iconProportions, iconTexCoords, iconAngle, buttonMat);
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (eyedropMode)
            return eyedropper.CanDesignateCell(c);

        if (!c.InBounds(Map) || c.Fogged(Map))
            return false;
        var roofColor = Map.VacBarrierRoofColorAt(c);
        return roofColor != null && roofColor != color;
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        if (eyedropMode)
        {
            eyedropper.DesignateSingleCell(c);
            return;
        }

        Map.SetVacBarrierRoofColorAt(c, color);
        Map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Roofs);
    }
}