using System;
using System.Collections.Generic;
using Deucarian.Attacks;
using Deucarian.Combat;
using Deucarian.Encounters;
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
        GameOver = 3
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
        public float EnemySpawnIntervalSeconds = 0.95f;
        public int EnemyMaximumAlive = 32;
        public float EnemyMaxHealth = 10f;
        public float EnemyMoveSpeed = 2.3f;
        public float EnemyRadius = 0.48f;
        public float EnemyContactDamage = 6f;
        public float EnemyContactIntervalSeconds = 0.7f;
        public int EnemyExperienceReward = 2;
        public float WeaponCooldownSeconds = 0.62f;
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
        public int ExperienceRequiredBase = 5;
        public int ExperienceRequiredPerLevel = 3;
        public int DraftChoiceCount = 3;
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
        public static readonly WorldSpawnableId ExperiencePickupSpawnableId = new WorldSpawnableId("pickup.survivors.experience");
        public static readonly WorldSpawnableId MagnetPickupSpawnableId = new WorldSpawnableId("pickup.survivors.magnet");
        public static readonly WorldSpawnableId ProjectileSpawnableId = new WorldSpawnableId("projectile.survivors.arcane-bolt");
        public static readonly WorldSpawnChannelId RadialSpawnChannelId = new WorldSpawnChannelId("spawn.survivors.radial");
        public static readonly WorldSpawnChannelId ExplicitSpawnChannelId = new WorldSpawnChannelId("spawn.survivors.explicit");
        public static readonly AttackDefinitionId ArcaneBoltAttackId = new AttackDefinitionId("attack.survivors.arcane-bolt");
        public static readonly ProjectileDefinitionId ArcaneBoltProjectileId = new ProjectileDefinitionId("projectile.survivors.arcane-bolt");
        public static readonly WeaponDefinitionId ArcaneWandWeaponId = new WeaponDefinitionId("weapon.survivors.arcane-wand");
        public static readonly WeaponDefinitionId StarBeamWeaponId = new WeaponDefinitionId("weapon.survivors.star-beam");
        public static readonly WeaponDefinitionId GravityGrenadeWeaponId = new WeaponDefinitionId("weapon.survivors.gravity-grenade");
        public static readonly WeaponDefinitionId RuneTrapWeaponId = new WeaponDefinitionId("weapon.survivors.rune-trap");
        public static readonly WeaponDefinitionId AetherMineWeaponId = new WeaponDefinitionId("weapon.survivors.aether-mine");
        public static readonly RunUpgradeEffectId DamageBonusEffect = new RunUpgradeEffectId("survivors.damage.flat");
        public static readonly RunUpgradeEffectId FireRateEffect = new RunUpgradeEffectId("survivors.weapon.cooldown_multiplier");
        public static readonly RunUpgradeEffectId MoveSpeedEffect = new RunUpgradeEffectId("survivors.player.move_speed");
        public static readonly RunUpgradeEffectId MagnetRangeEffect = new RunUpgradeEffectId("survivors.pickup.range");
        public static readonly RunUpgradeEffectId MaxHealthEffect = new RunUpgradeEffectId("survivors.player.max_health");
        public static readonly RunUpgradeEffectId OrbitBladeEffect = new RunUpgradeEffectId("survivors.orbit.blade_count");
        public static readonly RunUpgradeEffectId MeleeTargetEffect = new RunUpgradeEffectId("survivors.melee.target_count");
        public static readonly RunUpgradeEffectId BurstCountEffect = new RunUpgradeEffectId("survivors.burst.count");
        public static readonly RunUpgradeEffectId ProjectilePierceEffect = new RunUpgradeEffectId("survivors.projectile.pierce_count");
        public static readonly RunUpgradeEffectId ProjectileChainEffect = new RunUpgradeEffectId("survivors.projectile.chain_count");
        public static readonly RunUpgradeEffectId ProjectileForkEffect = new RunUpgradeEffectId("survivors.projectile.fork_count");
        public static readonly RunUpgradeEffectId ProjectileReturnEffect = new RunUpgradeEffectId("survivors.projectile.return_count");
        public static readonly RunUpgradeEffectId HitscanPierceEffect = new RunUpgradeEffectId("survivors.hitscan.pierce");
        public static readonly RunUpgradeEffectId PayloadCountEffect = new RunUpgradeEffectId("survivors.payload.count");
        public static readonly RunUpgradeEffectId PayloadRadiusEffect = new RunUpgradeEffectId("survivors.payload.radius");
        public static readonly RunUpgradeEffectId PayloadTriggerRadiusEffect = new RunUpgradeEffectId("survivors.payload.trigger_radius");
        public static readonly RunUpgradeTargetId PlayerTarget = new RunUpgradeTargetId("survivors.player");
        public static readonly RunUpgradeTargetId WeaponTarget = new RunUpgradeTargetId("survivors.weapon.arcane-wand");
        public static readonly RunUpgradeTargetId OrbitWeaponTarget = new RunUpgradeTargetId("survivors.weapon.orbit-ward");
        public static readonly RunUpgradeTargetId MeleeWeaponTarget = new RunUpgradeTargetId("survivors.weapon.moon-slash");
        public static readonly RunUpgradeTargetId BurstWeaponTarget = new RunUpgradeTargetId("survivors.weapon.star-nova");
        public static readonly RunUpgradeTargetId HitscanWeaponTarget = new RunUpgradeTargetId("survivors.weapon.star-beam");
        public static readonly RunUpgradeTargetId GrenadeWeaponTarget = new RunUpgradeTargetId("survivors.weapon.gravity-grenade");
        public static readonly RunUpgradeTargetId TrapWeaponTarget = new RunUpgradeTargetId("survivors.weapon.rune-trap");
        public static readonly RunUpgradeTargetId MineWeaponTarget = new RunUpgradeTargetId("survivors.weapon.aether-mine");
        public static readonly RunUpgradeTargetId PayloadWeaponTarget = new RunUpgradeTargetId("survivors.weapon.payloads");
        public static readonly RunUpgradeTargetId PickupTarget = new RunUpgradeTargetId("survivors.pickups");

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

        public static IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateWeaponArchetypeDefinitions(SurvivorsTemplateTuning tuning = null)
        {
            SurvivorsTemplateTuning resolved = tuning ?? CreateDefaultTuning();
            return new[]
            {
                new SurvivorsWeaponArchetypeDefinition(
                    ArcaneWandWeaponId.Value,
                    "Arcane Wand",
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
                    "weapon.survivors.orbit-ward",
                    "Orbit Ward",
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
                    "weapon.survivors.moon-slash",
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
                    "weapon.survivors.star-nova",
                    "Star Nova",
                    SurvivorsWeaponArchetype.Burst,
                    resolved.BurstCooldownSeconds,
                    resolved.BurstDamage,
                    resolved.BurstRadius,
                    new Color(1f, 0.42f, 0.72f),
                    burstCount: resolved.BurstCount,
                    burstRepeatIntervalSeconds: resolved.BurstRepeatIntervalSeconds,
                    burstVisualDurationSeconds: resolved.BurstVisualDurationSeconds),
                new SurvivorsWeaponArchetypeDefinition(
                    StarBeamWeaponId.Value,
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
                    GravityGrenadeWeaponId.Value,
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
                    RuneTrapWeaponId.Value,
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
                    AetherMineWeaponId.Value,
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
                Upgrade("upgrade.survivors.orbiting-focus", RunUpgradeRarity.Uncommon, 38, 4, OrbitBladeEffect, OrbitWeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.crescent-chain", RunUpgradeRarity.Uncommon, 34, 4, MeleeTargetEffect, MeleeWeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.nova-echo", RunUpgradeRarity.Rare, 24, 3, BurstCountEffect, BurstWeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.piercing-bolts", RunUpgradeRarity.Uncommon, 36, 4, ProjectilePierceEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.chain-bolts", RunUpgradeRarity.Rare, 26, 3, ProjectileChainEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.forked-bolts", RunUpgradeRarity.Rare, 24, 3, ProjectileForkEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.returning-bolts", RunUpgradeRarity.Rare, 22, 2, ProjectileReturnEffect, WeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.prismatic-beam", RunUpgradeRarity.Uncommon, 30, 3, HitscanPierceEffect, HitscanWeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.extra-payload", RunUpgradeRarity.Rare, 22, 2, PayloadCountEffect, PayloadWeaponTarget, 1.0d),
                Upgrade("upgrade.survivors.bigger-booms", RunUpgradeRarity.Uncommon, 32, 4, PayloadRadiusEffect, PayloadWeaponTarget, 0.45d),
                Upgrade("upgrade.survivors.wider-triggers", RunUpgradeRarity.Uncommon, 28, 4, PayloadTriggerRadiusEffect, PayloadWeaponTarget, 0.35d)
            });
        }

        public static IReadOnlyList<RunUpgradeTargetId> CreateKnownUpgradeTargets()
        {
            return new[]
            {
                PlayerTarget,
                WeaponTarget,
                OrbitWeaponTarget,
                MeleeWeaponTarget,
                BurstWeaponTarget,
                HitscanWeaponTarget,
                GrenadeWeaponTarget,
                TrapWeaponTarget,
                MineWeaponTarget,
                PayloadWeaponTarget,
                PickupTarget
            };
        }

        public static string GetUpgradeDisplayName(RunUpgradeId id)
        {
            string value = id.Value;
            if (value == "upgrade.survivors.arcane-damage") return "Arcane Damage";
            if (value == "upgrade.survivors.quick-casting") return "Quick Casting";
            if (value == "upgrade.survivors.swift-steps") return "Swift Steps";
            if (value == "upgrade.survivors.gem-magnet") return "Gem Magnet";
            if (value == "upgrade.survivors.iron-blood") return "Iron Blood";
            if (value == "upgrade.survivors.orbiting-focus") return "Orbiting Focus";
            if (value == "upgrade.survivors.crescent-chain") return "Crescent Chain";
            if (value == "upgrade.survivors.nova-echo") return "Nova Echo";
            if (value == "upgrade.survivors.piercing-bolts") return "Piercing Bolts";
            if (value == "upgrade.survivors.chain-bolts") return "Chain Bolts";
            if (value == "upgrade.survivors.forked-bolts") return "Forked Bolts";
            if (value == "upgrade.survivors.returning-bolts") return "Returning Bolts";
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
