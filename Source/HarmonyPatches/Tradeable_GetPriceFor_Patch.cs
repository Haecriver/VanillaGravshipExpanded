using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.GetPriceFor))]
    public static class Tradeable_GetPriceFor_Patch
    {
        public static void Postfix(Tradeable __instance, TradeAction action, ref float __result)
        {
            if (__instance.IsGravcoreTradeable())
            {
                int purchasedCount = World_ExposeData_Patch.gravcoresPurchasedCount;
                float priceModifier = GetGravcorePriceModifier(purchasedCount);
                __result *= priceModifier;
            }
        }

        public static float GetGravcorePriceModifier(int purchasedCount)
        {
            return purchasedCount switch
            {
                0 => 1.5f,
                1 => 2.0f,
                2 => 2.5f,
                3 => 3.0f,
                4 => 3.5f,
                5 => 4.0f,
                6 => 5.0f,
                7 => 6.0f,
                8 => 7.0f,
                _ => 8.0f
            };
        }
    }
}
