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
                return "VGE_BuildSingleGravEngineOnly".Translate(checkingDef);
            if (GravshipHelper.PlayerHasGravEngineUnderConstruction())
                return "VGE_BuildSingleGravEngineBuildBlueprintOnly".Translate(checkingDef);
        }
        // Disallow installing a duplicate grav engine on the same map
        else
        {
            if (ListerThingsUtility.AnyThingWithDefs(map, ThingDefOf.GravEngine, VGEDefOf.VGE_GravjumperEngine, VGEDefOf.VGE_GravhulkEngine))
                return "VGE_InstallSingleGravEngineOnMap".Translate(thingToIgnore);
            if (GravshipHelper.PlayerHasGravEngineUnderConstruction(map))
                return "VGE_InstallSingleGravEngineBuildBlueprintOnly".Translate(thingToIgnore);
            if (ListerThingsUtility.AnyThingWithInstallBlueprintDefs(map, ThingDefOf.GravEngine, VGEDefOf.VGE_GravjumperEngine, VGEDefOf.VGE_GravhulkEngine))
                return "VGE_InstallSingleGravEngineInstallBlueprintOnly".Translate(thingToIgnore);
        }

        return true;
    }
}