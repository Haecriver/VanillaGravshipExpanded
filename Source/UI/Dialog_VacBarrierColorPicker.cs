using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HotSwappable]
public class Dialog_VacBarrierColorPicker : Dialog_NoLimitColorPicker
{
    public Building_VacBarrier_Recolorable vacBarrier;
    public Building_VacBarrier_Recolorable[] extraVacBarriers;

    public override Color DefaultColor => vacBarrier.def.colorGenerator.ExemplaryColor;

    public Dialog_VacBarrierColorPicker(Building_VacBarrier_Recolorable vacBarrier, List<Building_VacBarrier_Recolorable> extraVacBarriers, Widgets.ColorComponents visibleTextfields, Widgets.ColorComponents editableTextfields) : base(visibleTextfields, editableTextfields)
    {
        this.vacBarrier = vacBarrier;
        this.extraVacBarriers = extraVacBarriers.ToArray();

        color = vacBarrier.barrierColor;
        oldColor = vacBarrier.barrierColor;
    }

    public override void SaveColor(Color color)
    {
        foreach (var extraVacBarrier in extraVacBarriers)
        {
            extraVacBarrier.barrierColor = color;
            extraVacBarrier.Notify_ColorChanged();
        }
    }
}