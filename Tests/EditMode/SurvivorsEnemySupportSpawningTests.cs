using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsEnemySupportSpawningTests
    {
        [Test]
        public void SplitterSeedsUseEachSuccessfulChildCountAndFeedbackCountsOnlySuccess()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            host.FailSpawn = index => index == 2;
            host.Support.SpawnSplitterChildren(Vector3.one, "");
            Assert.That(host.Requests.Select(request => request.seed), Is.EqualTo(new long[] { 1001, 1056, 1058 }));
            Assert.That(host.Requests.All(request => request.role == SurvivorsEnemyRole.Swarm && request.source == "splitter-children"), Is.True);
            Assert.That(host.Support.SplitterChildSpawnCount, Is.EqualTo(2));
            Assert.That(host.Support.SplitterSplitFeedbackCount, Is.EqualTo(1));
            Assert.That(host.LastLabel, Is.EqualTo("Splitter: +2 fragments"));
            Assert.That(host.LastBurst, Is.EqualTo(26));
            Assert.That(host.Events, Is.EqualTo(new[] { "spawn", "spawn", "spawn", "banner", "support-pulse" }));
        }

        [TestCase(SurvivorsRunState.GameOver, 0)]
        [TestCase(SurvivorsRunState.Victory, 0)]
        [TestCase(SurvivorsRunState.LevelUp, 3)]
        [TestCase(SurvivorsRunState.Booting, 3)]
        public void SplitterBlocksOnlyFinishedRunsAndDoesNotApplyAnAliveCapacity(SurvivorsRunState state, int expected)
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            host.State = state; host.MaximumAlive = 0; host.EnemyCount = 100;
            host.Support.SpawnSplitterChildren(Vector3.zero, "test");
            Assert.That(host.Requests.Count, Is.EqualTo(expected));
        }

        [Test]
        public void SplitterClampsRequestedCountAndUsesFallbackRadiusWithNoFailureFeedback()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            host.Tuning.SplitterChildCount = 100;
            host.Tuning.SplitterChildSpawnRadius = 0f;
            host.Tuning.EnemyRadius = 0.1f;
            host.FailSpawn = _ => true;
            host.Support.SpawnSplitterChildren(Vector3.zero, "test");
            Assert.That(host.Requests.Count, Is.EqualTo(8));
            Assert.That(host.Requests.All(request => request.minimum == 0.65f), Is.True);
            Assert.That(host.Support.SplitterSplitFeedbackCount, Is.Zero);
            host.Tuning.SplitterChildCount = -10;
            host.Support.SpawnSplitterChildren(Vector3.zero, "test");
            Assert.That(host.Requests.Count, Is.EqualTo(9));
        }

        [Test]
        public void SummonerSnapshotsSuccessfulOffsetWhileReadingEachCurrentSpawnSequence()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            var summoner = host.Add(SurvivorsEnemyRole.Summoner);
            host.FailSpawn = index => index == 2;
            Assert.That(host.Support.SpawnSummonerSupport(summoner), Is.EqualTo(3));
            Assert.That(host.Requests.Select(request => request.seed), Is.EqualTo(new long[] { 1041, 1043, 1045, 1047 }));
            Assert.That(host.Requests.Select(request => request.role), Is.EqualTo(new[] { SurvivorsEnemyRole.Swarm, SurvivorsEnemyRole.Swarm, SurvivorsEnemyRole.Swarm, SurvivorsEnemyRole.Runner }));
            Assert.That(host.Support.SummonerSupportSpawnCount, Is.EqualTo(3));
            Assert.That(host.Support.SpawnSummonerSupport(summoner), Is.EqualTo(4));
            Assert.That(host.Requests.Skip(4).Select(request => request.seed), Is.EqualTo(new long[] { 1222, 1224, 1226, 1228 }));
            Assert.That(host.Requests[4].role, Is.EqualTo(SurvivorsEnemyRole.Runner));
            Assert.That(host.Support.SummonerSupportSpawnCount, Is.EqualTo(7));
            Assert.That(host.Support.SummonerSupportFeedbackCount, Is.EqualTo(2));
        }

        [Test]
        public void SummonerCapacityAndLivenessGuardsPreserveDraftSupportAndZeroSpawnDiagnostics()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            var summoner = host.Add(SurvivorsEnemyRole.Summoner);
            Assert.That(host.Support.SpawnSummonerSupport(null), Is.Zero);
            Assert.That(host.Support.SpawnSummonerSupport(host.Add(SurvivorsEnemyRole.Summoner, 0f)), Is.Zero);
            host.MaximumAlive = 3; host.Tuning.SummonerSupportExtraAliveAllowance = 1;
            host.State = SurvivorsRunState.LevelUp;
            Assert.That(host.Support.SpawnSummonerSupport(summoner), Is.EqualTo(2));
            Assert.That(host.Requests.Count, Is.EqualTo(2));
            Assert.That(host.Support.SpawnSummonerSupport(summoner), Is.Zero);
            Assert.That(host.Support.SummonerSupportFeedbackCount, Is.EqualTo(1));
            host.State = SurvivorsRunState.Victory;
            host.MaximumAlive = 100;
            Assert.That(host.Support.SpawnSummonerSupport(summoner), Is.Zero);
        }

        [Test]
        public void EnrageSupportUsesCapacityAndPreIncrementCountButDoesNotOwnMajorDiagnostics()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            var enemy = host.Add(SurvivorsEnemyRole.Boss);
            host.MaximumAlive = 2; host.Tuning.MajorThreatEnrageExtraAliveAllowance = 1;
            Assert.That(host.Support.SpawnMajorThreatEnrageSupport(enemy, 2), Is.EqualTo(2));
            Assert.That(host.Requests.Select(request => request.seed), Is.EqualTo(new long[] { 773, 775 }));
            Assert.That(host.Requests.Select(request => request.role), Is.EqualTo(new[] { SurvivorsEnemyRole.Bruiser, SurvivorsEnemyRole.Swarm }));
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.Zero);
            Assert.That(host.Support.SummonerSupportSpawnCount, Is.Zero);
            Assert.That(host.Support.SplitterChildSpawnCount, Is.Zero);
        }

        [TestCase(SurvivorsEnemyRole.Boss, 0, SurvivorsEnemyRole.Bruiser)]
        [TestCase(SurvivorsEnemyRole.Boss, 5, SurvivorsEnemyRole.Splitter)]
        [TestCase(SurvivorsEnemyRole.Boss, 12, SurvivorsEnemyRole.Bruiser)]
        [TestCase(SurvivorsEnemyRole.Miniboss, 6, SurvivorsEnemyRole.Spitter)]
        [TestCase(SurvivorsEnemyRole.DreadElite, 4, SurvivorsEnemyRole.Spitter)]
        [TestCase(SurvivorsEnemyRole.Elite, 4, SurvivorsEnemyRole.Runner)]
        public void EnrageSupportRolePrecedenceIsDeterministic(SurvivorsEnemyRole role, int index, SurvivorsEnemyRole expected)
            => Assert.That(SurvivorsEnemySupportSpawning.ResolveMajorThreatEnrageSupportRole(role, index), Is.EqualTo(expected));
    }
}
