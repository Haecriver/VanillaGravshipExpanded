using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

// Not necessarily required (especially if inheriting from a different comp class),
// but makes it faster to create a new module that doesn't inherit from other stuff.
// Specifically, the orbital scanner module needs to inherit from actual orbital scanner.
public abstract class CompScannerCluster_BaseModule : ThingComp, CompScannerCluster.IScannerClusterModule
{
    protected float daysWorkingSinceLastFinding;
    protected float lastScanTick = -1f;

    public virtual CompScannerCluster ScannerCluster { get; set; }
    public virtual string ScannerModuleKey => Props.clusterModuleKey;
    public abstract Texture2D GizmoIcon { get; }
    public abstract string GizmoTunedToText { get; }
    public virtual bool CanEverScan => true;
    public virtual bool IsPassiveScannerModule => false;

    public virtual AcceptanceReport CanScanNow
    {
        get
        {
            if (!Props.noActiveScanningMessage.NullOrEmpty()) return Props.noActiveScanningMessage;

            if (Props.scanningResearchPrerequisites != null)
            {
                foreach (var researchPrerequisite in Props.scanningResearchPrerequisites)
                {
                    if (!researchPrerequisite.IsFinished)
                        return Props.missingResearchPrerequisiteMessageKey.Translate(researchPrerequisite.Named("RESEARCH"));
                }
            }

            return true;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref daysWorkingSinceLastFinding, $"{ScannerModuleKey}_{nameof(daysWorkingSinceLastFinding)}");
        Scribe_Values.Look(ref lastScanTick, $"{ScannerModuleKey}_{nameof(lastScanTick)}", -1f);
    }

    public CompProperties_ScannerCluster_ModuleBase Props => (CompProperties_ScannerCluster_ModuleBase)props;

    public virtual void DoFind(Pawn worker)
    {
    }

    public virtual void Notify_SetAsActiveScannerClusterComponent()
    {
        ScannerCluster.daysWorkingSinceLastFinding = daysWorkingSinceLastFinding;
        ScannerCluster.lastScanTick = lastScanTick;

        daysWorkingSinceLastFinding = 0f;
        lastScanTick = -1f;
    }

    public virtual void Notify_SetAsInactiveScannerClusterComponent()
    {
        daysWorkingSinceLastFinding = ScannerCluster.daysWorkingSinceLastFinding;
        lastScanTick = ScannerCluster.lastScanTick;
    }

    public virtual IEnumerable<FloatMenuOption> ScannerClusterActiveModulesFloatMenuOptions()
    {
        yield break;
    }

    public virtual IEnumerable<FloatMenuOption> ScannerClusterPassiveModuleFloatMenuOptions()
    {
        // Presumably, there will be none
        yield break;
    }

    public virtual float ScanFindMtbDaysOverride(float baseScanFindMtbDays) => Props.scanFindMtbDaysOverride ?? baseScanFindMtbDays;

    public virtual float ScanFindGuaranteedDaysOverride(float baseScanFindGuaranteedDays) => Props.scanFindGuaranteedDaysOverride ?? baseScanFindGuaranteedDays;

    public virtual void Notify_SetAsInactivePassiveScannerClusterComponent()
    {
        // Unused, not a passive module (most likely)
    }

    public virtual void Notify_SetAsActivePassiveScannerClusterComponent()
    {
        // Unused, not a passive module (most likely)
    }
}