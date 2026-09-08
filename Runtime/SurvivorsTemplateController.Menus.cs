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
    // Menu screen observations and commands, including compatibility test entry points.
    public sealed partial class SurvivorsTemplateController
    {
        private IReadOnlyList<string> ResolveBuildMenuCurrentBuildLines() => SurvivorsBuildMenuTextModel.CurrentBuild(ResolveBuildHudSummaryLines());

        private IReadOnlyList<string> ResolveBuildMenuControlsLines() => SurvivorsBuildMenuTextModel.Controls();

        private IReadOnlyList<string> ResolveBuildMenuStatsLines() => SurvivorsBuildMenuTextModel.Stats(CaptureBuildMenuStatsValues());

        private IReadOnlyList<string> ResolveBuildMenuRunInfoLines() => SurvivorsBuildMenuTextModel.RunInfo(CaptureBuildMenuRunInfoValues());

        private SurvivorsBuildMenuStatsValues CaptureBuildMenuStatsValues() => new SurvivorsBuildMenuStatsValues(
            damageBonusTotal: DamageBonus + PersistentDamageBonus + RelicDamageBonus,
            surgeDamageBonus: StreakSurgeDamageBonus + RoamingCacheSurgeDamageBonus + ArenaShrineSurgeDamageBonus + WaystoneFocusDamageBonus + WaystoneChainSurgeDamageBonus + HordeRushClearSurgeDamageBonus + WeaponLoadoutSurgeDamageBonus + PassiveLoadoutSurgeDamageBonus + BossRelicSurgeDamageBonus + GemRushDamageBonus + EvolutionChainSurgeDamageBonus + EndlessSurgeDamageBonus,
            weaponCooldownSeconds: WeaponCooldownSeconds,
            playerMoveSpeed: PlayerMoveSpeed,
            currentHealth: CurrentHealth,
            maxHealth: MaxHealth,
            barrierValue: BarrierValue,
            barrierCapacity: BarrierCapacity,
            contactInvulnerabilitySeconds: CurrentTuning.PlayerContactInvulnerabilitySeconds,
            currentPickupAttractRange: CurrentPickupAttractRange,
            currentPickupAttractionSpeed: CurrentPickupAttractionSpeed,
            experienceGainBonus: ExperienceGainMultiplierBonus + PassiveLoadoutSurgeExperienceGainMultiplierBonus,
            draftLuckBonus: DraftLuckBonus,
            areaRadiusBonus: AreaRadiusBonus,
            orbitRadiusBonus: OrbitRadiusBonus,
            deathNovaDamage: DeathNovaDamage,
            deathNovaRadius: DeathNovaRadius,
            payloadExplosionRadiusBonus: PayloadExplosionRadiusBonus,
            payloadTriggerRadiusBonus: PayloadTriggerRadiusBonus,
            poisonDamageRatio: PoisonDamageRatio,
            bleedDamageRatio: BleedDamageRatio,
            executeThresholdNormalized: ExecuteThresholdNormalized,
            lifestealRatio: LifestealRatio,
            criticalChanceNormalized: CriticalChanceNormalized,
            criticalDamageMultiplier: CriticalDamageMultiplier,
            projectileFanBonus: ProjectileFanBonus,
            projectilePierceBonus: ProjectilePierceBonus,
            projectileChainBonus: ProjectileChainBonus,
            projectileForkBonus: ProjectileForkBonus,
            projectileReturnBonus: ProjectileReturnBonus,
            payloadCountBonus: PayloadCountBonus,
            pickupPulseLabel: FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds));

        private SurvivorsBuildMenuRunInfoValues CaptureBuildMenuRunInfoValues() => new SurvivorsBuildMenuRunInfoValues(
            isEndlessRun: IsEndlessRun,
            currentRunModeDisplayName: CurrentRunModeDisplayName,
            pacingProfileLabel: BasicSurvivorsGame.GetPacingProfileDisplayName(CurrentPacingProfile),
            runTimeSeconds: RunTimeSeconds,
            survivalVictoryTimeSeconds: CurrentTuning.SurvivalVictoryTimeSeconds,
            currentRunMilestoneHudLabel: CurrentRunMilestoneHudLabel,
            phaseLabel: ResolveRunPhaseHudLabel(),
            runEscalationLevel: RunEscalationLevel,
            level: Level,
            killedCount: KilledCount,
            activeEnemyCount: ActiveEnemyCount,
            currentEnemyMaximumAlive: CurrentEnemyMaximumAlive,
            activeEliteCount: ActiveEliteCount,
            activeMinibossCount: ActiveMinibossCount,
            activeBossCount: ActiveBossCount,
            currencyEarned: BloodShardsEarnedThisRun + BonusBloodShardsEarnedThisRun,
            progressionEarned: LegacyExperienceEarnedThisRun + BonusLegacyExperienceEarnedThisRun,
            draftRerollsRemaining: DraftRerollsRemaining,
            draftBanishesRemaining: DraftBanishesRemaining,
            draftSkipBloodShards: DraftSkipBloodShards,
            waystoneDiscoveryCount: WaystoneDiscoveryCount,
            roamingCacheDropCount: RoamingCacheDropCount,
            arenaShrineTrialCount: ArenaShrineTrialCount,
            metaBloodShards: MetaBloodShards,
            lifetimeLegacyExperience: LifetimeLegacyExperience,
            currencyDisplayName: CurrencyDisplayName,
            progressionDisplayName: ProgressionDisplayName,
            currencyRewardLabel: CurrencyRewardLabel);

        private string FormatPersistentUpgradeOptionLabel(int index, SurvivorsPersistentUpgradeDefinition upgrade)
        {
            if (upgrade == null) return string.Empty;
            EnsureMetaProgressionLoaded();
            int currentRank = _metaProgression.GetPersistentUpgradeRank(upgrade.Id.Value);
            int nextCost = ResolveNextPersistentUpgradeCost(upgrade, currentRank);
            return SurvivorsProgressionChoiceLabels.PersistentUpgradeOption(index, upgrade,
                currentRank, nextCost, CurrencyRewardLabel);
        }

        private string FormatResultClassOptionLabel(int index, SurvivorsClassDefinition definition)
        {
            if (definition == null) return string.Empty;
            bool unlocked = IsResultClassUnlocked(definition);
            return SurvivorsProgressionChoiceLabels.ClassOption(index, definition, unlocked, SelectedClassId);
        }

        private string ResolveClassDisplayName(string classId, string fallback)
        {
            EnsureClassLibraryLoaded();
            return SurvivorsProgressionChoiceLabels.ClassDisplayName(_classLibrary, classId, fallback);
        }

        private SurvivorsRunResultPresenter ResultScreen => _resultScreen ?? (_resultScreen = new SurvivorsRunResultPresenter(this, () => ActiveUiTheme, _hudStyles));

        private void DrawRunResultOverlay(bool victory) => ResultScreen.Draw(victory);

        SurvivorsRunResultView ISurvivorsRunResultPort.ReadSummary() => new SurvivorsRunResultView(
            _lastRunSummaryTitle, $"Rewards {BloodShardsEarnedThisRun} {CurrencyRewardLabel} / {LegacyExperienceEarnedThisRun} {ProgressionDisplayName}",
            LastRunSummaryLines, CurrentTuning.EndlessContinuationEnabled);

        IReadOnlyList<SurvivorsResultClassChoice> ISurvivorsRunResultPort.ReadClasses()
        {
            IReadOnlyList<SurvivorsClassDefinition> options = ResolveResultClassOptions(ResultClassOptionCount);
            var rows = new SurvivorsResultClassChoice[options.Count];
            for (int i = 0; i < options.Count; i++)
            {
                SurvivorsClassDefinition option = options[i];
                rows[i] = new SurvivorsResultClassChoice(FormatResultClassOptionLabel(i, option), IsResultClassUnlocked(option),
                    option != null && string.Equals(SelectedClassId, option.Id, StringComparison.Ordinal));
            }
            return rows;
        }

        SurvivorsResultMetaView ISurvivorsRunResultPort.ReadMeta()
        {
            IReadOnlyList<SurvivorsPersistentUpgradeDefinition> options = ResolveResultMetaUpgradeOptions(ResultMetaUpgradeOptionCount);
            var labels = new string[options.Count];
            for (int i = 0; i < options.Count; i++) labels[i] = FormatPersistentUpgradeOptionLabel(i, options[i]);
            return new SurvivorsResultMetaView($"Meta Upgrades - {MetaBloodShards} {CurrencyDisplayName} banked",
                $"No affordable meta upgrades yet. Bank more {CurrencyDisplayName} from runs, elites, bosses, or skips.", labels);
        }

        void ISurvivorsRunResultPort.SelectClass(int index) => TrySelectResultClass(index);

        void ISurvivorsRunResultPort.PurchaseMeta(int index) => TryPurchaseResultMetaUpgrade(index);

        void ISurvivorsRunResultPort.Continue() => ContinueAfterVictory();

        void ISurvivorsRunResultPort.Restart() => RestartRun();

        void ISurvivorsRunResultPort.ChangeMode() => OpenRunModeSelection();

        void ISurvivorsRunResultPort.PlaySelect() => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);

        private SurvivorsDraftScreenPresenter DraftScreen => _draftScreen ?? (_draftScreen = new SurvivorsDraftScreenPresenter(
            DraftSession, () => ActiveUiTheme, _hudStyles,
            index => IsRelicChoiceOpen ? CreateRelicDraftCard(index, CurrentRelicChoices[index]) : CreateUpgradeDraftCard(index, CurrentDraftChoices[index]),
            ResolveRewardOverlayTitle, ResolveRewardTitleAccentColor, () => DraftSkipBloodShards));

        private void DrawLevelUpOverlay() => DraftScreen.Draw();

        private SurvivorsRunModePresenter RunModePresenter => _runModePresenter ?? (_runModePresenter = new SurvivorsRunModePresenter(this, () => ActiveUiTheme, _hudStyles));

        private void DrawRunModeSelectionOverlay() => RunModePresenter.Draw();

        bool ISurvivorsRunModePort.CanStart => CanStartConfiguredRun;

        bool ISurvivorsRunModePort.StrictAuthored => IsStrictAuthoredSample;

        string ISurvivorsRunModePort.AuthoredStatus => ContentBinding.AuthoredContentStatus;

        IReadOnlyList<SurvivorsUiTheme> ISurvivorsRunModePort.Themes => UiThemeSelection.AvailableThemes;

        int ISurvivorsRunModePort.SelectedThemeIndex => UiThemeSelection.SelectedIndex;

        SurvivorsRunModeCardView ISurvivorsRunModePort.ReadCard(SurvivorsPacingProfile profile)
        {
            SurvivorsTemplateTuning preview = CreateConfiguredTuning(profile);
            return new SurvivorsRunModeCardView(profile, preview, $"Boss {FormatRunTime(preview.BossSpawnTimeSeconds)}   Victory {FormatRunTime(preview.SurvivalVictoryTimeSeconds)}");
        }

        void ISurvivorsRunModePort.OpenTutorial() => OpenTutorialOverlay(markUnseen: false);

        void ISurvivorsRunModePort.EnsureThemes() => EnsureUiTheme();

        void ISurvivorsRunModePort.SelectTheme(int index) => SelectUiTheme(index);

        void ISurvivorsRunModePort.Hover() => PlayAudioEvent(AudioEventUiHover, _pickupClip, 0.08f);

        void ISurvivorsRunModePort.Select() => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);

        private SurvivorsMenuSession Menus => _menus ?? (_menus = new SurvivorsMenuSession(this));

        private SurvivorsBuildMenuPresenter BuildMenuPresenter => _buildMenuPresenter ?? (_buildMenuPresenter = new SurvivorsBuildMenuPresenter(
            Menus, () => ActiveUiTheme, _hudStyles, ResolveBuildMenuLines, () => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f)));

        private SurvivorsTutorialPresenter TutorialPresenter => _tutorialPresenter ?? (_tutorialPresenter = new SurvivorsTutorialPresenter(
            Menus, () => ActiveUiTheme, _hudStyles, ResolveTutorialStepTitle, ResolveTutorialStepLines));

        private void DrawBuildMenuOverlay() => BuildMenuPresenter.Draw();

        private void DrawTutorialOverlay() => TutorialPresenter.Draw();

        private void TryOpenFirstRunTutorial() => Menus.TryOpenFirstRunTutorial();

        private bool OpenTutorialOverlay(bool markUnseen) => Menus.OpenTutorialOverlay(markUnseen);

        private bool CloseTutorialOverlay(bool markSeen) => Menus.CloseTutorialOverlay(markSeen);

        private bool AdvanceTutorialStep() => Menus.AdvanceTutorialStep();

        private bool BackTutorialStep() => Menus.BackTutorialStep();

        bool ISurvivorsTutorialPort.HasProfile => _metaProgression != null;

        bool ISurvivorsTutorialPort.TutorialSeen => _metaProgression != null && _metaProgression.TutorialSeen;

        void ISurvivorsTutorialPort.EnsureProfile() => EnsureMetaProgressionLoaded();

        void ISurvivorsTutorialPort.ResetTutorialSeen() => _metaProgression?.ResetTutorialSeen();

        void ISurvivorsTutorialPort.MarkTutorialSeen() => _metaProgression?.MarkTutorialSeen();

        void ISurvivorsTutorialPort.PlaySelect() => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);

        public bool IsSprintRunMode => CurrentPacingProfile == SurvivorsPacingProfile.SprintRun;

        public bool IsRunModeSelectionOpen => Menus.ModeSelectionOpen;

        public string CurrentRunModeDisplayName => CurrentTuning.RunModeDisplayName;

        public bool IsBuildMenuOpen => Menus.BuildOpen;

        public string CurrentBuildMenuTabLabel => SurvivorsMenuSession.FormatBuildMenuTabLabel(Menus.BuildTab);

        public bool IsTutorialOverlayOpen => Menus.TutorialOpen;

        public string CurrentTutorialStepTitle => ResolveTutorialStepTitle(SurvivorsTutorialContent.ClampTutorialStepIndex(Menus.TutorialIndex));

        public int CurrentTutorialStepIndex => SurvivorsTutorialContent.ClampTutorialStepIndex(Menus.TutorialIndex);

        public int TutorialStepCount => SurvivorsTutorialContent.StepCount;

        private string ResolveTutorialStepTitle(int step)
        {
            EnsureUiTheme();
            int resolvedStep = SurvivorsTutorialContent.ClampTutorialStepIndex(step);
            return ActiveUiTheme.GetTutorialStepTitle(resolvedStep, SurvivorsTutorialContent.ResolveDefaultTutorialStepTitle(resolvedStep));
        }

        private IReadOnlyList<string> ResolveTutorialStepLines(int step)
        {
            EnsureUiTheme();
            int resolvedStep = SurvivorsTutorialContent.ClampTutorialStepIndex(step);
            return ActiveUiTheme.GetTutorialStepLines(resolvedStep, SurvivorsTutorialContent.ResolveDefaultTutorialStepLines(resolvedStep));
        }

        public bool SelectStandardRun()
        {
            return SelectRunMode(SurvivorsPacingProfile.HumanPlaytest);
        }

        public bool SelectSprintRun()
        {
            return SelectRunMode(SurvivorsPacingProfile.SprintRun);
        }

        public IReadOnlyList<string> GetResultMetaUpgradeOptionLabelsForTest()
        {
            IReadOnlyList<SurvivorsPersistentUpgradeDefinition> options = ResolveResultMetaUpgradeOptions(ResultMetaUpgradeOptionCount);
            string[] labels = new string[options.Count];
            for (int i = 0; i < options.Count; i++)
            {
                labels[i] = FormatPersistentUpgradeOptionLabel(i, options[i]);
            }

            return labels;
        }

        public IReadOnlyList<string> GetResultClassOptionLabelsForTest()
        {
            IReadOnlyList<SurvivorsClassDefinition> options = ResolveResultClassOptions(ResultClassOptionCount);
            string[] labels = new string[options.Count];
            for (int i = 0; i < options.Count; i++)
            {
                labels[i] = FormatResultClassOptionLabel(i, options[i]);
            }

            return labels;
        }

        public bool TryPurchaseResultMetaUpgradeForTest(int index)
        {
            return TryPurchaseResultMetaUpgrade(index);
        }

        public bool TrySelectResultClassForTest(int index)
        {
            return TrySelectResultClass(index);
        }

        public IReadOnlyList<string> CurrentBuildMenuLinesForTest()
        {
            EnsureRunStartedForTest();
            return ResolveBuildMenuLines(Menus.BuildTab);
        }

        public IReadOnlyList<string> CurrentTutorialLinesForTest()
        {
            return ResolveTutorialStepLines(SurvivorsTutorialContent.ClampTutorialStepIndex(Menus.TutorialIndex));
        }

        public void SetBuildMenuOpenForTest(bool open)
        {
            Menus.BuildOpen = open && _runSession.Started && State == SurvivorsRunState.Playing;
        }

        public void SetBuildMenuTabForTest(int tabIndex)
        {
            Menus.BuildTab = SurvivorsMenuSession.ClampBuildMenuTab(tabIndex);
        }

        public bool OpenTutorialForTest()
        {
            return OpenTutorialOverlay(markUnseen: false);
        }

        public bool SkipTutorialForTest()
        {
            return CloseTutorialOverlay(markSeen: true);
        }

        public bool AdvanceTutorialForTest()
        {
            return AdvanceTutorialStep();
        }

        public bool BackTutorialForTest()
        {
            return BackTutorialStep();
        }

        public Rect ResolveCenteredPanelRectForTest(float maxWidth, float maxHeight, float minWidth, float minHeight, float margin)
        {
            return SurvivorsScreenLayout.ResolveCenteredPanelRect(maxWidth, maxHeight, minWidth, minHeight, margin);
        }

        private IReadOnlyList<string> ResolveBuildMenuLines(BuildMenuTab tab)
        {
            switch (tab)
            {
                case BuildMenuTab.Stats:
                    return ResolveBuildMenuStatsLines();
                case BuildMenuTab.RunInfo:
                    return ResolveBuildMenuRunInfoLines();
                case BuildMenuTab.Controls:
                    return ResolveBuildMenuControlsLines();
                default:
                    return ResolveBuildMenuCurrentBuildLines();
            }
        }
    }
}
