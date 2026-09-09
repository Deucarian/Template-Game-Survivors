using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRunTelemetryTests
    {
        [Test]
        public void PreStartZeroAndRunResetUnseenValuesRetainComponentContract()
        {
            var telemetry = new SurvivorsRunTelemetry();
            Assert.AreEqual(0f, telemetry.FirstKillTimeSeconds);
            Assert.AreEqual(0f, telemetry.FirstEvolutionAcquiredTimeSeconds);
            Assert.AreEqual(0, telemetry.LevelAtOneMinute);
            Assert.AreEqual(0, telemetry.LevelAtFiveMinutes);
            telemetry.Record(SurvivorsRunMetric.FirstKill, 5f);
            telemetry.RecordLevelCheckpoints(300f, 10);
            Assert.AreEqual(0f, telemetry.FirstKillTimeSeconds);
            Assert.AreEqual(0, telemetry.LevelAtFiveMinutes);
            telemetry.Reset();
            Assert.AreEqual(-1f, telemetry.FirstKillTimeSeconds);
            Assert.AreEqual(-1f, telemetry.FirstEvolutionAcquiredTimeSeconds);
            Assert.AreEqual(-1, telemetry.LevelAtOneMinute);
            Assert.AreEqual(-1, telemetry.LevelAtFiveMinutes);
        }

        [Test]
        public void FirstTimestampAtZeroLocksIndependentlyUntilNextRun()
        {
            var telemetry = new SurvivorsRunTelemetry();
            telemetry.Reset();
            telemetry.Record(SurvivorsRunMetric.FirstKill, 0f);
            telemetry.Record(SurvivorsRunMetric.FirstKill, 3f);
            telemetry.Record(SurvivorsRunMetric.FirstExperiencePickup, 2f);
            telemetry.Record(SurvivorsRunMetric.FirstBossKill, 180f);
            Assert.AreEqual(0f, telemetry.FirstKillTimeSeconds);
            Assert.AreEqual(2f, telemetry.FirstExperiencePickupTimeSeconds);
            Assert.AreEqual(180f, telemetry.FirstBossKillTimeSeconds);
            Assert.AreEqual(-1f, telemetry.FirstBossSpawnTimeSeconds);
            telemetry.Reset();
            telemetry.Record(SurvivorsRunMetric.FirstKill, 4f);
            Assert.AreEqual(4f, telemetry.FirstKillTimeSeconds);
            Assert.AreEqual(-1f, telemetry.FirstExperiencePickupTimeSeconds);
        }

        [Test]
        public void InclusiveMinuteThresholdsCaptureCurrentLevelAcrossSkippedIntervalsOnlyOnce()
        {
            var telemetry = new SurvivorsRunTelemetry();
            telemetry.Reset();
            telemetry.RecordLevelCheckpoints(59.99f, 2);
            Assert.AreEqual(-1, telemetry.LevelAtOneMinute);
            telemetry.RecordLevelCheckpoints(60f, 3);
            Assert.AreEqual(3, telemetry.LevelAtOneMinute);
            Assert.AreEqual(-1, telemetry.LevelAtTwoMinutes);
            telemetry.RecordLevelCheckpoints(300f, 8);
            CollectionAssert.AreEqual(new[] { 3, 8, 8, 8, 8 }, new[] { telemetry.LevelAtOneMinute,
                telemetry.LevelAtTwoMinutes, telemetry.LevelAtThreeMinutes, telemetry.LevelAtFourMinutes, telemetry.LevelAtFiveMinutes });
            telemetry.RecordLevelCheckpoints(600f, 20);
            Assert.AreEqual(8, telemetry.LevelAtFiveMinutes);
            telemetry.Reset();
            telemetry.RecordLevelCheckpoints(120f, 4);
            Assert.AreEqual(4, telemetry.LevelAtOneMinute);
            Assert.AreEqual(4, telemetry.LevelAtTwoMinutes);
            Assert.AreEqual(-1, telemetry.LevelAtThreeMinutes);
        }
    }
}
