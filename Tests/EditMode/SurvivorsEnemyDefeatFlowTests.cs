using System.Collections.Generic;
using Deucarian.WorldSpawning;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsEnemyDefeatFlowTests
    {
        [Test]
        public void DefeatRemovesBeforePayoffsAndClearsEncountersAfterRoleRewards()
        {
            var port = new Port { Clears = new SurvivorsEncounterClears(true, true, true) };
            var flow = new SurvivorsEnemyDefeatFlow(port, new SurvivorsRunSession());
            port.Flow = flow;
            flow.HandleEnemyKilled(new Target { Role = SurvivorsEnemyRole.Elite, ExperienceReward = 0 }, "test", true);
            CollectionAssert.AreEqual(new[] { "metric:1", "release", "xp:1", "streak", "death", "nova:True", "major", "audio:34", "grant:1", "draft", "horde", "cache", "shrine" }, port.Events);
            Assert.AreEqual(1, flow.KilledCount);
            Assert.AreEqual(1, flow.EliteKilledCount);
        }

        [TestCase(SurvivorsEnemyRole.Miniboss, false, "relic")]
        [TestCase(SurvivorsEnemyRole.Miniboss, true, "draft")]
        [TestCase(SurvivorsEnemyRole.Boss, false, "victory")]
        [TestCase(SurvivorsEnemyRole.Boss, true, "draft")]
        public void MajorDraftFailureUsesRoleSpecificContinuation(SurvivorsEnemyRole role, bool opens, string last)
        {
            var port = new Port { DraftOpens = opens };
            var flow = new SurvivorsEnemyDefeatFlow(port, new SurvivorsRunSession()); port.Flow = flow;
            flow.HandleEnemyKilled(new Target { Role = role }, "test", false);
            Assert.AreEqual(last, port.Events[port.Events.Count - 1]);
            Assert.AreEqual(role == SurvivorsEnemyRole.Boss ? 1 : 0, flow.BossKilledCount);
            Assert.AreEqual(role == SurvivorsEnemyRole.Miniboss ? 1 : 0, flow.MinibossKilledCount);
        }

        [Test]
        public void EndlessBossFailureDoesNotWinAgainAndNullDefeatsDoNothing()
        {
            var session = new SurvivorsRunSession(); session.Win();
            var port = new Port();
            var flow = new SurvivorsEnemyDefeatFlow(port, session); port.Flow = flow;
            flow.HandleEnemyKilled(null, "test", true);
            Assert.AreEqual(0, flow.KilledCount);
            flow.HandleEnemyKilled(new Target { Role = SurvivorsEnemyRole.Boss }, "test", false);
            Assert.That(port.Events, Does.Not.Contain("victory"));
            flow.Reset();
            Assert.AreEqual(0, flow.KilledCount);
            Assert.AreEqual(0, flow.BossKilledCount);
        }

        [Test]
        public void SplitterReadsNameAfterPoolResetWhileRolePositionAndRewardStayCaptured()
        {
            var port = new Port { ResetOnRelease = true };
            var flow = new SurvivorsEnemyDefeatFlow(port, new SurvivorsRunSession()); port.Flow = flow;
            var target = new Target { Role = SurvivorsEnemyRole.Splitter, DisplayName = "Authored Splitter", ExperienceReward = 8 };
            flow.HandleEnemyKilled(target, "test", true);
            Assert.AreEqual("split:", port.Events[port.Events.Count - 1]);
            Assert.That(port.Events, Does.Contain("xp:8"));
            Assert.That(port.Events, Does.Contain("audio:34"));
            Assert.That(port.Events, Does.Not.Contain("major"));
        }

        private sealed class Target : ISurvivorsDefeatTarget
        {
            public Vector3 Position => Vector3.one;
            public SurvivorsEnemyRole Role { get; set; }
            public int ExperienceReward { get; set; } = 2;
            public float Radius => 1;
            public string DisplayName { get; set; } = "Enemy";
            public SpawnInstanceId InstanceId => default;
        }
        private sealed class Port : ISurvivorsEnemyDefeatPort
        {
            public readonly List<string> Events = new List<string>();
            public SurvivorsEnemyDefeatFlow Flow;
            public bool DraftOpens, ResetOnRelease;
            public SurvivorsEncounterClears Clears;
            public void RecordKillMetric(SurvivorsEnemyRole role) => Events.Add("metric:" + Flow.KilledCount);
            public SurvivorsEncounterClears ReleaseKilledEnemy(ISurvivorsDefeatTarget target)
            {
                Events.Add("release");
                if (ResetOnRelease) { ((Target)target).DisplayName = ""; ((Target)target).Role = SurvivorsEnemyRole.Swarm; }
                return Clears;
            }
            public void SpawnExperience(Vector3 p, int amount) => Events.Add("xp:" + amount);
            public void RegisterStreak(Vector3 p) => Events.Add("streak");
            public void ShowDeath(Vector3 p, SurvivorsEnemyRole role, float radius) => Events.Add("death");
            public void TriggerDeathNova(Vector3 p, string source, bool augments) => Events.Add("nova:" + augments);
            public void SpawnMajorRewards(Vector3 p, SurvivorsEnemyRole role, float radius, int xp) => Events.Add("major");
            public void PlayDeath(Vector3 p, int burst) => Events.Add("audio:" + burst);
            public void SpawnSplitterChildren(Vector3 p, string name) => Events.Add("split:" + name);
            public void GrantMajorEnemyReward(SurvivorsEnemyRole role) => Events.Add("grant:" + (Flow.EliteKilledCount + Flow.MinibossKilledCount + Flow.BossKilledCount));
            public bool OpenUpgradeRewardDraft(SurvivorsEnemyRole role) { Events.Add("draft"); return DraftOpens; }
            public void OpenBossRelicDraft() => Events.Add("relic");
            public void EnterVictory() => Events.Add("victory");
            public void RewardHordeClear(Vector3 p) => Events.Add("horde");
            public void RewardCacheClear(Vector3 p) => Events.Add("cache");
            public void RewardShrineClear(Vector3 p) => Events.Add("shrine");
        }
    }
}
