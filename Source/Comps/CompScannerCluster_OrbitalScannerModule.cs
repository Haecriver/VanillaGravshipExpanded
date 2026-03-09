using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class CompScannerCluster_OrbitalScannerModule : CompOrbitalScanner, CompScannerCluster.IScannerClusterModule
{
    protected static Texture2D gizmoIcon = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/AsteroidMine");

    public CompScannerCluster ScannerCluster { get; set; }

    public bool CanEverScan => false;

    public AcceptanceReport CanScanNow => Props.noActiveScanningMessage;

    public Texture2D GizmoIcon => gizmoIcon;

    public string GizmoTunedToText => "VGE_ScannerCluster_TunedToOrbitalScanning".Translate();

    public bool IsPassiveScannerModule => true;

    public string ScannerModuleKey => Props.clusterModuleKey;

    public CompProperties_ScannerCluster_ModuleBase Props => (CompProperties_ScannerCluster_ModuleBase)props;

    public override void CompTick()
    {
        if (ScannerCluster.IsPassiveModuleActive(this))
            base.CompTick();
        // If scanner is inactive, we need to increment the tick at which the signal will be found (or else it'll keep working despite being off)
        else if (locateSignalTick > 0)
            locateSignalTick++;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (ScannerCluster.IsPassiveModuleActive(this))
            return base.CompGetGizmosExtra();
        return [];
    }

    public override string CompInspectStringExtra()
    {
        if (ScannerCluster.IsPassiveModuleActive(this))
            return base.CompInspectStringExtra();
        return string.Empty;
    }

    public IEnumerable<FloatMenuOption> ScannerClusterActiveModulesFloatMenuOptions()
    {
        yield return new FloatMenuOption("VGE_ScannerCluster_TunedToOrbitalScanning".Translate(), () => ScannerCluster.ActiveModule = this, GizmoIcon, Color.white);
    }

    public IEnumerable<FloatMenuOption> ScannerClusterPassiveModuleFloatMenuOptions()
    {
        yield break; // TODO
    }

    float CompScannerCluster.IScannerClusterModule.ScanFindMtbDaysOverride(float baseScanFindMtbDays) => baseScanFindMtbDays;

    float CompScannerCluster.IScannerClusterModule.ScanFindGuaranteedDaysOverride(float baseScanFindGuaranteedDays) => baseScanFindGuaranteedDays;

    public void DoFind(Pawn worker)
    {
        // Unused
    }

    public void Notify_SetAsActiveScannerClusterComponent()
    {
        // Unused
    }

    public void Notify_SetAsInactiveScannerClusterComponent()
    {
        // Unused
    }

    public void Notify_SetAsActivePassiveScannerClusterComponent()
    {
        // Unused
    }

    public void SetAsInactivePassiveScannerClusterComponent()
    {
        // Unused
    }
}