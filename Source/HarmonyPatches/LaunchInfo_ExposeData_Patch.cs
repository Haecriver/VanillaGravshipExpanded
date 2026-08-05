using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(LaunchInfo), nameof(LaunchInfo.ExposeData))]
    public static class LaunchInfo_ExposeData_Patch
    {
        public static Dictionary<LaunchInfo, ExtendedLaunchInfo> extendedLaunchInfos = new Dictionary<LaunchInfo, ExtendedLaunchInfo>();

        public static ExtendedLaunchInfo ExtendedInfo(this LaunchInfo info, bool createIfMissing)
        {
            if (info == null)
                return null;
            if (!extendedLaunchInfos.TryGetValue(info, out var value) && createIfMissing)
                extendedLaunchInfos[info] = value = new ExtendedLaunchInfo();
            return value;
        }

        public static void Postfix(LaunchInfo __instance)
        {
            var extendedInfo = __instance.ExtendedInfo(true);
            // Don't scribe the actual object, as we'd add an extra XML object on top.
            // We want it to be backwards compatible with the previous implementation, so keep it as-is.
            extendedInfo.ExposeData();
        }
    }
}
