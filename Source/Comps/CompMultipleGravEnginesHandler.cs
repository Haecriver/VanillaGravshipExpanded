using System.Collections.Generic;
using RimWorld;
using VEF.CacheClearing;
using VEF.Graphics;
using Verse;

namespace VanillaGravshipExpanded;

public class CompMultipleGravEnginesHandler : ThingComp
{
    private static readonly HashSet<CompMultipleGravEnginesHandler> ActiveGravEngines = [];

    private CustomOverlayDrawer overlayDrawer;

    public static int ActiveGravEngineCount => ActiveGravEngines.Count;

    public static bool MultipleGravEnginesPresent => ActiveGravEngineCount >= 2;

    static CompMultipleGravEnginesHandler() => ClearCaches.clearCacheTypes.Add(typeof(CompMultipleGravEnginesHandler));

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        ActiveGravEngines.Add(this);
        overlayDrawer = parent.Map.GetComponent<CustomOverlayDrawer>();
        Notify_GravEngineCountChanged();
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        ActiveGravEngines.Remove(this);
        Notify_GravEngineCountChanged();
    }

    public override void Notify_MapRemoved()
    {
        // This method requires parent def to have notifyMapRemoved set to true.
        ActiveGravEngines.Remove(this);
        Notify_GravEngineCountChanged();
    }

    public override void PostDrawExtraSelectionOverlays()
    {
        if (!parent.Spawned)
            return;

        foreach (var other in ActiveGravEngines)
        {
            if (other != this && parent.Map == other.parent.Map)
                GenDraw.DrawLineBetween(parent.TrueCenter(), other.parent.TrueCenter(), SimpleColor.Red);
        }
    }

    private static void Notify_GravEngineCountChanged()
    {
        ActiveGravEngines.RemoveWhere(x => !x.parent.Spawned);

        foreach (var engine in ActiveGravEngines)
        {
            if (MultipleGravEnginesPresent)
                engine.overlayDrawer?.Enable(engine.parent, VGEDefOf.VGE_MultipleGravEnginesOverlay);
            else
                engine.overlayDrawer?.Disable(engine.parent, VGEDefOf.VGE_MultipleGravEnginesOverlay);
        }
    }

    public override string CompInspectStringExtra()
    {
        if (MultipleGravEnginesPresent)
            return $"{"VGE_GravEngineDisabled".Translate()} {"VGE_MultipleGravEnginesPresent".Translate().CapitalizeFirst()}".Colorize(ColorLibrary.Red);

        return null;
    }
}