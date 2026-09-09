using System;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsWeaponDiagnosticsTests
    {
        [Test]
        public void EveryTriggerRecordsItsOwnRetainedCounter()
        {
            var diagnostics = new SurvivorsWeaponDiagnostics();
            (Action record, Func<int> count)[] counters =
            {
                (diagnostics.RecordOrbitHit, () => diagnostics.OrbitHitCount),
                (diagnostics.RecordMeleeSwing, () => diagnostics.MeleeSwingCount),
                (diagnostics.RecordMeleeHit, () => diagnostics.MeleeHitCount),
                (diagnostics.RecordBurstPulse, () => diagnostics.BurstPulseCount),
                (diagnostics.RecordBurstHit, () => diagnostics.BurstHitCount),
                (diagnostics.RecordHitscanFire, () => diagnostics.HitscanFireCount),
                (diagnostics.RecordHitscanHit, () => diagnostics.HitscanHitCount),
                (() => diagnostics.RecordTempestPrismArcHit("a", "b"), () => diagnostics.TempestPrismArcHitCount),
                (diagnostics.RecordProjectilePierceHit, () => diagnostics.ProjectilePierceHitCount),
                (diagnostics.RecordProjectileChainHit, () => diagnostics.ProjectileChainHitCount),
                (diagnostics.RecordProjectileForkSpawn, () => diagnostics.ProjectileForkSpawnCount),
                (diagnostics.RecordProjectileReturnStart, () => diagnostics.ProjectileReturnStartCount),
                (() => diagnostics.RecordOrbitKnockback(false, "Orbit", "enemy"), () => diagnostics.OrbitKnockbackCount),
                (diagnostics.RecordPayloadThrow, () => diagnostics.PayloadThrowCount),
                (diagnostics.RecordPayloadPlaced, () => diagnostics.PayloadPlacedCount),
                (diagnostics.RecordPayloadDetonation, () => diagnostics.PayloadDetonationCount),
                (diagnostics.RecordPayloadExplosionHit, () => diagnostics.PayloadExplosionHitCount)
            };
            for (int i = 0; i < counters.Length; i++)
                for (int count = 0; count <= i; count++) counters[i].record();
            for (int i = 0; i < counters.Length; i++) Assert.That(counters[i].count(), Is.EqualTo(i + 1));
        }

        [Test]
        public void ResetPhasesOnlyClearTheirOwnCountersAndLabels()
        {
            var diagnostics = new SurvivorsWeaponDiagnostics();
            diagnostics.RecordTempestPrismArcHit("first", "second");
            diagnostics.RecordOrbitKnockback(true, "Halo", "Runner");
            diagnostics.RecordMeleeHit();
            diagnostics.RecordPayloadThrow();
            diagnostics.RecordPayloadPlaced();
            diagnostics.RecordPayloadDetonation();
            diagnostics.RecordPayloadExplosionHit();
            diagnostics.ResetPayloadDiagnostics();
            Assert.That(diagnostics.MeleeHitCount, Is.EqualTo(1));
            Assert.That(diagnostics.TempestPrismArcHitCount, Is.EqualTo(1));
            Assert.That(diagnostics.OrbitKnockbackCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.EqualTo("Crimson Aegis pushed Runner"));
            Assert.That(diagnostics.LastTempestPrismArcFeedbackLabel, Is.EqualTo("Tempest Prism arced from first to second"));
            Assert.That(new[] { diagnostics.PayloadThrowCount, diagnostics.PayloadPlacedCount,
                diagnostics.PayloadDetonationCount, diagnostics.PayloadExplosionHitCount }, Is.All.Zero);
            diagnostics.RecordPayloadThrow();
            diagnostics.ResetHitDiagnostics();
            Assert.That(diagnostics.PayloadThrowCount, Is.EqualTo(1));
            Assert.That(new[] { diagnostics.OrbitHitCount, diagnostics.MeleeSwingCount, diagnostics.MeleeHitCount,
                diagnostics.BurstPulseCount, diagnostics.BurstHitCount, diagnostics.HitscanFireCount, diagnostics.HitscanHitCount,
                diagnostics.TempestPrismArcHitCount, diagnostics.ProjectilePierceHitCount, diagnostics.ProjectileChainHitCount,
                diagnostics.ProjectileForkSpawnCount, diagnostics.ProjectileReturnStartCount, diagnostics.OrbitKnockbackCount }, Is.All.Zero);
            Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.Empty);
            Assert.That(diagnostics.LastTempestPrismArcFeedbackLabel, Is.Empty);
        }

        [Test]
        public void LabelsKeepNullWhitespaceAndCrimsonPrecedenceWithoutDroppingHistory()
        {
            var diagnostics = new SurvivorsWeaponDiagnostics();
            diagnostics.RecordTempestPrismArcHit(null, " ");
            Assert.That(diagnostics.LastTempestPrismArcFeedbackLabel, Is.EqualTo("Tempest Prism arced from target to nearby enemy"));
            diagnostics.RecordTempestPrismArcHit("  Boss  ", "Runner");
            Assert.That(diagnostics.LastTempestPrismArcFeedbackLabel, Is.EqualTo("Tempest Prism arced from   Boss   to Runner"));
            diagnostics.RecordOrbitKnockback(false, " ", "Runner");
            Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.EqualTo("Orbit pushed Runner"));
            diagnostics.RecordOrbitKnockback(true, "Other authored name", "Boss");
            Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.EqualTo("Crimson Aegis pushed Boss"));
            Assert.That(diagnostics.TempestPrismArcHitCount, Is.EqualTo(2));
            Assert.That(diagnostics.OrbitKnockbackCount, Is.EqualTo(2));
        }
    }
}
