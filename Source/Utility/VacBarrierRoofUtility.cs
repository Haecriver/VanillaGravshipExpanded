using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public static class VacBarrierRoofUtility
{
    public static readonly HashSet<RoofDef> ReplaceableRoofDefs = [RoofDefOf.RoofConstructed, RoofDefOf.RoofRockThin];

    public static Area_BuildVacBarrierRoof BuildVacBarrierRoof(this AreaManager manager) => manager.Get<Area_BuildVacBarrierRoof>();

    public static Area_NoVacBarrierRoof NoVacBarrierRoof(this AreaManager manager) => manager.Get<Area_NoVacBarrierRoof>();

    public static bool CanReplaceWithVacBarrier(this RoofDef roof) => roof == null || ReplaceableRoofDefs.Contains(roof);
}