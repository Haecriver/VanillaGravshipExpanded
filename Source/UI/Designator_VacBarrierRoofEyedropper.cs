using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Designator_VacBarrierRoofEyedropper : Designator_Eyedropper
{
    private Action<Color> vacBarrierRoofSelectAction;

    public Designator_VacBarrierRoofEyedropper(Action<Color> selectAction, string rejectMessage, string desc) : this(selectAction, def => selectAction?.Invoke(def.color), rejectMessage, desc)
    {
    }

    public Designator_VacBarrierRoofEyedropper(Action<Color> vacBarrierRoofSelectAction, Action<ColorDef> selectAction, string rejectMessage, string desc) : base(selectAction, rejectMessage, desc)
    {
        this.vacBarrierRoofSelectAction = vacBarrierRoofSelectAction;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (FloatMenuOptionsAt(c).Any())
            return true;
        if (!rejectMessage.NullOrEmpty())
            return rejectMessage;
        return false;
    }

    public override void DesignateSingleCell(IntVec3 cell)
    {
        var list = FloatMenuOptionsAt(cell).ToList();
        if (list.Count >= 2)
            Find.WindowStack.Add(new FloatMenu(list));
        else if (list.Count == 1)
            list[0].action();
        else if (!rejectMessage.NullOrEmpty())
            Messages.Message(rejectMessage, MessageTypeDefOf.RejectInput, false);
    }

    private IEnumerable<FloatMenuOption> FloatMenuOptionsAt(IntVec3 cell)
    {
        if (!cell.InBounds(Map) || cell.Fogged(Map))
            yield break;

        var vacBarrierRoofColor = VacBarrierRoofColorAt(cell);
        if (vacBarrierRoofColor != null)
        {
            yield return new FloatMenuOption("VGE_GrabVacBarrierRoofColor".Translate(), () =>
            {
                vacBarrierRoofSelectAction?.Invoke(vacBarrierRoofColor.Value);
                Messages.Message("GrabbedColor".Translate(), null, MessageTypeDefOf.NeutralEvent, false);
            }, extraPartWidth: FloatMenuOption.ExtraPartHeight, extraPartOnGUI: rect =>
            {
                Widgets.DrawRectFast(rect.ScaledBy(0.75f), vacBarrierRoofColor.Value);
                return false;
            });
        }

        foreach (var building in ColoredBuildingsAt(cell))
        {
            yield return new FloatMenuOption("VGE_GrabBuildingColor".Translate(building.Named("BUILDING"), building.PaintColorDef.Named("COLOR")), () =>
            {
                selectAction?.Invoke(building.PaintColorDef);
                Messages.Message($"{"GrabbedColor".Translate()}: {building.PaintColorDef.LabelCap}", null, MessageTypeDefOf.NeutralEvent, false);
            }, extraPartWidth: FloatMenuOption.ExtraPartHeight, extraPartOnGUI: rect =>
            {
                Widgets.DrawRectFast(rect.ScaledBy(0.75f), building.PaintColorDef.color);
                return false;
            });
        }

        var floorColor = FloorColorAt(cell);
        if (floorColor != null)
        {
            yield return new FloatMenuOption("VGE_GrabFloorColor".Translate(floorColor.Named("COLOR")), () =>
            {
                selectAction?.Invoke(floorColor);
                Messages.Message($"{"GrabbedColor".Translate()}: {floorColor.LabelCap}", null, MessageTypeDefOf.NeutralEvent, false);
            }, extraPartWidth: FloatMenuOption.ExtraPartHeight, extraPartOnGUI: rect =>
            {
                Widgets.DrawRectFast(rect.ScaledBy(0.75f), floorColor.color);
                return false;
            });
        }
    }

    private Color? VacBarrierRoofColorAt(IntVec3 cell) => Map.VacBarrierRoofColorAt(cell);

    private IEnumerable<Building> ColoredBuildingsAt(IntVec3 cell)
    {
        var things = cell.GetThingList(Map);
        for (var i = 0; i < things.Count; i++)
        {
            if (things[i] is Building building && building.def.building.paintable && building.PaintColorDef != null)
                yield return building;
        }
    }

    private ColorDef FloorColorAt(IntVec3 cell) => Map.terrainGrid.ColorAt(cell) ?? cell.GetTerrain(Map).colorDef;

    public override ColorDef ColorDefAt(IntVec3 cell) => ColoredBuildingsAt(cell).FirstOrDefault()?.PaintColorDef ?? FloorColorAt(cell);

    public override void DrawMouseAttachments()
    {
        if (useMouseIcon)
        {
            string text;
            var colorsAt = FloatMenuOptionsAt(UI.MouseCell()).Take(2).ToList();

            if (colorsAt.Count == 2)
                text = "VGE_SelectColorFromTile".Translate();
            else if (colorsAt.Count == 1)
                text = colorsAt[0].Label;
            else if (!rejectMessage.NullOrEmpty())
                text = rejectMessage;
            else
                text = string.Empty;

            GenUI.DrawMouseAttachment(icon, text, iconAngle, iconOffset);
        }
    }
}