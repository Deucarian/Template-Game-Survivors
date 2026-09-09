using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsTransientFeedbackTests
    {
        [Test]
        public void CombatFeedbackBoundsEachFamilyAndReleasesEvictedAndExpiredMaterials()
        {
            var root = new GameObject("combat-feedback-lifetime-test");
            using (var presenter = new SurvivorsCombatFeedbackPresenter(() => root.transform))
            {
                try
                {
                    presenter.RecordEnemyDeathEffect(Vector3.zero, SurvivorsEnemyRole.Swarm, 1f);
                    Material first = root.GetComponentInChildren<Renderer>().sharedMaterial;
                    for (int i = 0; i < 60; i++) presenter.RecordEnemyDeathEffect(Vector3.zero, SurvivorsEnemyRole.Elite, 1f);
                    Assert.IsTrue(first == null);
                    Assert.AreEqual(56, presenter.ActiveEnemyDeathEffectCount);
                    for (int i = 0; i < 35; i++) presenter.RecordEnemyRangedAttackFeedback(Vector3.zero, Vector3.right, SurvivorsEnemyRole.Spitter);
                    Assert.AreEqual(28, presenter.ActiveEnemyRangedAttackFeedbackCount);
                    presenter.TickEnemyRangedAttackFeedbackEffects(0.35f);
                    Assert.AreEqual(0, presenter.ActiveEnemyRangedAttackFeedbackCount);
                    Assert.AreEqual(56, presenter.ActiveEnemyDeathEffectCount);
                    presenter.TickWorldFeedbackEffects(0.43f);
                    Assert.AreEqual(0, root.transform.childCount);
                    Assert.AreEqual(61, presenter.EnemyDeathEffectCount);
                    presenter.ResetMetrics();
                    Assert.AreEqual(0, presenter.EnemyDeathEffectCount);
                }
                finally { Object.DestroyImmediate(root); }
            }
        }

        [Test]
        public void TelegraphsKeepDistinctFadeWindowsAndDisposeLeavesParentAlive()
        {
            var root = new GameObject("telegraph-lifetime-test");
            var presenter = new SurvivorsThreatTelegraphPresenter(() => root.transform);
            try
            {
                presenter.RecordIncomingThreatTelegraph(Vector3.zero, SurvivorsEnemyRole.Boss, null, 2f, 1f);
                presenter.RecordMajorThreatSlamTelegraphEffect(Vector3.zero, SurvivorsEnemyRole.Boss, 2f, 1f);
                Assert.AreEqual("FINAL BOSS INCOMING", presenter.LastIncomingThreatTelegraphLabel);
                presenter.TickIncomingThreatTelegraphEffects(1.13f);
                presenter.TickMajorThreatSlamTelegraphEffects(1.13f);
                Assert.AreEqual(1, presenter.ActiveIncomingThreatTelegraphEffectCount);
                Assert.AreEqual(0, presenter.ActiveMajorThreatSlamTelegraphEffectCount);
                Material material = root.GetComponentInChildren<Renderer>().sharedMaterial;
                presenter.Dispose();
                presenter.Dispose();
                Assert.IsTrue(material == null);
                Assert.IsTrue(root != null);
                Assert.AreEqual(0, root.transform.childCount);
            }
            finally { presenter.Dispose(); Object.DestroyImmediate(root); }
        }

        [Test]
        public void RewardDropPresentationDoesNotRequireGameplayAndCleansUpAfterParentLoss()
        {
            GameObject root = null;
            var presenter = new SurvivorsRewardDropPresenter(() => root == null ? null : root.transform, SurvivorsUiTheme.CreateDefault);
            try
            {
                presenter.RecordMajorRewardDropFeedback(Vector3.zero, SurvivorsEnemyRole.Boss, 1f);
                Assert.AreEqual(0, presenter.MajorRewardDropFeedbackCount);
                root = new GameObject("reward-feedback-lifetime-test");
                presenter.RecordMajorRewardDropFeedback(Vector3.zero, SurvivorsEnemyRole.Boss, 1f);
                Material material = root.GetComponentInChildren<Renderer>().sharedMaterial;
                Assert.AreEqual("Boss Reward Cache", presenter.LastMajorRewardDropFeedbackLabel);
                Object.DestroyImmediate(root);
                presenter.TickMajorRewardDropFeedbackEffects(0f);
                Assert.IsTrue(material == null);
                Assert.AreEqual(0, presenter.ActiveMajorRewardDropFeedbackCount);
                Assert.AreEqual(1, presenter.MajorRewardDropFeedbackCount);
                presenter.ResetMetrics();
                Assert.AreEqual(string.Empty, presenter.LastMajorRewardDropFeedbackLabel);
            }
            finally { presenter.Dispose(); if (root != null) Object.DestroyImmediate(root); }
        }
    }
}
