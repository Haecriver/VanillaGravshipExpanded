using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Page_ChooseStartingGravship : Page
    {
        private const float DescMargin = 10f;
        private const float ItemHeight = 270f;
        private const float ImageHeight = 160f;
        private const float LabelHeight = 24f;
        private const float SourceHeight = 20f;
        private const float ItemMargin = 8f;
        private const float ScrollBarWidth = 20f;
        private const int Columns = 3;
        public ScenPart_ChooseStartingGravship scenPart;
        private Vector2 scrollPosition;
        private StartingGravshipDef selectedDef;

        public override string PageTitle => "VGE_ChooseAStartingGravship".Translate();

        public Page_ChooseStartingGravship(ScenPart_ChooseStartingGravship scenPart)
        {
            this.scenPart = scenPart;
            selectedDef = DefDatabase<StartingGravshipDef>.AllDefsListForReading.First(x => x.gravshipTags.Contains(scenPart.tag));
        }

        public override void DoWindowContents(Rect rect)
        {
            DrawPageTitle(rect);
            var mainRect = GetMainRect(rect);

            Text.Font = GameFont.Small;
            var descHeight = Text.CalcHeight("VGE_ChooseAStartingGravshipDesc".Translate(), mainRect.width);
            var descRect = new Rect(mainRect.x, mainRect.y, mainRect.width, descHeight);
            Widgets.Label(descRect, "VGE_ChooseAStartingGravshipDesc".Translate());

            var gridOutRect = new Rect(mainRect.x, descRect.yMax + DescMargin, mainRect.width, mainRect.height - descHeight - DescMargin);
            var defs = DefDatabase<StartingGravshipDef>.AllDefsListForReading.Where(x => x.gravshipTags.Contains(scenPart.tag)).ToList();

            var itemWidth = (gridOutRect.width - (ItemMargin * 2f) - ScrollBarWidth) / Columns;
            var rows = Mathf.CeilToInt(defs.Count / (float)Columns);
            var viewRect = new Rect(0f, 0f, gridOutRect.width - ScrollBarWidth, rows * (ItemHeight + ItemMargin));

            Widgets.BeginScrollView(gridOutRect, ref scrollPosition, viewRect);
            for (int i = 0; i < defs.Count; i++)
            {
                var row = i / Columns;
                var itemRect = new Rect(i % Columns * (itemWidth + ItemMargin), row * (ItemHeight + ItemMargin), itemWidth, ItemHeight);
                DrawStartingGravshipItem(itemRect, defs[i]);
            }
            Widgets.EndScrollView();

            DoBottomButtons(rect, "Next".Translate());
        }

        private void DrawStartingGravshipItem(Rect rect, StartingGravshipDef def)
        {
            rect = rect.ContractedBy(2);
            Widgets.DrawOptionBackground(rect, selectedDef == def);
            var innerRect = rect.ContractedBy(ItemMargin);
            var imageRect = new Rect(innerRect.x, innerRect.y, innerRect.width, ImageHeight);
            GUI.DrawTexture(imageRect, def.ImagePreview, ScaleMode.ScaleToFit);

            Text.Font = GameFont.Small;
            var labelRect = new Rect(innerRect.x, imageRect.yMax + ItemMargin, innerRect.width, LabelHeight);
            Widgets.Label(labelRect, def.LabelCap.Colorize(ColoredText.TipSectionTitleColor));

            var descRect = new Rect(innerRect.x, labelRect.yMax - 3, innerRect.width, innerRect.height - ImageHeight - LabelHeight - SourceHeight - ItemMargin + 6);
            Widgets.Label(descRect, def.description);
            Text.Anchor = TextAnchor.LowerLeft;
            var sourceRect = new Rect(innerRect.x, innerRect.yMax - SourceHeight, innerRect.width, SourceHeight);
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(sourceRect, "VGE_SourceMod".Translate(def.modContentPack.Name));
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(rect))
            {
                selectedDef = def;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }

        public override bool CanDoNext()
        {
            scenPart.chosenDef = selectedDef;
            return base.CanDoNext();
        }
    }
}
