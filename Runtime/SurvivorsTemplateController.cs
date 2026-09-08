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
    public sealed class SurvivorsTemplateController : MonoBehaviour, ISurvivorsUpgradeEffectSink, ISurvivorsSwarmSpawnPort, ISurvivorsTimedEncounterPort, ISurvivorsHordeRushPort, ISurvivorsTraversalPort, ISurvivorsExplorationPort, ISurvivorsPlayerDamagePort, ISurvivorsPlayerMotionPort, ISurvivorsRunBuildPort, ISurvivorsDraftSessionPort, ISurvivorsTutorialPort, ISurvivorsRunModePort, ISurvivorsRunResultPort, ISurvivorsStreakRewardPort, ISurvivorsEnemyNavigationPort, ISurvivorsBuildSurgePort, ISurvivorsPersistentProgressionPort, ISurvivorsRunRewardPort, ISurvivorsPickupRewardPort, ISurvivorsContentBindingPort, ISurvivorsEnemyDefeatPort, ISurvivorsMajorRewardPickupCachePort, ISurvivorsPickupCollectionPort, ISurvivorsDamageAugmentPort, ISurvivorsMajorThreatAbilityPort, ISurvivorsEnemySupportSpawnPort, ISurvivorsFrameInputPort, ISurvivorsEnemySpawnPort, ISurvivorsPickupSpawnPort, ISurvivorsProjectileLaunchPort, ISurvivorsSpawnSafetyPort, ISurvivorsUiThemeSelectionPort, ISurvivorsRunLifecyclePort, ISurvivorsHudRenderPort, ISurvivorsRewardFeedbackPort, ISurvivorsDamageFeedbackPort, ISurvivorsRangedDodgePort, ISurvivorsRunWeaponPort, ISurvivorsProgressionFeedbackPort, ISurvivorsDebugWorldPort, ISurvivorsRunMetricsReadPort, ISurvivorsActiveRunMetricsReadPort, ISurvivorsOrbitKnockbackPort, ISurvivorsDebugDraftPort, ISurvivorsActorMembershipPort
    {
        private IReadOnlyList<string> ResolveBuildHudSummaryLines() => BuildHudModel.BuildLines(new SurvivorsBuildHudValues(ActiveWeaponIds, ActiveWeaponCount, CurrentPickupAttractRange, CurrentPickupAttractionSpeed, FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds), FormatSelectedRelicList()));

        private void DrawBuildHudPanel() => BuildHudPresenter.Draw();

        private string ShortWeaponName(string weaponId) => BuildContentLabels.ShortWeaponName(weaponId);

        private string ResolveWeaponBuildDisplayName(string weaponId) => BuildContentLabels.ResolveWeaponBuildDisplayName(weaponId);

        private SurvivorsBuildContentLabels _buildContentLabels;
        private SurvivorsBuildContentLabels BuildContentLabels => _buildContentLabels ?? (_buildContentLabels = new SurvivorsBuildContentLabels(RunBuild));
        private SurvivorsBuildHudModel _buildHudModel;
        private SurvivorsBuildHudModel BuildHudModel => _buildHudModel ?? (_buildHudModel = new SurvivorsBuildHudModel(RunBuild, BuildContentLabels, ResolveEvolutionObjectiveHudLabel));
        private SurvivorsBuildHudPresenter _buildHudPresenter;
        private SurvivorsBuildHudPresenter BuildHudPresenter => _buildHudPresenter ?? (_buildHudPresenter = new SurvivorsBuildHudPresenter(ResolveBuildHudSummaryLines, _hudStyles));

        public bool TryPurchasePersistentUpgrade(string id) => PersistentProgression.TryPurchasePersistentUpgrade(id, _runSession.Started);

        private bool TryPurchaseResultMetaUpgrade(int index) => PersistentProgression.TryPurchaseResultMetaUpgrade(index, ResultMetaUpgradeOptionCount, _runSession.Started);

        private IReadOnlyList<SurvivorsPersistentUpgradeDefinition> ResolveResultMetaUpgradeOptions(int limit) => PersistentProgression.ResolveResultMetaUpgradeOptions(limit);

        private IReadOnlyList<SurvivorsClassDefinition> ResolveResultClassOptions(int limit) => PersistentProgression.ResolveResultClassOptions(limit);

        private bool TrySelectResultClass(int index) => PersistentProgression.TrySelectResultClass(index, ResultClassOptionCount, _runSession.Started, State);

        private bool IsResultClassUnlocked(SurvivorsClassDefinition definition) => PersistentProgression.IsResultClassUnlocked(definition);

        private int ResolveNextPersistentUpgradeCost(SurvivorsPersistentUpgradeDefinition upgrade, int currentRank) => SurvivorsPersistentProgression.ResolveNextPersistentUpgradeCost(upgrade, currentRank);

        private void GrantMajorEnemyReward(SurvivorsEnemyRole role) => RunRewards.GrantMajorEnemyReward(role);

        private void GrantRunRewards(bool victory) => RunRewards.GrantRunRewards(victory, new SurvivorsRunRewardInput(RunTimeSeconds, Level, MinibossKilledCount, BossKilledCount, CurrentTuning.RunRewardMultiplier));

        private SurvivorsPersistentProgression _persistentProgression;
        private SurvivorsPersistentProgression PersistentProgression => _persistentProgression ?? (_persistentProgression = new SurvivorsPersistentProgression(this));
        private SurvivorsRunRewards _runRewards;
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

        private static string ResolveRewardKindLabel(SurvivorsRewardSelectionKind kind) => SurvivorsDraftSelectionRewards.ResolveRewardKindLabel(kind);

        private SurvivorsEndlessSurgeRewards _endlessSurges;
        private SurvivorsEndlessSurgeRewards EndlessSurges => _endlessSurges ?? (_endlessSurges = new SurvivorsEndlessSurgeRewards(_runSession, this));
        private SurvivorsDraftSelectionRewards _selectionRewards;
        private SurvivorsDraftSelectionRewards SelectionRewards => _selectionRewards ?? (_selectionRewards = new SurvivorsDraftSelectionRewards(RunBuild, this));
        private void TryActivateEndlessSurge(SurvivorsEnemyRole role, Vector3 position, int baseExperienceReward) => EndlessSurges.TryActivateEndlessSurge(role, position, baseExperienceReward);
        private void TickEndlessSurge(float deltaTime) => EndlessSurges.TickEndlessSurge(deltaTime);
        private void TriggerRewardUpgradeSurge(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind kind) => SelectionRewards.TriggerRewardUpgradeSurge(upgrade, kind);
        private void TriggerLevelUpPulse(RunUpgradeDefinition upgrade) => SelectionRewards.TriggerLevelUpPulse(upgrade);
        private void TriggerRewardJackpot(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind kind) => SelectionRewards.TriggerRewardJackpot(upgrade, kind);
        SurvivorsTemplateTuning ISurvivorsPickupRewardPort.Tuning => CurrentTuning;
        Vector3 ISurvivorsPickupRewardPort.PlayerPosition => PlayerPosition;
        string ISurvivorsPickupRewardPort.CurrencyLabel => CurrencyRewardLabel;
        int ISurvivorsPickupRewardPort.DamageNonMajor(Vector3 position, float radius, float damage, string source) => DamageNonMajorEnemies(position, radius, damage, source);
        bool ISurvivorsPickupRewardPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount, bool attract)
        {
            SurvivorsPickupActor pickup = SpawnPickup(kind, position, amount);
            if (pickup == null) return false;
            if (attract) StartMajorRewardCacheAttraction(pickup);
            return true;
        }
        void ISurvivorsPickupRewardPort.ShowFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);
        void ISurvivorsPickupRewardPort.PlayPulse(Vector3 position, int count, bool boss, bool pickupAudio) => PlayFeedback(boss ? _bossPulse : _levelUpPulse, position, count, pickupAudio ? _pickupClip : boss ? _bossClip : _levelUpClip);

        private SurvivorsRuntimeWorld _runtimeWorld;
        private SurvivorsRuntimeCamera _runtimeCamera;
        private SurvivorsRuntimeCamera RuntimeCamera => _runtimeCamera ?? (_runtimeCamera = new SurvivorsRuntimeCamera());
        private void BuildRuntimeWorld()
        {
            _runtimeWorld = new SurvivorsRuntimeWorld(transform, SurvivorsRuntimeWorldPalette.Capture(ActiveUiTheme));
            _poseResolver = new SurvivorsSpawnPoseResolver(this);
            _runtimeWorld.BuildSpawning(_poseResolver, CurrentTuning.EnemyMaximumAlive);
            if (buildVisualsOnAwake)
            {
                Arena.Build(_worldRoot);
                UpdateArenaPresentation();
            }
            BuildFeedbackPresentation();
            EnsureCamera();
        }
        private void EnsureCamera() => RuntimeCamera.Ensure(PlayerPosition, Camera.main);
        private void LateUpdate()
        {
            UpdateArenaPresentation();
            _runtimeCamera?.Follow(_playerObject == null ? null : _playerObject.transform, Time.deltaTime);
        }

        public bool ConfigureAuthoredContent(TextAsset enemyLibrary, TextAsset runFlowLibrary, TextAsset rewardLibrary) => ContentBinding.ConfigureAuthoredContent(enemyLibrary, runFlowLibrary, rewardLibrary);

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary) => ContentBinding.ConfigureAuthoredContent(weaponLibrary, upgradeLibrary, relicLibrary, classLibrary, progressionLibrary, enemyLibrary, runFlowLibrary, rewardLibrary);

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy) => ContentBinding.ConfigureAuthoredContent(weaponLibrary, upgradeLibrary, relicLibrary, classLibrary, progressionLibrary, enemyLibrary, runFlowLibrary, rewardLibrary, bindingPolicy);

        public bool ConfigureAuthoredContentJson(string enemyJson, string runFlowJson, string rewardJson) => ContentBinding.ConfigureAuthoredContentJson(enemyJson, runFlowJson, rewardJson);

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson) => ContentBinding.ConfigureAuthoredContentJson(weaponJson, upgradeJson, relicJson, classJson, progressionJson, enemyJson, runFlowJson, rewardJson);

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy) => ContentBinding.ConfigureAuthoredContentJson(weaponJson, upgradeJson, relicJson, classJson, progressionJson, enemyJson, runFlowJson, rewardJson, bindingPolicy);

        public bool ConfigureStrictSampleContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset pickupLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary,
            TextAsset defaultThemeLibrary,
            TextAsset alternateThemeLibrary) => ContentBinding.ConfigureStrictSampleContent(weaponLibrary, upgradeLibrary, relicLibrary, classLibrary, progressionLibrary, enemyLibrary, pickupLibrary, runFlowLibrary, rewardLibrary, defaultThemeLibrary, alternateThemeLibrary);

        private SurvivorsMetaProgressionDefinition ResolveMetaProgressionDefinition() => RuntimeContent.ResolveMetaProgressionDefinition();

        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateWeaponArchetypeDefinitions(SurvivorsTemplateTuning resolved) => RuntimeContent.CreateWeaponArchetypeDefinitions(resolved);

        private IReadOnlyList<SurvivorsRelicDefinition> CreateRelicDefinitions() => RuntimeContent.CreateRelicDefinitions();

        private SurvivorsClassLibraryDefinition CreateClassLibraryDefinition() => RuntimeContent.CreateClassLibraryDefinition();

        private IReadOnlyList<SurvivorsClassUpgradeGateDefinition> CreateClassUpgradeGates() => RuntimeContent.CreateClassUpgradeGates();

        private RunUpgradeCatalog CreateBaseRunUpgradeCatalog() => RuntimeContent.CreateBaseRunUpgradeCatalog();

        private IReadOnlyList<SurvivorsRunUpgradeMetadata> CreateRunUpgradeMetadata() => RuntimeContent.CreateRunUpgradeMetadata();

        private SurvivorsRunFlowDefinition CreateRunFlowDefinition(SurvivorsTemplateTuning resolved) => RuntimeContent.CreateRunFlowDefinition(resolved);

        private SurvivorsTemplateTuning CreateConfiguredTuning(SurvivorsPacingProfile profile) => RuntimeContent.CreateConfiguredTuning(profile);

        private SurvivorsContentBinding _contentBinding;
        private SurvivorsContentBinding ContentBinding => _contentBinding ?? (_contentBinding = new SurvivorsContentBinding(this));
        private SurvivorsRuntimeContentResolver _runtimeContent;
        private SurvivorsRuntimeContentResolver RuntimeContent => _runtimeContent ?? (_runtimeContent = new SurvivorsRuntimeContentResolver(ContentBinding));
        bool ISurvivorsContentBindingPort.RunStarted => _runSession.Started;
        void ISurvivorsContentBindingPort.RefreshConfiguredTuning() => tuning = CreateConfiguredTuning(pacingProfile);
        void ISurvivorsContentBindingPort.ReleaseProfile() => ReleaseMetaProgressionService();
        bool ISurvivorsContentBindingPort.ConfigureUiThemes(TextAsset primary, TextAsset alternate) => ConfigureUiThemes(primary, alternate);

        private SurvivorsEnemyDefeatFlow _defeats;
        private SurvivorsEnemyDefeatFlow Defeats => _defeats ?? (_defeats = new SurvivorsEnemyDefeatFlow(this, _runSession));
        internal void HandleEnemyKilled(SurvivorsEnemyActor enemy, string source, bool applyAugments)
        { if (enemy != null) Defeats.HandleEnemyKilled(enemy, source, applyAugments); }
        void ISurvivorsEnemyDefeatPort.RecordKillMetric(SurvivorsEnemyRole role)
        {
            Telemetry.Record(SurvivorsRunMetric.FirstKill, RunTimeSeconds);
            if (IsEliteRole(role)) Telemetry.Record(SurvivorsRunMetric.FirstEliteKill, RunTimeSeconds);
            else if (role == SurvivorsEnemyRole.Miniboss) Telemetry.Record(SurvivorsRunMetric.FirstMinibossKill, RunTimeSeconds);
            else if (role == SurvivorsEnemyRole.Boss) Telemetry.Record(SurvivorsRunMetric.FirstBossKill, RunTimeSeconds);
        }
        void ISurvivorsEnemyDefeatPort.SpawnExperience(Vector3 position, int amount) => SpawnPickup(SurvivorsPickupKind.Experience, position, amount);
        void ISurvivorsEnemyDefeatPort.RegisterStreak(Vector3 position) => RegisterKillStreak(position);
        void ISurvivorsEnemyDefeatPort.ShowDeath(Vector3 position, SurvivorsEnemyRole role, float radius) => RecordEnemyDeathEffect(position, role, radius);
        void ISurvivorsEnemyDefeatPort.TriggerDeathNova(Vector3 position, string source, bool applyAugments) => TryTriggerDeathNova(position, source, applyAugments);
        void ISurvivorsEnemyDefeatPort.SpawnMajorRewards(Vector3 position, SurvivorsEnemyRole role, float radius, int xp)
        {
            RecordMajorRewardDropFeedback(position, role, radius);
            SpawnMajorRewardPickupCache(position, role, radius);
            TryDropHealthPickup(position + new Vector3(radius * 0.7f, 0f, radius * 0.35f));
            TryActivateEndlessSurge(role, position, xp);
        }
        void ISurvivorsEnemyDefeatPort.PlayDeath(Vector3 position, int burst) => PlayFeedback(_killPulse, position, burst, _killClip, AudioEventEnemyDeath, 0.08f);
        void ISurvivorsEnemyDefeatPort.SpawnSplitterChildren(Vector3 position, string displayName) => SpawnSplitterChildren(position, displayName);
        void ISurvivorsEnemyDefeatPort.GrantMajorEnemyReward(SurvivorsEnemyRole role) => GrantMajorEnemyReward(role);
        bool ISurvivorsEnemyDefeatPort.OpenUpgradeRewardDraft(SurvivorsEnemyRole role) => OpenUpgradeRewardDraft(role, requireEvolutionChoice: false);
        void ISurvivorsEnemyDefeatPort.OpenBossRelicDraft() => OpenBossRelicDraft();
        void ISurvivorsEnemyDefeatPort.EnterVictory() => EnterVictory();
        void ISurvivorsEnemyDefeatPort.RewardHordeClear(Vector3 position) => HordeRush.SpawnHordeRushClearReward(position);
        void ISurvivorsEnemyDefeatPort.RewardCacheClear(Vector3 position) => RoamingCaches.SpawnRoamingCacheAmbushClearReward(position);
        void ISurvivorsEnemyDefeatPort.RewardShrineClear(Vector3 position) => ShrineTrials.SpawnArenaShrineClearReward(position);

        private SurvivorsRunSummaryModel _runSummary;
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

        private SurvivorsEvolutionHudModel _evolutionHud;
        private SurvivorsEvolutionHudModel EvolutionHud => _evolutionHud ?? (_evolutionHud = new SurvivorsEvolutionHudModel(RunBuild, DraftOffers.Catalogs));
        private string ResolveEvolutionGoalHudLabel() => EvolutionHud.Goal();
        private string ResolveEvolutionReadyHudLabel() => EvolutionHud.Ready();
        private string ResolveEvolutionObjectiveHudLabel() => EvolutionHud.Objective();

        private static string FormatRunTime(float seconds) => SurvivorsRunText.FormatRunTime(seconds);
        private static string FormatMetricTime(float seconds) => SurvivorsRunText.FormatMetricTime(seconds);
        private static string FormatRewardTimeout(float seconds) => SurvivorsRunText.FormatRewardTimeout(seconds);
        private string ResolveRunPhaseHudLabel() => SurvivorsRunText.FormatPhase(IsEndlessRun, RunPhase);
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

        private void SpawnMajorRewardPickupCache(Vector3 position, SurvivorsEnemyRole role, float radius) => MajorRewardPickupCache.SpawnMajorRewardPickupCache(position, role, radius);

        private bool StartMajorRewardCacheAttraction(SurvivorsPickupActor pickup) => MajorRewardPickupCache.StartMajorRewardCacheAttraction(pickup);

        private int StartMagnetRecall() => PickupCollection.StartMagnetRecall();

        private float ResolvePickupMagnetPulseIntervalSeconds() => PickupCollection.ResolvePickupMagnetPulseIntervalSeconds();

        private void TickPickupMagnetPulse(float deltaTime) => PickupCollection.TickPickupMagnetPulse(deltaTime);

        internal void CollectPickup(SurvivorsPickupActor pickup) => PickupCollection.CollectPickup(pickup);

        internal void RecordPickupAttractionFeedback(SurvivorsPickupKind kind, Vector3 position) => PickupCollection.RecordPickupAttractionFeedback(kind, position);

        private SurvivorsMajorRewardPickupCache _majorRewardPickupCache;
        private SurvivorsMajorRewardPickupCache MajorRewardPickupCache => _majorRewardPickupCache ?? (_majorRewardPickupCache = new SurvivorsMajorRewardPickupCache(this));
        private SurvivorsPickupCollection _pickupCollection;
        private SurvivorsPickupCollection PickupCollection => _pickupCollection ?? (_pickupCollection = new SurvivorsPickupCollection(_pickups, this));
        SurvivorsTemplateTuning ISurvivorsMajorRewardPickupCachePort.Tuning => CurrentTuning;
        int ISurvivorsMajorRewardPickupCachePort.RunEscalationLevel => RunEscalationLevel;
        bool ISurvivorsMajorRewardPickupCachePort.IsMajorRewardRole(SurvivorsEnemyRole role) => IsMajorRewardRole(role);
        SurvivorsPickupActor ISurvivorsMajorRewardPickupCachePort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount);
        string ISurvivorsMajorRewardPickupCachePort.ResolveMajorRewardDropLabel(SurvivorsEnemyRole role) => ResolveMajorRewardDropLabel(role);
        Color ISurvivorsMajorRewardPickupCachePort.ResolveMajorRewardDropColor(SurvivorsEnemyRole role) => ResolveMajorRewardDropColor(role);
        void ISurvivorsMajorRewardPickupCachePort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);
        SurvivorsTemplateTuning ISurvivorsPickupCollectionPort.Tuning => CurrentTuning;
        float ISurvivorsPickupCollectionPort.PickupMagnetPulseIntervalReductionBonus => PickupMagnetPulseIntervalReductionBonus;
        Vector3 ISurvivorsPickupCollectionPort.PlayerPosition => PlayerPosition;
        void ISurvivorsPickupCollectionPort.RecordFirstExperiencePickupTime() => Telemetry.Record(SurvivorsRunMetric.FirstExperiencePickup, RunTimeSeconds);
        int ISurvivorsPickupCollectionPort.GainExperience(int amount) => GainExperience(amount);
        void ISurvivorsPickupCollectionPort.RecordExperienceCombo(int gained) => RecordExperienceCombo(gained);
        void ISurvivorsPickupCollectionPort.RestoreHealthFromPickup(int amount) => PlayerVitals.RestoreHealthFromPickup(amount);
        void ISurvivorsPickupCollectionPort.AddBloodShards(int amount) => RunRewards.AddBloodShards(amount);
        void ISurvivorsPickupCollectionPort.PlayCollectionFeedback(Vector3 position, SurvivorsPickupKind kind, int burstCount)
        {
            string audioEventId = kind == SurvivorsPickupKind.Experience ? AudioEventXpPickup : AudioEventUiSelect;
            PlayFeedback(_pickupPulse, position, burstCount, _pickupClip, audioEventId, 0.12f);
        }
        void ISurvivorsPickupCollectionPort.PlayAttractionFeedback(Vector3 position) => PlayFeedback(_pickupPulse, position, 6, null);
        void ISurvivorsPickupCollectionPort.PlayMagnetRecallFeedback(Vector3 position, int burstCount) => PlayFeedback(_pickupPulse, position, burstCount, _pickupClip, AudioEventMagnetPulse, 0.4f);
        void ISurvivorsPickupCollectionPort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);
        void ISurvivorsPickupCollectionPort.DespawnCompletedPickup(SpawnInstanceId id) => _spawnService?.Despawn(id, DespawnReason.Completed);

        internal DamageResolutionResult ResolveEnemyDamage(HealthState health, float amount, string source, bool applyAugments) => EnemyDamage.ResolveEnemyDamage(health, amount, source, applyAugments);

        private static bool CanApplyDamageAugments(string source) => SurvivorsEnemyDamage.CanApplyDamageAugments(source);

        private SurvivorsDamageAugments _damageAugments;
        private SurvivorsDamageAugments DamageAugments => _damageAugments ?? (_damageAugments = new SurvivorsDamageAugments(this));
        private SurvivorsEnemyDamage _enemyDamage;
        private SurvivorsEnemyDamage EnemyDamage => _enemyDamage ?? (_enemyDamage = new SurvivorsEnemyDamage(() => CriticalChanceNormalized, () => CriticalDamageMultiplier));
        SurvivorsDamageAugmentValues ISurvivorsDamageAugmentPort.Values => new SurvivorsDamageAugmentValues(LifestealRatio, BarrierOnDamageRatio, PoisonDamageRatio, BleedDamageRatio, ExecuteThresholdNormalized);
        SurvivorsTemplateTuning ISurvivorsDamageAugmentPort.Tuning => CurrentTuning;
        bool ISurvivorsDamageAugmentPort.IsPlayerBound => PlayerVitals.IsBound;
        void ISurvivorsDamageAugmentPort.HealPlayer(float amount) => PlayerVitals.Heal(amount);
        void ISurvivorsDamageAugmentPort.RestoreBarrier(float amount) => PlayerVitals.RestoreBarrier(amount);
        bool ISurvivorsDamageAugmentPort.IsEvolutionActive(string id) => IsEvolutionActive(id);
        string ISurvivorsDamageAugmentPort.ResolveUpgradeName(string id) => ResolveUpgradeDisplayName(new RunUpgradeId(id));
        string ISurvivorsDamageAugmentPort.ResolveWeaponName(string id) => ResolveWeaponBuildDisplayName(id);
        internal void ApplyDamageAugmentsToEnemy(SurvivorsEnemyActor enemy, DamageResult damage, string source)
        { if (enemy != null) DamageAugments.ApplyDamageAugmentsToEnemy(enemy, damage, source); }
        internal void ApplyWeaponStatusEffectsToEnemy(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition, DamageResult damage)
        { if (enemy != null) DamageAugments.ApplyWeaponStatusEffectsToEnemy(enemy, definition, damage); }

        private SurvivorsPayloadHazardRewards _payloadHazards;
        private SurvivorsPayloadHazardRewards PayloadHazards => _payloadHazards ?? (_payloadHazards = new SurvivorsPayloadHazardRewards(this));
        private SurvivorsDeathNova _deathNova;
        private SurvivorsDeathNova DeathNova => _deathNova ?? (_deathNova = new SurvivorsDeathNova(_enemies, () => DeathNovaDamage, () => DeathNovaRadius, (position, count) => PlayFeedback(_killPulse, position, count, _killClip)));
        internal void RecordPayloadHazardTick() => PayloadHazards.RecordPayloadHazardTick();
        internal void RecordPayloadHazardSnare(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition, Vector3 origin)
        { if (enemy != null) PayloadHazards.RecordPayloadHazardSnare(enemy.DisplayName, definition, origin); }
        private void TickPayloadHazardChain(float deltaTime) => PayloadHazards.TickPayloadHazardChain(deltaTime);
        private void TryTriggerDeathNova(Vector3 position, string source, bool applyAugments) => DeathNova.TryTriggerDeathNova(position, source, applyAugments);

        private IReadOnlyList<string> ResolveBuildMenuCurrentBuildLines() => SurvivorsBuildMenuTextModel.CurrentBuild(ResolveBuildHudSummaryLines());
        private IReadOnlyList<string> ResolveBuildMenuControlsLines() => SurvivorsBuildMenuTextModel.Controls();
        private IReadOnlyList<string> ResolveBuildMenuStatsLines() => SurvivorsBuildMenuTextModel.Stats(CaptureBuildMenuStatsValues());
        private IReadOnlyList<string> ResolveBuildMenuRunInfoLines() => SurvivorsBuildMenuTextModel.RunInfo(CaptureBuildMenuRunInfoValues());
        private IReadOnlyList<string> ResolvePlayerHudLines() => SurvivorsPlayerHudTextModel.BuildLines(CapturePlayerHudValues());

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

        private SurvivorsPlayerHudValues CapturePlayerHudValues() => new SurvivorsPlayerHudValues(
            milestoneLabel: CurrentRunMilestoneHudLabel,
            buildSlotLabel: ResolveBuildSlotHudLabel(),
            activeWeaponLabel: FormatActiveWeaponList(),
            currentPickupAttractRange: CurrentPickupAttractRange,
            currentPickupAttractionSpeed: CurrentPickupAttractionSpeed,
            pickupPulseLabel: FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds),
            evolutionLabel: ResolveEvolutionObjectiveHudLabel());


        private readonly SurvivorsRunTelemetry Telemetry = new SurvivorsRunTelemetry();
        private void RecordLevelCheckpoints() => Telemetry.RecordLevelCheckpoints(RunTimeSeconds, Level);

        internal static bool IsMajorThreatSlamRole(SurvivorsEnemyRole role) => SurvivorsMajorThreatAbilities.IsMajorThreatSlamRole(role);

        internal void RecordMajorThreatSlamTelegraph(SurvivorsEnemyActor enemy) => MajorThreatAbilities.RecordMajorThreatSlamTelegraph(enemy);

        internal void ResolveMajorThreatSlam(SurvivorsEnemyActor enemy) => MajorThreatAbilities.ResolveMajorThreatSlam(enemy);

        private void TryTriggerMajorThreatEnrage(SurvivorsEnemyActor enemy) => MajorThreatAbilities.TryTriggerMajorThreatEnrage(enemy);

        private void SpawnSplitterChildren(Vector3 position, string splitterName) => EnemySupportSpawning.SpawnSplitterChildren(position, splitterName);

        internal int SpawnSummonerSupport(SurvivorsEnemyActor summoner) => EnemySupportSpawning.SpawnSummonerSupport(summoner);

        private SurvivorsEnemySupportSpawning _enemySupportSpawning;
        private SurvivorsEnemySupportSpawning EnemySupportSpawning => _enemySupportSpawning ?? (_enemySupportSpawning = new SurvivorsEnemySupportSpawning(this));
        private SurvivorsMajorThreatAbilities _majorThreatAbilities;
        private SurvivorsMajorThreatAbilities MajorThreatAbilities => _majorThreatAbilities ?? (_majorThreatAbilities = new SurvivorsMajorThreatAbilities(this, EnemySupportSpawning));
        SurvivorsTemplateTuning ISurvivorsEnemySupportSpawnPort.Tuning => CurrentTuning;
        SurvivorsRunState ISurvivorsEnemySupportSpawnPort.State => State;
        int ISurvivorsEnemySupportSpawnPort.MaximumAlive => ResolveEnemyMaximumAlive();
        int ISurvivorsEnemySupportSpawnPort.EnemyCount => _enemies.Count;
        long ISurvivorsEnemySupportSpawnPort.SpawnSequence => _spawnSequence;
        SurvivorsEnemyActor ISurvivorsEnemySupportSpawnPort.SpawnGameplayEnemyOffscreen(SurvivorsEnemyRole role, long seed, float minimumDistance, float maximumDistance, string spawnSource) => SpawnGameplayEnemyOffscreen(role, seed, minimumDistance, maximumDistance, spawnSource);
        void ISurvivorsEnemySupportSpawnPort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);
        void ISurvivorsEnemySupportSpawnPort.PlaySupportSpawnFeedback(Vector3 position, int burstCount) => PlayFeedback(_spawnPulse, position, burstCount, _spawnClip);
        SurvivorsTemplateTuning ISurvivorsMajorThreatAbilityPort.Tuning => CurrentTuning;
        SurvivorsRunState ISurvivorsMajorThreatAbilityPort.State => State;
        Vector3 ISurvivorsMajorThreatAbilityPort.PlayerPosition => PlayerPosition;
        float ISurvivorsMajorThreatAbilityPort.CurrentHealth => CurrentHealth;
        float ISurvivorsMajorThreatAbilityPort.BarrierValue => BarrierValue;
        bool ISurvivorsMajorThreatAbilityPort.IsMajorRewardRole(SurvivorsEnemyRole role) => IsMajorRewardRole(role);
        string ISurvivorsMajorThreatAbilityPort.ResolveMajorThreatHealthFallbackLabel(SurvivorsEnemyRole role) => ResolveMajorThreatHealthFallbackLabel(role);
        void ISurvivorsMajorThreatAbilityPort.ApplyDamageToPlayer(float amount, string source) => ApplyDamageToPlayer(amount, source);
        void ISurvivorsMajorThreatAbilityPort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);
        void ISurvivorsMajorThreatAbilityPort.RecordMajorThreatSlamTelegraphEffect(Vector3 position, SurvivorsEnemyRole role, float radius, float durationSeconds) => RecordMajorThreatSlamTelegraphEffect(position, role, radius, durationSeconds);
        void ISurvivorsMajorThreatAbilityPort.PlayBossFeedback(Vector3 position, int burstCount) => PlayFeedback(_bossPulse, position, burstCount, _dangerClip);

        private SurvivorsFrameInput _frameInput;
        private SurvivorsFrameInput FrameInput => _frameInput ?? (_frameInput = new SurvivorsFrameInput(_runSession, Menus, new SurvivorsUnityKeyboard(), this));
        private void Update() => FrameInput.Tick(Time.deltaTime);
        void ISurvivorsFrameInputPort.TickPresentation(float deltaTime) => TickPresentation(deltaTime);
        void ISurvivorsFrameInputPort.TickRewardSelectionTimeout(float deltaTime) => TickRewardSelectionTimeout(deltaTime);
        void ISurvivorsFrameInputPort.SelectMode(SurvivorsPacingProfile profile) => SelectRunMode(profile);
        void ISurvivorsFrameInputPort.ContinueAfterVictory() => ContinueAfterVictory();
        void ISurvivorsFrameInputPort.RestartRun() => RestartRun();
        void ISurvivorsFrameInputPort.TriggerMagnetRecall() => TriggerMagnetRecall();
        void ISurvivorsFrameInputPort.BanishDraftChoice(int index) => BanishDraftChoice(index);
        void ISurvivorsFrameInputPort.RerollCurrentDraft() => RerollCurrentDraft();
        void ISurvivorsFrameInputPort.SkipCurrentDraft() => SkipCurrentDraft();
        void ISurvivorsFrameInputPort.SelectUpgrade(int index) => SelectUpgrade(index);
        bool ISurvivorsFrameInputPort.TryPurchaseResultMetaUpgrade(int index) => TryPurchaseResultMetaUpgrade(index);
        void ISurvivorsFrameInputPort.Dash(Vector2 movement) => PlayerMotion.TryDash(movement);
        void ISurvivorsFrameInputPort.Simulate(float deltaTime, Vector2 movement) => Simulate(deltaTime, movement);
        private void TickPresentation(float dt)
        {
            TickDamagePopups(dt);
            TickWorldFeedbackEffects(dt);
            TickEnemyRangedAttackFeedbackEffects(dt);
            TickMajorThreatSlamTelegraphEffects(dt);
            TickIncomingThreatTelegraphEffects(dt);
            TickMajorRewardDropFeedbackEffects(dt);
            TickRewardFeedback(dt);
            TickStreakRewardFeedback(dt);
            TickClassUnlockRewardFeedback(dt);
            TickEvolutionReadyFeedback(dt);
            TickExperienceComboFeedback(dt);
        }

        private SurvivorsActorSimulation _actorSimulation;
        private SurvivorsActorSimulation ActorSimulation => _actorSimulation ?? (_actorSimulation = new SurvivorsActorSimulation(_enemies, _projectiles, _pickups, TryUpdateEnemyLeash,
            () => new SurvivorsPickupAttractionValues(CurrentPickupAttractRange, CurrentPickupAttractionSpeed, CurrentTuning.PickupCollectRadius)));
        private void TickEnemies(float deltaTime) => ActorSimulation.TickEnemies(deltaTime);
        private void TickProjectiles(float deltaTime) => ActorSimulation.TickProjectiles(deltaTime);
        private void TickPickups(float deltaTime) => ActorSimulation.TickPickups(deltaTime);
        private SurvivorsRunSimulation _simulation;
        private SurvivorsRunSimulation Simulation => _simulation ?? (_simulation = CreateSimulation());
        public void Simulate(float deltaTime, Vector2 movementInput = default) => Simulation.Simulate(deltaTime, movementInput);
        private SurvivorsRunSimulation CreateSimulation() => new SurvivorsRunSimulation(_runSession, Menus, TickPresentation, TickRewardSelectionTimeout,
            new Action<SurvivorsSimulationFrame>[]
            {
                frame => RecordLevelCheckpoints(),
                frame => TickLevelUpDraftCooldown(frame.DeltaTime),
                frame => TimedEncounters.TickMajorThreatWarnings(),
                frame => TimedEncounters.TickRunFlow(),
            },
            new Action<SurvivorsSimulationFrame>[]
            {
                frame => HordeRush.TickHordeRushEvents(),
                frame => PlayerVitals.TickSafety(frame.DeltaTime),
                frame => PlayerMotion.TickCooldown(frame.DeltaTime),
                frame => TickKillStreak(frame.DeltaTime),
                frame => TickStreakSurge(frame.DeltaTime),
                frame => RoamingCaches.TickRoamingCacheSurge(frame.DeltaTime),
                frame => ShrineTrials.TickArenaShrineSurge(frame.DeltaTime),
                frame => Waystones.TickWaystoneFocus(frame.DeltaTime),
                frame => Waystones.TickWaystoneChainSurge(frame.DeltaTime),
                frame => HordeRush.TickHordeRushClearSurge(frame.DeltaTime),
                frame => TickWeaponLoadoutSurge(frame.DeltaTime),
                frame => TickPassiveLoadoutSurge(frame.DeltaTime),
                frame => TickBossRelicSurge(frame.DeltaTime),
                frame => TickPayloadHazardChain(frame.DeltaTime),
                frame => TickGemRush(frame.DeltaTime),
                frame => TickPickupMagnetPulse(frame.DeltaTime),
                frame => TickEvolutionChainSurge(frame.DeltaTime),
                frame => TickEndlessSurge(frame.DeltaTime),
                frame => PlayerVitals.TickBarrier(frame.DeltaTime),
                frame => PlayerMotion.MovePlayer(frame.Movement, frame.DeltaTime),
                frame => TickArenaWaystoneDiscoveries(),
                frame => UpdateArenaPresentation(),
                frame => TickArenaWaystoneDiscoveries(),
                frame => TickEnemySpawning(frame.DeltaTime),
                frame => TickWeapon(frame.DeltaTime),
                frame => TickEnemies(frame.DeltaTime),
                frame => TickProjectiles(frame.DeltaTime),
                frame => TickPickups(frame.DeltaTime),
                frame => UpdateOffscreenThreatMarkerSnapshot(),
            });

        private SurvivorsEnemyProfile ResolveEnemyProfile(SurvivorsEnemyRole role) => EnemySpawner.ResolveEnemyProfile(role);

        private SurvivorsEnemyActor SpawnEnemy(Vector3 position, bool explicitPosition, SurvivorsEnemyRole role, bool gameplaySpawn = false, string spawnSource = null) => EnemySpawner.SpawnEnemy(position, explicitPosition, role, gameplaySpawn, spawnSource);

        private SurvivorsEnemyActor SpawnGameplayEnemyOffscreen(
            SurvivorsEnemyRole role,
            long seed,
            float minimumDistance,
            float maximumDistance,
            string spawnSource) => EnemySpawner.SpawnGameplayEnemyOffscreen(role, seed, minimumDistance, maximumDistance, spawnSource);

        private static WorldSpawnableId ResolveEnemySpawnableId(SurvivorsEnemyRole role) => SurvivorsEnemySpawner.ResolveEnemySpawnableId(role);

        private static string ResolveEnemyGroupId(SurvivorsEnemyRole role) => SurvivorsEnemySpawner.ResolveEnemyGroupId(role);

        private SurvivorsPickupActor SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => PickupSpawner.SpawnPickup(kind, position, amount);

        private static WorldSpawnableId ResolvePickupSpawnableId(SurvivorsPickupKind kind) => SurvivorsPickupSpawner.ResolvePickupSpawnableId(kind);

        internal bool LaunchProjectile(SurvivorsWeaponArchetypeDefinition definition, Vector3 direction) => ProjectileLauncher.LaunchProjectile(definition, direction);

        internal bool LaunchProjectileFrom(
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 origin,
            Vector3 direction,
            int remainingChains,
            int remainingPierces,
            int remainingForks,
            int remainingReturns,
            HashSet<int> ignoredEnemyIds) => ProjectileLauncher.LaunchProjectileFrom(definition, origin, direction, remainingChains, remainingPierces, remainingForks, remainingReturns, ignoredEnemyIds);

        private SurvivorsEnemySpawner _enemySpawner;
        private SurvivorsEnemySpawner EnemySpawner => _enemySpawner ?? (_enemySpawner = new SurvivorsEnemySpawner(this, SpawnSequence));
        private SurvivorsPickupSpawner _pickupSpawner;
        private SurvivorsPickupSpawner PickupSpawner => _pickupSpawner ?? (_pickupSpawner = new SurvivorsPickupSpawner(this, SpawnSequence));
        private SurvivorsProjectileLauncher _projectileLauncher;
        private SurvivorsProjectileLauncher ProjectileLauncher => _projectileLauncher ?? (_projectileLauncher = new SurvivorsProjectileLauncher(this, SpawnSequence));
        bool ISurvivorsSpawnBackend.HasSpawnService => _spawnService != null;
        void ISurvivorsSpawnBackend.RegisterExplicitPose(long sequence, Vector3 position) => _poseResolver.RegisterExplicitPose(sequence, position);
        SpawnResult ISurvivorsSpawnBackend.Spawn(WorldSpawnRequest request) => _spawnService.Spawn(request);
        SurvivorsTemplateTuning ISurvivorsEnemySpawnPort.Tuning => CurrentTuning;
        SurvivorsRunFlowRuntime ISurvivorsEnemySpawnPort.RunFlow => _runFlow;
        Vector3 ISurvivorsEnemySpawnPort.PlayerPosition => PlayerPosition;
        float ISurvivorsEnemySpawnPort.ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole role, float requested) => ResolveGameplaySpawnMinimumDistance(role, requested);
        float ISurvivorsEnemySpawnPort.ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole role, float minimum, float maximum) => ResolveGameplaySpawnMaximumDistance(role, minimum, maximum);
        float ISurvivorsEnemySpawnPort.ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string source) => ResolveOffscreenSpawnPadding(role, source);
        Vector3 ISurvivorsEnemySpawnPort.ResolveSafeOffscreenPosition(Vector3 center, float minimum, float maximum, long seed, float padding, float bandDepth) => ResolveSafeOffscreenPosition(center, minimum, maximum, seed, padding, bandDepth);
        void ISurvivorsEnemySpawnPort.InitializeEnemy(SurvivorsEnemyActor enemy, SurvivorsEnemyProfile profile) => enemy.Initialize(this, profile);
        void ISurvivorsEnemySpawnPort.RegisterEnemy(SurvivorsEnemyActor enemy) => _enemies.Add(enemy);
        void ISurvivorsEnemySpawnPort.RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string source) => RecordGameplaySpawnSafety(role, position, source);
        void ISurvivorsEnemySpawnPort.RecordSpawnMetric(SurvivorsEnemyRole role)
        {
            Telemetry.Record(role == SurvivorsEnemyRole.Miniboss ? SurvivorsRunMetric.FirstMinibossSpawn :
                role == SurvivorsEnemyRole.Boss ? SurvivorsRunMetric.FirstBossSpawn : SurvivorsRunMetric.FirstEliteSpawn, RunTimeSeconds);
        }
        void ISurvivorsEnemySpawnPort.ShowEnemySpawnFeedback(bool major, Vector3 position, int burst) =>
            PlayFeedback(major ? _bossPulse : _spawnPulse, position, burst, major ? _bossClip : _spawnClip);
        SurvivorsTemplateTuning ISurvivorsPickupSpawnPort.Tuning => CurrentTuning;
        float ISurvivorsPickupSpawnPort.CurrentPickupAttractRange => CurrentPickupAttractRange;
        float ISurvivorsPickupSpawnPort.CurrentPickupAttractionSpeed => CurrentPickupAttractionSpeed;
        void ISurvivorsPickupSpawnPort.InitializePickup(SurvivorsPickupActor pickup, SurvivorsPickupKind kind, int amount, float range, float speed, float radius) => pickup.Initialize(this, kind, amount, range, speed, radius);
        void ISurvivorsPickupSpawnPort.RegisterPickup(SurvivorsPickupActor pickup) => _pickups.Add(pickup);
        Vector3 ISurvivorsProjectileLaunchPort.PlayerPosition => PlayerPosition;
        int ISurvivorsProjectileLaunchPort.ProjectileChainBonus => ProjectileChainBonus;
        int ISurvivorsProjectileLaunchPort.ProjectilePierceBonus => ProjectilePierceBonus;
        int ISurvivorsProjectileLaunchPort.ProjectileForkBonus => ProjectileForkBonus;
        int ISurvivorsProjectileLaunchPort.ProjectileReturnBonus => ProjectileReturnBonus;
        float ISurvivorsProjectileLaunchPort.ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition) => ResolveWeaponDamage(definition);
        void ISurvivorsProjectileLaunchPort.InitializeProjectile(SurvivorsProjectileActor projectile, SurvivorsWeaponArchetypeDefinition definition, SurvivorsProjectileLaunchValues values) =>
            projectile.Initialize(this, definition, values.Direction, values.Speed, values.Damage, values.Radius, values.Lifetime,
                values.Chains, values.Pierces, values.Forks, values.Returns, values.IgnoredEnemyIds);
        void ISurvivorsProjectileLaunchPort.RegisterProjectile(SurvivorsProjectileActor projectile) => _projectiles.Add(projectile);
        void ISurvivorsProjectileLaunchPort.ShowProjectileLaunchFeedback(Vector3 origin) => PlayFeedback(_firePulse, origin, 8, _fireClip);

        internal Vector3 ResolveRuntimeEnemySpawnPositionForResolver(long seed) => SpawnSafety.ResolveRuntimeEnemySpawnPositionForResolver(seed);

        public bool IsWorldPositionInsideCameraViewportForTest(Vector3 position, float padding) => SpawnSafety.IsWorldPositionInsideCameraViewportForTest(position, padding);

        private Vector3 ResolveSafeOffscreenPosition(Vector3 center, float minimumDistance, float maximumDistance, long seed) => SpawnSafety.ResolveSafeOffscreenPosition(center, minimumDistance, maximumDistance, seed);

        private Vector3 ResolveSafeOffscreenPosition(
            Vector3 center, float minimumDistance, float maximumDistance, long seed, float padding, float bandDepth) => SpawnSafety.ResolveSafeOffscreenPosition(center, minimumDistance, maximumDistance, seed, padding, bandDepth);

        private float ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole role, float requested) => SpawnSafety.ResolveGameplaySpawnMinimumDistance(role, requested);

        private float ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole role, float minimumDistance, float requestedMaximum) => SpawnSafety.ResolveGameplaySpawnMaximumDistance(role, minimumDistance, requestedMaximum);

        private float ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string spawnSource) => SpawnSafety.ResolveOffscreenSpawnPadding(role, spawnSource);

        private void RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string spawnSource) => SpawnSafety.RecordGameplaySpawnSafety(role, position, spawnSource);

        private int CountEnemiesByRole(SurvivorsEnemyRole role) => EnemyRosterQueries.CountEnemiesByRole(role);

        private int CountEliteEnemies() => EnemyRosterQueries.CountEliteEnemies();

        private static bool IsEliteRole(SurvivorsEnemyRole role) => SurvivorsEnemyRosterQueries.IsEliteRole(role);

        private static bool IsMajorRewardRole(SurvivorsEnemyRole role) => SurvivorsEnemyRosterQueries.IsMajorRewardRole(role);

        private static SurvivorsEnemyRole ResolveDebugMajorEnemyRole(SurvivorsEnemyRole role) => SurvivorsEnemyRosterQueries.ResolveDebugMajorEnemyRole(role);

        private SurvivorsSpawnSafety _spawnSafety;
        private SurvivorsSpawnSafety SpawnSafety => _spawnSafety ?? (_spawnSafety = new SurvivorsSpawnSafety(this));
        private SurvivorsEnemyRosterQueries _enemyRosterQueries;
        private SurvivorsEnemyRosterQueries EnemyRosterQueries => _enemyRosterQueries ?? (_enemyRosterQueries = new SurvivorsEnemyRosterQueries(_enemies));
        SurvivorsTemplateTuning ISurvivorsSpawnSafetyPort.Tuning => CurrentTuning;
        Vector3 ISurvivorsSpawnSafetyPort.PlayerPosition => PlayerPosition;
        bool ISurvivorsSpawnSafetyPort.TryResolveCameraGroundRect(float padding, out Rect rect) => TryResolveCameraGroundRect(padding, out rect);

        private void PlayFeedback(ParticleSystem particles, Vector3 position, int count, AudioClip clip, string audioEventId = null, float audioThrottleSeconds = 0f) => FeedbackPulses.Play(particles, position, count, clip, audioEventId, audioThrottleSeconds);

        private SurvivorsFeedbackPulses _feedbackPulses;
        private SurvivorsFeedbackPulses FeedbackPulses => _feedbackPulses ?? (_feedbackPulses = new SurvivorsFeedbackPulses(clip => AudioPresentation.Play(clip), PlayAudioEvent));
        private void BuildFeedbackPresentation()
        { FeedbackPulses.Build(_worldRoot, ActiveUiTheme); AudioPresentation.Build(_feedbackRoot); }
        private void ApplyWorldPresentation()
        {
            if (_worldRoot == null) return;
            _runtimeWorld.ApplyPalette(SurvivorsRuntimeWorldPalette.Capture(ActiveUiTheme));
            Arena.ApplyTheme();
            _feedbackPulses?.ApplyTheme(ActiveUiTheme);
        }

        public bool ConfigureUiTheme(TextAsset themeLibrary) => UiThemeSelection.ConfigureUiTheme(themeLibrary);

        public bool ConfigureUiThemeJson(string themeJson) => UiThemeSelection.ConfigureUiThemeJson(themeJson);

        public bool ConfigureUiThemes(TextAsset defaultThemeLibrary, TextAsset alternateThemeLibrary) => UiThemeSelection.ConfigureUiThemes(defaultThemeLibrary, alternateThemeLibrary);

        public bool ConfigureAdditionalUiThemeJson(string themeJson) => UiThemeSelection.ConfigureAdditionalUiThemeJson(themeJson);

        private bool SelectUiTheme(int index) => UiThemeSelection.SelectUiTheme(index);

        private void EnsureUiTheme() => UiThemeSelection.EnsureUiTheme();

        private SurvivorsUiThemeSelection _uiThemeSelection;
        private SurvivorsUiThemeSelection UiThemeSelection => _uiThemeSelection ?? (_uiThemeSelection = new SurvivorsUiThemeSelection(this));
        SurvivorsUiTheme ISurvivorsUiThemeSelectionPort.SerializedTheme { get => uiTheme; set => uiTheme = value; }
        void ISurvivorsUiThemeSelectionPort.ResetHudStyles() => ResetHudStyles();
        void ISurvivorsUiThemeSelectionPort.ApplyWorldPresentation() => ApplyWorldPresentation();
        void ISurvivorsUiThemeSelectionPort.PlayThemeSelectionAudio() => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);

        private SurvivorsEvolutionAnnouncements _evolutionAnnouncements;
        private SurvivorsEvolutionAnnouncements EvolutionAnnouncements => _evolutionAnnouncements ??
            (_evolutionAnnouncements = new SurvivorsEvolutionAnnouncements(RunBuild, DraftOffers.Catalogs,
                RecordEvolutionGoalFeedback, RecordEvolutionReadyFeedback));
        private void RecordNewlyEligibleEvolutionFeedback() => EvolutionAnnouncements.Refresh();

        private SurvivorsRunLifecycle _lifecycle;
        private SurvivorsRunLifecycle Lifecycle => _lifecycle ?? (_lifecycle = new SurvivorsRunLifecycle(_runSession, Menus, this));
        private void Start() => Lifecycle.StartAutomatically(showRunModeSelection, autoStart);
        public void ConfigureRunModeSelection(bool enabled) => Lifecycle.ConfigureModeSelection(enabled);
        public void OpenRunModeSelection() => Lifecycle.OpenModeSelection();
        public bool SelectRunMode(SurvivorsPacingProfile profile) => Lifecycle.SelectMode(profile);
        public void StartRun() => Lifecycle.StartRun();
        public bool ContinueAfterVictory() => Lifecycle.ContinueAfterVictory();
        private void EnterVictory() => Lifecycle.EnterVictory();
        bool ISurvivorsRunLifecyclePort.CanStart => CanStartConfiguredRun;
        bool ISurvivorsRunLifecyclePort.EndlessEnabled => CurrentTuning.EndlessContinuationEnabled;
        void ISurvivorsRunLifecyclePort.SetStartupFlags(bool modeSelection, bool start) { showRunModeSelection = modeSelection; autoStart = start; }
        void ISurvivorsRunLifecyclePort.DisableAutoStart() => autoStart = false;
        void ISurvivorsRunLifecyclePort.ResetDebugVisibility() => _debugOverlayVisible = false;
        void ISurvivorsRunLifecyclePort.ResetScreenScrolls()
        { BuildMenuPresenter.ResetScroll(); ResultScreen.ResetScroll(); DraftScreen.ResetScroll(); RunModePresenter.ResetScroll(); }
        void ISurvivorsRunLifecyclePort.EnsureTheme() => EnsureUiTheme();
        void ISurvivorsRunLifecyclePort.RestoreTimeScale() => Time.timeScale = 1f;
        void ISurvivorsRunLifecyclePort.ReleaseRun() => ClearRun();
        void ISurvivorsRunLifecyclePort.InitializeRun() => InitializeNewRun();
        void ISurvivorsRunLifecyclePort.ClearDrafts() => ClearRewardDrafts();
        void ISurvivorsRunLifecyclePort.ApplyPacing(SurvivorsPacingProfile profile) => ApplyPacingProfile(profile, restartRun: false);
        void ISurvivorsRunLifecyclePort.PlayModeSelected() => PlayAudioEvent(AudioEventModeSelected, _levelUpClip, 0.05f);
        void ISurvivorsRunLifecyclePort.ScheduleContinuation()
        { SwarmSpawning.Reset(); TimedEncounters.ScheduleEndlessThreats(RunTimeSeconds); HordeRush.EnsureFutureHordeRushScheduled(); }
        void ISurvivorsRunLifecyclePort.PlayContinuation() => PlayFeedback(_levelUpPulse, PlayerPosition, 32, _levelUpClip);
        void ISurvivorsRunLifecyclePort.ConsumeBossVictory() => _runFlow?.TryConsumeBossVictory();
        void ISurvivorsRunLifecyclePort.GrantVictoryRewards() => GrantRunRewards(victory: true);
        void ISurvivorsRunLifecyclePort.PlayVictory() => PlayFeedback(_levelUpPulse, PlayerPosition, 42, _levelUpClip, AudioEventVictory, 0.5f);
        private void InitializeNewRun()
        {
            _audioEvents.Reset();
            RewardHistory.ResetBestMoment();
            SurvivorsTemplateTuning resolved = CurrentTuning;
            EnemyDamage.Reset(resolved.RunSeed);
            RunWeapons.InitializeFallbackDefinition(resolved);
            _relicDefinitions = CreateRelicDefinitions();
            _upgradeClassGates = CreateClassUpgradeGates();
            _classLibrary = CreateClassLibraryDefinition();
            EnsureMetaProgressionLoaded();
            _metaProgression.EnsureDefaultClassUnlocks(_classLibrary);
            _selectedClass = _metaProgression.ResolveSelectedClass(_classLibrary);
            RunBuild.Initialize(CreateBaseRunUpgradeCatalog(), CreateRunUpgradeMetadata(), _selectedClass, _upgradeClassGates);
            RelicInventory.Reset();
            DraftSession.Reset();
            PlayerVitals.Initialize(resolved.PlayerMaxHealth);
            PlayerMotion.Reset();
            EnemySpawner.ResetDiagnostics();
            Defeats.Reset();
            ProjectileLauncher.ResetDiagnostics();
            _weaponDiagnostics.ResetHitDiagnostics();
            DamageAugments.Reset();
            _weaponDiagnostics.ResetPayloadDiagnostics();
            PayloadHazards.Reset();
            EnemySupportSpawning.ResetDiagnostics();
            RunRewards.Reset();
            PersistentProgression.ResetRunDiagnostics();
            ProgressionFeedback.ResetHistory();
            _classUnlockRewardBanner.Reset();
            DamageFeedback.ResetPlayerFeedback();
            DamageFeedback.ResetEnemyFeedback();
            DeathNova.Reset();
            RangedDodgeRewards.Reset();
            MajorRewardPickupCache.ResetDiagnostics();
            MajorThreatAbilities.ResetDiagnostics();
            PickupCollection.ResetDiagnostics();
            RewardHistory.ResetFeedbackHistory();
            ExplorationFeedback.Reset();
            RoamingCaches.Reset();
            ShrineTrials.Reset();
            Waystones.Reset();
            Waystones.ClearDiscoveries();
            StreakFeedback.Reset();
            RunSummary.Clear();
            ResetRunMetrics();
            UpgradeModifiers.Reset();
            _runSession.Reset();
            RewardDrops.ResetMetrics();
            ThreatTelegraphs.ResetMetrics();
            CombatFeedback.ResetMetrics();
            _experienceProgression.Reset();
            PlayerVitals.SetBarrier(0f);
            _enemyNavigation?.ResetDiagnostics();
            ThreatHud.Reset();
            SpawnSafety.ResetDiagnostics();
            TimedEncounters.Reset();
            HordeRush.Reset();
            KillStreakRewards.Reset();
            BuildSurges.Reset();
            SelectionRewards.Reset();
            ExperienceRhythm.Reset();
            _rewardBanner.Reset();
            _streakRewardBanner.Reset();
            SwarmSpawning.Reset();
            PickupCollection.ResetPulseSchedule();
            Traversal.Reset();
            EndlessSurges.Reset();
            _evolutionAnnouncements?.Reset();
            _evolutionReadyBanner.Reset();
            SpawnSequence.Reset();
            _damageFeedback.Reset();
            ApplyPersistentMetaBonuses();
            ApplySelectedClassBonuses();
            PlayerVitals.SetBarrier(BarrierCapacity);
            BuildRuntimeWorld();
            _runFlow = new SurvivorsRunFlowRuntime(CreateRunFlowDefinition(resolved));
            RunWeapons.BuildLoadout(resolved);
        }

        private SurvivorsHudDispatch _hudDispatch;
        private SurvivorsHudDispatch HudDispatch => _hudDispatch ?? (_hudDispatch = new SurvivorsHudDispatch(_runSession, Menus, this));
        private void OnGUI() => HudDispatch.Draw();
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

        private void RecordRewardCardPresentation(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDraft draft) => RewardHistory.RecordRewardCardPresentation(selectionKind, draft);

        private void RecordRewardCardPresentation(SurvivorsRelicDraft draft) => RewardHistory.RecordRewardCardPresentation(draft);

        private void RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDefinition selected) => RewardHistory.RecordRewardSelectionFeedback(selectionKind, selected);

        private void RecordBestRewardMoment(RunUpgradeDefinition selected) => RewardHistory.RecordBestRewardMoment(selected);

        private void RecordRelicSelectionFeedback(SurvivorsRelicDefinition selected) => RewardHistory.RecordRelicSelectionFeedback(selected);

        private void RecordRewardSkipFeedback(SurvivorsRewardSelectionKind selectionKind) => RewardHistory.RecordRewardSkipFeedback(selectionKind);

        private static RunUpgradeRarity ResolveHighestRarity(IReadOnlyList<RunUpgradeDefinition> choices) => SurvivorsRewardFeedbackHistory.ResolveHighestRarity(choices);

        private SurvivorsRewardFeedbackHistory _rewardHistory;
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

        private SurvivorsHudVitals CaptureHudVitals() => new SurvivorsHudVitals(
            MaxHealth, CurrentHealth, BarrierCapacity, BarrierValue,
            Experience, RequiredExperienceForNextLevel, Level);
        private void DrawPlayerHud()
        {
            IReadOnlyList<string> lines = ResolvePlayerHudLines();
            SurvivorsPlayerHudPresenter.Draw(Screen.width, lines, CurrentRunModeDisplayName,
                CaptureHudVitals(), _hudStyles);
        }
        private void DrawDebugOverlay() => SurvivorsDebugHudPresenter.Draw(
            Screen.height, CaptureDebugHudValues(), _hudStyles);
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
        private SurvivorsCompactHudLabels _compactHudLabels;
        private SurvivorsCompactHudLabels CompactHudLabels => _compactHudLabels ??
            (_compactHudLabels = new SurvivorsCompactHudLabels(BuildContentLabels));
        private string ResolveWeaponHudLabel() => CompactHudLabels.Weapons(ActiveWeaponIds);
        private string FormatSelectedRelicList() => SurvivorsCompactHudLabels.Relics(RelicInventory.Selected);
        private string ResolveWaystoneCompassHudLabel()
        {
            if (CurrentTuning.WaystoneDiscoveryRadius <= 0f)
                return SurvivorsCompactHudLabels.Waystone(false, WaystoneDiscoveryCount, false, 0f, Vector3.zero, false);
            if (TryResolveClosestArenaLandmark(ignoreDiscovered: true, out _, out float distance, out Vector3 delta))
                return SurvivorsCompactHudLabels.Waystone(true, WaystoneDiscoveryCount, true, distance, delta, false);
            bool any = TryResolveClosestArenaLandmark(ignoreDiscovered: false, out _, out _, out _);
            return SurvivorsCompactHudLabels.Waystone(true, WaystoneDiscoveryCount, false, 0f, Vector3.zero, any);
        }
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

        internal void RecordEnemyDamageFeedback(SurvivorsEnemyActor enemy, DamageResult damage) { if (enemy == null) return; DamageFeedback.RecordEnemyDamageFeedback(enemy, damage); }

        private void RecordPlayerDamageFeedback(DamageResult damage, Vector3 position) => DamageFeedback.RecordPlayerDamageFeedback(damage, position);

        private void RecordDamagePopup(Vector3 worldPosition, float amount, bool playerDamage, bool critical) => DamageFeedback.RecordDamagePopup(worldPosition, amount, playerDamage, critical);

        private static float ResolveDamagePopupAmount(DamageResult damage) => SurvivorsDamageFeedback.ResolveDamagePopupAmount(damage);

        internal void RecordEnemyRangedAttackDodgeFeedback(SurvivorsEnemyActor enemy) { if (enemy == null) return; RangedDodgeRewards.RecordEnemyRangedAttackDodgeFeedback(enemy); }

        private Vector3 ResolveRangedDodgeRewardPosition(SurvivorsEnemyActor enemy) => RangedDodgeRewards.ResolveRangedDodgeRewardPosition(enemy == null ? null : (ISurvivorsFeedbackEnemy)enemy);

        private static string ResolveRangedDodgeFallbackLabel(SurvivorsEnemyRole role) => SurvivorsRangedDodgeRewards.ResolveRangedDodgeFallbackLabel(role);

        private void RecordStreakRewardFeedback(string label, Color color) => StreakFeedback.RecordStreakRewardFeedback(label, color);

        private SurvivorsDamageFeedback _hitFeedback;
        private SurvivorsDamageFeedback DamageFeedback => _hitFeedback ?? (_hitFeedback = new SurvivorsDamageFeedback(this));
        private SurvivorsRangedDodgeRewards _rangedDodgeRewards;
        private SurvivorsRangedDodgeRewards RangedDodgeRewards => _rangedDodgeRewards ?? (_rangedDodgeRewards = new SurvivorsRangedDodgeRewards(this));
        private SurvivorsStreakFeedbackHistory _streakFeedback;
        private SurvivorsStreakFeedbackHistory StreakFeedback => _streakFeedback ?? (_streakFeedback = new SurvivorsStreakFeedbackHistory(_streakRewardBanner.Show));
        void ISurvivorsDamageFeedbackPort.RecordPopup(Vector3 position, float amount, bool playerDamage, bool critical) => _damageFeedback.Record(position, amount, playerDamage, critical);
        void ISurvivorsDamageFeedbackPort.PlayCombatHitAudio() => PlayAudioEvent(AudioEventCombatHit, _fireClip, 0.08f);
        void ISurvivorsDamageFeedbackPort.TryEnrage(ISurvivorsFeedbackEnemy enemy) => TryTriggerMajorThreatEnrage((SurvivorsEnemyActor)enemy);
        SurvivorsTemplateTuning ISurvivorsRangedDodgePort.Tuning => CurrentTuning;
        Vector3 ISurvivorsRangedDodgePort.PlayerPosition => PlayerPosition;
        bool ISurvivorsRangedDodgePort.TrySpawnExperience(Vector3 position, int amount) => SpawnPickup(SurvivorsPickupKind.Experience, position, amount) != null;
        void ISurvivorsRangedDodgePort.ShowStreakFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);
        void ISurvivorsRangedDodgePort.PlayDodgePulse(Vector3 position, int count) => PlayFeedback(_pickupPulse, position, count, _pickupClip);

        private float ResolveDisplayedWeaponDamage() => RunWeapons.ResolveDisplayedWeaponDamage();

        private float ResolveDisplayedWeaponCooldownSeconds() => RunWeapons.ResolveDisplayedWeaponCooldownSeconds();

        private SurvivorsWeaponArchetypeDefinition ResolvePrimaryWeaponDefinitionForDisplay() => RunWeapons.ResolvePrimaryWeaponDefinitionForDisplay();

        internal float ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition) => RunWeapons.ResolveWeaponDamage(definition);

        internal float ResolveWeaponCooldownSeconds(SurvivorsWeaponArchetypeDefinition definition) => RunWeapons.ResolveWeaponCooldownSeconds(definition);

        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> ResolveStartingWeaponDefinitions(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> allDefinitions) => RunWeapons.ResolveStartingWeaponDefinitions(allDefinitions);

        private bool TryAddWeaponToLoadout(string weaponId) => RunWeapons.TryAddWeaponToLoadout(weaponId);

        private SurvivorsWeaponArchetypeDefinition FindWeaponDefinition(string weaponId) => RunWeapons.FindWeaponDefinition(weaponId);

        private SurvivorsRunWeapons _runWeapons;
        private SurvivorsRunWeapons RunWeapons => _runWeapons ?? (_runWeapons = new SurvivorsRunWeapons(this));
        SurvivorsTemplateTuning ISurvivorsRunWeaponPort.Tuning => CurrentTuning;
        IReadOnlyList<string> ISurvivorsRunWeaponPort.StartingWeaponIds => _selectedClass == null ? null : _selectedClass.StartingWeaponIds;
        int ISurvivorsRunWeaponPort.MaximumSlots => MaxWeaponSlots;
        IReadOnlyList<SurvivorsWeaponArchetypeDefinition> ISurvivorsRunWeaponPort.CreateDefinitions(SurvivorsTemplateTuning resolved)
            => CreateWeaponArchetypeDefinitions(resolved);
        ISurvivorsWeaponLoadoutSession ISurvivorsRunWeaponPort.CreateLoadout(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions)
            => new SurvivorsWeaponLoadoutSession(new SurvivorsWeaponLoadoutRuntime(this, definitions));
        void ISurvivorsRunWeaponPort.WeaponAdded(SurvivorsWeaponArchetypeDefinition definition)
            => TryTriggerWeaponLoadoutSurge(definition);
        SurvivorsWeaponBonusValues ISurvivorsRunWeaponPort.DamageBonuses => new SurvivorsWeaponBonusValues(
            DamageBonus,
            StreakSurgeDamageBonus,
            RoamingCacheSurgeDamageBonus,
            ArenaShrineSurgeDamageBonus,
            WaystoneFocusDamageBonus,
            WaystoneChainSurgeDamageBonus,
            HordeRushClearSurgeDamageBonus,
            WeaponLoadoutSurgeDamageBonus,
            PassiveLoadoutSurgeDamageBonus,
            BossRelicSurgeDamageBonus,
            GemRushDamageBonus,
            EvolutionChainSurgeDamageBonus,
            EndlessSurgeDamageBonus);
        SurvivorsWeaponBonusValues ISurvivorsRunWeaponPort.CooldownBonuses => new SurvivorsWeaponBonusValues(
            WeaponCooldownMultiplierBonus,
            StreakSurgeCooldownMultiplierBonus,
            RoamingCacheSurgeCooldownMultiplierBonus,
            ArenaShrineSurgeCooldownMultiplierBonus,
            WaystoneFocusCooldownMultiplierBonus,
            WaystoneChainSurgeCooldownMultiplierBonus,
            HordeRushClearSurgeCooldownMultiplierBonus,
            WeaponLoadoutSurgeCooldownMultiplierBonus,
            PassiveLoadoutSurgeCooldownMultiplierBonus,
            BossRelicSurgeCooldownMultiplierBonus,
            GemRushCooldownMultiplierBonus,
            EvolutionChainSurgeCooldownMultiplierBonus,
            EndlessSurgeCooldownMultiplierBonus);

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
        private static string FormatClassStatSummary(SurvivorsClassDefinition definition)
            => SurvivorsProgressionChoiceLabels.ClassStats(definition);
        private static string FormatClassStartingWeaponSummary(SurvivorsClassDefinition definition)
            => SurvivorsProgressionChoiceLabels.ClassStartingWeapons(definition);
        private static string FormatPersistentUpgradeEffectLabel(SurvivorsPersistentUpgradeDefinition upgrade)
            => SurvivorsProgressionChoiceLabels.PersistentUpgradeEffect(upgrade);
        private string ResolveClassDisplayName(string classId, string fallback)
        {
            EnsureClassLibraryLoaded();
            return SurvivorsProgressionChoiceLabels.ClassDisplayName(_classLibrary, classId, fallback);
        }

        private void RecordResultClassSelectionFeedback(SurvivorsClassDefinition selected) => ProgressionFeedback.RecordResultClassSelectionFeedback(selected);

        private void RecordMetaUpgradePurchaseFeedback(string id) => ProgressionFeedback.RecordMetaUpgradePurchaseFeedback(id);

        private void RecordEvolutionGoalFeedback(RunUpgradeDefinition evolution, RunUpgradeDefinition missingPassive) => ProgressionFeedback.RecordEvolutionGoalFeedback(evolution, missingPassive);

        private void RecordEvolutionReadyFeedback(RunUpgradeDefinition evolution) => ProgressionFeedback.RecordEvolutionReadyFeedback(evolution);

        private void RecordClassUnlockRewardFeedback() => ProgressionFeedback.RecordClassUnlockRewardFeedback();

        private SurvivorsProgressionFeedback _progressionFeedback;
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

        public SurvivorsEnemyActor SpawnEnemyForTest(Vector3 position, float healthOverride = -1f) => DebugWorld.SpawnWithHealth(position, SurvivorsEnemyRole.Swarm, healthOverride);

        public SurvivorsEnemyActor SpawnEnemyForTest(Vector3 position, SurvivorsEnemyRole role, float healthOverride = -1f) => DebugWorld.SpawnWithHealth(position, role, healthOverride);

        public SurvivorsEnemyActor SpawnMinibossForTest(Vector3 position, float healthOverride = -1f) => DebugWorld.SpawnWithHealth(position, SurvivorsEnemyRole.Miniboss, healthOverride);

        public SurvivorsEnemyActor SpawnBossForTest(Vector3 position, float healthOverride = -1f) => DebugWorld.SpawnWithHealth(position, SurvivorsEnemyRole.Boss, healthOverride);

        public int DebugClearActiveHordeRush() => DebugWorld.ClearEncounter(SurvivorsDebugEncounter.Horde);

        public int KillActiveRoamingCacheAmbushEnemiesForTest() => DebugWorld.ClearEncounter(SurvivorsDebugEncounter.RoamingCache);

        public int KillActiveArenaShrineEnemiesForTest() => DebugWorld.ClearEncounter(SurvivorsDebugEncounter.Shrine);

        public int DebugSpawnEnemyBurst(SurvivorsEnemyRole role, int count, float radius) => DebugWorld.SpawnBurst(role, count, radius);

        public SurvivorsEnemyActor DebugSpawnMajorEnemy(SurvivorsEnemyRole role, float radius) => DebugWorld.SpawnMajor(role, radius);

        public SurvivorsEnemyActor DebugSpawnSprintBoss(float radius) => DebugWorld.SpawnSprintBoss(radius);

        public int DebugFillArenaToTarget(SurvivorsEnemyRole role, int targetAlive, float radius) => DebugWorld.FillArena(role, targetAlive, radius);

        public void DebugApplyStressProfile(int targetAlive) => DebugWorld.ApplyStress(targetAlive);

        private SurvivorsDebugWorldCommands _debugWorld;
        private SurvivorsDebugWorldCommands DebugWorld => _debugWorld ?? (_debugWorld = new SurvivorsDebugWorldCommands(this));
        void ISurvivorsDebugWorldPort.EnsureRunStarted() => EnsureRunStartedForTest();
        bool ISurvivorsDebugWorldPort.Started => _runSession.Started;
        SurvivorsPacingProfile ISurvivorsDebugWorldPort.PacingProfile => CurrentPacingProfile;
        void ISurvivorsDebugWorldPort.ApplyPacing(SurvivorsPacingProfile profile, bool restart) => ApplyPacingProfile(profile, restart);
        void ISurvivorsDebugWorldPort.StartRun() => StartRun();
        SurvivorsTemplateTuning ISurvivorsDebugWorldPort.Tuning => CurrentTuning;
        Vector3 ISurvivorsDebugWorldPort.PlayerPosition => PlayerPosition;
        Vector3 ISurvivorsDebugWorldPort.PlayerForward => PlayerForward;
        int ISurvivorsDebugWorldPort.ActiveEnemyCount => ActiveEnemyCount;
        SurvivorsEnemyActor ISurvivorsDebugWorldPort.SpawnEnemy(Vector3 position, SurvivorsEnemyRole role) => SpawnEnemy(position, explicitPosition: true, role);
        int ISurvivorsDebugWorldPort.ActiveMembers(SurvivorsDebugEncounter encounter) => encounter == SurvivorsDebugEncounter.Horde ? HordeRush.ActiveCount :
            encounter == SurvivorsDebugEncounter.RoamingCache ? RoamingCaches.ActiveCount : ShrineTrials.ActiveCount;
        IEnumerable<long> ISurvivorsDebugWorldPort.Members(SurvivorsDebugEncounter encounter) => encounter == SurvivorsDebugEncounter.Horde ? HordeRush.ActiveMembers :
            encounter == SurvivorsDebugEncounter.RoamingCache ? RoamingCaches.ActiveMembers : ShrineTrials.ActiveMembers;
        bool ISurvivorsDebugWorldPort.DamageMember(long id, float damage, string source)
        {
            SurvivorsEnemyActor enemy = _enemies.Find(candidate => candidate != null && candidate.InstanceId.Value == id);
            if (enemy == null || !enemy.IsAlive) return false;
            enemy.ApplyDamage(damage, source);
            return true;
        }

        private SurvivorsRunMetricsReadModel _runMetricsReadModel;
        private SurvivorsRunMetricsReadModel RunMetricsReadModel => _runMetricsReadModel ?? (_runMetricsReadModel = new SurvivorsRunMetricsReadModel(this));
        public IReadOnlyList<string> DebugDescribeRunMetrics() => RunMetricsReadModel.Describe();

        string ISurvivorsRunMetricsReadPort.ModeName => CurrentRunModeDisplayName;
        SurvivorsPacingProfile ISurvivorsRunMetricsReadPort.PacingProfile => CurrentPacingProfile;
        float ISurvivorsRunMetricsReadPort.TargetDuration => CurrentTuning.TargetDurationSeconds;
        float ISurvivorsRunMetricsReadPort.BossSpawnTime => CurrentTuning.BossSpawnTimeSeconds;
        float ISurvivorsRunMetricsReadPort.VictoryTime => CurrentTuning.SurvivalVictoryTimeSeconds;
        bool ISurvivorsRunMetricsReadPort.Started => _runSession.Started;
        bool ISurvivorsRunMetricsReadPort.ModeSelectionOpen => Menus.ModeSelectionOpen;
        ISurvivorsActiveRunMetricsReadPort ISurvivorsRunMetricsReadPort.Active => this;

        float ISurvivorsActiveRunMetricsReadPort.RunTimeSeconds => RunTimeSeconds;
        SurvivorsRunState ISurvivorsActiveRunMetricsReadPort.State => State;
        SurvivorsRunTelemetry ISurvivorsActiveRunMetricsReadPort.Telemetry => Telemetry;
        SurvivorsRunMetricsDraftValues ISurvivorsActiveRunMetricsReadPort.CaptureDrafts() => new SurvivorsRunMetricsDraftValues(
            LevelUpDraftOpenCount, DraftSession.OpenCount, PendingLevelUps,
            ActiveWeaponCount, MaxWeaponSlots, ActivePassiveCount, MaxPassiveSlots, EvolvedWeaponCount);
        SurvivorsRunMetricsCombatValues ISurvivorsActiveRunMetricsReadPort.CaptureCombat() => new SurvivorsRunMetricsCombatValues(
            KilledCount, ExperienceCollected, Experience, RequiredExperienceForNextLevel, ThrottledExperienceOverflow, PlayerVitals.DamageTaken);
        SurvivorsRunMetricsPickupValues ISurvivorsActiveRunMetricsReadPort.CapturePickups() => new SurvivorsRunMetricsPickupValues(
            CurrentPickupAttractRange, CurrentPickupAttractionSpeed, CurrentPickupMagnetPulseIntervalSeconds,
            ActiveOffscreenThreatMarkerCount, NormalEnemyRecycleCount, MajorThreatRepositionCount);

        internal void RecordOrbitHit() => _weaponDiagnostics.RecordOrbitHit();

        internal void RecordMeleeSwing() => _weaponDiagnostics.RecordMeleeSwing();

        internal void RecordMeleeHit() => _weaponDiagnostics.RecordMeleeHit();

        internal void RecordBurstPulse() => _weaponDiagnostics.RecordBurstPulse();

        internal void RecordBurstHit() => _weaponDiagnostics.RecordBurstHit();

        internal void RecordHitscanFire() => _weaponDiagnostics.RecordHitscanFire();

        internal void RecordHitscanHit() => _weaponDiagnostics.RecordHitscanHit();

        internal void RecordProjectilePierceHit() => _weaponDiagnostics.RecordProjectilePierceHit();

        internal void RecordProjectileChainHit() => _weaponDiagnostics.RecordProjectileChainHit();

        internal void RecordProjectileForkSpawn() => _weaponDiagnostics.RecordProjectileForkSpawn();

        internal void RecordProjectileReturnStart() => _weaponDiagnostics.RecordProjectileReturnStart();

        internal void RecordPayloadThrow() => _weaponDiagnostics.RecordPayloadThrow();

        internal void RecordPayloadPlaced() => _weaponDiagnostics.RecordPayloadPlaced();

        internal void RecordPayloadDetonation() => _weaponDiagnostics.RecordPayloadDetonation();

        internal void RecordPayloadExplosionHit() => _weaponDiagnostics.RecordPayloadExplosionHit();

        private readonly SurvivorsWeaponDiagnostics _weaponDiagnostics = new SurvivorsWeaponDiagnostics();
        private SurvivorsOrbitKnockback _orbitKnockback;
        private SurvivorsOrbitKnockback OrbitKnockback => _orbitKnockback ??
            (_orbitKnockback = new SurvivorsOrbitKnockback(this, _weaponDiagnostics));
        bool ISurvivorsOrbitKnockbackPort.CrimsonAegisActive
            => IsEvolutionActive(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId);
        float ISurvivorsOrbitKnockbackPort.OrbitKnockbackDistance => CurrentTuning.OrbitKnockbackDistance;
        float ISurvivorsOrbitKnockbackPort.CrimsonAegisOrbitKnockbackDistance => CurrentTuning.CrimsonAegisOrbitKnockbackDistance;
        Vector3 ISurvivorsOrbitKnockbackPort.PlayerPosition => PlayerPosition;
        Vector3 ISurvivorsOrbitKnockbackPort.PlayerForward => PlayerForward;
        internal bool ApplyOrbitKnockback(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition)
            => OrbitKnockback.Apply(enemy, definition);
        internal void RecordTempestPrismArcHit(SurvivorsEnemyActor source, SurvivorsEnemyActor target)
            => _weaponDiagnostics.RecordTempestPrismArcHit(
                source == null ? null : source.DisplayName,
                target == null ? null : target.DisplayName);

        private void AppendSelectedUpgradeRankLines(List<string> lines) => DebugUpgradeFormatter.AppendSelectedUpgradeRankLines(lines);

        private string FormatDebugRankLine(RunUpgradeDefinition definition, int rank) => DebugUpgradeFormatter.FormatDebugRankLine(definition, rank);

        private string FormatDebugUpgradeLine(int index, RunUpgradeDefinition definition) => DebugUpgradeFormatter.FormatDebugUpgradeLine(index, definition);

        private string FormatUpgradeChoiceLabel(int index, RunUpgradeDefinition choice) => DebugUpgradeFormatter.FormatUpgradeChoiceLabel(index, choice);

        private string FormatRelicChoiceLabel(int index, SurvivorsRelicDefinition relic) => DebugUpgradeFormatter.FormatRelicChoiceLabel(index, relic);

        public IReadOnlyList<string> DebugDescribeEligibleEvolutionPool() { EnsureRunStartedForTest(); return DebugDraftModel.DebugDescribeEligibleEvolutionPool(); }

        public IReadOnlyList<string> DebugDescribeCurrentDraftPool() { EnsureRunStartedForTest(); return DebugDraftModel.DebugDescribeCurrentDraftPool(); }

        private SurvivorsDebugUpgradeFormatter _debugUpgradeFormatter;
        private SurvivorsDebugUpgradeFormatter DebugUpgradeFormatter => _debugUpgradeFormatter ?? (_debugUpgradeFormatter = new SurvivorsDebugUpgradeFormatter(RunBuild, BuildContentLabels, DraftCards));
        private SurvivorsDebugDraftModel _debugDraftModel;
        private SurvivorsDebugDraftModel DebugDraftModel => _debugDraftModel ?? (_debugDraftModel = new SurvivorsDebugDraftModel(RunBuild, DraftOffers.Catalogs, DebugUpgradeFormatter, BuildContentLabels, this));
        RunUpgradeDraft ISurvivorsDebugDraftPort.CurrentDraft => DraftSession.CurrentDraft;
        SurvivorsRelicDraft ISurvivorsDebugDraftPort.CurrentRelicDraft => DraftSession.CurrentRelicDraft;
        string ISurvivorsDebugDraftPort.RewardOverlayTitle => ResolveRewardOverlayTitle();
        private SurvivorsDebugBuildModel _debugBuildModel;
        private SurvivorsDebugBuildModel DebugBuildModel => _debugBuildModel ?? (_debugBuildModel = new SurvivorsDebugBuildModel(DebugUpgradeFormatter, ResolveEvolutionObjectiveHudLabel));
        public IReadOnlyList<string> DebugDescribeCurrentBuild()
        { EnsureRunStartedForTest(); return DebugBuildModel.Describe(CaptureDebugBuildValues()); }
        private SurvivorsDebugBuildValues CaptureDebugBuildValues() => new SurvivorsDebugBuildValues(
            activeWeaponCount: ActiveWeaponCount,
            maxWeaponSlots: MaxWeaponSlots,
            activeWeaponList: FormatActiveWeaponList(),
            activePassiveCount: ActivePassiveCount,
            maxPassiveSlots: MaxPassiveSlots,
            evolvedWeaponCount: EvolvedWeaponCount,
            selectedRelicCount: SelectedRelicCount,
            totalRelicCount: ResolveTotalRelicCount(),
            selectedRelicList: FormatSelectedRelicList(),
            damageBonus: DamageBonus,
            surgeDamageBonus: StreakSurgeDamageBonus + RoamingCacheSurgeDamageBonus + ArenaShrineSurgeDamageBonus + WaystoneFocusDamageBonus + WaystoneChainSurgeDamageBonus + HordeRushClearSurgeDamageBonus + WeaponLoadoutSurgeDamageBonus + PassiveLoadoutSurgeDamageBonus + BossRelicSurgeDamageBonus + GemRushDamageBonus + EvolutionChainSurgeDamageBonus + EndlessSurgeDamageBonus,
            criticalChanceNormalized: CriticalChanceNormalized,
            criticalDamageMultiplier: CriticalDamageMultiplier,
            draftLuckBonus: DraftLuckBonus,
            weaponCooldownSeconds: WeaponCooldownSeconds,
            playerMoveSpeed: PlayerMoveSpeed,
            currentPickupAttractRange: CurrentPickupAttractRange,
            currentPickupAttractionSpeed: CurrentPickupAttractionSpeed,
            currentPickupMagnetPulseIntervalSeconds: CurrentPickupMagnetPulseIntervalSeconds,
            totalExperienceGainBonus: ExperienceGainMultiplierBonus + PassiveLoadoutSurgeExperienceGainMultiplierBonus,
            projectileFanBonus: ProjectileFanBonus,
            projectilePierceBonus: ProjectilePierceBonus,
            projectileChainBonus: ProjectileChainBonus,
            projectileForkBonus: ProjectileForkBonus,
            projectileReturnBonus: ProjectileReturnBonus,
            areaRadiusBonus: AreaRadiusBonus,
            orbitRadiusBonus: OrbitRadiusBonus,
            burstCountBonus: BurstCountBonus,
            burstEchoBonus: BurstEchoBonus,
            payloadCountBonus: PayloadCountBonus,
            deathNovaDamage: DeathNovaDamage,
            deathNovaRadius: DeathNovaRadius,
            poisonDamageRatio: PoisonDamageRatio,
            bleedDamageRatio: BleedDamageRatio,
            executeThresholdNormalized: ExecuteThresholdNormalized,
            lifestealRatio: LifestealRatio);

        SurvivorsEncounterClears ISurvivorsEnemyDefeatPort.ReleaseKilledEnemy(ISurvivorsDefeatTarget target) => ActorMembership.ReleaseKilledEnemy((SurvivorsEnemyActor)target);

        internal void ReleaseEnemy(SurvivorsEnemyActor enemy, DespawnReason reason) => ActorMembership.ReleaseEnemy(enemy, reason);

        internal void ReleaseProjectile(SurvivorsProjectileActor projectile, DespawnReason reason) => ActorMembership.ReleaseProjectile(projectile, reason);

        private int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) => AreaDamage.DamageNonMajorEnemies(position, radius, damage, source);

        private SurvivorsActorMembership _actorMembership;
        private SurvivorsActorMembership ActorMembership => _actorMembership ?? (_actorMembership = new SurvivorsActorMembership(this));
        private SurvivorsAreaDamage _areaDamage;
        private SurvivorsAreaDamage AreaDamage => _areaDamage ?? (_areaDamage = new SurvivorsAreaDamage(EnemySpatialQueries, (enemy, damage, source) => enemy.ApplyDamage(damage, source)));
        bool ISurvivorsActorMembershipPort.RemoveHordeMember(long id) => HordeRush.RemoveEnemy(id);
        bool ISurvivorsActorMembershipPort.RemoveCacheMember(long id) => RoamingCaches.RemoveEnemy(id);
        bool ISurvivorsActorMembershipPort.RemoveShrineMember(long id) => ShrineTrials.RemoveEnemy(id);
        void ISurvivorsActorMembershipPort.ForgetMajorThreat(SurvivorsEnemyActor enemy) => MajorThreatAbilities.ForgetEnemy(enemy);
        bool ISurvivorsActorMembershipPort.HasSpawnService => _spawnService != null;
        void ISurvivorsActorMembershipPort.Despawn(SpawnInstanceId id, DespawnReason reason) => _spawnService.Despawn(id, reason);

        private const string AudioEventUiHover = "ui.hover";
        private const string AudioEventUiSelect = "ui.select";
        private const string AudioEventModeSelected = "mode.selected";
        private const string AudioEventDraftOpened = "draft.opened";
        private const string AudioEventDraftChoiceSelected = "draft.choice.selected";
        private const string AudioEventDraftReroll = "draft.reroll";
        private const string AudioEventDraftBanish = "draft.banish";
        private const string AudioEventDraftSkip = "draft.skip";
        private const string AudioEventLevelUp = "level.up";
        private const string AudioEventXpPickup = "pickup.xp";
        private const string AudioEventMagnetPulse = "pickup.magnet_pulse";
        private const string AudioEventCombatHit = "combat.hit";
        private const string AudioEventEnemyDeath = "combat.enemy_death";
        private const string AudioEventEliteWarning = "warning.elite";
        private const string AudioEventBossWarning = "warning.boss";
        private const string AudioEventLowHealthWarning = "warning.low_health";
        private const string AudioEventEvolution = "reward.evolution";
        private const string AudioEventRelic = "reward.relic";
        private const string AudioEventVictory = "run.victory";
        private const string AudioEventDefeat = "run.defeat";
        private const string AudioEventRunSummaryOpened = "run.summary.opened";
        private const float LowHealthWarningThreshold = 0.3f;
        private const float RewardFeedbackDurationSeconds = 2.35f;
        private const float StreakRewardFeedbackDurationSeconds = 1.8f;
        private const float EnemyHitFlashSeconds = 0.13f;
        private const float BaseDeathNovaRadius = 1.65f;
        private const int ResultMetaUpgradeOptionCount = 3;
        private const int ResultClassOptionCount = 4;

        private static readonly RunUpgradeDefinition[] EmptyChoices = Array.Empty<RunUpgradeDefinition>();
        private static readonly SurvivorsRelicDefinition[] EmptyRelicChoices = Array.Empty<SurvivorsRelicDefinition>();

        [SerializeField]
        private bool autoStart = false;

        [SerializeField]
        private bool showRunModeSelection = true;

        [SerializeField]
        private bool buildVisualsOnAwake = true;

        [SerializeField]
        private SurvivorsPacingProfile pacingProfile = SurvivorsPacingProfile.HumanPlaytest;

        [SerializeField]
        private SurvivorsTemplateTuning tuning;

        [SerializeField]
        private SurvivorsUiTheme uiTheme;

        private List<SurvivorsEnemyActor> _enemies => ActorMembership.Enemies;
        private List<SurvivorsPickupActor> _pickups => ActorMembership.Pickups;
        private List<SurvivorsProjectileActor> _projectiles => ActorMembership.Projectiles;
        private readonly SurvivorsFeedbackBannerPresenter _rewardBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Reward);
        private readonly SurvivorsFeedbackBannerPresenter _streakRewardBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Streak);
        private readonly SurvivorsFeedbackBannerPresenter _classUnlockRewardBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.ClassUnlock);
        private readonly SurvivorsFeedbackBannerPresenter _evolutionReadyBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Evolution);
        private SurvivorsCombatFeedbackPresenter _combatFeedback;
        private SurvivorsCombatFeedbackPresenter CombatFeedback => _combatFeedback ?? (_combatFeedback = new SurvivorsCombatFeedbackPresenter(() => _feedbackRoot));
        private SurvivorsThreatTelegraphPresenter _threatTelegraphs;
        private SurvivorsThreatTelegraphPresenter ThreatTelegraphs => _threatTelegraphs ?? (_threatTelegraphs = new SurvivorsThreatTelegraphPresenter(() => _feedbackRoot));
        private SurvivorsRewardDropPresenter _rewardDrops;
        private SurvivorsRewardDropPresenter RewardDrops => _rewardDrops ?? (_rewardDrops = new SurvivorsRewardDropPresenter(() => _feedbackRoot, () => ActiveUiTheme));
        private readonly SurvivorsDamagePopupPresenter _damageFeedback = new SurvivorsDamagePopupPresenter();
        private SurvivorsBuildSurgeRewards _buildSurges;
        private SurvivorsBuildSurgeRewards BuildSurges => _buildSurges ?? (_buildSurges = new SurvivorsBuildSurgeRewards(RunBuild, this));
        private void TriggerWeaponEvolutionSurge(RunUpgradeDefinition upgrade) => BuildSurges.TriggerWeaponEvolutionSurge(upgrade);
        private void TriggerEvolutionChainSurge(RunUpgradeDefinition upgrade) => BuildSurges.TriggerEvolutionChainSurge(upgrade);
        private void TryTriggerWeaponLoadoutSurge(SurvivorsWeaponArchetypeDefinition weapon) => BuildSurges.TryTriggerWeaponLoadoutSurge(weapon);
        private void TryTriggerPassiveLoadoutSurge(RunUpgradeDefinition passive) => BuildSurges.TryTriggerPassiveLoadoutSurge(passive);
        private void TriggerBossRelicSurge(SurvivorsRelicDefinition relic) => BuildSurges.TriggerBossRelicSurge(relic);
        private void TickWeaponLoadoutSurge(float deltaTime) => BuildSurges.TickWeaponLoadoutSurge(deltaTime);
        private void TickPassiveLoadoutSurge(float deltaTime) => BuildSurges.TickPassiveLoadoutSurge(deltaTime);
        private void TickBossRelicSurge(float deltaTime) => BuildSurges.TickBossRelicSurge(deltaTime);
        private void TickEvolutionChainSurge(float deltaTime) => BuildSurges.TickEvolutionChainSurge(deltaTime);
        SurvivorsTemplateTuning ISurvivorsBuildSurgePort.Tuning => CurrentTuning;
        Vector3 ISurvivorsBuildSurgePort.PlayerPosition => PlayerPosition;
        int ISurvivorsBuildSurgePort.WeaponCount => ActiveWeaponCount;
        int ISurvivorsBuildSurgePort.DamageNonMajor(Vector3 position, float radius, float damage, string source) => DamageNonMajorEnemies(position, radius, damage, source);
        int ISurvivorsBuildSurgePort.RecallGems() => StartMagnetRecall();
        Color ISurvivorsBuildSurgePort.RelicAccent(SurvivorsRelicDefinition relic) => ResolveRelicAccentColor(relic);
        void ISurvivorsBuildSurgePort.ShowFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);
        void ISurvivorsBuildSurgePort.PlayPulse(int count, bool boss) => PlayFeedback(boss ? _bossPulse : _levelUpPulse, PlayerPosition, count, boss ? _bossClip : _levelUpClip);

        private SurvivorsDraftCardFactory _draftCardFactory;
        private SurvivorsDraftCardFactory DraftCards => _draftCardFactory ?? (_draftCardFactory = new SurvivorsDraftCardFactory(RunBuild, ShortWeaponName));

        private SurvivorsDraftCard CreateUpgradeDraftCard(int index, RunUpgradeDefinition choice) =>
            DraftCards.CreateUpgradeCard(index, choice, ActiveUiTheme, choice == null ? default : CaptureDraftPreviewValues());
        private SurvivorsDraftCard CreateRelicDraftCard(int index, SurvivorsRelicDefinition relic) =>
            DraftCards.CreateRelicCard(index, relic, ActiveUiTheme);

        private SurvivorsDraftPreviewValues CaptureDraftPreviewValues() => new SurvivorsDraftPreviewValues
        {
            ProjectileDamage = ProjectileDamage,
            WeaponCooldownSeconds = WeaponCooldownSeconds,
            PlayerMoveSpeed = PlayerMoveSpeed,
            CurrentPickupAttractRange = CurrentPickupAttractRange,
            CurrentPickupAttractionSpeed = CurrentPickupAttractionSpeed,
            CurrentPickupMagnetPulseIntervalSeconds = CurrentPickupMagnetPulseIntervalSeconds,
            MaxHealth = MaxHealth,
            OrbitRadiusBonus = OrbitRadiusBonus,
            PayloadExplosionRadiusBonus = PayloadExplosionRadiusBonus,
            PayloadTriggerRadiusBonus = PayloadTriggerRadiusBonus,
            PoisonDamageRatio = PoisonDamageRatio,
            BleedDamageRatio = BleedDamageRatio,
            ExecuteThresholdNormalized = ExecuteThresholdNormalized,
            CriticalChanceNormalized = CriticalChanceNormalized,
            CriticalDamageMultiplier = CriticalDamageMultiplier,
            DraftLuckBonus = DraftLuckBonus,
            DeathNovaDamage = DeathNovaDamage,
            DeathNovaRadius = DeathNovaRadius,
            LifestealRatio = LifestealRatio,
            BarrierCapacity = BarrierCapacity,
            BarrierRegenPerSecondBonus = BarrierRegenPerSecondBonus,
            BarrierOnDamageRatio = BarrierOnDamageRatio,
            ExperienceGainMultiplierBonus = ExperienceGainMultiplierBonus,
            AreaRadiusBonus = AreaRadiusBonus,
            OrbitBladeBonus = OrbitBladeBonus,
            MeleeTargetBonus = MeleeTargetBonus,
            BurstCountBonus = BurstCountBonus,
            BurstEchoBonus = BurstEchoBonus,
            TargetedBurstSigilBonus = TargetedBurstSigilBonus,
            ProjectileFanBonus = ProjectileFanBonus,
            ProjectilePierceBonus = ProjectilePierceBonus,
            ProjectileChainBonus = ProjectileChainBonus,
            ProjectileForkBonus = ProjectileForkBonus,
            ProjectileReturnBonus = ProjectileReturnBonus,
            HitscanPierceBonus = HitscanPierceBonus,
            PayloadCountBonus = PayloadCountBonus,
            PickupMagnetPulseBaseIntervalSeconds = CurrentTuning.PickupMagnetPulseBaseIntervalSeconds,
            PickupMagnetPulseMinimumIntervalSeconds = CurrentTuning.PickupMagnetPulseMinimumIntervalSeconds,
        };

        private string ResolveUpgradeAffectedLabel(RunUpgradeDefinition choice) => DraftCards.ResolveUpgradeAffectedLabel(choice);
        private string FormatRelicEffectSummary(SurvivorsRelicDefinition relic) => DraftCards.FormatRelicEffectSummary(relic);
        private static string FormatUpgradeCategoryLabel(SurvivorsRunUpgradeCategory category) => SurvivorsDraftCardFactory.FormatUpgradeCategoryLabel(category);
        private static Color ResolveRarityAccentColor(RunUpgradeRarity rarity) => SurvivorsDraftCardFactory.ResolveRarityAccentColor(rarity);
        private static Color ResolveRelicAccentColor(SurvivorsRelicDefinition relic) => SurvivorsDraftCardFactory.ResolveRelicAccentColor(relic);

        private SurvivorsEnemySpatialQueries _enemySpatialQueries;
        private SurvivorsEnemySpatialQueries EnemySpatialQueries => _enemySpatialQueries ?? (_enemySpatialQueries = new SurvivorsEnemySpatialQueries(_enemies, () => CurrentTuning));
        private SurvivorsEnemyNavigation _enemyNavigation;
        private SurvivorsEnemyNavigation EnemyNavigation => _enemyNavigation ?? (_enemyNavigation = new SurvivorsEnemyNavigation(this));
        internal SurvivorsEnemyActor FindNearestEnemy(Vector3 origin, float range) => EnemySpatialQueries.FindNearestEnemy(origin, range);
        internal void CollectEnemiesWithinRadius(Vector3 origin, float radius, List<SurvivorsEnemyActor> results) => EnemySpatialQueries.CollectEnemiesWithinRadius(origin, radius, results);
        private void TryUpdateEnemyLeash(SurvivorsEnemyActor enemy, float deltaTime) => EnemyNavigation.TryUpdateEnemyLeash(enemy, deltaTime);
        internal float ResolveEnemyCatchUpMoveSpeedMultiplier(SurvivorsEnemyActor enemy, float distance) => EnemyNavigation.ResolveEnemyCatchUpMoveSpeedMultiplier(enemy, distance);
        internal Vector3 ResolveEnemyCrowdSeparation(SurvivorsEnemyActor actor) => EnemySpatialQueries.ResolveEnemyCrowdSeparation(actor);
        SurvivorsTemplateTuning ISurvivorsEnemyNavigationPort.Tuning => CurrentTuning;
        Vector3 ISurvivorsEnemyNavigationPort.PlayerPosition => PlayerPosition;
        bool ISurvivorsEnemyNavigationPort.IsMajorRewardRole(SurvivorsEnemyRole role) => IsMajorRewardRole(role);
        Vector3 ISurvivorsEnemyNavigationPort.ResolveSafeOffscreenPosition(Vector3 center, float minimumDistance, float maximumDistance, long seed, float padding, float bandDepth) =>
            ResolveSafeOffscreenPosition(center, minimumDistance, maximumDistance, seed, padding, bandDepth);
        float ISurvivorsEnemyNavigationPort.ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string reason) => ResolveOffscreenSpawnPadding(role, reason);
        void ISurvivorsEnemyNavigationPort.RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string reason) => RecordGameplaySpawnSafety(role, position, reason);
        void ISurvivorsEnemyNavigationPort.RecordMajorThreatReentry(SurvivorsEnemyRole role, string displayName, Vector3 playerToEnemy)
        {
            string name = string.IsNullOrWhiteSpace(displayName) ? ResolveMajorThreatHealthFallbackLabel(role) : displayName;
            ThreatHud.RecordMarkerLabel($"{name} re-entering {ResolveCompassDirectionLabel(playerToEnemy)}");
        }

        private SurvivorsKillStreakRewards _killStreakRewards;
        private SurvivorsKillStreakRewards KillStreakRewards => _killStreakRewards ?? (_killStreakRewards = new SurvivorsKillStreakRewards(this));
        private SurvivorsExperienceComboRewards _experienceRhythm;
        private SurvivorsExperienceComboRewards ExperienceRhythm => _experienceRhythm ?? (_experienceRhythm = new SurvivorsExperienceComboRewards(() => CurrentTuning));
        private void RegisterKillStreak(Vector3 position) => KillStreakRewards.RegisterKillStreak(position);
        private void TickKillStreak(float deltaTime) => KillStreakRewards.TickKillStreak(deltaTime);
        private void TickStreakSurge(float deltaTime) => KillStreakRewards.TickStreakSurge(deltaTime);
        private void RecordExperienceCombo(int gained) => ExperienceRhythm.RecordExperienceCombo(gained);
        private void TickGemRush(float deltaTime) => ExperienceRhythm.TickGemRush(deltaTime);
        private void TickExperienceComboFeedback(float deltaTime) => ExperienceRhythm.TickExperienceComboFeedback(deltaTime);
        SurvivorsTemplateTuning ISurvivorsStreakRewardPort.Tuning => CurrentTuning;
        string ISurvivorsStreakRewardPort.CurrencyLabel => CurrencyDisplayName;
        bool ISurvivorsStreakRewardPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount) != null;
        bool ISurvivorsStreakRewardPort.TryDropHealth(Vector3 position) => TryDropHealthPickup(position);
        void ISurvivorsStreakRewardPort.ShowFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        private SurvivorsRunResultPresenter _resultScreen;
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

        private SurvivorsDraftScreenPresenter _draftScreen;
        private SurvivorsDraftScreenPresenter DraftScreen => _draftScreen ?? (_draftScreen = new SurvivorsDraftScreenPresenter(
            DraftSession, () => ActiveUiTheme, _hudStyles,
            index => IsRelicChoiceOpen ? CreateRelicDraftCard(index, CurrentRelicChoices[index]) : CreateUpgradeDraftCard(index, CurrentDraftChoices[index]),
            ResolveRewardOverlayTitle, ResolveRewardTitleAccentColor, () => DraftSkipBloodShards));
        private void DrawLevelUpOverlay() => DraftScreen.Draw();

        private SurvivorsRunModePresenter _runModePresenter;
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
        void ISurvivorsRunModePort.Start(SurvivorsPacingProfile profile) => SelectRunMode(profile);
        void ISurvivorsRunModePort.OpenTutorial() => OpenTutorialOverlay(markUnseen: false);
        void ISurvivorsRunModePort.EnsureThemes() => EnsureUiTheme();
        void ISurvivorsRunModePort.SelectTheme(int index) => SelectUiTheme(index);
        void ISurvivorsRunModePort.Hover() => PlayAudioEvent(AudioEventUiHover, _pickupClip, 0.08f);
        void ISurvivorsRunModePort.Select() => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);

        private SurvivorsMenuSession _menus;
        private SurvivorsMenuSession Menus => _menus ?? (_menus = new SurvivorsMenuSession(this));
        private SurvivorsBuildMenuPresenter _buildMenuPresenter;
        private SurvivorsBuildMenuPresenter BuildMenuPresenter => _buildMenuPresenter ?? (_buildMenuPresenter = new SurvivorsBuildMenuPresenter(
            Menus, () => ActiveUiTheme, _hudStyles, ResolveBuildMenuLines, () => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f)));
        private SurvivorsTutorialPresenter _tutorialPresenter;
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

        private SurvivorsRelicInventory _relicInventory;
        private SurvivorsRelicInventory RelicInventory => _relicInventory ?? (_relicInventory = new SurvivorsRelicInventory(
            () => { EnsureClassLibraryLoaded(); return _relicDefinitions; }, ApplyRelic,
            selected => { RecordRelicSelectionFeedback(selected); TriggerBossRelicSurge(selected); }));
        private SurvivorsDraftSession _draftSession;
        private SurvivorsDraftSession DraftSession => _draftSession ?? (_draftSession = new SurvivorsDraftSession(
            RunBuild, DraftOffers, RelicInventory, _runSession, _experienceProgression, this));
        public bool ApplyUpgradeByIdForTest(string id) { EnsureRunStartedForTest(); return DraftSession.ApplyById(id); }
        public bool SelectUpgrade(int index) => DraftSession.Select(index);
        public bool RerollCurrentDraft() => DraftSession.Reroll();
        public bool SkipCurrentDraft() => DraftSession.Skip();
        public bool BanishDraftChoice(int index) => DraftSession.Banish(index);
        private bool CanRerollCurrentDraft() => DraftSession.CanReroll;
        private bool CanSkipCurrentDraft() => DraftSession.CanSkip;
        private bool CanBanishCurrentDraft() => DraftSession.CanBanish;
        private void ClearRewardDrafts() => DraftSession.Clear();
        private bool TryOpenPendingLevelUpDraft() => DraftSession.TryOpenPending();
        private void OpenLevelUpDraft(IReadOnlyList<RunUpgradeId> lockedChoices = null) => DraftSession.OpenLevelUp(lockedChoices);
        private bool OpenUpgradeRewardDraft(SurvivorsEnemyRole role, bool requireEvolutionChoice) => DraftSession.OpenReward(role, requireEvolutionChoice);
        private bool OpenBossRelicDraft() => DraftSession.OpenRelic();
        private void TickRewardSelectionTimeout(float deltaTime) => DraftSession.TickTimeout(deltaTime);
        private void AutoSelectRewardChoice() => DraftSession.AutoSelect();
        private bool SelectRelic(int index) => DraftSession.SelectRelic(index);
        SurvivorsTemplateTuning ISurvivorsDraftSessionPort.Tuning => CurrentTuning;
        int ISurvivorsDraftSessionPort.RerollCharges => TotalDraftRerollCharges;
        int ISurvivorsDraftSessionPort.RelicSeed => CurrentTuning.RunSeed + MinibossKilledCount + SelectedRelicCount + 97;
        void ISurvivorsDraftSessionPort.ResetScroll() => DraftScreen.ResetScroll();
        void ISurvivorsDraftSessionPort.ApplyUpgrade(RunUpgradeDefinition upgrade) => ApplyUpgrade(upgrade);
        void ISurvivorsDraftSessionPort.RecordDirectUpgrade(RunUpgradeDefinition upgrade) => RecordBestRewardMoment(upgrade);
        void ISurvivorsDraftSessionPort.PresentSelectedUpgrade(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition selected)
        {
            RecordRewardSelectionFeedback(kind, selected);
            PlayAudioEvent(AudioEventDraftChoiceSelected, _levelUpClip, 0.06f);
            if (IsEvolutionUpgrade(selected))
            {
                PlayAudioEvent(AudioEventEvolution, _levelUpClip, 0.08f);
            }
            if (kind == SurvivorsRewardSelectionKind.LevelUp && !IsEvolutionUpgrade(selected))
            {
                TriggerLevelUpPulse(selected);
            }

            if (IsRewardUpgradeSelectionKind(kind) && !IsEvolutionUpgrade(selected))
            {
                TriggerRewardJackpot(selected, kind);
                TriggerRewardUpgradeSurge(selected, kind);
            }

        }
        void ISurvivorsDraftSessionPort.PresentDraft(SurvivorsRewardSelectionKind kind, RunUpgradeDraft upgradeDraft, SurvivorsRelicDraft relicDraft, bool opening)
        {
            if (opening && kind == SurvivorsRewardSelectionKind.LevelUp) Telemetry.Record(SurvivorsRunMetric.FirstLevelUpDraft, RunTimeSeconds);
            if (relicDraft != null) RecordRewardCardPresentation(relicDraft);
            else RecordRewardCardPresentation(kind, upgradeDraft);
        }
        void ISurvivorsDraftSessionPort.PlayDraftOpened(SurvivorsRewardSelectionKind kind, SurvivorsEnemyRole role)
        {
            if (kind == SurvivorsRewardSelectionKind.LevelUp)
            {
                PlayAudioEvent(AudioEventLevelUp, _levelUpClip, 0.12f);
                PlayFeedback(_levelUpPulse, PlayerPosition, 34, _levelUpClip, AudioEventDraftOpened, 0.1f);
            }
            else PlayFeedback(_bossPulse, PlayerPosition, kind == SurvivorsRewardSelectionKind.BossRelic ? 44 : role == SurvivorsEnemyRole.Boss ? 72 : 54, _bossClip, AudioEventDraftOpened, 0.1f);
        }
        void ISurvivorsDraftSessionPort.PlayReroll() => PlayFeedback(_levelUpPulse, PlayerPosition, 18, _levelUpClip, AudioEventDraftReroll, 0.08f);
        void ISurvivorsDraftSessionPort.PlayBanish() => PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip, AudioEventDraftBanish, 0.08f);
        void ISurvivorsDraftSessionPort.GrantSkipReward(SurvivorsRewardSelectionKind kind)
        {
            RunRewards.AddBloodShards(DraftSkipBloodShards);
            RecordRewardSkipFeedback(kind);
        }
        void ISurvivorsDraftSessionPort.EnterVictory() => EnterVictory();

        private SurvivorsDraftOfferGenerator _draftOffers;
        private SurvivorsDraftOfferGenerator DraftOffers => _draftOffers ?? (_draftOffers = new SurvivorsDraftOfferGenerator(
            RunBuild, DraftRarity, () => CurrentTuning,
            () => new SurvivorsDraftProgress(CurrentTuning.RunSeed, Level, SelectedUpgradeCount, MinibossKilledCount, BossKilledCount)));
        private RunUpgradeCatalog CreateEligibleDraftCatalog() => DraftOffers.Catalogs.CreateEligibleDraftCatalog(DraftRarity.ResolveNormalDraftRarityProfile(Level));
        private bool IsEvolutionUpgrade(RunUpgradeDefinition definition) => DraftOffers.Catalogs.IsEvolutionUpgrade(definition);
        private bool TryResolveEvolutionMissingPassive(RunUpgradeDefinition evolution, out RunUpgradeDefinition passive) => DraftOffers.Catalogs.TryResolveEvolutionMissingPassive(evolution, out passive);
        private int CountEvolutionChoices(IReadOnlyList<RunUpgradeDefinition> definitions) => DraftOffers.Catalogs.CountEvolutionChoices(definitions);
        private bool DraftContainsEvolution(RunUpgradeDraft draft) => DraftOffers.Catalogs.DraftContainsEvolution(draft);
        private IReadOnlyList<RunUpgradeId> CreateEligibleEvolutionChoiceLocks(int maxCount) => DraftOffers.Guarantees.CreateEligibleEvolutionChoiceLocks(maxCount);
        private bool TryGenerateCurrentUpgradeDraft(SurvivorsRewardSelectionKind kind, int rerollIndex, IReadOnlyList<RunUpgradeId> lockedChoices, out RunUpgradeDraft draft) => DraftOffers.TryGenerate(kind, rerollIndex, lockedChoices, out draft);

        private SurvivorsRunBuildState _runBuild;
        private SurvivorsRunBuildState RunBuild => _runBuild ?? (_runBuild = new SurvivorsRunBuildState(this));
        private SurvivorsDraftRarityPolicy _draftRarity;
        private SurvivorsDraftRarityPolicy DraftRarity => _draftRarity ?? (_draftRarity = new SurvivorsDraftRarityPolicy(() => CurrentTuning, () => DraftLuckBonus));
        private bool TryGetRunUpgrade(string id, out RunUpgradeDefinition definition) => RunBuild.TryGetRunUpgrade(id, out definition);
        private bool TryGetUpgradeMetadata(string id, out SurvivorsRunUpgradeMetadata metadata) => RunBuild.TryGetUpgradeMetadata(id, out metadata);
        private string ResolveUpgradeDisplayName(RunUpgradeId id) => RunBuild.ResolveUpgradeDisplayName(id);
        private int ResolveRequiredUpgradeRank(SurvivorsRunUpgradeMetadata metadata) => RunBuild.ResolveRequiredUpgradeRank(metadata);
        private bool IsUpgradeEligibleForCurrentBuild(RunUpgradeDefinition upgrade) => RunBuild.IsUpgradeEligibleForCurrentBuild(upgrade);
        private SurvivorsRunUpgradeCategory ResolveCurrentUpgradeCategory(RunUpgradeDefinition upgrade) => RunBuild.ResolveCurrentUpgradeCategory(upgrade);
        private void RecordRunBuildSelection(RunUpgradeDefinition upgrade) => RunBuild.RecordRunBuildSelection(upgrade);
        SurvivorsTemplateTuning ISurvivorsRunBuildPort.Tuning => CurrentTuning;
        int ISurvivorsRunBuildPort.WeaponCount => ActiveWeaponCount;
        bool ISurvivorsRunBuildPort.HasWeapon(string id) => RunWeapons.ContainsWeapon(id);
        void ISurvivorsRunBuildPort.AddWeapon(string id) => TryAddWeaponToLoadout(id);
        void ISurvivorsRunBuildPort.PassiveAdded(RunUpgradeDefinition upgrade) => TryTriggerPassiveLoadoutSurge(upgrade);
        void ISurvivorsRunBuildPort.RecordEvolutionTime() => Telemetry.Record(SurvivorsRunMetric.FirstEvolutionAcquired, RunTimeSeconds);
        void ISurvivorsRunBuildPort.EvolutionAdded(RunUpgradeDefinition upgrade)
        {
            TriggerWeaponEvolutionSurge(upgrade);
            TriggerEvolutionChainSurge(upgrade);
        }

        private SurvivorsPlayerVitals _playerVitals;
        private SurvivorsPlayerVitals PlayerVitals => _playerVitals ?? (_playerVitals = new SurvivorsPlayerVitals(this));
        private SurvivorsPlayerMotion _playerMotion;
        private SurvivorsPlayerMotion PlayerMotion => _playerMotion ?? (_playerMotion = new SurvivorsPlayerMotion(this));
        public void ApplyDamageToPlayer(float amount, string source) => PlayerVitals.ApplyDamageToPlayer(amount, source);
        SurvivorsTemplateTuning ISurvivorsPlayerDamagePort.Tuning => CurrentTuning;
        int ISurvivorsPlayerDamagePort.DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) =>
            DamageNonMajorEnemies(position, radius, damage, source);
        void ISurvivorsPlayerDamagePort.ShowBlockedDamage(bool invulnerable)
        {
            if (invulnerable) PlayFeedback(_pickupPulse, PlayerPosition, 4, null);
            else PlayFeedback(_bossPulse, PlayerPosition, 8, _dangerClip);
        }
        void ISurvivorsPlayerDamagePort.RecordDamage(DamageResult damage, Vector3 position) => RecordPlayerDamageFeedback(damage, position);
        void ISurvivorsPlayerDamagePort.Defeat()
        {
            GrantRunRewards(victory: false);
            _runSession.Defeat();
            ClearRewardDrafts();
            PlayFeedback(_bossPulse, PlayerPosition, 34, _dangerClip, AudioEventDefeat, 0.5f);
        }
        void ISurvivorsPlayerDamagePort.ShowClutch(string label, int hitCount)
        {
            RecordStreakRewardFeedback(label, new Color(1f, 0.32f, 0.42f));
            PlayFeedback(_bossPulse, PlayerPosition, Mathf.Clamp(24 + hitCount * 5, 30, 72), _dangerClip, AudioEventLowHealthWarning, 0.5f);
        }
        void ISurvivorsPlayerDamagePort.ShowHurt() => PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip);
        SurvivorsTemplateTuning ISurvivorsPlayerMotionPort.Tuning => CurrentTuning;
        bool ISurvivorsPlayerMotionPort.HasPlayer => _playerObject != null;
        Vector3 ISurvivorsPlayerMotionPort.Position { get => PlayerPosition; set => _playerObject.transform.position = value; }
        Vector3 ISurvivorsPlayerMotionPort.Forward { get => PlayerForward; set => _playerObject.transform.forward = value; }
        float ISurvivorsPlayerMotionPort.MoveSpeed => PlayerMoveSpeed;
        void ISurvivorsPlayerMotionPort.RecordTravel(Vector3 delta) => RecordRoamingArenaTravel(delta);
        void ISurvivorsPlayerMotionPort.ExtendSafety(float seconds) => PlayerVitals.ExtendSafety(seconds);
        int ISurvivorsPlayerMotionPort.ApplyDashPressure(Vector3 start, Vector3 end, Vector3 direction, Action onDamageHit) =>
            SurvivorsDashPressure.Apply(CurrentTuning, _enemies, start, end, direction, onDamageHit);
        void ISurvivorsPlayerMotionPort.ShowDash(Vector3 position, string label, int shoved)
        {
            RecordStreakRewardFeedback(label, new Color(0.54f, 0.84f, 1f));
            PlayFeedback(_pickupPulse, position, Mathf.Clamp(18 + shoved * 4, 18, 54), _pickupClip);
        }

        private SurvivorsArenaPresenter _arena;
        private SurvivorsArenaPresenter Arena => _arena ?? (_arena = new SurvivorsArenaPresenter(() => ActiveUiTheme, key => Waystones.IsDiscovered(key)));
        private bool TryResolveClosestArenaLandmark(bool ignoreDiscovered, out Vector3 closest, out float distance, out Vector3 delta) =>
            Arena.TryResolveClosestArenaLandmark(PlayerPosition, ignoreDiscovered, out closest, out distance, out delta);
        private void UpdateArenaPresentation()
        {
            if (_playerObject != null) Arena.Update(PlayerPosition, CurrentTuning.WaystoneDiscoveryRadius, RunTimeSeconds);
        }
        private void TickArenaWaystoneDiscoveries()
        {
            for (int i = 0; i < Arena.LandmarkCount; i++)
            {
                if (Arena.TryGetLandmark(i, out long key, out Vector3 position)) Waystones.TryDiscover(key, position);
            }
        }

        private SurvivorsRoamingCacheEncounter _roamingCaches;
        private SurvivorsRoamingCacheEncounter RoamingCaches => _roamingCaches ?? (_roamingCaches = new SurvivorsRoamingCacheEncounter(this));
        private SurvivorsShrineEncounter _shrineTrials;
        private SurvivorsShrineEncounter ShrineTrials => _shrineTrials ?? (_shrineTrials = new SurvivorsShrineEncounter(this));
        private SurvivorsWaystoneExploration _waystones;
        private SurvivorsWaystoneExploration Waystones => _waystones ?? (_waystones = new SurvivorsWaystoneExploration(this));
        private SurvivorsExplorationBonuses ExplorationBonuses => new SurvivorsExplorationBonuses(_runSession.HasClearedVictory, EndlessSurgeTier);
        private SurvivorsExplorationFeedback _explorationFeedback;
        private SurvivorsExplorationFeedback ExplorationFeedback => _explorationFeedback ?? (_explorationFeedback = new SurvivorsExplorationFeedback(
            RecordStreakRewardFeedback,
            (position, particles, danger) => PlayFeedback(danger ? _bossPulse : _levelUpPulse, position, particles, danger ? _dangerClip : _pickupClip)));
        SurvivorsTemplateTuning ISurvivorsExplorationPort.Tuning => CurrentTuning;
        int ISurvivorsExplorationPort.Escalation => RunEscalationLevel;
        int ISurvivorsExplorationPort.MaximumAlive => ResolveEnemyMaximumAlive();
        long ISurvivorsExplorationPort.SpawnSequence => _spawnSequence;
        SurvivorsExplorationBonuses ISurvivorsExplorationPort.Endless => ExplorationBonuses;
        string ISurvivorsExplorationPort.CurrencyRewardLabel => CurrencyRewardLabel;
        SurvivorsExplorationFeedback ISurvivorsExplorationPort.Feedback => ExplorationFeedback;
        long ISurvivorsExplorationPort.SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum, string source) =>
            SpawnGameplayEnemyOffscreen(role, seed, minimum, maximum, source)?.InstanceId.Value ?? 0;
        bool ISurvivorsExplorationPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount) != null;
        int ISurvivorsExplorationPort.DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) =>
            DamageNonMajorEnemies(position, radius, damage, source);

        private SurvivorsHordeRushEncounter _hordeRush;
        private SurvivorsHordeRushEncounter HordeRush => _hordeRush ?? (_hordeRush = new SurvivorsHordeRushEncounter(this));
        SurvivorsTemplateTuning ISurvivorsHordeRushPort.Tuning => CurrentTuning;
        float ISurvivorsHordeRushPort.RunTime => RunTimeSeconds;
        int ISurvivorsHordeRushPort.Escalation => RunEscalationLevel;
        int ISurvivorsHordeRushPort.MaximumAlive => ResolveEnemyMaximumAlive();
        long ISurvivorsHordeRushPort.SpawnSequence => _spawnSequence;
        long ISurvivorsHordeRushPort.SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum) =>
            SpawnGameplayEnemyOffscreen(role, seed, minimum, maximum, "horde-rush")?.InstanceId.Value ?? 0;
        bool ISurvivorsHordeRushPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount) != null;
        int ISurvivorsHordeRushPort.DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) =>
            DamageNonMajorEnemies(position, radius, damage, source);
        void ISurvivorsHordeRushPort.ShowWarning(string label, float radius, float remaining)
        {
            RecordIncomingThreatTelegraph(PlayerPosition, SurvivorsEnemyRole.Runner, label, radius, remaining);
            PlayFeedback(_bossPulse, PlayerPosition, 22, _dangerClip);
        }
        void ISurvivorsHordeRushPort.ShowBurst(string label)
        {
            RecordStreakRewardFeedback(label, new Color(1f, 0.42f, 0.18f));
            PlayFeedback(_bossPulse, PlayerPosition, 28, _dangerClip);
        }
        void ISurvivorsHordeRushPort.ShowClear(Vector3 position, string label, int hitCount)
        {
            RecordStreakRewardFeedback(label, new Color(1f, 0.74f, 0.24f));
            PlayFeedback(_levelUpPulse, position, Mathf.Clamp(24 + hitCount * 5, 28, 78), _pickupClip);
        }
        private Transform _worldRoot => _runtimeWorld?.Root;
        private Transform _prefabRoot => _runtimeWorld?.PrefabRoot;
        private GameObject _playerObject => _runtimeWorld?.Player;
        private Renderer _playerRenderer => _runtimeWorld?.PlayerRenderer;
        private Camera _camera => _runtimeCamera?.Camera;
        private GameObject _enemyPrefab => _runtimeWorld?.EnemyPrefab;
        private GameObject _experiencePickupPrefab => _runtimeWorld?.ExperiencePrefab;
        private GameObject _magnetPickupPrefab => _runtimeWorld?.MagnetPrefab;
        private GameObject _healthPickupPrefab => _runtimeWorld?.HealthPrefab;
        private GameObject _bloodShardPickupPrefab => _runtimeWorld?.BloodShardPrefab;
        private GameObject _projectilePrefab => _runtimeWorld?.ProjectilePrefab;
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
        private readonly SurvivorsHudStyles _hudStyles = new SurvivorsHudStyles();
        private GUIStyle _hudTitleStyle => _hudStyles.HudTitleStyle;
        private GUIStyle _hudLabelStyle => _hudStyles.HudLabelStyle;
        private GUIStyle _hudSmallStyle => _hudStyles.HudSmallStyle;
        private GUIStyle _lowHealthStyle => _hudStyles.LowHealthStyle;
        private GUIStyle _majorThreatWarningStyle => _hudStyles.MajorThreatWarningStyle;
        private GUIStyle _rewardFeedbackStyle => _hudStyles.RewardFeedbackStyle;
        private GUIStyle _draftTitleStyle => _hudStyles.DraftTitleStyle;
        private GUIStyle _draftCardNameStyle => _hudStyles.DraftCardNameStyle;
        private GUIStyle _draftCardMetaStyle => _hudStyles.DraftCardMetaStyle;
        private GUIStyle _draftCardDescriptionStyle => _hudStyles.DraftCardDescriptionStyle;
        private GUIStyle _draftCardHotkeyStyle => _hudStyles.DraftCardHotkeyStyle;
        private GUIStyle _menuTitleStyle => _hudStyles.MenuTitleStyle;
        private GUIStyle _menuTabStyle => _hudStyles.MenuTabStyle;
        private GUIStyle _transparentButtonStyle => _hudStyles.TransparentButtonStyle;
        private SurvivorsSpawnPoseResolver _poseResolver;
        private WorldSpawnService _spawnService => _runtimeWorld?.Spawning;
        private SurvivorsRunFlowRuntime _runFlow;
        private IReadOnlyList<SurvivorsRelicDefinition> _relicDefinitions;
        private IReadOnlyList<SurvivorsClassUpgradeGateDefinition> _upgradeClassGates;
        private SurvivorsClassLibraryDefinition _classLibrary;
        private SurvivorsClassDefinition _selectedClass;
        private SurvivorsMetaProgressionService _metaProgression => _profileSession.Current;
        private SurvivorsTimedEncounterDirector _timedEncounters;
        private SurvivorsTimedEncounterDirector TimedEncounters => _timedEncounters ??
            (_timedEncounters = new SurvivorsTimedEncounterDirector(this));
        SurvivorsRunFlowRuntime ISurvivorsTimedEncounterPort.RunFlow => _runFlow;
        SurvivorsTemplateTuning ISurvivorsTimedEncounterPort.Tuning => CurrentTuning;
        float ISurvivorsTimedEncounterPort.RunTime => RunTimeSeconds;
        bool ISurvivorsTimedEncounterPort.IsEndlessPlaying => IsEndlessRun;
        bool ISurvivorsTimedEncounterPort.TrySpawn(SurvivorsEnemyRole role, string source) =>
            SpawnEnemy(Vector3.zero, explicitPosition: false, role, gameplaySpawn: true, spawnSource: source) != null;
        void ISurvivorsTimedEncounterPort.EnterVictory() => EnterVictory();
        void ISurvivorsTimedEncounterPort.ShowWarning(SurvivorsEnemyRole role, string label, float remainingSeconds)
        {
            RecordIncomingThreatTelegraph(PlayerPosition, role, label, ResolveIncomingThreatTelegraphRadius(role), remainingSeconds);
            PlayFeedback(_bossPulse, PlayerPosition, role == SurvivorsEnemyRole.Boss ? 44 : 28, _dangerClip,
                role == SurvivorsEnemyRole.Boss ? AudioEventBossWarning : AudioEventEliteWarning, 0.35f);
        }

        private SurvivorsSwarmSpawnCoordinator _swarmSpawning;
        private SurvivorsSwarmSpawnCoordinator SwarmSpawning => _swarmSpawning ??
            (_swarmSpawning = new SurvivorsSwarmSpawnCoordinator(this));
        long ISurvivorsSwarmSpawnPort.SpawnSequence => _spawnSequence;
        bool ISurvivorsSwarmSpawnPort.TrySpawn(SurvivorsEnemyRole role) =>
            SpawnEnemy(Vector3.zero, explicitPosition: false, role, gameplaySpawn: true, spawnSource: "normal-pack") != null;
        private SurvivorsTraversalDirector _traversal;
        private SurvivorsTraversalDirector Traversal => _traversal ?? (_traversal = new SurvivorsTraversalDirector(this));
        SurvivorsTemplateTuning ISurvivorsTraversalPort.Tuning => CurrentTuning;
        int ISurvivorsTraversalPort.ActiveShrineEnemyCount => ShrineTrials.ActiveCount;
        void ISurvivorsTraversalPort.SpawnShrine(Vector3 direction) => ShrineTrials.SpawnArenaShrineTrial(direction);
        void ISurvivorsTraversalPort.SpawnCache(Vector3 direction, int sequenceOffset) => RoamingCaches.SpawnRoamingArenaCache(direction, sequenceOffset);
        private readonly SurvivorsSpawnSequence SpawnSequence = new SurvivorsSpawnSequence();
        private long _spawnSequence => SpawnSequence.Current;
        private SurvivorsUpgradeModifiers _upgradeModifiers;
        private SurvivorsUpgradeModifiers UpgradeModifiers => _upgradeModifiers ?? (_upgradeModifiers = new SurvivorsUpgradeModifiers(this));
        private readonly SurvivorsRunSession _runSession = new SurvivorsRunSession();
        private readonly SurvivorsExperienceProgression _experienceProgression = new SurvivorsExperienceProgression();
        private SurvivorsAudioPresenter _audioPresentation;
        private SurvivorsAudioPresenter AudioPresentation => _audioPresentation ??
            (_audioPresentation = new SurvivorsAudioPresenter(() => ActiveUiTheme, () => Time.unscaledTime));
        private SurvivorsAudioEventRouter _audioEvents => AudioPresentation.Events;
        private readonly SurvivorsProfileSession _profileSession = new SurvivorsProfileSession(
            () => new PersistenceService(new FileTextStorage(new UnityPersistentDataPathProvider())));
        private bool _debugOverlayVisible { get => FrameInput.DebugVisible; set => FrameInput.DebugVisible = value; }
        private RunUpgradeRarity _highestChosenRarity => RewardHistory.HighestChosenRarity;
        private string _highestChosenRarityLabel => RewardHistory.HighestChosenRarityLabel;
        private string _bestMomentLabel => RewardHistory.BestMomentLabel;
        private SurvivorsThreatHudModel _threatHud;
        private SurvivorsThreatHudModel ThreatHud => _threatHud ??
            (_threatHud = new SurvivorsThreatHudModel(new SurvivorsEnemyHudSource(_enemies)));

        public SurvivorsRunState State => _runSession.State;
        public int Level => _experienceProgression.Level;
        public int Experience => _experienceProgression.Experience;
        public int PendingLevelUps => _experienceProgression.PendingLevelUps;
        public int SpawnedCount => EnemySpawner.SpawnedCount;
        public int KilledCount => Defeats.KilledCount;
        public int ProjectileLaunchCount => ProjectileLauncher.ProjectileLaunchCount;
        public int OrbitHitCount => _weaponDiagnostics.OrbitHitCount;
        public int MeleeSwingCount => _weaponDiagnostics.MeleeSwingCount;
        public int MeleeHitCount => _weaponDiagnostics.MeleeHitCount;
        public int BurstPulseCount => _weaponDiagnostics.BurstPulseCount;
        public int BurstHitCount => _weaponDiagnostics.BurstHitCount;
        public int HitscanFireCount => _weaponDiagnostics.HitscanFireCount;
        public int HitscanHitCount => _weaponDiagnostics.HitscanHitCount;
        public int TempestPrismArcHitCount => _weaponDiagnostics.TempestPrismArcHitCount;
        public string LastTempestPrismArcFeedbackLabel => _weaponDiagnostics.LastTempestPrismArcFeedbackLabel;
        public int ProjectilePierceHitCount => _weaponDiagnostics.ProjectilePierceHitCount;
        public int ProjectileChainHitCount => _weaponDiagnostics.ProjectileChainHitCount;
        public int ProjectileForkSpawnCount => _weaponDiagnostics.ProjectileForkSpawnCount;
        public int ProjectileReturnStartCount => _weaponDiagnostics.ProjectileReturnStartCount;
        public int OrbitKnockbackCount => _weaponDiagnostics.OrbitKnockbackCount;
        public string LastOrbitKnockbackFeedbackLabel => _weaponDiagnostics.LastOrbitKnockbackFeedbackLabel;
        public int FrostFanSlowApplicationCount => DamageAugments.FrostFanSlowApplicationCount;
        public string LastFrostFanSlowFeedbackLabel => DamageAugments.LastFrostFanSlowFeedbackLabel;
        public int CinderBurnApplicationCount => DamageAugments.CinderBurnApplicationCount;
        public string LastCinderBurnFeedbackLabel => DamageAugments.LastCinderBurnFeedbackLabel;
        public int PayloadThrowCount => _weaponDiagnostics.PayloadThrowCount;
        public int PayloadPlacedCount => _weaponDiagnostics.PayloadPlacedCount;
        public int PayloadDetonationCount => _weaponDiagnostics.PayloadDetonationCount;
        public int PayloadExplosionHitCount => _weaponDiagnostics.PayloadExplosionHitCount;
        public int PayloadHazardTickCount => PayloadHazards.PayloadHazardTickCount;
        public int PayloadHazardSnareCount => PayloadHazards.PayloadHazardSnareCount;
        public string LastPayloadHazardSnareFeedbackLabel => PayloadHazards.LastPayloadHazardSnareFeedbackLabel;
        public int PayloadHazardChainActivationCount => PayloadHazards.PayloadHazardChainActivationCount;
        public int PayloadHazardChainPulseHitCount => PayloadHazards.PayloadHazardChainPulseHitCount;
        public int PayloadHazardChainExperienceGemDropCount => PayloadHazards.PayloadHazardChainExperienceGemDropCount;
        public string LastPayloadHazardChainFeedbackLabel => PayloadHazards.LastPayloadHazardChainFeedbackLabel;
        public int SplitterChildSpawnCount => EnemySupportSpawning.SplitterChildSpawnCount;
        public int SplitterSplitFeedbackCount => EnemySupportSpawning.SplitterSplitFeedbackCount;
        public string LastSplitterSplitFeedbackLabel => EnemySupportSpawning.LastSplitterSplitFeedbackLabel;
        public int SummonerSupportSpawnCount => EnemySupportSpawning.SummonerSupportSpawnCount;
        public int SummonerSupportFeedbackCount => EnemySupportSpawning.SummonerSupportFeedbackCount;
        public string LastSummonerSupportFeedbackLabel => EnemySupportSpawning.LastSummonerSupportFeedbackLabel;
        public int MinibossSpawnCount => EnemySpawner.MinibossSpawnCount;
        public int BossSpawnCount => EnemySpawner.BossSpawnCount;
        public int EliteKilledCount => Defeats.EliteKilledCount;
        public int MinibossKilledCount => Defeats.MinibossKilledCount;
        public int BossKilledCount => Defeats.BossKilledCount;
        public int EliteRewardGrantCount => RunRewards.EliteRewardGrantCount;
        public int MinibossRewardGrantCount => RunRewards.MinibossRewardGrantCount;
        public int BossRewardGrantCount => RunRewards.BossRewardGrantCount;
        public int BossRelicDraftOpenCount => DraftSession.RelicOpenCount;
        public int EliteUpgradeDraftOpenCount => DraftSession.EliteOpenCount;
        public int BossUpgradeDraftOpenCount => DraftSession.BossOpenCount;
        public int LevelUpDraftOpenCount => DraftSession.LevelUpOpenCount;
        public int SelectedRelicCount => RelicInventory.Count;
        public int BossRelicSurgeCount => BuildSurges.BossRelicSurgeCount;
        public int BossRelicSurgeHitCount => BuildSurges.BossRelicSurgeHitCount;
        public string LastBossRelicSurgeFeedbackLabel => BuildSurges.LastBossRelicSurgeFeedbackLabel;
        public int WeaponLoadoutSurgeActivationCount => BuildSurges.WeaponLoadoutSurgeActivationCount;
        public int WeaponLoadoutSurgePulseHitCount => BuildSurges.WeaponLoadoutSurgePulseHitCount;
        public string LastWeaponLoadoutSurgeFeedbackLabel => BuildSurges.LastWeaponLoadoutSurgeFeedbackLabel;
        public int PassiveLoadoutSurgeActivationCount => BuildSurges.PassiveLoadoutSurgeActivationCount;
        public int PassiveLoadoutSurgePulseHitCount => BuildSurges.PassiveLoadoutSurgePulseHitCount;
        public string LastPassiveLoadoutSurgeFeedbackLabel => BuildSurges.LastPassiveLoadoutSurgeFeedbackLabel;
        public int LevelUpPulseCount => SelectionRewards.LevelUpPulseCount;
        public int LevelUpPulseHitCount => SelectionRewards.LevelUpPulseHitCount;
        public string LastLevelUpPulseFeedbackLabel => SelectionRewards.LastLevelUpPulseFeedbackLabel;
        public int SelectedRewardUpgradeCount => DraftSession.SelectedRewardUpgradeCount;
        public int RewardUpgradeSurgeCount => SelectionRewards.RewardUpgradeSurgeCount;
        public int RewardUpgradeSurgeHitCount => SelectionRewards.RewardUpgradeSurgeHitCount;
        public string LastRewardUpgradeSurgeFeedbackLabel => SelectionRewards.LastRewardUpgradeSurgeFeedbackLabel;
        public int RewardJackpotCount => SelectionRewards.RewardJackpotCount;
        public int RewardJackpotExperienceGemDropCount => SelectionRewards.RewardJackpotExperienceGemDropCount;
        public int RewardJackpotBloodShardDropCount => SelectionRewards.RewardJackpotBloodShardDropCount;
        public int RewardJackpotBloodShardsDropped => SelectionRewards.RewardJackpotBloodShardsDropped;
        public string LastRewardJackpotFeedbackLabel => SelectionRewards.LastRewardJackpotFeedbackLabel;
        public int RewardAutoSelectCount => DraftSession.AutoSelectCount;
        public int EndlessThreatSpawnCount => TimedEncounters.EndlessThreatSpawnCount;
        public int EndlessSurgeActivationCount => EndlessSurges.EndlessSurgeActivationCount;
        public int EndlessSurgeTier => EndlessSurges.EndlessSurgeTier;
        public int EndlessSurgeExperienceGemDropCount => EndlessSurges.EndlessSurgeExperienceGemDropCount;
        public int EndlessSurgeBloodShardDropCount => EndlessSurges.EndlessSurgeBloodShardDropCount;
        public int EndlessSurgePulseHitCount => EndlessSurges.EndlessSurgePulseHitCount;
        public string LastEndlessSurgeFeedbackLabel => EndlessSurges.LastEndlessSurgeFeedbackLabel;
        public int HordeRushSpawnCount => HordeRush.HordeRushSpawnCount;
        public int HordeRushEnemySpawnCount => HordeRush.HordeRushEnemySpawnCount;
        public int HordeRushWarningCount => HordeRush.HordeRushWarningCount;
        public int HordeRushClearRewardCount => HordeRush.HordeRushClearRewardCount;
        public int HordeRushClearExperienceGemDropCount => HordeRush.HordeRushClearExperienceGemDropCount;
        public int HordeRushClearSpecialDropCount => HordeRush.HordeRushClearSpecialDropCount;
        public int HordeRushClearPulseCount => HordeRush.HordeRushClearPulseCount;
        public int HordeRushClearPulseHitCount => HordeRush.HordeRushClearPulseHitCount;
        public int HordeRushClearSurgeActivationCount => HordeRush.HordeRushClearSurgeActivationCount;
        public string LastHordeRushFeedbackLabel => HordeRush.LastHordeRushFeedbackLabel;
        public string LastHordeRushClearFeedbackLabel => HordeRush.LastHordeRushClearFeedbackLabel;
        public string LastHordeRushClearPulseFeedbackLabel => HordeRush.LastHordeRushClearPulseFeedbackLabel;
        public int DraftRerollCount => DraftSession.RerollCount;
        public int DraftBanishCount => DraftSession.BanishCount;
        public int DraftSkipCount => DraftSession.SkipCount;
        public int ClassUnlockRewardCount => RunRewards.ClassUnlockRewardCount;
        public string LastClassUnlockRewardFeedbackLabel => ProgressionFeedback.LastClassUnlockRewardFeedbackLabel;
        public int DamagePopupSpawnCount => _damageFeedback.SpawnCount;
        public int PlayerDamageFeedbackCount => DamageFeedback.PlayerDamageFeedbackCount;
        public int LowHealthClutchPulseCount => PlayerVitals.LowHealthClutchPulseCount;
        public int LowHealthClutchPulseHitCount => PlayerVitals.LowHealthClutchPulseHitCount;
        public string LastLowHealthClutchPulseFeedbackLabel => PlayerVitals.LastLowHealthClutchPulseFeedbackLabel;
        public int DashUseCount => PlayerMotion.DashUseCount;
        public int DashEnemyShoveCount => PlayerMotion.DashEnemyShoveCount;
        public int DashDamageHitCount => PlayerMotion.DashDamageHitCount;
        public string LastDashFeedbackLabel => PlayerMotion.LastDashFeedbackLabel;
        public int EnemyHitFlashFeedbackCount => DamageFeedback.EnemyHitFlashFeedbackCount;
        public int CriticalHitFeedbackCount => DamageFeedback.CriticalHitFeedbackCount;
        public int DeathNovaTriggerCount => DeathNova.DeathNovaTriggerCount;
        public int DeathNovaHitCount => DeathNova.DeathNovaHitCount;
        public int EnemyDeathEffectCount => CombatFeedback.EnemyDeathEffectCount;
        public int EnemyRangedAttackFeedbackCount => CombatFeedback.EnemyRangedAttackFeedbackCount;
        public int EnemyRangedAttackDodgeFeedbackCount => RangedDodgeRewards.EnemyRangedAttackDodgeFeedbackCount;
        public int EnemyRangedAttackDodgeExperienceGemDropCount => RangedDodgeRewards.EnemyRangedAttackDodgeExperienceGemDropCount;
        public string LastEnemyRangedAttackDodgeFeedbackLabel => RangedDodgeRewards.LastEnemyRangedAttackDodgeFeedbackLabel;
        public int MajorRewardDropFeedbackCount => RewardDrops.MajorRewardDropFeedbackCount;
        public int MajorRewardCacheDropCount => MajorRewardPickupCache.MajorRewardCacheDropCount;
        public int MajorRewardCacheExperienceGemDropCount => MajorRewardPickupCache.MajorRewardCacheExperienceGemDropCount;
        public int MajorRewardCacheSpecialDropCount => MajorRewardPickupCache.MajorRewardCacheSpecialDropCount;
        public int MajorRewardCacheAttractedPickupCount => MajorRewardPickupCache.MajorRewardCacheAttractedPickupCount;
        public int WeaponEvolutionFeedbackCount => RunBuild.WeaponEvolutionFeedbackCount;
        public int WeaponEvolutionSurgeCount => BuildSurges.WeaponEvolutionSurgeCount;
        public int WeaponEvolutionSurgeHitCount => BuildSurges.WeaponEvolutionSurgeHitCount;
        public int EvolutionMagnetRecallCount => BuildSurges.EvolutionMagnetRecallCount;
        public int EvolutionMagnetRecallGemCount => BuildSurges.EvolutionMagnetRecallGemCount;
        public int EvolutionChainSurgeActivationCount => BuildSurges.EvolutionChainSurgeActivationCount;
        public int EvolutionChainSurgePulseHitCount => BuildSurges.EvolutionChainSurgePulseHitCount;
        public string LastWeaponEvolutionSurgeFeedbackLabel => BuildSurges.LastWeaponEvolutionSurgeFeedbackLabel;
        public string LastEvolutionMagnetRecallFeedbackLabel => BuildSurges.LastEvolutionMagnetRecallFeedbackLabel;
        public string LastEvolutionChainSurgeFeedbackLabel => BuildSurges.LastEvolutionChainSurgeFeedbackLabel;
        public int MetaUpgradePurchaseCount => PersistentProgression.MetaUpgradePurchaseCount;
        public string LastMetaUpgradePurchaseFeedbackLabel => ProgressionFeedback.LastMetaUpgradePurchaseFeedbackLabel;
        public int ResultClassSelectionCount => PersistentProgression.ResultClassSelectionCount;
        public string LastResultClassSelectionFeedbackLabel => ProgressionFeedback.LastResultClassSelectionFeedbackLabel;
        public int MajorThreatWarningCount => TimedEncounters.MajorThreatWarningCount;
        public int MajorThreatEnrageCount => MajorThreatAbilities.MajorThreatEnrageCount;
        public int MajorThreatEnrageSupportSpawnCount => MajorThreatAbilities.MajorThreatEnrageSupportSpawnCount;
        public string LastMajorThreatEnrageFeedbackLabel => MajorThreatAbilities.LastMajorThreatEnrageFeedbackLabel;
        public int MajorThreatSlamWarningCount => MajorThreatAbilities.MajorThreatSlamWarningCount;
        public int MajorThreatSlamCastCount => MajorThreatAbilities.MajorThreatSlamCastCount;
        public int MajorThreatSlamHitCount => MajorThreatAbilities.MajorThreatSlamHitCount;
        public int MajorThreatSlamTelegraphEffectCount => ThreatTelegraphs.MajorThreatSlamTelegraphEffectCount;
        public int IncomingThreatTelegraphEffectCount => ThreatTelegraphs.IncomingThreatTelegraphEffectCount;
        public string LastMajorThreatSlamFeedbackLabel => MajorThreatAbilities.LastMajorThreatSlamFeedbackLabel;
        public string LastIncomingThreatTelegraphLabel => ThreatTelegraphs.LastIncomingThreatTelegraphLabel;
        public int ExperiencePickupFeedbackCount => PickupCollection.ExperiencePickupFeedbackCount;
        public int ExperienceComboFeedbackCount => ExperienceRhythm.ExperienceComboFeedbackCount;
        public int GemRushActivationCount => ExperienceRhythm.GemRushActivationCount;
        public string LastExperienceComboFeedbackLabel => ExperienceRhythm.LastExperienceComboFeedbackLabel;
        public string LastGemRushFeedbackLabel => ExperienceRhythm.LastGemRushFeedbackLabel;
        public int EvolutionGoalFeedbackCount => ProgressionFeedback.EvolutionGoalFeedbackCount;
        public string LastEvolutionGoalFeedbackLabel => ProgressionFeedback.LastEvolutionGoalFeedbackLabel;
        public int EvolutionReadyFeedbackCount => ProgressionFeedback.EvolutionReadyFeedbackCount;
        public string LastEvolutionReadyFeedbackLabel => ProgressionFeedback.LastEvolutionReadyFeedbackLabel;
        public int HealthPickupCollectedCount => PlayerVitals.HealthPickupCollectedCount;
        public float HealthRestoredByPickups => PlayerVitals.HealthRestoredByPickups;
        public int BloodShardPickupCollectedCount => PickupCollection.BloodShardPickupCollectedCount;
        public int BloodShardsCollectedFromPickups => PickupCollection.BloodShardsCollectedFromPickups;
        public int PickupAttractionFeedbackCount => PickupCollection.PickupAttractionFeedbackCount;
        public int MagnetRecallFeedbackCount => PickupCollection.MagnetRecallFeedbackCount;
        public int RewardCardPresentationCount => RewardHistory.RewardCardPresentationCount;
        public int RewardSelectionFeedbackCount => RewardHistory.RewardSelectionFeedbackCount;
        public string LastRewardCardPresentationLabel => RewardHistory.LastRewardCardPresentationLabel;
        public string LastRewardSelectionFeedbackLabel => RewardHistory.LastRewardSelectionFeedbackLabel;
        public string LastMajorRewardDropFeedbackLabel => RewardDrops.LastMajorRewardDropFeedbackLabel;
        public string LastMajorRewardCacheFeedbackLabel => MajorRewardPickupCache.LastMajorRewardCacheFeedbackLabel;
        public int ExperienceCollected => _experienceProgression.ExperienceCollected;
        public int SelectedUpgradeCount => DraftSession.SelectedUpgradeCount;
        public int MagnetRecallCount => PickupCollection.MagnetRecallCount;
        public int RoamingCacheDropCount => RoamingCaches.RoamingCacheDropCount;
        public int RoamingCacheExperienceGemDropCount => RoamingCaches.RoamingCacheExperienceGemDropCount;
        public int RoamingCacheMagnetDropCount => RoamingCaches.RoamingCacheMagnetDropCount;
        public int RoamingCacheBloodShardDropCount => RoamingCaches.RoamingCacheBloodShardDropCount;
        public int RoamingCacheAmbushCount => RoamingCaches.RoamingCacheAmbushCount;
        public int RoamingCacheAmbushEnemySpawnCount => RoamingCaches.RoamingCacheAmbushEnemySpawnCount;
        public int RoamingCacheAmbushClearRewardCount => RoamingCaches.RoamingCacheAmbushClearRewardCount;
        public int RoamingCacheAmbushClearExperienceGemDropCount => RoamingCaches.RoamingCacheAmbushClearExperienceGemDropCount;
        public int RoamingCacheAmbushClearMagnetDropCount => RoamingCaches.RoamingCacheAmbushClearMagnetDropCount;
        public int RoamingCacheAmbushClearBloodShardDropCount => RoamingCaches.RoamingCacheAmbushClearBloodShardDropCount;
        public int RoamingCacheSurgeActivationCount => RoamingCaches.RoamingCacheSurgeActivationCount;
        public int RoamingCacheSurgeBonusExperienceGemDropCount => RoamingCaches.RoamingCacheSurgeBonusExperienceGemDropCount;
        public int RoamingCacheSurgePulseHitCount => RoamingCaches.RoamingCacheSurgePulseHitCount;
        public string LastRoamingCacheFeedbackLabel => ExplorationFeedback.LastLabel;
        public string LastRoamingCacheAmbushClearFeedbackLabel => RoamingCaches.LastRoamingCacheAmbushClearFeedbackLabel;
        public string LastRoamingCacheSurgeFeedbackLabel => RoamingCaches.LastRoamingCacheSurgeFeedbackLabel;
        public int ArenaShrineTrialCount => ShrineTrials.ArenaShrineTrialCount;
        public int ArenaShrineEnemySpawnCount => ShrineTrials.ArenaShrineEnemySpawnCount;
        public int ArenaShrineClearRewardCount => ShrineTrials.ArenaShrineClearRewardCount;
        public int ArenaShrineClearExperienceGemDropCount => ShrineTrials.ArenaShrineClearExperienceGemDropCount;
        public int ArenaShrineClearBloodShardDropCount => ShrineTrials.ArenaShrineClearBloodShardDropCount;
        public int ArenaShrineSurgeActivationCount => ShrineTrials.ArenaShrineSurgeActivationCount;
        public int ArenaShrineSurgePulseHitCount => ShrineTrials.ArenaShrineSurgePulseHitCount;
        public string LastArenaShrineFeedbackLabel => ShrineTrials.LastArenaShrineFeedbackLabel;
        public string LastArenaShrineClearFeedbackLabel => ShrineTrials.LastArenaShrineClearFeedbackLabel;
        public int WaystoneDiscoveryCount => Waystones.WaystoneDiscoveryCount;
        public int WaystoneExperienceGemDropCount => Waystones.WaystoneExperienceGemDropCount;
        public int WaystoneBloodShardDropCount => Waystones.WaystoneBloodShardDropCount;
        public int WaystoneAmbushCount => Waystones.WaystoneAmbushCount;
        public int WaystoneAmbushEnemySpawnCount => Waystones.WaystoneAmbushEnemySpawnCount;
        public int WaystoneFocusActivationCount => Waystones.WaystoneFocusActivationCount;
        public int WaystoneChainSurgeActivationCount => Waystones.WaystoneChainSurgeActivationCount;
        public int WaystoneChainSurgeBonusExperienceGemDropCount => Waystones.WaystoneChainSurgeBonusExperienceGemDropCount;
        public int WaystoneChainSurgePulseHitCount => Waystones.WaystoneChainSurgePulseHitCount;
        public string LastWaystoneDiscoveryFeedbackLabel => Waystones.LastWaystoneDiscoveryFeedbackLabel;
        public string LastWaystoneChainSurgeFeedbackLabel => Waystones.LastWaystoneChainSurgeFeedbackLabel;
        public int BestKillStreak => KillStreakRewards.BestKillStreak;
        public int StreakBonusDropCount => KillStreakRewards.StreakBonusDropCount;
        public int StreakHealthDropCount => KillStreakRewards.StreakHealthDropCount;
        public int StreakMagnetDropCount => KillStreakRewards.StreakMagnetDropCount;
        public int StreakBloodShardDropCount => KillStreakRewards.StreakBloodShardDropCount;
        public int StreakRewardFeedbackCount => StreakFeedback.StreakRewardFeedbackCount;
        public string LastStreakRewardFeedbackLabel => StreakFeedback.LastStreakRewardFeedbackLabel;
        public int StreakSurgeTier => KillStreakRewards.StreakSurgeTier;
        public int StreakSurgeActivationCount => KillStreakRewards.StreakSurgeActivationCount;
        public int CurrentKillStreak => KillStreakRewards.CurrentKillStreak;
        public bool IsStreakSurgeActive => KillStreakRewards.IsStreakSurgeActive;
        public float StreakSurgeRemainingSeconds => KillStreakRewards.StreakSurgeRemainingSeconds;
        public float StreakSurgeDamageBonus => KillStreakRewards.StreakSurgeDamageBonus;
        public float StreakSurgeMoveSpeedBonus => KillStreakRewards.StreakSurgeMoveSpeedBonus;
        public float StreakSurgeCooldownMultiplierBonus => KillStreakRewards.StreakSurgeCooldownMultiplierBonus;
        public float StreakSurgePickupRangeBonus => KillStreakRewards.StreakSurgePickupRangeBonus;
        public bool IsRoamingCacheSurgeActive => RoamingCaches.SurgeRemaining > 0f;
        public float RoamingCacheSurgeRemainingSeconds => Mathf.Max(0f, RoamingCaches.SurgeRemaining);
        public float RoamingCacheSurgeDamageBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, CurrentTuning.RoamingCacheSurgeDamageBonus) : 0f;
        public float RoamingCacheSurgeMoveSpeedBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, CurrentTuning.RoamingCacheSurgeMoveSpeedBonus) : 0f;
        public float RoamingCacheSurgeCooldownMultiplierBonus => IsRoamingCacheSurgeActive ? Mathf.Min(0f, CurrentTuning.RoamingCacheSurgeCooldownMultiplierBonus) : 0f;
        public float RoamingCacheSurgePickupRangeBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, CurrentTuning.RoamingCacheSurgePickupRangeBonus) : 0f;
        public bool IsArenaShrineSurgeActive => ShrineTrials.SurgeRemaining > 0f;
        public float ArenaShrineSurgeRemainingSeconds => Mathf.Max(0f, ShrineTrials.SurgeRemaining);
        public float ArenaShrineSurgeDamageBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, CurrentTuning.ArenaShrineSurgeDamageBonus) : 0f;
        public float ArenaShrineSurgeMoveSpeedBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, CurrentTuning.ArenaShrineSurgeMoveSpeedBonus) : 0f;
        public float ArenaShrineSurgeCooldownMultiplierBonus => IsArenaShrineSurgeActive ? Mathf.Min(0f, CurrentTuning.ArenaShrineSurgeCooldownMultiplierBonus) : 0f;
        public float ArenaShrineSurgePickupRangeBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, CurrentTuning.ArenaShrineSurgePickupRangeBonus) : 0f;
        public bool IsWaystoneFocusActive => Waystones.FocusRemaining > 0f;
        public float WaystoneFocusRemainingSeconds => Mathf.Max(0f, Waystones.FocusRemaining);
        public float WaystoneFocusDamageBonus => IsWaystoneFocusActive ? Mathf.Max(0f, CurrentTuning.WaystoneFocusDamageBonus) : 0f;
        public float WaystoneFocusMoveSpeedBonus => IsWaystoneFocusActive ? Mathf.Max(0f, CurrentTuning.WaystoneFocusMoveSpeedBonus) : 0f;
        public float WaystoneFocusCooldownMultiplierBonus => IsWaystoneFocusActive ? Mathf.Min(0f, CurrentTuning.WaystoneFocusCooldownMultiplierBonus) : 0f;
        public float WaystoneFocusPickupRangeBonus => IsWaystoneFocusActive ? Mathf.Max(0f, CurrentTuning.WaystoneFocusPickupRangeBonus) : 0f;
        public bool IsWaystoneChainSurgeActive => Waystones.ChainRemaining > 0f;
        public float WaystoneChainSurgeRemainingSeconds => Mathf.Max(0f, Waystones.ChainRemaining);
        public float WaystoneChainSurgeDamageBonus => IsWaystoneChainSurgeActive ? Mathf.Max(0f, CurrentTuning.WaystoneChainDamageBonus) : 0f;
        public float WaystoneChainSurgeMoveSpeedBonus => IsWaystoneChainSurgeActive ? Mathf.Max(0f, CurrentTuning.WaystoneChainMoveSpeedBonus) : 0f;
        public float WaystoneChainSurgeCooldownMultiplierBonus => IsWaystoneChainSurgeActive ? Mathf.Min(0f, CurrentTuning.WaystoneChainCooldownMultiplierBonus) : 0f;
        public float WaystoneChainSurgePickupRangeBonus => IsWaystoneChainSurgeActive ? Mathf.Max(0f, CurrentTuning.WaystoneChainPickupRangeBonus) : 0f;
        public bool IsHordeRushClearSurgeActive => HordeRush.ClearSurgeRemaining > 0f;
        public float HordeRushClearSurgeRemainingSeconds => HordeRush.ClearSurgeRemaining;
        public float HordeRushClearSurgeDamageBonus => IsHordeRushClearSurgeActive ? Mathf.Max(0f, CurrentTuning.HordeRushClearSurgeDamageBonus) : 0f;
        public float HordeRushClearSurgeMoveSpeedBonus => IsHordeRushClearSurgeActive ? Mathf.Max(0f, CurrentTuning.HordeRushClearSurgeMoveSpeedBonus) : 0f;
        public float HordeRushClearSurgeCooldownMultiplierBonus => IsHordeRushClearSurgeActive ? Mathf.Min(0f, CurrentTuning.HordeRushClearSurgeCooldownMultiplierBonus) : 0f;
        public float HordeRushClearSurgePickupRangeBonus => IsHordeRushClearSurgeActive ? Mathf.Max(0f, CurrentTuning.HordeRushClearSurgePickupRangeBonus) : 0f;
        public bool IsWeaponLoadoutSurgeActive => BuildSurges.IsWeaponLoadoutSurgeActive;
        public float WeaponLoadoutSurgeRemainingSeconds => BuildSurges.WeaponLoadoutSurgeRemainingSeconds;
        public float WeaponLoadoutSurgeDamageBonus => BuildSurges.WeaponLoadoutSurgeDamageBonus;
        public float WeaponLoadoutSurgeMoveSpeedBonus => BuildSurges.WeaponLoadoutSurgeMoveSpeedBonus;
        public float WeaponLoadoutSurgeCooldownMultiplierBonus => BuildSurges.WeaponLoadoutSurgeCooldownMultiplierBonus;
        public float WeaponLoadoutSurgePickupRangeBonus => BuildSurges.WeaponLoadoutSurgePickupRangeBonus;
        public bool IsPassiveLoadoutSurgeActive => BuildSurges.IsPassiveLoadoutSurgeActive;
        public float PassiveLoadoutSurgeRemainingSeconds => BuildSurges.PassiveLoadoutSurgeRemainingSeconds;
        public float PassiveLoadoutSurgeDamageBonus => BuildSurges.PassiveLoadoutSurgeDamageBonus;
        public float PassiveLoadoutSurgeMoveSpeedBonus => BuildSurges.PassiveLoadoutSurgeMoveSpeedBonus;
        public float PassiveLoadoutSurgeCooldownMultiplierBonus => BuildSurges.PassiveLoadoutSurgeCooldownMultiplierBonus;
        public float PassiveLoadoutSurgePickupRangeBonus => BuildSurges.PassiveLoadoutSurgePickupRangeBonus;
        public float PassiveLoadoutSurgeExperienceGainMultiplierBonus => BuildSurges.PassiveLoadoutSurgeExperienceGainMultiplierBonus;
        public bool IsBossRelicSurgeActive => BuildSurges.IsBossRelicSurgeActive;
        public float BossRelicSurgeRemainingSeconds => BuildSurges.BossRelicSurgeRemainingSeconds;
        public float BossRelicSurgeDamageBonus => BuildSurges.BossRelicSurgeDamageBonus;
        public float BossRelicSurgeMoveSpeedBonus => BuildSurges.BossRelicSurgeMoveSpeedBonus;
        public float BossRelicSurgeCooldownMultiplierBonus => BuildSurges.BossRelicSurgeCooldownMultiplierBonus;
        public float BossRelicSurgePickupRangeBonus => BuildSurges.BossRelicSurgePickupRangeBonus;
        public bool IsGemRushActive => ExperienceRhythm.IsGemRushActive;
        public float GemRushRemainingSeconds => ExperienceRhythm.GemRushRemainingSeconds;
        public float GemRushDamageBonus => ExperienceRhythm.GemRushDamageBonus;
        public float GemRushMoveSpeedBonus => ExperienceRhythm.GemRushMoveSpeedBonus;
        public float GemRushCooldownMultiplierBonus => ExperienceRhythm.GemRushCooldownMultiplierBonus;
        public float GemRushPickupRangeBonus => ExperienceRhythm.GemRushPickupRangeBonus;
        public bool IsEvolutionChainSurgeActive => BuildSurges.IsEvolutionChainSurgeActive;
        public float EvolutionChainSurgeRemainingSeconds => BuildSurges.EvolutionChainSurgeRemainingSeconds;
        public float EvolutionChainSurgeDamageBonus => BuildSurges.EvolutionChainSurgeDamageBonus;
        public float EvolutionChainSurgeMoveSpeedBonus => BuildSurges.EvolutionChainSurgeMoveSpeedBonus;
        public float EvolutionChainSurgeCooldownMultiplierBonus => BuildSurges.EvolutionChainSurgeCooldownMultiplierBonus;
        public float EvolutionChainSurgePickupRangeBonus => BuildSurges.EvolutionChainSurgePickupRangeBonus;
        public bool IsEndlessSurgeActive => EndlessSurges.IsEndlessSurgeActive;
        public float EndlessSurgeRemainingSeconds => EndlessSurges.EndlessSurgeRemainingSeconds;
        public float EndlessSurgeDamageBonus => EndlessSurges.EndlessSurgeDamageBonus;
        public float EndlessSurgeMoveSpeedBonus => EndlessSurges.EndlessSurgeMoveSpeedBonus;
        public float EndlessSurgeCooldownMultiplierBonus => EndlessSurges.EndlessSurgeCooldownMultiplierBonus;
        public float EndlessSurgePickupRangeBonus => EndlessSurges.EndlessSurgePickupRangeBonus;
        public int EndlessExplorationBonusTier => ExplorationBonuses.Tier;
        public int BonusBloodShardsEarnedThisRun => RunRewards.BonusBloodShards;
        public int BonusLegacyExperienceEarnedThisRun => RunRewards.BonusLegacyExperience;
        public int BloodShardsEarnedThisRun => RunRewards.BloodShardsEarned;
        public int LegacyExperienceEarnedThisRun => RunRewards.LegacyExperienceEarned;
        public SurvivorsRunRewardSummary LastRunResult => RunRewards.LastRunResult;
        public IReadOnlyList<string> LastRunSummaryLines => _lastRunSummaryLines;
        public float RunTimeSeconds => _runSession.ElapsedSeconds;
        public float MoveSpeedBonus => UpgradeModifiers.MoveSpeedBonus;
        public float DamageBonus => UpgradeModifiers.DamageBonus;
        public float PersistentDamageBonus => UpgradeModifiers.PersistentDamageBonus;
        public float PersistentMaxHealthBonus => UpgradeModifiers.PersistentMaxHealthBonus;
        public float PersistentPickupRangeBonus => UpgradeModifiers.PersistentPickupRangeBonus;
        public float PersistentExperienceGainMultiplierBonus => UpgradeModifiers.PersistentExperienceGainMultiplierBonus;
        public int PersistentDraftRerollBonus => UpgradeModifiers.PersistentDraftRerollBonus;
        public float RelicDamageBonus => UpgradeModifiers.RelicDamageBonus;
        public float RelicCooldownMultiplierBonus => UpgradeModifiers.RelicCooldownMultiplierBonus;
        public float RelicPickupRangeBonus => UpgradeModifiers.RelicPickupRangeBonus;
        public float WeaponCooldownMultiplierBonus => UpgradeModifiers.WeaponCooldownMultiplierBonus;
        public float PickupRangeBonus => UpgradeModifiers.PickupRangeBonus;
        public float BarrierValue => PlayerVitals.BarrierValue;
        public float BarrierCapacityBonus => UpgradeModifiers.BarrierCapacityBonus;
        public float BarrierRegenPerSecondBonus => UpgradeModifiers.BarrierRegenPerSecondBonus;
        public float BarrierOnDamageRatio => UpgradeModifiers.BarrierOnDamageRatio;
        public float PoisonDamageRatio => UpgradeModifiers.PoisonDamageRatio;
        public float BleedDamageRatio => UpgradeModifiers.BleedDamageRatio;
        public float ExecuteThresholdNormalized => UpgradeModifiers.ExecuteThresholdNormalized;
        public float CriticalChanceBonus => UpgradeModifiers.CriticalChanceBonus;
        public float CriticalDamageMultiplierBonus => UpgradeModifiers.CriticalDamageMultiplierBonus;
        public float DraftLuckBonus => UpgradeModifiers.DraftLuckBonus;
        public float DeathNovaDamageBonus => UpgradeModifiers.DeathNovaDamageBonus;
        public float DeathNovaRadiusBonus => UpgradeModifiers.DeathNovaRadiusBonus;
        public float LifestealRatio => UpgradeModifiers.LifestealRatio;
        public float ExperienceGainMultiplierBonus => UpgradeModifiers.ExperienceGainMultiplierBonus;
        public float AreaRadiusBonus => UpgradeModifiers.AreaRadiusBonus;
        public int ProjectileFanBonus => UpgradeModifiers.ProjectileFanBonus;
        public int OrbitBladeBonus => UpgradeModifiers.OrbitBladeBonus;
        public float OrbitRadiusBonus => UpgradeModifiers.OrbitRadiusBonus;
        public int MeleeTargetBonus => UpgradeModifiers.MeleeTargetBonus;
        public int BurstCountBonus => UpgradeModifiers.BurstCountBonus;
        public int BurstEchoBonus => UpgradeModifiers.BurstEchoBonus;
        public int TargetedBurstSigilBonus => UpgradeModifiers.TargetedBurstSigilBonus;
        public int ProjectilePierceBonus => UpgradeModifiers.ProjectilePierceBonus;
        public int ProjectileChainBonus => UpgradeModifiers.ProjectileChainBonus;
        public int ProjectileForkBonus => UpgradeModifiers.ProjectileForkBonus;
        public int ProjectileReturnBonus => UpgradeModifiers.ProjectileReturnBonus;
        public int HitscanPierceBonus => UpgradeModifiers.HitscanPierceBonus;
        public int PayloadCountBonus => UpgradeModifiers.PayloadCountBonus;
        public float PayloadExplosionRadiusBonus => UpgradeModifiers.PayloadExplosionRadiusBonus;
        public float PayloadTriggerRadiusBonus => UpgradeModifiers.PayloadTriggerRadiusBonus;
        public float PickupAttractionSpeedBonus => UpgradeModifiers.PickupAttractionSpeedBonus;
        public float PickupMagnetPulseIntervalReductionBonus => UpgradeModifiers.PickupMagnetPulseIntervalReductionBonus;
        public int ActiveEnemyCount => _enemies.Count;
        public int ActiveRunnerCount => CountEnemiesByRole(SurvivorsEnemyRole.Runner);
        public int ActiveBruiserCount => CountEnemiesByRole(SurvivorsEnemyRole.Bruiser);
        public int ActiveSpitterCount => CountEnemiesByRole(SurvivorsEnemyRole.Spitter);
        public int ActiveSplitterCount => CountEnemiesByRole(SurvivorsEnemyRole.Splitter);
        public int ActiveSummonerCount => CountEnemiesByRole(SurvivorsEnemyRole.Summoner);
        public int ActiveEliteCount => CountEliteEnemies();
        public int ActiveDreadEliteCount => CountEnemiesByRole(SurvivorsEnemyRole.DreadElite);
        public int ActiveMinibossCount => CountEnemiesByRole(SurvivorsEnemyRole.Miniboss);
        public int ActiveBossCount => CountEnemiesByRole(SurvivorsEnemyRole.Boss);
        public int ActiveBossLifeBarCount => CountAuthoredThreatLifeBars(showBossLifeBar: true);
        public int ActiveOverheadLifeBarCount => CountAuthoredThreatLifeBars(showBossLifeBar: false);
        public int ActiveMajorThreatLifeBarCount => ActiveBossLifeBarCount + ActiveOverheadLifeBarCount;
        public string ActiveMajorThreatLifeBarSummary => ResolveActiveMajorThreatLifeBarSummary();
        public bool IsMajorThreatHealthVisible => ThreatHud.SelectHealthThreat().HasValue;
        public string CurrentMajorThreatHealthLabel => ThreatHud.SelectHealthThreat()?.Name ?? string.Empty;
        public float CurrentMajorThreatHealthFraction => Mathf.Clamp01(ThreatHud.SelectHealthThreat()?.HealthFraction ?? 0f);
        public int ActivePickupCount => _pickups.Count;
        public int ActiveHordeRushEnemyCount => HordeRush.ActiveCount;
        public int ActiveRoamingCacheAmbushEnemyCount => RoamingCaches.ActiveCount;
        public int ActiveArenaShrineEnemyCount => ShrineTrials.ActiveCount;
        public int ActiveProjectileCount => _projectiles.Count;
        public int ActiveDamagePopupCount => _damageFeedback.ActiveCount;
        public int ActiveEnemyDeathEffectCount => CombatFeedback.ActiveEnemyDeathEffectCount;
        public int ActiveEnemyRangedAttackFeedbackCount => CombatFeedback.ActiveEnemyRangedAttackFeedbackCount;
        public int ActiveMajorRewardDropFeedbackCount => RewardDrops.ActiveMajorRewardDropFeedbackCount;
        public int ActiveMajorThreatSlamTelegraphEffectCount => ThreatTelegraphs.ActiveMajorThreatSlamTelegraphEffectCount;
        public int ActiveIncomingThreatTelegraphEffectCount => ThreatTelegraphs.ActiveIncomingThreatTelegraphEffectCount;
        public int NormalEnemyRecycleCount => _enemyNavigation?.NormalEnemyRecycleCount ?? 0;
        public int MajorThreatRepositionCount => _enemyNavigation?.MajorThreatRepositionCount ?? 0;
        public int GameplayEnemySpawnSafetyCheckCountForTest => SpawnSafety.GameplayEnemySpawnSafetyCheckCountForTest;
        public int GameplaySpawnInsideCameraViewViolationCountForTest => SpawnSafety.GameplaySpawnInsideCameraViewViolationCountForTest;
        public string LastGameplaySpawnSafetyFailureForTest => SpawnSafety.LastGameplaySpawnSafetyFailureForTest;
        public Vector3 LastGameplaySpawnPositionForTest => SpawnSafety.LastGameplaySpawnPositionForTest;
        public float LastGameplaySpawnPaddingForTest => SpawnSafety.LastGameplaySpawnPaddingForTest;
        public bool LastGameplaySpawnWasInsideCameraViewportForTest => SpawnSafety.LastGameplaySpawnWasInsideCameraViewportForTest;
        public int ActiveOffscreenThreatMarkerCount => CountOffscreenMajorThreatMarkers();
        public string CurrentOffscreenThreatMarkerLabel => ResolveFirstOffscreenMajorThreatMarkerLabel();
        public string LastOffscreenThreatMarkerLabel => ThreatHud.LastMarkerLabel;
        public int ActiveMajorRewardCacheAttractedPickupCount => PickupCollection.ActiveMajorRewardCacheAttractedPickupCount;

        public int ActiveWeaponCount => RunWeapons.ActiveWeaponCount;
        public int ActivePassiveCount => RunBuild.PassiveIds.Count;
        public int EvolvedWeaponCount => RunBuild.EvolutionIds.Count;
        public int MaxWeaponSlots => RunBuild.MaxWeaponSlots;
        public int MaxPassiveSlots => RunBuild.MaxPassiveSlots;
        public int InfiniteArenaTileCountForTest => Arena.TileCount;
        public int InfiniteArenaLandmarkCountForTest => Arena.LandmarkCount;
        public Vector3 ArenaPresentationCenterForTest => Arena.Center;
        public Vector3 FirstInfiniteArenaTilePositionForTest => Arena.FirstTilePosition;
        public Vector3 FirstInfiniteArenaLandmarkPositionForTest => Arena.FirstLandmarkPosition;
        public Vector3 ClosestInfiniteArenaLandmarkPositionForTest => ResolveClosestArenaLandmarkPositionForTest();
        public float CurrentWaystoneCompassDistanceForTest => TryResolveClosestArenaLandmark(ignoreDiscovered: true, out _, out float distance, out _) ? distance : 0f;
        public string CurrentWaystoneCompassHudLabel => ResolveWaystoneCompassHudLabel();
        public bool IsWaystoneCompassArrowVisibleForTest => Arena.CompassVisible;
        public Vector3 WaystoneCompassArrowForwardForTest => Arena.CompassForward;
        public IReadOnlyList<string> ActiveWeaponIds => RunWeapons.ActiveWeaponIds;
        public int ActiveOrbitBladeCount => RunWeapons.ActiveOrbitBladeCount;
        public float PlayerMoveSpeed => CurrentTuning.PlayerMoveSpeed + MoveSpeedBonus + StreakSurgeMoveSpeedBonus + RoamingCacheSurgeMoveSpeedBonus + ArenaShrineSurgeMoveSpeedBonus + WaystoneFocusMoveSpeedBonus + WaystoneChainSurgeMoveSpeedBonus + HordeRushClearSurgeMoveSpeedBonus + WeaponLoadoutSurgeMoveSpeedBonus + PassiveLoadoutSurgeMoveSpeedBonus + BossRelicSurgeMoveSpeedBonus + GemRushMoveSpeedBonus + EvolutionChainSurgeMoveSpeedBonus + EndlessSurgeMoveSpeedBonus;
        public float DashCooldownRemainingSeconds => PlayerMotion.CooldownRemaining;
        public float PlayerSafetyRemainingSeconds => PlayerVitals.SafetyRemaining;
        public bool IsPlayerSafetyActive => PlayerVitals.SafetyRemaining > 0f;
        public float ProjectileDamage => ResolveDisplayedWeaponDamage();
        public float WeaponCooldownSeconds => ResolveDisplayedWeaponCooldownSeconds();
        public float CurrentPickupAttractRange => Mathf.Max(0f, CurrentTuning.PickupAttractRange + PickupRangeBonus + StreakSurgePickupRangeBonus + RoamingCacheSurgePickupRangeBonus + ArenaShrineSurgePickupRangeBonus + WaystoneFocusPickupRangeBonus + WaystoneChainSurgePickupRangeBonus + HordeRushClearSurgePickupRangeBonus + WeaponLoadoutSurgePickupRangeBonus + PassiveLoadoutSurgePickupRangeBonus + BossRelicSurgePickupRangeBonus + GemRushPickupRangeBonus + EvolutionChainSurgePickupRangeBonus + EndlessSurgePickupRangeBonus);
        public float CurrentPickupAttractionSpeed => Mathf.Max(0.1f, CurrentTuning.PickupAttractionSpeed + PickupAttractionSpeedBonus);
        public float CurrentPickupMagnetPulseIntervalSeconds => ResolvePickupMagnetPulseIntervalSeconds();
        public float LevelUpDraftCooldownRemainingSeconds => Mathf.Max(0f, _experienceProgression.DraftCooldownRemaining);
        public int LevelAtOneMinute => Telemetry.LevelAtOneMinute;
        public int LevelAtTwoMinutes => Telemetry.LevelAtTwoMinutes;
        public int LevelAtThreeMinutes => Telemetry.LevelAtThreeMinutes;
        public int LevelAtFourMinutes => Telemetry.LevelAtFourMinutes;
        public int LevelAtFiveMinutes => Telemetry.LevelAtFiveMinutes;
        public int MagnetPulseActivationCount => PickupCollection.MagnetPulseActivationCount;
        public string LastMagnetPulseFeedbackLabel => PickupCollection.LastMagnetPulseFeedbackLabel;
        public float CriticalChanceNormalized => Mathf.Clamp01(CriticalChanceBonus);
        public float CriticalDamageMultiplier => Mathf.Clamp(1.5f + CriticalDamageMultiplierBonus, 1f, 100f);
        public float DeathNovaDamage => Mathf.Max(0f, DeathNovaDamageBonus);
        public float DeathNovaRadius => DeathNovaDamage <= 0f ? 0f : Mathf.Max(0f, BaseDeathNovaRadius + DeathNovaRadiusBonus + AreaRadiusBonus * 0.5f);
        public float CurrentHealth => PlayerVitals.CurrentHealth;
        public float MaxHealth => PlayerVitals.MaxHealth;
        public float BarrierCapacity => Mathf.Max(0f, CurrentTuning.StartingBarrierCapacity + BarrierCapacityBonus);
        public Vector3 PlayerPosition => _playerObject == null ? transform.position : _playerObject.transform.position;
        public Vector3 PlayerForward => _playerObject == null ? Vector3.forward : _playerObject.transform.forward;
        public CombatCatalog CombatCatalog => EnemyDamage.Catalog;
        public SurvivorsPacingProfile CurrentPacingProfile => CurrentTuning.PacingProfile;
        public bool IsHumanPlaytestPacing => CurrentPacingProfile == SurvivorsPacingProfile.HumanPlaytest;
        public bool IsDebugFastPacing => CurrentPacingProfile == SurvivorsPacingProfile.DebugFast;
        public bool IsSprintRunMode => CurrentPacingProfile == SurvivorsPacingProfile.SprintRun;
        public bool IsRunModeSelectionOpen => Menus.ModeSelectionOpen;
        public string CurrentRunModeDisplayName => CurrentTuning.RunModeDisplayName;
        public bool IsDebugOverlayVisible => _debugOverlayVisible;
        public bool IsBuildMenuOpen => Menus.BuildOpen;
        public string CurrentBuildMenuTabLabel => SurvivorsMenuSession.FormatBuildMenuTabLabel(Menus.BuildTab);
        public string CurrentUiThemeName => ActiveUiTheme.themeName;
        public IReadOnlyList<SurvivorsUiTheme> AvailableUiThemesForTest => UiThemeSelection.AvailableThemes;
        public int SelectedUiThemeIndex => UiThemeSelection.SelectedIndex;
        public bool IsTutorialOverlayOpen => Menus.TutorialOpen;
        public bool IsTutorialSeen => _metaProgression != null && _metaProgression.TutorialSeen;
        public string CurrentTutorialStepTitle => ResolveTutorialStepTitle(SurvivorsTutorialContent.ClampTutorialStepIndex(Menus.TutorialIndex));
        public int CurrentTutorialStepIndex => SurvivorsTutorialContent.ClampTutorialStepIndex(Menus.TutorialIndex);
        public int TutorialStepCount => SurvivorsTutorialContent.StepCount;
        public bool IsAudioMuted => _audioEvents.Muted;
        public int AudioEventDispatchCount => _audioEvents.DispatchCount;
        public string LastAudioEventId => _audioEvents.LastEventId;
        public string LastRunSummaryTitle => _lastRunSummaryTitle;
        public bool IsRunSummaryVisible => State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory;
        public bool IsPlayerFacingDraftOverlayVisible => State == SurvivorsRunState.LevelUp && (DraftSession.CurrentDraft != null || DraftSession.CurrentRelicDraft != null);
        public int CurrentDraftCardCountForTest => IsRelicChoiceOpen ? CurrentRelicChoices.Count : CurrentDraftChoices.Count;
        public string CurrentDraftOverlayTitleForTest => ResolveRewardOverlayTitle();
        public SurvivorsTemplateTuning CurrentTuning => tuning ?? (tuning = CreateConfiguredTuning(pacingProfile));
        public SurvivorsRunFlowDefinition CurrentRunFlowDefinition => _runFlow == null ? null : _runFlow.Definition;
        public bool IsAuthoredContentBound => ContentBinding.IsAuthoredContentBound;
        public bool IsStrictAuthoredSample => ContentBinding.IsStrictAuthoredSample;
        public bool IsFallbackContentActive => ContentBinding.IsFallbackContentActive;
        public bool CanStartConfiguredRun => ContentBinding.CanStartConfiguredRun;
        public bool IsUsingAuthoredRunFlow => RuntimeContent.IsUsingAuthoredRunFlow;
        public string AuthoredContentStatus => ContentBinding.AuthoredContentStatus;
        public string TopCenterTimerHudLabel => _runSession.Started ? ResolveTopCenterTimerHudLabel() : string.Empty;
        public bool IsTopCenterTimerVisible => _runSession.Started;
        public Rect TopCenterTimerRectForTest => ResolveTopCenterTimerRect();
        public float CurrentEnemySpawnIntervalSeconds => ResolveEnemySpawnIntervalSeconds();
        public int CurrentEnemySpawnPackSize => ResolveEnemySpawnPackSize();
        public int CurrentEnemyMaximumAlive => ResolveEnemyMaximumAlive();
        public float CurrentEnemySpeedMultiplier => _runFlow == null ? 1f : _runFlow.ResolveEnemySpeedMultiplier();
        public IReadOnlyList<RunUpgradeDefinition> CurrentDraftChoices => DraftSession.CurrentDraft == null ? EmptyChoices : DraftSession.CurrentDraft.Choices;
        public IReadOnlyList<SurvivorsRelicDefinition> CurrentRelicChoices => DraftSession.CurrentRelicDraft == null ? EmptyRelicChoices : DraftSession.CurrentRelicDraft.Choices;
        public float RewardSelectionRemainingSeconds => Mathf.Max(0f, DraftSession.RemainingSeconds);
        public SurvivorsClassDefinition SelectedClass => _selectedClass;
        public string SelectedClassId => _selectedClass == null ? string.Empty : _selectedClass.Id;
        public long MetaBloodShards => _metaProgression == null ? 0 : _metaProgression.UnspentBloodShards;
        public long LifetimeBloodShards => _metaProgression == null ? 0 : _metaProgression.LifetimeBloodShards;
        public long LifetimeLegacyExperience => _metaProgression == null ? 0 : _metaProgression.LifetimeLegacyExperience;
        public int MetaCompletedRuns => _metaProgression == null ? 0 : _metaProgression.CompletedRuns;
        public int MetaBossVictories => _metaProgression == null ? 0 : _metaProgression.BossVictories;
        public int MetaUnlockedClassCount => _metaProgression == null ? 0 : _metaProgression.UnlockedClassIds.Count;
        public SurvivorsRunPhase RunPhase => _runFlow == null ? SurvivorsRunPhase.Opening : _runFlow.Phase;
        public int RunEscalationLevel => _runFlow == null ? 0 : _runFlow.EscalationLevel;
        public bool IsPlaying => State == SurvivorsRunState.Playing;
        public bool IsRunStarted => _runSession.Started;
        public bool IsLevelUpOpen => State == SurvivorsRunState.LevelUp;
        public bool IsRunUpgradeDraftOpen => State == SurvivorsRunState.LevelUp && DraftSession.Kind == SurvivorsRewardSelectionKind.LevelUp;
        public bool IsRelicChoiceOpen => State == SurvivorsRunState.LevelUp && DraftSession.Kind == SurvivorsRewardSelectionKind.BossRelic;
        public bool IsUpgradeRewardChoiceOpen => State == SurvivorsRunState.LevelUp && IsRewardUpgradeSelectionKind(DraftSession.Kind);
        public bool IsGameOver => State == SurvivorsRunState.GameOver;
        public bool IsVictory => State == SurvivorsRunState.Victory;
        public bool HasClearedVictoryThisRun => _runSession.HasClearedVictory;
        public bool IsEndlessRun => State == SurvivorsRunState.Playing && _runSession.HasClearedVictory;
        public bool IsLowHealthWarningActive => (State == SurvivorsRunState.Playing || State == SurvivorsRunState.LevelUp) && MaxHealth > 0f && CurrentHealth / MaxHealth <= LowHealthWarningThreshold;
        public bool IsMajorThreatWarningActive => !string.IsNullOrEmpty(TimedEncounters.WarningLabel) && RunTimeSeconds < TimedEncounters.WarningTargetTime;
        public string CurrentMajorThreatWarningLabel => IsMajorThreatWarningActive ? TimedEncounters.WarningLabel : string.Empty;
        public float MajorThreatWarningRemainingSeconds => IsMajorThreatWarningActive ? Mathf.Max(0f, TimedEncounters.WarningTargetTime - RunTimeSeconds) : 0f;
        public bool IsHordeRushWarningActive => HordeRush.WarningActive;
        public string CurrentHordeRushWarningLabel => HordeRush.WarningLabel;
        public float HordeRushWarningRemainingSeconds => HordeRush.WarningRemaining;
        public float NextHordeRushTimeSecondsForTest => HordeRush.NextTime;
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
        public string ActiveRewardFeedbackLabel => _rewardBanner.RemainingSeconds > 0f ? _rewardBanner.Label : string.Empty;
        public float RewardFeedbackRemainingSeconds => Mathf.Max(0f, _rewardBanner.RemainingSeconds);
        public string ActiveStreakRewardFeedbackLabel => _streakRewardBanner.RemainingSeconds > 0f ? _streakRewardBanner.Label : string.Empty;
        public float StreakRewardFeedbackRemainingSeconds => Mathf.Max(0f, _streakRewardBanner.RemainingSeconds);
        public string ActiveClassUnlockRewardFeedbackLabel => _classUnlockRewardBanner.RemainingSeconds > 0f ? _classUnlockRewardBanner.Label : string.Empty;
        public float ClassUnlockRewardFeedbackRemainingSeconds => Mathf.Max(0f, _classUnlockRewardBanner.RemainingSeconds);
        public string ActiveEvolutionReadyFeedbackLabel => _evolutionReadyBanner.RemainingSeconds > 0f ? _evolutionReadyBanner.Label : string.Empty;
        public float EvolutionReadyFeedbackRemainingSeconds => Mathf.Max(0f, _evolutionReadyBanner.RemainingSeconds);
        public string CurrentEvolutionGoalHudLabel => ResolveEvolutionGoalHudLabel();
        public string CurrentEvolutionReadyHudLabel => ResolveEvolutionReadyHudLabel();
        public string ActiveExperienceComboFeedbackLabel => ExperienceRhythm.ActiveExperienceComboFeedbackLabel;
        public float ExperienceComboFeedbackRemainingSeconds => ExperienceRhythm.ExperienceComboFeedbackRemainingSeconds;
        public int CurrentExperienceComboPickupCount => ExperienceRhythm.CurrentExperienceComboPickupCount;
        public int CurrentExperienceComboAmount => ExperienceRhythm.CurrentExperienceComboAmount;
        public int RequiredExperienceForNextLevel => _experienceProgression.RequiredExperience(CurrentTuning);
        public int TotalDraftRerollCharges => Mathf.Max(0, CurrentTuning.DraftRerollCharges + PersistentDraftRerollBonus);
        public int DraftRerollsRemaining => DraftSession.RerollsRemaining;
        public int DraftBanishesRemaining => DraftSession.BanishesRemaining;
        public int DraftSkipBloodShards => Mathf.Max(0, CurrentTuning.DraftSkipBloodShards);
        public float FirstKillTimeSeconds => Telemetry.FirstKillTimeSeconds;
        public float FirstExperiencePickupTimeSeconds => Telemetry.FirstExperiencePickupTimeSeconds;
        public float FirstLevelUpDraftTimeSeconds => Telemetry.FirstLevelUpDraftTimeSeconds;
        public float FirstEliteSpawnTimeSeconds => Telemetry.FirstEliteSpawnTimeSeconds;
        public float FirstEliteKillTimeSeconds => Telemetry.FirstEliteKillTimeSeconds;
        public float FirstMinibossSpawnTimeSeconds => Telemetry.FirstMinibossSpawnTimeSeconds;
        public float FirstMinibossKillTimeSeconds => Telemetry.FirstMinibossKillTimeSeconds;
        public float FirstBossSpawnTimeSeconds => Telemetry.FirstBossSpawnTimeSeconds;
        public float FirstBossKillTimeSeconds => Telemetry.FirstBossKillTimeSeconds;
        public float FirstEvolutionEligibilityTimeSeconds => Telemetry.FirstEvolutionEligibilityTimeSeconds;
        public float FirstEvolutionAcquiredTimeSeconds => Telemetry.FirstEvolutionAcquiredTimeSeconds;
        public float DamageTakenThisRun => PlayerVitals.DamageTaken;
        public int ThrottledExperienceOverflow => _experienceProgression.ThrottledExperienceOverflow;
        public int DraftOpenCount => DraftSession.OpenCount;

        void ISurvivorsUpgradeEffectSink.IncreaseMaximumHealth(double amount)
        {
            if (PlayerVitals.IsBound)
            {
                PlayerVitals.IncreaseMaximumHealth(amount);
            }
        }

        void ISurvivorsUpgradeEffectSink.RestoreBarrier(float amount) => PlayerVitals.RestoreBarrier(amount);

        void ISurvivorsUpgradeEffectSink.ScheduleMagnetPulse() => PickupCollection.ScheduleMagnetPulse();

        private void Awake()
        {
            EnsureUiTheme();
            if (tuning == null)
            {
                tuning = CreateConfiguredTuning(pacingProfile);
            }
            else
            {
                pacingProfile = tuning.PacingProfile;
            }
        }










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






        public bool SelectUiThemeForTest(int index)
        {
            return SelectUiTheme(index);
        }










        public bool SelectStandardRun()
        {
            return SelectRunMode(SurvivorsPacingProfile.HumanPlaytest);
        }

        public bool SelectSprintRun()
        {
            return SelectRunMode(SurvivorsPacingProfile.SprintRun);
        }



        public void RestartRun()
        {
            StartRun();
        }







        public int KillActiveHordeRushEnemiesForTest()
        {
            return DebugClearActiveHordeRush();
        }




        public SurvivorsPickupActor SpawnExperienceForTest(Vector3 position, int amount)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.Experience, position, amount);
        }

        public SurvivorsPickupActor SpawnMagnetForTest(Vector3 position)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.Magnet, position, 1);
        }

        public SurvivorsPickupActor SpawnHealthForTest(Vector3 position, int amount)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.Health, position, amount);
        }

        public SurvivorsPickupActor SpawnBloodShardForTest(Vector3 position, int amount)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.BloodShard, position, amount);
        }

        public void FireWeaponForTest()
        {
            EnsureRunStartedForTest();
            RunWeapons.FireForTest(SurvivorsWeaponArchetype.Projectile);
        }

        public bool FireWeaponForTest(SurvivorsWeaponArchetype archetype)
        {
            EnsureRunStartedForTest();
            return RunWeapons.FireForTest(archetype);
        }

        public bool DashForTest(Vector2 directionInput)
        {
            EnsureRunStartedForTest();
            return PlayerMotion.TryDash(directionInput);
        }

        public void ForceLevelUp()
        {
            EnsureRunStartedForTest();
            _experienceProgression.QueueDebugLevelUp();
            OpenLevelUpDraft();
        }

        public void DebugGrantExperience(int amount)
        {
            EnsureRunStartedForTest();
            GainExperience(Mathf.Max(1, amount));
        }

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


        public SurvivorsEnemyActor DebugSpawnElite(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.Elite, radius);
        }

        public SurvivorsEnemyActor DebugSpawnDreadElite(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.DreadElite, radius);
        }

        public SurvivorsEnemyActor DebugSpawnMiniboss(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.Miniboss, radius);
        }

        public SurvivorsEnemyActor DebugSpawnBoss(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.Boss, radius);
        }




        public int DebugTriggerHordeRush()
        {
            EnsureRunStartedForTest();
            return HordeRush.Trigger();
        }


        public void DebugApplyPacingProfile(SurvivorsPacingProfile profile)
        {
            ApplyPacingProfile(profile, restartRun: _runSession.Started);
        }

        public void ApplyPacingProfileForTest(SurvivorsPacingProfile profile, bool restartRun = false)
        {
            ApplyPacingProfile(profile, restartRun);
        }

        public void DebugResetMetaProgression()
        {
            ResetMetaProgressionForTest();
        }

        public void ForceLevelUpWithLockedChoiceForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            _experienceProgression.QueueDebugLevelUp();
            var lockedChoices = string.IsNullOrWhiteSpace(upgradeId)
                ? null
                : new[] { new RunUpgradeId(upgradeId) };
            OpenLevelUpDraft(lockedChoices);
        }

        public void KillPlayerForTest()
        {
            ApplyDamageToPlayer(MaxHealth + 1000f, "test.kill");
        }

        public void ConfigureMetaPersistenceForTest(IPersistenceService persistence, SaveSlotId slotId)
        {
            _profileSession.ConfigureBorrowedPersistence(persistence, slotId);
        }

        public bool TryPurchasePersistentUpgradeForTest(string id)
        {
            return TryPurchasePersistentUpgrade(id);
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

        public bool HasWeaponInLoadoutForTest(string weaponId)
        {
            EnsureRunStartedForTest();
            return RunWeapons.ContainsWeapon(weaponId);
        }

        public bool IsUpgradeAvailableInRunForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return RunBuild.Catalog != null && !string.IsNullOrWhiteSpace(upgradeId) && RunBuild.Catalog.TryGet(new RunUpgradeId(upgradeId), out _);
        }

        private Vector3 ResolveClosestArenaLandmarkPositionForTest()
        {
            return TryResolveClosestArenaLandmark(ignoreDiscovered: false, out Vector3 closest, out _, out _)
                ? closest
                : Vector3.zero;
        }

        public bool IsUpgradeEligibleInCurrentBuildForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return TryGetRunUpgrade(upgradeId, out RunUpgradeDefinition upgrade) && IsUpgradeEligibleForCurrentBuild(upgrade);
        }

        public SurvivorsRunUpgradeCategory GetUpgradeCategoryForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return TryGetRunUpgrade(upgradeId, out RunUpgradeDefinition upgrade)
                ? ResolveCurrentUpgradeCategory(upgrade)
                : SurvivorsRunUpgradeCategory.WeaponUpgrade;
        }

        public string GetUpgradeDescriptionForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return TryGetUpgradeMetadata(upgradeId, out SurvivorsRunUpgradeMetadata metadata)
                ? metadata.Description
                : ResolveUpgradeDisplayName(new RunUpgradeId(upgradeId));
        }

        public string GetCurrentDraftChoiceLabelForTest(int index)
        {
            EnsureRunStartedForTest();
            if (DraftSession.CurrentDraft == null || index < 0 || index >= DraftSession.CurrentDraft.Choices.Count)
            {
                return string.Empty;
            }

            return FormatUpgradeChoiceLabel(index, DraftSession.CurrentDraft.Choices[index]);
        }

        public string GetCurrentDraftCardSummaryForTest(int index)
        {
            EnsureRunStartedForTest();
            if (IsRelicChoiceOpen)
            {
                if (DraftSession.CurrentRelicDraft == null || index < 0 || index >= DraftSession.CurrentRelicDraft.Choices.Count)
                {
                    return string.Empty;
                }

                return CreateRelicDraftCard(index, DraftSession.CurrentRelicDraft.Choices[index]).Summary;
            }

            if (DraftSession.CurrentDraft == null || index < 0 || index >= DraftSession.CurrentDraft.Choices.Count)
            {
                return string.Empty;
            }

            return CreateUpgradeDraftCard(index, DraftSession.CurrentDraft.Choices[index]).Summary;
        }

        public int GetRunUpgradeRankForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return string.IsNullOrWhiteSpace(upgradeId) ? 0 : RunBuild.State.GetRank(new RunUpgradeId(upgradeId));
        }

        public int GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity rarity)
        {
            EnsureRunStartedForTest();
            return DraftRarity.ResolveDraftRarityWeight(DraftRarityProfile.NormalMid, rarity);
        }

        public bool HasEvolvedUpgradeForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return RunBuild.HasEvolution(upgradeId);
        }

        internal bool IsEvolutionActive(string upgradeId)
        {
            return RunBuild.HasEvolution(upgradeId);
        }


        public IReadOnlyList<string> CurrentBuildHudLinesForTest()
        {
            EnsureRunStartedForTest();
            return ResolveBuildHudSummaryLines();
        }

        public IReadOnlyList<string> CurrentBuildMenuLinesForTest()
        {
            EnsureRunStartedForTest();
            return ResolveBuildMenuLines(Menus.BuildTab);
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

        public IReadOnlyList<string> CurrentTutorialLinesForTest()
        {
            return ResolveTutorialStepLines(SurvivorsTutorialContent.ClampTutorialStepIndex(Menus.TutorialIndex));
        }

        public void ToggleDebugOverlayForTest()
        {
            _debugOverlayVisible = !_debugOverlayVisible;
        }

        public void SetBuildMenuOpenForTest(bool open)
        {
            Menus.BuildOpen = open && _runSession.Started && State == SurvivorsRunState.Playing;
        }

        public void SetBuildMenuTabForTest(int tabIndex)
        {
            Menus.BuildTab = SurvivorsMenuSession.ClampBuildMenuTab(tabIndex);
        }

        public void SetAudioMutedForTest(bool muted)
        {
            _audioEvents.SetMuted(muted);
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

        public bool DispatchAudioEventForTest(string eventId)
        {
            return PlayAudioEvent(eventId, _pickupClip, 0f);
        }

        public Rect ResolveCenteredPanelRectForTest(float maxWidth, float maxHeight, float minWidth, float minHeight, float margin)
        {
            return SurvivorsScreenLayout.ResolveCenteredPanelRect(maxWidth, maxHeight, minWidth, minHeight, margin);
        }







        private void ResetRunMetrics()
        {
            DraftSession.ResetOpenCount();
            Telemetry.Reset();
            PlayerVitals.ResetDamageTaken();
            _runMetricsReadModel?.Clear();
        }



        private void TickLevelUpDraftCooldown(float deltaTime)
        {
            _experienceProgression.Tick(deltaTime, CurrentTuning);
            TryOpenPendingLevelUpDraft();
        }

        public bool OpenBossRelicDraftForTest()
        {
            EnsureRunStartedForTest();
            return OpenBossRelicDraft();
        }

        public bool SelectRelicForTest(int index)
        {
            return SelectRelic(index);
        }

        public bool SelectDraftHotkeyForTest(int hotkeyNumber)
        {
            return SelectUpgrade(hotkeyNumber - 1);
        }

        public void TriggerMagnetRecall()
        {
            StartMagnetRecall();
        }
















        internal IReadOnlyList<SurvivorsEnemyActor> ActiveEnemies => _enemies;

        internal Transform RuntimeWorldRoot => _worldRoot;

































        private int CountAuthoredThreatLifeBars(bool showBossLifeBar) => ThreatHud.CountLifeBars(showBossLifeBar);

        private string ResolveActiveMajorThreatLifeBarSummary() => ThreatHud.LifeBarSummary();

        private static string ResolveMajorThreatHealthFallbackLabel(SurvivorsEnemyRole role) => SurvivorsThreatHudModel.ResolveFallbackLabel(role);












        private void EnsureRunStartedForTest()
        {
            if (!_runSession.Started)
            {
                StartRun();
            }
        }











        private bool TryDropHealthPickup(Vector3 position)
        {
            if (!PlayerVitals.IsBound || CurrentTuning.HealthPickupHealAmount <= 0 || CurrentHealth >= MaxHealth - 0.01f)
            {
                return false;
            }

            return SpawnPickup(SurvivorsPickupKind.Health, position, CurrentTuning.HealthPickupHealAmount) != null;
        }



        internal Color ResolveProjectileFallbackColor()
        {
            return ActiveUiTheme.GetFeedbackAccentColor(new Color(0.78f, 0.42f, 1f));
        }

        internal Color ResolveProjectileTrailEndColor()
        {
            return WithAlpha(ActiveUiTheme.GetArenaAccentColor(new Color(0.18f, 0.86f, 1f)), 0f);
        }

        private static void SetRendererColor(Renderer renderer, Color color) => SurvivorsPrimitivePresentation.SetRendererColor(renderer, color);





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


        private void ClearRun()
        {
            _runSession.Stop();
            _audioPresentation?.ReleaseResources();
            _arena?.Dispose();
            _rewardDrops?.Dispose();
            _threatTelegraphs?.Dispose();
            _combatFeedback?.Dispose();
            _runWeapons?.DisposeLoadout();

            _runFlow = null;
            _enemies.Clear();
            HordeRush.ClearMembers();
            RoamingCaches.ClearMembers();
            ShrineTrials.ClearMembers();
            MajorThreatAbilities.ClearMembers();
            _pickups.Clear();
            _projectiles.Clear();
            Waystones.ClearDiscoveries();
            RunBuild.ClearOwnedSelections();
            _feedbackPulses?.Dispose();
            _runtimeWorld?.ReleaseSpawns();
            _runtimeWorld?.Dispose();
            _runtimeWorld = null;
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

        private static bool IsRunUpgradeSelectionKind(SurvivorsRewardSelectionKind selectionKind)
        {
            return selectionKind == SurvivorsRewardSelectionKind.LevelUp ||
                selectionKind == SurvivorsRewardSelectionKind.EliteUpgrade ||
                selectionKind == SurvivorsRewardSelectionKind.BossUpgrade;
        }

        private static bool IsRewardUpgradeSelectionKind(SurvivorsRewardSelectionKind selectionKind)
        {
            return selectionKind == SurvivorsRewardSelectionKind.EliteUpgrade ||
                selectionKind == SurvivorsRewardSelectionKind.BossUpgrade;
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

        private void ApplySelectedClassBonuses()
        {
            UpgradeModifiers.ApplyClass(_selectedClass);
        }












        private void ReleaseMetaProgressionService()
        {
            _profileSession.Release();
        }

        private void RecordRoamingArenaTravel(Vector3 delta) => Traversal.RecordTravel(delta, State == SurvivorsRunState.Playing);







        private float ResolveEnemySpawnIntervalSeconds() =>
            SurvivorsSwarmSpawnCoordinator.ResolveInterval(CurrentTuning, _runFlow, _runSession.HasClearedVictory);


        private int ResolveEnemyMaximumAlive() =>
            SurvivorsSwarmSpawnCoordinator.ResolveMaximumAlive(CurrentTuning, _runFlow, _runSession.HasClearedVictory);

        private int ResolveEnemySpawnPackSize() => SurvivorsSwarmSpawnCoordinator.ResolvePackSize(CurrentTuning, _runFlow);




        private void TickEnemySpawning(float deltaTime)
        {
            SwarmSpawning.Tick(deltaTime, RunTimeSeconds, CurrentTuning, _runFlow, _runSession.HasClearedVictory);
        }










        private void TickWeapon(float deltaTime)
        {
            RunWeapons.Tick(deltaTime);
        }










        private bool TryResolveCameraGroundRect(float padding, out Rect rect)
        {
            return SurvivorsCameraGroundProjection.TryResolve(_camera != null ? _camera : Camera.main, PlayerPosition.y, padding, out rect);
        }

        private int CountOffscreenMajorThreatMarkers() => ThreatHud.CountMarkers(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);

        private string ResolveFirstOffscreenMajorThreatMarkerLabel() => ThreatHud.MarkerLabel(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);

        private void UpdateOffscreenThreatMarkerSnapshot() => ThreatHud.UpdateLastMarker(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);



        private int GainExperience(int amount)
        {
            int gained = _experienceProgression.Gain(amount,
                ExperienceGainMultiplierBonus + PassiveLoadoutSurgeExperienceGainMultiplierBonus, CurrentTuning);
            TryOpenPendingLevelUpDraft();
            return gained;
        }

        private void ResolveLevelUpsFromExperienceBudget()
        {
            _experienceProgression.ResolveBudget(CurrentTuning);
        }

        private void ApplyPacingProfile(SurvivorsPacingProfile profile, bool restartRun)
        {
            tuning = CreateConfiguredTuning(profile);
            pacingProfile = tuning.PacingProfile;
            if (restartRun)
            {
                RestartRun();
            }
        }












        private void ApplyRelic(SurvivorsRelicDefinition relic)
        {
            UpgradeModifiers.ApplyRelic(relic);
        }

        private void ApplyUpgrade(RunUpgradeDefinition upgrade)
        {
            UpgradeModifiers.Apply(upgrade);
            RecordRunBuildSelection(upgrade);
            RecordNewlyEligibleEvolutionFeedback();
        }

        private SurvivorsUiTheme ActiveUiTheme => UiThemeSelection.ActiveTheme;


        private void ResetHudStyles()
        {
            _hudStyles.Reset();
            _damageFeedback.ResetStyles();
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





        private Color ResolveRewardTitleAccentColor()
        {
            if (IsRelicChoiceOpen)
            {
                return ActiveUiTheme.GetRarityAccentColor("Relic", new Color(1f, 0.84f, 0.42f));
            }

            if (DraftSession.CurrentDraft == null || DraftSession.CurrentDraft.Choices.Count == 0)
            {
                return ActiveUiTheme.GetRarityAccentColor("Common", Color.white);
            }

            for (int i = 0; i < DraftSession.CurrentDraft.Choices.Count; i++)
            {
                RunUpgradeDefinition choice = DraftSession.CurrentDraft.Choices[i];
                if (choice != null && ResolveCurrentUpgradeCategory(choice) == SurvivorsRunUpgradeCategory.Evolution)
                {
                    return ActiveUiTheme.GetRarityAccentColor("Evolution", new Color(1f, 0.38f, 0.56f));
                }
            }

            RunUpgradeRarity rarity = ResolveHighestRarity(DraftSession.CurrentDraft.Choices);
            return ActiveUiTheme.GetRarityAccentColor(rarity, ResolveRarityAccentColor(rarity));
        }









        private static Color ResolveRewardButtonBackgroundColor(Color accent)
        {
            return new Color(
                Mathf.Lerp(1f, accent.r, 0.28f),
                Mathf.Lerp(1f, accent.g, 0.28f),
                Mathf.Lerp(1f, accent.b, 0.28f),
                1f);
        }

        private static void DrawRewardChoiceCard(Rect rect, Color accent)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(0.015f, 0.02f, 0.028f, 0.82f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.22f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, rect.height), Texture2D.whiteTexture);
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.96f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 6f, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        private string ResolveRewardOverlayTitle()
        {
            if (DraftSession.Kind == SurvivorsRewardSelectionKind.BossRelic)
            {
                return "Choose a Boss Relic";
            }

            if (DraftSession.Kind == SurvivorsRewardSelectionKind.EliteUpgrade)
            {
                return "Elite Reward";
            }

            if (DraftSession.Kind == SurvivorsRewardSelectionKind.BossUpgrade)
            {
                return "Boss Evolution Reward";
            }

            return "Level Up";
        }

        private void EnsureHudStyles() => _hudStyles.Ensure();





        private void DrawLowHealthWarning() => SurvivorsStatusHudPresenter.DrawLowHealth(IsLowHealthWarningActive, _lowHealthStyle);

        private void DrawMajorThreatWarning() => SurvivorsStatusHudPresenter.DrawMajorThreat(
            IsMajorThreatWarningActive, IsTopCenterTimerVisible, CurrentMajorThreatWarningLabel, MajorThreatWarningRemainingSeconds, _majorThreatWarningStyle);

        private void DrawHordeRushWarning() => SurvivorsStatusHudPresenter.DrawHordeRush(
            IsHordeRushWarningActive, IsTopCenterTimerVisible, IsMajorThreatWarningActive, CurrentHordeRushWarningLabel, HordeRushWarningRemainingSeconds, _majorThreatWarningStyle);

        private void DrawMajorThreatHealthBar() => SurvivorsThreatHudPresenter.DrawLifeBars(ThreatHud, _camera,
            IsMajorThreatWarningActive, IsHordeRushWarningActive, ResolveMajorRewardDropColor, _hudSmallStyle);

        private void DrawOffscreenThreatMarker()
        {
            if (State != SurvivorsRunState.Playing) return;
            SurvivorsThreatHudItem? selected = ThreatHud.SelectMarker(PlayerPosition, CurrentTuning.OffscreenThreatMarkerDistance);
            if (selected.HasValue)
            {
                SurvivorsThreatHudPresenter.DrawMarker(selected, PlayerPosition, ResolveMajorRewardDropColor(selected.Value.Role), _hudSmallStyle);
            }
        }

        private void DrawRewardSelectionFeedback() => _rewardBanner.Draw(_rewardFeedbackStyle);

        private void DrawStreakRewardFeedback() => _streakRewardBanner.Draw(_rewardFeedbackStyle);

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

        private void TickStreakRewardFeedback(float deltaTime) => _streakRewardBanner.Tick(deltaTime);

        private void TickClassUnlockRewardFeedback(float deltaTime) => _classUnlockRewardBanner.Tick(deltaTime);

        private void TickEvolutionReadyFeedback(float deltaTime) => _evolutionReadyBanner.Tick(deltaTime);


















        private static string ResolveCompassDirectionLabel(Vector3 delta) => SurvivorsThreatHudModel.CompassDirection(delta);





        private int ResolveTotalRelicCount()
        {
            return _relicDefinitions == null ? 0 : _relicDefinitions.Count;
        }














        private static Material ApplyColor(Renderer renderer, Color color) => SurvivorsPrimitivePresentation.ApplyColor(renderer, color);

        private static Color WithAlpha(Color color, float alpha) => SurvivorsPrimitivePresentation.WithAlpha(color, alpha);

        private static void ReleaseTemplateObject(UnityEngine.Object target)
        {
            UnityObjectUtility.DestroySafely(target);
        }

        private void OnDestroy()
        {
            ClearRun();
            ReleaseMetaProgressionService();
            _runtimeCamera?.Dispose();
            _runtimeCamera = null;
        }

    }
}
