using System;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Dialog_VacBarrierRoofDesignatorColorPicker : Dialog_NoLimitColorPicker
{
    private readonly Action<Color> callback;

    public override Color DefaultColor { get; }

    public Dialog_VacBarrierRoofDesignatorColorPicker(Action<Color> callback, Color currentColor, Color defaultColor, Widgets.ColorComponents visibleTextfields, Widgets.ColorComponents editableTextfields) : base(visibleTextfields, editableTextfields)
    {
        color = currentColor;
        DefaultColor = defaultColor;
        this.callback = callback;
    }

    public override void SaveColor(Color color) => callback(color);
}