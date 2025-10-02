using Verse;

namespace VanillaGravshipExpanded;

public static class ListerThingsUtility
{
    public static bool AnyThingWithDefs(Map map, params ThingDef[] defs)
    {
        if (map == null || defs == null)
            return false;

        for (var i = 0; i < defs.Length; i++)
        {
            var def = defs[i];
            if (def != null && map.listerThings.AnyThingWithDef(def))
                return true;
        }

        return false;
    }

    public static bool AnyThingWithInstallBlueprintDefs(Map map, params ThingDef[] defs)
    {
        if (map == null || defs == null)
            return false;

        for (var i = 0; i < defs.Length; i++)
        {
            var def = defs[i];
            if (def is { installBlueprintDef: not null } && map.listerThings.AnyThingWithDef(def.installBlueprintDef))
                return true;
        }

        return false;
    }

    public static bool AnyThingWithBuildBlueprintDefs(Map map, params ThingDef[] defs)
    {
        if (map == null || defs == null)
            return false;

        for (var i = 0; i < defs.Length; i++)
        {
            var def = defs[i];
            if (def is { blueprintDef: not null } && map.listerThings.AnyThingWithDef(def.blueprintDef))
                return true;
        }

        return false;
    }

    public static bool AnyThingWithFrameDefs(Map map, params ThingDef[] defs)
    {
        if (map == null || defs == null)
            return false;

        for (var i = 0; i < defs.Length; i++)
        {
            var def = defs[i];
            if (def is { frameDef: not null } && map.listerThings.AnyThingWithDef(def.frameDef))
                return true;
        }

        return false;
    }
}