using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Dialog_ConfigureVacuumRequirement : Window
{
    private const float Height = 30f;
    private const float NumberYOffset = 10f;

    private float currentResistance;
    private bool currentAllowedDrafted;

    public override Vector2 InitialSize => new(300f, 100f + Height + Height);

    public override float Margin => 10f;

    public Dialog_ConfigureVacuumRequirement(float startingResistance, bool startingAllowedDrafted)
    {
        currentResistance = startingResistance;
        currentAllowedDrafted = startingAllowedDrafted;

        forcePause = true;
        closeOnClickedOutside = true;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var text = "VGE_VacCheckpoint_VacuumResistancePercent".Translate(currentResistance);
        var topRect = new Rect(inRect.x, inRect.y, inRect.width, Text.CalcHeight(text, inRect.width));

        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(topRect, text);

        Text.Anchor = TextAnchor.UpperLeft;
        var sliderRect = new Rect(inRect.x, inRect.y + topRect.height + NumberYOffset, inRect.width, Height);
        currentResistance = Widgets.HorizontalSlider(sliderRect, currentResistance, 0f, 1f, true, null, null, null, 0.01f);
        GUI.color = ColoredText.SubtleGrayColor;
        Text.Font = GameFont.Tiny;

        Widgets.Label(new Rect(inRect.x, sliderRect.yMax - NumberYOffset, inRect.width / 2f, Text.LineHeight), 0f.ToStringPercent());

        Text.Anchor = TextAnchor.UpperRight;

        Widgets.Label(new Rect(inRect.x + inRect.width / 2f, sliderRect.yMax - NumberYOffset, inRect.width / 2f, Text.LineHeight), 1f.ToStringPercent());

        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        var checkboxRect = new Rect(inRect.x, sliderRect.yMax + NumberYOffset, inRect.width, Height);
        Widgets.CheckboxLabeled(checkboxRect, "VGE_VacCheckpoint_AllowDraftedCheckbox".Translate(), ref currentAllowedDrafted);

        var buttonWidth = (inRect.width - NumberYOffset) / 2f;
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - Height, buttonWidth, Height), "CancelButton".Translate()))
        {
            Close();
        }
        if (Widgets.ButtonText(new Rect(inRect.x + buttonWidth + 10f, inRect.yMax - Height, buttonWidth, Height), "OK".Translate()))
        {
            Close();
            SetSelectedVacCheckpointsTo(currentResistance, currentAllowedDrafted);
        }
    }

    private static void SetSelectedVacCheckpointsTo(float resistance, bool allowDrafted)
    {
        resistance = Mathf.Clamp01(resistance);
        Map map = null;

        foreach (var selected in Find.Selector.SelectedObjects)
        {
            if (selected is Building_VacCheckpoint checkpoint && checkpoint.Faction == Faction.OfPlayer)
            {
                checkpoint.requiredResistance = resistance;
                checkpoint.allowDrafted = allowDrafted;
                map ??= checkpoint.Map;
            }
        }

        if (map?.Biome?.inVacuum == true)
        {
            ReachabilityCache.tmpCachedEntries.Clear();

            foreach (var entry in map.reachability.cache.cacheDict)
            {
                var pawn = entry.Key.TraverseParms.pawn;
                if (pawn != null && pawn.Faction == Faction.OfPlayer)
                    ReachabilityCache.tmpCachedEntries.Add(entry.Key);
            }

            for (var i = 0; i < ReachabilityCache.tmpCachedEntries.Count; i++)
                map.reachability.cache.cacheDict.Remove(ReachabilityCache.tmpCachedEntries[i]);

            ReachabilityCache.tmpCachedEntries.Clear();
        }
    }
}