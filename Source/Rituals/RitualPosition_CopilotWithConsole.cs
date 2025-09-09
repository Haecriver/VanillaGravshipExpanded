using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class RitualPosition_CopilotWithConsole : RitualPosition_Copilot
{
    public override void FindCells(List<IntVec3> cells, Thing thing, CellRect rect, IntVec3 spot, Rot4 rotation)
    {
        if (thing.TryGetComp<CompPilotConsole>(out var comp) && comp.engine != null)
        {
            foreach (var facility in comp.engine.AffectedByFacilities.LinkedFacilitiesListForReading)
            {
                if (facility.TryGetComp<CompCopilotConsole>(out var console) && console.CanBeActive)
                {
                    cells.Add(facility.InteractionCell);
                    return;
                }
            }
        }

        // If we didn't find any copilot consoles, fallback to original behaviour
        base.FindCells(cells, thing, rect, spot, rotation);
    }

    public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
    {
        var result = base.GetCell(spot, p, ritual);

        // Make sure the pawn is facing the copilot console, rather than the pilot console
        if (faceThing)
        {
            foreach (var cell in GenAdjFast.AdjacentCells8Way(result.cell))
            {
                var building = cell.GetEdificeSafe(ritual.Map);
                var comp = building?.GetComp<CompCopilotConsole>();
                if (comp is { CanBeActive: true } && building.InteractionCell == result.cell)
                    result.orientation = Rot4.FromAngleFlat((building.Position - result.cell).AngleFlat);
            }
        }

        return result;
    }
}