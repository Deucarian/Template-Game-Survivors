using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsExperienceArithmeticTests
    {
        [Test]
        public void SeparateNormalBonusesRetainOriginalClampAndRoundOrder()
        {
            float runBonus = 0.05f;
            float surgeBonus = 0.3f;
            int amount = 10;
            int original = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, amount) *
                Mathf.Max(0.1f, 1f + runBonus + surgeBonus)));
            var progression = new SurvivorsExperienceProgression();
            int gained = progression.Gain(amount, runBonus, surgeBonus, Tuning());
            Assert.That(gained, Is.EqualTo(original));
            Assert.That(progression.ExperienceCollected, Is.EqualTo(original));
            Assert.That(progression.Experience, Is.EqualTo(original));
        }

        [Test]
        public void FiniteCancellationKeepsBaseAdditionBeforeSurgeAndMinimumMultiplier()
        {
            var progression = new SurvivorsExperienceProgression();
            // Pre-summing these finite bonuses would retain the base 1 and incorrectly grant 100 XP.
            int gained = progression.Gain(100, -1e30f, 1e30f, Tuning());
            Assert.That(gained, Is.EqualTo(10));
            Assert.That(progression.ExperienceCollected, Is.EqualTo(10));
            Assert.That(progression.Experience, Is.EqualTo(10));
            Assert.That(progression.PendingLevelUps, Is.Zero);
        }

        private static SurvivorsTemplateTuning Tuning() => new SurvivorsTemplateTuning
        {
            ExperienceRequiredBase = 1000,
            ExperienceRequiredPerLevel = 0
        };
    }
}
