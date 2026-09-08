using System.Collections.Generic;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsTimedEncounterTests
    {
        [Test]
        public void RecurringWarningUsesEachNewTargetOnceAndSkipsExpiredWindows()
        {
            var port = new EncounterPort { RunFlow = Flow(5f, 5f) };
            var director = new SurvivorsTimedEncounterDirector(port);
            port.RunTime = 2.9f;
            director.TickMajorThreatWarnings();
            Assert.AreEqual(0, director.MajorThreatWarningCount);
            port.RunTime = 3f;
            director.TickMajorThreatWarnings();
            director.TickMajorThreatWarnings();
            Assert.AreEqual(1, director.MajorThreatWarningCount);
            Assert.AreEqual(5f, director.WarningTargetTime);
            Assert.AreEqual("ELITE INCOMING", director.WarningLabel);
            port.RunTime = 5f;
            director.TickRunFlow();
            port.RunTime = 8f;
            director.TickMajorThreatWarnings();
            Assert.AreEqual(2, director.MajorThreatWarningCount);
            Assert.AreEqual(10f, director.WarningTargetTime);
            port.RunTime = 15f;
            director.TickRunFlow();
            director.TickMajorThreatWarnings();
            Assert.AreEqual(2, director.MajorThreatWarningCount);
        }

        [Test]
        public void AuthoredSpawnsConsumeOnceEvenOnFailureAndVictoryFollowsDueThreats()
        {
            var port = new EncounterPort { RunFlow = Flow(5f, 0f, 5f, 5f, 5f), Fail = true, RunTime = 5f };
            var director = new SurvivorsTimedEncounterDirector(port);
            director.TickRunFlow();
            director.TickRunFlow();
            CollectionAssert.AreEqual(new[]
            {
                "timed-elite:Elite", "timed-miniboss:Miniboss", "timed-boss:Boss", "victory"
            }, port.Events);
        }

        [Test]
        public void EndlessFailureRetriesSameRoleAndSuccessSchedulesFromActualSpawnTime()
        {
            var port = new EncounterPort { RunFlow = Flow(0f), IsEndlessPlaying = true };
            port.Tuning.EndlessEliteSpawnIntervalSeconds = 2f;
            port.Tuning.EndlessMinibossSpawnIntervalSeconds = 0f;
            port.Tuning.EndlessBossSpawnIntervalSeconds = 0f;
            var director = new SurvivorsTimedEncounterDirector(port);
            director.ScheduleEndlessThreats(10f);
            port.RunTime = 12f;
            port.Fail = true;
            director.TickRunFlow();
            Assert.AreEqual(12f, director.NextEliteTime);
            Assert.AreEqual(0, director.EndlessThreatSpawnCount);
            port.RunTime = 13f;
            port.Fail = false;
            director.TickRunFlow();
            Assert.AreEqual(15f, director.NextEliteTime);
            Assert.AreEqual(SurvivorsEnemyRole.DreadElite, director.ResolveNextEndlessEliteRole());
            port.RunTime = 15f;
            director.TickRunFlow();
            CollectionAssert.AreEqual(new[]
            {
                "endless-threat:Elite", "endless-threat:Elite", "endless-threat:DreadElite"
            }, port.Events);
            Assert.AreEqual(2, director.EndlessThreatSpawnCount);
            director.Reset();
            Assert.AreEqual(0f, director.NextEliteTime);
            Assert.AreEqual(0, director.EndlessThreatSpawnCount);
            Assert.AreEqual(string.Empty, director.WarningLabel);
        }

        private static SurvivorsRunFlowRuntime Flow(float elite, float repeat = 0f,
            float miniboss = 1000f, float boss = 2000f, float victory = 3000f)
        {
            return new SurvivorsRunFlowRuntime(new SurvivorsRunFlowDefinition(
                60f, 0.1f, 0f, 0, 0f, 0f, 0f, miniboss, default, boss, default,
                victory, firstEliteSpawnTimeSeconds: elite, eliteSpawnIntervalSeconds: repeat,
                firstDreadEliteSpawnTimeSeconds: 0f));
        }

        private sealed class EncounterPort : ISurvivorsTimedEncounterPort
        {
            public SurvivorsRunFlowRuntime RunFlow { get; set; }
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning { MajorThreatWarningLeadSeconds = 2f };
            public float RunTime { get; set; }
            public bool IsEndlessPlaying { get; set; }
            public readonly List<string> Events = new List<string>();
            public bool Fail;
            public bool TrySpawn(SurvivorsEnemyRole role, string source)
            {
                Events.Add(source + ":" + role);
                return !Fail;
            }
            public void EnterVictory() => Events.Add("victory");
            public void ShowWarning(SurvivorsEnemyRole role, string label, float remainingSeconds) { }
        }
    }
}
