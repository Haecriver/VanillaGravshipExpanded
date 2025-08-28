using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GravshipCapturer), "BeginGravshipRender")]
    public static class GravshipCapturer_BeginGravshipRender_Patch
    {
        private static bool _previousShadowState;
        public static void Prefix()
        {
            _previousShadowState = DebugViewSettings.drawShadows;
            DebugViewSettings.drawShadows = false;
        }
        public static void Postfix(ref Action<Capture> ___onComplete)
        {
            ___onComplete = (Action<Capture>)Delegate.Combine(___onComplete, new Action<Capture>(EndCapture));
        }

        public static void EndCapture(Capture capture)
        {
            DebugViewSettings.drawShadows = _previousShadowState;
        }
    }
}
