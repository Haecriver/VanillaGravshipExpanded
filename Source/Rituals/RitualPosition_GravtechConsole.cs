using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace VanillaGravshipExpanded
{
    public class RitualPosition_GravtechConsole : RitualPosition_Cells
    {
        public override void FindCells(List<IntVec3> cells, Thing thing, CellRect rect, IntVec3 spot, Rot4 rotation)
        {
            var pilotConsole = thing.TryGetComp<CompPilotConsole>();
            var engine = pilotConsole.engine;
            var gravtechConsole = FindLinkedGravtechConsole(engine);
            
            if (gravtechConsole != null)
            {
                cells.Add(gravtechConsole.InteractionCell);
            }
            else
            {
                cells.Add(spot);
            }
        }
        
        public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
        {
            var pilotConsole = ritual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>();
            var engine = pilotConsole.engine;
            var gravtechConsole = FindLinkedGravtechConsole(engine);
            
            if (gravtechConsole != null)
            {
                IntVec3 cell = gravtechConsole.InteractionCell;
                Rot4 orientation = Rot4.FromAngleFlat((gravtechConsole.Position - cell).AngleFlat);
                return new PawnStagePosition(cell, gravtechConsole, orientation, highlight);
            }
            else
            {
                return new PawnStagePosition(spot, null, Rot4.South, highlight);
            }
        }
        
        private Thing FindLinkedGravtechConsole(Building_GravEngine engine)
        {
            if (engine == null)
                return null;
                
            foreach (var facility in engine.GravshipComponents)
            {
                if (facility.parent.def == VGEDefOf.VGE_GravtechConsole)
                {
                    return facility.parent;
                }
            }
            
            return null;
        }
    }
}
