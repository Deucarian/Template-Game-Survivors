using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns active offers, selection charges, timeout and ordered continuation into queued rewards.</summary>
    internal sealed class SurvivorsDraftSession
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly SurvivorsDraftOfferGenerator _offers;
        private readonly SurvivorsRelicInventory _relics;
        private readonly SurvivorsRunSession _run;
        private readonly SurvivorsExperienceProgression _experience;
        private readonly ISurvivorsDraftSessionPort _port;
        private int _rerollIndex;
        private float _timer;
        private bool _pendingVictory;
        private bool _pendingRelic;
        public RunUpgradeDraft CurrentDraft { get; private set; }
        public SurvivorsRelicDraft CurrentRelicDraft { get; private set; }
        public SurvivorsRewardSelectionKind Kind { get; private set; }
        public int SelectedUpgradeCount { get; private set; }
        public int SelectedRewardUpgradeCount { get; private set; }
        public int RerollCount { get; private set; }
        public int BanishCount { get; private set; }
        public int SkipCount { get; private set; }
        public int AutoSelectCount { get; private set; }
        public int OpenCount { get; private set; }
        public int LevelUpOpenCount { get; private set; }
        public int EliteOpenCount { get; private set; }
        public int BossOpenCount { get; private set; }
        public int RelicOpenCount { get; private set; }
        public float RemainingSeconds => Mathf.Max(0f, _timer);
        public int RerollsRemaining => Mathf.Max(0, _port.RerollCharges - RerollCount);
        public int BanishesRemaining => Mathf.Max(0, _port.Tuning.DraftBanishCharges - BanishCount);
        public bool CanSkip => _run.State == SurvivorsRunState.LevelUp && IsUpgradeKind(Kind) && CurrentDraft != null;
        public bool CanReroll => CanSkip && RerollsRemaining > 0;
        public bool CanBanish => CanSkip && Kind != SurvivorsRewardSelectionKind.BossUpgrade && BanishesRemaining > 0;
        public SurvivorsDraftSession(SurvivorsRunBuildState build, SurvivorsDraftOfferGenerator offers, SurvivorsRelicInventory relics,
            SurvivorsRunSession run, SurvivorsExperienceProgression experience, ISurvivorsDraftSessionPort port)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _offers = offers ?? throw new ArgumentNullException(nameof(offers));
            _relics = relics ?? throw new ArgumentNullException(nameof(relics));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _experience = experience ?? throw new ArgumentNullException(nameof(experience));
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }
        public static bool IsUpgradeKind(SurvivorsRewardSelectionKind kind) => kind == SurvivorsRewardSelectionKind.LevelUp || IsRewardKind(kind);
        public static bool IsRewardKind(SurvivorsRewardSelectionKind kind) =>
            kind == SurvivorsRewardSelectionKind.EliteUpgrade || kind == SurvivorsRewardSelectionKind.BossUpgrade;
        public void Reset()
        {
            Clear();
            SelectedUpgradeCount = SelectedRewardUpgradeCount = RerollCount = BanishCount = SkipCount = AutoSelectCount = 0;
            OpenCount = LevelUpOpenCount = EliteOpenCount = BossOpenCount = RelicOpenCount = 0;
        }
        public void ResetOpenCount() => OpenCount = 0;
        public void Clear()
        {
            CurrentDraft = null;
            CurrentRelicDraft = null;
            Kind = SurvivorsRewardSelectionKind.None;
            _timer = 0f;
            _rerollIndex = 0;
            _pendingVictory = _pendingRelic = false;
        }
        public bool ApplyById(string id)
        {
            if (!_build.TryGetRunUpgrade(id, out var upgrade) || !_build.IsUpgradeEligibleForCurrentBuild(upgrade)) return false;
            if (!_build.State.Select(_build.Catalog, upgrade.Id).Succeeded) return false;
            _port.ApplyUpgrade(upgrade);
            SelectedUpgradeCount++;
            _port.RecordDirectUpgrade(upgrade);
            return true;
        }
        public bool Select(int index)
        {
            if (Kind == SurvivorsRewardSelectionKind.BossRelic) return SelectRelic(index);
            if (!CanSkip || index < 0 || index >= CurrentDraft.Choices.Count) return false;
            SurvivorsRewardSelectionKind kind = Kind;
            RunUpgradeDefinition selected = CurrentDraft.Choices[index];
            if (!_build.IsUpgradeEligibleForCurrentBuild(selected) || !_build.State.Select(_build.Catalog, selected.Id).Succeeded) return false;
            _port.ApplyUpgrade(selected);
            SelectedUpgradeCount++;
            _port.PresentSelectedUpgrade(kind, selected);
            Complete(kind, kind == SurvivorsRewardSelectionKind.LevelUp, kind != SurvivorsRewardSelectionKind.LevelUp);
            return true;
        }
        public bool Reroll()
        {
            if (!CanReroll) return false;
            int next = _rerollIndex + 1;
            if (!_offers.TryGenerate(Kind, next, null, out var draft)) return false;
            CurrentDraft = draft;
            CurrentRelicDraft = null;
            _rerollIndex = next;
            _port.ResetScroll();
            RerollCount++;
            _port.PresentDraft(Kind, CurrentDraft, null, false);
            BeginTimeout();
            _port.PlayReroll();
            return true;
        }
        public bool Skip()
        {
            if (!CanSkip) return false;
            CompleteSkipped(Kind, Kind == SurvivorsRewardSelectionKind.LevelUp, false);
            return true;
        }
        public bool Banish(int index)
        {
            if (!CanBanish || index < 0 || index >= CurrentDraft.Choices.Count) return false;
            RunUpgradeDefinition choice = CurrentDraft.Choices[index];
            if (choice == null || !_build.State.Banish(choice.Id)) return false;
            BanishCount++;
            SurvivorsRewardSelectionKind kind = Kind;
            int next = _rerollIndex + 1;
            if (_offers.TryGenerate(kind, next, null, out var draft))
            {
                CurrentDraft = draft;
                CurrentRelicDraft = null;
                _rerollIndex = next;
                _port.ResetScroll();
                _port.PresentDraft(kind, CurrentDraft, null, false);
                BeginTimeout();
            }
            else Complete(kind, kind == SurvivorsRewardSelectionKind.LevelUp, false);
            _port.PlayBanish();
            return true;
        }
        public bool TryOpenPending()
        {
            if (_experience.PendingLevelUps <= 0 || _run.State != SurvivorsRunState.Playing ||
                Kind != SurvivorsRewardSelectionKind.None || _experience.DraftCooldownRemaining > 0f) return false;
            OpenLevelUp();
            return _run.State == SurvivorsRunState.LevelUp;
        }
        public void OpenLevelUp(IReadOnlyList<RunUpgradeId> lockedChoices = null)
        {
            if (Kind != SurvivorsRewardSelectionKind.None) return;
            _rerollIndex = 0;
            if (!_offers.TryGenerate(SurvivorsRewardSelectionKind.LevelUp, 0, lockedChoices, out var draft))
            {
                CurrentDraft = null;
                CurrentRelicDraft = null;
                Kind = SurvivorsRewardSelectionKind.None;
                CompleteSkipped(SurvivorsRewardSelectionKind.LevelUp, true, false);
                return;
            }
            CurrentDraft = draft;
            CurrentRelicDraft = null;
            Kind = SurvivorsRewardSelectionKind.LevelUp;
            _run.OpenRewardSelection();
            _port.ResetScroll();
            LevelUpOpenCount++;
            OpenCount++;
            _experience.BeginDraft(_port.Tuning.LevelUpDraftCooldownSeconds);
            _port.PresentDraft(Kind, CurrentDraft, null, true);
            BeginTimeout();
            _port.PlayDraftOpened(Kind, default);
        }
        public bool OpenReward(SurvivorsEnemyRole role, bool requireEvolutionChoice)
        {
            if (_run.State == SurvivorsRunState.GameOver || _run.State == SurvivorsRunState.Victory) return false;
            IReadOnlyList<RunUpgradeId> locks = _offers.Guarantees.CreateEligibleEvolutionChoiceLocks(_port.Tuning.DraftChoiceCount);
            if (requireEvolutionChoice && locks.Count == 0) return false;
            SurvivorsRewardSelectionKind kind = role == SurvivorsEnemyRole.Boss ? SurvivorsRewardSelectionKind.BossUpgrade : SurvivorsRewardSelectionKind.EliteUpgrade;
            _rerollIndex = 0;
            if (!_offers.TryGenerate(kind, 0, locks, out var draft)) { CurrentDraft = null; return false; }
            CurrentDraft = draft;
            CurrentRelicDraft = null;
            Kind = kind;
            _pendingVictory = role == SurvivorsEnemyRole.Boss && !_run.HasClearedVictory;
            _pendingRelic = role == SurvivorsEnemyRole.Miniboss;
            _port.ResetScroll();
            if (role == SurvivorsEnemyRole.Boss) BossOpenCount++; else EliteOpenCount++;
            _run.OpenRewardSelection();
            OpenCount++;
            _port.PresentDraft(kind, CurrentDraft, null, true);
            BeginTimeout();
            _port.PlayDraftOpened(kind, role);
            return true;
        }
        public bool OpenRelic()
        {
            if (_run.State == SurvivorsRunState.GameOver || _run.State == SurvivorsRunState.Victory) return false;
            CurrentRelicDraft = SurvivorsRelicDraftService.Generate(_relics.Available(), _port.Tuning.DraftChoiceCount, _port.RelicSeed);
            if (CurrentRelicDraft == null || CurrentRelicDraft.Choices.Count == 0) { CurrentRelicDraft = null; return false; }
            CurrentDraft = null;
            Kind = SurvivorsRewardSelectionKind.BossRelic;
            _pendingVictory = false;
            RelicOpenCount++;
            _run.OpenRewardSelection();
            _port.ResetScroll();
            OpenCount++;
            _port.PresentDraft(Kind, null, CurrentRelicDraft, true);
            BeginTimeout();
            _port.PlayDraftOpened(Kind, default);
            return true;
        }
        public bool SelectRelic(int index)
        {
            if (_run.State != SurvivorsRunState.LevelUp || Kind != SurvivorsRewardSelectionKind.BossRelic ||
                CurrentRelicDraft == null || index < 0 || index >= CurrentRelicDraft.Choices.Count) return false;
            if (!_relics.Select(CurrentRelicDraft.Choices[index])) return false;
            CurrentRelicDraft = null;
            Kind = SurvivorsRewardSelectionKind.None;
            _timer = 0f;
            _pendingVictory = _pendingRelic = false;
            _rerollIndex = 0;
            _run.ResumePlaying();
            _experience.ResolveBudget(_port.Tuning);
            TryOpenPending();
            return true;
        }
        private void BeginTimeout() => _timer = _port.Tuning.RewardSelectionTimeoutSeconds > 0f ? _port.Tuning.RewardSelectionTimeoutSeconds : 0f;
        public void TickTimeout(float deltaTime)
        {
            if (_run.State != SurvivorsRunState.LevelUp || _timer <= 0f) return;
            _timer -= Mathf.Max(0f, deltaTime);
            if (_timer <= 0f) { _timer = 0f; AutoSelect(); }
        }
        public void AutoSelect()
        {
            if (_run.State != SurvivorsRunState.LevelUp) return;
            if (Kind == SurvivorsRewardSelectionKind.BossRelic && CurrentRelicDraft != null && CurrentRelicDraft.Choices.Count > 0)
            { AutoSelectCount++; SelectRelic(0); }
            else if (IsUpgradeKind(Kind) && CurrentDraft != null && CurrentDraft.Choices.Count > 0)
            { AutoSelectCount++; Select(0); }
        }
        private void CompleteSkipped(SurvivorsRewardSelectionKind kind, bool consumeLevelUp, bool selectedReward)
        {
            SkipCount++;
            _port.GrantSkipReward(kind);
            Complete(kind, consumeLevelUp, selectedReward);
        }
        private void Complete(SurvivorsRewardSelectionKind kind, bool consumeLevelUp, bool selectedReward)
        {
            if (consumeLevelUp) _experience.ConsumeLevelUp();
            if (selectedReward) SelectedRewardUpgradeCount++;
            CurrentDraft = null;
            CurrentRelicDraft = null;
            Kind = SurvivorsRewardSelectionKind.None;
            _timer = 0f;
            _rerollIndex = 0;
            if (kind == SurvivorsRewardSelectionKind.BossUpgrade && _pendingVictory)
            {
                _pendingVictory = _pendingRelic = false;
                _port.EnterVictory();
            }
            else if (_pendingRelic)
            {
                _pendingRelic = false;
                if (OpenRelic()) return;
                _experience.ResolveBudget(_port.Tuning);
                if (TryOpenPending()) return;
                _run.ResumePlaying();
            }
            else
            {
                _run.ResumePlaying();
                _experience.ResolveBudget(_port.Tuning);
                TryOpenPending();
            }
        }
    }
}
