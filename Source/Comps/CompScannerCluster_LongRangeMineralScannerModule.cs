using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompScannerCluster_LongRangeMineralScannerModule : CompScannerCluster_BaseModule
{
    private ThingDef targetMineable;

    public override Texture2D GizmoIcon => targetMineable?.building.mineableThing.uiIcon;

    public override string GizmoTunedToText
    {
        get
        {
            if (targetMineable?.building?.mineableThing == null)
                return "VGE_ScannerCluster_TunedToNothing".Translate();
            return "VGE_ScannerCluster_LongRangeResource".Translate(targetMineable.building.mineableThing.LabelCap.Named("RESOURCE"));
        }
    }

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        SetDefaultTargetMineral();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Defs.Look(ref targetMineable, nameof(targetMineable));
        if (Scribe.mode == LoadSaveMode.PostLoadInit && targetMineable == null)
            SetDefaultTargetMineral();
    }

    private void SetDefaultTargetMineral()
    {
        targetMineable = ThingDefOf.MineableGold;
    }

    public override void DoFind(Pawn worker)
    {
        var slate = new Slate();
        slate.Set(SlateNames.Map, parent.Map);
        slate.Set(SlateNames.TargetMineable, targetMineable);
        slate.Set(SlateNames.TargetMineableThing, targetMineable.building.mineableThing);
        slate.Set(SlateNames.Worker, worker);

        if (!QuestScriptDefOf.LongRangeMineralScannerLump.CanRun(slate, parent.Map))
            return;

        var quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.LongRangeMineralScannerLump, slate);
        Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);
    }

    public override IEnumerable<FloatMenuOption> ScannerClusterActiveModulesFloatMenuOptions()
    {
        foreach (var mineable in ((GenStep_PreciousLump)GenStepDefOf.PreciousLump.genStep).mineables)
        {
            var localMineable = mineable;
            yield return new FloatMenuOption(
                "VGE_ScannerCluster_LongRangeResource".Translate(localMineable.building.mineableThing.LabelCap.Named("RESOURCE")),
                () => FloatMenuSelectMineable(localMineable),
                localMineable.building.mineableThing,
                extraPartWidth: 24f,
                extraPartOnGUI: rect => FloatMenuExtraPart(rect, mineable));
        }
    }

    private static void FloatMenuSelectMineable(ThingDef mineable)
    {
        foreach (var selected in Find.Selector.SelectedObjects)
        {
            if (selected is ThingWithComps thing && thing.GetComp<CompScannerCluster_LongRangeMineralScannerModule>() is { } module)
            {
                module.targetMineable = mineable;
                module.ScannerCluster.ActiveModule = module;
            }
        }
    }

    private static bool FloatMenuExtraPart(Rect rect, ThingDef mineable)
    {
        return Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, mineable.building.mineableThing);
    }
}