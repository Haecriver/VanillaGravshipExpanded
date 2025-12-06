using System.Collections.Generic;
using RimWorld;
using UnityEngine;
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
        else if (thing.Map != map)
        {
            if (ListerThingsUtility.AnyThingWithDefs(map, GravshipHelper.GravEngineDefs))
                return "VGE_InstallSingleGravEngineOnMap".Translate(thing);
            if (GravshipHelper.PlayerHasGravEngineUnderConstruction(map))
                return "VGE_InstallSingleGravEngineBuildBlueprintOnly".Translate(thing);
            if (ListerThingsUtility.AnyThingWithInstallBlueprintDefs(map, GravshipHelper.GravEngineDefs))
                return "VGE_InstallSingleGravEngineInstallBlueprintOnly".Translate(thing);
        }

        return true;
    }

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var map = Find.CurrentMap;
        if (map == null)
            return;
        // Always allow reinstalling an engine, unless across maps
        if (thing != null && thing.Map != map)
            return;

        var list = SimplePool<List<Thing>>.Get();

        foreach (var gravEngineDef in GravshipHelper.GravEngineDefs)
        {
            foreach (var engine in map.listerThings.ThingsOfDef(gravEngineDef))
                DrawLine(engine);
            if (gravEngineDef.frameDef != null)
            {
                foreach (var engine in map.listerThings.ThingsOfDef(gravEngineDef.frameDef))
                    DrawLine(engine);
            }
            if (gravEngineDef.blueprintDef != null)
            {
                foreach (var engine in map.listerThings.ThingsOfDef(gravEngineDef.blueprintDef))
                    DrawLine(engine);
            }
            if (gravEngineDef.minifiedDef != null && gravEngineDef.minifiedDef != ThingDefOf.MinifiedThing)
            {
                foreach (var engine in map.listerThings.ThingsOfDef(gravEngineDef.minifiedDef))
                    DrawLine(engine);

                list.Clear();
                ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForDef(gravEngineDef.minifiedDef), list, alsoGetSpawnedThings: false);
                foreach (var engine in list)
                    DrawLine(engine);
            }
        }

        list.Clear();
        SimplePool<List<Thing>>.Return(list);

        // Should be called (at most) once (unless dev mode or other shenanigans), so no need to cache the call to ToVector3Shifted
        void DrawLine(Thing engine) => GenDraw.DrawLineBetween(center.ToVector3Shifted(), engine.TrueCenter(), SimpleColor.Red);
    }
}