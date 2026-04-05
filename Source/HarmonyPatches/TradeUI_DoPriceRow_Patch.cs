using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.GetPriceTooltip))]
    public static class Tradeable_GetPriceTooltip_Patch
    {
        public static void Postfix(Tradeable __instance, TradeAction action, ref string __result)
        {
            if (!__instance.IsGravcoreTradeable(action))
            {
                return;
            }
            int purchasedCount = World_ExposeData_Patch.gravcoresPurchasedCount;
            float modifier = Tradeable_GetPriceFor_Patch.GetGravcorePriceModifier(purchasedCount);
            string buyingMarker = "(" + "Buying".Translate() + ")";
            string gravcoreTaxLine = "\n  x " + modifier.ToString("F2") + " (" + "VGE_GravcoreTax".Translate() + ")";
            int idx = __result.IndexOf(buyingMarker);
            if (idx >= 0)
            {
                __result = __result.Insert(idx + buyingMarker.Length, gravcoreTaxLine);
            }
        }
    }
}
