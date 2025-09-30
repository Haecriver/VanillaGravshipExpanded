using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class PlaceWorker_OnlySingleGravEngine : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        // Only check if thing is null. We don't want to prevent the player from reinstalling the engine they already have.
        if (thing == null)
        {
            if (GravshipUtility.PlayerHasGravEngine())
                return "VGE_SingleGravEngineOnly".Translate(checkingDef);
            if (GravshipHelper.PlayerHasGravEngineUnderConstruction())
                return "VGE_SingleGravEngineBlueprintOnly".Translate(checkingDef);
        }

        return true;
    }
}