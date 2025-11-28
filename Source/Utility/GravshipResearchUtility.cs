using RimWorld;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded;

public static class GravshipResearchUtility
{
    public static bool IsGravshipResearch(this ResearchProjectDef project) => project != null && (project.tab.IsGravshipResearchTab() || project.HasModExtension<GravtechResearchExtension>());

    public static bool IsGravshipResearchTab(this ResearchTabDef tab) => tab != null && tab.HasModExtension<GravtechResearchExtension>();

    public static bool SetGravshipResearch(this ResearchProjectDef project, bool playSound = true, bool sendMessage = true)
    {
        if (!project.IsGravshipResearch())
            return false;
        if (project.PrerequisitesCompleted is false)
            return true;
        if (playSound)
            SoundDefOf.ResearchStart.PlayOneShotOnCamera();
        World_ExposeData_Patch.currentGravtechProject = project;
        if (sendMessage)
            Messages.Message("VGE_GravtechNeedsGravdata".Translate(project.LabelCap), MessageTypeDefOf.NeutralEvent);

        return true;
    }

    public static void ResearchPerformed(float amount, Pawn actor = null)
    {
        if (World_ExposeData_Patch.currentGravtechProject == null)
            return;

        if (World_ExposeData_Patch.currentGravtechProject.PrerequisitesCompleted || World_ExposeData_Patch.currentGravtechProject.IsFinished)
            Find.ResearchManager.AddProgress(World_ExposeData_Patch.currentGravtechProject, amount, actor);
        else
            World_ExposeData_Patch.currentGravtechProject = null;
    }
}
