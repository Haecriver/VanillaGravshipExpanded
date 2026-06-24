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
            if (comp.worldTarget.IsValid)
            {
                var targetMap = GetTargetMap(comp.worldTarget);
                if (targetMap != null && targetMap != caster.Map)
                {
                    isCrossMap = true;
                }
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
                projectile2.Launch(turret, drawPos, GetProjectileDest(resultingLine.Dest), currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, null);
                return true;
            }
            else if (target.IsValid && target.Cell.IsValid && target.ThingDestroyed is false)
            {
                try
                {
                    bool num = base.TryCastShot();
                    if (num && CasterIsPawn)
                    {
                        CasterPawn.records.Increment(RecordDefOf.ShotsFired);
                    }
                    return num;
                }
                catch
                {
                    // by some reason it can error, idk why
                    // if anyone knows how to fix it, please do
                    // leaving the offending error there
                    /*
                    Exception ticking VGE_EnemyAnticraftCaster773793 (at (111, 0, 106)): System.IndexOutOfRangeException: Index was outside the bounds of the array.
                    [Ref BEEA095E]
                    at Verse.GasGrid.DensityAt (System.Int32 index, Verse.GasType gasType) [0x00000] in <239ae808e7f5427aa8022d09e89ee1ed>:0 
                    at Verse.GasGrid.DensityAt (Verse.IntVec3 cell, Verse.GasType gasType) [0x0001c] in <239ae808e7f5427aa8022d09e89ee1ed>:0 
                    at Verse.GasUtility.AnyGas (Verse.IntVec3 cell, Verse.Map map, Verse.GasType gasType) [0x00000] in <239ae808e7f5427aa8022d09e89ee1ed>:0 
                    at Verse.ShotReport.HitReportFor (Verse.Thing caster, Verse.Verb verb, Verse.LocalTargetInfo target) [0x00181] in <239ae808e7f5427aa8022d09e89ee1ed>:0 
                    - PREFIX OskarPotocki.VEF: Void VEF.Weapons.VanillaExpandedFramework_ShotReport_HitReportFor_Patch:Prefix(Thing caster, Verb verb, LocalTargetInfo target)
                    - POSTFIX vanillaexpanded.gravship: Void VanillaGravshipExpanded.ShotReport_HitReportFor_Patch:Postfix(ShotReport& __result, Thing caster, Verb verb, LocalTargetInfo target)
                    at Verse.Verb_LaunchProjectile.TryCastShot () [0x00661] in <239ae808e7f5427aa8022d09e89ee1ed>:0 
                    - TRANSPILER vanillaexpanded.gravship: IEnumerable`1 VanillaGravshipExpanded.CastSourceReplacer:Transpiler(IEnumerable`1 instructions)
                    - TRANSPILER vanillaexpanded.gravship: IEnumerable`1 VanillaGravshipExpanded.Verb_LaunchProjectile_TryCastShot_Patch:Transpiler(IEnumerable`1 instructions)
                    - PREFIX OskarPotocki.VEF: Void VEF.Weapons.VanillaExpandedFramework_Verb_LaunchProjectile_TryCastShot:Prefix(Verb_LaunchProjectile __instance)
                    - POSTFIX OskarPotocki.VEF: Void VEF.Buildings.Verb_LaunchProjectile_TryCastShot_Patch:Postfix(Verb_LaunchProjectile __instance, Boolean __result)
                    - FINALIZER OskarPotocki.VEF: Void VEF.Weapons.VanillaExpandedFramework_Verb_LaunchProjectile_TryCastShot:Finalizer()
                    */
                }
            }
            return false;
        }

        protected virtual IntVec3 GetProjectileDest(IntVec3 intVec3)
        {
            return intVec3;
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
