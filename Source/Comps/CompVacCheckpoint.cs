using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class CompVacCheckpoint : ThingComp
{
    private static readonly Texture2D VacuumResistanceGizmo = ContentFinder<Texture2D>.Get("UI/Gizmos/SetDesiredVacuumResistance");

    public float requiredResistance;
    public bool allowDrafted;

    public CompProperties_VacCheckpoint Props => (CompProperties_VacCheckpoint)props;

    public virtual bool CaresAboutThisVacBarrier(Pawn pawn)
    {
        if (parent.Map.Biome?.inVacuum != true)
            return false;
        if (pawn.RaceProps.IsMechanoid)
            return false;
        if (pawn.IsMutant && !pawn.mutant.Def.breathesAir)
            return false;
        if (allowDrafted && pawn.Drafted)
            return false;
        if (pawn.Faction != parent.Faction)
        {
            if (!Props.alwaysApplyToPlayer)
                return false;
            if (pawn.Faction is { IsPlayer: false })
                return false;
        }
        if (!Props.applyToPawnsWithMentalState && pawn.InMentalState)
            return false;

        // TODO: Double check if those may be needed
        // if (pawn.guest is { Released: true })
        //     return false;
        // if (pawn.pather.cachedReturningToCell && Faction == pawn.HostFaction)
        //     return false;

        return pawn.GetStatValue(StatDefOf.VacuumResistance, cacheStaleAfterTicks: 60) < requiredResistance;
    }

    public override void PostPostMake()
    {
        base.PostPostMake();

        requiredResistance = Mathf.Clamp01(Props.requiredResistanceByDefault);
        allowDrafted = Props.allowDraftedByDefault;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (parent.Faction == Faction.OfPlayer)
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

    public override string CompInspectStringExtra()
    {
        var str = new StringBuilder(base.CompInspectStringExtra());

        str.AppendInNewLine("VGE_VacCheckpoint_MinimumVacuumResistance".Translate(requiredResistance));
        str.AppendInNewLine((allowDrafted ? "VGE_VacCheckpoint_AllowDrafted" : "VGE_VacCheckpoint_DisallowDrafted").Translate());

        return str.ToString();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref requiredResistance, nameof(requiredResistance));
        Scribe_Values.Look(ref allowDrafted, nameof(allowDrafted));
    }
}