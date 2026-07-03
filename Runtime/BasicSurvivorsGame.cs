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

    public enum SurvivorsPacingProfile
    {
        HumanPlaytest = 0,
        Normal = 1,
        DebugFast = 2,
        Showcase = 3
    }

    [Serializable]
    public sealed class SurvivorsTemplateTuning
    {
        public SurvivorsPacingProfile PacingProfile = SurvivorsPacingProfile.HumanPlaytest;
        public float PlayerMoveSpeed = 5.4f;
        public float PlayerRadius = 0.55f;
        public float PlayerMaxHealth = 44f;
        public float PlayerContactInvulnerabilitySeconds = 0.35f;
        public float EnemySpawnRadius = 13.75f;
        public float EnemySpawnIntervalSeconds = 1.15f;
        public int EnemyMaximumAlive = 34;
        public float EnemyMaxHealth = 14f;
        public float EnemyMoveSpeed = 1.35f;
        public float EnemyRadius = 0.48f;
        public float EnemyContactDamage = 3.5f;
        public float EnemyContactIntervalSeconds = 0.7f;
        public int EnemyExperienceReward = 2;
        public float RunEscalationIntervalSeconds = 45f;
        public float MinimumEnemySpawnIntervalSeconds = 0.35f;
        public float EnemySpawnIntervalReductionPerEscalation = 0.08f;
        public int EnemyMaximumAliveIncreasePerEscalation = 8;
        public float EnemyHealthMultiplierPerEscalation = 0.12f;
        public float EnemyMoveSpeedMultiplierPerEscalation = 0.02f;
        public float EnemyExperienceMultiplierPerEscalation = 0.08f;
        public float MajorThreatWarningLeadSeconds = 8f;
        public float FirstEliteSpawnTimeSeconds = 180f;
        public float FirstDreadEliteSpawnTimeSeconds = 300f;
        public float MinibossSpawnTimeSeconds = 420f;
        public float MinibossMaxHealth = 260f;
        public float MinibossMoveSpeed = 1.35f;
        public float MinibossRadius = 0.82f;
        public float MinibossContactDamage = 8f;
        public float MinibossContactIntervalSeconds = 0.85f;
        public int MinibossExperienceReward = 40;
        public float BossSpawnTimeSeconds = 1200f;
        public float BossMaxHealth = 1400f;
        public float BossMoveSpeed = 1.25f;
        public float BossRadius = 1.18f;
        public float BossContactDamage = 16f;
        public float BossContactIntervalSeconds = 0.95f;
        public int BossExperienceReward = 140;
        public float SurvivalVictoryTimeSeconds = 1800f;
        public float WeaponCooldownSeconds = 0.85f;
        public float WeaponRange = 11.5f;
        public float ProjectileDamage = 5.8f;
        public float ProjectileSpeed = 8.5f;
        public float ProjectileRadius = 0.22f;
        public float ProjectileLifetimeSeconds = 2f;
        public int ProjectilePierceCount = 0;
        public int ProjectileChainCount = 0;
        public int ProjectileForkCount = 0;
        public int ProjectileReturnCount = 0;
        public float OrbitDamage = 1.2f;
        public int OrbitBladeCount = 1;
        public float OrbitRadius = 2.1f;
        public float OrbitBladeHitRadius = 0.38f;
        public float OrbitDegreesPerSecond = 95f;
        public float OrbitContactTickIntervalSeconds = 0.46f;
        public float MeleeDamage = 3.2f;
        public float MeleeCooldownSeconds = 1.8f;
        public float MeleeRange = 2.5f;
        public float MeleeArcDegrees = 120f;
        public int MeleeHitCount = 2;
        public float MeleeVisualDurationSeconds = 0.16f;
        public float BurstDamage = 2.4f;
        public float BurstCooldownSeconds = 4.2f;
        public float BurstRadius = 2.55f;
        public int BurstCount = 1;
        public float BurstRepeatIntervalSeconds = 0.18f;
        public float BurstVisualDurationSeconds = 0.22f;
        public float HitscanDamage = 3.5f;
        public float HitscanCooldownSeconds = 2.2f;
        public float HitscanRange = 9.5f;
        public int HitscanCount = 1;
        public float HitscanWidth = 0.24f;
        public float HitscanVisualDurationSeconds = 0.08f;
        public bool HitscanPierces = false;
        public float GrenadeDamage = 4.5f;
        public float GrenadeCooldownSeconds = 4.2f;
        public float GrenadeRange = 7f;
        public int GrenadePayloadCount = 1;
        public float GrenadePayloadTravelSpeed = 5.4f;
        public float GrenadePayloadArmingSeconds = 1f;
        public float GrenadePayloadExplosionRadius = 2.1f;
        public float PlacedPayloadDamage = 4.2f;
        public float TrapCooldownSeconds = 4.4f;
        public float MineCooldownSeconds = 4.8f;
        public float PlacedPayloadRange = 5.8f;
        public int PlacedPayloadCount = 1;
        public float PlacedPayloadArmingSeconds = 1f;
        public float PlacedPayloadLifetimeSeconds = 4.2f;
        public float PlacedPayloadTriggerRadius = 1.2f;
        public float PlacedPayloadExplosionRadius = 1.95f;
        public float PayloadHazardDurationSeconds = 1.6f;
        public float PayloadHazardTickIntervalSeconds = 0.45f;
        public float PayloadHazardDamageRatio = 0.18f;
        public float PickupAttractRange = 2.5f;
        public float PickupAttractionSpeed = 4.8f;
        public float PickupCollectRadius = 0.68f;
        public float MagnetRecallSpeedMultiplier = 1.65f;
        public int ExperienceRequiredBase = 8;
        public int ExperienceRequiredPerLevel = 5;
        public float StatusPoisonDurationSeconds = 4.5f;
        public float StatusBleedDurationSeconds = 5.5f;
        public float StartingBarrierCapacity = 8f;
        public float BaseBarrierRegenPerSecond = 0.12f;
        public int DraftChoiceCount = 3;
        public int DraftRerollCharges = 2;
        public int DraftBanishCharges = 2;
        public int DraftSkipBloodShards = 1;
        public int DraftMidRarityLevel = 6;
        public int DraftLateRarityLevel = 12;
        public int NormalEarlyCommonWeight = 120;
        public int NormalEarlyUncommonWeight = 60;
        public int NormalEarlyRareWeight = 12;
        public int NormalEarlyEpicWeight = 1;
        public int NormalEarlyLegendaryWeight = 0;
        public int NormalMidCommonWeight = 80;
        public int NormalMidUncommonWeight = 90;
        public int NormalMidRareWeight = 38;
        public int NormalMidEpicWeight = 8;
        public int NormalMidLegendaryWeight = 0;
        public int NormalLateCommonWeight = 55;
        public int NormalLateUncommonWeight = 80;
        public int NormalLateRareWeight = 60;
        public int NormalLateEpicWeight = 22;
        public int NormalLateLegendaryWeight = 4;
        public int EliteCommonWeight = 0;
        public int EliteUncommonWeight = 80;
        public int EliteRareWeight = 80;
        public int EliteEpicWeight = 35;
        public int EliteLegendaryWeight = 8;
        public int BossCommonWeight = 0;
        public int BossUncommonWeight = 0;
        public int BossRareWeight = 90;
        public int BossEpicWeight = 85;
        public int BossLegendaryWeight = 90;
        public float RewardSelectionTimeoutSeconds = 0f;
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
        public static readonly ResearchNodeId VitalWardMetaUpgradeId = new ResearchNodeId("meta.survivors.vital-ward");
        public static readonly ResearchNodeId GemheartLegacyMetaUpgradeId = new ResearchNodeId("meta.survivors.gemheart-legacy");
        public static readonly ResearchNodeId ScholarIndexMetaUpgradeId = new ResearchNodeId("meta.survivors.scholar-index");
        public static readonly ResearchNodeId PreparedDraftMetaUpgradeId = new ResearchNodeId("meta.survivors.prepared-draft");
        public const string MetaDamageEffectId = "survivors.meta.damage.flat";
        public const string MetaMaxHealthEffectId = "survivors.meta.player.max_health";
        public const string MetaPickupRangeEffectId = "survivors.meta.pickup.range";
        public const string MetaExperienceGainEffectId = "survivors.meta.experience.gain_multiplier";
        public const string MetaDraftRerollEffectId = "survivors.meta.draft.reroll_charge";
        public const string EliteRewardId = "reward.survivors.elite-clear";
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
        public const string FrostSplinterUpgradeId = "upgrade.survivors.frost-splinter";
        public const string FrostRicochetUpgradeId = "upgrade.survivors.frost-ricochet";
        public const string ThornHaloUpgradeId = "upgrade.survivors.thorn-halo-wall";
        public const string HaloSpiralUpgradeId = "upgrade.survivors.halo-spiral";
        public const string MoonlitEdgeUpgradeId = "upgrade.survivors.moonlit-edge";
        public const string CrescentChainUpgradeId = "upgrade.survivors.crescent-chain";
        public const string LunarTempoUpgradeId = "upgrade.survivors.lunar-tempo";
        public const string NovaEchoUpgradeId = "upgrade.survivors.nova-echo";
        public const string CinderEchoUpgradeId = "upgrade.survivors.cinder-echoes";
        public const string TargetedSigilUpgradeId = "upgrade.survivors.targeted-burst-sigils";
        public const string StarFocusUpgradeId = "upgrade.survivors.star-focus";
        public const string StarPulseUpgradeId = "upgrade.survivors.star-pulse";
        public const string PrismaticBeamUpgradeId = "upgrade.survivors.prismatic-beam";
        public const string ExtraPayloadUpgradeId = "upgrade.survivors.extra-payload";
        public const string BiggerBoomsUpgradeId = "upgrade.survivors.bigger-booms";
        public const string WiderTriggersUpgradeId = "upgrade.survivors.wider-triggers";
        public const string RuneLatticeUpgradeId = "upgrade.survivors.rune-lattice";
        public const string SnaringRunesUpgradeId = "upgrade.survivors.snaring-runes";
        public const string AetherBloomUpgradeId = "upgrade.survivors.aether-bloom";
        public const string ArcaneThesisUpgradeId = "upgrade.survivors.arcane-thesis";
        public const string FrostNeedleworkUpgradeId = "upgrade.survivors.frost-needlework";
        public const string BloodRingCanticleUpgradeId = "upgrade.survivors.blood-ring-canticle";
        public const string CinderScriptUpgradeId = "upgrade.survivors.cinder-script";
        public const string EmberForgeHeartUpgradeId = "upgrade.survivors.ember-forge-heart";
        public const string EmberTempoUpgradeId = "upgrade.survivors.ember-tempo";
        public const string MoonOathUpgradeId = "upgrade.survivors.moon-oath";
        public const string SiegePayloadsUpgradeId = "upgrade.survivors.siege-payloads";
        public const string EmberWardUpgradeId = "upgrade.survivors.ember-ward";
        public const string ScholarsLensUpgradeId = "upgrade.survivors.scholars-lens";
        public const string GiantRuneUpgradeId = "upgrade.survivors.giant-rune";
        public const string TwinCharmUpgradeId = "upgrade.survivors.twin-charm";
        public const string AstralConvergenceUpgradeId = "upgrade.survivors.astral-convergence";
        public const string StarBeamUnlockUpgradeId = "upgrade.survivors.weapon.star-beam";
        public const string GravityGrenadeUnlockUpgradeId = "upgrade.survivors.weapon.gravity-grenade";
        public const string ArcaneStormEvolutionUpgradeId = "upgrade.survivors.evolution.arcane-storm";
        public const string BlizzardCrownEvolutionUpgradeId = "upgrade.survivors.evolution.blizzard-crown";
        public const string CrimsonAegisEvolutionUpgradeId = "upgrade.survivors.evolution.crimson-aegis";
        public const string InfernoHeartEvolutionUpgradeId = "upgrade.survivors.evolution.inferno-heart";
        public const string TempestPrismEvolutionUpgradeId = "upgrade.survivors.evolution.tempest-prism";
        public const string GravefieldEngineEvolutionUpgradeId = "upgrade.survivors.evolution.gravefield-engine";
        public const string EclipseWaltzEvolutionUpgradeId = "upgrade.survivors.evolution.eclipse-waltz";
        public const string AetherfieldMatrixEvolutionUpgradeId = "upgrade.survivors.evolution.aetherfield-matrix";
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
        public static readonly RunUpgradeEffectId ExperienceGainEffect = new RunUpgradeEffectId("survivors.experience.gain_multiplier");
        public static readonly RunUpgradeEffectId AreaRadiusEffect = new RunUpgradeEffectId("survivors.area.radius");
        public static readonly RunUpgradeEffectId WeaponUnlockEffect = new RunUpgradeEffectId("survivors.weapon.unlock");
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
        public static readonly RunUpgradeTargetId ExperienceTarget = new RunUpgradeTargetId("survivors.experience");
        public static readonly RunUpgradeTargetId AreaTarget = new RunUpgradeTargetId("survivors.area");

        public static SurvivorsTemplateTuning CreateDefaultTuning()
        {
            return CreateTuning(SurvivorsPacingProfile.HumanPlaytest);
        }

        public static string GetPacingProfileDisplayName(SurvivorsPacingProfile profile)
        {
            switch (profile)
            {
                case SurvivorsPacingProfile.HumanPlaytest:
                    return "Human Playtest";
                case SurvivorsPacingProfile.DebugFast:
                    return "Debug Fast";
                case SurvivorsPacingProfile.Showcase:
                    return "Showcase";
                case SurvivorsPacingProfile.Normal:
                    return "Normal";
                default:
                    return profile.ToString();
            }
        }

        public static SurvivorsTemplateTuning CreateTuning(SurvivorsPacingProfile profile)
        {
            var tuning = new SurvivorsTemplateTuning
            {
                PacingProfile = profile
            };

            switch (profile)
            {
                case SurvivorsPacingProfile.Normal:
                    ApplyNormalTuning(tuning);
                    break;
                case SurvivorsPacingProfile.DebugFast:
                    ApplyDebugFastTuning(tuning);
                    break;
                case SurvivorsPacingProfile.Showcase:
                    ApplyShowcaseTuning(tuning);
                    break;
                default:
                    tuning.PacingProfile = SurvivorsPacingProfile.HumanPlaytest;
                    break;
            }

            return tuning;
        }

        private static void ApplyNormalTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.PacingProfile = SurvivorsPacingProfile.Normal;
            tuning.PlayerMaxHealth = 36f;
            tuning.EnemySpawnRadius = 13.5f;
            tuning.EnemySpawnIntervalSeconds = 0.9f;
            tuning.EnemyMaximumAlive = 48;
            tuning.EnemyMaxHealth = 12f;
            tuning.EnemyMoveSpeed = 1.45f;
            tuning.EnemyContactDamage = 5f;
            tuning.RunEscalationIntervalSeconds = 90f;
            tuning.MinimumEnemySpawnIntervalSeconds = 0.38f;
            tuning.EnemySpawnIntervalReductionPerEscalation = 0.045f;
            tuning.EnemyMaximumAliveIncreasePerEscalation = 8;
            tuning.EnemyHealthMultiplierPerEscalation = 0.16f;
            tuning.EnemyMoveSpeedMultiplierPerEscalation = 0.018f;
            tuning.EnemyExperienceMultiplierPerEscalation = 0.08f;
            tuning.FirstEliteSpawnTimeSeconds = 120f;
            tuning.FirstDreadEliteSpawnTimeSeconds = 210f;
            tuning.MinibossSpawnTimeSeconds = 300f;
            tuning.MinibossMoveSpeed = 1.65f;
            tuning.MinibossContactDamage = 10f;
            tuning.BossMoveSpeed = 1.45f;
            tuning.WeaponCooldownSeconds = 0.82f;
            tuning.WeaponRange = 11f;
            tuning.ProjectileDamage = 6.2f;
            tuning.ProjectileSpeed = 8.25f;
            tuning.ProjectileLifetimeSeconds = 2.1f;
            tuning.OrbitDamage = 2f;
            tuning.OrbitDegreesPerSecond = 145f;
            tuning.OrbitContactTickIntervalSeconds = 0.34f;
            tuning.MeleeDamage = 4.8f;
            tuning.MeleeCooldownSeconds = 1.4f;
            tuning.MeleeRange = 2.65f;
            tuning.BurstDamage = 3.6f;
            tuning.BurstCooldownSeconds = 3.4f;
            tuning.BurstRadius = 2.8f;
            tuning.HitscanDamage = 5.8f;
            tuning.HitscanCooldownSeconds = 1.6f;
            tuning.GrenadeDamage = 7.2f;
            tuning.GrenadeCooldownSeconds = 2.9f;
            tuning.GrenadeRange = 7.4f;
            tuning.GrenadePayloadTravelSpeed = 6.5f;
            tuning.GrenadePayloadArmingSeconds = 0.9f;
            tuning.GrenadePayloadExplosionRadius = 2.35f;
            tuning.PlacedPayloadDamage = 5.8f;
            tuning.TrapCooldownSeconds = 3.25f;
            tuning.MineCooldownSeconds = 3.8f;
            tuning.PlacedPayloadRange = 6.2f;
            tuning.PlacedPayloadArmingSeconds = 0.9f;
            tuning.PlacedPayloadLifetimeSeconds = 4.4f;
            tuning.PlacedPayloadTriggerRadius = 1.35f;
            tuning.PlacedPayloadExplosionRadius = 2.15f;
            tuning.PickupAttractRange = 2.35f;
            tuning.PickupAttractionSpeed = 4.6f;
            tuning.PickupCollectRadius = 0.72f;
            tuning.MagnetRecallSpeedMultiplier = 2f;
            tuning.ExperienceRequiredBase = 8;
            tuning.ExperienceRequiredPerLevel = 5;
            tuning.DraftRerollCharges = 3;
            tuning.DraftBanishCharges = 3;
            tuning.RewardSelectionTimeoutSeconds = 45f;
        }

        private static void ApplyDebugFastTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.PlayerMoveSpeed = 5.75f;
            tuning.EnemySpawnRadius = 11.5f;
            tuning.EnemySpawnIntervalSeconds = 0.35f;
            tuning.EnemyMaximumAlive = 96;
            tuning.EnemyMaxHealth = 10f;
            tuning.EnemyMoveSpeed = 2.4f;
            tuning.EnemyContactDamage = 6f;
            tuning.EnemyExperienceReward = 3;
            tuning.RunEscalationIntervalSeconds = 30f;
            tuning.MinimumEnemySpawnIntervalSeconds = 0.1f;
            tuning.EnemySpawnIntervalReductionPerEscalation = 0.035f;
            tuning.EnemyMaximumAliveIncreasePerEscalation = 14;
            tuning.EnemyHealthMultiplierPerEscalation = 0.24f;
            tuning.EnemyMoveSpeedMultiplierPerEscalation = 0.04f;
            tuning.EnemyExperienceMultiplierPerEscalation = 0.18f;
            tuning.FirstEliteSpawnTimeSeconds = 20f;
            tuning.FirstDreadEliteSpawnTimeSeconds = 40f;
            tuning.MinibossSpawnTimeSeconds = 60f;
            tuning.MinibossMaxHealth = 180f;
            tuning.MinibossMoveSpeed = 2.35f;
            tuning.MinibossExperienceReward = 48;
            tuning.BossSpawnTimeSeconds = 240f;
            tuning.BossMaxHealth = 850f;
            tuning.BossMoveSpeed = 2f;
            tuning.BossExperienceReward = 150;
            tuning.SurvivalVictoryTimeSeconds = 360f;
            tuning.WeaponCooldownSeconds = 0.48f;
            tuning.WeaponRange = 14f;
            tuning.ProjectileDamage = 7f;
            tuning.ProjectileSpeed = 11.5f;
            tuning.ProjectileLifetimeSeconds = 2.4f;
            tuning.OrbitDamage = 2.5f;
            tuning.OrbitDegreesPerSecond = 185f;
            tuning.MeleeDamage = 5.5f;
            tuning.MeleeCooldownSeconds = 1.15f;
            tuning.BurstDamage = 4.2f;
            tuning.BurstCooldownSeconds = 2.8f;
            tuning.BurstRadius = 3.15f;
            tuning.HitscanDamage = 6.5f;
            tuning.HitscanCooldownSeconds = 1.35f;
            tuning.GrenadeDamage = 8.5f;
            tuning.GrenadeCooldownSeconds = 2.4f;
            tuning.GrenadeRange = 8.5f;
            tuning.GrenadePayloadTravelSpeed = 9f;
            tuning.GrenadePayloadArmingSeconds = 0.7f;
            tuning.PlacedPayloadDamage = 6.8f;
            tuning.TrapCooldownSeconds = 2.75f;
            tuning.MineCooldownSeconds = 3.15f;
            tuning.PlacedPayloadRange = 7f;
            tuning.PlacedPayloadArmingSeconds = 0.7f;
            tuning.PlacedPayloadLifetimeSeconds = 4f;
            tuning.PickupAttractRange = 3.5f;
            tuning.PickupAttractionSpeed = 8.5f;
            tuning.MagnetRecallSpeedMultiplier = 2.6f;
            tuning.ExperienceRequiredBase = 3;
            tuning.ExperienceRequiredPerLevel = 2;
            tuning.DraftRerollCharges = 5;
            tuning.DraftBanishCharges = 5;
            tuning.RewardSelectionTimeoutSeconds = 8f;
        }

        private static void ApplyShowcaseTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.PlayerMoveSpeed = 5.6f;
            tuning.EnemySpawnRadius = 12.5f;
            tuning.EnemySpawnIntervalSeconds = 0.72f;
            tuning.EnemyMaximumAlive = 56;
            tuning.EnemyMaxHealth = 11f;
            tuning.EnemyMoveSpeed = 2f;
            tuning.EnemyExperienceReward = 2;
            tuning.RunEscalationIntervalSeconds = 60f;
            tuning.MinimumEnemySpawnIntervalSeconds = 0.2f;
            tuning.EnemySpawnIntervalReductionPerEscalation = 0.035f;
            tuning.EnemyMaximumAliveIncreasePerEscalation = 8;
            tuning.EnemyHealthMultiplierPerEscalation = 0.18f;
            tuning.EnemyMoveSpeedMultiplierPerEscalation = 0.025f;
            tuning.EnemyExperienceMultiplierPerEscalation = 0.12f;
            tuning.FirstEliteSpawnTimeSeconds = 60f;
            tuning.FirstDreadEliteSpawnTimeSeconds = 105f;
            tuning.MinibossSpawnTimeSeconds = 150f;
            tuning.MinibossMaxHealth = 220f;
            tuning.MinibossMoveSpeed = 2.05f;
            tuning.BossSpawnTimeSeconds = 600f;
            tuning.BossMaxHealth = 1200f;
            tuning.BossMoveSpeed = 1.8f;
            tuning.SurvivalVictoryTimeSeconds = 900f;
            tuning.WeaponCooldownSeconds = 0.62f;
            tuning.WeaponRange = 12f;
            tuning.ProjectileDamage = 6.5f;
            tuning.ProjectileSpeed = 9.8f;
            tuning.ProjectileLifetimeSeconds = 2.25f;
            tuning.OrbitDamage = 2.3f;
            tuning.OrbitDegreesPerSecond = 165f;
            tuning.MeleeCooldownSeconds = 1.25f;
            tuning.BurstCooldownSeconds = 3f;
            tuning.GrenadePayloadTravelSpeed = 7.8f;
            tuning.PickupAttractRange = 2.9f;
            tuning.PickupAttractionSpeed = 6.4f;
            tuning.MagnetRecallSpeedMultiplier = 2.25f;
            tuning.ExperienceRequiredBase = 5;
            tuning.ExperienceRequiredPerLevel = 3;
            tuning.DraftRerollCharges = 3;
            tuning.DraftBanishCharges = 3;
            tuning.RewardSelectionTimeoutSeconds = 30f;
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
                CreateEnemyProfileDefinitions(resolved),
                firstEliteSpawnTimeSeconds: resolved.FirstEliteSpawnTimeSeconds,
                firstDreadEliteSpawnTimeSeconds: resolved.FirstDreadEliteSpawnTimeSeconds);
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
                CreateEnemyProfile(SurvivorsEnemyRole.Splitter, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.Elite, resolved),
                CreateEnemyProfile(SurvivorsEnemyRole.DreadElite, resolved),
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
                case SurvivorsEnemyRole.Splitter:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.Splitter,
                        "enemy.survivors.splitter",
                        "Grave Husk Splitter",
                        resolved.EnemyMaxHealth * 2.05f,
                        resolved.EnemyMoveSpeed * 0.82f,
                        resolved.EnemyRadius * 1.18f,
                        resolved.EnemyContactDamage * 1.05f,
                        resolved.EnemyContactIntervalSeconds * 1.08f,
                        Mathf.Max(1, Mathf.RoundToInt(resolved.EnemyExperienceReward * 3.1f)),
                        new Color(0.36f, 0.54f, 0.92f));
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
                case SurvivorsEnemyRole.DreadElite:
                    return new SurvivorsEnemyProfile(
                        SurvivorsEnemyRole.DreadElite,
                        "enemy.survivors.dread-elite",
                        "Dread Lantern Elite",
                        resolved.EnemyMaxHealth * 4.9f,
                        resolved.EnemyMoveSpeed * 0.88f,
                        resolved.EnemyRadius * 1.28f,
                        resolved.EnemyContactDamage * 1.25f,
                        resolved.EnemyContactIntervalSeconds,
                        Mathf.Max(1, Mathf.RoundToInt(resolved.EnemyExperienceReward * 6.1f)),
                        new Color(0.38f, 0.28f, 1f),
                        rangedAttackRange: 7.4f,
                        rangedAttackDamage: resolved.EnemyContactDamage * 0.82f,
                        rangedAttackIntervalSeconds: 1.85f,
                        preferredRange: 5.4f);
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
                Upgrade(StarBeamUnlockUpgradeId, RunUpgradeRarity.Uncommon, 34, 1, WeaponUnlockEffect, HitscanWeaponTarget, 1.0d),
                Upgrade(GravityGrenadeUnlockUpgradeId, RunUpgradeRarity.Rare, 18, 1, WeaponUnlockEffect, GrenadeWeaponTarget, 1.0d),
                Upgrade(FrostFanUpgradeId, RunUpgradeRarity.Uncommon, 40, 5, ProjectileFanEffect, FrostFanWeaponTarget, 1.0d),
                Upgrade(FrostSplinterUpgradeId, RunUpgradeRarity.Rare, 24, 3, ProjectileForkEffect, FrostFanWeaponTarget, 1.0d),
                Upgrade(FrostRicochetUpgradeId, RunUpgradeRarity.Rare, 22, 2, ProjectileChainEffect, FrostFanWeaponTarget, 1.0d),
                Upgrade(OrbitingFocusUpgradeId, RunUpgradeRarity.Uncommon, 38, 5, OrbitBladeEffect, OrbitWeaponTarget, 1.0d),
                Upgrade(ThornHaloUpgradeId, RunUpgradeRarity.Rare, 30, 3, OrbitRadiusEffect, ThornHaloWeaponTarget, 0.35d),
                Upgrade(HaloSpiralUpgradeId, RunUpgradeRarity.Rare, 22, 2, OrbitRadiusEffect, ThornHaloWeaponTarget, 0.3d),
                Upgrade(MoonlitEdgeUpgradeId, RunUpgradeRarity.Uncommon, 30, 5, DamageBonusEffect, MeleeWeaponTarget, 1.5d),
                Upgrade(CrescentChainUpgradeId, RunUpgradeRarity.Uncommon, 34, 4, MeleeTargetEffect, MeleeWeaponTarget, 1.0d),
                Upgrade(LunarTempoUpgradeId, RunUpgradeRarity.Rare, 22, 2, FireRateEffect, MeleeWeaponTarget, -0.05d),
                Upgrade(NovaEchoUpgradeId, RunUpgradeRarity.Rare, 24, 5, BurstCountEffect, BurstWeaponTarget, 1.0d),
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
                Upgrade(ScholarsLensUpgradeId, RunUpgradeRarity.Uncommon, 36, 5, ExperienceGainEffect, ExperienceTarget, 0.12d),
                Upgrade(GiantRuneUpgradeId, RunUpgradeRarity.Uncommon, 34, 5, AreaRadiusEffect, AreaTarget, 0.22d),
                Upgrade(TwinCharmUpgradeId, RunUpgradeRarity.Epic, 16, 4, ProjectileFanEffect, WeaponTarget, 1.0d),
                UpgradeMulti(
                    AstralConvergenceUpgradeId,
                    RunUpgradeRarity.Epic,
                    12,
                    2,
                    new RunUpgradeEffectDescriptor(AreaRadiusEffect, AreaTarget, 0.36d),
                    new RunUpgradeEffectDescriptor(DamageBonusEffect, WeaponTarget, 3.0d)),
                Upgrade(ArcaneThesisUpgradeId, RunUpgradeRarity.Uncommon, 34, 4, DamageBonusEffect, WeaponTarget, 1.6d),
                Upgrade(FrostNeedleworkUpgradeId, RunUpgradeRarity.Uncommon, 32, 3, ProjectilePierceEffect, FrostFanWeaponTarget, 1.0d),
                Upgrade(BloodRingCanticleUpgradeId, RunUpgradeRarity.Rare, 22, 3, OrbitBladeEffect, OrbitWeaponTarget, 1.0d),
                Upgrade(CinderScriptUpgradeId, RunUpgradeRarity.Rare, 22, 2, TargetedBurstEffect, BurstWeaponTarget, 1.0d),
                Upgrade(EmberForgeHeartUpgradeId, RunUpgradeRarity.Uncommon, 34, 4, DamageBonusEffect, BurstWeaponTarget, 2.2d),
                Upgrade(EmberTempoUpgradeId, RunUpgradeRarity.Uncommon, 30, 3, FireRateEffect, HitscanWeaponTarget, -0.06d),
                Upgrade(MoonOathUpgradeId, RunUpgradeRarity.Rare, 22, 3, DamageBonusEffect, MeleeWeaponTarget, 1.8d),
                Upgrade(StarFocusUpgradeId, RunUpgradeRarity.Uncommon, 30, 5, DamageBonusEffect, HitscanWeaponTarget, 1.4d),
                Upgrade(StarPulseUpgradeId, RunUpgradeRarity.Rare, 20, 2, FireRateEffect, HitscanWeaponTarget, -0.05d),
                Upgrade(SiegePayloadsUpgradeId, RunUpgradeRarity.Rare, 20, 2, PayloadCountEffect, PayloadWeaponTarget, 1.0d),
                Upgrade(EmberWardUpgradeId, RunUpgradeRarity.Rare, 20, 3, BarrierCapacityEffect, BarrierTarget, 8.0d),
                Upgrade(PrismaticBeamUpgradeId, RunUpgradeRarity.Uncommon, 30, 3, HitscanPierceEffect, HitscanWeaponTarget, 1.0d),
                Upgrade(ExtraPayloadUpgradeId, RunUpgradeRarity.Rare, 22, 5, PayloadCountEffect, PayloadWeaponTarget, 1.0d),
                Upgrade(BiggerBoomsUpgradeId, RunUpgradeRarity.Uncommon, 32, 4, PayloadRadiusEffect, PayloadWeaponTarget, 0.45d),
                Upgrade(WiderTriggersUpgradeId, RunUpgradeRarity.Uncommon, 28, 4, PayloadTriggerRadiusEffect, PayloadWeaponTarget, 0.35d),
                Upgrade(RuneLatticeUpgradeId, RunUpgradeRarity.Uncommon, 28, 5, PayloadCountEffect, PayloadWeaponTarget, 1.0d),
                Upgrade(SnaringRunesUpgradeId, RunUpgradeRarity.Rare, 22, 3, PayloadTriggerRadiusEffect, PayloadWeaponTarget, 0.35d),
                Upgrade(AetherBloomUpgradeId, RunUpgradeRarity.Rare, 22, 3, PayloadRadiusEffect, PayloadWeaponTarget, 0.45d),
                UpgradeMulti(
                    ArcaneStormEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    14,
                    1,
                    new RunUpgradeEffectDescriptor(ProjectileChainEffect, WeaponTarget, 3.0d),
                    new RunUpgradeEffectDescriptor(ProjectileForkEffect, WeaponTarget, 1.0d),
                    new RunUpgradeEffectDescriptor(DamageBonusEffect, WeaponTarget, 5.0d)),
                UpgradeMulti(
                    BlizzardCrownEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    14,
                    1,
                    new RunUpgradeEffectDescriptor(ProjectileFanEffect, FrostFanWeaponTarget, 3.0d),
                    new RunUpgradeEffectDescriptor(ProjectilePierceEffect, FrostFanWeaponTarget, 2.0d)),
                UpgradeMulti(
                    CrimsonAegisEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    14,
                    1,
                    new RunUpgradeEffectDescriptor(OrbitBladeEffect, OrbitWeaponTarget, 3.0d),
                    new RunUpgradeEffectDescriptor(OrbitRadiusEffect, ThornHaloWeaponTarget, 0.75d)),
                UpgradeMulti(
                    InfernoHeartEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    14,
                    1,
                    new RunUpgradeEffectDescriptor(BurstCountEffect, BurstWeaponTarget, 2.0d),
                    new RunUpgradeEffectDescriptor(BurstEchoEffect, BurstWeaponTarget, 2.0d),
                    new RunUpgradeEffectDescriptor(TargetedBurstEffect, BurstWeaponTarget, 1.0d)),
                UpgradeMulti(
                    TempestPrismEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    10,
                    1,
                    new RunUpgradeEffectDescriptor(HitscanPierceEffect, HitscanWeaponTarget, 3.0d),
                    new RunUpgradeEffectDescriptor(FireRateEffect, HitscanWeaponTarget, -0.12d)),
                UpgradeMulti(
                    GravefieldEngineEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    10,
                    1,
                    new RunUpgradeEffectDescriptor(PayloadCountEffect, PayloadWeaponTarget, 2.0d),
                    new RunUpgradeEffectDescriptor(PayloadRadiusEffect, PayloadWeaponTarget, 0.85d),
                    new RunUpgradeEffectDescriptor(PayloadTriggerRadiusEffect, PayloadWeaponTarget, 0.65d)),
                UpgradeMulti(
                    EclipseWaltzEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    10,
                    1,
                    new RunUpgradeEffectDescriptor(MeleeTargetEffect, MeleeWeaponTarget, 3.0d),
                    new RunUpgradeEffectDescriptor(FireRateEffect, MeleeWeaponTarget, -0.1d),
                    new RunUpgradeEffectDescriptor(DamageBonusEffect, MeleeWeaponTarget, 4.0d)),
                UpgradeMulti(
                    AetherfieldMatrixEvolutionUpgradeId,
                    RunUpgradeRarity.Legendary,
                    10,
                    1,
                    new RunUpgradeEffectDescriptor(PayloadCountEffect, PayloadWeaponTarget, 2.0d),
                    new RunUpgradeEffectDescriptor(PayloadRadiusEffect, PayloadWeaponTarget, 0.75d),
                    new RunUpgradeEffectDescriptor(PayloadTriggerRadiusEffect, PayloadWeaponTarget, 0.65d))
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
                BarrierTarget,
                ExperienceTarget,
                AreaTarget
            };
        }

        public static IReadOnlyList<SurvivorsRunUpgradeMetadata> CreateRunUpgradeMetadata()
        {
            return new[]
            {
                UpgradeMetadata("upgrade.survivors.arcane-damage", "Arcane Damage", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, ArcaneWandWeaponContentId, "Arc Bolt deals more damage."),
                UpgradeMetadata("upgrade.survivors.quick-casting", "Quick Casting", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, ArcaneWandWeaponContentId, "Arc Bolt fires more often."),
                UpgradeMetadata("upgrade.survivors.swift-steps", "Swift Steps", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, PlayerTarget.Value, "Passive: move faster through horde pressure."),
                UpgradeMetadata("upgrade.survivors.gem-magnet", "Gem Magnet", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, PickupTarget.Value, "Passive: pull XP gems from farther away."),
                UpgradeMetadata("upgrade.survivors.iron-blood", "Iron Blood", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, PlayerTarget.Value, "Passive: increase maximum health."),
                UpgradeMetadata(FrostFanUpgradeId, "Frost Fan", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, FrostFanWeaponContentId, "Add more frost shards to the fan pattern."),
                UpgradeMetadata(FrostSplinterUpgradeId, "Frost Splinter", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, FrostFanWeaponContentId, "Frost shards split into secondary shards after impact.", FrostFanWeaponContentId, FrostFanUpgradeId, 2),
                UpgradeMetadata(FrostRicochetUpgradeId, "Frost Ricochet", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, FrostFanWeaponContentId, "Frost shards jump between packed enemies.", FrostFanWeaponContentId, FrostFanUpgradeId, 3),
                UpgradeMetadata(OrbitingFocusUpgradeId, "Orbiting Focus", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, OrbitWardWeaponContentId, "Add blades to the orbiting ward."),
                UpgradeMetadata(ThornHaloUpgradeId, "Thorn Halo Wall", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, ThornHaloWeaponContentId, "Push the halo outward and add another guard blade.", ThornHaloWeaponContentId, OrbitingFocusUpgradeId, 2),
                UpgradeMetadata(HaloSpiralUpgradeId, "Halo Spiral", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, ThornHaloWeaponContentId, "Thorn Halo widens into a denser spiral.", ThornHaloWeaponContentId, OrbitingFocusUpgradeId, 3),
                UpgradeMetadata(MoonlitEdgeUpgradeId, "Moonlit Edge", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, MoonSlashWeaponContentId, "Moon Slash cuts harder.", MoonSlashWeaponContentId),
                UpgradeMetadata(CrescentChainUpgradeId, "Crescent Chain", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, MoonSlashWeaponContentId, "Let Moon Slash cut through more targets.", MoonSlashWeaponContentId, MoonlitEdgeUpgradeId, 2),
                UpgradeMetadata(LunarTempoUpgradeId, "Lunar Tempo", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, MoonSlashWeaponContentId, "Moon Slash recovers faster between cuts.", MoonSlashWeaponContentId, MoonlitEdgeUpgradeId, 3),
                UpgradeMetadata(NovaEchoUpgradeId, "Nova Echo", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, StarNovaWeaponContentId, "Add another pulse to Cinder Burst."),
                UpgradeMetadata(CinderEchoUpgradeId, "Cinder Echoes", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, StarNovaWeaponContentId, "Repeat Cinder Burst with delayed echoes.", StarNovaWeaponContentId, NovaEchoUpgradeId, 2),
                UpgradeMetadata(TargetedSigilUpgradeId, "Targeted Burst Sigils", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, StarNovaWeaponContentId, "Cinder Burst leaves focused sigils on nearby enemies.", StarNovaWeaponContentId, NovaEchoUpgradeId, 3),
                UpgradeMetadata("upgrade.survivors.piercing-bolts", "Piercing Bolts", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, ArcaneWandWeaponContentId, "Arc Bolt pierces additional enemies."),
                UpgradeMetadata("upgrade.survivors.chain-bolts", "Chain Bolts", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, ArcaneWandWeaponContentId, "Arc Bolt jumps to a nearby enemy after impact."),
                UpgradeMetadata("upgrade.survivors.forked-bolts", "Forked Bolts", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, ArcaneWandWeaponContentId, "Arc Bolt splits after a hit."),
                UpgradeMetadata("upgrade.survivors.returning-bolts", "Returning Bolts", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, ArcaneWandWeaponContentId, "Arc Bolt boomerangs after impact."),
                UpgradeMetadata("upgrade.survivors.distilled-poison", "Distilled Poison", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, StatusTarget.Value, "Passive: weapon hits leave poison damage over time."),
                UpgradeMetadata("upgrade.survivors.hemorrhage-edge", "Hemorrhage Edge", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, StatusTarget.Value, "Passive: weapon hits add bleeding damage over time."),
                UpgradeMetadata("upgrade.survivors.execution-rite", "Execution Rite", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, StatusTarget.Value, "Passive: wounded enemies can be executed by weapon hits."),
                UpgradeMetadata("upgrade.survivors.sanguine-feast", "Sanguine Feast", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, PlayerTarget.Value, "Passive: heal from a portion of weapon damage."),
                UpgradeMetadata("upgrade.survivors.guardian-barrier", "Guardian Barrier", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, BarrierTarget.Value, "Passive: increase starting barrier capacity."),
                UpgradeMetadata("upgrade.survivors.ward-recovery", "Ward Recovery", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, BarrierTarget.Value, "Passive: regenerate barrier during the run."),
                UpgradeMetadata("upgrade.survivors.mirror-bulwark", "Mirror Bulwark", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, BarrierTarget.Value, "Passive: weapon damage restores a little barrier."),
                UpgradeMetadata(ScholarsLensUpgradeId, "Scholar's Lens", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, ExperienceTarget.Value, "Passive: increase XP gained from gems and debug grants."),
                UpgradeMetadata(GiantRuneUpgradeId, "Giant Rune", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, AreaTarget.Value, "Passive: enlarge bursts, orbit paths, payload explosions, and close-range swings."),
                UpgradeMetadata(TwinCharmUpgradeId, "Twin Charm", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, WeaponTarget.Value, "Epic passive: duplicate projectile-style attacks with extra fan shots."),
                UpgradeMetadata(AstralConvergenceUpgradeId, "Astral Convergence", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, AreaTarget.Value, "Epic mutation: enlarge area weapons and sharpen global damage."),
                UpgradeMetadata(StarBeamUnlockUpgradeId, "Star Beam", SurvivorsRunUpgradeCategory.Weapon, SurvivorsRunBuildSlotKind.Weapon, StarBeamWeaponContentId, "Unlock weapon: a focused beam that pierces the front line."),
                UpgradeMetadata(GravityGrenadeUnlockUpgradeId, "Gravity Grenade", SurvivorsRunUpgradeCategory.Weapon, SurvivorsRunBuildSlotKind.Weapon, GravityGrenadeWeaponContentId, "Unlock weapon: lob a heavy payload that detonates inside the horde."),
                UpgradeMetadata(ArcaneThesisUpgradeId, "Arcane Thesis", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, ArcaneWandWeaponContentId, "Passive: commit to Arc Bolt damage and unlock Arcane Storm."),
                UpgradeMetadata(FrostNeedleworkUpgradeId, "Frost Needlework", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, FrostFanWeaponContentId, "Passive: sharpen frost shards and unlock Blizzard Crown."),
                UpgradeMetadata(BloodRingCanticleUpgradeId, "Blood Ring Canticle", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, OrbitWardWeaponContentId, "Passive: deepen orbit damage and unlock Crimson Aegis."),
                UpgradeMetadata(CinderScriptUpgradeId, "Cinder Script", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, StarNovaWeaponContentId, "Passive: focus burst sigils and unlock Inferno Heart."),
                UpgradeMetadata(EmberForgeHeartUpgradeId, "Ember Forge Heart", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, StarBeamWeaponContentId, "Passive: raise Ember weapon damage."),
                UpgradeMetadata(EmberTempoUpgradeId, "Ember Tempo", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, StarBeamWeaponContentId, "Passive: quicken Star Beam for Ember builds."),
                UpgradeMetadata(MoonOathUpgradeId, "Moon Oath", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, MoonSlashWeaponContentId, "Passive: sharpen Moon Slash and unlock Eclipse Waltz."),
                UpgradeMetadata(SiegePayloadsUpgradeId, "Siege Payloads", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, PayloadWeaponTarget.Value, "Passive: carry heavier payloads for Ember payload builds."),
                UpgradeMetadata(EmberWardUpgradeId, "Ember Ward", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, BarrierTarget.Value, "Passive: add a larger defensive barrier."),
                UpgradeMetadata(StarFocusUpgradeId, "Star Focus", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, StarBeamWeaponContentId, "Star Beam deals more damage.", StarBeamWeaponContentId),
                UpgradeMetadata(StarPulseUpgradeId, "Star Pulse", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, StarBeamWeaponContentId, "Star Beam fires more often.", StarBeamWeaponContentId, StarFocusUpgradeId, 3),
                UpgradeMetadata(PrismaticBeamUpgradeId, "Prismatic Beam", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, StarBeamWeaponContentId, "Star Beam pierces more enemies.", StarBeamWeaponContentId, StarFocusUpgradeId, 2),
                UpgradeMetadata(ExtraPayloadUpgradeId, "Extra Payload", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, GravityGrenadeWeaponContentId, "Throw or place additional payloads.", GravityGrenadeWeaponContentId),
                UpgradeMetadata(BiggerBoomsUpgradeId, "Bigger Booms", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, GravityGrenadeWeaponContentId, "Payload explosions cover a wider area.", GravityGrenadeWeaponContentId, ExtraPayloadUpgradeId, 2),
                UpgradeMetadata(WiderTriggersUpgradeId, "Wider Triggers", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, GravityGrenadeWeaponContentId, "Payloads trigger from farther away.", GravityGrenadeWeaponContentId, ExtraPayloadUpgradeId, 2),
                UpgradeMetadata(RuneLatticeUpgradeId, "Rune Lattice", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, RuneTrapWeaponContentId, "Rune Trap and Aether Mine deploy more hazards.", RuneTrapWeaponContentId),
                UpgradeMetadata(SnaringRunesUpgradeId, "Snaring Runes", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, RuneTrapWeaponContentId, "Placed hazards trigger from farther away.", RuneTrapWeaponContentId, RuneLatticeUpgradeId, 2),
                UpgradeMetadata(AetherBloomUpgradeId, "Aether Bloom", SurvivorsRunUpgradeCategory.Mutation, SurvivorsRunBuildSlotKind.None, AetherMineWeaponContentId, "Placed hazard detonations cover a wider area.", AetherMineWeaponContentId, RuneLatticeUpgradeId, 3),
                UpgradeMetadata(ArcaneStormEvolutionUpgradeId, "Arcane Storm", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, ArcaneWandWeaponContentId, "Evolution: Arc Bolt becomes a storm of chaining, forking bolts.", ArcaneWandWeaponContentId, "upgrade.survivors.arcane-damage", 5, ArcaneThesisUpgradeId),
                UpgradeMetadata(BlizzardCrownEvolutionUpgradeId, "Blizzard Crown", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, FrostFanWeaponContentId, "Evolution: Frost Fan expands into a piercing crown of shards.", FrostFanWeaponContentId, FrostFanUpgradeId, 5, FrostNeedleworkUpgradeId),
                UpgradeMetadata(CrimsonAegisEvolutionUpgradeId, "Crimson Aegis", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, OrbitWardWeaponContentId, "Evolution: Blood Ring and Thorn Halo gain a counter-rotating shield ring.", OrbitWardWeaponContentId, OrbitingFocusUpgradeId, 5, BloodRingCanticleUpgradeId),
                UpgradeMetadata(InfernoHeartEvolutionUpgradeId, "Inferno Heart", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, StarNovaWeaponContentId, "Evolution: Cinder Burst repeats, echoes, and targets the horde.", StarNovaWeaponContentId, NovaEchoUpgradeId, 5, CinderScriptUpgradeId),
                UpgradeMetadata(TempestPrismEvolutionUpgradeId, "Tempest Prism", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, StarBeamWeaponContentId, "Evolution: Star Beam becomes a rapid prism that splits into angled side beams.", StarBeamWeaponContentId, StarFocusUpgradeId, 5, TwinCharmUpgradeId),
                UpgradeMetadata(GravefieldEngineEvolutionUpgradeId, "Gravefield Engine", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, GravityGrenadeWeaponContentId, "Evolution: payload detonations seed satellite danger fields.", GravityGrenadeWeaponContentId, BiggerBoomsUpgradeId, 4, GiantRuneUpgradeId),
                UpgradeMetadata(EclipseWaltzEvolutionUpgradeId, "Eclipse Waltz", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, MoonSlashWeaponContentId, "Evolution: Moon Slash cuts forward and backward in a rapid sweeping dance.", MoonSlashWeaponContentId, MoonlitEdgeUpgradeId, 5, MoonOathUpgradeId),
                UpgradeMetadata(AetherfieldMatrixEvolutionUpgradeId, "Aetherfield Matrix", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, RuneTrapWeaponContentId, "Evolution: Rune Trap and Aether Mine seed satellite hazard fields.", RuneTrapWeaponContentId, RuneLatticeUpgradeId, 5, SiegePayloadsUpgradeId)
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
                    Node("node.survivors.ember-vanguard.moon-oath", "Moon Oath", MoonOathUpgradeId, SurvivorsProgressionNodeKind.Passive, 2, 1, 3),
                    Node("node.survivors.ember-vanguard.siege-payloads", "Siege Payloads", SiegePayloadsUpgradeId, SurvivorsProgressionNodeKind.Passive, 3, 2, 2),
                    Node("node.survivors.ember-vanguard.ember-ward", "Ember Ward", EmberWardUpgradeId, SurvivorsProgressionNodeKind.Passive, 4, 2, 3)),
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
                    Node("node.survivors.frost-fan.unlock", "Frost Fan", FrostFanUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 5),
                    Node("node.survivors.frost-fan.splinter", "Frost Splinter", FrostSplinterUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 1, 3),
                    Node("node.survivors.frost-fan.ricochet", "Frost Ricochet", FrostRicochetUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 2),
                    Node("node.survivors.frost-fan.needles", "Frost Needlework", FrostNeedleworkUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 3, 1, 3)),
                Track(
                    "progression.survivors.blood-ring.weapon",
                    "Blood Ring Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    OrbitWardWeaponContentId,
                    Node("node.survivors.blood-ring.focus", "Orbiting Focus", OrbitingFocusUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 5),
                    Node("node.survivors.blood-ring.canticle", "Blood Ring Canticle", BloodRingCanticleUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 2, 3)),
                Track(
                    "progression.survivors.thorn-halo.weapon",
                    "Thorn Halo Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    ThornHaloWeaponContentId,
                    Node("node.survivors.thorn-halo.wall", "Thorn Halo Wall", ThornHaloUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 0, 1, 3),
                    Node("node.survivors.thorn-halo.spiral", "Halo Spiral", HaloSpiralUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 1, 2)),
                Track(
                    "progression.survivors.cinder-burst.weapon",
                    "Cinder Burst Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    StarNovaWeaponContentId,
                    Node("node.survivors.cinder-burst.nova-echo", "Nova Echo", NovaEchoUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 5),
                    Node("node.survivors.cinder-burst.cinder-echoes", "Cinder Echoes", CinderEchoUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 2, 3),
                    Node("node.survivors.cinder-burst.targeted-sigils", "Targeted Burst Sigils", TargetedSigilUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 2, 1)),
                Track(
                    "progression.survivors.star-beam.weapon",
                    "Star Beam Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    StarBeamWeaponContentId,
                    Node("node.survivors.star-beam.unlock", "Star Beam", StarBeamUnlockUpgradeId, SurvivorsProgressionNodeKind.WeaponUnlock, 0, 1, 1),
                    Node("node.survivors.star-beam.focus", "Star Focus", StarFocusUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 1, 1, 5),
                    Node("node.survivors.star-beam.prismatic-beam", "Prismatic Beam", PrismaticBeamUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 3),
                    Node("node.survivors.star-beam.pulse", "Star Pulse", StarPulseUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 3, 1, 2)),
                Track(
                    "progression.survivors.ember-vanguard.moon-slash.weapon",
                    "Ember Vanguard Moon Slash Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    EmberVanguardClassId,
                    MoonSlashWeaponContentId,
                    Node("node.survivors.ember-vanguard.moon-slash.edge", "Moonlit Edge", MoonlitEdgeUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 5),
                    Node("node.survivors.ember-vanguard.moon-slash.crescent-chain", "Crescent Chain", CrescentChainUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 1, 4),
                    Node("node.survivors.ember-vanguard.moon-slash.tempo", "Lunar Tempo", LunarTempoUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 2)),
                Track(
                    "progression.survivors.gravity-grenade.unlock",
                    "Gravity Grenade Skill Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    string.Empty,
                    GravityGrenadeWeaponContentId,
                    Node("node.survivors.gravity-grenade.unlock", "Gravity Grenade", GravityGrenadeUnlockUpgradeId, SurvivorsProgressionNodeKind.WeaponUnlock, 0, 1, 1),
                    Node("node.survivors.gravity-grenade.extra", "Extra Payload", ExtraPayloadUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 1, 1, 5),
                    Node("node.survivors.gravity-grenade.bigger-booms", "Bigger Booms", BiggerBoomsUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 4),
                    Node("node.survivors.gravity-grenade.wider-triggers", "Wider Triggers", WiderTriggersUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 4)),
                Track(
                    "progression.survivors.ember-vanguard.payloads.weapon",
                    "Ember Vanguard Payload Track",
                    SurvivorsProgressionTrackKind.WeaponSkillTrack,
                    EmberVanguardClassId,
                    RuneTrapWeaponContentId,
                    Node("node.survivors.ember-vanguard.payloads.rune-lattice", "Rune Lattice", RuneLatticeUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 0, 1, 5),
                    Node("node.survivors.ember-vanguard.payloads.snaring-runes", "Snaring Runes", SnaringRunesUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 1, 1, 3),
                    Node("node.survivors.ember-vanguard.payloads.aether-bloom", "Aether Bloom", AetherBloomUpgradeId, SurvivorsProgressionNodeKind.WeaponMutation, 2, 1, 3),
                    Node("node.survivors.ember-vanguard.payloads.siege", "Siege Payloads", SiegePayloadsUpgradeId, SurvivorsProgressionNodeKind.WeaponRank, 3, 2, 2))
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
                        amountPerRank: 1.5f),
                    new SurvivorsPersistentUpgradeDefinition(
                        new ResearchNodeId("meta.survivors.battle-tempo"),
                        "Battle Tempo",
                        WeaponTarget.Value,
                        MetaDamageEffectId,
                        maxRank: 3,
                        rankCosts: new[] { 6, 10, 16 },
                        amountPerRank: 1.0f),
                    new SurvivorsPersistentUpgradeDefinition(
                        new ResearchNodeId("meta.survivors.boss-hunters-ledger"),
                        "Boss Hunter's Ledger",
                        WeaponTarget.Value,
                        MetaDamageEffectId,
                        maxRank: 2,
                        rankCosts: new[] { 12, 20 },
                        amountPerRank: 2.25f),
                    new SurvivorsPersistentUpgradeDefinition(
                        VitalWardMetaUpgradeId,
                        "Vital Ward",
                        PlayerTarget.Value,
                        MetaMaxHealthEffectId,
                        maxRank: 3,
                        rankCosts: new[] { 4, 8, 12 },
                        amountPerRank: 6f),
                    new SurvivorsPersistentUpgradeDefinition(
                        GemheartLegacyMetaUpgradeId,
                        "Gemheart Legacy",
                        PickupTarget.Value,
                        MetaPickupRangeEffectId,
                        maxRank: 3,
                        rankCosts: new[] { 4, 7, 11 },
                        amountPerRank: 0.75f),
                    new SurvivorsPersistentUpgradeDefinition(
                        ScholarIndexMetaUpgradeId,
                        "Scholar Index",
                        ExperienceTarget.Value,
                        MetaExperienceGainEffectId,
                        maxRank: 3,
                        rankCosts: new[] { 6, 10, 15 },
                        amountPerRank: 0.08f),
                    new SurvivorsPersistentUpgradeDefinition(
                        PreparedDraftMetaUpgradeId,
                        "Prepared Draft",
                        PlayerTarget.Value,
                        MetaDraftRerollEffectId,
                        maxRank: 2,
                        rankCosts: new[] { 10, 18 },
                        amountPerRank: 1f)
                },
                new[]
                {
                    new SurvivorsRewardDefinition(EliteRewardId, BloodShardsCurrencyId, 2, LegacyExperienceTrackId, 12),
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
            if (value == FrostSplinterUpgradeId) return "Frost Splinter";
            if (value == FrostRicochetUpgradeId) return "Frost Ricochet";
            if (value == "upgrade.survivors.orbiting-focus") return "Orbiting Focus";
            if (value == "upgrade.survivors.thorn-halo-wall") return "Thorn Halo Wall";
            if (value == HaloSpiralUpgradeId) return "Halo Spiral";
            if (value == MoonlitEdgeUpgradeId) return "Moonlit Edge";
            if (value == "upgrade.survivors.crescent-chain") return "Crescent Chain";
            if (value == LunarTempoUpgradeId) return "Lunar Tempo";
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
            if (value == ScholarsLensUpgradeId) return "Scholar's Lens";
            if (value == GiantRuneUpgradeId) return "Giant Rune";
            if (value == TwinCharmUpgradeId) return "Twin Charm";
            if (value == AstralConvergenceUpgradeId) return "Astral Convergence";
            if (value == StarBeamUnlockUpgradeId) return "Star Beam";
            if (value == GravityGrenadeUnlockUpgradeId) return "Gravity Grenade";
            if (value == "upgrade.survivors.arcane-thesis") return "Arcane Thesis";
            if (value == "upgrade.survivors.frost-needlework") return "Frost Needlework";
            if (value == "upgrade.survivors.blood-ring-canticle") return "Blood Ring Canticle";
            if (value == "upgrade.survivors.cinder-script") return "Cinder Script";
            if (value == "upgrade.survivors.ember-forge-heart") return "Ember Forge Heart";
            if (value == "upgrade.survivors.ember-tempo") return "Ember Tempo";
            if (value == MoonOathUpgradeId) return "Moon Oath";
            if (value == "upgrade.survivors.siege-payloads") return "Siege Payloads";
            if (value == "upgrade.survivors.ember-ward") return "Ember Ward";
            if (value == StarFocusUpgradeId) return "Star Focus";
            if (value == StarPulseUpgradeId) return "Star Pulse";
            if (value == "upgrade.survivors.prismatic-beam") return "Prismatic Beam";
            if (value == "upgrade.survivors.extra-payload") return "Extra Payload";
            if (value == "upgrade.survivors.bigger-booms") return "Bigger Booms";
            if (value == "upgrade.survivors.wider-triggers") return "Wider Triggers";
            if (value == RuneLatticeUpgradeId) return "Rune Lattice";
            if (value == SnaringRunesUpgradeId) return "Snaring Runes";
            if (value == AetherBloomUpgradeId) return "Aether Bloom";
            if (value == ArcaneStormEvolutionUpgradeId) return "Arcane Storm";
            if (value == BlizzardCrownEvolutionUpgradeId) return "Blizzard Crown";
            if (value == CrimsonAegisEvolutionUpgradeId) return "Crimson Aegis";
            if (value == InfernoHeartEvolutionUpgradeId) return "Inferno Heart";
            if (value == TempestPrismEvolutionUpgradeId) return "Tempest Prism";
            if (value == GravefieldEngineEvolutionUpgradeId) return "Gravefield Engine";
            if (value == EclipseWaltzEvolutionUpgradeId) return "Eclipse Waltz";
            if (value == AetherfieldMatrixEvolutionUpgradeId) return "Aetherfield Matrix";
            return value;
        }

        private static SurvivorsRunUpgradeMetadata UpgradeMetadata(
            string id,
            string displayName,
            SurvivorsRunUpgradeCategory category,
            SurvivorsRunBuildSlotKind slotKind,
            string affectedContentId,
            string description,
            string requiredOwnedWeaponId = null,
            string requiredUpgradeId = null,
            int requiredUpgradeRank = 0,
            string requiredPassiveUpgradeId = null)
        {
            return new SurvivorsRunUpgradeMetadata(
                id,
                displayName,
                category,
                slotKind,
                affectedContentId,
                description,
                requiredOwnedWeaponId,
                requiredUpgradeId,
                requiredUpgradeRank,
                requiredPassiveUpgradeId);
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

        private static RunUpgradeDefinition UpgradeMulti(string id, RunUpgradeRarity rarity, int weight, int maxRank, params RunUpgradeEffectDescriptor[] effects)
        {
            return new RunUpgradeDefinition(
                new RunUpgradeId(id),
                rarity,
                weight,
                maxRank,
                effects);
        }
    }
}
