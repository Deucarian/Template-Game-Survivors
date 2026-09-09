using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsPresentationLifecycleTests
    {
        [Test]
        public void DamageFeedbackIsBoundedRejectsInvalidAmountsAndExpiresWithoutScene()
        {
            var feedback = new SurvivorsDamagePopupPresenter();
            feedback.Record(Vector3.zero, float.NaN, false, false);
            feedback.Record(Vector3.zero, float.PositiveInfinity, false, false);
            feedback.Record(Vector3.zero, 0f, false, false);
            Assert.AreEqual(0, feedback.SpawnCount);
            for (int i = 0; i < 100; i++) feedback.Record(Vector3.zero, 1f, false, false);
            Assert.AreEqual(100, feedback.SpawnCount);
            Assert.AreEqual(72, feedback.ActiveCount);
            feedback.Tick(-1f);
            feedback.Tick(0.89f);
            Assert.AreEqual(72, feedback.ActiveCount);
            feedback.Tick(0.02f);
            Assert.AreEqual(0, feedback.ActiveCount);
            feedback.Reset();
            Assert.AreEqual(0, feedback.SpawnCount);
        }

        [Test]
        public void AudioRebuildAndDisposeReleaseOwnedClipsAndSources()
        {
            var root = new GameObject("audio-composition-test");
            var presenter = new SurvivorsAudioPresenter(SurvivorsUiTheme.CreateDefault, () => 0f);
            try
            {
                presenter.Build(root.transform);
                AudioClip firstClip = presenter.FireClip;
                AudioSource firstSource = root.GetComponentInChildren<AudioSource>();
                Assert.IsNotNull(firstClip);
                presenter.Build(root.transform);
                Assert.IsTrue(firstClip == null);
                Assert.IsTrue(firstSource == null);
                Assert.AreEqual(1, root.GetComponentsInChildren<AudioSource>().Length);
                AudioClip nextClip = presenter.FireClip;
                presenter.Dispose();
                presenter.Dispose();
                Assert.IsTrue(nextClip == null);
                Assert.AreEqual(0, root.GetComponentsInChildren<AudioSource>().Length);
            }
            finally
            {
                presenter.Dispose();
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void EnemyPresentationResetsFlashAndOwnsOnlyItsRuntimeMaterial()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var presenter = new SurvivorsEnemyPresentation(root.transform, () => false, () => false);
            try
            {
                presenter.Initialize(0.5f, Color.red);
                var material = root.GetComponent<Renderer>().sharedMaterial;
                presenter.TriggerHitFlash(false, 1f);
                Assert.IsTrue(presenter.IsHitFlashActive);
                Assert.Greater(root.transform.localScale.x, 1f);
                presenter.ResetForPool();
                Assert.IsFalse(presenter.IsHitFlashActive);
                Assert.AreEqual(Vector3.one, root.transform.localScale);
                Assert.AreEqual(Color.red, material.color);
                presenter.Dispose();
                presenter.Dispose();
                Assert.IsTrue(material == null);
                Assert.IsTrue(root != null);
            }
            finally
            {
                presenter.Dispose();
                Object.DestroyImmediate(root);
            }
        }
    }
}
