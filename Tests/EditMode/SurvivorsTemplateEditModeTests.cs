using System;
using System.Collections.Generic;
using System.IO;
using Deucarian.Persistence;
using Deucarian.Projectiles;
using Deucarian.Progression;
using Deucarian.RunUpgrades;
using Deucarian.WeaponSystems;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsTemplateEditModeTests
    {
        [Test]
        public void DefaultContentUsesStableDeucarianDescriptors()
        {
            WeaponDefinition weapon = BasicSurvivorsGame.CreateWeaponDefinition();
            ProjectileDefinition projectile = BasicSurvivorsGame.CreateProjectileDefinition();
            RunUpgradeCatalog upgrades = BasicSurvivorsGame.CreateRunUpgradeCatalog();
            var archetypes = BasicSurvivorsGame.CreateWeaponArchetypeDefinitions();
            SurvivorsMetaProgressionDefinition meta = BasicSurvivorsGame.CreateMetaProgressionDefinition();
            var relics = BasicSurvivorsGame.CreateRelicDefinitions();
            SurvivorsClassLibraryDefinition classes = BasicSurvivorsGame.CreateClassLibraryDefinition();
            var progressionTracks = BasicSurvivorsGame.CreateProgressionTrackDefinitions();
            var upgradeMetadata = BasicSurvivorsGame.CreateRunUpgradeMetadata();

            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponId, weapon.Id);
            Assert.AreEqual(WeaponFireMode.Projectile, weapon.FireMode);
            Assert.AreEqual(BasicSurvivorsGame.ArcaneBoltProjectileId, projectile.Id);
            Assert.AreEqual(BasicSurvivorsGame.ProjectileSpawnableId, projectile.SpawnableId);
            Assert.That(upgrades.Definitions.Count, Is.GreaterThanOrEqualTo(30));
            Assert.That(archetypes.Count, Is.GreaterThanOrEqualTo(10));
            Assert.That(archetypes[0].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Projectile));
            Assert.That(archetypes[1].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Projectile));
            Assert.That(archetypes[2].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Orbit));
            Assert.That(archetypes[3].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Orbit));
            Assert.That(archetypes[4].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Melee));
            Assert.That(archetypes[5].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Burst));
            Assert.AreEqual(BasicSurvivorsGame.BloodShardsCurrencyId, meta.BloodShardsCurrencyId);
            Assert.AreEqual(BasicSurvivorsGame.LegacyExperienceTrackId, meta.LegacyExperienceTrackId);
            Assert.That(meta.PersistentUpgrades.Count, Is.GreaterThanOrEqualTo(7));
            Assert.IsTrue(meta.TryGetPersistentUpgrade(BasicSurvivorsGame.VitalWardMetaUpgradeId.Value, out SurvivorsPersistentUpgradeDefinition vitalWard));
            Assert.AreEqual(BasicSurvivorsGame.MetaMaxHealthEffectId, vitalWard.EffectId);
            Assert.IsTrue(meta.TryGetPersistentUpgrade(BasicSurvivorsGame.GemheartLegacyMetaUpgradeId.Value, out SurvivorsPersistentUpgradeDefinition gemheartLegacy));
            Assert.AreEqual(BasicSurvivorsGame.MetaPickupRangeEffectId, gemheartLegacy.EffectId);
            Assert.IsTrue(meta.TryGetPersistentUpgrade(BasicSurvivorsGame.ScholarIndexMetaUpgradeId.Value, out SurvivorsPersistentUpgradeDefinition scholarIndex));
            Assert.AreEqual(BasicSurvivorsGame.MetaExperienceGainEffectId, scholarIndex.EffectId);
            Assert.IsTrue(meta.TryGetPersistentUpgrade(BasicSurvivorsGame.PreparedDraftMetaUpgradeId.Value, out SurvivorsPersistentUpgradeDefinition preparedDraft));
            Assert.AreEqual(BasicSurvivorsGame.MetaDraftRerollEffectId, preparedDraft.EffectId);
            Assert.That(meta.Rewards.Count, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(6, relics.Count);
            Assert.AreEqual(2, classes.Classes.Count);
            Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, classes.DefaultClassId);
            Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, classes.Classes[0].Id);
            Assert.That(classes.Classes[0].StartingWeaponIds.Count, Is.GreaterThanOrEqualTo(5));
            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponContentId, classes.Classes[0].StartingWeaponIds[0]);
            Assert.That(classes.Classes[0].StartingWeaponIds, Does.Contain(BasicSurvivorsGame.FrostFanWeaponContentId));
            Assert.That(classes.Classes[0].StartingWeaponIds, Does.Contain(BasicSurvivorsGame.OrbitWardWeaponContentId));
            Assert.That(classes.Classes[0].StartingWeaponIds, Does.Contain(BasicSurvivorsGame.ThornHaloWeaponContentId));
            Assert.That(classes.Classes[0].StartingWeaponIds, Does.Contain(BasicSurvivorsGame.StarNovaWeaponContentId));
            Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, classes.Classes[1].Id);
            Assert.That(classes.Classes[1].StartingWeaponIds.Count, Is.GreaterThan(classes.Classes[0].StartingWeaponIds.Count));
            Assert.That(progressionTracks.Count, Is.GreaterThanOrEqualTo(10));
            Assert.AreEqual(SurvivorsProgressionTrackKind.PassiveAtlas, progressionTracks[0].Kind);
            Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, progressionTracks[0].ClassId);
            Assert.That(progressionTracks[0].Nodes.Count, Is.GreaterThanOrEqualTo(4));
            Assert.That(BasicSurvivorsGame.CreateClassUpgradeGates().Count, Is.GreaterThanOrEqualTo(9));
            AssertRunUpgradeMetadataCoversPassivesAndEvolutions(upgrades, upgradeMetadata);
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.ScholarsLensUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.GiantRuneUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.TwinCharmUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.AstralConvergenceUpgradeId));
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.ArcaneWandUnlockUpgradeId, BasicSurvivorsGame.ArcaneWandWeaponContentId);
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.FrostFanUnlockUpgradeId, BasicSurvivorsGame.FrostFanWeaponContentId);
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.OrbitWardUnlockUpgradeId, BasicSurvivorsGame.OrbitWardWeaponContentId);
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.ThornHaloUnlockUpgradeId, BasicSurvivorsGame.ThornHaloWeaponContentId);
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.StarNovaUnlockUpgradeId, BasicSurvivorsGame.StarNovaWeaponContentId);
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.StarBeamUnlockUpgradeId));
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.StarBeamUnlockUpgradeId, BasicSurvivorsGame.StarBeamWeaponContentId);
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.StarPulseUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.GravityGrenadeUnlockUpgradeId));
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.GravityGrenadeUnlockUpgradeId, BasicSurvivorsGame.GravityGrenadeWeaponContentId);
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.MoonSlashUnlockUpgradeId, BasicSurvivorsGame.MoonSlashWeaponContentId);
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.RuneTrapUnlockUpgradeId, BasicSurvivorsGame.RuneTrapWeaponContentId);
            AssertWeaponUnlockMetadata(upgrades, upgradeMetadata, BasicSurvivorsGame.AetherMineUnlockUpgradeId, BasicSurvivorsGame.AetherMineWeaponContentId);
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.FrostSplinterUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.FrostRicochetUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.SerratedOrbitUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.BrambleGuardUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.HaloSpiralUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.LunarTempoUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.MoonOathUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.SnaringRunesUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.AetherBloomUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.FrostFanUpgradeId), Is.GreaterThanOrEqualTo(5));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.OrbitingFocusUpgradeId), Is.GreaterThanOrEqualTo(5));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.BrambleGuardUpgradeId), Is.GreaterThanOrEqualTo(5));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.MoonlitEdgeUpgradeId), Is.GreaterThanOrEqualTo(5));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.RuneLatticeUpgradeId), Is.GreaterThanOrEqualTo(5));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.NovaEchoUpgradeId), Is.GreaterThanOrEqualTo(5));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.StarFocusUpgradeId), Is.GreaterThanOrEqualTo(5));
            Assert.That(GetMaxRank(upgrades, BasicSurvivorsGame.ExtraPayloadUpgradeId), Is.GreaterThanOrEqualTo(5));
            AssertProgressionTrackContains(progressionTracks, "progression.survivors.blood-ring.weapon", BasicSurvivorsGame.SerratedOrbitUpgradeId);
            AssertProgressionTrackContains(progressionTracks, "progression.survivors.thorn-halo.weapon", BasicSurvivorsGame.BrambleGuardUpgradeId);
            Assert.That(CountUpgradesByRarity(upgrades, RunUpgradeRarity.Epic), Is.GreaterThanOrEqualTo(2));
            Assert.IsNotNull(BasicSurvivorsGame.CreateEncounterDefinition());
        }

        private static void AssertProgressionTrackContains(
            IReadOnlyList<SurvivorsProgressionTrackDefinition> tracks,
            string trackId,
            string upgradeId)
        {
            Assert.NotNull(tracks);
            for (int trackIndex = 0; trackIndex < tracks.Count; trackIndex++)
            {
                SurvivorsProgressionTrackDefinition track = tracks[trackIndex];
                if (track == null || !string.Equals(track.Id, trackId, StringComparison.Ordinal))
                {
                    continue;
                }

                for (int nodeIndex = 0; nodeIndex < track.Nodes.Count; nodeIndex++)
                {
                    SurvivorsProgressionNodeDefinition node = track.Nodes[nodeIndex];
                    if (node != null && string.Equals(node.UpgradeId, upgradeId, StringComparison.Ordinal))
                    {
                        return;
                    }
                }

                Assert.Fail("Track " + trackId + " is missing upgrade " + upgradeId);
            }

            Assert.Fail("Missing progression track " + trackId);
        }

        private static bool ContainsUpgrade(RunUpgradeCatalog catalog, string upgradeId)
        {
            return catalog != null &&
                !string.IsNullOrWhiteSpace(upgradeId) &&
                catalog.TryGet(new RunUpgradeId(upgradeId), out _);
        }

        private static int CountUpgradesByRarity(RunUpgradeCatalog catalog, RunUpgradeRarity rarity)
        {
            if (catalog == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < catalog.Definitions.Count; i++)
            {
                if (catalog.Definitions[i] != null && catalog.Definitions[i].Rarity == rarity)
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetMaxRank(RunUpgradeCatalog catalog, string upgradeId)
        {
            return catalog != null &&
                !string.IsNullOrWhiteSpace(upgradeId) &&
                catalog.TryGet(new RunUpgradeId(upgradeId), out RunUpgradeDefinition definition)
                    ? definition.MaxRank
                    : 0;
        }

        private static void AssertWeaponUnlockMetadata(
            RunUpgradeCatalog catalog,
            IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata,
            string upgradeId,
            string weaponId)
        {
            Assert.IsTrue(ContainsUpgrade(catalog, upgradeId), "Missing weapon unlock: " + upgradeId);
            Assert.That(GetMaxRank(catalog, upgradeId), Is.EqualTo(1));
            SurvivorsRunUpgradeMetadata entry = FindMetadata(metadata, upgradeId);
            Assert.NotNull(entry, "Missing metadata for " + upgradeId);
            Assert.AreEqual(SurvivorsRunUpgradeCategory.Weapon, entry.Category);
            Assert.AreEqual(SurvivorsRunBuildSlotKind.Weapon, entry.SlotKind);
            Assert.AreEqual(weaponId, entry.AffectedContentId);
        }

        private static SurvivorsRunUpgradeMetadata FindMetadata(IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata, string upgradeId)
        {
            if (metadata == null)
            {
                return null;
            }

            for (int i = 0; i < metadata.Count; i++)
            {
                if (metadata[i] != null && string.Equals(metadata[i].UpgradeId, upgradeId, StringComparison.Ordinal))
                {
                    return metadata[i];
                }
            }

            return null;
        }

        private static void AssertRunUpgradeMetadataCoversPassivesAndEvolutions(RunUpgradeCatalog upgrades, IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata)
        {
            Assert.NotNull(metadata);
            Assert.That(metadata.Count, Is.GreaterThanOrEqualTo(upgrades.Definitions.Count));

            int passiveCount = 0;
            int weaponUnlockCount = 0;
            int evolutionCount = 0;
            var metadataIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < metadata.Count; i++)
            {
                SurvivorsRunUpgradeMetadata entry = metadata[i];
                Assert.NotNull(entry);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.UpgradeId));
                Assert.IsTrue(metadataIds.Add(entry.UpgradeId), "Duplicate metadata id: " + entry.UpgradeId);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Description));

                if (entry.UsesPassiveSlot)
                {
                    passiveCount++;
                }

                if (entry.UsesWeaponSlot)
                {
                    weaponUnlockCount++;
                    Assert.AreEqual(SurvivorsRunUpgradeCategory.Weapon, entry.Category);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.AffectedContentId));
                }

                if (entry.IsEvolution)
                {
                    evolutionCount++;
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.RequiredOwnedWeaponId));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.RequiredUpgradeId));
                    Assert.That(entry.RequiredUpgradeRank, Is.GreaterThan(0));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.RequiredPassiveUpgradeId));
                }
            }

            for (int i = 0; i < upgrades.Definitions.Count; i++)
            {
                Assert.IsTrue(metadataIds.Contains(upgrades.Definitions[i].Id.Value), "Missing metadata for " + upgrades.Definitions[i].Id.Value);
            }

            Assert.That(passiveCount, Is.GreaterThanOrEqualTo(8));
            Assert.That(weaponUnlockCount, Is.GreaterThanOrEqualTo(10));
            Assert.That(evolutionCount, Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void RuntimeContentValidationPassesForDefaultCatalogs()
        {
            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateRuntimeContent(
                BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(),
                BasicSurvivorsGame.CreateRunUpgradeCatalog(),
                BasicSurvivorsGame.CreateKnownUpgradeTargets(),
                BasicSurvivorsGame.CreateRunFlowDefinition(),
                BasicSurvivorsGame.CreateMetaProgressionDefinition(),
                BasicSurvivorsGame.CreateRelicDefinitions(),
                BasicSurvivorsGame.CreateClassLibraryDefinition(),
                BasicSurvivorsGame.CreateClassUpgradeGates(),
                BasicSurvivorsGame.CreateProgressionTrackDefinitions());

            Assert.IsTrue(result.Succeeded, string.Join(Environment.NewLine, result.Errors));
        }

        [Test]
        public void DefaultPacingProfileIsHumanPlaytestAndReadable()
        {
            SurvivorsTemplateTuning tuning = BasicSurvivorsGame.CreateDefaultTuning();
            SurvivorsEnemyProfile swarm = BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm, tuning);
            SurvivorsEnemyProfile runner = BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Runner, tuning);
            SurvivorsRunFlowDefinition flow = BasicSurvivorsGame.CreateRunFlowDefinition(tuning);
            var runtime = new SurvivorsRunFlowRuntime(flow);

            Assert.AreEqual(SurvivorsPacingProfile.HumanPlaytest, tuning.PacingProfile);
            Assert.That(tuning.EnemySpawnIntervalSeconds, Is.InRange(0.9f, 1.4f));
            Assert.That(tuning.MinimumEnemySpawnIntervalSeconds, Is.GreaterThanOrEqualTo(0.3f));
            Assert.That(tuning.EnemyMaximumAlive, Is.InRange(24, 40));
            Assert.That(tuning.MajorThreatEnrageHealthThreshold, Is.InRange(0.4f, 0.65f));
            Assert.That(tuning.MajorThreatEnrageBossSupportCount, Is.GreaterThan(tuning.MajorThreatEnrageMinibossSupportCount));
            Assert.That(tuning.MajorThreatEnrageMinibossSupportCount, Is.GreaterThan(tuning.MajorThreatEnrageEliteSupportCount));
            Assert.That(tuning.EnemySpawnPackBaseCount, Is.InRange(2, 3));
            Assert.That(tuning.EnemySpawnPackMaxCount, Is.GreaterThan(tuning.EnemySpawnPackBaseCount));
            Assert.That(tuning.EnemySpawnPackIncreaseEveryEscalations, Is.GreaterThanOrEqualTo(1));
            Assert.That(swarm.MoveSpeed, Is.InRange(1.1f, 1.6f));
            Assert.That(swarm.MoveSpeed, Is.LessThan(tuning.PlayerMoveSpeed));
            Assert.That(runner.MoveSpeed, Is.LessThan(tuning.PlayerMoveSpeed));
            Assert.That(tuning.ProjectileSpeed, Is.LessThanOrEqualTo(9f));
            Assert.That(tuning.PickupAttractRange, Is.InRange(2f, 3f));
            Assert.That(tuning.RoamingCacheTravelInterval, Is.InRange(14f, 22f));
            Assert.That(tuning.RoamingCacheExperienceGemCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(tuning.RoamingCacheMagnetInterval, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.RoamingCacheBloodShardInterval, Is.GreaterThan(tuning.RoamingCacheMagnetInterval));
            Assert.That(tuning.RoamingCacheAmbushStartCache, Is.GreaterThanOrEqualTo(3));
            Assert.That(tuning.RoamingCacheAmbushMaxEnemyCount, Is.GreaterThanOrEqualTo(tuning.RoamingCacheAmbushBaseEnemyCount));
            Assert.That(tuning.RewardSelectionTimeoutSeconds, Is.LessThanOrEqualTo(0f));
            Assert.That(tuning.ExperienceRequiredBase, Is.InRange(6, 10));
            Assert.That(tuning.ExperienceRequiredBase * tuning.EnemySpawnIntervalSeconds, Is.InRange(7f, 14f));
            Assert.That(tuning.EvolutionSurgeDamage, Is.GreaterThan(0f));
            Assert.That(tuning.EvolutionSurgeRadius, Is.InRange(4f, 8f));
            Assert.That(tuning.MajorThreatWarningLeadSeconds, Is.InRange(5f, 12f));
            Assert.That(tuning.FirstEliteSpawnTimeSeconds, Is.InRange(120f, 240f));
            Assert.That(tuning.FirstDreadEliteSpawnTimeSeconds, Is.InRange(240f, 360f));
            Assert.That(tuning.MinibossSpawnTimeSeconds, Is.InRange(300f, 480f));
            Assert.That(tuning.BossSpawnTimeSeconds, Is.InRange(1140f, 1260f));
            Assert.That(tuning.SurvivalVictoryTimeSeconds, Is.GreaterThanOrEqualTo(1800f));

            Assert.IsFalse(runtime.TryConsumeTimedEliteSpawn(tuning.FirstEliteSpawnTimeSeconds - 0.1f, out _));
            Assert.IsTrue(runtime.TryConsumeTimedEliteSpawn(tuning.FirstEliteSpawnTimeSeconds, out SurvivorsEnemyRole firstEliteRole));
            Assert.AreEqual(SurvivorsEnemyRole.Elite, firstEliteRole);
            Assert.IsTrue(runtime.TryConsumeTimedEliteSpawn(tuning.FirstDreadEliteSpawnTimeSeconds, out SurvivorsEnemyRole firstDreadEliteRole));
            Assert.AreEqual(SurvivorsEnemyRole.DreadElite, firstDreadEliteRole);
            Assert.IsFalse(runtime.TryConsumeTimedEliteSpawn(tuning.FirstDreadEliteSpawnTimeSeconds + 1f, out _));

            runtime.Tick(30f);
            Assert.AreEqual(SurvivorsRunPhase.Opening, runtime.Phase);
            Assert.AreEqual(SurvivorsEnemyRole.Swarm, runtime.ResolveNextSwarmRole(30f, 0L));

            runtime.Tick(60f);
            Assert.That(runtime.ResolveMaximumAlive(tuning.EnemyMaximumAlive), Is.InRange(36, 48));

            runtime.Tick(240f);
            Assert.That(runtime.ResolveMaximumAlive(tuning.EnemyMaximumAlive), Is.InRange(64, 84));
        }

        [Test]
        public void DebugFastPacingProfileIsOptInAndAccelerated()
        {
            SurvivorsTemplateTuning human = BasicSurvivorsGame.CreateDefaultTuning();
            SurvivorsTemplateTuning normal = BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Normal);
            SurvivorsTemplateTuning debugFast = BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.DebugFast);
            SurvivorsTemplateTuning showcase = BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Showcase);

            Assert.AreEqual(SurvivorsPacingProfile.HumanPlaytest, human.PacingProfile);
            Assert.AreEqual(SurvivorsPacingProfile.Normal, normal.PacingProfile);
            Assert.AreEqual(SurvivorsPacingProfile.DebugFast, debugFast.PacingProfile);
            Assert.AreEqual(SurvivorsPacingProfile.Showcase, showcase.PacingProfile);
            Assert.That(normal.EnemySpawnIntervalSeconds, Is.LessThan(human.EnemySpawnIntervalSeconds));
            Assert.That(normal.EnemyMaximumAlive, Is.GreaterThan(human.EnemyMaximumAlive));
            Assert.That(debugFast.EnemySpawnIntervalSeconds, Is.LessThan(human.EnemySpawnIntervalSeconds));
            Assert.That(debugFast.MinimumEnemySpawnIntervalSeconds, Is.LessThan(human.MinimumEnemySpawnIntervalSeconds));
            Assert.That(debugFast.EnemyMaximumAlive, Is.GreaterThan(human.EnemyMaximumAlive));
            Assert.That(debugFast.EnemySpawnPackBaseCount, Is.GreaterThanOrEqualTo(human.EnemySpawnPackBaseCount));
            Assert.That(debugFast.EnemySpawnPackMaxCount, Is.GreaterThan(human.EnemySpawnPackMaxCount));
            Assert.That(debugFast.ExperienceRequiredBase, Is.LessThan(human.ExperienceRequiredBase));
            Assert.That(debugFast.RoamingCacheTravelInterval, Is.LessThan(human.RoamingCacheTravelInterval));
            Assert.That(debugFast.RoamingCacheAmbushStartCache, Is.LessThan(human.RoamingCacheAmbushStartCache));
            Assert.That(showcase.RoamingCacheTravelInterval, Is.LessThan(human.RoamingCacheTravelInterval));
            Assert.That(debugFast.FirstEliteSpawnTimeSeconds, Is.LessThan(human.FirstEliteSpawnTimeSeconds));
            Assert.That(debugFast.MinibossSpawnTimeSeconds, Is.LessThan(human.MinibossSpawnTimeSeconds));
            Assert.That(normal.FirstDreadEliteSpawnTimeSeconds, Is.LessThan(normal.MinibossSpawnTimeSeconds));
            Assert.That(debugFast.FirstDreadEliteSpawnTimeSeconds, Is.LessThan(debugFast.MinibossSpawnTimeSeconds));
            Assert.That(showcase.FirstDreadEliteSpawnTimeSeconds, Is.LessThan(showcase.MinibossSpawnTimeSeconds));
            Assert.That(debugFast.RewardSelectionTimeoutSeconds, Is.GreaterThan(human.RewardSelectionTimeoutSeconds));
            Assert.That(showcase.EnemySpawnIntervalSeconds, Is.GreaterThan(debugFast.EnemySpawnIntervalSeconds));
            Assert.That(showcase.EnemySpawnIntervalSeconds, Is.LessThan(human.EnemySpawnIntervalSeconds));
        }

        [Test]
        public void SampleContentJsonLoadsAndValidates()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string pickupJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultPickups", "pickups.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, enemyJson, rewardJson, relicJson, classJson, progressionJson, pickupJson);

            Assert.IsTrue(result.Succeeded, string.Join(Environment.NewLine, result.Errors));
        }

        [Test]
        public void InvalidSampleContentReportsDuplicateAndMissingReferences()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.dup\",\"fireMode\":\"Projectile\",\"projectileId\":\"projectile.missing\"},{\"id\":\"weapon.dup\",\"fireMode\":\"NoSuchArchetype\"}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.dup\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"target.missing\"},{\"id\":\"upgrade.dup\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.player\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate weapon id", errors);
            StringAssert.Contains("references missing projectile id", errors);
            StringAssert.Contains("unknown archetype", errors);
            StringAssert.Contains("Duplicate upgrade id", errors);
            StringAssert.Contains("unknown target", errors);
        }

        [Test]
        public void InvalidPayloadSampleContentReportsPayloadErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.payload.bad\",\"fireMode\":\"Grenade\",\"payloadCount\":0,\"payloadTravelSpeed\":0,\"payloadArmingSeconds\":0,\"payloadLifetimeSeconds\":0,\"payloadTriggerRadius\":0,\"payloadExplosionRadius\":0,\"payloadLeavesHazard\":true,\"payloadHazardDurationSeconds\":0,\"payloadHazardTickIntervalSeconds\":0,\"payloadHazardDamageRatio\":-1}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.payloads\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("payload count", errors);
            StringAssert.Contains("payload travel speed", errors);
            StringAssert.Contains("payload arming time", errors);
            StringAssert.Contains("payload lifetime", errors);
            StringAssert.Contains("payload trigger radius", errors);
            StringAssert.Contains("payload explosion radius", errors);
            StringAssert.Contains("hazard duration", errors);
            StringAssert.Contains("hazard tick interval", errors);
            StringAssert.Contains("negative hazard damage ratio", errors);
        }

        [Test]
        public void InvalidPickupSampleContentReportsPickupErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\"}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\"}]}";
            string pickupJson = "{\"pickups\":[{\"id\":\"pickup.dup\",\"displayName\":\"\",\"attractRange\":-1,\"attractionSpeed\":-2},{\"id\":\"pickup.dup\",\"displayName\":\"Duplicate\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, pickupJson: pickupJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate pickup id", errors);
            StringAssert.Contains("missing a display name", errors);
            StringAssert.Contains("negative attract range", errors);
            StringAssert.Contains("negative attraction speed", errors);
            StringAssert.Contains("requires behavior text", errors);
            StringAssert.Contains("missing required pickup id", errors);
        }

        [Test]
        public void InvalidRewardSampleContentReportsMetaErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\",\"hitscanCount\":1,\"hitscanWidth\":1}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\"}]}";
            string rewardJson = "{\"currencies\":[{\"id\":\"currency.dup\"},{\"id\":\"currency.dup\"}],\"tracks\":[{\"id\":\"track.valid\"}],\"persistentUpgrades\":[{\"id\":\"meta.dup\",\"target\":\"target.missing\",\"effect\":\"\",\"maxRank\":2,\"rankCosts\":[5]},{\"id\":\"meta.dup\",\"target\":\"survivors.weapon.arcane-wand\",\"effect\":\"effect.valid\",\"maxRank\":1,\"rankCosts\":[0]}],\"rewards\":[{\"id\":\"reward.dup\",\"currencyId\":\"currency.missing\",\"trackId\":\"track.missing\",\"currencyAmount\":0,\"trackAmount\":0},{\"id\":\"reward.dup\",\"currencyId\":\"currency.dup\",\"trackId\":\"track.valid\",\"currencyAmount\":1,\"trackAmount\":0}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, rewardJson: rewardJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate currency id", errors);
            StringAssert.Contains("Duplicate persistent upgrade id", errors);
            StringAssert.Contains("unknown target", errors);
            StringAssert.Contains("missing an effect id", errors);
            StringAssert.Contains("rank cost count", errors);
            StringAssert.Contains("rank cost must be above zero", errors);
            StringAssert.Contains("amount per rank", errors);
            StringAssert.Contains("Duplicate reward id", errors);
            StringAssert.Contains("unknown currency", errors);
            StringAssert.Contains("unknown progression track", errors);
            StringAssert.Contains("must grant currency or legacy XP", errors);
        }

        [Test]
        public void InvalidRelicAndClassSampleContentReportsErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\",\"hitscanCount\":1,\"hitscanWidth\":1}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\",\"allowedClasses\":[\"class.missing\",\"class.missing\",\"\"]}]}";
            string relicJson = "{\"relics\":[{\"id\":\"relic.dup\",\"target\":\"target.missing\",\"effect\":\"\",\"effectKind\":\"NoSuchKind\",\"amount\":0,\"weight\":0},{\"id\":\"relic.dup\",\"target\":\"survivors.pickups\",\"effect\":\"effect.valid\",\"effectKind\":\"PickupRange\",\"amount\":1,\"weight\":1}]}";
            string classJson = "{\"defaultClassId\":\"class.missing\",\"classes\":[{\"id\":\"class.dup\",\"startingWeaponId\":\"weapon.missing\",\"startingWeaponIds\":[\"weapon.missing\",\"weapon.missing\",\"\"] ,\"unlockedByDefault\":false,\"unlockRewardId\":\"\",\"statModifiers\":[{\"stat\":\"NoSuchStat\",\"amount\":0}]},{\"id\":\"class.dup\",\"startingWeaponId\":\"weapon.valid\",\"startingWeaponIds\":[],\"unlockedByDefault\":false,\"unlockRewardId\":\"reward.unlock\",\"statModifiers\":[]}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, relicJson: relicJson, classJson: classJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate relic id", errors);
            StringAssert.Contains("unknown target", errors);
            StringAssert.Contains("missing an effect id", errors);
            StringAssert.Contains("unknown effect kind", errors);
            StringAssert.Contains("weight above zero", errors);
            StringAssert.Contains("Duplicate class id", errors);
            StringAssert.Contains("unknown starting weapon", errors);
            StringAssert.Contains("loadout weapon", errors);
            StringAssert.Contains("duplicate starting weapon id", errors);
            StringAssert.Contains("starting weapon loadout contains an empty id", errors);
            StringAssert.Contains("allowed class list contains an empty id", errors);
            StringAssert.Contains("allowed class list contains duplicate id", errors);
            StringAssert.Contains("references unknown class id", errors);
            StringAssert.Contains("unknown default class id", errors);
            StringAssert.Contains("unlock reward id", errors);
            StringAssert.Contains("unknown stat", errors);
            StringAssert.Contains("requires at least one default unlocked class", errors);
        }

        [Test]
        public void InvalidRuntimeClassUpgradeGateReportsErrors()
        {
            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateRuntimeContent(
                BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(),
                BasicSurvivorsGame.CreateRunUpgradeCatalog(),
                BasicSurvivorsGame.CreateKnownUpgradeTargets(),
                classes: BasicSurvivorsGame.CreateClassLibraryDefinition(),
                classUpgradeGates: new[]
                {
                    new SurvivorsClassUpgradeGateDefinition("upgrade.survivors.missing", new[] { BasicSurvivorsGame.EmberVanguardClassId }),
                    new SurvivorsClassUpgradeGateDefinition(BasicSurvivorsGame.PrismaticBeamUpgradeId, new[] { "class.survivors.missing" }),
                    new SurvivorsClassUpgradeGateDefinition(BasicSurvivorsGame.PrismaticBeamUpgradeId, new[] { BasicSurvivorsGame.EmberVanguardClassId })
                });
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("unknown upgrade id", errors);
            StringAssert.Contains("unknown class id", errors);
            StringAssert.Contains("Duplicate class upgrade gate id", errors);
        }

        [Test]
        public void InvalidRuntimeProgressionTracksReportErrors()
        {
            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateRuntimeContent(
                BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(),
                BasicSurvivorsGame.CreateRunUpgradeCatalog(),
                BasicSurvivorsGame.CreateKnownUpgradeTargets(),
                classes: BasicSurvivorsGame.CreateClassLibraryDefinition(),
                progressionTracks: new[]
                {
                    new SurvivorsProgressionTrackDefinition(
                        "progression.dup",
                        "Duplicate",
                        SurvivorsProgressionTrackKind.PassiveAtlas,
                        "class.survivors.missing",
                        string.Empty,
                        new[]
                        {
                            new SurvivorsProgressionNodeDefinition("node.dup", "Missing Upgrade", "upgrade.survivors.missing", SurvivorsProgressionNodeKind.Passive, 0, 1, 1)
                        }),
                    new SurvivorsProgressionTrackDefinition(
                        "progression.dup",
                        "Bad Weapon",
                        SurvivorsProgressionTrackKind.WeaponSkillTrack,
                        BasicSurvivorsGame.DefaultClassId,
                        "weapon.survivors.missing",
                        new[]
                        {
                            new SurvivorsProgressionNodeDefinition("node.dup", "Too Many Ranks", "upgrade.survivors.arcane-damage", SurvivorsProgressionNodeKind.WeaponRank, -1, 0, 999)
                        })
                });
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate progression track id", errors);
            StringAssert.Contains("unknown class id", errors);
            StringAssert.Contains("unknown target weapon", errors);
            StringAssert.Contains("unknown upgrade id", errors);
            StringAssert.Contains("Duplicate progression node id", errors);
            StringAssert.Contains("negative tier", errors);
            StringAssert.Contains("point cost above zero", errors);
            StringAssert.Contains("max rank exceeds", errors);
            StringAssert.Contains("requires a passive atlas", errors);
        }

        [Test]
        public void InvalidSampleProgressionReportsReferenceErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\",\"hitscanCount\":1,\"hitscanWidth\":1}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\"}]}";
            string classJson = "{\"defaultClassId\":\"class.valid\",\"classes\":[{\"id\":\"class.valid\",\"startingWeaponId\":\"weapon.valid\",\"startingWeaponIds\":[\"weapon.valid\"],\"unlockedByDefault\":true,\"unlockRewardId\":\"\",\"statModifiers\":[]}]}";
            string progressionJson = "{\"tracks\":[{\"id\":\"progression.dup\",\"kind\":\"PassiveAtlas\",\"classId\":\"class.missing\",\"targetWeaponId\":\"weapon.valid\",\"nodes\":[{\"id\":\"node.dup\",\"upgradeId\":\"upgrade.missing\",\"kind\":\"NoSuchNode\",\"tier\":-1,\"pointCost\":0,\"maxRank\":0}]},{\"id\":\"progression.dup\",\"kind\":\"WeaponSkillTrack\",\"classId\":\"class.valid\",\"targetWeaponId\":\"weapon.missing\",\"nodes\":[{\"id\":\"node.dup\",\"upgradeId\":\"upgrade.valid\",\"kind\":\"WeaponRank\",\"tier\":0,\"pointCost\":1,\"maxRank\":1}]}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, classJson: classJson, progressionJson: progressionJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate progression track id", errors);
            StringAssert.Contains("unknown class id", errors);
            StringAssert.Contains("should not target a weapon", errors);
            StringAssert.Contains("unknown target weapon", errors);
            StringAssert.Contains("unknown upgrade id", errors);
            StringAssert.Contains("Duplicate progression node id", errors);
            StringAssert.Contains("unknown kind", errors);
            StringAssert.Contains("negative tier", errors);
            StringAssert.Contains("point cost above zero", errors);
            StringAssert.Contains("max rank above zero", errors);
            StringAssert.Contains("requires a passive atlas", errors);
        }

        [Test]
        public void RunRewardCalculatorMatchesReferenceShape()
        {
            SurvivorsRunRewardSummary summary = SurvivorsRunRewardCalculator.Calculate(
                runDurationSeconds: 121f,
                levelReached: 5,
                minibossKills: 2,
                bossKills: 1,
                victory: true,
                bonusBloodShards: 18,
                bonusLegacyExperience: 120);

            Assert.AreEqual(34, summary.BloodShardsEarned);
            Assert.AreEqual(235, summary.LegacyExperienceEarned);
            Assert.AreEqual(5, summary.LevelReached);
            Assert.IsTrue(summary.Victory);
        }

        [Test]
        public void MetaProfileSaveMigrationMapsLegacyCurrencyAndRanks()
        {
            var storage = new InMemoryTextStorage();
            var persistence = new PersistenceService(storage);
            var slotId = new SaveSlotId("migration-test");
            var serializer = new NewtonsoftPersistenceSerializer();
            var legacyDefinition = new DocumentDefinition<LegacyMetaProfileV1>(
                SurvivorsMetaProgressionService.ProfileDocumentId,
                new SchemaVersion(1),
                () => new LegacyMetaProfileV1());
            var legacyProfile = new LegacyMetaProfileV1
            {
                BloodShards = 11,
                LegacyExperience = 42,
                HighestLevelReached = 6,
                BestRunDurationSeconds = 77f,
                CompletedRuns = 2,
                BossVictories = 1,
                PersistentUpgradeRanks =
                {
                    new SurvivorsPersistentUpgradeRankRecord
                    {
                        Id = BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value,
                        Rank = 1
                    }
                }
            };
            storage.Files["survivors-meta-profile__migration-test.json"] = SaveEnvelopeCodec.Create(
                legacyDefinition,
                legacyProfile,
                serializer,
                DateTimeOffset.UtcNow);
            using (var service = new SurvivorsMetaProgressionService(persistence, slotId))
            {
                LoadResult<SurvivorsMetaProfileDocument> load = service.Load();

                Assert.IsTrue(load.Succeeded, load.Message);
                Assert.AreEqual(LoadOutcome.Migrated, load.Outcome);
                Assert.AreEqual(11, service.LifetimeBloodShards);
                Assert.AreEqual(42, service.LifetimeLegacyExperience);
                Assert.AreEqual(6, service.HighestLevelReached);
                Assert.AreEqual(77f, service.BestRunDurationSeconds);
                Assert.AreEqual(2, service.CompletedRuns);
                Assert.AreEqual(1, service.BossVictories);
                Assert.AreEqual(1, service.GetPersistentUpgradeRank(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));
                Assert.AreEqual(string.Empty, service.SelectedClassId);
            }
        }

        [Test]
        public void ClassUnlockAndSelectionPersistInMetaProfile()
        {
            var storage = new InMemoryTextStorage();
            var slotId = new SaveSlotId("class-persistence-test");
            SurvivorsClassLibraryDefinition classes = BasicSurvivorsGame.CreateClassLibraryDefinition();

            using (var service = new SurvivorsMetaProgressionService(new PersistenceService(storage), slotId))
            {
                service.Load();
                service.EnsureDefaultClassUnlocks(classes);

                Assert.IsTrue(service.IsClassUnlocked(BasicSurvivorsGame.DefaultClassId, classes));
                Assert.IsFalse(service.IsClassUnlocked(BasicSurvivorsGame.EmberVanguardClassId, classes));
                Assert.IsTrue(service.UnlockClass(BasicSurvivorsGame.EmberVanguardClassId, classes));
                Assert.IsTrue(service.TrySetSelectedClass(BasicSurvivorsGame.EmberVanguardClassId, classes));
            }

            using (var service = new SurvivorsMetaProgressionService(new PersistenceService(storage), slotId))
            {
                service.Load();
                service.EnsureDefaultClassUnlocks(classes);

                Assert.IsTrue(service.IsClassUnlocked(BasicSurvivorsGame.EmberVanguardClassId, classes));
                Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, service.SelectedClassId);
                Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, service.ResolveSelectedClass(classes).Id);
            }
        }

        [Test]
        public void LockedOrMissingSelectedClassFallsBackToDefault()
        {
            var storage = new InMemoryTextStorage();
            var slotId = new SaveSlotId("class-fallback-test");
            SurvivorsClassLibraryDefinition classes = BasicSurvivorsGame.CreateClassLibraryDefinition();

            using (var service = new SurvivorsMetaProgressionService(new PersistenceService(storage), slotId))
            {
                service.Load();
                service.Profile.SelectedClassId = BasicSurvivorsGame.EmberVanguardClassId;
                service.Profile.UnlockedClassIds.Clear();
                service.Profile.UnlockedClassIds.Add(BasicSurvivorsGame.DefaultClassId);
                service.Save();
            }

            using (var service = new SurvivorsMetaProgressionService(new PersistenceService(storage), slotId))
            {
                service.Load();

                Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, service.SelectedClassId);
                Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, service.ResolveSelectedClass(classes).Id);
                Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, service.SelectedClassId);

                service.Profile.SelectedClassId = "class.survivors.removed";
                service.Save();
            }

            using (var service = new SurvivorsMetaProgressionService(new PersistenceService(storage), slotId))
            {
                service.Load();

                Assert.AreEqual("class.survivors.removed", service.SelectedClassId);
                Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, service.ResolveSelectedClass(classes).Id);
                Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, service.SelectedClassId);
            }
        }

        [Test]
        public void InvalidBossAndMinibossContentReportsRunFlowErrors()
        {
            var badRunFlow = new SurvivorsRunFlowDefinition(
                escalationIntervalSeconds: 0f,
                minimumEnemySpawnIntervalSeconds: 0f,
                enemySpawnIntervalReductionPerEscalation: -1f,
                enemyMaximumAliveIncreasePerEscalation: -1,
                enemyHealthMultiplierPerEscalation: 0f,
                enemyMoveSpeedMultiplierPerEscalation: 0f,
                enemyExperienceMultiplierPerEscalation: 0f,
                minibossSpawnTimeSeconds: 20f,
                miniboss: new SurvivorsEnemyProfile(SurvivorsEnemyRole.Boss, "", "", 0f, 0f, 0f, -1f, 0f, 0, Color.white),
                bossSpawnTimeSeconds: 10f,
                boss: new SurvivorsEnemyProfile(SurvivorsEnemyRole.Miniboss, "enemy.bad.boss", "", 0f, 0f, 0f, -1f, 0f, 0, Color.white),
                survivalVictoryTimeSeconds: 8f,
                firstEliteSpawnTimeSeconds: -1f,
                firstDreadEliteSpawnTimeSeconds: -2f);

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateRunFlowContent(badRunFlow);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("escalation interval", errors);
            StringAssert.Contains("First elite spawn time", errors);
            StringAssert.Contains("First dread elite spawn time", errors);
            StringAssert.Contains("Boss spawn time", errors);
            StringAssert.Contains("Survival victory time", errors);
            StringAssert.Contains("miniboss profile", errors);
            StringAssert.Contains("boss profile", errors);
            StringAssert.Contains("health above zero", errors);
        }

        [Test]
        public void RunFlowRuntimeEscalatesAndConsumesBossEvents()
        {
            SurvivorsRunFlowDefinition definition = BasicSurvivorsGame.CreateRunFlowDefinition();
            var runtime = new SurvivorsRunFlowRuntime(definition);

            runtime.Tick(definition.EscalationIntervalSeconds + 0.1f);

            Assert.AreEqual(1, runtime.EscalationLevel);
            Assert.AreEqual(SurvivorsRunPhase.Escalating, runtime.Phase);
            Assert.IsTrue(runtime.TryConsumeMinibossSpawn(definition.MinibossSpawnTimeSeconds));
            Assert.IsFalse(runtime.TryConsumeMinibossSpawn(definition.MinibossSpawnTimeSeconds + 1f));
            Assert.IsTrue(runtime.TryConsumeBossSpawn(definition.BossSpawnTimeSeconds));
            Assert.IsFalse(runtime.TryConsumeBossSpawn(definition.BossSpawnTimeSeconds + 1f));
            Assert.IsTrue(runtime.TryConsumeBossVictory());
            Assert.AreEqual(SurvivorsRunPhase.Victory, runtime.Phase);
        }

        [Test]
        public void InvalidEnemySampleContentReportsBossDefinitionErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Grenade\",\"payloadCount\":1,\"payloadTravelSpeed\":1,\"payloadArmingSeconds\":1,\"payloadLifetimeSeconds\":1,\"payloadTriggerRadius\":1,\"payloadExplosionRadius\":1}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.payloads\"}]}";
            string enemyJson = "{\"enemies\":[{\"id\":\"enemy.dup\",\"role\":\"Miniboss\",\"health\":0,\"moveSpeed\":0,\"radius\":0,\"contactDamage\":-1,\"contactIntervalSeconds\":0,\"experienceDrop\":0,\"spawnTimeSeconds\":0},{\"id\":\"enemy.dup\",\"role\":\"NoSuchRole\",\"health\":1,\"moveSpeed\":1,\"radius\":1,\"contactDamage\":0,\"contactIntervalSeconds\":1,\"experienceDrop\":1}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, enemyJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate enemy id", errors);
            StringAssert.Contains("unknown role", errors);
            StringAssert.Contains("health above zero", errors);
            StringAssert.Contains("spawn time above zero", errors);
            StringAssert.Contains("boss definition", errors);
        }

        [Test]
        public void RunUpgradeDraftOffersThreeDeterministicChoices()
        {
            RunUpgradeCatalog catalog = BasicSurvivorsGame.CreateRunUpgradeCatalog();
            var state = new RunUpgradeState();

            RunUpgradeDraft first = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(3, 20260624));
            RunUpgradeDraft second = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(3, 20260624));

            Assert.AreEqual(3, first.Choices.Count);
            Assert.AreEqual(3, second.Choices.Count);
            for (int i = 0; i < first.Choices.Count; i++)
            {
                Assert.AreEqual(first.Choices[i].Id, second.Choices[i].Id);
            }
        }

        [Test]
        public void RadialSpawningCreatesEnemyAroundPlayer()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.Simulate(1.1f);

                Assert.That(controller.SpawnedCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.ActiveEnemyCount, Is.GreaterThanOrEqualTo(1));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void WeaponProjectileCanKillEnemyAndDropExperience()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.2f, 0f, 0f), 1f);
                controller.FireWeaponForTest();
                Step(controller, 90, 1f / 60f);

                Assert.That(controller.ProjectileLaunchCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.ExperienceCollected + controller.ActivePickupCount, Is.GreaterThanOrEqualTo(1));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ExperienceCollectionOpensLevelUpDraft()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                int requiredExperience = controller.RequiredExperienceForNextLevel;
                controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(0.15f, 0f, 0.1f), requiredExperience);
                Step(controller, 8, 1f / 30f);

                Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
                Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
                Assert.That(controller.ExperienceCollected, Is.GreaterThanOrEqualTo(requiredExperience));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void SelectingUpgradeAppliesEffectAndResumesRun()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.ForceLevelUp();
                float previousDamage = controller.ProjectileDamage;
                float previousMove = controller.PlayerMoveSpeed;
                float previousCooldown = controller.WeaponCooldownSeconds;
                float previousMaxHealth = controller.MaxHealth;
                int previousProjectilePierce = controller.ProjectilePierceBonus;
                int previousProjectileChain = controller.ProjectileChainBonus;
                int previousProjectileFork = controller.ProjectileForkBonus;
                int previousProjectileReturn = controller.ProjectileReturnBonus;
                int previousHitscanPierce = controller.HitscanPierceBonus;
                int previousPayloadCount = controller.PayloadCountBonus;
                float previousPayloadRadius = controller.PayloadExplosionRadiusBonus;
                float previousPayloadTriggerRadius = controller.PayloadTriggerRadiusBonus;
                int previousProjectileFan = controller.ProjectileFanBonus;
                float previousOrbitRadius = controller.OrbitRadiusBonus;
                int previousBurstEcho = controller.BurstEchoBonus;
                int previousTargetedBurst = controller.TargetedBurstSigilBonus;
                float previousPoison = controller.PoisonDamageRatio;
                float previousBleed = controller.BleedDamageRatio;
                float previousExecute = controller.ExecuteThresholdNormalized;
                float previousLifesteal = controller.LifestealRatio;
                float previousExperienceGain = controller.ExperienceGainMultiplierBonus;
                float previousAreaRadius = controller.AreaRadiusBonus;
                float previousBarrierCapacity = controller.BarrierCapacityBonus;
                float previousBarrierRegen = controller.BarrierRegenPerSecondBonus;
                float previousBarrierOnDamage = controller.BarrierOnDamageRatio;

                Assert.IsTrue(controller.SelectUpgrade(0));

                Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
                Assert.AreEqual(1, controller.SelectedUpgradeCount);
                bool changed = controller.ProjectileDamage > previousDamage ||
                    controller.PlayerMoveSpeed > previousMove ||
                    controller.WeaponCooldownSeconds < previousCooldown ||
                    controller.MaxHealth > previousMaxHealth ||
                    controller.PickupRangeBonus > 0f ||
                    controller.ProjectilePierceBonus > previousProjectilePierce ||
                    controller.ProjectileChainBonus > previousProjectileChain ||
                    controller.ProjectileForkBonus > previousProjectileFork ||
                    controller.ProjectileReturnBonus > previousProjectileReturn ||
                    controller.HitscanPierceBonus > previousHitscanPierce ||
                    controller.PayloadCountBonus > previousPayloadCount ||
                    controller.PayloadExplosionRadiusBonus > previousPayloadRadius ||
                    controller.PayloadTriggerRadiusBonus > previousPayloadTriggerRadius ||
                    controller.ProjectileFanBonus > previousProjectileFan ||
                    controller.OrbitRadiusBonus > previousOrbitRadius ||
                    controller.BurstEchoBonus > previousBurstEcho ||
                    controller.TargetedBurstSigilBonus > previousTargetedBurst ||
                    controller.PoisonDamageRatio > previousPoison ||
                    controller.BleedDamageRatio > previousBleed ||
                    controller.ExecuteThresholdNormalized > previousExecute ||
                    controller.LifestealRatio > previousLifesteal ||
                    controller.ExperienceGainMultiplierBonus > previousExperienceGain ||
                    controller.AreaRadiusBonus > previousAreaRadius ||
                    controller.BarrierCapacityBonus > previousBarrierCapacity ||
                    controller.BarrierRegenPerSecondBonus > previousBarrierRegen ||
                    controller.BarrierOnDamageRatio > previousBarrierOnDamage;
                Assert.IsTrue(changed);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void OrbitUpgradeAddsBladeToLocalWeaponRuntime()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            try
            {
                controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 20f);
                Step(controller, 1, 1f / 60f);
                int baseline = controller.ActiveOrbitBladeCount;

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.orbiting-focus"));
                Step(controller, 1, 1f / 60f);

                Assert.That(baseline, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.OrbitBladeBonus, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.ActiveOrbitBladeCount, Is.GreaterThan(baseline));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ProjectileModifierUpgradeAppliesLocalBonus()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                Assert.AreEqual(0, controller.ProjectilePierceBonus);

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.piercing-bolts"));

                Assert.That(controller.ProjectilePierceBonus, Is.GreaterThanOrEqualTo(1));
                Assert.AreEqual(1, controller.SelectedUpgradeCount);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void PayloadUpgradeAppliesLocalBonus()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            try
            {
                Assert.AreEqual(0f, controller.PayloadExplosionRadiusBonus);

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));

                Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(0f));
                Assert.AreEqual(3, controller.SelectedUpgradeCount);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ExpandedEnemyCatalogIncludesPressureRoles()
        {
            var profiles = BasicSurvivorsGame.CreateEnemyProfileDefinitions();

            Assert.That(profiles.Count, Is.GreaterThanOrEqualTo(9));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Runner).MoveSpeed, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm).MoveSpeed));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Bruiser).MaxHealth, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm).MaxHealth));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Spitter).RangedAttackRange, Is.GreaterThan(0f));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Splitter).MaxHealth, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm).MaxHealth));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Elite).ExperienceReward, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Bruiser).ExperienceReward));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.DreadElite).RangedAttackRange, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Elite).RangedAttackRange));
            Assert.IsTrue(BasicSurvivorsGame.CreateMetaProgressionDefinition().TryGetReward(BasicSurvivorsGame.EliteRewardId, out _));
        }

        [Test]
        public void BarrierAbsorbsDamageBeforeHealth()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                float previousHealth = controller.CurrentHealth;
                float previousBarrier = controller.BarrierValue;

                controller.ApplyDamageToPlayer(3f, "test.enemy");

                Assert.AreEqual(previousHealth, controller.CurrentHealth);
                Assert.That(controller.BarrierValue, Is.LessThan(previousBarrier));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void PoisonUpgradeAddsDamageOverTimeToWeaponHits()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.distilled-poison"));
                SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2f, 0f, 0f), 30f);

                enemy.ApplyDamage(10f, "weapon.survivors.arcane-wand");
                float afterHit = enemy.CurrentHealth;
                Step(controller, 30, 0.1f);

                Assert.That(controller.PoisonDamageRatio, Is.GreaterThan(0f));
                Assert.That(enemy.CurrentHealth, Is.LessThan(afterHit));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void MagnetRecallPullsExperienceGemsIntoPlayer()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(8f, 0f, 0f), 1);
                controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(-7f, 0f, 5f), 1);
                controller.SpawnMagnetForTest(controller.PlayerPosition + new Vector3(0.1f, 0f, 0.1f));
                Step(controller, 180, 1f / 60f);

                Assert.That(controller.MagnetRecallCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.MagnetRecallFeedbackCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.PickupAttractionFeedbackCount, Is.GreaterThanOrEqualTo(2));
                Assert.That(controller.ExperiencePickupFeedbackCount, Is.GreaterThanOrEqualTo(2));
                Assert.That(controller.ExperienceCollected, Is.GreaterThanOrEqualTo(2));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        private static SurvivorsTemplateController CreateController(bool startRun = true)
        {
            var root = new GameObject("Survivors Template EditMode Test");
            SurvivorsTemplateController controller = root.AddComponent<SurvivorsTemplateController>();
            controller.ConfigureMetaPersistenceForTest(
                new PersistenceService(new InMemoryTextStorage()),
                new SaveSlotId("edit-" + Guid.NewGuid().ToString("N")));
            if (startRun)
            {
                controller.StartRun();
            }

            return controller;
        }

        private static SurvivorsTemplateController CreateControllerWithClass(string classId)
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            Assert.IsTrue(controller.UnlockClassForTest(classId));
            Assert.IsTrue(controller.TrySelectClassForTest(classId));
            controller.StartRun();
            return controller;
        }

        private static string GetSampleRoot()
        {
            var packageInfo = PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            Assert.IsNotNull(packageInfo);
            string sampleRoot = Path.Combine(packageInfo.resolvedPath, "Samples~", "BasicSurvivorsGame");
            Assert.IsTrue(Directory.Exists(sampleRoot), "Sample root missing: " + sampleRoot);
            return sampleRoot;
        }

        private static void Step(SurvivorsTemplateController controller, int frames, float deltaTime)
        {
            for (int i = 0; i < frames; i++)
            {
                controller.Simulate(deltaTime);
            }
        }

        private static void DestroyController(SurvivorsTemplateController controller)
        {
            if (controller != null)
            {
                UnityEngine.Object.DestroyImmediate(controller.gameObject);
            }
        }

        private sealed class LegacyMetaProfileV1
        {
            public long BloodShards;
            public long LegacyExperience;
            public int HighestLevelReached;
            public float BestRunDurationSeconds;
            public int CompletedRuns;
            public int BossVictories;
            public System.Collections.Generic.List<SurvivorsPersistentUpgradeRankRecord> PersistentUpgradeRanks = new System.Collections.Generic.List<SurvivorsPersistentUpgradeRankRecord>();
        }
    }
}
