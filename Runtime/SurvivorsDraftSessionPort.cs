using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsDraftSessionPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        int RerollCharges { get; }
        int RelicSeed { get; }
        void ResetScroll();
        void ApplyUpgrade(RunUpgradeDefinition upgrade);
        void RecordDirectUpgrade(RunUpgradeDefinition upgrade);
        void PresentSelectedUpgrade(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition upgrade);
        void PresentDraft(SurvivorsRewardSelectionKind kind, RunUpgradeDraft upgradeDraft, SurvivorsRelicDraft relicDraft, bool opening);
        void PlayDraftOpened(SurvivorsRewardSelectionKind kind, SurvivorsEnemyRole role);
        void PlayReroll();
        void PlayBanish();
        void GrantSkipReward(SurvivorsRewardSelectionKind kind);
        void EnterVictory();
    }
}
