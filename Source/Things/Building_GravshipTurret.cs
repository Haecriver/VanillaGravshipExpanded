using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VEF.Graphics;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Building_GravshipTurret : Building_TurretGun
    {
        private float curAngle;
        private float rotationSpeed;
        private float rotationVelocity;
        private int barrelIndex = -1;
        private List<Vector3> barrels;
        public ITurretLinker linkedTerminal;
        private CustomOverlayDrawer overlayDrawer;
        public bool unlinking;
        private static readonly Texture2D ForceTargetIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/GravshipArtilleryForceTarget");
        private static readonly Texture2D HoldFireIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/GravshipArtilleryHoldFire");
        private static readonly Texture2D LinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/LinkWithTerminal");
        private static readonly Texture2D UnlinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/UnlinkWithTerminal");
        private static readonly Texture2D SelectIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/SelectLinkedTerminal");
        
        public bool permanentlyDisabled;
        public void DisablePermanently()
        {
            permanentlyDisabled = true;
            currentTargetInt = LocalTargetInfo.Invalid;
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }

        public virtual bool CanFire => !permanentlyDisabled && (linkedTerminal?.MannedByPlayer ?? false);

        public virtual bool CanAutoAttack => false;
        public Pawn ManningPawn => linkedTerminal?.ManningPawn;

        public virtual float GravshipTargeting => linkedTerminal?.GravshipTargeting ?? 0f;

        protected virtual bool ShowNoLinkedTerminalOverlay => true;

        public Vector3 CastSource
        {
            get
            {
                if (barrels != null)
                {
                    if (barrelIndex < 0)
                    {
                        barrelIndex = 0;
                    }
                    var result = DrawPos + barrels[barrelIndex].RotatedBy(top.CurRotation);
                    return result;
                }
                return DrawPos;
            }
        }

        public static Vector3 GetCastSource(Thing thing) => thing is Building_GravshipTurret turret ? turret.CastSource : thing.DrawPos;

        public void TrySwitchBarrel()
        {
            if (barrels != null)
            {
                barrelIndex = (barrelIndex + 1) % barrels.Count;
            }
        }

        public override bool CanSetForcedTarget => !permanentlyDisabled && linkedTerminal != null && linkedTerminal.MannedByPlayer;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            var ext = def.GetModExtension<TurretExtension_RotationSpeed>();
            if (ext != null)
            {
                rotationSpeed = ext.rotationSpeed;
            }

            var barrelExt = def.GetModExtension<TurretExtension_Barrels>();
            if (barrelExt != null)
            {
                barrels = barrelExt.barrels;
            }

            overlayDrawer = map.GetComponent<CustomOverlayDrawer>();
            if (linkedTerminal == null && ShowNoLinkedTerminalOverlay)
            {
                EnableOverlay();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            overlayDrawer = null;
        }

        protected float angleDiff;
        public override void Tick()
        {
            base.Tick();
            if (linkedTerminal != null)
            {
                var linkerThing = linkedTerminal.LinkerThing;
                if (linkerThing is null || linkerThing.Destroyed || !linkedTerminal.LinkerThing.Spawned && linkedTerminal is not Apparel)
                {       
                    Unlink();
                }
            }

            if (rotationSpeed > 0)
            {
                if (CanFire && CurrentTarget.IsValid && Active && AttackVerb.Available())
                {
                    var targetAngle = (CurrentTarget.Cell.ToVector3Shifted() - DrawPos).AngleFlat();
                    if (targetAngle < 0)
                    {
                        targetAngle += 360;
                    }
                    curAngle = top.CurRotation = Mathf.SmoothDampAngle(curAngle, targetAngle, ref rotationVelocity, 0.01f, rotationSpeed, 1f / GenTicks.TicksPerRealSecond);
                    if (curAngle < 0)
                    {
                        curAngle += 360;
                    }
                    else if (curAngle > 360)
                    {
                        curAngle -= 360;
                    }
                    angleDiff = Mathf.Min(Mathf.Abs(curAngle - targetAngle), 360 - Mathf.Abs(curAngle - targetAngle));
                    if (angleDiff > 0.1f && AttackVerb.state != VerbState.Bursting && burstCooldownTicksLeft <= 0)
                    {
                        burstWarmupTicksLeft++;
                    }
                }
                else
                {
                    curAngle = top.CurRotation;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rotationVelocity, "rotationVelocity");
            Scribe_Values.Look(ref barrelIndex, "barrelIndex", -1);
            Scribe_References.Look(ref linkedTerminal, "linkedTerminal");
            Scribe_Values.Look(ref curAngle, "curAngle");
            Scribe_Values.Look(ref permanentlyDisabled, "permanentlyDisabled");
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (permanentlyDisabled)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }
                text += "VGE_PermanentlyDisabled".Translate();
            }
            else if (ShowNoLinkedTerminalOverlay && Faction == Faction.OfPlayer && linkedTerminal == null)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }
                text += "VGE_NeedsLinkedTargetingTerminal".Translate();
            }
            return text;
        }

        public float GetLocalForcedMissRadius(float baseMissRadius)
        {
        	return GravshipHelper.CalculateAdjustedForcedMissRadius(baseMissRadius, this.Map, this.def, this.Position, this.Faction, this.GravshipTargeting, useMapMultiplier: true);
        }

        public void LinkTo(ITurretLinker terminal)
        {
            if (linkedTerminal == terminal)
            {
                return;
            }
            if (linkedTerminal != null && linkedTerminal != terminal)
            {
                linkedTerminal.Unlink(this);
            }
            linkedTerminal = terminal;

            if (terminal != null && !terminal.LinkedTurrets.Contains(this))
            {
                terminal.LinkTo(this);
            }

            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            DisableOverlay();
            linkedTerminal.DisableOverlay();
        }

        public void Unlink()
        {
            var prevTerminal = linkedTerminal;
            linkedTerminal = null;
            if (prevTerminal != null && !unlinking)
            {
                prevTerminal.Unlink(this);
            }
            else
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                if (ShowNoLinkedTerminalOverlay)
                {
                    EnableOverlay();
                }
            }
        }

        private void SelectLinkedTerminal()
        {
            if (linkedTerminal != null && linkedTerminal.LinkerThing != null)
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(linkedTerminal.LinkerThing);
            }
            else if (linkedTerminal != null)
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select((Thing)linkedTerminal);
            }
        }
        private void StartLinking()
        {
            var targetingParameters = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo t) => t.Thing is Building_TargetingTerminal && t.Thing.Position.InHorDistOf(this.Position, 36)
            };
            Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo t)
            {
                var terminal = t.Thing as Building_TargetingTerminal;
                LinkTo(terminal);
            }, onGuiAction: delegate { GenDraw.DrawRadiusRing(this.Position, 36f); });
        }

        public override LocalTargetInfo TryFindNewTarget()
        {
            if (permanentlyDisabled) return LocalTargetInfo.Invalid;
            return base.TryFindNewTarget();
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            if (permanentlyDisabled) return;
            base.OrderAttack(targ);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (linkedTerminal != null && linkedTerminal.LinkerThing != null && linkedTerminal.LinkerThing.Spawned)
            {
                GenDraw.DrawLineBetween(this.TrueCenter(), linkedTerminal.LinkerThing.DrawPos, SimpleColor.White);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if (gizmo is Command_VerbTarget command && command.defaultLabel == "CommandSetForceAttackTarget".Translate())
                {
                    command.icon = ForceTargetIcon;
                    if (linkedTerminal is Apparel)
                    {
                        command.Disable("VGE_MustBeAimedViaEquippedTargeter".Translate());
                    }
                    else if (!CanFire)
                    {
                        command.Disable("VGE_NeedsMannedTargetingTerminal".Translate());
                    }
                }
                else if (gizmo is Command_Toggle command2 && command2.defaultLabel == "CommandHoldFire".Translate())
                {
                    command2.icon = HoldFireIcon;
                }
                yield return gizmo;
            }

            if (Faction != Faction.OfPlayer || permanentlyDisabled)
            {
                yield break;
            }

            if (linkedTerminal == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "VGE_LinkWithTerminal".Translate(),
                    defaultDesc = "VGE_LinkWithTerminalDesc".Translate(),
                    icon = LinkIcon,
                    action = delegate { StartLinking(); }
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "VGE_UnlinkWithTerminal".Translate(),
                    defaultDesc = "VGE_UnlinkWithTerminalDesc".Translate(),
                    icon = UnlinkIcon,
                    action = delegate { Unlink(); }
                };
                yield return new Command_Action
                {
                    defaultLabel = "VGE_SelectLinkedTerminal".Translate(),
                    defaultDesc = "VGE_SelectLinkedTerminalDesc".Translate(),
                    icon = SelectIcon,
                    action = delegate { SelectLinkedTerminal(); }
                };
            }
        }

        public void EnableOverlay() => overlayDrawer?.Enable(this, VGEDefOf.VGE_NoLinkedTerminalOverlay);

        public void DisableOverlay() => overlayDrawer?.Disable(this, VGEDefOf.VGE_NoLinkedTerminalOverlay);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
