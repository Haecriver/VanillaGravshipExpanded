
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
namespace VanillaGravshipExpanded
{
    public static class AstrofireUtility
    {

        private static readonly List<Astrofire> fireList = new List<Astrofire>();

        public static bool IsAstroBurning(this TargetInfo t)
        {
            if (t.HasThing)
            {
                return t.Thing.IsAstroBurning();
            }
            return t.Cell.ContainsStaticAstrofire(t.Map);
        }

        public static bool IsAstroBurning(this Thing t)
        {
            if (t.Destroyed || !t.Spawned)
            {
                return false;
            }
            if (t.def.size == IntVec2.One)
            {
                if (t is Pawn)
                {
                    return t.HasAttachment(VGEDefOf.VGE_Astrofire);
                }
                return t.Position.ContainsStaticAstrofire(t.Map);
            }
            foreach (IntVec3 item in t.OccupiedRect())
            {
                if (item.ContainsStaticAstrofire(t.Map))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsStaticAstrofire(this IntVec3 c, Map map)
        {
            List<Thing> list = map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                Astrofire fire = list[i] as Astrofire;
                if (fire != null && fire.parent == null)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Astrofire> GetAstrofiresNearCell(this IntVec3 cell, Map map)
        {
            fireList.Clear();
            Room room = RegionAndRoomQuery.RoomAt(cell, map);
            if (room == null || room.Dereferenced || room.Fogged || room.IsHuge || room.TouchesMapEdge)
            {
                Region region = cell.GetRegion(map);
                if (region == null)
                {
                    List<Thing> list = map.thingGrid.ThingsListAt(cell);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Astrofire fire = list[i] as Astrofire;
                        if (fire != null && fire.parent == null)
                        {
                            fireList.Add(fire);
                        }
                    }
                }
                else
                {
                    region.ListerThings.GetThingsOfType(fireList);
                }
            }
            else
            {
                List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
                for (int j = 0; j < containedAndAdjacentThings.Count; j++)
                {
                    Astrofire fire2 = containedAndAdjacentThings[j] as Astrofire;
                    if (fire2 != null)
                    {
                        fireList.Add(fire2);
                    }
                }
            }
            fireList.Shuffle();
            fireList.Swap(0, fireList.FindIndex(0, (Astrofire f) => f.Position == cell));
            return fireList;
        }
    }


}
