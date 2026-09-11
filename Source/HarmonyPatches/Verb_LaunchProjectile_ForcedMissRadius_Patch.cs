using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch]
    public static class Verb_LaunchProjectile_ForcedMissRadius_Patch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Verb_LaunchProjectile), "TryCastShot");
            yield return AccessTools.Method(typeof(Verb_LaunchProjectile), "HighlightFieldRadiusAroundTarget");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var forcedMissRadiusGetter = AccessTools.PropertyGetter(typeof(VerbProperties), nameof(VerbProperties.ForcedMissRadius));
            var helperMethod = AccessTools.Method(typeof(Verb_LaunchProjectile_ForcedMissRadius_Patch), nameof(GetAdjustedForcedMissRadius));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(forcedMissRadiusGetter))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, helperMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static float GetAdjustedForcedMissRadius(VerbProperties props, Verb verb)
        {
            float baseMiss = props.ForcedMissRadius;
            if (verb != null && verb.caster is Building_GravshipTurret turret)
            {
                return turret.GetLocalForcedMissRadius(baseMiss);
            }
            return baseMiss;
        }
    }
}
