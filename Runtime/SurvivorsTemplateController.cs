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
    // Serialized configuration and owner references retain their original initialization order. Unity messages delegate to the composed lifecycle and presentation owners.
    public sealed partial class SurvivorsTemplateController :
        MonoBehaviour,
        ISurvivorsUpgradeEffectSink,
        ISurvivorsSwarmSpawnPort,
        ISurvivorsTimedEncounterPort,
        ISurvivorsHordeRushPort,
        ISurvivorsTraversalPort,
        ISurvivorsExplorationPort,
        ISurvivorsPlayerDamagePort,
        ISurvivorsPlayerMotionPort,
        ISurvivorsRunBuildPort,
        ISurvivorsDraftSessionPort,
        ISurvivorsTutorialPort,
        ISurvivorsRunModePort,
        ISurvivorsRunResultPort,
        ISurvivorsStreakRewardPort,
        ISurvivorsEnemyNavigationPort,
        ISurvivorsBuildSurgePort,
        ISurvivorsPersistentProgressionPort,
        ISurvivorsRunRewardPort,
        ISurvivorsPickupRewardPort,
        ISurvivorsContentBindingPort,
        ISurvivorsEnemyDefeatPort,
        ISurvivorsMajorRewardPickupCachePort,
        ISurvivorsPickupCollectionPort,
        ISurvivorsDamageAugmentPort,
        ISurvivorsMajorThreatAbilityPort,
        ISurvivorsEnemySupportSpawnPort,
        ISurvivorsFrameInputPort,
        ISurvivorsEnemySpawnPort,
        ISurvivorsPickupSpawnPort,
        ISurvivorsProjectileLaunchPort,
        ISurvivorsSpawnSafetyPort,
        ISurvivorsUiThemeSelectionPort,
        ISurvivorsRunLifecyclePort,
        ISurvivorsHudRenderPort,
        ISurvivorsRewardFeedbackPort,
        ISurvivorsDamageFeedbackPort,
        ISurvivorsRangedDodgePort,
        ISurvivorsRunWeaponPort,
        ISurvivorsProgressionFeedbackPort,
        ISurvivorsDebugWorldPort,
        ISurvivorsRunMetricsReadPort,
        ISurvivorsActiveRunMetricsReadPort,
        ISurvivorsOrbitKnockbackPort,
        ISurvivorsDebugDraftPort,
        ISurvivorsActorMembershipPort,
        ISurvivorsDraftFeedbackPort,
        ISurvivorsPlayerStatReadPort,
        ISurvivorsHealthPickupDropPort,
        ISurvivorsDraftHeaderReadPort
    {
        private SurvivorsBuildContentLabels _buildContentLabels;

        private SurvivorsBuildHudModel _buildHudModel;

        private SurvivorsBuildHudPresenter _buildHudPresenter;

        private SurvivorsPersistentProgression _persistentProgression;

        private SurvivorsRunRewards _runRewards;

        private SurvivorsEndlessSurgeRewards _endlessSurges;

        private SurvivorsDraftSelectionRewards _selectionRewards;

        private SurvivorsRuntimeWorld _runtimeWorld;

        private SurvivorsRuntimeCamera _runtimeCamera;

        private void LateUpdate()
        {
            UpdateArenaPresentation();
            _runtimeCamera?.Follow(_playerObject == null ? null : _playerObject.transform, Time.deltaTime);
        }

        private SurvivorsContentBinding _contentBinding;

        private SurvivorsRuntimeContentResolver _runtimeContent;

        private SurvivorsEnemyDefeatFlow _defeats;

        private SurvivorsRunSummaryModel _runSummary;

        private SurvivorsEvolutionHudModel _evolutionHud;

        private SurvivorsMajorRewardPickupCache _majorRewardPickupCache;

        private SurvivorsPickupCollection _pickupCollection;

        private SurvivorsDamageAugments _damageAugments;

        private SurvivorsEnemyDamage _enemyDamage;

        private SurvivorsPayloadHazardRewards _payloadHazards;

        private SurvivorsDeathNova _deathNova;

        private readonly SurvivorsRunTelemetry Telemetry = new SurvivorsRunTelemetry();

        private SurvivorsEnemySupportSpawning _enemySupportSpawning;

        private SurvivorsMajorThreatAbilities _majorThreatAbilities;

        private SurvivorsFrameInput _frameInput;

        private void Update() => FrameInput.Tick(Time.deltaTime);

        private SurvivorsActorSimulation _actorSimulation;

        private SurvivorsRunSimulation _simulation;

        private SurvivorsEnemySpawner _enemySpawner;

        private SurvivorsPickupSpawner _pickupSpawner;

        private SurvivorsProjectileLauncher _projectileLauncher;

        private SurvivorsSpawnSafety _spawnSafety;

        private SurvivorsEnemyRosterQueries _enemyRosterQueries;

        private SurvivorsFeedbackPulses _feedbackPulses;

        private SurvivorsUiThemeSelection _uiThemeSelection;

        private SurvivorsEvolutionAnnouncements _evolutionAnnouncements;

        private SurvivorsRunLifecycle _lifecycle;

        private void Start() => Lifecycle.StartAutomatically(showRunModeSelection, autoStart);

        private SurvivorsHudDispatch _hudDispatch;

        private void OnGUI() => HudDispatch.Draw();

        private SurvivorsRewardFeedbackHistory _rewardHistory;

        private SurvivorsCompactHudLabels _compactHudLabels;

        private SurvivorsDamageFeedback _hitFeedback;

        private SurvivorsRangedDodgeRewards _rangedDodgeRewards;

        private SurvivorsStreakFeedbackHistory _streakFeedback;

        private SurvivorsRunWeapons _runWeapons;

        private SurvivorsProgressionFeedback _progressionFeedback;

        private SurvivorsDebugWorldCommands _debugWorld;

        private SurvivorsRunMetricsReadModel _runMetricsReadModel;

        private readonly SurvivorsWeaponDiagnostics _weaponDiagnostics = new SurvivorsWeaponDiagnostics();

        private SurvivorsOrbitKnockback _orbitKnockback;

        private SurvivorsDebugUpgradeFormatter _debugUpgradeFormatter;

        private SurvivorsDebugDraftModel _debugDraftModel;

        private SurvivorsDebugBuildModel _debugBuildModel;

        private SurvivorsActorMembership _actorMembership;

        private SurvivorsAreaDamage _areaDamage;

        private SurvivorsDraftFeedback _draftFeedback;

        private SurvivorsPlayerStats _playerStats;

        private SurvivorsHealthPickupDrops _healthPickupDrops;

        private SurvivorsDraftHeader _draftHeader;

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

        private readonly SurvivorsFeedbackBannerPresenter _rewardBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Reward);

        private readonly SurvivorsFeedbackBannerPresenter _streakRewardBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Streak);

        private readonly SurvivorsFeedbackBannerPresenter _classUnlockRewardBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.ClassUnlock);

        private readonly SurvivorsFeedbackBannerPresenter _evolutionReadyBanner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Evolution);

        private SurvivorsCombatFeedbackPresenter _combatFeedback;

        private SurvivorsThreatTelegraphPresenter _threatTelegraphs;

        private SurvivorsRewardDropPresenter _rewardDrops;

        private readonly SurvivorsDamagePopupPresenter _damageFeedback = new SurvivorsDamagePopupPresenter();

        private SurvivorsBuildSurgeRewards _buildSurges;

        private SurvivorsDraftCardFactory _draftCardFactory;

        private SurvivorsEnemySpatialQueries _enemySpatialQueries;

        private SurvivorsEnemyNavigation _enemyNavigation;

        private SurvivorsKillStreakRewards _killStreakRewards;

        private SurvivorsExperienceComboRewards _experienceRhythm;

        private SurvivorsRunResultPresenter _resultScreen;

        private SurvivorsDraftScreenPresenter _draftScreen;

        private SurvivorsRunModePresenter _runModePresenter;

        void ISurvivorsRunModePort.Start(SurvivorsPacingProfile profile) => SelectRunMode(profile);

        private SurvivorsMenuSession _menus;

        private SurvivorsBuildMenuPresenter _buildMenuPresenter;

        private SurvivorsTutorialPresenter _tutorialPresenter;

        private SurvivorsRelicInventory _relicInventory;

        private SurvivorsDraftSession _draftSession;

        private SurvivorsDraftOfferGenerator _draftOffers;

        private SurvivorsRunBuildState _runBuild;

        private SurvivorsDraftRarityPolicy _draftRarity;

        private SurvivorsPlayerVitals _playerVitals;

        private SurvivorsPlayerMotion _playerMotion;

        private SurvivorsArenaPresenter _arena;

        private SurvivorsRoamingCacheEncounter _roamingCaches;

        private SurvivorsShrineEncounter _shrineTrials;

        private SurvivorsWaystoneExploration _waystones;

        private SurvivorsExplorationFeedback _explorationFeedback;

        private SurvivorsHordeRushEncounter _hordeRush;

        private readonly SurvivorsHudStyles _hudStyles = new SurvivorsHudStyles();

        private SurvivorsSpawnPoseResolver _poseResolver;

        private SurvivorsRunFlowRuntime _runFlow;

        private IReadOnlyList<SurvivorsRelicDefinition> _relicDefinitions;

        private IReadOnlyList<SurvivorsClassUpgradeGateDefinition> _upgradeClassGates;

        private SurvivorsClassLibraryDefinition _classLibrary;

        private SurvivorsClassDefinition _selectedClass;

        private SurvivorsTimedEncounterDirector _timedEncounters;

        private SurvivorsSwarmSpawnCoordinator _swarmSpawning;

        private SurvivorsTraversalDirector _traversal;

        private readonly SurvivorsSpawnSequence SpawnSequence = new SurvivorsSpawnSequence();

        private SurvivorsUpgradeModifiers _upgradeModifiers;

        private readonly SurvivorsRunSession _runSession = new SurvivorsRunSession();

        private readonly SurvivorsExperienceProgression _experienceProgression = new SurvivorsExperienceProgression();

        private SurvivorsAudioPresenter _audioPresentation;

        private readonly SurvivorsProfileSession _profileSession = new SurvivorsProfileSession(
            () => new PersistenceService(new FileTextStorage(new UnityPersistentDataPathProvider())));

        private SurvivorsThreatHudModel _threatHud;

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

        private void OnDestroy()
        {
            ClearRun();
            ReleaseMetaProgressionService();
            _runtimeCamera?.Dispose();
            _runtimeCamera = null;
        }
    }
}
