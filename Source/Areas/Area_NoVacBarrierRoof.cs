using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Area_NoVacBarrierRoof : Area
{
    public override string Label => "VGE_NoVacBarrierRoof".Translate();
    public override Color Color => new(0.9f, 0.5f, 0.1f);
    public override int ListPriority => 20800;

    public Area_NoVacBarrierRoof()
    {
    }

    public Area_NoVacBarrierRoof(AreaManager areaManager) : base(areaManager)
    {
    }

    public override string GetUniqueLoadID()
    {
        return $"VGE_Area_{ID.ToString()}_NoVacBarrierRoof";
    }
}