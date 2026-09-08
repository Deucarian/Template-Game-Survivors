using System;

using System.Collections.Generic;

using Deucarian.Common;

using Deucarian.Combat;

using Deucarian.GameplayFoundation;

using Deucarian.Persistence;

using Deucarian.Persistence.Unity;

using Deucarian.Projectiles;

using Deucarian.RunUpgrades;

using Deucarian.WeaponSystems;

using Deucarian.WorldSpawning;

using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    // Presentation commands and retained feedback history bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private void PlayFeedback(ParticleSystem particles, Vector3 position, int count, AudioClip clip, string audioEventId = null, float audioThrottleSeconds = 0f) => FeedbackPulses.Play(particles, position, count, clip, audioEventId, audioThrottleSeconds);

        private SurvivorsFeedbackPulses FeedbackPulses => _feedbackPulses ?? (_feedbackPulses = new SurvivorsFeedbackPulses(clip => AudioPresentation.Play(clip), PlayAudioEvent));

        private void BuildFeedbackPresentation()
        { FeedbackPulses.Build(_worldRoot, ActiveUiTheme); AudioPresentation.Build(_feedbackRoot); }

        private void RecordNewlyEligibleEvolutionFeedback() => EvolutionAnnouncements.Refresh();

        private void RecordRewardCardPresentation(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDraft draft) => RewardHistory.RecordRewardCardPresentation(selectionKind, draft);

        private void RecordRewardCardPresentation(SurvivorsRelicDraft draft) => RewardHistory.RecordRewardCardPresentation(draft);

        private void RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDefinition selected) => RewardHistory.RecordRewardSelectionFeedback(selectionKind, selected);

        private void RecordBestRewardMoment(RunUpgradeDefinition selected) => RewardHistory.RecordBestRewardMoment(selected);

        private void RecordRelicSelectionFeedback(SurvivorsRelicDefinition selected) => RewardHistory.RecordRelicSelectionFeedback(selected);

        private void RecordRewardSkipFeedback(SurvivorsRewardSelectionKind selectionKind) => RewardHistory.RecordRewardSkipFeedback(selectionKind);

        private SurvivorsRewardFeedbackHistory RewardHistory => _rewardHistory ?? (_rewardHistory = new SurvivorsRewardFeedbackHistory(this));

        string ISurvivorsRewardFeedbackPort.ResolveUpgradeDisplayName(RunUpgradeId id) => ResolveUpgradeDisplayName(id);

        SurvivorsRunUpgradeCategory ISurvivorsRewardFeedbackPort.ResolveCurrentUpgradeCategory(RunUpgradeDefinition selected) => ResolveCurrentUpgradeCategory(selected);

        string ISurvivorsRewardFeedbackPort.ResolveUpgradeAffectedLabel(RunUpgradeDefinition selected) => ResolveUpgradeAffectedLabel(selected);

        bool ISurvivorsRewardFeedbackPort.IsEvolutionUpgrade(RunUpgradeDefinition selected) => IsEvolutionUpgrade(selected);

        string ISurvivorsRewardFeedbackPort.FormatRelicEffectSummary(SurvivorsRelicDefinition selected) => FormatRelicEffectSummary(selected);

        float ISurvivorsRewardFeedbackPort.RunTimeSeconds => RunTimeSeconds;

        int ISurvivorsRewardFeedbackPort.DraftSkipBloodShards => DraftSkipBloodShards;

        string ISurvivorsRewardFeedbackPort.CurrencyRewardLabel => CurrencyRewardLabel;

        void ISurvivorsRewardFeedbackPort.ShowRewardBanner(string label, float duration, Color color) => _rewardBanner.Show(label, duration, color);

        void ISurvivorsRewardFeedbackPort.PlayRelicAudio() => PlayAudioEvent(AudioEventRelic, _bossClip, 0.08f);

        void ISurvivorsRewardFeedbackPort.PlaySkipAudio() => PlayAudioEvent(AudioEventDraftSkip, _pickupClip, 0.08f);

        internal void RecordEnemyDamageFeedback(SurvivorsEnemyActor enemy, DamageResult damage) { if (enemy == null) return; DamageFeedback.RecordEnemyDamageFeedback(enemy, damage); }

        private static float ResolveDamagePopupAmount(DamageResult damage) => SurvivorsDamageFeedback.ResolveDamagePopupAmount(damage);

        internal void RecordEnemyRangedAttackDodgeFeedback(SurvivorsEnemyActor enemy) { if (enemy == null) return; RangedDodgeRewards.RecordEnemyRangedAttackDodgeFeedback(enemy); }

        private Vector3 ResolveRangedDodgeRewardPosition(SurvivorsEnemyActor enemy) => RangedDodgeRewards.ResolveRangedDodgeRewardPosition(enemy == null ? null : (ISurvivorsFeedbackEnemy)enemy);

        private static string ResolveRangedDodgeFallbackLabel(SurvivorsEnemyRole role) => SurvivorsRangedDodgeRewards.ResolveRangedDodgeFallbackLabel(role);

        private SurvivorsDamageFeedback DamageFeedback => _hitFeedback ?? (_hitFeedback = new SurvivorsDamageFeedback(this));

        private SurvivorsRangedDodgeRewards RangedDodgeRewards => _rangedDodgeRewards ?? (_rangedDodgeRewards = new SurvivorsRangedDodgeRewards(this));

        private SurvivorsCombatFeedbackPresenter CombatFeedback => _combatFeedback ?? (_combatFeedback = new SurvivorsCombatFeedbackPresenter(() => _feedbackRoot));

        private SurvivorsThreatTelegraphPresenter ThreatTelegraphs => _threatTelegraphs ?? (_threatTelegraphs = new SurvivorsThreatTelegraphPresenter(() => _feedbackRoot));

        private SurvivorsRewardDropPresenter RewardDrops => _rewardDrops ?? (_rewardDrops = new SurvivorsRewardDropPresenter(() => _feedbackRoot, () => ActiveUiTheme));

        private void TickExperienceComboFeedback(float deltaTime) => ExperienceRhythm.TickExperienceComboFeedback(deltaTime);

        private Transform _feedbackRoot => _feedbackPulses?.Root;

        private ParticleSystem _spawnPulse => _feedbackPulses?.Spawn;

        private ParticleSystem _firePulse => _feedbackPulses?.Fire;

        private ParticleSystem _killPulse => _feedbackPulses?.Kill;

        private ParticleSystem _pickupPulse => _feedbackPulses?.Pickup;

        private ParticleSystem _levelUpPulse => _feedbackPulses?.LevelUp;

        private ParticleSystem _bossPulse => _feedbackPulses?.Boss;

        private AudioClip _spawnClip => AudioPresentation.SpawnClip;

        private AudioClip _fireClip => AudioPresentation.FireClip;

        private AudioClip _killClip => AudioPresentation.KillClip;

        private AudioClip _pickupClip => AudioPresentation.PickupClip;

        private AudioClip _levelUpClip => AudioPresentation.LevelUpClip;

        private AudioClip _bossClip => AudioPresentation.BossClip;

        private AudioClip _dangerClip => AudioPresentation.DangerClip;

        private SurvivorsAudioPresenter AudioPresentation => _audioPresentation ??
            (_audioPresentation = new SurvivorsAudioPresenter(() => ActiveUiTheme, () => Time.unscaledTime));

        private SurvivorsAudioEventRouter _audioEvents => AudioPresentation.Events;

        private string _highestChosenRarityLabel => RewardHistory.HighestChosenRarityLabel;

        private string _bestMomentLabel => RewardHistory.BestMomentLabel;

        public int DamagePopupSpawnCount => _damageFeedback.SpawnCount;

        public int EnemyDeathEffectCount => CombatFeedback.EnemyDeathEffectCount;

        public int EnemyRangedAttackFeedbackCount => CombatFeedback.EnemyRangedAttackFeedbackCount;

        public int EnemyRangedAttackDodgeFeedbackCount => RangedDodgeRewards.EnemyRangedAttackDodgeFeedbackCount;

        public string LastEnemyRangedAttackDodgeFeedbackLabel => RangedDodgeRewards.LastEnemyRangedAttackDodgeFeedbackLabel;

        public int MajorRewardDropFeedbackCount => RewardDrops.MajorRewardDropFeedbackCount;

        public int MajorThreatSlamTelegraphEffectCount => ThreatTelegraphs.MajorThreatSlamTelegraphEffectCount;

        public int IncomingThreatTelegraphEffectCount => ThreatTelegraphs.IncomingThreatTelegraphEffectCount;

        public string LastIncomingThreatTelegraphLabel => ThreatTelegraphs.LastIncomingThreatTelegraphLabel;

        public int ExperienceComboFeedbackCount => ExperienceRhythm.ExperienceComboFeedbackCount;

        public string LastExperienceComboFeedbackLabel => ExperienceRhythm.LastExperienceComboFeedbackLabel;

        public int RewardCardPresentationCount => RewardHistory.RewardCardPresentationCount;

        public int RewardSelectionFeedbackCount => RewardHistory.RewardSelectionFeedbackCount;

        public string LastRewardCardPresentationLabel => RewardHistory.LastRewardCardPresentationLabel;

        public string LastRewardSelectionFeedbackLabel => RewardHistory.LastRewardSelectionFeedbackLabel;

        public string LastMajorRewardDropFeedbackLabel => RewardDrops.LastMajorRewardDropFeedbackLabel;

        public string LastMajorRewardCacheFeedbackLabel => MajorRewardPickupCache.LastMajorRewardCacheFeedbackLabel;

        public int ActiveDamagePopupCount => _damageFeedback.ActiveCount;

        public int ActiveEnemyDeathEffectCount => CombatFeedback.ActiveEnemyDeathEffectCount;

        public int ActiveEnemyRangedAttackFeedbackCount => CombatFeedback.ActiveEnemyRangedAttackFeedbackCount;

        public int ActiveMajorRewardDropFeedbackCount => RewardDrops.ActiveMajorRewardDropFeedbackCount;

        public int ActiveMajorThreatSlamTelegraphEffectCount => ThreatTelegraphs.ActiveMajorThreatSlamTelegraphEffectCount;

        public int ActiveIncomingThreatTelegraphEffectCount => ThreatTelegraphs.ActiveIncomingThreatTelegraphEffectCount;

        public bool IsAudioMuted => _audioEvents.Muted;

        public int AudioEventDispatchCount => _audioEvents.DispatchCount;

        public string LastAudioEventId => _audioEvents.LastEventId;

        public string ActiveRewardFeedbackLabel => _rewardBanner.RemainingSeconds > 0f ? _rewardBanner.Label : string.Empty;

        public float RewardFeedbackRemainingSeconds => Mathf.Max(0f, _rewardBanner.RemainingSeconds);

        public string ActiveClassUnlockRewardFeedbackLabel => _classUnlockRewardBanner.RemainingSeconds > 0f ? _classUnlockRewardBanner.Label : string.Empty;

        public float ClassUnlockRewardFeedbackRemainingSeconds => Mathf.Max(0f, _classUnlockRewardBanner.RemainingSeconds);

        public string ActiveEvolutionReadyFeedbackLabel => _evolutionReadyBanner.RemainingSeconds > 0f ? _evolutionReadyBanner.Label : string.Empty;

        public float EvolutionReadyFeedbackRemainingSeconds => Mathf.Max(0f, _evolutionReadyBanner.RemainingSeconds);

        public string ActiveExperienceComboFeedbackLabel => ExperienceRhythm.ActiveExperienceComboFeedbackLabel;

        public float ExperienceComboFeedbackRemainingSeconds => ExperienceRhythm.ExperienceComboFeedbackRemainingSeconds;

        public void SetAudioMutedForTest(bool muted)
        {
            _audioEvents.SetMuted(muted);
        }

        public bool DispatchAudioEventForTest(string eventId)
        {
            return PlayAudioEvent(eventId, _pickupClip, 0f);
        }

        private bool PlayAudioEvent(string eventId, AudioClip fallbackClip, float fallbackThrottleSeconds)
        {
            return AudioPresentation.PlayEvent(eventId, fallbackClip, fallbackThrottleSeconds);
        }

        private void RecordEnemyDeathEffect(Vector3 position, SurvivorsEnemyRole role, float radius) => CombatFeedback.RecordEnemyDeathEffect(position, role, radius);

        private void TickWorldFeedbackEffects(float deltaTime) => CombatFeedback.TickWorldFeedbackEffects(deltaTime);

        internal void RecordEnemyRangedAttackFeedback(Vector3 origin, Vector3 target, SurvivorsEnemyRole role) => CombatFeedback.RecordEnemyRangedAttackFeedback(origin, target, role);

        private void TickEnemyRangedAttackFeedbackEffects(float deltaTime) => CombatFeedback.TickEnemyRangedAttackFeedbackEffects(deltaTime);

        private void RecordMajorThreatSlamTelegraphEffect(Vector3 position, SurvivorsEnemyRole role, float radius, float durationSeconds) => ThreatTelegraphs.RecordMajorThreatSlamTelegraphEffect(position, role, radius, durationSeconds);

        private void TickMajorThreatSlamTelegraphEffects(float deltaTime) => ThreatTelegraphs.TickMajorThreatSlamTelegraphEffects(deltaTime);

        private void RecordIncomingThreatTelegraph(Vector3 position, SurvivorsEnemyRole role, string label, float radius, float durationSeconds) => ThreatTelegraphs.RecordIncomingThreatTelegraph(position, role, label, radius, durationSeconds);

        private void TickIncomingThreatTelegraphEffects(float deltaTime) => ThreatTelegraphs.TickIncomingThreatTelegraphEffects(deltaTime);

        private static float ResolveIncomingThreatTelegraphRadius(SurvivorsEnemyRole role) => SurvivorsThreatTelegraphPresenter.ResolveIncomingThreatTelegraphRadius(role);

        private void RecordMajorRewardDropFeedback(Vector3 position, SurvivorsEnemyRole role, float radius) => RewardDrops.RecordMajorRewardDropFeedback(position, role, radius);

        private void TickMajorRewardDropFeedbackEffects(float deltaTime) => RewardDrops.TickMajorRewardDropFeedbackEffects(deltaTime);

        private Color ResolveMajorRewardDropColor(SurvivorsEnemyRole role) => RewardDrops.ResolveMajorRewardDropColor(role);

        private static string ResolveMajorRewardDropLabel(SurvivorsEnemyRole role) => SurvivorsRewardDropPresenter.ResolveMajorRewardDropLabel(role);

        private void DrawRewardSelectionFeedback() => _rewardBanner.Draw(_rewardFeedbackStyle);

        private void DrawClassUnlockRewardFeedback() => _classUnlockRewardBanner.Draw(_rewardFeedbackStyle);

        private void DrawExperienceComboFeedback() => ExperienceRhythm.Banner.Draw(_rewardFeedbackStyle);

        private void DrawEvolutionReadyFeedback() => _evolutionReadyBanner.Draw(_rewardFeedbackStyle);

        private void DrawDamagePopups()
        {
            _damageFeedback.Draw(_camera != null ? _camera : Camera.main);
        }

        private void TickDamagePopups(float deltaTime)
        {
            _damageFeedback.Tick(deltaTime);
        }

        private void TickRewardFeedback(float deltaTime) => _rewardBanner.Tick(deltaTime);

        private void TickClassUnlockRewardFeedback(float deltaTime) => _classUnlockRewardBanner.Tick(deltaTime);

        private void TickEvolutionReadyFeedback(float deltaTime) => _evolutionReadyBanner.Tick(deltaTime);
    }
}
