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
            Assert.AreEqual(1, classes.Classes[0].StartingWeaponIds.Count);
            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponContentId, classes.Classes[0].StartingWeaponIds[0]);
            Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, classes.Classes[1].Id);
            Assert.AreEqual(1, classes.Classes[1].StartingWeaponIds.Count);
            Assert.AreEqual(BasicSurvivorsGame.StarBeamWeaponContentId, classes.Classes[1].StartingWeaponIds[0]);
            Assert.That(progressionTracks.Count, Is.GreaterThanOrEqualTo(10));
            Assert.AreEqual(SurvivorsProgressionTrackKind.PassiveAtlas, progressionTracks[0].Kind);
            Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, progressionTracks[0].ClassId);
            Assert.That(progressionTracks[0].Nodes.Count, Is.GreaterThanOrEqualTo(4));
            Assert.That(BasicSurvivorsGame.CreateClassUpgradeGates().Count, Is.GreaterThanOrEqualTo(9));
            AssertRunUpgradeMetadataCoversPassivesAndEvolutions(upgrades, upgradeMetadata);
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.ScholarsLensUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.GiantRuneUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.TwinCharmUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.FateLensUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.SoulflareGlyphUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.AstralConvergenceUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.GemMagnetUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.LodestoneSigilUpgradeId));
            Assert.IsTrue(ContainsUpgrade(upgrades, BasicSurvivorsGame.VacuumPulseUpgradeId));
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
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.arc-bolt.weapon", BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.frost-fan.weapon", BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.blood-ring.weapon", BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.thorn-halo.weapon", BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.cinder-burst.weapon", BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.star-beam.weapon", BasicSurvivorsGame.TempestPrismEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.ember-vanguard.moon-slash.weapon", BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.gravity-grenade.unlock", BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            AssertProgressionTrackContainsNode(progressionTracks, "progression.survivors.ember-vanguard.payloads.weapon", BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId, SurvivorsProgressionNodeKind.Evolution);
            Assert.That(CountUpgradesByRarity(upgrades, RunUpgradeRarity.Epic), Is.GreaterThanOrEqualTo(2));
            Assert.IsNotNull(BasicSurvivorsGame.CreateEncounterDefinition());
        }

        [Test]
        public void SampleUiThemeLoadsDraftCardAndModePresentationTokens()
        {
            string themePath = Path.Combine(GetSampleRoot(), "Content", "DefaultUiTheme", "ui-theme.json");
            Assert.IsTrue(File.Exists(themePath), themePath);

            Assert.IsTrue(SurvivorsUiTheme.TryFromJson(File.ReadAllText(themePath), out SurvivorsUiTheme theme, out string error), error);
            Assert.AreEqual("Deucarian Survivors Default", theme.themeName);
            Assert.AreEqual("Standard / Human Playtest", theme.GetModeDisplayName(SurvivorsPacingProfile.HumanPlaytest, string.Empty));
            Assert.AreEqual("Sprint Run", theme.GetModeDisplayName(SurvivorsPacingProfile.SprintRun, string.Empty));
            Assert.AreEqual("Pickup/Magnet", theme.GetCategoryDisplayName("PickupMagnet", string.Empty));
            Assert.AreEqual("Evolution", theme.GetRarityDisplayName("Evolution"));
            Assert.AreEqual("Reroll", theme.rerollButtonLabel);
            Assert.AreEqual("Run Summary", theme.runSummaryTitle);
            Assert.AreEqual("Survivor Primer", theme.tutorialTitle);
            Assert.AreEqual("Restart Same", theme.restartSameButtonLabel);
            Assert.AreEqual("Change Mode", theme.changeModeButtonLabel);
            Assert.AreEqual("Continue", theme.continueButtonLabel);
            Assert.AreEqual("Move And Survive", theme.GetTutorialStepTitle(0, string.Empty));
            Assert.That(theme.GetTutorialStepLines(0, Array.Empty<string>()).Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(theme.GetTutorialStepLines(5, Array.Empty<string>())[0], Does.Contain("Standard / Human Playtest"));
            Assert.IsNotNull(theme.GetAudioEvent("run.victory"));
            Assert.IsNotNull(theme.GetAudioEvent("pickup.xp"));

            string neonPath = Path.Combine(GetSampleRoot(), "Content", "NeonArcanaUiTheme", "ui-theme.json");
            Assert.IsTrue(File.Exists(neonPath), neonPath);
            Assert.IsTrue(SurvivorsUiTheme.TryFromJson(File.ReadAllText(neonPath), out SurvivorsUiTheme neon, out error), error);
            Assert.AreEqual("Neon Arcana", neon.themeName);
            Assert.AreEqual("Arcane Breakthrough", neon.levelUpTitle);
            Assert.AreEqual("Collector", neon.GetCategoryDisplayName("PickupMagnet", string.Empty));
            Assert.AreEqual("Ascended", neon.GetRarityDisplayName("Evolution"));
            Assert.AreEqual("Run Chronicle", neon.runSummaryTitle);
            Assert.AreEqual("Pick A Run Mode", neon.GetTutorialStepTitle(5, string.Empty));
            Assert.IsNotNull(neon.GetAudioEvent("reward.evolution"));
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

        private static void AssertProgressionTrackContainsNode(
            IReadOnlyList<SurvivorsProgressionTrackDefinition> tracks,
            string trackId,
            string upgradeId,
            SurvivorsProgressionNodeKind kind)
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
                    if (node != null &&
                        string.Equals(node.UpgradeId, upgradeId, StringComparison.Ordinal) &&
                        node.Kind == kind)
                    {
                        return;
                    }
                }

                Assert.Fail("Track " + trackId + " is missing " + kind + " upgrade " + upgradeId);
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
                    Assert.IsTrue(
                        upgrades.TryGet(new RunUpgradeId(entry.UpgradeId), out RunUpgradeDefinition passiveDefinition),
                        "Missing passive upgrade definition for " + entry.UpgradeId);
                    Assert.That(
                        passiveDefinition.MaxRank,
                        Is.InRange(3, 5),
                        entry.DisplayName + " should use 3-5 passive ranks.");
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
            Assert.That(tuning.SummonerSupportCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.SummonerSupportInitialDelaySeconds, Is.InRange(0.5f, 2f));
            Assert.That(tuning.SummonerSupportIntervalSeconds, Is.InRange(3.5f, 6f));
            Assert.That(tuning.SummonerSupportExtraAliveAllowance, Is.GreaterThanOrEqualTo(4));
            Assert.That(tuning.SplitterChildCount, Is.InRange(2, 4));
            Assert.That(tuning.SplitterChildSpawnRadius, Is.InRange(0.6f, 1f));
            Assert.That(tuning.MajorThreatSlamIntervalSeconds, Is.InRange(4f, 7f));
            Assert.That(tuning.MajorThreatSlamTelegraphSeconds, Is.InRange(0.4f, 0.9f));
            Assert.That(tuning.MajorThreatSlamRadius, Is.InRange(2.5f, 4.5f));
            Assert.That(tuning.MajorThreatSlamDamage, Is.GreaterThan(0f));
            Assert.That(tuning.EnemySpawnPackBaseCount, Is.InRange(2, 3));
            Assert.That(tuning.EnemySpawnPackMaxCount, Is.GreaterThan(tuning.EnemySpawnPackBaseCount));
            Assert.That(tuning.EnemySpawnPackIncreaseEveryEscalations, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.EnemyRangedAttackDodgeExperienceReward, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.HordeRushClearPulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.HordeRushClearPulseRadius, Is.InRange(4f, 6f));
            Assert.That(tuning.HordeRushClearSurgeDurationSeconds, Is.InRange(3f, 7f));
            Assert.That(tuning.HordeRushClearSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.HordeRushClearSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.HordeRushClearSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.HordeRushClearSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WeaponLoadoutSurgeDurationSeconds, Is.InRange(3f, 7f));
            Assert.That(tuning.WeaponLoadoutSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WeaponLoadoutSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WeaponLoadoutSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.WeaponLoadoutSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WeaponLoadoutSurgePulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.WeaponLoadoutSurgePulseRadius, Is.InRange(4f, 7f));
            Assert.That(tuning.PassiveLoadoutSurgeDurationSeconds, Is.InRange(3f, 7f));
            Assert.That(tuning.PassiveLoadoutSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.PassiveLoadoutSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.PassiveLoadoutSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.PassiveLoadoutSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.PassiveLoadoutSurgeExperienceGainMultiplierBonus, Is.GreaterThan(0f));
            Assert.That(tuning.PassiveLoadoutSurgePulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.PassiveLoadoutSurgePulseRadius, Is.InRange(4f, 7f));
            Assert.That(swarm.MoveSpeed, Is.InRange(1.1f, 1.6f));
            Assert.That(swarm.MoveSpeed, Is.LessThan(tuning.PlayerMoveSpeed));
            Assert.That(runner.MoveSpeed, Is.LessThan(tuning.PlayerMoveSpeed));
            Assert.That(tuning.ProjectileSpeed, Is.LessThanOrEqualTo(10f));
            Assert.That(tuning.PickupAttractRange, Is.InRange(2f, 3f));
            Assert.That(tuning.MajorRewardCacheAttractionSpeedMultiplier, Is.InRange(1.75f, 3f));
            Assert.That(tuning.RoamingCacheTravelInterval, Is.InRange(14f, 22f));
            Assert.That(tuning.RoamingCacheExperienceGemCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(tuning.RoamingCacheMagnetInterval, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.RoamingCacheBloodShardInterval, Is.GreaterThan(tuning.RoamingCacheMagnetInterval));
            Assert.That(tuning.RoamingCacheAmbushStartCache, Is.GreaterThanOrEqualTo(3));
            Assert.That(tuning.RoamingCacheAmbushMaxEnemyCount, Is.GreaterThanOrEqualTo(tuning.RoamingCacheAmbushBaseEnemyCount));
            Assert.That(tuning.RoamingCacheSurgeInterval, Is.InRange(3, 6));
            Assert.That(tuning.RoamingCacheSurgeBonusGemCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.RoamingCacheSurgeDurationSeconds, Is.InRange(4f, 8f));
            Assert.That(tuning.RoamingCacheSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.RoamingCacheSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.RoamingCacheSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.RoamingCacheSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.RoamingCacheSurgePulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.RoamingCacheSurgePulseRadius, Is.InRange(3f, 6f));
            Assert.That(tuning.WaystoneDiscoveryRadius, Is.InRange(1f, 2.25f));
            Assert.That(tuning.WaystoneExperienceGemCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(tuning.WaystoneBloodShardInterval, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.WaystoneAmbushInterval, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.WaystoneAmbushBaseEnemyCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.WaystoneAmbushExtraAliveAllowance, Is.GreaterThanOrEqualTo(tuning.WaystoneAmbushBaseEnemyCount));
            Assert.That(tuning.WaystoneAmbushRadius, Is.InRange(2f, 5f));
            Assert.That(tuning.WaystoneFocusDurationSeconds, Is.InRange(3f, 8f));
            Assert.That(tuning.WaystoneFocusDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WaystoneFocusMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WaystoneFocusCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.WaystoneFocusPickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WaystoneChainInterval, Is.InRange(2, 4));
            Assert.That(tuning.WaystoneChainBonusGemCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.WaystoneChainDurationSeconds, Is.InRange(4f, 8f));
            Assert.That(tuning.WaystoneChainDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WaystoneChainMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WaystoneChainCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.WaystoneChainPickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.WaystoneChainPulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.WaystoneChainPulseRadius, Is.InRange(4f, 6f));
            Assert.That(tuning.RewardSelectionTimeoutSeconds, Is.LessThanOrEqualTo(0f));
            Assert.That(tuning.ExperienceRequiredBase, Is.InRange(6, 10));
            Assert.That(tuning.ExperienceRequiredBase * tuning.EnemySpawnIntervalSeconds, Is.InRange(5f, 10f));
            Assert.That(tuning.GemRushDurationSeconds, Is.InRange(2f, 5f));
            Assert.That(tuning.GemRushDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.GemRushMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.GemRushCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.GemRushPickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EvolutionSurgeDamage, Is.GreaterThan(0f));
            Assert.That(tuning.EvolutionSurgeRadius, Is.InRange(4f, 8f));
            Assert.That(tuning.EvolutionChainSurgeMinimumEvolutions, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.EvolutionChainSurgeDurationSeconds, Is.InRange(4f, 9f));
            Assert.That(tuning.EvolutionChainSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EvolutionChainSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EvolutionChainSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.EvolutionChainSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EvolutionChainSurgePulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.EvolutionChainSurgePulseRadius, Is.InRange(4f, 7f));
            Assert.That(tuning.LevelUpPulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.LevelUpPulseRadius, Is.InRange(3f, 5f));
            Assert.That(tuning.LowHealthClutchPulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.LowHealthClutchPulseRadius, Is.InRange(3f, 6f));
            Assert.That(tuning.LowHealthClutchSafetySeconds, Is.InRange(0.4f, 1.2f));
            Assert.That(tuning.RewardJackpotExperienceGemBaseCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(tuning.RewardJackpotExperienceGemPerRarityTier, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.RewardJackpotBloodShardBaseAmount, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.RewardJackpotLegendaryExtraBloodShardAmount, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.BossRelicSurgeDamage, Is.GreaterThan(0f));
            Assert.That(tuning.BossRelicSurgeRadius, Is.InRange(4f, 7f));
            Assert.That(tuning.BossRelicSurgeDurationSeconds, Is.InRange(3f, 7f));
            Assert.That(tuning.BossRelicSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.BossRelicSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.BossRelicSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.BossRelicSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EndlessSurgeExperienceGemCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(tuning.EndlessSurgeExperienceMultiplier, Is.GreaterThan(1f));
            Assert.That(tuning.EndlessSurgeBloodShardAmount, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.EndlessSurgeDurationSeconds, Is.InRange(5f, 8f));
            Assert.That(tuning.EndlessSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EndlessSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EndlessSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.EndlessSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(tuning.EndlessSurgePulseDamage, Is.GreaterThan(0f));
            Assert.That(tuning.EndlessSurgePulseRadius, Is.InRange(5f, 7f));
            Assert.That(tuning.ArenaShrineTravelInterval, Is.GreaterThan(30f));
            Assert.That(tuning.ArenaShrineBaseEnemyCount, Is.GreaterThanOrEqualTo(4));
            Assert.That(tuning.ArenaShrineMaxEnemyCount, Is.GreaterThanOrEqualTo(tuning.ArenaShrineBaseEnemyCount));
            Assert.That(tuning.ArenaShrineClearExperienceGemCount, Is.GreaterThanOrEqualTo(4));
            Assert.That(tuning.ArenaShrineClearExperienceMultiplier, Is.GreaterThan(1f));
            Assert.That(tuning.ArenaShrineClearBloodShardAmount, Is.GreaterThanOrEqualTo(1));
            Assert.That(tuning.ArenaShrineSurgeDurationSeconds, Is.InRange(5f, 8f));
            Assert.That(tuning.ArenaShrineSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(tuning.ArenaShrineSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(tuning.ArenaShrineSurgePulseRadius, Is.GreaterThan(4f));
            Assert.That(tuning.MajorThreatWarningLeadSeconds, Is.InRange(5f, 12f));
            Assert.That(tuning.FirstEliteSpawnTimeSeconds, Is.InRange(120f, 240f));
            Assert.That(tuning.EliteSpawnIntervalSeconds, Is.InRange(180f, 240f));
            Assert.That(tuning.FirstDreadEliteSpawnTimeSeconds, Is.InRange(240f, 360f));
            Assert.That(tuning.DreadEliteSpawnIntervalSeconds, Is.InRange(360f, 480f));
            Assert.That(tuning.MinibossSpawnTimeSeconds, Is.InRange(300f, 480f));
            Assert.That(tuning.BossSpawnTimeSeconds, Is.InRange(1140f, 1260f));
            Assert.That(tuning.SurvivalVictoryTimeSeconds, Is.GreaterThanOrEqualTo(1800f));

            Assert.IsFalse(runtime.TryConsumeTimedEliteSpawn(tuning.FirstEliteSpawnTimeSeconds - 0.1f, out _));
            Assert.IsTrue(runtime.TryConsumeTimedEliteSpawn(tuning.FirstEliteSpawnTimeSeconds, out SurvivorsEnemyRole firstEliteRole));
            Assert.AreEqual(SurvivorsEnemyRole.Elite, firstEliteRole);
            Assert.IsTrue(runtime.TryConsumeTimedEliteSpawn(tuning.FirstDreadEliteSpawnTimeSeconds, out SurvivorsEnemyRole firstDreadEliteRole));
            Assert.AreEqual(SurvivorsEnemyRole.DreadElite, firstDreadEliteRole);
            Assert.IsFalse(runtime.TryConsumeTimedEliteSpawn(tuning.FirstDreadEliteSpawnTimeSeconds + 1f, out _));
            Assert.IsTrue(runtime.TryConsumeTimedEliteSpawn(tuning.FirstEliteSpawnTimeSeconds + tuning.EliteSpawnIntervalSeconds, out SurvivorsEnemyRole recurringEliteRole));
            Assert.AreEqual(SurvivorsEnemyRole.Elite, recurringEliteRole);
            Assert.IsFalse(runtime.TryConsumeTimedEliteSpawn(tuning.FirstEliteSpawnTimeSeconds + tuning.EliteSpawnIntervalSeconds + 1f, out _));
            Assert.IsTrue(runtime.TryConsumeTimedEliteSpawn(tuning.FirstEliteSpawnTimeSeconds + tuning.EliteSpawnIntervalSeconds * 2f, out SurvivorsEnemyRole secondRecurringEliteRole));
            Assert.AreEqual(SurvivorsEnemyRole.Elite, secondRecurringEliteRole);
            Assert.IsTrue(runtime.TryConsumeTimedEliteSpawn(tuning.FirstDreadEliteSpawnTimeSeconds + tuning.DreadEliteSpawnIntervalSeconds, out SurvivorsEnemyRole recurringDreadEliteRole));
            Assert.AreEqual(SurvivorsEnemyRole.DreadElite, recurringDreadEliteRole);

            runtime.Tick(30f);
            Assert.AreEqual(SurvivorsRunPhase.Opening, runtime.Phase);
            Assert.AreEqual(SurvivorsEnemyRole.Swarm, runtime.ResolveNextSwarmRole(30f, 0L));

            runtime.Tick(60f);
            Assert.That(runtime.ResolveMaximumAlive(tuning.EnemyMaximumAlive), Is.InRange(36, 48));

            runtime.Tick(180f);
            bool sawSummoner = false;
            for (long sequence = 0L; sequence < 80L; sequence++)
            {
                if (runtime.ResolveNextSwarmRole(180f, sequence) == SurvivorsEnemyRole.Summoner)
                {
                    sawSummoner = true;
                    break;
                }
            }

            Assert.IsTrue(sawSummoner);

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
            Assert.That(debugFast.EnemyRangedAttackDodgeExperienceReward, Is.GreaterThanOrEqualTo(human.EnemyRangedAttackDodgeExperienceReward));
            Assert.That(debugFast.ExperienceRequiredBase, Is.LessThan(human.ExperienceRequiredBase));
            Assert.That(debugFast.RoamingCacheTravelInterval, Is.LessThan(human.RoamingCacheTravelInterval));
            Assert.That(debugFast.MajorRewardCacheAttractionSpeedMultiplier, Is.GreaterThan(human.MajorRewardCacheAttractionSpeedMultiplier));
            Assert.That(debugFast.RoamingCacheAmbushStartCache, Is.LessThan(human.RoamingCacheAmbushStartCache));
            Assert.That(showcase.RoamingCacheTravelInterval, Is.LessThan(human.RoamingCacheTravelInterval));
            Assert.That(debugFast.SplitterChildCount, Is.GreaterThanOrEqualTo(human.SplitterChildCount));
            Assert.That(showcase.SplitterChildCount, Is.GreaterThanOrEqualTo(human.SplitterChildCount));
            Assert.That(debugFast.FirstEliteSpawnTimeSeconds, Is.LessThan(human.FirstEliteSpawnTimeSeconds));
            Assert.That(debugFast.EliteSpawnIntervalSeconds, Is.LessThan(human.EliteSpawnIntervalSeconds));
            Assert.That(debugFast.MinibossSpawnTimeSeconds, Is.LessThan(human.MinibossSpawnTimeSeconds));
            Assert.That(normal.FirstDreadEliteSpawnTimeSeconds, Is.LessThan(normal.MinibossSpawnTimeSeconds));
            Assert.That(normal.DreadEliteSpawnIntervalSeconds, Is.LessThan(human.DreadEliteSpawnIntervalSeconds));
            Assert.That(debugFast.FirstDreadEliteSpawnTimeSeconds, Is.LessThan(debugFast.MinibossSpawnTimeSeconds));
            Assert.That(debugFast.DreadEliteSpawnIntervalSeconds, Is.LessThan(human.DreadEliteSpawnIntervalSeconds));
            Assert.That(showcase.FirstDreadEliteSpawnTimeSeconds, Is.LessThan(showcase.MinibossSpawnTimeSeconds));
            Assert.That(showcase.EliteSpawnIntervalSeconds, Is.LessThan(human.EliteSpawnIntervalSeconds));
            Assert.That(debugFast.MajorThreatSlamIntervalSeconds, Is.LessThan(human.MajorThreatSlamIntervalSeconds));
            Assert.That(showcase.MajorThreatSlamIntervalSeconds, Is.LessThan(human.MajorThreatSlamIntervalSeconds));
            Assert.That(debugFast.RewardSelectionTimeoutSeconds, Is.GreaterThan(human.RewardSelectionTimeoutSeconds));
            Assert.That(showcase.EnemySpawnIntervalSeconds, Is.GreaterThan(debugFast.EnemySpawnIntervalSeconds));
            Assert.That(showcase.EnemySpawnIntervalSeconds, Is.LessThan(human.EnemySpawnIntervalSeconds));
        }

        [Test]
        public void SprintRunProfileIsSeparateFiveMinuteMode()
        {
            SurvivorsTemplateTuning standard = BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.HumanPlaytest);
            SurvivorsTemplateTuning sprint = BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.SprintRun);

            Assert.AreEqual(SurvivorsPacingProfile.HumanPlaytest, standard.PacingProfile);
            Assert.AreEqual(SurvivorsPacingProfile.SprintRun, sprint.PacingProfile);
            Assert.AreEqual("Standard Run", standard.RunModeDisplayName);
            Assert.AreEqual("Sprint Run", sprint.RunModeDisplayName);
            Assert.AreEqual(1800f, standard.SurvivalVictoryTimeSeconds);
            Assert.AreEqual(1800f, standard.TargetDurationSeconds);
            Assert.IsTrue(standard.EndlessContinuationEnabled);
            Assert.AreEqual(1f, standard.RunRewardMultiplier);
            Assert.AreEqual(0, standard.EvolutionRequiredRankReduction);
            Assert.AreEqual(300f, sprint.SurvivalVictoryTimeSeconds);
            Assert.AreEqual(300f, sprint.TargetDurationSeconds);
            Assert.IsFalse(sprint.EndlessContinuationEnabled);
            Assert.That(sprint.RunRewardMultiplier, Is.InRange(0.5f, 0.8f));
            Assert.AreEqual(2, sprint.EvolutionRequiredRankReduction);
            Assert.That(sprint.FirstEliteSpawnTimeSeconds, Is.InRange(75f, 90f));
            Assert.That(sprint.HordeRushFirstTimeSeconds, Is.EqualTo(120f).Within(0.01f));
            Assert.That(sprint.MinibossSpawnTimeSeconds, Is.InRange(150f, 180f));
            Assert.That(sprint.BossSpawnTimeSeconds, Is.InRange(255f, 285f));
            Assert.That(sprint.BossSpawnTimeSeconds, Is.LessThan(standard.BossSpawnTimeSeconds));
            Assert.That(sprint.ExperienceRequiredBase, Is.GreaterThan(standard.ExperienceRequiredBase));
            Assert.That(sprint.ExperienceRequiredBase, Is.GreaterThanOrEqualTo(20));
            Assert.That(sprint.ExperienceRequiredPerLevel, Is.GreaterThan(standard.ExperienceRequiredPerLevel));
            Assert.That(sprint.ExperienceRequiredPerLevel, Is.GreaterThanOrEqualTo(14));
            Assert.That(sprint.LevelUpDraftCooldownSeconds, Is.GreaterThanOrEqualTo(4f));
            Assert.That(sprint.MaximumQueuedLevelUps, Is.LessThanOrEqualTo(3));
            Assert.That(sprint.DraftMidRarityLevel, Is.LessThan(standard.DraftMidRarityLevel));
            Assert.That(sprint.NormalMidRareWeight, Is.GreaterThan(standard.NormalMidRareWeight));
            Assert.That(sprint.DraftRerollCharges, Is.GreaterThan(standard.DraftRerollCharges));
            Assert.That(sprint.DraftBanishCharges, Is.GreaterThan(standard.DraftBanishCharges));
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
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            string uiThemeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUiTheme", "ui-theme.json"));
            string alternateUiThemeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "NeonArcanaUiTheme", "ui-theme.json"));

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, enemyJson, rewardJson, relicJson, classJson, progressionJson, pickupJson, runFlowJson, uiThemeJson, alternateUiThemeJson);

            Assert.IsTrue(result.Succeeded, string.Join(Environment.NewLine, result.Errors));
        }

        [Test]
        public void SampleAuthoredContentBuildsRuntimeRunFlowAndRewards()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));

            Assert.IsTrue(
                SurvivorsAuthoredContentDefinition.TryCreate(
                    weaponJson,
                    upgradeJson,
                    relicJson,
                    classJson,
                    progressionJson,
                    enemyJson,
                    runFlowJson,
                    rewardJson,
                    out SurvivorsAuthoredContentDefinition authored,
                    out string error),
                error);

            SurvivorsTemplateTuning sprint = authored.CreateTuning(SurvivorsPacingProfile.SprintRun);
            SurvivorsRunFlowDefinition flow = authored.CreateRunFlowDefinition(sprint);

            Assert.That(authored.WeaponDefinitions.Count, Is.GreaterThanOrEqualTo(10));
            Assert.That(authored.RunUpgradeCatalog.Definitions.Count, Is.GreaterThanOrEqualTo(60));
            Assert.That(authored.RunUpgradeMetadata.Count, Is.EqualTo(authored.RunUpgradeCatalog.Definitions.Count));
            Assert.That(authored.RelicDefinitions.Count, Is.GreaterThanOrEqualTo(6));
            Assert.That(authored.ClassLibrary.Classes.Count, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(1, authored.ClassLibrary.Classes[0].StartingWeaponIds.Count);
            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponContentId, authored.ClassLibrary.Classes[0].StartingWeaponIds[0]);
            Assert.AreEqual(1, authored.ClassLibrary.Classes[1].StartingWeaponIds.Count);
            Assert.AreEqual(BasicSurvivorsGame.StarBeamWeaponContentId, authored.ClassLibrary.Classes[1].StartingWeaponIds[0]);
            Assert.That(authored.ProgressionTracks.Count, Is.GreaterThanOrEqualTo(8));
            Assert.AreEqual("Sprint Run", sprint.RunModeDisplayName);
            Assert.AreEqual("5 min", sprint.RunModeDurationLabel);
            Assert.That(sprint.RunModeDescription, Does.Contain("quicker XP"));
            Assert.AreEqual(300f, sprint.SurvivalVictoryTimeSeconds);
            Assert.AreEqual(102, sprint.ExperienceRequiredBase);
            Assert.AreEqual(20, sprint.ExperienceRequiredPerLevel);
            Assert.AreEqual(75, sprint.NormalEarlyCommonWeight);
            Assert.AreEqual(120, sprint.BossLegendaryWeight);
            Assert.AreEqual(BasicSurvivorsGame.MinibossEnemySpawnableId.Value, flow.Miniboss.Id);
            Assert.AreEqual(BasicSurvivorsGame.BossEnemySpawnableId.Value, flow.Boss.Id);
            Assert.IsTrue(flow.Miniboss.ShowOverheadLifeBar);
            Assert.IsFalse(flow.Miniboss.ShowBossLifeBar);
            Assert.IsTrue(flow.Boss.ShowBossLifeBar);
            Assert.IsFalse(flow.Boss.ShowOverheadLifeBar);
            Assert.AreEqual(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Boss, sprint).MaxHealth, flow.Boss.MaxHealth, 0.001f);
            Assert.IsTrue(authored.MetaProgressionDefinition.TryGetReward(BasicSurvivorsGame.EliteRewardId, out SurvivorsRewardDefinition eliteReward));
            Assert.AreEqual(2, eliteReward.CurrencyAmount);
        }

        [Test]
        public void StrictAuthoredWeaponMutationsDriveRuntimeValuesWithoutFallbacks()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"))
                .Replace("\"cooldownSeconds\": 0.52", "\"cooldownSeconds\": 0.91")
                .Replace("\"damage\": 7", "\"damage\": 9.25")
                .Replace("\"speed\": 11.5", "\"speed\": 13.75")
                .Replace("\"lifetimeSeconds\": 2.4", "\"lifetimeSeconds\": 3.1");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, weaponJson: weaponJson),
                error);
            SurvivorsWeaponArchetypeDefinition arcane = FindWeapon(authored.WeaponDefinitions, BasicSurvivorsGame.ArcaneWandWeaponContentId);

            Assert.IsTrue(authored.IsStrictSample);
            Assert.IsFalse(authored.UsesBuiltInFallbacks);
            Assert.AreEqual(0.91f, arcane.CooldownSeconds, 0.001f);
            Assert.AreEqual(9.25f, arcane.Damage, 0.001f);
            Assert.AreEqual(13.75f, arcane.ProjectileSpeed, 0.001f);
            Assert.AreEqual(3.1f, arcane.ProjectileLifetimeSeconds, 0.001f);
            Assert.AreEqual(0.22f, arcane.ProjectileRadius, 0.001f);
        }

        [Test]
        public void StrictAuthoredBindingRejectsMissingRequiredWeaponFields()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            weaponJson = ReplaceFirst(weaponJson, "\"damage\": 7", "\"omittedDamage\": 7");
            weaponJson = ReplaceFirst(weaponJson, "\"projectileRadius\": 0.22", "\"omittedProjectileRadius\": 0.22");

            bool created = TryCreateStrictSampleContent(out _, out string error, weaponJson: weaponJson);

            Assert.IsFalse(created);
            StringAssert.Contains("requires damage above zero", error);
            StringAssert.Contains("requires projectile radius above zero", error);
        }

        [Test]
        public void StrictAuthoredUpgradeWeightRankAndAmountDriveRuntimeCatalog()
        {
            string sampleRoot = GetSampleRoot();
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            upgradeJson = ReplaceFirst(upgradeJson, "\"weight\": 70", "\"weight\": 91");
            upgradeJson = ReplaceFirst(upgradeJson, "\"maxRank\": 8", "\"maxRank\": 9");
            upgradeJson = ReplaceFirst(upgradeJson, "\"amount\": 2.0", "\"amount\": 3.25");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, upgradeJson: upgradeJson),
                error);
            Assert.IsTrue(authored.RunUpgradeCatalog.TryGet(new RunUpgradeId("upgrade.survivors.arcane-damage"), out RunUpgradeDefinition upgrade));

            Assert.AreEqual(91, upgrade.Weight);
            Assert.AreEqual(9, upgrade.MaxRank);
            Assert.AreEqual(3.25d, upgrade.Effects[0].Amount, 0.001d);
        }

        [Test]
        public void StrictAuthoredBindingRejectsMissingRequiredUpgradeFields()
        {
            string sampleRoot = GetSampleRoot();
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            upgradeJson = ReplaceFirst(upgradeJson, "\"maxRank\": 8", "\"omittedMaxRank\": 8");
            upgradeJson = ReplaceFirst(upgradeJson, "\"amount\": 2.0", "\"omittedAmount\": 2.0");

            bool created = TryCreateStrictSampleContent(out _, out string error, upgradeJson: upgradeJson);

            Assert.IsFalse(created);
            StringAssert.Contains("requires max rank above zero", error);
            StringAssert.Contains("missing a non-zero amount", error);
        }

        [Test]
        public void StrictAuthoredEnemyMutationsDriveStatsLifecycleAndSpawnGate()
        {
            string sampleRoot = GetSampleRoot();
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"))
                .Replace("\"health\": 12.5", "\"health\": 23.5")
                .Replace("\"moveSpeed\": 1.35", "\"moveSpeed\": 1.85")
                .Replace("\"contactDamage\": 3.5", "\"contactDamage\": 5.25")
                .Replace("\"spawnTimeSeconds\": 20", "\"spawnTimeSeconds\": 42");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, enemyJson: enemyJson),
                error);
            Assert.IsTrue(authored.TryGetEnemyProfile(SurvivorsEnemyRole.Swarm, out SurvivorsEnemyProfile swarm));
            Assert.IsTrue(authored.TryGetEnemyProfile(SurvivorsEnemyRole.Runner, out SurvivorsEnemyProfile runner));

            Assert.AreEqual(23.5f, swarm.MaxHealth, 0.001f);
            Assert.AreEqual(1.85f, swarm.MoveSpeed, 0.001f);
            Assert.AreEqual(5.25f, swarm.ContactDamage, 0.001f);
            Assert.IsTrue(swarm.CanRecycle);
            Assert.IsTrue(swarm.CanLeash);
            Assert.IsTrue(swarm.CanReposition);
            Assert.IsFalse(swarm.ShowOffscreenMarker);
            Assert.AreEqual(42f, runner.SpawnTimeSeconds, 0.001f);
        }

        [Test]
        public void StrictAuthoredBindingRejectsMissingRequiredEnemyFields()
        {
            string sampleRoot = GetSampleRoot();
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            enemyJson = ReplaceFirst(enemyJson, "\"health\": 12.5", "\"omittedHealth\": 12.5");
            enemyJson = ReplaceFirst(enemyJson, "\"tint\": \"#E03852\"", "\"omittedTint\": \"#E03852\"");

            bool created = TryCreateStrictSampleContent(out _, out string error, enemyJson: enemyJson);

            Assert.IsFalse(created);
            StringAssert.Contains("requires health above zero", error);
            StringAssert.Contains("requires an authored HTML color value", error);
        }

        [Test]
        public void StrictAuthoredRunFlowMutationsRemainSeparateForStandardAndSprint()
        {
            string sampleRoot = GetSampleRoot();
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            runFlowJson = ReplaceFirst(runFlowJson, "\"survivalVictoryTimeSeconds\": 1800", "\"survivalVictoryTimeSeconds\": 1777");
            runFlowJson = ReplaceFirst(runFlowJson, "\"survivalVictoryTimeSeconds\": 300", "\"survivalVictoryTimeSeconds\": 333");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, runFlowJson: runFlowJson),
                error);

            Assert.AreEqual(1777f, authored.CreateTuning(SurvivorsPacingProfile.HumanPlaytest).SurvivalVictoryTimeSeconds, 0.001f);
            Assert.AreEqual(333f, authored.CreateTuning(SurvivorsPacingProfile.SprintRun).SurvivalVictoryTimeSeconds, 0.001f);
        }

        [Test]
        public void StrictAuthoredSharedGameplayAndProfileOverridesDriveRuntimeTuning()
        {
            string sampleRoot = GetSampleRoot();
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            runFlowJson = ReplaceFirst(runFlowJson, "\"playerMoveSpeed\": 5.55", "\"playerMoveSpeed\": 6.15");
            runFlowJson = ReplaceFirst(runFlowJson, "\"dashDistance\": 3.35", "\"dashDistance\": 4.1");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, runFlowJson: runFlowJson),
                error);

            Assert.AreEqual(6.15f, authored.CreateTuning(SurvivorsPacingProfile.HumanPlaytest).PlayerMoveSpeed, 0.001f);
            Assert.AreEqual(4.1f, authored.CreateTuning(SurvivorsPacingProfile.SprintRun).DashDistance, 0.001f);
            Assert.AreEqual(3.15f, authored.CreateTuning(SurvivorsPacingProfile.HumanPlaytest).DashDistance, 0.001f);
        }

        [Test]
        public void StrictAuthoredGameplayTuningPreservesEveryPreviousEffectiveProfileValue()
        {
            Assert.IsTrue(TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error), error);
            var profiles = new[]
            {
                SurvivorsPacingProfile.HumanPlaytest,
                SurvivorsPacingProfile.SprintRun,
                SurvivorsPacingProfile.Normal,
                SurvivorsPacingProfile.DebugFast,
                SurvivorsPacingProfile.Showcase
            };
            IReadOnlyList<string> authoredFieldNames = SurvivorsAuthoredContentDefinition.AuthoredGameplayTuningFieldNames;

            for (int profileIndex = 0; profileIndex < profiles.Length; profileIndex++)
            {
                SurvivorsPacingProfile profile = profiles[profileIndex];
                SurvivorsTemplateTuning previousEffective = BasicSurvivorsGame.CreateTuning(profile);
                SurvivorsTemplateTuning strictAuthored = authored.CreateTuning(profile);
                for (int fieldIndex = 0; fieldIndex < authoredFieldNames.Count; fieldIndex++)
                {
                    string authoredFieldName = authoredFieldNames[fieldIndex];
                    string tuningFieldName = char.ToUpperInvariant(authoredFieldName[0]) + authoredFieldName.Substring(1);
                    System.Reflection.FieldInfo tuningField = typeof(SurvivorsTemplateTuning).GetField(tuningFieldName);
                    Assert.IsNotNull(tuningField, $"Missing runtime tuning field for authored value {authoredFieldName}.");

                    if (tuningField.FieldType == typeof(float))
                    {
                        Assert.AreEqual(
                            (float)tuningField.GetValue(previousEffective),
                            (float)tuningField.GetValue(strictAuthored),
                            0.0001f,
                            $"{profile}.{tuningFieldName}");
                    }
                    else
                    {
                        Assert.AreEqual(
                            tuningField.GetValue(previousEffective),
                            tuningField.GetValue(strictAuthored),
                            $"{profile}.{tuningFieldName}");
                    }
                }
            }
        }

        [Test]
        public void StrictAuthoredBindingRejectsMissingRunFlowFields()
        {
            string sampleRoot = GetSampleRoot();
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            runFlowJson = ReplaceFirst(
                runFlowJson,
                "\"playerMoveSpeed\": 5.55",
                "\"omittedPlayerMoveSpeed\": 5.55");
            runFlowJson = ReplaceFirst(
                runFlowJson,
                "\"enemySpawnIntervalSeconds\": 0.95",
                "\"omittedEnemySpawnIntervalSeconds\": 0.95");

            bool created = TryCreateStrictSampleContent(out _, out string error, runFlowJson: runFlowJson);

            Assert.IsFalse(created);
            StringAssert.Contains("requires playerMoveSpeed above zero", error);
            StringAssert.Contains("enemy spawn interval above zero", error);
        }

        [Test]
        public void StrictAuthoredRewardMutationDrivesRuntimeGrant()
        {
            string sampleRoot = GetSampleRoot();
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            rewardJson = ReplaceFirst(rewardJson, "\"currencyAmount\": 2", "\"currencyAmount\": 7");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, rewardJson: rewardJson),
                error);
            Assert.IsTrue(authored.MetaProgressionDefinition.TryGetReward(BasicSurvivorsGame.EliteRewardId, out SurvivorsRewardDefinition reward));
            Assert.AreEqual(7, reward.CurrencyAmount);
        }

        [Test]
        public void StrictAuthoredBindingRejectsBrokenRewardReferences()
        {
            string sampleRoot = GetSampleRoot();
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            rewardJson = ReplaceFirst(
                rewardJson,
                "\"currencyId\": \"currency.survivors.blood-shards\"",
                "\"currencyId\": \"currency.survivors.missing\"");

            bool created = TryCreateStrictSampleContent(out _, out string error, rewardJson: rewardJson);

            Assert.IsFalse(created);
            StringAssert.Contains("references unknown currency", error);
        }

        [Test]
        public void ProgressionMutationChangesLiveClassGateOwnership()
        {
            string sampleRoot = GetSampleRoot();
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"))
                .Replace("upgrade.survivors.arcane-thesis", "upgrade.survivors.swap-placeholder")
                .Replace("upgrade.survivors.ember-forge-heart", "upgrade.survivors.arcane-thesis")
                .Replace("upgrade.survivors.swap-placeholder", "upgrade.survivors.ember-forge-heart");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, progressionJson: progressionJson),
                error);
            SurvivorsClassUpgradeGateDefinition gate = FindClassGate(authored.ClassUpgradeGates, BasicSurvivorsGame.ArcaneThesisUpgradeId);

            Assert.That(gate.AllowedClassIds, Does.Contain(BasicSurvivorsGame.EmberVanguardClassId));
            Assert.That(gate.AllowedClassIds, Does.Not.Contain(BasicSurvivorsGame.DefaultClassId));

            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    controller.ConfigureAuthoredContentJson(
                        File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json")),
                        File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json")),
                        File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json")),
                        File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json")),
                        progressionJson,
                        File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json")),
                        File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json")),
                        File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"))),
                    controller.AuthoredContentStatus);
                controller.StartRun();

                Assert.IsFalse(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ProgressionMutationChangesBoundWeaponEvolutionTrackMetadata()
        {
            string sampleRoot = GetSampleRoot();
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            progressionJson = ReplaceFirst(
                progressionJson,
                "\"id\": \"node.survivors.arc-bolt.evolution\", \"displayName\": \"Arcane Storm\", \"upgradeId\": \"upgrade.survivors.evolution.arcane-storm\", \"kind\": \"Evolution\", \"tier\": 5, \"pointCost\": 3, \"maxRank\": 1",
                "\"id\": \"node.survivors.arc-bolt.evolution\", \"displayName\": \"Arcane Storm\", \"upgradeId\": \"upgrade.survivors.evolution.arcane-storm\", \"kind\": \"Evolution\", \"tier\": 5, \"pointCost\": 7, \"maxRank\": 1");

            Assert.IsTrue(
                TryCreateStrictSampleContent(out SurvivorsAuthoredContentDefinition authored, out string error, progressionJson: progressionJson),
                error);
            SurvivorsProgressionTrackDefinition track = FindProgressionTrack(
                authored.ProgressionTracks,
                "progression.survivors.arc-bolt.weapon");
            SurvivorsProgressionNodeDefinition node = FindProgressionNode(
                track,
                "node.survivors.arc-bolt.evolution");

            Assert.AreEqual(SurvivorsProgressionTrackKind.WeaponSkillTrack, track.Kind);
            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponContentId, track.TargetWeaponId);
            Assert.AreEqual(SurvivorsProgressionNodeKind.Evolution, node.Kind);
            Assert.AreEqual(7, node.PointCost);
        }

        [Test]
        public void StrictBindingFailureCannotStartFallbackBalancedRun()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            weaponJson = ReplaceFirst(weaponJson, "\"damage\": 7", "\"omittedDamage\": 7");
            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                bool configured = controller.ConfigureAuthoredContentJson(
                    weaponJson,
                    File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json")),
                    File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json")),
                    File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json")),
                    File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json")),
                    File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json")),
                    File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json")),
                    File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json")),
                    SurvivorsAuthoredContentBindingPolicy.StrictSample);

                Assert.IsFalse(configured);
                Assert.IsTrue(controller.IsStrictAuthoredSample);
                Assert.IsFalse(controller.IsAuthoredContentBound);
                Assert.IsFalse(controller.IsFallbackContentActive);
                Assert.IsFalse(controller.CanStartConfiguredRun);
                Assert.IsFalse(controller.SelectSprintRun());
                Assert.IsFalse(controller.IsRunStarted);
                Assert.AreEqual(SurvivorsRunState.Booting, controller.State);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void UnboundHostUsesIdentifiableFallbackPolicy()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                Assert.IsFalse(controller.IsStrictAuthoredSample);
                Assert.IsTrue(controller.IsFallbackContentActive);
                Assert.IsTrue(controller.CanStartConfiguredRun);
                StringAssert.Contains("Fallback content active", controller.AuthoredContentStatus);

                controller.StartRun();

                Assert.IsTrue(controller.IsRunStarted);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void InvalidAuthoredRuntimeBindingReportsMissingRewardReferences()
        {
            string sampleRoot = GetSampleRoot();
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"))
                .Replace(BasicSurvivorsGame.BossRewardId, "reward.survivors.missing-final-boss");
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));

            bool created = SurvivorsAuthoredContentDefinition.TryCreate(enemyJson, runFlowJson, rewardJson, out _, out string error);

            Assert.IsFalse(created);
            StringAssert.Contains(BasicSurvivorsGame.BossRewardId, error);
        }

        [Test]
        public void ControllerUsesAuthoredContentForRunFlowRewardsAndThreatLifeBars()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    controller.ConfigureAuthoredContentJson(
                        weaponJson,
                        upgradeJson,
                        relicJson,
                        classJson,
                        progressionJson,
                        enemyJson,
                        runFlowJson,
                        rewardJson),
                    controller.AuthoredContentStatus);
                controller.ApplyPacingProfileForTest(SurvivorsPacingProfile.SprintRun);
                controller.StartRun();

                Assert.IsTrue(controller.IsAuthoredContentBound);
                Assert.IsTrue(controller.IsUsingAuthoredRunFlow);
                Assert.That(controller.AuthoredContentStatus, Does.Contain("10 weapons"));
                Assert.That(controller.AuthoredContentStatus, Does.Contain("10 enemies"));
                Assert.AreEqual(1, controller.ActiveWeaponCount);
                Assert.That(controller.ActiveWeaponIds, Does.Contain(BasicSurvivorsGame.ArcaneWandWeaponContentId));
                Assert.That(controller.CurrentBuildHudLinesForTest()[0], Does.Contain("Weapons 1/"));
                Assert.AreEqual(BasicSurvivorsGame.BossEnemySpawnableId.Value, controller.CurrentRunFlowDefinition.Boss.Id);
                Assert.IsTrue(controller.CurrentRunFlowDefinition.Boss.ShowBossLifeBar);
                Assert.IsTrue(controller.CurrentRunFlowDefinition.Miniboss.ShowOverheadLifeBar);

                SurvivorsEnemyActor elite = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), SurvivorsEnemyRole.Elite, 80f);
                SurvivorsEnemyActor miniboss = controller.SpawnMinibossForTest(controller.PlayerPosition + new Vector3(4f, 0f, 0f), 120f);
                SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(5f, 0f, 0f), 180f);

                Assert.NotNull(elite);
                Assert.NotNull(miniboss);
                Assert.NotNull(boss);
                Assert.AreEqual(2, controller.ActiveOverheadLifeBarCount);
                Assert.AreEqual(1, controller.ActiveBossLifeBarCount);
                Assert.AreEqual(3, controller.ActiveMajorThreatLifeBarCount);
                Assert.That(controller.ActiveMajorThreatLifeBarSummary, Does.Contain("Blood Warden Elite"));
                Assert.That(controller.ActiveMajorThreatLifeBarSummary, Does.Contain("Bloodbound Miniboss"));
                Assert.That(controller.ActiveMajorThreatLifeBarSummary, Does.Contain("Eclipse Boss"));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ControllerUsesAuthoredEnemyStatsWhenSampleContentChanges()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"))
                .Replace("\"displayName\": \"Eclipse Boss\"", "\"displayName\": \"Authored Eclipse Boss\"")
                .Replace("\"health\": 1400", "\"health\": 2100");
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    controller.ConfigureAuthoredContentJson(
                        weaponJson,
                        upgradeJson,
                        relicJson,
                        classJson,
                        progressionJson,
                        enemyJson,
                        runFlowJson,
                        rewardJson),
                    controller.AuthoredContentStatus);
                controller.ApplyPacingProfileForTest(SurvivorsPacingProfile.SprintRun);
                controller.StartRun();

                SurvivorsEnemyProfile boss = controller.CurrentRunFlowDefinition.Boss;
                float defaultSprintBossHealth = BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Boss, controller.CurrentTuning).MaxHealth;
                Assert.AreEqual("Authored Eclipse Boss", boss.DisplayName);
                Assert.AreEqual(defaultSprintBossHealth * 1.5f, boss.MaxHealth, 0.001f);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ControllerUsesAuthoredClassWeaponLoadoutWhenSampleContentChanges()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"))
                .Replace("\"weapon.survivors.arcane-wand\"", "\"weapon.survivors.star-beam\"");
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    controller.ConfigureAuthoredContentJson(
                        weaponJson,
                        upgradeJson,
                        relicJson,
                        classJson,
                        progressionJson,
                        enemyJson,
                        runFlowJson,
                        rewardJson),
                    controller.AuthoredContentStatus);

                controller.StartRun();

                Assert.AreEqual(1, controller.ActiveWeaponCount);
                Assert.That(controller.ActiveWeaponIds, Does.Contain(BasicSurvivorsGame.StarBeamWeaponContentId));
                Assert.That(controller.ActiveWeaponIds, Does.Not.Contain(BasicSurvivorsGame.ArcaneWandWeaponContentId));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ControllerUsesAuthoredUpgradeAmountsWhenSampleContentChanges()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = ReplaceFirst(
                File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json")),
                "\"amount\": 2.0,",
                "\"amount\": 9.5,");
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    controller.ConfigureAuthoredContentJson(
                        weaponJson,
                        upgradeJson,
                        relicJson,
                        classJson,
                        progressionJson,
                        enemyJson,
                        runFlowJson,
                        rewardJson),
                    controller.AuthoredContentStatus);

                controller.StartRun();
                float previousDamage = controller.DamageBonus;

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.arcane-damage"));
                Assert.AreEqual(previousDamage + 9.5f, controller.DamageBonus, 0.001f);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void AuthoredProgressionTracksDriveClassSpecificUpgradeGates()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            upgradeJson = upgradeJson.Replace(
                ",\n      \"allowedClasses\": [\n        \"class.survivors.ember-vanguard\"\n      ]",
                string.Empty);
            upgradeJson = upgradeJson.Replace(
                ",\r\n      \"allowedClasses\": [\r\n        \"class.survivors.ember-vanguard\"\r\n      ]",
                string.Empty);
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"));
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));

            SurvivorsTemplateController defaultController = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    defaultController.ConfigureAuthoredContentJson(
                        weaponJson,
                        upgradeJson,
                        relicJson,
                        classJson,
                        progressionJson,
                        enemyJson,
                        runFlowJson,
                        rewardJson),
                    defaultController.AuthoredContentStatus);
                defaultController.StartRun();

                Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId));
            }
            finally
            {
                DestroyController(defaultController);
            }

            SurvivorsTemplateController emberController = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    emberController.ConfigureAuthoredContentJson(
                        weaponJson,
                        upgradeJson,
                        relicJson,
                        classJson,
                        progressionJson,
                        enemyJson,
                        runFlowJson,
                        rewardJson),
                    emberController.AuthoredContentStatus);
                Assert.IsTrue(emberController.UnlockClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
                Assert.IsTrue(emberController.TrySelectClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
                emberController.StartRun();

                Assert.IsTrue(emberController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId));
            }
            finally
            {
                DestroyController(emberController);
            }
        }

        [Test]
        public void ControllerUsesAuthoredMetaUpgradeOptionsWhenRewardContentChanges()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string relicJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            string rewardJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json"))
                .Replace("\"displayName\": \"Arcane Legacy\"", "\"displayName\": \"Authored Arcane Legacy\"")
                .Replace("\"rankCosts\": [5, 8, 13]", "\"rankCosts\": [3, 8, 13]")
                .Replace("\"amountPerRank\": 1.5", "\"amountPerRank\": 2.5");
            SurvivorsTemplateController controller = CreateController(startRun: false);
            try
            {
                Assert.IsTrue(
                    controller.ConfigureAuthoredContentJson(
                        weaponJson,
                        upgradeJson,
                        relicJson,
                        classJson,
                        progressionJson,
                        enemyJson,
                        runFlowJson,
                        rewardJson),
                    controller.AuthoredContentStatus);

                Assert.IsTrue(controller.DebugGrantBloodShards(3));
                IReadOnlyList<string> options = controller.GetResultMetaUpgradeOptionLabelsForTest();

                Assert.That(options.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(options[0], Does.Contain("Authored Arcane Legacy"));
                Assert.That(options[0], Does.Contain("3 shards"));
                Assert.That(options[0], Does.Contain("+2.5 starting damage"));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void SampleEnemyContentDefinesThreatLifecycleAuthoring()
        {
            string sampleRoot = GetSampleRoot();
            string enemyJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json"));
            EnemyLibraryForTest library = JsonUtility.FromJson<EnemyLibraryForTest>(enemyJson);

            Assert.NotNull(library);
            Assert.NotNull(library.enemies);
            Assert.That(library.enemies.Length, Is.GreaterThanOrEqualTo(10));
            for (int i = 0; i < library.enemies.Length; i++)
            {
                EnemyRecordForTest enemy = library.enemies[i];
                Assert.NotNull(enemy);
                Assert.IsTrue(Enum.TryParse(enemy.role, ignoreCase: true, out SurvivorsEnemyRole role), enemy.id);
                bool major = role == SurvivorsEnemyRole.Elite ||
                    role == SurvivorsEnemyRole.DreadElite ||
                    role == SurvivorsEnemyRole.Miniboss ||
                    role == SurvivorsEnemyRole.Boss;
                Assert.IsTrue(enemy.canLeash, enemy.id);
                Assert.IsTrue(enemy.canReposition, enemy.id);
                Assert.That(enemy.softLeashRadius, Is.GreaterThan(0f), enemy.id);
                Assert.That(enemy.hardRecycleRadius, Is.GreaterThan(enemy.softLeashRadius), enemy.id);
                Assert.That(enemy.catchUpSpeedMultiplier, Is.GreaterThanOrEqualTo(1f), enemy.id);
                Assert.That(enemy.repositionTimeoutSeconds, Is.GreaterThan(0f), enemy.id);
                if (major)
                {
                    Assert.IsFalse(enemy.canRecycle, enemy.id);
                    Assert.IsTrue(enemy.showOffscreenMarker, enemy.id);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(enemy.markerStyle), enemy.id);
                    if (role == SurvivorsEnemyRole.Boss)
                    {
                        Assert.IsTrue(enemy.showBossLifeBar, enemy.id);
                    }
                    else
                    {
                        Assert.IsTrue(enemy.showOverheadLifeBar, enemy.id);
                    }
                }
                else
                {
                    Assert.IsTrue(enemy.canRecycle, enemy.id);
                    Assert.IsFalse(enemy.showOffscreenMarker, enemy.id);
                    Assert.IsFalse(enemy.showOverheadLifeBar, enemy.id);
                    Assert.IsFalse(enemy.showBossLifeBar, enemy.id);
                }
            }
        }

        [Test]
        public void SampleContentValidationRequiresVerticalSliceShape()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.survivors.arcane-wand\",\"fireMode\":\"Projectile\",\"projectileId\":\"projectile.valid\",\"fanCount\":1}],\"projectiles\":[{\"id\":\"projectile.valid\"}]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.weapon.arcane-wand\"}]}";
            string enemyJson = "{\"enemies\":[{\"id\":\"enemy.survivors.swarm\",\"role\":\"Swarm\",\"health\":10,\"moveSpeed\":2,\"radius\":0.5,\"contactDamage\":1,\"contactIntervalSeconds\":0.5,\"experienceDrop\":1}]}";
            string classJson = "{\"defaultClassId\":\"class.valid\",\"classes\":[{\"id\":\"class.valid\",\"startingWeaponId\":\"weapon.survivors.arcane-wand\",\"startingWeaponIds\":[\"weapon.survivors.arcane-wand\"],\"unlockedByDefault\":true,\"unlockRewardId\":\"\",\"statModifiers\":[]}]}";
            string progressionJson = "{\"tracks\":[{\"id\":\"progression.valid.passives\",\"kind\":\"PassiveAtlas\",\"classId\":\"class.valid\",\"targetWeaponId\":\"\",\"nodes\":[{\"id\":\"node.passive\",\"upgradeId\":\"upgrade.valid\",\"kind\":\"Passive\",\"tier\":0,\"pointCost\":1,\"maxRank\":1}]},{\"id\":\"progression.valid.weapon\",\"kind\":\"WeaponSkillTrack\",\"classId\":\"\",\"targetWeaponId\":\"weapon.survivors.arcane-wand\",\"nodes\":[{\"id\":\"node.unlock\",\"upgradeId\":\"upgrade.valid\",\"kind\":\"WeaponUnlock\",\"tier\":0,\"pointCost\":1,\"maxRank\":1}]}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(
                weaponJson,
                upgradeJson,
                enemyJson: enemyJson,
                classJson: classJson,
                progressionJson: progressionJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("at least 6 playable weapons", errors);
            StringAssert.Contains("spread or fan projectile weapon", errors);
            StringAssert.Contains("orbiting weapon", errors);
            StringAssert.Contains("area burst weapon", errors);
            StringAssert.Contains("beam or hitscan weapon", errors);
            StringAssert.Contains("bomb, trap, or hazard payload weapon", errors);
            StringAssert.Contains("at least 6 enemy definitions", errors);
            StringAssert.Contains("enemy role Runner", errors);
            StringAssert.Contains("at least 2 elite variants", errors);
            StringAssert.Contains("at least 8 passive upgrades", errors);
            StringAssert.Contains("must include a weapon unlock, a five-rank weapon path, at least two mutation nodes, and an evolution node", errors);
            StringAssert.Contains("at least 6 complete weapon skill tracks", errors);
        }

        [Test]
        public void TemplateContractDocumentsVerticalSliceExtractionGuardrails()
        {
            string packageRoot = GetPackageRoot();
            string contractPath = Path.Combine(packageRoot, "Documentation~", "template-contract.md");
            string candidatesPath = Path.Combine(packageRoot, "Documentation~", "survivors-package-extraction-candidates.md");
            Assert.IsTrue(File.Exists(contractPath), contractPath);
            Assert.IsTrue(File.Exists(candidatesPath), candidatesPath);

            string contract = File.ReadAllText(contractPath);
            string candidates = File.ReadAllText(candidatesPath);

            StringAssert.Contains("Extract only reusable infrastructure, never the playable vertical slice.", contract);
            StringAssert.Contains("This template must always import/open as a complete playable vertical slice.", contract);
            StringAssert.Contains("Fallback/default runtime code may exist only as safe/debug/unbound-host fallback.", contract);
            StringAssert.Contains("Runtime code owns logic/execution. Authored content owns asset-flippable game content.", contract);
            StringAssert.Contains("Basic Survivors sample scene opens.", contract);
            StringAssert.Contains("Mode selector appears.", contract);
            StringAssert.Contains("Sprint Run starts.", contract);
            StringAssert.Contains("Standard / Human Playtest starts.", contract);
            StringAssert.Contains("Top-center timer works.", contract);
            StringAssert.Contains("Player starts with authored starting loadout.", contract);
            StringAssert.Contains("Weapons come from authored content.", contract);
            StringAssert.Contains("Enemies/elites/bosses come from authored content.", contract);
            StringAssert.Contains("No manual reconstruction is required after import.", contract);
            StringAssert.Contains("Known Contract Risks / Future Hardening", contract);
            StringAssert.Contains("SurvivorsAuthoredContentBindingPolicy.StrictSample", contract);
            StringAssert.Contains("gameplayTuningOverrides", contract);
            StringAssert.Contains("`DefaultProgression/progression.json` owns class passive atlases", contract);
            StringAssert.Contains("Prove extraction with Idle Auto Defense", contract);

            StringAssert.Contains("Specific Survivors weapons/enemies/bosses/evolutions remain local content.", candidates);
            StringAssert.Contains("Survivors tuning and balance remain local content.", candidates);
            StringAssert.Contains("The Basic Survivors sample scene remains in the template.", candidates);
            StringAssert.Contains("The playable vertical slice remains in the template.", candidates);
            StringAssert.Contains("Extraction must be tested by the template still playing.", candidates);
            StringAssert.Contains("## Reward Draft UI / Card System", candidates);
            StringAssert.Contains("## Rarity Tables And Rarity Presentation", candidates);
            StringAssert.Contains("## Upgrade Comparison Preview Helper", candidates);
            StringAssert.Contains("## Run Summary UI / Model", candidates);
            StringAssert.Contains("## Tutorial / Onboarding Overlay", candidates);
            StringAssert.Contains("## Audio Event Palette / Event Routing", candidates);
            StringAssert.Contains("## Mobile-Safe UI Primitives", candidates);
            StringAssert.Contains("## Theme / Style Token System", candidates);
            StringAssert.Contains("## Build Menu / Stat Summary Model", candidates);
            StringAssert.Contains("## Run Profile Selection", candidates);
            StringAssert.Contains("## Health Bars / Threat Markers", candidates);
            StringAssert.Contains("## Offscreen Spawn Resolver", candidates);
            StringAssert.Contains("## Content Validation Helpers", candidates);
            StringAssert.Contains("## Runtime Debug Overlay Framework", candidates);
            StringAssert.Contains("Vertical-slice preservation risk:", candidates);
        }

        [Test]
        public void SampleRunFlowContentIncludesRewardTuning()
        {
            string sampleRoot = GetSampleRoot();
            string runFlowJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json"));
            RunFlowLibraryForTest library = JsonUtility.FromJson<RunFlowLibraryForTest>(runFlowJson);

            RunFlowProfileForTest human = FindRunFlowProfile(library, SurvivorsPacingProfile.HumanPlaytest.ToString());
            RunFlowProfileForTest sprint = FindRunFlowProfile(library, SurvivorsPacingProfile.SprintRun.ToString());
            RunFlowProfileForTest normal = FindRunFlowProfile(library, SurvivorsPacingProfile.Normal.ToString());
            RunFlowProfileForTest debugFast = FindRunFlowProfile(library, SurvivorsPacingProfile.DebugFast.ToString());
            RunFlowProfileForTest showcase = FindRunFlowProfile(library, SurvivorsPacingProfile.Showcase.ToString());

            Assert.AreEqual("Full 30-minute run with boss, victory, rewards, and endless continuation.", human.description);
            Assert.AreEqual(1800f, human.targetDurationSeconds);
            Assert.AreEqual(1f, human.runRewardMultiplier);
            Assert.AreEqual(0, human.evolutionRequiredRankReduction);
            Assert.IsTrue(human.endlessContinuationEnabled);
            Assert.That(human.enemyRangedAttackDodgeExperienceReward, Is.GreaterThanOrEqualTo(1));
            Assert.That(human.hordeRushEnemyCountIncreasePerRush, Is.GreaterThanOrEqualTo(2));
            Assert.That(human.hordeRushExtraAliveAllowance, Is.GreaterThanOrEqualTo(12));
            Assert.That(human.hordeRushSpawnRadius, Is.InRange(7f, 10f));
            Assert.That(human.hordeRushClearExperienceGemCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(human.hordeRushClearExperienceMultiplier, Is.GreaterThan(1f));
            Assert.That(human.hordeRushClearMagnetEveryRush, Is.GreaterThanOrEqualTo(1));
            Assert.That(human.hordeRushClearBloodShardEveryRush, Is.GreaterThan(human.hordeRushClearMagnetEveryRush));
            Assert.That(human.hordeRushClearPulseDamage, Is.GreaterThan(0f));
            Assert.That(human.hordeRushClearPulseRadius, Is.InRange(4f, 6f));
            AssertRunFlowTrapChainMatchesTuning(human, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.HumanPlaytest));
            AssertRunFlowTrapChainMatchesTuning(sprint, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.SprintRun));
            AssertRunFlowTrapChainMatchesTuning(normal, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Normal));
            AssertRunFlowTrapChainMatchesTuning(debugFast, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.DebugFast));
            AssertRunFlowTrapChainMatchesTuning(showcase, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Showcase));
            AssertRunFlowLeashAndDraftThrottleMatchesTuning(human, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.HumanPlaytest));
            AssertRunFlowLeashAndDraftThrottleMatchesTuning(sprint, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.SprintRun));
            AssertRunFlowLeashAndDraftThrottleMatchesTuning(normal, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Normal));
            AssertRunFlowLeashAndDraftThrottleMatchesTuning(debugFast, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.DebugFast));
            AssertRunFlowLeashAndDraftThrottleMatchesTuning(showcase, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Showcase));
            AssertRunFlowEnemyStatsMatchesTuning(human, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.HumanPlaytest));
            AssertRunFlowEnemyStatsMatchesTuning(sprint, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.SprintRun));
            AssertRunFlowEnemyStatsMatchesTuning(normal, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Normal));
            AssertRunFlowEnemyStatsMatchesTuning(debugFast, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.DebugFast));
            AssertRunFlowEnemyStatsMatchesTuning(showcase, BasicSurvivorsGame.CreateTuning(SurvivorsPacingProfile.Showcase));
            Assert.AreEqual("Sprint Run", sprint.displayName);
            Assert.AreEqual(300f, sprint.targetDurationSeconds);
            Assert.AreEqual(0.65f, sprint.runRewardMultiplier);
            Assert.AreEqual(2, sprint.evolutionRequiredRankReduction);
            Assert.IsFalse(sprint.endlessContinuationEnabled);
            Assert.That(sprint.survivalVictoryTimeSeconds, Is.EqualTo(300f).Within(0.01f));
            Assert.That(sprint.firstEliteSpawnTimeSeconds, Is.InRange(75f, 90f));
            Assert.That(sprint.hordeRushFirstTimeSeconds, Is.EqualTo(120f).Within(0.01f));
            Assert.That(sprint.firstDreadEliteSpawnTimeSeconds, Is.InRange(150f, 180f));
            Assert.That(sprint.minibossSpawnTimeSeconds, Is.InRange(150f, 180f));
            Assert.That(sprint.bossSpawnTimeSeconds, Is.InRange(255f, 285f));
            Assert.That(sprint.rewardSelectionTimeoutSeconds, Is.GreaterThan(human.rewardSelectionTimeoutSeconds));
            Assert.That(sprint.draftRerollCharges, Is.GreaterThan(human.draftRerollCharges));
            Assert.That(sprint.draftBanishCharges, Is.GreaterThan(human.draftBanishCharges));
            Assert.That(sprint.hordeRushClearExperienceGemCount, Is.EqualTo(2));
            Assert.That(sprint.hordeRushClearExperienceMultiplier, Is.EqualTo(1.8f).Within(0.001f));
            Assert.That(sprint.experienceRequiredBase, Is.EqualTo(102));
            Assert.That(sprint.experienceRequiredPerLevel, Is.EqualTo(20));
            Assert.That(sprint.levelUpDraftCooldownSeconds, Is.EqualTo(18f).Within(0.001f));
            Assert.That(sprint.maximumQueuedLevelUps, Is.EqualTo(1));
            Assert.That(human.payloadHazardChainSnareThreshold, Is.GreaterThanOrEqualTo(4));
            Assert.That(human.payloadHazardChainExperienceGemCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(human.payloadHazardChainPulseDamage, Is.GreaterThan(0f));
            Assert.That(human.roamingCacheSurgeInterval, Is.InRange(3, 6));
            Assert.That(human.roamingCacheSurgeBonusGemCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(human.roamingCacheSurgeDurationSeconds, Is.InRange(4f, 8f));
            Assert.That(human.roamingCacheSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(human.roamingCacheSurgeMoveSpeedBonus, Is.GreaterThan(0f));
            Assert.That(human.roamingCacheSurgeCooldownMultiplierBonus, Is.LessThan(0f));
            Assert.That(human.roamingCacheSurgePickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(human.roamingCacheSurgePulseDamage, Is.GreaterThan(0f));
            Assert.That(human.roamingCacheSurgePulseRadius, Is.InRange(3f, 6f));
            Assert.That(human.roamingCacheMagnetInterval, Is.GreaterThanOrEqualTo(2));
            Assert.That(human.roamingCacheBloodShardInterval, Is.GreaterThan(human.roamingCacheMagnetInterval));
            Assert.That(human.roamingCacheAmbushStartCache, Is.GreaterThanOrEqualTo(3));
            Assert.That(human.roamingCacheAmbushInterval, Is.GreaterThanOrEqualTo(3));
            Assert.That(human.roamingCacheAmbushBaseEnemyCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(human.roamingCacheAmbushMaxEnemyCount, Is.GreaterThanOrEqualTo(human.roamingCacheAmbushBaseEnemyCount));
            Assert.That(human.roamingCacheAmbushExtraAliveAllowance, Is.GreaterThanOrEqualTo(human.roamingCacheAmbushMaxEnemyCount));
            Assert.That(human.roamingCacheAmbushRadius, Is.InRange(2f, 5f));
            Assert.That(human.roamingCacheAmbushClearMagnetInterval, Is.GreaterThanOrEqualTo(1));
            Assert.That(human.roamingCacheAmbushClearBloodShardInterval, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(0f, human.rewardSelectionTimeoutSeconds);
            Assert.That(human.draftRerollCharges, Is.GreaterThanOrEqualTo(2));
            Assert.That(human.draftBanishCharges, Is.GreaterThanOrEqualTo(2));
            Assert.That(human.draftSkipBloodShards, Is.GreaterThanOrEqualTo(1));
            Assert.That(normal.rewardSelectionTimeoutSeconds, Is.GreaterThan(human.rewardSelectionTimeoutSeconds));
            Assert.That(normal.draftRerollCharges, Is.GreaterThanOrEqualTo(human.draftRerollCharges));
            Assert.That(normal.draftBanishCharges, Is.GreaterThanOrEqualTo(human.draftBanishCharges));
            Assert.That(debugFast.roamingCacheSurgeInterval, Is.LessThanOrEqualTo(human.roamingCacheSurgeInterval));
            Assert.That(debugFast.roamingCacheSurgeBonusGemCount, Is.GreaterThanOrEqualTo(human.roamingCacheSurgeBonusGemCount));
            Assert.That(debugFast.roamingCacheSurgeDamageBonus, Is.GreaterThan(human.roamingCacheSurgeDamageBonus));
            Assert.That(debugFast.roamingCacheSurgePulseDamage, Is.GreaterThan(human.roamingCacheSurgePulseDamage));
            Assert.That(debugFast.hordeRushEnemyCountIncreasePerRush, Is.GreaterThan(human.hordeRushEnemyCountIncreasePerRush));
            Assert.That(debugFast.hordeRushExtraAliveAllowance, Is.GreaterThan(human.hordeRushExtraAliveAllowance));
            Assert.That(debugFast.hordeRushClearPulseDamage, Is.GreaterThan(human.hordeRushClearPulseDamage));
            Assert.That(debugFast.payloadHazardChainSnareThreshold, Is.LessThanOrEqualTo(human.payloadHazardChainSnareThreshold));
            Assert.That(debugFast.payloadHazardChainExperienceGemCount, Is.GreaterThanOrEqualTo(human.payloadHazardChainExperienceGemCount));
            Assert.That(debugFast.payloadHazardChainPulseDamage, Is.GreaterThan(human.payloadHazardChainPulseDamage));
            Assert.That(debugFast.draftRerollCharges, Is.GreaterThan(normal.draftRerollCharges));
            Assert.That(debugFast.draftBanishCharges, Is.GreaterThan(normal.draftBanishCharges));
            Assert.That(debugFast.rewardSelectionTimeoutSeconds, Is.GreaterThan(human.rewardSelectionTimeoutSeconds));
            Assert.That(debugFast.rewardSelectionTimeoutSeconds, Is.LessThan(normal.rewardSelectionTimeoutSeconds));
            Assert.That(debugFast.roamingCacheAmbushStartCache, Is.LessThan(human.roamingCacheAmbushStartCache));
            Assert.That(debugFast.roamingCacheAmbushMaxEnemyCount, Is.GreaterThan(human.roamingCacheAmbushMaxEnemyCount));
            Assert.That(debugFast.roamingCacheAmbushClearMagnetInterval, Is.LessThanOrEqualTo(human.roamingCacheAmbushClearMagnetInterval));
            Assert.That(debugFast.enemyRangedAttackDodgeExperienceReward, Is.GreaterThanOrEqualTo(human.enemyRangedAttackDodgeExperienceReward));
            Assert.That(showcase.roamingCacheSurgeInterval, Is.LessThanOrEqualTo(human.roamingCacheSurgeInterval));
            Assert.That(showcase.roamingCacheSurgePickupRangeBonus, Is.GreaterThan(human.roamingCacheSurgePickupRangeBonus));
            Assert.That(showcase.roamingCacheSurgePulseRadius, Is.GreaterThan(human.roamingCacheSurgePulseRadius));
            Assert.That(showcase.hordeRushClearPulseRadius, Is.GreaterThanOrEqualTo(human.hordeRushClearPulseRadius));
            Assert.That(showcase.rewardSelectionTimeoutSeconds, Is.GreaterThan(debugFast.rewardSelectionTimeoutSeconds));
            Assert.That(showcase.rewardSelectionTimeoutSeconds, Is.LessThan(normal.rewardSelectionTimeoutSeconds));
            Assert.That(showcase.roamingCacheAmbushInterval, Is.LessThanOrEqualTo(human.roamingCacheAmbushInterval));
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
        public void InvalidUpgradeSampleContentReportsMissingDraftCardMetadata()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\"}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("missing a display name", errors);
            StringAssert.Contains("missing a draft category", errors);
            StringAssert.Contains("missing draft description text", errors);
        }

        [Test]
        public void InvalidThemeSampleContentReportsMissingAudioAndTutorialAuthoring()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string uiThemeJson = "{\"themeName\":\"Broken Theme\",\"standardModeDisplayName\":\"Standard / Human Playtest\",\"sprintModeDisplayName\":\"Sprint Run\",\"tutorialTitle\":\"Primer\",\"audioEvents\":[],\"tutorialSteps\":[]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, uiThemeJson: uiThemeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("audio event palette", errors);
            StringAssert.Contains("tutorial step copy", errors);
        }

        [Test]
        public void TutorialValidationMatchesThreeLinePanelCapacity()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string themeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUiTheme", "ui-theme.json"));
            string overCapacity = ReplaceFirst(
                themeJson,
                "        \"The goal is not to stand still. Kite, collect, and keep the horde just barely under control.\"\n      ]",
                "        \"The goal is not to stand still. Kite, collect, and keep the horde just barely under control.\",\n" +
                "        \"A fourth row would overlap the current small-screen controls.\"\n      ]");

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(
                weaponJson,
                upgradeJson,
                uiThemeJson: overCapacity);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.That(SurvivorsContentValidator.MaximumTutorialLineCount, Is.EqualTo(3));
            Assert.That(result.Succeeded, Is.False);
            StringAssert.Contains("supports at most 3 lines in the current tutorial panel", errors);
        }

        [Test]
        public void SampleUpgradeContentRequiresEvolutionRecords()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\"}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("missing evolution upgrade records", errors);
        }

        [Test]
        public void SampleUpgradeContentRequiresExplicitEffectAmounts()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = ReplaceFirst(
                File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json")),
                "\"amount\": 2.0,",
                string.Empty);

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Upgrade upgrade.survivors.arcane-damage is missing a non-zero amount", errors);
        }

        [Test]
        public void SampleProgressionContentRequiresEvolutionNodes()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));
            string classJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json"));
            string progressionJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json"));

            progressionJson = progressionJson.Replace(
                "\"id\": \"node.survivors.arc-bolt.evolution\", \"displayName\": \"Arcane Storm\", \"upgradeId\": \"upgrade.survivors.evolution.arcane-storm\", \"kind\": \"Evolution\"",
                "\"id\": \"node.survivors.arc-bolt.evolution\", \"displayName\": \"Arcane Storm\", \"upgradeId\": \"upgrade.survivors.evolution.arcane-storm\", \"kind\": \"WeaponMutation\"");

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, classJson: classJson, progressionJson: progressionJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("missing evolution node", errors);
            StringAssert.Contains(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId, errors);
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
            string pickupJson = "{\"pickups\":[{\"id\":\"pickup.dup\",\"displayName\":\"\"},{\"id\":\"pickup.dup\",\"displayName\":\"Duplicate\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, pickupJson: pickupJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate pickup id", errors);
            StringAssert.Contains("missing a display name", errors);
            StringAssert.Contains("requires behavior text", errors);
            StringAssert.Contains("missing required pickup id", errors);
        }

        [Test]
        public void InvalidRunFlowSampleContentReportsPacingErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\"}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\"}]}";
            string runFlowJson = "{\"profiles\":[{\"id\":\"HumanPlaytest\",\"enemySpawnIntervalSeconds\":0,\"enemyMaximumAlive\":0,\"enemySpawnPackBaseCount\":3,\"enemySpawnPackMaxCount\":2,\"escalationIntervalSeconds\":0,\"minimumEnemySpawnIntervalSeconds\":0,\"enemySpawnIntervalReductionPerEscalation\":-1,\"enemyMaximumAliveIncreasePerEscalation\":-1,\"enemyHealthMultiplierPerEscalation\":0,\"enemyMoveSpeedMultiplierPerEscalation\":0,\"enemyExperienceMultiplierPerEscalation\":0,\"firstEliteSpawnTimeSeconds\":-1,\"eliteSpawnIntervalSeconds\":0,\"firstDreadEliteSpawnTimeSeconds\":-2,\"dreadEliteSpawnIntervalSeconds\":0,\"minibossSpawnTimeSeconds\":4,\"bossSpawnTimeSeconds\":3,\"survivalVictoryTimeSeconds\":2,\"hordeRushFirstTimeSeconds\":0,\"hordeRushIntervalSeconds\":0,\"hordeRushWarningLeadSeconds\":0,\"hordeRushBaseEnemyCount\":0,\"hordeRushEnemyCountIncreasePerRush\":0,\"hordeRushMaxEnemyCount\":0,\"hordeRushExtraAliveAllowance\":0,\"hordeRushSpawnRadius\":0,\"hordeRushClearExperienceGemCount\":0,\"hordeRushClearExperienceMultiplier\":0,\"hordeRushClearMagnetEveryRush\":2,\"hordeRushClearBloodShardEveryRush\":1,\"hordeRushClearPulseDamage\":0,\"hordeRushClearPulseRadius\":0,\"roamingCacheTravelInterval\":0,\"roamingCacheExperienceGemCount\":0,\"roamingCacheMagnetInterval\":0,\"roamingCacheBloodShardInterval\":0,\"roamingCacheAmbushStartCache\":0,\"roamingCacheAmbushInterval\":0,\"roamingCacheAmbushBaseEnemyCount\":3,\"roamingCacheAmbushMaxEnemyCount\":2,\"roamingCacheAmbushExtraAliveAllowance\":0,\"roamingCacheAmbushRadius\":0,\"roamingCacheAmbushClearMagnetInterval\":0,\"roamingCacheAmbushClearBloodShardInterval\":0,\"roamingCacheSurgeInterval\":0,\"roamingCacheSurgeBonusGemCount\":0,\"roamingCacheSurgeDurationSeconds\":0,\"roamingCacheSurgeDamageBonus\":0,\"roamingCacheSurgeMoveSpeedBonus\":0,\"roamingCacheSurgeCooldownMultiplierBonus\":0,\"roamingCacheSurgePickupRangeBonus\":0,\"roamingCacheSurgePulseDamage\":0,\"roamingCacheSurgePulseRadius\":0,\"draftChoiceCount\":0,\"draftRerollCharges\":0,\"draftBanishCharges\":0,\"draftSkipBloodShards\":0,\"rewardSelectionTimeoutSeconds\":-1,\"maxWeaponSlots\":0,\"maxPassiveSlots\":0,\"draftMidRarityLevel\":3,\"draftLateRarityLevel\":2,\"rarityTables\":[{\"id\":\"NormalEarly\",\"common\":0,\"uncommon\":1,\"rare\":1,\"epic\":0,\"legendary\":1},{\"id\":\"NormalMid\",\"common\":1,\"uncommon\":0,\"rare\":0,\"epic\":0,\"legendary\":2},{\"id\":\"Elite\",\"common\":5,\"uncommon\":0,\"rare\":0,\"epic\":0,\"legendary\":0},{\"id\":\"Boss\",\"common\":0,\"uncommon\":1,\"rare\":0,\"epic\":0,\"legendary\":0},{\"id\":\"Boss\",\"common\":0,\"uncommon\":0,\"rare\":1,\"epic\":1,\"legendary\":1},{\"id\":\"Broken\",\"common\":-1,\"uncommon\":0,\"rare\":0,\"epic\":0,\"legendary\":0}],\"endlessEliteSpawnIntervalSeconds\":0,\"endlessMinibossSpawnIntervalSeconds\":0,\"endlessBossSpawnIntervalSeconds\":0}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, runFlowJson: runFlowJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Run flow", errors);
            StringAssert.Contains("display name", errors);
            StringAssert.Contains("description", errors);
            StringAssert.Contains("target duration", errors);
            StringAssert.Contains("run reward multiplier", errors);
            StringAssert.Contains("SprintRun", errors);
            StringAssert.Contains("enemy spawn interval", errors);
            StringAssert.Contains("enemy ranged dodge XP reward", errors);
            StringAssert.Contains("horde rush", errors);
            StringAssert.Contains("payload hazard chain", errors);
            StringAssert.Contains("roaming cache ambush", errors);
            StringAssert.Contains("roaming cache surge", errors);
            StringAssert.Contains("draft reroll", errors);
            StringAssert.Contains("draft banish", errors);
            StringAssert.Contains("draft skip", errors);
            StringAssert.Contains("reward selection timeout", errors);
            StringAssert.Contains("max weapon slots", errors);
            StringAssert.Contains("max passive slots", errors);
            StringAssert.Contains("late rarity level", errors);
            StringAssert.Contains("rarity table", errors);
            StringAssert.Contains("duplicate rarity table id", errors);
            StringAssert.Contains("NormalLate", errors);
            StringAssert.Contains("Boss", errors);
            StringAssert.Contains("endless boss interval", errors);
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
        public void InvalidClassUnlockRewardReferenceReportsErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.valid\",\"fireMode\":\"Hitscan\",\"hitscanCount\":1,\"hitscanWidth\":1}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.arcane-wand\"}]}";
            string rewardJson = "{\"currencies\":[{\"id\":\"currency.survivors.blood-shards\"}],\"tracks\":[{\"id\":\"track.survivors.legacy-xp\"}],\"persistentUpgrades\":[{\"id\":\"meta.survivors.valid\",\"target\":\"survivors.weapon.arcane-wand\",\"effect\":\"survivors.meta.damage.flat\",\"maxRank\":1,\"rankCosts\":[1],\"amountPerRank\":1}],\"rewards\":[{\"id\":\"reward.survivors.valid\",\"currencyId\":\"currency.survivors.blood-shards\",\"trackId\":\"track.survivors.legacy-xp\",\"currencyAmount\":1,\"trackAmount\":1}]}";
            string classJson = "{\"defaultClassId\":\"class.survivors.default\",\"classes\":[{\"id\":\"class.survivors.default\",\"startingWeaponId\":\"weapon.survivors.arcane-wand\",\"startingWeaponIds\":[\"weapon.survivors.arcane-wand\"],\"unlockedByDefault\":true,\"unlockRewardId\":\"\",\"statModifiers\":[]},{\"id\":\"class.survivors.locked\",\"startingWeaponId\":\"weapon.survivors.arcane-wand\",\"startingWeaponIds\":[\"weapon.survivors.arcane-wand\"],\"unlockedByDefault\":false,\"unlockRewardId\":\"reward.survivors.missing\",\"statModifiers\":[]}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, rewardJson: rewardJson, classJson: classJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("unknown unlock reward id", errors);
            StringAssert.Contains("reward.survivors.missing", errors);
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
                Assert.IsTrue(service.TutorialSeen);
            }
        }

        [Test]
        public void TutorialSeenFlagPersistsAndCanBeReset()
        {
            var storage = new InMemoryTextStorage();
            var slotId = new SaveSlotId("tutorial-seen-test");

            using (var service = new SurvivorsMetaProgressionService(new PersistenceService(storage), slotId))
            {
                service.Load();
                Assert.IsFalse(service.TutorialSeen);
                Assert.IsTrue(service.MarkTutorialSeen().Succeeded);
                Assert.IsTrue(service.TutorialSeen);
            }

            using (var service = new SurvivorsMetaProgressionService(new PersistenceService(storage), slotId))
            {
                service.Load();
                Assert.IsTrue(service.TutorialSeen);
                Assert.IsTrue(service.ResetTutorialSeen().Succeeded);
                Assert.IsFalse(service.TutorialSeen);
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
                eliteSpawnIntervalSeconds: 0f,
                firstDreadEliteSpawnTimeSeconds: -2f,
                dreadEliteSpawnIntervalSeconds: 0f);

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateRunFlowContent(badRunFlow);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("escalation interval", errors);
            StringAssert.Contains("First elite spawn time", errors);
            StringAssert.Contains("Recurring elite spawn interval", errors);
            StringAssert.Contains("First dread elite spawn time", errors);
            StringAssert.Contains("Recurring dread elite spawn interval", errors);
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
            string enemyJson = "{\"enemies\":[{\"id\":\"enemy.dup\",\"role\":\"Miniboss\",\"health\":0,\"moveSpeed\":0,\"radius\":0,\"contactDamage\":-1,\"contactIntervalSeconds\":0,\"experienceDrop\":0,\"spawnTimeSeconds\":0},{\"id\":\"enemy.dup\",\"role\":\"NoSuchRole\",\"health\":1,\"moveSpeed\":1,\"radius\":1,\"contactDamage\":0,\"contactIntervalSeconds\":1,\"experienceDrop\":1},{\"id\":\"enemy.split.bad\",\"role\":\"Splitter\",\"health\":1,\"moveSpeed\":1,\"radius\":1,\"contactDamage\":0,\"contactIntervalSeconds\":1,\"experienceDrop\":1,\"spawnTimeSeconds\":1,\"splitChildCount\":0,\"splitChildRadius\":0}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson, enemyJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate enemy id", errors);
            StringAssert.Contains("unknown role", errors);
            StringAssert.Contains("health above zero", errors);
            StringAssert.Contains("spawn time above zero", errors);
            StringAssert.Contains("split child count", errors);
            StringAssert.Contains("split child radius", errors);
            StringAssert.Contains("splitter behavior text", errors);
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
                controller.ForceLevelUpWithLockedChoiceForTest("upgrade.survivors.arcane-damage");
                int previousWeaponCount = controller.ActiveWeaponCount;
                int previousPassiveCount = controller.ActivePassiveCount;
                int previousEvolutionCount = controller.EvolvedWeaponCount;
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
                float previousCriticalChance = controller.CriticalChanceNormalized;
                float previousCriticalDamageMultiplier = controller.CriticalDamageMultiplier;
                float previousDraftLuck = controller.DraftLuckBonus;
                float previousDeathNovaDamage = controller.DeathNovaDamage;
                float previousDeathNovaRadius = controller.DeathNovaRadius;
                float previousLifesteal = controller.LifestealRatio;
                float previousExperienceGain = controller.ExperienceGainMultiplierBonus;
                float previousAreaRadius = controller.AreaRadiusBonus;
                float previousBarrierCapacity = controller.BarrierCapacityBonus;
                float previousBarrierRegen = controller.BarrierRegenPerSecondBonus;
                float previousBarrierOnDamage = controller.BarrierOnDamageRatio;

                Assert.IsTrue(controller.SelectUpgrade(0));

                Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
                Assert.AreEqual(1, controller.SelectedUpgradeCount);
                bool changed = controller.ActiveWeaponCount > previousWeaponCount ||
                    controller.ActivePassiveCount > previousPassiveCount ||
                    controller.EvolvedWeaponCount > previousEvolutionCount ||
                    controller.ProjectileDamage > previousDamage ||
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
                    controller.CriticalChanceNormalized > previousCriticalChance ||
                    controller.CriticalDamageMultiplier > previousCriticalDamageMultiplier ||
                    controller.DraftLuckBonus > previousDraftLuck ||
                    controller.DeathNovaDamage > previousDeathNovaDamage ||
                    controller.DeathNovaRadius > previousDeathNovaRadius ||
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
            SurvivorsTemplateController controller = CreateController();
            try
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitWardUnlockUpgradeId));
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
            SurvivorsTemplateController controller = CreateController();
            try
            {
                Assert.AreEqual(0f, controller.PayloadExplosionRadiusBonus);

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GravityGrenadeUnlockUpgradeId));
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));

                Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(0f));
                Assert.AreEqual(4, controller.SelectedUpgradeCount);
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
            SurvivorsTemplateTuning tuning = BasicSurvivorsGame.CreateDefaultTuning();

            Assert.That(profiles.Count, Is.GreaterThanOrEqualTo(9));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Runner).MoveSpeed, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm).MoveSpeed));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Bruiser).MaxHealth, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm).MaxHealth));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Spitter).RangedAttackRange, Is.GreaterThan(0f));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Splitter).MaxHealth, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm).MaxHealth));
            Assert.That(tuning.SplitterChildCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Summoner).PreferredRange, Is.GreaterThan(0f));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Summoner).ExperienceReward, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Runner).ExperienceReward));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Elite).ExperienceReward, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Bruiser).ExperienceReward));
            Assert.That(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.DreadElite).RangedAttackRange, Is.GreaterThan(BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Elite).RangedAttackRange));
            Assert.IsTrue(BasicSurvivorsGame.CreateMetaProgressionDefinition().TryGetReward(BasicSurvivorsGame.EliteRewardId, out _));
            Assert.IsTrue(BasicSurvivorsGame.CreateMetaProgressionDefinition().TryGetReward(BasicSurvivorsGame.EmberVanguardUnlockRewardId, out _));
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

        private static SurvivorsTemplateController CreateController(bool startRun = true, bool tutorialSeen = true)
        {
            var root = new GameObject("Survivors Template EditMode Test");
            SurvivorsTemplateController controller = root.AddComponent<SurvivorsTemplateController>();
            controller.ConfigureMetaPersistenceForTest(
                new PersistenceService(new InMemoryTextStorage()),
                new SaveSlotId("edit-" + Guid.NewGuid().ToString("N")));
            if (tutorialSeen)
            {
                controller.MarkTutorialSeenForTest();
            }

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

        private static bool TryCreateStrictSampleContent(
            out SurvivorsAuthoredContentDefinition definition,
            out string error,
            string weaponJson = null,
            string upgradeJson = null,
            string relicJson = null,
            string classJson = null,
            string progressionJson = null,
            string enemyJson = null,
            string runFlowJson = null,
            string rewardJson = null)
        {
            string sampleRoot = GetSampleRoot();
            return SurvivorsAuthoredContentDefinition.TryCreate(
                weaponJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json")),
                upgradeJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json")),
                relicJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRelics", "relics.json")),
                classJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultClasses", "classes.json")),
                progressionJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultProgression", "progression.json")),
                enemyJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultEnemies", "enemies.json")),
                runFlowJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRunFlow", "run-flow.json")),
                rewardJson ?? File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultRewards", "rewards.json")),
                SurvivorsAuthoredContentBindingPolicy.StrictSample,
                out definition,
                out error);
        }

        private static SurvivorsWeaponArchetypeDefinition FindWeapon(
            IReadOnlyList<SurvivorsWeaponArchetypeDefinition> weapons,
            string weaponId)
        {
            if (weapons != null)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] != null && string.Equals(weapons[i].Id, weaponId, StringComparison.Ordinal))
                    {
                        return weapons[i];
                    }
                }
            }

            Assert.Fail("Missing authored weapon " + weaponId);
            return null;
        }

        private static SurvivorsClassUpgradeGateDefinition FindClassGate(
            IReadOnlyList<SurvivorsClassUpgradeGateDefinition> gates,
            string upgradeId)
        {
            if (gates != null)
            {
                for (int i = 0; i < gates.Count; i++)
                {
                    if (gates[i] != null && string.Equals(gates[i].UpgradeId, upgradeId, StringComparison.Ordinal))
                    {
                        return gates[i];
                    }
                }
            }

            Assert.Fail("Missing authored class gate " + upgradeId);
            return null;
        }

        private static SurvivorsProgressionTrackDefinition FindProgressionTrack(
            IReadOnlyList<SurvivorsProgressionTrackDefinition> tracks,
            string trackId)
        {
            if (tracks != null)
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    if (tracks[i] != null && string.Equals(tracks[i].Id, trackId, StringComparison.Ordinal))
                    {
                        return tracks[i];
                    }
                }
            }

            Assert.Fail("Missing authored progression track " + trackId);
            return null;
        }

        private static SurvivorsProgressionNodeDefinition FindProgressionNode(
            SurvivorsProgressionTrackDefinition track,
            string nodeId)
        {
            if (track != null && track.Nodes != null)
            {
                for (int i = 0; i < track.Nodes.Count; i++)
                {
                    if (track.Nodes[i] != null && string.Equals(track.Nodes[i].Id, nodeId, StringComparison.Ordinal))
                    {
                        return track.Nodes[i];
                    }
                }
            }

            Assert.Fail("Missing authored progression node " + nodeId);
            return null;
        }

        private static string GetSampleRoot()
        {
            string sampleRoot = Path.Combine(GetPackageRoot(), "Samples~", "BasicSurvivorsGame");
            Assert.IsTrue(Directory.Exists(sampleRoot), "Sample root missing: " + sampleRoot);
            return sampleRoot;
        }

        private static string GetPackageRoot()
        {
            var packageInfo = PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            Assert.IsNotNull(packageInfo);
            Assert.IsTrue(Directory.Exists(packageInfo.resolvedPath), "Package root missing: " + packageInfo.resolvedPath);
            return packageInfo.resolvedPath;
        }

        private static RunFlowProfileForTest FindRunFlowProfile(RunFlowLibraryForTest library, string id)
        {
            Assert.IsNotNull(library);
            Assert.IsNotNull(library.profiles);
            for (int i = 0; i < library.profiles.Length; i++)
            {
                RunFlowProfileForTest profile = library.profiles[i];
                if (profile != null && string.Equals(profile.id, id, StringComparison.Ordinal))
                {
                    return profile;
                }
            }

            Assert.Fail("Missing run flow profile: " + id);
            return null;
        }

        private static void AssertRunFlowTrapChainMatchesTuning(RunFlowProfileForTest profile, SurvivorsTemplateTuning tuning)
        {
            Assert.NotNull(profile);
            Assert.NotNull(tuning);
            Assert.AreEqual(tuning.PayloadHazardChainSnareThreshold, profile.payloadHazardChainSnareThreshold);
            Assert.That(profile.payloadHazardChainWindowSeconds, Is.EqualTo(tuning.PayloadHazardChainWindowSeconds).Within(0.001f));
            Assert.That(profile.payloadHazardChainCooldownSeconds, Is.EqualTo(tuning.PayloadHazardChainCooldownSeconds).Within(0.001f));
            Assert.AreEqual(tuning.PayloadHazardChainExperienceGemCount, profile.payloadHazardChainExperienceGemCount);
            Assert.That(profile.payloadHazardChainExperienceMultiplier, Is.EqualTo(tuning.PayloadHazardChainExperienceMultiplier).Within(0.001f));
            Assert.That(profile.payloadHazardChainPulseDamage, Is.EqualTo(tuning.PayloadHazardChainPulseDamage).Within(0.001f));
            Assert.That(profile.payloadHazardChainPulseRadius, Is.EqualTo(tuning.PayloadHazardChainPulseRadius).Within(0.001f));
        }

        private static void AssertRunFlowLeashAndDraftThrottleMatchesTuning(RunFlowProfileForTest profile, SurvivorsTemplateTuning tuning)
        {
            Assert.NotNull(profile);
            Assert.NotNull(tuning);
            Assert.That(profile.enemySoftLeashRadius, Is.EqualTo(tuning.EnemySoftLeashRadius).Within(0.001f));
            Assert.That(profile.enemyHardRecycleRadius, Is.EqualTo(tuning.EnemyHardRecycleRadius).Within(0.001f));
            Assert.That(profile.enemyRecycleDelaySeconds, Is.EqualTo(tuning.EnemyRecycleDelaySeconds).Within(0.001f));
            Assert.That(profile.enemyRecycleMinimumRespawnDistance, Is.EqualTo(tuning.EnemyRecycleMinimumRespawnDistance).Within(0.001f));
            Assert.That(profile.enemyRecycleMaximumRespawnDistance, Is.EqualTo(tuning.EnemyRecycleMaximumRespawnDistance).Within(0.001f));
            Assert.That(profile.majorThreatCatchUpRadius, Is.EqualTo(tuning.MajorThreatCatchUpRadius).Within(0.001f));
            Assert.That(profile.majorThreatCatchUpSpeedMultiplier, Is.EqualTo(tuning.MajorThreatCatchUpSpeedMultiplier).Within(0.001f));
            Assert.That(profile.majorThreatRepositionRadius, Is.EqualTo(tuning.MajorThreatRepositionRadius).Within(0.001f));
            Assert.That(profile.majorThreatRepositionDelaySeconds, Is.EqualTo(tuning.MajorThreatRepositionDelaySeconds).Within(0.001f));
            Assert.That(profile.offscreenSpawnPadding, Is.EqualTo(tuning.OffscreenSpawnPadding).Within(0.001f));
            Assert.That(profile.spawnBandDepth, Is.EqualTo(tuning.SpawnBandDepth).Within(0.001f));
            Assert.That(profile.majorThreatOffscreenSpawnPadding, Is.EqualTo(tuning.MajorThreatOffscreenSpawnPadding).Within(0.001f));
            Assert.That(profile.recycledEnemyOffscreenSpawnPadding, Is.EqualTo(tuning.RecycledEnemyOffscreenSpawnPadding).Within(0.001f));
            Assert.That(profile.offscreenThreatMarkerDistance, Is.EqualTo(tuning.OffscreenThreatMarkerDistance).Within(0.001f));
            Assert.AreEqual(tuning.ExperienceRequiredBase, profile.experienceRequiredBase);
            Assert.AreEqual(tuning.ExperienceRequiredPerLevel, profile.experienceRequiredPerLevel);
            Assert.That(profile.levelUpDraftCooldownSeconds, Is.EqualTo(tuning.LevelUpDraftCooldownSeconds).Within(0.001f));
            Assert.AreEqual(tuning.MaximumQueuedLevelUps, profile.maximumQueuedLevelUps);
            Assert.That(profile.pickupMagnetPulseBaseIntervalSeconds, Is.EqualTo(tuning.PickupMagnetPulseBaseIntervalSeconds).Within(0.001f));
            Assert.That(profile.pickupMagnetPulseMinimumIntervalSeconds, Is.EqualTo(tuning.PickupMagnetPulseMinimumIntervalSeconds).Within(0.001f));
        }

        private static void AssertRunFlowEnemyStatsMatchesTuning(RunFlowProfileForTest profile, SurvivorsTemplateTuning tuning)
        {
            Assert.NotNull(profile);
            Assert.NotNull(tuning);
            Assert.That(profile.enemyMaxHealth, Is.EqualTo(tuning.EnemyMaxHealth).Within(0.001f));
            Assert.That(profile.enemyMoveSpeed, Is.EqualTo(tuning.EnemyMoveSpeed).Within(0.001f));
            Assert.That(profile.enemyRadius, Is.EqualTo(tuning.EnemyRadius).Within(0.001f));
            Assert.That(profile.enemyContactDamage, Is.EqualTo(tuning.EnemyContactDamage).Within(0.001f));
            Assert.That(profile.enemyContactIntervalSeconds, Is.EqualTo(tuning.EnemyContactIntervalSeconds).Within(0.001f));
            Assert.AreEqual(tuning.EnemyExperienceReward, profile.enemyExperienceReward);
            Assert.That(profile.minibossMaxHealth, Is.EqualTo(tuning.MinibossMaxHealth).Within(0.001f));
            Assert.That(profile.minibossMoveSpeed, Is.EqualTo(tuning.MinibossMoveSpeed).Within(0.001f));
            Assert.That(profile.minibossRadius, Is.EqualTo(tuning.MinibossRadius).Within(0.001f));
            Assert.That(profile.minibossContactDamage, Is.EqualTo(tuning.MinibossContactDamage).Within(0.001f));
            Assert.That(profile.minibossContactIntervalSeconds, Is.EqualTo(tuning.MinibossContactIntervalSeconds).Within(0.001f));
            Assert.AreEqual(tuning.MinibossExperienceReward, profile.minibossExperienceReward);
            Assert.That(profile.bossMaxHealth, Is.EqualTo(tuning.BossMaxHealth).Within(0.001f));
            Assert.That(profile.bossMoveSpeed, Is.EqualTo(tuning.BossMoveSpeed).Within(0.001f));
            Assert.That(profile.bossRadius, Is.EqualTo(tuning.BossRadius).Within(0.001f));
            Assert.That(profile.bossContactDamage, Is.EqualTo(tuning.BossContactDamage).Within(0.001f));
            Assert.That(profile.bossContactIntervalSeconds, Is.EqualTo(tuning.BossContactIntervalSeconds).Within(0.001f));
            Assert.AreEqual(tuning.BossExperienceReward, profile.bossExperienceReward);
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

        private static string ReplaceFirst(string text, string oldValue, string newValue)
        {
            int index = text.IndexOf(oldValue, StringComparison.Ordinal);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "Text did not contain expected fragment: " + oldValue);
            return text.Substring(0, index) + newValue + text.Substring(index + oldValue.Length);
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

        [Serializable]
        private sealed class RunFlowLibraryForTest
        {
            public RunFlowProfileForTest[] profiles;
        }

        [Serializable]
        private sealed class RunFlowProfileForTest
        {
            public string id;
            public string displayName;
            public string description;
            public float targetDurationSeconds;
            public float runRewardMultiplier;
            public int evolutionRequiredRankReduction;
            public bool endlessContinuationEnabled;
            public float firstEliteSpawnTimeSeconds;
            public float firstDreadEliteSpawnTimeSeconds;
            public float minibossSpawnTimeSeconds;
            public float minibossMaxHealth;
            public float minibossMoveSpeed;
            public float minibossRadius;
            public float minibossContactDamage;
            public float minibossContactIntervalSeconds;
            public int minibossExperienceReward;
            public float bossSpawnTimeSeconds;
            public float bossMaxHealth;
            public float bossMoveSpeed;
            public float bossRadius;
            public float bossContactDamage;
            public float bossContactIntervalSeconds;
            public int bossExperienceReward;
            public float survivalVictoryTimeSeconds;
            public float hordeRushFirstTimeSeconds;
            public int enemyRangedAttackDodgeExperienceReward;
            public float enemyMaxHealth;
            public float enemyMoveSpeed;
            public float enemyRadius;
            public float enemyContactDamage;
            public float enemyContactIntervalSeconds;
            public int enemyExperienceReward;
            public float enemySoftLeashRadius;
            public float enemyHardRecycleRadius;
            public float enemyRecycleDelaySeconds;
            public float enemyRecycleMinimumRespawnDistance;
            public float enemyRecycleMaximumRespawnDistance;
            public float majorThreatCatchUpRadius;
            public float majorThreatCatchUpSpeedMultiplier;
            public float majorThreatRepositionRadius;
            public float majorThreatRepositionDelaySeconds;
            public float offscreenSpawnPadding;
            public float spawnBandDepth;
            public float majorThreatOffscreenSpawnPadding;
            public float recycledEnemyOffscreenSpawnPadding;
            public float offscreenThreatMarkerDistance;
            public int hordeRushEnemyCountIncreasePerRush;
            public int hordeRushExtraAliveAllowance;
            public float hordeRushSpawnRadius;
            public int hordeRushClearExperienceGemCount;
            public float hordeRushClearExperienceMultiplier;
            public int hordeRushClearMagnetEveryRush;
            public int hordeRushClearBloodShardEveryRush;
            public float hordeRushClearPulseDamage;
            public float hordeRushClearPulseRadius;
            public int payloadHazardChainSnareThreshold;
            public float payloadHazardChainWindowSeconds;
            public float payloadHazardChainCooldownSeconds;
            public int payloadHazardChainExperienceGemCount;
            public float payloadHazardChainExperienceMultiplier;
            public float payloadHazardChainPulseDamage;
            public float payloadHazardChainPulseRadius;
            public int roamingCacheMagnetInterval;
            public int roamingCacheBloodShardInterval;
            public int roamingCacheAmbushStartCache;
            public int roamingCacheAmbushInterval;
            public int roamingCacheAmbushBaseEnemyCount;
            public int roamingCacheAmbushMaxEnemyCount;
            public int roamingCacheAmbushExtraAliveAllowance;
            public float roamingCacheAmbushRadius;
            public int roamingCacheAmbushClearMagnetInterval;
            public int roamingCacheAmbushClearBloodShardInterval;
            public int roamingCacheSurgeInterval;
            public int roamingCacheSurgeBonusGemCount;
            public float roamingCacheSurgeDurationSeconds;
            public float roamingCacheSurgeDamageBonus;
            public float roamingCacheSurgeMoveSpeedBonus;
            public float roamingCacheSurgeCooldownMultiplierBonus;
            public float roamingCacheSurgePickupRangeBonus;
            public float roamingCacheSurgePulseDamage;
            public float roamingCacheSurgePulseRadius;
            public int draftRerollCharges;
            public int draftBanishCharges;
            public int draftSkipBloodShards;
            public float rewardSelectionTimeoutSeconds;
            public int experienceRequiredBase;
            public int experienceRequiredPerLevel;
            public float levelUpDraftCooldownSeconds;
            public int maximumQueuedLevelUps;
            public float pickupMagnetPulseBaseIntervalSeconds;
            public float pickupMagnetPulseMinimumIntervalSeconds;
        }

        [Serializable]
        private sealed class EnemyLibraryForTest
        {
            public EnemyRecordForTest[] enemies;
        }

        [Serializable]
        private sealed class EnemyRecordForTest
        {
            public string id;
            public string role;
            public bool canRecycle;
            public bool canLeash;
            public bool canReposition;
            public bool showOffscreenMarker;
            public bool showOverheadLifeBar;
            public bool showBossLifeBar;
            public string markerStyle;
            public float softLeashRadius;
            public float hardRecycleRadius;
            public float catchUpSpeedMultiplier;
            public float repositionTimeoutSeconds;
        }
    }
}
