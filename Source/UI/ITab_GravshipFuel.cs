using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class ITab_GravshipFuel : ITab
{
    private const float MinHeight = 450f;

    private Vector2 scrollPosition;
    private float viewHeight = 1000f;

    public override bool IsVisible => base.IsVisible && GravEngine != null;

    protected Building_GravEngine GravEngine
    {
        get
        {
            return SelThing switch
            {
                null => null,
                Building_GravEngine engine => engine,
                ThingWithComps thing => thing.GetComp<CompGravshipFacility>()?.engine,
                _ => null
            };
        }
    }

    public ITab_GravshipFuel()
    {
        size = new Vector2(420f, MinHeight);
        labelKey = "VGE_GravshipFuelTab";
    }

    public override void FillTab()
    {
        var gravEngine = GravEngine;
        if (gravEngine == null)
        {
            CloseTab();
            return;
        }

        var entries = GravshipFuelProviderUtility.GetFuelTabEntriesForAllProviders(gravEngine);
        // TODO: Special text?
        if (entries.NullOrEmpty())
        {
            size.y = MinHeight;
            return;
        }

        entries.SortBy(x => x.SortingOrder, x => x.Title);

        var outerRect = new Rect(0f, 16f, size.x, size.y).ContractedBy(10f);
        var viewRect = new Rect(0f, 0f, outerRect.width - 16f, viewHeight);

        Widgets.BeginScrollView(outerRect, ref scrollPosition, viewRect);
        var y = 0f;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var entryRect = entry.DoInterface(0f, y, outerRect.width, i);
            y += entryRect.height + 6f;
        }

        if (Event.current.type == EventType.Layout)
        {
            viewHeight = y + 60f;
            size.y = Mathf.Clamp(viewHeight, MinHeight, UI.screenHeight - InspectPaneUtility.PaneHeight - InspectPaneUtility.TabHeight - 90f);
        }

        Widgets.EndScrollView();
    }
}