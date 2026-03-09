using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompScannerCluster : CompScanner
{
    public IScannerClusterModule ActiveModule
    {
        get;
        set
        {
            field?.Notify_SetAsInactiveScannerClusterComponent();
            field = value;
            value?.Notify_SetAsActiveScannerClusterComponent();
        }
    } = null;

    public IScannerClusterModule PassiveModule
    {
        get;
        set
        {
            if (value is { IsPassiveScannerModule: false })
            {
                Log.Error($"[VGE] Trying to set a passive module with key '{value.ScannerModuleKey}' as a passive module, but it's not a passive module.");
                return;
            }

            value?.SetAsInactivePassiveScannerClusterComponent();
            field = value;
            value?.Notify_SetAsActivePassiveScannerClusterComponent();
        }
    } = null;

    public List<IScannerClusterModule> Components { get; protected set; }

    public override AcceptanceReport CanUseNow
    {
        get
        {
            if (ActiveModule == null)
                return "VGE.NoComponentActive".Translate();

            var canScan = ActiveModule.CanScanNow;
            if (!canScan)
                return canScan;

            return base.CanUseNow;
        }
    }

    public new CompProperties_ScannerCluster Props => (CompProperties_ScannerCluster)props;

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    protected virtual void InitializeComps()
    {
        if (Components != null)
            return;

        Components = [];

        // Since this is a comp, comps list shouldn't be null
        if (parent.comps != null)
        {
            foreach (var comp in parent.comps)
            {
                if (comp is IScannerClusterModule scannerComponent)
                {
                    if (Components.Any(x => x.ScannerModuleKey == scannerComponent.ScannerModuleKey))
                    {
                        Log.Error($"[VGE] Scanner cluster {parent} has a component with a duplicate key ({scannerComponent.ScannerModuleKey}).");
                    }
                    else
                    {
                        scannerComponent.ScannerCluster = this;
                        Components.Add(scannerComponent);
                    }
                }
            }
        }

        if (Components.Count == 0)
        {
            Log.Error($"[VGE] CompScannerCluster did not find any scanner component for {parent}.");
        }
        else
        {
            if (!Props.defaultModuleKey.NullOrEmpty())
            {
                var comp = Components.FirstOrDefault(x => x.ScannerModuleKey == Props.defaultModuleKey);
                if (comp == null)
                {
                    Log.Error($"[VGE] CompScannerCluster for {parent} uses '{Props.defaultModuleKey}' as a default module, but doesn't contain a module with such a key.");
                }
                else
                {
                    ActiveModule = comp;
                }
            }

            if (!Props.defaultPassiveModuleKey.NullOrEmpty())
            {
                var comp = Components.FirstOrDefault(x => x.ScannerModuleKey == Props.defaultPassiveModuleKey);
                if (comp == null)
                {
                    Log.Error($"[VGE] CompScannerCluster for {parent} uses '{Props.defaultPassiveModuleKey}' as a default passive module, but doesn't contain a module with such a key.");
                }
                else if (!comp.IsPassiveScannerModule)
                {
                    Log.Error($"[VGE] CompScannerCluster for {parent} uses '{Props.defaultPassiveModuleKey}' as a default passive module, but it's not a passive module.");
                }
                else
                {
                    PassiveModule = comp;
                }
            }
        }
    }

    public override void DoFind(Pawn worker) => ActiveModule?.DoFind(worker);

    public override bool TickDoesFind(float scanSpeed)
    {
        using (new PropsChangerBlock(this))
            return base.TickDoesFind(scanSpeed);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (ActiveModule is { CanEverScan: true })
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;
        }

        if (parent.Faction == Faction.OfPlayer)
        {
            if (Components.Any(x => x.ScannerClusterActiveModulesFloatMenuOptions().Any()))
            {
                yield return new Command_Action
                {
                    defaultLabel = $"{"CommandSelectMineralToScanFor".Translate()}: {ActiveModule?.GizmoTunedToText ?? "VGE_ScannerCluster_TunedToNothing".Translate()}",
                    defaultDesc = "CommandSelectMineralToScanForDesc".Translate(),
                    icon = ActiveModule?.GizmoIcon ?? Props.DefaultGizmoIcon,
                    action = () => Find.WindowStack.Add(new FloatMenu(Components.SelectMany(x => x.ScannerClusterActiveModulesFloatMenuOptions()).ToList())),
                };
            }

            // TODO
            // if (Components.Any(x => x.ScannerClusterPassiveModuleFloatMenuOptions().Skip(1).Any()))
            // {
            //     yield return new Command_Action
            //     {
            //         defaultLabel = $"{"CommandSelectMineralToScanFor".Translate()}: {PassiveModule?.GizmoTunedToText ?? "VGE_ScannerCluster_TunedToNothing".Translate()}",
            //         defaultDesc = "CommandSelectMineralToScanForDesc".Translate(),
            //         icon = ActiveModule?.GizmoIcon ?? Props.DefaultGizmoIcon,
            //         action = () => Find.WindowStack.Add(new FloatMenu(Components.SelectMany(x => x.ScannerClusterActiveModulesFloatMenuOptions()).ToList())),
            //     };
            // }
        }
    }

    public override string CompInspectStringExtra()
    {
        if (ActiveModule is { CanEverScan: true })
        {
            using (new PropsChangerBlock(this))
                return base.CompInspectStringExtra();
        }

        return string.Empty;
    }

    public bool IsPassiveModuleActive(IScannerClusterModule module) => PassiveModule == module && lastScanTick + 20 < Find.TickManager.TicksGame;

    public interface IScannerClusterModule
    {
        CompScannerCluster ScannerCluster { set; }
        string ScannerModuleKey { get; }
        bool CanEverScan { get; }
        AcceptanceReport CanScanNow { get; }
        Texture2D GizmoIcon { get; }
        string GizmoTunedToText { get; }
        bool IsPassiveScannerModule { get; }

        void DoFind(Pawn worker);

        void Notify_SetAsActiveScannerClusterComponent();
        void Notify_SetAsInactiveScannerClusterComponent();

        void SetAsInactivePassiveScannerClusterComponent();
        void Notify_SetAsActivePassiveScannerClusterComponent();

        IEnumerable<FloatMenuOption> ScannerClusterActiveModulesFloatMenuOptions();
        IEnumerable<FloatMenuOption> ScannerClusterPassiveModuleFloatMenuOptions();

        float ScanFindMtbDaysOverride(float baseScanFindMtbDays);
        float ScanFindGuaranteedDaysOverride(float baseScanFindGuaranteedDays);
    }

    private readonly struct PropsChangerBlock : IDisposable
    {
        private readonly CompScannerCluster cluster;
        private readonly float originalScanFindMtbDays;
        private readonly float originalScanFindGuaranteedDays;

        public PropsChangerBlock(CompScannerCluster cluster)
        {
            this.cluster = cluster;
            originalScanFindMtbDays = cluster.Props.scanFindMtbDays;
            originalScanFindGuaranteedDays = cluster.Props.scanFindGuaranteedDays;

            if (cluster.ActiveModule != null)
            {
                cluster.Props.scanFindMtbDays = cluster.ActiveModule.ScanFindMtbDaysOverride(originalScanFindMtbDays);
                cluster.Props.scanFindGuaranteedDays = cluster.ActiveModule.ScanFindGuaranteedDaysOverride(originalScanFindGuaranteedDays);
            }
        }

        public void Dispose()
        {
            cluster.Props.scanFindMtbDays = originalScanFindMtbDays;
            cluster.Props.scanFindGuaranteedDays = originalScanFindGuaranteedDays;
        }
    }
}