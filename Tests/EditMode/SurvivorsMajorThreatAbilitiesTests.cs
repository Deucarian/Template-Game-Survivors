using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsMajorThreatAbilitiesTests
    {
        [Test]
        public void EnrageMarksBeforeSupportCallbacksAndUsesPreIncrementCountForLiveSeeds()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            var enemy = host.Add(SurvivorsEnemyRole.Elite, 50f);
            host.OnSpawn = () =>
            {
                Assert.That(host.Abilities.MajorThreatEnrageCount, Is.Zero);
                host.Abilities.TryTriggerMajorThreatEnrage(enemy);
            };
            host.Abilities.TryTriggerMajorThreatEnrage(enemy);
            Assert.That(host.Requests.Select(request => request.seed), Is.EqualTo(new long[] { 711, 713, 715 }));
            Assert.That(host.Requests.Select(request => request.role), Is.EqualTo(new[] { SurvivorsEnemyRole.Runner, SurvivorsEnemyRole.Swarm, SurvivorsEnemyRole.Runner }));
            Assert.That(host.Requests.All(request => request.source == "major-threat-enrage"), Is.True);
            Assert.That(host.Requests[0].minimum, Is.EqualTo(2.35f));
            Assert.That(host.Requests[1].minimum, Is.EqualTo(2.35f), "The source's unused alternating lane radius does not change the offscreen command.");
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.EqualTo(1));
            Assert.That(host.Abilities.MajorThreatEnrageSupportSpawnCount, Is.EqualTo(3));
            Assert.That(host.Events, Is.EqualTo(new[] { "spawn", "spawn", "spawn", "banner", "boss-pulse" }));
            Assert.That(host.LastLabel, Is.EqualTo("Test Elite enraged: +3 support"));
            Assert.That(host.LastBurst, Is.EqualTo(42));
        }

        [Test]
        public void DiagnosticsResetRetainsEnrageMembershipUntilForgetOrClear()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            host.Tuning.MajorThreatEnrageEliteSupportCount = 0;
            var enemy = host.Add(SurvivorsEnemyRole.Elite, 40f);
            host.Abilities.TryTriggerMajorThreatEnrage(enemy);
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.EqualTo(1));
            host.Abilities.ResetDiagnostics();
            host.Abilities.TryTriggerMajorThreatEnrage(enemy);
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.Zero);
            host.Abilities.ForgetEnemy(enemy);
            host.Abilities.TryTriggerMajorThreatEnrage(enemy);
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.EqualTo(1));
            host.Abilities.ClearMembers();
            host.Abilities.TryTriggerMajorThreatEnrage(enemy);
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.EqualTo(2));
            Assert.That(host.Abilities.LastMajorThreatEnrageFeedbackLabel, Is.EqualTo("Test Elite enraged: +0 support"));
        }

        [Test]
        public void EnrageGuardsRequireAliveMajorPlayingAndPositiveReachedThreshold()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            var alive = host.Add(SurvivorsEnemyRole.Boss, 51f);
            host.Abilities.TryTriggerMajorThreatEnrage(null);
            host.Abilities.TryTriggerMajorThreatEnrage(host.Add(SurvivorsEnemyRole.Boss, 0f));
            host.Abilities.TryTriggerMajorThreatEnrage(host.Add(SurvivorsEnemyRole.Swarm, 10f));
            host.Abilities.TryTriggerMajorThreatEnrage(alive);
            host.State = SurvivorsRunState.LevelUp;
            host.Abilities.TryTriggerMajorThreatEnrage(host.Add(SurvivorsEnemyRole.Boss, 20f));
            host.State = SurvivorsRunState.Playing;
            host.Tuning.MajorThreatEnrageHealthThreshold = -1f;
            host.Abilities.TryTriggerMajorThreatEnrage(host.Add(SurvivorsEnemyRole.Boss, 20f));
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.Zero);
            Assert.That(host.Events, Is.Empty);
            host.Tuning.MajorThreatEnrageHealthThreshold = 2f;
            host.Abilities.TryTriggerMajorThreatEnrage(alive);
            Assert.That(host.Abilities.MajorThreatEnrageCount, Is.EqualTo(1));
        }

        [Test]
        public void SlamUsesFullDistanceAndCountsBarrierLossAsDamageButNotAnImmuneHit()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            var enemy = host.Add(SurvivorsEnemyRole.Boss);
            enemy.transform.position = Vector3.up * 4f;
            host.Abilities.ResolveMajorThreatSlam(enemy);
            Assert.That(host.LastDamage, Is.Zero);
            Assert.That(host.Abilities.MajorThreatSlamCastCount, Is.EqualTo(1));
            Assert.That(host.Abilities.LastMajorThreatSlamFeedbackLabel, Is.EqualTo("Test Boss slam missed"));
            enemy.transform.position = Vector3.up * 2.85f;
            host.DamageEffect = amount => host.BarrierValue -= amount;
            host.Abilities.ResolveMajorThreatSlam(enemy);
            Assert.That(host.CurrentHealth, Is.EqualTo(100f));
            Assert.That(host.BarrierValue, Is.EqualTo(10f));
            Assert.That(host.Abilities.MajorThreatSlamHitCount, Is.EqualTo(1));
            Assert.That(host.LastDamageSource, Is.EqualTo("combatant.survivors.enemy.slam.1"));
            Assert.That(host.Abilities.LastMajorThreatSlamFeedbackLabel, Is.EqualTo("Test Boss slam hit"));
            host.DamageEffect = _ => { };
            host.Abilities.ResolveMajorThreatSlam(enemy);
            Assert.That(host.Abilities.MajorThreatSlamHitCount, Is.EqualTo(1));
            Assert.That(host.Abilities.LastMajorThreatSlamFeedbackLabel, Is.EqualTo("Test Boss slam missed"));
            Assert.That(host.LastBurst, Is.EqualTo(58));
        }

        [Test]
        public void SlamCastGuardsAndZeroDamageDoNotInventDamageEvents()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            var enemy = host.Add(SurvivorsEnemyRole.Miniboss);
            host.Abilities.ResolveMajorThreatSlam(null);
            host.Abilities.ResolveMajorThreatSlam(host.Add(SurvivorsEnemyRole.Miniboss, 0f));
            host.State = SurvivorsRunState.LevelUp;
            host.Abilities.ResolveMajorThreatSlam(enemy);
            Assert.That(host.Events, Is.Empty);
            host.State = SurvivorsRunState.Playing;
            host.Tuning.MajorThreatSlamDamage = -10f;
            host.Abilities.ResolveMajorThreatSlam(enemy);
            Assert.That(host.Abilities.MajorThreatSlamCastCount, Is.EqualTo(1));
            Assert.That(host.Abilities.MajorThreatSlamHitCount, Is.Zero);
            Assert.That(host.Events, Is.EqualTo(new[] { "boss-pulse" }));
            Assert.That(host.LastBurst, Is.EqualTo(40));
        }

        [Test]
        public void TelegraphRetainsNullOnlyGuardAndBannerEffectPulseOrder()
        {
            using var host = new SurvivorsThreatAbilityTestHost();
            host.State = SurvivorsRunState.LevelUp;
            var enemy = host.Add(SurvivorsEnemyRole.Boss, 0f);
            host.Abilities.RecordMajorThreatSlamTelegraph(null);
            host.Abilities.RecordMajorThreatSlamTelegraph(enemy);
            Assert.That(host.Events, Is.EqualTo(new[] { "banner", "telegraph", "boss-pulse" }));
            Assert.That(host.Abilities.MajorThreatSlamWarningCount, Is.EqualTo(1));
            Assert.That(host.LastTelegraphRadius, Is.EqualTo(2.35f));
            Assert.That(host.LastTelegraphDuration, Is.EqualTo(0.7f));
            Assert.That(host.LastLabel, Is.EqualTo("Test Boss winding slam"));
            Assert.That(host.LastBurst, Is.EqualTo(42));
        }

        [TestCase(SurvivorsEnemyRole.Elite, false)]
        [TestCase(SurvivorsEnemyRole.DreadElite, true)]
        [TestCase(SurvivorsEnemyRole.Miniboss, true)]
        [TestCase(SurvivorsEnemyRole.Boss, true)]
        public void SlamRolePolicyRetainsDreadEliteDistinction(SurvivorsEnemyRole role, bool expected)
            => Assert.That(SurvivorsMajorThreatAbilities.IsMajorThreatSlamRole(role), Is.EqualTo(expected));
    }
}
