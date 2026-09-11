using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public static class TurretLinkerUtility
    {
        public static readonly Texture2D LinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/LinkWithTurret");
        public static readonly Texture2D UnlinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/UnlinkWithTurret");
        public static readonly Texture2D SelectIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/SelectLinkedTurret");

        public static void StartLinking(this ITurretLinker linker, float range)
        {
            var targetingParams = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = t => t.Thing != null
            };
            Find.Targeter.BeginTargeting(targetingParams, t =>
            {
                if (t.Thing is Building_GravshipTurret turret)
                {
                    if (turret.Position.InHorDistOf(linker.LinkerPosition, range))
                    {
                        linker.LinkTo(turret);
                    }
                    else
                    {
                        Messages.Message("VGE_TargetOutOfRange".Translate(), MessageTypeDefOf.RejectInput, false);
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    }
                }
                else
                {
                    Messages.Message(linker.OnlyArtilleryErrorKey.Translate(), MessageTypeDefOf.RejectInput, false);
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
            }, onGuiAction: _ => GenDraw.DrawRadiusRing(linker.LinkerPosition, range));
        }

        public static IEnumerable<Gizmo> GetLinkerGizmos(this ITurretLinker linker, float range)
        {
            int count = 0;
            foreach (var turret in linker.LinkedTurrets)
            {
                count++;
                var currentTurret = turret;
                yield return new Command_Action
                {
                    defaultLabel = "VGE_UnlinkWithTurret".Translate(currentTurret.Label),
                    defaultDesc = linker.UnlinkGizmoDesc,
                    icon = UnlinkIcon,
                    action = currentTurret.Unlink
                };
                yield return new Command_Action
                {
                    defaultLabel = "VGE_SelectLinkedTurret".Translate(currentTurret.Label),
                    defaultDesc = linker.SelectGizmoDesc,
                    icon = SelectIcon,
                    action = delegate
                    {
                        Find.Selector.ClearSelection();
                        Find.Selector.Select(currentTurret);
                    }
                };
            }

            int availableLinks = linker.MaxLinkedTurrets - count;
            for (int i = 0; i < availableLinks; i++)
            {
                yield return new Command_Action
                {
                    defaultLabel = "VGE_LinkWithTurret".Translate(),
                    defaultDesc = linker.LinkGizmoDesc,
                    icon = LinkIcon,
                    action = () => linker.StartLinking(range)
                };
            }
        }
    }
}
