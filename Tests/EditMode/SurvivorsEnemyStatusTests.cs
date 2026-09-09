using System.Collections.Generic;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsEnemyStatusTests
    {
        [Test]
        public void SlowKeepsStrongestMultiplierAndLongestDurationThenResets()
        {
            int changed = 0;
            var status = new SurvivorsEnemyStatusEffects(() => true, (_, __) => { }, () => changed++);
            Assert.IsTrue(status.ApplyMovementSlow(0.5f, 1f));
            Assert.IsTrue(status.ApplyMovementSlow(0.8f, 2f));
            Assert.AreEqual(0.5f, status.CurrentMoveSpeedMultiplier);
            status.TickMovementSlow(1f);
            Assert.IsTrue(status.IsMovementSlowed);
            status.TickMovementSlow(1f);
            Assert.AreEqual(1f, status.CurrentMoveSpeedMultiplier);
            Assert.AreEqual(3, changed);
            Assert.IsFalse(status.ApplyMovementSlow(1f, 2f));
        }

        [Test]
        public void DamageStatusesStackRatesAndStopAfterAnEarlierStatusKills()
        {
            bool alive = true;
            var sources = new List<string>();
            var amounts = new List<float>();
            var status = new SurvivorsEnemyStatusEffects(() => alive, (amount, source) =>
            {
                amounts.Add(amount);
                sources.Add(source);
                alive = false;
            }, () => { });
            status.ApplyDamageOverTime(4f, 2f, "poison", "ignored-original-source");
            status.ApplyDamageOverTime(6f, 3f, "poison", "ignored-original-source");
            status.ApplyDamageOverTime(10f, 1f, "status.survivors.bleed", "unused");
            status.ApplyDamageOverTime(10f, 1f, "status.survivors.burn", "unused");
            status.TickDamageOverTime(0.5f);
            CollectionAssert.AreEqual(new[] { "survivors.status.poison" }, sources);
            CollectionAssert.AreEqual(new[] { 2f }, amounts);
            status.Reset();
            Assert.IsFalse(status.IsBurning);
            Assert.IsFalse(status.IsMovementSlowed);
            alive = true;
            status.TickDamageOverTime(1f);
            Assert.AreEqual(1, sources.Count);
        }

        [Test]
        public void BurnUsesExistingWholeTickRuleAndRestoresPresentationAtExpiry()
        {
            float damage = 0f;
            int changes = 0;
            var status = new SurvivorsEnemyStatusEffects(() => true, (amount, _) => damage += amount, () => changes++);
            status.ApplyDamageOverTime(5f, 0.5f, "status.survivors.burn", "unused");
            status.TickDamageOverTime(1f);
            Assert.AreEqual(10f, damage, "Existing gameplay charges a whole simulation tick, including the final tick.");
            Assert.IsFalse(status.IsBurning);
            Assert.AreEqual(2, changes);
        }
    }
}
