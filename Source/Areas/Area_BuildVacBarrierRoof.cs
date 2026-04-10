using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Area_BuildVacBarrierRoof : Area
{
    public override string Label => "VGE_BuildVacBarrierRoof".Translate();
    public override Color Color => new(0.3f, 0f, 0.4f);
    public override int ListPriority => 8787; // Lower priority than normal roof area, higher than remove roof area

    public Area_BuildVacBarrierRoof()
    {
        // Presumably needed by the IExposable interface
    }

    public Area_BuildVacBarrierRoof(AreaManager areaManager) : base(areaManager)
    {
    }

    public override string GetUniqueLoadID()
    {
        return $"VGE_Area_{ID.ToString()}_BuildVacBarrierRoof";
    }
}