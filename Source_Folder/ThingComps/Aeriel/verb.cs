﻿using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    // Token: 0x020008AA RID: 2218
    public class AERIALLaunch : Verb
    {
        // Token: 0x170008AA RID: 2218
        // (get) Token: 0x06003731 RID: 14129 RVA: 0x0015FD1C File Offset: 0x0015DF1C
        public virtual ThingDef Projectile
        {
            get
            {

                if (base.EquipmentSource != null)
                {
                    AERIALChangeableProjectile comp = base.EquipmentSource.GetComp<AERIALChangeableProjectile>();
                    if (comp != null && comp.Loaded)
                    {
                        return comp.Projectile;
                    }
                }
                return verbProps.defaultProjectile;
            }
        }

        //jn Token: 0x06003732 RID: 14130 RVA: 0x0015FD5C File Offset: 0x0015DF5C
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null, (base.EquipmentSource != null) ? base.EquipmentSource.def : null, Projectile, ShotsPerBurst > 1));
        }

        protected override bool TryCastShot()
        {
            AERIALChangeableProjectile comp = EquipmentSource.GetComp<AERIALChangeableProjectile>();
            Thing launcher = caster;
            ThingDef projectile = Projectile;
            if (projectile == null)
            {
                return false;
            }
            Thing equipment = EquipmentSource;
            Vector3 drawPos = caster.DrawPos;
            bool flag = base.TryFindShootLineFromTo(caster.Position, currentTarget, out ShootLine shootLine);
            float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius, currentTarget.Cell - caster.Position);
            int max = GenRadial.NumCellsInRadius(num);
            int num2 = Rand.Range(0, max);
            IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];



            ThrowDebugText("ToRadius");
            ThrowDebugText("Rad\nDest", c);
            ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
            if (Rand.Chance(0.5f))
            {
                projectileHitFlags = ProjectileHitFlags.All;
            }
            if (!canHitNonTargetPawnsNow)
            {
                projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
            }
            IntVec3 pos = shootLine.Source;
            pos.x += 20;
            pos.z += 20;
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, pos, caster.Map, WipeMode.Vanish);
            projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment: equipment);
            comp.loadedShells.RemoveAt(comp.loadedShells.Count - 1);

            if (!comp.loadedShells.NullOrEmpty())
            {
                pos = shootLine.Source;
                pos.z -= 20;
                pos.x -= 20;
                projectile = Projectile;
                projectile2 = (Projectile)GenSpawn.Spawn(projectile, pos, caster.Map, WipeMode.Vanish);
                projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment: equipment);
                comp.loadedShells.RemoveAt(comp.loadedShells.Count - 1);
            }

            return true;
        }
        protected bool oldShotCast()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            ThingDef projectile = Projectile;
            if (projectile == null)
            {
                return false;
            }
            bool flag = base.TryFindShootLineFromTo(caster.Position, currentTarget, out ShootLine shootLine);
            if (verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }
            if (base.EquipmentSource != null)
            {
                AERIALChangeableProjectile comp = base.EquipmentSource.GetComp<AERIALChangeableProjectile>();
                if (comp != null)
                {
                    comp.Notify_ProjectileLaunched();
                }
                CompReloadable comp2 = base.EquipmentSource.GetComp<CompReloadable>();
                if (comp2 != null)
                {
                    comp2.UsedOnce();
                }
            }
            Thing launcher = caster;
            Thing equipment = base.EquipmentSource;
            CompMannable compMannable = caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = caster;
            }
            Vector3 drawPos = caster.DrawPos;
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, caster.Map, WipeMode.Vanish);
            if (verbProps.ForcedMissRadius > 0.5f)
            {

                float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius, currentTarget.Cell - caster.Position);
                if (num > 0.5f)
                {
                    int max = GenRadial.NumCellsInRadius(num);
                    int num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];
                        ThrowDebugText("ToRadius");
                        ThrowDebugText("Rad\nDest", c);
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }
                        if (!canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                        }
                        projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment: equipment);
                        return true;
                    }
                }
            }
            ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = (randomCoverToMissInto != null) ? randomCoverToMissInto.def : null;
            if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                ThrowDebugText("ToWild" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
                ThrowDebugText("Wild\nDest", shootLine.Dest);
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags2, equipment: equipment);
                return true;
            }
            if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
            {
                ThrowDebugText("ToCover" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
                ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                if (canHitNonTargetPawnsNow)
                {
                    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, equipment: equipment);
                return true;
            }
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (canHitNonTargetPawnsNow)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }
            ThrowDebugText("ToHit" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
            if (currentTarget.Thing != null)
            {
                projectile2.Launch(launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, equipment: equipment);
                ThrowDebugText("Hit\nDest", currentTarget.Cell);
            }
            else
            {
                projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags4, equipment: equipment);
                ThrowDebugText("Hit\nDest", shootLine.Dest);
            }
            return true;
        }

        // Token: 0x06003734 RID: 14132 RVA: 0x0002AB7F File Offset: 0x00028D7F
        private void ThrowDebugText(string text)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(caster.DrawPos, caster.Map, text, -1f);
            }
        }

        // Token: 0x06003735 RID: 14133 RVA: 0x0002ABA9 File Offset: 0x00028DA9
        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), caster.Map, text, -1f);
            }
        }

        // Token: 0x06003736 RID: 14134 RVA: 0x001601E0 File Offset: 0x0015E3E0
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            ThingDef projectile = Projectile;
            if (projectile == null)
            {
                return 0f;
            }
            return projectile.projectile.explosionRadius;
        }

        // Token: 0x06003737 RID: 14135 RVA: 0x0016020C File Offset: 0x0015E40C
        public override bool Available()
        {
            if (!base.Available())
            {
                return false;
            }
            if (CasterIsPawn)
            {
                Pawn casterPawn = CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }
            return Projectile != null;
        }
    }
}
