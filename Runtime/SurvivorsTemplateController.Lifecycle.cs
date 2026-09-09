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
    // Ordered run admission, initialization, reset, frame and cleanup bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private void RecordLevelCheckpoints() => Telemetry.RecordLevelCheckpoints(RunTimeSeconds, Level);

        private SurvivorsFrameInput FrameInput => _frameInput ?? (_frameInput = new SurvivorsFrameInput(_runSession, Menus, new SurvivorsUnityKeyboard(), this));

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

        private SurvivorsActorSimulation ActorSimulation => _actorSimulation ?? (_actorSimulation = new SurvivorsActorSimulation(_enemies, _projectiles, _pickups, TryUpdateEnemyLeash,
            () => new SurvivorsPickupAttractionValues(CurrentPickupAttractRange, CurrentPickupAttractionSpeed, CurrentTuning.PickupCollectRadius)));

        private void TickEnemies(float deltaTime) => ActorSimulation.TickEnemies(deltaTime);

        private void TickProjectiles(float deltaTime) => ActorSimulation.TickProjectiles(deltaTime);

        private void TickPickups(float deltaTime) => ActorSimulation.TickPickups(deltaTime);

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
                frame => KillStreakRewards.TickKillStreak(frame.DeltaTime),
                frame => KillStreakRewards.TickStreakSurge(frame.DeltaTime),
                frame => RoamingCaches.TickRoamingCacheSurge(frame.DeltaTime),
                frame => ShrineTrials.TickArenaShrineSurge(frame.DeltaTime),
                frame => Waystones.TickWaystoneFocus(frame.DeltaTime),
                frame => Waystones.TickWaystoneChainSurge(frame.DeltaTime),
                frame => HordeRush.TickHordeRushClearSurge(frame.DeltaTime),
                frame => BuildSurges.TickWeaponLoadoutSurge(frame.DeltaTime),
                frame => BuildSurges.TickPassiveLoadoutSurge(frame.DeltaTime),
                frame => BuildSurges.TickBossRelicSurge(frame.DeltaTime),
                frame => TickPayloadHazardChain(frame.DeltaTime),
                frame => ExperienceRhythm.TickGemRush(frame.DeltaTime),
                frame => TickPickupMagnetPulse(frame.DeltaTime),
                frame => BuildSurges.TickEvolutionChainSurge(frame.DeltaTime),
                frame => EndlessSurges.TickEndlessSurge(frame.DeltaTime),
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

        private SurvivorsRunLifecycle Lifecycle => _lifecycle ?? (_lifecycle = new SurvivorsRunLifecycle(_runSession, Menus, this));

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

        void ISurvivorsRunLifecyclePort.ClearDrafts() => DraftSession.Clear();

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

        private bool _debugOverlayVisible { get => FrameInput.DebugVisible; set => FrameInput.DebugVisible = value; }

        public SurvivorsRunState State => _runSession.State;

        public int Level => _experienceProgression.Level;

        public int PendingLevelUps => _experienceProgression.PendingLevelUps;

        public float RunTimeSeconds => _runSession.ElapsedSeconds;

        public string TopCenterTimerHudLabel => _runSession.Started ? ResolveTopCenterTimerHudLabel() : string.Empty;

        public bool IsTopCenterTimerVisible => _runSession.Started;

        public bool IsPlaying => State == SurvivorsRunState.Playing;

        public bool IsRunStarted => _runSession.Started;

        public bool IsLevelUpOpen => State == SurvivorsRunState.LevelUp;

        public bool IsGameOver => State == SurvivorsRunState.GameOver;

        public bool IsVictory => State == SurvivorsRunState.Victory;

        public bool HasClearedVictoryThisRun => _runSession.HasClearedVictory;

        public bool IsEndlessRun => State == SurvivorsRunState.Playing && _runSession.HasClearedVictory;

        public void RestartRun()
        {
            StartRun();
        }

        public void ForceLevelUp()
        {
            EnsureRunStartedForTest();
            _experienceProgression.QueueDebugLevelUp();
            DraftSession.OpenLevelUp();
        }

        public void DebugApplyPacingProfile(SurvivorsPacingProfile profile)
        {
            ApplyPacingProfile(profile, restartRun: _runSession.Started);
        }

        public void ForceLevelUpWithLockedChoiceForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            _experienceProgression.QueueDebugLevelUp();
            var lockedChoices = string.IsNullOrWhiteSpace(upgradeId)
                ? null
                : new[] { new RunUpgradeId(upgradeId) };
            DraftSession.OpenLevelUp(lockedChoices);
        }

        private void EnsureRunStartedForTest()
        {
            if (!_runSession.Started)
            {
                StartRun();
            }
        }

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
    }
}
