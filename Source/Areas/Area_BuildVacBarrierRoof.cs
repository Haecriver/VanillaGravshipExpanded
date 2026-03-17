using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Area_BuildVacBarrierRoof : Area
{
    public override string Label => "VGE_BuildVacBarrierRoof".Translate();
    public override Color Color => new(0.9f, 0.9f, 0.5f);
    public override int ListPriority => 20900;

    public Area_BuildVacBarrierRoof()
    {
    }

    public Area_BuildVacBarrierRoof(AreaManager areaManager) : base(areaManager)
    {
    }

    public override string GetUniqueLoadID()
    {
        return $"VGE_Area_{ID.ToString()}_BuildVacBarrierRoof";
    }
}