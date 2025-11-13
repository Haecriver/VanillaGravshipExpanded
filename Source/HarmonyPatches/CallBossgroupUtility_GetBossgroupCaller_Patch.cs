using System.Reflection;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CallBossgroupUtility), nameof(CallBossgroupUtility.GetBossgroupCaller))]
public static class CallBossgroupUtility_GetBossgroupCaller_Patch
{
    public static ThingDef temporaryBossgroupCallerOverride;

    // Only apply this patch if we have Biotech active
    private static bool Prepare(MethodBase method) => method != null || ModsConfig.BiotechActive;

    private static bool Prefix(ref ThingDef __result)
    {
        if (temporaryBossgroupCallerOverride == null)
            return true;

        __result = temporaryBossgroupCallerOverride;
        return false;
    }
}