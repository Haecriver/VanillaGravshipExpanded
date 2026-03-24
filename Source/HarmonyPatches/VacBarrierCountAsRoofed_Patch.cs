using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class VacBarrierCountAsRoofed_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // We treat the vac barrier as unroofed, and it works fine for the majority of situations.
        // However, there's some that need patching so they do count as roofed.

        // Needed so the rooms don't leak oxygen
        yield return typeof(District).DeclaredMethod(nameof(District.OpenRoofCountStopAt));
        // Make sure roofs won't be built over vac barriers (including on surrounding tiles)
        yield return typeof(JobDriver_BuildRoof).DeclaredMethod(nameof(JobDriver_BuildRoof.DoEffect));
        // Make sure the pawns don't try to build over vac barriers in the first place (excluding surrounding tiles)
        yield return typeof(JobDriver_BuildRoof).DeclaredMethod(nameof(JobDriver_BuildRoof.DoWorkFailOn));
        // Pawns can't attempt to build on top of vac barrier, should fix the prioritizing float menu option
        yield return typeof(WorkGiver_BuildRoof).DeclaredMethod(nameof(WorkGiver_BuildRoof.HasJobOnCell));
        // Vac barrier can hold other roofs up, can collapse, etc.
        yield return typeof(RoofCollapseCellsFinder).DeclaredMethod(nameof(RoofCollapseCellsFinder.CheckAndRemoveCollpsingRoofs));
        yield return typeof(RoofCollapseCellsFinder).DeclaredMethod(nameof(RoofCollapseCellsFinder.ProcessRoofHolderDespawned), [typeof(CellRect), typeof(IntVec3), typeof(Map), typeof(bool), typeof(bool)]);
        yield return typeof(RoofCollapseCellsFinder).DeclaredMethod(nameof(RoofCollapseCellsFinder.CheckCollapseFlyingRoofAtAndAdjInternal));
        yield return typeof(RoofCollapseCellsFinder).DeclaredMethod(nameof(RoofCollapseCellsFinder.ConnectsToRoofHolder));
        foreach (var method in typeof(RoofCollapserImmediate).GetDeclaredMethods().Where(method => method.Name == nameof(RoofCollapserImmediate.DropRoofInCells)))
            yield return method;
        yield return typeof(RoofCollapseUtility).DeclaredMethod(nameof(RoofCollapseUtility.ConnectedToRoofHolder));
        yield return typeof(RoofCollapseUtility).DeclaredMethod(nameof(RoofCollapseUtility.WithinRangeOfRoofHolder));
        // Allow vanilla remove roof work and job giver to detect vac barrier roofs
        yield return typeof(JobDriver_RemoveRoof).DeclaredMethod(nameof(JobDriver_RemoveRoof.DoWorkFailOn));
        yield return typeof(WorkGiver_RemoveRoof).DeclaredMethod(nameof(WorkGiver_RemoveRoof.HasJobOnCell));
        yield return typeof(WorkGiver_RemoveRoof).DeclaredMethod(nameof(WorkGiver_RemoveRoof.GetPriority));
    }

    private static void Prefix(out bool __state)
    {
        // Only modify the temp variable if it's not set already
        if (!RoofGrid_Roofed_Patch.countVacBarrierAsRoofed)
        {
            __state = true;
            RoofGrid_Roofed_Patch.countVacBarrierAsRoofed = true;
        }
        else
        {
            __state = false;
        }
    }

    private static void Finalizer(bool __state)
    {
        // Only restore the temporary variable if we set it to true, and not something else
        if (__state)
            RoofGrid_Roofed_Patch.countVacBarrierAsRoofed = false;
    }
}