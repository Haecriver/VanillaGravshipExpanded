using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class ExtraGravshipDataComp : WorldObjectComp
{
    private Dictionary<IntVec3, Color> vacBarrierRoofColors;
    public IEnumerable<(IntVec3, Color)> VacBarrierRoofColors => ((Gravship)parent).GetRotatedValues(vacBarrierRoofColors);

    public override void Initialize(WorldObjectCompProperties props)
    {
        base.Initialize(props);

        // In constructor, parent is null. We need it to be non-null for extra safety checks, so we use Initialize.

        if (Gravship_CopyCellContents.tempVacBarrierRoofColors != null && Gravship_CopyCellContents.currentGravship == parent && Scribe.mode == LoadSaveMode.Inactive)
            vacBarrierRoofColors = Gravship_CopyCellContents.tempVacBarrierRoofColors;
        else
            vacBarrierRoofColors = new Dictionary<IntVec3, Color>();

        Gravship_CopyCellContents.tempVacBarrierRoofColors = null;
        Gravship_CopyCellContents.currentGravship = null;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Collections.Look(ref vacBarrierRoofColors, nameof(vacBarrierRoofColors), LookMode.Value, LookMode.Value);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos && parent is Gravship gravship && gravship.Engine?.launchInfo?.ExtendedInfo(false) is { } extendedInfo)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEV: Try set forced mishap",
                action = () =>
                {
                    var options = new List<FloatMenuOption>
                    {
                        new("Natural random", () => extendedInfo.forcedMishap = null)
                    };
                    options.AddRange(DefDatabase<LandingOutcomeDef>.AllDefs.Select(def =>
                        new FloatMenuOption((def.label ?? def.defName).CapitalizeFirst(), () =>
                        {
                            extendedInfo.forcedMishap = def;
                            extendedInfo.forcedBoon = null;
                        }))
                    );

                    if (options.Count > 0)
                        Find.WindowStack.Add(new FloatMenu(options));
                    else
                        Messages.Message("No mishaps present in-game.", MessageTypeDefOf.RejectInput, false);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEV: Try set forced boon",
                action = () =>
                {
                    var options = new List<FloatMenuOption>
                    {
                        new("Natural random", () => extendedInfo.forcedBoon = null)
                    };
                    options.AddRange(DefDatabase<LaunchBoonDef>.AllDefs.Select(def =>
                        new FloatMenuOption((def.label ?? def.defName).CapitalizeFirst(), () =>
                        {
                            extendedInfo.forcedMishap = null;
                            extendedInfo.forcedBoon = def;
                        }))
                    );

                    if (options.Count > 0)
                        Find.WindowStack.Add(new FloatMenu(options));
                    else
                        Messages.Message("No boons present in-game.", MessageTypeDefOf.RejectInput, false);
                }
            };
        }
    }
}