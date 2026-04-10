using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace VanillaGravshipExpanded
{
    public class CompProperties_GravheatAbsorber : CompProperties
    {
        public int cooldownTicks = 900000;
        public float heatPushedPerSecond = 21f;
        public CompProperties_GravheatAbsorber()
        {
            compClass = typeof(CompGravheatAbsorber);
        }
    }
    [StaticConstructorOnStartup]
    [HotSwappable]
    public class CompGravheatAbsorber : CompFacilityConnected, IThingGlower
    {
        public CompProperties_GravheatAbsorber Props => props as CompProperties_GravheatAbsorber;
        private int cooldownEndTick = -1;
        private bool isAbsorbing = false;
        private static readonly Texture2D GizmoIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/GravheatAbsorber");

        private Graphic cooldownGraphic;
        public Graphic CooldownGraphic => cooldownGraphic ??= GraphicDatabase.Get<Graphic_Multi>(parent.Graphic.path + "_Cooldown", parent.Graphic.Shader, parent.Graphic.drawSize, parent.Graphic.color);
        public bool IsOnCooldown => Find.TickManager.TicksGame < cooldownEndTick;
        public bool IsAbsorbing => isAbsorbing;
        public CompGlower glower;

        public bool ShouldBeLitNow() => IsOnCooldown;

        public override void PostPostMake()
        {
            base.PostPostMake();
            InitializeComps();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownEndTick, "cooldownEndTick", -1);
            Scribe_Values.Look(ref isAbsorbing, "isAbsorbing");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitializeComps();
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (isAbsorbing && parent.Spawned)
            {
                var room = parent.Position.GetRoom(parent.Map);
                if (room != null)
                {
                    room.PushHeat(Props.heatPushedPerSecond * delta / 60f);
                }
                if (Find.TickManager.TicksGame >= cooldownEndTick)
                {
                    isAbsorbing = false;
                    glower.UpdateLit(parent.Map);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (parent.Spawned)
            {
                if (!IsOnCooldown)
                {
                    var absorbGizmo = new Command_Action
                    {
                        defaultLabel = "VGE_AbsorbGravheat".Translate(),
                        defaultDesc = "VGE_AbsorbGravheatDesc".Translate(),
                        icon = GizmoIcon,
                        action = AbsorbGravheat
                    };

                    var heatManager = FindHeatManager();
                    if (heatManager == null)
                    {
                        absorbGizmo.Disable("VGE_NoGravEngine".Translate());
                    }
                    else
                    {
                        var gravEngine = heatManager.Engine;
                        if (gravEngine.cooldownCompleteTick <= Find.TickManager.TicksGame)
                        {
                            absorbGizmo.Disable("VGE_NoCooldownToAbsorb".Translate());
                        }
                    }

                    yield return absorbGizmo;
                }
                if (DebugSettings.ShowDevGizmos)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Reset cooldown",
                        defaultDesc = "Reset the cooldown timer to 0",
                        action = () =>
                        {
                            cooldownEndTick = Find.TickManager.TicksGame;
                            glower.UpdateLit(parent.Map);
                        }
                    };

                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Stop absorption",
                        defaultDesc = "Stop current heat absorption",
                        action = () => isAbsorbing = false
                    };
                }
            }
        }

        private void AbsorbGravheat()
        {
            var heatManager = FindHeatManager();
            if (heatManager == null)
                return;

            heatManager.ClearGravEngineHeat();
            ResetGravshipCooldown();
            cooldownEndTick = Find.TickManager.TicksGame + Props.cooldownTicks;
            isAbsorbing = true;
            glower.UpdateLit(parent.Map);
        }

        private CompHeatManager FindHeatManager()
        {
            if (!CanBeOn(out Building_GravEngine engine))
                return null;

            return engine?.GetComp<CompHeatManager>();
        }

        private void ResetGravshipCooldown()
        {
            if (!CanBeOn(out Building_GravEngine engine))
                return;

            var heatManager = engine?.GetComp<CompHeatManager>();
            if (heatManager == null)
                return;

            var gravEngine = heatManager.Engine;
            gravEngine.cooldownCompleteTick = Find.TickManager.TicksGame;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (IsOnCooldown)
            {
                CooldownGraphic.Draw(parent.DrawPos + new Vector3(0f, 0.1f, 0f), parent.Rotation, parent);
            }
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            if (parent.Spawned && parent.Map.gameConditionManager.ConditionIsActive(VGEDefOf.VGE_SpaceSolarFlare))
            {
                sb.AppendLine("VGE_ProtectingFromSpaceSolarFlare".Translate());
            }
            if (IsOnCooldown)
            {
                int ticksRemaining = cooldownEndTick - Find.TickManager.TicksGame;
                sb.AppendLine("VGE_GravheatAbsorberCoolingDown".Translate(ticksRemaining.ToStringTicksToDays()));
            }
            return sb.ToString().TrimEndNewlines();
        }

        private new void InitializeComps()
        {
            // Called from PostPostMake and PostExposeData, as PostSpawnSetup won't be called for something that never
            // spawned yet. And it would get called each time it's spawned, and we don't need to initialize comps each time.
            glower = parent.GetComp<CompGlower>();
        }
    }
}
