using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsDraftFeedbackTests
    {
        [Test]
        public void LevelUpRecordsSelectionAndAudioBeforeRepeatedEvolutionReadAndLevelPulse()
        {
            var port = new Port(false, false);
            var selected = Upgrade();
            new SurvivorsDraftFeedback(port).PresentSelectedUpgrade(SurvivorsRewardSelectionKind.LevelUp, selected);
            Assert.That(port.Calls, Is.EqualTo(new[] { "selection:LevelUp", "choice-audio", "evolution:False", "evolution:False", "level-pulse", "reward-kind:LevelUp" }));
            Assert.That(port.Selected, Is.SameAs(selected));
            Assert.That(port.EvolutionResults, Is.Empty);
        }

        [Test]
        public void EligibilityIsReadAgainAfterEvolutionAudioInsteadOfCachingTheFirstAnswer()
        {
            var port = new Port(true, false);
            new SurvivorsDraftFeedback(port).PresentSelectedUpgrade(SurvivorsRewardSelectionKind.LevelUp, Upgrade());
            Assert.That(port.Calls, Is.EqualTo(new[] { "selection:LevelUp", "choice-audio", "evolution:True", "evolution-audio", "evolution:False", "level-pulse", "reward-kind:LevelUp" }));
            Assert.That(port.EvolutionResults, Is.Empty);
        }

        [Test]
        public void RewardUpgradeRechecksEvolutionThenOrdersJackpotBeforeSurge()
        {
            var selected = Upgrade();
            var port = new Port(false, false);
            var feedback = new SurvivorsDraftFeedback(port);
            feedback.PresentSelectedUpgrade(SurvivorsRewardSelectionKind.BossUpgrade, selected);
            Assert.That(port.Calls, Is.EqualTo(new[] { "selection:BossUpgrade", "choice-audio", "evolution:False", "reward-kind:BossUpgrade", "evolution:False", "jackpot:BossUpgrade", "surge:BossUpgrade" }));
            Assert.That(port.Selected, Is.SameAs(selected));
            port = new Port(false, true);
            new SurvivorsDraftFeedback(port).PresentSelectedUpgrade(SurvivorsRewardSelectionKind.EliteUpgrade, selected);
            Assert.That(port.Calls, Is.EqualTo(new[] { "selection:EliteUpgrade", "choice-audio", "evolution:False", "reward-kind:EliteUpgrade", "evolution:True" }));
        }

        [Test]
        public void OtherSelectionKindsKeepShortCircuitAndStillForwardNullSelectionToHistory()
        {
            var port = new Port(false);
            new SurvivorsDraftFeedback(port).PresentSelectedUpgrade(SurvivorsRewardSelectionKind.BossRelic, null);
            Assert.That(port.Calls, Is.EqualTo(new[] { "selection:BossRelic", "choice-audio", "evolution:False", "reward-kind:BossRelic" }));
            Assert.That(port.Selected, Is.Null);
            Assert.That(port.EvolutionResults, Is.Empty, "Nonmatching kinds must not ask either conditional evolution query.");
        }

        [Test]
        public void OpeningLevelMetricPrecedesCardsAndRelicDraftWinsOverUpgradeDraft()
        {
            var port = new Port();
            var feedback = new SurvivorsDraftFeedback(port);
            var upgrade = new RunUpgradeDraft(new[] { Upgrade() });
            var relic = new SurvivorsRelicDraft(Array.Empty<SurvivorsRelicDefinition>());
            feedback.PresentDraft(SurvivorsRewardSelectionKind.LevelUp, upgrade, relic, true);
            Assert.That(port.Calls, Is.EqualTo(new[] { "first-level-draft", "relic-cards" }));
            Assert.That(port.RelicDraft, Is.SameAs(relic));
            Assert.That(port.UpgradeDraft, Is.Null);
            port.Calls.Clear();
            feedback.PresentDraft(SurvivorsRewardSelectionKind.LevelUp, upgrade, null, false);
            Assert.That(port.Calls, Is.EqualTo(new[] { "upgrade-cards:LevelUp" }));
            Assert.That(port.UpgradeDraft, Is.SameAs(upgrade));
            port.Calls.Clear();
            feedback.PresentDraft(SurvivorsRewardSelectionKind.BossUpgrade, null, null, true);
            Assert.That(port.Calls, Is.EqualTo(new[] { "upgrade-cards:BossUpgrade" }));
            Assert.That(port.UpgradeDraft, Is.Null);
        }

        [Test]
        public void DraftOpeningKeepsLevelAudioBeforePulseAndBossRelicCountBeforeRoleFallback()
        {
            var port = new Port();
            var feedback = new SurvivorsDraftFeedback(port);
            feedback.PlayDraftOpened(SurvivorsRewardSelectionKind.LevelUp, SurvivorsEnemyRole.Boss);
            Assert.That(port.Calls, Is.EqualTo(new[] { "level-audio", "pulse:True:34" }));
            port.Calls.Clear();
            feedback.PlayDraftOpened(SurvivorsRewardSelectionKind.BossRelic, SurvivorsEnemyRole.Boss);
            feedback.PlayDraftOpened(SurvivorsRewardSelectionKind.BossUpgrade, SurvivorsEnemyRole.Boss);
            feedback.PlayDraftOpened(SurvivorsRewardSelectionKind.EliteUpgrade, SurvivorsEnemyRole.Elite);
            Assert.That(port.Calls, Is.EqualTo(new[] { "pulse:False:44", "pulse:False:72", "pulse:False:54" }));
        }

        private static RunUpgradeDefinition Upgrade() => new RunUpgradeDefinition(new RunUpgradeId("test.upgrade"),
            RunUpgradeRarity.Common, 1, 3, new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1) });
        private sealed class Port : ISurvivorsDraftFeedbackPort
        {
            internal readonly List<string> Calls = new List<string>();
            internal readonly Queue<bool> EvolutionResults;
            internal RunUpgradeDefinition Selected;
            internal RunUpgradeDraft UpgradeDraft;
            internal SurvivorsRelicDraft RelicDraft;
            internal Port(params bool[] evolutionResults) => EvolutionResults = new Queue<bool>(evolutionResults);
            public void RecordSelection(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition selected) { Selected = selected; Calls.Add("selection:" + kind); }
            public void PlayChoiceAudio() => Calls.Add("choice-audio");
            public bool IsEvolution(RunUpgradeDefinition selected) { bool value = EvolutionResults.Dequeue(); Calls.Add("evolution:" + value); return value; }
            public void PlayEvolutionAudio() => Calls.Add("evolution-audio");
            public void TriggerLevelPulse(RunUpgradeDefinition selected) { Assert.That(selected, Is.SameAs(Selected)); Calls.Add("level-pulse"); }
            public bool IsRewardUpgradeKind(SurvivorsRewardSelectionKind kind) { Calls.Add("reward-kind:" + kind); return kind == SurvivorsRewardSelectionKind.EliteUpgrade || kind == SurvivorsRewardSelectionKind.BossUpgrade; }
            public void TriggerJackpot(RunUpgradeDefinition selected, SurvivorsRewardSelectionKind kind) { Assert.That(selected, Is.SameAs(Selected)); Calls.Add("jackpot:" + kind); }
            public void TriggerSurge(RunUpgradeDefinition selected, SurvivorsRewardSelectionKind kind) { Assert.That(selected, Is.SameAs(Selected)); Calls.Add("surge:" + kind); }
            public void RecordFirstLevelUpDraft() => Calls.Add("first-level-draft");
            public void RecordRelicCards(SurvivorsRelicDraft draft) { RelicDraft = draft; Calls.Add("relic-cards"); }
            public void RecordUpgradeCards(SurvivorsRewardSelectionKind kind, RunUpgradeDraft draft) { UpgradeDraft = draft; Calls.Add("upgrade-cards:" + kind); }
            public void PlayLevelUpAudio() => Calls.Add("level-audio");
            public void PlayOpeningPulse(bool levelUp, int count) => Calls.Add("pulse:" + levelUp + ":" + count);
        }
    }
}
