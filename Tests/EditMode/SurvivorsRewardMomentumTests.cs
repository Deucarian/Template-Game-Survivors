using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRewardMomentumTests
    {
        [Test]
        public void EndlessRewardsRequireVictoryAndMajorRole()
        {
            var session = new SurvivorsRunSession();
            var port = new Port();
            var owner = new SurvivorsEndlessSurgeRewards(session, port);
            owner.TryActivateEndlessSurge(SurvivorsEnemyRole.Boss, Vector3.zero, 10);
            session.Win();
            owner.TryActivateEndlessSurge(SurvivorsEnemyRole.Swarm, Vector3.zero, 10);
            Assert.AreEqual(0, owner.EndlessSurgeActivationCount);
            Assert.IsEmpty(port.Drops);
            owner.TryActivateEndlessSurge(SurvivorsEnemyRole.Elite, Vector3.zero, 10);
            Assert.AreEqual(1, owner.EndlessSurgeActivationCount);
            Assert.AreEqual(1, owner.EndlessSurgeTier);
        }

        [Test]
        public void FailedEndlessDropsStillAdvanceTierAndPulseButDoNotClaimDroppedExperience()
        {
            var session = new SurvivorsRunSession(); session.Win();
            var port = new Port { Success = false, Hits = 2 };
            var owner = new SurvivorsEndlessSurgeRewards(session, port);
            owner.TryActivateEndlessSurge(SurvivorsEnemyRole.Boss, Vector3.one, 10);
            Assert.AreEqual(0, owner.EndlessSurgeExperienceGemDropCount);
            Assert.AreEqual(0, owner.EndlessSurgeBloodShardDropCount);
            Assert.AreEqual(2, owner.EndlessSurgePulseHitCount);
            Assert.That(owner.LastEndlessSurgeFeedbackLabel, Does.Contain("+0 XP"));
            Assert.That(owner.LastEndlessSurgeFeedbackLabel, Does.Contain("+3 Shards"), "Legacy feedback shows the attempted shard amount.");
            Assert.AreEqual("pulse:False:True", port.Events[port.Events.Count - 1]);
            Assert.AreEqual(4, port.Drops.Count);
            foreach (var drop in port.Drops) Assert.IsFalse(drop.Attract);
        }

        [Test]
        public void EndlessTierSurvivesExpiryAndIntensityCapsWhileTuningStaysLive()
        {
            var session = new SurvivorsRunSession(); session.Win();
            var port = new Port();
            port.Tuning.EndlessSurgeDurationSeconds = 0;
            port.Tuning.EndlessSurgeDamageBonus = 2;
            var owner = new SurvivorsEndlessSurgeRewards(session, port);
            for (int i = 0; i < 12; i++) owner.TryActivateEndlessSurge(SurvivorsEnemyRole.Elite, Vector3.zero, 1);
            Assert.AreEqual(0.1f, owner.EndlessSurgeRemainingSeconds);
            Assert.AreEqual(4.5f, owner.EndlessSurgeDamageBonus);
            owner.TickEndlessSurge(-1);
            Assert.IsTrue(owner.IsEndlessSurgeActive);
            owner.TickEndlessSurge(1);
            Assert.AreEqual(12, owner.EndlessSurgeTier);
            Assert.AreEqual(0, owner.EndlessSurgeDamageBonus);
            owner.TryActivateEndlessSurge(SurvivorsEnemyRole.Elite, Vector3.zero, 1);
            Assert.AreEqual(13, owner.EndlessSurgeTier);
            owner.Reset();
            Assert.AreEqual(0, owner.EndlessSurgeTier);
            Assert.IsEmpty(owner.LastEndlessSurgeFeedbackLabel);
        }

        [Test]
        public void JackpotFiltersNormalAndLowRarityChoicesAndCountsOnlySuccessfulDrops()
        {
            var port = new Port();
            var rewards = Selection(port);
            rewards.TriggerRewardJackpot(Upgrade(RunUpgradeRarity.Common), SurvivorsRewardSelectionKind.BossUpgrade);
            rewards.TriggerRewardJackpot(Upgrade(RunUpgradeRarity.Legendary), SurvivorsRewardSelectionKind.None);
            Assert.IsEmpty(port.Drops);
            rewards.TriggerRewardJackpot(Upgrade(RunUpgradeRarity.Rare), SurvivorsRewardSelectionKind.BossUpgrade);
            Assert.AreEqual(1, rewards.RewardJackpotCount);
            Assert.AreEqual(2, rewards.RewardJackpotExperienceGemDropCount);
            Assert.AreEqual(2, rewards.RewardJackpotBloodShardsDropped);
            foreach (var drop in port.Drops) Assert.IsTrue(drop.Attract);
            Assert.AreEqual("pulse:True:True", port.Events[port.Events.Count - 1]);
            port.Success = false;
            rewards.TriggerRewardJackpot(Upgrade(RunUpgradeRarity.Legendary), SurvivorsRewardSelectionKind.BossUpgrade);
            Assert.AreEqual(1, rewards.RewardJackpotCount);
            Assert.AreEqual(2, rewards.RewardJackpotBloodShardsDropped);
        }

        [Test]
        public void DisabledSelectionDamageStillRecordsFeedbackAndDistinctPulseAudio()
        {
            var port = new Port();
            port.Tuning.LevelUpPulseDamage = 0;
            port.Tuning.RewardUpgradeSurgeRadius = 0;
            var rewards = Selection(port);
            rewards.TriggerLevelUpPulse(null);
            rewards.TriggerRewardUpgradeSurge(null, SurvivorsRewardSelectionKind.BossUpgrade);
            CollectionAssert.AreEqual(new[] { "feedback", "pulse:False:False", "feedback", "pulse:True:False" }, port.Events);
            Assert.AreEqual(1, rewards.LevelUpPulseCount);
            Assert.AreEqual(1, rewards.RewardUpgradeSurgeCount);
            Assert.AreEqual(0, rewards.RewardUpgradeSurgeHitCount);
            rewards.Reset();
            Assert.IsEmpty(rewards.LastLevelUpPulseFeedbackLabel);
        }

        private static SurvivorsDraftSelectionRewards Selection(Port port)
        {
            var build = new SurvivorsRunBuildState(port);
            build.Initialize(new RunUpgradeCatalog(new[] { Upgrade(RunUpgradeRarity.Common) }), Array.Empty<SurvivorsRunUpgradeMetadata>(), null, null);
            return new SurvivorsDraftSelectionRewards(build, port);
        }
        private static RunUpgradeDefinition Upgrade(RunUpgradeRarity rarity) => new RunUpgradeDefinition(new RunUpgradeId("test"), rarity, 100, 1,
            new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1d) });
        private sealed class Port : ISurvivorsPickupRewardPort, ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning {
                EndlessSurgeExperienceGemCount = 1, EndlessSurgeBloodShardAmount = 1,
                EndlessSurgePulseRadius = 1, EndlessSurgePulseDamage = 1,
                RewardJackpotExperienceGemBaseCount = 1, RewardJackpotBloodShardBaseAmount = 1 };
            public Vector3 PlayerPosition => Vector3.one;
            public string CurrencyLabel => "Shards";
            public bool Success = true;
            public int Hits;
            public readonly List<(SurvivorsPickupKind Kind, int Amount, bool Attract)> Drops = new List<(SurvivorsPickupKind, int, bool)>();
            public readonly List<string> Events = new List<string>();
            public int DamageNonMajor(Vector3 p, float radius, float damage, string source) { Events.Add(source); return Hits; }
            public bool SpawnPickup(SurvivorsPickupKind kind, Vector3 p, int amount, bool attract) { Drops.Add((kind, amount, attract)); return Success; }
            public void ShowFeedback(string label, Color color) => Events.Add("feedback");
            public void PlayPulse(Vector3 p, int count, bool boss, bool pickupAudio) => Events.Add($"pulse:{boss}:{pickupAudio}");
            public int WeaponCount => 0;
            public bool HasWeapon(string id) => false;
            public void AddWeapon(string id) { }
            public void PassiveAdded(RunUpgradeDefinition upgrade) { }
            public void RecordEvolutionTime() { }
            public void EvolutionAdded(RunUpgradeDefinition upgrade) { }
        }
    }
}
