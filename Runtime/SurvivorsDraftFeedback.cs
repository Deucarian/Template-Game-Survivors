using System;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsDraftFeedbackPort
    {
        void RecordSelection(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition selected);
        void PlayChoiceAudio();
        bool IsEvolution(RunUpgradeDefinition selected);
        void PlayEvolutionAudio();
        void TriggerLevelPulse(RunUpgradeDefinition selected);
        bool IsRewardUpgradeKind(SurvivorsRewardSelectionKind kind);
        void TriggerJackpot(RunUpgradeDefinition selected, SurvivorsRewardSelectionKind kind);
        void TriggerSurge(RunUpgradeDefinition selected, SurvivorsRewardSelectionKind kind);
        void RecordFirstLevelUpDraft();
        void RecordRelicCards(SurvivorsRelicDraft draft);
        void RecordUpgradeCards(SurvivorsRewardSelectionKind kind, RunUpgradeDraft draft);
        void PlayLevelUpAudio();
        void PlayOpeningPulse(bool levelUp, int count);
    }

    /// <summary>Orders draft presentation and reward follow-up commands without owning draft or reward state.</summary>
    internal sealed class SurvivorsDraftFeedback
    {
        private readonly ISurvivorsDraftFeedbackPort _port;
        internal SurvivorsDraftFeedback(ISurvivorsDraftFeedbackPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal void PresentSelectedUpgrade(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition selected)
        {
            _port.RecordSelection(kind, selected);
            _port.PlayChoiceAudio();
            if (_port.IsEvolution(selected)) _port.PlayEvolutionAudio();
            if (kind == SurvivorsRewardSelectionKind.LevelUp && !_port.IsEvolution(selected))
                _port.TriggerLevelPulse(selected);
            if (_port.IsRewardUpgradeKind(kind) && !_port.IsEvolution(selected))
            {
                _port.TriggerJackpot(selected, kind);
                _port.TriggerSurge(selected, kind);
            }
        }

        internal void PresentDraft(SurvivorsRewardSelectionKind kind, RunUpgradeDraft upgradeDraft, SurvivorsRelicDraft relicDraft, bool opening)
        {
            if (opening && kind == SurvivorsRewardSelectionKind.LevelUp) _port.RecordFirstLevelUpDraft();
            if (relicDraft != null) _port.RecordRelicCards(relicDraft);
            else _port.RecordUpgradeCards(kind, upgradeDraft);
        }

        internal void PlayDraftOpened(SurvivorsRewardSelectionKind kind, SurvivorsEnemyRole role)
        {
            if (kind == SurvivorsRewardSelectionKind.LevelUp)
            {
                _port.PlayLevelUpAudio();
                _port.PlayOpeningPulse(true, 34);
            }
            else _port.PlayOpeningPulse(false, kind == SurvivorsRewardSelectionKind.BossRelic ? 44 : role == SurvivorsEnemyRole.Boss ? 72 : 54);
        }
    }
}
