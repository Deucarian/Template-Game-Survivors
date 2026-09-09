using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsProgressionFeedbackPort
    {
        void EnsureProfile();
        SurvivorsMetaProgressionDefinition MetaDefinition { get; }
        int GetPersistentRank(string id);
        string ResolveUpgradeDisplayName(RunUpgradeId id);
        string ResolveClassDisplayName(string classId, string fallback);
        string CurrencyRewardLabel { get; }
        string ProgressionRewardLabel { get; }
        void RecordEvolutionEligibility();
        void PlayEvolutionPulse(int count);
        void PlayClassUnlockPulse();
    }

    /// <summary>Retains progression feedback history while borrowing transient banners and authored/profile reads.</summary>
    internal sealed class SurvivorsProgressionFeedback
    {
        private const float RewardFeedbackDurationSeconds = 2.35f;
        private const float ClassUnlockRewardFeedbackDurationSeconds = 2.65f;
        private const float EvolutionReadyFeedbackDurationSeconds = 2.4f;
        private readonly ISurvivorsProgressionFeedbackPort _port;
        private readonly SurvivorsFeedbackBannerPresenter _rewardBanner, _classUnlockRewardBanner, _evolutionReadyBanner;
        public SurvivorsProgressionFeedback(ISurvivorsProgressionFeedbackPort port, SurvivorsFeedbackBannerPresenter reward,
            SurvivorsFeedbackBannerPresenter classUnlock, SurvivorsFeedbackBannerPresenter evolution)
        { _port = port; _rewardBanner = reward; _classUnlockRewardBanner = classUnlock; _evolutionReadyBanner = evolution; }
        public string LastMetaUpgradePurchaseFeedbackLabel { get; private set; } = string.Empty;
        public string LastResultClassSelectionFeedbackLabel { get; private set; } = string.Empty;
        public int EvolutionGoalFeedbackCount { get; private set; }
        public string LastEvolutionGoalFeedbackLabel { get; private set; } = string.Empty;
        public int EvolutionReadyFeedbackCount { get; private set; }
        public string LastEvolutionReadyFeedbackLabel { get; private set; } = string.Empty;
        public string LastClassUnlockRewardFeedbackLabel { get; private set; } = string.Empty;
        public void ResetHistory()
        {
            LastMetaUpgradePurchaseFeedbackLabel = string.Empty;
            LastResultClassSelectionFeedbackLabel = string.Empty;
            EvolutionGoalFeedbackCount = 0;
            LastEvolutionGoalFeedbackLabel = string.Empty;
            EvolutionReadyFeedbackCount = 0;
            LastEvolutionReadyFeedbackLabel = string.Empty;
            LastClassUnlockRewardFeedbackLabel = string.Empty;
        }

        public void RecordResultClassSelectionFeedback(SurvivorsClassDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            LastResultClassSelectionFeedbackLabel = "Next Run Class: " + selected.DisplayName;
            _rewardBanner.Show(LastResultClassSelectionFeedbackLabel, RewardFeedbackDurationSeconds, new Color(0.8f, 0.58f, 1f));
        }

        public void RecordMetaUpgradePurchaseFeedback(string id)
        {
            _port.EnsureProfile();
            SurvivorsMetaProgressionDefinition definition = _port.MetaDefinition;
            string displayName = id;
            if (definition.TryGetPersistentUpgrade(id, out SurvivorsPersistentUpgradeDefinition upgrade))
            {
                displayName = upgrade.DisplayName;
            }

            int rank = _port.GetPersistentRank(id);
            LastMetaUpgradePurchaseFeedbackLabel = $"Meta Upgrade: {displayName} rank {rank}";
            _rewardBanner.Show(LastMetaUpgradePurchaseFeedbackLabel, RewardFeedbackDurationSeconds, new Color(0.45f, 0.95f, 0.76f));
        }

        public void RecordEvolutionGoalFeedback(RunUpgradeDefinition evolution, RunUpgradeDefinition missingPassive)
        {
            string evolutionName = evolution == null ? "Evolution" : _port.ResolveUpgradeDisplayName(evolution.Id);
            string passiveName = missingPassive == null ? "matching passive" : _port.ResolveUpgradeDisplayName(missingPassive.Id);
            _evolutionReadyBanner.Show($"Evolution Goal: {passiveName} for {evolutionName}", EvolutionReadyFeedbackDurationSeconds, Color.white);
            EvolutionGoalFeedbackCount++;
            LastEvolutionGoalFeedbackLabel = _evolutionReadyBanner.Label;
            _port.PlayEvolutionPulse(22);
        }

        public void RecordEvolutionReadyFeedback(RunUpgradeDefinition evolution)
        {
            _port.RecordEvolutionEligibility();
            string name = _port.ResolveUpgradeDisplayName(evolution.Id);
            _evolutionReadyBanner.Show($"Evolution Ready: {name}", EvolutionReadyFeedbackDurationSeconds, Color.white);
            EvolutionReadyFeedbackCount++;
            LastEvolutionReadyFeedbackLabel = _evolutionReadyBanner.Label;
            _port.PlayEvolutionPulse(36);
        }

        public void RecordClassUnlockRewardFeedback()
        {
            LastClassUnlockRewardFeedbackLabel = "Class Unlocked: " + _port.ResolveClassDisplayName(BasicSurvivorsGame.EmberVanguardClassId, "Ember Vanguard");
            SurvivorsMetaProgressionDefinition definition = _port.MetaDefinition;
            if (definition.TryGetReward(BasicSurvivorsGame.EmberVanguardUnlockRewardId, out SurvivorsRewardDefinition reward))
            {
                LastClassUnlockRewardFeedbackLabel += $" +{reward.CurrencyAmount} {_port.CurrencyRewardLabel} +{reward.TrackAmount} {_port.ProgressionRewardLabel}";
            }

            _classUnlockRewardBanner.Show(LastClassUnlockRewardFeedbackLabel, ClassUnlockRewardFeedbackDurationSeconds, Color.white);
            _port.PlayClassUnlockPulse();
        }

    }
}
