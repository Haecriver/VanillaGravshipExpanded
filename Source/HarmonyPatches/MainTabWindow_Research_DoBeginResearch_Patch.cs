using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.DoBeginResearch))]
    public static class MainTabWindow_Research_DoBeginResearch_Patch
    {
        public static bool Prefix(ResearchProjectDef projectToStart)
        {
            if (projectToStart?.tab == VGEDefOf.VGE_Gravtech)
            {
                SoundDefOf.ResearchStart.PlayOneShotOnCamera();
                projectToStart.SetGravshipResearch();
                return false;
            }
            return true;
        }

        public static void SetGravshipResearch(this ResearchProjectDef gravTech)
        {
            World_ExposeData_Patch.currentGravtechProject = gravTech;
            Messages.Message("VGE_GravtechNeedsGravdata".Translate(gravTech.LabelCap), MessageTypeDefOf.NeutralEvent);
        }
    }
}
