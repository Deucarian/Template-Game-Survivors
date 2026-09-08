using System;
using Deucarian.Progression;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsProgressionChoiceLabelsTests
    {
        [Test]
        public void MissingDefinitionsKeepTheirDistinctEmptyAndBalancedFallbacks()
        {
            Assert.That(SurvivorsProgressionChoiceLabels.ClassStats(null), Is.EqualTo("Balanced"));
            Assert.That(SurvivorsProgressionChoiceLabels.ClassStartingWeapons(null), Is.EqualTo("0 weapons"));
            Assert.That(SurvivorsProgressionChoiceLabels.PersistentUpgradeEffect(null), Is.Empty);
            Assert.That(SurvivorsProgressionChoiceLabels.PersistentUpgradeOption(2, null, 3, 20, "Coins"), Is.Empty);
            Assert.That(SurvivorsProgressionChoiceLabels.ClassOption(2, null, true, "class"), Is.Empty);
        }

        [Test]
        public void ClassStatsPreserveSourceOrderDuplicatesNegativeSignsPrecisionAndUnknownSkipping()
        {
            var definition = Class(new[]
            {
                new SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind.MaxHealth, 12.34f), null,
                new SurvivorsClassStatModifierDefinition((SurvivorsClassStatKind)999, 100f),
                new SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind.MoveSpeed, -2.126f),
                new SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind.Damage, 0.126f),
                new SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind.MoveSpeed, 1f)
            });
            Assert.That(SurvivorsProgressionChoiceLabels.ClassStats(definition), Is.EqualTo("HP +12.3, Move +-2.13, Dmg +0.13, Move +1"));
            Assert.That(SurvivorsProgressionChoiceLabels.ClassStats(Class(new[] { new SurvivorsClassStatModifierDefinition((SurvivorsClassStatKind)999, 1f), null })), Is.EqualTo("Balanced"));
            Assert.That(definition.StartingStatModifiers.Count, Is.EqualTo(6));
        }

        [Test]
        public void StartingWeaponsUseTheResolvedDefinitionCountAndExistingSingularPluralCopy()
        {
            Assert.That(SurvivorsProgressionChoiceLabels.ClassStartingWeapons(Class(null)), Is.EqualTo("0 weapons"));
            Assert.That(SurvivorsProgressionChoiceLabels.ClassStartingWeapons(Class(null, "wand")), Is.EqualTo("1 weapon"));
            Assert.That(SurvivorsProgressionChoiceLabels.ClassStartingWeapons(Class(null, "wand", "frost")), Is.EqualTo("2 weapons"));
        }

        [Test]
        public void PersistentEffectsKeepAllFiveMappingsAndExactUnknownIdFallback()
        {
            string[] ids = { BasicSurvivorsGame.MetaDamageEffectId, BasicSurvivorsGame.MetaMaxHealthEffectId,
                BasicSurvivorsGame.MetaPickupRangeEffectId, BasicSurvivorsGame.MetaExperienceGainEffectId, BasicSurvivorsGame.MetaDraftRerollEffectId };
            string[] expected = { "+1.3 starting damage", "+1.3 max health", "+1.3 pickup range", "+126% XP gain", "+1 draft reroll" };
            for (int i = 0; i < ids.Length; i++) Assert.That(SurvivorsProgressionChoiceLabels.PersistentUpgradeEffect(Upgrade(ids[i], 1.26f)), Is.EqualTo(expected[i]));
            string differentCase = BasicSurvivorsGame.MetaDamageEffectId.ToUpperInvariant();
            Assert.That(SurvivorsProgressionChoiceLabels.PersistentUpgradeEffect(Upgrade(differentCase, 9f)), Is.EqualTo(differentCase));
        }

        [Test]
        public void PersistentOptionUsesObservedRankCostCurrencyAndClampsOnlyTheDisplayedNextRank()
        {
            var upgrade = Upgrade(BasicSurvivorsGame.MetaDamageEffectId, 1.26f);
            Assert.That(SurvivorsProgressionChoiceLabels.PersistentUpgradeOption(2, upgrade, 1, 87, "Prisms"),
                Is.EqualTo("3. Authored Upgrade rank 1->2/3 (87 Prisms) - +1.3 starting damage"));
            Assert.That(SurvivorsProgressionChoiceLabels.PersistentUpgradeOption(0, upgrade, 5, 0, "Shards"),
                Is.EqualTo("1. Authored Upgrade rank 5->3/3 (0 Shards) - +1.3 starting damage"));
            Assert.That(upgrade.RankCosts, Is.EqualTo(new[] { 3, 6, 9 }), "Formatting cannot recompute or change the supplied purchase observation.");
        }

        [TestCase(false, "class", "Selected")]
        [TestCase(true, "CLASS", "Unlocked")]
        [TestCase(false, "other", "Locked")]
        public void ClassOptionKeepsSelectedPrecedenceOrdinalIdentityAndTwoLineCopy(bool unlocked, string selected, string state)
        {
            Assert.That(SurvivorsProgressionChoiceLabels.ClassOption(1, Class(null, "wand"), unlocked, selected),
                Is.EqualTo("2. Authored Class [" + state + "]\nBalanced | 1 weapon"));
        }

        [Test]
        public void ClassNameReadsBorrowedLibraryAndKeepsMissingBlankAndOrdinalFallbacks()
        {
            var library = new SurvivorsClassLibraryDefinition(new[] { Class(null), new SurvivorsClassDefinition("", "", "", false, "", null) });
            Assert.That(SurvivorsProgressionChoiceLabels.ClassDisplayName(library, "class", "Fallback"), Is.EqualTo("Authored Class"));
            Assert.That(SurvivorsProgressionChoiceLabels.ClassDisplayName(library, "CLASS", "Fallback"), Is.EqualTo("Fallback"));
            Assert.That(SurvivorsProgressionChoiceLabels.ClassDisplayName(library, "", "Fallback"), Is.EqualTo("Fallback"));
            Assert.That(SurvivorsProgressionChoiceLabels.ClassDisplayName(null, "class", "Fallback"), Is.EqualTo("Fallback"));
        }

        private static SurvivorsClassDefinition Class(SurvivorsClassStatModifierDefinition[] modifiers, params string[] weapons)
            => new SurvivorsClassDefinition("class", "Authored Class", "", true, "", modifiers, weapons);
        private static SurvivorsPersistentUpgradeDefinition Upgrade(string effect, float amount)
            => new SurvivorsPersistentUpgradeDefinition(new ResearchNodeId("test.upgrade"), "Authored Upgrade", "player", effect, 3, new[] { 3, 6, 9 }, amount);
    }
}
