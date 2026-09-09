using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns retained reward observations, independently of the transient banner.</summary>
    internal sealed class SurvivorsRewardFeedbackHistory
    {
        private const float RewardFeedbackDurationSeconds = 2.35f;
        private readonly ISurvivorsRewardFeedbackPort _port;

        public SurvivorsRewardFeedbackHistory(ISurvivorsRewardFeedbackPort port) { _port = port; }

        public int RewardCardPresentationCount { get; private set; }
        public int RewardSelectionFeedbackCount { get; private set; }
        public string LastRewardCardPresentationLabel { get; private set; } = string.Empty;
        public string LastRewardSelectionFeedbackLabel { get; private set; } = string.Empty;
        public RunUpgradeRarity HighestChosenRarity { get; private set; }
        public string HighestChosenRarityLabel { get; private set; } = string.Empty;
        public string BestMomentLabel { get; private set; } = string.Empty;

        public void ResetBestMoment()
        {
            HighestChosenRarity = RunUpgradeRarity.Common;
            HighestChosenRarityLabel = string.Empty;
            BestMomentLabel = string.Empty;
        }

        public void ResetFeedbackHistory()
        {
            RewardCardPresentationCount = 0;
            RewardSelectionFeedbackCount = 0;
            LastRewardCardPresentationLabel = string.Empty;
            LastRewardSelectionFeedbackLabel = string.Empty;
        }

        public void RecordRewardCardPresentation(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDraft draft)
        {
            int choiceCount = draft == null ? 0 : draft.Choices.Count;
            if (choiceCount <= 0)
            {
                return;
            }

            RewardCardPresentationCount += choiceCount;
            RunUpgradeRarity highestRarity = ResolveHighestRarity(draft.Choices);
            LastRewardCardPresentationLabel = $"{SurvivorsDraftSelectionRewards.ResolveRewardKindLabel(selectionKind)} - {choiceCount} cards, best {highestRarity}";
        }

        public void RecordRewardCardPresentation(SurvivorsRelicDraft draft)
        {
            int choiceCount = draft == null ? 0 : draft.Choices.Count;
            if (choiceCount <= 0)
            {
                return;
            }

            RewardCardPresentationCount += choiceCount;
            LastRewardCardPresentationLabel = $"Boss Relic - {choiceCount} cards";
        }

        public void RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            string name = _port.ResolveUpgradeDisplayName(selected.Id);
            string category = _port.ResolveCurrentUpgradeCategory(selected).ToString();
            string affected = _port.ResolveUpgradeAffectedLabel(selected);
            LastRewardSelectionFeedbackLabel = $"{SurvivorsDraftSelectionRewards.ResolveRewardKindLabel(selectionKind)}: {selected.Rarity} {category} - {name} ({affected})";
            RewardSelectionFeedbackCount++;
            _port.ShowRewardBanner(LastRewardSelectionFeedbackLabel, RewardFeedbackDurationSeconds, SurvivorsDraftCardFactory.ResolveRarityAccentColor(selected.Rarity));
            RecordBestRewardMoment(selected);
        }

        public void RecordBestRewardMoment(RunUpgradeDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            string name = _port.ResolveUpgradeDisplayName(selected.Id);
            if ((int)selected.Rarity >= (int)HighestChosenRarity)
            {
                HighestChosenRarity = selected.Rarity;
                HighestChosenRarityLabel = selected.Rarity + " - " + name;
                BestMomentLabel = "Highest rarity chosen: " + HighestChosenRarityLabel;
            }

            if (_port.IsEvolutionUpgrade(selected))
            {
                BestMomentLabel = "Evolution acquired: " + name + " at " + SurvivorsRunText.FormatRunTime(_port.RunTimeSeconds);
            }
        }

        public void RecordRelicSelectionFeedback(SurvivorsRelicDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            LastRewardSelectionFeedbackLabel = $"Boss Relic: {selected.DisplayName} - {_port.FormatRelicEffectSummary(selected)}";
            RewardSelectionFeedbackCount++;
            _port.ShowRewardBanner(LastRewardSelectionFeedbackLabel, RewardFeedbackDurationSeconds, SurvivorsDraftCardFactory.ResolveRelicAccentColor(selected));
            _port.PlayRelicAudio();
        }

        public void RecordRewardSkipFeedback(SurvivorsRewardSelectionKind selectionKind)
        {
            LastRewardSelectionFeedbackLabel = $"{SurvivorsDraftSelectionRewards.ResolveRewardKindLabel(selectionKind)} skipped +{_port.DraftSkipBloodShards} {_port.CurrencyRewardLabel}";
            RewardSelectionFeedbackCount++;
            _port.ShowRewardBanner(LastRewardSelectionFeedbackLabel, RewardFeedbackDurationSeconds, new Color(0.72f, 0.84f, 0.9f));
            _port.PlaySkipAudio();
        }

        public static RunUpgradeRarity ResolveHighestRarity(IReadOnlyList<RunUpgradeDefinition> choices)
        {
            RunUpgradeRarity highest = RunUpgradeRarity.Common;
            if (choices == null)
            {
                return highest;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                RunUpgradeDefinition choice = choices[i];
                if (choice != null && (int)choice.Rarity > (int)highest)
                {
                    highest = choice.Rarity;
                }
            }

            return highest;
        }
    }
}
