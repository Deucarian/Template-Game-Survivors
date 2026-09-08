using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsMajorRewardPickupCacheTests
    {
        [TestCase(SurvivorsEnemyRole.Elite, 5, 18, 1, 0)]
        [TestCase(SurvivorsEnemyRole.DreadElite, 7, 24, 2, 3)]
        [TestCase(SurvivorsEnemyRole.Miniboss, 8, 32, 2, 4)]
        [TestCase(SurvivorsEnemyRole.Boss, 12, 55, 2, 6)]
        public void RoleRewardsPreserveRoundedExperienceSpecialOrderAndAttraction(
            SurvivorsEnemyRole role, int gems, int perGem, int specials, int shards)
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Tuning.EnemyExperienceReward = 10;
            host.Tuning.BloodShardPickupAmount = 2;
            host.Cache.SpawnMajorRewardPickupCache(Vector3.one, role, 1f);
            Assert.That(host.Requests.Count, Is.EqualTo(gems + specials));
            Assert.That(host.Requests.Take(gems).All(request => request.kind == SurvivorsPickupKind.Experience && request.amount == perGem), Is.True);
            Assert.That(host.Requests[gems].kind, Is.EqualTo(SurvivorsPickupKind.Magnet));
            if (shards > 0)
            {
                Assert.That(host.Requests[gems + 1].kind, Is.EqualTo(SurvivorsPickupKind.BloodShard));
                Assert.That(host.Requests[gems + 1].amount, Is.EqualTo(shards));
            }
            Assert.That(host.Cache.MajorRewardCacheDropCount, Is.EqualTo(1));
            Assert.That(host.Cache.MajorRewardCacheExperienceGemDropCount, Is.EqualTo(gems));
            Assert.That(host.Cache.MajorRewardCacheSpecialDropCount, Is.EqualTo(specials));
            Assert.That(host.Cache.MajorRewardCacheAttractedPickupCount, Is.EqualTo(gems + specials));
            Assert.That(host.Collection.ActiveMajorRewardCacheAttractedPickupCount, Is.EqualTo(gems + specials));
            Assert.That(host.LastLabel, Is.EqualTo($"{role}: Cache +{gems * perGem} XP + {specials} special pull x{gems + specials}"));
            Assert.That(host.LastColor, Is.EqualTo(Color.magenta));
        }

        [Test]
        public void OrdinaryEnemyProducesNoCacheAndAllFailedSpawnsProduceNoSuccessFeedback()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Cache.SpawnMajorRewardPickupCache(Vector3.zero, SurvivorsEnemyRole.Swarm, 2f);
            Assert.That(host.Requests, Is.Empty);
            host.FailSpawn = _ => true;
            host.Cache.SpawnMajorRewardPickupCache(Vector3.zero, SurvivorsEnemyRole.Boss, 2f);
            Assert.That(host.Requests.Count, Is.EqualTo(14));
            Assert.That(host.Cache.MajorRewardCacheDropCount, Is.Zero);
            Assert.That(host.Cache.MajorRewardCacheExperienceGemDropCount, Is.Zero);
            Assert.That(host.Cache.MajorRewardCacheSpecialDropCount, Is.Zero);
            Assert.That(host.Events, Is.Empty);
        }

        [Test]
        public void PartialSpawnFailureAndInactiveAttractionCountOnlyTheirActualOutcomes()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Tuning.EnemyExperienceReward = 10;
            host.FailSpawn = index => index % 2 == 0;
            host.InactiveSpawn = index => index == 3;
            host.Cache.SpawnMajorRewardPickupCache(Vector3.zero, SurvivorsEnemyRole.Elite, 1f);
            Assert.That(host.Requests.Count, Is.EqualTo(6));
            Assert.That(host.Cache.MajorRewardCacheDropCount, Is.EqualTo(1));
            Assert.That(host.Cache.MajorRewardCacheExperienceGemDropCount, Is.EqualTo(3));
            Assert.That(host.Cache.MajorRewardCacheSpecialDropCount, Is.Zero);
            Assert.That(host.Cache.MajorRewardCacheAttractedPickupCount, Is.EqualTo(2));
            Assert.That(host.LastLabel, Is.EqualTo("Elite: Cache +54 XP pull x2"));
        }

        [Test]
        public void RingAlternatesRadiusAndKeepsCenterHeightWhileSpecialOffsetsStayDistinct()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            var center = new Vector3(3f, 7f, -2f);
            host.Cache.SpawnMajorRewardPickupCache(center, SurvivorsEnemyRole.Elite, 0.1f);
            var first = host.Requests[0].position - center;
            var second = host.Requests[1].position - center;
            Assert.That(first.x, Is.EqualTo(0.8770742f).Within(0.00001f));
            Assert.That(first.z, Is.EqualTo(0.2018437f).Within(0.00001f));
            Assert.That(first.magnitude, Is.EqualTo(0.9f).Within(0.00001f));
            Assert.That(second.magnitude, Is.EqualTo(1.08f).Within(0.00001f));
            Assert.That(host.Requests.All(request => request.position.y == center.y), Is.True);
            Assert.That(Vector3.Distance(host.Requests[5].position, center + new Vector3(0.558f, 0f, -0.324f)), Is.LessThan(0.00001f));
        }

        [Test]
        public void LiveTuningAndEscalationUseCurrentValuesAndResetDoesNotClearActorAttraction()
        {
            using var host = new SurvivorsPickupRewardTestHost();
            host.Tuning.EnemyExperienceReward = 10;
            host.RunEscalationLevel = 2;
            Assert.That(host.Cache.ResolveMajorRewardCacheExperiencePerGem(SurvivorsEnemyRole.Boss), Is.EqualTo(66));
            host.Tuning = new SurvivorsTemplateTuning { EnemyExperienceReward = -10, BloodShardPickupAmount = -5 };
            Assert.That(host.Cache.ResolveMajorRewardCacheExperiencePerGem(SurvivorsEnemyRole.Boss), Is.EqualTo(1));
            Assert.That(host.Cache.ResolveMajorRewardCacheBloodShardAmount(SurvivorsEnemyRole.Boss), Is.EqualTo(5));
            host.Cache.SpawnMajorRewardPickupCache(Vector3.zero, SurvivorsEnemyRole.Boss, 1f);
            host.Cache.ResetDiagnostics();
            Assert.That(host.Cache.MajorRewardCacheDropCount, Is.Zero);
            Assert.That(host.Cache.MajorRewardCacheAttractedPickupCount, Is.Zero);
            Assert.That(host.Cache.LastMajorRewardCacheFeedbackLabel, Is.Empty);
            Assert.That(host.Collection.ActiveMajorRewardCacheAttractedPickupCount, Is.EqualTo(14));
        }
    }
}
