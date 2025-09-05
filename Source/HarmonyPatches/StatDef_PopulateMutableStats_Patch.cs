using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(StatDef), nameof(StatDef.PopulateMutableStats))]
public class StatDef_PopulateMutableStats_Patch
{
    private static void Postfix()
    {
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            var props = thingDef.GetCompProperties<CompProperties_ConstantGravshipFacilityBonus>();
            if (props is { statOffsets: not null })
                StatDef.mutableStats.AddRange(props.statOffsets.Select(mod => mod.stat));
        }
    }
}