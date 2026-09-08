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
    // Copied HUD observations, summary formatting and renderer bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private IReadOnlyList<string> ResolveBuildHudSummaryLines() => BuildHudModel.BuildLines(new SurvivorsBuildHudValues(ActiveWeaponIds, ActiveWeaponCount, CurrentPickupAttractRange, CurrentPickupAttractionSpeed, FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds), FormatSelectedRelicList()));

        private void DrawBuildHudPanel() => BuildHudPresenter.Draw();

        private string ShortWeaponName(string weaponId) => BuildContentLabels.ShortWeaponName(weaponId);

        private string ResolveWeaponBuildDisplayName(string weaponId) => BuildContentLabels.ResolveWeaponBuildDisplayName(weaponId);

        private SurvivorsBuildContentLabels BuildContentLabels => _buildContentLabels ?? (_buildContentLabels = new SurvivorsBuildContentLabels(RunBuild));

        private SurvivorsBuildHudModel BuildHudModel => _buildHudModel ?? (_buildHudModel = new SurvivorsBuildHudModel(RunBuild, BuildContentLabels, ResolveEvolutionObjectiveHudLabel));

        private SurvivorsBuildHudPresenter BuildHudPresenter => _buildHudPresenter ?? (_buildHudPresenter = new SurvivorsBuildHudPresenter(ResolveBuildHudSummaryLines, _hudStyles));

        private SurvivorsRunSummaryModel RunSummary => _runSummary ?? (_runSummary = new SurvivorsRunSummaryModel(RunBuild, BuildContentLabels));

        private IReadOnlyList<string> _lastRunSummaryLines => RunSummary.Lines;

        private string _lastRunSummaryTitle => RunSummary.Title;

        private void RebuildLastRunSummaryLines(bool victory) => RunSummary.Rebuild(CaptureRunSummaryValues(victory));

        private string FormatActiveWeaponList() => RunSummary.FormatActiveWeaponList(ActiveWeaponIds);

        private string FormatRunSummaryUpgradeList(IReadOnlyCollection<string> ids, bool includeRanks) => RunSummary.FormatRunSummaryUpgradeList(ids, includeRanks);

        private SurvivorsRunSummaryValues CaptureRunSummaryValues(bool victory) => new SurvivorsRunSummaryValues(
            new SurvivorsRunSummaryOutcome(
                victory: victory,
                endlessContinuationEnabled: CurrentTuning.EndlessContinuationEnabled,
                themeTitle: ActiveUiTheme.runSummaryTitle,
                modeDisplayName: CurrentRunModeDisplayName,
                runTimeSeconds: RunTimeSeconds,
                survivalVictoryTimeSeconds: CurrentTuning.SurvivalVictoryTimeSeconds,
                targetDurationSeconds: CurrentTuning.TargetDurationSeconds,
                runRewardMultiplier: CurrentTuning.RunRewardMultiplier,
                level: Level,
                experienceCollected: ExperienceCollected,
                experience: Experience,
                requiredExperienceForNextLevel: RequiredExperienceForNextLevel),
            new SurvivorsRunSummaryRewards(
                bloodShardsEarnedThisRun: BloodShardsEarnedThisRun,
                legacyExperienceEarnedThisRun: LegacyExperienceEarnedThisRun,
                currencyRewardLabel: CurrencyRewardLabel,
                progressionRewardLabel: ProgressionRewardLabel,
                metaBloodShards: MetaBloodShards,
                lifetimeLegacyExperience: LifetimeLegacyExperience,
                currencyDisplayName: CurrencyDisplayName,
                progressionDisplayName: ProgressionDisplayName),
            new SurvivorsRunSummaryCombat(
                killedCount: KilledCount,
                eliteKilledCount: EliteKilledCount,
                minibossKilledCount: MinibossKilledCount,
                bossKilledCount: BossKilledCount,
                damageTakenThisRun: DamageTakenThisRun,
                currentHealth: CurrentHealth,
                maxHealth: MaxHealth,
                bestMomentLabel: _bestMomentLabel,
                firstEvolutionAcquiredTimeSeconds: Telemetry.FirstEvolutionAcquiredTimeSeconds,
                highestChosenRarityLabel: _highestChosenRarityLabel,
                bestKillStreak: BestKillStreak),
            new SurvivorsRunSummaryCollection(
                activeWeaponIds: ActiveWeaponIds,
                activeWeaponCount: ActiveWeaponCount,
                selectedRelicCount: SelectedRelicCount,
                totalRelicCount: ResolveTotalRelicCount(),
                selectedRelicLabel: FormatSelectedRelicList(),
                currentPickupAttractRange: CurrentPickupAttractRange,
                currentPickupAttractionSpeed: CurrentPickupAttractionSpeed,
                currentPickupMagnetPulseIntervalSeconds: CurrentPickupMagnetPulseIntervalSeconds,
                magnetRecallCount: MagnetRecallCount,
                classUnlockRewardCount: ClassUnlockRewardCount,
                unlockedClassDisplayName: ClassUnlockRewardCount > 0 ? ResolveClassDisplayName(BasicSurvivorsGame.EmberVanguardClassId, "Ember Vanguard") : string.Empty));

        private SurvivorsEvolutionHudModel EvolutionHud => _evolutionHud ?? (_evolutionHud = new SurvivorsEvolutionHudModel(RunBuild, DraftOffers.Catalogs));

        private string ResolveEvolutionGoalHudLabel() => EvolutionHud.Goal();

        private string ResolveEvolutionReadyHudLabel() => EvolutionHud.Ready();

        private string ResolveEvolutionObjectiveHudLabel() => EvolutionHud.Objective();

        private static string FormatRunTime(float seconds) => SurvivorsRunText.FormatRunTime(seconds);

        private static string FormatMetricTime(float seconds) => SurvivorsRunText.FormatMetricTime(seconds);

        private static string FormatRewardTimeout(float seconds) => SurvivorsRunText.FormatRewardTimeout(seconds);

        private string ResolveDashHudLabel() => SurvivorsRunText.FormatDash(DashCooldownRemainingSeconds, IsPlayerSafetyActive, PlayerSafetyRemainingSeconds);

        private string ResolveBuildSlotHudLabel() => SurvivorsRunText.FormatBuildSlots(ActiveWeaponCount, MaxWeaponSlots, ActivePassiveCount, MaxPassiveSlots, EvolvedWeaponCount, SelectedRelicCount, ResolveTotalRelicCount());

        private string ResolveRunMilestoneHudLabel() => SurvivorsRunMilestoneModel.Label(CaptureRunMilestoneValues());

        private bool TryResolveRunMilestone(out string name, out float targetTimeSeconds, out float remainingSeconds) =>
            SurvivorsRunMilestoneModel.TryResolve(CaptureRunMilestoneValues(), out name, out targetTimeSeconds, out remainingSeconds);

        private SurvivorsRunMilestoneValues CaptureRunMilestoneValues()
        {
            if (!_runSession.Started || State == SurvivorsRunState.Victory || State == SurvivorsRunState.GameOver)
                return new SurvivorsRunMilestoneValues(_runSession.Started, State, 0f, false, 0f, default, default);
            float hordeTime = HordeRush.NextTime;
            bool endless = IsEndlessRun;
            SurvivorsNormalMilestoneTimes normal = default;
            SurvivorsEndlessMilestoneTimes endlessTimes = default;
            if (endless)
                endlessTimes = new SurvivorsEndlessMilestoneTimes(TimedEncounters.ResolveNextEndlessEliteRole(),
                    TimedEncounters.NextEliteTime, TimedEncounters.NextMinibossTime, TimedEncounters.NextBossTime);
            else if (_runFlow != null && _runFlow.Definition != null)
                normal = new SurvivorsNormalMilestoneTimes(true, _runFlow.NextEliteSpawnTimeSeconds, _runFlow.NextDreadEliteSpawnTimeSeconds,
                    _runFlow.Definition.MinibossSpawnTimeSeconds, _runFlow.Definition.BossSpawnTimeSeconds, _runFlow.Definition.SurvivalVictoryTimeSeconds);
            return new SurvivorsRunMilestoneValues(true, State, RunTimeSeconds, endless, hordeTime, normal, endlessTimes);
        }

        private string ResolveSurgeHudLabel() => SurvivorsSurgeHudModel.Format(CaptureSurgeHudValues());

        private SurvivorsSurgeHudValues CaptureSurgeHudValues() => new SurvivorsSurgeHudValues(
            new SurvivorsSurgeHudState(IsStreakSurgeActive, StreakSurgeRemainingSeconds, StreakSurgeTier),
            new SurvivorsSurgeHudState(IsRoamingCacheSurgeActive, RoamingCacheSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsArenaShrineSurgeActive, ArenaShrineSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsWaystoneFocusActive, WaystoneFocusRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsWaystoneChainSurgeActive, WaystoneChainSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsHordeRushClearSurgeActive, HordeRushClearSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsWeaponLoadoutSurgeActive, WeaponLoadoutSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsPassiveLoadoutSurgeActive, PassiveLoadoutSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsBossRelicSurgeActive, BossRelicSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsGemRushActive, GemRushRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsEvolutionChainSurgeActive, EvolutionChainSurgeRemainingSeconds, 0),
            new SurvivorsSurgeHudState(IsEndlessSurgeActive, EndlessSurgeRemainingSeconds, EndlessSurgeTier));

        private IReadOnlyList<string> ResolvePlayerHudLines() => SurvivorsPlayerHudTextModel.BuildLines(CapturePlayerHudValues());

        private SurvivorsPlayerHudValues CapturePlayerHudValues() => new SurvivorsPlayerHudValues(
            milestoneLabel: CurrentRunMilestoneHudLabel,
            buildSlotLabel: ResolveBuildSlotHudLabel(),
            activeWeaponLabel: FormatActiveWeaponList(),
            currentPickupAttractRange: CurrentPickupAttractRange,
            currentPickupAttractionSpeed: CurrentPickupAttractionSpeed,
            pickupPulseLabel: FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds),
            evolutionLabel: ResolveEvolutionObjectiveHudLabel());

        private SurvivorsHudDispatch HudDispatch => _hudDispatch ?? (_hudDispatch = new SurvivorsHudDispatch(_runSession, Menus, this));

        bool ISurvivorsHudRenderPort.DebugVisible => _debugOverlayVisible;

        void ISurvivorsHudRenderPort.EnsureStyles() => EnsureHudStyles();

        void ISurvivorsHudRenderPort.DrawModeSelection() => DrawRunModeSelectionOverlay();

        void ISurvivorsHudRenderPort.DrawTutorial() => DrawTutorialOverlay();

        void ISurvivorsHudRenderPort.DrawTimer() => DrawTopCenterTimerHud();

        void ISurvivorsHudRenderPort.DrawPlayer() => DrawPlayerHud();

        void ISurvivorsHudRenderPort.DrawDebug() => DrawDebugOverlay();

        void ISurvivorsHudRenderPort.DrawBuildHud() => DrawBuildHudPanel();

        void ISurvivorsHudRenderPort.DrawDraft() => DrawLevelUpOverlay();

        void ISurvivorsHudRenderPort.DrawResult(bool victory) => DrawRunResultOverlay(victory);

        void ISurvivorsHudRenderPort.DrawBuildMenu() => DrawBuildMenuOverlay();

        void ISurvivorsHudRenderPort.DrawRunFeedback()
        {
            DrawLowHealthWarning();
            DrawMajorThreatWarning();
            DrawHordeRushWarning();
            DrawMajorThreatHealthBar();
            DrawOffscreenThreatMarker();
            DrawRewardSelectionFeedback();
            DrawStreakRewardFeedback();
            DrawClassUnlockRewardFeedback();
            DrawEvolutionReadyFeedback();
            DrawExperienceComboFeedback();
            DrawDamagePopups();
        }

        private SurvivorsHudVitals CaptureHudVitals() => new SurvivorsHudVitals(
            MaxHealth, CurrentHealth, BarrierCapacity, BarrierValue,
            Experience, RequiredExperienceForNextLevel, Level);

        private void DrawPlayerHud()
        {
            IReadOnlyList<string> lines = ResolvePlayerHudLines();
            SurvivorsPlayerHudPresenter.Draw(Screen.width, lines, CurrentRunModeDisplayName,
                CaptureHudVitals(), _hudStyles);
        }

        private SurvivorsTopTimerValues CaptureTopTimerValues()
        {
            if (!_runSession.Started) return default;
            return new SurvivorsTopTimerValues(true, IsEndlessRun,
                CurrentTuning.SurvivalVictoryTimeSeconds, RunTimeSeconds,
                CurrentRunModeDisplayName, CurrentPacingProfile);
        }

        private void DrawTopCenterTimerHud() => SurvivorsTopTimerHud.Draw(
            Screen.width, CaptureTopTimerValues(), _majorThreatWarningStyle);

        private Rect ResolveTopCenterTimerRect() => SurvivorsTopTimerHud.Panel(Screen.width);

        private string ResolveTopCenterTimerHudLabel() => SurvivorsTopTimerHud.Label(CaptureTopTimerValues());

        private SurvivorsCompactHudLabels CompactHudLabels => _compactHudLabels ??
            (_compactHudLabels = new SurvivorsCompactHudLabels(BuildContentLabels));

        private string FormatSelectedRelicList() => SurvivorsCompactHudLabels.Relics(RelicInventory.Selected);

        private SurvivorsDebugHudValues CaptureDebugHudValues()
        {
            string evolution = ResolveEvolutionObjectiveHudLabel();
            string waystone = ResolveWaystoneCompassHudLabel();
            return new SurvivorsDebugHudValues(
            vitals: CaptureHudVitals(),
            evolutionObjective: evolution,
            waystoneCompass: waystone,
            runTimeSeconds: RunTimeSeconds,
            survivalVictoryTimeSeconds: CurrentTuning.SurvivalVictoryTimeSeconds,
            runPhaseLabel: ResolveRunPhaseHudLabel(),
            runEscalationLevel: RunEscalationLevel,
            milestoneLabel: CurrentRunMilestoneHudLabel,
            activeEnemyCount: ActiveEnemyCount,
            enemyMaximumAlive: CurrentEnemyMaximumAlive,
            killedCount: KilledCount,
            splitterCount: ActiveSplitterCount,
            summonerCount: ActiveSummonerCount,
            eliteCount: ActiveEliteCount,
            minibossCount: ActiveMinibossCount,
            bossCount: ActiveBossCount,
            currencyDisplayName: CurrencyDisplayName,
            metaBloodShards: MetaBloodShards,
            poisonDamageRatio: PoisonDamageRatio,
            bleedDamageRatio: BleedDamageRatio,
            executeThresholdNormalized: ExecuteThresholdNormalized,
            weaponLabel: ResolveWeaponHudLabel(),
            modeDisplayName: CurrentRunModeDisplayName,
            pacingProfile: CurrentPacingProfile,
            enemySpawnIntervalSeconds: CurrentEnemySpawnIntervalSeconds,
            enemySpeedMultiplier: CurrentEnemySpeedMultiplier,
            currentKillStreak: CurrentKillStreak,
            bestKillStreak: BestKillStreak,
            streakBonusDropCount: StreakBonusDropCount,
            surgeLabel: ResolveSurgeHudLabel(),
            rewardSelectionTimeoutSeconds: CurrentTuning.RewardSelectionTimeoutSeconds,
            draftRerollsRemaining: DraftRerollsRemaining,
            draftBanishesRemaining: DraftBanishesRemaining,
            buildSlotLabel: ResolveBuildSlotHudLabel(),
            dashLabel: ResolveDashHudLabel());
        }

        private GUIStyle _hudSmallStyle => _hudStyles.HudSmallStyle;

        private GUIStyle _lowHealthStyle => _hudStyles.LowHealthStyle;

        private GUIStyle _majorThreatWarningStyle => _hudStyles.MajorThreatWarningStyle;

        private GUIStyle _rewardFeedbackStyle => _hudStyles.RewardFeedbackStyle;

        private SurvivorsThreatHudModel ThreatHud => _threatHud ??
            (_threatHud = new SurvivorsThreatHudModel(new SurvivorsEnemyHudSource(_enemies)));

        public IReadOnlyList<string> LastRunSummaryLines => _lastRunSummaryLines;

        public int ActiveOverheadLifeBarCount => CountAuthoredThreatLifeBars(showBossLifeBar: false);

        public bool IsMajorThreatHealthVisible => ThreatHud.SelectHealthThreat().HasValue;

        public string CurrentMajorThreatHealthLabel => ThreatHud.SelectHealthThreat()?.Name ?? string.Empty;

        public string LastOffscreenThreatMarkerLabel => ThreatHud.LastMarkerLabel;

        public string LastRunSummaryTitle => _lastRunSummaryTitle;

        public bool IsRunSummaryVisible => State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory;

        public Rect TopCenterTimerRectForTest => ResolveTopCenterTimerRect();

        public string CurrentRunMilestoneName
        {
            get
            {
                return TryResolveRunMilestone(out string name, out _, out _)
                    ? name
                    : string.Empty;
            }
        }

        public float CurrentRunMilestoneTargetTimeSeconds
        {
            get
            {
                return TryResolveRunMilestone(out _, out float targetTimeSeconds, out _)
                    ? targetTimeSeconds
                    : 0f;
            }
        }

        public float CurrentRunMilestoneRemainingSeconds
        {
            get
            {
                return TryResolveRunMilestone(out _, out _, out float remainingSeconds)
                    ? remainingSeconds
                    : 0f;
            }
        }

        public string CurrentRunMilestoneHudLabel => ResolveRunMilestoneHudLabel();

        public string CurrentEvolutionGoalHudLabel => ResolveEvolutionGoalHudLabel();

        public string CurrentEvolutionReadyHudLabel => ResolveEvolutionReadyHudLabel();

        public IReadOnlyList<string> CurrentBuildHudLinesForTest()
        {
            EnsureRunStartedForTest();
            return ResolveBuildHudSummaryLines();
        }

        public IReadOnlyList<string> RunSummaryLinesForTest()
        {
            return LastRunSummaryLines;
        }

        public IReadOnlyList<string> PlayerHudLinesForTest()
        {
            EnsureRunStartedForTest();
            return ResolvePlayerHudLines();
        }

        private int CountAuthoredThreatLifeBars(bool showBossLifeBar) => ThreatHud.CountLifeBars(showBossLifeBar);

        private string ResolveActiveMajorThreatLifeBarSummary() => ThreatHud.LifeBarSummary();

        private static void SetRendererColor(Renderer renderer, Color color) => SurvivorsPrimitivePresentation.SetRendererColor(renderer, color);

        private int CountOffscreenMajorThreatMarkers() => ThreatHud.CountMarkers(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);

        private string ResolveFirstOffscreenMajorThreatMarkerLabel() => ThreatHud.MarkerLabel(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);

        private void UpdateOffscreenThreatMarkerSnapshot() => ThreatHud.UpdateLastMarker(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);

        private void ResetHudStyles()
        {
            _hudStyles.Reset();
            _damageFeedback.ResetStyles();
        }

        private void EnsureHudStyles() => _hudStyles.Ensure();

        private void DrawOffscreenThreatMarker()
        {
            if (State != SurvivorsRunState.Playing) return;
            SurvivorsThreatHudItem? selected = ThreatHud.SelectMarker(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);
            if (selected.HasValue)
            {
                SurvivorsThreatHudPresenter.DrawMarker(selected, PlayerPosition, ResolveMajorRewardDropColor(selected.Value.Role), _hudSmallStyle);
            }
        }

        private static Material ApplyColor(Renderer renderer, Color color) => SurvivorsPrimitivePresentation.ApplyColor(renderer, color);

        private static Color WithAlpha(Color color, float alpha) => SurvivorsPrimitivePresentation.WithAlpha(color, alpha);
    }
}
