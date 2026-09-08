using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsRewardFeedbackPort
    {
        string ResolveUpgradeDisplayName(RunUpgradeId id);
        SurvivorsRunUpgradeCategory ResolveCurrentUpgradeCategory(RunUpgradeDefinition selected);
        string ResolveUpgradeAffectedLabel(RunUpgradeDefinition selected);
        bool IsEvolutionUpgrade(RunUpgradeDefinition selected);
        string FormatRelicEffectSummary(SurvivorsRelicDefinition selected);
        float RunTimeSeconds { get; }
        int DraftSkipBloodShards { get; }
        string CurrencyRewardLabel { get; }
        void ShowRewardBanner(string label, float duration, Color color);
        void PlayRelicAudio();
        void PlaySkipAudio();
    }
}
