using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class ExtraGravshipDataComp : WorldObjectComp
{
    private Dictionary<IntVec3, Color> vacBarrierRoofColors;
    public IEnumerable<(IntVec3, Color)> VacBarrierRoofColors => ((Gravship)parent).GetRotatedValues(vacBarrierRoofColors);

    public override void Initialize(WorldObjectCompProperties props)
    {
        base.Initialize(props);

        // In constructor, parent is null. We need it to be non-null for extra safety checks, so we use Initialize.

        if (Gravship_CopyCellContents.tempVacBarrierRoofColors != null && Gravship_CopyCellContents.currentGravship == parent && Scribe.mode == LoadSaveMode.Inactive)
            vacBarrierRoofColors = Gravship_CopyCellContents.tempVacBarrierRoofColors;
        else
            vacBarrierRoofColors = new Dictionary<IntVec3, Color>();

        Gravship_CopyCellContents.tempVacBarrierRoofColors = null;
        Gravship_CopyCellContents.currentGravship = null;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Collections.Look(ref vacBarrierRoofColors, nameof(vacBarrierRoofColors), LookMode.Value, LookMode.Value);
    }
}