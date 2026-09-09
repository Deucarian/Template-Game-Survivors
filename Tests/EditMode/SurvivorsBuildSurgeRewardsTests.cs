using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsBuildSurgeRewardsTests
    {
        [Test]
        public void FullWeaponSlotsConsumeOneUseEvenWhenDurationAndPulseAreDisabled()
        {
            var port = new Port();
            var rewards = Create(port, out _);
            port.Tuning.WeaponLoadoutSurgeDurationSeconds = 0;
            port.Tuning.WeaponLoadoutSurgePulseRadius = 0;
            rewards.TryTriggerWeaponLoadoutSurge(null);
            Assert.AreEqual(0, rewards.WeaponLoadoutSurgeActivationCount);
            port.WeaponCount = 1;
            rewards.TryTriggerWeaponLoadoutSurge(null);
            port.Tuning.WeaponLoadoutSurgeDurationSeconds = 10;
            rewards.TryTriggerWeaponLoadoutSurge(null);
            Assert.AreEqual(1, rewards.WeaponLoadoutSurgeActivationCount);
            Assert.IsFalse(rewards.IsWeaponLoadoutSurgeActive);
            Assert.AreEqual(0, port.DamageCalls);
            CollectionAssert.AreEqual(new[] { "feedback", "pulse:42:False" }, port.Events);
            rewards.Reset();
            rewards.TryTriggerWeaponLoadoutSurge(null);
            Assert.AreEqual(1, rewards.WeaponLoadoutSurgeActivationCount);
            Assert.AreEqual(10, rewards.WeaponLoadoutSurgeRemainingSeconds);
        }

        [Test]
        public void PassiveMilestoneReadsAuthoritativeSlotsAndLiveClampedBonuses()
        {
            var port = new Port();
            var rewards = Create(port, out var build);
            rewards.TryTriggerPassiveLoadoutSurge(null);
            Assert.AreEqual(0, rewards.PassiveLoadoutSurgeActivationCount);
            build.RecordRunBuildSelection(Upgrade("passive"));
            port.Tuning.PassiveLoadoutSurgeDurationSeconds = 5;
            rewards.TryTriggerPassiveLoadoutSurge(Upgrade("passive"));
            Assert.AreEqual(1, rewards.PassiveLoadoutSurgeActivationCount);
            port.Tuning.PassiveLoadoutSurgeExperienceGainMultiplierBonus = 0.75f;
            Assert.AreEqual(0.75f, rewards.PassiveLoadoutSurgeExperienceGainMultiplierBonus);
            port.Tuning.PassiveLoadoutSurgeExperienceGainMultiplierBonus = -1;
            port.Tuning.PassiveLoadoutSurgeCooldownMultiplierBonus = 1;
            Assert.AreEqual(0, rewards.PassiveLoadoutSurgeExperienceGainMultiplierBonus);
            Assert.AreEqual(0, rewards.PassiveLoadoutSurgeCooldownMultiplierBonus);
            build.RecordRunBuildSelection(Upgrade("passive"));
            rewards.TryTriggerPassiveLoadoutSurge(null);
            Assert.AreEqual(1, rewards.PassiveLoadoutSurgeActivationCount);
            rewards.TickPassiveLoadoutSurge(5);
            Assert.IsFalse(rewards.IsPassiveLoadoutSurgeActive);
        }

        [Test]
        public void EvolutionDamagesThenRecallsBeforeFeedbackAndCountsOnlySuccessfulRecalls()
        {
            var port = new Port { Hits = 2, Gems = 3 };
            port.Tuning.EvolutionSurgeRadius = 4;
            port.Tuning.EvolutionSurgeDamage = 8;
            var rewards = Create(port, out _);
            rewards.TriggerWeaponEvolutionSurge(Upgrade("evolution-a"));
            CollectionAssert.AreEqual(new[] { "survivors.evolution.surge", "recall", "feedback", "pulse:56:False" }, port.Events);
            Assert.AreEqual(2, rewards.WeaponEvolutionSurgeHitCount);
            Assert.AreEqual(1, rewards.EvolutionMagnetRecallCount);
            Assert.AreEqual(3, rewards.EvolutionMagnetRecallGemCount);
            Assert.That(rewards.LastWeaponEvolutionSurgeFeedbackLabel, Does.Contain("3 XP recalled"));
            port.Gems = 0;
            rewards.TriggerWeaponEvolutionSurge(null);
            Assert.AreEqual(2, rewards.WeaponEvolutionSurgeCount);
            Assert.AreEqual(1, rewards.EvolutionMagnetRecallCount);
            Assert.That(rewards.LastWeaponEvolutionSurgeFeedbackLabel, Does.Not.Contain("XP recalled"));
        }

        [Test]
        public void EvolutionRecallStillRunsWithoutDamageAndPulseIsAlwaysPresented()
        {
            var port = new Port();
            port.Tuning.EvolutionSurgeDamage = -1;
            var rewards = Create(port, out _);
            rewards.TriggerWeaponEvolutionSurge(null);
            CollectionAssert.AreEqual(new[] { "recall", "feedback", "pulse:54:False" }, port.Events);
            Assert.AreEqual(1, rewards.WeaponEvolutionSurgeCount);
            Assert.AreEqual(0, rewards.WeaponEvolutionSurgeHitCount);
        }

        [Test]
        public void EvolutionChainRequiresTwoOwnedEvolutionsEvenWhenConfiguredMinimumIsLower()
        {
            var port = new Port();
            port.Tuning.EvolutionChainSurgeMinimumEvolutions = 0;
            port.Tuning.EvolutionChainSurgeDurationSeconds = 0;
            port.Tuning.EvolutionChainSurgePulseRadius = 0;
            var rewards = Create(port, out var build);
            build.RecordRunBuildSelection(Upgrade("evolution-a"));
            rewards.TriggerEvolutionChainSurge(null);
            Assert.AreEqual(0, rewards.EvolutionChainSurgeActivationCount);
            build.RecordRunBuildSelection(Upgrade("evolution-b"));
            rewards.TriggerEvolutionChainSurge(null);
            Assert.AreEqual(1, rewards.EvolutionChainSurgeActivationCount);
            Assert.IsFalse(rewards.IsEvolutionChainSurgeActive);
            Assert.That(rewards.LastEvolutionChainSurgeFeedbackLabel, Does.Contain("2 evolutions"));
            Assert.That(rewards.LastEvolutionChainSurgeFeedbackLabel, Does.Not.Contain("rush"));
            port.Tuning.EvolutionChainSurgeMinimumEvolutions = 3;
            rewards.TriggerEvolutionChainSurge(null);
            Assert.AreEqual(1, rewards.EvolutionChainSurgeActivationCount);
        }

        [Test]
        public void RepeatRelicsRefreshWithoutShorteningAndAllTimersRemainIndependent()
        {
            var port = new Port { WeaponCount = 1, Hits = 30 };
            port.Tuning.WeaponLoadoutSurgeDurationSeconds = 10;
            port.Tuning.BossRelicSurgeDurationSeconds = 8;
            port.Tuning.BossRelicSurgeRadius = 1;
            port.Tuning.BossRelicSurgeDamage = 1;
            var rewards = Create(port, out _);
            rewards.TryTriggerWeaponLoadoutSurge(null);
            rewards.TriggerBossRelicSurge(null);
            rewards.TickBossRelicSurge(2);
            port.Tuning.BossRelicSurgeDurationSeconds = 3;
            rewards.TriggerBossRelicSurge(null);
            rewards.TickBossRelicSurge(-1);
            Assert.AreEqual(6, rewards.BossRelicSurgeRemainingSeconds);
            Assert.AreEqual(10, rewards.WeaponLoadoutSurgeRemainingSeconds);
            Assert.AreEqual(2, rewards.BossRelicSurgeCount);
            Assert.AreEqual(60, rewards.BossRelicSurgeHitCount);
            Assert.AreEqual("pulse:88:True", port.Events[port.Events.Count - 1]);
            rewards.TickBossRelicSurge(20);
            Assert.IsFalse(rewards.IsBossRelicSurgeActive);
            Assert.IsTrue(rewards.IsWeaponLoadoutSurgeActive);
            rewards.Reset();
            Assert.AreEqual(0, rewards.BossRelicSurgeCount);
            Assert.IsEmpty(rewards.LastBossRelicSurgeFeedbackLabel);
            Assert.IsFalse(rewards.IsWeaponLoadoutSurgeActive);
        }

        private static SurvivorsBuildSurgeRewards Create(Port port, out SurvivorsRunBuildState build)
        {
            build = new SurvivorsRunBuildState(port);
            build.Initialize(new RunUpgradeCatalog(new[] { Upgrade("passive"), Upgrade("evolution-a"), Upgrade("evolution-b") }), new[] {
                Metadata("passive", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive),
                Metadata("evolution-a", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None),
                Metadata("evolution-b", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None) }, null, null);
            return new SurvivorsBuildSurgeRewards(build, port);
        }
        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Common, 100, 3,
            new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1d) });
        private static SurvivorsRunUpgradeMetadata Metadata(string id, SurvivorsRunUpgradeCategory category, SurvivorsRunBuildSlotKind slot) =>
            new SurvivorsRunUpgradeMetadata(id, id, category, slot, id, "Test");
        private sealed class Port : ISurvivorsBuildSurgePort, ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning { MaxWeaponSlots = 1, MaxPassiveSlots = 1 };
            public Vector3 PlayerPosition => new Vector3(2, 0, 3);
            public int WeaponCount { get; set; }
            public int Hits, Gems, DamageCalls;
            public readonly List<string> Events = new List<string>();
            public int DamageNonMajor(Vector3 position, float radius, float damage, string source)
            { Assert.AreEqual(PlayerPosition, position); DamageCalls++; Events.Add(source); return Hits; }
            public int RecallGems() { Events.Add("recall"); return Gems; }
            public Color RelicAccent(SurvivorsRelicDefinition relic) => Color.red;
            public void ShowFeedback(string label, Color color) => Events.Add("feedback");
            public void PlayPulse(int count, bool boss) => Events.Add($"pulse:{count}:{boss}");
            public bool HasWeapon(string id) => false;
            public void AddWeapon(string id) { }
            public void PassiveAdded(RunUpgradeDefinition upgrade) { }
            public void RecordEvolutionTime() { }
            public void EvolutionAdded(RunUpgradeDefinition upgrade) { }
        }
    }
}
