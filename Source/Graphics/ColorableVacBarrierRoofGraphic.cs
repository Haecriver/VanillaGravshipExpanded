using System.Collections.Generic;
using UnityEngine;
using VEF.Maps;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class ColorableVacBarrierRoofGraphic : RoofExtension.CustomRoofGraphic
{
    public Dictionary<Color, RoofDrawData> drawDataForColor = [];

    public override RoofDrawData DrawDataAt(Map map, IntVec3 cell, RoofDef roof)
    {
        var color = map.VacBarrierRoofColorAt(cell) ?? customRoofGraphicColor;
        // If close enough to original, just use that
        if (color.IndistinguishableFromFast(customRoofGraphicColor))
            return base.DrawDataAt(map, cell, roof);
        // Use cached data if present
        if (drawDataForColor.TryGetValue(color, out var data))
            return data;

        // If not present, create new data
        drawDataForColor[color] = data = new RoofDrawData
        {
            drawSize = drawSize,
            offset = offset,
            layer = layer,
            material = MaterialPool.MatFrom(customRoofGraphicPath, customRoofGraphicShader?.Shader ?? ShaderDatabase.Cutout, color, renderQueue)
        };
        return data;
    }
}