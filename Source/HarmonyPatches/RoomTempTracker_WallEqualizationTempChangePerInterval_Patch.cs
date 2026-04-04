using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(RoomTempTracker), "WallEqualizationTempChangePerInterval")]
    public static class RoomTempTracker_WallEqualizationTempChangePerInterval_Patch
    {
        private static readonly FieldInfo roomField =
            AccessTools.DeclaredField(typeof(RoomTempTracker), "room");
        private static readonly MethodInfo originalAirTempMethod =
            AccessTools.Method(typeof(GenTemperature), nameof(GenTemperature.TryGetDirectAirTemperatureForCell));
        private static readonly MethodInfo replacementMethod =
            AccessTools.Method(typeof(RoomTempTracker_WallEqualizationTempChangePerInterval_Patch), nameof(TryGetAirTempProtected));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int replacements = 0;
            foreach (var ci in instructions)
            {
                if (ci.Calls(originalAirTempMethod))
                {
                    yield return CodeInstruction.LoadArgument(0);
                    yield return CodeInstruction.LoadArgument(0);
                    yield return new CodeInstruction(OpCodes.Ldfld, roomField);
                    yield return new CodeInstruction(OpCodes.Call, replacementMethod);
                    replacements++;
                }
                else
                {
                    yield return ci;
                }
            }

            if (replacements < 1)
                Log.Error("[VGE] WallEqualizationTempChangePerInterval transpiler: expected at least 1 TryGetDirectAirTemperatureForCell replacement.");
        }

        private static bool TryGetAirTempProtected(IntVec3 c, Map map, out float temperature, RoomTempTracker tracker, Room room)
        {
            if (!GenTemperature.TryGetDirectAirTemperatureForCell(c, map, out temperature))
                return false;

            var cellRoom = c.GetRoom(map);
            if (cellRoom == null || !cellRoom.UsesOutdoorTemperature)
                return true;

            if (GravheatUtility.IsRoomProtectedByGravheatAbsorber(room))
            {
                temperature = tracker.Temperature;
            }
            return true;
        }
    }
}
