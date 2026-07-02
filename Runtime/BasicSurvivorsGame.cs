using System;
using System.Collections.Generic;
using Deucarian.Attacks;
using Deucarian.Combat;
using Deucarian.Encounters;
using Deucarian.Progression;
using Deucarian.Projectiles;
using Deucarian.RunUpgrades;
using Deucarian.WeaponSystems;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsRunState
    {
        Booting = 0,
        Playing = 1,
        LevelUp = 2,
        GameOver = 3,
        Victory = 4
    }

    public enum SurvivorsPickupKind
    {
        Experience = 0,
        Magnet = 1
    }

    [Serializable]
    public sealed class SurvivorsTemplateTuning
    {
        public float PlayerMoveSpeed = 5.75f;
        public float PlayerRadius = 0.55f;
        public float PlayerMaxHealth = 36f;
        public float PlayerContactInvulnerabilitySeconds = 0.35f;
        public float EnemySpawnRadius = 12f;
        public float EnemySpawnIntervalSeconds = 0.48f;
        public int EnemyMaximumAlive = 72;
        public float EnemyMaxHealth = 10f;
        public float EnemyMoveSpeed = 2.3f;
        public float EnemyRadius = 0.48f;
        public float EnemyContactDamage = 6f;
        public float EnemyContactIntervalSeconds = 0.7f;
        public int EnemyExperienceReward = 2;
        public float RunEscalationIntervalSeconds = 60f;
        public float MinimumEnemySpawnIntervalSeconds = 0.14f;
        public float EnemySpawnIntervalReductionPerEscalation = 0.025f;
        public int EnemyMaximumAliveIncreasePerEscalation = 8;
        public float EnemyHealthMultiplierPerEscalation = 0.18f;
        public float EnemyMoveSpeedMultiplierPerEscalation = 0.025f;
        public float EnemyExperienceMultiplierPerEscalation = 0.12f;
        public float MinibossSpawnTimeSeconds = 300f;
        public float MinibossMaxHealth = 220f;
        public float MinibossMoveSpeed = 2.15f;
        public float MinibossRadius = 0.82f;
        public float MinibossContactDamage = 10f;
        public float MinibossContactIntervalSeconds = 0.85f;
        public int MinibossExperienceReward = 34;
        public float BossSpawnTimeSeconds = 1200f;
        public float BossMaxHealth = 1200f;
        public float BossMoveSpeed = 1.85f;
        public float BossRadius = 1.18f;
        public float BossContactDamage = 16f;
        public float BossContactIntervalSeconds = 0.95f;
        public int BossExperienceReward = 120;
        public float SurvivalVictoryTimeSeconds = 1800f;
        public float WeaponCooldownSeconds = 0.52f;
        public float WeaponRange = 14f;
        public float ProjectileDamage = 7f;
        public float ProjectileSpeed = 11.5f;
        public float ProjectileRadius = 0.22f;
        public float ProjectileLifetimeSeconds = 2.4f;
        public int ProjectilePierceCount = 0;
        public int ProjectileChainCount = 0;
        public int ProjectileForkCount = 0;
        public int ProjectileReturnCount = 0;
        public float OrbitDamage = 2.5f;
        public int OrbitBladeCount = 1;
        public float OrbitRadius = 2.1f;
        public float OrbitBladeHitRadius = 0.38f;
        public float OrbitDegreesPerSecond = 185f;
        public float OrbitContactTickIntervalSeconds = 0.34f;
        public float MeleeDamage = 5.5f;
        public float MeleeCooldownSeconds = 1.15f;
        public float MeleeRange = 2.65f;
        public float MeleeArcDegrees = 120f;
        public int MeleeHitCount = 2;
        public float MeleeVisualDurationSeconds = 0.16f;
        public float BurstDamage = 4.2f;
        public float BurstCooldownSeconds = 2.8f;
        public float BurstRadius = 3.15f;
        public int BurstCount = 1;
        public float BurstRepeatIntervalSeconds = 0.18f;
        public float BurstVisualDurationSeconds = 0.22f;
        public float HitscanDamage = 6.5f;
        public float HitscanCooldownSeconds = 1.35f;
        public float HitscanRange = 9.5f;
        public int HitscanCount = 1;
        public float HitscanWidth = 0.24f;
        public float HitscanVisualDurationSeconds = 0.08f;
        public bool HitscanPierces = false;
        public float GrenadeDamage = 8.5f;
        public float GrenadeCooldownSeconds = 2.4f;
        public float GrenadeRange = 8.5f;
        public int GrenadePayloadCount = 1;
        public float GrenadePayloadTravelSpeed = 9f;
        public float GrenadePayloadArmingSeconds = 0.7f;
        public float GrenadePayloadExplosionRadius = 2.35f;
        public float PlacedPayloadDamage = 6.8f;
        public float TrapCooldownSeconds = 2.75f;
        public float MineCooldownSeconds = 3.15f;
        public float PlacedPayloadRange = 7f;
        public int PlacedPayloadCount = 1;
        public float PlacedPayloadArmingSeconds = 0.7f;
        public float PlacedPayloadLifetimeSeconds = 4f;
        public float PlacedPayloadTriggerRadius = 1.35f;
        public float PlacedPayloadExplosionRadius = 2.15f;
        public float PayloadHazardDurationSeconds = 1.6f;
        public float PayloadHazardTickIntervalSeconds = 0.45f;
        public float PayloadHazardDamageRatio = 0.18f;
        public float PickupAttractRange = 3.25f;
        public float PickupAttractionSpeed = 7.8f;
        public float PickupCollectRadius = 0.72f;
        public float MagnetRecallSpeedMultiplier = 2.4f;
        public int ExperienceRequiredBase = 4;
        public int ExperienceRequiredPerLevel = 3;
        public float StatusPoisonDurationSeconds = 4.5f;
        public float StatusBleedDurationSeconds = 5.5f;
        public float StartingBarrierCapacity = 8f;
        public float BaseBarrierRegenPerSecond = 0.12f;
        public int DraftChoiceCount = 3;
        public float RewardSelectionTimeoutSeconds = 18f;
        public int RunSeed = 20260624;

        public SurvivorsTemplateTuning Clone()
        {
            return (SurvivorsTemplateTuning)MemberwiseClone();
        }
    }

    public static class BasicSurvivorsGame
    {
        public static readonly DamageTypeId ArcaneDamageType = new DamageTypeId("damage.survivors.arcane");
        public static readonly WorldSpawnableId SwarmEnemySpawnableId = new WorldSpawnableId("enemy.survivors.swarm");
        public static readonly WorldSpawnableId MinibossEnemySpawnableId = new WorldSpawnableId("enemy.survivors.miniboss");
        public static readonly WorldSpawnableId BossEnemySpawnableId = new WorldSpawnableId("enemy.survivors.boss");
        public static readonly WorldSpawnableId ExperiencePickupSpawnableId = new WorldSpawnableId("pickup.survivors.experience");
        public static readonly WorldSpawnableId MagnetPickupSpawnableId = new WorldSpawnableId("pickup.survivors.magnet");
        public static readonly WorldSpawnableId ProjectileSpawnableId = new WorldSpawnableId("projectile.survivors.arcane-bolt");
        public static readonly WorldSpawnChannelId RadialSpawnChannelId = new WorldSpawnChannelId("spawn.survivors.radial");
        public static readonly WorldSpawnChannelId ExplicitSpawnChannelId = new WorldSpawnChannelId("spawn.survivors.explicit");
        public static readonly AttackDefinitionId ArcaneBoltAttackId = new AttackDefinitionId("attack.survivors.arcane-bolt");
        public static readonly ProjectileDefinitionId ArcaneBoltProjectileId = new ProjectileDefinitionId("projectile.survivors.arcane-bolt");
        public const string ArcaneWandWeaponContentId = "weapon.survivors.arcane-wand";
        public const string FrostFanWeaponContentId = "weapon.survivors.frost-fan";
        public const string OrbitWardWeaponContentId = "weapon.survivors.orbit-ward";
        public const string ThornHaloWeaponContentId = "weapon.survivors.thorn-halo";
        public const string MoonSlashWeaponContentId = "weapon.survivors.moon-slash";
        public const string StarNovaWeaponContentId = "weapon.survivors.star-nova";
        public const string StarBeamWeaponContentId = "weapon.survivors.star-beam";
        public const string GravityGrenadeWeaponContentId = "weapon.survivors.gravity-grenade";
        public const string RuneTrapWeaponContentId = "weapon.survivors.rune-trap";
        public const string AetherMineWeaponContentId = "weapon.survivors.aether-mine";
        public static readonly WeaponDefinitionId ArcaneWandWeaponId = new WeaponDefinitionId(ArcaneWandWeaponContentId);
        public static readonly WeaponDefinitionId StarBeamWeaponId = new WeaponDefinitionId(StarBeamWeaponContentId);
        public static readonly WeaponDefinitionId GravityGrenadeWeaponId = new WeaponDefinitionId(GravityGrenadeWeaponContentId);
        public static readonly WeaponDefinitionId RuneTrapWeaponId = new WeaponDefinitionId(RuneTrapWeaponContentId);
        public static readonly WeaponDefinitionId AetherMineWeaponId = new WeaponDefinitionId(AetherMineWeaponContentId);
        public static readonly CurrencyId BloodShardsCurrencyId = new CurrencyId("currency.survivors.blood-shards");
        public static readonly TrackId LegacyExperienceTrackId = new TrackId("track.survivors.legacy-xp");
        public static readonly ResearchNodeId ArcaneLegacyMetaUpgradeId = new ResearchNodeId("meta.survivors.arcane-legacy");
        public const string MetaDamageEffectId = "survivors.meta.damage.flat";
        public const string MinibossRewardId = "reward.survivors.miniboss";
        public const string BossRewardId = "reward.survivors.final-boss";
        public const string BloodStarRelicId = "relic.survivors.blood-star";
        public const string QuickenedSigilRelicId = "relic.survivors.quickened-sigil";
        public const string GemheartCharmRelicId = "relic.survivors.gemheart-charm";
        public const string DefaultClassId = "class.survivors.arcane-initiate";
        public const string EmberVanguardClassId = "class.survivors.ember-vanguard";
        public const string EmberVanguardUnlockRewardId = "reward.survivors.class.ember-vanguard";
        public const string OrbitingFocusUpgradeId = "upgrade.survivors.orbiting-focus";
        public const string FrostFanUpgradeId = "upgrade.survivors.frost-fan";
        public const string ThornHaloUpgradeId = "upgrade.survivors.thorn-halo-wall";
        public const string CrescentChainUpgradeId = "upgrade.survivors.crescent-chain";
        public const string NovaEchoUpgradeId = "upgrade.survivors.nova-echo";
        public const string CinderEchoUpgradeId = "upgrade.survivors.cinder-echoes";
        public const string TargetedSigilUpgradeId = "upgrade.survivors.targeted-burst-sigils";
        public const string PrismaticBeamUpgradeId = "upgrade.survivors.prismatic-beam";
        public const string ExtraPayloadUpgradeId = "upgrade.survivors.extra-payload";
        public const string BiggerBoomsUpgradeId = "upgrade.survivors.bigger-booms";
        public const string WiderTriggersUpgradeId = "upgrade.survivors.wider-triggers";
        public const string ArcaneThesisUpgradeId = "upgrade.survivors.arcane-thesis";
        public const string FrostNeedleworkUpgradeId = "upgrade.survivors.frost-needlework";
        public const string BloodRingCanticleUpgradeId = "upgrade.survivors.blood-ring-canticle";
        public const string CinderScriptUpgradeId = "upgrade.survivors.cinder-script";
        public const string EmberForgeHeartUpgradeId = "upgrade.survivors.ember-forge-heart";
        public const string EmberTempoUpgradeId = "upgrade.survivors.ember-tempo";
        public const string SiegePayloadsUpgradeId = "upgrade.survivors.siege-payloads";
        public const string EmberWardUpgradeId = "upgrade.survivors.ember-ward";
        public static readonly RunUpgradeEffectId DamageBonusEffect = new RunUpgradeEffectId("survivors.damage.flat");
        public static readonly RunUpgradeEffectId FireRateEffect = new RunUpgradeEffectId("survivors.weapon.cooldown_multiplier");
        public static readonly RunUpgradeEffectId MoveSpeedEffect = new RunUpgradeEffectId("survivors.player.move_speed");
        public static readonly RunUpgradeEffectId MagnetRangeEffect = new RunUpgradeEffectId("survivors.pickup.range");
        public static readonly RunUpgradeEffectId MaxHealthEffect = new RunUpgradeEffectId("survivors.player.max_health");
        public static readonly RunUpgradeEffectId OrbitBladeEffect = new RunUpgradeEffectId("survivors.orbit.blade_count");
        public static readonly RunUpgradeEffectId OrbitRadiusEffect = new RunUpgradeEffectId("survivors.orbit.radius");
        public static readonly RunUpgradeEffectId MeleeTargetEffect = new RunUpgradeEffectId("survivors.melee.target_count");
        public static readonly RunUpgradeEffectId BurstCountEffect = new RunUpgradeEffectId("survivors.burst.count");
        public static readonly RunUpgradeEffectId BurstEchoEffect = new RunUpgradeEffectId("survivors.burst.echo_count");
        public static readonly RunUpgradeEffectId TargetedBurstEffect = new RunUpgradeEffectId("survivors.burst.targeted_sigils");
        public static readonly RunUpgradeEffectId ProjectileFanEffect = new RunUpgradeEffectId("survivors.projectile.fan_count");
        public static readonly RunUpgradeEffectId ProjectilePierceEffect = new RunUpgradeEffectId("survivors.projectile.pierce_count");
        public static readonly RunUpgradeEffectId ProjectileChainEffect = new RunUpgradeEffectId("survivors.projectile.chain_count");
        public static readonly RunUpgradeEffectId ProjectileForkEffect = new RunUpgradeEffectId("survivors.projectile.fork_count");
        public static readonly RunUpgradeEffectId ProjectileReturnEffect = new RunUpgradeEffectId("survivors.projectile.return_count");
        public static readonly RunUpgradeEffectId HitscanPierceEffect = new RunUpgradeEffectId("survivors.hitscan.pierce");
        public static readonly RunUpgradeEffectId PayloadCountEffect = new RunUpgradeEffectId("survivors.payload.count");
        public static readonly RunUpgradeEffectId PayloadRadiusEffect = new RunUpgradeEffectId("survivors.payload.radius");
        public static readonly RunUpgradeEffectId PayloadTriggerRadiusEffect = new RunUpgradeEffectId("survivors.payload.trigger_radius");
        public static readonly RunUpgradeEffectId PoisonEffect = new RunUpgradeEffectId("survivors.status.poison_ratio");
        public static readonly RunUpgradeEffectId BleedEffect = new RunUpgradeEffectId("survivors.status.bleed_ratio");
        public static readonly RunUpgradeEffectId ExecuteEffect = new RunUpgradeEffectId("survivors.status.execute_threshold");
        public static readonly RunUpgradeEffectId LifestealEffect = new RunUpgradeEffectId("survivors.sustain.lifesteal_ratio");
        public static readonly RunUpgradeEffectId BarrierCapacityEffect = new RunUpgradeEffectId("survivors.barrier.capacity");
        public static readonly RunUpgradeEffectId BarrierRegenEffect = new RunUpgradeEffectId("survivors.barrier.regen");
        public static readonly RunUpgradeEffectId BarrierOnDamageEffect = new RunUpgradeEffectId("survivors.barrier.on_damage_ratio");
        public static readonly RunUpgradeTargetId PlayerTarget = new RunUpgradeTargetId("survivors.player");
        public static readonly RunUpgradeTargetId WeaponTarget = new RunUpgradeTargetId("survivors.weapon.arcane-wand");
        public static readonly RunUpgradeTargetId FrostFanWeaponTarget = new RunUpgradeTargetId("survivors.weapon.frost-fan");
        public static readonly RunUpgradeTargetId OrbitWeaponTarget = new RunUpgradeTargetId("survivors.weapon.orbit-ward");
        public static readonly RunUpgradeTargetId ThornHaloWeaponTarget = new RunUpgradeTargetId("survivors.weapon.thorn-halo");
        public static readonly RunUpgradeTargetId MeleeWeaponTarget = new RunUpgradeTargetId("survivors.weapon.moon-slash");
        public static readonly RunUpgradeTargetId BurstWeaponTarget = new RunUpgradeTargetId("survivors.weapon.star-nova");
        public static readonly RunUpgradeTargetId HitscanWeaponTarget = new RunUpgradeTargetId("survivors.weapon.star-beam");
        public static readonly RunUpgradeTargetId GrenadeWeaponTarget = new RunUpgradeTargetId("survivors.weapon.gravity-grenade");
        public static readonly RunUpgradeTargetId TrapWeaponTarget = new RunUpgradeTargetId("survivors.weapon.rune-trap");
        public static readonly RunUpgradeTargetId MineWeaponTarget = new RunUpgradeTargetId("survivors.weapon.aether-mine");
        public static readonly RunUpgradeTargetId PayloadWeaponTarget = new RunUpgradeTargetId("survivors.weapon.payloads");
        public static readonly RunUpgradeTargetId PickupTarget = new RunUpgradeTargetId("survivors.pickups");
        public static readonly RunUpgradeTargetId StatusTarget = new RunUpgradeTargetId("survivors.status");
        public static readonly RunUpgradeTargetId BarrierTarget = new RunUpgradeTargetId("survivors.barrier");

        public static SurvivorsTemplateTuning CreateDefaultTuning()
        {
            return new SurvivorsTemplateTuning();
        }

        public static CombatCatalog CreateCombatCatalog()
        {
            return new CombatCatalog(new[]
            {
                new DamageTypeDefinition(ArcaneDamageType)
            });
        }

        public static WeaponDefinition CreateWeaponDefinition()
        {
            return new WeaponDefinition(
                ArcaneWandWeaponId,
                WeaponFireMode.Projectile,
                ArcaneBoltAttackId,
                cooldownTicks: 37,
                projectileDefinitionId: ArcaneBoltProjectileId,
                pattern: WeaponFirePattern.Single);
        }

        public static ProjectileDefinition CreateProjectileDefinition(SurvivorsTemplateTuning tuning = null)
        {
            SurvivorsTemplateTuning resolved = tuning ?? CreateDefaultTuning();
            return new ProjectileDefinition(
                ArcaneBoltProjectileId,
                ProjectileSpawnableId,
                ArcaneDamageType,
                resolved.ProjectileDamage,
                Mathf.Max(1, Mathf.RoundToInt(resolved.ProjectileLifetimeSeconds * 60f)),
                resolved.ProjectileSpeed,
                maxImpacts: 1);
        }

        public static SurvivorsRunFlowDefinition CreateRunFlowDefinition(SurvivorsTemplateTuning tuning = null)
        {
            SurvivorsTemplateTuning resolved = tuning ?? CreateDefaultTuning();
            return new SurvivorsRunFlowDefinition(
                resolved.RunEscalationIntervalSeconds,
                resolved.MinimumEnemySpawnIntervalSeconds,
                resolved.EnemySpawnIntervalReductionPerEscalation,
                resolved.EnemyMaximumAliveIncreasePerEscalation,
                resolved.EnemyHealthMultiplierPerEscalation,
                resolved.EnemyMoveSpeedMultiplierPerEscalation,
                resolved.EnemyExperienceMultiplierPerEscalation,
                resolved.MinibossSpawnTimeSeconds,
                CreateEnemyProfile(SurvivorsEnemyRole.Miniboss, resolved),
                resolved.BossSpawnTimeSeconds,
                CreateEnemyProfile(SurvivorsEnemyRole.Boss, resolved),
                resolved.SurvivalVictoryTimeSeconds,
                CreateEnemyProfileDefinitions(resolved));
        }

        public static IReadOnlyList<SurvivorsEnemyProfile> CreateEnemyProfileDefinitions(SurvivorsTemplateTuning tuning = null)
        {
            SurvivorsTemplateTuning resolved = tuning ?? CreateDefaultTuning();
            return new[]
            {
                CreateEnemyProfile(SurvivorsEnemyRole.Swarm, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.Runner, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.Bruiser, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.Spitter, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.Elite, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.Miniboss, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.Boss, resolved)
            };
        }

        public static SurvivorsEnemyProfile CreateEnemyProfile(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning = null)
        {
            SurvivorsTemplateTuning resolved = tuning ?? CreateDefaultTuning();
            switch (role)
            {
                case SurvivorsEnemyRole.Runner:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Runner,
                        "enemy.survivors.runner",
                        "Ghoul Runner",
                        resolved.EnemyMaxHealth * 0.72f,
                        resolved.EnemyMoveSpeed * 1.55f,
                        resolved.EnemyRadius * 0.86f,
                        resolved.EnemyContactDamage * 0.75f,
                        resolved.EnemyContactIntervalSeconds * 0.82f,
                        Mathf.Max(1, Mathf.RoundToInt(resolved.EnemyExperienceReward * 1.1f)),
                        new Color(1f, 0.62f, 0.24f));
                case SurvivorsEnemyRole.Bruiser:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Bruiser,
                        "enemy.survivors.bruiser",
                        "Grave Bruiser",
                        resolved.EnemyMaxHealth * 2.8f,
                        resolved.EnemyMoveSpeed * 0.72f,
                        resolved.EnemyRadius * 1.32f,
                        resolved.EnemyContactDamage * 1.45f,
                        resolved.EnemyContactIntervalSeconds * 1.18f,
                        Mathf.Max(1, Mathf.RoundToInt(resolved.EnemyExperienceReward * 2.4f)),
                        new Color(0.72f, 0.18f, 0.16f));
                case SurvivorsEnemyRole.Spitter:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Spitter,
                        "enemy.survivors.spitter",
                        "Cult Spitter",
                        resolved.EnemyMaxHealth * 1.25f,
                        resolved.EnemyMoveSpeed * 0.92f,
                        resolved.EnemyRadius,
                        resolved.EnemyContactDamage * 0.65f,
                        resolved.EnemyContactIntervalSeconds,
                        Mathf.Max(1, Mathf.RoundToInt(resolved.EnemyExperienceReward * 1.7f)),
                        new Color(0.48f, 0.88f, 0.36f),
                        rangedAttackRange: 5.8f,
                        rangedAttackDamage: resolved.EnemyContactDamage * 0.62f,
                        rangedAttackIntervalSeconds: 2.2f,
                        preferredRange: 4.2f);
                case SurvivorsEnemyRole.Elite:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Elite,
                        "enemy.survivors.elite",
                        "Blood Warden Elite",
                        resolved.EnemyMaxHealth * 6.2f,
                        resolved.EnemyMoveSpeed * 1.05f,
                        resolved.EnemyRadius * 1.52f,
                        resolved.EnemyContactDamage * 1.75f,
                        resolved.EnemyContactIntervalSeconds * 0.9f,
                        Mathf.Max(1, Mathf.RoundToInt(resolved.EnemyExperienceReward * 6.5f)),
                        new Color(1f, 0.18f, 0.54f),
                        rangedAttackRange: 4.6f,
                        rangedAttackDamage: resolved.EnemyContactDamage * 0.5f,
                        rangedAttackIntervalSeconds: 2.6f,
                        preferredRange: 2.8f);
                case SurvivorsEnemyRole.Miniboss:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Miniboss,
                        MinibossEnemySpawnableId.Value,
                        "Bloodbound Miniboss",
                        resolved.MinibossMaxHealth,
                        resolved.MinibossMoveSpeed,
                        resolved.MinibossRadius,
                        resolved.MinibossContactDamage,
                        resolved.MinibossContactIntervalSeconds,
                        resolved.MinibossExperienceReward,
                        new Color(1f, 0.48f, 0.18f),
                        rangedAttackRange: 5.2f,
                        rangedAttackDamage: resolved.MinibossContactDamage * 0.45f,
                        rangedAttackIntervalSeconds: 2.8f,
                        preferredRange: 3.4f);
                case SurvivorsEnemyRole.Boss:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Boss,
                        BossEnemySpawnableId.Value,
                        "Eclipse Boss",
                        resolved.BossMaxHealth,
                        resolved.BossMoveSpeed,
                        resolved.BossRadius,
                        resolved.BossContactDamage,
                        resolved.BossContactIntervalSeconds,
                        resolved.BossExperienceReward,
                        new Color(0.58f, 0.18f, 1f),
                        rangedAttackRange: 7.2f,
                        rangedAttackDamage: resolved.BossContactDamage * 0.55f,
                        rangedAttackIntervalSeconds: 2.4f,
                        preferredRange: 4.5f);
                default:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Swarm,
                        SwarmEnemySpawnableId.Value,
                        "Swarm Thrall",
                        resolved.EnemyMaxHealth,
                        resolved.EnemyMoveSpeed,
                        resolved.EnemyRadius,
                        resolved.EnemyContactDamage,
                        resolved.EnemyContactIntervalSeconds,
                        resolved.EnemyExperienceReward,
                        new Color(0.88f, 0.22f, 0.32f));
            }
        }

        public static IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateWeaponArchetypeDefinitions(SurvivorsTemplateTuning tuning = null)
        {
            SurvivorsTemplateTuning resolved = tuning ?? CreateDefaultTuning();
            return new[]
            {
                new SurvivorsWeaponArchetypeDefinition(
                    ArcaneWandWeaponContentId,
                    "Arc Bolt",
                    SurvivorsWeaponArchetype.Projectile,
                    resolved.WeaponCooldownSeconds,
                    resolved.ProjectileDamage,
                    resolved.WeaponRange,
                    new Color(0.78f, 0.42f, 1f),
                    projectileSpeed: resolved.ProjectileSpeed,
                    projectileRadius: resolved.ProjectileRadius,
                    projectileLifetimeSeconds: resolved.ProjectileLifetimeSeconds,
                    projectilePierceCount: resolved.ProjectilePierceCount,
                    projectileChainCount: resolved.ProjectileChainCount,
                    projectileForkCount: resolved.ProjectileForkCount,
                    projectileReturnCount: resolved.ProjectileReturnCount),
                new SurvivorsWeaponArchetypeDefinition(
                    FrostFanWeaponContentId,
                    "Frost Fan",
                    SurvivorsWeaponArchetype.Projectile,
                    cooldownSeconds: 1.05f,
                    damage: resolved.ProjectileDamage * 0.72f,
                    range: resolved.WeaponRange * 0.82f,
                    tint: new Color(0.52f, 0.86f, 1f),
                    projectileSpeed: resolved.ProjectileSpeed * 0.92f,
                    projectileRadius: resolved.ProjectileRadius * 0.86f,
                    projectileLifetimeSeconds: resolved.ProjectileLifetimeSeconds * 0.85f,
                    projectilePierceCount: 1,
                    projectileFanCount: 5,
                    projectileSpreadDegrees: 68f),
                new SurvivorsWeaponArchetypeDefinition(
                    OrbitWardWeaponContentId,
                    "Blood Ring",
                    SurvivorsWeaponArchetype.Orbit,
                    cooldownSeconds: 0.05f,
                    damage: resolved.OrbitDamage,
                    range: resolved.OrbitBladeHitRadius,
                    tint: new Color(0.38f, 0.9f, 1f),
                    orbitCount: resolved.OrbitBladeCount,
                    orbitRadius: resolved.OrbitRadius,
                    orbitDegreesPerSecond: resolved.OrbitDegreesPerSecond,
                    orbitContactTickIntervalSeconds: resolved.OrbitContactTickIntervalSeconds),
                new SurvivorsWeaponArchetypeDefinition(
                    ThornHaloWeaponContentId,
                    "Thorn Halo",
                    SurvivorsWeaponArchetype.Orbit,
                    cooldownSeconds: 0.05f,
                    damage: resolved.OrbitDamage * 0.78f,
                    range: resolved.OrbitBladeHitRadius * 1.12f,
                    tint: new Color(0.62f, 1f, 0.46f),
                    orbitCount: 2,
                    orbitRadius: resolved.OrbitRadius + 1.25f,
                    orbitDegreesPerSecond: resolved.OrbitDegreesPerSecond * 0.72f,
                    orbitContactTickIntervalSeconds: resolved.OrbitContactTickIntervalSeconds * 0.92f),
                new SurvivorsWeaponArchetypeDefinition(
                    MoonSlashWeaponContentId,
                    "Moon Slash",
                    SurvivorsWeaponArchetype.Melee,
                    resolved.MeleeCooldownSeconds,
                    resolved.MeleeDamage,
                    resolved.MeleeRange,
                    new Color(1f, 0.68f, 0.34f),
                    meleeHitCount: resolved.MeleeHitCount,
                    meleeArcDegrees: resolved.MeleeArcDegrees,
                    meleeVisualDurationSeconds: resolved.MeleeVisualDurationSeconds),
                new SurvivorsWeaponArchetypeDefinition(
                    StarNovaWeaponContentId,
                    "Cinder Burst",
                    SurvivorsWeaponArchetype.Burst,
                    resolved.BurstCooldownSeconds,
                    resolved.BurstDamage,
                    resolved.BurstRadius,
                    new Color(1f, 0.42f, 0.72f),
                    burstCount: resolved.BurstCount,
                    burstRepeatIntervalSeconds: resolved.BurstRepeatIntervalSeconds,
                    burstVisualDurationSeconds: resolved.BurstVisualDurationSeconds),
                new SurvivorsWeaponArchetypeDefinition(
                    StarBeamWeaponContentId,
                    "Star Beam",
                    SurvivorsWeaponArchetype.Hitscan,
                    resolved.HitscanCooldownSeconds,
                    resolved.HitscanDamage,
                    resolved.HitscanRange,
                    new Color(0.3f, 0.88f, 1f),
                    hitscanCount: resolved.HitscanCount,
                    hitscanWidth: resolved.HitscanWidth,
                    hitscanVisualDurationSeconds: resolved.HitscanVisualDurationSeconds,
                    hitscanPierces: resolved.HitscanPierces),
                new SurvivorsWeaponArchetypeDefinition(
                    GravityGrenadeWeaponContentId,
                    "Gravity Grenade",
                    SurvivorsWeaponArchetype.Grenade,
                    resolved.GrenadeCooldownSeconds,
                    resolved.GrenadeDamage,
                    resolved.GrenadeRange,
                    new Color(0.62f, 0.94f, 0.42f),
                    payloadCount: resolved.GrenadePayloadCount,
                    payloadTravelSpeed: resolved.GrenadePayloadTravelSpeed,
                    payloadArmingSeconds: resolved.GrenadePayloadArmingSeconds,
                    payloadLifetimeSeconds: resolved.PlacedPayloadLifetimeSeconds,
                    payloadTriggerRadius: resolved.PlacedPayloadTriggerRadius,
                    payloadExplosionRadius: resolved.GrenadePayloadExplosionRadius,
                    payloadLeavesHazard: true,
                    payloadHazardDurationSeconds: resolved.PayloadHazardDurationSeconds,
                    payloadHazardTickIntervalSeconds: resolved.PayloadHazardTickIntervalSeconds,
                    payloadHazardDamageRatio: resolved.PayloadHazardDamageRatio),
                new SurvivorsWeaponArchetypeDefinition(
                    RuneTrapWeaponContentId,
                    "Rune Trap",
                    SurvivorsWeaponArchetype.Trap,
                    resolved.TrapCooldownSeconds,
                    resolved.PlacedPayloadDamage,
                    resolved.PlacedPayloadRange,
                    new Color(1f, 0.8f, 0.28f),
                    payloadCount: resolved.PlacedPayloadCount,
                    payloadArmingSeconds: resolved.PlacedPayloadArmingSeconds,
                    payloadLifetimeSeconds: resolved.PlacedPayloadLifetimeSeconds,
                    payloadTriggerRadius: resolved.PlacedPayloadTriggerRadius,
                    payloadExplosionRadius: resolved.PlacedPayloadExplosionRadius,
                    payloadPlacementRadius: 1.25f,
                    payloadAutoDetonateAtExpiry: false,
                    payloadLeavesHazard: true,
                    payloadHazardDurationSeconds: resolved.PayloadHazardDurationSeconds,
                    payloadHazardTickIntervalSeconds: resolved.PayloadHazardTickIntervalSeconds,
                    payloadHazardDamageRatio: resolved.PayloadHazardDamageRatio),
                new SurvivorsWeaponArchetypeDefinition(
                    AetherMineWeaponContentId,
                    "Aether Mine",
                    SurvivorsWeaponArchetype.Mine,
                    resolved.MineCooldownSeconds,
                    resolved.PlacedPayloadDamage,
                    resolved.PlacedPayloadRange,
                    new Color(0.95f, 0.48f, 0.92f),
                    payloadCount: resolved.PlacedPayloadCount,
                    payloadArmingSeconds: resolved.PlacedPayloadArmingSeconds,
                    payloadLifetimeSeconds: resolved.PlacedPayloadLifetimeSeconds,
                    payloadTriggerRadius: resolved.PlacedPayloadTriggerRadius,
                    payloadExplosionRadius: resolved.PlacedPayloadExplosionRadius,
                    payloadPlacementRadius: 1.8f,
                    payloadAutoDetonateAtExpiry: true,
                    payloadLeavesHazard: false)
            };
        }

        public static EncounterDefinition CreateEncounterDefinition()
        {
            return new EncounterDefinition(
                new EncounterId("encounter.survivors.first-slice"),
                Array.Empty<WeightedSpawnTableDefinition>(),
                new[]
                {
                    new WaveDefinition(
                        new WaveId("wave.survivors.opening-ring"),
                        0,
                        new[]
                        {
                            SpawnGroupDefinition.Fixed(
                                new SpawnGroupId("group.survivors.opening-swarm"),
                                new SpawnableId(SwarmEnemySpawnableId.Value),
                                count: 128,
                                batchSize: 1,
                                initialDelayTicks: 0,
                                intervalTicks: 57,
                                channelId: new SpawnChannelId(RadialSpawnChannelId.Value))
                        })
                },
                new[]
                {
                    ObjectiveDefinition.Manual(new EncounterObjectiveId("objective.survivors.keep-running"), ObjectiveRole.Completion)
                },
                seed: 20260624);
        }

        public static RunUpgradeCatalog CreateRunUpgradeCatalog()
        {
            return new RunUpgradeCatalog(new[]
            {
                Upgrade("upgrade.survivors.arcane-damage", RunUpgradeRarity.Common, 70, 8, DamageBonusEffect, WeaponTarget, 2.0d),
                Upgrade("upgrade.survivors.quick-casting", RunUpgradeRarity.Common, 60, 6, FireRateEffect, WeaponTarget, -0.08d),
                Upgrade("upgrade.survivors.swift-steps", RunUpgradeRarity.Uncommon, 44, 5, MoveSpeedEffect, PlayerTarget, 0.45d),
                Upgrade("upgrade.survivors.gem-magnet", RunUpgradeRarity.Uncommon, 44, 5, MagnetRangeEffect, PickupTarget, 1.1d),
                Upgrade("upgrade.survivors.iron-blood", RunUpgradeRarity.Rare, 28, 4, MaxHealthEffect, PlayerTarget, 8.0d),
                Upgrade(FrostFanUpgradeId, RunUpgradeRarity.Uncommon, 40, 3, ProjectileFanEffect, FrostFanWeaponTarget, 1.0d),
                Upgrade(OrbitingFocusUpgradeId, RunUpgradeRarity.Uncommon, 38, 4, OrbitBladeEffect, OrbitWeaponTarget, 1.0d),
                Upgrade(ThornHaloUpgradeId, RunUpgradeRarity.Rare, 30, 3, OrbitRadiusEffect, ThornHaloWeaponTarget, 0.35d),
                Upgrade(CrescentChainUpgradeId, RunUpgradeRarity.Uncommon, 34, 4, MeleeTargetEffect, MeleeWeaponTarget, 1.0d),
                Upgrade(NovaEchoUpgradeId, RunUpgradeRarity.Rare, 24, 3, BurstCountEffect, BurstWeaponTarget, 1.0d),
                Upgrade(CinderEchoUpgradeId, RunUpgradeRarity.Rare, 24, 3, BurstEchoEffect, BurstWeaponTarget, 1.0d),
                Upgrade(TargetedSigilUpgradeId, RunUpgradeRarity.Rare, 22, 1, TargetedBurstEffect, BurstWeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.piercing-bolts", RunUpgradeRarity.Uncommon, 36, 4, ProjectilePierceEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.chain-bolts", RunUpgradeRarity.Rare, 26, 3, ProjectileChainEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.forked-bolts", RunUpgradeRarity.Rare, 24, 3, ProjectileForkEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.returning-bolts", RunUpgradeRarity.Rare, 22, 2, ProjectileReturnEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.distilled-poison", RunUpgradeRarity.Uncommon, 34, 5, PoisonEffect, StatusTarget, 0.16d),
                Upgrade("upgrade.survivors.hemorrhage-edge", RunUpgradeRarity.Uncommon, 32, 5, BleedEffect, StatusTarget, 0.18d),
                Upgrade("upgrade.survivors.execution-rite", RunUpgradeRarity.Rare, 22, 3, ExecuteEffect, StatusTarget, 0.08d),
                Upgrade("upgrade.survivors.sanguine-feast", RunUpgradeRarity.Rare, 24, 4, LifestealEffect, PlayerTarget, 0.035d),
                Upgrade("upgrade.survivors.guardian-barrier", RunUpgradeRarity.Uncommon, 30, 4, BarrierCapacityEffect, BarrierTarget, 6.0d),
                Upgrade("upgrade.survivors.ward-recovery", RunUpgradeRarity.Uncommon, 28, 4, BarrierRegenEffect, BarrierTarget, 0.18d),
                Upgrade("upgrade.survivors.mirror-bulwark", RunUpgradeRarity.Rare, 20, 3, BarrierOnDamageEffect, BarrierTarget, 0.04d),
                Upgrade(ArcaneThesisUpgradeId, RunUpgradeRarity.Uncommon, 34, 4, DamageBonusEffect, WeaponTarget, 1.6d),
                Upgrade(FrostNeedleworkUpgradeId, RunUpgradeRarity.Uncommon, 32, 3, ProjectilePierceEffect, FrostFanWeaponTarget, 1.0d),
                Upgrade(BloodRingCanticleUpgradeId, RunUpgradeRarity.Rare, 22, 3, OrbitBladeEffect, OrbitWeaponTarget, 1.0d),
                Upgrade(CinderScriptUpgradeId, RunUpgradeRarity.Rare, 22, 2, TargetedBurstEffect, BurstWeaponTarget, 1.0d),
                Upgrade(EmberForgeHeartUpgradeId, RunUpgradeRarity.Uncommon, 34, 4, DamageBonusEffect, BurstWeaponTarget, 2.2d),
                Upgrade(EmberTempoUpgradeId, RunUpgradeRarity.Uncommon, 30, 3, FireRateEffect, HitscanWeaponTarget, -0.06d),
                Upgrade(SiegePayloadsUpgradeId, RunUpgradeRarity.Rare, 20, 2, PayloadCountEffect, PayloadWeaponTarget, 1.0d),
                Upgrade(EmberWardUpgradeId, RunUpgradeRarity.Rare, 20, 3, BarrierCapacityEffect, BarrierTarget, 8.0d),
                Upgrade(PrismaticBeamUpgradeId, RunUpgradeRarity.Uncommon, 30, 3, HitscanPierceEffect, HitscanWeaponTarget, 1.0d),
                Upgrade(ExtraPayloadUpgradeId, RunUpgradeRarity.Rare, 22, 2, PayloadCountEffect, PayloadWeaponTarget, 1.0d),
                Upgrade(BiggerBoomsUpgradeId, RunUpgradeRarity.Uncommon, 32, 4, PayloadRadiusEffect, PayloadWeaponTarget, 0.45d),
                Upgrade(WiderTriggersUpgradeId, RunUpgradeRarity.Uncommon, 28, 4, PayloadTriggerRadiusEffect, PayloadWeaponTarget, 0.35d)
            });
        }

        public static IReadOnlyList<RunUpgradeTargetId> CreateKnownUpgradeTargets()
        {
            return new[]
            {
                PlayerTarget,
                WeaponTarget,
                FrostFanWeaponTarget,
                OrbitWeaponTarget,
                ThornHaloWeaponTarget,
                MeleeWeaponTarget,
                BurstWeaponTarget,
                HitscanWeaponTarget,
                GrenadeWeaponTarget,
                TrapWeaponTarget,
                MineWeaponTarget,
                PayloadWeaponTarget,
                PickupTarget,
                StatusTarget,
                BarrierTarget
            };
        }

        public static IReadOnlyList<SurvivorsRelicDefinition> CreateRelicDefinitions()
        {
            return new[]
            {
                new SurvivorsRelicDefinition(
                    BloodStarRelicId,
                    "Blood Star Relic",
                    WeaponTarget.Value,
                    DamageBonusEffect.Value,
                    SurvivorsRelicEffectKind.DamageBonus,
                    amount: 3f,
                    weight: 60),
                new SurvivorsRelicDefinition(
                    QuickenedSigilRelicId,
                    "Quickened Sigil",
                    WeaponTarget.Value,
                    FireRateEffect.Value,
                    SurvivorsRelicEffectKind.CooldownMultiplier,
                    amount: -0.12f,
                    weight: 45),
                new SurvivorsRelicDefinition(
                    GemheartCharmRelicId,
                    "Gemheart Charm",
                    PickupTarget.Value,
                    MagnetRangeEffect.Value,
                    SurvivorsRelicEffectKind.PickupRange,
                    amount: 1.25f,
                    weight: 40),
                new SurvivorsRelicDefinition(
                    "relic.survivors.monolith-splinter",
                    "Monolith Splinter",
                    WeaponTarget.Value,
                    DamageBonusEffect.Value,
                    SurvivorsRelicEffectKind.DamageBonus,
                    amount: 5f,
                    weight: 28),
                new SurvivorsRelicDefinition(
                    "relic.survivors.fever-engine",
                    "Fever Engine",
                    BurstWeaponTarget.Value,
                    FireRateEffect.Value,
                    SurvivorsRelicEffectKind.CooldownMultiplier,
                    amount: -0.18f,
                    weight: 26),
                new SurvivorsRelicDefinition(
                    "relic.survivors.vacuum-lattice",
                    "Vacuum Lattice",
                    PickupTarget.Value,
                    MagnetRangeEffect.Value,
                    SurvivorsRelicEffectKind.PickupRange,
                    amount: 2.25f,
                    weight: 24)
            };
        }

        public static SurvivorsClassLibraryDefinition CreateClassLibraryDefinition()
        {
            return new SurvivorsClassLibraryDefinition(new[]
            {
                new SurvivorsClassDefinition(
                    DefaultClassId,
                    "Arcane Initiate",
                    ArcaneWandWeaponContentId,
                    isUnlockedByDefault: true,
                    unlockRewardId: string.Empty,
                    startingStatModifiers: Array.Empty<SurvivorsClassStatModifierDefinition>(),
                    startingWeaponIds: new[]
                    {
                        ArcaneWandWeaponContentId,
                        FrostFanWeaponContentId,
                        OrbitWardWeaponContentId,
                        ThornHaloWeaponContentId,
                        StarNovaWeaponContentId
                    }),
                new SurvivorsClassDefinition(
                    EmberVanguardClassId,
                    "Ember Vanguard",
                    StarBeamWeaponContentId,
                    isUnlockedByDefault: false,
                    unlockRewardId: EmberVanguardUnlockRewardId,
                    startingStatModifiers: new[]
                    {
                        new SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind.MoveSpeed, 0.65f),
                        new SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind.Damage, 1.25f),
                        new SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind.MaxHealth, 6f)
                    },
                    startingWeaponIds: new[]
                    {
                        ArcaneWandWeaponContentId,
                        FrostFanWeaponContentId,
                        OrbitWardWeaponContentId,
                        ThornHaloWeaponContentId,
                        MoonSlashWeaponContentId,
                        StarNovaWeaponContentId,
                        StarBeamWeaponContentId,
                        GravityGrenadeWeaponContentId,
                        RuneTrapWeaponContentId,
                        AetherMineWeaponContentId
                    })
            }, DefaultClassId);
        }

        public static IReadOnlyList<SurvivorsClassUpgradeGateDefinition> CreateClassUpgradeGates()
        {
            var gates = new List<SurvivorsClassUpgradeGateDefinition>();
            IReadOnlyList<SurvivorsProgressionTrackDefinition> tracks = CreateProgressionTrackDefinitions();
            for (int trackIndex = 0; trackIndex < tracks.Count; trackIndex++)
            {
                SurvivorsProgressionTrackDefinition track = tracks[trackIndex];
                if (track == null || !track.IsClassSpecific)
                {
                    continue;
                }

                for (int nodeIndex = 0; nodeIndex < track.Nodes.Count; nodeIndex++)
                {
                    SurvivorsProgressionNodeDefinition node = track.Nodes[nodeIndex];
                    if (node != null)
                    {
                        AddClassGate(gates, node.UpgradeId, track.ClassId);
                    }
                }
            }

            return gates.ToArray();
        }

        public static IReadOnlyList<SurvivorsProgressionTrackDefinition> CreateProgressionTrackDefinitions()
        {
            return new[]
            {
                Track(
                    "progression.survivors.arcane-initiate.passives",
                    "Arcane Initiate Passive Atlas",
                    SurvivorsProgressionTrackKind.PassiveAtlas,
                    DefaultClassId,
                    string.Empty,
                    Node("node.survivors.arcane-initiate.arcane-thesis", "Arcane Thesis", ArcaneThesisUpgradeId, SurvivorsProgressionNodeKind.Passive, 0, 1, 4),
                    Node("node.survivors.arcane-initiate.frost-needlework", "Frost Needlework", FrostNeedleworkUpgradeId, SurvivorsProgressionNodeKind.Passive, 1, 1, 3),
                    Node("node.survivors.arcane-initiate.blood-ring-canticle", "Blood Ring Canticle", BloodRingCanticleUpgradeId, SurvivorsProgressionNodeKind.Passive, 2, 2, 3),
                    Node("node.survivors.arcane-initiate.cinder-script", "Cinder Script", CinderScriptUpgradeId, SurvivorsProgressionNodeKind.Passive, 3, 2, 2)),
                Track(
                    "progression.survivors.ember-vanguard.passives",
                    "Ember Vanguard Passive Atlas",
                    SurvivorsProgressionTrackKind.PassiveAtlas,
                    EmberVanguardClassId,
                    string.Empty,
                    Node("node.survivors.ember-vanguard.forge-heart", "Forge Heart", EmberForgeHeartUpgradeId, SurvivorsProgressionNodeKind.Passive, 0, 1, 4),
                    Node("node.survivors.ember-vanguard.battle-tempo", "Ember Tempo", EmberTempoUpgradeId, SurvivorsProgressionNodeKind.Passive, 1, 1, 3),
                    Node("node.survivors.ember-vanguard.siege-payloads", "Siege Payloads", SiegePayloadsUpgradeId, SurvivorsProgressionNodeKind.Passive, 2, 2, 2),
                    Node("node.survivors.ember-vanguard.ember-ward", "Ember Ward", EmberWardUpgradeId, SurvivorsProgressionNodeKind.Passive, 3, 2, 3)),
                Track(
                    "progression.survivors.arc-bolt.weapon",
                    "Arc Bolt Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    ArcaneWandWeaponContentId,
                    Node("node.survivors.arc-bolt.damage", "Arcane Damage", "upgrade.survivors.arcane-damage", SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 8),
                    Node("node.survivors.arc-bolt.pierce", "Piercing Bolts", "upgrade.survivors.piercing-bolts", SurvivorsProgressionNodeKind.WeaponMutation, 1, 1, 4),
                    Node("node.survivors.arc-bolt.chain", "Chain Bolts", "upgrade.survivors.chain-bolts", SurvivorsProgressionNodeKind.WeaponMutation, 2, 2, 3),
                    Node("node.survivors.arc-bolt.return", "Returning Bolts", "upgrade.survivors.returning-bolts", SurvivorsProgressionNodeKind.WeaponMutation, 3, 2, 2)),
                Track(
                    "progression.survivors.frost-fan.weapon",
                    "Frost Fan Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    FrostFanWeaponContentId,
                    Node("node.survivors.frost-fan.unlock", "Frost Fan", FrostFanUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 3),
                    Node("node.survivors.frost-fan.needles", "Frost Needlework", FrostNeedleworkUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 1, 3)),
                Track(
                    "progression.survivors.blood-ring.weapon",
                    "Blood Ring Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    OrbitWardWeaponContentId,
                    Node("node.survivors.blood-ring.focus", "Orbiting Focus", OrbitingFocusUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 4),
                    Node("node.survivors.blood-ring.canticle", "Blood Ring Canticle", BloodRingCanticleUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 2, 3)),
                Track(
                    "progression.survivors.thorn-halo.weapon",
                    "Thorn Halo Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    ThornHaloWeaponContentId,
                    Node("node.survivors.thorn-halo.wall", "Thorn Halo Wall", ThornHaloUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 0, 1, 3)),
                Track(
                    "progression.survivors.cinder-burst.weapon",
                    "Cinder Burst Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    StarNovaWeaponContentId,
                    Node("node.survivors.cinder-burst.nova-echo", "Nova Echo", NovaEchoUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 3),
                    Node("node.survivors.cinder-burst.cinder-echoes", "Cinder Echoes", CinderEchoUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 2, 3),
                    Node("node.survivors.cinder-burst.targeted-sigils", "Targeted Burst Sigils", TargetedSigilUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 2, 1)),
                Track(
                    "progression.survivors.ember-vanguard.star-beam.weapon",
                    "Ember Vanguard Star Beam Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    EmberVanguardClassId,
                    StarBeamWeaponContentId,
                    Node("node.survivors.ember-vanguard.star-beam.prismatic-beam", "Prismatic Beam", PrismaticBeamUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 0, 1, 3)),
                Track(
                    "progression.survivors.ember-vanguard.moon-slash.weapon",
                    "Ember Vanguard Moon Slash Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    EmberVanguardClassId,
                    MoonSlashWeaponContentId,
                    Node("node.survivors.ember-vanguard.moon-slash.crescent-chain", "Crescent Chain", CrescentChainUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 0, 1, 4)),
                Track(
                    "progression.survivors.ember-vanguard.payloads.weapon",
                    "Ember Vanguard Payload Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    EmberVanguardClassId,
                    GravityGrenadeWeaponContentId,
                    Node("node.survivors.ember-vanguard.payloads.extra", "Extra Payload", ExtraPayloadUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 2),
                    Node("node.survivors.ember-vanguard.payloads.siege", "Siege Payloads", SiegePayloadsUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 1, 2, 2),
                    Node("node.survivors.ember-vanguard.payloads.bigger-booms", "Bigger Booms", BiggerBoomsUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 4),
                    Node("node.survivors.ember-vanguard.payloads.wider-triggers", "Wider Triggers", WiderTriggersUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 4))
            };
        }

        private static void AddClassGate(List<SurvivorsClassUpgradeGateDefinition> gates, string upgradeId, string classId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId) || string.IsNullOrWhiteSpace(classId))
            {
                return;
            }

            for (int i = 0; i < gates.Count; i++)
            {
                SurvivorsClassUpgradeGateDefinition gate = gates[i];
                if (gate != null && string.Equals(gate.UpgradeId, upgradeId, StringComparison.Ordinal))
                {
                    var classIds = new List<string>(gate.AllowedClassIds.Count + 1);
                    for (int classIndex = 0; classIndex < gate.AllowedClassIds.Count; classIndex++)
                    {
                        if (string.Equals(gate.AllowedClassIds[classIndex], classId, StringComparison.Ordinal))
                        {
                            return;
                        }

                        classIds.Add(gate.AllowedClassIds[classIndex]);
                    }

                    classIds.Add(classId);
                    gates[i] = new SurvivorsClassUpgradeGateDefinition(upgradeId, classIds);
                    return;
                }
            }

            gates.Add(new SurvivorsClassUpgradeGateDefinition(upgradeId, new[] { classId }));
        }

        private static SurvivorsProgressionTrackDefinition Track(
            string id,
            string displayName,
            SurvivorsProgressionTrackKind kind,
            string classId,
            string targetWeaponId,
            params SurvivorsProgressionNodeDefinition[] nodes)
        {
            return new SurvivorsProgressionTrackDefinition(id, displayName, kind, classId, targetWeaponId, nodes);
        }

        private static SurvivorsProgressionNodeDefinition Node(
            string id,
            string displayName,
            string upgradeId,
            SurvivorsProgressionNodeKind kind,
            int tier,
            int pointCost,
            int maxRank)
        {
            return new SurvivorsProgressionNodeDefinition(id, displayName, upgradeId, kind, tier, pointCost, maxRank);
        }

        public static SurvivorsMetaProgressionDefinition CreateMetaProgressionDefinition()
        {
            return new SurvivorsMetaProgressionDefinition(
                BloodShardsCurrencyId,
                LegacyExperienceTrackId,
                new[]
                {
                    new SurvivorsPersistentUpgradeDefinition(
                        ArcaneLegacyMetaUpgradeId,
                        "Arcane Legacy",
                        WeaponTarget.Value,
                        MetaDamageEffectId,
                        maxRank: 3,
                        rankCosts: new[] { 5, 8, 13 },
                        damageBonusPerRank: 1.5f),
                    new SurvivorsPersistentUpgradeDefinition(
                        new ResearchNodeId("meta.survivors.battle-tempo"),
                        "Battle Tempo",
                        WeaponTarget.Value,
                        MetaDamageEffectId,
                        maxRank: 3,
                        rankCosts: new[] { 6, 10, 16 },
                        damageBonusPerRank: 1.0f),
                    new SurvivorsPersistentUpgradeDefinition(
                        new ResearchNodeId("meta.survivors.boss-hunters-ledger"),
                        "Boss Hunter's Ledger",
                        WeaponTarget.Value,
                        MetaDamageEffectId,
                        maxRank: 2,
                        rankCosts: new[] { 12, 20 },
                        damageBonusPerRank: 2.25f)
                },
                new[]
                {
                    new SurvivorsRewardDefinition(MinibossRewardId, BloodShardsCurrencyId, 4, LegacyExperienceTrackId, 25),
                    new SurvivorsRewardDefinition(BossRewardId, BloodShardsCurrencyId, 18, LegacyExperienceTrackId, 120)
                });
        }

        public static string GetUpgradeDisplayName(RunUpgradeId id)
        {
            string value = id.Value;
            if (value == "upgrade.survivors.arcane-damage") return "Arcane Damage";
            if (value == "upgrade.survivors.quick-casting") return "Quick Casting";
            if (value == "upgrade.survivors.swift-steps") return "Swift Steps";
            if (value == "upgrade.survivors.gem-magnet") return "Gem Magnet";
            if (value == "upgrade.survivors.iron-blood") return "Iron Blood";
            if (value == "upgrade.survivors.frost-fan") return "Frost Fan";
            if (value == "upgrade.survivors.orbiting-focus") return "Orbiting Focus";
            if (value == "upgrade.survivors.thorn-halo-wall") return "Thorn Halo Wall";
            if (value == "upgrade.survivors.crescent-chain") return "Crescent Chain";
            if (value == "upgrade.survivors.nova-echo") return "Nova Echo";
            if (value == "upgrade.survivors.cinder-echoes") return "Cinder Echoes";
            if (value == "upgrade.survivors.targeted-burst-sigils") return "Targeted Burst Sigils";
            if (value == "upgrade.survivors.piercing-bolts") return "Piercing Bolts";
            if (value == "upgrade.survivors.chain-bolts") return "Chain Bolts";
            if (value == "upgrade.survivors.forked-bolts") return "Forked Bolts";
            if (value == "upgrade.survivors.returning-bolts") return "Returning Bolts";
            if (value == "upgrade.survivors.distilled-poison") return "Distilled Poison";
            if (value == "upgrade.survivors.hemorrhage-edge") return "Hemorrhage Edge";
            if (value == "upgrade.survivors.execution-rite") return "Execution Rite";
            if (value == "upgrade.survivors.sanguine-feast") return "Sanguine Feast";
            if (value == "upgrade.survivors.guardian-barrier") return "Guardian Barrier";
            if (value == "upgrade.survivors.ward-recovery") return "Ward Recovery";
            if (value == "upgrade.survivors.mirror-bulwark") return "Mirror Bulwark";
            if (value == "upgrade.survivors.arcane-thesis") return "Arcane Thesis";
            if (value == "upgrade.survivors.frost-needlework") return "Frost Needlework";
            if (value == "upgrade.survivors.blood-ring-canticle") return "Blood Ring Canticle";
            if (value == "upgrade.survivors.cinder-script") return "Cinder Script";
            if (value == "upgrade.survivors.ember-forge-heart") return "Ember Forge Heart";
            if (value == "upgrade.survivors.ember-tempo") return "Ember Tempo";
            if (value == "upgrade.survivors.siege-payloads") return "Siege Payloads";
            if (value == "upgrade.survivors.ember-ward") return "Ember Ward";
            if (value == "upgrade.survivors.prismatic-beam") return "Prismatic Beam";
            if (value == "upgrade.survivors.extra-payload") return "Extra Payload";
            if (value == "upgrade.survivors.bigger-booms") return "Bigger Booms";
            if (value == "upgrade.survivors.wider-triggers") return "Wider Triggers";
            return value;
        }

        private static RunUpgradeDefinition Upgrade(string id, RunUpgradeRarity rarity, int weight, int maxRank, RunUpgradeEffectId effect, RunUpgradeTargetId target, double amount)
        {
            return new RunUpgradeDefinition(
                new RunUpgradeId(id),
                rarity,
                weight,
                maxRank,
                new[] { new RunUpgradeEffectDescriptor(effect, target, amount) });
        }
    }
}
