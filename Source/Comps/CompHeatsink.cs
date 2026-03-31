using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class CompProperties_Heatsink : CompProperties
    {
        public float maxHeat = 10f;
        public float heatConsumptionPerHour = 1f;
        public float powerGenerated = 100f;
        public float cooldownReductionPercent;
        public float heatPushedPerSecond = 21f;
        public List<HeatUpgrades> upgrades;
        [Unsaved]
        public CachedHeatsinkStats cachedStats = null;

        public CompProperties_Heatsink()
        {
            compClass = typeof(CompHeatsink);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);

            // Clear cached stats on research finished, in case research changes stats of heatsinks
            ResearchManager_FinishProject_Patch.allHeatsinkComps.Add(this);
        }

        public class HeatUpgrades
        {
            public ResearchProjectDef researchProject;
            public float maxHeatFactor = 1f;
            public float heatConsumptionPerHourFactor = 1f;
            public float cooldownReductionPercentFactor = 1f;
            public float heatPushedPerSecondFactor = 1f;
        }

        public class CachedHeatsinkStats
        {
            public readonly float maxHeat;
            public readonly float heatConsumptionPerHour;
            public readonly float powerGenerated;
            public readonly float cooldownReductionPercent;
            public readonly float heatPushedPerSecond;

            public CachedHeatsinkStats(CompProperties_Heatsink heatsink, CompProperties_Power powerTrader)
            {
                maxHeat = heatsink.maxHeat;
                heatConsumptionPerHour = heatsink.heatConsumptionPerHour;
                powerGenerated = heatsink.powerGenerated;
                cooldownReductionPercent = heatsink.cooldownReductionPercent;
                heatPushedPerSecond = heatsink.heatPushedPerSecond;

                if (powerTrader?.powerUpgrades != null)
                {
                    for (var i = 0; i < powerTrader.powerUpgrades.Count; i++)
                    {
                        var upgrade = powerTrader.powerUpgrades[i];
                        if (upgrade?.researchProject is { IsFinished: true })
                            powerGenerated *= upgrade.factor;
                    }
                }

                if (heatsink.upgrades != null)
                {
                    for (var i = 0; i < heatsink.upgrades.Count; i++)
                    {
                        var upgrade = heatsink.upgrades[i];
                        if (upgrade?.researchProject is { IsFinished: true })
                        {
                            maxHeat *= upgrade.maxHeatFactor;
                            heatConsumptionPerHour *= upgrade.heatConsumptionPerHourFactor;
                            cooldownReductionPercent *= upgrade.cooldownReductionPercentFactor;
                            heatPushedPerSecond *= upgrade.heatPushedPerSecondFactor;
                        }
                    }
                }
            }
        }
    }

    [HotSwappable]
    [StaticConstructorOnStartup]
    public class CompHeatsink : CompFacilityConnected
    {
        public CompProperties_Heatsink Props => props as CompProperties_Heatsink;

        protected float storedHeat;
        protected CompPowerTrader powerComp;
        public CompProperties_Heatsink.CachedHeatsinkStats CachedStats => Props.cachedStats ??= new CompProperties_Heatsink.CachedHeatsinkStats(Props, powerComp?.Props);

        public float StoredHeat => storedHeat;
        public float EffectiveMaxHeat => CachedStats.maxHeat * CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier;
        public bool IsActive => StoredHeat < EffectiveMaxHeat && (powerComp?.PowerOn ?? false) && CanBeOn(out _);
        private Graphic overlayGraphic;
        public Graphic OverlayGraphic => overlayGraphic ??= GraphicDatabase.Get<Graphic_Multi>(parent.Graphic.path + "_Overlay", parent.Graphic.Shader, parent.Graphic.drawSize, parent.Graphic.color);
        public CompGlower glower;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();
            glower = parent.GetComp<CompGlower>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref storedHeat, "storedHeat");
        }


        public void AddHeat(float amount)
        {
            storedHeat = Mathf.Min(storedHeat + amount, CachedStats.maxHeat * CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier);
            UpdateLit();
        }

        public void ClearHeat()
        {
            storedHeat = 0;
            UpdateLit();
        }

        public float ActualStoredHeat => storedHeat / (CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier);

        private void UpdateLit()
        {
            if (parent.Map != null)
            {
                glower.UpdateLit(parent.Map);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Map is null) return;

            if (storedHeat <= 0 || !CanBeOn(out _))
            {
                powerComp?.PowerOutput = powerComp.Props.basePowerConsumption;
                return;
            }

            if (powerComp == null || powerComp.PowerOn)
            {
                float heatToConsume = (CachedStats.heatConsumptionPerHour * CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier) / 2500f;
                if (storedHeat >= heatToConsume)
                {
                    storedHeat -= heatToConsume;
                    powerComp?.PowerOutput = CachedStats.powerGenerated;
                    var room = parent.Position.GetRoom(parent.Map);
                    if (room != null)
                    {
                        room.PushHeat(CachedStats.heatPushedPerSecond / 60f);
                    }
                }
                else
                {
                    storedHeat = 0;
                    powerComp?.PowerOutput = powerComp.Props.basePowerConsumption;
                    UpdateLit();
                }
            }
            else
            {
                powerComp.PowerOutput = powerComp.Props.basePowerConsumption;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (storedHeat <= 0)
                return;

            var drawPos = parent.TrueCenter();
            drawPos.y += 0.1f;
            var transparency = 1f - (ActualStoredHeat / CachedStats.maxHeat);
            var overlayColor = new Color(1f, 1f, 1f, transparency);
            OverlayGraphic.color = overlayColor;
            OverlayGraphic.Draw(parent.DrawPos + new Vector3(0f, 0.1f, 0f), parent.Rotation, parent);
        }

        public override string CompInspectStringExtra()
        {
            return "VGE_HeatsinkHeatStored".Translate(ActualStoredHeat.ToString("F1"), CachedStats.maxHeat.ToString("F1"));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Add heat",
                    defaultDesc = "Add 1 heat to heatsink",
                    action = () => AddHeat(1f)
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Add max heat",
                    defaultDesc = "Set heat to maximum",
                    action = () => AddHeat(EffectiveMaxHeat - storedHeat)
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Clear heat",
                    defaultDesc = "Remove all stored heat",
                    action = ClearHeat
                };
            }
        }
    }
}
