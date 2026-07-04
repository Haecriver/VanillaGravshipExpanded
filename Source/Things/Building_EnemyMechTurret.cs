using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Building_EnemyMechTurret : Building_GravshipTurret
    {
        private List<Map> cachedMapsInRange;
        public override bool CanFire => true;
        public override bool CanAutoAttack => true;
        public override float GravshipTargeting => 1f;
        public override bool CanSetForcedTarget => true;
        public override bool HideForceTargetGizmo => true;

        protected override bool ShowNoLinkedTerminalOverlay => false;

        private CompWorldArtillery compWorldArtillery;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compWorldArtillery = GetComp<CompWorldArtillery>();
        }

        public override float BurstCooldownTime()
        {
            float cooldown = base.BurstCooldownTime();
            float factor = 1f;
            float flat = 0f;
            int bufferLinks = 0;
            foreach (var b in Map.listerBuildings.allBuildingsNonColonist)
            {
                if (b.Faction == Faction)
                {
                    if (b.TryGetComp<CompEnemyTerminal>() is CompEnemyTerminal terminal && terminal.IsManned)
                    {
                        factor *= terminal.Props.cooldownFactor;
                        flat += terminal.Props.cooldownFlatOffset;
                    }
                    if (b.TryGetComp<CompEnemyTurretBuffer>() is CompEnemyTurretBuffer buffer && buffer.Active && buffer.Props.validTurrets.Contains(this.def) && b.Position.DistanceTo(this.Position) <= buffer.Props.radius)
                    {
                        if (buffer.Props.cooldownReductionTicks > 0)
                        {
                            flat += buffer.Props.cooldownReductionTicks;
                            bufferLinks++;
                            if (bufferLinks >= buffer.Props.maxLinks) break;
                        }
                    }
                }
            }
            return Mathf.Max(6.33f, (cooldown * factor) - (flat / 60f));
        }
        private int GetTargetPriority(Thing t)
        {
            if (t is Building_GravshipTurret)
                return 1;
            if (t.def == VGEDefOf.VGE_GiantThruster)
                return 2;
            if (t.def == ThingDefOf.LargeThruster)
                return 3;
            if (t.def == ThingDefOf.SmallThruster)
                return 4;
            if (t.def == VGEDefOf.VGE_GiantAstrofuelTank)
                return 5;
            if (t.def == VGEDefOf.LargeChemfuelTank)
                return 6;
            if (t.def == ThingDefOf.ChemfuelTank)
                return 7;
            if (t is Building_Bed)
                return 8;
            if (t is Pawn pawn && pawn.IsColonist && pawn.Downed is false)
                return 9;
            return 10;
        }

        public override LocalTargetInfo TryFindNewTarget()
        {
            if (compWorldArtillery != null)
            {
                if (cachedMapsInRange == null || this.IsHashIntervalTick(250))
                {
                    var mapsWithDist = new List<(Map map, float distance)>();
                    foreach (var map in Find.Maps)
                    {
                        if (map.IsPocketMap is false)
                        {
                            float dist = GravshipHelper.GetDistance(Map.Tile, map.Tile);
                            if (dist <= compWorldArtillery.Props.worldMapAttackRange)
                            {
                                mapsWithDist.Add((map, dist));
                            }
                        }
                    }
                    mapsWithDist.Sort((a, b) => a.distance.CompareTo(b.distance));
                    cachedMapsInRange = mapsWithDist.Select(x => x.map).ToList();
                }
                foreach (var map in cachedMapsInRange)
                {
                    if (map == null || map.Disposed)
                    {
                        continue;
                    }
                    var target = GetTargetForMap(map);
                    if (target.IsValid)
                    {
                        if (target.Thing.Map != Map)
                        {
                            compWorldArtillery.StartAttack(new GlobalTargetInfo(target.Thing), target, this);
                            return LocalTargetInfo.Invalid;
                        }
                        return target;
                    }
                }
            }
            return GetTargetForMap(Map);
        }

        private LocalTargetInfo GetTargetForMap(Map map)
        {
            var searcher = this;
            var verb = AttackVerb;
            var searcherThing = searcher;
            TargetScanFlags flags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (!AttackVerb.ProjectileFliesOverhead())
            {
                flags |= TargetScanFlags.NeedLOSToAll;
                flags |= TargetScanFlags.LOSBlockableByGas;
            }
            if (AttackVerb.IsIncendiary_Ranged())
            {
                flags |= TargetScanFlags.NeedNonBurning;
            }
            if (IsMortar)
            {
                flags |= TargetScanFlags.NeedNotUnderThickRoof;
            }

            Predicate<IAttackTarget> innerValidator = delegate (IAttackTarget t)
            {
                Thing thing = t.Thing;
                if (t == searcher)
                {
                    return false;
                }
                if (thing.Map == Map)
                {
                    float num3 = verb.verbProps.EffectiveMinRange(thing, searcherThing);
                    if (num3 > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < num3 * num3)
                    {
                        return false;
                    }
                }
                if (!searcherThing.HostileTo(thing))
                {
                    return false;
                }
                if ((flags & TargetScanFlags.NeedNotUnderThickRoof) != 0)
                {
                    RoofDef roof = thing.Position.GetRoof(thing.Map);
                    if (roof != null && roof.isThickRoof)
                    {
                        return false;
                    }
                }
                if (((flags & TargetScanFlags.NeedThreat) != 0 || (flags & TargetScanFlags.NeedAutoTargetable) != 0) && t.ThreatDisabled(searcher))
                {
                    return false;
                }
                if ((flags & TargetScanFlags.NeedAutoTargetable) != 0 && !AttackTargetFinder.IsAutoTargetable(t))
                {
                    return false;
                }
                if ((flags & TargetScanFlags.NeedActiveThreat) != 0 && !GenHostility.IsActiveThreatTo(t, searcher.Faction))
                {
                    return false;
                }
                return true;
            };

            var seenTargets = new Dictionary<Thing, int>();
            foreach (IAttackTarget target in map.attackTargetsCache.GetPotentialTargetsFor(this))
            {
                if (innerValidator(target))
                {
                    seenTargets.TryAdd(target.Thing, GetTargetPriority(target.Thing));
                }
            }
            foreach (var building in map.listerBuildings.allBuildingsColonist)
            {
                if (!seenTargets.ContainsKey(building))
                {
                    int priority = GetTargetPriority(building);
                    if (priority < 10)
                    {
                        seenTargets[building] = priority;
                    }
                }
            }
            var potentialTargets = seenTargets.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            foreach (Thing target in potentialTargets)
            {
                if (map == Map)
                {
                    if (verb.CanHitTarget(target))
                    {
                        return target;
                    }
                }
                else
                {
                    return target;
                }
            }
            return LocalTargetInfo.Invalid;
        }
    }
}
