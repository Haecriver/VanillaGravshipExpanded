using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class GravshipsMod : Mod
{
    public const string HarmonyLatePatchCategory = "LatePatch";

    public GravshipsMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("vanillaexpanded.gravship");
        harmony.PatchAllUncategorized();
        // Some patches may need to be loaded later to avoid initializing resources on main thread.
        LongEventHandler.ExecuteWhenFinished(() => harmony.PatchCategory(HarmonyLatePatchCategory));
        settings = GetSettings<GravshipsMod_Settings>();
    }

    public static GravshipsMod_Settings settings;
  
    public override string SettingsCategory()
    {
        return "VE - Gravships";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        settings.DoWindowContents(inRect);
    }

}
