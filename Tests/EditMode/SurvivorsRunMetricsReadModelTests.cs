using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRunMetricsReadModelTests
    {
        [TestCase(false, "Run not started")]
        [TestCase(true, "Run mode selection open")]
        public void StoppedRunReadsHeadersAndMenuButNeverActiveObservations(bool menu, string lastLine)
        {
            var port = new MetricsPort { Menu = menu, RejectActive = true };
            var model = new SurvivorsRunMetricsReadModel(port);
            Assert.That(model.Describe(), Is.EqualTo(new[]
            {
                "Mode Custom Mode (Sprint Run)", "Target 02:00 - boss 01:00 - victory 03:00", lastLine
            }));
            Assert.That(port.Events, Is.EqualTo(new[] { "mode", "pacing", "target", "boss", "victory", "started", "menu" }));
        }

        [Test]
        public void ActiveRunDoesNotReadModeSelectionAndPreservesLazyCapturePhases()
        {
            var port = new MetricsPort();
            var model = new SurvivorsRunMetricsReadModel(port);
            IReadOnlyList<string> lines = model.Describe();
            port.StartedValue = true;
            port.RejectMenu = true;
            port.Events.Clear();
            port.OnRead = name =>
            {
                if (name == "mode") Assert.That(lines, Is.Empty, "Clear precedes the first header observation.");
                if (name == "pacing") Assert.That(lines.Count, Is.Zero);
                if (name == "target" || name == "boss" || name == "victory") Assert.That(lines.Count, Is.EqualTo(1));
                if (name == "started" || name == "active" || name == "runtime" || name == "state") Assert.That(lines.Count, Is.EqualTo(2));
                if (name == "drafts") Assert.That(lines.Count, Is.EqualTo(15));
                if (name == "combat") Assert.That(lines.Count, Is.EqualTo(16));
                if (name == "pickups") Assert.That(lines.Count, Is.EqualTo(17));
            };
            Assert.That(model.Describe(), Is.SameAs(lines));
            var expected = new List<string> { "mode", "pacing", "target", "boss", "victory", "started", "active", "runtime", "state" };
            for (int i = 0; i < 16; i++) expected.Add("telemetry");
            expected.AddRange(new[] { "drafts", "combat", "pickups" });
            Assert.That(port.Events, Is.EqualTo(expected));
        }

        [Test]
        public void AllElevenMetricLabelsAndFiveCheckpointLevelsRetainTheirOrder()
        {
            var port = new MetricsPort { StartedValue = true };
            port.Data.Reset();
            for (int i = 0; i < 11; i++) port.Data.Record((SurvivorsRunMetric)i, i + 1f);
            for (int i = 1; i <= 5; i++) port.Data.RecordLevelCheckpoints(i * 60f, i + 1);
            var model = new SurvivorsRunMetricsReadModel(port);
            IReadOnlyList<string> lines = model.Describe();
            Assert.That(lines.Count, Is.EqualTo(18));
            Assert.That(lines[2], Is.EqualTo("Runtime 01:15 - state Playing"));
            string[] expected =
            {
                "First kill: 00:01", "First XP pickup: 00:02", "First level-up draft: 00:03",
                "First elite spawn: 00:04", "First elite kill: 00:05", "First miniboss spawn: 00:06",
                "First miniboss kill: 00:07", "First boss spawn: 00:08", "First boss kill: 00:09",
                "First evolution ready: 00:10", "First evolution acquired: 00:11"
            };
            for (int i = 0; i < expected.Length; i++) Assert.That(lines[i + 3], Is.EqualTo(expected[i]));
            Assert.That(lines[14], Is.EqualTo("Levels 1m 2, 2m 3, 3m 4, 4m 5, 5m 6"));
            Assert.That(lines[15], Is.EqualTo("Drafts level 1, total 2, pending 3, weapons 4/5, passives 6/7, evolutions 8"));
        }

        [Test]
        public void UnseenTimesAndNonpositiveCheckpointLevelsRenderNotYet()
        {
            var port = new MetricsPort { StartedValue = true };
            port.Data.Reset();
            port.Data.RecordLevelCheckpoints(60f, 0);
            var lines = new SurvivorsRunMetricsReadModel(port).Describe();
            for (int i = 3; i < 14; i++) Assert.That(lines[i], Does.EndWith(": not yet"));
            Assert.That(lines[14], Is.EqualTo("Levels 1m not yet, 2m not yet, 3m not yet, 4m not yet, 5m not yet"));
        }

        [Test]
        public void RepeatedCallsReuseListAndReadNewTelemetryWithoutTakingOwnership()
        {
            var port = new MetricsPort { StartedValue = true };
            port.Data.Reset();
            var model = new SurvivorsRunMetricsReadModel(port);
            IReadOnlyList<string> first = model.Describe();
            Assert.That(first[3], Is.EqualTo("First kill: not yet"));
            port.Data.Record(SurvivorsRunMetric.FirstKill, 42f);
            port.StateValue = SurvivorsRunState.GameOver;
            Assert.That(model.Describe(), Is.SameAs(first));
            Assert.That(first[3], Is.EqualTo("First kill: 00:42"));
            Assert.That(first[2], Is.EqualTo("Runtime 01:15 - state GameOver"));
            Assert.That(port.Data.FirstKillTimeSeconds, Is.EqualTo(42f));
        }

        [Test]
        public void ClearEmptiesReturnedListWithoutReadingPortsOrResettingTelemetry()
        {
            var port = new MetricsPort { StartedValue = true };
            port.Data.Reset(); port.Data.Record(SurvivorsRunMetric.FirstKill, 42f);
            var model = new SurvivorsRunMetricsReadModel(port);
            IReadOnlyList<string> first = model.Describe(); port.Events.Clear();
            model.Clear(); model.Clear();
            Assert.That(first, Is.Empty);
            Assert.That(port.Events, Is.Empty);
            Assert.That(port.Data.FirstKillTimeSeconds, Is.EqualTo(42f));
            Assert.That(model.Describe(), Is.SameAs(first));
        }

        [Test]
        public void ReturningToStoppedModeRemovesPreviousActiveLinesWithoutActiveReads()
        {
            var port = new MetricsPort { StartedValue = true };
            var model = new SurvivorsRunMetricsReadModel(port);
            IReadOnlyList<string> first = model.Describe();
            port.StartedValue = false; port.Menu = true; port.RejectActive = true;
            Assert.That(model.Describe(), Is.SameAs(first));
            Assert.That(first.Count, Is.EqualTo(3));
            Assert.That(first[2], Is.EqualTo("Run mode selection open"));
        }

        [TestCase("en-US", "4.5", "6.5", "7.5")]
        [TestCase("fr-FR", "4,5", "6,5", "7,5")]
        public void NumericDetailsRetainCallingCulture(string culture, string damage, string range, string speed)
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                var port = new MetricsPort { StartedValue = true };
                var lines = new SurvivorsRunMetricsReadModel(port).Describe();
                Assert.That(lines[16], Is.EqualTo("Kills 9, XP 10, stored 11/12, overflow 13, damage taken " + damage));
                Assert.That(lines[17], Is.EqualTo("Pickup range " + range + ", pull " + speed + ", pulse 00:08, markers 14, recycles 15, major repositions 16"));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test]
        public void ClearOccursEvenIfFirstHeaderObservationThrows()
        {
            var port = new MetricsPort();
            var model = new SurvivorsRunMetricsReadModel(port);
            IReadOnlyList<string> first = model.Describe();
            port.OnRead = name => { if (name == "mode") throw new InvalidOperationException("header"); };
            Assert.Throws<InvalidOperationException>(() => model.Describe());
            Assert.That(first, Is.Empty);
        }

        private sealed class MetricsPort : ISurvivorsRunMetricsReadPort, ISurvivorsActiveRunMetricsReadPort
        {
            internal readonly List<string> Events = new List<string>();
            internal readonly SurvivorsRunTelemetry Data = new SurvivorsRunTelemetry();
            internal bool StartedValue, Menu, RejectActive, RejectMenu;
            internal SurvivorsRunState StateValue = SurvivorsRunState.Playing;
            internal Action<string> OnRead;
            private T Read<T>(string name, T value) { Events.Add(name); OnRead?.Invoke(name); return value; }
            public string ModeName => Read("mode", "Custom Mode");
            public SurvivorsPacingProfile PacingProfile => Read("pacing", SurvivorsPacingProfile.SprintRun);
            public float TargetDuration => Read("target", 120f);
            public float BossSpawnTime => Read("boss", 60f);
            public float VictoryTime => Read("victory", 180f);
            public bool Started => Read("started", StartedValue);
            public bool ModeSelectionOpen
            {
                get { Assert.That(RejectMenu, Is.False, "Active runs do not observe the stopped-mode menu."); return Read("menu", Menu); }
            }
            public ISurvivorsActiveRunMetricsReadPort Active
            {
                get { Assert.That(RejectActive, Is.False, "Stopped runs must not touch active observations."); return Read<ISurvivorsActiveRunMetricsReadPort>("active", this); }
            }
            public float RunTimeSeconds => Read("runtime", 75.9f);
            public SurvivorsRunState State => Read("state", StateValue);
            public SurvivorsRunTelemetry Telemetry => Read("telemetry", Data);
            public SurvivorsRunMetricsDraftValues CaptureDrafts() => Read("drafts", new SurvivorsRunMetricsDraftValues(1, 2, 3, 4, 5, 6, 7, 8));
            public SurvivorsRunMetricsCombatValues CaptureCombat() => Read("combat", new SurvivorsRunMetricsCombatValues(9, 10, 11, 12, 13, 4.5f));
            public SurvivorsRunMetricsPickupValues CapturePickups() => Read("pickups", new SurvivorsRunMetricsPickupValues(6.5f, 7.5f, 8.9f, 14, 15, 16));
        }
    }
}
