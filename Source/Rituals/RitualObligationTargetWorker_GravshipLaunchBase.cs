using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

// Inherit from vanilla GravshipLaunch in case some mods care about that. It also handles CanUseTargetInternal method.
// As opposed to the vanilla class, we require this obligation's Def to specify buildings in thingDefs list.
public class RitualObligationTargetWorker_GravshipLaunchBase : RitualObligationTargetWorker_GravshipLaunch
{
    public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
    {
        foreach (var targetDef in def.thingDefs)
        {
            foreach (var thing in map.listerThings.ThingsOfDef(targetDef))
            {
                if (thing.TryGetComp(out CompPilotConsole compPilotConsole) && compPilotConsole.CanUseNow())
                    yield return thing;
            }
        }
    }

    public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
    {
        foreach (var thing in def.thingDefs)
            yield return "VGE_ValidThing".Translate(thing.Named("THING"));
    }
}