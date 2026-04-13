using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class WorkGiver_HaulToCompactBiosculpterPod : WorkGiver_HaulToBiosculpterPod
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(VGEDefOf.VGE_CompactBiosculpterPod);
}