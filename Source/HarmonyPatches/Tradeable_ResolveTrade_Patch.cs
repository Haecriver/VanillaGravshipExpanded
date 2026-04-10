using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.ResolveTrade))]
    public static class Tradeable_ResolveTrade_Patch
    {
        public static void Prefix(Tradeable __instance)
        {
            if (__instance.IsGravcoreTradeable())
            {
                World_ExposeData_Patch.gravcoresPurchasedCount += Mathf.Abs(__instance.CountToTransfer);
            }
        }

        public static bool IsGravcoreTradeable(this Tradeable __instance, TradeAction action = TradeAction.None)
        {
            if (action == TradeAction.None)
            {
                action = __instance.ActionToDo;
            }
            return action != TradeAction.PlayerSells && __instance.FirstThingTrader?.def == ThingDefOf.Gravcore;
        }
    }
}
