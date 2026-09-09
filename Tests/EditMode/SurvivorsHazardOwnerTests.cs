using System;
using System.Collections.Generic;
using Deucarian.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsHazardOwnerTests
    {
        [Test]
        public void NovaUsesHorizontalBodyRadiiAndCapturesTargetsBeforeOrderedDamage()
        {
            var first = new Target { Position = new Vector3(2, 20, 0), Radius = 1 };
            var second = new Target { Position = Vector3.zero };
            var outside = new Target { Position = new Vector3(2.01f, 0, 0), Radius = 1 };
            var targets = new List<ISurvivorsDeathNovaTarget> { first, second, outside };
            int pulse = 0;
            first.OnDamage = () => { second.IsAlive = false; targets.Clear(); };
            var nova = new SurvivorsDeathNova(targets, () => 8, () => 1, (p, n) => pulse = n);
            nova.TryTriggerDeathNova(Vector3.zero, "weapon", true);
            Assert.AreEqual(30, pulse, "Feedback counts candidates before later liveness changes.");
            Assert.AreEqual(1, first.Hits);
            Assert.AreEqual(0, second.Hits);
            Assert.AreEqual(0, outside.Hits);
            Assert.AreEqual(1, nova.DeathNovaHitCount);
        }

        [Test]
        public void NovaRejectsRecursiveSourcesDisabledAugmentsAndZeroDamage()
        {
            float damage = 4;
            var target = new Target();
            var nova = new SurvivorsDeathNova(new[] { target }, () => damage, () => 1, (p, n) => { });
            nova.TryTriggerDeathNova(Vector3.zero, "survivors.augment.death-nova", true);
            nova.TryTriggerDeathNova(Vector3.zero, "weapon", false);
            damage = 0;
            nova.TryTriggerDeathNova(Vector3.zero, "weapon", true);
            Assert.AreEqual(0, nova.DeathNovaTriggerCount);
            damage = 1;
            nova.TryTriggerDeathNova(Vector3.zero, "weapon", true);
            Assert.AreEqual(1, nova.DeathNovaTriggerCount);
            nova.Reset();
            Assert.AreEqual(0, nova.DeathNovaHitCount);
        }

        [Test]
        public void DestroyedUnityNovaTargetIsIgnoredThroughItsExplicitLivenessAdapter()
        {
            var gameObject = new GameObject("nova-target");
            var actor = gameObject.AddComponent<SurvivorsEnemyActor>();
            actor.Initialize(null, BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm, new SurvivorsTemplateTuning()));
            var nova = new SurvivorsDeathNova(new List<SurvivorsEnemyActor> { actor }, () => 1, () => 1, (p, n) => { });
            UnityEngine.Object.DestroyImmediate(gameObject);
            Assert.DoesNotThrow(() => nova.TryTriggerDeathNova(Vector3.zero, "weapon", true));
            Assert.AreEqual(0, nova.DeathNovaTriggerCount);
        }

        [Test]
        public void SnareWindowExpiresButCooldownStillCountsSnareFeedbackWithoutBuildingChain()
        {
            var port = new Port();
            var hazards = new SurvivorsPayloadHazardRewards(port);
            var weapon = BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(port.Tuning)[0];
            hazards.RecordPayloadHazardSnare("a", weapon, Vector3.zero);
            hazards.TickPayloadHazardChain(0.5f);
            hazards.RecordPayloadHazardSnare("b", weapon, Vector3.zero);
            Assert.AreEqual(0, hazards.PayloadHazardChainActivationCount);
            hazards.RecordPayloadHazardSnare("c", weapon, Vector3.zero);
            Assert.AreEqual(1, hazards.PayloadHazardChainActivationCount);
            hazards.RecordPayloadHazardSnare("d", weapon, Vector3.zero);
            hazards.RecordPayloadHazardSnare("e", weapon, Vector3.zero);
            Assert.AreEqual(5, hazards.PayloadHazardSnareCount);
            Assert.AreEqual(1, hazards.PayloadHazardChainActivationCount);
            hazards.TickPayloadHazardChain(1);
            hazards.RecordPayloadHazardSnare("f", weapon, Vector3.zero);
            Assert.AreEqual(1, hazards.PayloadHazardChainActivationCount);
        }

        [Test]
        public void FailedHazardDropsKeepActivationAndDisabledPulseStillShowsFeedback()
        {
            var port = new Port { Success = false };
            port.Tuning.PayloadHazardChainSnareThreshold = 0;
            port.Tuning.PayloadHazardChainPulseDamage = 0;
            var hazards = new SurvivorsPayloadHazardRewards(port);
            hazards.RecordPayloadHazardSnare("a", BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(port.Tuning)[0], Vector3.zero);
            Assert.AreEqual(1, hazards.PayloadHazardChainActivationCount);
            Assert.AreEqual(0, hazards.PayloadHazardChainPulseHitCount);
            Assert.AreEqual(0, hazards.PayloadHazardChainExperienceGemDropCount);
            CollectionAssert.AreEqual(new[] { "drop", "drop", "feedback", "pulse" }, port.Events);
            Assert.That(hazards.LastPayloadHazardChainFeedbackLabel, Does.Contain("+0 XP"));
            hazards.RecordPayloadHazardTick();
            hazards.Reset();
            Assert.AreEqual(0, hazards.PayloadHazardTickCount);
            Assert.IsEmpty(hazards.LastPayloadHazardChainFeedbackLabel);
        }

        private sealed class Target : ISurvivorsDeathNovaTarget
        {
            public bool IsAlive { get; set; } = true;
            public Vector3 Position { get; set; }
            public float Radius { get; set; }
            public int Hits;
            public Action OnDamage;
            public DamageResult ApplyDamage(float amount, string source)
            {
                Hits++; OnDamage?.Invoke();
                return new DamageResult(CombatStatus.Success, default, Array.Empty<DamageComponentResult>(), 40d, 30d, 20d, amount, 0d, default, default, Array.Empty<StatusApplicationResult>());
            }
        }
        private sealed class Port : ISurvivorsPickupRewardPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning {
                PayloadHazardChainSnareThreshold = 2, PayloadHazardChainWindowSeconds = 0.5f,
                PayloadHazardChainCooldownSeconds = 1, PayloadHazardChainExperienceGemCount = 2 };
            public Vector3 PlayerPosition => Vector3.zero;
            public string CurrencyLabel => "Shards";
            public bool Success = true;
            public readonly List<string> Events = new List<string>();
            public int DamageNonMajor(Vector3 p, float radius, float damage, string source) { Events.Add("damage"); return 1; }
            public bool SpawnPickup(SurvivorsPickupKind kind, Vector3 p, int amount, bool attract) { Events.Add("drop"); return Success; }
            public void ShowFeedback(string label, Color color) => Events.Add("feedback");
            public void PlayPulse(Vector3 p, int count, bool boss, bool pickupAudio) => Events.Add("pulse");
        }
    }
}
