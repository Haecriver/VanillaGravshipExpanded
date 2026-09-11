using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(RitualPosition_Cells), nameof(RitualPosition_Cells.GetCell))]
public static class RitualPosition_Cells_GetCell_Patch
{
    private static readonly Predicate<Pawn, IntVec3> Reachable_PotentiallyUnsafe = (pawn, cell) => pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly);

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        // Vanilla has a bug that triggers if the copilot has minor (or worse) hypothermia/heatstroke and their ritual spot happens to be in
        // a room with temperature that's dangerous for them. Since we use similar code (and update copilot code) on our side, this also
        // affects our researcher and engineer roles during the ritual. This happens due to RitualPosition_Cells removing ritual spots
        // that are considered unreachable, which includes dangerous temperature with one of those 2 conditions. This is not an issue
        // with any other rituals (as far as I'm aware), as those seem to prevent rituals in unsafe temperatures in the first place.
        // This is not a restriction on gravship launches, which causes errors.

        var matcher = new CodeMatcher(instr, generator);

        // Find RemoveLeastDesirableRitualCells call, and call our own variant if the class is RitualPosition_Copilot
        matcher.MatchEndForward(CodeMatch.Calls(() => CommonRitualCellPredicates.RemoveLeastDesirableRitualCells));
        WrapAroundOriginal(CodeInstruction.Call(() => RemoveLeastDesirableRitualCells_GravshipSafe));

        matcher.Reset();
        // Find DefaultValidator call (used for fallback spot), and call our own variant if the class is RitualPosition_Copilot
        matcher.MatchEndForward(CodeMatch.Calls(() => CommonRitualCellPredicates.DefaultValidator));
        WrapAroundOriginal(CodeInstruction.Call(() => DefaultValidator_GravshipSafe));

        return matcher.Instructions();

        void WrapAroundOriginal(CodeInstruction newInstr)
        {
            // Create necessary labels
            matcher.CreateLabel(out var originalMethodLabel);
            matcher.CreateLabelWithOffsets(1, out var afterOriginalMethodLabel);

            matcher.Insert(
                // Load "this" (always arg 0 on non-static method)
                CodeInstruction.LoadArgument(0),
                // Check if "this" is an instance of RitualPosition_Copilot
                new CodeInstruction(OpCodes.Isinst, typeof(RitualPosition_Copilot)),
                // If it's not, jump to original method
                new CodeInstruction(OpCodes.Brfalse_S, originalMethodLabel),
                // If it is, call our method
                newInstr,
                // and jump over the original method
                new CodeInstruction(OpCodes.Br_S, afterOriginalMethodLabel)
            );
        }
    }

    public static bool RemoveLeastDesirableRitualCells_GravshipSafe(List<IntVec3> cells, IntVec3 spot, Map map, Pawn pawn, CellRect rect)
    {
        cells.RemoveAll(rect, CommonRitualCellPredicates.InsideRect);
        cells.RemoveAll(map, CommonRitualCellPredicates.Standable, true);
        cells.RemoveAll((map, spot), CommonRitualCellPredicates.InSameRoomAsSpot, true);
        cells.RemoveAll(pawn, Reachable_PotentiallyUnsafe, true);
        return cells.RemoveAll_IfNotAll(map, CommonRitualCellPredicates.NotOnBed) && cells.RemoveAll_IfNotAll(map, CommonRitualCellPredicates.NotInDoor);
    }

    public static Func<IntVec3, bool> DefaultValidator_GravshipSafe(IntVec3 spot, Map map, Pawn pawn, CellRect rect)
    {
        return cell => !CommonRitualCellPredicates.InsideRect(rect, cell) &&
                       CommonRitualCellPredicates.Standable(map, cell) &&
                       CommonRitualCellPredicates.InSameRoomAsSpot((map, spot), cell) &&
                       Reachable_PotentiallyUnsafe(pawn, cell);
    }

    // private static RitualPosition_Cells cells = null;
    // private static bool faceThing = false;
    // private static bool highlight = false;
    // private static Rot4 facing;
    //
    // public static PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
    // {
    //     RitualPosition_Cells.tmpPotentialCells.Clear();
    //     Thing thing = spot.GetThingList(ritual.Map).FirstOrDefault(t => t == ritual.selectedTarget.Thing);
    //     Map mapHeld = p.MapHeld;
    //     CellRect cellRect = ((thing != null) ? thing.OccupiedRect() : CellRect.CenteredOn(spot, 0));
    //     cells.FindCells(RitualPosition_Cells.tmpPotentialCells, thing, cellRect, spot, (thing != null) ? thing.Rotation : Rot4.South);
    //     CommonRitualCellPredicates.RemoveLeastDesirableRitualCells(RitualPosition_Cells.tmpPotentialCells, spot, mapHeld, p, cellRect);
    //     Func<IntVec3, bool> func = CommonRitualCellPredicates.DefaultValidator(spot, mapHeld, p, cellRect);
    //     IntVec3 intVec;
    //     if (RitualPosition_Cells.tmpPotentialCells.Count != 0)
    //     {
    //         intVec = RitualPosition_Cells.tmpPotentialCells[0];
    //     }
    //     else
    //     {
    //         intVec = cells.GetFallbackSpot(cellRect, spot, p, ritual, func);
    //     }
    //     if (!intVec.IsValid)
    //     {
    //         return null;
    //     }
    //     Rot4 rot;
    //     if (faceThing)
    //     {
    //         if (facing != Rot4.Invalid)
    //         {
    //             Log.Error("Only one of faceThing and facing should be specified.");
    //         }
    //         rot = Rot4.FromAngleFlat((thing.Position - intVec).AngleFlat);
    //     }
    //     else
    //     {
    //         rot = facing;
    //     }
    //     return new PawnStagePosition(intVec, thing, rot, highlight);
    // }
}