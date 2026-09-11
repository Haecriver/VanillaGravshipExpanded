using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Dialog_NamePlayerGravship), MethodType.Constructor, typeof(Building_GravEngine))]
public class Dialog_NamePlayerGravship_Ctor_Patch
{
    private static void Postfix(Dialog_NamePlayerGravship __instance)
    {
        if (!World_ExposeData_Patch.lastGravshipName.NullOrEmpty() && __instance.IsValidName(World_ExposeData_Patch.lastGravshipName))
            __instance.curName = World_ExposeData_Patch.lastGravshipName;
    }
}