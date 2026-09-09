using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsArenaPresentationTests
    {
        [TestCase(5.99f, 0f)]
        [TestCase(6f, 12f)]
        [TestCase(-6f, 0f)]
        [TestCase(-6.01f, -12f)]
        public void ArenaAnchorsKeepOriginalHalfCellBoundaries(float position, float expected)
        {
            Assert.AreEqual(expected, SurvivorsArenaGeometry.ResolveArenaPresentationAnchor(position));
        }

        [Test]
        public void LandmarkKeysPreserveSignedCoordinatesAndDeterministicOffsets()
        {
            var keys = new HashSet<long>();
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++) keys.Add(SurvivorsArenaGeometry.ResolveArenaLandmarkKey(x, z));
            }
            Assert.AreEqual(9, keys.Count);
            SurvivorsArenaGeometry.ResolveArenaLandmarkOffset(0, out int firstX, out int firstZ);
            SurvivorsArenaGeometry.ResolveArenaLandmarkOffset(7, out int lastX, out int lastZ);
            Assert.AreEqual(-1, firstX);
            Assert.AreEqual(-1, firstZ);
            Assert.AreEqual(1, lastX);
            Assert.AreEqual(1, lastZ);
            float jitter = SurvivorsArenaGeometry.ResolveArenaLandmarkJitter(-4, 3, 1);
            Assert.That(jitter, Is.InRange(-0.5f, 0.5f));
            Assert.AreEqual(jitter, SurvivorsArenaGeometry.ResolveArenaLandmarkJitter(-4, 3, 1));
        }

        [Test]
        public void ArenaRepositionsOwnedVisualsAndCompassObservesDiscoveredKeys()
        {
            var root = new GameObject("arena test root");
            var discovered = new HashSet<long>();
            var presenter = new SurvivorsArenaPresenter(SurvivorsUiTheme.CreateDefault, discovered.Contains);
            try
            {
                presenter.Build(root.transform);
                Vector3 player = new Vector3(39f, 1.2f, -50f);
                presenter.Update(player, 2f, 1f);
                Assert.AreEqual(25, presenter.TileCount);
                Assert.AreEqual(8, presenter.LandmarkCount);
                Assert.AreEqual(new Vector3(39f, 0f, -50f), presenter.Center);
                Assert.That(Vector3.Distance(new Vector3(12f, -0.08f, -72f), presenter.FirstTilePosition), Is.LessThan(0.001f));
                Assert.IsTrue(presenter.TryResolveClosestArenaLandmark(player, true, out _, out float distance, out Vector3 delta));
                Assert.That(distance, Is.GreaterThan(0f));
                Assert.IsTrue(presenter.CompassVisible);
                Assert.That(Vector3.Dot(presenter.CompassForward, delta.normalized), Is.GreaterThan(0.999f));
                for (int i = 0; i < presenter.LandmarkCount; i++)
                {
                    Assert.IsTrue(presenter.TryGetLandmark(i, out long key, out Vector3 position));
                    Assert.That(position.y, Is.EqualTo(0.38f).Within(0.001f));
                    discovered.Add(key);
                }
                Assert.AreEqual(8, discovered.Count);
                presenter.Update(player, 2f, 1f);
                Assert.IsFalse(presenter.CompassVisible);
                Assert.IsTrue(presenter.TryResolveClosestArenaLandmark(player, false, out _, out _, out _));
                discovered.Clear();
                presenter.Update(player, 0f, 1f);
                Assert.IsFalse(presenter.CompassVisible);
            }
            finally
            {
                presenter.Dispose();
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RebuildReleasesPreviousHierarchyAndMaterialsAndDisposalSurvivesParentLoss()
        {
            var root = new GameObject("arena lifetime root");
            var presenter = new SurvivorsArenaPresenter(SurvivorsUiTheme.CreateDefault, key => false);
            try
            {
                presenter.Build(root.transform);
                var firstMaterials = new List<Material>();
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) firstMaterials.Add(renderer.sharedMaterial);
                Assert.AreEqual(36, firstMaterials.Count);
                Assert.AreEqual(2, root.transform.childCount);
                presenter.Build(root.transform);
                Assert.AreEqual(2, root.transform.childCount);
                foreach (Material material in firstMaterials) Assert.IsTrue(material == null);
                Assert.IsFalse(presenter.TryGetLandmark(-1, out _, out _));
                Assert.IsFalse(presenter.TryGetLandmark(8, out _, out _));
                Object.DestroyImmediate(root);
                Assert.DoesNotThrow(presenter.Dispose);
                Assert.AreEqual(0, presenter.TileCount);
                Assert.AreEqual(0, presenter.LandmarkCount);
                Assert.AreEqual(Vector3.zero, presenter.Center);
                Assert.IsFalse(presenter.CompassVisible);
            }
            finally
            {
                presenter.Dispose();
                if (root != null) Object.DestroyImmediate(root);
            }
        }
    }
}
