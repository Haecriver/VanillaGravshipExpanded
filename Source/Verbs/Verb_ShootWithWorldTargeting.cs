using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using System.Linq;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Verb_ShootWithWorldTargeting : Verb_LaunchProjectile
    {
        public override int ShotsPerBurst => base.BurstShotCount;
        public Building_GravshipTurret Turret => (Building_GravshipTurret)caster;

        public override bool CanHitTarget(LocalTargetInfo targ)
        {
            if (caster == null || !caster.Spawned)
            {
                return false;
            }
            if (targ == caster)
            {
                return true;
            }
            var comp = caster.TryGetComp<CompWorldArtillery>();
            if (comp != null && comp.worldTarget.IsValid)
            {
                var targetMap = GetTargetMap(comp.worldTarget);
                if (targetMap != null && targetMap != caster.Map)
                {
                    return true;
                }
            }
            if (targ.HasThing && targ.Thing.Map != null && targ.Thing.Map != caster.Map)
            {
                return true;
            }
            return base.CanHitTarget(targ);
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            var casterPawn = (caster as Building_GravshipTurret).ManningPawn;
            if (casterPawn == null || casterPawn.skills == null) return;
            if (currentTarget.Thing is Pawn { Downed: false, IsColonyMech: false } pawn)
            {
                float num = (pawn.HostileTo(caster) ? 170f : 20f);
                float num2 = verbProps.AdjustedFullCycleTime(this, casterPawn);
                casterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
            }
        }

        public override bool TryCastShot()
        {
            var target = CurrentTarget;
            var comp = caster.TryGetComp<CompWorldArtillery>();

            bool isCrossMap = false;
            if (comp != null && comp.worldTarget.IsValid)
            {
                var targetMap = GetTargetMap(comp.worldTarget);
                if (targetMap != null && targetMap != caster.Map)
                {
                    isCrossMap = true;
                }
            }
            if (target.HasThing && target.Thing.Map != null && target.Thing.Map != caster.Map)
            {
                isCrossMap = true;
            }

            if (isCrossMap)
            {
                var turret = caster as Building_GravshipTurret;
                var targetMap = GetTargetMap(comp.worldTarget);
                bool invalid = false;
                if (comp.worldTarget.IsValid is false) invalid = true;
                if (targetMap == null) invalid = true;
                if (targetMap != null && Find.Maps.IndexOf(targetMap) < 0) invalid = true;
                if (comp.worldTarget.WorldObject != null && comp.worldTarget.WorldObject.Destroyed) invalid = true;
                if (invalid)
                {
                    turret.ResetForcedTarget();
                    return false;
                }
                ThingDef projectile = Projectile;
                ShootLine resultingLine = new ShootLine(caster.Position, currentTarget.Cell);
                Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
                ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
                Vector3 drawPos = Building_GravshipTurret.GetCastSource(caster);
                Thing equipmentSource = base.EquipmentSource;
                projectile2.Launch(turret, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, null);
                return true;
            }
            else
            {
                bool num = base.TryCastShot();
                if (num && CasterIsPawn)
                {
                    CasterPawn.records.Increment(RecordDefOf.ShotsFired);
                }
                return num;
            }
        }

        public Map GetTargetMap(GlobalTargetInfo target)
        {
            if (target.HasThing) return target.Thing.Map;
            if (target.HasWorldObject && target.WorldObject is MapParent mp) return mp.Map;
            if (target.Tile >= 0) return Find.Maps.FirstOrDefault(m => m.Tile == target.Tile);
            return null;
        }
    }
}
