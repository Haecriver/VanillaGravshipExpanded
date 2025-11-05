using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class CompGravEngineInspectable : ThingComp
{
    public CompProperties_GravEngineInspectable Props => (CompProperties_GravEngineInspectable)props;

    public override bool CompPreventClaimingBy(Faction faction)
    {
        // Prevent claiming without inspecting first.
        // Preferably, we'd leave a message like in vanilla. But comps can't do that, as they don't return an AcceptanceReport.
        return !Find.ResearchManager.gravEngineInspected || base.CompPreventClaimingBy(faction);
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        foreach (var option in base.CompFloatMenuOptions(selPawn))
            yield return option;

        if (!Find.ResearchManager.gravEngineInspected)
        {
            yield return new FloatMenuOption("CommandInspectGravEngine".Translate(parent),
                () => selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VGEDefOf.VGE_InspectMechanoidGravEngine, parent)));
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (!parent.Spawned)
                yield break;

            if (!Find.ResearchManager.gravEngineInspected)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandInspectGravEngine".Translate(parent),
                    defaultDesc = "CommandInspectGravEngineDesc".Translate(),
                    icon = Building_GravEngine.InspectCommandTex,
                    action = () =>
                    {
                        Find.Targeter.BeginTargeting(TargetingParameters.ForColonist(),
                            targ => (targ.Thing as Pawn)?.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VGEDefOf.VGE_InspectMechanoidGravEngine, parent)),
                            targ =>
                            {
                                if (ValidateInspectionTarget(targ))
                                    GenDraw.DrawTargetHighlight(targ);
                            },
                            ValidateInspectionTarget,
                            mouseAttachment: Building_GravEngine.InspectCommandTex,
                            onGuiAction: _ => Widgets.MouseAttachedLabel("ChooseWhoShouldInspect".Translate(parent)));
                    },
                };
                if (DebugSettings.ShowDevGizmos)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Inspect now",
                        action = () => GravshipHelper.InspectGravEngine(parent, questTags: parent.questTags, setToPlayerFaction: Props.setToPlayerFaction),
                    };
                }
            }

            bool ValidateInspectionTarget(LocalTargetInfo targ)
                => targ.Thing is Pawn { IsColonistPlayerControlled: true, Downed: false } pawn &&
                   pawn.CanReach(pawn.Position, PathEndMode.Touch, Danger.Deadly) &&
                   pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving);
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();

        if (parent.Spawned && !Find.ResearchManager.gravEngineInspected)
            return new StringBuilder(text).AppendInNewLine("GravEngineNotInspected".Translate()).ToString();
        return text;
    }
}