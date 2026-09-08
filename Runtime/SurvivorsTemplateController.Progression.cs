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
    // Borrowed profile, persistent progression and run reward transaction bindings.
    public sealed partial class SurvivorsTemplateController
    {
        public bool TryPurchasePersistentUpgrade(string id) => PersistentProgression.TryPurchasePersistentUpgrade(id, _runSession.Started);

        private bool TryPurchaseResultMetaUpgrade(int index) => PersistentProgression.TryPurchaseResultMetaUpgrade(index, ResultMetaUpgradeOptionCount, _runSession.Started);

        private IReadOnlyList<SurvivorsPersistentUpgradeDefinition> ResolveResultMetaUpgradeOptions(int limit) => PersistentProgression.ResolveResultMetaUpgradeOptions(limit);

        private IReadOnlyList<SurvivorsClassDefinition> ResolveResultClassOptions(int limit) => PersistentProgression.ResolveResultClassOptions(limit);

        private bool TrySelectResultClass(int index) => PersistentProgression.TrySelectResultClass(index, ResultClassOptionCount, _runSession.Started, State);

        private bool IsResultClassUnlocked(SurvivorsClassDefinition definition) => PersistentProgression.IsResultClassUnlocked(definition);

        private int ResolveNextPersistentUpgradeCost(SurvivorsPersistentUpgradeDefinition upgrade, int currentRank) => SurvivorsPersistentProgression.ResolveNextPersistentUpgradeCost(upgrade, currentRank);

        private void GrantMajorEnemyReward(SurvivorsEnemyRole role) => RunRewards.GrantMajorEnemyReward(role);

        private void GrantRunRewards(bool victory) => RunRewards.GrantRunRewards(victory, new SurvivorsRunRewardInput(RunTimeSeconds, Level, MinibossKilledCount, BossKilledCount, CurrentTuning.RunRewardMultiplier));

        private SurvivorsPersistentProgression PersistentProgression => _persistentProgression ?? (_persistentProgression = new SurvivorsPersistentProgression(this));

        private SurvivorsRunRewards RunRewards => _runRewards ?? (_runRewards = new SurvivorsRunRewards(this));

        SurvivorsMetaProgressionService ISurvivorsProgressionContentPort.EnsureProgression()
        { EnsureMetaProgressionLoaded(); return _metaProgression; }

        SurvivorsMetaProgressionDefinition ISurvivorsProgressionContentPort.ProgressionDefinition => ResolveMetaProgressionDefinition();

        SurvivorsClassLibraryDefinition ISurvivorsProgressionContentPort.EnsureClasses()
        { EnsureClassLibraryLoaded(); return _classLibrary; }

        void ISurvivorsPersistentProgressionPort.ApplyPersistentBonuses() => ApplyPersistentMetaBonuses();

        void ISurvivorsPersistentProgressionPort.SetSelectedClass(SurvivorsClassDefinition selected) => _selectedClass = selected;

        void ISurvivorsPersistentProgressionPort.ShowMetaPurchase(string id) => RecordMetaUpgradePurchaseFeedback(id);

        void ISurvivorsPersistentProgressionPort.ShowClassSelection(SurvivorsClassDefinition selected) => RecordResultClassSelectionFeedback(selected);

        void ISurvivorsRunRewardPort.ShowClassUnlock() => RecordClassUnlockRewardFeedback();

        void ISurvivorsRunRewardPort.ShowRunSummary(bool victory)
        { RebuildLastRunSummaryLines(victory); PlayAudioEvent(AudioEventRunSummaryOpened, _levelUpClip, 0.25f); }

        private void RecordResultClassSelectionFeedback(SurvivorsClassDefinition selected) => ProgressionFeedback.RecordResultClassSelectionFeedback(selected);

        private void RecordMetaUpgradePurchaseFeedback(string id) => ProgressionFeedback.RecordMetaUpgradePurchaseFeedback(id);

        private void RecordEvolutionGoalFeedback(RunUpgradeDefinition evolution, RunUpgradeDefinition missingPassive) => ProgressionFeedback.RecordEvolutionGoalFeedback(evolution, missingPassive);

        private void RecordEvolutionReadyFeedback(RunUpgradeDefinition evolution) => ProgressionFeedback.RecordEvolutionReadyFeedback(evolution);

        private void RecordClassUnlockRewardFeedback() => ProgressionFeedback.RecordClassUnlockRewardFeedback();

        private SurvivorsProgressionFeedback ProgressionFeedback => _progressionFeedback ?? (_progressionFeedback = new SurvivorsProgressionFeedback(this, _rewardBanner, _classUnlockRewardBanner, _evolutionReadyBanner));

        void ISurvivorsProgressionFeedbackPort.EnsureProfile() => EnsureMetaProgressionLoaded();

        SurvivorsMetaProgressionDefinition ISurvivorsProgressionFeedbackPort.MetaDefinition => ResolveMetaProgressionDefinition();

        int ISurvivorsProgressionFeedbackPort.GetPersistentRank(string id) => _metaProgression.GetPersistentUpgradeRank(id);

        string ISurvivorsProgressionFeedbackPort.ResolveUpgradeDisplayName(RunUpgradeId id) => ResolveUpgradeDisplayName(id);

        string ISurvivorsProgressionFeedbackPort.ResolveClassDisplayName(string classId, string fallback) => ResolveClassDisplayName(classId, fallback);

        string ISurvivorsProgressionFeedbackPort.CurrencyRewardLabel => CurrencyRewardLabel;

        string ISurvivorsProgressionFeedbackPort.ProgressionRewardLabel => ProgressionRewardLabel;

        void ISurvivorsProgressionFeedbackPort.RecordEvolutionEligibility() => Telemetry.Record(SurvivorsRunMetric.FirstEvolutionEligibility, RunTimeSeconds);

        void ISurvivorsProgressionFeedbackPort.PlayEvolutionPulse(int count) => PlayFeedback(_levelUpPulse, PlayerPosition, count, _levelUpClip);

        void ISurvivorsProgressionFeedbackPort.PlayClassUnlockPulse() => PlayFeedback(_bossPulse, PlayerPosition, 52, _levelUpClip);

        private SurvivorsMetaProgressionService _metaProgression => _profileSession.Current;

        public int EliteRewardGrantCount => RunRewards.EliteRewardGrantCount;

        public int MinibossRewardGrantCount => RunRewards.MinibossRewardGrantCount;

        public int BossRewardGrantCount => RunRewards.BossRewardGrantCount;

        public int ClassUnlockRewardCount => RunRewards.ClassUnlockRewardCount;

        public string LastClassUnlockRewardFeedbackLabel => ProgressionFeedback.LastClassUnlockRewardFeedbackLabel;

        public int MetaUpgradePurchaseCount => PersistentProgression.MetaUpgradePurchaseCount;

        public string LastMetaUpgradePurchaseFeedbackLabel => ProgressionFeedback.LastMetaUpgradePurchaseFeedbackLabel;

        public int ResultClassSelectionCount => PersistentProgression.ResultClassSelectionCount;

        public string LastResultClassSelectionFeedbackLabel => ProgressionFeedback.LastResultClassSelectionFeedbackLabel;

        public int EvolutionGoalFeedbackCount => ProgressionFeedback.EvolutionGoalFeedbackCount;

        public string LastEvolutionGoalFeedbackLabel => ProgressionFeedback.LastEvolutionGoalFeedbackLabel;

        public int EvolutionReadyFeedbackCount => ProgressionFeedback.EvolutionReadyFeedbackCount;

        public string LastEvolutionReadyFeedbackLabel => ProgressionFeedback.LastEvolutionReadyFeedbackLabel;

        public int BonusBloodShardsEarnedThisRun => RunRewards.BonusBloodShards;

        public int BonusLegacyExperienceEarnedThisRun => RunRewards.BonusLegacyExperience;

        public int BloodShardsEarnedThisRun => RunRewards.BloodShardsEarned;

        public int LegacyExperienceEarnedThisRun => RunRewards.LegacyExperienceEarned;

        public SurvivorsRunRewardSummary LastRunResult => RunRewards.LastRunResult;

        public SurvivorsPacingProfile CurrentPacingProfile => CurrentTuning.PacingProfile;

        public bool IsTutorialSeen => _metaProgression != null && _metaProgression.TutorialSeen;

        public SurvivorsClassDefinition SelectedClass => _selectedClass;

        public string SelectedClassId => _selectedClass == null ? string.Empty : _selectedClass.Id;

        public long MetaBloodShards => _metaProgression == null ? 0 : _metaProgression.UnspentBloodShards;

        public long LifetimeBloodShards => _metaProgression == null ? 0 : _metaProgression.LifetimeBloodShards;

        public long LifetimeLegacyExperience => _metaProgression == null ? 0 : _metaProgression.LifetimeLegacyExperience;

        public int MetaCompletedRuns => _metaProgression == null ? 0 : _metaProgression.CompletedRuns;

        public int MetaBossVictories => _metaProgression == null ? 0 : _metaProgression.BossVictories;

        public int MetaUnlockedClassCount => _metaProgression == null ? 0 : _metaProgression.UnlockedClassIds.Count;

        public bool DebugGrantBloodShards(int amount)
        {
            EnsureMetaProgressionLoaded();
            bool granted = _metaProgression.GrantBloodShardsForDebug(Mathf.Max(1, amount)).Succeeded;
            if (granted && _runSession.Started)
            {
                ApplyPersistentMetaBonuses();
            }

            return granted;
        }

        public void ApplyPacingProfileForTest(SurvivorsPacingProfile profile, bool restartRun = false)
        {
            ApplyPacingProfile(profile, restartRun);
        }

        public void DebugResetMetaProgression()
        {
            ResetMetaProgressionForTest();
        }

        public bool TryPurchasePersistentUpgradeForTest(string id)
        {
            return TryPurchasePersistentUpgrade(id);
        }

        public int GetPersistentUpgradeRankForTest(string id)
        {
            EnsureMetaProgressionLoaded();
            return _metaProgression.GetPersistentUpgradeRank(id);
        }

        public void ResetMetaProgressionForTest()
        {
            EnsureMetaProgressionLoaded();
            _metaProgression.Reset();
            _metaProgression.Load();
            if (_runSession.Started)
            {
                ApplyPersistentMetaBonuses();
            }
        }

        public bool UnlockClassForTest(string classId)
        {
            EnsureMetaProgressionLoaded();
            EnsureClassLibraryLoaded();
            return _metaProgression.UnlockClass(classId, _classLibrary);
        }

        public bool TrySelectClassForTest(string classId)
        {
            EnsureMetaProgressionLoaded();
            EnsureClassLibraryLoaded();
            bool selected = _metaProgression.TrySetSelectedClass(classId, _classLibrary);
            if (selected)
            {
                _selectedClass = _metaProgression.ResolveSelectedClass(_classLibrary);
            }

            return selected;
        }

        public bool IsClassUnlockedForTest(string classId)
        {
            EnsureMetaProgressionLoaded();
            EnsureClassLibraryLoaded();
            return _metaProgression.IsClassUnlocked(classId, _classLibrary);
        }

        public void ResetTutorialSeenForTest()
        {
            EnsureMetaProgressionLoaded();
            _metaProgression.ResetTutorialSeen();
            Menus.TutorialOpen = false;
            Menus.TutorialIndex = 0;
        }

        public void MarkTutorialSeenForTest()
        {
            EnsureMetaProgressionLoaded();
            _metaProgression.MarkTutorialSeen();
            Menus.TutorialOpen = false;
        }

        private void EnsureMetaProgressionLoaded()
        {
            _profileSession.EnsureLoaded(ResolveMetaProgressionDefinition());
        }

        private string CurrencyDisplayName => ResolveMetaProgressionDefinition().CurrencyDisplayName;

        private string CurrencyRewardLabel => string.Equals(CurrencyDisplayName, "Blood Shards", StringComparison.Ordinal)
            ? "shards"
            : CurrencyDisplayName;

        private string ProgressionDisplayName => ResolveMetaProgressionDefinition().LegacyExperienceDisplayName;

        private string ProgressionRewardLabel => string.Equals(ProgressionDisplayName, "Legacy XP", StringComparison.Ordinal)
            ? "XP"
            : ProgressionDisplayName;

        private void EnsureClassLibraryLoaded()
        {
            _classLibrary ??= CreateClassLibraryDefinition();
            _relicDefinitions ??= CreateRelicDefinitions();
            _upgradeClassGates ??= CreateClassUpgradeGates();
            if (_metaProgression != null)
            {
                _metaProgression.EnsureDefaultClassUnlocks(_classLibrary);
            }
        }

        private void ApplyPersistentMetaBonuses()
        {
            EnsureMetaProgressionLoaded();
            UpgradeModifiers.ApplyPersistent(new SurvivorsPersistentBonuses(
                _metaProgression.GetPersistentDamageBonus(BasicSurvivorsGame.WeaponTarget.Value),
                _metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaMaxHealthEffectId, BasicSurvivorsGame.PlayerTarget.Value),
                _metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaPickupRangeEffectId, BasicSurvivorsGame.PickupTarget.Value),
                _metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaExperienceGainEffectId, BasicSurvivorsGame.ExperienceTarget.Value),
                _metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaDraftRerollEffectId, BasicSurvivorsGame.PlayerTarget.Value)));
        }

        private void ReleaseMetaProgressionService()
        {
            _profileSession.Release();
        }
    }
}
