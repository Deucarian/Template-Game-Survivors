using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsPickupCollectionTests
    {
        [Test]
        public void ExperienceCollectionKeepsRewardFeedbackRemovalAndDespawnOrder()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            var pickup = host.Add(SurvivorsPickupKind.Experience, 4);
            pickup.transform.position = Vector3.up * 3f;
            host.OnReward = () => Assert.That(host.Collection.ExperiencePickupFeedbackCount, Is.Zero);
            host.OnCollectionFeedback = () =>
            {
                Assert.That(host.Pickups, Does.Contain(pickup));
                Assert.That(host.Collection.ExperiencePickupFeedbackCount, Is.EqualTo(1));
            };
            host.OnDespawn = () => Assert.That(host.Pickups, Has.No.Member(pickup));
            host.Collection.CollectPickup(pickup);
            Assert.That(host.Events, Is.EqualTo(new[] { "first-xp", "gain:4", "combo:13", "collect:Experience", "despawn:1" }));
            Assert.That(host.LastBurst, Is.EqualTo(10));
            Assert.That(host.LastPosition, Is.EqualTo(Vector3.up * 3f));
        }

        [TestCase(SurvivorsPickupKind.Health, "health:3", 22)]
        [TestCase(SurvivorsPickupKind.BloodShard, "shards:3", 18)]
        public void HealthAndShardCollectionUseTheirRewardPortBeforeFeedback(SurvivorsPickupKind kind, string first, int burst)
        {
            using var host = new SurvivorsPickupRewardTestHost();
            var pickup = host.Add(kind, 3);
            host.Collection.CollectPickup(pickup);
            Assert.That(host.Events, Is.EqualTo(new[] { first, "collect:" + kind, "despawn:1" }));
            Assert.That(host.LastBurst, Is.EqualTo(burst));
            Assert.That(host.Collection.ExperiencePickupFeedbackCount, Is.Zero);
            Assert.That(host.Collection.BloodShardPickupCollectedCount, Is.EqualTo(kind == SurvivorsPickupKind.BloodShard ? 1 : 0));
            Assert.That(host.Collection.BloodShardsCollectedFromPickups, Is.EqualTo(kind == SurvivorsPickupKind.BloodShard ? 3 : 0));
        }

        [Test]
        public void MagnetCollectionRecallsExperienceBeforeCollectionFeedbackAndDoesNotGrantExperience()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            var magnet = host.Add(SurvivorsPickupKind.Magnet);
            var gem = host.Add(SurvivorsPickupKind.Experience);
            host.Collection.CollectPickup(magnet);
            Assert.That(gem.IsGlobalRecallActive, Is.True);
            Assert.That(host.Events, Is.EqualTo(new[] { "recall", "collect:Magnet", "despawn:1" }));
            Assert.That(host.LastBurst, Is.EqualTo(28));
            Assert.That(host.Collection.MagnetRecallCount, Is.EqualTo(1));
            Assert.That(host.Collection.ExperiencePickupFeedbackCount, Is.Zero);
        }

        [Test]
        public void MissingPickupDoesNothingAndUnpooledPickupStillCollectsWithoutDespawn()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Collection.CollectPickup(null);
            Assert.That(host.Events, Is.Empty);
            var pickup = host.Add(SurvivorsPickupKind.BloodShard, 0, false);
            host.Collection.CollectPickup(pickup);
            Assert.That(host.Events, Is.EqualTo(new[] { "shards:1", "collect:BloodShard" }));
            Assert.That(host.Collection.BloodShardsCollectedFromPickups, Is.EqualTo(1));
            Assert.That(host.Pickups, Is.Empty);
        }

        [Test]
        public void RecallReadsLiveMembersAndRetainsInactiveExperienceCountingAndBurstClamp()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            var gem = host.Add(SurvivorsPickupKind.Experience);
            var inactive = host.Add(SurvivorsPickupKind.Experience);
            inactive.ResetForWorldSpawn();
            host.Add(SurvivorsPickupKind.Health);
            host.Pickups.Add(null);
            Assert.That(host.Collection.StartMagnetRecall(), Is.EqualTo(2));
            Assert.That(gem.IsGlobalRecallActive, Is.True);
            Assert.That(inactive.IsGlobalRecallActive, Is.False);
            Assert.That(host.LastBurst, Is.EqualTo(18));
            for (int i = 0; i < 25; i++) host.Add(SurvivorsPickupKind.Experience);
            Assert.That(host.Collection.StartMagnetRecall(), Is.EqualTo(27));
            Assert.That(host.LastBurst, Is.EqualTo(72));
            host.Pickups.Clear();
            Assert.That(host.Collection.StartMagnetRecall(), Is.Zero);
            Assert.That(host.Collection.MagnetRecallCount, Is.EqualTo(3));
            Assert.That(host.Collection.MagnetRecallFeedbackCount, Is.EqualTo(2));
        }

        [Test]
        public void IntervalAndSchedulingReadCurrentTuningWithoutRestartingAnActiveCountdown()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Tuning.PickupMagnetPulseBaseIntervalSeconds = 10f;
            host.Tuning.PickupMagnetPulseMinimumIntervalSeconds = 2f;
            Assert.That(host.Collection.ResolvePickupMagnetPulseIntervalSeconds(), Is.EqualTo(-1f));
            host.PickupMagnetPulseIntervalReductionBonus = 3f;
            host.Collection.ScheduleMagnetPulse();
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(7f));
            host.PickupMagnetPulseIntervalReductionBonus = 20f;
            host.Collection.ScheduleMagnetPulse();
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(7f));
            host.Collection.ResetPulseSchedule();
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(2f));
            host.Tuning = new SurvivorsTemplateTuning { PickupMagnetPulseBaseIntervalSeconds = -1f, PickupMagnetPulseMinimumIntervalSeconds = -2f };
            Assert.That(host.Collection.ResolvePickupMagnetPulseIntervalSeconds(), Is.EqualTo(1f));
        }

        [Test]
        public void PulseClampsNegativeDeltaAndFiresOnceWithoutCatchingUpALargeDelta()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Tuning.PickupMagnetPulseBaseIntervalSeconds = 5f;
            host.Tuning.PickupMagnetPulseMinimumIntervalSeconds = 1f;
            host.PickupMagnetPulseIntervalReductionBonus = 1f;
            host.Add(SurvivorsPickupKind.Experience);
            host.Collection.ResetPulseSchedule();
            host.Collection.TickPickupMagnetPulse(-2f);
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(4f));
            host.Collection.TickPickupMagnetPulse(3f);
            Assert.That(host.Collection.MagnetRecallCount, Is.Zero);
            host.Collection.TickPickupMagnetPulse(20f);
            Assert.That(host.Collection.MagnetRecallCount, Is.EqualTo(1));
            Assert.That(host.Collection.MagnetPulseActivationCount, Is.EqualTo(1));
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(4f));
            Assert.That(host.Events, Is.EqualTo(new[] { "recall", "banner" }));
            Assert.That(host.Collection.LastMagnetPulseFeedbackLabel, Is.EqualTo("Vacuum Pulse: 1 XP gems pulled"));
            host.Collection.ResetDiagnostics();
            Assert.That(host.Collection.MagnetRecallCount, Is.Zero);
            Assert.That(host.Collection.MagnetPulseActivationCount, Is.Zero);
            Assert.That(host.Collection.LastMagnetPulseFeedbackLabel, Is.Empty);
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(4f));
        }

        [Test]
        public void EmptyDisabledAndNonExperienceListsPreserveDistinctPulseResetBehavior()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Tuning.PickupMagnetPulseBaseIntervalSeconds = 3f;
            host.Tuning.PickupMagnetPulseMinimumIntervalSeconds = 1f;
            host.PickupMagnetPulseIntervalReductionBonus = 1f;
            host.Collection.TickPickupMagnetPulse(100f);
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(2f));
            Assert.That(host.Collection.MagnetRecallCount, Is.Zero);
            host.Add(SurvivorsPickupKind.Health);
            host.Collection.TickPickupMagnetPulse(2f);
            Assert.That(host.Collection.MagnetRecallCount, Is.EqualTo(1));
            Assert.That(host.Collection.MagnetPulseActivationCount, Is.Zero);
            Assert.That(host.Events, Is.Empty);
            host.PickupMagnetPulseIntervalReductionBonus = 0f;
            host.Collection.TickPickupMagnetPulse(100f);
            Assert.That(host.Collection.PulseSecondsRemaining, Is.EqualTo(-1f));
            Assert.That(host.Collection.MagnetRecallCount, Is.EqualTo(1));
        }

        [Test]
        public void AttractionFeedbackIgnoresMagnetsAndActiveCacheCountTracksActorResetAndMembership()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Collection.RecordPickupAttractionFeedback(SurvivorsPickupKind.Magnet, Vector3.zero);
            Assert.That(host.Events, Is.Empty);
            host.Collection.RecordPickupAttractionFeedback(SurvivorsPickupKind.Health, Vector3.one);
            Assert.That(host.Collection.PickupAttractionFeedbackCount, Is.EqualTo(1));
            Assert.That(host.LastPosition, Is.EqualTo(Vector3.one));
            var gem = host.Add(SurvivorsPickupKind.Experience);
            Assert.That(host.Cache.StartMajorRewardCacheAttraction(gem), Is.True);
            Assert.That(host.Collection.ActiveMajorRewardCacheAttractedPickupCount, Is.EqualTo(1));
            gem.ResetForWorldSpawn();
            Assert.That(host.Cache.StartMajorRewardCacheAttraction(gem), Is.False);
            Assert.That(host.Collection.ActiveMajorRewardCacheAttractedPickupCount, Is.Zero);
            Assert.That(host.Cache.StartMajorRewardCacheAttraction(null), Is.False);
        }
    }
}
