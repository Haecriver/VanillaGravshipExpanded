using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class Building_VacCheckpoint : Building_Door
{
    private static readonly Texture2D VacuumResistanceGizmo = ContentFinder<Texture2D>.Get("UI/Gizmos/SetDesiredVacuumResistance");

    public float requiredResistance = 0.7f;
    public bool allowDrafted = true;

    public override bool AlwaysOpen => true;

    public override bool CanDrawMovers => false;

    public bool CaresAboutThisVacBarrier(Pawn pawn)
    {
        if (Map.Biome?.inVacuum != true)
            return false;
        if (allowDrafted && pawn.Drafted)
            return false;
        if (pawn.Faction != Faction.OfPlayer)
            return false;
        if (pawn.InMentalState)
            return false;

        // TODO: Double check if those may be needed
        // if (pawn.guest is { Released: true })
        //     return false;
        // if (pawn.pather.cachedReturningToCell && Faction == pawn.HostFaction)
        //     return false;

        return pawn.GetStatValue(StatDefOf.VacuumResistance, cacheStaleAfterTicks: 60) < requiredResistance;
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        yield return new Command_Action
        {
            defaultLabel = "VGE_SetVacuumResistance".Translate(),
            defaultDesc = "VGE_SetVacuumResistanceDesc".Translate(),
            icon = VacuumResistanceGizmo,
            // TODO: Make a custom dialog, since the vanilla one seems to do some weird rounding.
            action = () => Find.WindowStack.Add(new Dialog_Slider(x => "VGE_VacuumResistancePercent".Translate(x), 0, 100, SetSelectedVacCheckpointsTo, (int)(requiredResistance * 100))),
        };
    }

    private static void SetSelectedVacCheckpointsTo(int resistancePercent)
    {
        var resistance = Mathf.Clamp01(resistancePercent / 100f);

        foreach (var selected in Find.Selector.SelectedObjects)
        {
            if (selected is Building_VacCheckpoint checkpoint)
                checkpoint.requiredResistance = resistance;
        }
    }

    public override string GetInspectString()
    {
        var str = base.GetInspectString();

        if (!str.NullOrEmpty())
            str += "\n";
        str += "VGE_MinimumVacuumResistance".Translate(requiredResistance);

        return str;
    }
}