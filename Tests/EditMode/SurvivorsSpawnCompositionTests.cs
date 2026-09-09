using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsSpawnCompositionTests
    {
        [Test]
        public void SwarmCadenceLimitsCapacityAndResumesAfterBlockedPopulation()
        {
            var port = new SpawnPort();
            var spawning = new SurvivorsSwarmSpawnCoordinator(port);
            var tuning = new SurvivorsTemplateTuning
            {
                EnemyMaximumAlive = 3,
                EnemySpawnPackBaseCount = 2,
                EnemySpawnPackMaxCount = 2,
                EnemySpawnIntervalSeconds = 1f
            };
            spawning.Tick(0f, 0f, tuning, null, false);
            Assert.AreEqual(2, port.ActiveEnemyCount);
            spawning.Tick(0.5f, 0.5f, tuning, null, false);
            Assert.AreEqual(2, port.ActiveEnemyCount);
            spawning.Tick(0.5f, 1f, tuning, null, false);
            Assert.AreEqual(3, port.ActiveEnemyCount);
            spawning.Tick(10f, 11f, tuning, null, false);
            port.ActiveEnemyCount = 0;
            spawning.Tick(0f, 11f, tuning, null, false);
            Assert.AreEqual(2, port.ActiveEnemyCount);
        }

        [Test]
        public void FailedSpawnStopsPackAndStillConsumesTheCadenceWindow()
        {
            var port = new SpawnPort { Fail = true };
            var spawning = new SurvivorsSwarmSpawnCoordinator(port);
            var tuning = new SurvivorsTemplateTuning
            {
                EnemyMaximumAlive = 20,
                EnemySpawnPackBaseCount = 5,
                EnemySpawnPackMaxCount = 5,
                EnemySpawnIntervalSeconds = 2f
            };
            spawning.Tick(0f, 0f, tuning, null, false);
            Assert.AreEqual(1, port.Attempts);
            spawning.Tick(1f, 1f, tuning, null, false);
            Assert.AreEqual(1, port.Attempts);
            spawning.Reset();
            port.Fail = false;
            spawning.Tick(0f, 1f, tuning, null, false);
            Assert.AreEqual(5, port.ActiveEnemyCount);
            Assert.AreEqual(44, SurvivorsSwarmSpawnCoordinator.ResolveMaximumAlive(tuning, null, true));
            Assert.AreEqual(1.64f, SurvivorsSwarmSpawnCoordinator.ResolveInterval(tuning, null, true), 0.0001f);
        }

        [Test]
        public void OffscreenGeometryIsRepeatableAndOutsideSuppliedGroundBounds()
        {
            var bounds = Rect.MinMaxRect(-18f, -12f, 18f, 12f);
            var center = new Vector3(1f, 7f, 2f);
            for (long seed = -50; seed <= 50; seed++)
            {
                Vector3 position = SurvivorsOffscreenSpawnPolicy.Resolve(center, 3f, 5f, seed, 4f, bounds);
                Assert.IsFalse(SurvivorsOffscreenSpawnPolicy.ContainsGroundPoint(bounds, position), "Seed " + seed);
                Assert.AreEqual(7f, position.y);
                Assert.AreEqual(position, SurvivorsOffscreenSpawnPolicy.Resolve(center, 3f, 5f, seed, 4f, bounds));
            }
        }

        [Test]
        public void MissingCameraUsesTheExistingRadialBandFallback()
        {
            Vector3 center = new Vector3(2f, 3f, 4f);
            Vector3 position = SurvivorsOffscreenSpawnPolicy.Resolve(center, 5f, 8f, 17, 2f, null);
            Assert.AreEqual(center.y, position.y);
            Assert.That(Vector3.Distance(center, position), Is.InRange(5f, 8f));
        }

        [Test]
        public void BannerReplacementRestartsExpiryAndResetClearsPresentation()
        {
            var banner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Reward);
            banner.Show("First", 1f, Color.red);
            banner.Tick(0.8f);
            banner.Show("Second", 2f, Color.green);
            banner.Tick(0.3f);
            Assert.AreEqual("Second", banner.Label);
            Assert.AreEqual(Color.green, banner.Accent);
            banner.Tick(2f);
            Assert.AreEqual(string.Empty, banner.Label);
            banner.Reset();
            Assert.AreEqual(0f, banner.RemainingSeconds);
        }

        private sealed class SpawnPort : ISurvivorsSwarmSpawnPort
        {
            public int ActiveEnemyCount { get; set; }
            public long SpawnSequence { get; private set; }
            public int Attempts;
            public bool Fail;
            public bool TrySpawn(SurvivorsEnemyRole role)
            {
                Attempts++;
                if (Fail) return false;
                ActiveEnemyCount++;
                SpawnSequence++;
                return true;
            }
        }
    }
}
