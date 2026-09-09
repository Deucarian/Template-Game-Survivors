using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRewardFeedbackHistoryTests
    {
        private Host _host;
        private SurvivorsRewardFeedbackHistory _history;

        [SetUp]
        public void SetUp()
        {
            _host = new Host();
            _history = new SurvivorsRewardFeedbackHistory(_host);
            _host.History = _history;
        }

        [Test]
        public void EmptyAndNullDraftsDoNotEraseLastCardObservation()
        {
            _history.RecordRewardCardPresentation(SurvivorsRewardSelectionKind.EliteUpgrade,
                new RunUpgradeDraft(new[] { Upgrade("first", RunUpgradeRarity.Rare), null }));
            Assert.AreEqual(2, _history.RewardCardPresentationCount);
            string label = _history.LastRewardCardPresentationLabel;
            StringAssert.Contains("2 cards, best Rare", label);
            _history.RecordRewardCardPresentation(SurvivorsRewardSelectionKind.LevelUp, null);
            _history.RecordRewardCardPresentation(SurvivorsRewardSelectionKind.LevelUp,
                new RunUpgradeDraft(Array.Empty<RunUpgradeDefinition>()));
            _history.RecordRewardCardPresentation((SurvivorsRelicDraft)null);
            _history.RecordRewardCardPresentation(new SurvivorsRelicDraft(null));
            Assert.AreEqual(2, _history.RewardCardPresentationCount);
            Assert.AreEqual(label, _history.LastRewardCardPresentationLabel);
            Assert.IsEmpty(_host.Events);
        }

        [Test]
        public void UpgradeAndRelicCardsShareCountButKeepLatestLabel()
        {
            _history.RecordRewardCardPresentation(SurvivorsRewardSelectionKind.LevelUp,
                new RunUpgradeDraft(new[] { Upgrade("common"), Upgrade("epic", RunUpgradeRarity.Epic) }));
            _history.RecordRewardCardPresentation(new SurvivorsRelicDraft(new[] { Relic(), null, Relic() }));
            Assert.AreEqual(5, _history.RewardCardPresentationCount);
            Assert.AreEqual("Boss Relic - 3 cards", _history.LastRewardCardPresentationLabel);
            Assert.AreEqual(0, _history.RewardSelectionFeedbackCount);
        }

        [Test]
        public void HighestRaritySkipsNullsAndKeepsCommonFloor()
        {
            Assert.AreEqual(RunUpgradeRarity.Common, SurvivorsRewardFeedbackHistory.ResolveHighestRarity(null));
            Assert.AreEqual(RunUpgradeRarity.Common, SurvivorsRewardFeedbackHistory.ResolveHighestRarity(new RunUpgradeDefinition[] { null }));
            Assert.AreEqual(RunUpgradeRarity.Legendary, SurvivorsRewardFeedbackHistory.ResolveHighestRarity(
                new[] { Upgrade("epic", RunUpgradeRarity.Epic), null, Upgrade("legend", RunUpgradeRarity.Legendary), Upgrade("rare", RunUpgradeRarity.Rare) }));
        }

        [Test]
        public void SelectionPublishesHistoryBeforeBannerThenReadsBestMomentLive()
        {
            _host.AfterBanner = () => { _host.Name = "Renamed after banner"; _host.Evolution = true; };
            _history.RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind.LevelUp, Upgrade("id", RunUpgradeRarity.Rare));
            Assert.AreEqual("Level Up: Rare WeaponUpgrade - Authored name (Arc Bolt)", _history.LastRewardSelectionFeedbackLabel);
            Assert.AreEqual(1, _history.RewardSelectionFeedbackCount);
            Assert.AreEqual("Evolution acquired: Renamed after banner at 01:05", _history.BestMomentLabel);
            CollectionAssert.AreEqual(new[] { "name", "category", "affected", "banner:1", "name", "evolution", "time" }, _host.Events);
            Assert.AreEqual(2.35f, _host.Banner.RemainingSeconds);
            Assert.AreEqual(SurvivorsDraftCardFactory.ResolveRarityAccentColor(RunUpgradeRarity.Rare), _host.Banner.Accent);
        }

        [Test]
        public void EqualRarityReplacesBestLabelAndLowerRarityDoesNot()
        {
            _history.RecordBestRewardMoment(Upgrade("first", RunUpgradeRarity.Epic));
            _host.Name = "Second";
            _history.RecordBestRewardMoment(Upgrade("second", RunUpgradeRarity.Epic));
            Assert.AreEqual("Epic - Second", _history.HighestChosenRarityLabel);
            _host.Name = "Lower";
            _history.RecordBestRewardMoment(Upgrade("lower", RunUpgradeRarity.Rare));
            Assert.AreEqual("Highest rarity chosen: Epic - Second", _history.BestMomentLabel);
            Assert.AreEqual(RunUpgradeRarity.Epic, _history.HighestChosenRarity);
            Assert.AreEqual(0, _history.RewardSelectionFeedbackCount);
            Assert.IsEmpty(_host.Banner.Label);
        }

        [Test]
        public void LowerRarityEvolutionOverridesMomentWithoutReplacingHighestRarity()
        {
            _history.RecordBestRewardMoment(Upgrade("legend", RunUpgradeRarity.Legendary));
            _host.Name = "Evolution";
            _host.Evolution = true;
            _history.RecordBestRewardMoment(Upgrade("evolution", RunUpgradeRarity.Common));
            Assert.AreEqual("Evolution acquired: Evolution at 01:05", _history.BestMomentLabel);
            Assert.AreEqual("Legendary - Authored name", _history.HighestChosenRarityLabel);
        }

        [Test]
        public void NullSelectionsDoNotObservePortsOrChangeHistory()
        {
            _history.RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind.LevelUp, null);
            _history.RecordRelicSelectionFeedback(null);
            _history.RecordBestRewardMoment(null);
            Assert.AreEqual(0, _history.RewardSelectionFeedbackCount);
            Assert.IsEmpty(_history.LastRewardSelectionFeedbackLabel);
            Assert.IsEmpty(_host.Events);
        }

        [Test]
        public void BannerExpiryAndResetPreserveRetainedSelectionHistory()
        {
            _history.RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind.BossUpgrade, Upgrade("id"));
            string selected = _history.LastRewardSelectionFeedbackLabel;
            string best = _history.BestMomentLabel;
            _host.Banner.Tick(10f);
            Assert.IsEmpty(_host.Banner.Label);
            _host.Banner.Reset();
            Assert.AreEqual(selected, _history.LastRewardSelectionFeedbackLabel);
            Assert.AreEqual(best, _history.BestMomentLabel);
            Assert.AreEqual(1, _history.RewardSelectionFeedbackCount);
        }

        [Test]
        public void RelicPublishesThenShowsBannerBeforeAudioWithoutChangingBestMoment()
        {
            _history.RecordBestRewardMoment(Upgrade("best", RunUpgradeRarity.Epic));
            string best = _history.BestMomentLabel;
            _host.Events.Clear();
            SurvivorsRelicDefinition relic = Relic();
            _history.RecordRelicSelectionFeedback(relic);
            Assert.AreEqual("Boss Relic: Authored relic - Relic effects", _history.LastRewardSelectionFeedbackLabel);
            CollectionAssert.AreEqual(new[] { "relic-summary", "banner:1", "relic-audio" }, _host.Events);
            Assert.AreEqual(SurvivorsDraftCardFactory.ResolveRelicAccentColor(relic), _host.Banner.Accent);
            Assert.AreEqual(best, _history.BestMomentLabel);
        }

        [Test]
        public void SkipReadsCurrentRewardCopyThenPublishesBeforeBannerAndAudio()
        {
            _host.SkipAmount = 7;
            _host.Currency = "Tokens";
            _history.RecordRewardSkipFeedback(SurvivorsRewardSelectionKind.LevelUp);
            Assert.AreEqual("Level Up skipped +7 Tokens", _history.LastRewardSelectionFeedbackLabel);
            CollectionAssert.AreEqual(new[] { "skip-amount", "currency", "banner:1", "skip-audio" }, _host.Events);
            Assert.AreEqual(new Color(0.72f, 0.84f, 0.9f), _host.Banner.Accent);
            Assert.IsEmpty(_history.BestMomentLabel);
        }

        [Test]
        public void FailedBannerRetainsPublishedCountAndLabelButDoesNotRecordBestMoment()
        {
            _host.AfterBanner = () => throw new InvalidOperationException("fixture");
            Assert.Throws<InvalidOperationException>(() => _history.RecordRewardSelectionFeedback(
                SurvivorsRewardSelectionKind.LevelUp, Upgrade("id", RunUpgradeRarity.Epic)));
            Assert.AreEqual(1, _history.RewardSelectionFeedbackCount);
            StringAssert.Contains("Epic", _history.LastRewardSelectionFeedbackLabel);
            Assert.IsEmpty(_history.BestMomentLabel);
            Assert.IsEmpty(_history.HighestChosenRarityLabel);
        }

        [Test]
        public void SeparateRunResetPhasesDoNotResetBorrowedBannerOrEachOther()
        {
            _history.RecordRewardCardPresentation(new SurvivorsRelicDraft(new[] { Relic() }));
            _history.RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind.LevelUp, Upgrade("id", RunUpgradeRarity.Rare));
            _history.ResetBestMoment();
            Assert.AreEqual(RunUpgradeRarity.Common, _history.HighestChosenRarity);
            Assert.IsEmpty(_history.BestMomentLabel);
            Assert.IsEmpty(_history.HighestChosenRarityLabel);
            Assert.AreEqual(1, _history.RewardCardPresentationCount);
            Assert.AreEqual(1, _history.RewardSelectionFeedbackCount);
            string banner = _host.Banner.Label;
            _history.RecordBestRewardMoment(Upgrade("new", RunUpgradeRarity.Epic));
            _history.ResetFeedbackHistory();
            Assert.AreEqual(0, _history.RewardCardPresentationCount);
            Assert.AreEqual(0, _history.RewardSelectionFeedbackCount);
            Assert.IsEmpty(_history.LastRewardCardPresentationLabel);
            Assert.IsEmpty(_history.LastRewardSelectionFeedbackLabel);
            Assert.AreEqual(RunUpgradeRarity.Epic, _history.HighestChosenRarity);
            Assert.AreEqual(banner, _host.Banner.Label);
        }

        private static RunUpgradeDefinition Upgrade(string id, RunUpgradeRarity rarity = RunUpgradeRarity.Common) =>
            new RunUpgradeDefinition(new RunUpgradeId(id), rarity, 1, 1, new[] {
                new RunUpgradeEffectDescriptor(new RunUpgradeEffectId("effect"), new RunUpgradeTargetId("target"), 1d) });

        private static SurvivorsRelicDefinition Relic() => new SurvivorsRelicDefinition(
            "relic", "Authored relic", "target", "effect", SurvivorsRelicEffectKind.DamageBonus, 1f, 1);

        private sealed class Host : ISurvivorsRewardFeedbackPort
        {
            public readonly List<string> Events = new List<string>();
            public readonly SurvivorsFeedbackBannerPresenter Banner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Reward);
            public SurvivorsRewardFeedbackHistory History;
            public string Name = "Authored name";
            public string Currency = "Blood Shards";
            public int SkipAmount = 2;
            public bool Evolution;
            public Action AfterBanner;
            public string ResolveUpgradeDisplayName(RunUpgradeId id) { Events.Add("name"); return Name; }
            public SurvivorsRunUpgradeCategory ResolveCurrentUpgradeCategory(RunUpgradeDefinition selected) { Events.Add("category"); return SurvivorsRunUpgradeCategory.WeaponUpgrade; }
            public string ResolveUpgradeAffectedLabel(RunUpgradeDefinition selected) { Events.Add("affected"); return "Arc Bolt"; }
            public bool IsEvolutionUpgrade(RunUpgradeDefinition selected) { Events.Add("evolution"); return Evolution; }
            public string FormatRelicEffectSummary(SurvivorsRelicDefinition selected) { Events.Add("relic-summary"); return "Relic effects"; }
            public float RunTimeSeconds { get { Events.Add("time"); return 65f; } }
            public int DraftSkipBloodShards { get { Events.Add("skip-amount"); return SkipAmount; } }
            public string CurrencyRewardLabel { get { Events.Add("currency"); return Currency; } }
            public void ShowRewardBanner(string label, float duration, Color color)
            {
                Assert.AreEqual(History.LastRewardSelectionFeedbackLabel, label);
                Events.Add("banner:" + History.RewardSelectionFeedbackCount);
                Banner.Show(label, duration, color);
                AfterBanner?.Invoke();
            }
            public void PlayRelicAudio() => Events.Add("relic-audio");
            public void PlaySkipAudio() => Events.Add("skip-audio");
        }
    }
}
