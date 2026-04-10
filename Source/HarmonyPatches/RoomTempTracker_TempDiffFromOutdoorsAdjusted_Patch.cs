using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(RoomTempTracker), "TempDiffFromOutdoorsAdjusted")]
    public static class RoomTempTracker_TempDiffFromOutdoorsAdjusted_Patch
    {
        private static readonly FieldInfo roomField =
            AccessTools.DeclaredField(typeof(RoomTempTracker), "room");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var outdoorTempGetter = AccessTools.PropertyGetter(typeof(MapTemperature), nameof(MapTemperature.OutdoorTemp));
            var replacement = AccessTools.Method(typeof(RoomTempTracker_TempDiffFromOutdoorsAdjusted_Patch), nameof(GetProtectedOutdoorTemp));

            int replacements = 0;
            foreach (var ci in instructions)
            {
                if (ci.Calls(outdoorTempGetter))
                {
                    yield return CodeInstruction.LoadArgument(0);
                    yield return new CodeInstruction(OpCodes.Ldfld, roomField);
                    yield return new CodeInstruction(OpCodes.Call, replacement);
                    replacements++;
                }
                else
                {
                    yield return ci;
                }
            }

            if (replacements < 1)
                Log.Error("[VGE] TempDiffFromOutdoorsAdjusted transpiler: expected at least 1 OutdoorTemp replacement.");
        }

        private static float GetProtectedOutdoorTemp(MapTemperature mapTemp, Room room)
        {
            if (room == null || room.UsesOutdoorTemperature)
                return mapTemp.OutdoorTemp;

            if (GravheatUtility.IsRoomProtectedByGravheatAbsorber(room))
            {
                return room.Temperature;
            }
            return mapTemp.OutdoorTemp;
        }
    }
}
