using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class AdditionalGravshipComponentTypesExtension : DefModExtension
{
    public List<GravshipComponentTypeDef> additionalComponentTypeDefs = [];

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (additionalComponentTypeDefs is { Count: > 0 })
        {
            for (var i = 0; i < additionalComponentTypeDefs.Count; i++)
            {
                if (additionalComponentTypeDefs[i].requiredForLaunch)
                    Building_GravEngine_MissingComponents_Patch.isPatchActive = true;
                if (additionalComponentTypeDefs[i] == GravshipComponentTypeDefOf.SignalJammer)
                    Building_GravEngine_HasSignalJammer_Patch.isPatchActive = true;
            }
        }
    }
}