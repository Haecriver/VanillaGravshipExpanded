using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.EqualizeTemperaturesThroughBuilding))]
    public static class GenTemperature_EqualizeTemperaturesThroughBuilding_Patch
    {
        private static readonly MethodInfo roomTempSetter =
            AccessTools.PropertySetter(typeof(Room), nameof(Room.Temperature));
        private static readonly MethodInfo replacementMethod =
            AccessTools.Method(typeof(GenTemperature_EqualizeTemperaturesThroughBuilding_Patch), nameof(SetTemperatureIfNotProtected));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int replacements = 0;
            foreach (var ci in instructions)
            {
                if (ci.Calls(roomTempSetter))
                {
                    yield return new CodeInstruction(OpCodes.Call, replacementMethod);
                    replacements++;
                }
                else
                {
                    yield return ci;
                }
            }

            if (replacements < 1)
                Log.Error("[VGE] EqualizeTemperaturesThroughBuilding transpiler: expected at least 1 set_Temperature replacement.");
        }

        private static void SetTemperatureIfNotProtected(Room room, float newTemp)
        {
            if (room == null || room.UsesOutdoorTemperature)
            {
                room.Temperature = newTemp;
                return;
            }

            var map = room.Map;
            if (map == null || !map.Biome.inVacuum || !map.gameConditionManager.ConditionIsActive(VGEDefOf.VGE_SpaceSolarFlare))
            {
                room.Temperature = newTemp;
                return;
            }

            var hasOutdoorRoom = GenTemperature.beqRooms.Any(r => r.UsesOutdoorTemperature);
            if (!hasOutdoorRoom || !GravheatUtility.IsRoomProtectedByGravheatAbsorber(room))
            {
                room.Temperature = newTemp;
            }
        }
    }
}
