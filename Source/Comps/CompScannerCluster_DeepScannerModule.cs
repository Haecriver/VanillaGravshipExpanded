using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using VEF.Maps;
using Verse;

namespace VanillaGravshipExpanded;

public class CompScannerCluster_DeepScannerModule : CompScannerCluster_BaseModule
{
    private ThingDef targetMineable;

    public override Texture2D GizmoIcon => targetMineable?.uiIcon ?? Props.RandomResourceGizmoIcon;

    public override string GizmoTunedToText
    {
        get
        {
            if (targetMineable == null)
                return "VGE_ScannerCluster_TunedToNothing".Translate();
            return "VGE_ScannerCluster_UndergroundResource".Translate(targetMineable.LabelCap.Named("RESOURCE"));
        }
    }

    public override AcceptanceReport CanScanNow
    {
        get
        {
            if (!parent.Map.Biome.hasBedrock)
                return "CannotUseScannerNoBedrock".Translate();
            return base.CanScanNow;
        }
    }

    public new CompProperties_ScannerCluster_DeepScannerModule Props => (CompProperties_ScannerCluster_DeepScannerModule)props;

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Defs.Look(ref targetMineable, nameof(targetMineable));
    }

    public override void DoFind(Pawn worker)
    {
        if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10, x => CanScatterAt(x, parent.Map), parent.Map, out var pos))
            Log.Error("Could not find a center cell for deep scanning lump generation!");

        var lumpDef = ChooseLumpThingDef();
        var lumpSize = Mathf.CeilToInt(lumpDef.deepLumpSizeRange.RandomInRange);

        foreach (var resourcePos in GridShapeMaker.IrregularLump(pos, parent.Map, lumpSize))
        {
            if (CanScatterAt(resourcePos, parent.Map) && !resourcePos.InNoBuildEdgeArea(parent.Map))
                parent.Map.deepResourceGrid.SetAt(resourcePos, lumpDef, GetResourceCount(lumpDef.deepCountPerCell));
        }

        string text;
        if ("LetterDeepScannerFoundLump".CanTranslate())
            text = "LetterDeepScannerFoundLump";
        else if ("DeepScannerFoundLump".CanTranslate())
            text = "DeepScannerFoundLump";
        else
            text = "LetterDeepScannerFoundLump";

        Find.LetterStack.ReceiveLetter("LetterLabelDeepScannerFoundLump".Translate() + ": " + lumpDef.LabelCap, text.Translate(lumpDef.label, worker.Named("FINDER")), LetterDefOf.PositiveEvent, new LookTargets(pos, parent.Map));
    }

    private int GetResourceCount(int baseAmount)
    {
        if (VGEDefOf.VFE_TileMutatorMechanics.IsActive)
            return VanillaExpandedFramework_CompDeepScanner_DoFind_Patch.MultiplyDeepResourceNumbers(baseAmount, parent.Map);
        return baseAmount;
    }

    private bool CanScatterAt(IntVec3 pos, Map map)
    {
        var terrainDef = map.terrainGrid.BaseTerrainAt(pos);
        return (terrainDef is not { IsWater: true } || terrainDef.passability != Traversability.Impassable) &&
               pos.GetAffordances(map).Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded) &&
               !map.deepResourceGrid.GetCellBool(CellIndicesUtility.CellToIndex(pos, map.Size.x));
    }

    protected ThingDef ChooseLumpThingDef() => targetMineable ?? DefDatabase<ThingDef>.AllDefs.RandomElementByWeight(def => def.deepCommonality);

    public override IEnumerable<FloatMenuOption> ScannerClusterActiveModulesFloatMenuOptions()
    {
        yield return new FloatMenuOption(
            "VGE_ScannerCluster_RandomResource".Translate(),
            () => FloatMenuSelectMineable(null),
            Props.RandomResourceGizmoIcon,
            Color.white);

        if (Props.allowForResourceSelection)
        {
            foreach (var mineable in DefDatabase<ThingDef>.AllDefs)
            {
                if (mineable.deepCommonality > 0)
                {
                    var localMineable = mineable;
                    yield return new FloatMenuOption(
                        "VGE_ScannerCluster_UndergroundResource".Translate(localMineable.LabelCap.Named("RESOURCE")),
                        () => FloatMenuSelectMineable(localMineable),
                        localMineable,
                        extraPartWidth: 24f,
                        extraPartOnGUI: rect => FloatMenuExtraPart(rect, mineable));
                }
            }
        }
    }

    private static void FloatMenuSelectMineable(ThingDef mineable)
    {
        foreach (var selected in Find.Selector.SelectedObjects)
        {
            if (selected is ThingWithComps thing && thing.GetComp<CompScannerCluster_DeepScannerModule>() is { } module)
            {
                module.targetMineable = mineable;
                module.ScannerCluster.ActiveModule = module;
            }
        }
    }

    private static bool FloatMenuExtraPart(Rect rect, ThingDef mineable)
    {
        return Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, mineable);
    }

    public override float ScanFindMtbDaysOverride(float baseScanFindMtbDays) => base.ScanFindMtbDaysOverride(baseScanFindMtbDays) * CurrentMineableCommonalityMultiplier();

    public override float ScanFindGuaranteedDaysOverride(float baseScanFindGuaranteedDays) => base.ScanFindGuaranteedDaysOverride(baseScanFindGuaranteedDays) * CurrentMineableCommonalityMultiplier();

    private float CurrentMineableCommonalityMultiplier() => (ThingDefOf.Steel?.deepCommonality ?? 4f) / (targetMineable?.deepCommonality ?? 4f);
}