using HarmonyLib;
using PipeSystem;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.TotalFuel), MethodType.Getter)]
public static class Building_GravEngine_TotalFuel_Patch
{
    private static void Postfix(Building_GravEngine __instance, ref float __result)
    {
        foreach (var comp in __instance.GravshipComponents)
        {
            if (comp.parent.Spawned && comp.Props.providesFuel && comp.CanBeActive && comp.parent.GetComp<CompResourceStorage>() is {} storage)
                __result += storage.AmountStored;
        }
    }
}