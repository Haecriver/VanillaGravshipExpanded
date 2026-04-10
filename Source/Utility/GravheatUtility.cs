using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public static class GravheatUtility
    {
        public static bool IsRoomProtectedByGravheatAbsorber(Room room)
        {
            if (room == null || room.UsesOutdoorTemperature)
                return false;

            var map = room.Map;
            if (map == null || !map.Biome.inVacuum)
                return false;

            if (!map.gameConditionManager.ConditionIsActive(VGEDefOf.VGE_SpaceSolarFlare))
                return false;

            foreach (var thing in map.listerThings.ThingsOfDef(VGEDefOf.VGE_GravheatAbsorber))
            {
                var absorber = thing.TryGetComp<CompGravheatAbsorber>();
                if (absorber != null && absorber.CanBeOn(out Building_GravEngine engine)
                    && engine.ValidSubstructure.Contains(room.Cells.First()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
