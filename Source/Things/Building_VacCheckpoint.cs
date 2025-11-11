using System.Collections.Generic;
using System.Text;
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
        if (pawn.RaceProps.IsMechanoid)
            return false;
        if (pawn.IsMutant && !pawn.mutant.Def.breathesAir)
            return false;
        if (allowDrafted && pawn.Drafted)
            return false;
        if (pawn.Faction != Faction)
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

        if (Faction == Faction.OfPlayer)
        {
            yield return new Command_Action
            {
                defaultLabel = "VGE_VacCheckpoint_SetVacuumResistance".Translate(),
                defaultDesc = "VGE_VacCheckpoint_SetVacuumResistanceDesc".Translate(),
                icon = VacuumResistanceGizmo,
                action = () => Find.WindowStack.Add(new Dialog_ConfigureVacuumRequirement(requiredResistance, allowDrafted)),
            };
        }
    }

    public override string GetInspectString()
    {
        var str = new StringBuilder(base.GetInspectString());

        str.AppendInNewLine("VGE_VacCheckpoint_MinimumVacuumResistance".Translate(requiredResistance));
        str.AppendInNewLine((allowDrafted ? "VGE_VacCheckpoint_AllowDrafted" : "VGE_VacCheckpoint_DisallowDrafted").Translate());

        return str.ToString();
    }

    public override void ExposeData()
    {
        base.ExposeData();
    
        Scribe_Values.Look(ref requiredResistance, nameof(requiredResistance));
        Scribe_Values.Look(ref allowDrafted, nameof(allowDrafted));
    }
}