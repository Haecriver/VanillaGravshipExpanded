using RimWorld;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded;

public static class GravshipResearchUtility
{
    public static bool IsGravshipResearch(this ResearchProjectDef project) => project != null && (project.tab?.HasModExtension<GravtechResearchExtension>() == true || project.HasModExtension<GravtechResearchExtension>());

    public static bool SetGravshipResearch(this ResearchProjectDef project, bool playSound = true, bool sendMessage = true)
    {
        if (!project.IsGravshipResearch())
            return false;

        if (playSound)
            SoundDefOf.ResearchStart.PlayOneShotOnCamera();
        World_ExposeData_Patch.currentGravtechProject = project;
        if (sendMessage)
            Messages.Message("VGE_GravtechNeedsGravdata".Translate(project.LabelCap), MessageTypeDefOf.NeutralEvent);

        return true;
    }
}