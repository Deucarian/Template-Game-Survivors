using System;
using System.Collections.Generic;
using Deucarian.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsHitFeedbackTests
    {
        [TestCase(3d, 8d, 3f)]
        [TestCase(0d, 8d, 8f)]
        [TestCase(-2d, 8d, 8f)]
        [TestCase(0d, 0d, 0f)]
        [TestCase(0d, -1d, 0f)]
        [TestCase(double.NaN, 8d, 8f)]
        [TestCase(0d, double.NaN, 0f)]
        [TestCase(double.PositiveInfinity, 8d, 0f)]
        [TestCase(0d, double.PositiveInfinity, 0f)]
        [TestCase(double.MaxValue, 8d, float.MaxValue)]
        public void PopupAmountPreservesHealthPreferenceFiniteGateAndFloatCap(double health, double final, float expected)
        {
            Assert.AreEqual(expected, SurvivorsDamageFeedback.ResolveDamagePopupAmount(Damage(health, final)));
        }

        [Test]
        public void RejectedNullAndZeroDamageHaveNoFeedbackEffects()
        {
            var host = new Host();
            var enemy = new Enemy(host.Events);
            host.Damage.RecordEnemyDamageFeedback(enemy, null);
            host.Damage.RecordEnemyDamageFeedback(enemy, Damage(5d, 5d, false, CombatStatus.Rejected));
            host.Damage.RecordEnemyDamageFeedback(enemy, Damage(0d, 0d));
            host.Damage.RecordPlayerDamageFeedback(Damage(0d, 0d), Vector3.one);
            Assert.IsEmpty(host.Events);
            Assert.AreEqual(0, host.Damage.EnemyHitFlashFeedbackCount);
            Assert.AreEqual(0, host.Damage.PlayerDamageFeedbackCount);
        }

        [Test]
        public void EnemyFeedbackUsesLivePoseAndOrdersPopupFlashCountersAudioEnrage()
        {
            var host = new Host();
            var enemy = new Enemy(host.Events) { PositionValue = new Vector3(2f, 3f, 4f) };
            host.Damage.RecordEnemyDamageFeedback(enemy, Damage(4d, 10d, true));
            CollectionAssert.AreEqual(new[] { "enemy-position", "popup:False:0", "flash:True:0.13", "audio:1:1", "enrage" }, host.Events);
            Assert.AreEqual(enemy.PositionValue, host.PopupPosition);
            Assert.AreEqual(4f, host.PopupAmount);
            Assert.IsTrue(host.PopupCritical);
            Assert.AreSame(enemy, host.EnragedEnemy);
            Assert.AreEqual(1, host.Damage.CriticalHitFeedbackCount);
        }

        [Test]
        public void FailedHitFlashDoesNotPublishEnemyCountersOrRunLaterEffects()
        {
            var host = new Host();
            var enemy = new Enemy(host.Events) { FailFlash = true };
            Assert.Throws<InvalidOperationException>(() => host.Damage.RecordEnemyDamageFeedback(enemy, Damage(3d, 3d, true)));
            Assert.AreEqual(3, host.Events.Count);
            Assert.AreEqual(0, host.Damage.EnemyHitFlashFeedbackCount);
            Assert.AreEqual(0, host.Damage.CriticalHitFeedbackCount);
            Assert.IsNull(host.EnragedEnemy);
        }

        [Test]
        public void PlayerPublishesCountBeforePopupAndIndependentResetsKeepOtherCounters()
        {
            var host = new Host();
            host.Damage.RecordPlayerDamageFeedback(Damage(2d, 7d, true, CombatStatus.NoOp), Vector3.up);
            Assert.AreEqual("popup:True:1", host.Events[0]);
            Assert.AreEqual(2f, host.PopupAmount);
            host.Damage.RecordEnemyDamageFeedback(new Enemy(host.Events), Damage(3d, 3d, true));
            host.Damage.ResetPlayerFeedback();
            Assert.AreEqual(0, host.Damage.PlayerDamageFeedbackCount);
            Assert.AreEqual(1, host.Damage.CriticalHitFeedbackCount);
            host.Damage.RecordPlayerDamageFeedback(Damage(1d, 1d), Vector3.zero);
            host.Damage.ResetEnemyFeedback();
            Assert.AreEqual(1, host.Damage.PlayerDamageFeedbackCount);
            Assert.AreEqual(0, host.Damage.EnemyHitFlashFeedbackCount);
            Assert.AreEqual(0, host.Damage.CriticalHitFeedbackCount);
        }

        [Test]
        public void NullEnemiesDoNotReadPortsOrProduceRewards()
        {
            var host = new Host();
            host.Damage.RecordEnemyDamageFeedback(null, Damage(1d, 1d));
            host.Dodge.RecordEnemyRangedAttackDodgeFeedback(null);
            Assert.IsEmpty(host.Events);
            Assert.AreEqual(0, host.Dodge.EnemyRangedAttackDodgeFeedbackCount);
        }

        [Test]
        public void DodgePublishesSuccessfulDropBeforeLateNameAndFeedbackBeforeLivePulsePosition()
        {
            var host = new Host { Player = new Vector3(1f, 2f, 3f) };
            host.TuningValue.EnemyRangedAttackDodgeExperienceReward = 7;
            host.TuningValue.PickupCollectRadius = 1f;
            var enemy = new Enemy(host.Events) { PositionValue = new Vector3(-4f, 20f, 3f), Name = "Before" };
            host.AfterSpawn = () => enemy.Name = "After";
            host.AfterStreak = () => host.Player = new Vector3(9f, 2f, 7f);
            host.Dodge.RecordEnemyRangedAttackDodgeFeedback(enemy);
            Assert.AreEqual(new Vector3(2.35f, 2f, 3f), host.DropPosition);
            Assert.AreEqual(7, host.DropAmount);
            Assert.AreEqual("After shot dodged: +7 XP", host.Dodge.LastEnemyRangedAttackDodgeFeedbackLabel);
            Assert.AreEqual(1, host.Dodge.EnemyRangedAttackDodgeExperienceGemDropCount);
            CollectionAssert.AreEqual(new[] { "tuning", "player", "enemy-position", "tuning", "spawn:0", "name", "name", "streak:1:1", "player", "pulse:14" }, host.Events);
            Assert.AreEqual(host.Player, host.PulsePosition);
            Assert.AreEqual(new Color(0.52f, 0.95f, 1f), host.StreakColor);
        }

        [TestCase(0, true)]
        [TestCase(-3, true)]
        [TestCase(5, false)]
        public void DisabledOrFailedDodgeDropsStillShowDodgeWithoutXpClaim(int reward, bool succeeds)
        {
            var host = new Host { SpawnSucceeds = succeeds };
            host.TuningValue.EnemyRangedAttackDodgeExperienceReward = reward;
            var enemy = new Enemy(host.Events) { Name = " ", RoleValue = SurvivorsEnemyRole.DreadElite };
            host.Dodge.RecordEnemyRangedAttackDodgeFeedback(enemy);
            Assert.AreEqual("Elite shot dodged", host.Dodge.LastEnemyRangedAttackDodgeFeedbackLabel);
            Assert.AreEqual(1, host.Dodge.EnemyRangedAttackDodgeFeedbackCount);
            Assert.AreEqual(0, host.Dodge.EnemyRangedAttackDodgeExperienceGemDropCount);
            Assert.AreEqual(8, host.PulseCount);
            Assert.AreEqual(reward > 0 ? 1 : 0, host.SpawnCalls);
        }

        [TestCase(SurvivorsEnemyRole.Spitter, "Spitter")]
        [TestCase(SurvivorsEnemyRole.Elite, "Elite")]
        [TestCase(SurvivorsEnemyRole.DreadElite, "Elite")]
        [TestCase(SurvivorsEnemyRole.Miniboss, "Miniboss")]
        [TestCase(SurvivorsEnemyRole.Boss, "Boss")]
        [TestCase(SurvivorsEnemyRole.Summoner, "Ranged shot")]
        public void DodgeFallbackLabelsRetainRoleMapping(SurvivorsEnemyRole role, string expected)
        {
            Assert.AreEqual(expected, SurvivorsRangedDodgeRewards.ResolveRangedDodgeFallbackLabel(role));
        }

        [Test]
        public void DodgePlacementUsesHorizontalDirectionAndMinimumOffsetForMissingOrCoincidentEnemy()
        {
            var host = new Host { Player = new Vector3(2f, 5f, 3f) };
            host.TuningValue.PickupCollectRadius = -2f;
            Vector3 expected = new Vector3(2f, 5f, 3.75f);
            Assert.AreEqual(expected, host.Dodge.ResolveRangedDodgeRewardPosition(null));
            Assert.AreEqual(expected, host.Dodge.ResolveRangedDodgeRewardPosition(new Enemy(host.Events) { PositionValue = new Vector3(2f, 90f, 3f) }));
        }

        [Test]
        public void DodgeResetClearsRetainedObservationsWithoutReplayingEffects()
        {
            var host = new Host();
            host.TuningValue.EnemyRangedAttackDodgeExperienceReward = 1;
            host.Dodge.RecordEnemyRangedAttackDodgeFeedback(new Enemy(host.Events));
            host.Events.Clear();
            host.Dodge.Reset();
            Assert.AreEqual(0, host.Dodge.EnemyRangedAttackDodgeFeedbackCount);
            Assert.AreEqual(0, host.Dodge.EnemyRangedAttackDodgeExperienceGemDropCount);
            Assert.IsEmpty(host.Dodge.LastEnemyRangedAttackDodgeFeedbackLabel);
            Assert.IsEmpty(host.Events);
        }

        [Test]
        public void StreakHistoryPublishesBeforeBannerAndSurvivesExpiryUntilExplicitReset()
        {
            var banner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Streak);
            SurvivorsStreakFeedbackHistory history = null;
            history = new SurvivorsStreakFeedbackHistory((label, seconds, color) =>
            {
                Assert.AreEqual(1, history.StreakRewardFeedbackCount);
                Assert.AreEqual(label, history.LastStreakRewardFeedbackLabel);
                Assert.AreEqual(1.8f, seconds);
                banner.Show(label, seconds, color);
            });
            history.RecordStreakRewardFeedback(null, Color.black);
            history.RecordStreakRewardFeedback("  ", Color.black);
            Assert.AreEqual(0, history.StreakRewardFeedbackCount);
            history.RecordStreakRewardFeedback("  original spacing  ", Color.cyan);
            banner.Tick(2f);
            Assert.IsEmpty(banner.Label);
            Assert.AreEqual("  original spacing  ", history.LastStreakRewardFeedbackLabel);
            banner.Show("borrowed", 4f, Color.red);
            history.Reset();
            Assert.AreEqual(0, history.StreakRewardFeedbackCount);
            Assert.IsEmpty(history.LastStreakRewardFeedbackLabel);
            Assert.AreEqual("borrowed", banner.Label);
        }

        [Test]
        public void DestroyedUnityEnemyIsRejectedByExistingControllerFacade()
        {
            var root = new GameObject("Damage feedback null boundary");
            root.SetActive(false);
            var controller = root.AddComponent<SurvivorsTemplateController>();
            var enemyRoot = new GameObject("Destroyed feedback target");
            var enemy = enemyRoot.AddComponent<SurvivorsEnemyActor>();
            UnityEngine.Object.DestroyImmediate(enemyRoot);
            try
            {
                Assert.DoesNotThrow(() => controller.RecordEnemyDamageFeedback(enemy, Damage(1d, 1d)));
                Assert.DoesNotThrow(() => controller.RecordEnemyRangedAttackDodgeFeedback(enemy));
                Assert.AreEqual(0, controller.EnemyHitFlashFeedbackCount);
                Assert.AreEqual(0, controller.EnemyRangedAttackDodgeFeedbackCount);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static DamageResult Damage(double health, double final, bool critical = false, CombatStatus status = CombatStatus.Success) =>
            new DamageResult(status, new CriticalHitResult(critical, 2d, 0d, false), null, final, final, 0d, health, 0d, default, default, null);

        private sealed class Enemy : ISurvivorsFeedbackEnemy
        {
            private readonly List<string> _events;
            public Enemy(List<string> events) { _events = events; }
            public Vector3 PositionValue;
            public string Name = "Authored enemy";
            public SurvivorsEnemyRole RoleValue = SurvivorsEnemyRole.Spitter;
            public bool FailFlash;
            public Vector3 Position { get { _events.Add("enemy-position"); return PositionValue; } }
            public string DisplayName { get { _events.Add("name"); return Name; } }
            public SurvivorsEnemyRole Role { get { _events.Add("role"); return RoleValue; } }
            public void TriggerHitFlash(bool critical, float seconds)
            {
                _events.Add("flash:" + critical + ":" + seconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (FailFlash) throw new InvalidOperationException("fixture");
            }
        }

        private sealed class Host : ISurvivorsDamageFeedbackPort, ISurvivorsRangedDodgePort
        {
            public readonly List<string> Events = new List<string>();
            public readonly SurvivorsDamageFeedback Damage;
            public readonly SurvivorsRangedDodgeRewards Dodge;
            public readonly SurvivorsTemplateTuning TuningValue = new SurvivorsTemplateTuning();
            public Vector3 Player, PopupPosition, DropPosition, PulsePosition;
            public float PopupAmount;
            public bool PopupCritical, SpawnSucceeds = true;
            public int DropAmount, PulseCount, SpawnCalls;
            public Color StreakColor;
            public ISurvivorsFeedbackEnemy EnragedEnemy;
            public Action AfterSpawn, AfterStreak;
            public Host() { Damage = new SurvivorsDamageFeedback(this); Dodge = new SurvivorsRangedDodgeRewards(this); }
            public SurvivorsTemplateTuning Tuning { get { Events.Add("tuning"); return TuningValue; } }
            public Vector3 PlayerPosition { get { Events.Add("player"); return Player; } }
            public void RecordPopup(Vector3 position, float amount, bool playerDamage, bool critical)
            {
                Events.Add("popup:" + playerDamage + ":" + Damage.PlayerDamageFeedbackCount);
                PopupPosition = position; PopupAmount = amount; PopupCritical = critical;
            }
            public void PlayCombatHitAudio() => Events.Add("audio:" + Damage.EnemyHitFlashFeedbackCount + ":" + Damage.CriticalHitFeedbackCount);
            public void TryEnrage(ISurvivorsFeedbackEnemy enemy) { Events.Add("enrage"); EnragedEnemy = enemy; }
            public bool TrySpawnExperience(Vector3 position, int amount)
            {
                Events.Add("spawn:" + Dodge.EnemyRangedAttackDodgeExperienceGemDropCount);
                SpawnCalls++; DropPosition = position; DropAmount = amount; AfterSpawn?.Invoke(); return SpawnSucceeds;
            }
            public void ShowStreakFeedback(string label, Color color)
            {
                Assert.AreEqual(Dodge.LastEnemyRangedAttackDodgeFeedbackLabel, label);
                Events.Add("streak:" + Dodge.EnemyRangedAttackDodgeFeedbackCount + ":" + Dodge.EnemyRangedAttackDodgeExperienceGemDropCount);
                StreakColor = color; AfterStreak?.Invoke();
            }
            public void PlayDodgePulse(Vector3 position, int count) { Events.Add("pulse:" + count); PulsePosition = position; PulseCount = count; }
        }
    }
}
