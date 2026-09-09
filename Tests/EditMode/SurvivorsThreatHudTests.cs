using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsThreatHudTests
    {
        [Test]
        public void LifeBarsHonorAuthoredFlagsAndSummaryKeepsSourceOrderWithoutDoubleLabels()
        {
            var model = new SurvivorsThreatHudModel(new[]
            {
                Item(SurvivorsEnemyRole.Swarm, "Custom", 0.4f, boss: true, overhead: true),
                Item(SurvivorsEnemyRole.Boss, "Dead", 0f, alive: false, boss: true),
                Item(SurvivorsEnemyRole.Boss, "Hidden", 1f),
                Item(SurvivorsEnemyRole.Elite, "", 0.6f, overhead: true)
            });
            Assert.AreEqual(1, model.CountLifeBars(true));
            Assert.AreEqual(2, model.CountLifeBars(false));
            Assert.AreEqual("Custom 40%, Elite 60%", model.LifeBarSummary());
        }

        [Test]
        public void HealthSelectionUsesRoleThenLowestClampedHealthAndKeepsFirstExactTie()
        {
            var items = new List<SurvivorsThreatHudItem>
            {
                Item(SurvivorsEnemyRole.Miniboss, "Low miniboss", 0.01f, overhead: true),
                Item(SurvivorsEnemyRole.Boss, "High boss", 0.9f, boss: true),
                Item(SurvivorsEnemyRole.Boss, "Low boss", 0.4f, boss: true),
                Item(SurvivorsEnemyRole.Boss, "Tied boss", 0.4f, boss: true)
            };
            var model = new SurvivorsThreatHudModel(items);
            Assert.AreEqual("Low boss", model.SelectHealthThreat().Value.Name);
            items.Clear();
            Assert.IsFalse(model.SelectHealthThreat().HasValue);
        }

        [Test]
        public void MarkerEligibilityUsesGroundDistanceAndSelectionRetainsFullDistanceTieRule()
        {
            var model = new SurvivorsThreatHudModel(new[]
            {
                Item(SurvivorsEnemyRole.Boss, "Vertically far", 1f, new Vector3(0f, 100f, 0f), marker: true),
                Item(SurvivorsEnemyRole.Boss, "Ground", 1f, new Vector3(12f, 0f, 0f), marker: true),
                Item(SurvivorsEnemyRole.Boss, "Elevated", 1f, new Vector3(10f, 20f, 0f), marker: true),
                Item(SurvivorsEnemyRole.Miniboss, "Farther lower role", 1f, Vector3.right * 100f, marker: true),
                Item(SurvivorsEnemyRole.Boss, "Authored hidden", 1f, Vector3.left * 1000f)
            });
            Assert.AreEqual(3, model.CountMarkers(Vector3.zero, 10f));
            Assert.AreEqual("Elevated", model.SelectMarker(Vector3.zero, 10f).Value.Name);
            Assert.AreEqual("Elevated E 10m", model.MarkerLabel(Vector3.zero, 10f));
            Assert.IsFalse(SurvivorsThreatHudModel.IsMarkerVisible(Item(SurvivorsEnemyRole.Boss, "", 1f, marker: true), Vector3.zero, -1f));
        }

        [Test]
        public void LastMarkerRetainsLastObservationAndExplicitReentryUntilRunReset()
        {
            var items = new List<SurvivorsThreatHudItem>
            {
                Item(SurvivorsEnemyRole.Elite, "Scout", 1f, new Vector3(-12f, 0f, 12f), marker: true)
            };
            var model = new SurvivorsThreatHudModel(items);
            model.UpdateLastMarker(Vector3.zero, 10f);
            Assert.AreEqual("Scout NW 17m", model.LastMarkerLabel);
            items.Clear();
            model.UpdateLastMarker(Vector3.zero, 10f);
            Assert.AreEqual("Scout NW 17m", model.LastMarkerLabel);
            model.RecordMarkerLabel("Scout re-entering E");
            Assert.AreEqual("Scout re-entering E", model.LastMarkerLabel);
            model.Reset();
            Assert.AreEqual(string.Empty, model.LastMarkerLabel);
        }

        [TestCase(320f, 240f)]
        [TestCase(1920f, 1080f)]
        public void MarkerAndOverheadLayoutsStayInTheirAuthoredViewportBands(float width, float height)
        {
            var viewport = new Vector2(width, height);
            foreach (Vector3 direction in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, new Vector3(1f, 0f, 1f) })
            {
                Assert.IsTrue(SurvivorsThreatHudLayout.TryMarkerPanel(viewport, direction, out Rect marker));
                Assert.GreaterOrEqual(marker.xMin, 18f);
                Assert.LessOrEqual(marker.xMax, width - 18f);
                Assert.GreaterOrEqual(marker.yMin, 96f);
                Assert.LessOrEqual(marker.yMax, height - 18f);
            }
            Assert.IsFalse(SurvivorsThreatHudLayout.TryMarkerPanel(viewport, Vector3.up, out _));
            Rect overhead = SurvivorsThreatHudLayout.OverheadPanel(viewport, new Vector3(-1000f, -1000f, 1f), SurvivorsEnemyRole.Miniboss);
            Assert.AreEqual(190f, overhead.width);
            Assert.AreEqual(12f, overhead.xMin);
            Assert.AreEqual(height - 12f, overhead.yMax);
            Assert.AreEqual(186f, SurvivorsThreatHudLayout.BossPanel(viewport, 1, true, true).y);
        }

        [Test]
        public void ActorAdapterReturnsCopiedValuesAndObservesCurrentCollectionWithoutOwningActors()
        {
            var root = new GameObject("threat-hud-source-test");
            try
            {
                var actor = root.AddComponent<SurvivorsEnemyActor>();
                actor.Initialize(null, new SurvivorsEnemyProfile(SurvivorsEnemyRole.Boss, "test", "Test Boss",
                    10f, 0f, 1f, 0f, 1f, 1, Color.red));
                var actors = new List<SurvivorsEnemyActor> { actor };
                var source = new SurvivorsEnemyHudSource(actors);
                SurvivorsThreatHudItem first = source[0];
                root.transform.position = Vector3.right * 12f;
                Assert.AreEqual(Vector3.zero, first.Position);
                Assert.AreEqual(Vector3.right * 12f, source[0].Position);
                actors.Add(null);
                Assert.AreEqual(2, source.Count);
                Assert.IsFalse(source[1].IsAlive);
                Object.DestroyImmediate(root);
                Assert.IsFalse(source[0].IsAlive);
            }
            finally { if (root != null) Object.DestroyImmediate(root); }
        }

        private static SurvivorsThreatHudItem Item(SurvivorsEnemyRole role, string name, float health,
            Vector3 position = default, bool alive = true, bool boss = false, bool overhead = false, bool marker = false)
        {
            return new SurvivorsThreatHudItem(role, name, health, position, 1f, alive, boss, overhead, marker);
        }
    }
}
