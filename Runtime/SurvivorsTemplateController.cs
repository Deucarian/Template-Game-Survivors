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
    public sealed class SurvivorsTemplateController : MonoBehaviour
    {
        private const string FeedbackRootName = "Survivors Feedback Presentation";
        private const string SpawnPulseName = "Survivors Spawn Pulse";
        private const string FirePulseName = "Survivors Weapon Fire Pulse";
        private const string KillPulseName = "Survivors Kill Burst";
        private const string PickupPulseName = "Survivors Pickup Pulse";
        private const string LevelUpPulseName = "Survivors Level Up Pulse";
        private const string BossPulseName = "Survivors Boss Cue Pulse";
        private const string FeedbackAudioName = "Survivors Feedback Audio";
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
        private const string DirectionalLightName = "Survivors Directional Light";
        private const float InfiniteArenaTileSize = 12f;
        private const int InfiniteArenaGridRadius = 2;
        private const int InfiniteArenaLandmarkCount = 8;
        private const float InfiniteArenaLandmarkJitterRadius = 2.35f;
        private const float KillStreakWindowSeconds = 3.8f;
        private const int KillStreakExperienceInterval = 8;
        private const int KillStreakHealthInterval = 16;
        private const int KillStreakMagnetInterval = 24;
        private const int KillStreakBloodShardInterval = 32;
        private const int KillStreakSurgeInterval = 16;
        private const int KillStreakSurgeMaxTier = 5;
        private const float StreakSurgeDurationSeconds = 6f;
        private const float StreakSurgeDamageBonusPerTier = 1.1f;
        private const float StreakSurgeMoveSpeedBonusPerTier = 0.16f;
        private const float StreakSurgeCooldownReductionPerTier = 0.025f;
        private const float StreakSurgePickupRangeBonusPerTier = 0.18f;
        private const int DefaultMaxWeaponSlots = 6;
        private const int DefaultMaxPassiveSlots = 6;
        private const float DamagePopupLifetimeSeconds = 0.9f;
        private const float DamagePopupRiseHeight = 1.25f;
        private const int DamagePopupLimit = 72;
        private const float LowHealthWarningThreshold = 0.3f;
        private const float RewardFeedbackDurationSeconds = 2.35f;
        private const float StreakRewardFeedbackDurationSeconds = 1.8f;
        private const float ClassUnlockRewardFeedbackDurationSeconds = 2.65f;
        private const float EvolutionReadyFeedbackDurationSeconds = 2.4f;
        private const float FrostFanEnemyMoveSpeedMultiplier = 0.68f;
        private const float FrostFanEnemySlowDurationSeconds = 1.65f;
        private const float BlizzardCrownEnemyMoveSpeedMultiplier = 0.52f;
        private const float BlizzardCrownEnemySlowDurationSeconds = 2.35f;
        private const float CinderBurstBurnDamageRatio = 0.26f;
        private const float InfernoHeartBurnDamageRatio = 0.42f;
        private const float InfernoHeartBurnDurationMultiplier = 1.45f;
        private const float ExperienceComboWindowSeconds = 0.85f;
        private const float ExperienceComboFeedbackDurationSeconds = 1.35f;
        private const int ExperienceComboMinimumPickupCount = 3;
        private const float EnemyHitFlashSeconds = 0.13f;
        private const float EnemyDeathEffectLifetimeSeconds = 0.42f;
        private const int EnemyDeathEffectLimit = 56;
        private const float BaseDeathNovaRadius = 1.65f;
        private const float MajorRewardDropLifetimeSeconds = 1.25f;
        private const int MajorRewardDropEffectLimit = 8;
        private const float MajorRewardPickupCacheRadiusPadding = 0.55f;
        private const int MajorThreatSlamTelegraphEffectLimit = 12;
        private const float MajorThreatSlamTelegraphFadePaddingSeconds = 0.12f;
        private const int IncomingThreatTelegraphEffectLimit = 6;
        private const float IncomingThreatTelegraphFadePaddingSeconds = 0.18f;
        private const float EnemyRangedAttackFeedbackLifetimeSeconds = 0.34f;
        private const int EnemyRangedAttackFeedbackLimit = 28;
        private const float EndlessSpawnIntervalMultiplier = 0.82f;
        private const int EndlessEnemyAliveBonus = 24;
        private const int NormalDraftRarityLockSeedSalt = 7919;
        private const int EarlyPassiveDraftLockSeedSalt = 12289;
        private const int EvolutionPrimerPassiveDraftLockSeedSalt = 17443;
        private const int RewardDraftRarityLockSeedSalt = 23831;
        private const int ResultMetaUpgradeOptionCount = 3;
        private const int ResultClassOptionCount = 4;

        private enum DraftRarityProfile
        {
            NormalEarly = 0,
            NormalMid = 1,
            NormalLate = 2,
            Elite = 3,
            Boss = 4
        }

        private enum BuildMenuTab
        {
            CurrentBuild = 0,
            Stats = 1,
            RunInfo = 2,
            Controls = 3
        }

        private enum TutorialStep
        {
            Movement = 0,
            Combat = 1,
            Experience = 2,
            Drafts = 3,
            ElitesBosses = 4,
            Evolutions = 5,
            Modes = 6
        }

        private struct DraftCardPresentation
        {
            public int Index;
            public string Hotkey;
            public string Name;
            public string RarityLabel;
            public string CategoryId;
            public string CategoryLabel;
            public string AffectedLabel;
            public string RankLabel;
            public string Description;
            public string EffectPreview;
            public string RequirementHint;
            public string IconId;
            public string StyleToken;
            public bool IsEvolution;
            public Color AccentColor;

            public string Summary
            {
                get
                {
                    return $"{Hotkey} | {Name} | {RarityLabel} | {CategoryLabel} | {AffectedLabel} | {RankLabel} | {Description} | Preview: {EffectPreview} | {RequirementHint}";
                }
            }
        }

        private static readonly RunUpgradeDefinition[] EmptyChoices = Array.Empty<RunUpgradeDefinition>();
        private static readonly SurvivorsRelicDefinition[] EmptyRelicChoices = Array.Empty<SurvivorsRelicDefinition>();
        private static readonly string[] EmptyWeaponIds = Array.Empty<string>();

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

        private readonly List<SurvivorsEnemyActor> _enemies = new List<SurvivorsEnemyActor>(64);
        private readonly List<SurvivorsPickupActor> _pickups = new List<SurvivorsPickupActor>(128);
        private readonly List<SurvivorsProjectileActor> _projectiles = new List<SurvivorsProjectileActor>(64);
        private readonly List<Transform> _arenaTiles = new List<Transform>(25);
        private readonly List<Transform> _arenaLandmarks = new List<Transform>(InfiniteArenaLandmarkCount);
        private readonly List<long> _arenaLandmarkKeys = new List<long>(InfiniteArenaLandmarkCount);
        private readonly List<SurvivorsDamagePopup> _damagePopups = new List<SurvivorsDamagePopup>(DamagePopupLimit);
        private readonly List<SurvivorsWorldFeedbackEffect> _worldFeedbackEffects = new List<SurvivorsWorldFeedbackEffect>(EnemyDeathEffectLimit);
        private readonly List<SurvivorsRewardDropFeedbackEffect> _rewardDropFeedbackEffects = new List<SurvivorsRewardDropFeedbackEffect>(MajorRewardDropEffectLimit);
        private readonly List<SurvivorsMajorThreatSlamTelegraphEffect> _majorThreatSlamTelegraphEffects = new List<SurvivorsMajorThreatSlamTelegraphEffect>(MajorThreatSlamTelegraphEffectLimit);
        private readonly List<SurvivorsIncomingThreatTelegraphEffect> _incomingThreatTelegraphEffects = new List<SurvivorsIncomingThreatTelegraphEffect>(IncomingThreatTelegraphEffectLimit);
        private readonly List<SurvivorsEnemyRangedAttackFeedbackEffect> _enemyRangedAttackFeedbackEffects = new List<SurvivorsEnemyRangedAttackFeedbackEffect>(EnemyRangedAttackFeedbackLimit);
        private readonly List<string> _lastRunSummaryLines = new List<string>(8);
        private readonly List<string> _runMetricsLines = new List<string>(16);
        private readonly List<SurvivorsUiTheme> _availableUiThemes = new List<SurvivorsUiTheme>(2);
        private readonly Dictionary<string, float> _audioEventNextAllowedTime = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly List<SurvivorsPersistentUpgradeDefinition> _resultMetaUpgradeOptions = new List<SurvivorsPersistentUpgradeDefinition>(ResultMetaUpgradeOptionCount);
        private readonly List<SurvivorsClassDefinition> _resultClassOptions = new List<SurvivorsClassDefinition>(ResultClassOptionCount);
        private readonly HashSet<SurvivorsEnemyActor> _activeHordeRushEnemies = new HashSet<SurvivorsEnemyActor>();
        private readonly HashSet<SurvivorsEnemyActor> _activeRoamingCacheAmbushEnemies = new HashSet<SurvivorsEnemyActor>();
        private readonly HashSet<SurvivorsEnemyActor> _activeArenaShrineEnemies = new HashSet<SurvivorsEnemyActor>();
        private readonly HashSet<SurvivorsEnemyActor> _enragedMajorThreats = new HashSet<SurvivorsEnemyActor>();
        private readonly HashSet<long> _discoveredArenaWaystones = new HashSet<long>();
        private readonly Dictionary<string, SurvivorsRunUpgradeMetadata> _upgradeMetadataById = new Dictionary<string, SurvivorsRunUpgradeMetadata>(StringComparer.Ordinal);
        private readonly HashSet<string> _ownedPassiveUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _ownedEvolutionUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _announcedEvolutionGoalUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _announcedEvolutionReadyUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _selectedRelicIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<SurvivorsRelicDefinition> _selectedRelics = new List<SurvivorsRelicDefinition>(8);
        private Transform _worldRoot;
        private Transform _prefabRoot;
        private Transform _arenaTileRoot;
        private Transform _arenaFollowRoot;
        private Transform _waystoneCompassRoot;
        private GameObject _playerObject;
        private Renderer _playerRenderer;
        private Camera _camera;
        private GameObject _enemyPrefab;
        private GameObject _experiencePickupPrefab;
        private GameObject _magnetPickupPrefab;
        private GameObject _healthPickupPrefab;
        private GameObject _bloodShardPickupPrefab;
        private GameObject _projectilePrefab;
        private Transform _feedbackRoot;
        private ParticleSystem _spawnPulse;
        private ParticleSystem _firePulse;
        private ParticleSystem _killPulse;
        private ParticleSystem _pickupPulse;
        private ParticleSystem _levelUpPulse;
        private ParticleSystem _bossPulse;
        private AudioSource _feedbackAudio;
        private AudioClip _spawnClip;
        private AudioClip _fireClip;
        private AudioClip _killClip;
        private AudioClip _pickupClip;
        private AudioClip _levelUpClip;
        private AudioClip _bossClip;
        private AudioClip _dangerClip;
        private GUIStyle _hudTitleStyle;
        private GUIStyle _hudLabelStyle;
        private GUIStyle _hudSmallStyle;
        private GUIStyle _damagePopupStyle;
        private GUIStyle _playerDamagePopupStyle;
        private GUIStyle _lowHealthStyle;
        private GUIStyle _majorThreatWarningStyle;
        private GUIStyle _rewardFeedbackStyle;
        private GUIStyle _draftTitleStyle;
        private GUIStyle _draftCardNameStyle;
        private GUIStyle _draftCardMetaStyle;
        private GUIStyle _draftCardDescriptionStyle;
        private GUIStyle _draftCardHotkeyStyle;
        private GUIStyle _menuTitleStyle;
        private GUIStyle _menuTabStyle;
        private GUIStyle _transparentButtonStyle;
        private SurvivorsSpawnPoseResolver _poseResolver;
        private WorldSpawnService _spawnService;
        private HealthState _playerHealth;
        private RunUpgradeCatalog _upgradeCatalog;
        private RunUpgradeState _upgradeState;
        private RunUpgradeDraft _currentDraft;
        private SurvivorsRelicDraft _currentRelicDraft;
        private SurvivorsRewardSelectionKind _rewardSelectionKind;
        private CombatCatalog _combatCatalog;
        private DeterministicRandom _combatRandom;
        private WeaponDefinition _weaponDefinition;
        private ProjectileDefinition _projectileDefinition;
        private SurvivorsWeaponLoadoutRuntime _weaponLoadout;
        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> _weaponArchetypeDefinitions = Array.Empty<SurvivorsWeaponArchetypeDefinition>();
        private SurvivorsRunFlowRuntime _runFlow;
        private SurvivorsAuthoredContentDefinition _authoredContent;
        private SurvivorsAuthoredContentBindingPolicy _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.AllowFallbacks;
        private bool _strictSampleContentReady;
        private string _authoredContentStatus = "Fallback content active for an unbound host.";
        private bool _usingAuthoredRunFlow;
        private IReadOnlyList<SurvivorsRelicDefinition> _relicDefinitions;
        private IReadOnlyList<SurvivorsClassUpgradeGateDefinition> _upgradeClassGates;
        private SurvivorsClassLibraryDefinition _classLibrary;
        private SurvivorsClassDefinition _selectedClass;
        private SurvivorsMetaProgressionService _metaProgression;
        private IPersistenceService _injectedMetaPersistence;
        private SaveSlotId _metaSaveSlotId = new SaveSlotId("survivors-template");
        private float _enemySpawnTimer;
        private float _playerInvulnerabilityTimer;
        private float _dashCooldownTimer;
        private float _rewardSelectionTimer;
        private float _killStreakTimer;
        private float _roamingCacheTravelDistance;
        private float _arenaShrineTravelDistance;
        private long _spawnSequence;
        private int _killStreakCount;
        private int _currentDraftRerollIndex;
        private float _streakSurgeTimer;
        private float _roamingCacheSurgeTimer;
        private float _arenaShrineSurgeTimer;
        private float _waystoneFocusTimer;
        private float _waystoneChainSurgeTimer;
        private float _hordeRushClearSurgeTimer;
        private float _weaponLoadoutSurgeTimer;
        private float _passiveLoadoutSurgeTimer;
        private float _bossRelicSurgeTimer;
        private float _evolutionChainSurgeTimer;
        private float _endlessSurgeTimer;
        private float _payloadHazardChainWindowTimer;
        private float _payloadHazardChainCooldownTimer;
        private int _payloadHazardChainSnareCount;
        private bool _runStarted;
        private bool _runModeSelectionOpen;
        private bool _debugOverlayVisible;
        private bool _buildMenuOpen;
        private BuildMenuTab _buildMenuTab;
        private Vector2 _buildMenuScrollPosition;
        private Vector2 _runSummaryScrollPosition;
        private Vector2 _draftCardScrollPosition;
        private Vector2 _modeSelectionScrollPosition;
        private bool _tutorialOverlayOpen;
        private int _tutorialStepIndex;
        private int _selectedUiThemeIndex;
        private bool _audioMuted;
        private int _audioEventDispatchCount;
        private string _lastAudioEventId = string.Empty;
        private string _lastRunSummaryTitle = string.Empty;
        private RunUpgradeRarity _highestChosenRarity;
        private string _highestChosenRarityLabel = string.Empty;
        private string _bestMomentLabel = string.Empty;
        private bool _lowHealthClutchPulseUsed;
        private bool _weaponLoadoutSurgeUsed;
        private bool _passiveLoadoutSurgeUsed;
        private bool _ownsMetaProgressionService;
        private bool _metaProfileLoaded;
        private bool _runRewardsGranted;
        private bool _pendingVictoryAfterRewardDraft;
        private bool _pendingBossRelicAfterRewardDraft;
        private bool _victoryClearedThisRun;
        private bool _firstEliteWarningShown;
        private bool _firstDreadEliteWarningShown;
        private float _timedEliteWarningTargetTimeSeconds;
        private float _timedDreadEliteWarningTargetTimeSeconds;
        private bool _minibossWarningShown;
        private bool _bossWarningShown;
        private bool _endlessEliteWarningShown;
        private bool _endlessMinibossWarningShown;
        private bool _endlessBossWarningShown;
        private float _nextEndlessEliteSpawnTimeSeconds;
        private float _nextEndlessMinibossSpawnTimeSeconds;
        private float _nextEndlessBossSpawnTimeSeconds;
        private int _endlessEliteSpawnSequence;
        private bool _hordeRushWarningShown;
        private float _nextHordeRushTimeSeconds;
        private int _hordeRushSequence;
        private string _hordeRushWarningLabel = string.Empty;
        private float _hordeRushWarningTargetTimeSeconds;
        private string _majorThreatWarningLabel = string.Empty;
        private float _majorThreatWarningTargetTimeSeconds;
        private string _rewardFeedbackLabel = string.Empty;
        private Color _rewardFeedbackColor = Color.white;
        private float _rewardFeedbackTimer;
        private string _streakRewardFeedbackLabel = string.Empty;
        private Color _streakRewardFeedbackColor = Color.white;
        private float _streakRewardFeedbackTimer;
        private string _classUnlockRewardFeedbackLabel = string.Empty;
        private float _classUnlockRewardFeedbackTimer;
        private string _evolutionReadyFeedbackLabel = string.Empty;
        private float _evolutionReadyFeedbackTimer;
        private string _experienceComboFeedbackLabel = string.Empty;
        private float _experienceComboTimer;
        private float _experienceComboFeedbackTimer;
        private float _gemRushTimer;
        private int _experienceComboPickupCount;
        private int _experienceComboAmount;
        private bool _gemRushActivatedForCurrentCombo;
        private int _bonusBloodShardsEarnedThisRun;
        private int _bonusLegacyExperienceEarnedThisRun;
        private float _firstKillTimeSeconds;
        private float _firstExperiencePickupTimeSeconds;
        private float _firstLevelUpDraftTimeSeconds;
        private float _firstEliteSpawnTimeSeconds;
        private float _firstEliteKillTimeSeconds;
        private float _firstMinibossSpawnTimeSeconds;
        private float _firstMinibossKillTimeSeconds;
        private float _firstBossSpawnTimeSeconds;
        private float _firstBossKillTimeSeconds;
        private float _firstEvolutionEligibilityTimeSeconds;
        private float _firstEvolutionAcquiredTimeSeconds;
        private float _damageTakenThisRun;
        private int _draftOpenCount;
        private float _levelUpDraftCooldownTimer;
        private int _levelAtOneMinute;
        private int _levelAtTwoMinutes;
        private int _levelAtThreeMinutes;
        private int _levelAtFourMinutes;
        private int _levelAtFiveMinutes;
        private float _pickupMagnetPulseTimer;
        private string _lastMagnetPulseFeedbackLabel = string.Empty;
        private string _lastOffscreenThreatMarkerLabel = string.Empty;
        private string _lastGameplaySpawnSafetyFailure = string.Empty;
        private Vector3 _lastGameplaySpawnPosition = Vector3.zero;
        private float _lastGameplaySpawnPadding;
        private bool _lastGameplaySpawnWasInsideCameraViewport;

        public SurvivorsRunState State { get; private set; } = SurvivorsRunState.Booting;
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int PendingLevelUps { get; private set; }
        public int SpawnedCount { get; private set; }
        public int KilledCount { get; private set; }
        public int ProjectileLaunchCount { get; private set; }
        public int OrbitHitCount { get; private set; }
        public int MeleeSwingCount { get; private set; }
        public int MeleeHitCount { get; private set; }
        public int BurstPulseCount { get; private set; }
        public int BurstHitCount { get; private set; }
        public int HitscanFireCount { get; private set; }
        public int HitscanHitCount { get; private set; }
        public int TempestPrismArcHitCount { get; private set; }
        public string LastTempestPrismArcFeedbackLabel { get; private set; } = string.Empty;
        public int ProjectilePierceHitCount { get; private set; }
        public int ProjectileChainHitCount { get; private set; }
        public int ProjectileForkSpawnCount { get; private set; }
        public int ProjectileReturnStartCount { get; private set; }
        public int OrbitKnockbackCount { get; private set; }
        public string LastOrbitKnockbackFeedbackLabel { get; private set; } = string.Empty;
        public int FrostFanSlowApplicationCount { get; private set; }
        public string LastFrostFanSlowFeedbackLabel { get; private set; } = string.Empty;
        public int CinderBurnApplicationCount { get; private set; }
        public string LastCinderBurnFeedbackLabel { get; private set; } = string.Empty;
        public int PayloadThrowCount { get; private set; }
        public int PayloadPlacedCount { get; private set; }
        public int PayloadDetonationCount { get; private set; }
        public int PayloadExplosionHitCount { get; private set; }
        public int PayloadHazardTickCount { get; private set; }
        public int PayloadHazardSnareCount { get; private set; }
        public string LastPayloadHazardSnareFeedbackLabel { get; private set; } = string.Empty;
        public int PayloadHazardChainActivationCount { get; private set; }
        public int PayloadHazardChainPulseHitCount { get; private set; }
        public int PayloadHazardChainExperienceGemDropCount { get; private set; }
        public string LastPayloadHazardChainFeedbackLabel { get; private set; } = string.Empty;
        public int SplitterChildSpawnCount { get; private set; }
        public int SplitterSplitFeedbackCount { get; private set; }
        public string LastSplitterSplitFeedbackLabel { get; private set; } = string.Empty;
        public int SummonerSupportSpawnCount { get; private set; }
        public int SummonerSupportFeedbackCount { get; private set; }
        public string LastSummonerSupportFeedbackLabel { get; private set; } = string.Empty;
        public int MinibossSpawnCount { get; private set; }
        public int BossSpawnCount { get; private set; }
        public int EliteKilledCount { get; private set; }
        public int MinibossKilledCount { get; private set; }
        public int BossKilledCount { get; private set; }
        public int EliteRewardGrantCount { get; private set; }
        public int MinibossRewardGrantCount { get; private set; }
        public int BossRewardGrantCount { get; private set; }
        public int BossRelicDraftOpenCount { get; private set; }
        public int EliteUpgradeDraftOpenCount { get; private set; }
        public int BossUpgradeDraftOpenCount { get; private set; }
        public int LevelUpDraftOpenCount { get; private set; }
        public int SelectedRelicCount { get; private set; }
        public int BossRelicSurgeCount { get; private set; }
        public int BossRelicSurgeHitCount { get; private set; }
        public string LastBossRelicSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int WeaponLoadoutSurgeActivationCount { get; private set; }
        public int WeaponLoadoutSurgePulseHitCount { get; private set; }
        public string LastWeaponLoadoutSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int PassiveLoadoutSurgeActivationCount { get; private set; }
        public int PassiveLoadoutSurgePulseHitCount { get; private set; }
        public string LastPassiveLoadoutSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int LevelUpPulseCount { get; private set; }
        public int LevelUpPulseHitCount { get; private set; }
        public string LastLevelUpPulseFeedbackLabel { get; private set; } = string.Empty;
        public int SelectedRewardUpgradeCount { get; private set; }
        public int RewardUpgradeSurgeCount { get; private set; }
        public int RewardUpgradeSurgeHitCount { get; private set; }
        public string LastRewardUpgradeSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int RewardJackpotCount { get; private set; }
        public int RewardJackpotExperienceGemDropCount { get; private set; }
        public int RewardJackpotBloodShardDropCount { get; private set; }
        public int RewardJackpotBloodShardsDropped { get; private set; }
        public string LastRewardJackpotFeedbackLabel { get; private set; } = string.Empty;
        public int RewardAutoSelectCount { get; private set; }
        public int EndlessThreatSpawnCount { get; private set; }
        public int EndlessSurgeActivationCount { get; private set; }
        public int EndlessSurgeTier { get; private set; }
        public int EndlessSurgeExperienceGemDropCount { get; private set; }
        public int EndlessSurgeBloodShardDropCount { get; private set; }
        public int EndlessSurgePulseHitCount { get; private set; }
        public string LastEndlessSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int HordeRushSpawnCount { get; private set; }
        public int HordeRushEnemySpawnCount { get; private set; }
        public int HordeRushWarningCount { get; private set; }
        public int HordeRushClearRewardCount { get; private set; }
        public int HordeRushClearExperienceGemDropCount { get; private set; }
        public int HordeRushClearSpecialDropCount { get; private set; }
        public int HordeRushClearPulseCount { get; private set; }
        public int HordeRushClearPulseHitCount { get; private set; }
        public int HordeRushClearSurgeActivationCount { get; private set; }
        public string LastHordeRushFeedbackLabel { get; private set; } = string.Empty;
        public string LastHordeRushClearFeedbackLabel { get; private set; } = string.Empty;
        public string LastHordeRushClearPulseFeedbackLabel { get; private set; } = string.Empty;
        public int DraftRerollCount { get; private set; }
        public int DraftBanishCount { get; private set; }
        public int DraftSkipCount { get; private set; }
        public int ClassUnlockRewardCount { get; private set; }
        public string LastClassUnlockRewardFeedbackLabel { get; private set; } = string.Empty;
        public int DamagePopupSpawnCount { get; private set; }
        public int PlayerDamageFeedbackCount { get; private set; }
        public int LowHealthClutchPulseCount { get; private set; }
        public int LowHealthClutchPulseHitCount { get; private set; }
        public string LastLowHealthClutchPulseFeedbackLabel { get; private set; } = string.Empty;
        public int DashUseCount { get; private set; }
        public int DashEnemyShoveCount { get; private set; }
        public int DashDamageHitCount { get; private set; }
        public string LastDashFeedbackLabel { get; private set; } = string.Empty;
        public int EnemyHitFlashFeedbackCount { get; private set; }
        public int CriticalHitFeedbackCount { get; private set; }
        public int DeathNovaTriggerCount { get; private set; }
        public int DeathNovaHitCount { get; private set; }
        public int EnemyDeathEffectCount { get; private set; }
        public int EnemyRangedAttackFeedbackCount { get; private set; }
        public int EnemyRangedAttackDodgeFeedbackCount { get; private set; }
        public int EnemyRangedAttackDodgeExperienceGemDropCount { get; private set; }
        public string LastEnemyRangedAttackDodgeFeedbackLabel { get; private set; } = string.Empty;
        public int MajorRewardDropFeedbackCount { get; private set; }
        public int MajorRewardCacheDropCount { get; private set; }
        public int MajorRewardCacheExperienceGemDropCount { get; private set; }
        public int MajorRewardCacheSpecialDropCount { get; private set; }
        public int MajorRewardCacheAttractedPickupCount { get; private set; }
        public int WeaponEvolutionFeedbackCount { get; private set; }
        public int WeaponEvolutionSurgeCount { get; private set; }
        public int WeaponEvolutionSurgeHitCount { get; private set; }
        public int EvolutionMagnetRecallCount { get; private set; }
        public int EvolutionMagnetRecallGemCount { get; private set; }
        public int EvolutionChainSurgeActivationCount { get; private set; }
        public int EvolutionChainSurgePulseHitCount { get; private set; }
        public string LastWeaponEvolutionSurgeFeedbackLabel { get; private set; } = string.Empty;
        public string LastEvolutionMagnetRecallFeedbackLabel { get; private set; } = string.Empty;
        public string LastEvolutionChainSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int MetaUpgradePurchaseCount { get; private set; }
        public string LastMetaUpgradePurchaseFeedbackLabel { get; private set; } = string.Empty;
        public int ResultClassSelectionCount { get; private set; }
        public string LastResultClassSelectionFeedbackLabel { get; private set; } = string.Empty;
        public int MajorThreatWarningCount { get; private set; }
        public int MajorThreatEnrageCount { get; private set; }
        public int MajorThreatEnrageSupportSpawnCount { get; private set; }
        public string LastMajorThreatEnrageFeedbackLabel { get; private set; } = string.Empty;
        public int MajorThreatSlamWarningCount { get; private set; }
        public int MajorThreatSlamCastCount { get; private set; }
        public int MajorThreatSlamHitCount { get; private set; }
        public int MajorThreatSlamTelegraphEffectCount { get; private set; }
        public int IncomingThreatTelegraphEffectCount { get; private set; }
        public string LastMajorThreatSlamFeedbackLabel { get; private set; } = string.Empty;
        public string LastIncomingThreatTelegraphLabel { get; private set; } = string.Empty;
        public int ExperiencePickupFeedbackCount { get; private set; }
        public int ExperienceComboFeedbackCount { get; private set; }
        public int GemRushActivationCount { get; private set; }
        public string LastExperienceComboFeedbackLabel { get; private set; } = string.Empty;
        public string LastGemRushFeedbackLabel { get; private set; } = string.Empty;
        public int EvolutionGoalFeedbackCount { get; private set; }
        public string LastEvolutionGoalFeedbackLabel { get; private set; } = string.Empty;
        public int EvolutionReadyFeedbackCount { get; private set; }
        public string LastEvolutionReadyFeedbackLabel { get; private set; } = string.Empty;
        public int HealthPickupCollectedCount { get; private set; }
        public float HealthRestoredByPickups { get; private set; }
        public int BloodShardPickupCollectedCount { get; private set; }
        public int BloodShardsCollectedFromPickups { get; private set; }
        public int PickupAttractionFeedbackCount { get; private set; }
        public int MagnetRecallFeedbackCount { get; private set; }
        public int RewardCardPresentationCount { get; private set; }
        public int RewardSelectionFeedbackCount { get; private set; }
        public string LastRewardCardPresentationLabel { get; private set; } = string.Empty;
        public string LastRewardSelectionFeedbackLabel { get; private set; } = string.Empty;
        public string LastMajorRewardDropFeedbackLabel { get; private set; } = string.Empty;
        public string LastMajorRewardCacheFeedbackLabel { get; private set; } = string.Empty;
        public int ExperienceCollected { get; private set; }
        public int SelectedUpgradeCount { get; private set; }
        public int MagnetRecallCount { get; private set; }
        public int RoamingCacheDropCount { get; private set; }
        public int RoamingCacheExperienceGemDropCount { get; private set; }
        public int RoamingCacheMagnetDropCount { get; private set; }
        public int RoamingCacheBloodShardDropCount { get; private set; }
        public int RoamingCacheAmbushCount { get; private set; }
        public int RoamingCacheAmbushEnemySpawnCount { get; private set; }
        public int RoamingCacheAmbushClearRewardCount { get; private set; }
        public int RoamingCacheAmbushClearExperienceGemDropCount { get; private set; }
        public int RoamingCacheAmbushClearMagnetDropCount { get; private set; }
        public int RoamingCacheAmbushClearBloodShardDropCount { get; private set; }
        public int RoamingCacheSurgeActivationCount { get; private set; }
        public int RoamingCacheSurgeBonusExperienceGemDropCount { get; private set; }
        public int RoamingCacheSurgePulseHitCount { get; private set; }
        public string LastRoamingCacheFeedbackLabel { get; private set; } = string.Empty;
        public string LastRoamingCacheAmbushClearFeedbackLabel { get; private set; } = string.Empty;
        public string LastRoamingCacheSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int ArenaShrineTrialCount { get; private set; }
        public int ArenaShrineEnemySpawnCount { get; private set; }
        public int ArenaShrineClearRewardCount { get; private set; }
        public int ArenaShrineClearExperienceGemDropCount { get; private set; }
        public int ArenaShrineClearBloodShardDropCount { get; private set; }
        public int ArenaShrineSurgeActivationCount { get; private set; }
        public int ArenaShrineSurgePulseHitCount { get; private set; }
        public string LastArenaShrineFeedbackLabel { get; private set; } = string.Empty;
        public string LastArenaShrineClearFeedbackLabel { get; private set; } = string.Empty;
        public int WaystoneDiscoveryCount { get; private set; }
        public int WaystoneExperienceGemDropCount { get; private set; }
        public int WaystoneBloodShardDropCount { get; private set; }
        public int WaystoneAmbushCount { get; private set; }
        public int WaystoneAmbushEnemySpawnCount { get; private set; }
        public int WaystoneFocusActivationCount { get; private set; }
        public int WaystoneChainSurgeActivationCount { get; private set; }
        public int WaystoneChainSurgeBonusExperienceGemDropCount { get; private set; }
        public int WaystoneChainSurgePulseHitCount { get; private set; }
        public string LastWaystoneDiscoveryFeedbackLabel { get; private set; } = string.Empty;
        public string LastWaystoneChainSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int BestKillStreak { get; private set; }
        public int StreakBonusDropCount { get; private set; }
        public int StreakHealthDropCount { get; private set; }
        public int StreakMagnetDropCount { get; private set; }
        public int StreakBloodShardDropCount { get; private set; }
        public int StreakRewardFeedbackCount { get; private set; }
        public string LastStreakRewardFeedbackLabel { get; private set; } = string.Empty;
        public int StreakSurgeTier { get; private set; }
        public int StreakSurgeActivationCount { get; private set; }
        public int CurrentKillStreak => _killStreakTimer > 0f ? _killStreakCount : 0;
        public bool IsStreakSurgeActive => _streakSurgeTimer > 0f && StreakSurgeTier > 0;
        public float StreakSurgeRemainingSeconds => Mathf.Max(0f, _streakSurgeTimer);
        public float StreakSurgeDamageBonus => IsStreakSurgeActive ? StreakSurgeTier * StreakSurgeDamageBonusPerTier : 0f;
        public float StreakSurgeMoveSpeedBonus => IsStreakSurgeActive ? StreakSurgeTier * StreakSurgeMoveSpeedBonusPerTier : 0f;
        public float StreakSurgeCooldownMultiplierBonus => IsStreakSurgeActive ? -StreakSurgeTier * StreakSurgeCooldownReductionPerTier : 0f;
        public float StreakSurgePickupRangeBonus => IsStreakSurgeActive ? StreakSurgeTier * StreakSurgePickupRangeBonusPerTier : 0f;
        public bool IsRoamingCacheSurgeActive => _roamingCacheSurgeTimer > 0f;
        public float RoamingCacheSurgeRemainingSeconds => Mathf.Max(0f, _roamingCacheSurgeTimer);
        public float RoamingCacheSurgeDamageBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, CurrentTuning.RoamingCacheSurgeDamageBonus) : 0f;
        public float RoamingCacheSurgeMoveSpeedBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, CurrentTuning.RoamingCacheSurgeMoveSpeedBonus) : 0f;
        public float RoamingCacheSurgeCooldownMultiplierBonus => IsRoamingCacheSurgeActive ? Mathf.Min(0f, CurrentTuning.RoamingCacheSurgeCooldownMultiplierBonus) : 0f;
        public float RoamingCacheSurgePickupRangeBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, CurrentTuning.RoamingCacheSurgePickupRangeBonus) : 0f;
        public bool IsArenaShrineSurgeActive => _arenaShrineSurgeTimer > 0f;
        public float ArenaShrineSurgeRemainingSeconds => Mathf.Max(0f, _arenaShrineSurgeTimer);
        public float ArenaShrineSurgeDamageBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, CurrentTuning.ArenaShrineSurgeDamageBonus) : 0f;
        public float ArenaShrineSurgeMoveSpeedBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, CurrentTuning.ArenaShrineSurgeMoveSpeedBonus) : 0f;
        public float ArenaShrineSurgeCooldownMultiplierBonus => IsArenaShrineSurgeActive ? Mathf.Min(0f, CurrentTuning.ArenaShrineSurgeCooldownMultiplierBonus) : 0f;
        public float ArenaShrineSurgePickupRangeBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, CurrentTuning.ArenaShrineSurgePickupRangeBonus) : 0f;
        public bool IsWaystoneFocusActive => _waystoneFocusTimer > 0f;
        public float WaystoneFocusRemainingSeconds => Mathf.Max(0f, _waystoneFocusTimer);
        public float WaystoneFocusDamageBonus => IsWaystoneFocusActive ? Mathf.Max(0f, CurrentTuning.WaystoneFocusDamageBonus) : 0f;
        public float WaystoneFocusMoveSpeedBonus => IsWaystoneFocusActive ? Mathf.Max(0f, CurrentTuning.WaystoneFocusMoveSpeedBonus) : 0f;
        public float WaystoneFocusCooldownMultiplierBonus => IsWaystoneFocusActive ? Mathf.Min(0f, CurrentTuning.WaystoneFocusCooldownMultiplierBonus) : 0f;
        public float WaystoneFocusPickupRangeBonus => IsWaystoneFocusActive ? Mathf.Max(0f, CurrentTuning.WaystoneFocusPickupRangeBonus) : 0f;
        public bool IsWaystoneChainSurgeActive => _waystoneChainSurgeTimer > 0f;
        public float WaystoneChainSurgeRemainingSeconds => Mathf.Max(0f, _waystoneChainSurgeTimer);
        public float WaystoneChainSurgeDamageBonus => IsWaystoneChainSurgeActive ? Mathf.Max(0f, CurrentTuning.WaystoneChainDamageBonus) : 0f;
        public float WaystoneChainSurgeMoveSpeedBonus => IsWaystoneChainSurgeActive ? Mathf.Max(0f, CurrentTuning.WaystoneChainMoveSpeedBonus) : 0f;
        public float WaystoneChainSurgeCooldownMultiplierBonus => IsWaystoneChainSurgeActive ? Mathf.Min(0f, CurrentTuning.WaystoneChainCooldownMultiplierBonus) : 0f;
        public float WaystoneChainSurgePickupRangeBonus => IsWaystoneChainSurgeActive ? Mathf.Max(0f, CurrentTuning.WaystoneChainPickupRangeBonus) : 0f;
        public bool IsHordeRushClearSurgeActive => _hordeRushClearSurgeTimer > 0f;
        public float HordeRushClearSurgeRemainingSeconds => Mathf.Max(0f, _hordeRushClearSurgeTimer);
        public float HordeRushClearSurgeDamageBonus => IsHordeRushClearSurgeActive ? Mathf.Max(0f, CurrentTuning.HordeRushClearSurgeDamageBonus) : 0f;
        public float HordeRushClearSurgeMoveSpeedBonus => IsHordeRushClearSurgeActive ? Mathf.Max(0f, CurrentTuning.HordeRushClearSurgeMoveSpeedBonus) : 0f;
        public float HordeRushClearSurgeCooldownMultiplierBonus => IsHordeRushClearSurgeActive ? Mathf.Min(0f, CurrentTuning.HordeRushClearSurgeCooldownMultiplierBonus) : 0f;
        public float HordeRushClearSurgePickupRangeBonus => IsHordeRushClearSurgeActive ? Mathf.Max(0f, CurrentTuning.HordeRushClearSurgePickupRangeBonus) : 0f;
        public bool IsWeaponLoadoutSurgeActive => _weaponLoadoutSurgeTimer > 0f;
        public float WeaponLoadoutSurgeRemainingSeconds => Mathf.Max(0f, _weaponLoadoutSurgeTimer);
        public float WeaponLoadoutSurgeDamageBonus => IsWeaponLoadoutSurgeActive ? Mathf.Max(0f, CurrentTuning.WeaponLoadoutSurgeDamageBonus) : 0f;
        public float WeaponLoadoutSurgeMoveSpeedBonus => IsWeaponLoadoutSurgeActive ? Mathf.Max(0f, CurrentTuning.WeaponLoadoutSurgeMoveSpeedBonus) : 0f;
        public float WeaponLoadoutSurgeCooldownMultiplierBonus => IsWeaponLoadoutSurgeActive ? Mathf.Min(0f, CurrentTuning.WeaponLoadoutSurgeCooldownMultiplierBonus) : 0f;
        public float WeaponLoadoutSurgePickupRangeBonus => IsWeaponLoadoutSurgeActive ? Mathf.Max(0f, CurrentTuning.WeaponLoadoutSurgePickupRangeBonus) : 0f;
        public bool IsPassiveLoadoutSurgeActive => _passiveLoadoutSurgeTimer > 0f;
        public float PassiveLoadoutSurgeRemainingSeconds => Mathf.Max(0f, _passiveLoadoutSurgeTimer);
        public float PassiveLoadoutSurgeDamageBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, CurrentTuning.PassiveLoadoutSurgeDamageBonus) : 0f;
        public float PassiveLoadoutSurgeMoveSpeedBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, CurrentTuning.PassiveLoadoutSurgeMoveSpeedBonus) : 0f;
        public float PassiveLoadoutSurgeCooldownMultiplierBonus => IsPassiveLoadoutSurgeActive ? Mathf.Min(0f, CurrentTuning.PassiveLoadoutSurgeCooldownMultiplierBonus) : 0f;
        public float PassiveLoadoutSurgePickupRangeBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, CurrentTuning.PassiveLoadoutSurgePickupRangeBonus) : 0f;
        public float PassiveLoadoutSurgeExperienceGainMultiplierBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, CurrentTuning.PassiveLoadoutSurgeExperienceGainMultiplierBonus) : 0f;
        public bool IsBossRelicSurgeActive => _bossRelicSurgeTimer > 0f;
        public float BossRelicSurgeRemainingSeconds => Mathf.Max(0f, _bossRelicSurgeTimer);
        public float BossRelicSurgeDamageBonus => IsBossRelicSurgeActive ? Mathf.Max(0f, CurrentTuning.BossRelicSurgeDamageBonus) : 0f;
        public float BossRelicSurgeMoveSpeedBonus => IsBossRelicSurgeActive ? Mathf.Max(0f, CurrentTuning.BossRelicSurgeMoveSpeedBonus) : 0f;
        public float BossRelicSurgeCooldownMultiplierBonus => IsBossRelicSurgeActive ? Mathf.Min(0f, CurrentTuning.BossRelicSurgeCooldownMultiplierBonus) : 0f;
        public float BossRelicSurgePickupRangeBonus => IsBossRelicSurgeActive ? Mathf.Max(0f, CurrentTuning.BossRelicSurgePickupRangeBonus) : 0f;
        public bool IsGemRushActive => _gemRushTimer > 0f;
        public float GemRushRemainingSeconds => Mathf.Max(0f, _gemRushTimer);
        public float GemRushDamageBonus => IsGemRushActive ? Mathf.Max(0f, CurrentTuning.GemRushDamageBonus) : 0f;
        public float GemRushMoveSpeedBonus => IsGemRushActive ? Mathf.Max(0f, CurrentTuning.GemRushMoveSpeedBonus) : 0f;
        public float GemRushCooldownMultiplierBonus => IsGemRushActive ? Mathf.Min(0f, CurrentTuning.GemRushCooldownMultiplierBonus) : 0f;
        public float GemRushPickupRangeBonus => IsGemRushActive ? Mathf.Max(0f, CurrentTuning.GemRushPickupRangeBonus) : 0f;
        public bool IsEvolutionChainSurgeActive => _evolutionChainSurgeTimer > 0f;
        public float EvolutionChainSurgeRemainingSeconds => Mathf.Max(0f, _evolutionChainSurgeTimer);
        public float EvolutionChainSurgeDamageBonus => IsEvolutionChainSurgeActive ? Mathf.Max(0f, CurrentTuning.EvolutionChainSurgeDamageBonus) : 0f;
        public float EvolutionChainSurgeMoveSpeedBonus => IsEvolutionChainSurgeActive ? Mathf.Max(0f, CurrentTuning.EvolutionChainSurgeMoveSpeedBonus) : 0f;
        public float EvolutionChainSurgeCooldownMultiplierBonus => IsEvolutionChainSurgeActive ? Mathf.Min(0f, CurrentTuning.EvolutionChainSurgeCooldownMultiplierBonus) : 0f;
        public float EvolutionChainSurgePickupRangeBonus => IsEvolutionChainSurgeActive ? Mathf.Max(0f, CurrentTuning.EvolutionChainSurgePickupRangeBonus) : 0f;
        public bool IsEndlessSurgeActive => _endlessSurgeTimer > 0f && EndlessSurgeTier > 0;
        public float EndlessSurgeRemainingSeconds => Mathf.Max(0f, _endlessSurgeTimer);
        public float EndlessSurgeDamageBonus => IsEndlessSurgeActive ? Mathf.Max(0f, CurrentTuning.EndlessSurgeDamageBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public float EndlessSurgeMoveSpeedBonus => IsEndlessSurgeActive ? Mathf.Max(0f, CurrentTuning.EndlessSurgeMoveSpeedBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public float EndlessSurgeCooldownMultiplierBonus => IsEndlessSurgeActive ? Mathf.Min(0f, CurrentTuning.EndlessSurgeCooldownMultiplierBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public float EndlessSurgePickupRangeBonus => IsEndlessSurgeActive ? Mathf.Max(0f, CurrentTuning.EndlessSurgePickupRangeBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public int EndlessExplorationBonusTier => ResolveEndlessExplorationBonusTier();
        public int BonusBloodShardsEarnedThisRun => _bonusBloodShardsEarnedThisRun;
        public int BonusLegacyExperienceEarnedThisRun => _bonusLegacyExperienceEarnedThisRun;
        public int BloodShardsEarnedThisRun { get; private set; }
        public int LegacyExperienceEarnedThisRun { get; private set; }
        public SurvivorsRunRewardSummary LastRunResult { get; private set; }
        public IReadOnlyList<string> LastRunSummaryLines => _lastRunSummaryLines;
        public float RunTimeSeconds { get; private set; }
        public float MoveSpeedBonus { get; private set; }
        public float DamageBonus { get; private set; }
        public float PersistentDamageBonus { get; private set; }
        public float PersistentMaxHealthBonus { get; private set; }
        public float PersistentPickupRangeBonus { get; private set; }
        public float PersistentExperienceGainMultiplierBonus { get; private set; }
        public int PersistentDraftRerollBonus { get; private set; }
        public float RelicDamageBonus { get; private set; }
        public float RelicCooldownMultiplierBonus { get; private set; }
        public float RelicPickupRangeBonus { get; private set; }
        public float WeaponCooldownMultiplierBonus { get; private set; }
        public float PickupRangeBonus { get; private set; }
        public float BarrierValue { get; private set; }
        public float BarrierCapacityBonus { get; private set; }
        public float BarrierRegenPerSecondBonus { get; private set; }
        public float BarrierOnDamageRatio { get; private set; }
        public float PoisonDamageRatio { get; private set; }
        public float BleedDamageRatio { get; private set; }
        public float ExecuteThresholdNormalized { get; private set; }
        public float CriticalChanceBonus { get; private set; }
        public float CriticalDamageMultiplierBonus { get; private set; }
        public float DraftLuckBonus { get; private set; }
        public float DeathNovaDamageBonus { get; private set; }
        public float DeathNovaRadiusBonus { get; private set; }
        public float LifestealRatio { get; private set; }
        public float ExperienceGainMultiplierBonus { get; private set; }
        public float AreaRadiusBonus { get; private set; }
        public int ProjectileFanBonus { get; private set; }
        public int OrbitBladeBonus { get; private set; }
        public float OrbitRadiusBonus { get; private set; }
        public int MeleeTargetBonus { get; private set; }
        public int BurstCountBonus { get; private set; }
        public int BurstEchoBonus { get; private set; }
        public int TargetedBurstSigilBonus { get; private set; }
        public int ProjectilePierceBonus { get; private set; }
        public int ProjectileChainBonus { get; private set; }
        public int ProjectileForkBonus { get; private set; }
        public int ProjectileReturnBonus { get; private set; }
        public int HitscanPierceBonus { get; private set; }
        public int PayloadCountBonus { get; private set; }
        public float PayloadExplosionRadiusBonus { get; private set; }
        public float PayloadTriggerRadiusBonus { get; private set; }
        public float PickupAttractionSpeedBonus { get; private set; }
        public float PickupMagnetPulseIntervalReductionBonus { get; private set; }
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
        public bool IsMajorThreatHealthVisible => ResolveCurrentMajorThreatForHud() != null;
        public string CurrentMajorThreatHealthLabel
        {
            get
            {
                SurvivorsEnemyActor enemy = ResolveCurrentMajorThreatForHud();
                if (enemy == null)
                {
                    return string.Empty;
                }

                return string.IsNullOrWhiteSpace(enemy.DisplayName) ? ResolveMajorThreatHealthFallbackLabel(enemy.Role) : enemy.DisplayName;
            }
        }

        public float CurrentMajorThreatHealthFraction
        {
            get
            {
                SurvivorsEnemyActor enemy = ResolveCurrentMajorThreatForHud();
                return enemy == null ? 0f : Mathf.Clamp01(enemy.HealthFraction);
            }
        }
        public int ActivePickupCount => _pickups.Count;
        public int ActiveHordeRushEnemyCount => _activeHordeRushEnemies.Count;
        public int ActiveRoamingCacheAmbushEnemyCount => _activeRoamingCacheAmbushEnemies.Count;
        public int ActiveArenaShrineEnemyCount => _activeArenaShrineEnemies.Count;
        public int ActiveProjectileCount => _projectiles.Count;
        public int ActiveDamagePopupCount => _damagePopups.Count;
        public int ActiveEnemyDeathEffectCount => _worldFeedbackEffects.Count;
        public int ActiveEnemyRangedAttackFeedbackCount => _enemyRangedAttackFeedbackEffects.Count;
        public int ActiveMajorRewardDropFeedbackCount => _rewardDropFeedbackEffects.Count;
        public int ActiveMajorThreatSlamTelegraphEffectCount => _majorThreatSlamTelegraphEffects.Count;
        public int ActiveIncomingThreatTelegraphEffectCount => _incomingThreatTelegraphEffects.Count;
        public int NormalEnemyRecycleCount { get; private set; }
        public int MajorThreatRepositionCount { get; private set; }
        public int GameplayEnemySpawnSafetyCheckCountForTest { get; private set; }
        public int GameplaySpawnInsideCameraViewViolationCountForTest { get; private set; }
        public string LastGameplaySpawnSafetyFailureForTest => _lastGameplaySpawnSafetyFailure;
        public Vector3 LastGameplaySpawnPositionForTest => _lastGameplaySpawnPosition;
        public float LastGameplaySpawnPaddingForTest => _lastGameplaySpawnPadding;
        public bool LastGameplaySpawnWasInsideCameraViewportForTest => _lastGameplaySpawnWasInsideCameraViewport;
        public int ActiveOffscreenThreatMarkerCount => CountOffscreenMajorThreatMarkers();
        public string CurrentOffscreenThreatMarkerLabel => ResolveFirstOffscreenMajorThreatMarkerLabel();
        public string LastOffscreenThreatMarkerLabel => _lastOffscreenThreatMarkerLabel;
        public int ActiveMajorRewardCacheAttractedPickupCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _pickups.Count; i++)
                {
                    SurvivorsPickupActor pickup = _pickups[i];
                    if (pickup != null && pickup.IsRewardCacheAttractionActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int ActiveWeaponCount => _weaponLoadout == null ? 0 : _weaponLoadout.WeaponCount;
        public int ActivePassiveCount => _ownedPassiveUpgradeIds.Count;
        public int EvolvedWeaponCount => _ownedEvolutionUpgradeIds.Count;
        public int MaxWeaponSlots => CurrentTuning.MaxWeaponSlots > 0 ? CurrentTuning.MaxWeaponSlots : DefaultMaxWeaponSlots;
        public int MaxPassiveSlots => CurrentTuning.MaxPassiveSlots > 0 ? CurrentTuning.MaxPassiveSlots : DefaultMaxPassiveSlots;
        public int InfiniteArenaTileCountForTest => _arenaTiles.Count;
        public int InfiniteArenaLandmarkCountForTest => _arenaLandmarks.Count;
        public Vector3 ArenaPresentationCenterForTest => _arenaFollowRoot == null ? Vector3.zero : _arenaFollowRoot.position;
        public Vector3 FirstInfiniteArenaTilePositionForTest => _arenaTiles.Count == 0 || _arenaTiles[0] == null ? Vector3.zero : _arenaTiles[0].position;
        public Vector3 FirstInfiniteArenaLandmarkPositionForTest => _arenaLandmarks.Count == 0 || _arenaLandmarks[0] == null ? Vector3.zero : _arenaLandmarks[0].position;
        public Vector3 ClosestInfiniteArenaLandmarkPositionForTest => ResolveClosestArenaLandmarkPositionForTest();
        public float CurrentWaystoneCompassDistanceForTest => TryResolveClosestArenaLandmark(ignoreDiscovered: true, out _, out float distance, out _) ? distance : 0f;
        public string CurrentWaystoneCompassHudLabel => ResolveWaystoneCompassHudLabel();
        public bool IsWaystoneCompassArrowVisibleForTest => _waystoneCompassRoot != null && _waystoneCompassRoot.gameObject.activeSelf;
        public Vector3 WaystoneCompassArrowForwardForTest => _waystoneCompassRoot == null ? Vector3.zero : _waystoneCompassRoot.forward;
        public IReadOnlyList<string> ActiveWeaponIds => _weaponLoadout == null ? EmptyWeaponIds : _weaponLoadout.WeaponIds;
        public int ActiveOrbitBladeCount => _weaponLoadout == null ? 0 : _weaponLoadout.ActiveOrbitBladeCount;
        public float PlayerMoveSpeed => CurrentTuning.PlayerMoveSpeed + MoveSpeedBonus + StreakSurgeMoveSpeedBonus + RoamingCacheSurgeMoveSpeedBonus + ArenaShrineSurgeMoveSpeedBonus + WaystoneFocusMoveSpeedBonus + WaystoneChainSurgeMoveSpeedBonus + HordeRushClearSurgeMoveSpeedBonus + WeaponLoadoutSurgeMoveSpeedBonus + PassiveLoadoutSurgeMoveSpeedBonus + BossRelicSurgeMoveSpeedBonus + GemRushMoveSpeedBonus + EvolutionChainSurgeMoveSpeedBonus + EndlessSurgeMoveSpeedBonus;
        public float DashCooldownRemainingSeconds => Mathf.Max(0f, _dashCooldownTimer);
        public float PlayerSafetyRemainingSeconds => Mathf.Max(0f, _playerInvulnerabilityTimer);
        public bool IsPlayerSafetyActive => _playerInvulnerabilityTimer > 0f;
        public float ProjectileDamage => ResolveDisplayedWeaponDamage();
        public float WeaponCooldownSeconds => ResolveDisplayedWeaponCooldownSeconds();
        public float CurrentPickupAttractRange => Mathf.Max(0f, CurrentTuning.PickupAttractRange + PickupRangeBonus + StreakSurgePickupRangeBonus + RoamingCacheSurgePickupRangeBonus + ArenaShrineSurgePickupRangeBonus + WaystoneFocusPickupRangeBonus + WaystoneChainSurgePickupRangeBonus + HordeRushClearSurgePickupRangeBonus + WeaponLoadoutSurgePickupRangeBonus + PassiveLoadoutSurgePickupRangeBonus + BossRelicSurgePickupRangeBonus + GemRushPickupRangeBonus + EvolutionChainSurgePickupRangeBonus + EndlessSurgePickupRangeBonus);
        public float CurrentPickupAttractionSpeed => Mathf.Max(0.1f, CurrentTuning.PickupAttractionSpeed + PickupAttractionSpeedBonus);
        public float CurrentPickupMagnetPulseIntervalSeconds => ResolvePickupMagnetPulseIntervalSeconds();
        public float LevelUpDraftCooldownRemainingSeconds => Mathf.Max(0f, _levelUpDraftCooldownTimer);
        public int LevelAtOneMinute => _levelAtOneMinute;
        public int LevelAtTwoMinutes => _levelAtTwoMinutes;
        public int LevelAtThreeMinutes => _levelAtThreeMinutes;
        public int LevelAtFourMinutes => _levelAtFourMinutes;
        public int LevelAtFiveMinutes => _levelAtFiveMinutes;
        public int MagnetPulseActivationCount { get; private set; }
        public string LastMagnetPulseFeedbackLabel => _lastMagnetPulseFeedbackLabel;
        public float CriticalChanceNormalized => Mathf.Clamp01(CriticalChanceBonus);
        public float CriticalDamageMultiplier => Mathf.Clamp(1.5f + CriticalDamageMultiplierBonus, 1f, 100f);
        public float DeathNovaDamage => Mathf.Max(0f, DeathNovaDamageBonus);
        public float DeathNovaRadius => DeathNovaDamage <= 0f ? 0f : Mathf.Max(0f, BaseDeathNovaRadius + DeathNovaRadiusBonus + AreaRadiusBonus * 0.5f);
        public float CurrentHealth => _playerHealth == null ? 0f : (float)_playerHealth.CurrentHealth;
        public float MaxHealth => _playerHealth == null ? 0f : (float)_playerHealth.MaximumHealth;
        public float BarrierCapacity => Mathf.Max(0f, CurrentTuning.StartingBarrierCapacity + BarrierCapacityBonus);
        public Vector3 PlayerPosition => _playerObject == null ? transform.position : _playerObject.transform.position;
        public Vector3 PlayerForward => _playerObject == null ? Vector3.forward : _playerObject.transform.forward;
        public CombatCatalog CombatCatalog => _combatCatalog;
        public SurvivorsPacingProfile CurrentPacingProfile => CurrentTuning.PacingProfile;
        public bool IsHumanPlaytestPacing => CurrentPacingProfile == SurvivorsPacingProfile.HumanPlaytest;
        public bool IsDebugFastPacing => CurrentPacingProfile == SurvivorsPacingProfile.DebugFast;
        public bool IsSprintRunMode => CurrentPacingProfile == SurvivorsPacingProfile.SprintRun;
        public bool IsRunModeSelectionOpen => _runModeSelectionOpen;
        public string CurrentRunModeDisplayName => CurrentTuning.RunModeDisplayName;
        public bool IsDebugOverlayVisible => _debugOverlayVisible;
        public bool IsBuildMenuOpen => _buildMenuOpen;
        public string CurrentBuildMenuTabLabel => FormatBuildMenuTabLabel(_buildMenuTab);
        public string CurrentUiThemeName => ActiveUiTheme.themeName;
        public IReadOnlyList<SurvivorsUiTheme> AvailableUiThemesForTest => _availableUiThemes;
        public int SelectedUiThemeIndex => _selectedUiThemeIndex;
        public bool IsTutorialOverlayOpen => _tutorialOverlayOpen;
        public bool IsTutorialSeen => _metaProgression != null && _metaProgression.TutorialSeen;
        public string CurrentTutorialStepTitle => ResolveTutorialStepTitle(ClampTutorialStepIndex(_tutorialStepIndex));
        public int CurrentTutorialStepIndex => ClampTutorialStepIndex(_tutorialStepIndex);
        public int TutorialStepCount => Enum.GetValues(typeof(TutorialStep)).Length;
        public bool IsAudioMuted => _audioMuted;
        public int AudioEventDispatchCount => _audioEventDispatchCount;
        public string LastAudioEventId => _lastAudioEventId;
        public string LastRunSummaryTitle => _lastRunSummaryTitle;
        public bool IsRunSummaryVisible => State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory;
        public bool IsPlayerFacingDraftOverlayVisible => State == SurvivorsRunState.LevelUp && (_currentDraft != null || _currentRelicDraft != null);
        public int CurrentDraftCardCountForTest => IsRelicChoiceOpen ? CurrentRelicChoices.Count : CurrentDraftChoices.Count;
        public string CurrentDraftOverlayTitleForTest => ResolveRewardOverlayTitle();
        public SurvivorsTemplateTuning CurrentTuning => tuning ?? (tuning = CreateConfiguredTuning(pacingProfile));
        public SurvivorsRunFlowDefinition CurrentRunFlowDefinition => _runFlow == null ? null : _runFlow.Definition;
        public bool IsAuthoredContentBound => _authoredContent != null;
        public bool IsStrictAuthoredSample => _authoredBindingPolicy == SurvivorsAuthoredContentBindingPolicy.StrictSample;
        public bool IsFallbackContentActive => !IsStrictAuthoredSample && (_authoredContent == null || _authoredContent.UsesBuiltInFallbacks);
        public bool CanStartConfiguredRun => !IsStrictAuthoredSample || (_strictSampleContentReady && _authoredContent != null);
        public bool IsUsingAuthoredRunFlow => _usingAuthoredRunFlow;
        public string AuthoredContentStatus => _authoredContentStatus;
        public string TopCenterTimerHudLabel => _runStarted ? ResolveTopCenterTimerHudLabel() : string.Empty;
        public bool IsTopCenterTimerVisible => _runStarted;
        public Rect TopCenterTimerRectForTest => ResolveTopCenterTimerRect();
        public float CurrentEnemySpawnIntervalSeconds => ResolveEnemySpawnIntervalSeconds();
        public int CurrentEnemySpawnPackSize => ResolveEnemySpawnPackSize();
        public int CurrentEnemyMaximumAlive => ResolveEnemyMaximumAlive();
        public float CurrentEnemySpeedMultiplier => _runFlow == null ? 1f : _runFlow.ResolveEnemySpeedMultiplier();
        public IReadOnlyList<RunUpgradeDefinition> CurrentDraftChoices => _currentDraft == null ? EmptyChoices : _currentDraft.Choices;
        public IReadOnlyList<SurvivorsRelicDefinition> CurrentRelicChoices => _currentRelicDraft == null ? EmptyRelicChoices : _currentRelicDraft.Choices;
        public float RewardSelectionRemainingSeconds => Mathf.Max(0f, _rewardSelectionTimer);
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
        public bool IsRunStarted => _runStarted;
        public bool IsLevelUpOpen => State == SurvivorsRunState.LevelUp;
        public bool IsRunUpgradeDraftOpen => State == SurvivorsRunState.LevelUp && _rewardSelectionKind == SurvivorsRewardSelectionKind.LevelUp;
        public bool IsRelicChoiceOpen => State == SurvivorsRunState.LevelUp && _rewardSelectionKind == SurvivorsRewardSelectionKind.BossRelic;
        public bool IsUpgradeRewardChoiceOpen => State == SurvivorsRunState.LevelUp && IsRewardUpgradeSelectionKind(_rewardSelectionKind);
        public bool IsGameOver => State == SurvivorsRunState.GameOver;
        public bool IsVictory => State == SurvivorsRunState.Victory;
        public bool HasClearedVictoryThisRun => _victoryClearedThisRun;
        public bool IsEndlessRun => State == SurvivorsRunState.Playing && _victoryClearedThisRun;
        public bool IsLowHealthWarningActive => (State == SurvivorsRunState.Playing || State == SurvivorsRunState.LevelUp) && MaxHealth > 0f && CurrentHealth / MaxHealth <= LowHealthWarningThreshold;
        public bool IsMajorThreatWarningActive => !string.IsNullOrEmpty(_majorThreatWarningLabel) && RunTimeSeconds < _majorThreatWarningTargetTimeSeconds;
        public string CurrentMajorThreatWarningLabel => IsMajorThreatWarningActive ? _majorThreatWarningLabel : string.Empty;
        public float MajorThreatWarningRemainingSeconds => IsMajorThreatWarningActive ? Mathf.Max(0f, _majorThreatWarningTargetTimeSeconds - RunTimeSeconds) : 0f;
        public bool IsHordeRushWarningActive => !string.IsNullOrEmpty(_hordeRushWarningLabel) && RunTimeSeconds < _hordeRushWarningTargetTimeSeconds;
        public string CurrentHordeRushWarningLabel => IsHordeRushWarningActive ? _hordeRushWarningLabel : string.Empty;
        public float HordeRushWarningRemainingSeconds => IsHordeRushWarningActive ? Mathf.Max(0f, _hordeRushWarningTargetTimeSeconds - RunTimeSeconds) : 0f;
        public float NextHordeRushTimeSecondsForTest => _nextHordeRushTimeSeconds;
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
        public string ActiveRewardFeedbackLabel => _rewardFeedbackTimer > 0f ? _rewardFeedbackLabel : string.Empty;
        public float RewardFeedbackRemainingSeconds => Mathf.Max(0f, _rewardFeedbackTimer);
        public string ActiveStreakRewardFeedbackLabel => _streakRewardFeedbackTimer > 0f ? _streakRewardFeedbackLabel : string.Empty;
        public float StreakRewardFeedbackRemainingSeconds => Mathf.Max(0f, _streakRewardFeedbackTimer);
        public string ActiveClassUnlockRewardFeedbackLabel => _classUnlockRewardFeedbackTimer > 0f ? _classUnlockRewardFeedbackLabel : string.Empty;
        public float ClassUnlockRewardFeedbackRemainingSeconds => Mathf.Max(0f, _classUnlockRewardFeedbackTimer);
        public string ActiveEvolutionReadyFeedbackLabel => _evolutionReadyFeedbackTimer > 0f ? _evolutionReadyFeedbackLabel : string.Empty;
        public float EvolutionReadyFeedbackRemainingSeconds => Mathf.Max(0f, _evolutionReadyFeedbackTimer);
        public string CurrentEvolutionGoalHudLabel => ResolveEvolutionGoalHudLabel();
        public string CurrentEvolutionReadyHudLabel => ResolveEvolutionReadyHudLabel();
        public string ActiveExperienceComboFeedbackLabel => _experienceComboFeedbackTimer > 0f ? _experienceComboFeedbackLabel : string.Empty;
        public float ExperienceComboFeedbackRemainingSeconds => Mathf.Max(0f, _experienceComboFeedbackTimer);
        public int CurrentExperienceComboPickupCount => _experienceComboTimer > 0f ? _experienceComboPickupCount : 0;
        public int CurrentExperienceComboAmount => _experienceComboTimer > 0f ? _experienceComboAmount : 0;
        public int RequiredExperienceForNextLevel => Mathf.Max(1, CurrentTuning.ExperienceRequiredBase + ((Level - 1) * CurrentTuning.ExperienceRequiredPerLevel));
        public int TotalDraftRerollCharges => Mathf.Max(0, CurrentTuning.DraftRerollCharges + PersistentDraftRerollBonus);
        public int DraftRerollsRemaining => Mathf.Max(0, TotalDraftRerollCharges - DraftRerollCount);
        public int DraftBanishesRemaining => Mathf.Max(0, CurrentTuning.DraftBanishCharges - DraftBanishCount);
        public int DraftSkipBloodShards => Mathf.Max(0, CurrentTuning.DraftSkipBloodShards);
        public float FirstKillTimeSeconds => _firstKillTimeSeconds;
        public float FirstExperiencePickupTimeSeconds => _firstExperiencePickupTimeSeconds;
        public float FirstLevelUpDraftTimeSeconds => _firstLevelUpDraftTimeSeconds;
        public float FirstEliteSpawnTimeSeconds => _firstEliteSpawnTimeSeconds;
        public float FirstEliteKillTimeSeconds => _firstEliteKillTimeSeconds;
        public float FirstMinibossSpawnTimeSeconds => _firstMinibossSpawnTimeSeconds;
        public float FirstMinibossKillTimeSeconds => _firstMinibossKillTimeSeconds;
        public float FirstBossSpawnTimeSeconds => _firstBossSpawnTimeSeconds;
        public float FirstBossKillTimeSeconds => _firstBossKillTimeSeconds;
        public float FirstEvolutionEligibilityTimeSeconds => _firstEvolutionEligibilityTimeSeconds;
        public float FirstEvolutionAcquiredTimeSeconds => _firstEvolutionAcquiredTimeSeconds;
        public float DamageTakenThisRun => _damageTakenThisRun;
        public int ThrottledExperienceOverflow { get; private set; }
        public int DraftOpenCount => _draftOpenCount;

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

        private void Start()
        {
            if (showRunModeSelection && !_runStarted)
            {
                _runModeSelectionOpen = true;
                autoStart = false;
                return;
            }

            if (autoStart && !_runStarted)
            {
                StartRun();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _debugOverlayVisible = !_debugOverlayVisible;
            }

            if (!_runStarted)
            {
                if (_tutorialOverlayOpen)
                {
                    HandleTutorialInput();
                }
                else if (_runModeSelectionOpen)
                {
                    HandleRunModeSelectionInput();
                }

                return;
            }

            if (_tutorialOverlayOpen)
            {
                HandleTutorialInput();
                return;
            }

            if (HandleBuildMenuInput())
            {
                return;
            }

            if (State == SurvivorsRunState.LevelUp)
            {
                TickDamagePopups(Time.deltaTime);
                TickWorldFeedbackEffects(Time.deltaTime);
                TickEnemyRangedAttackFeedbackEffects(Time.deltaTime);
                TickMajorThreatSlamTelegraphEffects(Time.deltaTime);
                TickIncomingThreatTelegraphEffects(Time.deltaTime);
                TickMajorRewardDropFeedbackEffects(Time.deltaTime);
                TickRewardFeedback(Time.deltaTime);
                TickStreakRewardFeedback(Time.deltaTime);
                TickClassUnlockRewardFeedback(Time.deltaTime);
                TickEvolutionReadyFeedback(Time.deltaTime);
                TickExperienceComboFeedback(Time.deltaTime);
                TickRewardSelectionTimeout(Time.deltaTime);
                HandleLevelUpInput();
                return;
            }

            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                TickDamagePopups(Time.deltaTime);
                TickWorldFeedbackEffects(Time.deltaTime);
                TickEnemyRangedAttackFeedbackEffects(Time.deltaTime);
                TickMajorThreatSlamTelegraphEffects(Time.deltaTime);
                TickIncomingThreatTelegraphEffects(Time.deltaTime);
                TickMajorRewardDropFeedbackEffects(Time.deltaTime);
                TickRewardFeedback(Time.deltaTime);
                TickStreakRewardFeedback(Time.deltaTime);
                TickClassUnlockRewardFeedback(Time.deltaTime);
                TickEvolutionReadyFeedback(Time.deltaTime);
                TickExperienceComboFeedback(Time.deltaTime);
                if (HandleResultMetaUpgradeInput())
                {
                    return;
                }

                if (State == SurvivorsRunState.Victory && Input.GetKeyDown(KeyCode.C))
                {
                    ContinueAfterVictory();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartRun();
                }
                return;
            }

            Vector2 movement = ReadMovementInput();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryDash(movement);
            }

            Simulate(Time.deltaTime, movement);

            if (Input.GetKeyDown(KeyCode.M))
            {
                TriggerMagnetRecall();
            }
        }

        private void LateUpdate()
        {
            UpdateArenaPresentation();
            if (_camera == null || _playerObject == null)
            {
                return;
            }

            Vector3 target = _playerObject.transform.position + new Vector3(0f, 13.5f, -9.5f);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, target, 14f * Time.deltaTime);
            _camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }

        private void OnGUI()
        {
            if (!_runStarted)
            {
                if (_runModeSelectionOpen)
                {
                    EnsureHudStyles();
                    DrawRunModeSelectionOverlay();
                }

                if (_tutorialOverlayOpen)
                {
                    EnsureHudStyles();
                    DrawTutorialOverlay();
                }

                return;
            }

            EnsureHudStyles();
            DrawTopCenterTimerHud();
            DrawPlayerHud();
            if (_debugOverlayVisible)
            {
                DrawDebugOverlay();
                DrawBuildHudPanel();
            }

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

            if (_tutorialOverlayOpen)
            {
                DrawTutorialOverlay();
                return;
            }

            if (State == SurvivorsRunState.LevelUp)
            {
                DrawLevelUpOverlay();
            }
            else if (State == SurvivorsRunState.GameOver)
            {
                DrawRunResultOverlay(victory: false);
            }
            else if (State == SurvivorsRunState.Victory)
            {
                DrawRunResultOverlay(victory: true);
            }

            if (_buildMenuOpen && State == SurvivorsRunState.Playing)
            {
                DrawBuildMenuOverlay();
            }
        }

        private void HandleRunModeSelectionInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Return))
            {
                SelectStandardRun();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.S))
            {
                SelectSprintRun();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                OpenTutorialOverlay(markUnseen: false);
            }
        }

        private bool HandleBuildMenuInput()
        {
            if (State != SurvivorsRunState.Playing)
            {
                _buildMenuOpen = false;
                return false;
            }

            bool menuToggle = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B);
            if (menuToggle)
            {
                _buildMenuOpen = !_buildMenuOpen;
                if (_buildMenuOpen)
                {
                    _buildMenuTab = BuildMenuTab.CurrentBuild;
                }

                return _buildMenuOpen;
            }

            if (!_buildMenuOpen)
            {
                return false;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _buildMenuTab = BuildMenuTab.CurrentBuild;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _buildMenuTab = BuildMenuTab.Stats;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _buildMenuTab = BuildMenuTab.RunInfo;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _buildMenuTab = BuildMenuTab.Controls;
            }

            return true;
        }

        private void DrawPlayerHud()
        {
            IReadOnlyList<string> lines = ResolvePlayerHudLines();
            float panelWidth = Mathf.Min(340f, Mathf.Max(270f, Screen.width - 32f));
            float lineHeight = 19f;
            float panelHeight = 126f + Mathf.Min(4, lines.Count) * lineHeight;
            Rect panel = new Rect(12f, 58f, panelWidth, panelHeight);
            DrawSolidRect(panel, new Color(0.015f, 0.02f, 0.026f, 0.74f));
            DrawSolidRect(new Rect(panel.x, panel.y, 4f, panel.height), new Color(0.2f, 0.78f, 1f, 0.9f));
            GUI.Label(new Rect(panel.x + 14f, panel.y + 8f, panel.width - 28f, 22f), CurrentRunModeDisplayName, _hudTitleStyle);
            DrawHudBar(new Rect(panel.x + 14f, panel.y + 36f, panel.width - 28f, 18f), "Health", MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth, new Color(0.9f, 0.22f, 0.24f));
            if (BarrierCapacity > 0.01f)
            {
                DrawHudBar(new Rect(panel.x + 14f, panel.y + 60f, panel.width - 28f, 18f), "Barrier", BarrierValue / BarrierCapacity, new Color(0.42f, 0.8f, 1f));
            }

            DrawHudBar(new Rect(panel.x + 14f, panel.y + 84f, panel.width - 28f, 18f), $"XP L{Level}", Experience / (float)RequiredExperienceForNextLevel, new Color(0.2f, 0.78f, 1f));
            float y = panel.y + 110f;
            int shown = Mathf.Min(4, lines.Count);
            for (int i = 0; i < shown; i++)
            {
                GUI.Label(new Rect(panel.x + 14f, y, panel.width - 28f, lineHeight), lines[i], _hudSmallStyle);
                y += lineHeight;
            }
        }

        private void DrawDebugOverlay()
        {
            string evolutionObjectiveHud = ResolveEvolutionObjectiveHudLabel();
            string waystoneCompassHud = ResolveWaystoneCompassHudLabel();
            float panelHeight = string.IsNullOrWhiteSpace(evolutionObjectiveHud) ? 424f : 448f;
            Rect panel = new Rect(12f, 58f + Mathf.Min(206f, Screen.height * 0.22f), 356f, panelHeight);
            DrawSolidRect(panel, new Color(0.02f, 0.024f, 0.03f, 0.86f));
            DrawSolidRect(new Rect(panel.x, panel.y, 4f, panel.height), new Color(1f, 0.72f, 0.22f, 0.86f));
            GUI.Label(new Rect(panel.x + 12f, panel.y + 10f, 300f, 22f), "Survivors Debug Overlay", _hudTitleStyle);
            DrawHudBar(new Rect(panel.x + 12f, panel.y + 38f, 318f, 18f), "Health", MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth, new Color(0.9f, 0.22f, 0.24f));
            DrawHudBar(new Rect(panel.x + 12f, panel.y + 62f, 318f, 18f), "Barrier", BarrierCapacity <= 0f ? 0f : BarrierValue / BarrierCapacity, new Color(0.42f, 0.8f, 1f));
            DrawHudBar(new Rect(panel.x + 12f, panel.y + 86f, 318f, 18f), "XP", Experience / (float)RequiredExperienceForNextLevel, new Color(0.2f, 0.78f, 1f));
            DrawHudBar(new Rect(panel.x + 12f, panel.y + 110f, 318f, 18f), "Run", Mathf.Clamp01(RunTimeSeconds / Mathf.Max(1f, CurrentTuning.SurvivalVictoryTimeSeconds)), new Color(0.72f, 0.44f, 1f));
            GUI.Label(new Rect(panel.x + 12f, panel.y + 136f, 318f, 22f), $"LV {Level}   Time {FormatRunTime(RunTimeSeconds)}   Phase {ResolveRunPhaseHudLabel()} +{RunEscalationLevel}", _hudLabelStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 158f, 318f, 22f), CurrentRunMilestoneHudLabel, _hudLabelStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 180f, 318f, 22f), $"Enemies {ActiveEnemyCount}/{CurrentEnemyMaximumAlive}   Kills {KilledCount}", _hudLabelStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 202f, 318f, 22f), $"Split {ActiveSplitterCount}   Call {ActiveSummonerCount}   Elite {ActiveEliteCount}   Mini {ActiveMinibossCount}   Boss {ActiveBossCount}", _hudLabelStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 224f, 318f, 22f), $"{CurrencyDisplayName} {MetaBloodShards}   Poison {PoisonDamageRatio:0.##}   Bleed {BleedDamageRatio:0.##}   Execute {ExecuteThresholdNormalized:P0}", _hudSmallStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 246f, 318f, 22f), "Weapons: " + ResolveWeaponHudLabel(), _hudSmallStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 268f, 318f, 22f), $"Mode {CurrentRunModeDisplayName}   Profile {BasicSurvivorsGame.GetPacingProfileDisplayName(CurrentPacingProfile)}", _hudSmallStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 290f, 318f, 22f), $"Spawn {CurrentEnemySpawnIntervalSeconds:0.00}s   Enemy Speed x{CurrentEnemySpeedMultiplier:0.##}", _hudSmallStyle);
            string surgeHud = ResolveSurgeHudLabel();
            GUI.Label(new Rect(panel.x + 12f, panel.y + 312f, 318f, 22f), $"Streak {CurrentKillStreak}   Best {BestKillStreak}   Bonus Drops {StreakBonusDropCount}{surgeHud}", _hudSmallStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 334f, 318f, 22f), $"Reward Timeout {FormatRewardTimeout(CurrentTuning.RewardSelectionTimeoutSeconds)}   Reroll {DraftRerollsRemaining}   Banish {DraftBanishesRemaining}", _hudSmallStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 356f, 318f, 22f), ResolveBuildSlotHudLabel(), _hudSmallStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 378f, 318f, 22f), waystoneCompassHud, _hudSmallStyle);
            if (!string.IsNullOrWhiteSpace(evolutionObjectiveHud))
            {
                GUI.Label(new Rect(panel.x + 12f, panel.y + 400f, 318f, 22f), evolutionObjectiveHud, _hudSmallStyle);
            }

            GUI.Label(new Rect(panel.x + 12f, string.IsNullOrWhiteSpace(evolutionObjectiveHud) ? panel.y + 400f : panel.y + 422f, 318f, 22f), ResolveDashHudLabel(), _hudSmallStyle);
        }

        private void DrawBuildMenuOverlay()
        {
            DrawDimOverlay(0.62f);
            Rect panel = ResolveCenteredPanelRect(860f, 620f, 360f, 360f, 24f);
            DrawSolidRect(panel, new Color(0.018f, 0.023f, 0.032f, 0.96f));
            Color accent = ActiveUiTheme.GetHudAccentColor(new Color(0.2f, 0.78f, 1f));
            DrawSolidRect(new Rect(panel.x, panel.y, panel.width, 4f), new Color(accent.r, accent.g, accent.b, 0.9f));
            GUI.Label(new Rect(panel.x + 24f, panel.y + 18f, panel.width - 48f, 32f), ActiveUiTheme.buildMenuTitle, _menuTitleStyle);
            if (GUI.Button(new Rect(panel.xMax - 94f, panel.y + 18f, 70f, 28f), "Close"))
            {
                PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
                _buildMenuOpen = false;
                return;
            }

            DrawBuildMenuTabs(new Rect(panel.x + 24f, panel.y + 62f, panel.width - 48f, 34f));
            IReadOnlyList<string> lines = ResolveBuildMenuLines(_buildMenuTab);
            Rect viewRect = new Rect(panel.x + 24f, panel.y + 110f, panel.width - 48f, panel.height - 132f);
            float lineHeight = 22f;
            Rect contentRect = new Rect(0f, 0f, Mathf.Max(1f, viewRect.width - 18f), Mathf.Max(viewRect.height, lines.Count * lineHeight + 10f));
            _buildMenuScrollPosition = GUI.BeginScrollView(viewRect, _buildMenuScrollPosition, contentRect);
            float y = 0f;
            for (int i = 0; i < lines.Count; i++)
            {
                GUI.Label(new Rect(0f, y, contentRect.width, lineHeight), lines[i], _hudLabelStyle);
                y += lineHeight;
            }
            GUI.EndScrollView();
        }

        private void DrawBuildMenuTabs(Rect rect)
        {
            float gap = 8f;
            float width = (rect.width - gap * 3f) / 4f;
            DrawBuildMenuTabButton(new Rect(rect.x, rect.y, width, rect.height), BuildMenuTab.CurrentBuild, "1 Current Build");
            DrawBuildMenuTabButton(new Rect(rect.x + (width + gap), rect.y, width, rect.height), BuildMenuTab.Stats, "2 Stats");
            DrawBuildMenuTabButton(new Rect(rect.x + (width + gap) * 2f, rect.y, width, rect.height), BuildMenuTab.RunInfo, "3 Run Info");
            DrawBuildMenuTabButton(new Rect(rect.x + (width + gap) * 3f, rect.y, width, rect.height), BuildMenuTab.Controls, "4 Controls");
        }

        private void DrawBuildMenuTabButton(Rect rect, BuildMenuTab tab, string label)
        {
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = _buildMenuTab == tab ? new Color(0.35f, 0.76f, 1f) : new Color(0.42f, 0.48f, 0.56f);
            if (GUI.Button(rect, label, _menuTabStyle))
            {
                _buildMenuTab = tab;
            }

            GUI.backgroundColor = oldColor;
        }

        private void DrawRunModeSelectionOverlay()
        {
            Rect rect = ResolveCenteredPanelRect(820f, 430f, 340f, 360f, 20f);
            float width = rect.width;
            GUI.Box(rect, "Choose Run Mode");
            GUI.Label(new Rect(rect.x + 28f, rect.y + 34f, width - 56f, 28f), "Deucarian Survivors Run", _hudTitleStyle);
            GUI.Label(new Rect(rect.x + 28f, rect.y + 64f, width - 56f, 22f), "Pick Standard / Human Playtest for the full 30-minute arc or Sprint Run for the compact five-minute loop.", _hudLabelStyle);

            float viewOffset = 98f;
            float viewBottomPadding = 154f;
            if (IsStrictAuthoredSample && !CanStartConfiguredRun)
            {
                GUI.Label(new Rect(rect.x + 28f, rect.y + 86f, width - 56f, 34f), _authoredContentStatus, _hudSmallStyle);
                viewOffset = 122f;
                viewBottomPadding = 178f;
            }

            Rect viewRect = new Rect(rect.x + 28f, rect.y + viewOffset, width - 56f, rect.height - viewBottomPadding);
            float contentWidth = Mathf.Max(1f, viewRect.width - 18f);
            bool narrow = contentWidth < 700f;
            float cardGap = 16f;
            float cardWidth = narrow ? contentWidth : (contentWidth - cardGap) * 0.5f;
            float cardHeight = narrow ? 132f : 190f;
            float selectorY = narrow ? cardHeight * 2f + cardGap + 12f : cardHeight + 14f;
            float selectorHeight = contentWidth < 360f ? 64f : 46f;
            float contentHeight = Mathf.Max(viewRect.height, selectorY + selectorHeight);

            _modeSelectionScrollPosition = GUI.BeginScrollView(viewRect, _modeSelectionScrollPosition, new Rect(0f, 0f, contentWidth, contentHeight));
            Rect standardRect = new Rect(0f, 0f, cardWidth, cardHeight);
            Rect sprintRect = narrow
                ? new Rect(0f, standardRect.yMax + cardGap, cardWidth, cardHeight)
                : new Rect(standardRect.xMax + cardGap, 0f, cardWidth, cardHeight);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && CanStartConfiguredRun;
            if (DrawRunModeCard(standardRect, SurvivorsPacingProfile.HumanPlaytest, "Start Standard"))
            {
                SelectStandardRun();
            }

            if (DrawRunModeCard(sprintRect, SurvivorsPacingProfile.SprintRun, "Start Sprint"))
            {
                SelectSprintRun();
            }
            GUI.enabled = previousEnabled;

            DrawThemeSelector(new Rect(0f, selectorY, contentWidth, 42f));
            GUI.EndScrollView();

            if (GUI.Button(new Rect(rect.x + 28f, rect.yMax - 42f, Mathf.Min(180f, width - 56f), 30f), ActiveUiTheme.showTutorialButtonLabel + " (T)"))
            {
                OpenTutorialOverlay(markUnseen: false);
            }
        }

        private bool DrawRunModeCard(Rect rect, SurvivorsPacingProfile profile, string buttonLabel)
        {
            SurvivorsTemplateTuning preview = CreateConfiguredTuning(profile);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 16f, rect.width - 32f, 24f), FormatRunModeSelectionTitle(profile, preview), _hudLabelStyle);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 44f, rect.width - 32f, 20f), preview.RunModeDurationLabel, _hudSmallStyle);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 70f, rect.width - 32f, 42f), preview.RunModeDescription, _hudSmallStyle);
            if (rect.height > 150f)
            {
                GUI.Label(new Rect(rect.x + 16f, rect.y + 118f, rect.width - 32f, 20f), $"Boss {FormatRunTime(preview.BossSpawnTimeSeconds)}   Victory {FormatRunTime(preview.SurvivalVictoryTimeSeconds)}", _hudSmallStyle);
                GUI.Label(new Rect(rect.x + 16f, rect.y + 140f, rect.width - 32f, 20f), $"Profile {GetPacingProfileDisplayNameForCard(profile)}", _hudSmallStyle);
            }

            bool hovered = rect.Contains(Event.current.mousePosition);
            if (hovered)
            {
                PlayAudioEvent(AudioEventUiHover, _pickupClip, 0.08f);
            }

            bool clicked = GUI.Button(new Rect(rect.x + 16f, rect.yMax - 38f, rect.width - 32f, 30f), buttonLabel);
            if (clicked)
            {
                PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
            }

            return clicked;
        }

        private void DrawThemeSelector(Rect rect)
        {
            EnsureUiTheme();
            bool stacked = rect.width < 360f;
            GUI.Label(new Rect(rect.x, rect.y, stacked ? rect.width : Mathf.Min(96f, rect.width), 24f), ActiveUiTheme.themeSelectorTitle, _hudLabelStyle);
            float x = stacked ? rect.x : rect.x + 104f;
            float y = stacked ? rect.y + 24f : rect.y;
            float availableWidth = Mathf.Max(1f, rect.xMax - x);
            float width = Mathf.Max(96f, Mathf.Min(180f, (availableWidth - 8f) / Mathf.Max(1, _availableUiThemes.Count)));
            for (int i = 0; i < _availableUiThemes.Count; i++)
            {
                SurvivorsUiTheme option = _availableUiThemes[i];
                if (option == null)
                {
                    continue;
                }

                Rect button = new Rect(x + i * (width + 8f), y, width, 32f);
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = i == _selectedUiThemeIndex
                    ? ActiveUiTheme.GetHudAccentColor(new Color(0.2f, 0.78f, 1f))
                    : new Color(0.72f, 0.72f, 0.76f);
                if (GUI.Button(button, option.themeName))
                {
                    SelectUiTheme(i);
                }

                GUI.backgroundColor = oldColor;
            }
        }

        private bool SelectUiTheme(int index)
        {
            EnsureUiTheme();
            if (index < 0 || index >= _availableUiThemes.Count || _availableUiThemes[index] == null)
            {
                return false;
            }

            _selectedUiThemeIndex = index;
            uiTheme = _availableUiThemes[index];
            ResetHudStyles();
            ApplyWorldPresentation();
            PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
            return true;
        }

        private void HandleTutorialInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.S))
            {
                CloseTutorialOverlay(markSeen: true);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                AdvanceTutorialStep();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Backspace))
            {
                BackTutorialStep();
            }
        }

        private void TryOpenFirstRunTutorial()
        {
            EnsureMetaProgressionLoaded();
            if (_metaProgression != null && !_metaProgression.TutorialSeen)
            {
                OpenTutorialOverlay(markUnseen: true);
            }
        }

        private bool OpenTutorialOverlay(bool markUnseen)
        {
            EnsureMetaProgressionLoaded();
            _tutorialOverlayOpen = true;
            _tutorialStepIndex = 0;
            _buildMenuOpen = false;
            if (markUnseen && _metaProgression != null && _metaProgression.TutorialSeen)
            {
                _metaProgression.ResetTutorialSeen();
            }

            PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
            return true;
        }

        private bool CloseTutorialOverlay(bool markSeen)
        {
            if (!_tutorialOverlayOpen && !markSeen)
            {
                return false;
            }

            _tutorialOverlayOpen = false;
            if (markSeen)
            {
                EnsureMetaProgressionLoaded();
                _metaProgression?.MarkTutorialSeen();
            }

            PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
            return true;
        }

        private bool AdvanceTutorialStep()
        {
            int max = TutorialStepCount - 1;
            if (_tutorialStepIndex >= max)
            {
                return CloseTutorialOverlay(markSeen: true);
            }

            _tutorialStepIndex = Mathf.Min(max, _tutorialStepIndex + 1);
            PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
            return true;
        }

        private bool BackTutorialStep()
        {
            if (!_tutorialOverlayOpen || _tutorialStepIndex <= 0)
            {
                return false;
            }

            _tutorialStepIndex--;
            PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
            return true;
        }

        private void DrawTutorialOverlay()
        {
            DrawDimOverlay(0.7f);
            Rect panel = ResolveCenteredPanelRect(720f, 420f, 320f, 330f, 22f);
            Color accent = ActiveUiTheme.GetHudAccentColor(new Color(0.2f, 0.78f, 1f));
            DrawSolidRect(panel, new Color(0.016f, 0.02f, 0.03f, 0.97f));
            DrawSolidRect(new Rect(panel.x, panel.y, panel.width, 4f), new Color(accent.r, accent.g, accent.b, 0.94f));
            int step = ClampTutorialStepIndex(_tutorialStepIndex);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 20f, panel.width - 48f, 32f), ActiveUiTheme.tutorialTitle, _menuTitleStyle);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 56f, panel.width - 48f, 28f), $"{step + 1}/{TutorialStepCount}  {ResolveTutorialStepTitle(step)}", _hudLabelStyle);

            IReadOnlyList<string> lines = ResolveTutorialStepLines(step);
            float y = panel.y + 104f;
            for (int i = 0; i < lines.Count; i++)
            {
                GUI.Label(new Rect(panel.x + 28f, y, panel.width - 56f, 34f), lines[i], _draftCardDescriptionStyle);
                y += 42f;
            }

            float buttonY = panel.yMax - 52f;
            if (GUI.Button(new Rect(panel.x + 24f, buttonY, 108f, 34f), "Back"))
            {
                BackTutorialStep();
            }

            if (GUI.Button(new Rect(panel.x + 144f, buttonY, 120f, 34f), "Skip"))
            {
                CloseTutorialOverlay(markSeen: true);
            }

            string nextLabel = step >= TutorialStepCount - 1 ? "Finish" : "Next";
            if (GUI.Button(new Rect(panel.xMax - 152f, buttonY, 128f, 34f), nextLabel))
            {
                AdvanceTutorialStep();
            }
        }

        private static int ClampTutorialStepIndex(int index)
        {
            int max = Enum.GetValues(typeof(TutorialStep)).Length - 1;
            return Mathf.Clamp(index, 0, Mathf.Max(0, max));
        }

        private string ResolveTutorialStepTitle(int step)
        {
            EnsureUiTheme();
            int resolvedStep = ClampTutorialStepIndex(step);
            return ActiveUiTheme.GetTutorialStepTitle(resolvedStep, ResolveDefaultTutorialStepTitle(resolvedStep));
        }

        private static string ResolveDefaultTutorialStepTitle(int step)
        {
            switch ((TutorialStep)ClampTutorialStepIndex(step))
            {
                case TutorialStep.Combat:
                    return "Move And Survive";
                case TutorialStep.Experience:
                    return "Collect XP Gems";
                case TutorialStep.Drafts:
                    return "Choose A Build";
                case TutorialStep.ElitesBosses:
                    return "Elites, Bosses, Rewards";
                case TutorialStep.Evolutions:
                    return "Evolve Weapons";
                case TutorialStep.Modes:
                    return "Pick A Run Mode";
                default:
                    return "Move To Survive";
            }
        }

        private IReadOnlyList<string> ResolveTutorialStepLines(int step)
        {
            EnsureUiTheme();
            int resolvedStep = ClampTutorialStepIndex(step);
            return ActiveUiTheme.GetTutorialStepLines(resolvedStep, ResolveDefaultTutorialStepLines(resolvedStep));
        }

        private static IReadOnlyList<string> ResolveDefaultTutorialStepLines(int step)
        {
            switch ((TutorialStep)ClampTutorialStepIndex(step))
            {
                case TutorialStep.Combat:
                    return new[] { "Move with WASD or the left stick. Your weapons fire automatically at nearby enemies.", "Use Arc Step to dash through pressure, shove enemies back, and buy a short safety window.", "The goal is not to stand still. Kite, collect, and keep the horde just barely under control." };
                case TutorialStep.Experience:
                    return new[] { "Enemies drop blue XP gems. Move near them to pull them in and fill the level bar.", "Magnet pickups and pickup-radius upgrades help recover loose gems without flooding drafts.", "Streak rewards, horde clears, and waystones can add extra pickups when you play actively." };
                case TutorialStep.Drafts:
                    return new[] { "Level-ups pause the run and offer draft cards. Pick weapons, passives, mutations, and evolutions.", "Reroll changes the offered cards, Banish removes a card for the run, and Skip grants blood shards.", "Open the Build panel to compare current weapons, passives, relics, run info, and controls." };
                case TutorialStep.ElitesBosses:
                    return new[] { "Elites, dread elites, minibosses, and bosses keep their health, show bars, and stay tracked offscreen.", "Major threats drop recoverable reward caches, relic drafts, upgrade drafts, or class unlock rewards.", "Warnings, slam markers, and support-call banners tell you when the arena is about to spike." };
                case TutorialStep.Evolutions:
                    return new[] { "Evolutions need a ranked weapon path plus its matching passive. Ready banners call out missing pieces.", "Evolution picks trigger big payoff surges, XP recall, and stronger weapon behavior.", "Multiple evolutions can stack into a late-run Legend Surge." };
                case TutorialStep.Modes:
                    return new[] { "Standard / Human Playtest is the full 30-minute arc with victory, boss rewards, and endless continuation.", "Sprint Run compresses the game into 5 minutes with quicker XP, early elites, a faster boss climax, and fast restart.", "After victory or defeat you can restart the same mode or return to mode selection." };
                default:
                    return new[] { "Move to survive. Standing still becomes dangerous unless your build is already overpowering the arena.", "Arc Step with Space can buy room when enemies get close." };
            }
        }

        private static string GetPacingProfileDisplayNameForCard(SurvivorsPacingProfile profile)
        {
            return BasicSurvivorsGame.GetPacingProfileDisplayName(profile);
        }

        private static string FormatRunModeSelectionTitle(SurvivorsPacingProfile profile, SurvivorsTemplateTuning preview)
        {
            if (profile == SurvivorsPacingProfile.HumanPlaytest)
            {
                return "Standard / Human Playtest";
            }

            return preview == null || string.IsNullOrWhiteSpace(preview.RunModeDisplayName)
                ? BasicSurvivorsGame.GetPacingProfileDisplayName(profile)
                : preview.RunModeDisplayName;
        }

        public void ConfigureRunModeSelection(bool enabled)
        {
            showRunModeSelection = enabled;
            autoStart = !enabled;
            if (!_runStarted)
            {
                _runModeSelectionOpen = enabled;
                State = SurvivorsRunState.Booting;
            }
        }

        public bool ConfigureUiTheme(TextAsset themeLibrary)
        {
            return ConfigureUiThemeJson(themeLibrary == null ? null : themeLibrary.text);
        }

        public bool ConfigureUiThemeJson(string themeJson)
        {
            if (!SurvivorsUiTheme.TryFromJson(themeJson, out SurvivorsUiTheme parsed, out _))
            {
                uiTheme = SurvivorsUiTheme.CreateDefault();
                _availableUiThemes.Clear();
                _availableUiThemes.Add(uiTheme);
                _selectedUiThemeIndex = 0;
                ResetHudStyles();
                ApplyWorldPresentation();
                return false;
            }

            uiTheme = parsed;
            _availableUiThemes.Clear();
            _availableUiThemes.Add(uiTheme);
            _selectedUiThemeIndex = 0;
            ResetHudStyles();
            ApplyWorldPresentation();
            return true;
        }

        public bool ConfigureUiThemes(TextAsset defaultThemeLibrary, TextAsset alternateThemeLibrary)
        {
            bool configuredDefault = ConfigureUiThemeJson(defaultThemeLibrary == null ? null : defaultThemeLibrary.text);
            if (alternateThemeLibrary == null)
            {
                return configuredDefault;
            }

            return ConfigureAdditionalUiThemeJson(alternateThemeLibrary.text) && configuredDefault;
        }

        public bool ConfigureAdditionalUiThemeJson(string themeJson)
        {
            if (!SurvivorsUiTheme.TryFromJson(themeJson, out SurvivorsUiTheme parsed, out _))
            {
                EnsureUiTheme();
                return false;
            }

            EnsureUiTheme();
            if (_availableUiThemes.Count == 0)
            {
                _availableUiThemes.Add(uiTheme);
            }

            _availableUiThemes.Add(parsed);
            return true;
        }

        public bool SelectUiThemeForTest(int index)
        {
            return SelectUiTheme(index);
        }

        public bool ConfigureAuthoredContent(TextAsset enemyLibrary, TextAsset runFlowLibrary, TextAsset rewardLibrary)
        {
            _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.AllowFallbacks;
            _strictSampleContentReady = false;
            return ConfigureAuthoredContentJson(
                enemyLibrary == null ? null : enemyLibrary.text,
                runFlowLibrary == null ? null : runFlowLibrary.text,
                rewardLibrary == null ? null : rewardLibrary.text);
        }

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary)
        {
            return ConfigureAuthoredContent(
                weaponLibrary,
                upgradeLibrary,
                relicLibrary,
                classLibrary,
                progressionLibrary,
                enemyLibrary,
                runFlowLibrary,
                rewardLibrary,
                SurvivorsAuthoredContentBindingPolicy.StrictSample);
        }

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy)
        {
            return ConfigureAuthoredContentJson(
                weaponLibrary == null ? null : weaponLibrary.text,
                upgradeLibrary == null ? null : upgradeLibrary.text,
                relicLibrary == null ? null : relicLibrary.text,
                classLibrary == null ? null : classLibrary.text,
                progressionLibrary == null ? null : progressionLibrary.text,
                enemyLibrary == null ? null : enemyLibrary.text,
                runFlowLibrary == null ? null : runFlowLibrary.text,
                rewardLibrary == null ? null : rewardLibrary.text,
                bindingPolicy);
        }

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
            TextAsset alternateThemeLibrary)
        {
            _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.StrictSample;
            _strictSampleContentReady = false;
            TextAsset[] requiredAssets =
            {
                weaponLibrary,
                upgradeLibrary,
                relicLibrary,
                classLibrary,
                progressionLibrary,
                enemyLibrary,
                pickupLibrary,
                runFlowLibrary,
                rewardLibrary,
                defaultThemeLibrary,
                alternateThemeLibrary
            };
            for (int i = 0; i < requiredAssets.Length; i++)
            {
                if (requiredAssets[i] == null)
                {
                    return SetAuthoredBindingFailure($"Strict authored sample is missing required TextAsset at slot {i}.");
                }
            }

            SurvivorsContentValidationResult validation = SurvivorsContentValidator.ValidateSampleJson(
                weaponLibrary.text,
                upgradeLibrary.text,
                enemyLibrary.text,
                rewardLibrary.text,
                relicLibrary.text,
                classLibrary.text,
                progressionLibrary.text,
                pickupLibrary.text,
                runFlowLibrary.text,
                defaultThemeLibrary.text,
                alternateThemeLibrary.text);
            if (!validation.Succeeded)
            {
                return SetAuthoredBindingFailure("Strict authored sample validation failed: " + string.Join("; ", validation.Errors));
            }

            if (!ConfigureAuthoredContent(
                weaponLibrary,
                upgradeLibrary,
                relicLibrary,
                classLibrary,
                progressionLibrary,
                enemyLibrary,
                runFlowLibrary,
                rewardLibrary,
                SurvivorsAuthoredContentBindingPolicy.StrictSample))
            {
                return false;
            }

            if (!ConfigureUiThemes(defaultThemeLibrary, alternateThemeLibrary))
            {
                return SetAuthoredBindingFailure("Strict authored sample UI themes failed to bind.");
            }

            _strictSampleContentReady = true;
            _authoredContentStatus = "Strict authored Survivors sample bound: " + _authoredContent.SourceSummary;
            return true;
        }

        public bool ConfigureAuthoredContentJson(string enemyJson, string runFlowJson, string rewardJson)
        {
            _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.AllowFallbacks;
            _strictSampleContentReady = false;
            if (!SurvivorsAuthoredContentDefinition.TryCreate(
                enemyJson,
                runFlowJson,
                rewardJson,
                out SurvivorsAuthoredContentDefinition definition,
                out string error))
            {
                _authoredContent = null;
                _authoredContentStatus = string.IsNullOrWhiteSpace(error)
                    ? "Fallback policy active after authored Survivors content failed to bind."
                    : "Fallback policy active after authored Survivors content failed to bind: " + error;
                if (!_runStarted)
                {
                    tuning = CreateConfiguredTuning(pacingProfile);
                }

                ReleaseMetaProgressionService();
                return false;
            }

            _authoredContent = definition;
            _authoredContentStatus = "Authored Survivors content bound with fallback policy active: " + definition.SourceSummary;
            if (!_runStarted)
            {
                tuning = CreateConfiguredTuning(pacingProfile);
            }

            ReleaseMetaProgressionService();
            return true;
        }

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson)
        {
            return ConfigureAuthoredContentJson(
                weaponJson,
                upgradeJson,
                relicJson,
                classJson,
                progressionJson,
                enemyJson,
                runFlowJson,
                rewardJson,
                SurvivorsAuthoredContentBindingPolicy.StrictSample);
        }

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy)
        {
            _authoredBindingPolicy = bindingPolicy;
            _strictSampleContentReady = false;
            if (!SurvivorsAuthoredContentDefinition.TryCreate(
                weaponJson,
                upgradeJson,
                relicJson,
                classJson,
                progressionJson,
                enemyJson,
                runFlowJson,
                rewardJson,
                bindingPolicy,
                out SurvivorsAuthoredContentDefinition definition,
                out string error))
            {
                _authoredContent = null;
                _authoredContentStatus = string.IsNullOrWhiteSpace(error)
                    ? "Authored Survivors content failed to bind."
                    : "Authored Survivors content failed to bind: " + error;
                if (!_runStarted)
                {
                    tuning = CreateConfiguredTuning(pacingProfile);
                }

                ReleaseMetaProgressionService();
                return false;
            }

            _authoredContent = definition;
            _strictSampleContentReady = bindingPolicy == SurvivorsAuthoredContentBindingPolicy.StrictSample;
            _authoredContentStatus = definition.IsStrictSample
                ? "Strict authored Survivors content bound: " + definition.SourceSummary
                : "Authored Survivors content bound with fallback policy: " + definition.SourceSummary;
            if (!_runStarted)
            {
                tuning = CreateConfiguredTuning(pacingProfile);
            }

            ReleaseMetaProgressionService();
            return true;
        }

        private bool SetAuthoredBindingFailure(string message)
        {
            _authoredContent = null;
            _strictSampleContentReady = false;
            _authoredContentStatus = string.IsNullOrWhiteSpace(message)
                ? "Strict authored Survivors content failed to bind."
                : message;
            if (!_runStarted)
            {
                tuning = CreateConfiguredTuning(pacingProfile);
            }

            ReleaseMetaProgressionService();
            return false;
        }

        public void OpenRunModeSelection()
        {
            if (_runStarted)
            {
                ClearRun();
            }

            ClearRewardDrafts();
            showRunModeSelection = true;
            autoStart = false;
            _debugOverlayVisible = false;
            _buildMenuOpen = false;
            _buildMenuTab = BuildMenuTab.CurrentBuild;
            _buildMenuScrollPosition = Vector2.zero;
            _runSummaryScrollPosition = Vector2.zero;
            _draftCardScrollPosition = Vector2.zero;
            _modeSelectionScrollPosition = Vector2.zero;
            _tutorialOverlayOpen = false;
            _tutorialStepIndex = 0;
            _runModeSelectionOpen = true;
            State = SurvivorsRunState.Booting;
        }

        public bool SelectStandardRun()
        {
            return SelectRunMode(SurvivorsPacingProfile.HumanPlaytest);
        }

        public bool SelectSprintRun()
        {
            return SelectRunMode(SurvivorsPacingProfile.SprintRun);
        }

        public bool SelectRunMode(SurvivorsPacingProfile profile)
        {
            if (!CanStartConfiguredRun)
            {
                _runModeSelectionOpen = true;
                State = SurvivorsRunState.Booting;
                return false;
            }

            ApplyPacingProfile(profile, restartRun: false);
            _runModeSelectionOpen = false;
            StartRun();
            PlayAudioEvent(AudioEventModeSelected, _levelUpClip, 0.05f);
            return true;
        }

        public void StartRun()
        {
            if (!CanStartConfiguredRun)
            {
                _runModeSelectionOpen = true;
                State = SurvivorsRunState.Booting;
                return;
            }

            EnsureUiTheme();
            Time.timeScale = 1f;
            ClearRun();
            _runModeSelectionOpen = false;
            _debugOverlayVisible = false;
            _buildMenuOpen = false;
            _buildMenuTab = BuildMenuTab.CurrentBuild;
            _buildMenuScrollPosition = Vector2.zero;
            _runSummaryScrollPosition = Vector2.zero;
            _draftCardScrollPosition = Vector2.zero;
            _modeSelectionScrollPosition = Vector2.zero;
            _tutorialOverlayOpen = false;
            _tutorialStepIndex = 0;
            _audioEventNextAllowedTime.Clear();
            _audioEventDispatchCount = 0;
            _lastAudioEventId = string.Empty;
            _highestChosenRarity = RunUpgradeRarity.Common;
            _highestChosenRarityLabel = string.Empty;
            _bestMomentLabel = string.Empty;
            SurvivorsTemplateTuning resolved = CurrentTuning;
            _combatCatalog = BasicSurvivorsGame.CreateCombatCatalog();
            _combatRandom = new DeterministicRandom(resolved.RunSeed + 4049);
            _weaponDefinition = BasicSurvivorsGame.CreateWeaponDefinition();
            _projectileDefinition = BasicSurvivorsGame.CreateProjectileDefinition(resolved);
            _upgradeState = new RunUpgradeState();
            _relicDefinitions = CreateRelicDefinitions();
            _upgradeClassGates = CreateClassUpgradeGates();
            _classLibrary = CreateClassLibraryDefinition();
            EnsureMetaProgressionLoaded();
            _metaProgression.EnsureDefaultClassUnlocks(_classLibrary);
            _selectedClass = _metaProgression.ResolveSelectedClass(_classLibrary);
            _upgradeCatalog = CreateRunUpgradeCatalogForSelectedClass();
            BuildUpgradeMetadataIndex();
            _ownedPassiveUpgradeIds.Clear();
            _ownedEvolutionUpgradeIds.Clear();
            _selectedRelicIds.Clear();
            _selectedRelics.Clear();
            _playerHealth = new HealthState(new CombatantId("combatant.survivors.player"), resolved.PlayerMaxHealth, resolved.PlayerMaxHealth);
            Level = 1;
            Experience = 0;
            PendingLevelUps = 0;
            SpawnedCount = 0;
            KilledCount = 0;
            ProjectileLaunchCount = 0;
            OrbitHitCount = 0;
            MeleeSwingCount = 0;
            MeleeHitCount = 0;
            BurstPulseCount = 0;
            BurstHitCount = 0;
            HitscanFireCount = 0;
            HitscanHitCount = 0;
            TempestPrismArcHitCount = 0;
            LastTempestPrismArcFeedbackLabel = string.Empty;
            ProjectilePierceHitCount = 0;
            ProjectileChainHitCount = 0;
            ProjectileForkSpawnCount = 0;
            ProjectileReturnStartCount = 0;
            OrbitKnockbackCount = 0;
            LastOrbitKnockbackFeedbackLabel = string.Empty;
            FrostFanSlowApplicationCount = 0;
            LastFrostFanSlowFeedbackLabel = string.Empty;
            CinderBurnApplicationCount = 0;
            LastCinderBurnFeedbackLabel = string.Empty;
            PayloadThrowCount = 0;
            PayloadPlacedCount = 0;
            PayloadDetonationCount = 0;
            PayloadExplosionHitCount = 0;
            PayloadHazardTickCount = 0;
            PayloadHazardSnareCount = 0;
            LastPayloadHazardSnareFeedbackLabel = string.Empty;
            PayloadHazardChainActivationCount = 0;
            PayloadHazardChainPulseHitCount = 0;
            PayloadHazardChainExperienceGemDropCount = 0;
            LastPayloadHazardChainFeedbackLabel = string.Empty;
            _payloadHazardChainSnareCount = 0;
            _payloadHazardChainWindowTimer = 0f;
            _payloadHazardChainCooldownTimer = 0f;
            SplitterChildSpawnCount = 0;
            SplitterSplitFeedbackCount = 0;
            LastSplitterSplitFeedbackLabel = string.Empty;
            SummonerSupportSpawnCount = 0;
            SummonerSupportFeedbackCount = 0;
            LastSummonerSupportFeedbackLabel = string.Empty;
            MinibossSpawnCount = 0;
            BossSpawnCount = 0;
            EliteKilledCount = 0;
            MinibossKilledCount = 0;
            BossKilledCount = 0;
            EliteRewardGrantCount = 0;
            MinibossRewardGrantCount = 0;
            BossRewardGrantCount = 0;
            WeaponEvolutionFeedbackCount = 0;
            WeaponEvolutionSurgeCount = 0;
            WeaponEvolutionSurgeHitCount = 0;
            EvolutionMagnetRecallCount = 0;
            EvolutionMagnetRecallGemCount = 0;
            EvolutionChainSurgeActivationCount = 0;
            EvolutionChainSurgePulseHitCount = 0;
            LastWeaponEvolutionSurgeFeedbackLabel = string.Empty;
            LastEvolutionMagnetRecallFeedbackLabel = string.Empty;
            LastEvolutionChainSurgeFeedbackLabel = string.Empty;
            MetaUpgradePurchaseCount = 0;
            LastMetaUpgradePurchaseFeedbackLabel = string.Empty;
            ResultClassSelectionCount = 0;
            LastResultClassSelectionFeedbackLabel = string.Empty;
            EvolutionGoalFeedbackCount = 0;
            LastEvolutionGoalFeedbackLabel = string.Empty;
            EvolutionReadyFeedbackCount = 0;
            LastEvolutionReadyFeedbackLabel = string.Empty;
            BossRelicDraftOpenCount = 0;
            EliteUpgradeDraftOpenCount = 0;
            BossUpgradeDraftOpenCount = 0;
            LevelUpDraftOpenCount = 0;
            SelectedRelicCount = 0;
            BossRelicSurgeCount = 0;
            BossRelicSurgeHitCount = 0;
            LastBossRelicSurgeFeedbackLabel = string.Empty;
            WeaponLoadoutSurgeActivationCount = 0;
            WeaponLoadoutSurgePulseHitCount = 0;
            LastWeaponLoadoutSurgeFeedbackLabel = string.Empty;
            PassiveLoadoutSurgeActivationCount = 0;
            PassiveLoadoutSurgePulseHitCount = 0;
            LastPassiveLoadoutSurgeFeedbackLabel = string.Empty;
            LevelUpPulseCount = 0;
            LevelUpPulseHitCount = 0;
            LastLevelUpPulseFeedbackLabel = string.Empty;
            SelectedRewardUpgradeCount = 0;
            RewardUpgradeSurgeCount = 0;
            RewardUpgradeSurgeHitCount = 0;
            LastRewardUpgradeSurgeFeedbackLabel = string.Empty;
            RewardJackpotCount = 0;
            RewardJackpotExperienceGemDropCount = 0;
            RewardJackpotBloodShardDropCount = 0;
            RewardJackpotBloodShardsDropped = 0;
            LastRewardJackpotFeedbackLabel = string.Empty;
            RewardAutoSelectCount = 0;
            EndlessThreatSpawnCount = 0;
            EndlessSurgeActivationCount = 0;
            EndlessSurgeTier = 0;
            EndlessSurgeExperienceGemDropCount = 0;
            EndlessSurgeBloodShardDropCount = 0;
            EndlessSurgePulseHitCount = 0;
            LastEndlessSurgeFeedbackLabel = string.Empty;
            HordeRushSpawnCount = 0;
            HordeRushEnemySpawnCount = 0;
            HordeRushWarningCount = 0;
            HordeRushClearRewardCount = 0;
            HordeRushClearExperienceGemDropCount = 0;
            HordeRushClearSpecialDropCount = 0;
            HordeRushClearPulseCount = 0;
            HordeRushClearPulseHitCount = 0;
            HordeRushClearSurgeActivationCount = 0;
            LastHordeRushFeedbackLabel = string.Empty;
            LastHordeRushClearFeedbackLabel = string.Empty;
            LastHordeRushClearPulseFeedbackLabel = string.Empty;
            DraftRerollCount = 0;
            DraftBanishCount = 0;
            DraftSkipCount = 0;
            ClassUnlockRewardCount = 0;
            LastClassUnlockRewardFeedbackLabel = string.Empty;
            _classUnlockRewardFeedbackLabel = string.Empty;
            _classUnlockRewardFeedbackTimer = 0f;
            DamagePopupSpawnCount = 0;
            PlayerDamageFeedbackCount = 0;
            LowHealthClutchPulseCount = 0;
            LowHealthClutchPulseHitCount = 0;
            LastLowHealthClutchPulseFeedbackLabel = string.Empty;
            DashUseCount = 0;
            DashEnemyShoveCount = 0;
            DashDamageHitCount = 0;
            LastDashFeedbackLabel = string.Empty;
            EnemyHitFlashFeedbackCount = 0;
            CriticalHitFeedbackCount = 0;
            DeathNovaTriggerCount = 0;
            DeathNovaHitCount = 0;
            EnemyDeathEffectCount = 0;
            EnemyRangedAttackFeedbackCount = 0;
            EnemyRangedAttackDodgeFeedbackCount = 0;
            EnemyRangedAttackDodgeExperienceGemDropCount = 0;
            LastEnemyRangedAttackDodgeFeedbackLabel = string.Empty;
            MajorRewardDropFeedbackCount = 0;
            MajorRewardCacheDropCount = 0;
            MajorRewardCacheExperienceGemDropCount = 0;
            MajorRewardCacheSpecialDropCount = 0;
            MajorRewardCacheAttractedPickupCount = 0;
            MajorThreatWarningCount = 0;
            MajorThreatEnrageCount = 0;
            MajorThreatEnrageSupportSpawnCount = 0;
            LastMajorThreatEnrageFeedbackLabel = string.Empty;
            MajorThreatSlamWarningCount = 0;
            MajorThreatSlamCastCount = 0;
            MajorThreatSlamHitCount = 0;
            MajorThreatSlamTelegraphEffectCount = 0;
            IncomingThreatTelegraphEffectCount = 0;
            LastMajorThreatSlamFeedbackLabel = string.Empty;
            LastIncomingThreatTelegraphLabel = string.Empty;
            ExperiencePickupFeedbackCount = 0;
            ExperienceComboFeedbackCount = 0;
            GemRushActivationCount = 0;
            LastExperienceComboFeedbackLabel = string.Empty;
            LastGemRushFeedbackLabel = string.Empty;
            HealthPickupCollectedCount = 0;
            HealthRestoredByPickups = 0f;
            BloodShardPickupCollectedCount = 0;
            BloodShardsCollectedFromPickups = 0;
            PickupAttractionFeedbackCount = 0;
            MagnetRecallFeedbackCount = 0;
            RewardCardPresentationCount = 0;
            RewardSelectionFeedbackCount = 0;
            LastRewardCardPresentationLabel = string.Empty;
            LastRewardSelectionFeedbackLabel = string.Empty;
            LastMajorRewardDropFeedbackLabel = string.Empty;
            LastMajorRewardCacheFeedbackLabel = string.Empty;
            ExperienceCollected = 0;
            SelectedUpgradeCount = 0;
            MagnetRecallCount = 0;
            RoamingCacheDropCount = 0;
            RoamingCacheExperienceGemDropCount = 0;
            RoamingCacheMagnetDropCount = 0;
            RoamingCacheBloodShardDropCount = 0;
            RoamingCacheAmbushCount = 0;
            RoamingCacheAmbushEnemySpawnCount = 0;
            RoamingCacheAmbushClearRewardCount = 0;
            RoamingCacheAmbushClearExperienceGemDropCount = 0;
            RoamingCacheAmbushClearMagnetDropCount = 0;
            RoamingCacheAmbushClearBloodShardDropCount = 0;
            RoamingCacheSurgeActivationCount = 0;
            RoamingCacheSurgeBonusExperienceGemDropCount = 0;
            RoamingCacheSurgePulseHitCount = 0;
            LastRoamingCacheFeedbackLabel = string.Empty;
            LastRoamingCacheAmbushClearFeedbackLabel = string.Empty;
            LastRoamingCacheSurgeFeedbackLabel = string.Empty;
            ArenaShrineTrialCount = 0;
            ArenaShrineEnemySpawnCount = 0;
            ArenaShrineClearRewardCount = 0;
            ArenaShrineClearExperienceGemDropCount = 0;
            ArenaShrineClearBloodShardDropCount = 0;
            ArenaShrineSurgeActivationCount = 0;
            ArenaShrineSurgePulseHitCount = 0;
            LastArenaShrineFeedbackLabel = string.Empty;
            LastArenaShrineClearFeedbackLabel = string.Empty;
            WaystoneDiscoveryCount = 0;
            WaystoneExperienceGemDropCount = 0;
            WaystoneBloodShardDropCount = 0;
            WaystoneAmbushCount = 0;
            WaystoneAmbushEnemySpawnCount = 0;
            WaystoneFocusActivationCount = 0;
            WaystoneChainSurgeActivationCount = 0;
            WaystoneChainSurgeBonusExperienceGemDropCount = 0;
            WaystoneChainSurgePulseHitCount = 0;
            LastWaystoneDiscoveryFeedbackLabel = string.Empty;
            LastWaystoneChainSurgeFeedbackLabel = string.Empty;
            _discoveredArenaWaystones.Clear();
            BestKillStreak = 0;
            StreakBonusDropCount = 0;
            StreakHealthDropCount = 0;
            StreakMagnetDropCount = 0;
            StreakBloodShardDropCount = 0;
            StreakRewardFeedbackCount = 0;
            LastStreakRewardFeedbackLabel = string.Empty;
            StreakSurgeTier = 0;
            StreakSurgeActivationCount = 0;
            BloodShardsEarnedThisRun = 0;
            LegacyExperienceEarnedThisRun = 0;
            LastRunResult = null;
            _lastRunSummaryLines.Clear();
            _lastRunSummaryTitle = string.Empty;
            ResetRunMetrics();
            RunTimeSeconds = 0f;
            MoveSpeedBonus = 0f;
            DamageBonus = 0f;
            PersistentDamageBonus = 0f;
            PersistentMaxHealthBonus = 0f;
            PersistentPickupRangeBonus = 0f;
            PersistentExperienceGainMultiplierBonus = 0f;
            PersistentDraftRerollBonus = 0;
            RelicDamageBonus = 0f;
            RelicCooldownMultiplierBonus = 0f;
            RelicPickupRangeBonus = 0f;
            WeaponCooldownMultiplierBonus = 0f;
            PickupRangeBonus = 0f;
            BarrierValue = 0f;
            BarrierCapacityBonus = 0f;
            BarrierRegenPerSecondBonus = 0f;
            BarrierOnDamageRatio = 0f;
            PoisonDamageRatio = 0f;
            BleedDamageRatio = 0f;
            ExecuteThresholdNormalized = 0f;
            CriticalChanceBonus = 0f;
            CriticalDamageMultiplierBonus = 0f;
            DraftLuckBonus = 0f;
            DeathNovaDamageBonus = 0f;
            DeathNovaRadiusBonus = 0f;
            LifestealRatio = 0f;
            ExperienceGainMultiplierBonus = 0f;
            AreaRadiusBonus = 0f;
            ProjectileFanBonus = 0;
            OrbitBladeBonus = 0;
            OrbitRadiusBonus = 0f;
            MeleeTargetBonus = 0;
            BurstCountBonus = 0;
            BurstEchoBonus = 0;
            TargetedBurstSigilBonus = 0;
            ProjectilePierceBonus = 0;
            ProjectileChainBonus = 0;
            ProjectileForkBonus = 0;
            ProjectileReturnBonus = 0;
            HitscanPierceBonus = 0;
            PayloadCountBonus = 0;
            PayloadExplosionRadiusBonus = 0f;
            PayloadTriggerRadiusBonus = 0f;
            PickupAttractionSpeedBonus = 0f;
            PickupMagnetPulseIntervalReductionBonus = 0f;
            NormalEnemyRecycleCount = 0;
            MajorThreatRepositionCount = 0;
            MagnetPulseActivationCount = 0;
            _lastMagnetPulseFeedbackLabel = string.Empty;
            _lastOffscreenThreatMarkerLabel = string.Empty;
            GameplayEnemySpawnSafetyCheckCountForTest = 0;
            GameplaySpawnInsideCameraViewViolationCountForTest = 0;
            _lastGameplaySpawnSafetyFailure = string.Empty;
            _lastGameplaySpawnPosition = Vector3.zero;
            _lastGameplaySpawnPadding = 0f;
            _lastGameplaySpawnWasInsideCameraViewport = false;
            _runRewardsGranted = false;
            _pendingVictoryAfterRewardDraft = false;
            _victoryClearedThisRun = false;
            _firstEliteWarningShown = false;
            _firstDreadEliteWarningShown = false;
            _timedEliteWarningTargetTimeSeconds = 0f;
            _timedDreadEliteWarningTargetTimeSeconds = 0f;
            _minibossWarningShown = false;
            _bossWarningShown = false;
            ResetEndlessThreatSchedule();
            ResetHordeRushSchedule();
            _majorThreatWarningLabel = string.Empty;
            _majorThreatWarningTargetTimeSeconds = 0f;
            _rewardFeedbackLabel = string.Empty;
            _rewardFeedbackTimer = 0f;
            _rewardFeedbackColor = Color.white;
            _streakRewardFeedbackLabel = string.Empty;
            _streakRewardFeedbackTimer = 0f;
            _streakRewardFeedbackColor = Color.white;
            _experienceComboFeedbackLabel = string.Empty;
            _experienceComboTimer = 0f;
            _experienceComboFeedbackTimer = 0f;
            _gemRushTimer = 0f;
            _experienceComboPickupCount = 0;
            _experienceComboAmount = 0;
            _gemRushActivatedForCurrentCombo = false;
            _bonusBloodShardsEarnedThisRun = 0;
            _bonusLegacyExperienceEarnedThisRun = 0;
            _enemySpawnTimer = 0f;
            _levelUpDraftCooldownTimer = 0f;
            _pickupMagnetPulseTimer = ResolvePickupMagnetPulseIntervalSeconds();
            _playerInvulnerabilityTimer = 0f;
            _lowHealthClutchPulseUsed = false;
            _dashCooldownTimer = 0f;
            _killStreakTimer = 0f;
            _roamingCacheTravelDistance = 0f;
            _killStreakCount = 0;
            _streakSurgeTimer = 0f;
            _roamingCacheSurgeTimer = 0f;
            _waystoneFocusTimer = 0f;
            _waystoneChainSurgeTimer = 0f;
            _hordeRushClearSurgeTimer = 0f;
            _weaponLoadoutSurgeTimer = 0f;
            _passiveLoadoutSurgeTimer = 0f;
            _bossRelicSurgeTimer = 0f;
            _evolutionChainSurgeTimer = 0f;
            _endlessSurgeTimer = 0f;
            _arenaShrineTravelDistance = 0f;
            _arenaShrineSurgeTimer = 0f;
            _weaponLoadoutSurgeUsed = false;
            _passiveLoadoutSurgeUsed = false;
            _announcedEvolutionGoalUpgradeIds.Clear();
            _announcedEvolutionReadyUpgradeIds.Clear();
            _evolutionReadyFeedbackLabel = string.Empty;
            _evolutionReadyFeedbackTimer = 0f;
            _spawnSequence = 0;
            _currentDraft = null;
            _currentRelicDraft = null;
            _damagePopups.Clear();
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
            _currentDraftRerollIndex = 0;
            ApplyPersistentMetaBonuses();
            ApplySelectedClassBonuses();
            BarrierValue = BarrierCapacity;
            BuildRuntimeWorld();
            _runFlow = new SurvivorsRunFlowRuntime(CreateRunFlowDefinition(resolved));
            _weaponArchetypeDefinitions = CreateWeaponArchetypeDefinitions(resolved);
            _weaponLoadout = new SurvivorsWeaponLoadoutRuntime(this, ResolveStartingWeaponDefinitions(_weaponArchetypeDefinitions));
            State = SurvivorsRunState.Playing;
            _runStarted = true;
            TryOpenFirstRunTutorial();
        }

        public void RestartRun()
        {
            StartRun();
        }

        public bool ContinueAfterVictory()
        {
            if (State != SurvivorsRunState.Victory || !_runStarted || !CurrentTuning.EndlessContinuationEnabled)
            {
                return false;
            }

            _victoryClearedThisRun = true;
            ClearRewardDrafts();
            _tutorialOverlayOpen = false;
            State = SurvivorsRunState.Playing;
            _enemySpawnTimer = 0f;
            ScheduleEndlessThreats(RunTimeSeconds);
            EnsureFutureHordeRushScheduled();
            PlayFeedback(_levelUpPulse, PlayerPosition, 32, _levelUpClip);
            return true;
        }

        public void Simulate(float deltaTime, Vector2 movementInput = default)
        {
            if (!_runStarted)
            {
                return;
            }

            if (_buildMenuOpen && State == SurvivorsRunState.Playing)
            {
                return;
            }

            if (_tutorialOverlayOpen)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
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
            if (State == SurvivorsRunState.LevelUp)
            {
                TickRewardSelectionTimeout(dt);
                return;
            }

            if (State != SurvivorsRunState.Playing)
            {
                return;
            }

            RunTimeSeconds += dt;
            RecordLevelCheckpoints();
            TickLevelUpDraftCooldown(dt);
            TickMajorThreatWarnings();
            TickRunFlow();
            if (State != SurvivorsRunState.Playing)
            {
                return;
            }

            TickHordeRushEvents();
            _playerInvulnerabilityTimer = Mathf.Max(0f, _playerInvulnerabilityTimer - dt);
            _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - dt);
            TickKillStreak(dt);
            TickStreakSurge(dt);
            TickRoamingCacheSurge(dt);
            TickArenaShrineSurge(dt);
            TickWaystoneFocus(dt);
            TickWaystoneChainSurge(dt);
            TickHordeRushClearSurge(dt);
            TickWeaponLoadoutSurge(dt);
            TickPassiveLoadoutSurge(dt);
            TickBossRelicSurge(dt);
            TickPayloadHazardChain(dt);
            TickGemRush(dt);
            TickPickupMagnetPulse(dt);
            TickEvolutionChainSurge(dt);
            TickEndlessSurge(dt);
            TickBarrier(dt);
            MovePlayer(movementInput, dt);
            TickArenaWaystoneDiscoveries();
            UpdateArenaPresentation();
            TickArenaWaystoneDiscoveries();
            TickEnemySpawning(dt);
            TickWeapon(dt);
            TickEnemies(dt);
            TickProjectiles(dt);
            TickPickups(dt);
            UpdateOffscreenThreatMarkerSnapshot();
        }

        public SurvivorsEnemyActor SpawnEnemyForTest(Vector3 position, float healthOverride = -1f)
        {
            EnsureRunStartedForTest();
            SurvivorsEnemyActor enemy = SpawnEnemy(position, explicitPosition: true, SurvivorsEnemyRole.Swarm);
            if (enemy != null && healthOverride > 0f)
            {
                enemy.OverrideHealthForTest(healthOverride);
            }

            return enemy;
        }

        public SurvivorsEnemyActor SpawnEnemyForTest(Vector3 position, SurvivorsEnemyRole role, float healthOverride = -1f)
        {
            EnsureRunStartedForTest();
            SurvivorsEnemyActor enemy = SpawnEnemy(position, explicitPosition: true, role);
            if (enemy != null && healthOverride > 0f)
            {
                enemy.OverrideHealthForTest(healthOverride);
            }

            return enemy;
        }

        public SurvivorsEnemyActor SpawnMinibossForTest(Vector3 position, float healthOverride = -1f)
        {
            EnsureRunStartedForTest();
            SurvivorsEnemyActor enemy = SpawnEnemy(position, explicitPosition: true, SurvivorsEnemyRole.Miniboss);
            if (enemy != null && healthOverride > 0f)
            {
                enemy.OverrideHealthForTest(healthOverride);
            }

            return enemy;
        }

        public SurvivorsEnemyActor SpawnBossForTest(Vector3 position, float healthOverride = -1f)
        {
            EnsureRunStartedForTest();
            SurvivorsEnemyActor enemy = SpawnEnemy(position, explicitPosition: true, SurvivorsEnemyRole.Boss);
            if (enemy != null && healthOverride > 0f)
            {
                enemy.OverrideHealthForTest(healthOverride);
            }

            return enemy;
        }

        public int KillActiveHordeRushEnemiesForTest()
        {
            return DebugClearActiveHordeRush();
        }

        public int DebugClearActiveHordeRush()
        {
            EnsureRunStartedForTest();
            if (_activeHordeRushEnemies.Count == 0)
            {
                return 0;
            }

            var enemies = new List<SurvivorsEnemyActor>(_activeHordeRushEnemies);
            int killed = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                enemy.ApplyDamage(10000f, "test.horde-rush-clear");
                killed++;
            }

            return killed;
        }

        public int KillActiveRoamingCacheAmbushEnemiesForTest()
        {
            EnsureRunStartedForTest();
            if (_activeRoamingCacheAmbushEnemies.Count == 0)
            {
                return 0;
            }

            var enemies = new List<SurvivorsEnemyActor>(_activeRoamingCacheAmbushEnemies);
            int killed = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                enemy.ApplyDamage(10000f, "test.roaming-cache-ambush-clear");
                killed++;
            }

            return killed;
        }

        public int KillActiveArenaShrineEnemiesForTest()
        {
            EnsureRunStartedForTest();
            if (_activeArenaShrineEnemies.Count == 0)
            {
                return 0;
            }

            var enemies = new List<SurvivorsEnemyActor>(_activeArenaShrineEnemies);
            int killed = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                enemy.ApplyDamage(10000f, "test.arena-shrine-clear");
                killed++;
            }

            return killed;
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
            _weaponLoadout?.FireForTest(SurvivorsWeaponArchetype.Projectile);
        }

        public bool FireWeaponForTest(SurvivorsWeaponArchetype archetype)
        {
            EnsureRunStartedForTest();
            return _weaponLoadout != null && _weaponLoadout.FireForTest(archetype);
        }

        public bool DashForTest(Vector2 directionInput)
        {
            EnsureRunStartedForTest();
            return TryDash(directionInput);
        }

        public void ForceLevelUp()
        {
            EnsureRunStartedForTest();
            PendingLevelUps++;
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
            if (granted && _runStarted)
            {
                ApplyPersistentMetaBonuses();
            }

            return granted;
        }

        public int DebugSpawnEnemyBurst(SurvivorsEnemyRole role, int count, float radius)
        {
            EnsureRunStartedForTest();
            int spawned = 0;
            int resolvedCount = Mathf.Clamp(count, 1, 256);
            float resolvedRadius = Mathf.Max(0.5f, radius);
            for (int index = 0; index < resolvedCount; index++)
            {
                float angle = (index / (float)resolvedCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * resolvedRadius;
                if (SpawnEnemy(PlayerPosition + offset, explicitPosition: true, role) != null)
                {
                    spawned++;
                }
            }

            return spawned;
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

        public SurvivorsEnemyActor DebugSpawnSprintBoss(float radius)
        {
            if (CurrentPacingProfile != SurvivorsPacingProfile.SprintRun)
            {
                ApplyPacingProfile(SurvivorsPacingProfile.SprintRun, restartRun: _runStarted);
            }

            if (!_runStarted)
            {
                StartRun();
            }

            return DebugSpawnBoss(radius);
        }

        public SurvivorsEnemyActor DebugSpawnMajorEnemy(SurvivorsEnemyRole role, float radius)
        {
            EnsureRunStartedForTest();
            SurvivorsEnemyRole resolvedRole = ResolveDebugMajorEnemyRole(role);
            Vector3 forward = PlayerForward;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = Vector3.forward;
            }

            float resolvedRadius = Mathf.Clamp(radius, 2f, 40f);
            return SpawnEnemy(PlayerPosition + (forward.normalized * resolvedRadius), explicitPosition: true, resolvedRole);
        }

        public int DebugFillArenaToTarget(SurvivorsEnemyRole role, int targetAlive, float radius)
        {
            EnsureRunStartedForTest();
            int target = Mathf.Clamp(targetAlive, 1, 512);
            int needed = Mathf.Max(0, target - ActiveEnemyCount);
            return needed <= 0 ? 0 : DebugSpawnEnemyBurst(role, needed, radius);
        }

        public int DebugTriggerHordeRush()
        {
            EnsureRunStartedForTest();
            int spawned = SpawnHordeRushBurst();
            _hordeRushSequence++;
            ScheduleNextHordeRush(RunTimeSeconds, firstRush: false);
            if (spawned <= 0)
            {
                return 0;
            }

            HordeRushSpawnCount++;
            HordeRushEnemySpawnCount += spawned;
            LastHordeRushFeedbackLabel = $"Horde Rush {HordeRushSpawnCount}: {spawned} enemies";
            RecordStreakRewardFeedback(LastHordeRushFeedbackLabel, new Color(1f, 0.42f, 0.18f));
            PlayFeedback(_bossPulse, PlayerPosition, 28, _dangerClip);
            return spawned;
        }

        public void DebugApplyStressProfile(int targetAlive)
        {
            int target = Mathf.Clamp(targetAlive, 50, 512);
            CurrentTuning.EnemyMaximumAlive = target;
            CurrentTuning.EnemySpawnIntervalSeconds = Mathf.Min(CurrentTuning.EnemySpawnIntervalSeconds, target >= 250 ? 0.18f : 0.28f);
            DebugFillArenaToTarget(SurvivorsEnemyRole.Swarm, Mathf.Min(target, 160), CurrentTuning.EnemySpawnRadius);
        }

        public void DebugApplyPacingProfile(SurvivorsPacingProfile profile)
        {
            ApplyPacingProfile(profile, restartRun: _runStarted);
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
            PendingLevelUps++;
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
            _injectedMetaPersistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _metaSaveSlotId = slotId;
            ReleaseMetaProgressionService();
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

        public bool TryPurchasePersistentUpgrade(string id)
        {
            EnsureMetaProgressionLoaded();
            bool purchased = _metaProgression.TryPurchasePersistentUpgrade(id);
            if (purchased && _runStarted)
            {
                ApplyPersistentMetaBonuses();
            }

            if (purchased)
            {
                RecordMetaUpgradePurchaseFeedback(id);
            }

            return purchased;
        }

        public int GetPersistentUpgradeRankForTest(string id)
        {
            EnsureMetaProgressionLoaded();
            return _metaProgression.GetPersistentUpgradeRank(id);
        }

        private bool TryPurchaseResultMetaUpgrade(int index)
        {
            IReadOnlyList<SurvivorsPersistentUpgradeDefinition> options = ResolveResultMetaUpgradeOptions(ResultMetaUpgradeOptionCount);
            if (index < 0 || index >= options.Count)
            {
                return false;
            }

            return TryPurchasePersistentUpgrade(options[index].Id.Value);
        }

        private IReadOnlyList<SurvivorsPersistentUpgradeDefinition> ResolveResultMetaUpgradeOptions(int limit)
        {
            _resultMetaUpgradeOptions.Clear();
            if (limit <= 0)
            {
                return _resultMetaUpgradeOptions;
            }

            EnsureMetaProgressionLoaded();
            SurvivorsMetaProgressionDefinition definition = ResolveMetaProgressionDefinition();
            for (int i = 0; i < definition.PersistentUpgrades.Count && _resultMetaUpgradeOptions.Count < limit; i++)
            {
                SurvivorsPersistentUpgradeDefinition upgrade = definition.PersistentUpgrades[i];
                if (upgrade == null)
                {
                    continue;
                }

                int currentRank = _metaProgression.GetPersistentUpgradeRank(upgrade.Id.Value);
                int nextCost = ResolveNextPersistentUpgradeCost(upgrade, currentRank);
                if (currentRank < upgrade.MaxRank && nextCost > 0 && MetaBloodShards >= nextCost)
                {
                    _resultMetaUpgradeOptions.Add(upgrade);
                }
            }

            return _resultMetaUpgradeOptions;
        }

        private IReadOnlyList<SurvivorsClassDefinition> ResolveResultClassOptions(int limit)
        {
            _resultClassOptions.Clear();
            if (limit <= 0)
            {
                return _resultClassOptions;
            }

            EnsureMetaProgressionLoaded();
            EnsureClassLibraryLoaded();
            if (_classLibrary == null)
            {
                return _resultClassOptions;
            }

            for (int i = 0; i < _classLibrary.Classes.Count && _resultClassOptions.Count < limit; i++)
            {
                SurvivorsClassDefinition definition = _classLibrary.Classes[i];
                if (definition != null)
                {
                    _resultClassOptions.Add(definition);
                }
            }

            return _resultClassOptions;
        }

        private bool TrySelectResultClass(int index)
        {
            if (_runStarted && State != SurvivorsRunState.GameOver && State != SurvivorsRunState.Victory)
            {
                return false;
            }

            IReadOnlyList<SurvivorsClassDefinition> options = ResolveResultClassOptions(ResultClassOptionCount);
            if (index < 0 || index >= options.Count)
            {
                return false;
            }

            SurvivorsClassDefinition selected = options[index];
            if (!IsResultClassUnlocked(selected))
            {
                return false;
            }

            bool changed = _metaProgression.TrySetSelectedClass(selected.Id, _classLibrary);
            _selectedClass = _metaProgression.ResolveSelectedClass(_classLibrary);
            if (changed)
            {
                RecordResultClassSelectionFeedback(selected);
            }

            return changed;
        }

        private bool IsResultClassUnlocked(SurvivorsClassDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            EnsureMetaProgressionLoaded();
            EnsureClassLibraryLoaded();
            return _metaProgression != null && _metaProgression.IsClassUnlocked(definition.Id, _classLibrary);
        }

        private int ResolveNextPersistentUpgradeCost(SurvivorsPersistentUpgradeDefinition upgrade, int currentRank)
        {
            if (upgrade == null || currentRank < 0 || currentRank >= upgrade.MaxRank || upgrade.RankCosts.Count == 0)
            {
                return 0;
            }

            int costIndex = Mathf.Clamp(currentRank, 0, upgrade.RankCosts.Count - 1);
            return Mathf.Max(0, upgrade.RankCosts[costIndex]);
        }

        private string FormatPersistentUpgradeOptionLabel(int index, SurvivorsPersistentUpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return string.Empty;
            }

            EnsureMetaProgressionLoaded();
            int currentRank = _metaProgression.GetPersistentUpgradeRank(upgrade.Id.Value);
            int nextCost = ResolveNextPersistentUpgradeCost(upgrade, currentRank);
            return $"{index + 1}. {upgrade.DisplayName} rank {currentRank}->{Mathf.Min(upgrade.MaxRank, currentRank + 1)}/{upgrade.MaxRank} ({nextCost} {CurrencyRewardLabel}) - {FormatPersistentUpgradeEffectLabel(upgrade)}";
        }

        private string FormatResultClassOptionLabel(int index, SurvivorsClassDefinition definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            bool unlocked = IsResultClassUnlocked(definition);
            bool selected = string.Equals(SelectedClassId, definition.Id, StringComparison.Ordinal);
            string state = selected ? "Selected" : (unlocked ? "Unlocked" : "Locked");
            return $"{index + 1}. {definition.DisplayName} [{state}]\n{FormatClassStatSummary(definition)} | {FormatClassStartingWeaponSummary(definition)}";
        }

        private static string FormatClassStatSummary(SurvivorsClassDefinition definition)
        {
            if (definition == null || definition.StartingStatModifiers.Count == 0)
            {
                return "Balanced";
            }

            var labels = new List<string>(definition.StartingStatModifiers.Count);
            for (int i = 0; i < definition.StartingStatModifiers.Count; i++)
            {
                SurvivorsClassStatModifierDefinition modifier = definition.StartingStatModifiers[i];
                if (modifier == null)
                {
                    continue;
                }

                if (modifier.StatKind == SurvivorsClassStatKind.MoveSpeed)
                {
                    labels.Add($"Move +{modifier.Amount:0.##}");
                }
                else if (modifier.StatKind == SurvivorsClassStatKind.Damage)
                {
                    labels.Add($"Dmg +{modifier.Amount:0.##}");
                }
                else if (modifier.StatKind == SurvivorsClassStatKind.MaxHealth)
                {
                    labels.Add($"HP +{modifier.Amount:0.#}");
                }
            }

            return labels.Count == 0 ? "Balanced" : string.Join(", ", labels);
        }

        private static string FormatClassStartingWeaponSummary(SurvivorsClassDefinition definition)
        {
            if (definition == null || definition.StartingWeaponIds.Count == 0)
            {
                return "0 weapons";
            }

            return definition.StartingWeaponIds.Count == 1
                ? "1 weapon"
                : definition.StartingWeaponIds.Count.ToString() + " weapons";
        }

        private static string FormatPersistentUpgradeEffectLabel(SurvivorsPersistentUpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return string.Empty;
            }

            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaDamageEffectId, StringComparison.Ordinal))
            {
                return $"+{upgrade.AmountPerRank:0.#} starting damage";
            }

            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaMaxHealthEffectId, StringComparison.Ordinal))
            {
                return $"+{upgrade.AmountPerRank:0.#} max health";
            }

            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaPickupRangeEffectId, StringComparison.Ordinal))
            {
                return $"+{upgrade.AmountPerRank:0.#} pickup range";
            }

            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaExperienceGainEffectId, StringComparison.Ordinal))
            {
                return $"+{upgrade.AmountPerRank:P0} XP gain";
            }

            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaDraftRerollEffectId, StringComparison.Ordinal))
            {
                return $"+{upgrade.AmountPerRank:0} draft reroll";
            }

            return upgrade.EffectId;
        }

        private void RecordResultClassSelectionFeedback(SurvivorsClassDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            ResultClassSelectionCount++;
            LastResultClassSelectionFeedbackLabel = "Next Run Class: " + selected.DisplayName;
            _rewardFeedbackLabel = LastResultClassSelectionFeedbackLabel;
            _rewardFeedbackColor = new Color(0.8f, 0.58f, 1f);
            _rewardFeedbackTimer = RewardFeedbackDurationSeconds;
        }

        private void RecordMetaUpgradePurchaseFeedback(string id)
        {
            EnsureMetaProgressionLoaded();
            SurvivorsMetaProgressionDefinition definition = ResolveMetaProgressionDefinition();
            string displayName = id;
            if (definition.TryGetPersistentUpgrade(id, out SurvivorsPersistentUpgradeDefinition upgrade))
            {
                displayName = upgrade.DisplayName;
            }

            int rank = _metaProgression.GetPersistentUpgradeRank(id);
            MetaUpgradePurchaseCount++;
            LastMetaUpgradePurchaseFeedbackLabel = $"Meta Upgrade: {displayName} rank {rank}";
            _rewardFeedbackLabel = LastMetaUpgradePurchaseFeedbackLabel;
            _rewardFeedbackColor = new Color(0.45f, 0.95f, 0.76f);
            _rewardFeedbackTimer = RewardFeedbackDurationSeconds;
        }

        public void ResetMetaProgressionForTest()
        {
            EnsureMetaProgressionLoaded();
            _metaProgression.Reset();
            _metaProgression.Load();
            if (_runStarted)
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
            return _weaponLoadout != null && _weaponLoadout.ContainsWeapon(weaponId);
        }

        public bool IsUpgradeAvailableInRunForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return _upgradeCatalog != null && !string.IsNullOrWhiteSpace(upgradeId) && _upgradeCatalog.TryGet(new RunUpgradeId(upgradeId), out _);
        }

        private Vector3 ResolveClosestArenaLandmarkPositionForTest()
        {
            return TryResolveClosestArenaLandmark(ignoreDiscovered: false, out Vector3 closest, out _, out _)
                ? closest
                : Vector3.zero;
        }

        private bool TryResolveClosestArenaLandmark(bool ignoreDiscovered, out Vector3 closest, out float closestDistance, out Vector3 closestDelta)
        {
            closest = Vector3.zero;
            closestDistance = 0f;
            closestDelta = Vector3.zero;
            if (_arenaLandmarks.Count == 0)
            {
                return false;
            }

            Vector3 player = PlayerPosition;
            float closestDistanceSquared = float.MaxValue;
            for (int i = 0; i < _arenaLandmarks.Count; i++)
            {
                Transform landmark = _arenaLandmarks[i];
                if (landmark == null)
                {
                    continue;
                }

                if (ignoreDiscovered &&
                    i < _arenaLandmarkKeys.Count &&
                    _discoveredArenaWaystones.Contains(_arenaLandmarkKeys[i]))
                {
                    continue;
                }

                Vector3 delta = landmark.position - player;
                delta.y = 0f;
                float distanceSquared = delta.sqrMagnitude;
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closest = landmark.position;
                    closestDelta = delta;
                }
            }

            if (closestDistanceSquared >= float.MaxValue)
            {
                return false;
            }

            closestDistance = Mathf.Sqrt(closestDistanceSquared);
            return true;
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
            if (_currentDraft == null || index < 0 || index >= _currentDraft.Choices.Count)
            {
                return string.Empty;
            }

            return FormatUpgradeChoiceLabel(index, _currentDraft.Choices[index]);
        }

        public string GetCurrentDraftCardSummaryForTest(int index)
        {
            EnsureRunStartedForTest();
            if (IsRelicChoiceOpen)
            {
                if (_currentRelicDraft == null || index < 0 || index >= _currentRelicDraft.Choices.Count)
                {
                    return string.Empty;
                }

                return CreateRelicDraftCard(index, _currentRelicDraft.Choices[index]).Summary;
            }

            if (_currentDraft == null || index < 0 || index >= _currentDraft.Choices.Count)
            {
                return string.Empty;
            }

            return CreateUpgradeDraftCard(index, _currentDraft.Choices[index]).Summary;
        }

        public int GetRunUpgradeRankForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return string.IsNullOrWhiteSpace(upgradeId) ? 0 : _upgradeState.GetRank(new RunUpgradeId(upgradeId));
        }

        public int GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity rarity)
        {
            EnsureRunStartedForTest();
            return ResolveDraftRarityWeight(DraftRarityProfile.NormalMid, rarity);
        }

        public bool HasEvolvedUpgradeForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return !string.IsNullOrWhiteSpace(upgradeId) && _ownedEvolutionUpgradeIds.Contains(upgradeId);
        }

        internal bool IsEvolutionActive(string upgradeId)
        {
            return !string.IsNullOrWhiteSpace(upgradeId) && _ownedEvolutionUpgradeIds.Contains(upgradeId);
        }

        public IReadOnlyList<string> DebugDescribeCurrentBuild()
        {
            EnsureRunStartedForTest();
            var lines = new List<string>
            {
                $"Weapons {ActiveWeaponCount}/{MaxWeaponSlots}: {FormatActiveWeaponList()}",
                $"Passives {ActivePassiveCount}/{MaxPassiveSlots}, Evolutions {EvolvedWeaponCount}",
                $"Relics {SelectedRelicCount}/{ResolveTotalRelicCount()}: {FormatSelectedRelicList()}",
                $"Stats: damage +{DamageBonus:0.#} surge +{StreakSurgeDamageBonus + RoamingCacheSurgeDamageBonus + ArenaShrineSurgeDamageBonus + WaystoneFocusDamageBonus + WaystoneChainSurgeDamageBonus + HordeRushClearSurgeDamageBonus + WeaponLoadoutSurgeDamageBonus + PassiveLoadoutSurgeDamageBonus + BossRelicSurgeDamageBonus + GemRushDamageBonus + EvolutionChainSurgeDamageBonus + EndlessSurgeDamageBonus:0.#}, crit {CriticalChanceNormalized:P0} x{CriticalDamageMultiplier:0.0}, luck +{DraftLuckBonus:P0}, cooldown {WeaponCooldownSeconds:0.00}s, move {PlayerMoveSpeed:0.0}, pickup {CurrentPickupAttractRange:0.#}, pull {CurrentPickupAttractionSpeed:0.#}, pulse {FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds)}, XP +{ExperienceGainMultiplierBonus + PassiveLoadoutSurgeExperienceGainMultiplierBonus:P0}",
                $"Projectiles: fan +{ProjectileFanBonus}, pierce +{ProjectilePierceBonus}, chain +{ProjectileChainBonus}, fork +{ProjectileForkBonus}, return +{ProjectileReturnBonus}",
                $"Area: global +{AreaRadiusBonus:0.#}, orbit +{OrbitRadiusBonus:0.#}, burst +{BurstCountBonus}, echoes +{BurstEchoBonus}, payload +{PayloadCountBonus}, death nova {DeathNovaDamage:0.#}/{DeathNovaRadius:0.#}",
                $"Status: poison {PoisonDamageRatio:P0}, bleed {BleedDamageRatio:P0}, execute {ExecuteThresholdNormalized:P0}, lifesteal {LifestealRatio:P0}"
            };

            string evolutionObjective = ResolveEvolutionObjectiveHudLabel();
            if (!string.IsNullOrWhiteSpace(evolutionObjective))
            {
                lines.Add(evolutionObjective);
            }

            AppendSelectedUpgradeRankLines(lines);
            return lines;
        }

        public IReadOnlyList<string> CurrentBuildHudLinesForTest()
        {
            EnsureRunStartedForTest();
            return ResolveBuildHudSummaryLines();
        }

        public IReadOnlyList<string> CurrentBuildMenuLinesForTest()
        {
            EnsureRunStartedForTest();
            return ResolveBuildMenuLines(_buildMenuTab);
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
            return ResolveTutorialStepLines(ClampTutorialStepIndex(_tutorialStepIndex));
        }

        public void ToggleDebugOverlayForTest()
        {
            _debugOverlayVisible = !_debugOverlayVisible;
        }

        public void SetBuildMenuOpenForTest(bool open)
        {
            _buildMenuOpen = open && _runStarted && State == SurvivorsRunState.Playing;
        }

        public void SetBuildMenuTabForTest(int tabIndex)
        {
            _buildMenuTab = ClampBuildMenuTab(tabIndex);
        }

        public void SetAudioMutedForTest(bool muted)
        {
            _audioMuted = muted;
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
            _tutorialOverlayOpen = false;
            _tutorialStepIndex = 0;
        }

        public void MarkTutorialSeenForTest()
        {
            EnsureMetaProgressionLoaded();
            _metaProgression.MarkTutorialSeen();
            _tutorialOverlayOpen = false;
        }

        public bool DispatchAudioEventForTest(string eventId)
        {
            return PlayAudioEvent(eventId, _pickupClip, 0f);
        }

        public Rect ResolveCenteredPanelRectForTest(float maxWidth, float maxHeight, float minWidth, float minHeight, float margin)
        {
            return ResolveCenteredPanelRect(maxWidth, maxHeight, minWidth, minHeight, margin);
        }

        public IReadOnlyList<string> DebugDescribeEligibleEvolutionPool()
        {
            EnsureRunStartedForTest();
            if (_upgradeCatalog == null)
            {
                return Array.Empty<string>();
            }

            var lines = new List<string>();
            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition != null && IsEvolutionUpgrade(definition) && IsUpgradeEligibleForCurrentBuild(definition))
                {
                    lines.Add(FormatDebugUpgradeLine(-1, definition));
                }
            }

            if (lines.Count == 0)
            {
                lines.Add("No eligible evolutions yet. Max a weapon path and own its matching passive.");
            }

            return lines;
        }

        public IReadOnlyList<string> DebugDescribeCurrentDraftPool()
        {
            EnsureRunStartedForTest();
            var lines = new List<string>();
            if (_currentDraft != null && _currentDraft.Choices.Count > 0)
            {
                lines.Add(ResolveRewardOverlayTitle());
                for (int i = 0; i < _currentDraft.Choices.Count; i++)
                {
                    lines.Add(FormatDebugUpgradeLine(i, _currentDraft.Choices[i]));
                }
            }

            if (_currentRelicDraft != null && _currentRelicDraft.Choices.Count > 0)
            {
                lines.Add("Boss Relics");
                for (int i = 0; i < _currentRelicDraft.Choices.Count; i++)
                {
                    SurvivorsRelicDefinition relic = _currentRelicDraft.Choices[i];
                    if (relic == null)
                    {
                        lines.Add($"{i + 1}. Missing relic");
                    }
                    else
                    {
                        lines.Add($"{i + 1}. {relic.DisplayName} [{relic.EffectKind}] +{relic.Amount:0.##} {ShortWeaponName(relic.TargetId)}");
                    }
                }
            }

            if (lines.Count == 0)
            {
                lines.Add("No draft or reward pool is currently open.");
            }

            return lines;
        }

        public IReadOnlyList<string> DebugDescribeRunMetrics()
        {
            _runMetricsLines.Clear();
            _runMetricsLines.Add($"Mode {CurrentRunModeDisplayName} ({BasicSurvivorsGame.GetPacingProfileDisplayName(CurrentPacingProfile)})");
            _runMetricsLines.Add($"Target {FormatMetricTime(CurrentTuning.TargetDurationSeconds)} - boss {FormatMetricTime(CurrentTuning.BossSpawnTimeSeconds)} - victory {FormatMetricTime(CurrentTuning.SurvivalVictoryTimeSeconds)}");
            if (!_runStarted)
            {
                _runMetricsLines.Add(_runModeSelectionOpen ? "Run mode selection open" : "Run not started");
                return _runMetricsLines;
            }

            _runMetricsLines.Add($"Runtime {FormatMetricTime(RunTimeSeconds)} - state {State}");
            AppendMetricTime(_runMetricsLines, "First kill", _firstKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First XP pickup", _firstExperiencePickupTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First level-up draft", _firstLevelUpDraftTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First elite spawn", _firstEliteSpawnTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First elite kill", _firstEliteKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First miniboss spawn", _firstMinibossSpawnTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First miniboss kill", _firstMinibossKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First boss spawn", _firstBossSpawnTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First boss kill", _firstBossKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First evolution ready", _firstEvolutionEligibilityTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First evolution acquired", _firstEvolutionAcquiredTimeSeconds);
            _runMetricsLines.Add($"Levels 1m {FormatMetricLevel(_levelAtOneMinute)}, 2m {FormatMetricLevel(_levelAtTwoMinutes)}, 3m {FormatMetricLevel(_levelAtThreeMinutes)}, 4m {FormatMetricLevel(_levelAtFourMinutes)}, 5m {FormatMetricLevel(_levelAtFiveMinutes)}");
            _runMetricsLines.Add($"Drafts level {LevelUpDraftOpenCount}, total {_draftOpenCount}, pending {PendingLevelUps}, weapons {ActiveWeaponCount}/{MaxWeaponSlots}, passives {ActivePassiveCount}/{MaxPassiveSlots}, evolutions {EvolvedWeaponCount}");
            _runMetricsLines.Add($"Kills {KilledCount}, XP {ExperienceCollected}, stored {Experience}/{RequiredExperienceForNextLevel}, overflow {ThrottledExperienceOverflow}, damage taken {_damageTakenThisRun:0.#}");
            _runMetricsLines.Add($"Pickup range {CurrentPickupAttractRange:0.#}, pull {CurrentPickupAttractionSpeed:0.#}, pulse {FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds)}, markers {ActiveOffscreenThreatMarkerCount}, recycles {NormalEnemyRecycleCount}, major repositions {MajorThreatRepositionCount}");
            return _runMetricsLines;
        }

        private static string FormatMetricLevel(int level)
        {
            return level > 0 ? level.ToString() : "not yet";
        }

        private static void AppendMetricTime(List<string> lines, string label, float seconds)
        {
            lines.Add(label + ": " + FormatMetricTime(seconds));
        }

        private static string FormatMetricTime(float seconds)
        {
            return seconds >= 0f ? FormatRunTime(seconds) : "not yet";
        }

        private void ResetRunMetrics()
        {
            _firstKillTimeSeconds = -1f;
            _firstExperiencePickupTimeSeconds = -1f;
            _firstLevelUpDraftTimeSeconds = -1f;
            _firstEliteSpawnTimeSeconds = -1f;
            _firstEliteKillTimeSeconds = -1f;
            _firstMinibossSpawnTimeSeconds = -1f;
            _firstMinibossKillTimeSeconds = -1f;
            _firstBossSpawnTimeSeconds = -1f;
            _firstBossKillTimeSeconds = -1f;
            _firstEvolutionEligibilityTimeSeconds = -1f;
            _firstEvolutionAcquiredTimeSeconds = -1f;
            _damageTakenThisRun = 0f;
            ThrottledExperienceOverflow = 0;
            _draftOpenCount = 0;
            _levelAtOneMinute = -1;
            _levelAtTwoMinutes = -1;
            _levelAtThreeMinutes = -1;
            _levelAtFourMinutes = -1;
            _levelAtFiveMinutes = -1;
            _runMetricsLines.Clear();
        }

        private void RecordMetricTime(ref float field)
        {
            if (field < 0f)
            {
                field = RunTimeSeconds;
            }
        }

        private void RecordLevelCheckpoints()
        {
            if (_levelAtOneMinute < 0 && RunTimeSeconds >= 60f)
            {
                _levelAtOneMinute = Level;
            }

            if (_levelAtTwoMinutes < 0 && RunTimeSeconds >= 120f)
            {
                _levelAtTwoMinutes = Level;
            }

            if (_levelAtThreeMinutes < 0 && RunTimeSeconds >= 180f)
            {
                _levelAtThreeMinutes = Level;
            }

            if (_levelAtFourMinutes < 0 && RunTimeSeconds >= 240f)
            {
                _levelAtFourMinutes = Level;
            }

            if (_levelAtFiveMinutes < 0 && RunTimeSeconds >= 300f)
            {
                _levelAtFiveMinutes = Level;
            }
        }

        private void TickLevelUpDraftCooldown(float deltaTime)
        {
            if (_levelUpDraftCooldownTimer > 0f)
            {
                _levelUpDraftCooldownTimer = Mathf.Max(0f, _levelUpDraftCooldownTimer - Mathf.Max(0f, deltaTime));
            }

            ResolveLevelUpsFromExperienceBudget();
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

        public bool ApplyUpgradeByIdForTest(string id)
        {
            EnsureRunStartedForTest();
            if (_upgradeCatalog == null || _upgradeState == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition upgrade = _upgradeCatalog.Definitions[i];
                if (upgrade == null || !string.Equals(upgrade.Id.Value, id, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!IsUpgradeEligibleForCurrentBuild(upgrade))
                {
                    return false;
                }

                RunUpgradeSelectionResult selection = _upgradeState.Select(_upgradeCatalog, upgrade.Id);
                if (!selection.Succeeded)
                {
                    return false;
                }

                ApplyUpgrade(upgrade);
                SelectedUpgradeCount++;
                RecordBestRewardMoment(upgrade);
                return true;
            }

            return false;
        }

        public void ApplyDamageToPlayer(float amount, string source)
        {
            if (_playerHealth == null || State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                return;
            }

            float incoming = Mathf.Max(0f, amount);
            if (_playerInvulnerabilityTimer > 0f && incoming > 0f)
            {
                PlayFeedback(_pickupPulse, PlayerPosition, 4, null);
                return;
            }

            if (incoming > 0f)
            {
                _playerInvulnerabilityTimer = Mathf.Max(_playerInvulnerabilityTimer, CurrentTuning.PlayerContactInvulnerabilitySeconds);
            }

            if (BarrierValue > 0f && incoming > 0f)
            {
                float absorbed = Mathf.Min(BarrierValue, incoming);
                BarrierValue -= absorbed;
                incoming -= absorbed;
            }

            if (incoming <= 0f)
            {
                PlayFeedback(_bossPulse, PlayerPosition, 8, _dangerClip);
                return;
            }

            float healthFractionBefore = MaxHealth <= 0f ? 1f : CurrentHealth / MaxHealth;
            DamageRequest request = new DamageRequest(
                _playerHealth.Id,
                new[] { new DamageComponent(BasicSurvivorsGame.ArcaneDamageType, incoming) },
                sourceId: new CombatantId(string.IsNullOrWhiteSpace(source) ? "combatant.survivors.enemy" : source),
                preResolvedCritical: false);
            DamageResolutionResult result = CombatDamageResolver.Resolve(_combatCatalog, _playerHealth, null, request);
            if (result != null && result.Damage != null)
            {
                _damageTakenThisRun += Mathf.Max(0f, (float)result.Damage.HealthDamage);
            }

            RecordPlayerDamageFeedback(result == null ? null : result.Damage, PlayerPosition);
            if (!_playerHealth.IsAlive)
            {
                GrantRunRewards(victory: false);
                State = SurvivorsRunState.GameOver;
                ClearRewardDrafts();
                PlayFeedback(_bossPulse, PlayerPosition, 34, _dangerClip, AudioEventDefeat, 0.5f);
            }
            else
            {
                if (!TryTriggerLowHealthClutchPulse(healthFractionBefore))
                {
                    PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip);
                }
            }
        }

        private bool TryTriggerLowHealthClutchPulse(float healthFractionBefore)
        {
            if (_lowHealthClutchPulseUsed || _playerHealth == null || !_playerHealth.IsAlive || MaxHealth <= 0f)
            {
                return false;
            }

            float healthFractionAfter = CurrentHealth / MaxHealth;
            if (healthFractionBefore <= LowHealthWarningThreshold || healthFractionAfter > LowHealthWarningThreshold)
            {
                return false;
            }

            _lowHealthClutchPulseUsed = true;

            float safetySeconds = Mathf.Max(0f, CurrentTuning.LowHealthClutchSafetySeconds);
            if (safetySeconds > 0f)
            {
                _playerInvulnerabilityTimer = Mathf.Max(_playerInvulnerabilityTimer, safetySeconds);
            }

            float radius = Mathf.Max(0f, CurrentTuning.LowHealthClutchPulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.LowHealthClutchPulseDamage);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.low-health.clutch-pulse");
                    hitCount++;
                }
            }

            LowHealthClutchPulseCount++;
            LowHealthClutchPulseHitCount += hitCount;
            LastLowHealthClutchPulseFeedbackLabel = $"Clutch Pulse: {hitCount} enemies hit, safety {safetySeconds:0.#}s";
            RecordStreakRewardFeedback(LastLowHealthClutchPulseFeedbackLabel, new Color(1f, 0.32f, 0.42f));
            PlayFeedback(_bossPulse, PlayerPosition, Mathf.Clamp(24 + hitCount * 5, 30, 72), _dangerClip, AudioEventLowHealthWarning, 0.5f);
            return true;
        }

        public bool SelectUpgrade(int index)
        {
            if (_rewardSelectionKind == SurvivorsRewardSelectionKind.BossRelic)
            {
                return SelectRelic(index);
            }

            if (State != SurvivorsRunState.LevelUp || !IsRunUpgradeSelectionKind(_rewardSelectionKind) || _currentDraft == null || index < 0 || index >= _currentDraft.Choices.Count)
            {
                return false;
            }

            SurvivorsRewardSelectionKind selectionKind = _rewardSelectionKind;
            RunUpgradeDefinition selected = _currentDraft.Choices[index];
            if (!IsUpgradeEligibleForCurrentBuild(selected))
            {
                return false;
            }

            RunUpgradeSelectionResult selection = _upgradeState.Select(_upgradeCatalog, selected.Id);
            if (!selection.Succeeded)
            {
                return false;
            }

            ApplyUpgrade(selected);
            SelectedUpgradeCount++;
            RecordRewardSelectionFeedback(selectionKind, selected);
            PlayAudioEvent(AudioEventDraftChoiceSelected, _levelUpClip, 0.06f);
            if (IsEvolutionUpgrade(selected))
            {
                PlayAudioEvent(AudioEventEvolution, _levelUpClip, 0.08f);
            }
            if (selectionKind == SurvivorsRewardSelectionKind.LevelUp && !IsEvolutionUpgrade(selected))
            {
                TriggerLevelUpPulse(selected);
            }

            if (IsRewardUpgradeSelectionKind(selectionKind) && !IsEvolutionUpgrade(selected))
            {
                TriggerRewardJackpot(selected, selectionKind);
                TriggerRewardUpgradeSurge(selected, selectionKind);
            }

            CompleteUpgradeDraftSelection(
                selectionKind,
                consumeLevelUp: selectionKind == SurvivorsRewardSelectionKind.LevelUp,
                selectedRewardUpgrade: selectionKind != SurvivorsRewardSelectionKind.LevelUp);

            return true;
        }

        public bool SelectDraftHotkeyForTest(int hotkeyNumber)
        {
            return SelectUpgrade(hotkeyNumber - 1);
        }

        public bool RerollCurrentDraft()
        {
            if (!CanRerollCurrentDraft())
            {
                return false;
            }

            int nextRerollIndex = _currentDraftRerollIndex + 1;
            if (!TryGenerateCurrentUpgradeDraft(_rewardSelectionKind, nextRerollIndex, lockedChoices: null, out RunUpgradeDraft rerolled))
            {
                return false;
            }

            _currentDraft = rerolled;
            _currentRelicDraft = null;
            _currentDraftRerollIndex = nextRerollIndex;
            _draftCardScrollPosition = Vector2.zero;
            DraftRerollCount++;
            RecordRewardCardPresentation(_rewardSelectionKind, _currentDraft);
            BeginRewardSelectionTimeout();
            PlayFeedback(_levelUpPulse, PlayerPosition, 18, _levelUpClip, AudioEventDraftReroll, 0.08f);
            return true;
        }

        public bool SkipCurrentDraft()
        {
            if (!CanSkipCurrentDraft())
            {
                return false;
            }

            SurvivorsRewardSelectionKind selectionKind = _rewardSelectionKind;
            CompleteSkippedUpgradeDraft(
                selectionKind,
                consumeLevelUp: selectionKind == SurvivorsRewardSelectionKind.LevelUp,
                selectedRewardUpgrade: false);
            return true;
        }

        public bool BanishDraftChoice(int index)
        {
            if (!CanBanishCurrentDraft() || _currentDraft == null || index < 0 || index >= _currentDraft.Choices.Count)
            {
                return false;
            }

            RunUpgradeDefinition banished = _currentDraft.Choices[index];
            if (banished == null || !_upgradeState.Banish(banished.Id))
            {
                return false;
            }

            DraftBanishCount++;
            SurvivorsRewardSelectionKind selectionKind = _rewardSelectionKind;
            int nextRerollIndex = _currentDraftRerollIndex + 1;
            if (TryGenerateCurrentUpgradeDraft(selectionKind, nextRerollIndex, lockedChoices: null, out RunUpgradeDraft rerolled))
            {
                _currentDraft = rerolled;
                _currentRelicDraft = null;
                _currentDraftRerollIndex = nextRerollIndex;
                _draftCardScrollPosition = Vector2.zero;
                RecordRewardCardPresentation(selectionKind, _currentDraft);
                BeginRewardSelectionTimeout();
            }
            else
            {
                CompleteUpgradeDraftSelection(
                    selectionKind,
                    consumeLevelUp: selectionKind == SurvivorsRewardSelectionKind.LevelUp,
                    selectedRewardUpgrade: false);
            }

            PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip, AudioEventDraftBanish, 0.08f);
            return true;
        }

        public void TriggerMagnetRecall()
        {
            StartMagnetRecall();
        }

        private float ResolvePickupMagnetPulseIntervalSeconds()
        {
            if (PickupMagnetPulseIntervalReductionBonus <= 0f)
            {
                return -1f;
            }

            float baseInterval = Mathf.Max(1f, CurrentTuning.PickupMagnetPulseBaseIntervalSeconds);
            float minimum = Mathf.Max(1f, CurrentTuning.PickupMagnetPulseMinimumIntervalSeconds);
            return Mathf.Max(minimum, baseInterval - PickupMagnetPulseIntervalReductionBonus);
        }

        private void TickPickupMagnetPulse(float deltaTime)
        {
            float interval = ResolvePickupMagnetPulseIntervalSeconds();
            if (interval <= 0f || _pickups.Count == 0)
            {
                _pickupMagnetPulseTimer = interval;
                return;
            }

            if (_pickupMagnetPulseTimer <= 0f)
            {
                _pickupMagnetPulseTimer = interval;
            }

            _pickupMagnetPulseTimer -= Mathf.Max(0f, deltaTime);
            if (_pickupMagnetPulseTimer > 0f)
            {
                return;
            }

            _pickupMagnetPulseTimer = interval;
            int recalled = StartMagnetRecall();
            if (recalled <= 0)
            {
                return;
            }

            MagnetPulseActivationCount++;
            _lastMagnetPulseFeedbackLabel = $"Vacuum Pulse: {recalled} XP gems pulled";
            RecordStreakRewardFeedback(_lastMagnetPulseFeedbackLabel, new Color(0.42f, 0.88f, 1f));
        }

        private int StartMagnetRecall()
        {
            MagnetRecallCount++;
            int recalled = 0;
            for (int i = 0; i < _pickups.Count; i++)
            {
                SurvivorsPickupActor pickup = _pickups[i];
                if (pickup != null && pickup.Kind == SurvivorsPickupKind.Experience)
                {
                    pickup.StartGlobalRecall(CurrentTuning.MagnetRecallSpeedMultiplier);
                    recalled++;
                }
            }

            if (recalled > 0)
            {
                MagnetRecallFeedbackCount++;
                PlayFeedback(_pickupPulse, PlayerPosition, Mathf.Clamp(12 + recalled * 3, 18, 72), _pickupClip, AudioEventMagnetPulse, 0.4f);
            }

            return recalled;
        }

        internal void ApplyDamageAugmentsToEnemy(SurvivorsEnemyActor enemy, DamageResult damage, string source)
        {
            if (enemy == null || damage == null || !enemy.IsAlive || !CanApplyDamageAugments(source))
            {
                return;
            }

            float dealt = Mathf.Max(0f, (float)damage.HealthDamage);
            if (dealt <= 0f)
            {
                return;
            }

            if (LifestealRatio > 0f && _playerHealth != null)
            {
                _playerHealth.Heal(dealt * LifestealRatio);
            }

            if (BarrierOnDamageRatio > 0f)
            {
                RestoreBarrier(dealt * BarrierOnDamageRatio);
            }

            if (PoisonDamageRatio > 0f)
            {
                enemy.ApplyDamageOverTime(
                    dealt * PoisonDamageRatio,
                    CurrentTuning.StatusPoisonDurationSeconds,
                    "status.survivors.poison",
                    source);
            }

            if (BleedDamageRatio > 0f)
            {
                enemy.ApplyDamageOverTime(
                    dealt * BleedDamageRatio,
                    CurrentTuning.StatusBleedDurationSeconds,
                    "status.survivors.bleed",
                    source);
            }

            if (ExecuteThresholdNormalized > 0f && enemy.HealthFraction <= ExecuteThresholdNormalized)
            {
                enemy.ExecuteFromAugment(source);
            }
        }

        internal void ApplyWeaponStatusEffectsToEnemy(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition, DamageResult damage)
        {
            if (enemy == null || definition == null || !enemy.IsAlive)
            {
                return;
            }

            if (string.Equals(definition.Id, BasicSurvivorsGame.FrostFanWeaponContentId, StringComparison.Ordinal))
            {
                bool evolved = IsEvolutionActive(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId);
                float multiplier = evolved ? BlizzardCrownEnemyMoveSpeedMultiplier : FrostFanEnemyMoveSpeedMultiplier;
                float duration = evolved ? BlizzardCrownEnemySlowDurationSeconds : FrostFanEnemySlowDurationSeconds;
                if (!enemy.ApplyMovementSlow(multiplier, duration))
                {
                    return;
                }

                FrostFanSlowApplicationCount++;
                string sourceName = evolved
                    ? ResolveUpgradeDisplayName(new RunUpgradeId(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId))
                    : ResolveWeaponBuildDisplayName(definition.Id);
                LastFrostFanSlowFeedbackLabel = $"{sourceName} chilled {enemy.DisplayName}";
            }

            if (!string.Equals(definition.Id, BasicSurvivorsGame.StarNovaWeaponContentId, StringComparison.Ordinal) || damage == null)
            {
                return;
            }

            float dealt = Mathf.Max(0f, (float)damage.HealthDamage);
            if (dealt <= 0f)
            {
                return;
            }

            bool inferno = IsEvolutionActive(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId);
            float ratio = inferno ? InfernoHeartBurnDamageRatio : CinderBurstBurnDamageRatio;
            float durationMultiplier = inferno ? InfernoHeartBurnDurationMultiplier : 1f;
            enemy.ApplyDamageOverTime(
                dealt * ratio,
                CurrentTuning.StatusBurnDurationSeconds * durationMultiplier,
                "status.survivors.burn",
                definition.Id);
            CinderBurnApplicationCount++;
            LastCinderBurnFeedbackLabel = inferno
                ? $"Inferno Heart burned {enemy.DisplayName}"
                : $"Cinder Burst burned {enemy.DisplayName}";
        }

        internal void RecordEnemyDamageFeedback(SurvivorsEnemyActor enemy, DamageResult damage)
        {
            if (enemy == null)
            {
                return;
            }

            float resolvedDamage = ResolveDamagePopupAmount(damage);
            if (resolvedDamage <= 0f)
            {
                return;
            }

            RecordDamagePopup(enemy.transform.position, resolvedDamage, playerDamage: false, critical: damage.Critical.IsCritical);
            enemy.TriggerHitFlash(damage.Critical.IsCritical, EnemyHitFlashSeconds);
            EnemyHitFlashFeedbackCount++;
            if (damage.Critical.IsCritical)
            {
                CriticalHitFeedbackCount++;
            }

            PlayAudioEvent(AudioEventCombatHit, _fireClip, 0.08f);
            TryTriggerMajorThreatEnrage(enemy);
        }

        internal DamageResolutionResult ResolveEnemyDamage(HealthState health, float amount, string source, bool applyAugments)
        {
            if (health == null)
            {
                return null;
            }

            CombatSourceSnapshot combatSource = CreateEnemyDamageSourceSnapshot(source, applyAugments);
            bool allowCritical = combatSource != null;
            DamageRequest request = new DamageRequest(
                health.Id,
                new[] { new DamageComponent(BasicSurvivorsGame.ArcaneDamageType, amount) },
                source: combatSource,
                sourceId: new CombatantId(string.IsNullOrWhiteSpace(source) ? "combatant.survivors.player" : source),
                preResolvedCritical: allowCritical ? (bool?)null : false);
            return CombatDamageResolver.Resolve(_combatCatalog, health, null, request, allowCritical ? _combatRandom : null);
        }

        internal void HandleEnemyKilled(SurvivorsEnemyActor enemy, string source, bool applyAugments)
        {
            if (enemy == null)
            {
                return;
            }

            KilledCount++;
            Vector3 position = enemy.transform.position;
            SurvivorsEnemyRole role = enemy.Role;
            RecordMetricTime(ref _firstKillTimeSeconds);
            if (IsEliteRole(role))
            {
                RecordMetricTime(ref _firstEliteKillTimeSeconds);
            }
            else if (role == SurvivorsEnemyRole.Miniboss)
            {
                RecordMetricTime(ref _firstMinibossKillTimeSeconds);
            }
            else if (role == SurvivorsEnemyRole.Boss)
            {
                RecordMetricTime(ref _firstBossKillTimeSeconds);
            }

            int xp = Mathf.Max(1, enemy.ExperienceReward);
            float radius = enemy.Radius;
            _enemies.Remove(enemy);
            bool clearedHordeRushEnemy = _activeHordeRushEnemies.Remove(enemy) && _activeHordeRushEnemies.Count == 0;
            bool clearedRoamingCacheAmbushEnemy = _activeRoamingCacheAmbushEnemies.Remove(enemy) && _activeRoamingCacheAmbushEnemies.Count == 0;
            bool clearedArenaShrineEnemy = _activeArenaShrineEnemies.Remove(enemy) && _activeArenaShrineEnemies.Count == 0;
            _enragedMajorThreats.Remove(enemy);
            if (_spawnService != null && enemy.InstanceId.Value > 0)
            {
                _spawnService.Despawn(enemy.InstanceId, DespawnReason.Killed);
            }

            SpawnPickup(SurvivorsPickupKind.Experience, position, xp);
            RegisterKillStreak(position);
            RecordEnemyDeathEffect(position, role, radius);
            TryTriggerDeathNova(position, source, applyAugments);
            if (IsMajorRewardRole(role))
            {
                RecordMajorRewardDropFeedback(position, role, radius);
                SpawnMajorRewardPickupCache(position, role, radius);
                TryDropHealthPickup(position + new Vector3(radius * 0.7f, 0f, radius * 0.35f));
                TryActivateEndlessSurge(role, position, xp);
            }

            PlayFeedback(_killPulse, position, role == SurvivorsEnemyRole.Swarm ? 18 : 34, _killClip, AudioEventEnemyDeath, 0.08f);
            if (role == SurvivorsEnemyRole.Splitter)
            {
                SpawnSplitterChildren(position, enemy.DisplayName);
            }
            else if (IsEliteRole(role))
            {
                EliteKilledCount++;
                GrantMajorEnemyReward(role);
                OpenUpgradeRewardDraft(role, requireEvolutionChoice: false);
            }
            else if (role == SurvivorsEnemyRole.Miniboss)
            {
                MinibossKilledCount++;
                GrantMajorEnemyReward(SurvivorsEnemyRole.Miniboss);
                if (!OpenUpgradeRewardDraft(SurvivorsEnemyRole.Miniboss, requireEvolutionChoice: false))
                {
                    OpenBossRelicDraft();
                }
            }
            else if (role == SurvivorsEnemyRole.Boss)
            {
                BossKilledCount++;
                GrantMajorEnemyReward(SurvivorsEnemyRole.Boss);
                if (!OpenUpgradeRewardDraft(SurvivorsEnemyRole.Boss, requireEvolutionChoice: false) && !_victoryClearedThisRun)
                {
                    EnterVictory();
                }
            }

            if (clearedHordeRushEnemy)
            {
                SpawnHordeRushClearReward(position);
            }

            if (clearedRoamingCacheAmbushEnemy)
            {
                SpawnRoamingCacheAmbushClearReward(position);
            }

            if (clearedArenaShrineEnemy)
            {
                SpawnArenaShrineClearReward(position);
            }
        }

        private void TryTriggerDeathNova(Vector3 position, string source, bool applyAugments)
        {
            float damage = DeathNovaDamage;
            float radius = DeathNovaRadius;
            if (damage <= 0f || radius <= 0f || !applyAugments || !CanApplyDamageAugments(source))
            {
                return;
            }

            var targets = new List<SurvivorsEnemyActor>();
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor target = _enemies[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                Vector3 delta = target.transform.position - position;
                delta.y = 0f;
                float range = radius + Mathf.Max(0f, target.Radius);
                if (delta.sqrMagnitude <= range * range)
                {
                    targets.Add(target);
                }
            }

            if (targets.Count == 0)
            {
                return;
            }

            DeathNovaTriggerCount++;
            PlayFeedback(_killPulse, position, Mathf.Clamp(16 + targets.Count * 7, 18, 72), _killClip);
            for (int i = 0; i < targets.Count; i++)
            {
                SurvivorsEnemyActor target = targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                DamageResult result = target.ApplyDamage(damage, "survivors.augment.death-nova");
                if (result != null && result.HealthDamage > 0d)
                {
                    DeathNovaHitCount++;
                }
            }
        }

        internal void ReleaseEnemy(SurvivorsEnemyActor enemy, DespawnReason reason)
        {
            if (enemy == null)
            {
                return;
            }

            _enemies.Remove(enemy);
            _activeHordeRushEnemies.Remove(enemy);
            _activeRoamingCacheAmbushEnemies.Remove(enemy);
            _activeArenaShrineEnemies.Remove(enemy);
            _enragedMajorThreats.Remove(enemy);
            if (_spawnService != null && enemy.InstanceId.Value > 0)
            {
                _spawnService.Despawn(enemy.InstanceId, reason);
            }
        }

        internal void ReleaseProjectile(SurvivorsProjectileActor projectile, DespawnReason reason)
        {
            if (projectile == null)
            {
                return;
            }

            _projectiles.Remove(projectile);
            if (_spawnService != null && projectile.InstanceId.Value > 0)
            {
                _spawnService.Despawn(projectile.InstanceId, reason);
            }
        }

        internal void CollectPickup(SurvivorsPickupActor pickup)
        {
            if (pickup == null)
            {
                return;
            }

            if (pickup.Kind == SurvivorsPickupKind.Magnet)
            {
                TriggerMagnetRecall();
            }
            else if (pickup.Kind == SurvivorsPickupKind.Health)
            {
                RestoreHealthFromPickup(Mathf.Max(1, pickup.Amount));
            }
            else if (pickup.Kind == SurvivorsPickupKind.BloodShard)
            {
                CollectBloodShardPickup(Mathf.Max(1, pickup.Amount));
            }
            else
            {
                RecordMetricTime(ref _firstExperiencePickupTimeSeconds);
                int gained = GainExperience(Mathf.Max(1, pickup.Amount));
                RecordExperienceCombo(gained);
                ExperiencePickupFeedbackCount++;
            }

            string audioEventId = pickup.Kind == SurvivorsPickupKind.Experience ? AudioEventXpPickup : AudioEventUiSelect;
            PlayFeedback(_pickupPulse, pickup.transform.position, ResolvePickupFeedbackBurstCount(pickup.Kind), _pickupClip, audioEventId, 0.12f);
            _pickups.Remove(pickup);
            if (_spawnService != null && pickup.InstanceId.Value > 0)
            {
                _spawnService.Despawn(pickup.InstanceId, DespawnReason.Completed);
            }
        }

        private void RestoreHealthFromPickup(int amount)
        {
            if (_playerHealth == null || amount <= 0)
            {
                return;
            }

            float before = CurrentHealth;
            _playerHealth.Heal(amount);
            float restored = Mathf.Max(0f, CurrentHealth - before);
            HealthPickupCollectedCount++;
            HealthRestoredByPickups += restored;
        }

        private void CollectBloodShardPickup(int amount)
        {
            int gained = Mathf.Max(1, amount);
            _bonusBloodShardsEarnedThisRun += gained;
            BloodShardPickupCollectedCount++;
            BloodShardsCollectedFromPickups += gained;
        }

        private static int ResolvePickupFeedbackBurstCount(SurvivorsPickupKind kind)
        {
            switch (kind)
            {
                case SurvivorsPickupKind.Magnet:
                    return 28;
                case SurvivorsPickupKind.Health:
                    return 22;
                case SurvivorsPickupKind.BloodShard:
                    return 18;
                default:
                    return 10;
            }
        }

        internal void RecordPickupAttractionFeedback(SurvivorsPickupKind kind, Vector3 position)
        {
            if (kind == SurvivorsPickupKind.Magnet)
            {
                return;
            }

            PickupAttractionFeedbackCount++;
            PlayFeedback(_pickupPulse, position, 6, null);
        }

        internal SurvivorsEnemyActor FindNearestEnemy(Vector3 origin, float range)
        {
            SurvivorsEnemyActor best = null;
            float bestDistance = range * range;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float distance = (enemy.transform.position - origin).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }

        internal IReadOnlyList<SurvivorsEnemyActor> ActiveEnemies => _enemies;

        internal Transform RuntimeWorldRoot => _worldRoot;

        private float ResolveDisplayedWeaponDamage()
        {
            SurvivorsWeaponArchetypeDefinition definition = ResolvePrimaryWeaponDefinitionForDisplay();
            if (definition != null)
            {
                return ResolveWeaponDamage(definition);
            }

            float baseDamage = _projectileDefinition == null ? 0f : (float)_projectileDefinition.BaseDamage;
            return Mathf.Max(0f, baseDamage + DamageBonus + StreakSurgeDamageBonus + RoamingCacheSurgeDamageBonus + ArenaShrineSurgeDamageBonus + WaystoneFocusDamageBonus + WaystoneChainSurgeDamageBonus + HordeRushClearSurgeDamageBonus + WeaponLoadoutSurgeDamageBonus + PassiveLoadoutSurgeDamageBonus + BossRelicSurgeDamageBonus + GemRushDamageBonus + EvolutionChainSurgeDamageBonus + EndlessSurgeDamageBonus);
        }

        private float ResolveDisplayedWeaponCooldownSeconds()
        {
            SurvivorsWeaponArchetypeDefinition definition = ResolvePrimaryWeaponDefinitionForDisplay();
            if (definition != null)
            {
                return ResolveWeaponCooldownSeconds(definition);
            }

            return Mathf.Max(0.12f, CurrentTuning.WeaponCooldownSeconds * Mathf.Max(0.2f, 1f + WeaponCooldownMultiplierBonus + StreakSurgeCooldownMultiplierBonus + RoamingCacheSurgeCooldownMultiplierBonus + ArenaShrineSurgeCooldownMultiplierBonus + WaystoneFocusCooldownMultiplierBonus + WaystoneChainSurgeCooldownMultiplierBonus + HordeRushClearSurgeCooldownMultiplierBonus + WeaponLoadoutSurgeCooldownMultiplierBonus + PassiveLoadoutSurgeCooldownMultiplierBonus + BossRelicSurgeCooldownMultiplierBonus + GemRushCooldownMultiplierBonus + EvolutionChainSurgeCooldownMultiplierBonus + EndlessSurgeCooldownMultiplierBonus));
        }

        private SurvivorsWeaponArchetypeDefinition ResolvePrimaryWeaponDefinitionForDisplay()
        {
            IReadOnlyList<string> weaponIds = _weaponLoadout == null ? null : _weaponLoadout.WeaponIds;
            IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions = _weaponArchetypeDefinitions;
            if (weaponIds == null || weaponIds.Count == 0 || definitions == null)
            {
                return null;
            }

            string primaryWeaponId = weaponIds[0];
            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition definition = definitions[i];
                if (definition != null && string.Equals(definition.Id, primaryWeaponId, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        internal float ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, definition.Damage + DamageBonus + StreakSurgeDamageBonus + RoamingCacheSurgeDamageBonus + ArenaShrineSurgeDamageBonus + WaystoneFocusDamageBonus + WaystoneChainSurgeDamageBonus + HordeRushClearSurgeDamageBonus + WeaponLoadoutSurgeDamageBonus + PassiveLoadoutSurgeDamageBonus + BossRelicSurgeDamageBonus + GemRushDamageBonus + EvolutionChainSurgeDamageBonus + EndlessSurgeDamageBonus);
        }

        internal float ResolveWeaponCooldownSeconds(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null)
            {
                return WeaponCooldownSeconds;
            }

            return Mathf.Max(0.08f, definition.CooldownSeconds * Mathf.Max(0.2f, 1f + WeaponCooldownMultiplierBonus + StreakSurgeCooldownMultiplierBonus + RoamingCacheSurgeCooldownMultiplierBonus + ArenaShrineSurgeCooldownMultiplierBonus + WaystoneFocusCooldownMultiplierBonus + WaystoneChainSurgeCooldownMultiplierBonus + HordeRushClearSurgeCooldownMultiplierBonus + WeaponLoadoutSurgeCooldownMultiplierBonus + PassiveLoadoutSurgeCooldownMultiplierBonus + BossRelicSurgeCooldownMultiplierBonus + GemRushCooldownMultiplierBonus + EvolutionChainSurgeCooldownMultiplierBonus + EndlessSurgeCooldownMultiplierBonus));
        }

        internal bool LaunchProjectile(SurvivorsWeaponArchetypeDefinition definition, Vector3 direction)
        {
            if (definition == null)
            {
                return false;
            }

            return LaunchProjectileFrom(
                definition,
                PlayerPosition + Vector3.up * 0.4f,
                direction,
                definition.ProjectileChainCount + ProjectileChainBonus,
                definition.ProjectilePierceCount + ProjectilePierceBonus,
                definition.ProjectileForkCount + ProjectileForkBonus,
                definition.ProjectileReturnCount + ProjectileReturnBonus,
                null);
        }

        internal bool LaunchProjectileFrom(
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 origin,
            Vector3 direction,
            int remainingChains,
            int remainingPierces,
            int remainingForks,
            int remainingReturns,
            HashSet<int> ignoredEnemyIds)
        {
            if (definition == null || _spawnService == null)
            {
                return false;
            }

            Vector3 resolvedDirection = direction.sqrMagnitude <= 0.001f ? Vector3.forward : direction.normalized;
            long sequence = ++_spawnSequence;
            _poseResolver.RegisterExplicitPose(sequence, origin + resolvedDirection * 0.55f);
            SpawnResult result = _spawnService.Spawn(new WorldSpawnRequest(
                BasicSurvivorsGame.ProjectileSpawnableId,
                BasicSurvivorsGame.ExplicitSpawnChannelId,
                sequence,
                new WorldSpawnRequestContext("SurvivorsTemplate", groupId: definition.Id)));
            if (!result.Succeeded || result.Instance == null)
            {
                return false;
            }

            SurvivorsProjectileActor projectile = result.Instance.GetComponent<SurvivorsProjectileActor>();
            projectile.Initialize(
                this,
                definition,
                resolvedDirection,
                definition.ProjectileSpeed,
                ResolveWeaponDamage(definition),
                definition.ProjectileRadius,
                definition.ProjectileLifetimeSeconds,
                remainingChains,
                remainingPierces,
                remainingForks,
                remainingReturns,
                ignoredEnemyIds);
            _projectiles.Add(projectile);
            ProjectileLaunchCount++;
            PlayFeedback(_firePulse, origin, 8, _fireClip);
            return true;
        }

        internal void CollectEnemiesWithinRadius(Vector3 origin, float radius, List<SurvivorsEnemyActor> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            float radiusSquared = radius * radius;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if ((enemy.transform.position - origin).sqrMagnitude <= radiusSquared)
                {
                    results.Add(enemy);
                }
            }
        }

        internal void RecordOrbitHit()
        {
            OrbitHitCount++;
        }

        internal bool ApplyOrbitKnockback(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition)
        {
            if (enemy == null || definition == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
            {
                return false;
            }

            bool crimsonAegis = IsCrimsonAegisOrbitDefinition(definition) &&
                IsEvolutionActive(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId);
            float distance = crimsonAegis
                ? Mathf.Max(CurrentTuning.OrbitKnockbackDistance, CurrentTuning.CrimsonAegisOrbitKnockbackDistance)
                : CurrentTuning.OrbitKnockbackDistance;
            distance = Mathf.Max(0f, distance);
            if (distance <= 0f)
            {
                return false;
            }

            Vector3 enemyPosition = enemy.transform.position;
            Vector3 away = enemyPosition - PlayerPosition;
            away.y = 0f;
            if (away.sqrMagnitude <= 0.0001f)
            {
                away = PlayerForward;
            }

            if (away.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            Vector3 direction = away.normalized;
            enemy.transform.position += direction * distance;
            enemy.transform.forward = direction;
            OrbitKnockbackCount++;
            string name = string.IsNullOrWhiteSpace(definition.DisplayName) ? "Orbit" : definition.DisplayName;
            LastOrbitKnockbackFeedbackLabel = crimsonAegis
                ? $"Crimson Aegis pushed {enemy.DisplayName}"
                : $"{name} pushed {enemy.DisplayName}";
            return true;
        }

        private static bool IsCrimsonAegisOrbitDefinition(SurvivorsWeaponArchetypeDefinition definition)
        {
            return definition != null &&
                (string.Equals(definition.Id, BasicSurvivorsGame.OrbitWardWeaponContentId, StringComparison.Ordinal) ||
                 string.Equals(definition.Id, BasicSurvivorsGame.ThornHaloWeaponContentId, StringComparison.Ordinal));
        }

        internal void RecordMeleeSwing()
        {
            MeleeSwingCount++;
        }

        internal void RecordMeleeHit()
        {
            MeleeHitCount++;
        }

        internal void RecordBurstPulse()
        {
            BurstPulseCount++;
        }

        internal void RecordBurstHit()
        {
            BurstHitCount++;
        }

        internal void RecordHitscanFire()
        {
            HitscanFireCount++;
        }

        internal void RecordHitscanHit()
        {
            HitscanHitCount++;
        }

        internal void RecordTempestPrismArcHit(SurvivorsEnemyActor source, SurvivorsEnemyActor target)
        {
            TempestPrismArcHitCount++;
            string sourceName = source == null || string.IsNullOrWhiteSpace(source.DisplayName) ? "target" : source.DisplayName;
            string targetName = target == null || string.IsNullOrWhiteSpace(target.DisplayName) ? "nearby enemy" : target.DisplayName;
            LastTempestPrismArcFeedbackLabel = $"Tempest Prism arced from {sourceName} to {targetName}";
        }

        internal void RecordProjectilePierceHit()
        {
            ProjectilePierceHitCount++;
        }

        internal void RecordProjectileChainHit()
        {
            ProjectileChainHitCount++;
        }

        internal void RecordProjectileForkSpawn()
        {
            ProjectileForkSpawnCount++;
        }

        internal void RecordProjectileReturnStart()
        {
            ProjectileReturnStartCount++;
        }

        internal void RecordPayloadThrow()
        {
            PayloadThrowCount++;
        }

        internal void RecordPayloadPlaced()
        {
            PayloadPlacedCount++;
        }

        internal void RecordPayloadDetonation()
        {
            PayloadDetonationCount++;
        }

        internal void RecordPayloadExplosionHit()
        {
            PayloadExplosionHitCount++;
        }

        internal void RecordPayloadHazardTick()
        {
            PayloadHazardTickCount++;
        }

        internal void RecordPayloadHazardSnare(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition, Vector3 origin)
        {
            if (enemy == null || definition == null)
            {
                return;
            }

            PayloadHazardSnareCount++;
            LastPayloadHazardSnareFeedbackLabel = $"{definition.DisplayName} hazard snared {enemy.DisplayName}";
            RegisterPayloadHazardChainSnare(origin, definition);
        }

        private void RegisterPayloadHazardChainSnare(Vector3 origin, SurvivorsWeaponArchetypeDefinition definition)
        {
            if (_payloadHazardChainCooldownTimer > 0f)
            {
                return;
            }

            if (_payloadHazardChainWindowTimer <= 0f)
            {
                _payloadHazardChainSnareCount = 0;
            }

            _payloadHazardChainSnareCount++;
            _payloadHazardChainWindowTimer = Mathf.Max(0.05f, CurrentTuning.PayloadHazardChainWindowSeconds);
            int threshold = Mathf.Max(1, CurrentTuning.PayloadHazardChainSnareThreshold);
            if (_payloadHazardChainSnareCount < threshold)
            {
                return;
            }

            TriggerPayloadHazardChain(origin, definition);
        }

        private void TriggerPayloadHazardChain(Vector3 origin, SurvivorsWeaponArchetypeDefinition definition)
        {
            int caughtSnares = Mathf.Max(1, _payloadHazardChainSnareCount);
            _payloadHazardChainSnareCount = 0;
            _payloadHazardChainWindowTimer = 0f;
            _payloadHazardChainCooldownTimer = Mathf.Max(0f, CurrentTuning.PayloadHazardChainCooldownSeconds);

            float radius = Mathf.Max(0f, CurrentTuning.PayloadHazardChainPulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.PayloadHazardChainPulseDamage);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(origin, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor target = targets[i];
                    if (target == null || !target.IsAlive || IsMajorRewardRole(target.Role))
                    {
                        continue;
                    }

                    target.ApplyDamage(damage, "survivors.payload-hazard.chain");
                    hitCount++;
                }
            }

            int gemCount = Mathf.Max(0, CurrentTuning.PayloadHazardChainExperienceGemCount);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * Mathf.Max(0.1f, CurrentTuning.PayloadHazardChainExperienceMultiplier)));
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.23f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.78f;
                if (SpawnPickup(SurvivorsPickupKind.Experience, origin + offset, xpPerGem) != null)
                {
                    spawnedExperience += xpPerGem;
                    PayloadHazardChainExperienceGemDropCount++;
                }
            }

            PayloadHazardChainActivationCount++;
            PayloadHazardChainPulseHitCount += hitCount;
            string name = definition == null || string.IsNullOrWhiteSpace(definition.DisplayName)
                ? "Payload"
                : definition.DisplayName;
            LastPayloadHazardChainFeedbackLabel = $"Trap Chain: {name} caught {caughtSnares} snares, +{spawnedExperience} XP, {hitCount} enemies hit";
            RecordStreakRewardFeedback(LastPayloadHazardChainFeedbackLabel, new Color(0.62f, 0.9f, 1f));
            PlayFeedback(_levelUpPulse, origin, Mathf.Clamp(28 + hitCount * 4, 34, 78), _pickupClip);
        }

        private void TickPayloadHazardChain(float deltaTime)
        {
            float dt = Mathf.Max(0f, deltaTime);
            if (_payloadHazardChainCooldownTimer > 0f)
            {
                _payloadHazardChainCooldownTimer = Mathf.Max(0f, _payloadHazardChainCooldownTimer - dt);
            }

            if (_payloadHazardChainWindowTimer <= 0f)
            {
                return;
            }

            _payloadHazardChainWindowTimer = Mathf.Max(0f, _payloadHazardChainWindowTimer - dt);
            if (_payloadHazardChainWindowTimer <= 0f)
            {
                _payloadHazardChainSnareCount = 0;
            }
        }

        private int CountEnemiesByRole(SurvivorsEnemyRole role)
        {
            int count = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy != null && enemy.IsAlive && enemy.Role == role)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountEliteEnemies()
        {
            int count = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy != null && enemy.IsAlive && IsEliteRole(enemy.Role))
                {
                    count++;
                }
            }

            return count;
        }

        private int CountAuthoredThreatLifeBars(bool showBossLifeBar)
        {
            int count = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if (showBossLifeBar && enemy.ShowBossLifeBar)
                {
                    count++;
                }
                else if (!showBossLifeBar && enemy.ShowOverheadLifeBar)
                {
                    count++;
                }
            }

            return count;
        }

        private string ResolveActiveMajorThreatLifeBarSummary()
        {
            var labels = new List<string>(ActiveMajorThreatLifeBarCount);
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive || !ShouldShowThreatLifeBar(enemy))
                {
                    continue;
                }

                string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                    ? ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                    : enemy.DisplayName;
                labels.Add($"{name} {Mathf.RoundToInt(enemy.HealthFraction * 100f)}%");
            }

            return string.Join(", ", labels);
        }

        private SurvivorsEnemyActor ResolveCurrentMajorThreatForHud()
        {
            SurvivorsEnemyActor selected = null;
            int selectedPriority = -1;
            float selectedHealthFraction = 2f;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive || !ShouldShowThreatLifeBar(enemy))
                {
                    continue;
                }

                int priority = ResolveMajorThreatHealthPriority(enemy.Role);
                float healthFraction = Mathf.Clamp01(enemy.HealthFraction);
                if (priority > selectedPriority ||
                    (priority == selectedPriority && healthFraction < selectedHealthFraction))
                {
                    selected = enemy;
                    selectedPriority = priority;
                    selectedHealthFraction = healthFraction;
                }
            }

            return selected;
        }

        private static bool ShouldShowThreatLifeBar(SurvivorsEnemyActor enemy)
        {
            return enemy != null &&
                enemy.IsAlive &&
                (enemy.ShowBossLifeBar || enemy.ShowOverheadLifeBar);
        }

        private static int ResolveMajorThreatHealthPriority(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return 4;
                case SurvivorsEnemyRole.Miniboss:
                    return 3;
                case SurvivorsEnemyRole.DreadElite:
                    return 2;
                case SurvivorsEnemyRole.Elite:
                    return 1;
                default:
                    return 0;
            }
        }

        private static string ResolveMajorThreatHealthFallbackLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return "Final Boss";
                case SurvivorsEnemyRole.Miniboss:
                    return "Miniboss";
                case SurvivorsEnemyRole.DreadElite:
                    return "Dread Elite";
                default:
                    return "Elite";
            }
        }

        private static bool IsEliteRole(SurvivorsEnemyRole role)
        {
            return role == SurvivorsEnemyRole.Elite || role == SurvivorsEnemyRole.DreadElite;
        }

        private static bool IsMajorRewardRole(SurvivorsEnemyRole role)
        {
            return IsEliteRole(role) || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;
        }

        internal static bool IsMajorThreatSlamRole(SurvivorsEnemyRole role)
        {
            return role == SurvivorsEnemyRole.DreadElite || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;
        }

        internal void RecordMajorThreatSlamTelegraph(SurvivorsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return;
            }

            MajorThreatSlamWarningCount++;
            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastMajorThreatSlamFeedbackLabel = $"{name} winding slam";
            RecordStreakRewardFeedback(LastMajorThreatSlamFeedbackLabel, ResolveMajorThreatEnrageFeedbackColor(enemy.Role));
            float radius = Mathf.Max(0.5f, CurrentTuning.MajorThreatSlamRadius + enemy.Radius * 0.35f);
            RecordMajorThreatSlamTelegraphEffect(enemy.transform.position, enemy.Role, radius, CurrentTuning.MajorThreatSlamTelegraphSeconds);
            PlayFeedback(_bossPulse, enemy.transform.position, enemy.Role == SurvivorsEnemyRole.Boss ? 42 : 30, _dangerClip);
        }

        internal void ResolveMajorThreatSlam(SurvivorsEnemyActor enemy)
        {
            if (enemy == null || !enemy.IsAlive || State != SurvivorsRunState.Playing)
            {
                return;
            }

            MajorThreatSlamCastCount++;
            float radius = Mathf.Max(0.5f, CurrentTuning.MajorThreatSlamRadius + enemy.Radius * 0.35f);
            float damage = Mathf.Max(0f, CurrentTuning.MajorThreatSlamDamage);
            float distance = Vector3.Distance(enemy.transform.position, PlayerPosition);
            bool hit = damage > 0f && distance <= radius + CurrentTuning.PlayerRadius;
            float healthBefore = CurrentHealth;
            float barrierBefore = BarrierValue;
            if (hit)
            {
                ApplyDamageToPlayer(damage, "combatant.survivors.enemy.slam." + enemy.InstanceId.Value);
            }

            bool damagedPlayer = CurrentHealth < healthBefore || BarrierValue < barrierBefore;
            if (damagedPlayer)
            {
                MajorThreatSlamHitCount++;
            }

            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastMajorThreatSlamFeedbackLabel = damagedPlayer ? $"{name} slam hit" : $"{name} slam missed";
            PlayFeedback(_bossPulse, enemy.transform.position, enemy.Role == SurvivorsEnemyRole.Boss ? 58 : 40, _dangerClip);
        }

        private void TryTriggerMajorThreatEnrage(SurvivorsEnemyActor enemy)
        {
            if (enemy == null ||
                !enemy.IsAlive ||
                State != SurvivorsRunState.Playing ||
                !IsMajorRewardRole(enemy.Role) ||
                _enragedMajorThreats.Contains(enemy))
            {
                return;
            }

            float threshold = Mathf.Clamp01(CurrentTuning.MajorThreatEnrageHealthThreshold);
            if (threshold <= 0f || enemy.HealthFraction > threshold)
            {
                return;
            }

            _enragedMajorThreats.Add(enemy);
            int spawned = SpawnMajorThreatEnrageSupport(enemy);
            MajorThreatEnrageCount++;
            MajorThreatEnrageSupportSpawnCount += spawned;

            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastMajorThreatEnrageFeedbackLabel = $"{name} enraged: +{spawned} support";
            RecordStreakRewardFeedback(LastMajorThreatEnrageFeedbackLabel, ResolveMajorThreatEnrageFeedbackColor(enemy.Role));
            PlayFeedback(_bossPulse, enemy.transform.position, enemy.Role == SurvivorsEnemyRole.Boss ? 64 : 42, _dangerClip);
        }

        private int SpawnMajorThreatEnrageSupport(SurvivorsEnemyActor enemy)
        {
            int requested = ResolveMajorThreatEnrageSupportCount(enemy.Role);
            if (requested <= 0)
            {
                return 0;
            }

            int available = Mathf.Max(
                0,
                ResolveEnemyMaximumAlive() + Mathf.Max(0, CurrentTuning.MajorThreatEnrageExtraAliveAllowance) - _enemies.Count);
            int targetCount = Mathf.Min(requested, available);
            if (targetCount <= 0)
            {
                return 0;
            }

            Vector3 center = enemy.transform.position;
            float radius = Mathf.Max(enemy.Radius + 1.35f, CurrentTuning.MajorThreatEnrageSupportRadius);
            int spawned = 0;
            for (int i = 0; i < targetCount; i++)
            {
                float laneRadius = radius + ((i & 1) == 0 ? 0f : 1.15f);
                SurvivorsEnemyRole supportRole = ResolveMajorThreatEnrageSupportRole(enemy.Role, i);
                if (SpawnGameplayEnemyOffscreen(
                    supportRole,
                    _spawnSequence + i + MajorThreatEnrageCount * 31 + 701,
                    radius,
                    radius + CurrentTuning.SpawnBandDepth,
                    "major-threat-enrage") != null)
                {
                    spawned++;
                }
            }

            return spawned;
        }

        private int ResolveMajorThreatEnrageSupportCount(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return Mathf.Max(0, CurrentTuning.MajorThreatEnrageBossSupportCount);
                case SurvivorsEnemyRole.Miniboss:
                    return Mathf.Max(0, CurrentTuning.MajorThreatEnrageMinibossSupportCount);
                case SurvivorsEnemyRole.DreadElite:
                    return Mathf.Max(0, CurrentTuning.MajorThreatEnrageEliteSupportCount + 2);
                default:
                    return Mathf.Max(0, CurrentTuning.MajorThreatEnrageEliteSupportCount);
            }
        }

        private static SurvivorsEnemyRole ResolveMajorThreatEnrageSupportRole(SurvivorsEnemyRole majorRole, int index)
        {
            if (majorRole == SurvivorsEnemyRole.Boss)
            {
                if (index % 6 == 0) return SurvivorsEnemyRole.Bruiser;
                if (index % 5 == 0) return SurvivorsEnemyRole.Splitter;
                if (index % 4 == 0) return SurvivorsEnemyRole.Spitter;
                if (index % 2 == 0) return SurvivorsEnemyRole.Runner;
                return SurvivorsEnemyRole.Swarm;
            }

            if (majorRole == SurvivorsEnemyRole.Miniboss)
            {
                if (index % 4 == 0) return SurvivorsEnemyRole.Bruiser;
                if (index % 3 == 0) return SurvivorsEnemyRole.Spitter;
                if (index % 2 == 0) return SurvivorsEnemyRole.Runner;
                return SurvivorsEnemyRole.Swarm;
            }

            if (majorRole == SurvivorsEnemyRole.DreadElite && index % 4 == 0)
            {
                return SurvivorsEnemyRole.Spitter;
            }

            return index % 2 == 0 ? SurvivorsEnemyRole.Runner : SurvivorsEnemyRole.Swarm;
        }

        private static Color ResolveMajorThreatEnrageFeedbackColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.2f, 0.3f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(0.95f, 0.36f, 1f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.35f, 0.85f, 1f);
                default:
                    return new Color(1f, 0.68f, 0.2f);
            }
        }

        private static SurvivorsEnemyRole ResolveDebugMajorEnemyRole(SurvivorsEnemyRole role)
        {
            if (IsEliteRole(role) || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss)
            {
                return role;
            }

            return SurvivorsEnemyRole.Elite;
        }

        private void EnsureRunStartedForTest()
        {
            if (!_runStarted)
            {
                StartRun();
            }
        }

        private void BuildRuntimeWorld()
        {
            _worldRoot = new GameObject("SurvivorsRuntimeWorld").transform;
            _worldRoot.SetParent(transform, false);
            _prefabRoot = new GameObject("SurvivorsTemplatePrefabSources").transform;
            _prefabRoot.SetParent(_worldRoot, false);
            _prefabRoot.gameObject.SetActive(false);
            _playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _playerObject.name = "Survivors Player";
            _playerObject.transform.SetParent(_worldRoot, false);
            _playerObject.transform.localScale = new Vector3(0.85f, 1f, 0.85f);
            _playerRenderer = _playerObject.GetComponentInChildren<Renderer>();
            ApplyColor(_playerRenderer, ActiveUiTheme.GetPlayerColor(new Color(0.78f, 0.91f, 1f)));

            _enemyPrefab = CreatePrimitivePrefab("Survivors Swarm Enemy Prefab", PrimitiveType.Capsule, new Color(0.88f, 0.22f, 0.32f), typeof(SurvivorsEnemyActor));
            _experiencePickupPrefab = CreatePrimitivePrefab("Survivors XP Gem Prefab", PrimitiveType.Sphere, ActiveUiTheme.GetExperiencePickupColor(new Color(0.22f, 0.83f, 1f)), typeof(SurvivorsPickupActor));
            _magnetPickupPrefab = CreatePrimitivePrefab("Survivors Magnet Prefab", PrimitiveType.Sphere, ActiveUiTheme.GetArenaAccentColor(new Color(1f, 0.86f, 0.18f)), typeof(SurvivorsPickupActor));
            _healthPickupPrefab = CreatePrimitivePrefab("Survivors Vital Shard Prefab", PrimitiveType.Sphere, ActiveUiTheme.GetHealthPickupColor(new Color(1f, 0.24f, 0.42f)), typeof(SurvivorsPickupActor));
            _bloodShardPickupPrefab = CreatePrimitivePrefab("Survivors Blood Shard Prefab", PrimitiveType.Sphere, ActiveUiTheme.GetCurrencyPickupColor(new Color(0.96f, 0.08f, 0.18f)), typeof(SurvivorsPickupActor));
            _projectilePrefab = CreatePrimitivePrefab("Survivors Arcane Bolt Prefab", PrimitiveType.Sphere, ResolveProjectileFallbackColor(), typeof(SurvivorsProjectileActor));

            BuildSceneLighting();

            _poseResolver = new SurvivorsSpawnPoseResolver(this);
            var spawnables = new[]
            {
                new SpawnableDefinition(BasicSurvivorsGame.SwarmEnemySpawnableId, new GameObjectPrefabProvider(_enemyPrefab), 24, Mathf.Max(CurrentTuning.EnemyMaximumAlive + 96, 256), "survivors-enemy-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.MinibossEnemySpawnableId, new GameObjectPrefabProvider(_enemyPrefab), 1, 8, "survivors-miniboss-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.BossEnemySpawnableId, new GameObjectPrefabProvider(_enemyPrefab), 1, 4, "survivors-boss-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.ExperiencePickupSpawnableId, new GameObjectPrefabProvider(_experiencePickupPrefab), 16, 256, "survivors-xp-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.MagnetPickupSpawnableId, new GameObjectPrefabProvider(_magnetPickupPrefab), 2, 16, "survivors-magnet-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.HealthPickupSpawnableId, new GameObjectPrefabProvider(_healthPickupPrefab), 4, 32, "survivors-health-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.BloodShardPickupSpawnableId, new GameObjectPrefabProvider(_bloodShardPickupPrefab), 4, 48, "survivors-blood-shard-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.ProjectileSpawnableId, new GameObjectPrefabProvider(_projectilePrefab), 12, 96, "survivors-projectile-pool")
            };
            _spawnService = new WorldSpawnService(new SpawnableCatalog(spawnables), _poseResolver, _worldRoot, rootName: "SurvivorsWorldSpawning");
            _spawnService.Warmup();

            if (buildVisualsOnAwake)
            {
                BuildArenaVisuals();
            }

            BuildFeedbackPresentation();
            EnsureCamera();
        }

        private GameObject CreatePrimitivePrefab(string name, PrimitiveType primitive, Color color, Type actorType)
        {
            GameObject prefab = GameObject.CreatePrimitive(primitive);
            prefab.name = name;
            prefab.transform.SetParent(_prefabRoot, false);
            prefab.transform.localScale = Vector3.one;
            ApplyColor(prefab.GetComponentInChildren<Renderer>(), color);
            prefab.AddComponent(actorType);
            if (actorType == typeof(SurvivorsProjectileActor))
            {
                var trail = prefab.AddComponent<TrailRenderer>();
                trail.time = 0.18f;
                trail.startWidth = 0.2f;
                trail.endWidth = 0.02f;
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.startColor = WithAlpha(ActiveUiTheme.GetFeedbackAccentColor(new Color(0.86f, 0.48f, 1f)), 0.95f);
                trail.endColor = WithAlpha(ActiveUiTheme.GetArenaAccentColor(new Color(0.18f, 0.86f, 1f)), 0f);
            }

            prefab.SetActive(false);
            return prefab;
        }

        private void BuildSceneLighting()
        {
            GameObject lightObject = new GameObject(DirectionalLightName);
            lightObject.transform.SetParent(_worldRoot, false);
            lightObject.transform.localRotation = Quaternion.Euler(52f, -34f, 0f);

            Light directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.color = new Color(1f, 0.96f, 0.86f);
            directionalLight.intensity = 1.15f;
            directionalLight.shadows = LightShadows.Soft;
        }

        private void BuildArenaVisuals()
        {
            _arenaTiles.Clear();
            _arenaLandmarks.Clear();
            _arenaLandmarkKeys.Clear();

            GameObject tileRoot = new GameObject("Survivors Infinite Arena Tiles");
            tileRoot.transform.SetParent(_worldRoot, false);
            _arenaTileRoot = tileRoot.transform;
            BuildInfiniteArenaTiles();
            BuildInfiniteArenaLandmarks();

            GameObject followRoot = new GameObject("Survivors Moving Arena Readability");
            followRoot.transform.SetParent(_worldRoot, false);
            _arenaFollowRoot = followRoot.transform;

            BuildWaystoneCompassArrow();
            UpdateArenaPresentation();
        }

        private void BuildWaystoneCompassArrow()
        {
            GameObject root = new GameObject("Waystone Compass Arrow");
            root.transform.SetParent(_arenaFollowRoot, false);
            root.transform.localPosition = Vector3.zero;
            _waystoneCompassRoot = root.transform;

            CreateArenaPrimitive(
                "Waystone Compass Shaft",
                PrimitiveType.Cube,
                new Vector3(0f, 0.18f, 3.08f),
                new Vector3(0.16f, 0.08f, 1.36f),
                ActiveUiTheme.GetArenaAccentColor(new Color(0.32f, 0.94f, 1f)),
                _waystoneCompassRoot);

            GameObject head = CreateArenaPrimitive(
                "Waystone Compass Head",
                PrimitiveType.Cube,
                new Vector3(0f, 0.2f, 3.88f),
                new Vector3(0.62f, 0.1f, 0.62f),
                ActiveUiTheme.GetFeedbackAccentColor(new Color(0.92f, 1f, 0.42f)),
                _waystoneCompassRoot);
            head.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);

            CreateArenaPrimitive(
                "Waystone Compass Pulse",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.13f, 3.88f),
                new Vector3(1.05f, 0.025f, 1.05f),
                ActiveUiTheme.GetArenaGridColor(new Color(0.12f, 0.58f, 0.72f)),
                _waystoneCompassRoot);

            root.SetActive(false);
        }

        private void BuildInfiniteArenaTiles()
        {
            for (int x = -InfiniteArenaGridRadius; x <= InfiniteArenaGridRadius; x++)
            {
                for (int z = -InfiniteArenaGridRadius; z <= InfiniteArenaGridRadius; z++)
                {
                    bool alternate = ((x + z) & 1) == 0;
                    Color tileColor = alternate
                        ? ActiveUiTheme.GetArenaFloorColor(new Color(0.045f, 0.055f, 0.058f))
                        : ActiveUiTheme.GetArenaGridColor(new Color(0.055f, 0.065f, 0.05f));
                    GameObject tile = CreateArenaPrimitive(
                        "Infinite Arena Tile " + x.ToString() + "," + z.ToString(),
                        PrimitiveType.Cube,
                        new Vector3(x * InfiniteArenaTileSize, -0.08f, z * InfiniteArenaTileSize),
                        new Vector3(InfiniteArenaTileSize, 0.035f, InfiniteArenaTileSize),
                        tileColor,
                        _arenaTileRoot);
                    _arenaTiles.Add(tile.transform);
                }
            }
        }

        private void BuildInfiniteArenaLandmarks()
        {
            for (int i = 0; i < InfiniteArenaLandmarkCount; i++)
            {
                GameObject landmark = CreateArenaPrimitive(
                    "Infinite Arena Waystone " + (i + 1).ToString(),
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(0.42f, 0.52f, 0.42f),
                    ResolveArenaLandmarkColor(i),
                    _arenaTileRoot);
                _arenaLandmarks.Add(landmark.transform);
                _arenaLandmarkKeys.Add(0L);
            }
        }

        private void RegisterKillStreak(Vector3 position)
        {
            _killStreakCount = _killStreakTimer > 0f ? _killStreakCount + 1 : 1;
            _killStreakTimer = KillStreakWindowSeconds;
            BestKillStreak = Mathf.Max(BestKillStreak, _killStreakCount);

            if (_killStreakCount % KillStreakExperienceInterval == 0)
            {
                int amount = Mathf.Max(2, Mathf.CeilToInt(CurrentTuning.EnemyExperienceReward * (2f + _killStreakCount * 0.15f)));
                Vector3 offset = new Vector3(Mathf.Sin(_killStreakCount) * 0.55f, 0f, Mathf.Cos(_killStreakCount) * 0.55f);
                if (SpawnPickup(SurvivorsPickupKind.Experience, position + offset, amount) != null)
                {
                    StreakBonusDropCount++;
                    RecordStreakRewardFeedback($"{_killStreakCount} Streak: Bonus XP +{amount}", new Color(0.28f, 0.86f, 1f));
                }
            }

            if (_killStreakCount % KillStreakHealthInterval == 0)
            {
                Vector3 offset = new Vector3(Mathf.Cos(_killStreakCount * 0.7f) * 0.65f, 0f, Mathf.Sin(_killStreakCount * 0.7f) * 0.65f);
                if (TryDropHealthPickup(position + offset))
                {
                    StreakHealthDropCount++;
                    RecordStreakRewardFeedback($"{_killStreakCount} Streak: Vital Shard", new Color(0.42f, 1f, 0.56f));
                }
            }

            if (_killStreakCount % KillStreakMagnetInterval == 0)
            {
                Vector3 offset = new Vector3(Mathf.Cos(_killStreakCount) * 0.75f, 0f, Mathf.Sin(_killStreakCount) * 0.75f);
                if (SpawnPickup(SurvivorsPickupKind.Magnet, position + offset, 1) != null)
                {
                    StreakMagnetDropCount++;
                    RecordStreakRewardFeedback($"{_killStreakCount} Streak: Magnet Recall", new Color(0.55f, 0.78f, 1f));
                }
            }

            if (_killStreakCount % KillStreakBloodShardInterval == 0)
            {
                int amount = Mathf.Max(1, CurrentTuning.BloodShardPickupAmount + (_killStreakCount / KillStreakBloodShardInterval) - 1);
                Vector3 offset = new Vector3(Mathf.Sin(_killStreakCount * 0.31f) * 0.82f, 0f, Mathf.Cos(_killStreakCount * 0.31f) * 0.82f);
                if (SpawnPickup(SurvivorsPickupKind.BloodShard, position + offset, amount) != null)
                {
                    StreakBloodShardDropCount++;
                    RecordStreakRewardFeedback($"{_killStreakCount} Streak: {CurrencyDisplayName} +{amount}", new Color(1f, 0.34f, 0.42f));
                }
            }

            TryActivateStreakSurge();
        }

        private void SpawnMajorRewardPickupCache(Vector3 position, SurvivorsEnemyRole role, float radius)
        {
            if (!IsMajorRewardRole(role))
            {
                return;
            }

            int gemCount = ResolveMajorRewardCacheExperienceGemCount(role);
            int xpPerGem = ResolveMajorRewardCacheExperiencePerGem(role);
            float cacheRadius = Mathf.Max(0.9f, radius + MajorRewardPickupCacheRadiusPadding);
            int spawnedExperience = 0;
            int attractedPickupCount = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.18f) / gemCount) * Mathf.PI * 2f;
                float ringOffset = 0.18f * (i % 2);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (cacheRadius + ringOffset);
                SurvivorsPickupActor pickup = SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem);
                if (pickup != null)
                {
                    spawnedExperience += xpPerGem;
                    MajorRewardCacheExperienceGemDropCount++;
                    if (StartMajorRewardCacheAttraction(pickup))
                    {
                        attractedPickupCount++;
                    }
                }
            }

            int specialDropCount = SpawnMajorRewardSpecialPickups(position, role, cacheRadius, out int attractedSpecialDropCount);
            attractedPickupCount += attractedSpecialDropCount;
            if (spawnedExperience <= 0 && specialDropCount <= 0)
            {
                return;
            }

            MajorRewardCacheDropCount++;
            MajorRewardCacheSpecialDropCount += specialDropCount;
            MajorRewardCacheAttractedPickupCount += attractedPickupCount;
            string rewardLabel = ResolveMajorRewardDropLabel(role);
            string specialLabel = specialDropCount > 0 ? $" + {specialDropCount} special" : string.Empty;
            string pullLabel = attractedPickupCount > 0 ? $" pull x{attractedPickupCount}" : string.Empty;
            string label = $"{rewardLabel}: Cache +{spawnedExperience} XP{specialLabel}{pullLabel}";
            LastMajorRewardCacheFeedbackLabel = label;
            RecordStreakRewardFeedback(label, ResolveMajorRewardDropColor(role));
        }

        private int ResolveMajorRewardCacheExperienceGemCount(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return 12;
                case SurvivorsEnemyRole.Miniboss:
                    return 8;
                case SurvivorsEnemyRole.DreadElite:
                    return 7;
                default:
                    return 5;
            }
        }

        private int ResolveMajorRewardCacheExperiencePerGem(SurvivorsEnemyRole role)
        {
            float multiplier;
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    multiplier = 5.5f;
                    break;
                case SurvivorsEnemyRole.Miniboss:
                    multiplier = 3.25f;
                    break;
                case SurvivorsEnemyRole.DreadElite:
                    multiplier = 2.35f;
                    break;
                default:
                    multiplier = 1.85f;
                    break;
            }

            float escalationMultiplier = 1f + RunEscalationLevel * 0.1f;
            return Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * multiplier * escalationMultiplier));
        }

        private int SpawnMajorRewardSpecialPickups(Vector3 position, SurvivorsEnemyRole role, float cacheRadius, out int attractedPickupCount)
        {
            attractedPickupCount = 0;
            int specialDropCount = 0;
            Vector3 magnetPosition = position + new Vector3(cacheRadius * 0.62f, 0f, -cacheRadius * 0.36f);
            SurvivorsPickupActor magnet = SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1);
            if (magnet != null)
            {
                specialDropCount++;
                if (StartMajorRewardCacheAttraction(magnet))
                {
                    attractedPickupCount++;
                }
            }

            int shardAmount = ResolveMajorRewardCacheBloodShardAmount(role);
            if (shardAmount > 0)
            {
                Vector3 shardPosition = position + new Vector3(-cacheRadius * 0.58f, 0f, cacheRadius * 0.42f);
                SurvivorsPickupActor shard = SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount);
                if (shard != null)
                {
                    specialDropCount++;
                    if (StartMajorRewardCacheAttraction(shard))
                    {
                        attractedPickupCount++;
                    }
                }
            }

            return specialDropCount;
        }

        private bool StartMajorRewardCacheAttraction(SurvivorsPickupActor pickup)
        {
            return pickup != null && pickup.StartRewardCacheAttraction(CurrentTuning.MajorRewardCacheAttractionSpeedMultiplier);
        }

        private int ResolveMajorRewardCacheBloodShardAmount(SurvivorsEnemyRole role)
        {
            int baseAmount = Mathf.Max(1, CurrentTuning.BloodShardPickupAmount);
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return baseAmount + 4;
                case SurvivorsEnemyRole.Miniboss:
                    return baseAmount + 2;
                case SurvivorsEnemyRole.DreadElite:
                    return baseAmount + 1;
                default:
                    return 0;
            }
        }

        private void TryActivateStreakSurge()
        {
            if (_killStreakCount <= 0 || _killStreakCount % KillStreakSurgeInterval != 0)
            {
                return;
            }

            int tier = Mathf.Clamp(_killStreakCount / KillStreakSurgeInterval, 1, KillStreakSurgeMaxTier);
            StreakSurgeTier = tier;
            StreakSurgeActivationCount++;
            _streakSurgeTimer = StreakSurgeDurationSeconds;
            RecordStreakRewardFeedback($"{_killStreakCount} Streak: Tempo Surge T{tier}", new Color(1f, 0.74f, 0.24f));
        }

        private void RecordStreakRewardFeedback(string label, Color color)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return;
            }

            StreakRewardFeedbackCount++;
            LastStreakRewardFeedbackLabel = label;
            _streakRewardFeedbackLabel = label;
            _streakRewardFeedbackColor = color;
            _streakRewardFeedbackTimer = StreakRewardFeedbackDurationSeconds;
        }

        private bool TryDropHealthPickup(Vector3 position)
        {
            if (_playerHealth == null || CurrentTuning.HealthPickupHealAmount <= 0 || CurrentHealth >= MaxHealth - 0.01f)
            {
                return false;
            }

            return SpawnPickup(SurvivorsPickupKind.Health, position, CurrentTuning.HealthPickupHealAmount) != null;
        }

        private GameObject CreateArenaPrimitive(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            return CreateArenaPrimitive(name, primitive, position, scale, color, _worldRoot);
        }

        private GameObject CreateArenaPrimitive(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Transform parent)
        {
            GameObject instance = GameObject.CreatePrimitive(primitive);
            instance.name = name;
            instance.transform.SetParent(parent == null ? _worldRoot : parent, false);
            instance.transform.localPosition = position;
            instance.transform.localScale = scale;
            ApplyColor(instance.GetComponentInChildren<Renderer>(), color);
            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            return instance;
        }

        private void BuildFeedbackPresentation()
        {
            GameObject root = new GameObject(FeedbackRootName);
            root.transform.SetParent(_worldRoot, false);
            _feedbackRoot = root.transform;
            Color accent = ActiveUiTheme.GetFeedbackAccentColor(new Color(0.82f, 0.4f, 1f));
            Color arenaAccent = ActiveUiTheme.GetArenaAccentColor(new Color(0.2f, 0.82f, 1f));
            Color elite = ActiveUiTheme.GetEliteThreatColor(new Color(1f, 0.85f, 0.24f));
            Color boss = ActiveUiTheme.GetBossThreatColor(new Color(1f, 0.3f, 0.82f));
            _spawnPulse = CreateFeedbackPulse(SpawnPulseName, Color.Lerp(boss, arenaAccent, 0.25f), 0.2f, 2.2f, 0.5f);
            _firePulse = CreateFeedbackPulse(FirePulseName, accent, 0.16f, 2.8f, 0.35f);
            _killPulse = CreateFeedbackPulse(KillPulseName, Color.Lerp(arenaAccent, accent, 0.25f), 0.22f, 3.2f, 0.45f);
            _pickupPulse = CreateFeedbackPulse(PickupPulseName, arenaAccent, 0.14f, 2.4f, 0.35f);
            _levelUpPulse = CreateFeedbackPulse(LevelUpPulseName, elite, 0.28f, 2.0f, 0.7f);
            _bossPulse = CreateFeedbackPulse(BossPulseName, boss, 0.32f, 3.6f, 0.65f);

            GameObject audioObject = new GameObject(FeedbackAudioName);
            audioObject.transform.SetParent(_feedbackRoot, false);
            _feedbackAudio = audioObject.AddComponent<AudioSource>();
            _feedbackAudio.playOnAwake = false;
            _feedbackAudio.spatialBlend = 0f;
            _feedbackAudio.volume = 0.28f;
            _spawnClip = CreateTone("survivors-spawn", 170f, 0.12f, 0.16f);
            _fireClip = CreateTone("survivors-fire", 540f, 0.07f, 0.13f);
            _killClip = CreateTone("survivors-kill", 760f, 0.1f, 0.18f);
            _pickupClip = CreateTone("survivors-pickup", 1040f, 0.07f, 0.16f);
            _levelUpClip = CreateTone("survivors-level-up", 880f, 0.2f, 0.2f);
            _bossClip = CreateTone("survivors-boss", 92f, 0.28f, 0.24f);
            _dangerClip = CreateTone("survivors-danger", 130f, 0.16f, 0.2f);
        }

        private void ApplyWorldPresentation()
        {
            if (_worldRoot == null)
            {
                return;
            }

            SetRendererColor(_playerRenderer, ActiveUiTheme.GetPlayerColor(new Color(0.78f, 0.91f, 1f)));
            SetPrefabColor(_experiencePickupPrefab, ActiveUiTheme.GetExperiencePickupColor(new Color(0.22f, 0.83f, 1f)));
            SetPrefabColor(_magnetPickupPrefab, ActiveUiTheme.GetArenaAccentColor(new Color(1f, 0.86f, 0.18f)));
            SetPrefabColor(_healthPickupPrefab, ActiveUiTheme.GetHealthPickupColor(new Color(1f, 0.24f, 0.42f)));
            SetPrefabColor(_bloodShardPickupPrefab, ActiveUiTheme.GetCurrencyPickupColor(new Color(0.96f, 0.08f, 0.18f)));
            SetPrefabColor(_projectilePrefab, ResolveProjectileFallbackColor());

            TrailRenderer projectileTrail = _projectilePrefab == null ? null : _projectilePrefab.GetComponent<TrailRenderer>();
            if (projectileTrail != null)
            {
                projectileTrail.startColor = WithAlpha(ActiveUiTheme.GetFeedbackAccentColor(new Color(0.86f, 0.48f, 1f)), 0.95f);
                projectileTrail.endColor = ResolveProjectileTrailEndColor();
            }

            for (int i = 0; i < _arenaTiles.Count; i++)
            {
                Color color = (i & 1) == 0
                    ? ActiveUiTheme.GetArenaFloorColor(new Color(0.045f, 0.055f, 0.058f))
                    : ActiveUiTheme.GetArenaGridColor(new Color(0.055f, 0.065f, 0.05f));
                SetTransformColor(_arenaTiles[i], color);
            }

            for (int i = 0; i < _arenaLandmarks.Count; i++)
            {
                SetTransformColor(_arenaLandmarks[i], ResolveArenaLandmarkColor(i));
            }

            if (_waystoneCompassRoot != null)
            {
                Renderer[] compassRenderers = _waystoneCompassRoot.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < compassRenderers.Length; i++)
                {
                    Renderer renderer = compassRenderers[i];
                    Color color = renderer != null && renderer.gameObject.name.IndexOf("Head", StringComparison.Ordinal) >= 0
                        ? ActiveUiTheme.GetFeedbackAccentColor(new Color(0.92f, 1f, 0.42f))
                        : renderer != null && renderer.gameObject.name.IndexOf("Pulse", StringComparison.Ordinal) >= 0
                            ? ActiveUiTheme.GetArenaGridColor(new Color(0.12f, 0.58f, 0.72f))
                            : ActiveUiTheme.GetArenaAccentColor(new Color(0.32f, 0.94f, 1f));
                    SetRendererColor(renderer, color);
                }
            }

            Color accent = ActiveUiTheme.GetFeedbackAccentColor(new Color(0.82f, 0.4f, 1f));
            Color arenaAccent = ActiveUiTheme.GetArenaAccentColor(new Color(0.2f, 0.82f, 1f));
            Color elite = ActiveUiTheme.GetEliteThreatColor(new Color(1f, 0.85f, 0.24f));
            Color boss = ActiveUiTheme.GetBossThreatColor(new Color(1f, 0.3f, 0.82f));
            SetParticleColor(_spawnPulse, Color.Lerp(boss, arenaAccent, 0.25f));
            SetParticleColor(_firePulse, accent);
            SetParticleColor(_killPulse, Color.Lerp(arenaAccent, accent, 0.25f));
            SetParticleColor(_pickupPulse, arenaAccent);
            SetParticleColor(_levelUpPulse, elite);
            SetParticleColor(_bossPulse, boss);
        }

        internal Color ResolveProjectileFallbackColor()
        {
            return ActiveUiTheme.GetFeedbackAccentColor(new Color(0.78f, 0.42f, 1f));
        }

        internal Color ResolveProjectileTrailEndColor()
        {
            return WithAlpha(ActiveUiTheme.GetArenaAccentColor(new Color(0.18f, 0.86f, 1f)), 0f);
        }

        private static void SetPrefabColor(GameObject prefab, Color color)
        {
            SetRendererColor(prefab == null ? null : prefab.GetComponentInChildren<Renderer>(), color);
        }

        private static void SetTransformColor(Transform target, Color color)
        {
            SetRendererColor(target == null ? null : target.GetComponentInChildren<Renderer>(), color);
        }

        private static void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                renderer.sharedMaterial.color = color;
            }
        }

        private static void SetParticleColor(ParticleSystem particles, Color color)
        {
            if (particles == null)
            {
                return;
            }

            ParticleSystem.MainModule main = particles.main;
            main.startColor = color;
        }

        private ParticleSystem CreateFeedbackPulse(string name, Color color, float startSize, float startSpeed, float lifetime)
        {
            GameObject instance = new GameObject(name);
            instance.transform.SetParent(_feedbackRoot, false);
            ParticleSystem particles = instance.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = color;
            main.maxParticles = 120;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;
            return particles;
        }

        private void PlayFeedback(ParticleSystem particles, Vector3 position, int count, AudioClip clip, string audioEventId = null, float audioThrottleSeconds = 0f)
        {
            if (particles != null)
            {
                particles.transform.position = position + Vector3.up * 0.28f;
                particles.Emit(Mathf.Max(1, count));
            }

            if (!string.IsNullOrWhiteSpace(audioEventId))
            {
                PlayAudioEvent(audioEventId, clip, audioThrottleSeconds);
                return;
            }

            if (!_audioMuted && _feedbackAudio != null && clip != null)
            {
                _feedbackAudio.PlayOneShot(clip);
            }
        }

        private bool PlayAudioEvent(string eventId, AudioClip fallbackClip, float fallbackThrottleSeconds)
        {
            if (string.IsNullOrWhiteSpace(eventId) || _audioMuted)
            {
                return false;
            }

            EnsureUiTheme();
            string normalized = eventId.Trim();
            float now = Time.unscaledTime;
            float throttleSeconds = ActiveUiTheme.GetAudioEventThrottleSeconds(normalized, fallbackThrottleSeconds);
            if (throttleSeconds > 0f &&
                _audioEventNextAllowedTime.TryGetValue(normalized, out float nextAllowed) &&
                now < nextAllowed)
            {
                return false;
            }

            if (throttleSeconds > 0f)
            {
                _audioEventNextAllowedTime[normalized] = now + throttleSeconds;
            }

            _audioEventDispatchCount++;
            _lastAudioEventId = normalized;
            if (_feedbackAudio != null && fallbackClip != null)
            {
                float oldVolume = _feedbackAudio.volume;
                _feedbackAudio.volume = ActiveUiTheme.GetAudioEventVolume(normalized, oldVolume);
                _feedbackAudio.PlayOneShot(fallbackClip);
                _feedbackAudio.volume = oldVolume;
            }

            return true;
        }

        private void RecordEnemyDeathEffect(Vector3 position, SurvivorsEnemyRole role, float radius)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_worldFeedbackEffects.Count >= EnemyDeathEffectLimit)
            {
                ReleaseWorldFeedbackEffect(0);
            }

            Color color = ResolveEnemyDeathEffectColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            instance.name = "Survivors Enemy Death Burst";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = position + Vector3.up * 0.16f;
            Vector3 baseScale = Vector3.one * Mathf.Max(0.35f, radius * 1.7f);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                ReleaseTemplateObject(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            _worldFeedbackEffects.Add(new SurvivorsWorldFeedbackEffect(instance, renderer, material, color, baseScale));
            EnemyDeathEffectCount++;
        }

        private void TickWorldFeedbackEffects(float deltaTime)
        {
            if (_worldFeedbackEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _worldFeedbackEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsWorldFeedbackEffect effect = _worldFeedbackEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= EnemyDeathEffectLifetimeSeconds)
                {
                    ReleaseWorldFeedbackEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / EnemyDeathEffectLifetimeSeconds);
                float scale = Mathf.Lerp(1f, 2.65f, normalizedAge);
                effect.Instance.transform.localScale = effect.BaseScale * scale;
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.72f, 0.04f, normalizedAge);
                    effect.Material.color = color;
                }

                _worldFeedbackEffects[i] = effect;
            }
        }

        private void ReleaseWorldFeedbackEffect(int index)
        {
            if (index < 0 || index >= _worldFeedbackEffects.Count)
            {
                return;
            }

            SurvivorsWorldFeedbackEffect effect = _worldFeedbackEffects[index];
            _worldFeedbackEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                ReleaseTemplateObject(effect.Material);
            }

            if (effect.Instance != null)
            {
                ReleaseTemplateObject(effect.Instance);
            }
        }

        internal void RecordEnemyRangedAttackFeedback(Vector3 origin, Vector3 target, SurvivorsEnemyRole role)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            Vector3 start = origin + Vector3.up * 0.42f;
            Vector3 end = target + Vector3.up * 0.36f;
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            if (distance <= 0.05f)
            {
                return;
            }

            while (_enemyRangedAttackFeedbackEffects.Count >= EnemyRangedAttackFeedbackLimit)
            {
                ReleaseEnemyRangedAttackFeedbackEffect(0);
            }

            Color color = ResolveEnemyRangedAttackColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "Survivors Enemy Ranged Attack Cue";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = (start + end) * 0.5f;
            instance.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Vector3 baseScale = new Vector3(0.1f, 0.1f, distance);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                ReleaseTemplateObject(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            _enemyRangedAttackFeedbackEffects.Add(new SurvivorsEnemyRangedAttackFeedbackEffect(instance, material, color, baseScale));
            EnemyRangedAttackFeedbackCount++;
        }

        internal void RecordEnemyRangedAttackDodgeFeedback(SurvivorsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return;
            }

            int experienceReward = Mathf.Max(0, CurrentTuning.EnemyRangedAttackDodgeExperienceReward);
            bool spawnedExperience = false;
            if (experienceReward > 0)
            {
                Vector3 rewardPosition = ResolveRangedDodgeRewardPosition(enemy);
                spawnedExperience = SpawnPickup(SurvivorsPickupKind.Experience, rewardPosition, experienceReward) != null;
                if (spawnedExperience)
                {
                    EnemyRangedAttackDodgeExperienceGemDropCount++;
                }
            }

            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? ResolveRangedDodgeFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastEnemyRangedAttackDodgeFeedbackLabel = spawnedExperience
                ? $"{name} shot dodged: +{experienceReward} XP"
                : $"{name} shot dodged";
            EnemyRangedAttackDodgeFeedbackCount++;
            RecordStreakRewardFeedback(LastEnemyRangedAttackDodgeFeedbackLabel, new Color(0.52f, 0.95f, 1f));
            PlayFeedback(_pickupPulse, PlayerPosition, spawnedExperience ? 14 : 8, _pickupClip);
        }

        private Vector3 ResolveRangedDodgeRewardPosition(SurvivorsEnemyActor enemy)
        {
            Vector3 player = PlayerPosition;
            Vector3 away = enemy == null ? Vector3.forward : player - enemy.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude <= 0.001f)
            {
                away = Vector3.forward;
            }

            float offset = Mathf.Max(CurrentTuning.PickupCollectRadius + 0.35f, 0.75f);
            return player + away.normalized * offset;
        }

        private static string ResolveRangedDodgeFallbackLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Spitter:
                    return "Spitter";
                case SurvivorsEnemyRole.Elite:
                case SurvivorsEnemyRole.DreadElite:
                    return "Elite";
                case SurvivorsEnemyRole.Miniboss:
                    return "Miniboss";
                case SurvivorsEnemyRole.Boss:
                    return "Boss";
                default:
                    return "Ranged shot";
            }
        }

        private void TickEnemyRangedAttackFeedbackEffects(float deltaTime)
        {
            if (_enemyRangedAttackFeedbackEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _enemyRangedAttackFeedbackEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsEnemyRangedAttackFeedbackEffect effect = _enemyRangedAttackFeedbackEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= EnemyRangedAttackFeedbackLifetimeSeconds)
                {
                    ReleaseEnemyRangedAttackFeedbackEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / EnemyRangedAttackFeedbackLifetimeSeconds);
                float width = Mathf.Lerp(1.35f, 0.25f, normalizedAge);
                effect.Instance.transform.localScale = new Vector3(effect.BaseScale.x * width, effect.BaseScale.y * width, effect.BaseScale.z);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.86f, 0.05f, normalizedAge);
                    effect.Material.color = color;
                }

                _enemyRangedAttackFeedbackEffects[i] = effect;
            }
        }

        private void ReleaseEnemyRangedAttackFeedbackEffect(int index)
        {
            if (index < 0 || index >= _enemyRangedAttackFeedbackEffects.Count)
            {
                return;
            }

            SurvivorsEnemyRangedAttackFeedbackEffect effect = _enemyRangedAttackFeedbackEffects[index];
            _enemyRangedAttackFeedbackEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                ReleaseTemplateObject(effect.Material);
            }

            if (effect.Instance != null)
            {
                ReleaseTemplateObject(effect.Instance);
            }
        }

        private static Color ResolveEnemyRangedAttackColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.18f, 0.52f, 0.86f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.42f, 0.86f, 1f, 0.82f);
                case SurvivorsEnemyRole.Miniboss:
                case SurvivorsEnemyRole.Elite:
                    return new Color(1f, 0.56f, 0.2f, 0.84f);
                default:
                    return new Color(0.54f, 1f, 0.32f, 0.78f);
            }
        }

        private void RecordMajorThreatSlamTelegraphEffect(Vector3 position, SurvivorsEnemyRole role, float radius, float durationSeconds)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_majorThreatSlamTelegraphEffects.Count >= MajorThreatSlamTelegraphEffectLimit)
            {
                ReleaseMajorThreatSlamTelegraphEffect(0);
            }

            Color color = ResolveMajorThreatSlamTelegraphColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            instance.name = "Survivors Major Threat Slam Telegraph";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = new Vector3(position.x, 0.115f, position.z);
            float diameter = Mathf.Max(0.4f, radius * 2f);
            Vector3 baseScale = new Vector3(diameter, 0.028f, diameter);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                ReleaseTemplateObject(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            float duration = Mathf.Max(0.08f, durationSeconds) + MajorThreatSlamTelegraphFadePaddingSeconds;
            _majorThreatSlamTelegraphEffects.Add(new SurvivorsMajorThreatSlamTelegraphEffect(instance, material, color, baseScale, duration));
            MajorThreatSlamTelegraphEffectCount++;
        }

        private void TickMajorThreatSlamTelegraphEffects(float deltaTime)
        {
            if (_majorThreatSlamTelegraphEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _majorThreatSlamTelegraphEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsMajorThreatSlamTelegraphEffect effect = _majorThreatSlamTelegraphEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= effect.DurationSeconds)
                {
                    ReleaseMajorThreatSlamTelegraphEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / effect.DurationSeconds);
                float charge = Mathf.Lerp(0.58f, 1.05f, normalizedAge);
                float pulse = 1f + Mathf.Sin(effect.ElapsedSeconds * 26f) * 0.06f;
                effect.Instance.transform.localScale = effect.BaseScale * Mathf.Max(0.45f, charge * pulse);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.82f, 0.22f, normalizedAge);
                    effect.Material.color = color;
                }

                _majorThreatSlamTelegraphEffects[i] = effect;
            }
        }

        private void ReleaseMajorThreatSlamTelegraphEffect(int index)
        {
            if (index < 0 || index >= _majorThreatSlamTelegraphEffects.Count)
            {
                return;
            }

            SurvivorsMajorThreatSlamTelegraphEffect effect = _majorThreatSlamTelegraphEffects[index];
            _majorThreatSlamTelegraphEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                ReleaseTemplateObject(effect.Material);
            }

            if (effect.Instance != null)
            {
                ReleaseTemplateObject(effect.Instance);
            }
        }

        private void RecordIncomingThreatTelegraph(Vector3 position, SurvivorsEnemyRole role, string label, float radius, float durationSeconds)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_incomingThreatTelegraphEffects.Count >= IncomingThreatTelegraphEffectLimit)
            {
                ReleaseIncomingThreatTelegraphEffect(0);
            }

            Color color = ResolveIncomingThreatTelegraphColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            instance.name = "Survivors Incoming Threat Telegraph";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = new Vector3(position.x, 0.095f, position.z);
            float diameter = Mathf.Max(1.2f, radius * 2f);
            Vector3 baseScale = new Vector3(diameter, 0.024f, diameter);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                ReleaseTemplateObject(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            float duration = Mathf.Max(0.12f, durationSeconds) + IncomingThreatTelegraphFadePaddingSeconds;
            _incomingThreatTelegraphEffects.Add(new SurvivorsIncomingThreatTelegraphEffect(instance, material, color, baseScale, duration));
            IncomingThreatTelegraphEffectCount++;
            LastIncomingThreatTelegraphLabel = string.IsNullOrWhiteSpace(label) ? ResolveMajorThreatWarningLabel(role) : label;
        }

        private void TickIncomingThreatTelegraphEffects(float deltaTime)
        {
            if (_incomingThreatTelegraphEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _incomingThreatTelegraphEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsIncomingThreatTelegraphEffect effect = _incomingThreatTelegraphEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= effect.DurationSeconds)
                {
                    ReleaseIncomingThreatTelegraphEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / effect.DurationSeconds);
                float pulse = 1f + Mathf.Sin(effect.ElapsedSeconds * 18f) * 0.08f;
                float charge = Mathf.Lerp(0.88f, 1.16f, normalizedAge);
                effect.Instance.transform.localScale = effect.BaseScale * Mathf.Max(0.42f, charge * pulse);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.66f, 0.08f, normalizedAge);
                    effect.Material.color = color;
                }

                _incomingThreatTelegraphEffects[i] = effect;
            }
        }

        private void ReleaseIncomingThreatTelegraphEffect(int index)
        {
            if (index < 0 || index >= _incomingThreatTelegraphEffects.Count)
            {
                return;
            }

            SurvivorsIncomingThreatTelegraphEffect effect = _incomingThreatTelegraphEffects[index];
            _incomingThreatTelegraphEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                ReleaseTemplateObject(effect.Material);
            }

            if (effect.Instance != null)
            {
                ReleaseTemplateObject(effect.Instance);
            }
        }

        private static Color ResolveIncomingThreatTelegraphColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.12f, 0.48f, 0.66f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(1f, 0.36f, 0.12f, 0.64f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.38f, 0.86f, 1f, 0.62f);
                default:
                    return new Color(1f, 0.72f, 0.16f, 0.6f);
            }
        }

        private static float ResolveIncomingThreatTelegraphRadius(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return 9.5f;
                case SurvivorsEnemyRole.Miniboss:
                    return 8f;
                case SurvivorsEnemyRole.DreadElite:
                    return 6.8f;
                default:
                    return 5.8f;
            }
        }

        private static Color ResolveMajorThreatSlamTelegraphColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.1f, 0.5f, 0.82f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(1f, 0.38f, 0.14f, 0.8f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.42f, 0.88f, 1f, 0.78f);
                default:
                    return new Color(1f, 0.62f, 0.18f, 0.76f);
            }
        }

        private void RecordMajorRewardDropFeedback(Vector3 position, SurvivorsEnemyRole role, float radius)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_rewardDropFeedbackEffects.Count >= MajorRewardDropEffectLimit)
            {
                ReleaseMajorRewardDropFeedbackEffect(0);
            }

            string label = ResolveMajorRewardDropLabel(role);
            Color color = ResolveMajorRewardDropColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "Survivors " + label;
            instance.transform.SetParent(_feedbackRoot, false);
            Vector3 basePosition = position + Vector3.up * Mathf.Max(0.56f, radius * 1.1f);
            instance.transform.position = basePosition;
            instance.transform.rotation = Quaternion.Euler(12f, 45f, 18f);
            float size = role == SurvivorsEnemyRole.Boss
                ? Mathf.Max(0.9f, radius * 0.95f)
                : Mathf.Max(0.56f, radius * 0.82f);
            Vector3 baseScale = new Vector3(size, size, size);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                ReleaseTemplateObject(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            _rewardDropFeedbackEffects.Add(new SurvivorsRewardDropFeedbackEffect(instance, material, color, basePosition, baseScale));
            MajorRewardDropFeedbackCount++;
            LastMajorRewardDropFeedbackLabel = label;
        }

        private void TickMajorRewardDropFeedbackEffects(float deltaTime)
        {
            if (_rewardDropFeedbackEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _rewardDropFeedbackEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsRewardDropFeedbackEffect effect = _rewardDropFeedbackEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= MajorRewardDropLifetimeSeconds)
                {
                    ReleaseMajorRewardDropFeedbackEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / MajorRewardDropLifetimeSeconds);
                float pulse = Mathf.Sin(normalizedAge * Mathf.PI);
                effect.Instance.transform.position = effect.BasePosition + Vector3.up * (0.58f * pulse);
                effect.Instance.transform.localScale = effect.BaseScale * Mathf.Lerp(1f, 1.48f, pulse);
                effect.Instance.transform.Rotate(0f, 260f * dt, 0f, Space.World);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.95f, 0.1f, normalizedAge);
                    effect.Material.color = color;
                }

                _rewardDropFeedbackEffects[i] = effect;
            }
        }

        private void ReleaseMajorRewardDropFeedbackEffect(int index)
        {
            if (index < 0 || index >= _rewardDropFeedbackEffects.Count)
            {
                return;
            }

            SurvivorsRewardDropFeedbackEffect effect = _rewardDropFeedbackEffects[index];
            _rewardDropFeedbackEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                ReleaseTemplateObject(effect.Material);
            }

            if (effect.Instance != null)
            {
                ReleaseTemplateObject(effect.Instance);
            }
        }

        private Color ResolveMajorRewardDropColor(SurvivorsEnemyRole role)
        {
            Color elite = ActiveUiTheme.GetEliteThreatColor(new Color(1f, 0.76f, 0.2f));
            Color boss = ActiveUiTheme.GetBossThreatColor(new Color(1f, 0.2f, 0.45f));
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return WithAlpha(boss, 0.95f);
                case SurvivorsEnemyRole.Miniboss:
                    return WithAlpha(Color.Lerp(elite, boss, 0.62f), 0.95f);
                case SurvivorsEnemyRole.DreadElite:
                    return WithAlpha(Color.Lerp(elite, ActiveUiTheme.GetArenaAccentColor(new Color(0.38f, 0.9f, 1f)), 0.58f), 0.92f);
                default:
                    return WithAlpha(elite, 0.92f);
            }
        }

        private static string ResolveMajorRewardDropLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return "Boss Reward Cache";
                case SurvivorsEnemyRole.Miniboss:
                    return "Miniboss Reward Cache";
                case SurvivorsEnemyRole.DreadElite:
                    return "Dread Elite Reward Cache";
                default:
                    return "Elite Reward Cache";
            }
        }

        private static Color ResolveEnemyDeathEffectColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Elite:
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(1f, 0.62f, 0.18f, 0.72f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(1f, 0.3f, 0.74f, 0.72f);
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.15f, 0.22f, 0.76f);
                default:
                    return new Color(0.35f, 0.95f, 1f, 0.68f);
            }
        }

        private static AudioClip CreateTone(string name, float frequency, float durationSeconds, float volume)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * durationSeconds));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float fade = Mathf.Clamp01(1f - i / (float)sampleCount);
                samples[i] = Mathf.Sin(Mathf.PI * 2f * frequency * t) * volume * fade;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void EnsureCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }
            else if (_camera.GetComponent<AudioListener>() == null)
            {
                _camera.gameObject.AddComponent<AudioListener>();
            }

            _camera.orthographic = true;
            _camera.orthographicSize = 8.2f;
            _camera.transform.position = PlayerPosition + new Vector3(0f, 13.5f, -9.5f);
            _camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }

        private void ClearRun()
        {
            _runStarted = false;
            if (_weaponLoadout != null)
            {
                _weaponLoadout.Dispose();
                _weaponLoadout = null;
            }

            _runFlow = null;
            _enemies.Clear();
            _activeHordeRushEnemies.Clear();
            _activeRoamingCacheAmbushEnemies.Clear();
            _activeArenaShrineEnemies.Clear();
            _enragedMajorThreats.Clear();
            _pickups.Clear();
            _projectiles.Clear();
            while (_worldFeedbackEffects.Count > 0)
            {
                ReleaseWorldFeedbackEffect(_worldFeedbackEffects.Count - 1);
            }

            while (_enemyRangedAttackFeedbackEffects.Count > 0)
            {
                ReleaseEnemyRangedAttackFeedbackEffect(_enemyRangedAttackFeedbackEffects.Count - 1);
            }

            while (_majorThreatSlamTelegraphEffects.Count > 0)
            {
                ReleaseMajorThreatSlamTelegraphEffect(_majorThreatSlamTelegraphEffects.Count - 1);
            }

            while (_incomingThreatTelegraphEffects.Count > 0)
            {
                ReleaseIncomingThreatTelegraphEffect(_incomingThreatTelegraphEffects.Count - 1);
            }

            while (_rewardDropFeedbackEffects.Count > 0)
            {
                ReleaseMajorRewardDropFeedbackEffect(_rewardDropFeedbackEffects.Count - 1);
            }

            _arenaTiles.Clear();
            _arenaLandmarks.Clear();
            _arenaLandmarkKeys.Clear();
            _discoveredArenaWaystones.Clear();
            _ownedPassiveUpgradeIds.Clear();
            _ownedEvolutionUpgradeIds.Clear();
            _arenaTileRoot = null;
            _arenaFollowRoot = null;
            _waystoneCompassRoot = null;
            if (_spawnService != null)
            {
                _spawnService.Dispose();
                _spawnService = null;
            }

            if (_worldRoot != null)
            {
                ReleaseTemplateObject(_worldRoot.gameObject);
                _worldRoot = null;
            }
        }

        private void EnsureMetaProgressionLoaded()
        {
            if (_metaProgression == null)
            {
                IPersistenceService persistence = _injectedMetaPersistence ??
                    new PersistenceService(new FileTextStorage(new UnityPersistentDataPathProvider()));
                _ownsMetaProgressionService = _injectedMetaPersistence == null;
                _metaProgression = new SurvivorsMetaProgressionService(
                    persistence,
                    _metaSaveSlotId,
                    ResolveMetaProgressionDefinition());
                _metaProfileLoaded = false;
            }

            if (!_metaProfileLoaded)
            {
                _metaProgression.Load();
                _metaProfileLoaded = true;
            }
        }

        private SurvivorsMetaProgressionDefinition ResolveMetaProgressionDefinition()
        {
            return _authoredContent != null && _authoredContent.MetaProgressionDefinition != null
                ? _authoredContent.MetaProgressionDefinition
                : BasicSurvivorsGame.CreateMetaProgressionDefinition();
        }

        private string CurrencyDisplayName => ResolveMetaProgressionDefinition().CurrencyDisplayName;

        private string CurrencyRewardLabel => string.Equals(CurrencyDisplayName, "Blood Shards", StringComparison.Ordinal)
            ? "shards"
            : CurrencyDisplayName;

        private string ProgressionDisplayName => ResolveMetaProgressionDefinition().LegacyExperienceDisplayName;

        private string ProgressionRewardLabel => string.Equals(ProgressionDisplayName, "Legacy XP", StringComparison.Ordinal)
            ? "XP"
            : ProgressionDisplayName;

        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateWeaponArchetypeDefinitions(SurvivorsTemplateTuning resolved)
        {
            return _authoredContent != null && _authoredContent.HasWeaponDefinitions
                ? _authoredContent.WeaponDefinitions
                : BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(resolved);
        }

        private IReadOnlyList<SurvivorsRelicDefinition> CreateRelicDefinitions()
        {
            return _authoredContent != null && _authoredContent.HasRelicDefinitions
                ? _authoredContent.RelicDefinitions
                : BasicSurvivorsGame.CreateRelicDefinitions();
        }

        private SurvivorsClassLibraryDefinition CreateClassLibraryDefinition()
        {
            return _authoredContent != null && _authoredContent.HasClassLibrary
                ? _authoredContent.ClassLibrary
                : BasicSurvivorsGame.CreateClassLibraryDefinition();
        }

        private IReadOnlyList<SurvivorsClassUpgradeGateDefinition> CreateClassUpgradeGates()
        {
            return _authoredContent != null && _authoredContent.HasClassUpgradeGates
                ? _authoredContent.ClassUpgradeGates
                : BasicSurvivorsGame.CreateClassUpgradeGates();
        }

        private RunUpgradeCatalog CreateBaseRunUpgradeCatalog()
        {
            return _authoredContent != null && _authoredContent.HasRunUpgradeCatalog
                ? _authoredContent.RunUpgradeCatalog
                : BasicSurvivorsGame.CreateRunUpgradeCatalog();
        }

        private IReadOnlyList<SurvivorsRunUpgradeMetadata> CreateRunUpgradeMetadata()
        {
            return _authoredContent != null && _authoredContent.HasRunUpgradeMetadata
                ? _authoredContent.RunUpgradeMetadata
                : BasicSurvivorsGame.CreateRunUpgradeMetadata();
        }

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

        private RunUpgradeCatalog CreateRunUpgradeCatalogForSelectedClass()
        {
            RunUpgradeCatalog fullCatalog = CreateBaseRunUpgradeCatalog();
            var definitions = new List<RunUpgradeDefinition>(fullCatalog.Definitions.Count);
            for (int i = 0; i < fullCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = fullCatalog.Definitions[i];
                if (definition != null && IsUpgradeAllowedForSelectedClass(definition.Id.Value))
                {
                    definitions.Add(definition);
                }
            }

            return definitions.Count == 0 ? fullCatalog : new RunUpgradeCatalog(definitions);
        }

        private void BuildUpgradeMetadataIndex()
        {
            _upgradeMetadataById.Clear();
            IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata = CreateRunUpgradeMetadata();
            for (int i = 0; i < metadata.Count; i++)
            {
                SurvivorsRunUpgradeMetadata entry = metadata[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.UpgradeId) || _upgradeMetadataById.ContainsKey(entry.UpgradeId))
                {
                    continue;
                }

                _upgradeMetadataById.Add(entry.UpgradeId, entry);
            }
        }

        private RunUpgradeCatalog CreateEligibleDraftCatalog()
        {
            return CreateEligibleDraftCatalog(ResolveNormalDraftRarityProfile());
        }

        private RunUpgradeCatalog CreateEligibleDraftCatalog(DraftRarityProfile profile)
        {
            return CreateEligibleDraftCatalog(profile, RunUpgradeRarity.Common, requireMinimumRarity: false);
        }

        private RunUpgradeCatalog CreateEligibleDraftCatalog(
            DraftRarityProfile profile,
            RunUpgradeRarity minimumRarity,
            bool requireMinimumRarity)
        {
            if (_upgradeCatalog == null)
            {
                return null;
            }

            var eligible = new List<RunUpgradeDefinition>(_upgradeCatalog.Definitions.Count);
            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition != null &&
                    (!requireMinimumRarity || definition.Rarity >= minimumRarity) &&
                    IsUpgradeEligibleForCurrentBuild(definition))
                {
                    eligible.Add(definition);
                }
            }

            return CreateWeightedDraftCatalog(eligible, profile);
        }

        private RunUpgradeCatalog CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole role, bool requireEvolutionChoice)
        {
            if (_upgradeCatalog == null)
            {
                return null;
            }

            RunUpgradeRarity minimumRarity = role == SurvivorsEnemyRole.Boss ? RunUpgradeRarity.Rare : RunUpgradeRarity.Uncommon;
            var allEligible = new List<RunUpgradeDefinition>(_upgradeCatalog.Definitions.Count);
            var preferred = new List<RunUpgradeDefinition>(_upgradeCatalog.Definitions.Count);
            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition == null || !IsUpgradeEligibleForCurrentBuild(definition))
                {
                    continue;
                }

                allEligible.Add(definition);
                if (definition.Rarity >= minimumRarity || IsEvolutionUpgrade(definition))
                {
                    preferred.Add(definition);
                }
            }

            if (requireEvolutionChoice && CountEvolutionChoices(preferred) <= 0)
            {
                return null;
            }

            List<RunUpgradeDefinition> selected = preferred.Count >= CurrentTuning.DraftChoiceCount ? preferred : allEligible;
            DraftRarityProfile profile = role == SurvivorsEnemyRole.Boss ? DraftRarityProfile.Boss : DraftRarityProfile.Elite;
            return CreateWeightedDraftCatalog(selected, profile);
        }

        private DraftRarityProfile ResolveNormalDraftRarityProfile()
        {
            if (Level >= Mathf.Max(1, CurrentTuning.DraftLateRarityLevel))
            {
                return DraftRarityProfile.NormalLate;
            }

            if (Level >= Mathf.Max(1, CurrentTuning.DraftMidRarityLevel))
            {
                return DraftRarityProfile.NormalMid;
            }

            return DraftRarityProfile.NormalEarly;
        }

        private RunUpgradeCatalog CreateWeightedDraftCatalog(IReadOnlyList<RunUpgradeDefinition> definitions, DraftRarityProfile profile)
        {
            if (definitions == null || definitions.Count == 0)
            {
                return null;
            }

            var weighted = new List<RunUpgradeDefinition>(definitions.Count);
            for (int i = 0; i < definitions.Count; i++)
            {
                RunUpgradeDefinition definition = definitions[i];
                if (definition == null)
                {
                    continue;
                }

                int rarityWeight = ResolveDraftRarityWeight(profile, definition.Rarity);
                if (rarityWeight <= 0)
                {
                    continue;
                }

                weighted.Add(CloneUpgradeWithDraftWeight(definition, ResolveWeightedDraftWeight(definition.Weight, rarityWeight)));
            }

            return weighted.Count == 0 ? null : new RunUpgradeCatalog(weighted);
        }

        private int ResolveWeightedDraftWeight(int baseWeight, int rarityWeight)
        {
            long resolved = (long)Mathf.Max(1, baseWeight) * Mathf.Max(0, rarityWeight);
            resolved = Mathf.Max(1, Mathf.RoundToInt(resolved / 100f));
            return resolved > int.MaxValue ? int.MaxValue : (int)resolved;
        }

        private RunUpgradeDefinition CloneUpgradeWithDraftWeight(RunUpgradeDefinition definition, int weight)
        {
            return new RunUpgradeDefinition(
                definition.Id,
                definition.Rarity,
                Mathf.Max(1, weight),
                definition.MaxRank,
                definition.Effects,
                definition.Prerequisites,
                definition.Exclusions);
        }

        private int ResolveDraftRarityWeight(DraftRarityProfile profile, RunUpgradeRarity rarity)
        {
            return ApplyDraftLuckToRarityWeight(ResolveBaseDraftRarityWeight(profile, rarity), rarity);
        }

        private int ResolveBaseDraftRarityWeight(DraftRarityProfile profile, RunUpgradeRarity rarity)
        {
            switch (profile)
            {
                case DraftRarityProfile.NormalEarly:
                    return ResolveNormalEarlyRarityWeight(rarity);
                case DraftRarityProfile.NormalMid:
                    return ResolveNormalMidRarityWeight(rarity);
                case DraftRarityProfile.NormalLate:
                    return ResolveNormalLateRarityWeight(rarity);
                case DraftRarityProfile.Elite:
                    return ResolveEliteRarityWeight(rarity);
                case DraftRarityProfile.Boss:
                    return ResolveBossRarityWeight(rarity);
                default:
                    return 100;
            }
        }

        private int ApplyDraftLuckToRarityWeight(int baseWeight, RunUpgradeRarity rarity)
        {
            if (baseWeight <= 0 || DraftLuckBonus <= 0f)
            {
                return baseWeight;
            }

            float luck = Mathf.Max(0f, DraftLuckBonus);
            float multiplier;
            switch (rarity)
            {
                case RunUpgradeRarity.Common:
                    multiplier = Mathf.Max(0.35f, 1f - luck * 0.35f);
                    break;
                case RunUpgradeRarity.Uncommon:
                    multiplier = 1f + luck * 0.15f;
                    break;
                case RunUpgradeRarity.Rare:
                    multiplier = 1f + luck;
                    break;
                case RunUpgradeRarity.Epic:
                    multiplier = 1f + luck * 1.6f;
                    break;
                case RunUpgradeRarity.Legendary:
                    multiplier = 1f + luck * 2.2f;
                    break;
                default:
                    multiplier = 1f;
                    break;
            }

            return Mathf.Max(1, Mathf.RoundToInt(baseWeight * multiplier));
        }

        private int ResolveNormalEarlyRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.NormalEarlyCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.NormalEarlyUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.NormalEarlyRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.NormalEarlyEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.NormalEarlyLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveNormalMidRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.NormalMidCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.NormalMidUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.NormalMidRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.NormalMidEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.NormalMidLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveNormalLateRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.NormalLateCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.NormalLateUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.NormalLateRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.NormalLateEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.NormalLateLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveEliteRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.EliteCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.EliteUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.EliteRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.EliteEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.EliteLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveBossRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.BossCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.BossUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.BossRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.BossEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.BossLegendaryWeight;
                default: return 0;
            }
        }

        private IReadOnlyList<RunUpgradeId> CreateNormalDraftRarityLocks(
            DraftRarityProfile profile,
            int rerollIndex)
        {
            if (!TryResolveNormalDraftGuaranteedMinimumRarity(profile, out RunUpgradeRarity minimumRarity))
            {
                return Array.Empty<RunUpgradeId>();
            }

            RunUpgradeCatalog highRarityCatalog = CreateEligibleDraftCatalog(
                profile,
                minimumRarity,
                requireMinimumRarity: true);
            if (highRarityCatalog == null || highRarityCatalog.Definitions.Count == 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                highRarityCatalog,
                _upgradeState,
                new RunUpgradeDraftRequest(
                    1,
                    ResolveDraftSeed(SurvivorsRewardSelectionKind.LevelUp) + NormalDraftRarityLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            return new[] { lockDraft.Choices[0].Id };
        }

        private IReadOnlyList<RunUpgradeId> CreateNormalDraftChoiceLocks(
            DraftRarityProfile profile,
            int rerollIndex)
        {
            int choiceCount = Mathf.Max(0, CurrentTuning.DraftChoiceCount);
            if (choiceCount <= 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            var locks = new List<RunUpgradeId>(choiceCount);
            IReadOnlyList<RunUpgradeId> rarityLocks = CreateNormalDraftRarityLocks(profile, rerollIndex);
            if (rarityLocks != null)
            {
                for (int i = 0; i < rarityLocks.Count && locks.Count < choiceCount; i++)
                {
                    AddUniqueDraftLock(locks, rarityLocks[i], choiceCount);
                }
            }

            if (TryCreateEvolutionPrimerPassiveChoiceLock(profile, rerollIndex, out RunUpgradeId evolutionPassiveLock))
            {
                AddUniqueDraftLock(locks, evolutionPassiveLock, choiceCount);
            }

            if (TryCreateEarlyPassiveChoiceLock(profile, rerollIndex, out RunUpgradeId passiveLock))
            {
                AddUniqueDraftLock(locks, passiveLock, choiceCount);
            }

            return locks.Count == 0 ? Array.Empty<RunUpgradeId>() : locks.ToArray();
        }

        private bool TryCreateEvolutionPrimerPassiveChoiceLock(
            DraftRarityProfile profile,
            int rerollIndex,
            out RunUpgradeId passiveLock)
        {
            passiveLock = default;
            if (_upgradeCatalog == null ||
                _upgradeState == null ||
                ActivePassiveCount >= MaxPassiveSlots ||
                CurrentTuning.DraftChoiceCount <= 0)
            {
                return false;
            }

            var candidates = new List<RunUpgradeDefinition>(_upgradeCatalog.Definitions.Count);
            var seenPassiveIds = new HashSet<RunUpgradeId>();
            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition evolution = _upgradeCatalog.Definitions[i];
                if (!TryResolveEvolutionMissingPassive(evolution, out RunUpgradeDefinition passive) ||
                    !seenPassiveIds.Add(passive.Id))
                {
                    continue;
                }

                candidates.Add(passive);
            }

            RunUpgradeCatalog passiveCatalog = CreateWeightedDraftCatalog(candidates, profile);
            if (passiveCatalog == null || passiveCatalog.Definitions.Count == 0)
            {
                return false;
            }

            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                passiveCatalog,
                _upgradeState,
                new RunUpgradeDraftRequest(
                    1,
                    ResolveDraftSeed(SurvivorsRewardSelectionKind.LevelUp) + EvolutionPrimerPassiveDraftLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return false;
            }

            passiveLock = lockDraft.Choices[0].Id;
            return true;
        }

        private bool TryCreateEarlyPassiveChoiceLock(
            DraftRarityProfile profile,
            int rerollIndex,
            out RunUpgradeId passiveLock)
        {
            passiveLock = default;
            if (_upgradeCatalog == null ||
                ActivePassiveCount > 0 ||
                ActivePassiveCount >= MaxPassiveSlots ||
                CurrentTuning.DraftChoiceCount <= 0)
            {
                return false;
            }

            var candidates = new List<RunUpgradeDefinition>(_upgradeCatalog.Definitions.Count);
            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition == null ||
                    definition.Rarity > RunUpgradeRarity.Uncommon ||
                    !IsUpgradeEligibleForCurrentBuild(definition) ||
                    !TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata) ||
                    metadata.Category != SurvivorsRunUpgradeCategory.Passive ||
                    !metadata.UsesPassiveSlot ||
                    _upgradeState.GetRank(definition.Id) > 0)
                {
                    continue;
                }

                candidates.Add(definition);
            }

            RunUpgradeCatalog passiveCatalog = CreateWeightedDraftCatalog(candidates, profile);
            if (passiveCatalog == null || passiveCatalog.Definitions.Count == 0)
            {
                return false;
            }

            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                passiveCatalog,
                _upgradeState,
                new RunUpgradeDraftRequest(
                    1,
                    ResolveDraftSeed(SurvivorsRewardSelectionKind.LevelUp) + EarlyPassiveDraftLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return false;
            }

            passiveLock = lockDraft.Choices[0].Id;
            return true;
        }

        private static void AddUniqueDraftLock(List<RunUpgradeId> locks, RunUpgradeId candidate, int maxCount)
        {
            if (locks == null || locks.Count >= maxCount)
            {
                return;
            }

            for (int i = 0; i < locks.Count; i++)
            {
                if (locks[i].Equals(candidate))
                {
                    return;
                }
            }

            locks.Add(candidate);
        }

        private static bool TryResolveNormalDraftGuaranteedMinimumRarity(
            DraftRarityProfile profile,
            out RunUpgradeRarity minimumRarity)
        {
            switch (profile)
            {
                case DraftRarityProfile.NormalMid:
                    minimumRarity = RunUpgradeRarity.Rare;
                    return true;
                case DraftRarityProfile.NormalLate:
                    minimumRarity = RunUpgradeRarity.Epic;
                    return true;
                default:
                    minimumRarity = RunUpgradeRarity.Common;
                    return false;
            }
        }

        private IReadOnlyList<RunUpgradeId> CreateEligibleEvolutionChoiceLocks(int maxCount)
        {
            if (_upgradeCatalog == null || maxCount <= 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            var locks = new List<RunUpgradeId>(Mathf.Min(maxCount, _upgradeCatalog.Definitions.Count));
            for (int i = 0; i < _upgradeCatalog.Definitions.Count && locks.Count < maxCount; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition != null && IsEvolutionUpgrade(definition) && IsUpgradeEligibleForCurrentBuild(definition))
                {
                    locks.Add(definition.Id);
                }
            }

            return locks;
        }

        private IReadOnlyList<RunUpgradeId> CreateRewardDraftChoiceLocks(
            SurvivorsEnemyRole role,
            int maxCount,
            int rerollIndex)
        {
            if (maxCount <= 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            IReadOnlyList<RunUpgradeId> evolutionLocks = CreateEligibleEvolutionChoiceLocks(maxCount);
            if (evolutionLocks != null && evolutionLocks.Count > 0)
            {
                return evolutionLocks;
            }

            return TryCreateRewardRarityChoiceLock(role, rerollIndex, out RunUpgradeId rarityLock)
                ? new[] { rarityLock }
                : Array.Empty<RunUpgradeId>();
        }

        private bool TryCreateRewardRarityChoiceLock(
            SurvivorsEnemyRole role,
            int rerollIndex,
            out RunUpgradeId rarityLock)
        {
            rarityLock = default;
            if (!TryResolveRewardDraftGuaranteedMinimumRarity(role, out RunUpgradeRarity minimumRarity))
            {
                return false;
            }

            DraftRarityProfile profile = role == SurvivorsEnemyRole.Boss ? DraftRarityProfile.Boss : DraftRarityProfile.Elite;
            RunUpgradeCatalog highRarityCatalog = CreateEligibleDraftCatalog(
                profile,
                minimumRarity,
                requireMinimumRarity: true);
            if (highRarityCatalog == null || highRarityCatalog.Definitions.Count == 0)
            {
                return false;
            }

            SurvivorsRewardSelectionKind selectionKind = role == SurvivorsEnemyRole.Boss
                ? SurvivorsRewardSelectionKind.BossUpgrade
                : SurvivorsRewardSelectionKind.EliteUpgrade;
            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                highRarityCatalog,
                _upgradeState,
                new RunUpgradeDraftRequest(
                    1,
                    ResolveDraftSeed(selectionKind) + RewardDraftRarityLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return false;
            }

            rarityLock = lockDraft.Choices[0].Id;
            return true;
        }

        private static bool TryResolveRewardDraftGuaranteedMinimumRarity(
            SurvivorsEnemyRole role,
            out RunUpgradeRarity minimumRarity)
        {
            if (role == SurvivorsEnemyRole.Boss)
            {
                minimumRarity = RunUpgradeRarity.Rare;
                return true;
            }

            minimumRarity = RunUpgradeRarity.Uncommon;
            return role == SurvivorsEnemyRole.Elite ||
                role == SurvivorsEnemyRole.DreadElite ||
                role == SurvivorsEnemyRole.Miniboss;
        }

        private bool IsEvolutionUpgrade(RunUpgradeDefinition definition)
        {
            return definition != null &&
                TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata) &&
                metadata.IsEvolution;
        }

        private bool TryResolveEvolutionMissingPassive(RunUpgradeDefinition evolution, out RunUpgradeDefinition passive)
        {
            passive = null;
            if (evolution == null ||
                _upgradeState == null ||
                !TryGetUpgradeMetadata(evolution.Id.Value, out SurvivorsRunUpgradeMetadata evolutionMetadata) ||
                !evolutionMetadata.IsEvolution ||
                string.IsNullOrWhiteSpace(evolutionMetadata.RequiredUpgradeId) ||
                string.IsNullOrWhiteSpace(evolutionMetadata.RequiredPassiveUpgradeId) ||
                _upgradeState.GetRank(evolution.Id) > 0)
            {
                return false;
            }

            int requiredRank = ResolveRequiredUpgradeRank(evolutionMetadata);
            if (_upgradeState.GetRank(new RunUpgradeId(evolutionMetadata.RequiredUpgradeId)) < requiredRank)
            {
                return false;
            }

            var requiredPassiveId = new RunUpgradeId(evolutionMetadata.RequiredPassiveUpgradeId);
            if (_upgradeState.GetRank(requiredPassiveId) > 0 ||
                !TryGetRunUpgrade(evolutionMetadata.RequiredPassiveUpgradeId, out passive) ||
                !IsUpgradeEligibleForCurrentBuild(passive) ||
                !TryGetUpgradeMetadata(passive.Id.Value, out SurvivorsRunUpgradeMetadata passiveMetadata) ||
                passiveMetadata.Category != SurvivorsRunUpgradeCategory.Passive ||
                !passiveMetadata.UsesPassiveSlot)
            {
                passive = null;
                return false;
            }

            return true;
        }

        private int CountEvolutionChoices(IReadOnlyList<RunUpgradeDefinition> definitions)
        {
            if (definitions == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < definitions.Count; i++)
            {
                if (IsEvolutionUpgrade(definitions[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private bool DraftContainsEvolution(RunUpgradeDraft draft)
        {
            return draft != null && CountEvolutionChoices(draft.Choices) > 0;
        }

        private bool TryGenerateCurrentUpgradeDraft(
            SurvivorsRewardSelectionKind selectionKind,
            int rerollIndex,
            IReadOnlyList<RunUpgradeId> lockedChoices,
            out RunUpgradeDraft draft)
        {
            draft = null;
            RunUpgradeCatalog draftCatalog;
            IReadOnlyList<RunUpgradeId> resolvedLocks = lockedChoices;
            bool requireEvolutionChoice = false;
            switch (selectionKind)
            {
                case SurvivorsRewardSelectionKind.LevelUp:
                    DraftRarityProfile normalProfile = ResolveNormalDraftRarityProfile();
                    draftCatalog = CreateEligibleDraftCatalog(normalProfile);
                    if (resolvedLocks == null || resolvedLocks.Count == 0)
                    {
                        resolvedLocks = CreateNormalDraftChoiceLocks(normalProfile, rerollIndex);
                    }
                    break;
                case SurvivorsRewardSelectionKind.EliteUpgrade:
                    draftCatalog = CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Miniboss, requireEvolutionChoice: false);
                    resolvedLocks = CreateRewardDraftChoiceLocks(SurvivorsEnemyRole.Miniboss, CurrentTuning.DraftChoiceCount, rerollIndex);
                    break;
                case SurvivorsRewardSelectionKind.BossUpgrade:
                    draftCatalog = CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Boss, requireEvolutionChoice: false);
                    resolvedLocks = CreateRewardDraftChoiceLocks(SurvivorsEnemyRole.Boss, CurrentTuning.DraftChoiceCount, rerollIndex);
                    break;
                default:
                    return false;
            }

            if (draftCatalog == null)
            {
                return false;
            }

            int choiceCount = Mathf.Max(0, CurrentTuning.DraftChoiceCount);
            if (choiceCount <= 0)
            {
                return false;
            }

            draft = RunUpgradeDraftService.Generate(
                draftCatalog,
                _upgradeState,
                new RunUpgradeDraftRequest(
                    choiceCount,
                    ResolveDraftSeed(selectionKind),
                    Mathf.Max(0, rerollIndex),
                    resolvedLocks == null || resolvedLocks.Count == 0 ? null : resolvedLocks));
            if (draft == null || draft.Choices.Count == 0)
            {
                draft = null;
                return false;
            }

            if (requireEvolutionChoice && !DraftContainsEvolution(draft))
            {
                draft = null;
                return false;
            }

            return true;
        }

        private int ResolveDraftSeed(SurvivorsRewardSelectionKind selectionKind)
        {
            int seedSalt = 0;
            if (selectionKind == SurvivorsRewardSelectionKind.EliteUpgrade)
            {
                seedSalt = 173;
            }
            else if (selectionKind == SurvivorsRewardSelectionKind.BossUpgrade)
            {
                seedSalt = 313;
            }

            return CurrentTuning.RunSeed + Level + SelectedUpgradeCount + MinibossKilledCount + (BossKilledCount * 11) + seedSalt;
        }

        private bool TryGetRunUpgrade(string upgradeId, out RunUpgradeDefinition definition)
        {
            definition = null;
            return _upgradeCatalog != null &&
                !string.IsNullOrWhiteSpace(upgradeId) &&
                _upgradeCatalog.TryGet(new RunUpgradeId(upgradeId), out definition);
        }

        private bool TryGetUpgradeMetadata(string upgradeId, out SurvivorsRunUpgradeMetadata metadata)
        {
            metadata = null;
            return !string.IsNullOrWhiteSpace(upgradeId) && _upgradeMetadataById.TryGetValue(upgradeId, out metadata);
        }

        private string ResolveUpgradeDisplayName(RunUpgradeId upgradeId)
        {
            return TryGetUpgradeMetadata(upgradeId.Value, out SurvivorsRunUpgradeMetadata metadata) &&
                !string.IsNullOrWhiteSpace(metadata.DisplayName)
                    ? metadata.DisplayName
                    : BasicSurvivorsGame.GetUpgradeDisplayName(upgradeId);
        }

        private int ResolveRequiredUpgradeRank(SurvivorsRunUpgradeMetadata metadata)
        {
            if (metadata == null)
            {
                return 1;
            }

            int requiredRank = Mathf.Max(1, metadata.RequiredUpgradeRank);
            if (metadata.IsEvolution)
            {
                requiredRank = Mathf.Max(1, requiredRank - Mathf.Max(0, CurrentTuning.EvolutionRequiredRankReduction));
            }

            return requiredRank;
        }

        private bool CanRerollCurrentDraft()
        {
            return State == SurvivorsRunState.LevelUp &&
                IsRunUpgradeSelectionKind(_rewardSelectionKind) &&
                _currentDraft != null &&
                DraftRerollsRemaining > 0;
        }

        private bool CanSkipCurrentDraft()
        {
            return State == SurvivorsRunState.LevelUp &&
                IsRunUpgradeSelectionKind(_rewardSelectionKind) &&
                _currentDraft != null;
        }

        private bool CanBanishCurrentDraft()
        {
            return State == SurvivorsRunState.LevelUp &&
                _rewardSelectionKind != SurvivorsRewardSelectionKind.BossUpgrade &&
                IsRunUpgradeSelectionKind(_rewardSelectionKind) &&
                _currentDraft != null &&
                DraftBanishesRemaining > 0;
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

        private bool IsUpgradeEligibleForCurrentBuild(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null || _upgradeCatalog == null || _upgradeState == null)
            {
                return false;
            }

            if (RunUpgradeDraftService.GetAvailability(_upgradeCatalog, _upgradeState, upgrade) != RunUpgradeSelectionStatus.Selected)
            {
                return false;
            }

            if (!TryGetUpgradeMetadata(upgrade.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return true;
            }

            if (metadata.UsesPassiveSlot && _upgradeState.GetRank(upgrade.Id) <= 0 && ActivePassiveCount >= MaxPassiveSlots)
            {
                return false;
            }

            if (metadata.UsesWeaponSlot && _upgradeState.GetRank(upgrade.Id) <= 0)
            {
                if (ActiveWeaponCount >= MaxWeaponSlots)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(metadata.AffectedContentId) && HasWeaponInLoadoutForTest(metadata.AffectedContentId))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredOwnedWeaponId) && !HasWeaponInLoadoutForTest(metadata.RequiredOwnedWeaponId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredUpgradeId))
            {
                int requiredRank = ResolveRequiredUpgradeRank(metadata);
                if (_upgradeState.GetRank(new RunUpgradeId(metadata.RequiredUpgradeId)) < requiredRank)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredPassiveUpgradeId) &&
                _upgradeState.GetRank(new RunUpgradeId(metadata.RequiredPassiveUpgradeId)) <= 0)
            {
                return false;
            }

            return true;
        }

        private SurvivorsRunUpgradeCategory ResolveCurrentUpgradeCategory(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null || !TryGetUpgradeMetadata(upgrade.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return SurvivorsRunUpgradeCategory.WeaponUpgrade;
            }

            if (metadata.Category == SurvivorsRunUpgradeCategory.Passive && _upgradeState.GetRank(upgrade.Id) > 0)
            {
                return SurvivorsRunUpgradeCategory.PassiveUpgrade;
            }

            if (metadata.Category == SurvivorsRunUpgradeCategory.Weapon && _upgradeState.GetRank(upgrade.Id) > 0)
            {
                return SurvivorsRunUpgradeCategory.WeaponUpgrade;
            }

            return metadata.Category;
        }

        private void RecordRunBuildSelection(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null || !TryGetUpgradeMetadata(upgrade.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return;
            }

            if (metadata.UsesPassiveSlot)
            {
                bool addedPassive = _ownedPassiveUpgradeIds.Add(upgrade.Id.Value);
                if (addedPassive)
                {
                    TryTriggerPassiveLoadoutSurge(upgrade);
                }
            }

            if (metadata.UsesWeaponSlot)
            {
                TryAddWeaponToLoadout(metadata.AffectedContentId);
            }

            if (metadata.IsEvolution)
            {
                RecordMetricTime(ref _firstEvolutionAcquiredTimeSeconds);
                _ownedEvolutionUpgradeIds.Add(upgrade.Id.Value);
                WeaponEvolutionFeedbackCount++;
                TriggerWeaponEvolutionSurge(upgrade);
                TriggerEvolutionChainSurge(upgrade);
            }
        }

        private void TriggerWeaponEvolutionSurge(RunUpgradeDefinition upgrade)
        {
            float radius = Mathf.Max(0f, CurrentTuning.EvolutionSurgeRadius);
            float damage = Mathf.Max(0f, CurrentTuning.EvolutionSurgeDamage);
            string name = upgrade == null ? "Evolution" : ResolveUpgradeDisplayName(upgrade.Id);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.evolution.surge");
                    hitCount++;
                }
            }

            int recalledGemCount = StartMagnetRecall();
            if (recalledGemCount > 0)
            {
                EvolutionMagnetRecallCount++;
                EvolutionMagnetRecallGemCount += recalledGemCount;
                LastEvolutionMagnetRecallFeedbackLabel = $"{name} Recall: {recalledGemCount} XP gems pulled";
            }

            WeaponEvolutionSurgeCount++;
            WeaponEvolutionSurgeHitCount += hitCount;
            LastWeaponEvolutionSurgeFeedbackLabel = $"{name} Surge: {hitCount} enemies hit";
            if (recalledGemCount > 0)
            {
                LastWeaponEvolutionSurgeFeedbackLabel += $", {recalledGemCount} XP recalled";
            }

            RecordStreakRewardFeedback(LastWeaponEvolutionSurgeFeedbackLabel, new Color(0.72f, 0.42f, 1f));
            PlayFeedback(_levelUpPulse, PlayerPosition, Mathf.Clamp(46 + hitCount * 5, 54, 96), _levelUpClip);
        }

        private void TriggerEvolutionChainSurge(RunUpgradeDefinition upgrade)
        {
            int evolutionCount = EvolvedWeaponCount;
            if (evolutionCount < Mathf.Max(2, CurrentTuning.EvolutionChainSurgeMinimumEvolutions))
            {
                return;
            }

            float duration = Mathf.Max(0f, CurrentTuning.EvolutionChainSurgeDurationSeconds);
            if (duration > 0f)
            {
                _evolutionChainSurgeTimer = Mathf.Max(_evolutionChainSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, CurrentTuning.EvolutionChainSurgePulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.EvolutionChainSurgePulseDamage);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.evolution.legend-surge");
                    hitCount++;
                }
            }

            string name = upgrade == null ? "Evolution" : ResolveUpgradeDisplayName(upgrade.Id);
            EvolutionChainSurgeActivationCount++;
            EvolutionChainSurgePulseHitCount += hitCount;
            LastEvolutionChainSurgeFeedbackLabel = $"{name} Legend Surge: {evolutionCount} evolutions, {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastEvolutionChainSurgeFeedbackLabel += $", rush {EvolutionChainSurgeRemainingSeconds:0.#}s";
            }

            RecordStreakRewardFeedback(LastEvolutionChainSurgeFeedbackLabel, new Color(1f, 0.66f, 0.2f));
            PlayFeedback(_levelUpPulse, PlayerPosition, Mathf.Clamp(58 + hitCount * 5, 64, 112), _levelUpClip);
        }

        private void TriggerRewardUpgradeSurge(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind selectionKind)
        {
            float radius = Mathf.Max(0f, CurrentTuning.RewardUpgradeSurgeRadius);
            float damage = Mathf.Max(0f, CurrentTuning.RewardUpgradeSurgeDamage);
            string name = upgrade == null ? ResolveRewardKindLabel(selectionKind) : ResolveUpgradeDisplayName(upgrade.Id);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.reward.surge");
                    hitCount++;
                }
            }

            RewardUpgradeSurgeCount++;
            RewardUpgradeSurgeHitCount += hitCount;
            LastRewardUpgradeSurgeFeedbackLabel = $"{name} Reward Surge: {hitCount} enemies hit";
            Color accent = selectionKind == SurvivorsRewardSelectionKind.BossUpgrade
                ? new Color(1f, 0.52f, 0.24f)
                : new Color(0.36f, 0.82f, 1f);
            RecordStreakRewardFeedback(LastRewardUpgradeSurgeFeedbackLabel, accent);
            PlayFeedback(
                selectionKind == SurvivorsRewardSelectionKind.BossUpgrade ? _bossPulse : _levelUpPulse,
                PlayerPosition,
                Mathf.Clamp(34 + hitCount * 4, 40, 78),
                selectionKind == SurvivorsRewardSelectionKind.BossUpgrade ? _bossClip : _levelUpClip);
        }

        private void TriggerLevelUpPulse(RunUpgradeDefinition upgrade)
        {
            float radius = Mathf.Max(0f, CurrentTuning.LevelUpPulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.LevelUpPulseDamage);
            string name = upgrade == null ? "Level Up" : ResolveUpgradeDisplayName(upgrade.Id);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.level-up.pulse");
                    hitCount++;
                }
            }

            LevelUpPulseCount++;
            LevelUpPulseHitCount += hitCount;
            LastLevelUpPulseFeedbackLabel = $"{name} Level Pulse: {hitCount} enemies hit";
            RecordStreakRewardFeedback(LastLevelUpPulseFeedbackLabel, new Color(1f, 0.84f, 0.32f));
            PlayFeedback(_levelUpPulse, PlayerPosition, Mathf.Clamp(20 + hitCount * 4, 26, 64), _levelUpClip);
        }

        private void TriggerRewardJackpot(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind selectionKind)
        {
            if (upgrade == null || upgrade.Rarity < RunUpgradeRarity.Rare || !IsRewardUpgradeSelectionKind(selectionKind))
            {
                return;
            }

            int rarityTier = Mathf.Max(0, (int)upgrade.Rarity - (int)RunUpgradeRarity.Rare);
            int rewardTierBonus = selectionKind == SurvivorsRewardSelectionKind.BossUpgrade ? 1 : 0;
            int gemCount = Mathf.Max(
                0,
                CurrentTuning.RewardJackpotExperienceGemBaseCount +
                rarityTier * Mathf.Max(0, CurrentTuning.RewardJackpotExperienceGemPerRarityTier) +
                rewardTierBonus);
            int xpPerGem = Mathf.Max(
                1,
                Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * (2.5f + rarityTier * 0.7f + rewardTierBonus * 0.5f)));
            int shardAmount = Mathf.Max(
                0,
                CurrentTuning.RewardJackpotBloodShardBaseAmount +
                rarityTier +
                rewardTierBonus +
                (upgrade.Rarity >= RunUpgradeRarity.Legendary ? Mathf.Max(0, CurrentTuning.RewardJackpotLegendaryExtraBloodShardAmount) : 0));

            Vector3 origin = PlayerPosition;
            float cacheRadius = 0.82f + rarityTier * 0.12f;
            int spawnedGems = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.35f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * cacheRadius;
                SurvivorsPickupActor pickup = SpawnPickup(SurvivorsPickupKind.Experience, origin + offset, xpPerGem);
                if (pickup != null)
                {
                    spawnedGems++;
                    RewardJackpotExperienceGemDropCount++;
                    StartMajorRewardCacheAttraction(pickup);
                }
            }

            int spawnedShardAmount = 0;
            if (shardAmount > 0)
            {
                SurvivorsPickupActor shard = SpawnPickup(SurvivorsPickupKind.BloodShard, origin + new Vector3(0f, 0f, cacheRadius * 0.45f), shardAmount);
                if (shard != null)
                {
                    spawnedShardAmount = shardAmount;
                    RewardJackpotBloodShardDropCount++;
                    RewardJackpotBloodShardsDropped += shardAmount;
                    StartMajorRewardCacheAttraction(shard);
                }
            }

            if (spawnedGems <= 0 && spawnedShardAmount <= 0)
            {
                return;
            }

            RewardJackpotCount++;
            string rewardLabel = ResolveRewardKindLabel(selectionKind);
            int totalExperience = spawnedGems * xpPerGem;
            LastRewardJackpotFeedbackLabel = $"{rewardLabel} Jackpot: {upgrade.Rarity} +{totalExperience} XP";
            if (spawnedShardAmount > 0)
            {
                LastRewardJackpotFeedbackLabel += $" +{spawnedShardAmount} {CurrencyRewardLabel}";
            }

            RecordStreakRewardFeedback(LastRewardJackpotFeedbackLabel, ResolveRarityAccentColor(upgrade.Rarity));
            PlayFeedback(
                selectionKind == SurvivorsRewardSelectionKind.BossUpgrade ? _bossPulse : _levelUpPulse,
                origin,
                Mathf.Clamp(24 + spawnedGems * 4 + spawnedShardAmount * 3, 32, 86),
                _pickupClip);
        }

        private void RecordNewlyEligibleEvolutionFeedback()
        {
            if (_upgradeCatalog == null)
            {
                return;
            }

            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition == null || !IsEvolutionUpgrade(definition))
                {
                    continue;
                }

                string upgradeId = definition.Id.Value;
                if (_ownedEvolutionUpgradeIds.Contains(upgradeId) || _announcedEvolutionReadyUpgradeIds.Contains(upgradeId))
                {
                    continue;
                }

                if (!IsUpgradeEligibleForCurrentBuild(definition))
                {
                    if (!_announcedEvolutionGoalUpgradeIds.Contains(upgradeId) &&
                        TryResolveEvolutionMissingPassive(definition, out RunUpgradeDefinition missingPassive))
                    {
                        _announcedEvolutionGoalUpgradeIds.Add(upgradeId);
                        RecordEvolutionGoalFeedback(definition, missingPassive);
                    }

                    continue;
                }

                _announcedEvolutionReadyUpgradeIds.Add(upgradeId);
                RecordEvolutionReadyFeedback(definition);
            }
        }

        private void RecordEvolutionGoalFeedback(RunUpgradeDefinition evolution, RunUpgradeDefinition missingPassive)
        {
            string evolutionName = evolution == null ? "Evolution" : ResolveUpgradeDisplayName(evolution.Id);
            string passiveName = missingPassive == null ? "matching passive" : ResolveUpgradeDisplayName(missingPassive.Id);
            _evolutionReadyFeedbackLabel = $"Evolution Goal: {passiveName} for {evolutionName}";
            _evolutionReadyFeedbackTimer = EvolutionReadyFeedbackDurationSeconds;
            EvolutionGoalFeedbackCount++;
            LastEvolutionGoalFeedbackLabel = _evolutionReadyFeedbackLabel;
            PlayFeedback(_levelUpPulse, PlayerPosition, 22, _levelUpClip);
        }

        private void RecordEvolutionReadyFeedback(RunUpgradeDefinition evolution)
        {
            RecordMetricTime(ref _firstEvolutionEligibilityTimeSeconds);
            string name = ResolveUpgradeDisplayName(evolution.Id);
            _evolutionReadyFeedbackLabel = $"Evolution Ready: {name}";
            _evolutionReadyFeedbackTimer = EvolutionReadyFeedbackDurationSeconds;
            EvolutionReadyFeedbackCount++;
            LastEvolutionReadyFeedbackLabel = _evolutionReadyFeedbackLabel;
            PlayFeedback(_levelUpPulse, PlayerPosition, 36, _levelUpClip);
        }

        private bool IsUpgradeAllowedForSelectedClass(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId) || _upgradeClassGates == null)
            {
                return true;
            }

            for (int i = 0; i < _upgradeClassGates.Count; i++)
            {
                SurvivorsClassUpgradeGateDefinition gate = _upgradeClassGates[i];
                if (gate != null && string.Equals(gate.UpgradeId, upgradeId, StringComparison.Ordinal))
                {
                    return gate.IsAvailableToClass(_selectedClass);
                }
            }

            return true;
        }

        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> ResolveStartingWeaponDefinitions(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> allDefinitions)
        {
            if (allDefinitions == null || allDefinitions.Count == 0)
            {
                return Array.Empty<SurvivorsWeaponArchetypeDefinition>();
            }

            if (_selectedClass == null || _selectedClass.StartingWeaponIds.Count == 0)
            {
                return new[] { allDefinitions[0] };
            }

            var selected = new List<SurvivorsWeaponArchetypeDefinition>(_selectedClass.StartingWeaponIds.Count);
            for (int classWeaponIndex = 0; classWeaponIndex < _selectedClass.StartingWeaponIds.Count; classWeaponIndex++)
            {
                string weaponId = _selectedClass.StartingWeaponIds[classWeaponIndex];
                for (int definitionIndex = 0; definitionIndex < allDefinitions.Count; definitionIndex++)
                {
                    SurvivorsWeaponArchetypeDefinition definition = allDefinitions[definitionIndex];
                    if (definition != null && string.Equals(definition.Id, weaponId, StringComparison.Ordinal))
                    {
                        selected.Add(definition);
                        break;
                    }
                }
            }

            return selected.Count == 0 ? new[] { allDefinitions[0] } : selected;
        }

        private bool TryAddWeaponToLoadout(string weaponId)
        {
            if (_weaponLoadout == null || string.IsNullOrWhiteSpace(weaponId) || ActiveWeaponCount >= MaxWeaponSlots)
            {
                return false;
            }

            if (_weaponLoadout.ContainsWeapon(weaponId))
            {
                return false;
            }

            SurvivorsWeaponArchetypeDefinition definition = FindWeaponDefinition(weaponId);
            if (definition == null || !_weaponLoadout.TryAddWeapon(definition))
            {
                return false;
            }

            TryTriggerWeaponLoadoutSurge(definition);
            return true;
        }

        private void TryTriggerWeaponLoadoutSurge(SurvivorsWeaponArchetypeDefinition addedWeapon)
        {
            if (_weaponLoadoutSurgeUsed || ActiveWeaponCount < MaxWeaponSlots)
            {
                return;
            }

            _weaponLoadoutSurgeUsed = true;

            float duration = Mathf.Max(0f, CurrentTuning.WeaponLoadoutSurgeDurationSeconds);
            if (duration > 0f)
            {
                _weaponLoadoutSurgeTimer = Mathf.Max(_weaponLoadoutSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, CurrentTuning.WeaponLoadoutSurgePulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.WeaponLoadoutSurgePulseDamage);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.weapon-loadout.arsenal-surge");
                    hitCount++;
                }
            }

            string name = addedWeapon == null ? "Weapon" : addedWeapon.DisplayName;
            WeaponLoadoutSurgeActivationCount++;
            WeaponLoadoutSurgePulseHitCount += hitCount;
            LastWeaponLoadoutSurgeFeedbackLabel = $"{name} Arsenal Surge: {ActiveWeaponCount}/{MaxWeaponSlots} weapons, {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastWeaponLoadoutSurgeFeedbackLabel += $", rush {WeaponLoadoutSurgeRemainingSeconds:0.#}s";
            }

            RecordStreakRewardFeedback(LastWeaponLoadoutSurgeFeedbackLabel, new Color(0.45f, 1f, 0.62f));
            PlayFeedback(_levelUpPulse, PlayerPosition, Mathf.Clamp(34 + hitCount * 5, 42, 86), _levelUpClip);
        }

        private void TryTriggerPassiveLoadoutSurge(RunUpgradeDefinition addedPassive)
        {
            if (_passiveLoadoutSurgeUsed || ActivePassiveCount < MaxPassiveSlots)
            {
                return;
            }

            _passiveLoadoutSurgeUsed = true;

            float duration = Mathf.Max(0f, CurrentTuning.PassiveLoadoutSurgeDurationSeconds);
            if (duration > 0f)
            {
                _passiveLoadoutSurgeTimer = Mathf.Max(_passiveLoadoutSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, CurrentTuning.PassiveLoadoutSurgePulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.PassiveLoadoutSurgePulseDamage);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.passive-loadout.harmony-surge");
                    hitCount++;
                }
            }

            string name = addedPassive == null ? "Passive" : ResolveUpgradeDisplayName(addedPassive.Id);
            PassiveLoadoutSurgeActivationCount++;
            PassiveLoadoutSurgePulseHitCount += hitCount;
            LastPassiveLoadoutSurgeFeedbackLabel = $"{name} Harmony Surge: {ActivePassiveCount}/{MaxPassiveSlots} passives, {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastPassiveLoadoutSurgeFeedbackLabel += $", rush {PassiveLoadoutSurgeRemainingSeconds:0.#}s";
            }

            RecordStreakRewardFeedback(LastPassiveLoadoutSurgeFeedbackLabel, new Color(0.58f, 0.92f, 1f));
            PlayFeedback(_levelUpPulse, PlayerPosition, Mathf.Clamp(32 + hitCount * 5, 40, 82), _levelUpClip);
        }

        private SurvivorsWeaponArchetypeDefinition FindWeaponDefinition(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions = _weaponArchetypeDefinitions;
            if (definitions == null || definitions.Count == 0)
            {
                definitions = CreateWeaponArchetypeDefinitions(CurrentTuning);
                _weaponArchetypeDefinitions = definitions;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition definition = definitions[i];
                if (definition != null && string.Equals(definition.Id, weaponId, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        private void ApplyPersistentMetaBonuses()
        {
            EnsureMetaProgressionLoaded();
            float previousDamageBonus = PersistentDamageBonus;
            float previousMaxHealthBonus = PersistentMaxHealthBonus;
            float previousPickupRangeBonus = PersistentPickupRangeBonus;
            float previousExperienceGainBonus = PersistentExperienceGainMultiplierBonus;

            PersistentDamageBonus = _metaProgression.GetPersistentDamageBonus(BasicSurvivorsGame.WeaponTarget.Value);
            PersistentMaxHealthBonus = _metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaMaxHealthEffectId, BasicSurvivorsGame.PlayerTarget.Value);
            PersistentPickupRangeBonus = _metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaPickupRangeEffectId, BasicSurvivorsGame.PickupTarget.Value);
            PersistentExperienceGainMultiplierBonus = _metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaExperienceGainEffectId, BasicSurvivorsGame.ExperienceTarget.Value);
            PersistentDraftRerollBonus = Mathf.Max(0, Mathf.RoundToInt(_metaProgression.GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaDraftRerollEffectId, BasicSurvivorsGame.PlayerTarget.Value)));

            DamageBonus += PersistentDamageBonus - previousDamageBonus;
            PickupRangeBonus += PersistentPickupRangeBonus - previousPickupRangeBonus;
            ExperienceGainMultiplierBonus += PersistentExperienceGainMultiplierBonus - previousExperienceGainBonus;
            if (_playerHealth != null)
            {
                float maxHealthDelta = PersistentMaxHealthBonus - previousMaxHealthBonus;
                if (Mathf.Abs(maxHealthDelta) > 0.001f)
                {
                    _playerHealth.ChangeMaximumHealth(_playerHealth.MaximumHealth + maxHealthDelta, MaximumChangePolicy.FillToMaximum);
                }
            }
        }

        private void ApplySelectedClassBonuses()
        {
            if (_selectedClass == null)
            {
                return;
            }

            for (int i = 0; i < _selectedClass.StartingStatModifiers.Count; i++)
            {
                SurvivorsClassStatModifierDefinition modifier = _selectedClass.StartingStatModifiers[i];
                if (modifier == null)
                {
                    continue;
                }

                if (modifier.StatKind == SurvivorsClassStatKind.MoveSpeed)
                {
                    MoveSpeedBonus += modifier.Amount;
                }
                else if (modifier.StatKind == SurvivorsClassStatKind.Damage)
                {
                    DamageBonus += modifier.Amount;
                }
                else if (modifier.StatKind == SurvivorsClassStatKind.MaxHealth && _playerHealth != null)
                {
                    _playerHealth.ChangeMaximumHealth(_playerHealth.MaximumHealth + modifier.Amount, MaximumChangePolicy.FillToMaximum);
                }
            }
        }

        private void GrantMajorEnemyReward(SurvivorsEnemyRole role)
        {
            string rewardId = BasicSurvivorsGame.MinibossRewardId;
            if (role == SurvivorsEnemyRole.Boss)
            {
                rewardId = BasicSurvivorsGame.BossRewardId;
            }
            else if (IsEliteRole(role))
            {
                rewardId = BasicSurvivorsGame.EliteRewardId;
            }

            SurvivorsMetaProgressionDefinition definition = ResolveMetaProgressionDefinition();
            if (!definition.TryGetReward(rewardId, out SurvivorsRewardDefinition reward))
            {
                return;
            }

            _bonusBloodShardsEarnedThisRun += reward.CurrencyAmount;
            _bonusLegacyExperienceEarnedThisRun += reward.TrackAmount;
            if (role == SurvivorsEnemyRole.Boss)
            {
                BossRewardGrantCount++;
            }
            else if (IsEliteRole(role))
            {
                EliteRewardGrantCount++;
            }
            else
            {
                MinibossRewardGrantCount++;
            }
        }

        private void GrantRunRewards(bool victory)
        {
            if (_runRewardsGranted)
            {
                return;
            }

            EnsureMetaProgressionLoaded();
            bool grantVictoryClassUnlockReward = ShouldGrantVictoryClassUnlockReward(victory);
            if (grantVictoryClassUnlockReward)
            {
                AddClassUnlockRewardToRunBonus();
            }

            LastRunResult = SurvivorsRunRewardCalculator.Calculate(
                RunTimeSeconds,
                Level,
                MinibossKilledCount,
                BossKilledCount,
                victory,
                _bonusBloodShardsEarnedThisRun,
                _bonusLegacyExperienceEarnedThisRun);
            ApplyRunRewardMultiplier(LastRunResult, CurrentTuning.RunRewardMultiplier);
            BloodShardsEarnedThisRun = LastRunResult.BloodShardsEarned;
            LegacyExperienceEarnedThisRun = LastRunResult.LegacyExperienceEarned;
            if (_metaProgression.GrantRunRewards(LastRunResult).Succeeded)
            {
                _runRewardsGranted = true;
                if (grantVictoryClassUnlockReward)
                {
                    GrantVictoryClassUnlockReward();
                }
            }

            RebuildLastRunSummaryLines(victory);
            PlayAudioEvent(AudioEventRunSummaryOpened, _levelUpClip, 0.25f);
        }

        private static void ApplyRunRewardMultiplier(SurvivorsRunRewardSummary summary, float multiplier)
        {
            if (summary == null)
            {
                return;
            }

            float resolvedMultiplier = Mathf.Max(0f, multiplier);
            if (Mathf.Approximately(resolvedMultiplier, 1f))
            {
                return;
            }

            summary.BloodShardsEarned = ScaleReward(summary.BloodShardsEarned, resolvedMultiplier);
            summary.LegacyExperienceEarned = ScaleReward(summary.LegacyExperienceEarned, resolvedMultiplier);
        }

        private static int ScaleReward(int amount, float multiplier)
        {
            if (amount <= 0 || multiplier <= 0f)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
        }

        private void RebuildLastRunSummaryLines(bool victory)
        {
            _lastRunSummaryLines.Clear();
            string result = victory
                ? (CurrentTuning.EndlessContinuationEnabled ? "Victory - Endless continuation available" : "Victory")
                : "Defeat";
            _lastRunSummaryTitle = ActiveUiTheme.runSummaryTitle + " - " + (victory ? "Victory" : "Defeat");
            _lastRunSummaryLines.Add("Run mode: " + CurrentRunModeDisplayName);
            _lastRunSummaryLines.Add("Result: " + result);
            _lastRunSummaryLines.Add($"Run time: {FormatRunTime(RunTimeSeconds)} / target {FormatRunTime(CurrentTuning.SurvivalVictoryTimeSeconds)}");
            _lastRunSummaryLines.Add($"Level reached: {Level}   XP collected: {ExperienceCollected}   Stored XP: {Experience}/{RequiredExperienceForNextLevel}");
            _lastRunSummaryLines.Add($"Kills: {KilledCount}   Elites: {EliteKilledCount}   Minibosses: {MinibossKilledCount}   Bosses: {BossKilledCount}");
            _lastRunSummaryLines.Add($"Rewards earned: +{BloodShardsEarnedThisRun} {CurrencyRewardLabel}, +{LegacyExperienceEarnedThisRun} {ProgressionRewardLabel}");
            _lastRunSummaryLines.Add($"Reward profile: target {FormatRunTime(CurrentTuning.TargetDurationSeconds)}, multiplier x{CurrentTuning.RunRewardMultiplier:0.##}");
            _lastRunSummaryLines.Add($"Meta bank: {MetaBloodShards} {CurrencyDisplayName}, {LifetimeLegacyExperience} {ProgressionDisplayName}");
            _lastRunSummaryLines.Add($"Damage taken: {DamageTakenThisRun:0.#}   Final health: {CurrentHealth:0.#}/{MaxHealth:0.#}");
            _lastRunSummaryLines.Add($"Weapons {ActiveWeaponCount}/{MaxWeaponSlots}: {FormatActiveWeaponList()}");
            _lastRunSummaryLines.Add($"Passives {ActivePassiveCount}/{MaxPassiveSlots}: {FormatRunSummaryUpgradeList(_ownedPassiveUpgradeIds, includeRanks: true)}");
            _lastRunSummaryLines.Add($"Evolutions {EvolvedWeaponCount}: {FormatRunSummaryUpgradeList(_ownedEvolutionUpgradeIds, includeRanks: false)}");
            _lastRunSummaryLines.Add($"Relics {SelectedRelicCount}/{ResolveTotalRelicCount()}: {FormatSelectedRelicList()}");
            _lastRunSummaryLines.Add($"Pickup build: radius {CurrentPickupAttractRange:0.#}, magnet range {CurrentPickupAttractRange:0.#}, pull speed {CurrentPickupAttractionSpeed:0.#}, pulse {FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds)}, recalls {MagnetRecallCount}");
            _lastRunSummaryLines.Add("Top weapon by damage: not tracked yet");
            _lastRunSummaryLines.Add("Top weapon by kills: not tracked yet");
            _lastRunSummaryLines.Add("Best moment: " + (string.IsNullOrWhiteSpace(_bestMomentLabel) ? ResolveFallbackBestMomentLabel() : _bestMomentLabel));
            if (ClassUnlockRewardCount > 0)
            {
                _lastRunSummaryLines.Add("Class unlocked: " + ResolveClassDisplayName(BasicSurvivorsGame.EmberVanguardClassId, "Ember Vanguard"));
            }
        }

        private string ResolveFallbackBestMomentLabel()
        {
            if (_firstEvolutionAcquiredTimeSeconds >= 0f)
            {
                return "First evolution at " + FormatRunTime(_firstEvolutionAcquiredTimeSeconds);
            }

            if (!string.IsNullOrWhiteSpace(_highestChosenRarityLabel))
            {
                return "Highest rarity chosen: " + _highestChosenRarityLabel;
            }

            if (BestKillStreak > 0)
            {
                return "Best streak " + BestKillStreak.ToString();
            }

            return "First run data captured";
        }

        private void GrantVictoryClassUnlockReward()
        {
            EnsureClassLibraryLoaded();
            if (_metaProgression != null && _metaProgression.UnlockClass(BasicSurvivorsGame.EmberVanguardClassId, _classLibrary))
            {
                ClassUnlockRewardCount++;
                RecordClassUnlockRewardFeedback();
            }
        }

        private void RecordClassUnlockRewardFeedback()
        {
            LastClassUnlockRewardFeedbackLabel = "Class Unlocked: " + ResolveClassDisplayName(BasicSurvivorsGame.EmberVanguardClassId, "Ember Vanguard");
            SurvivorsMetaProgressionDefinition definition = ResolveMetaProgressionDefinition();
            if (definition.TryGetReward(BasicSurvivorsGame.EmberVanguardUnlockRewardId, out SurvivorsRewardDefinition reward))
            {
                LastClassUnlockRewardFeedbackLabel += $" +{reward.CurrencyAmount} {CurrencyRewardLabel} +{reward.TrackAmount} {ProgressionRewardLabel}";
            }

            _classUnlockRewardFeedbackLabel = LastClassUnlockRewardFeedbackLabel;
            _classUnlockRewardFeedbackTimer = ClassUnlockRewardFeedbackDurationSeconds;
            PlayFeedback(_bossPulse, PlayerPosition, 52, _levelUpClip);
        }

        private string ResolveClassDisplayName(string classId, string fallback)
        {
            EnsureClassLibraryLoaded();
            return _classLibrary != null &&
                _classLibrary.TryGetClass(classId, out SurvivorsClassDefinition definition) &&
                !string.IsNullOrWhiteSpace(definition.DisplayName)
                    ? definition.DisplayName
                    : fallback;
        }

        private bool ShouldGrantVictoryClassUnlockReward(bool victory)
        {
            if (!victory)
            {
                return false;
            }

            EnsureClassLibraryLoaded();
            return _metaProgression != null &&
                _classLibrary != null &&
                !_metaProgression.IsClassUnlocked(BasicSurvivorsGame.EmberVanguardClassId, _classLibrary);
        }

        private void AddClassUnlockRewardToRunBonus()
        {
            SurvivorsMetaProgressionDefinition definition = ResolveMetaProgressionDefinition();
            if (!definition.TryGetReward(BasicSurvivorsGame.EmberVanguardUnlockRewardId, out SurvivorsRewardDefinition reward))
            {
                return;
            }

            _bonusBloodShardsEarnedThisRun += reward.CurrencyAmount;
            _bonusLegacyExperienceEarnedThisRun += reward.TrackAmount;
        }

        private void ReleaseMetaProgressionService()
        {
            if (_metaProgression != null && _ownsMetaProgressionService)
            {
                _metaProgression.Dispose();
            }

            _metaProgression = null;
            _ownsMetaProgressionService = false;
            _metaProfileLoaded = false;
        }

        private void MovePlayer(Vector2 movementInput, float deltaTime)
        {
            if (_playerObject == null || movementInput.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector2 normalized = movementInput.sqrMagnitude > 1f ? movementInput.normalized : movementInput;
            Vector3 delta = new Vector3(normalized.x, 0f, normalized.y) * (PlayerMoveSpeed * deltaTime);
            _playerObject.transform.position += delta;
            if (delta.sqrMagnitude > 0.0001f)
            {
                _playerObject.transform.forward = delta.normalized;
                RecordRoamingArenaTravel(delta);
            }
        }

        private bool TryDash(Vector2 directionInput)
        {
            if (State != SurvivorsRunState.Playing || _playerObject == null || _dashCooldownTimer > 0f)
            {
                return false;
            }

            Vector3 direction = ResolveDashDirection(directionInput);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            float distance = Mathf.Max(0f, CurrentTuning.DashDistance);
            if (distance <= 0f)
            {
                return false;
            }

            Vector3 start = PlayerPosition;
            Vector3 delta = direction.normalized * distance;
            Vector3 end = start + delta;
            _playerObject.transform.position = end;
            _playerObject.transform.forward = direction.normalized;
            RecordRoamingArenaTravel(delta);

            _dashCooldownTimer = Mathf.Max(0.05f, CurrentTuning.DashCooldownSeconds);
            _playerInvulnerabilityTimer = Mathf.Max(_playerInvulnerabilityTimer, CurrentTuning.DashInvulnerabilitySeconds);
            DashUseCount++;

            int shoved = ApplyDashEnemyPressure(start, end, direction.normalized);
            DashEnemyShoveCount += shoved;
            LastDashFeedbackLabel = shoved > 0 ? $"Arc Step: shoved {shoved}" : "Arc Step";
            RecordStreakRewardFeedback(LastDashFeedbackLabel, new Color(0.54f, 0.84f, 1f));
            PlayFeedback(_pickupPulse, end, Mathf.Clamp(18 + shoved * 4, 18, 54), _pickupClip);
            return true;
        }

        private Vector3 ResolveDashDirection(Vector2 directionInput)
        {
            Vector2 planar = directionInput.sqrMagnitude > 1f ? directionInput.normalized : directionInput;
            Vector3 direction = planar.sqrMagnitude > 0.0001f
                ? new Vector3(planar.x, 0f, planar.y)
                : PlayerForward;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            return direction.normalized;
        }

        private int ApplyDashEnemyPressure(Vector3 start, Vector3 end, Vector3 dashDirection)
        {
            float pressureRadius = Mathf.Max(0f, CurrentTuning.DashKnockbackRadius);
            float knockbackDistance = Mathf.Max(0f, CurrentTuning.DashKnockbackDistance);
            float damage = Mathf.Max(0f, CurrentTuning.DashDamage);
            if ((pressureRadius <= 0f && damage <= 0f) || _enemies.Count == 0)
            {
                return 0;
            }

            int impacted = 0;
            var enemies = new List<SurvivorsEnemyActor>(_enemies);
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector3 enemyPosition = enemy.transform.position;
                Vector3 closest = ClosestPointOnSegment(start, end, enemyPosition);
                float allowedDistance = pressureRadius + enemy.Radius;
                if ((enemyPosition - closest).sqrMagnitude > allowedDistance * allowedDistance)
                {
                    continue;
                }

                impacted++;
                if (damage > 0f && enemy.ApplyDamage(damage, "survivors.player.arc-step") != null)
                {
                    DashDamageHitCount++;
                }

                if (!enemy.IsAlive || knockbackDistance <= 0f)
                {
                    continue;
                }

                Vector3 away = enemyPosition - closest;
                away.y = 0f;
                if (away.sqrMagnitude <= 0.0001f)
                {
                    away = dashDirection;
                }

                enemy.transform.position += away.normalized * knockbackDistance;
                enemy.transform.forward = away.normalized;
            }

            return impacted;
        }

        private static Vector3 ClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.0001f)
            {
                return start;
            }

            float t = Vector3.Dot(point - start, segment) / lengthSquared;
            return start + segment * Mathf.Clamp01(t);
        }

        private void RecordRoamingArenaTravel(Vector3 delta)
        {
            if (State != SurvivorsRunState.Playing || delta.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float distance = delta.magnitude;
            _roamingCacheTravelDistance += distance;
            RecordArenaShrineTravel(delta, distance);
            float travelInterval = Mathf.Max(1f, CurrentTuning.RoamingCacheTravelInterval);
            if (_roamingCacheTravelDistance < travelInterval)
            {
                return;
            }

            int cacheCount = Mathf.Min(3, Mathf.FloorToInt(_roamingCacheTravelDistance / travelInterval));
            _roamingCacheTravelDistance = Mathf.Max(0f, _roamingCacheTravelDistance - cacheCount * travelInterval);
            Vector3 direction = delta.normalized;
            for (int i = 0; i < cacheCount; i++)
            {
                SpawnRoamingArenaCache(direction, i);
            }
        }

        private void SpawnRoamingArenaCache(Vector3 direction, int sequenceOffset)
        {
            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : PlayerForward;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            Vector3 side = new Vector3(-forward.z, 0f, forward.x);
            int nextCacheNumber = RoamingCacheDropCount + 1;
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * (1f + RunEscalationLevel * 0.08f) * ResolveEndlessExplorationExperienceMultiplier()));
            int gemCount = Mathf.Max(1, CurrentTuning.RoamingCacheExperienceGemCount + ResolveEndlessExplorationGemBonus());
            int spawnedExperience = 0;
            Vector3 origin = PlayerPosition + forward * (3.1f + sequenceOffset * 0.45f);
            for (int i = 0; i < gemCount; i++)
            {
                float lane = (i - (gemCount - 1) * 0.5f) * 0.72f;
                Vector3 position = origin + side * lane + forward * (i * 0.18f);
                if (SpawnPickup(SurvivorsPickupKind.Experience, position, xpPerGem) != null)
                {
                    spawnedExperience += xpPerGem;
                    RoamingCacheExperienceGemDropCount++;
                }
            }

            bool spawnedMagnet = false;
            if (ShouldTriggerRoamingCacheCadence(nextCacheNumber, CurrentTuning.RoamingCacheMagnetInterval))
            {
                Vector3 magnetPosition = origin + forward * 0.8f;
                if (SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1) != null)
                {
                    spawnedMagnet = true;
                    RoamingCacheMagnetDropCount++;
                }
            }

            bool spawnedBloodShard = false;
            if (ShouldTriggerRoamingCacheCadence(nextCacheNumber, CurrentTuning.RoamingCacheBloodShardInterval))
            {
                Vector3 shardPosition = origin - forward * 0.35f;
                int shardAmount = CurrentTuning.BloodShardPickupAmount + ResolveEndlessExplorationShardBonus();
                if (SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount) != null)
                {
                    spawnedBloodShard = true;
                    RoamingCacheBloodShardDropCount++;
                }
            }

            int ambushSpawned = SpawnRoamingArenaCacheAmbush(origin, forward, side, nextCacheNumber);
            int surgeExperience = TryActivateRoamingCacheSurge(nextCacheNumber, origin, forward, side, xpPerGem, out int surgeHitCount);
            if (spawnedExperience <= 0 && !spawnedMagnet && !spawnedBloodShard && ambushSpawned <= 0 && surgeExperience <= 0 && surgeHitCount <= 0)
            {
                return;
            }

            RoamingCacheDropCount++;
            string label = $"Roaming Cache: +{spawnedExperience} XP";
            if (spawnedMagnet)
            {
                label += " + Magnet";
            }

            if (spawnedBloodShard)
            {
                label += " + Shard";
            }

            if (ambushSpawned > 0)
            {
                label += $" + Ambush x{ambushSpawned}";
            }

            if (surgeExperience > 0 || surgeHitCount > 0)
            {
                label += $" + Wayfinder Surge (+{surgeExperience} XP, {surgeHitCount} hit)";
            }

            label += ResolveEndlessExplorationLabelSuffix();
            LastRoamingCacheFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(0.35f, 0.9f, 1f));
        }

        private int TryActivateRoamingCacheSurge(int cacheNumber, Vector3 origin, Vector3 forward, Vector3 side, int xpPerGem, out int pulseHitCount)
        {
            pulseHitCount = 0;
            if (!ShouldTriggerRoamingCacheCadence(cacheNumber, CurrentTuning.RoamingCacheSurgeInterval))
            {
                return 0;
            }

            _roamingCacheSurgeTimer = Mathf.Max(0.1f, CurrentTuning.RoamingCacheSurgeDurationSeconds);
            RoamingCacheSurgeActivationCount++;

            int gemCount = Mathf.Max(0, CurrentTuning.RoamingCacheSurgeBonusGemCount + ResolveEndlessExplorationGemBonus());
            int spawnedExperience = 0;
            int bonusXpPerGem = Mathf.Max(1, Mathf.RoundToInt(xpPerGem * (2f + ResolveEndlessExplorationBonusTier() * 0.12f)));
            Vector3 center = origin + forward * 1.15f;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.35f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = side * (Mathf.Cos(angle) * 0.92f) + forward * (Mathf.Sin(angle) * 0.92f);
                if (SpawnPickup(SurvivorsPickupKind.Experience, center + offset, bonusXpPerGem) != null)
                {
                    spawnedExperience += bonusXpPerGem;
                    RoamingCacheSurgeBonusExperienceGemDropCount++;
                }
            }

            pulseHitCount = TriggerRoamingCacheSurgePulse(center);
            LastRoamingCacheSurgeFeedbackLabel = $"Wayfinder Surge: +{spawnedExperience} XP, {pulseHitCount} enemies hit{ResolveEndlessExplorationLabelSuffix()}";
            return spawnedExperience;
        }

        private int TriggerRoamingCacheSurgePulse(Vector3 center)
        {
            float radius = Mathf.Max(0f, CurrentTuning.RoamingCacheSurgePulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.RoamingCacheSurgePulseDamage * ResolveEndlessExplorationPulseMultiplier());
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(center, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.roaming-cache.surge");
                    hitCount++;
                }
            }

            RoamingCacheSurgePulseHitCount += hitCount;
            PlayFeedback(_levelUpPulse, center, Mathf.Clamp(32 + hitCount * 4, 40, 80), _pickupClip);
            return hitCount;
        }

        private int SpawnRoamingArenaCacheAmbush(Vector3 origin, Vector3 forward, Vector3 side, int cacheNumber)
        {
            if (!ShouldTriggerRoamingCacheAmbush(cacheNumber))
            {
                return 0;
            }

            int baseCount = Mathf.Max(0, CurrentTuning.RoamingCacheAmbushBaseEnemyCount);
            if (baseCount <= 0)
            {
                return 0;
            }

            int maxCount = Mathf.Max(baseCount, CurrentTuning.RoamingCacheAmbushMaxEnemyCount);
            int interval = Mathf.Max(1, CurrentTuning.RoamingCacheAmbushInterval);
            int start = Mathf.Max(1, CurrentTuning.RoamingCacheAmbushStartCache);
            int distancePressureBonus = Mathf.Max(0, cacheNumber - start) / (interval * 2);
            int escalationBonus = Mathf.Max(0, RunEscalationLevel) / 3;
            int targetCount = Mathf.Clamp(baseCount + distancePressureBonus + escalationBonus + ResolveEndlessExplorationPressureBonus(), 1, maxCount);
            int available = Mathf.Max(0, ResolveEnemyMaximumAlive() + Mathf.Max(0, CurrentTuning.RoamingCacheAmbushExtraAliveAllowance) - _enemies.Count);
            targetCount = Mathf.Min(targetCount, available);
            if (targetCount <= 0)
            {
                return 0;
            }

            int spawned = 0;
            float radius = Mathf.Max(1f, CurrentTuning.RoamingCacheAmbushRadius);
            Vector3 center = origin - forward * (radius * 0.35f);
            for (int i = 0; i < targetCount; i++)
            {
                SurvivorsEnemyRole role = ResolveRoamingCacheAmbushRole(i, cacheNumber);
                SurvivorsEnemyActor enemy = SpawnGameplayEnemyOffscreen(
                    role,
                    _spawnSequence + i + cacheNumber * 37 + 809,
                    radius,
                    radius + CurrentTuning.SpawnBandDepth,
                    "roaming-cache-ambush");
                if (enemy != null)
                {
                    _activeRoamingCacheAmbushEnemies.Add(enemy);
                    spawned++;
                }
            }

            if (spawned > 0)
            {
                RoamingCacheAmbushCount++;
                RoamingCacheAmbushEnemySpawnCount += spawned;
            }

            return spawned;
        }

        private bool ShouldTriggerRoamingCacheAmbush(int cacheNumber)
        {
            int start = Mathf.Max(1, CurrentTuning.RoamingCacheAmbushStartCache);
            int interval = Mathf.Max(0, CurrentTuning.RoamingCacheAmbushInterval);
            return interval > 0 && cacheNumber >= start && ((cacheNumber - start) % interval) == 0;
        }

        private static bool ShouldTriggerRoamingCacheCadence(int cacheNumber, int cadence)
        {
            return cadence > 0 && cacheNumber > 0 && cacheNumber % cadence == 0;
        }

        private SurvivorsEnemyRole ResolveRoamingCacheAmbushRole(int index, int cacheNumber)
        {
            int pressure = Mathf.Max(0, RunEscalationLevel);
            int seed = index + cacheNumber;
            if (pressure >= 6 && seed % 7 == 0)
            {
                return SurvivorsEnemyRole.Splitter;
            }

            if (pressure >= 3 && seed % 5 == 0)
            {
                return SurvivorsEnemyRole.Spitter;
            }

            return seed % 3 == 0 ? SurvivorsEnemyRole.Runner : SurvivorsEnemyRole.Swarm;
        }

        private void SpawnRoamingCacheAmbushClearReward(Vector3 position)
        {
            int gemCount = Mathf.Max(1, CurrentTuning.RoamingCacheExperienceGemCount + 1 + ResolveEndlessExplorationGemBonus());
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * (1.35f + RunEscalationLevel * 0.08f) * ResolveEndlessExplorationExperienceMultiplier()));
            float radius = 0.72f + Mathf.Min(0.9f, gemCount * 0.08f);
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.2f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem) != null)
                {
                    spawnedExperience += xpPerGem;
                    RoamingCacheAmbushClearExperienceGemDropCount++;
                }
            }

            if (spawnedExperience <= 0)
            {
                return;
            }

            int clearNumber = RoamingCacheAmbushClearRewardCount + 1;
            bool spawnedMagnet = false;
            if (ShouldTriggerRoamingCacheCadence(clearNumber, CurrentTuning.RoamingCacheAmbushClearMagnetInterval))
            {
                Vector3 magnetPosition = position + new Vector3(radius * 0.85f, 0f, -radius * 0.3f);
                if (SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1) != null)
                {
                    spawnedMagnet = true;
                    RoamingCacheAmbushClearMagnetDropCount++;
                }
            }

            bool spawnedBloodShard = false;
            if (ShouldTriggerRoamingCacheCadence(clearNumber, CurrentTuning.RoamingCacheAmbushClearBloodShardInterval))
            {
                Vector3 shardPosition = position + new Vector3(-radius * 0.7f, 0f, radius * 0.45f);
                int shardAmount = CurrentTuning.BloodShardPickupAmount + ResolveEndlessExplorationShardBonus();
                if (SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount) != null)
                {
                    spawnedBloodShard = true;
                    RoamingCacheAmbushClearBloodShardDropCount++;
                }
            }

            RoamingCacheAmbushClearRewardCount++;
            string label = $"Roaming Ambush Cleared: +{spawnedExperience} XP";
            if (spawnedMagnet)
            {
                label += " + Magnet";
            }

            if (spawnedBloodShard)
            {
                label += " + Shard";
            }

            label += ResolveEndlessExplorationLabelSuffix();
            LastRoamingCacheAmbushClearFeedbackLabel = label;
            LastRoamingCacheFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(0.5f, 1f, 0.68f));
            PlayFeedback(_levelUpPulse, position, 18, _pickupClip);
        }

        private void RecordArenaShrineTravel(Vector3 delta, float distance)
        {
            float interval = Mathf.Max(0f, CurrentTuning.ArenaShrineTravelInterval);
            if (interval <= 0f || distance <= 0f || _activeArenaShrineEnemies.Count > 0)
            {
                return;
            }

            _arenaShrineTravelDistance += distance;
            if (_arenaShrineTravelDistance < interval)
            {
                return;
            }

            _arenaShrineTravelDistance = Mathf.Max(0f, _arenaShrineTravelDistance - interval);
            SpawnArenaShrineTrial(delta);
        }

        private int SpawnArenaShrineTrial(Vector3 direction)
        {
            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : PlayerForward;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            Vector3 side = new Vector3(-forward.z, 0f, forward.x);
            int trialNumber = ArenaShrineTrialCount + 1;
            int baseCount = Mathf.Max(1, CurrentTuning.ArenaShrineBaseEnemyCount);
            int maxCount = Mathf.Max(baseCount, CurrentTuning.ArenaShrineMaxEnemyCount);
            int targetCount = Mathf.Clamp(
                baseCount + Mathf.Max(0, trialNumber - 1) * Mathf.Max(0, CurrentTuning.ArenaShrineEnemyCountIncreasePerTrial) + Mathf.Max(0, RunEscalationLevel) / 3 + ResolveEndlessExplorationPressureBonus(),
                1,
                maxCount);
            int available = Mathf.Max(0, ResolveEnemyMaximumAlive() + Mathf.Max(0, CurrentTuning.ArenaShrineExtraAliveAllowance) - _enemies.Count);
            targetCount = Mathf.Min(targetCount, available);
            if (targetCount <= 0)
            {
                return 0;
            }

            float radius = Mathf.Max(2f, CurrentTuning.ArenaShrineSpawnRadius);
            Vector3 center = PlayerPosition + forward * Mathf.Max(3.5f, radius * 0.85f);
            int spawned = 0;
            for (int i = 0; i < targetCount; i++)
            {
                SurvivorsEnemyRole role = ResolveArenaShrineRole(i, trialNumber);
                SurvivorsEnemyActor enemy = SpawnGameplayEnemyOffscreen(
                    role,
                    _spawnSequence + i + trialNumber * 41 + 853,
                    radius,
                    radius + CurrentTuning.SpawnBandDepth,
                    "arena-shrine");
                if (enemy == null)
                {
                    continue;
                }

                _activeArenaShrineEnemies.Add(enemy);
                spawned++;
            }

            if (spawned <= 0)
            {
                return 0;
            }

            ArenaShrineTrialCount++;
            ArenaShrineEnemySpawnCount += spawned;
            LastArenaShrineFeedbackLabel = $"Arena Trial {ArenaShrineTrialCount}: shrine ring x{spawned}{ResolveEndlessExplorationLabelSuffix()}";
            LastRoamingCacheFeedbackLabel = LastArenaShrineFeedbackLabel;
            RecordStreakRewardFeedback(LastArenaShrineFeedbackLabel, new Color(1f, 0.78f, 0.35f));
            PlayFeedback(_bossPulse, center, Mathf.Clamp(28 + spawned * 3, 36, 82), _dangerClip);
            return spawned;
        }

        private SurvivorsEnemyRole ResolveArenaShrineRole(int index, int trialNumber)
        {
            int pressure = Mathf.Max(0, RunEscalationLevel) + Mathf.Max(0, trialNumber - 1);
            int seed = index + trialNumber * 3;
            if (pressure >= 5 && seed % 6 == 0)
            {
                return SurvivorsEnemyRole.Splitter;
            }

            if (pressure >= 3 && seed % 5 == 0)
            {
                return SurvivorsEnemyRole.Spitter;
            }

            if (seed % 4 == 0)
            {
                return SurvivorsEnemyRole.Bruiser;
            }

            return seed % 2 == 0 ? SurvivorsEnemyRole.Runner : SurvivorsEnemyRole.Swarm;
        }

        private void SpawnArenaShrineClearReward(Vector3 position)
        {
            int gemCount = Mathf.Max(1, CurrentTuning.ArenaShrineClearExperienceGemCount + ResolveEndlessExplorationGemBonus());
            float xpMultiplier = Mathf.Max(0.1f, CurrentTuning.ArenaShrineClearExperienceMultiplier);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * xpMultiplier * (1f + RunEscalationLevel * 0.1f) * ResolveEndlessExplorationExperienceMultiplier()));
            float radius = 0.95f + Mathf.Min(1.25f, gemCount * 0.09f);
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.3f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem) != null)
                {
                    spawnedExperience += xpPerGem;
                    ArenaShrineClearExperienceGemDropCount++;
                }
            }

            int shardAmount = Mathf.Max(0, CurrentTuning.ArenaShrineClearBloodShardAmount + ResolveEndlessExplorationShardBonus());
            bool spawnedBloodShard = false;
            if (shardAmount > 0 && SpawnPickup(SurvivorsPickupKind.BloodShard, position + new Vector3(-radius * 0.75f, 0f, radius * 0.45f), shardAmount) != null)
            {
                spawnedBloodShard = true;
                ArenaShrineClearBloodShardDropCount++;
            }

            bool surgeActivated = ActivateArenaShrineSurge();
            int pulseHitCount = TriggerArenaShrineSurgePulse(position);
            ArenaShrineClearRewardCount++;
            string label = $"Arena Trial Cleared: +{spawnedExperience} XP";
            if (spawnedBloodShard)
            {
                label += $" +{shardAmount} {CurrencyRewardLabel}";
            }

            label += $" + Shrine Surge ({pulseHitCount} hit)";
            if (surgeActivated)
            {
                label += $" {ArenaShrineSurgeRemainingSeconds:0.#}s";
            }

            label += ResolveEndlessExplorationLabelSuffix();
            LastArenaShrineClearFeedbackLabel = label;
            LastArenaShrineFeedbackLabel = label;
            LastRoamingCacheFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(1f, 0.86f, 0.42f));
            PlayFeedback(_levelUpPulse, position, Mathf.Clamp(34 + pulseHitCount * 5, 42, 92), _pickupClip);
        }

        private bool ActivateArenaShrineSurge()
        {
            float duration = Mathf.Max(0f, CurrentTuning.ArenaShrineSurgeDurationSeconds);
            if (duration <= 0f)
            {
                return false;
            }

            _arenaShrineSurgeTimer = Mathf.Max(_arenaShrineSurgeTimer, duration);
            ArenaShrineSurgeActivationCount++;
            return true;
        }

        private int TriggerArenaShrineSurgePulse(Vector3 position)
        {
            float radius = Mathf.Max(0f, CurrentTuning.ArenaShrineSurgePulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.ArenaShrineSurgePulseDamage * ResolveEndlessExplorationPulseMultiplier());
            if (radius <= 0f || damage <= 0f)
            {
                return 0;
            }

            int hitCount = 0;
            var targets = new List<SurvivorsEnemyActor>();
            CollectEnemiesWithinRadius(position, radius, targets);
            for (int i = 0; i < targets.Count; i++)
            {
                SurvivorsEnemyActor enemy = targets[i];
                if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                {
                    continue;
                }

                enemy.ApplyDamage(damage, "survivors.arena-shrine.surge");
                hitCount++;
            }

            ArenaShrineSurgePulseHitCount += hitCount;
            return hitCount;
        }

        private void UpdateArenaPresentation()
        {
            if (_playerObject == null)
            {
                return;
            }

            Vector3 player = PlayerPosition;
            Vector3 playerGround = new Vector3(player.x, 0f, player.z);
            if (_arenaFollowRoot != null)
            {
                _arenaFollowRoot.position = playerGround;
            }

            if (_arenaTiles.Count == 0)
            {
                return;
            }

            float anchorX = ResolveArenaPresentationAnchor(player.x);
            float anchorZ = ResolveArenaPresentationAnchor(player.z);
            int tileIndex = 0;
            for (int x = -InfiniteArenaGridRadius; x <= InfiniteArenaGridRadius; x++)
            {
                for (int z = -InfiniteArenaGridRadius; z <= InfiniteArenaGridRadius; z++)
                {
                    if (tileIndex >= _arenaTiles.Count)
                    {
                        return;
                    }

                    Transform tile = _arenaTiles[tileIndex++];
                    if (tile != null)
                    {
                        tile.position = new Vector3(anchorX + x * InfiniteArenaTileSize, -0.08f, anchorZ + z * InfiniteArenaTileSize);
                    }
                }
            }

            UpdateArenaLandmarks(anchorX, anchorZ);
            UpdateWaystoneCompassPresentation();
        }

        private void UpdateWaystoneCompassPresentation()
        {
            if (_waystoneCompassRoot == null)
            {
                return;
            }

            float distance = 0f;
            Vector3 delta = Vector3.zero;
            bool visible = CurrentTuning.WaystoneDiscoveryRadius > 0f &&
                TryResolveClosestArenaLandmark(ignoreDiscovered: true, out _, out distance, out delta);
            if (!visible || delta.sqrMagnitude <= 0.0001f)
            {
                _waystoneCompassRoot.gameObject.SetActive(false);
                return;
            }

            _waystoneCompassRoot.gameObject.SetActive(true);
            float headingDegrees = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            _waystoneCompassRoot.localRotation = Quaternion.Euler(0f, headingDegrees, 0f);

            float nearDistance = Mathf.Max(1f, CurrentTuning.WaystoneDiscoveryRadius * 3.5f);
            float proximityPulse = Mathf.Clamp01(1f - distance / nearDistance) * 0.16f;
            float timePulse = Mathf.Sin(RunTimeSeconds * 7.5f) * 0.035f;
            float scale = Mathf.Max(0.86f, 1f + proximityPulse + timePulse);
            _waystoneCompassRoot.localScale = new Vector3(scale, scale, scale);
        }

        private void UpdateArenaLandmarks(float anchorX, float anchorZ)
        {
            if (_arenaLandmarks.Count == 0)
            {
                return;
            }

            int anchorCellX = Mathf.FloorToInt(anchorX / InfiniteArenaTileSize);
            int anchorCellZ = Mathf.FloorToInt(anchorZ / InfiniteArenaTileSize);
            for (int i = 0; i < _arenaLandmarks.Count; i++)
            {
                Transform landmark = _arenaLandmarks[i];
                if (landmark == null)
                {
                    continue;
                }

                ResolveArenaLandmarkOffset(i, out int offsetX, out int offsetZ);
                int cellX = anchorCellX + offsetX;
                int cellZ = anchorCellZ + offsetZ;
                long key = ResolveArenaLandmarkKey(cellX, cellZ);
                if (i < _arenaLandmarkKeys.Count)
                {
                    _arenaLandmarkKeys[i] = key;
                }
                else
                {
                    _arenaLandmarkKeys.Add(key);
                }

                float jitterX = ResolveArenaLandmarkJitter(cellX, cellZ, i * 2 + 1) * InfiniteArenaLandmarkJitterRadius;
                float jitterZ = ResolveArenaLandmarkJitter(cellX, cellZ, i * 2 + 2) * InfiniteArenaLandmarkJitterRadius;
                landmark.position = new Vector3(
                    cellX * InfiniteArenaTileSize + InfiniteArenaTileSize * 0.5f + jitterX,
                    0.38f,
                    cellZ * InfiniteArenaTileSize + InfiniteArenaTileSize * 0.5f + jitterZ);
                landmark.rotation = Quaternion.Euler(0f, (cellX * 37f + cellZ * 19f + i * 31f) % 360f, 0f);
            }
        }

        private static float ResolveArenaPresentationAnchor(float value)
        {
            return Mathf.Floor((value + InfiniteArenaTileSize * 0.5f) / InfiniteArenaTileSize) * InfiniteArenaTileSize;
        }

        private void TickArenaWaystoneDiscoveries()
        {
            if (_arenaLandmarks.Count == 0 || CurrentTuning.WaystoneDiscoveryRadius <= 0f)
            {
                return;
            }

            Vector3 player = PlayerPosition;
            float radiusSquared = CurrentTuning.WaystoneDiscoveryRadius * CurrentTuning.WaystoneDiscoveryRadius;
            for (int i = 0; i < _arenaLandmarks.Count; i++)
            {
                Transform landmark = _arenaLandmarks[i];
                if (landmark == null || i >= _arenaLandmarkKeys.Count)
                {
                    continue;
                }

                long key = _arenaLandmarkKeys[i];
                if (_discoveredArenaWaystones.Contains(key))
                {
                    continue;
                }

                Vector3 delta = landmark.position - player;
                delta.y = 0f;
                if (delta.sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                _discoveredArenaWaystones.Add(key);
                SpawnWaystoneDiscoveryReward(landmark.position);
            }
        }

        private void SpawnWaystoneDiscoveryReward(Vector3 position)
        {
            int discoveryNumber = WaystoneDiscoveryCount + 1;
            bool focusActivated = ActivateWaystoneFocus();
            int gemCount = Mathf.Max(1, CurrentTuning.WaystoneExperienceGemCount + ResolveEndlessExplorationGemBonus());
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * (1.45f + RunEscalationLevel * 0.08f) * ResolveEndlessExplorationExperienceMultiplier()));
            int spawnedExperience = 0;
            float rewardRadius = 0.7f + Mathf.Min(0.8f, gemCount * 0.08f);
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.15f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * rewardRadius;
                if (SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem) != null)
                {
                    spawnedExperience += xpPerGem;
                    WaystoneExperienceGemDropCount++;
                }
            }

            bool spawnedBloodShard = false;
            if (ShouldTriggerRoamingCacheCadence(discoveryNumber, CurrentTuning.WaystoneBloodShardInterval))
            {
                int shardAmount = CurrentTuning.BloodShardPickupAmount + ResolveEndlessExplorationShardBonus();
                if (SpawnPickup(SurvivorsPickupKind.BloodShard, position + new Vector3(-rewardRadius, 0f, rewardRadius * 0.45f), shardAmount) != null)
                {
                    spawnedBloodShard = true;
                    WaystoneBloodShardDropCount++;
                }
            }

            int ambushSpawned = SpawnWaystoneDiscoveryAmbush(position, discoveryNumber);
            int chainExperience = TryActivateWaystoneChainSurge(discoveryNumber, position, xpPerGem, out int chainHitCount);
            WaystoneDiscoveryCount++;
            string label = $"Waystone Discovered: +{spawnedExperience} XP";
            if (spawnedBloodShard)
            {
                label += " + Shard";
            }

            if (focusActivated)
            {
                label += $" + Focus {WaystoneFocusRemainingSeconds:0.#}s";
            }

            if (ambushSpawned > 0)
            {
                label += $" + Ambush x{ambushSpawned}";
            }

            if (chainExperience > 0 || chainHitCount > 0)
            {
                label += $" + Waystone Chain (+{chainExperience} XP, {chainHitCount} hit)";
            }

            label += ResolveEndlessExplorationLabelSuffix();
            LastWaystoneDiscoveryFeedbackLabel = label;
            LastRoamingCacheFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(0.58f, 0.95f, 1f));
            PlayFeedback(_levelUpPulse, position, ambushSpawned > 0 ? 24 : 16, _pickupClip);
        }

        private bool ActivateWaystoneFocus()
        {
            float duration = Mathf.Max(0f, CurrentTuning.WaystoneFocusDurationSeconds);
            if (duration <= 0f)
            {
                return false;
            }

            _waystoneFocusTimer = Mathf.Max(_waystoneFocusTimer, duration);
            WaystoneFocusActivationCount++;
            return true;
        }

        private int TryActivateWaystoneChainSurge(int discoveryNumber, Vector3 position, int xpPerGem, out int pulseHitCount)
        {
            pulseHitCount = 0;
            if (!ShouldTriggerRoamingCacheCadence(discoveryNumber, CurrentTuning.WaystoneChainInterval))
            {
                return 0;
            }

            _waystoneChainSurgeTimer = Mathf.Max(0.1f, CurrentTuning.WaystoneChainDurationSeconds);
            WaystoneChainSurgeActivationCount++;

            int gemCount = Mathf.Max(0, CurrentTuning.WaystoneChainBonusGemCount + ResolveEndlessExplorationGemBonus());
            int spawnedExperience = 0;
            int bonusXpPerGem = Mathf.Max(1, Mathf.RoundToInt(xpPerGem * (2.35f + ResolveEndlessExplorationBonusTier() * 0.12f)));
            float rewardRadius = 0.85f + Mathf.Min(0.95f, gemCount * 0.09f);
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.4f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * rewardRadius;
                if (SpawnPickup(SurvivorsPickupKind.Experience, position + offset, bonusXpPerGem) != null)
                {
                    spawnedExperience += bonusXpPerGem;
                    WaystoneChainSurgeBonusExperienceGemDropCount++;
                }
            }

            pulseHitCount = TriggerWaystoneChainSurgePulse(position);
            LastWaystoneChainSurgeFeedbackLabel = $"Waystone Chain: {discoveryNumber} discoveries, +{spawnedExperience} XP, {pulseHitCount} enemies hit{ResolveEndlessExplorationLabelSuffix()}";
            return spawnedExperience;
        }

        private int TriggerWaystoneChainSurgePulse(Vector3 center)
        {
            float radius = Mathf.Max(0f, CurrentTuning.WaystoneChainPulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.WaystoneChainPulseDamage * ResolveEndlessExplorationPulseMultiplier());
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(center, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.waystone.chain-surge");
                    hitCount++;
                }
            }

            WaystoneChainSurgePulseHitCount += hitCount;
            PlayFeedback(_levelUpPulse, center, Mathf.Clamp(34 + hitCount * 5, 42, 86), _pickupClip);
            return hitCount;
        }

        private int SpawnWaystoneDiscoveryAmbush(Vector3 position, int discoveryNumber)
        {
            if (!ShouldTriggerRoamingCacheCadence(discoveryNumber, CurrentTuning.WaystoneAmbushInterval))
            {
                return 0;
            }

            int baseCount = Mathf.Max(0, CurrentTuning.WaystoneAmbushBaseEnemyCount);
            if (baseCount <= 0)
            {
                return 0;
            }

            int available = Mathf.Max(0, ResolveEnemyMaximumAlive() + Mathf.Max(0, CurrentTuning.WaystoneAmbushExtraAliveAllowance) - _enemies.Count);
            int targetCount = Mathf.Min(baseCount + Mathf.Max(0, RunEscalationLevel) / 4 + ResolveEndlessExplorationPressureBonus(), available);
            if (targetCount <= 0)
            {
                return 0;
            }

            int spawned = 0;
            float radius = Mathf.Max(1f, CurrentTuning.WaystoneAmbushRadius);
            for (int i = 0; i < targetCount; i++)
            {
                SurvivorsEnemyRole role = ResolveRoamingCacheAmbushRole(i, discoveryNumber + 2);
                if (SpawnGameplayEnemyOffscreen(
                    role,
                    _spawnSequence + i + discoveryNumber * 43 + 907,
                    radius,
                    radius + CurrentTuning.SpawnBandDepth,
                    "waystone-ambush") != null)
                {
                    spawned++;
                }
            }

            if (spawned > 0)
            {
                WaystoneAmbushCount++;
                WaystoneAmbushEnemySpawnCount += spawned;
            }

            return spawned;
        }

        private static void ResolveArenaLandmarkOffset(int index, out int offsetX, out int offsetZ)
        {
            switch (index % InfiniteArenaLandmarkCount)
            {
                case 0:
                    offsetX = -1;
                    offsetZ = -1;
                    return;
                case 1:
                    offsetX = 0;
                    offsetZ = -1;
                    return;
                case 2:
                    offsetX = 1;
                    offsetZ = -1;
                    return;
                case 3:
                    offsetX = -1;
                    offsetZ = 0;
                    return;
                case 4:
                    offsetX = 1;
                    offsetZ = 0;
                    return;
                case 5:
                    offsetX = -1;
                    offsetZ = 1;
                    return;
                case 6:
                    offsetX = 0;
                    offsetZ = 1;
                    return;
                default:
                    offsetX = 1;
                    offsetZ = 1;
                    return;
            }
        }

        private static float ResolveArenaLandmarkJitter(int cellX, int cellZ, int salt)
        {
            unchecked
            {
                int hash = cellX * 73856093 ^ cellZ * 19349663 ^ salt * 83492791;
                int positive = hash & 0x7fffffff;
                return (positive % 1001) / 1000f - 0.5f;
            }
        }

        private static long ResolveArenaLandmarkKey(int cellX, int cellZ)
        {
            return ((long)cellX << 32) ^ (uint)cellZ;
        }

        private Color ResolveArenaLandmarkColor(int index)
        {
            switch (index % 4)
            {
                case 0:
                    return ActiveUiTheme.GetArenaAccentColor(new Color(0.28f, 0.9f, 1f));
                case 1:
                    return ActiveUiTheme.GetBossThreatColor(new Color(0.95f, 0.38f, 0.56f));
                case 2:
                    return ActiveUiTheme.GetEliteThreatColor(new Color(0.98f, 0.82f, 0.24f));
                default:
                    return ActiveUiTheme.GetFeedbackAccentColor(new Color(0.48f, 0.92f, 0.42f));
            }
        }

        private void TickRunFlow()
        {
            if (_runFlow == null)
            {
                return;
            }

            _runFlow.Tick(RunTimeSeconds);
            while (_runFlow.TryConsumeTimedEliteSpawn(RunTimeSeconds, out SurvivorsEnemyRole timedEliteRole))
            {
                SpawnEnemy(Vector3.zero, explicitPosition: false, timedEliteRole, gameplaySpawn: true, spawnSource: "timed-elite");
            }

            if (_runFlow.TryConsumeMinibossSpawn(RunTimeSeconds))
            {
                SpawnEnemy(Vector3.zero, explicitPosition: false, SurvivorsEnemyRole.Miniboss, gameplaySpawn: true, spawnSource: "timed-miniboss");
            }

            if (_runFlow.TryConsumeBossSpawn(RunTimeSeconds))
            {
                SpawnEnemy(Vector3.zero, explicitPosition: false, SurvivorsEnemyRole.Boss, gameplaySpawn: true, spawnSource: "timed-boss");
            }

            if (_runFlow.TryConsumeSurvivalVictory(RunTimeSeconds))
            {
                EnterVictory();
            }

            TickEndlessThreatSpawns();
        }

        private void TickMajorThreatWarnings()
        {
            if (_runFlow == null || _runFlow.Definition == null)
            {
                return;
            }

            SurvivorsRunFlowDefinition definition = _runFlow.Definition;
            TryBeginRecurringMajorThreatWarning(ref _firstEliteWarningShown, ref _timedEliteWarningTargetTimeSeconds, _runFlow.NextEliteSpawnTimeSeconds, SurvivorsEnemyRole.Elite);
            TryBeginRecurringMajorThreatWarning(ref _firstDreadEliteWarningShown, ref _timedDreadEliteWarningTargetTimeSeconds, _runFlow.NextDreadEliteSpawnTimeSeconds, SurvivorsEnemyRole.DreadElite);
            TryBeginMajorThreatWarning(ref _minibossWarningShown, definition.MinibossSpawnTimeSeconds, SurvivorsEnemyRole.Miniboss);
            TryBeginMajorThreatWarning(ref _bossWarningShown, definition.BossSpawnTimeSeconds, SurvivorsEnemyRole.Boss);
            if (_victoryClearedThisRun && State == SurvivorsRunState.Playing)
            {
                TryBeginMajorThreatWarning(ref _endlessEliteWarningShown, _nextEndlessEliteSpawnTimeSeconds, ResolveNextEndlessEliteRole());
                TryBeginMajorThreatWarning(ref _endlessMinibossWarningShown, _nextEndlessMinibossSpawnTimeSeconds, SurvivorsEnemyRole.Miniboss);
                TryBeginMajorThreatWarning(ref _endlessBossWarningShown, _nextEndlessBossSpawnTimeSeconds, SurvivorsEnemyRole.Boss);
            }
        }

        private void TryBeginRecurringMajorThreatWarning(ref bool warningShown, ref float warningTargetTimeSeconds, float targetTimeSeconds, SurvivorsEnemyRole role)
        {
            if (targetTimeSeconds <= 0f)
            {
                warningShown = false;
                warningTargetTimeSeconds = 0f;
                return;
            }

            if (!Mathf.Approximately(warningTargetTimeSeconds, targetTimeSeconds))
            {
                warningShown = false;
                warningTargetTimeSeconds = targetTimeSeconds;
            }

            TryBeginMajorThreatWarning(ref warningShown, targetTimeSeconds, role);
        }

        private void TryBeginMajorThreatWarning(ref bool warningShown, float targetTimeSeconds, SurvivorsEnemyRole role)
        {
            float leadSeconds = Mathf.Max(0f, CurrentTuning.MajorThreatWarningLeadSeconds);
            if (warningShown || leadSeconds <= 0f || targetTimeSeconds <= 0f)
            {
                return;
            }

            float warningTime = Mathf.Max(0f, targetTimeSeconds - leadSeconds);
            if (RunTimeSeconds < warningTime || RunTimeSeconds >= targetTimeSeconds)
            {
                return;
            }

            warningShown = true;
            _majorThreatWarningLabel = ResolveMajorThreatWarningLabel(role);
            _majorThreatWarningTargetTimeSeconds = targetTimeSeconds;
            MajorThreatWarningCount++;
            RecordIncomingThreatTelegraph(PlayerPosition, role, _majorThreatWarningLabel, ResolveIncomingThreatTelegraphRadius(role), targetTimeSeconds - RunTimeSeconds);
            PlayFeedback(_bossPulse, PlayerPosition, role == SurvivorsEnemyRole.Boss ? 44 : 28, _dangerClip, role == SurvivorsEnemyRole.Boss ? AudioEventBossWarning : AudioEventEliteWarning, 0.35f);
        }

        private static string ResolveMajorThreatWarningLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.DreadElite:
                    return "DREAD ELITE INCOMING";
                case SurvivorsEnemyRole.Miniboss:
                    return "MINIBOSS INCOMING";
                case SurvivorsEnemyRole.Boss:
                    return "FINAL BOSS INCOMING";
                default:
                    return "ELITE INCOMING";
            }
        }

        private void ResetEndlessThreatSchedule()
        {
            _endlessEliteWarningShown = false;
            _endlessMinibossWarningShown = false;
            _endlessBossWarningShown = false;
            _nextEndlessEliteSpawnTimeSeconds = 0f;
            _nextEndlessMinibossSpawnTimeSeconds = 0f;
            _nextEndlessBossSpawnTimeSeconds = 0f;
            _endlessEliteSpawnSequence = 0;
        }

        private void ScheduleEndlessThreats(float startTimeSeconds)
        {
            _endlessEliteSpawnSequence = 0;
            ScheduleNextEndlessThreat(
                ref _nextEndlessEliteSpawnTimeSeconds,
                ref _endlessEliteWarningShown,
                startTimeSeconds,
                CurrentTuning.EndlessEliteSpawnIntervalSeconds);
            ScheduleNextEndlessThreat(
                ref _nextEndlessMinibossSpawnTimeSeconds,
                ref _endlessMinibossWarningShown,
                startTimeSeconds,
                CurrentTuning.EndlessMinibossSpawnIntervalSeconds);
            ScheduleNextEndlessThreat(
                ref _nextEndlessBossSpawnTimeSeconds,
                ref _endlessBossWarningShown,
                startTimeSeconds,
                CurrentTuning.EndlessBossSpawnIntervalSeconds);
        }

        private static void ScheduleNextEndlessThreat(
            ref float targetTimeSeconds,
            ref bool warningShown,
            float startTimeSeconds,
            float intervalSeconds)
        {
            warningShown = false;
            targetTimeSeconds = intervalSeconds <= 0f
                ? 0f
                : Mathf.Max(0f, startTimeSeconds) + Mathf.Max(0.1f, intervalSeconds);
        }

        private void TickEndlessThreatSpawns()
        {
            if (!_victoryClearedThisRun || State != SurvivorsRunState.Playing)
            {
                return;
            }

            SurvivorsEnemyRole eliteRole = ResolveNextEndlessEliteRole();
            if (TrySpawnEndlessThreat(eliteRole, _nextEndlessEliteSpawnTimeSeconds))
            {
                _endlessEliteSpawnSequence++;
                ScheduleNextEndlessThreat(
                    ref _nextEndlessEliteSpawnTimeSeconds,
                    ref _endlessEliteWarningShown,
                    RunTimeSeconds,
                    CurrentTuning.EndlessEliteSpawnIntervalSeconds);
            }

            if (TrySpawnEndlessThreat(SurvivorsEnemyRole.Miniboss, _nextEndlessMinibossSpawnTimeSeconds))
            {
                ScheduleNextEndlessThreat(
                    ref _nextEndlessMinibossSpawnTimeSeconds,
                    ref _endlessMinibossWarningShown,
                    RunTimeSeconds,
                    CurrentTuning.EndlessMinibossSpawnIntervalSeconds);
            }

            if (TrySpawnEndlessThreat(SurvivorsEnemyRole.Boss, _nextEndlessBossSpawnTimeSeconds))
            {
                ScheduleNextEndlessThreat(
                    ref _nextEndlessBossSpawnTimeSeconds,
                    ref _endlessBossWarningShown,
                    RunTimeSeconds,
                    CurrentTuning.EndlessBossSpawnIntervalSeconds);
            }
        }

        private bool TrySpawnEndlessThreat(SurvivorsEnemyRole role, float targetTimeSeconds)
        {
            if (targetTimeSeconds <= 0f || RunTimeSeconds < targetTimeSeconds)
            {
                return false;
            }

            if (SpawnEnemy(Vector3.zero, explicitPosition: false, role, gameplaySpawn: true, spawnSource: "endless-threat") == null)
            {
                return false;
            }

            EndlessThreatSpawnCount++;
            return true;
        }

        private SurvivorsEnemyRole ResolveNextEndlessEliteRole()
        {
            return (_endlessEliteSpawnSequence % 2) == 0
                ? SurvivorsEnemyRole.Elite
                : SurvivorsEnemyRole.DreadElite;
        }

        private void TryActivateEndlessSurge(SurvivorsEnemyRole role, Vector3 position, int baseExperienceReward)
        {
            if (!_victoryClearedThisRun || !IsMajorRewardRole(role))
            {
                return;
            }

            float duration = Mathf.Max(0.1f, CurrentTuning.EndlessSurgeDurationSeconds);
            _endlessSurgeTimer = Mathf.Max(_endlessSurgeTimer, duration);
            EndlessSurgeActivationCount++;
            EndlessSurgeTier = Mathf.Max(1, EndlessSurgeTier + 1);

            int threatTier = ResolveEndlessSurgeThreatTier(role);
            int gemCount = Mathf.Max(0, CurrentTuning.EndlessSurgeExperienceGemCount + threatTier - 1 + Mathf.Min(6, EndlessSurgeTier / 2));
            int xpPerGem = Mathf.Max(
                1,
                Mathf.RoundToInt(Mathf.Max(1, baseExperienceReward) * Mathf.Max(0.1f, CurrentTuning.EndlessSurgeExperienceMultiplier) * (0.35f + threatTier * 0.25f)));
            int spawnedExperience = 0;
            float rewardRadius = 1.15f + Mathf.Min(1.3f, gemCount * 0.08f);
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.21f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                float laneOffset = 0.14f * (i % 3);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (rewardRadius + laneOffset);
                if (SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem) != null)
                {
                    spawnedExperience += xpPerGem;
                    EndlessSurgeExperienceGemDropCount++;
                }
            }

            int shardAmount = Mathf.Max(1, CurrentTuning.EndlessSurgeBloodShardAmount + threatTier - 1 + EndlessSurgeTier / 3);
            if (SpawnPickup(SurvivorsPickupKind.BloodShard, position + new Vector3(-rewardRadius * 0.55f, 0f, rewardRadius * 0.38f), shardAmount) != null)
            {
                EndlessSurgeBloodShardDropCount++;
            }

            int pulseHitCount = TriggerEndlessSurgePulse(position);
            string label = $"Endless Surge T{EndlessSurgeTier}: {ResolveEndlessSurgeThreatLabel(role)} cleared, +{spawnedExperience} XP, +{shardAmount} {CurrencyRewardLabel}, {pulseHitCount} hit";
            LastEndlessSurgeFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(0.52f, 0.84f, 1f));
            PlayFeedback(_levelUpPulse, position, Mathf.Clamp(32 + pulseHitCount * 5 + threatTier * 6, 42, 96), _pickupClip);
        }

        private int TriggerEndlessSurgePulse(Vector3 position)
        {
            float radius = Mathf.Max(0f, CurrentTuning.EndlessSurgePulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.EndlessSurgePulseDamage * ResolveEndlessSurgeIntensityMultiplier());
            if (radius <= 0f || damage <= 0f)
            {
                return 0;
            }

            int hitCount = 0;
            var targets = new List<SurvivorsEnemyActor>();
            CollectEnemiesWithinRadius(position, radius, targets);
            for (int i = 0; i < targets.Count; i++)
            {
                SurvivorsEnemyActor enemy = targets[i];
                if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                {
                    continue;
                }

                enemy.ApplyDamage(damage, "survivors.endless.surge");
                hitCount++;
            }

            EndlessSurgePulseHitCount += hitCount;
            return hitCount;
        }

        private float ResolveEndlessSurgeIntensityMultiplier()
        {
            return Mathf.Clamp(1f + Mathf.Max(0, EndlessSurgeTier - 1) * 0.16f, 1f, 2.25f);
        }

        private int ResolveEndlessExplorationBonusTier()
        {
            if (!_victoryClearedThisRun)
            {
                return 0;
            }

            return Mathf.Clamp(Mathf.Max(1, EndlessSurgeTier), 1, 12);
        }

        private float ResolveEndlessExplorationExperienceMultiplier()
        {
            int tier = ResolveEndlessExplorationBonusTier();
            return tier <= 0 ? 1f : Mathf.Clamp(1f + tier * 0.12f, 1f, 2.5f);
        }

        private float ResolveEndlessExplorationPulseMultiplier()
        {
            int tier = ResolveEndlessExplorationBonusTier();
            return tier <= 0 ? 1f : Mathf.Clamp(1f + tier * 0.08f, 1f, 2f);
        }

        private int ResolveEndlessExplorationGemBonus()
        {
            int tier = ResolveEndlessExplorationBonusTier();
            return tier <= 0 ? 0 : Mathf.Min(6, (tier + 1) / 2);
        }

        private int ResolveEndlessExplorationPressureBonus()
        {
            int tier = ResolveEndlessExplorationBonusTier();
            return tier <= 0 ? 0 : Mathf.Min(8, 1 + tier / 2);
        }

        private int ResolveEndlessExplorationShardBonus()
        {
            int tier = ResolveEndlessExplorationBonusTier();
            return tier <= 1 ? 0 : Mathf.Min(4, 1 + tier / 3);
        }

        private string ResolveEndlessExplorationLabelSuffix()
        {
            int tier = ResolveEndlessExplorationBonusTier();
            return tier <= 0 ? string.Empty : $" + Endless T{tier}";
        }

        private static int ResolveEndlessSurgeThreatTier(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return 3;
                case SurvivorsEnemyRole.Miniboss:
                case SurvivorsEnemyRole.DreadElite:
                    return 2;
                default:
                    return 1;
            }
        }

        private static string ResolveEndlessSurgeThreatLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return "Endless Boss";
                case SurvivorsEnemyRole.Miniboss:
                    return "Endless Miniboss";
                case SurvivorsEnemyRole.DreadElite:
                    return "Endless Dread";
                default:
                    return "Endless Elite";
            }
        }

        private void ResetHordeRushSchedule()
        {
            _hordeRushSequence = 0;
            _hordeRushWarningShown = false;
            _hordeRushWarningLabel = string.Empty;
            _hordeRushWarningTargetTimeSeconds = 0f;
            ScheduleNextHordeRush(0f, firstRush: true);
        }

        private void EnsureFutureHordeRushScheduled()
        {
            if (_nextHordeRushTimeSeconds <= RunTimeSeconds)
            {
                ScheduleNextHordeRush(RunTimeSeconds, firstRush: false);
            }
        }

        private void ScheduleNextHordeRush(float startTimeSeconds, bool firstRush)
        {
            _hordeRushWarningShown = false;
            _hordeRushWarningLabel = string.Empty;
            _hordeRushWarningTargetTimeSeconds = 0f;
            float interval = firstRush
                ? CurrentTuning.HordeRushFirstTimeSeconds
                : CurrentTuning.HordeRushIntervalSeconds;
            _nextHordeRushTimeSeconds = interval <= 0f
                ? 0f
                : Mathf.Max(0f, startTimeSeconds) + Mathf.Max(0.1f, interval);
        }

        private void TickHordeRushEvents()
        {
            if (_nextHordeRushTimeSeconds <= 0f)
            {
                return;
            }

            TryBeginHordeRushWarning();
            if (RunTimeSeconds < _nextHordeRushTimeSeconds)
            {
                return;
            }

            int spawned = SpawnHordeRushBurst();
            _hordeRushSequence++;
            ScheduleNextHordeRush(RunTimeSeconds, firstRush: false);
            if (spawned <= 0)
            {
                return;
            }

            HordeRushSpawnCount++;
            HordeRushEnemySpawnCount += spawned;
            LastHordeRushFeedbackLabel = $"Horde Rush {HordeRushSpawnCount}: {spawned} enemies";
            RecordStreakRewardFeedback(LastHordeRushFeedbackLabel, new Color(1f, 0.42f, 0.18f));
            PlayFeedback(_bossPulse, PlayerPosition, 28, _dangerClip);
        }

        private void TryBeginHordeRushWarning()
        {
            float leadSeconds = Mathf.Max(0f, CurrentTuning.HordeRushWarningLeadSeconds);
            if (_hordeRushWarningShown || leadSeconds <= 0f || _nextHordeRushTimeSeconds <= 0f)
            {
                return;
            }

            float warningTime = Mathf.Max(0f, _nextHordeRushTimeSeconds - leadSeconds);
            if (RunTimeSeconds < warningTime || RunTimeSeconds >= _nextHordeRushTimeSeconds)
            {
                return;
            }

            _hordeRushWarningShown = true;
            _hordeRushWarningLabel = "HORDE RUSH INCOMING";
            _hordeRushWarningTargetTimeSeconds = _nextHordeRushTimeSeconds;
            HordeRushWarningCount++;
            LastHordeRushFeedbackLabel = _hordeRushWarningLabel;
            RecordIncomingThreatTelegraph(PlayerPosition, SurvivorsEnemyRole.Runner, _hordeRushWarningLabel, Mathf.Max(3f, CurrentTuning.HordeRushSpawnRadius), _nextHordeRushTimeSeconds - RunTimeSeconds);
            PlayFeedback(_bossPulse, PlayerPosition, 22, _dangerClip);
        }

        private int SpawnHordeRushBurst()
        {
            int available = Mathf.Max(0, ResolveEnemyMaximumAlive() + Mathf.Max(0, CurrentTuning.HordeRushExtraAliveAllowance) - _enemies.Count);
            if (available <= 0)
            {
                return 0;
            }

            int targetCount = Mathf.Min(ResolveHordeRushEnemyCount(), available);
            float radius = Mathf.Max(3f, CurrentTuning.HordeRushSpawnRadius);
            int spawned = 0;
            for (int i = 0; i < targetCount; i++)
            {
                float laneRadius = radius + ((i & 1) == 0 ? 0f : 1.35f);
                SurvivorsEnemyRole role = ResolveHordeRushRole(i);
                SurvivorsEnemyActor enemy = SpawnGameplayEnemyOffscreen(
                    role,
                    _spawnSequence + i + _hordeRushSequence * 47 + 947,
                    laneRadius,
                    laneRadius + CurrentTuning.SpawnBandDepth,
                    "horde-rush");
                if (enemy != null)
                {
                    _activeHordeRushEnemies.Add(enemy);
                    spawned++;
                }
            }

            return spawned;
        }

        private void SpawnHordeRushClearReward(Vector3 position)
        {
            int gemCount = Mathf.Max(1, CurrentTuning.HordeRushClearExperienceGemCount);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(
                CurrentTuning.EnemyExperienceReward *
                Mathf.Max(0.1f, CurrentTuning.HordeRushClearExperienceMultiplier) *
                (1f + RunEscalationLevel * 0.08f)));
            float radius = 0.85f + Mathf.Min(1.1f, gemCount * 0.08f);
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.1f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem) != null)
                {
                    spawnedExperience += xpPerGem;
                    HordeRushClearExperienceGemDropCount++;
                }
            }

            int specialDropCount = 0;
            int nextClearNumber = HordeRushClearRewardCount + 1;
            if (ShouldDropHordeRushSpecial(nextClearNumber, CurrentTuning.HordeRushClearMagnetEveryRush))
            {
                Vector3 magnetPosition = position + new Vector3(radius * 0.7f, 0f, -radius * 0.35f);
                if (SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1) != null)
                {
                    specialDropCount++;
                }
            }

            if (ShouldDropHordeRushSpecial(nextClearNumber, CurrentTuning.HordeRushClearBloodShardEveryRush))
            {
                int shardAmount = Mathf.Max(1, CurrentTuning.BloodShardPickupAmount);
                Vector3 shardPosition = position + new Vector3(-radius * 0.55f, 0f, radius * 0.48f);
                if (SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount) != null)
                {
                    specialDropCount++;
                }
            }

            if (spawnedExperience <= 0 && specialDropCount <= 0)
            {
                return;
            }

            HordeRushClearRewardCount++;
            HordeRushClearSpecialDropCount += specialDropCount;
            int pulseHitCount = TriggerHordeRushClearPulse(position);
            bool surgeActivated = ActivateHordeRushClearSurge();
            string specialLabel = specialDropCount > 0 ? $" + {specialDropCount} special" : string.Empty;
            string pulseLabel = pulseHitCount > 0 ? $" + Breaker Pulse ({pulseHitCount} hit)" : string.Empty;
            string surgeLabel = surgeActivated ? $" + Breaker Surge {HordeRushClearSurgeRemainingSeconds:0.#}s" : string.Empty;
            string label = $"Horde Rush Cleared: +{spawnedExperience} XP{specialLabel}{pulseLabel}{surgeLabel}";
            LastHordeRushClearFeedbackLabel = label;
            LastHordeRushFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(1f, 0.74f, 0.24f));
            PlayFeedback(_levelUpPulse, position, Mathf.Clamp(24 + pulseHitCount * 5, 28, 78), _pickupClip);
        }

        private bool ActivateHordeRushClearSurge()
        {
            float duration = Mathf.Max(0f, CurrentTuning.HordeRushClearSurgeDurationSeconds);
            if (duration <= 0f)
            {
                return false;
            }

            _hordeRushClearSurgeTimer = Mathf.Max(_hordeRushClearSurgeTimer, duration);
            HordeRushClearSurgeActivationCount++;
            return true;
        }

        private int TriggerHordeRushClearPulse(Vector3 position)
        {
            float radius = Mathf.Max(0f, CurrentTuning.HordeRushClearPulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.HordeRushClearPulseDamage);
            if (radius <= 0f || damage <= 0f)
            {
                return 0;
            }

            int hitCount = 0;
            var targets = new List<SurvivorsEnemyActor>();
            CollectEnemiesWithinRadius(position, radius, targets);
            for (int i = 0; i < targets.Count; i++)
            {
                SurvivorsEnemyActor enemy = targets[i];
                if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                {
                    continue;
                }

                enemy.ApplyDamage(damage, "survivors.horde-rush.clear-pulse");
                hitCount++;
            }

            HordeRushClearPulseCount++;
            HordeRushClearPulseHitCount += hitCount;
            LastHordeRushClearPulseFeedbackLabel = $"Breaker Pulse: {hitCount} enemies hit";
            return hitCount;
        }

        private static bool ShouldDropHordeRushSpecial(int clearNumber, int cadence)
        {
            return cadence > 0 && clearNumber > 0 && clearNumber % cadence == 0;
        }

        private int ResolveHordeRushEnemyCount()
        {
            int baseCount = Mathf.Max(1, CurrentTuning.HordeRushBaseEnemyCount);
            int increase = Mathf.Max(0, CurrentTuning.HordeRushEnemyCountIncreasePerRush);
            int maxCount = Mathf.Max(baseCount, CurrentTuning.HordeRushMaxEnemyCount);
            int escalationBonus = Mathf.Max(0, RunEscalationLevel);
            return Mathf.Clamp(baseCount + _hordeRushSequence * increase + escalationBonus, 1, maxCount);
        }

        private SurvivorsEnemyRole ResolveHordeRushRole(int index)
        {
            if (RunTimeSeconds >= 150f && index % 11 == 0)
            {
                return SurvivorsEnemyRole.Splitter;
            }

            if (RunTimeSeconds >= 110f && index % 7 == 0)
            {
                return SurvivorsEnemyRole.Bruiser;
            }

            if (RunTimeSeconds >= 95f && index % 5 == 0)
            {
                return SurvivorsEnemyRole.Spitter;
            }

            if (index % 3 == 0)
            {
                return SurvivorsEnemyRole.Runner;
            }

            return SurvivorsEnemyRole.Swarm;
        }

        private void EnterVictory()
        {
            if (State == SurvivorsRunState.Victory || State == SurvivorsRunState.GameOver)
            {
                return;
            }

            if (_runFlow != null)
            {
                _runFlow.TryConsumeBossVictory();
            }

            GrantRunRewards(victory: true);
            ClearRewardDrafts();
            _victoryClearedThisRun = true;
            State = SurvivorsRunState.Victory;
            PlayFeedback(_levelUpPulse, PlayerPosition, 42, _levelUpClip, AudioEventVictory, 0.5f);
        }

        private float ResolveEnemySpawnIntervalSeconds()
        {
            float interval = _runFlow == null
                ? Mathf.Max(0.05f, CurrentTuning.EnemySpawnIntervalSeconds)
                : _runFlow.ResolveSpawnInterval(CurrentTuning.EnemySpawnIntervalSeconds);
            return ResolveEndlessSpawnInterval(interval);
        }

        private SurvivorsRunFlowDefinition CreateRunFlowDefinition(SurvivorsTemplateTuning resolved)
        {
            if (_authoredContent != null)
            {
                _usingAuthoredRunFlow = true;
                return _authoredContent.CreateRunFlowDefinition(resolved);
            }

            _usingAuthoredRunFlow = false;
            return BasicSurvivorsGame.CreateRunFlowDefinition(resolved);
        }

        private int ResolveEnemyMaximumAlive()
        {
            int maximumAlive = _runFlow == null
                ? Mathf.Max(1, CurrentTuning.EnemyMaximumAlive)
                : _runFlow.ResolveMaximumAlive(CurrentTuning.EnemyMaximumAlive);
            return _victoryClearedThisRun ? maximumAlive + EndlessEnemyAliveBonus : maximumAlive;
        }

        private int ResolveEnemySpawnPackSize()
        {
            int baseCount = Mathf.Max(1, CurrentTuning.EnemySpawnPackBaseCount);
            int maxCount = Mathf.Max(baseCount, CurrentTuning.EnemySpawnPackMaxCount);
            int escalationStep = Mathf.Max(1, CurrentTuning.EnemySpawnPackIncreaseEveryEscalations);
            int escalationBonus = Mathf.Max(0, RunEscalationLevel) / escalationStep;
            return Mathf.Clamp(baseCount + escalationBonus, 1, maxCount);
        }

        private SurvivorsEnemyProfile ResolveEnemyProfile(SurvivorsEnemyRole role)
        {
            if (_runFlow != null && _runFlow.Definition != null)
            {
                if (role == SurvivorsEnemyRole.Miniboss)
                {
                    return _runFlow.Definition.Miniboss;
                }

                if (role == SurvivorsEnemyRole.Boss)
                {
                    return _runFlow.Definition.Boss;
                }

                return _runFlow.ResolveSwarmProfile(CurrentTuning, role);
            }

            return BasicSurvivorsGame.CreateEnemyProfile(role, CurrentTuning);
        }

        private static WorldSpawnableId ResolveEnemySpawnableId(SurvivorsEnemyRole role)
        {
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return BasicSurvivorsGame.MinibossEnemySpawnableId;
            }

            if (role == SurvivorsEnemyRole.Boss)
            {
                return BasicSurvivorsGame.BossEnemySpawnableId;
            }

            return BasicSurvivorsGame.SwarmEnemySpawnableId;
        }

        private static string ResolveEnemyGroupId(SurvivorsEnemyRole role)
        {
            if (role == SurvivorsEnemyRole.Runner)
            {
                return "group.survivors.runners";
            }

            if (role == SurvivorsEnemyRole.Bruiser)
            {
                return "group.survivors.bruisers";
            }

            if (role == SurvivorsEnemyRole.Spitter)
            {
                return "group.survivors.spitters";
            }

            if (role == SurvivorsEnemyRole.Splitter)
            {
                return "group.survivors.splitters";
            }

            if (role == SurvivorsEnemyRole.Summoner)
            {
                return "group.survivors.summoners";
            }

            if (role == SurvivorsEnemyRole.Elite)
            {
                return "group.survivors.elites";
            }

            if (role == SurvivorsEnemyRole.DreadElite)
            {
                return "group.survivors.dread-elites";
            }

            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return "group.survivors.miniboss";
            }

            if (role == SurvivorsEnemyRole.Boss)
            {
                return "group.survivors.boss";
            }

            return "group.survivors.opening-swarm";
        }

        private void TickEnemySpawning(float deltaTime)
        {
            _enemySpawnTimer -= deltaTime;
            int maximumAlive = ResolveEnemyMaximumAlive();
            if (_enemySpawnTimer > 0f || _enemies.Count >= maximumAlive)
            {
                return;
            }

            int packCount = Mathf.Min(ResolveEnemySpawnPackSize(), maximumAlive - _enemies.Count);
            for (int i = 0; i < packCount; i++)
            {
                SurvivorsEnemyRole role = _runFlow == null
                    ? SurvivorsEnemyRole.Swarm
                    : _runFlow.ResolveNextSwarmRole(RunTimeSeconds, _spawnSequence + 1 + i);
                if (SpawnEnemy(Vector3.zero, explicitPosition: false, role, gameplaySpawn: true, spawnSource: "normal-pack") == null)
                {
                    break;
                }
            }

            _enemySpawnTimer = ResolveEnemySpawnIntervalSeconds();
        }

        private SurvivorsEnemyActor SpawnGameplayEnemyOffscreen(
            SurvivorsEnemyRole role,
            long seed,
            float minimumDistance,
            float maximumDistance,
            string spawnSource)
        {
            Vector3 position = ResolveSafeOffscreenPosition(
                PlayerPosition,
                ResolveGameplaySpawnMinimumDistance(role, minimumDistance),
                ResolveGameplaySpawnMaximumDistance(role, minimumDistance, maximumDistance),
                seed,
                ResolveOffscreenSpawnPadding(role, spawnSource),
                CurrentTuning.SpawnBandDepth);
            return SpawnEnemy(position, explicitPosition: true, role, gameplaySpawn: true, spawnSource: spawnSource);
        }

        private SurvivorsEnemyActor SpawnEnemy(Vector3 position, bool explicitPosition, SurvivorsEnemyRole role, bool gameplaySpawn = false, string spawnSource = null)
        {
            long sequence = ++_spawnSequence;
            bool trackSpawnSafety = gameplaySpawn || !explicitPosition;
            if (!explicitPosition)
            {
                position = ResolveSafeOffscreenPosition(
                    PlayerPosition,
                    ResolveGameplaySpawnMinimumDistance(role, CurrentTuning.EnemySpawnRadius),
                    ResolveGameplaySpawnMaximumDistance(role, CurrentTuning.EnemySpawnRadius, CurrentTuning.EnemySpawnRadius + CurrentTuning.SpawnBandDepth),
                    sequence,
                    ResolveOffscreenSpawnPadding(role, spawnSource),
                    CurrentTuning.SpawnBandDepth);
                explicitPosition = true;
            }

            WorldSpawnChannelId channel = explicitPosition ? BasicSurvivorsGame.ExplicitSpawnChannelId : BasicSurvivorsGame.RadialSpawnChannelId;
            if (explicitPosition)
            {
                _poseResolver.RegisterExplicitPose(sequence, position);
            }

            SurvivorsEnemyProfile profile = ResolveEnemyProfile(role);
            SpawnResult result = _spawnService.Spawn(new WorldSpawnRequest(
                ResolveEnemySpawnableId(role),
                channel,
                sequence,
                new WorldSpawnRequestContext("SurvivorsTemplate", waveId: "wave.survivors.opening-ring", groupId: ResolveEnemyGroupId(role))));
            if (!result.Succeeded || result.Instance == null)
            {
                return null;
            }

            SurvivorsEnemyActor enemy = result.Instance.GetComponent<SurvivorsEnemyActor>();
            enemy.Initialize(this, profile);
            _enemies.Add(enemy);
            SpawnedCount++;
            if (trackSpawnSafety)
            {
                RecordGameplaySpawnSafety(role, enemy.transform.position, spawnSource);
            }

            if (role == SurvivorsEnemyRole.Miniboss)
            {
                RecordMetricTime(ref _firstMinibossSpawnTimeSeconds);
                MinibossSpawnCount++;
                PlayFeedback(_bossPulse, enemy.transform.position, 42, _bossClip);
            }
            else if (role == SurvivorsEnemyRole.Boss)
            {
                RecordMetricTime(ref _firstBossSpawnTimeSeconds);
                BossSpawnCount++;
                PlayFeedback(_bossPulse, enemy.transform.position, 58, _bossClip);
            }
            else if (IsEliteRole(role))
            {
                RecordMetricTime(ref _firstEliteSpawnTimeSeconds);
                PlayFeedback(_bossPulse, enemy.transform.position, 30, _bossClip);
            }
            else
            {
                PlayFeedback(_spawnPulse, enemy.transform.position, 10, _spawnClip);
            }

            return enemy;
        }

        private void SpawnSplitterChildren(Vector3 position, string splitterName)
        {
            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                return;
            }

            int childCount = Mathf.Clamp(CurrentTuning.SplitterChildCount, 1, 8);
            float radius = CurrentTuning.SplitterChildSpawnRadius > 0f
                ? CurrentTuning.SplitterChildSpawnRadius
                : Mathf.Max(0.65f, CurrentTuning.EnemyRadius * 1.5f);
            int spawned = 0;
            for (int index = 0; index < childCount; index++)
            {
                if (SpawnGameplayEnemyOffscreen(
                    SurvivorsEnemyRole.Swarm,
                    _spawnSequence + index + SplitterChildSpawnCount * 53 + 991,
                    radius,
                    radius + CurrentTuning.SpawnBandDepth,
                    "splitter-children") != null)
                {
                    SplitterChildSpawnCount++;
                    spawned++;
                }
            }

            if (spawned > 0)
            {
                RecordSplitterSplitFeedback(splitterName, position, spawned);
            }
        }

        private void RecordSplitterSplitFeedback(string splitterName, Vector3 position, int spawned)
        {
            string name = string.IsNullOrWhiteSpace(splitterName) ? "Splitter" : splitterName;
            LastSplitterSplitFeedbackLabel = $"{name}: +{spawned} fragments";
            SplitterSplitFeedbackCount++;
            RecordStreakRewardFeedback(LastSplitterSplitFeedbackLabel, new Color(0.78f, 0.58f, 1f));
            PlayFeedback(_spawnPulse, position, Mathf.Clamp(16 + spawned * 5, 24, 64), _spawnClip);
        }

        internal int SpawnSummonerSupport(SurvivorsEnemyActor summoner)
        {
            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory || summoner == null || !summoner.IsAlive)
            {
                return 0;
            }

            int requested = Mathf.Max(0, CurrentTuning.SummonerSupportCount);
            int available = Mathf.Max(
                0,
                ResolveEnemyMaximumAlive() + Mathf.Max(0, CurrentTuning.SummonerSupportExtraAliveAllowance) - _enemies.Count);
            int count = Mathf.Min(requested, available);
            if (count <= 0)
            {
                return 0;
            }

            Vector3 center = summoner.transform.position;
            float radius = Mathf.Max(summoner.Radius + 0.85f, CurrentTuning.SummonerSupportRadius);
            int spawned = 0;
            int sequenceOffset = SummonerSupportSpawnCount;
            for (int index = 0; index < count; index++)
            {
                SurvivorsEnemyRole role = ResolveSummonerSupportRole(sequenceOffset + index);
                if (SpawnGameplayEnemyOffscreen(
                    role,
                    _spawnSequence + index + sequenceOffset * 59 + 1031,
                    radius,
                    radius + CurrentTuning.SpawnBandDepth,
                    "summoner-support") != null)
                {
                    spawned++;
                }
            }

            SummonerSupportSpawnCount += spawned;
            if (spawned > 0)
            {
                RecordSummonerSupportFeedback(summoner, spawned);
            }

            return spawned;
        }

        private void RecordSummonerSupportFeedback(SurvivorsEnemyActor summoner, int spawned)
        {
            if (summoner == null || spawned <= 0)
            {
                return;
            }

            string name = string.IsNullOrWhiteSpace(summoner.DisplayName) ? "Rift Caller" : summoner.DisplayName;
            LastSummonerSupportFeedbackLabel = $"{name}: +{spawned} support";
            SummonerSupportFeedbackCount++;
            RecordStreakRewardFeedback(LastSummonerSupportFeedbackLabel, new Color(0.56f, 0.72f, 1f));
            PlayFeedback(_spawnPulse, summoner.transform.position, Mathf.Clamp(18 + spawned * 6, 24, 60), _spawnClip);
        }

        private static SurvivorsEnemyRole ResolveSummonerSupportRole(int sequence)
        {
            return sequence % 4 == 3 ? SurvivorsEnemyRole.Runner : SurvivorsEnemyRole.Swarm;
        }

        private SurvivorsPickupActor SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount)
        {
            long sequence = ++_spawnSequence;
            WorldSpawnableId spawnable = ResolvePickupSpawnableId(kind);
            _poseResolver.RegisterExplicitPose(sequence, position);
            SpawnResult result = _spawnService.Spawn(new WorldSpawnRequest(
                spawnable,
                BasicSurvivorsGame.ExplicitSpawnChannelId,
                sequence,
                new WorldSpawnRequestContext("SurvivorsTemplate", groupId: kind.ToString())));
            if (!result.Succeeded || result.Instance == null)
            {
                return null;
            }

            SurvivorsPickupActor pickup = result.Instance.GetComponent<SurvivorsPickupActor>();
            pickup.Initialize(this, kind, Mathf.Max(1, amount), CurrentPickupAttractRange, CurrentPickupAttractionSpeed, CurrentTuning.PickupCollectRadius);
            _pickups.Add(pickup);
            return pickup;
        }

        private static WorldSpawnableId ResolvePickupSpawnableId(SurvivorsPickupKind kind)
        {
            switch (kind)
            {
                case SurvivorsPickupKind.Magnet:
                    return BasicSurvivorsGame.MagnetPickupSpawnableId;
                case SurvivorsPickupKind.Health:
                    return BasicSurvivorsGame.HealthPickupSpawnableId;
                case SurvivorsPickupKind.BloodShard:
                    return BasicSurvivorsGame.BloodShardPickupSpawnableId;
                default:
                    return BasicSurvivorsGame.ExperiencePickupSpawnableId;
            }
        }

        private void TickWeapon(float deltaTime)
        {
            _weaponLoadout?.Tick(deltaTime);
        }

        private void TickEnemies(float deltaTime)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                TryUpdateEnemyLeash(enemy, deltaTime);
                enemy.Simulate(deltaTime);
            }
        }

        private void TryUpdateEnemyLeash(SurvivorsEnemyActor enemy, float deltaTime)
        {
            if (enemy == null || !enemy.IsAlive || !enemy.CanLeash)
            {
                return;
            }

            Vector3 playerPosition = PlayerPosition;
            Vector3 offset = enemy.transform.position - playerPosition;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (IsMajorRewardRole(enemy.Role))
            {
                TryUpdateMajorThreatLeash(enemy, playerPosition, distance, deltaTime);
            }
            else
            {
                TryUpdateNormalEnemyLeash(enemy, playerPosition, distance, deltaTime);
            }
        }

        private void TryUpdateNormalEnemyLeash(SurvivorsEnemyActor enemy, Vector3 playerPosition, float distance, float deltaTime)
        {
            if (!enemy.CanRecycle)
            {
                return;
            }

            float softRadius = Mathf.Max(0f, CurrentTuning.EnemySoftLeashRadius);
            float hardRadius = Mathf.Max(softRadius + 0.1f, CurrentTuning.EnemyHardRecycleRadius);
            if (softRadius <= 0f || distance <= softRadius)
            {
                enemy.ResetLeashTimer();
                return;
            }

            enemy.AddLeashTime(deltaTime);
            if (distance < hardRadius ||
                enemy.LeashTimerSeconds < Mathf.Max(0f, CurrentTuning.EnemyRecycleDelaySeconds) ||
                enemy.HealthFraction <= 0.15f)
            {
                return;
            }

            enemy.transform.position = ResolveSafeOffscreenPosition(
                playerPosition,
                Mathf.Max(CurrentTuning.EnemyRecycleMinimumRespawnDistance, CurrentTuning.PlayerRadius + enemy.Radius + 1.8f),
                Mathf.Max(CurrentTuning.EnemyRecycleMaximumRespawnDistance, CurrentTuning.EnemyRecycleMinimumRespawnDistance + 1f),
                enemy.InstanceId.Value + NormalEnemyRecycleCount + 17,
                ResolveOffscreenSpawnPadding(enemy.Role, "normal-recycle"),
                CurrentTuning.SpawnBandDepth);
            RecordGameplaySpawnSafety(enemy.Role, enemy.transform.position, "normal-recycle");
            enemy.ResetLeashTimer();
            NormalEnemyRecycleCount++;
        }

        private void TryUpdateMajorThreatLeash(SurvivorsEnemyActor enemy, Vector3 playerPosition, float distance, float deltaTime)
        {
            if (!enemy.CanReposition)
            {
                return;
            }

            float repositionRadius = Mathf.Max(0f, CurrentTuning.MajorThreatRepositionRadius);
            if (repositionRadius <= 0f || distance <= repositionRadius)
            {
                enemy.ResetLeashTimer();
                return;
            }

            enemy.AddLeashTime(deltaTime);
            if (enemy.LeashTimerSeconds < Mathf.Max(0f, CurrentTuning.MajorThreatRepositionDelaySeconds))
            {
                return;
            }

            float minimum = Mathf.Max(CurrentTuning.MajorThreatCatchUpRadius, CurrentTuning.PlayerRadius + enemy.Radius + 3.2f);
            float maximum = Mathf.Max(minimum + 1.5f, minimum + 4.5f);
            enemy.transform.position = ResolveSafeOffscreenPosition(
                playerPosition,
                minimum,
                maximum,
                enemy.InstanceId.Value + MajorThreatRepositionCount + 43,
                ResolveOffscreenSpawnPadding(enemy.Role, "major-threat-reentry"),
                CurrentTuning.SpawnBandDepth);
            RecordGameplaySpawnSafety(enemy.Role, enemy.transform.position, "major-threat-reentry");
            enemy.ResetLeashTimer();
            MajorThreatRepositionCount++;
            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            _lastOffscreenThreatMarkerLabel = $"{name} re-entering {ResolveCompassDirectionLabel(enemy.transform.position - playerPosition)}";
        }

        internal float ResolveEnemyCatchUpMoveSpeedMultiplier(SurvivorsEnemyActor enemy, float distance)
        {
            if (enemy == null || !enemy.IsAlive || !IsMajorRewardRole(enemy.Role))
            {
                return 1f;
            }

            float catchUpRadius = Mathf.Max(0f, CurrentTuning.MajorThreatCatchUpRadius);
            if (catchUpRadius <= 0f || distance <= catchUpRadius)
            {
                return 1f;
            }

            return Mathf.Max(1f, CurrentTuning.MajorThreatCatchUpSpeedMultiplier);
        }

        internal Vector3 ResolveRuntimeEnemySpawnPositionForResolver(long seed)
        {
            return ResolveSafeOffscreenPosition(
                PlayerPosition,
                ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole.Swarm, CurrentTuning.EnemySpawnRadius),
                ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole.Swarm, CurrentTuning.EnemySpawnRadius, CurrentTuning.EnemySpawnRadius + CurrentTuning.SpawnBandDepth),
                seed,
                ResolveOffscreenSpawnPadding(SurvivorsEnemyRole.Swarm, "radial-resolver"),
                CurrentTuning.SpawnBandDepth);
        }

        public bool IsWorldPositionInsideCameraViewportForTest(Vector3 position, float padding)
        {
            return TryResolveCameraGroundRect(Mathf.Max(0f, padding), out Rect visibleRect) &&
                ContainsGroundPoint(visibleRect, position);
        }

        private Vector3 ResolveSafeOffscreenPosition(Vector3 center, float minimumDistance, float maximumDistance, long seed)
        {
            return ResolveSafeOffscreenPosition(
                center,
                minimumDistance,
                maximumDistance,
                seed,
                CurrentTuning.OffscreenSpawnPadding,
                CurrentTuning.SpawnBandDepth);
        }

        private Vector3 ResolveSafeOffscreenPosition(
            Vector3 center,
            float minimumDistance,
            float maximumDistance,
            long seed,
            float padding,
            float bandDepth)
        {
            float min = Mathf.Max(1f, minimumDistance);
            float max = Mathf.Max(min + 0.1f, maximumDistance);
            float resolvedSeed = Mathf.Abs((float)(seed % 100000L));
            float angle = Mathf.Repeat(resolvedSeed * 137.508f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
            float span = Mathf.Max(0.1f, max - min);
            float radialDistance = min + Mathf.Repeat(resolvedSeed * 0.381966f, 1f) * span;
            float resolvedBandDepth = Mathf.Max(0.25f, bandDepth);
            if (TryResolveCameraGroundRect(Mathf.Max(0f, padding), out Rect visibleRect))
            {
                float exitDistance = ResolveDistanceToExitRect(center, direction, visibleRect);
                float bandOffset = 0.15f + Mathf.Repeat(resolvedSeed * 0.754877f, 1f) * resolvedBandDepth;
                float distance = Mathf.Max(radialDistance, exitDistance + bandOffset);
                Vector3 candidate = center + direction * distance;
                if (ContainsGroundPoint(visibleRect, candidate))
                {
                    candidate = center + direction * (exitDistance + resolvedBandDepth + 0.5f);
                }

                candidate.y = center.y;
                return candidate;
            }

            Vector3 fallback = center + direction * radialDistance;
            fallback.y = center.y;
            return fallback;
        }

        private float ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole role, float requested)
        {
            float minimum = Mathf.Max(1f, requested);
            if (IsMajorRewardRole(role))
            {
                minimum = Mathf.Max(minimum, CurrentTuning.MajorThreatCatchUpRadius);
            }

            return minimum;
        }

        private float ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole role, float minimumDistance, float requestedMaximum)
        {
            float band = Mathf.Max(0.25f, CurrentTuning.SpawnBandDepth);
            float maximum = Mathf.Max(minimumDistance + band, requestedMaximum);
            if (IsMajorRewardRole(role))
            {
                maximum = Mathf.Max(maximum, CurrentTuning.MajorThreatCatchUpRadius + band);
            }

            return maximum;
        }

        private float ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string spawnSource)
        {
            if (string.Equals(spawnSource, "normal-recycle", StringComparison.Ordinal))
            {
                return Mathf.Max(0.1f, CurrentTuning.RecycledEnemyOffscreenSpawnPadding);
            }

            return IsMajorRewardRole(role)
                ? Mathf.Max(0.1f, CurrentTuning.MajorThreatOffscreenSpawnPadding)
                : Mathf.Max(0.1f, CurrentTuning.OffscreenSpawnPadding);
        }

        private void RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string spawnSource)
        {
            float padding = ResolveOffscreenSpawnPadding(role, spawnSource);
            GameplayEnemySpawnSafetyCheckCountForTest++;
            _lastGameplaySpawnPosition = position;
            _lastGameplaySpawnPadding = padding;
            _lastGameplaySpawnWasInsideCameraViewport = IsWorldPositionInsideCameraViewportForTest(position, padding);
            if (!_lastGameplaySpawnWasInsideCameraViewport)
            {
                return;
            }

            GameplaySpawnInsideCameraViewViolationCountForTest++;
            string source = string.IsNullOrWhiteSpace(spawnSource) ? "runtime" : spawnSource;
            _lastGameplaySpawnSafetyFailure = $"{source} spawned {role} inside camera viewport at {position}";
        }

        private bool TryResolveCameraGroundRect(float padding, out Rect rect)
        {
            rect = default;
            Camera camera = _camera != null ? _camera : Camera.main;
            if (camera == null)
            {
                return false;
            }

            var ground = new Plane(Vector3.up, new Vector3(0f, PlayerPosition.y, 0f));
            Vector3[] corners =
            {
                ResolveViewportGroundPoint(camera, ground, new Vector3(0f, 0f, 0f)),
                ResolveViewportGroundPoint(camera, ground, new Vector3(0f, 1f, 0f)),
                ResolveViewportGroundPoint(camera, ground, new Vector3(1f, 0f, 0f)),
                ResolveViewportGroundPoint(camera, ground, new Vector3(1f, 1f, 0f))
            };

            float minX = corners[0].x;
            float maxX = corners[0].x;
            float minZ = corners[0].z;
            float maxZ = corners[0].z;
            for (int i = 1; i < corners.Length; i++)
            {
                minX = Mathf.Min(minX, corners[i].x);
                maxX = Mathf.Max(maxX, corners[i].x);
                minZ = Mathf.Min(minZ, corners[i].z);
                maxZ = Mathf.Max(maxZ, corners[i].z);
            }

            float resolvedPadding = Mathf.Max(0f, padding);
            rect = Rect.MinMaxRect(minX - resolvedPadding, minZ - resolvedPadding, maxX + resolvedPadding, maxZ + resolvedPadding);
            return rect.width > 0.01f && rect.height > 0.01f;
        }

        private static Vector3 ResolveViewportGroundPoint(Camera camera, Plane ground, Vector3 viewportPoint)
        {
            Ray ray = camera.ViewportPointToRay(viewportPoint);
            if (ground.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            Vector3 fallback = camera.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, Mathf.Max(1f, camera.nearClipPlane)));
            fallback.y = 0f;
            return fallback;
        }

        private static float ResolveDistanceToExitRect(Vector3 center, Vector3 direction, Rect rect)
        {
            const float Epsilon = 0.0001f;
            float exit = float.PositiveInfinity;
            if (Mathf.Abs(direction.x) > Epsilon)
            {
                float boundary = direction.x > 0f ? rect.xMax : rect.xMin;
                exit = Mathf.Min(exit, (boundary - center.x) / direction.x);
            }

            if (Mathf.Abs(direction.z) > Epsilon)
            {
                float boundary = direction.z > 0f ? rect.yMax : rect.yMin;
                exit = Mathf.Min(exit, (boundary - center.z) / direction.z);
            }

            return float.IsInfinity(exit) ? 0f : Mathf.Max(0f, exit);
        }

        private static bool ContainsGroundPoint(Rect rect, Vector3 position)
        {
            return position.x >= rect.xMin &&
                position.x <= rect.xMax &&
                position.z >= rect.yMin &&
                position.z <= rect.yMax;
        }

        private int CountOffscreenMajorThreatMarkers()
        {
            int count = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (ShouldShowOffscreenThreatMarker(_enemies[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private string ResolveFirstOffscreenMajorThreatMarkerLabel()
        {
            SurvivorsEnemyActor selected = ResolveFirstOffscreenMajorThreatMarkerEnemy();
            if (selected == null)
            {
                return string.Empty;
            }

            return FormatOffscreenThreatMarkerLabel(selected);
        }

        private SurvivorsEnemyActor ResolveFirstOffscreenMajorThreatMarkerEnemy()
        {
            SurvivorsEnemyActor selected = null;
            int selectedPriority = -1;
            float selectedDistance = 0f;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (!ShouldShowOffscreenThreatMarker(enemy))
                {
                    continue;
                }

                int priority = ResolveMajorThreatHealthPriority(enemy.Role);
                float distance = Vector3.Distance(PlayerPosition, enemy.transform.position);
                if (priority > selectedPriority || (priority == selectedPriority && distance > selectedDistance))
                {
                    selected = enemy;
                    selectedPriority = priority;
                    selectedDistance = distance;
                }
            }

            return selected;
        }

        private bool ShouldShowOffscreenThreatMarker(SurvivorsEnemyActor enemy)
        {
            if (enemy == null || !enemy.IsAlive || !enemy.ShowOffscreenMarker)
            {
                return false;
            }

            Vector3 offset = enemy.transform.position - PlayerPosition;
            offset.y = 0f;
            float markerDistance = Mathf.Max(0.1f, CurrentTuning.OffscreenThreatMarkerDistance);
            return offset.sqrMagnitude >= markerDistance * markerDistance;
        }

        private string FormatOffscreenThreatMarkerLabel(SurvivorsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return string.Empty;
            }

            Vector3 delta = enemy.transform.position - PlayerPosition;
            delta.y = 0f;
            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            return $"{name} {ResolveCompassDirectionLabel(delta)} {delta.magnitude:0}m";
        }

        private void UpdateOffscreenThreatMarkerSnapshot()
        {
            string label = ResolveFirstOffscreenMajorThreatMarkerLabel();
            if (!string.IsNullOrWhiteSpace(label))
            {
                _lastOffscreenThreatMarkerLabel = label;
            }
        }

        internal Vector3 ResolveEnemyCrowdSeparation(SurvivorsEnemyActor actor)
        {
            if (actor == null || _enemies.Count <= 1)
            {
                return Vector3.zero;
            }

            float strength = Mathf.Max(0f, CurrentTuning.EnemySeparationStrength);
            float baseRadius = Mathf.Max(0f, CurrentTuning.EnemySeparationRadius);
            if (strength <= 0f || baseRadius <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 origin = actor.transform.position;
            Vector3 push = Vector3.zero;
            int neighborCount = 0;
            int maxNeighbors = Mathf.Max(1, CurrentTuning.EnemySeparationMaxNeighbors);
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor other = _enemies[i];
                if (other == null || other == actor || !other.IsAlive)
                {
                    continue;
                }

                float radius = Mathf.Max(baseRadius, actor.Radius + other.Radius);
                Vector3 away = origin - other.transform.position;
                away.y = 0f;
                float sqrDistance = away.sqrMagnitude;
                if (sqrDistance > radius * radius)
                {
                    continue;
                }

                float distance = 0f;
                if (sqrDistance <= 0.0001f)
                {
                    away = ResolveDeterministicSeparationDirection(actor, other);
                }
                else
                {
                    distance = Mathf.Sqrt(sqrDistance);
                    away /= distance;
                }

                float weight = radius <= 0f ? 0f : 1f - Mathf.Clamp01(distance / radius);
                push += away * weight;
                neighborCount++;
                if (neighborCount >= maxNeighbors)
                {
                    break;
                }
            }

            if (push.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            return push.normalized * Mathf.Min(strength, push.magnitude * strength);
        }

        private static Vector3 ResolveDeterministicSeparationDirection(SurvivorsEnemyActor first, SurvivorsEnemyActor second)
        {
            int hash = first.GetInstanceID() ^ (second.GetInstanceID() << 1);
            float angle = ((hash & 0x7fffffff) % 360) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }

        private void TickProjectiles(float deltaTime)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                SurvivorsProjectileActor projectile = _projectiles[i];
                if (projectile == null || !projectile.IsActive)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Simulate(deltaTime);
            }
        }

        private void TickPickups(float deltaTime)
        {
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                SurvivorsPickupActor pickup = _pickups[i];
                if (pickup == null || !pickup.IsActive)
                {
                    _pickups.RemoveAt(i);
                    continue;
                }

                pickup.UpdateAttractionSettings(CurrentPickupAttractRange, CurrentPickupAttractionSpeed, CurrentTuning.PickupCollectRadius);
                pickup.Simulate(deltaTime);
            }
        }

        private void ClearRewardDrafts()
        {
            _currentDraft = null;
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
            _currentDraftRerollIndex = 0;
            _pendingVictoryAfterRewardDraft = false;
            _pendingBossRelicAfterRewardDraft = false;
        }

        private int GainExperience(int amount)
        {
            int baseAmount = Mathf.Max(1, amount);
            int gained = Mathf.Max(1, Mathf.RoundToInt(baseAmount * Mathf.Max(0.1f, 1f + ExperienceGainMultiplierBonus + PassiveLoadoutSurgeExperienceGainMultiplierBonus)));
            ExperienceCollected += gained;
            Experience += gained;
            ResolveLevelUpsFromExperienceBudget();
            TryOpenPendingLevelUpDraft();

            return gained;
        }

        private void ResolveLevelUpsFromExperienceBudget()
        {
            int queueLimit = Mathf.Max(1, CurrentTuning.MaximumQueuedLevelUps);
            if (CurrentTuning.LevelUpDraftCooldownSeconds > 0f &&
                _levelUpDraftCooldownTimer > 0f &&
                PendingLevelUps <= 0)
            {
                CapStoredExperienceForDraftThrottle();
                return;
            }

            int guard = 0;
            while (Experience >= RequiredExperienceForNextLevel && PendingLevelUps < queueLimit && guard++ < 256)
            {
                Experience -= RequiredExperienceForNextLevel;
                Level++;
                PendingLevelUps++;
            }

            if (CurrentTuning.LevelUpDraftCooldownSeconds > 0f && PendingLevelUps >= queueLimit)
            {
                CapStoredExperienceForDraftThrottle();
            }
        }

        private void CapStoredExperienceForDraftThrottle()
        {
            int storedExperienceCap = Mathf.Max(0, RequiredExperienceForNextLevel - 1);
            if (Experience <= storedExperienceCap)
            {
                return;
            }

            ThrottledExperienceOverflow += Experience - storedExperienceCap;
            Experience = storedExperienceCap;
        }

        private bool TryOpenPendingLevelUpDraft()
        {
            if (PendingLevelUps <= 0 ||
                State != SurvivorsRunState.Playing ||
                _rewardSelectionKind != SurvivorsRewardSelectionKind.None ||
                _levelUpDraftCooldownTimer > 0f)
            {
                return false;
            }

            OpenLevelUpDraft();
            return State == SurvivorsRunState.LevelUp;
        }

        private void RecordExperienceCombo(int gained)
        {
            if (gained <= 0)
            {
                return;
            }

            if (_experienceComboTimer <= 0f)
            {
                _experienceComboPickupCount = 0;
                _experienceComboAmount = 0;
                _gemRushActivatedForCurrentCombo = false;
            }

            _experienceComboPickupCount++;
            _experienceComboAmount += gained;
            _experienceComboTimer = ExperienceComboWindowSeconds;

            if (_experienceComboPickupCount < ExperienceComboMinimumPickupCount)
            {
                return;
            }

            ExperienceComboFeedbackCount++;
            ActivateGemRush();
            _experienceComboFeedbackLabel = $"{_experienceComboPickupCount} Gem Rush: +{_experienceComboAmount} XP, rush {GemRushRemainingSeconds:0.#}s";
            LastExperienceComboFeedbackLabel = _experienceComboFeedbackLabel;
            _experienceComboFeedbackTimer = ExperienceComboFeedbackDurationSeconds;
        }

        private void ActivateGemRush()
        {
            float duration = Mathf.Max(0f, CurrentTuning.GemRushDurationSeconds);
            if (duration <= 0f)
            {
                return;
            }

            _gemRushTimer = Mathf.Max(_gemRushTimer, duration);
            if (!_gemRushActivatedForCurrentCombo)
            {
                GemRushActivationCount++;
                _gemRushActivatedForCurrentCombo = true;
            }

            LastGemRushFeedbackLabel = $"Gem Rush: damage +{GemRushDamageBonus:0.#}, cooldown {GemRushCooldownMultiplierBonus:P0}, pickup +{GemRushPickupRangeBonus:0.#}";
        }

        private void OpenLevelUpDraft(IReadOnlyList<RunUpgradeId> lockedChoices = null)
        {
            if (_rewardSelectionKind != SurvivorsRewardSelectionKind.None)
            {
                return;
            }

            _currentDraftRerollIndex = 0;
            if (!TryGenerateCurrentUpgradeDraft(
                SurvivorsRewardSelectionKind.LevelUp,
                _currentDraftRerollIndex,
                lockedChoices,
                out RunUpgradeDraft draft))
            {
                _currentDraft = null;
                _currentRelicDraft = null;
                _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
                CompleteSkippedUpgradeDraft(
                    SurvivorsRewardSelectionKind.LevelUp,
                    consumeLevelUp: true,
                    selectedRewardUpgrade: false);
                return;
            }

            _currentDraft = draft;
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.LevelUp;
            State = SurvivorsRunState.LevelUp;
            _draftCardScrollPosition = Vector2.zero;
            LevelUpDraftOpenCount++;
            _draftOpenCount++;
            _levelUpDraftCooldownTimer = Mathf.Max(0f, CurrentTuning.LevelUpDraftCooldownSeconds);
            RecordMetricTime(ref _firstLevelUpDraftTimeSeconds);
            RecordRewardCardPresentation(_rewardSelectionKind, _currentDraft);
            BeginRewardSelectionTimeout();
            PlayAudioEvent(AudioEventLevelUp, _levelUpClip, 0.12f);
            PlayFeedback(_levelUpPulse, PlayerPosition, 34, _levelUpClip, AudioEventDraftOpened, 0.1f);
        }

        private bool OpenUpgradeRewardDraft(SurvivorsEnemyRole role, bool requireEvolutionChoice)
        {
            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                return false;
            }

            IReadOnlyList<RunUpgradeId> lockedChoices = CreateEligibleEvolutionChoiceLocks(CurrentTuning.DraftChoiceCount);
            if (requireEvolutionChoice && lockedChoices.Count == 0)
            {
                return false;
            }

            SurvivorsRewardSelectionKind selectionKind = role == SurvivorsEnemyRole.Boss
                ? SurvivorsRewardSelectionKind.BossUpgrade
                : SurvivorsRewardSelectionKind.EliteUpgrade;
            _currentDraftRerollIndex = 0;
            if (!TryGenerateCurrentUpgradeDraft(selectionKind, _currentDraftRerollIndex, lockedChoices, out RunUpgradeDraft draft))
            {
                _currentDraft = null;
                return false;
            }

            _currentDraft = draft;
            _currentRelicDraft = null;
            _rewardSelectionKind = selectionKind;
            _pendingVictoryAfterRewardDraft = role == SurvivorsEnemyRole.Boss && !_victoryClearedThisRun;
            _pendingBossRelicAfterRewardDraft = role == SurvivorsEnemyRole.Miniboss;
            _draftCardScrollPosition = Vector2.zero;
            if (role == SurvivorsEnemyRole.Boss)
            {
                BossUpgradeDraftOpenCount++;
            }
            else
            {
                EliteUpgradeDraftOpenCount++;
            }

            State = SurvivorsRunState.LevelUp;
            _draftOpenCount++;
            RecordRewardCardPresentation(selectionKind, _currentDraft);
            BeginRewardSelectionTimeout();
            PlayFeedback(_bossPulse, PlayerPosition, role == SurvivorsEnemyRole.Boss ? 72 : 54, _bossClip, AudioEventDraftOpened, 0.1f);
            return true;
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

        private SurvivorsTemplateTuning CreateConfiguredTuning(SurvivorsPacingProfile profile)
        {
            SurvivorsTemplateTuning configured = _authoredContent == null
                ? BasicSurvivorsGame.CreateTuning(profile)
                : _authoredContent.CreateTuning(profile);
            configured.PacingProfile = profile;
            return configured;
        }

        private bool OpenBossRelicDraft()
        {
            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                return false;
            }

            EnsureClassLibraryLoaded();
            IReadOnlyList<SurvivorsRelicDefinition> availableRelics = CreateAvailableRelicDefinitions();
            _currentRelicDraft = SurvivorsRelicDraftService.Generate(
                availableRelics,
                CurrentTuning.DraftChoiceCount,
                CurrentTuning.RunSeed + MinibossKilledCount + SelectedRelicCount + 97);
            if (_currentRelicDraft == null || _currentRelicDraft.Choices.Count == 0)
            {
                _currentRelicDraft = null;
                return false;
            }

            _currentDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.BossRelic;
            _pendingVictoryAfterRewardDraft = false;
            BossRelicDraftOpenCount++;
            State = SurvivorsRunState.LevelUp;
            _draftCardScrollPosition = Vector2.zero;
            _draftOpenCount++;
            RecordRewardCardPresentation(_currentRelicDraft);
            BeginRewardSelectionTimeout();
            PlayFeedback(_bossPulse, PlayerPosition, 44, _bossClip, AudioEventDraftOpened, 0.1f);
            return true;
        }

        private IReadOnlyList<SurvivorsRelicDefinition> CreateAvailableRelicDefinitions()
        {
            if (_relicDefinitions == null || _relicDefinitions.Count == 0)
            {
                return EmptyRelicChoices;
            }

            if (_selectedRelicIds.Count == 0)
            {
                return _relicDefinitions;
            }

            var availableRelics = new List<SurvivorsRelicDefinition>(_relicDefinitions.Count);
            for (int i = 0; i < _relicDefinitions.Count; i++)
            {
                SurvivorsRelicDefinition relic = _relicDefinitions[i];
                if (relic == null || string.IsNullOrWhiteSpace(relic.Id) || _selectedRelicIds.Contains(relic.Id))
                {
                    continue;
                }

                availableRelics.Add(relic);
            }

            return availableRelics.Count == 0 ? EmptyRelicChoices : availableRelics;
        }

        private void BeginRewardSelectionTimeout()
        {
            float timeout = CurrentTuning.RewardSelectionTimeoutSeconds;
            _rewardSelectionTimer = timeout > 0f ? timeout : 0f;
        }

        private void RecordRewardCardPresentation(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDraft draft)
        {
            int choiceCount = draft == null ? 0 : draft.Choices.Count;
            if (choiceCount <= 0)
            {
                return;
            }

            RewardCardPresentationCount += choiceCount;
            RunUpgradeRarity highestRarity = ResolveHighestRarity(draft.Choices);
            LastRewardCardPresentationLabel = $"{ResolveRewardKindLabel(selectionKind)} - {choiceCount} cards, best {highestRarity}";
        }

        private void RecordRewardCardPresentation(SurvivorsRelicDraft draft)
        {
            int choiceCount = draft == null ? 0 : draft.Choices.Count;
            if (choiceCount <= 0)
            {
                return;
            }

            RewardCardPresentationCount += choiceCount;
            LastRewardCardPresentationLabel = $"Boss Relic - {choiceCount} cards";
        }

        private void RecordRewardSelectionFeedback(SurvivorsRewardSelectionKind selectionKind, RunUpgradeDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            string name = ResolveUpgradeDisplayName(selected.Id);
            string category = ResolveCurrentUpgradeCategory(selected).ToString();
            string affected = ResolveUpgradeAffectedLabel(selected);
            LastRewardSelectionFeedbackLabel = $"{ResolveRewardKindLabel(selectionKind)}: {selected.Rarity} {category} - {name} ({affected})";
            RewardSelectionFeedbackCount++;
            _rewardFeedbackLabel = LastRewardSelectionFeedbackLabel;
            _rewardFeedbackColor = ResolveRarityAccentColor(selected.Rarity);
            _rewardFeedbackTimer = RewardFeedbackDurationSeconds;
            RecordBestRewardMoment(selected);
        }

        private void RecordBestRewardMoment(RunUpgradeDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            string name = ResolveUpgradeDisplayName(selected.Id);
            if ((int)selected.Rarity >= (int)_highestChosenRarity)
            {
                _highestChosenRarity = selected.Rarity;
                _highestChosenRarityLabel = selected.Rarity + " - " + name;
                _bestMomentLabel = "Highest rarity chosen: " + _highestChosenRarityLabel;
            }

            if (IsEvolutionUpgrade(selected))
            {
                _bestMomentLabel = "Evolution acquired: " + name + " at " + FormatRunTime(RunTimeSeconds);
            }
        }

        private void RecordRelicSelectionFeedback(SurvivorsRelicDefinition selected)
        {
            if (selected == null)
            {
                return;
            }

            LastRewardSelectionFeedbackLabel = $"Boss Relic: {selected.DisplayName} - {FormatRelicEffectSummary(selected)}";
            RewardSelectionFeedbackCount++;
            _rewardFeedbackLabel = LastRewardSelectionFeedbackLabel;
            _rewardFeedbackColor = ResolveRelicAccentColor(selected);
            _rewardFeedbackTimer = RewardFeedbackDurationSeconds;
            PlayAudioEvent(AudioEventRelic, _bossClip, 0.08f);
        }

        private void RecordRewardSkipFeedback(SurvivorsRewardSelectionKind selectionKind)
        {
            LastRewardSelectionFeedbackLabel = $"{ResolveRewardKindLabel(selectionKind)} skipped +{DraftSkipBloodShards} {CurrencyRewardLabel}";
            RewardSelectionFeedbackCount++;
            _rewardFeedbackLabel = LastRewardSelectionFeedbackLabel;
            _rewardFeedbackColor = new Color(0.72f, 0.84f, 0.9f);
            _rewardFeedbackTimer = RewardFeedbackDurationSeconds;
            PlayAudioEvent(AudioEventDraftSkip, _pickupClip, 0.08f);
        }

        private static RunUpgradeRarity ResolveHighestRarity(IReadOnlyList<RunUpgradeDefinition> choices)
        {
            RunUpgradeRarity highest = RunUpgradeRarity.Common;
            if (choices == null)
            {
                return highest;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                RunUpgradeDefinition choice = choices[i];
                if (choice != null && (int)choice.Rarity > (int)highest)
                {
                    highest = choice.Rarity;
                }
            }

            return highest;
        }

        private float ResolveEndlessSpawnInterval(float interval)
        {
            float resolved = Mathf.Max(0.05f, interval);
            if (!_victoryClearedThisRun)
            {
                return resolved;
            }

            float minimum = _runFlow == null || _runFlow.Definition == null
                ? 0.05f
                : _runFlow.Definition.MinimumEnemySpawnIntervalSeconds;
            return Mathf.Max(minimum, resolved * EndlessSpawnIntervalMultiplier);
        }

        private void TickRewardSelectionTimeout(float deltaTime)
        {
            if (State != SurvivorsRunState.LevelUp || _rewardSelectionTimer <= 0f)
            {
                return;
            }

            _rewardSelectionTimer -= Mathf.Max(0f, deltaTime);
            if (_rewardSelectionTimer <= 0f)
            {
                _rewardSelectionTimer = 0f;
                AutoSelectRewardChoice();
            }
        }

        private void CompleteSkippedUpgradeDraft(
            SurvivorsRewardSelectionKind selectionKind,
            bool consumeLevelUp,
            bool selectedRewardUpgrade)
        {
            DraftSkipCount++;
            _bonusBloodShardsEarnedThisRun += DraftSkipBloodShards;
            RecordRewardSkipFeedback(selectionKind);
            CompleteUpgradeDraftSelection(
                selectionKind,
                consumeLevelUp,
                selectedRewardUpgrade);
        }

        private void AutoSelectRewardChoice()
        {
            if (State != SurvivorsRunState.LevelUp)
            {
                return;
            }

            if (_rewardSelectionKind == SurvivorsRewardSelectionKind.BossRelic && _currentRelicDraft != null && _currentRelicDraft.Choices.Count > 0)
            {
                RewardAutoSelectCount++;
                SelectRelic(0);
            }
            else if (IsRunUpgradeSelectionKind(_rewardSelectionKind) && _currentDraft != null && _currentDraft.Choices.Count > 0)
            {
                RewardAutoSelectCount++;
                SelectUpgrade(0);
            }
        }

        private void CompleteUpgradeDraftSelection(
            SurvivorsRewardSelectionKind selectionKind,
            bool consumeLevelUp,
            bool selectedRewardUpgrade)
        {
            if (consumeLevelUp)
            {
                PendingLevelUps = Mathf.Max(0, PendingLevelUps - 1);
            }

            if (selectedRewardUpgrade)
            {
                SelectedRewardUpgradeCount++;
            }

            _currentDraft = null;
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
            _currentDraftRerollIndex = 0;
            if (selectionKind == SurvivorsRewardSelectionKind.BossUpgrade && _pendingVictoryAfterRewardDraft)
            {
                _pendingVictoryAfterRewardDraft = false;
                _pendingBossRelicAfterRewardDraft = false;
                EnterVictory();
            }
            else if (_pendingBossRelicAfterRewardDraft)
            {
                _pendingBossRelicAfterRewardDraft = false;
                if (OpenBossRelicDraft())
                {
                    return;
                }

                ResolveLevelUpsFromExperienceBudget();
                if (TryOpenPendingLevelUpDraft())
                {
                    return;
                }

                State = SurvivorsRunState.Playing;
            }
            else
            {
                State = SurvivorsRunState.Playing;
                ResolveLevelUpsFromExperienceBudget();
                TryOpenPendingLevelUpDraft();
            }
        }

        private bool SelectRelic(int index)
        {
            if (State != SurvivorsRunState.LevelUp || _rewardSelectionKind != SurvivorsRewardSelectionKind.BossRelic || _currentRelicDraft == null || index < 0 || index >= _currentRelicDraft.Choices.Count)
            {
                return false;
            }

            SurvivorsRelicDefinition selected = _currentRelicDraft.Choices[index];
            if (selected == null || string.IsNullOrWhiteSpace(selected.Id) || !_selectedRelicIds.Add(selected.Id))
            {
                return false;
            }

            ApplyRelic(selected);
            SelectedRelicCount++;
            _selectedRelics.Add(selected);
            RecordRelicSelectionFeedback(selected);
            TriggerBossRelicSurge(selected);
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
            _pendingVictoryAfterRewardDraft = false;
            _pendingBossRelicAfterRewardDraft = false;
            _currentDraftRerollIndex = 0;
            State = SurvivorsRunState.Playing;
            ResolveLevelUpsFromExperienceBudget();
            if (TryOpenPendingLevelUpDraft())
            {
                return true;
            }

            return true;
        }

        private void TickKillStreak(float deltaTime)
        {
            if (_killStreakTimer <= 0f || _killStreakCount <= 0)
            {
                return;
            }

            _killStreakTimer = Mathf.Max(0f, _killStreakTimer - Mathf.Max(0f, deltaTime));
            if (_killStreakTimer <= 0f)
            {
                _killStreakCount = 0;
            }
        }

        private void TickStreakSurge(float deltaTime)
        {
            if (_streakSurgeTimer <= 0f || StreakSurgeTier <= 0)
            {
                return;
            }

            _streakSurgeTimer = Mathf.Max(0f, _streakSurgeTimer - Mathf.Max(0f, deltaTime));
            if (_streakSurgeTimer <= 0f)
            {
                StreakSurgeTier = 0;
            }
        }

        private void TickRoamingCacheSurge(float deltaTime)
        {
            if (_roamingCacheSurgeTimer <= 0f)
            {
                return;
            }

            _roamingCacheSurgeTimer = Mathf.Max(0f, _roamingCacheSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickArenaShrineSurge(float deltaTime)
        {
            if (_arenaShrineSurgeTimer <= 0f)
            {
                return;
            }

            _arenaShrineSurgeTimer = Mathf.Max(0f, _arenaShrineSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickWaystoneFocus(float deltaTime)
        {
            if (_waystoneFocusTimer <= 0f)
            {
                return;
            }

            _waystoneFocusTimer = Mathf.Max(0f, _waystoneFocusTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickWaystoneChainSurge(float deltaTime)
        {
            if (_waystoneChainSurgeTimer <= 0f)
            {
                return;
            }

            _waystoneChainSurgeTimer = Mathf.Max(0f, _waystoneChainSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickHordeRushClearSurge(float deltaTime)
        {
            if (_hordeRushClearSurgeTimer <= 0f)
            {
                return;
            }

            _hordeRushClearSurgeTimer = Mathf.Max(0f, _hordeRushClearSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickWeaponLoadoutSurge(float deltaTime)
        {
            if (_weaponLoadoutSurgeTimer <= 0f)
            {
                return;
            }

            _weaponLoadoutSurgeTimer = Mathf.Max(0f, _weaponLoadoutSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickPassiveLoadoutSurge(float deltaTime)
        {
            if (_passiveLoadoutSurgeTimer <= 0f)
            {
                return;
            }

            _passiveLoadoutSurgeTimer = Mathf.Max(0f, _passiveLoadoutSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickBossRelicSurge(float deltaTime)
        {
            if (_bossRelicSurgeTimer <= 0f)
            {
                return;
            }

            _bossRelicSurgeTimer = Mathf.Max(0f, _bossRelicSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickGemRush(float deltaTime)
        {
            if (_gemRushTimer <= 0f)
            {
                return;
            }

            _gemRushTimer = Mathf.Max(0f, _gemRushTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickEvolutionChainSurge(float deltaTime)
        {
            if (_evolutionChainSurgeTimer <= 0f)
            {
                return;
            }

            _evolutionChainSurgeTimer = Mathf.Max(0f, _evolutionChainSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickEndlessSurge(float deltaTime)
        {
            if (_endlessSurgeTimer <= 0f)
            {
                return;
            }

            _endlessSurgeTimer = Mathf.Max(0f, _endlessSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        private void TickBarrier(float deltaTime)
        {
            float regen = Mathf.Max(0f, CurrentTuning.BaseBarrierRegenPerSecond + BarrierRegenPerSecondBonus);
            if (regen > 0f)
            {
                RestoreBarrier(regen * deltaTime);
            }
        }

        private void RestoreBarrier(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            BarrierValue = Mathf.Min(BarrierCapacity, BarrierValue + amount);
        }

        private static bool CanApplyDamageAugments(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return true;
            }

            return source.IndexOf(".status.", StringComparison.Ordinal) < 0 &&
                source.IndexOf(".augment.", StringComparison.Ordinal) < 0 &&
                source.IndexOf("combatant.survivors.enemy", StringComparison.Ordinal) < 0;
        }

        private CombatSourceSnapshot CreateEnemyDamageSourceSnapshot(string source, bool applyAugments)
        {
            if (!applyAugments || !CanApplyDamageAugments(source) || CriticalChanceNormalized <= 0f)
            {
                return null;
            }

            return new CombatSourceSnapshot(CriticalChanceNormalized, CriticalDamageMultiplier);
        }

        private void ApplyRelic(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return;
            }

            if (relic.EffectKind == SurvivorsRelicEffectKind.DamageBonus)
            {
                RelicDamageBonus += relic.Amount;
                DamageBonus += relic.Amount;
            }
            else if (relic.EffectKind == SurvivorsRelicEffectKind.CooldownMultiplier)
            {
                RelicCooldownMultiplierBonus += relic.Amount;
                WeaponCooldownMultiplierBonus = Mathf.Max(-0.75f, WeaponCooldownMultiplierBonus + relic.Amount);
            }
            else if (relic.EffectKind == SurvivorsRelicEffectKind.PickupRange)
            {
                RelicPickupRangeBonus += relic.Amount;
                PickupRangeBonus += relic.Amount;
            }
        }

        private void TriggerBossRelicSurge(SurvivorsRelicDefinition relic)
        {
            float duration = Mathf.Max(0f, CurrentTuning.BossRelicSurgeDurationSeconds);
            if (duration > 0f)
            {
                _bossRelicSurgeTimer = Mathf.Max(_bossRelicSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, CurrentTuning.BossRelicSurgeRadius);
            float damage = Mathf.Max(0f, CurrentTuning.BossRelicSurgeDamage);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                var targets = new List<SurvivorsEnemyActor>();
                CollectEnemiesWithinRadius(PlayerPosition, radius, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    SurvivorsEnemyActor enemy = targets[i];
                    if (enemy == null || !enemy.IsAlive || IsMajorRewardRole(enemy.Role))
                    {
                        continue;
                    }

                    enemy.ApplyDamage(damage, "survivors.relic.surge");
                    hitCount++;
                }
            }

            BossRelicSurgeCount++;
            BossRelicSurgeHitCount += hitCount;
            string name = relic == null || string.IsNullOrWhiteSpace(relic.DisplayName) ? "Boss Relic" : relic.DisplayName;
            LastBossRelicSurgeFeedbackLabel = $"{name} Surge: {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastBossRelicSurgeFeedbackLabel += $", rush {BossRelicSurgeRemainingSeconds:0.#}s";
            }

            RecordStreakRewardFeedback(LastBossRelicSurgeFeedbackLabel, ResolveRelicAccentColor(relic));
            PlayFeedback(_bossPulse, PlayerPosition, Mathf.Clamp(42 + hitCount * 4, 48, 88), _bossClip);
        }

        private void ApplyUpgrade(RunUpgradeDefinition upgrade)
        {
            for (int i = 0; i < upgrade.Effects.Count; i++)
            {
                RunUpgradeEffectDescriptor effect = upgrade.Effects[i];
                if (effect.EffectId.Equals(BasicSurvivorsGame.DamageBonusEffect))
                {
                    DamageBonus += (float)effect.Amount;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.FireRateEffect))
                {
                    WeaponCooldownMultiplierBonus = Mathf.Max(-0.75f, WeaponCooldownMultiplierBonus + (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MoveSpeedEffect))
                {
                    MoveSpeedBonus += (float)effect.Amount;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetRangeEffect))
                {
                    PickupRangeBonus += (float)effect.Amount;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetSpeedEffect))
                {
                    PickupAttractionSpeedBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetPulseEffect))
                {
                    PickupMagnetPulseIntervalReductionBonus += Mathf.Max(0f, (float)effect.Amount);
                    if (_pickupMagnetPulseTimer <= 0f)
                    {
                        _pickupMagnetPulseTimer = ResolvePickupMagnetPulseIntervalSeconds();
                    }
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MaxHealthEffect) && _playerHealth != null)
                {
                    _playerHealth.ChangeMaximumHealth(_playerHealth.MaximumHealth + effect.Amount, MaximumChangePolicy.FillToMaximum);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitBladeEffect))
                {
                    OrbitBladeBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitRadiusEffect))
                {
                    OrbitRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                    OrbitBladeBonus += 1;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MeleeTargetEffect))
                {
                    MeleeTargetBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BurstCountEffect))
                {
                    BurstCountBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BurstEchoEffect))
                {
                    BurstEchoBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.TargetedBurstEffect))
                {
                    TargetedBurstSigilBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileFanEffect))
                {
                    ProjectileFanBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectilePierceEffect))
                {
                    ProjectilePierceBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileChainEffect))
                {
                    ProjectileChainBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileForkEffect))
                {
                    ProjectileForkBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileReturnEffect))
                {
                    ProjectileReturnBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.HitscanPierceEffect))
                {
                    HitscanPierceBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadCountEffect))
                {
                    PayloadCountBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadRadiusEffect))
                {
                    PayloadExplosionRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadTriggerRadiusEffect))
                {
                    PayloadTriggerRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PoisonEffect))
                {
                    PoisonDamageRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BleedEffect))
                {
                    BleedDamageRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ExecuteEffect))
                {
                    ExecuteThresholdNormalized = Mathf.Clamp01(ExecuteThresholdNormalized + (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalChanceEffect))
                {
                    CriticalChanceBonus = Mathf.Clamp01(CriticalChanceBonus + Mathf.Max(0f, (float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalDamageEffect))
                {
                    CriticalDamageMultiplierBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.DraftLuckEffect))
                {
                    DraftLuckBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaDamageEffect))
                {
                    DeathNovaDamageBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaRadiusEffect))
                {
                    DeathNovaRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.LifestealEffect))
                {
                    LifestealRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierCapacityEffect))
                {
                    BarrierCapacityBonus += Mathf.Max(0f, (float)effect.Amount);
                    RestoreBarrier((float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierRegenEffect))
                {
                    BarrierRegenPerSecondBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierOnDamageEffect))
                {
                    BarrierOnDamageRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ExperienceGainEffect))
                {
                    ExperienceGainMultiplierBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.AreaRadiusEffect))
                {
                    AreaRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.WeaponUnlockEffect))
                {
                    // Weapon unlocks are applied through run-build metadata so slot rules stay centralized.
                }
            }

            RecordRunBuildSelection(upgrade);
            RecordNewlyEligibleEvolutionFeedback();
        }

        private void DrawLevelUpOverlay()
        {
            bool upgradeDraftOpen = !IsRelicChoiceOpen;
            int choiceCount = IsRelicChoiceOpen ? CurrentRelicChoices.Count : CurrentDraftChoices.Count;
            DrawDimOverlay(0.72f);
            Rect rect = ResolveCenteredPanelRect(1120f, 690f, 320f, 420f, 20f);
            DrawSolidRect(rect, new Color(0.014f, 0.018f, 0.027f, 0.96f));
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 4f), ResolveRewardTitleAccentColor());
            string title = ResolveRewardOverlayTitle();
            GUI.Label(new Rect(rect.x + 30f, rect.y + 20f, rect.width - 60f, 38f), title, _draftTitleStyle);
            GUI.Label(
                new Rect(rect.x + 30f, rect.y + 58f, rect.width - 60f, 22f),
                RewardSelectionRemainingSeconds > 0f ? $"Auto-pick in {RewardSelectionRemainingSeconds:0}s" : "Choose one card. Mouse, 1/2/3, R reroll, Shift+number banish, S skip.",
                _hudSmallStyle);

            if (choiceCount <= 0)
            {
                GUI.Label(new Rect(rect.x + 30f, rect.y + 110f, rect.width - 60f, 28f), "No rewards available.", _hudLabelStyle);
                return;
            }

            bool horizontal = rect.width >= 760f;
            float cardAreaTop = rect.y + 100f;
            float cardAreaBottom = upgradeDraftOpen ? rect.yMax - 86f : rect.yMax - 34f;
            float cardGap = 16f;
            float cardWidth = horizontal
                ? (rect.width - 60f - cardGap * Mathf.Max(0, choiceCount - 1)) / choiceCount
                : rect.width - 60f;
            float cardHeight = horizontal
                ? cardAreaBottom - cardAreaTop
                : Mathf.Max(220f, Mathf.Min(330f, cardAreaBottom - cardAreaTop - 12f));
            Rect cardViewRect = horizontal
                ? Rect.zero
                : new Rect(rect.x + 30f, cardAreaTop, cardWidth, Mathf.Max(1f, cardAreaBottom - cardAreaTop));
            Rect cardContentRect = horizontal
                ? Rect.zero
                : new Rect(0f, 0f, cardWidth - 18f, choiceCount * cardHeight + Mathf.Max(0, choiceCount - 1) * cardGap);
            if (!horizontal)
            {
                _draftCardScrollPosition = GUI.BeginScrollView(cardViewRect, _draftCardScrollPosition, cardContentRect);
            }

            for (int i = 0; i < choiceCount; i++)
            {
                Rect cardRect = horizontal
                    ? new Rect(rect.x + 30f + i * (cardWidth + cardGap), cardAreaTop, cardWidth, cardHeight)
                    : new Rect(0f, i * (cardHeight + cardGap), cardContentRect.width, cardHeight);
                DraftCardPresentation card = IsRelicChoiceOpen
                    ? CreateRelicDraftCard(i, CurrentRelicChoices[i])
                    : CreateUpgradeDraftCard(i, CurrentDraftChoices[i]);
                bool hover = cardRect.Contains(Event.current.mousePosition);
                DrawDraftChoiceCard(cardRect, card, hover);
                Rect selectRect = upgradeDraftOpen
                    ? new Rect(cardRect.x, cardRect.y, cardRect.width, Mathf.Max(24f, cardRect.height - 48f))
                    : cardRect;
                if (GUI.Button(selectRect, GUIContent.none, _transparentButtonStyle))
                {
                    if (SelectUpgrade(i))
                    {
                        return;
                    }
                }

                if (upgradeDraftOpen)
                {
                    bool previousEnabled = GUI.enabled;
                    GUI.enabled = previousEnabled && CanBanishCurrentDraft();
                    Rect banishRect = new Rect(cardRect.x + 14f, cardRect.yMax - 40f, Mathf.Min(112f, cardRect.width - 28f), 28f);
                    if (GUI.Button(banishRect, ActiveUiTheme.banishButtonLabel))
                    {
                        if (BanishDraftChoice(i))
                        {
                            GUI.enabled = previousEnabled;
                            return;
                        }
                    }

                    GUI.enabled = previousEnabled;
                }
            }

            if (!horizontal)
            {
                GUI.EndScrollView();
            }

            if (upgradeDraftOpen)
            {
                float footerY = rect.yMax - 58f;
                float footerX = rect.x + 30f;
                float footerWidth = rect.width - 60f;
                float footerGap = 10f;
                bool compactFooter = footerWidth < 560f;
                float buttonWidth = compactFooter ? (footerWidth - footerGap) * 0.5f : 170f;
                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && CanRerollCurrentDraft();
                if (GUI.Button(new Rect(footerX, footerY, buttonWidth, 34f), $"{ActiveUiTheme.rerollButtonLabel} ({DraftRerollsRemaining})"))
                {
                    if (RerollCurrentDraft())
                    {
                        GUI.enabled = previousEnabled;
                        return;
                    }
                }

                GUI.enabled = previousEnabled && CanSkipCurrentDraft();
                float skipX = footerX + buttonWidth + footerGap;
                float skipWidth = compactFooter ? buttonWidth : 210f;
                if (GUI.Button(new Rect(skipX, footerY, skipWidth, 34f), $"{ActiveUiTheme.skipButtonLabel} (+{DraftSkipBloodShards})"))
                {
                    if (SkipCurrentDraft())
                    {
                        GUI.enabled = previousEnabled;
                        return;
                    }
                }

                GUI.enabled = previousEnabled;
                if (!compactFooter)
                {
                    GUI.Label(
                        new Rect(rect.x + 438f, footerY + 7f, rect.width - 468f, 20f),
                        $"Banishes {DraftBanishesRemaining}",
                        _hudSmallStyle);
                }
            }
        }

        private DraftCardPresentation CreateUpgradeDraftCard(int index, RunUpgradeDefinition choice)
        {
            if (choice == null)
            {
                return new DraftCardPresentation
                {
                    Index = index,
                    Hotkey = (index + 1).ToString(),
                    Name = "Missing Choice",
                    RarityLabel = "Missing",
                    CategoryId = "MetaReward",
                    CategoryLabel = ActiveUiTheme.GetCategoryDisplayName("MetaReward", "Meta/Reward"),
                    AffectedLabel = "Affects Build",
                    RankLabel = "No rank",
                    Description = "Missing upgrade definition.",
                    EffectPreview = "No effect preview.",
                    RequirementHint = string.Empty,
                    IconId = ActiveUiTheme.GetCategoryIconId("MetaReward", "reward"),
                    StyleToken = ActiveUiTheme.GetRarityStyleToken("Common"),
                    AccentColor = Color.white
                };
            }

            TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata);
            SurvivorsRunUpgradeCategory category = ResolveCurrentUpgradeCategory(choice);
            string categoryId = ResolvePlayerUpgradeCategoryId(choice, category, metadata);
            bool isEvolution = category == SurvivorsRunUpgradeCategory.Evolution;
            string rarityId = isEvolution ? "Evolution" : choice.Rarity.ToString();
            Color fallbackAccent = isEvolution
                ? new Color(1f, 0.38f, 0.56f)
                : ResolveRarityAccentColor(choice.Rarity);
            int currentRank = _upgradeState == null ? 0 : _upgradeState.GetRank(choice.Id);
            int nextRank = Mathf.Min(choice.MaxRank, currentRank + 1);
            return new DraftCardPresentation
            {
                Index = index,
                Hotkey = (index + 1).ToString(),
                Name = ResolveUpgradeDisplayName(choice.Id),
                RarityLabel = ActiveUiTheme.GetRarityDisplayName(rarityId),
                CategoryId = categoryId,
                CategoryLabel = ActiveUiTheme.GetCategoryDisplayName(categoryId, FormatUpgradeCategoryLabel(category)),
                AffectedLabel = ResolveUpgradeAffectedLabel(choice),
                RankLabel = choice.MaxRank <= 1 ? "One-time unlock" : $"Rank {currentRank}->{nextRank}/{choice.MaxRank}",
                Description = metadata == null ? ResolveUpgradeDisplayName(choice.Id) : metadata.Description,
                EffectPreview = ResolveUpgradeEffectPreview(choice),
                RequirementHint = ResolveUpgradeRequirementHint(choice, metadata, category),
                IconId = ActiveUiTheme.GetCategoryIconId(categoryId, categoryId),
                StyleToken = ActiveUiTheme.GetRarityStyleToken(rarityId),
                IsEvolution = isEvolution,
                AccentColor = ActiveUiTheme.GetRarityAccentColor(rarityId, fallbackAccent)
            };
        }

        private DraftCardPresentation CreateRelicDraftCard(int index, SurvivorsRelicDefinition relic)
        {
            Color accent = ActiveUiTheme.GetRarityAccentColor("Relic", ResolveRelicAccentColor(relic));
            return new DraftCardPresentation
            {
                Index = index,
                Hotkey = (index + 1).ToString(),
                Name = relic == null || string.IsNullOrWhiteSpace(relic.DisplayName) ? "Boss Relic" : relic.DisplayName,
                RarityLabel = ActiveUiTheme.GetRarityDisplayName("Relic"),
                CategoryId = "Relic",
                CategoryLabel = ActiveUiTheme.GetCategoryDisplayName("Relic", "Relic"),
                AffectedLabel = relic == null ? "Affects Build" : "Affects " + ShortWeaponName(relic.TargetId),
                RankLabel = "Run relic",
                Description = relic == null ? "Missing relic definition." : FormatRelicEffectSummary(relic),
                EffectPreview = relic == null ? "No effect preview." : ResolveRelicEffectPreview(relic),
                RequirementHint = "Unique boss relic for this run.",
                IconId = ActiveUiTheme.GetCategoryIconId("Relic", "relic"),
                StyleToken = ActiveUiTheme.GetRarityStyleToken("Relic"),
                AccentColor = accent
            };
        }

        private void DrawDraftChoiceCard(Rect rect, DraftCardPresentation card, bool hover)
        {
            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * (card.IsEvolution ? 5.5f : 3.2f)) * 0.14f;
            Color accent = card.AccentColor;
            DrawSolidRect(rect, new Color(0.022f, 0.027f, 0.038f, 0.96f));
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, rect.height), new Color(accent.r, accent.g, accent.b, hover ? 0.18f : 0.1f));
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 8f), new Color(accent.r, accent.g, accent.b, hover ? 1f : 0.82f));
            DrawSolidRect(new Rect(rect.x, rect.y, 4f, rect.height), new Color(accent.r, accent.g, accent.b, card.IsEvolution ? 0.94f : 0.78f));
            DrawSolidRect(new Rect(rect.xMax - 4f, rect.y, 4f, rect.height), new Color(accent.r, accent.g, accent.b, hover ? 0.74f : 0.38f));
            if (card.IsEvolution || hover)
            {
                DrawSolidRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, 2f), new Color(accent.r, accent.g, accent.b, 0.22f + pulse * 0.18f));
            }

            Rect hotkey = new Rect(rect.xMax - 54f, rect.y + 18f, 34f, 30f);
            DrawSolidRect(hotkey, new Color(accent.r, accent.g, accent.b, 0.84f));
            GUI.Label(hotkey, card.Hotkey, _draftCardHotkeyStyle);
            Rect icon = new Rect(rect.x + 18f, rect.y + 22f, 58f, 58f);
            DrawSolidRect(icon, new Color(0.04f, 0.05f, 0.065f, 0.92f));
            DrawSolidRect(new Rect(icon.x, icon.yMax - 3f, icon.width, 3f), new Color(accent.r, accent.g, accent.b, 0.86f));
            GUI.Label(icon, ActiveUiTheme.iconPlaceholderPrefix + "\n" + card.IconId, _draftCardMetaStyle);

            float x = rect.x + 90f;
            float width = rect.width - 116f;
            GUI.Label(new Rect(x, rect.y + 20f, width - 42f, 20f), card.RarityLabel + "  |  " + card.CategoryLabel, _draftCardMetaStyle);
            GUI.Label(new Rect(x, rect.y + 44f, width - 42f, 42f), card.Name, _draftCardNameStyle);
            if (card.IsEvolution)
            {
                DrawSolidRect(new Rect(rect.x + 18f, rect.y + 92f, rect.width - 36f, 24f), new Color(accent.r, accent.g, accent.b, 0.22f));
                GUI.Label(new Rect(rect.x + 24f, rect.y + 94f, rect.width - 48f, 20f), "EVOLUTION REWARD", _draftCardMetaStyle);
            }

            float y = card.IsEvolution ? rect.y + 124f : rect.y + 94f;
            GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, 22f), card.AffectedLabel + "   " + card.RankLabel, _hudSmallStyle);
            y += 28f;
            GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, Mathf.Max(72f, rect.height * 0.25f)), card.Description, _draftCardDescriptionStyle);
            y += Mathf.Max(82f, rect.height * 0.27f);
            GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, 48f), "Effect: " + card.EffectPreview, _hudSmallStyle);
            y += 54f;
            if (!string.IsNullOrWhiteSpace(card.RequirementHint))
            {
                GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, 44f), card.RequirementHint, _hudSmallStyle);
            }
        }

        private string ResolvePlayerUpgradeCategoryId(RunUpgradeDefinition choice, SurvivorsRunUpgradeCategory category, SurvivorsRunUpgradeMetadata metadata)
        {
            if (choice != null && IsPickupMagnetUpgrade(choice))
            {
                return "PickupMagnet";
            }

            if (category == SurvivorsRunUpgradeCategory.Weapon)
            {
                return "NewWeapon";
            }

            return category.ToString();
        }

        private bool IsPickupMagnetUpgrade(RunUpgradeDefinition choice)
        {
            if (choice == null || choice.Effects == null)
            {
                return false;
            }

            for (int i = 0; i < choice.Effects.Count; i++)
            {
                RunUpgradeEffectDescriptor effect = choice.Effects[i];
                if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetRangeEffect) ||
                    effect.EffectId.Equals(BasicSurvivorsGame.MagnetSpeedEffect) ||
                    effect.EffectId.Equals(BasicSurvivorsGame.MagnetPulseEffect))
                {
                    return true;
                }
            }

            return false;
        }

        private string ResolveUpgradeEffectPreview(RunUpgradeDefinition choice)
        {
            if (choice == null || choice.Effects == null || choice.Effects.Count == 0)
            {
                return "No effect preview.";
            }

            string label = string.Empty;
            if (IsEvolutionUpgrade(choice))
            {
                label = "Evolves into " + ResolveUpgradeDisplayName(choice.Id);
            }

            int shown = Mathf.Min(2, choice.Effects.Count);
            for (int i = 0; i < shown; i++)
            {
                string preview = FormatUpgradeEffectPreview(choice, choice.Effects[i]);
                if (string.IsNullOrWhiteSpace(preview))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(label))
                {
                    label += "; ";
                }

                label += preview;
            }

            if (choice.Effects.Count > shown)
            {
                label += "; +" + (choice.Effects.Count - shown).ToString() + " more";
            }

            return string.IsNullOrWhiteSpace(label) ? "Adds behavior." : label;
        }

        private string ResolveRelicEffectPreview(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return "No effect preview.";
            }

            switch (relic.EffectKind)
            {
                case SurvivorsRelicEffectKind.DamageBonus:
                    return $"+{relic.Amount:0.#} damage while this relic is held";
                case SurvivorsRelicEffectKind.CooldownMultiplier:
                    return $"{Mathf.Abs(relic.Amount):P0} faster attacks for {ShortWeaponName(relic.TargetId)}";
                case SurvivorsRelicEffectKind.PickupRange:
                    return $"+{relic.Amount:0.#} pickup radius and relic surge momentum";
                default:
                    return FormatRelicEffectSummary(relic);
            }
        }

        private string FormatUpgradeEffectPreview(RunUpgradeDefinition choice, RunUpgradeEffectDescriptor effect)
        {
            float amount = (float)effect.Amount;
            if (effect.EffectId.Equals(BasicSurvivorsGame.DamageBonusEffect)) return FormatComparison("Damage", ProjectileDamage, ProjectileDamage + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.FireRateEffect)) return FormatComparison("Cooldown", WeaponCooldownSeconds, Mathf.Max(0.12f, WeaponCooldownSeconds * Mathf.Max(0.2f, 1f + amount)), "0.00s");
            if (effect.EffectId.Equals(BasicSurvivorsGame.MoveSpeedEffect)) return FormatComparison("Move Speed", PlayerMoveSpeed, PlayerMoveSpeed + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetRangeEffect)) return FormatComparison("Pickup Radius", CurrentPickupAttractRange, CurrentPickupAttractRange + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetSpeedEffect)) return FormatComparison("Magnet Pull Speed", CurrentPickupAttractionSpeed, CurrentPickupAttractionSpeed + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetPulseEffect)) return FormatPulseIntervalComparison(amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.MaxHealthEffect)) return FormatComparison("Max Health", MaxHealth, MaxHealth + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitBladeEffect)) return FormatComparison("Orbit Blades", OrbitBladeBonus, OrbitBladeBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitRadiusEffect)) return FormatComparison("Orbit Radius", OrbitRadiusBonus, OrbitRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.MeleeTargetEffect)) return FormatComparison("Slash Targets", MeleeTargetBonus, MeleeTargetBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.BurstCountEffect)) return FormatComparison("Burst Pulses", BurstCountBonus, BurstCountBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.BurstEchoEffect)) return FormatComparison("Echo Pulses", BurstEchoBonus, BurstEchoBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.TargetedBurstEffect)) return FormatComparison("Targeted Sigils", TargetedBurstSigilBonus, TargetedBurstSigilBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileFanEffect)) return FormatComparison("Projectiles", ProjectileFanBonus + 1, ProjectileFanBonus + 1 + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectilePierceEffect)) return FormatComparison("Pierce", ProjectilePierceBonus, ProjectilePierceBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileChainEffect)) return FormatComparison("Chain Count", ProjectileChainBonus, ProjectileChainBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileForkEffect)) return FormatComparison("Forks", ProjectileForkBonus, ProjectileForkBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileReturnEffect)) return FormatComparison("Returns", ProjectileReturnBonus, ProjectileReturnBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.HitscanPierceEffect)) return FormatComparison("Beam Pierce", HitscanPierceBonus, HitscanPierceBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadCountEffect)) return FormatComparison("Payloads", PayloadCountBonus, PayloadCountBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadRadiusEffect)) return FormatComparison("Area Radius", PayloadExplosionRadiusBonus, PayloadExplosionRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadTriggerRadiusEffect)) return FormatComparison("Trigger Range", PayloadTriggerRadiusBonus, PayloadTriggerRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.PoisonEffect)) return FormatPercentComparison("Poison", PoisonDamageRatio, PoisonDamageRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BleedEffect)) return FormatPercentComparison("Bleed", BleedDamageRatio, BleedDamageRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.ExecuteEffect)) return FormatPercentComparison("Execute Threshold", ExecuteThresholdNormalized, Mathf.Clamp01(ExecuteThresholdNormalized + amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalChanceEffect)) return FormatPercentComparison("Crit Chance", CriticalChanceNormalized, Mathf.Clamp01(CriticalChanceNormalized + Mathf.Max(0f, amount)));
            if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalDamageEffect)) return FormatComparison("Crit Damage", CriticalDamageMultiplier, CriticalDamageMultiplier + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.DraftLuckEffect)) return FormatPercentComparison("Draft Luck", DraftLuckBonus, DraftLuckBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaDamageEffect)) return FormatComparison("Death Nova Damage", DeathNovaDamage, DeathNovaDamage + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaRadiusEffect)) return FormatComparison("Death Nova Radius", DeathNovaRadius, DeathNovaRadius + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.LifestealEffect)) return FormatPercentComparison("Lifesteal", LifestealRatio, LifestealRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierCapacityEffect)) return FormatComparison("Barrier", BarrierCapacity, BarrierCapacity + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierRegenEffect)) return FormatComparison("Barrier Regen", BarrierRegenPerSecondBonus, BarrierRegenPerSecondBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierOnDamageEffect)) return FormatPercentComparison("Barrier On Damage", BarrierOnDamageRatio, BarrierOnDamageRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.ExperienceGainEffect)) return FormatPercentComparison("XP Gain", ExperienceGainMultiplierBonus, ExperienceGainMultiplierBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.AreaRadiusEffect)) return FormatComparison("Area Radius", AreaRadiusBonus, AreaRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.WeaponUnlockEffect))
            {
                string displayName = choice != null &&
                    TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata) &&
                    !string.IsNullOrWhiteSpace(metadata.DisplayName)
                        ? metadata.DisplayName
                        : ShortWeaponName(effect.TargetId.Value);
                return "Unlocks " + displayName;
            }
            if (choice != null && IsPickupMagnetUpgrade(choice)) return "Improves collector effects.";
            return "Adds behavior.";
        }

        private string FormatPulseIntervalComparison(float reduction)
        {
            float before = CurrentPickupMagnetPulseIntervalSeconds;
            float baseInterval = Mathf.Max(1f, CurrentTuning.PickupMagnetPulseBaseIntervalSeconds);
            float minimum = Mathf.Max(1f, CurrentTuning.PickupMagnetPulseMinimumIntervalSeconds);
            float after = before > 0f
                ? Mathf.Max(minimum, before - Mathf.Max(0f, reduction))
                : Mathf.Max(minimum, baseInterval - Mathf.Max(0f, reduction));
            string beforeLabel = before > 0f ? before.ToString("0.#") + "s" : "inactive";
            return "Pulse Interval: " + beforeLabel + " -> " + after.ToString("0.#") + "s";
        }

        private static string FormatComparison(string label, float before, float after, string format = "0.#")
        {
            return label + ": " + before.ToString(format) + " -> " + after.ToString(format);
        }

        private static string FormatComparison(string label, int before, int after, string format = "0")
        {
            return label + ": " + before.ToString(format) + " -> " + after.ToString(format);
        }

        private static string FormatPercentComparison(string label, float before, float after)
        {
            return label + ": " + before.ToString("P0") + " -> " + after.ToString("P0");
        }

        private string ResolveUpgradeRequirementHint(RunUpgradeDefinition choice, SurvivorsRunUpgradeMetadata metadata, SurvivorsRunUpgradeCategory category)
        {
            if (choice == null || metadata == null)
            {
                return string.Empty;
            }

            if (category == SurvivorsRunUpgradeCategory.Evolution)
            {
                string weapon = ShortWeaponName(metadata.AffectedContentId);
                string rank = string.IsNullOrWhiteSpace(metadata.RequiredUpgradeId)
                    ? "max weapon rank"
                    : ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredUpgradeId)) + " rank " + ResolveRequiredUpgradeRank(metadata).ToString();
                string passive = string.IsNullOrWhiteSpace(metadata.RequiredPassiveUpgradeId)
                    ? "matching passive"
                    : ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredPassiveUpgradeId));
                return "Evolution path: " + weapon + " needs " + rank + " plus " + passive + ".";
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredUpgradeId))
            {
                return "Requires " + ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredUpgradeId)) + " rank " + ResolveRequiredUpgradeRank(metadata).ToString() + ".";
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredPassiveUpgradeId))
            {
                return "Requires " + ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredPassiveUpgradeId)) + ".";
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredOwnedWeaponId))
            {
                return "Requires " + ShortWeaponName(metadata.RequiredOwnedWeaponId) + ".";
            }

            if (IsPickupMagnetUpgrade(choice))
            {
                return "Pickup build: improves gem reach without bypassing draft pacing.";
            }

            return string.Empty;
        }

        private SurvivorsUiTheme ActiveUiTheme
        {
            get
            {
                EnsureUiTheme();
                return uiTheme;
            }
        }

        private void EnsureUiTheme()
        {
            if (uiTheme == null)
            {
                uiTheme = SurvivorsUiTheme.CreateDefault();
            }

            if (_availableUiThemes.Count == 0)
            {
                _availableUiThemes.Add(uiTheme);
                _selectedUiThemeIndex = 0;
            }
            else if (_selectedUiThemeIndex < 0 || _selectedUiThemeIndex >= _availableUiThemes.Count)
            {
                _selectedUiThemeIndex = 0;
                uiTheme = _availableUiThemes[0] ?? SurvivorsUiTheme.CreateDefault();
            }
        }

        private void ResetHudStyles()
        {
            _hudTitleStyle = null;
            _hudLabelStyle = null;
            _hudSmallStyle = null;
            _damagePopupStyle = null;
            _playerDamagePopupStyle = null;
            _lowHealthStyle = null;
            _majorThreatWarningStyle = null;
            _rewardFeedbackStyle = null;
            _draftTitleStyle = null;
            _draftCardNameStyle = null;
            _draftCardMetaStyle = null;
            _draftCardDescriptionStyle = null;
            _draftCardHotkeyStyle = null;
            _menuTitleStyle = null;
            _menuTabStyle = null;
            _transparentButtonStyle = null;
        }

        private IReadOnlyList<string> ResolvePlayerHudLines()
        {
            var lines = new List<string>(6)
            {
                CurrentRunMilestoneHudLabel,
                ResolveBuildSlotHudLabel(),
                "Weapons: " + FormatActiveWeaponList(),
                $"Pickup {CurrentPickupAttractRange:0.#}   Pull {CurrentPickupAttractionSpeed:0.#}   Pulse {FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds)}"
            };

            string evolution = ResolveEvolutionObjectiveHudLabel();
            if (!string.IsNullOrWhiteSpace(evolution))
            {
                lines.Add(evolution);
            }

            lines.Add("Tab/B Build Menu");
            return lines;
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

        private IReadOnlyList<string> ResolveBuildMenuCurrentBuildLines()
        {
            var lines = new List<string>(ResolveBuildHudSummaryLines());
            if (lines.Count == 0)
            {
                lines.Add("No build data yet.");
            }

            return lines;
        }

        private IReadOnlyList<string> ResolveBuildMenuStatsLines()
        {
            return new List<string>
            {
                $"Damage +{DamageBonus + PersistentDamageBonus + RelicDamageBonus:0.#}   Surge +{StreakSurgeDamageBonus + RoamingCacheSurgeDamageBonus + ArenaShrineSurgeDamageBonus + WaystoneFocusDamageBonus + WaystoneChainSurgeDamageBonus + HordeRushClearSurgeDamageBonus + WeaponLoadoutSurgeDamageBonus + PassiveLoadoutSurgeDamageBonus + BossRelicSurgeDamageBonus + GemRushDamageBonus + EvolutionChainSurgeDamageBonus + EndlessSurgeDamageBonus:0.#}",
                $"Cooldown {WeaponCooldownSeconds:0.00}s   Move speed {PlayerMoveSpeed:0.0}",
                $"Health {CurrentHealth:0}/{MaxHealth:0}   Barrier {BarrierValue:0.#}/{BarrierCapacity:0.#}   Armor: contact safety {CurrentTuning.PlayerContactInvulnerabilitySeconds:0.##}s",
                $"Pickup radius {CurrentPickupAttractRange:0.#}   Magnet range {CurrentPickupAttractRange:0.#}   Magnet speed {CurrentPickupAttractionSpeed:0.#}   Pulse {FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds)}",
                $"XP gain +{ExperienceGainMultiplierBonus + PassiveLoadoutSurgeExperienceGainMultiplierBonus:P0}   Draft luck +{DraftLuckBonus:P0}",
                $"Area/radius +{AreaRadiusBonus:0.#}   Orbit +{OrbitRadiusBonus:0.#}   Death nova {DeathNovaDamage:0.#}/{DeathNovaRadius:0.#}",
                $"Projectiles fan +{ProjectileFanBonus}   pierce +{ProjectilePierceBonus}   chain +{ProjectileChainBonus}   fork +{ProjectileForkBonus}   return +{ProjectileReturnBonus}",
                $"Payloads +{PayloadCountBonus}   payload radius +{PayloadExplosionRadiusBonus:0.#}   trigger +{PayloadTriggerRadiusBonus:0.#}",
                $"Status poison {PoisonDamageRatio:P0}   bleed {BleedDamageRatio:P0}   execute {ExecuteThresholdNormalized:P0}   lifesteal {LifestealRatio:P0}",
                $"Crit {CriticalChanceNormalized:P0} x{CriticalDamageMultiplier:0.0}"
            };
        }

        private IReadOnlyList<string> ResolveBuildMenuRunInfoLines()
        {
            string remaining = IsEndlessRun
                ? "Endless"
                : FormatRunTime(Mathf.Max(0f, CurrentTuning.SurvivalVictoryTimeSeconds - RunTimeSeconds));
            return new List<string>
            {
                $"Mode: {CurrentRunModeDisplayName} ({BasicSurvivorsGame.GetPacingProfileDisplayName(CurrentPacingProfile)})",
                $"Elapsed {FormatRunTime(RunTimeSeconds)}   Remaining {remaining}",
                $"Milestone: {CurrentRunMilestoneHudLabel}",
                $"Phase {ResolveRunPhaseHudLabel()} +{RunEscalationLevel}   Level {Level}   Kills {KilledCount}",
                $"Enemies {ActiveEnemyCount}/{CurrentEnemyMaximumAlive}   Elites {ActiveEliteCount}   Minibosses {ActiveMinibossCount}   Bosses {ActiveBossCount}",
                $"Run rewards: {BloodShardsEarnedThisRun + BonusBloodShardsEarnedThisRun} {CurrencyDisplayName}, {LegacyExperienceEarnedThisRun + BonusLegacyExperienceEarnedThisRun} {ProgressionDisplayName}",
                $"Meta bank: {MetaBloodShards} {CurrencyDisplayName}   {ProgressionDisplayName} {LifetimeLegacyExperience}",
                $"Rerolls {DraftRerollsRemaining}   Banishes {DraftBanishesRemaining}   Skip reward +{DraftSkipBloodShards} {CurrencyRewardLabel}",
                $"Waystones {WaystoneDiscoveryCount}   Roaming caches {RoamingCacheDropCount}   Arena trials {ArenaShrineTrialCount}"
            };
        }

        private IReadOnlyList<string> ResolveBuildMenuControlsLines()
        {
            return new List<string>
            {
                "Move: WASD or arrow keys",
                "Arc Step: Space",
                "Draft choice: mouse or 1/2/3",
                "Draft tools: R reroll, S skip, Shift+1/2/3 banish",
                "Build menu: Esc, Tab, or B opens and closes",
                "Build menu tabs: 1 Current Build, 2 Stats, 3 Run Info, 4 Controls",
                "Debug overlay: F1",
                "Victory: C continues Standard into endless if available",
                "Result screen: Restart Same or Change Mode buttons"
            };
        }

        private static BuildMenuTab ClampBuildMenuTab(int tabIndex)
        {
            if (tabIndex <= 0) return BuildMenuTab.CurrentBuild;
            if (tabIndex == 1) return BuildMenuTab.Stats;
            if (tabIndex == 2) return BuildMenuTab.RunInfo;
            return BuildMenuTab.Controls;
        }

        private static string FormatBuildMenuTabLabel(BuildMenuTab tab)
        {
            switch (tab)
            {
                case BuildMenuTab.Stats:
                    return "Stats";
                case BuildMenuTab.RunInfo:
                    return "Run Info";
                case BuildMenuTab.Controls:
                    return "Controls";
                default:
                    return "Current Build";
            }
        }

        private Color ResolveRewardTitleAccentColor()
        {
            if (IsRelicChoiceOpen)
            {
                return ActiveUiTheme.GetRarityAccentColor("Relic", new Color(1f, 0.84f, 0.42f));
            }

            if (_currentDraft == null || _currentDraft.Choices.Count == 0)
            {
                return ActiveUiTheme.GetRarityAccentColor("Common", Color.white);
            }

            for (int i = 0; i < _currentDraft.Choices.Count; i++)
            {
                RunUpgradeDefinition choice = _currentDraft.Choices[i];
                if (choice != null && ResolveCurrentUpgradeCategory(choice) == SurvivorsRunUpgradeCategory.Evolution)
                {
                    return ActiveUiTheme.GetRarityAccentColor("Evolution", new Color(1f, 0.38f, 0.56f));
                }
            }

            RunUpgradeRarity rarity = ResolveHighestRarity(_currentDraft.Choices);
            return ActiveUiTheme.GetRarityAccentColor(rarity, ResolveRarityAccentColor(rarity));
        }

        private static void DrawDimOverlay(float alpha)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        private static Rect ResolveCenteredPanelRect(float maxWidth, float maxHeight, float minWidth, float minHeight, float margin)
        {
            float safeMargin = Mathf.Max(8f, margin);
            float availableWidth = Mathf.Max(220f, Screen.width - safeMargin * 2f);
            float availableHeight = Mathf.Max(220f, Screen.height - safeMargin * 2f);
            float width = Mathf.Min(Mathf.Max(1f, maxWidth), availableWidth);
            float height = Mathf.Min(Mathf.Max(1f, maxHeight), availableHeight);
            if (width < minWidth)
            {
                width = availableWidth;
            }

            if (height < minHeight)
            {
                height = availableHeight;
            }

            return new Rect(
                Mathf.Max(safeMargin, Screen.width * 0.5f - width * 0.5f),
                Mathf.Max(safeMargin, Screen.height * 0.5f - height * 0.5f),
                width,
                height);
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        private void DrawRunResultOverlay(bool victory)
        {
            DrawDimOverlay(0.72f);
            Rect rect = ResolveCenteredPanelRect(760f, 700f, 320f, 400f, 20f);
            Color accent = ActiveUiTheme.GetHudAccentColor(new Color(0.2f, 0.78f, 1f));
            DrawSolidRect(rect, new Color(0.014f, 0.018f, 0.027f, 0.97f));
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 4f), new Color(accent.r, accent.g, accent.b, 0.95f));
            string title = string.IsNullOrWhiteSpace(_lastRunSummaryTitle)
                ? ActiveUiTheme.runSummaryTitle + " - " + (victory ? "Victory" : "Defeat")
                : _lastRunSummaryTitle;
            GUI.Label(new Rect(rect.x + 24f, rect.y + 18f, rect.width - 48f, 34f), title, _menuTitleStyle);

            IReadOnlyList<string> lines = LastRunSummaryLines;
            int lineCount = lines == null ? 0 : lines.Count;
            Rect viewRect = new Rect(rect.x + 24f, rect.y + 64f, rect.width - 48f, rect.height - 130f);
            float lineHeight = 22f;
            float summaryHeight = Mathf.Max(58f, Mathf.Max(1, lineCount) * lineHeight + 12f);
            float contentWidth = Mathf.Max(1f, viewRect.width - 18f);
            float classOptionsY = summaryHeight + 8f;
            float metaOptionsY = classOptionsY + 82f;
            float contentHeight = Mathf.Max(viewRect.height, metaOptionsY + 132f);
            _runSummaryScrollPosition = GUI.BeginScrollView(viewRect, _runSummaryScrollPosition, new Rect(0f, 0f, contentWidth, contentHeight));
            if (lineCount == 0)
            {
                GUI.Label(new Rect(0f, 0f, contentWidth, 22f), $"Rewards {BloodShardsEarnedThisRun} {CurrencyRewardLabel} / {LegacyExperienceEarnedThisRun} {ProgressionDisplayName}", _hudLabelStyle);
            }
            else
            {
                for (int i = 0; i < lineCount; i++)
                {
                    GUI.Label(new Rect(0f, i * lineHeight, contentWidth, lineHeight), lines[i], _hudSmallStyle);
                }
            }

            DrawResultClassOptions(new Rect(0f, classOptionsY, contentWidth, 66f));
            DrawResultMetaUpgradeOptions(new Rect(0f, metaOptionsY, contentWidth, 96f));
            GUI.EndScrollView();

            float buttonY = rect.yMax - 48f;
            if (victory)
            {
                int buttonCount = CurrentTuning.EndlessContinuationEnabled ? 3 : 2;
                float gap = 10f;
                float buttonWidth = (rect.width - 48f - gap * (buttonCount - 1)) / buttonCount;
                float x = rect.x + 24f;
                if (CurrentTuning.EndlessContinuationEnabled && GUI.Button(new Rect(x, buttonY, buttonWidth, 36f), ActiveUiTheme.continueButtonLabel))
                {
                    PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
                    ContinueAfterVictory();
                }

                if (CurrentTuning.EndlessContinuationEnabled)
                {
                    x += buttonWidth + gap;
                }

                if (GUI.Button(new Rect(x, buttonY, buttonWidth, 36f), ActiveUiTheme.restartSameButtonLabel))
                {
                    PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
                    RestartRun();
                }

                x += buttonWidth + gap;
                if (GUI.Button(new Rect(x, buttonY, buttonWidth, 36f), ActiveUiTheme.changeModeButtonLabel))
                {
                    PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
                    OpenRunModeSelection();
                }
            }
            else
            {
                float gap = 10f;
                float buttonWidth = (rect.width - 48f - gap) * 0.5f;
                if (GUI.Button(new Rect(rect.x + 24f, buttonY, buttonWidth, 36f), ActiveUiTheme.restartSameButtonLabel))
                {
                    PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
                    RestartRun();
                }

                if (GUI.Button(new Rect(rect.x + 24f + buttonWidth + gap, buttonY, buttonWidth, 36f), ActiveUiTheme.changeModeButtonLabel))
                {
                    PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);
                    OpenRunModeSelection();
                }
            }
        }

        private void DrawResultClassOptions(Rect rect)
        {
            IReadOnlyList<SurvivorsClassDefinition> options = ResolveResultClassOptions(ResultClassOptionCount);
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20f), "Next Run Class", _hudLabelStyle);
            if (options.Count == 0)
            {
                GUI.Label(new Rect(rect.x, rect.y + 24f, rect.width, 20f), "No class definitions found.", _hudSmallStyle);
                return;
            }

            float gap = 8f;
            float buttonWidth = (rect.width - gap * (options.Count - 1)) / options.Count;
            for (int i = 0; i < options.Count; i++)
            {
                SurvivorsClassDefinition option = options[i];
                bool unlocked = IsResultClassUnlocked(option);
                bool selected = option != null && string.Equals(SelectedClassId, option.Id, StringComparison.Ordinal);
                Rect buttonRect = new Rect(rect.x + i * (buttonWidth + gap), rect.y + 24f, buttonWidth, 42f);

                bool previousEnabled = GUI.enabled;
                Color previousBackgroundColor = GUI.backgroundColor;
                GUI.enabled = previousEnabled && unlocked && !selected;
                GUI.backgroundColor = selected
                    ? new Color(0.48f, 0.84f, 0.62f)
                    : (unlocked ? new Color(0.8f, 0.58f, 1f) : new Color(0.38f, 0.38f, 0.38f));

                if (GUI.Button(buttonRect, FormatResultClassOptionLabel(i, option)))
                {
                    TrySelectResultClass(i);
                }

                GUI.backgroundColor = previousBackgroundColor;
                GUI.enabled = previousEnabled;
            }
        }

        private void DrawResultMetaUpgradeOptions(Rect rect)
        {
            IReadOnlyList<SurvivorsPersistentUpgradeDefinition> options = ResolveResultMetaUpgradeOptions(ResultMetaUpgradeOptionCount);
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20f), $"Meta Upgrades - {MetaBloodShards} {CurrencyDisplayName} banked", _hudLabelStyle);
            if (options.Count == 0)
            {
                GUI.Label(new Rect(rect.x, rect.y + 24f, rect.width, 20f), $"No affordable meta upgrades yet. Bank more {CurrencyDisplayName} from runs, elites, bosses, or skips.", _hudSmallStyle);
                return;
            }

            for (int i = 0; i < options.Count; i++)
            {
                Rect buttonRect = new Rect(rect.x, rect.y + 24f + i * 28f, rect.width, 24f);
                if (GUI.Button(buttonRect, FormatPersistentUpgradeOptionLabel(i, options[i])))
                {
                    TryPurchaseResultMetaUpgrade(i);
                    return;
                }
            }
        }

        private string FormatActiveWeaponList()
        {
            IReadOnlyList<string> weaponIds = ActiveWeaponIds;
            if (weaponIds == null || weaponIds.Count == 0)
            {
                return "None";
            }

            var labels = new List<string>(weaponIds.Count);
            for (int i = 0; i < weaponIds.Count; i++)
            {
                labels.Add(ShortWeaponName(weaponIds[i]));
            }

            return string.Join(", ", labels);
        }

        private string FormatRunSummaryUpgradeList(HashSet<string> upgradeIds, bool includeRanks)
        {
            if (upgradeIds == null || upgradeIds.Count == 0 || _upgradeCatalog == null)
            {
                return "None yet";
            }

            var labels = new List<string>(upgradeIds.Count);
            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition == null || !upgradeIds.Contains(definition.Id.Value))
                {
                    continue;
                }

                string label = ResolveUpgradeDisplayName(definition.Id);
                if (includeRanks && _upgradeState != null)
                {
                    int rank = Mathf.Max(1, _upgradeState.GetRank(definition.Id));
                    label += " " + rank.ToString() + "/" + definition.MaxRank.ToString();
                }

                labels.Add(label);
            }

            return labels.Count == 0 ? "None yet" : string.Join(", ", labels);
        }

        private void AppendSelectedUpgradeRankLines(List<string> lines)
        {
            if (lines == null || _upgradeCatalog == null || _upgradeState == null)
            {
                return;
            }

            bool addedHeader = false;
            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                int rank = definition == null ? 0 : _upgradeState.GetRank(definition.Id);
                if (rank <= 0)
                {
                    continue;
                }

                if (!addedHeader)
                {
                    lines.Add("Ranks");
                    addedHeader = true;
                }

                lines.Add("  " + FormatDebugRankLine(definition, rank));
            }
        }

        private string FormatDebugRankLine(RunUpgradeDefinition definition, int rank)
        {
            if (definition == null)
            {
                return "Missing upgrade";
            }

            string name = ResolveUpgradeDisplayName(definition.Id);
            SurvivorsRunUpgradeCategory category = ResolveCurrentUpgradeCategory(definition);
            string affected = TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata)
                ? ShortWeaponName(metadata.AffectedContentId)
                : "Build";
            return $"{name} [{category}] rank {rank}/{definition.MaxRank} ({definition.Rarity}) - {affected}";
        }

        private string FormatDebugUpgradeLine(int index, RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return index >= 0 ? $"{index + 1}. Missing upgrade" : "Missing upgrade";
            }

            string prefix = index >= 0 ? $"{index + 1}. " : string.Empty;
            string name = ResolveUpgradeDisplayName(definition.Id);
            SurvivorsRunUpgradeCategory category = ResolveCurrentUpgradeCategory(definition);
            int currentRank = _upgradeState == null ? 0 : _upgradeState.GetRank(definition.Id);
            int nextRank = Mathf.Min(definition.MaxRank, currentRank + 1);
            string description = TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata)
                ? metadata.Description
                : name;
            string affected = metadata == null ? "Build" : ShortWeaponName(metadata.AffectedContentId);
            return $"{prefix}{name} [{FormatUpgradeCategoryLabel(category)}/{definition.Rarity}] rank {currentRank}->{nextRank}/{definition.MaxRank} - {affected}: {description}";
        }

        private string FormatUpgradeChoiceLabel(int index, RunUpgradeDefinition choice)
        {
            if (choice == null)
            {
                return (index + 1).ToString() + ". Missing Choice";
            }

            SurvivorsRunUpgradeCategory category = ResolveCurrentUpgradeCategory(choice);
            string name = ResolveUpgradeDisplayName(choice.Id);
            string affected = ResolveUpgradeAffectedLabel(choice);
            string description = TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata)
                ? metadata.Description
                : name;
            int currentRank = _upgradeState == null ? 0 : _upgradeState.GetRank(choice.Id);
            int nextRank = Mathf.Min(choice.MaxRank, currentRank + 1);
            return $"{index + 1}. {choice.Rarity} {FormatUpgradeCategoryLabel(category)}: {name}\n{affected}  Rank {currentRank}->{nextRank}/{choice.MaxRank} - {description}";
        }

        private static string FormatUpgradeCategoryLabel(SurvivorsRunUpgradeCategory category)
        {
            switch (category)
            {
                case SurvivorsRunUpgradeCategory.Weapon:
                    return "Weapon";
                case SurvivorsRunUpgradeCategory.WeaponUpgrade:
                    return "Weapon Upgrade";
                case SurvivorsRunUpgradeCategory.Passive:
                    return "Passive";
                case SurvivorsRunUpgradeCategory.PassiveUpgrade:
                    return "Passive Upgrade";
                case SurvivorsRunUpgradeCategory.Mutation:
                    return "Mutation";
                case SurvivorsRunUpgradeCategory.Evolution:
                    return "Evolution";
                default:
                    return category.ToString();
            }
        }

        private string FormatRelicChoiceLabel(int index, SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return (index + 1).ToString() + ". Missing Relic";
            }

            return $"{index + 1}. Boss Relic: {relic.DisplayName}\n{FormatRelicEffectSummary(relic)}";
        }

        private string FormatRelicEffectSummary(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return "Missing effect";
            }

            string target = ShortWeaponName(relic.TargetId);
            switch (relic.EffectKind)
            {
                case SurvivorsRelicEffectKind.DamageBonus:
                    return $"+{relic.Amount:0.#} damage to {target}";
                case SurvivorsRelicEffectKind.CooldownMultiplier:
                    return $"{Mathf.Abs(relic.Amount):P0} faster cooldown on {target}";
                case SurvivorsRelicEffectKind.PickupRange:
                    return $"+{relic.Amount:0.#} pickup range";
                default:
                    return target;
            }
        }

        private static string ResolveRewardKindLabel(SurvivorsRewardSelectionKind selectionKind)
        {
            switch (selectionKind)
            {
                case SurvivorsRewardSelectionKind.LevelUp:
                    return "Level Up";
                case SurvivorsRewardSelectionKind.BossRelic:
                    return "Boss Relic";
                case SurvivorsRewardSelectionKind.EliteUpgrade:
                    return "Elite Reward";
                case SurvivorsRewardSelectionKind.BossUpgrade:
                    return "Boss Reward";
                default:
                    return "Reward";
            }
        }

        private static Color ResolveRarityAccentColor(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common:
                    return new Color(0.78f, 0.84f, 0.88f);
                case RunUpgradeRarity.Uncommon:
                    return new Color(0.34f, 0.94f, 0.56f);
                case RunUpgradeRarity.Rare:
                    return new Color(0.34f, 0.7f, 1f);
                case RunUpgradeRarity.Epic:
                    return new Color(0.9f, 0.46f, 1f);
                case RunUpgradeRarity.Legendary:
                    return new Color(1f, 0.76f, 0.2f);
                default:
                    return Color.white;
            }
        }

        private static Color ResolveRelicAccentColor(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return Color.white;
            }

            switch (relic.EffectKind)
            {
                case SurvivorsRelicEffectKind.DamageBonus:
                    return new Color(1f, 0.45f, 0.28f);
                case SurvivorsRelicEffectKind.CooldownMultiplier:
                    return new Color(0.34f, 0.88f, 1f);
                case SurvivorsRelicEffectKind.PickupRange:
                    return new Color(0.42f, 1f, 0.6f);
                default:
                    return new Color(1f, 0.76f, 0.2f);
            }
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
            if (_rewardSelectionKind == SurvivorsRewardSelectionKind.BossRelic)
            {
                return "Choose a Boss Relic";
            }

            if (_rewardSelectionKind == SurvivorsRewardSelectionKind.EliteUpgrade)
            {
                return "Elite Reward";
            }

            if (_rewardSelectionKind == SurvivorsRewardSelectionKind.BossUpgrade)
            {
                return "Boss Evolution Reward";
            }

            return "Level Up";
        }

        private void EnsureHudStyles()
        {
            if (_hudTitleStyle != null)
            {
                return;
            }

            _hudTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.94f, 1f) }
            };
            _hudLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            _hudSmallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.78f, 0.88f, 0.95f) },
                wordWrap = true
            };
            _damagePopupStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.9f, 0.35f) }
            };
            _playerDamagePopupStyle = new GUIStyle(_damagePopupStyle)
            {
                fontSize = 17,
                normal = { textColor = new Color(1f, 0.24f, 0.18f) }
            };
            _lowHealthStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.3f, 0.24f) }
            };
            _majorThreatWarningStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.78f, 0.24f) }
            };
            _rewardFeedbackStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            _draftTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _draftCardNameStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            _draftCardMetaStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.92f, 1f) },
                wordWrap = true
            };
            _draftCardDescriptionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                normal = { textColor = new Color(0.88f, 0.93f, 0.97f) },
                wordWrap = true
            };
            _draftCardHotkeyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.black }
            };
            _menuTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _menuTabStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            _transparentButtonStyle = new GUIStyle(GUIStyle.none);
        }

        private void DrawHudBar(Rect rect, string label, float value, Color fill)
        {
            GUI.Box(rect, GUIContent.none);
            Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(value), rect.height - 4f);
            Color oldColor = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUI.Label(rect, label + " " + Mathf.RoundToInt(Mathf.Clamp01(value) * 100f).ToString() + "%", _hudSmallStyle);
        }

        private void DrawTopCenterTimerHud()
        {
            Rect panel = ResolveTopCenterTimerRect();
            string label = ResolveTopCenterTimerHudLabel();
            if (string.IsNullOrWhiteSpace(label))
            {
                return;
            }

            Color oldColor = GUI.color;
            GUI.color = new Color(0.015f, 0.02f, 0.026f, 0.78f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.78f, 1f, 0.92f);
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(panel, label, _majorThreatWarningStyle);
            GUI.color = oldColor;
        }

        private Rect ResolveTopCenterTimerRect()
        {
            float width = Mathf.Min(360f, Mathf.Max(240f, Screen.width - 32f));
            return new Rect(Screen.width * 0.5f - width * 0.5f, 12f, width, 36f);
        }

        private string ResolveTopCenterTimerHudLabel()
        {
            if (!_runStarted)
            {
                return string.Empty;
            }

            float target = Mathf.Max(0f, CurrentTuning.SurvivalVictoryTimeSeconds);
            string elapsed = FormatRunTime(RunTimeSeconds);
            string mode = string.IsNullOrWhiteSpace(CurrentRunModeDisplayName)
                ? BasicSurvivorsGame.GetPacingProfileDisplayName(CurrentPacingProfile)
                : CurrentRunModeDisplayName;
            if (IsEndlessRun || target <= 0f)
            {
                return mode + "  TIME " + elapsed + "  ENDLESS";
            }

            return mode + "  TIME " + elapsed + "  LEFT " + FormatRunTime(Mathf.Max(0f, target - RunTimeSeconds));
        }

        private void DrawLowHealthWarning()
        {
            if (!IsLowHealthWarningActive)
            {
                return;
            }

            float pulse = 0.75f + Mathf.Sin(Time.unscaledTime * 7f) * 0.25f;
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 0.04f, 0.04f, 0.12f + 0.08f * pulse);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, 12f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, Screen.height - 12f, Screen.width, 12f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, 0f, 12f, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - 12f, 0f, 12f, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.95f);
            GUI.Label(new Rect(24, 370, 318, 22), "LOW HEALTH", _lowHealthStyle);
            GUI.color = oldColor;
        }

        private void DrawMajorThreatWarning()
        {
            if (!IsMajorThreatWarningActive)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 8f) * 0.28f;
            float y = IsTopCenterTimerVisible ? 58f : 24f;
            Rect panel = new Rect(Screen.width * 0.5f - 184f, y, 368f, 52f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.22f, 0.05f, 0.04f, 0.56f + 0.18f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            GUI.Label(panel, $"{CurrentMajorThreatWarningLabel}  {Mathf.CeilToInt(MajorThreatWarningRemainingSeconds)}s", _majorThreatWarningStyle);
            GUI.color = oldColor;
        }

        private void DrawHordeRushWarning()
        {
            if (!IsHordeRushWarningActive)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 8.5f) * 0.28f;
            float baseY = IsTopCenterTimerVisible ? 58f : 24f;
            float y = IsMajorThreatWarningActive ? baseY + 60f : baseY;
            Rect panel = new Rect(Screen.width * 0.5f - 170f, y, 340f, 46f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.28f, 0.08f, 0.02f, 0.52f + 0.18f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            GUI.Label(panel, $"{CurrentHordeRushWarningLabel}  {Mathf.CeilToInt(HordeRushWarningRemainingSeconds)}s", _majorThreatWarningStyle);
            GUI.color = oldColor;
        }

        private void DrawMajorThreatHealthBar()
        {
            DrawBossLifeBars();
            DrawOverheadThreatLifeBars();
        }

        private void DrawBossLifeBars()
        {
            float width = Mathf.Min(420f, Mathf.Max(260f, Screen.width - 48f));
            float x = Mathf.Max(24f, Screen.width - width - 24f);
            float y = 24f;
            if (IsMajorThreatWarningActive)
            {
                y += 60f;
            }

            if (IsHordeRushWarningActive)
            {
                y += 52f;
            }

            int drawn = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive || !enemy.ShowBossLifeBar)
                {
                    continue;
                }

                Rect panel = new Rect(x, y + drawn * 50f, width, 46f);
                Color oldColor = GUI.color;
                GUI.color = new Color(0.03f, 0.02f, 0.02f, 0.66f);
                GUI.DrawTexture(panel, Texture2D.whiteTexture);
                GUI.color = oldColor;
                string label = (string.IsNullOrWhiteSpace(enemy.DisplayName) ? ResolveMajorThreatHealthFallbackLabel(enemy.Role) : enemy.DisplayName) + " HP";
                DrawHudBar(new Rect(panel.x + 10f, panel.y + 13f, panel.width - 20f, 20f), label, enemy.HealthFraction, ResolveMajorRewardDropColor(enemy.Role));
                drawn++;
            }
        }

        private void DrawOverheadThreatLifeBars()
        {
            if (_camera == null)
            {
                return;
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive || !enemy.ShowOverheadLifeBar)
                {
                    continue;
                }

                Vector3 world = enemy.transform.position + Vector3.up * Mathf.Max(1.2f, enemy.Radius + 0.95f);
                Vector3 screen = _camera.WorldToScreenPoint(world);
                if (screen.z <= 0f)
                {
                    continue;
                }

                float width = enemy.Role == SurvivorsEnemyRole.Miniboss ? 190f : 156f;
                float height = 28f;
                float x = Mathf.Clamp(screen.x - width * 0.5f, 12f, Mathf.Max(12f, Screen.width - width - 12f));
                float y = Mathf.Clamp(Screen.height - screen.y, 58f, Mathf.Max(58f, Screen.height - height - 12f));
                Rect panel = new Rect(x, y, width, height);
                Color oldColor = GUI.color;
                GUI.color = new Color(0.025f, 0.018f, 0.02f, 0.72f);
                GUI.DrawTexture(panel, Texture2D.whiteTexture);
                GUI.color = oldColor;
                string label = string.IsNullOrWhiteSpace(enemy.DisplayName) ? ResolveMajorThreatHealthFallbackLabel(enemy.Role) : enemy.DisplayName;
                DrawHudBar(new Rect(panel.x + 6f, panel.y + 6f, panel.width - 12f, 16f), label, enemy.HealthFraction, ResolveMajorRewardDropColor(enemy.Role));
            }
        }

        private void DrawOffscreenThreatMarker()
        {
            if (State != SurvivorsRunState.Playing)
            {
                return;
            }

            SurvivorsEnemyActor enemy = ResolveFirstOffscreenMajorThreatMarkerEnemy();
            if (enemy == null)
            {
                return;
            }

            Vector3 delta = enemy.transform.position - PlayerPosition;
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector2 screenDirection = new Vector2(delta.x, -delta.z).normalized;
            if (screenDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            const float markerWidth = 248f;
            const float markerHeight = 38f;
            const float edgeMargin = 18f;
            float halfWidth = Screen.width * 0.5f;
            float halfHeight = Screen.height * 0.5f;
            float xLimit = Mathf.Max(1f, halfWidth - markerWidth * 0.5f - edgeMargin);
            float yLimit = Mathf.Max(1f, halfHeight - markerHeight * 0.5f - edgeMargin);
            float xScale = Mathf.Abs(screenDirection.x) <= 0.001f ? float.MaxValue : xLimit / Mathf.Abs(screenDirection.x);
            float yScale = Mathf.Abs(screenDirection.y) <= 0.001f ? float.MaxValue : yLimit / Mathf.Abs(screenDirection.y);
            float scale = Mathf.Min(xScale, yScale);
            Vector2 center = new Vector2(halfWidth, halfHeight) + screenDirection * scale;
            center.x = Mathf.Clamp(center.x, edgeMargin + markerWidth * 0.5f, Screen.width - edgeMargin - markerWidth * 0.5f);
            center.y = Mathf.Clamp(center.y, 96f + markerHeight * 0.5f, Screen.height - edgeMargin - markerHeight * 0.5f);

            Rect panel = new Rect(center.x - markerWidth * 0.5f, center.y - markerHeight * 0.5f, markerWidth, markerHeight);
            Color threatColor = ResolveMajorRewardDropColor(enemy.Role);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.02f, 0.015f, 0.02f, 0.76f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(threatColor.r, threatColor.g, threatColor.b, 0.92f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 10f, panel.y + 7f, panel.width - 20f, panel.height - 12f), FormatOffscreenThreatMarkerLabel(enemy), _hudSmallStyle);
            GUI.color = oldColor;
        }

        private void DrawRewardSelectionFeedback()
        {
            if (_rewardFeedbackTimer <= 0f || string.IsNullOrWhiteSpace(_rewardFeedbackLabel))
            {
                return;
            }

            float width = Mathf.Min(500f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 9f) * 0.28f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 84f, width, 46f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.015f, 0.02f, 0.028f, 0.74f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(_rewardFeedbackColor.r, _rewardFeedbackColor.g, _rewardFeedbackColor.b, 0.2f + 0.14f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(_rewardFeedbackColor.r, _rewardFeedbackColor.g, _rewardFeedbackColor.b, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), _rewardFeedbackLabel, _rewardFeedbackStyle);
            GUI.color = oldColor;
        }

        private void DrawStreakRewardFeedback()
        {
            if (_streakRewardFeedbackTimer <= 0f || string.IsNullOrWhiteSpace(_streakRewardFeedbackLabel))
            {
                return;
            }

            float width = Mathf.Min(360f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 10f) * 0.28f;
            float x = Mathf.Max(16f, Screen.width - width - 18f);
            float y = Mathf.Min(144f, Mathf.Max(16f, Screen.height - 58f));
            Rect panel = new Rect(x, y, width, 44f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.02f, 0.018f, 0.024f, 0.72f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(_streakRewardFeedbackColor.r, _streakRewardFeedbackColor.g, _streakRewardFeedbackColor.b, 0.22f + 0.12f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(_streakRewardFeedbackColor.r, _streakRewardFeedbackColor.g, _streakRewardFeedbackColor.b, 0.95f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, 5f, panel.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), _streakRewardFeedbackLabel, _rewardFeedbackStyle);
            GUI.color = oldColor;
        }

        private void DrawClassUnlockRewardFeedback()
        {
            if (_classUnlockRewardFeedbackTimer <= 0f || string.IsNullOrWhiteSpace(_classUnlockRewardFeedbackLabel))
            {
                return;
            }

            float width = Mathf.Min(500f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.7f + Mathf.Sin(Time.unscaledTime * 8f) * 0.3f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 188f, width, 44f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.032f, 0.018f, 0.012f, 0.78f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.62f, 0.24f, 0.22f + 0.16f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.78f, 0.36f, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panel.x, panel.y + panel.height - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), _classUnlockRewardFeedbackLabel, _rewardFeedbackStyle);
            GUI.color = oldColor;
        }

        private void DrawExperienceComboFeedback()
        {
            if (_experienceComboFeedbackTimer <= 0f || string.IsNullOrWhiteSpace(_experienceComboFeedbackLabel))
            {
                return;
            }

            float width = Mathf.Min(420f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.74f + Mathf.Sin(Time.unscaledTime * 11f) * 0.26f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 136f, width, 42f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.018f, 0.024f, 0.03f, 0.7f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.22f, 0.9f, 1f, 0.2f + 0.16f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.22f, 0.9f, 1f, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y + panel.height - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 5f, panel.width - 24f, panel.height - 10f), _experienceComboFeedbackLabel, _rewardFeedbackStyle);
            GUI.color = oldColor;
        }

        private void DrawEvolutionReadyFeedback()
        {
            if (_evolutionReadyFeedbackTimer <= 0f || string.IsNullOrWhiteSpace(_evolutionReadyFeedbackLabel))
            {
                return;
            }

            float width = Mathf.Min(460f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.7f + Mathf.Sin(Time.unscaledTime * 9f) * 0.3f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 88f, width, 44f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.03f, 0.018f, 0.04f, 0.76f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.78f, 0.22f, 0.2f + 0.16f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.78f, 0.22f, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panel.x, panel.y + panel.height - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), _evolutionReadyFeedbackLabel, _rewardFeedbackStyle);
            GUI.color = oldColor;
        }

        private void DrawDamagePopups()
        {
            if (_damagePopups.Count == 0)
            {
                return;
            }

            Camera popupCamera = _camera != null ? _camera : Camera.main;
            if (popupCamera == null)
            {
                return;
            }

            for (int i = 0; i < _damagePopups.Count; i++)
            {
                SurvivorsDamagePopup popup = _damagePopups[i];
                float normalizedAge = Mathf.Clamp01(popup.ElapsedSeconds / DamagePopupLifetimeSeconds);
                Vector3 world = popup.WorldPosition + Vector3.up * (DamagePopupRiseHeight * normalizedAge);
                Vector3 screen = popupCamera.WorldToScreenPoint(world);
                if (screen.z <= 0f)
                {
                    continue;
                }

                GUIStyle style = popup.PlayerDamage ? _playerDamagePopupStyle : _damagePopupStyle;
                Color color = popup.Color;
                color.a *= 1f - normalizedAge;
                style.normal.textColor = color;
                Rect rect = new Rect(screen.x - 40f, Screen.height - screen.y - 14f, 80f, 24f);
                GUI.Label(rect, popup.Label, style);
            }
        }

        private void TickDamagePopups(float deltaTime)
        {
            if (_damagePopups.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _damagePopups.Count - 1; i >= 0; i--)
            {
                SurvivorsDamagePopup popup = _damagePopups[i];
                popup.ElapsedSeconds += dt;
                if (popup.ElapsedSeconds >= DamagePopupLifetimeSeconds)
                {
                    _damagePopups.RemoveAt(i);
                }
                else
                {
                    _damagePopups[i] = popup;
                }
            }
        }

        private void TickRewardFeedback(float deltaTime)
        {
            if (_rewardFeedbackTimer <= 0f)
            {
                return;
            }

            _rewardFeedbackTimer = Mathf.Max(0f, _rewardFeedbackTimer - Mathf.Max(0f, deltaTime));
            if (_rewardFeedbackTimer <= 0f)
            {
                _rewardFeedbackLabel = string.Empty;
            }
        }

        private void TickStreakRewardFeedback(float deltaTime)
        {
            if (_streakRewardFeedbackTimer <= 0f)
            {
                return;
            }

            _streakRewardFeedbackTimer = Mathf.Max(0f, _streakRewardFeedbackTimer - Mathf.Max(0f, deltaTime));
            if (_streakRewardFeedbackTimer <= 0f)
            {
                _streakRewardFeedbackLabel = string.Empty;
            }
        }

        private void TickClassUnlockRewardFeedback(float deltaTime)
        {
            if (_classUnlockRewardFeedbackTimer <= 0f)
            {
                return;
            }

            _classUnlockRewardFeedbackTimer = Mathf.Max(0f, _classUnlockRewardFeedbackTimer - Mathf.Max(0f, deltaTime));
            if (_classUnlockRewardFeedbackTimer <= 0f)
            {
                _classUnlockRewardFeedbackLabel = string.Empty;
            }
        }

        private void TickEvolutionReadyFeedback(float deltaTime)
        {
            if (_evolutionReadyFeedbackTimer <= 0f)
            {
                return;
            }

            _evolutionReadyFeedbackTimer = Mathf.Max(0f, _evolutionReadyFeedbackTimer - Mathf.Max(0f, deltaTime));
            if (_evolutionReadyFeedbackTimer <= 0f)
            {
                _evolutionReadyFeedbackLabel = string.Empty;
            }
        }

        private void TickExperienceComboFeedback(float deltaTime)
        {
            float dt = Mathf.Max(0f, deltaTime);
            if (_experienceComboTimer > 0f)
            {
                _experienceComboTimer = Mathf.Max(0f, _experienceComboTimer - dt);
                if (_experienceComboTimer <= 0f)
                {
                    _experienceComboPickupCount = 0;
                    _experienceComboAmount = 0;
                }
            }

            if (_experienceComboFeedbackTimer <= 0f)
            {
                return;
            }

            _experienceComboFeedbackTimer = Mathf.Max(0f, _experienceComboFeedbackTimer - dt);
            if (_experienceComboFeedbackTimer <= 0f)
            {
                _experienceComboFeedbackLabel = string.Empty;
            }
        }

        private void RecordPlayerDamageFeedback(DamageResult damage, Vector3 position)
        {
            float resolvedDamage = ResolveDamagePopupAmount(damage);
            if (resolvedDamage <= 0f)
            {
                return;
            }

            PlayerDamageFeedbackCount++;
            RecordDamagePopup(position, resolvedDamage, playerDamage: true, critical: damage.Critical.IsCritical);
        }

        private void RecordDamagePopup(Vector3 worldPosition, float amount, bool playerDamage, bool critical)
        {
            if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
            {
                return;
            }

            if (_damagePopups.Count >= DamagePopupLimit)
            {
                _damagePopups.RemoveAt(0);
            }

            int sequence = DamagePopupSpawnCount++;
            float lane = ((sequence % 5) - 2) * 0.18f;
            float lift = 1.1f + (sequence % 3) * 0.12f;
            string label = (playerDamage ? "-" : string.Empty) + Mathf.CeilToInt(amount).ToString();
            Color color = playerDamage
                ? new Color(1f, 0.24f, 0.18f, 1f)
                : critical
                    ? new Color(1f, 0.68f, 0.12f, 1f)
                    : new Color(1f, 0.92f, 0.42f, 1f);
            _damagePopups.Add(new SurvivorsDamagePopup(label, worldPosition + new Vector3(lane, lift, 0f), color, playerDamage));
        }

        private static float ResolveDamagePopupAmount(DamageResult damage)
        {
            if (damage == null || !damage.Succeeded)
            {
                return 0f;
            }

            double amount = damage.HealthDamage > 0d ? damage.HealthDamage : damage.FinalDamage;
            if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
            {
                return 0f;
            }

            return (float)Math.Min(float.MaxValue, amount);
        }

        private string ResolveWeaponHudLabel()
        {
            if (ActiveWeaponIds.Count == 0)
            {
                return "none";
            }

            const int maxShown = 4;
            string label = string.Empty;
            int shown = Mathf.Min(maxShown, ActiveWeaponIds.Count);
            for (int i = 0; i < shown; i++)
            {
                if (i > 0)
                {
                    label += ", ";
                }

                label += ShortWeaponName(ActiveWeaponIds[i]);
            }

            if (ActiveWeaponIds.Count > shown)
            {
                label += " +" + (ActiveWeaponIds.Count - shown).ToString();
            }

            return label;
        }

        private string ResolveBuildSlotHudLabel()
        {
            return $"Build W {ActiveWeaponCount}/{MaxWeaponSlots}   P {ActivePassiveCount}/{MaxPassiveSlots}   Evo {EvolvedWeaponCount}   Relic {SelectedRelicCount}/{ResolveTotalRelicCount()}";
        }

        private IReadOnlyList<string> ResolveBuildHudSummaryLines()
        {
            var lines = new List<string>(18)
            {
                $"Weapons {ActiveWeaponCount}/{MaxWeaponSlots}"
            };

            AppendWeaponBuildHudLines(lines);
            lines.Add($"Passives {ActivePassiveCount}/{MaxPassiveSlots}");
            AppendPassiveBuildHudLines(lines);
            lines.Add($"Evolutions {EvolvedWeaponCount}");
            AppendEvolutionBuildHudLines(lines);
            lines.Add($"Pickup range {CurrentPickupAttractRange:0.#}   Pull {CurrentPickupAttractionSpeed:0.#}   Pulse {FormatMetricTime(CurrentPickupMagnetPulseIntervalSeconds)}");
            lines.Add("Relics " + FormatSelectedRelicList());
            return lines;
        }

        private void AppendWeaponBuildHudLines(List<string> lines)
        {
            if (lines == null)
            {
                return;
            }

            IReadOnlyList<string> weaponIds = ActiveWeaponIds;
            if (weaponIds.Count == 0)
            {
                lines.Add("  none");
                return;
            }

            for (int i = 0; i < weaponIds.Count; i++)
            {
                string weaponId = weaponIds[i];
                var fragments = new List<string>(4);
                int hidden = AppendBuildRankFragments(
                    fragments,
                    metadata => string.Equals(metadata.AffectedContentId, weaponId, StringComparison.Ordinal) &&
                        (metadata.Category == SurvivorsRunUpgradeCategory.Weapon ||
                            metadata.Category == SurvivorsRunUpgradeCategory.WeaponUpgrade ||
                            metadata.Category == SurvivorsRunUpgradeCategory.Mutation),
                    3);

                if (hidden > 0)
                {
                    fragments.Add("+" + hidden.ToString());
                }

                string weaponName = ResolveWeaponBuildDisplayName(weaponId);
                lines.Add(fragments.Count == 0
                    ? "  " + weaponName
                    : "  " + weaponName + ": " + string.Join(", ", fragments));
            }
        }

        private void AppendPassiveBuildHudLines(List<string> lines)
        {
            if (lines == null)
            {
                return;
            }

            int added = 0;
            AppendSelectedBuildRanks(
                (definition, metadata, rank) =>
                {
                    if (metadata.Category != SurvivorsRunUpgradeCategory.Passive)
                    {
                        return;
                    }

                    added++;
                    lines.Add("  " + FormatPassiveBuildHudLine(definition, metadata, rank));
                });

            if (added == 0)
            {
                lines.Add("  none");
            }
        }

        private void AppendEvolutionBuildHudLines(List<string> lines)
        {
            if (lines == null)
            {
                return;
            }

            int added = 0;
            AppendSelectedBuildRanks(
                (definition, metadata, rank) =>
                {
                    if (metadata.Category != SurvivorsRunUpgradeCategory.Evolution)
                    {
                        return;
                    }

                    added++;
                    lines.Add("  " + FormatBuildRankFragment(definition, rank));
                });

            if (added > 0)
            {
                return;
            }

            string objective = ResolveEvolutionObjectiveHudLabel();
            lines.Add(string.IsNullOrWhiteSpace(objective) ? "  none" : "  " + objective);
        }

        private int AppendBuildRankFragments(List<string> fragments, Func<SurvivorsRunUpgradeMetadata, bool> matches, int maxFragments)
        {
            int hidden = 0;
            AppendSelectedBuildRanks(
                (definition, metadata, rank) =>
                {
                    if (matches == null || !matches(metadata))
                    {
                        return;
                    }

                    if (fragments != null && fragments.Count < maxFragments)
                    {
                        fragments.Add(FormatBuildRankFragment(definition, rank));
                    }
                    else
                    {
                        hidden++;
                    }
                });

            return hidden;
        }

        private void AppendSelectedBuildRanks(Action<RunUpgradeDefinition, SurvivorsRunUpgradeMetadata, int> append)
        {
            if (append == null || _upgradeCatalog == null || _upgradeState == null)
            {
                return;
            }

            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                int rank = definition == null ? 0 : _upgradeState.GetRank(definition.Id);
                if (rank <= 0 || !TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
                {
                    continue;
                }

                append(definition, metadata, rank);
            }
        }

        private string ResolveWeaponBuildDisplayName(string weaponId)
        {
            return TryResolveWeaponUpgradeDisplayName(weaponId, out string displayName, out _)
                ? displayName
                : ShortWeaponName(weaponId);
        }

        private bool TryResolveWeaponUpgradeDisplayName(string weaponId, out string displayName, out RunUpgradeId upgradeId)
        {
            displayName = string.Empty;
            upgradeId = default;
            if (_upgradeCatalog == null)
            {
                return false;
            }

            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _upgradeCatalog.Definitions[i];
                if (definition == null ||
                    !TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata) ||
                    metadata.Category != SurvivorsRunUpgradeCategory.Weapon ||
                    !string.Equals(metadata.AffectedContentId, weaponId, StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(metadata.DisplayName))
                {
                    continue;
                }

                displayName = metadata.DisplayName;
                upgradeId = definition.Id;
                return true;
            }

            return false;
        }

        private string FormatBuildRankFragment(RunUpgradeDefinition definition, int rank)
        {
            if (definition == null)
            {
                return "Missing";
            }

            return $"{ResolveUpgradeDisplayName(definition.Id)} {rank}/{Mathf.Max(1, definition.MaxRank)}";
        }

        private string FormatPassiveBuildHudLine(RunUpgradeDefinition definition, SurvivorsRunUpgradeMetadata metadata, int rank)
        {
            string line = FormatBuildRankFragment(definition, rank);
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.AffectedContentId))
            {
                return line;
            }

            return line + " - " + ShortWeaponName(metadata.AffectedContentId);
        }

        private void DrawBuildHudPanel()
        {
            float panelWidth = Mathf.Min(382f, Screen.width - 392f);
            if (panelWidth < 300f)
            {
                return;
            }

            IReadOnlyList<string> lines = ResolveBuildHudSummaryLines();
            int visibleLineCount = Mathf.Min(18, lines.Count);
            float lineHeight = 19f;
            float panelHeight = 42f + (visibleLineCount * lineHeight) + (lines.Count > visibleLineCount ? lineHeight : 0f);
            Rect panel = new Rect(Screen.width - panelWidth - 12f, 12f, panelWidth, panelHeight);
            GUI.Box(panel, string.Empty);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 22f), "Current Build", _hudTitleStyle);

            float y = panel.y + 34f;
            for (int i = 0; i < visibleLineCount; i++)
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, lineHeight), lines[i], _hudSmallStyle);
                y += lineHeight;
            }

            if (lines.Count > visibleLineCount)
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, lineHeight), "+" + (lines.Count - visibleLineCount).ToString() + " more", _hudSmallStyle);
            }
        }

        private string ResolveWaystoneCompassHudLabel()
        {
            if (CurrentTuning.WaystoneDiscoveryRadius <= 0f)
            {
                return $"Explore Waystones off   Found {WaystoneDiscoveryCount}";
            }

            if (TryResolveClosestArenaLandmark(ignoreDiscovered: true, out _, out float distance, out Vector3 delta))
            {
                return $"Explore Waystone {ResolveCompassDirectionLabel(delta)} {distance:0}m   Found {WaystoneDiscoveryCount}";
            }

            if (TryResolveClosestArenaLandmark(ignoreDiscovered: false, out _, out _, out _))
            {
                return $"Explore Waystone new grid   Found {WaystoneDiscoveryCount}";
            }

            return $"Explore Waystone --   Found {WaystoneDiscoveryCount}";
        }

        private static string ResolveCompassDirectionLabel(Vector3 delta)
        {
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.0001f)
            {
                return "here";
            }

            float absX = Mathf.Abs(delta.x);
            float absZ = Mathf.Abs(delta.z);
            string horizontal = delta.x >= 0f ? "E" : "W";
            string vertical = delta.z >= 0f ? "N" : "S";
            if (absX > absZ * 1.85f)
            {
                return horizontal;
            }

            if (absZ > absX * 1.85f)
            {
                return vertical;
            }

            return vertical + horizontal;
        }

        private string ResolveEvolutionGoalHudLabel()
        {
            if (_upgradeCatalog == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition evolution = _upgradeCatalog.Definitions[i];
                if (TryResolveEvolutionMissingPassive(evolution, out RunUpgradeDefinition passive))
                {
                    return $"Goal {ResolveUpgradeDisplayName(passive.Id)} -> {ResolveUpgradeDisplayName(evolution.Id)}";
                }
            }

            return string.Empty;
        }

        private string ResolveEvolutionReadyHudLabel()
        {
            if (_upgradeCatalog == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < _upgradeCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition evolution = _upgradeCatalog.Definitions[i];
                if (evolution != null && IsEvolutionUpgrade(evolution) && IsUpgradeEligibleForCurrentBuild(evolution))
                {
                    return $"Ready {ResolveUpgradeDisplayName(evolution.Id)} -> elite/boss reward";
                }
            }

            return string.Empty;
        }

        private string ResolveEvolutionObjectiveHudLabel()
        {
            string goal = ResolveEvolutionGoalHudLabel();
            return string.IsNullOrWhiteSpace(goal) ? ResolveEvolutionReadyHudLabel() : goal;
        }

        private string ResolveSurgeHudLabel()
        {
            string label = string.Empty;
            if (IsStreakSurgeActive)
            {
                label = $"   Surge T{StreakSurgeTier} {StreakSurgeRemainingSeconds:0.0}s";
            }

            if (IsRoamingCacheSurgeActive)
            {
                string travel = $"Way {RoamingCacheSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + travel : label + "   " + travel;
            }

            if (IsArenaShrineSurgeActive)
            {
                string shrine = $"Shrine {ArenaShrineSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + shrine : label + "   " + shrine;
            }

            if (IsWaystoneFocusActive)
            {
                string focus = $"Focus {WaystoneFocusRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + focus : label + "   " + focus;
            }

            if (IsWaystoneChainSurgeActive)
            {
                string chain = $"Chain {WaystoneChainSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + chain : label + "   " + chain;
            }

            if (IsHordeRushClearSurgeActive)
            {
                string breaker = $"Breaker {HordeRushClearSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + breaker : label + "   " + breaker;
            }

            if (IsWeaponLoadoutSurgeActive)
            {
                string arsenal = $"Arsenal {WeaponLoadoutSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + arsenal : label + "   " + arsenal;
            }

            if (IsPassiveLoadoutSurgeActive)
            {
                string harmony = $"Harmony {PassiveLoadoutSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + harmony : label + "   " + harmony;
            }

            if (IsBossRelicSurgeActive)
            {
                string relic = $"Relic {BossRelicSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + relic : label + "   " + relic;
            }

            if (IsGemRushActive)
            {
                string gem = $"Gem {GemRushRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + gem : label + "   " + gem;
            }

            if (IsEvolutionChainSurgeActive)
            {
                string legend = $"Legend {EvolutionChainSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + legend : label + "   " + legend;
            }

            if (IsEndlessSurgeActive)
            {
                string endless = $"Endless T{EndlessSurgeTier} {EndlessSurgeRemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + endless : label + "   " + endless;
            }

            return label;
        }

        private int ResolveTotalRelicCount()
        {
            return _relicDefinitions == null ? 0 : _relicDefinitions.Count;
        }

        private string FormatSelectedRelicList()
        {
            if (_selectedRelics.Count == 0)
            {
                return "none";
            }

            const int maxShown = 3;
            string label = string.Empty;
            int shown = Mathf.Min(maxShown, _selectedRelics.Count);
            for (int i = 0; i < shown; i++)
            {
                if (i > 0)
                {
                    label += ", ";
                }

                SurvivorsRelicDefinition relic = _selectedRelics[i];
                label += relic == null || string.IsNullOrWhiteSpace(relic.DisplayName) ? "Unknown Relic" : relic.DisplayName;
            }

            if (_selectedRelics.Count > shown)
            {
                label += " +" + (_selectedRelics.Count - shown).ToString();
            }

            return label;
        }

        private string ResolveRunPhaseHudLabel()
        {
            return IsEndlessRun ? "Endless" : RunPhase.ToString();
        }

        private string ResolveRunMilestoneHudLabel()
        {
            if (!TryResolveRunMilestone(out string milestoneName, out _, out float remainingSeconds))
            {
                return "Next Objective: survive";
            }

            if (State == SurvivorsRunState.Victory)
            {
                return "Victory Clear - continue or restart";
            }

            if (State == SurvivorsRunState.GameOver)
            {
                return "Run Ended - restart to try again";
            }

            return "Next " + milestoneName + " in " + FormatRunTime(remainingSeconds);
        }

        private bool TryResolveRunMilestone(out string name, out float targetTimeSeconds, out float remainingSeconds)
        {
            name = string.Empty;
            targetTimeSeconds = 0f;
            remainingSeconds = 0f;

            if (!_runStarted)
            {
                return false;
            }

            if (State == SurvivorsRunState.Victory)
            {
                name = "Victory Clear";
                return true;
            }

            if (State == SurvivorsRunState.GameOver)
            {
                name = "Run Ended";
                return true;
            }

            float bestTargetTimeSeconds = float.MaxValue;
            string bestName = string.Empty;
            ConsiderRunMilestone("Horde Rush", _nextHordeRushTimeSeconds, ref bestName, ref bestTargetTimeSeconds);

            if (IsEndlessRun)
            {
                ConsiderRunMilestone(ResolveEndlessMilestoneName(ResolveNextEndlessEliteRole()), _nextEndlessEliteSpawnTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone("Endless Miniboss", _nextEndlessMinibossSpawnTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone("Endless Boss", _nextEndlessBossSpawnTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
            }
            else if (_runFlow != null && _runFlow.Definition != null)
            {
                SurvivorsRunFlowDefinition definition = _runFlow.Definition;
                ConsiderRunMilestone("Elite", _runFlow.NextEliteSpawnTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone("Dread Elite", _runFlow.NextDreadEliteSpawnTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone("Miniboss", definition.MinibossSpawnTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone("Final Boss", definition.BossSpawnTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone("Victory", definition.SurvivalVictoryTimeSeconds, ref bestName, ref bestTargetTimeSeconds);
            }

            if (string.IsNullOrWhiteSpace(bestName) || bestTargetTimeSeconds == float.MaxValue)
            {
                return false;
            }

            name = bestName;
            targetTimeSeconds = bestTargetTimeSeconds;
            remainingSeconds = Mathf.Max(0f, bestTargetTimeSeconds - RunTimeSeconds);
            return true;
        }

        private void ConsiderRunMilestone(string candidateName, float candidateTimeSeconds, ref string bestName, ref float bestTargetTimeSeconds)
        {
            if (string.IsNullOrWhiteSpace(candidateName) || candidateTimeSeconds <= 0f || candidateTimeSeconds <= RunTimeSeconds)
            {
                return;
            }

            if (candidateTimeSeconds < bestTargetTimeSeconds - 0.001f)
            {
                bestName = candidateName;
                bestTargetTimeSeconds = candidateTimeSeconds;
            }
        }

        private static string ResolveEndlessMilestoneName(SurvivorsEnemyRole role)
        {
            return role == SurvivorsEnemyRole.DreadElite
                ? "Endless Dread Elite"
                : "Endless Elite";
        }

        private string ResolveUpgradeAffectedLabel(RunUpgradeDefinition choice)
        {
            if (choice == null || !TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return "Build";
            }

            if (string.IsNullOrWhiteSpace(metadata.AffectedContentId))
            {
                return "Build";
            }

            return "Affects " + ShortWeaponName(metadata.AffectedContentId);
        }

        private string ShortWeaponName(string weaponId)
        {
            if (TryResolveWeaponUpgradeDisplayName(weaponId, out string authoredName, out RunUpgradeId upgradeId) &&
                !string.Equals(authoredName, BasicSurvivorsGame.GetUpgradeDisplayName(upgradeId), StringComparison.Ordinal))
            {
                return authoredName;
            }

            if (weaponId == BasicSurvivorsGame.ArcaneWandWeaponContentId) return "Wand";
            if (weaponId == BasicSurvivorsGame.FrostFanWeaponContentId) return "Frost";
            if (weaponId == BasicSurvivorsGame.OrbitWardWeaponContentId) return "Orbit";
            if (weaponId == BasicSurvivorsGame.ThornHaloWeaponContentId) return "Halo";
            if (weaponId == BasicSurvivorsGame.MoonSlashWeaponContentId) return "Slash";
            if (weaponId == BasicSurvivorsGame.StarNovaWeaponContentId) return "Nova";
            if (weaponId == BasicSurvivorsGame.StarBeamWeaponContentId) return "Beam";
            if (weaponId == BasicSurvivorsGame.GravityGrenadeWeaponContentId) return "Grenade";
            if (weaponId == BasicSurvivorsGame.RuneTrapWeaponContentId) return "Trap";
            if (weaponId == BasicSurvivorsGame.AetherMineWeaponContentId) return "Mine";
            if (weaponId == BasicSurvivorsGame.PlayerTarget.Value) return "Player";
            if (weaponId == BasicSurvivorsGame.PickupTarget.Value) return "Pickups";
            if (weaponId == BasicSurvivorsGame.StatusTarget.Value) return "Status";
            if (weaponId == BasicSurvivorsGame.BarrierTarget.Value) return "Barrier";
            if (weaponId == BasicSurvivorsGame.PayloadWeaponTarget.Value) return "Payloads";
            if (weaponId == BasicSurvivorsGame.ExperienceTarget.Value) return "XP";
            if (weaponId == BasicSurvivorsGame.AreaTarget.Value) return "Area";
            return string.IsNullOrWhiteSpace(weaponId) ? "unknown" : weaponId;
        }

        private static string FormatRunTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        private static string FormatRewardTimeout(float seconds)
        {
            return seconds > 0f ? seconds.ToString("0.#") + "s" : "Off";
        }

        private string ResolveDashHudLabel()
        {
            string cooldown = DashCooldownRemainingSeconds <= 0.01f
                ? "Ready"
                : DashCooldownRemainingSeconds.ToString("0.0") + "s";
            string safety = IsPlayerSafetyActive ? "   Safe " + PlayerSafetyRemainingSeconds.ToString("0.0") + "s" : string.Empty;
            return "Arc Step " + cooldown + safety;
        }

        private void HandleLevelUpInput()
        {
            bool banishModifier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (banishModifier && Input.GetKeyDown(KeyCode.Alpha1))
            {
                BanishDraftChoice(0);
            }
            else if (banishModifier && Input.GetKeyDown(KeyCode.Alpha2))
            {
                BanishDraftChoice(1);
            }
            else if (banishModifier && Input.GetKeyDown(KeyCode.Alpha3))
            {
                BanishDraftChoice(2);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                RerollCurrentDraft();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                SkipCurrentDraft();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectUpgrade(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectUpgrade(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectUpgrade(2);
            }
        }

        private bool HandleResultMetaUpgradeInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                return TryPurchaseResultMetaUpgrade(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                return TryPurchaseResultMetaUpgrade(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                return TryPurchaseResultMetaUpgrade(2);
            }

            return false;
        }

        private Vector2 ReadMovementInput()
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            return new Vector2(x, y);
        }

        private static Material ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return null;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader) { color = color };
            renderer.sharedMaterial = material;
            return material;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static void ReleaseTemplateObject(UnityEngine.Object target)
        {
            UnityObjectUtility.DestroySafely(target);
        }

        private void OnDestroy()
        {
            ClearRun();
            ReleaseMetaProgressionService();
        }

        private struct SurvivorsDamagePopup
        {
            public SurvivorsDamagePopup(string label, Vector3 worldPosition, Color color, bool playerDamage)
            {
                Label = label;
                WorldPosition = worldPosition;
                Color = color;
                PlayerDamage = playerDamage;
                ElapsedSeconds = 0f;
            }

            public string Label { get; }
            public Vector3 WorldPosition { get; }
            public Color Color { get; }
            public bool PlayerDamage { get; }
            public float ElapsedSeconds;
        }

        private struct SurvivorsWorldFeedbackEffect
        {
            public SurvivorsWorldFeedbackEffect(GameObject instance, Renderer renderer, Material material, Color color, Vector3 baseScale)
            {
                Instance = instance;
                Renderer = renderer;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Renderer Renderer { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float ElapsedSeconds;
        }

        private struct SurvivorsEnemyRangedAttackFeedbackEffect
        {
            public SurvivorsEnemyRangedAttackFeedbackEffect(GameObject instance, Material material, Color color, Vector3 baseScale)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float ElapsedSeconds;
        }

        private struct SurvivorsMajorThreatSlamTelegraphEffect
        {
            public SurvivorsMajorThreatSlamTelegraphEffect(GameObject instance, Material material, Color color, Vector3 baseScale, float durationSeconds)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                DurationSeconds = Mathf.Max(0.08f, durationSeconds);
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float DurationSeconds { get; }
            public float ElapsedSeconds;
        }

        private struct SurvivorsIncomingThreatTelegraphEffect
        {
            public SurvivorsIncomingThreatTelegraphEffect(GameObject instance, Material material, Color color, Vector3 baseScale, float durationSeconds)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                DurationSeconds = Mathf.Max(0.12f, durationSeconds);
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float DurationSeconds { get; }
            public float ElapsedSeconds;
        }

        private struct SurvivorsRewardDropFeedbackEffect
        {
            public SurvivorsRewardDropFeedbackEffect(GameObject instance, Material material, Color color, Vector3 basePosition, Vector3 baseScale)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BasePosition = basePosition;
                BaseScale = baseScale;
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BasePosition { get; }
            public Vector3 BaseScale { get; }
            public float ElapsedSeconds;
        }
    }

    public sealed class SurvivorsEnemyActor : MonoBehaviour, IWorldSpawnedObject, IWorldSpawnResettable
    {
        private SurvivorsTemplateController _controller;
        private HealthState _health;
        private float _moveSpeed;
        private float _contactDamage;
        private float _contactInterval;
        private float _contactCooldown;
        private float _rangedAttackRange;
        private float _rangedAttackDamage;
        private float _rangedAttackInterval;
        private float _rangedAttackWindupSeconds;
        private float _preferredRange;
        private float _rangedAttackCooldown;
        private float _rangedAttackWindupTimer;
        private bool _rangedAttackTelegraphing;
        private float _summonSupportCooldown;
        private float _majorThreatSlamCooldown;
        private float _majorThreatSlamTelegraphTimer;
        private bool _majorThreatSlamTelegraphing;
        private float _poisonDamagePerSecond;
        private float _poisonRemainingSeconds;
        private float _bleedDamagePerSecond;
        private float _bleedRemainingSeconds;
        private float _burnDamagePerSecond;
        private float _burnRemainingSeconds;
        private float _moveSpeedMultiplier = 1f;
        private float _slowRemainingSeconds;
        private Renderer _renderer;
        private Material _runtimeMaterial;
        private Color _baseTint;
        private Vector3 _baseScale = Vector3.one;
        private float _hitFlashTimer;
        private float _hitFlashDuration;
        private bool _hitFlashCritical;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsAlive => _health != null && _health.IsAlive;
        public bool IsHitFlashActive => _hitFlashTimer > 0f;
        public SurvivorsEnemyRole Role { get; private set; }
        public string ProfileId { get; private set; }
        public string DisplayName { get; private set; } = string.Empty;
        public float Radius { get; private set; }
        public int ExperienceReward { get; private set; }
        public float CurrentHealth => _health == null ? 0f : (float)_health.CurrentHealth;
        public float MaxHealth => _health == null ? 0f : (float)_health.MaximumHealth;
        public float HealthFraction => MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth;
        public float LeashTimerSeconds { get; private set; }
        public bool CanRecycle { get; private set; } = true;
        public bool CanLeash { get; private set; } = true;
        public bool CanReposition { get; private set; } = true;
        public bool ShowOffscreenMarker { get; private set; }
        public bool ShowOverheadLifeBar { get; private set; }
        public bool ShowBossLifeBar { get; private set; }
        public string MarkerStyle { get; private set; } = string.Empty;
        public bool IsMovementSlowed => _slowRemainingSeconds > 0f && _moveSpeedMultiplier < 1f;
        public float CurrentMoveSpeedMultiplier => IsMovementSlowed ? _moveSpeedMultiplier : 1f;
        public bool IsBurning => _burnRemainingSeconds > 0f && _burnDamagePerSecond > 0f;
        public float CurrentBurnDamagePerSecond => IsBurning ? _burnDamagePerSecond : 0f;

        public void Initialize(SurvivorsTemplateController controller, SurvivorsEnemyProfile profile)
        {
            _controller = controller;
            Role = profile.Role;
            ProfileId = profile.Id;
            DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Role.ToString() : profile.DisplayName;
            _moveSpeed = Mathf.Max(0f, profile.MoveSpeed);
            Radius = Mathf.Max(0.05f, profile.Radius);
            _contactDamage = Mathf.Max(0f, profile.ContactDamage);
            _contactInterval = Mathf.Max(0.05f, profile.ContactIntervalSeconds);
            _contactCooldown = 0f;
            _rangedAttackRange = Mathf.Max(0f, profile.RangedAttackRange);
            _rangedAttackDamage = Mathf.Max(0f, profile.RangedAttackDamage);
            _rangedAttackInterval = Mathf.Max(0.05f, profile.RangedAttackIntervalSeconds);
            _rangedAttackWindupSeconds = controller == null ? 0f : Mathf.Max(0f, controller.CurrentTuning.EnemyRangedAttackWindupSeconds);
            _preferredRange = Mathf.Max(0f, profile.PreferredRange);
            _rangedAttackCooldown = Mathf.Min(0.75f, _rangedAttackInterval);
            _rangedAttackWindupTimer = 0f;
            _rangedAttackTelegraphing = false;
            _summonSupportCooldown = ResolveInitialSummonSupportCooldown(profile.Role, controller == null ? null : controller.CurrentTuning);
            _majorThreatSlamCooldown = ResolveInitialMajorThreatSlamCooldown(profile.Role, controller == null ? null : controller.CurrentTuning);
            _majorThreatSlamTelegraphTimer = 0f;
            _majorThreatSlamTelegraphing = false;
            CanRecycle = profile.CanRecycle;
            CanLeash = profile.CanLeash;
            CanReposition = profile.CanReposition;
            ShowOffscreenMarker = profile.ShowOffscreenMarker;
            ShowOverheadLifeBar = profile.ShowOverheadLifeBar;
            ShowBossLifeBar = profile.ShowBossLifeBar;
            MarkerStyle = string.IsNullOrWhiteSpace(profile.MarkerStyle) ? profile.Role.ToString() : profile.MarkerStyle;
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            _burnDamagePerSecond = 0f;
            _burnRemainingSeconds = 0f;
            _moveSpeedMultiplier = 1f;
            _slowRemainingSeconds = 0f;
            ExperienceReward = Mathf.Max(1, profile.ExperienceReward);
            string id = InstanceId.Value > 0 ? "combatant.survivors.enemy." + InstanceId.Value : "combatant.survivors.enemy.pending";
            float maxHealth = Mathf.Max(1f, profile.MaxHealth);
            _health = new HealthState(new CombatantId(id), maxHealth, maxHealth);
            transform.localScale = Vector3.one * (Radius * 2f);
            _baseScale = transform.localScale;
            _hitFlashTimer = 0f;
            _hitFlashDuration = 0f;
            _hitFlashCritical = false;
            LeashTimerSeconds = 0f;
            ApplyTint(profile.Tint);
        }

        public void AddLeashTime(float deltaTime)
        {
            LeashTimerSeconds += Mathf.Max(0f, deltaTime);
        }

        public void ResetLeashTimer()
        {
            LeashTimerSeconds = 0f;
        }

        public void Simulate(float deltaTime)
        {
            if (!IsAlive || _controller == null || !_controller.IsPlaying)
            {
                return;
            }

            TickHitFlash(deltaTime);
            TickMovementSlow(deltaTime);
            TickDamageOverTime(deltaTime);
            if (!IsAlive)
            {
                return;
            }

            Vector3 direction = _controller.PlayerPosition - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            Vector3 moveDirection = Vector3.zero;
            Vector3 facingDirection = Vector3.forward;
            if (distance > 0.001f)
            {
                Vector3 normalized = direction / distance;
                moveDirection = ResolveMoveDirection(normalized, distance);
                facingDirection = normalized;
            }

            Vector3 separation = _controller.ResolveEnemyCrowdSeparation(this);
            if (separation.sqrMagnitude > 0.001f)
            {
                moveDirection += separation;
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Vector3 resolvedMove = moveDirection.normalized;
                float catchUpMultiplier = _controller.ResolveEnemyCatchUpMoveSpeedMultiplier(this, distance);
                transform.position += resolvedMove * (_moveSpeed * CurrentMoveSpeedMultiplier * catchUpMultiplier * deltaTime);
                transform.forward = resolvedMove;
            }
            else if (distance > 0.001f)
            {
                transform.forward = facingDirection;
            }

            TickMajorThreatSlam(deltaTime, distance);
            TickRangedAttack(deltaTime, distance);
            TickSummonSupport(deltaTime);
            _contactCooldown -= deltaTime;
            if (_contactCooldown <= 0f && distance <= Radius + _controller.CurrentTuning.PlayerRadius)
            {
                _controller.ApplyDamageToPlayer(_contactDamage, "combatant.survivors.enemy." + InstanceId.Value);
                _contactCooldown = _contactInterval;
            }
        }

        public DamageResult ApplyDamage(float amount, string source)
        {
            return ApplyDamageInternal(amount, source, applyAugments: true);
        }

        public void ApplyDamageOverTime(float totalDamage, float durationSeconds, string statusId, string source)
        {
            float duration = Mathf.Max(0.1f, durationSeconds);
            float perSecond = Mathf.Max(0f, totalDamage) / duration;
            if (perSecond <= 0f)
            {
                return;
            }

            if (string.Equals(statusId, "status.survivors.burn", StringComparison.Ordinal))
            {
                _burnDamagePerSecond += perSecond;
                _burnRemainingSeconds = Mathf.Max(_burnRemainingSeconds, duration);
                ApplyHitFlashPresentation(_hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(_hitFlashTimer / _hitFlashDuration));
            }
            else if (string.Equals(statusId, "status.survivors.bleed", StringComparison.Ordinal))
            {
                _bleedDamagePerSecond += perSecond;
                _bleedRemainingSeconds = Mathf.Max(_bleedRemainingSeconds, duration);
            }
            else
            {
                _poisonDamagePerSecond += perSecond;
                _poisonRemainingSeconds = Mathf.Max(_poisonRemainingSeconds, duration);
            }
        }

        public bool ApplyMovementSlow(float multiplier, float durationSeconds)
        {
            if (!IsAlive)
            {
                return false;
            }

            float resolvedMultiplier = Mathf.Clamp(multiplier, 0.15f, 1f);
            float duration = Mathf.Max(0f, durationSeconds);
            if (resolvedMultiplier >= 1f || duration <= 0f)
            {
                return false;
            }

            _moveSpeedMultiplier = Mathf.Min(_moveSpeedMultiplier, resolvedMultiplier);
            _slowRemainingSeconds = Mathf.Max(_slowRemainingSeconds, duration);
            ApplyHitFlashPresentation(_hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(_hitFlashTimer / _hitFlashDuration));
            return true;
        }

        public void ExecuteFromAugment(string source)
        {
            if (!IsAlive)
            {
                return;
            }

            ApplyDamageInternal(Mathf.Max(1f, CurrentHealth), (string.IsNullOrWhiteSpace(source) ? "survivors" : source) + ".augment.execute", applyAugments: false);
        }

        private DamageResult ApplyDamageInternal(float amount, string source, bool applyAugments)
        {
            if (_controller == null || _health == null || !IsAlive)
            {
                return null;
            }

            DamageResolutionResult result = _controller.ResolveEnemyDamage(_health, amount, source, applyAugments);
            if (result != null)
            {
                _controller.RecordEnemyDamageFeedback(this, result.Damage);
            }

            if (applyAugments && result != null && result.Damage != null && IsAlive)
            {
                _controller.ApplyDamageAugmentsToEnemy(this, result.Damage, source);
            }

            if (_health != null && !_health.IsAlive)
            {
                _controller.HandleEnemyKilled(this, source, applyAugments);
            }

            return result.Damage;
        }

        public void TriggerHitFlash(bool critical, float durationSeconds)
        {
            _hitFlashCritical = critical;
            _hitFlashDuration = Mathf.Max(0.01f, durationSeconds);
            _hitFlashTimer = _hitFlashDuration;
            ApplyHitFlashPresentation(1f);
        }

        private void TickHitFlash(float deltaTime)
        {
            if (_hitFlashTimer <= 0f)
            {
                return;
            }

            _hitFlashTimer = Mathf.Max(0f, _hitFlashTimer - Mathf.Max(0f, deltaTime));
            float intensity = _hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(_hitFlashTimer / _hitFlashDuration);
            ApplyHitFlashPresentation(intensity);
        }

        private void ApplyHitFlashPresentation(float intensity)
        {
            if (_runtimeMaterial != null)
            {
                Color baseTint = ResolvePresentationBaseTint();
                Color flash = _hitFlashCritical
                    ? new Color(1f, 0.86f, 0.25f)
                    : Color.white;
                _runtimeMaterial.color = Color.Lerp(baseTint, flash, Mathf.Clamp01(intensity));
            }

            transform.localScale = _baseScale * (1f + 0.18f * Mathf.Clamp01(intensity));
        }

        private Color ResolvePresentationBaseTint()
        {
            Color tint = IsMovementSlowed
                ? Color.Lerp(_baseTint, new Color(0.52f, 0.88f, 1f), 0.48f)
                : _baseTint;
            return IsBurning
                ? Color.Lerp(tint, new Color(1f, 0.42f, 0.12f), 0.42f)
                : tint;
        }

        private Vector3 ResolveMoveDirection(Vector3 normalizedToPlayer, float distance)
        {
            if (_preferredRange <= 0f)
            {
                return normalizedToPlayer;
            }

            if (distance > _preferredRange * 1.12f)
            {
                return normalizedToPlayer;
            }

            if (distance < _preferredRange * 0.68f)
            {
                return -normalizedToPlayer;
            }

            return Vector3.zero;
        }

        private void TickMajorThreatSlam(float deltaTime, float distanceToPlayer)
        {
            if (_controller == null || !SurvivorsTemplateController.IsMajorThreatSlamRole(Role))
            {
                return;
            }

            SurvivorsTemplateTuning tuning = _controller.CurrentTuning;
            float interval = Mathf.Max(0f, tuning.MajorThreatSlamIntervalSeconds);
            float radius = Mathf.Max(0.5f, tuning.MajorThreatSlamRadius + Radius * 0.35f);
            if (interval <= 0f || radius <= 0f)
            {
                return;
            }

            if (_majorThreatSlamTelegraphing)
            {
                _majorThreatSlamTelegraphTimer = Mathf.Max(0f, _majorThreatSlamTelegraphTimer - Mathf.Max(0f, deltaTime));
                if (_majorThreatSlamTelegraphTimer <= 0f)
                {
                    _majorThreatSlamTelegraphing = false;
                    _majorThreatSlamCooldown = interval;
                    _controller.ResolveMajorThreatSlam(this);
                }

                return;
            }

            _majorThreatSlamCooldown = Mathf.Max(0f, _majorThreatSlamCooldown - Mathf.Max(0f, deltaTime));
            if (_majorThreatSlamCooldown > 0f || distanceToPlayer > radius * 1.35f)
            {
                return;
            }

            _majorThreatSlamTelegraphing = true;
            _majorThreatSlamTelegraphTimer = Mathf.Max(0.05f, tuning.MajorThreatSlamTelegraphSeconds);
            _controller.RecordMajorThreatSlamTelegraph(this);
        }

        private static float ResolveInitialMajorThreatSlamCooldown(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (tuning == null || !SurvivorsTemplateController.IsMajorThreatSlamRole(role))
            {
                return 0f;
            }

            return Mathf.Max(0f, tuning.MajorThreatSlamIntervalSeconds);
        }

        private static float ResolveInitialSummonSupportCooldown(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (tuning == null || role != SurvivorsEnemyRole.Summoner)
            {
                return 0f;
            }

            return Mathf.Max(0f, tuning.SummonerSupportInitialDelaySeconds);
        }

        private void TickRangedAttack(float deltaTime, float distance)
        {
            if (_controller == null || _rangedAttackRange <= 0f || _rangedAttackDamage <= 0f)
            {
                _rangedAttackTelegraphing = false;
                _rangedAttackWindupTimer = 0f;
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            if (_rangedAttackTelegraphing)
            {
                _rangedAttackWindupTimer = Mathf.Max(0f, _rangedAttackWindupTimer - dt);
                if (_rangedAttackWindupTimer > 0f)
                {
                    return;
                }

                _rangedAttackTelegraphing = false;
                _rangedAttackCooldown = _rangedAttackInterval;
                if (ResolveDistanceToPlayer() <= _rangedAttackRange)
                {
                    _controller.ApplyDamageToPlayer(_rangedAttackDamage, "combatant.survivors.enemy.ranged." + InstanceId.Value);
                }
                else
                {
                    _controller.RecordEnemyRangedAttackDodgeFeedback(this);
                }

                return;
            }

            _rangedAttackCooldown = Mathf.Max(0f, _rangedAttackCooldown - dt);
            if (_rangedAttackCooldown > 0f || distance > _rangedAttackRange)
            {
                return;
            }

            _controller.RecordEnemyRangedAttackFeedback(transform.position, _controller.PlayerPosition, Role);
            if (_rangedAttackWindupSeconds <= 0f)
            {
                _controller.ApplyDamageToPlayer(_rangedAttackDamage, "combatant.survivors.enemy.ranged." + InstanceId.Value);
                _rangedAttackCooldown = _rangedAttackInterval;
                return;
            }

            _rangedAttackTelegraphing = true;
            _rangedAttackWindupTimer = _rangedAttackWindupSeconds;
        }

        private void TickSummonSupport(float deltaTime)
        {
            if (_controller == null || Role != SurvivorsEnemyRole.Summoner)
            {
                return;
            }

            SurvivorsTemplateTuning tuning = _controller.CurrentTuning;
            float interval = Mathf.Max(0f, tuning.SummonerSupportIntervalSeconds);
            if (interval <= 0f || tuning.SummonerSupportCount <= 0)
            {
                return;
            }

            _summonSupportCooldown = Mathf.Max(0f, _summonSupportCooldown - Mathf.Max(0f, deltaTime));
            if (_summonSupportCooldown > 0f)
            {
                return;
            }

            int spawned = _controller.SpawnSummonerSupport(this);
            _summonSupportCooldown = spawned > 0 ? interval : Mathf.Min(interval, 0.75f);
        }

        private float ResolveDistanceToPlayer()
        {
            if (_controller == null)
            {
                return float.MaxValue;
            }

            Vector3 delta = _controller.PlayerPosition - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private void TickMovementSlow(float deltaTime)
        {
            if (_slowRemainingSeconds <= 0f)
            {
                return;
            }

            _slowRemainingSeconds = Mathf.Max(0f, _slowRemainingSeconds - Mathf.Max(0f, deltaTime));
            if (_slowRemainingSeconds > 0f)
            {
                return;
            }

            _moveSpeedMultiplier = 1f;
            ApplyHitFlashPresentation(_hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(_hitFlashTimer / _hitFlashDuration));
        }

        private void TickDamageOverTime(float deltaTime)
        {
            if (_poisonRemainingSeconds > 0f && _poisonDamagePerSecond > 0f)
            {
                float tick = _poisonDamagePerSecond * deltaTime;
                _poisonRemainingSeconds = Mathf.Max(0f, _poisonRemainingSeconds - deltaTime);
                ApplyDamageInternal(tick, "survivors.status.poison", applyAugments: false);
                if (_poisonRemainingSeconds <= 0f)
                {
                    _poisonDamagePerSecond = 0f;
                }
            }

            if (!IsAlive)
            {
                return;
            }

            if (_bleedRemainingSeconds > 0f && _bleedDamagePerSecond > 0f)
            {
                float tick = _bleedDamagePerSecond * deltaTime;
                _bleedRemainingSeconds = Mathf.Max(0f, _bleedRemainingSeconds - deltaTime);
                ApplyDamageInternal(tick, "survivors.status.bleed", applyAugments: false);
                if (_bleedRemainingSeconds <= 0f)
                {
                    _bleedDamagePerSecond = 0f;
                }
            }

            if (!IsAlive)
            {
                return;
            }

            if (_burnRemainingSeconds > 0f && _burnDamagePerSecond > 0f)
            {
                float tick = _burnDamagePerSecond * deltaTime;
                _burnRemainingSeconds = Mathf.Max(0f, _burnRemainingSeconds - deltaTime);
                ApplyDamageInternal(tick, "survivors.status.burn", applyAugments: false);
                if (_burnRemainingSeconds <= 0f)
                {
                    _burnDamagePerSecond = 0f;
                    ApplyHitFlashPresentation(_hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(_hitFlashTimer / _hitFlashDuration));
                }
            }
        }

        public void OverrideHealthForTest(float health)
        {
            string id = InstanceId.Value > 0 ? "combatant.survivors.enemy." + InstanceId.Value : "combatant.survivors.enemy.test";
            float resolved = Mathf.Max(1f, health);
            _health = new HealthState(new CombatantId(id), resolved, resolved);
        }

        public void OnWorldSpawned(WorldSpawnContext context)
        {
            InstanceId = context.InstanceId;
        }

        public void OnWorldDespawned(DespawnReason reason)
        {
            _controller = null;
            _health = null;
            ProfileId = null;
            DisplayName = string.Empty;
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            _burnDamagePerSecond = 0f;
            _burnRemainingSeconds = 0f;
            _moveSpeedMultiplier = 1f;
            _slowRemainingSeconds = 0f;
            _rangedAttackWindupTimer = 0f;
            _rangedAttackTelegraphing = false;
            _summonSupportCooldown = 0f;
            _hitFlashTimer = 0f;
            _hitFlashDuration = 0f;
            _hitFlashCritical = false;
            LeashTimerSeconds = 0f;
            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.color = _baseTint;
            }

            transform.localScale = _baseScale;
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            _health = null;
            _contactCooldown = 0f;
            _rangedAttackRange = 0f;
            _rangedAttackDamage = 0f;
            _rangedAttackInterval = 0f;
            _rangedAttackWindupSeconds = 0f;
            _preferredRange = 0f;
            _rangedAttackCooldown = 0f;
            _rangedAttackWindupTimer = 0f;
            _rangedAttackTelegraphing = false;
            _summonSupportCooldown = 0f;
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            _burnDamagePerSecond = 0f;
            _burnRemainingSeconds = 0f;
            _moveSpeedMultiplier = 1f;
            _slowRemainingSeconds = 0f;
            _hitFlashTimer = 0f;
            _hitFlashDuration = 0f;
            _hitFlashCritical = false;
            LeashTimerSeconds = 0f;
            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.color = _baseTint;
            }

            transform.localScale = _baseScale;
            Role = SurvivorsEnemyRole.Swarm;
            ProfileId = null;
            DisplayName = string.Empty;
            InstanceId = default;
        }

        private void ApplyTint(Color tint)
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _renderer = renderer;
            _baseTint = tint;
            if (_runtimeMaterial == null || _runtimeMaterial.shader != shader)
            {
                _runtimeMaterial = new Material(shader);
            }

            _runtimeMaterial.color = tint;
            _renderer.sharedMaterial = _runtimeMaterial;
        }
    }

    public sealed class SurvivorsProjectileActor : MonoBehaviour, IWorldSpawnedObject, IWorldSpawnResettable
    {
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>();
        private readonly List<SurvivorsEnemyActor> _forkTargets = new List<SurvivorsEnemyActor>();
        private SurvivorsTemplateController _controller;
        private SurvivorsWeaponArchetypeDefinition _definition;
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _radius;
        private float _lifetime;
        private int _remainingChains;
        private int _remainingPierces;
        private int _remainingForks;
        private int _remainingReturns;
        private bool _returningToPlayer;
        private Renderer _renderer;
        private TrailRenderer _trail;
        private Material _runtimeMaterial;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsActive { get; private set; }

        public void Initialize(
            SurvivorsTemplateController controller,
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 direction,
            float speed,
            float damage,
            float radius,
            float lifetime,
            int remainingChains,
            int remainingPierces,
            int remainingForks,
            int remainingReturns,
            HashSet<int> ignoredEnemyIds = null)
        {
            _controller = controller;
            _definition = definition;
            _direction = direction.sqrMagnitude <= 0.001f ? Vector3.forward : direction.normalized;
            _speed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(0f, damage);
            _radius = Mathf.Max(0.05f, radius);
            _lifetime = Mathf.Max(0.05f, lifetime);
            _remainingChains = Mathf.Max(0, remainingChains);
            _remainingPierces = Mathf.Max(0, remainingPierces);
            _remainingForks = Mathf.Max(0, remainingForks);
            _remainingReturns = Mathf.Max(0, remainingReturns);
            _returningToPlayer = false;
            ApplyPresentation(definition == null
                ? controller == null ? new Color(0.78f, 0.42f, 1f) : controller.ResolveProjectileFallbackColor()
                : definition.Tint);
            _hitEnemyIds.Clear();
            if (ignoredEnemyIds != null)
            {
                foreach (int ignoredId in ignoredEnemyIds)
                {
                    _hitEnemyIds.Add(ignoredId);
                }
            }

            IsActive = true;
            transform.localScale = Vector3.one * (_radius * 2f);
        }

        public void Simulate(float deltaTime)
        {
            if (!IsActive || _controller == null)
            {
                return;
            }

            _lifetime -= deltaTime;
            if (_lifetime <= 0f)
            {
                if (!TryStartReturnToPlayer())
                {
                    Release(DespawnReason.OutOfBounds);
                }

                return;
            }

            if (_returningToPlayer)
            {
                Vector3 toPlayer = _controller.PlayerPosition + Vector3.up * 0.4f - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude <= 0.18f)
                {
                    Release(DespawnReason.Completed);
                    return;
                }

                if (toPlayer.sqrMagnitude > 0.001f)
                {
                    _direction = toPlayer.normalized;
                }
            }

            Vector3 previousPosition = transform.position;
            Vector3 currentPosition = previousPosition + _direction * (_speed * deltaTime);
            transform.position = currentPosition;
            if (TryFindSegmentHit(previousPosition, currentPosition, out SurvivorsEnemyActor hitEnemy, out float hitRange, out Vector3 hitPosition))
            {
                transform.position = hitPosition;
                int enemyId = hitEnemy.GetInstanceID();
                _hitEnemyIds.Add(enemyId);
                DamageResult damage = hitEnemy.ApplyDamage(_damage, _definition == null ? "combatant.survivors.player" : _definition.Id);
                _controller.ApplyWeaponStatusEffectsToEnemy(hitEnemy, _definition, damage);
                SpawnForkProjectiles(hitEnemy);
                if (_remainingPierces > 0)
                {
                    _remainingPierces--;
                    _controller.RecordProjectilePierceHit();
                    transform.position += _direction * Mathf.Max(0.08f, hitRange);
                    return;
                }

                if (_remainingChains > 0 && TryRetargetFrom(hitEnemy))
                {
                    _remainingChains--;
                    _controller.RecordProjectileChainHit();
                    return;
                }

                if (!TryStartReturnToPlayer())
                {
                    Release(DespawnReason.Completed);
                }

                return;
            }
        }

        private bool TryFindSegmentHit(Vector3 start, Vector3 end, out SurvivorsEnemyActor hitEnemy, out float hitRange, out Vector3 hitPosition)
        {
            hitEnemy = null;
            hitRange = 0f;
            hitPosition = end;
            if (_controller == null)
            {
                return false;
            }

            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            Vector3 segment = end - start;
            segment.y = 0f;
            float segmentLengthSquared = segment.sqrMagnitude;
            float bestSegmentT = float.MaxValue;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                int enemyId = enemy.GetInstanceID();
                if (_hitEnemyIds.Contains(enemyId))
                {
                    continue;
                }

                float candidateHitRange = _radius + enemy.Radius;
                Vector3 enemyPosition = enemy.transform.position;
                Vector3 closestPoint = end;
                float segmentT = 1f;
                if (segmentLengthSquared > 0.0001f)
                {
                    Vector3 enemyOffset = enemyPosition - start;
                    enemyOffset.y = 0f;
                    segmentT = Mathf.Clamp01(Vector3.Dot(enemyOffset, segment) / segmentLengthSquared);
                    closestPoint = start + segment * segmentT;
                }

                Vector3 missOffset = enemyPosition - closestPoint;
                missOffset.y = 0f;
                if (missOffset.sqrMagnitude > candidateHitRange * candidateHitRange || segmentT >= bestSegmentT)
                {
                    continue;
                }

                bestSegmentT = segmentT;
                hitEnemy = enemy;
                hitRange = candidateHitRange;
                hitPosition = closestPoint;
            }

            return hitEnemy != null;
        }

        private void SpawnForkProjectiles(SurvivorsEnemyActor lastHitEnemy)
        {
            if (_controller == null || _definition == null || _remainingForks <= 0)
            {
                return;
            }

            _forkTargets.Clear();
            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            Vector3 origin = transform.position;
            const float forkSearchRange = 5f;
            float forkSearchRangeSquared = forkSearchRange * forkSearchRange;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || _hitEnemyIds.Contains(enemy.GetInstanceID()))
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - origin;
                offset.y = 0f;
                if (offset.sqrMagnitude <= forkSearchRangeSquared)
                {
                    _forkTargets.Add(enemy);
                }
            }

            _forkTargets.Sort((left, right) =>
            {
                float leftDistance = (left.transform.position - origin).sqrMagnitude;
                float rightDistance = (right.transform.position - origin).sqrMagnitude;
                return leftDistance.CompareTo(rightDistance);
            });

            int forkCount = Mathf.Min(_remainingForks, Mathf.Max(1, _forkTargets.Count));
            _remainingForks = Mathf.Max(0, _remainingForks - 1);
            for (int i = 0; i < forkCount; i++)
            {
                Vector3 forkDirection;
                if (i < _forkTargets.Count && _forkTargets[i] != null)
                {
                    forkDirection = _forkTargets[i].transform.position - origin;
                    forkDirection.y = 0f;
                }
                else
                {
                    float angle = (-18f + i * 36f) * Mathf.Deg2Rad;
                    forkDirection = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f) * _direction;
                }

                if (forkDirection.sqrMagnitude <= 0.001f)
                {
                    forkDirection = _direction;
                }

                if (_controller.LaunchProjectileFrom(
                    _definition,
                    origin,
                    forkDirection.normalized,
                    _remainingChains,
                    _remainingPierces,
                    _remainingForks,
                    _remainingReturns,
                    _hitEnemyIds))
                {
                    _controller.RecordProjectileForkSpawn();
                }
            }

            _forkTargets.Clear();
        }

        private bool TryRetargetFrom(SurvivorsEnemyActor lastHitEnemy)
        {
            if (_controller == null)
            {
                return false;
            }

            SurvivorsEnemyActor best = null;
            float bestDistance = 5f * 5f;
            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            Vector3 origin = lastHitEnemy == null ? transform.position : lastHitEnemy.transform.position;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || _hitEnemyIds.Contains(enemy.GetInstanceID()))
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - origin;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            if (best == null)
            {
                return false;
            }

            Vector3 direction = best.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            _direction = direction.normalized;
            _lifetime = Mathf.Max(_lifetime, 0.7f);
            return true;
        }

        private bool TryStartReturnToPlayer()
        {
            if (_returningToPlayer || _controller == null || _remainingReturns <= 0)
            {
                return false;
            }

            Vector3 direction = _controller.PlayerPosition + Vector3.up * 0.4f - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            _remainingReturns--;
            _returningToPlayer = true;
            _direction = direction.normalized;
            _speed *= 1.2f;
            _lifetime = Mathf.Max(_lifetime, 1.25f);
            _controller.RecordProjectileReturnStart();
            return true;
        }

        private void Release(DespawnReason reason)
        {
            IsActive = false;
            if (_controller != null)
            {
                _controller.ReleaseProjectile(this, reason);
            }
        }

        public void OnWorldSpawned(WorldSpawnContext context)
        {
            InstanceId = context.InstanceId;
        }

        public void OnWorldDespawned(DespawnReason reason)
        {
            _controller = null;
            _definition = null;
            _hitEnemyIds.Clear();
            _forkTargets.Clear();
            IsActive = false;
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            _definition = null;
            _hitEnemyIds.Clear();
            _forkTargets.Clear();
            _remainingChains = 0;
            _remainingPierces = 0;
            _remainingForks = 0;
            _remainingReturns = 0;
            _returningToPlayer = false;
            IsActive = false;
            InstanceId = default;
        }

        private void ApplyPresentation(Color tint)
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }

            if (_renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (_runtimeMaterial == null || _runtimeMaterial.shader != shader)
                {
                    _runtimeMaterial = new Material(shader);
                }

                _runtimeMaterial.color = tint;
                _renderer.sharedMaterial = _runtimeMaterial;
            }

            if (_trail == null)
            {
                _trail = GetComponent<TrailRenderer>();
            }

            if (_trail != null)
            {
                Color trailStart = tint;
                trailStart.a = 0.95f;
                _trail.startColor = trailStart;
                _trail.endColor = _controller == null
                    ? new Color(0.18f, 0.86f, 1f, 0f)
                    : _controller.ResolveProjectileTrailEndColor();
            }
        }
    }

    public sealed class SurvivorsPickupActor : MonoBehaviour, IWorldSpawnedObject, IWorldSpawnResettable
    {
        private SurvivorsTemplateController _controller;
        private float _attractRange;
        private float _attractionSpeed;
        private float _collectRadius;
        private bool _globalRecall;
        private bool _rewardCacheAttraction;
        private bool _attractionFeedbackSent;
        private float _recallSpeedMultiplier;
        private float _currentSpeed;
        private Vector3 _baseScale;
        private float _pulseSeconds;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsActive { get; private set; }
        public SurvivorsPickupKind Kind { get; private set; }
        public int Amount { get; private set; }
        public bool IsGlobalRecallActive => IsActive && _globalRecall;
        public bool IsRewardCacheAttractionActive => IsActive && _rewardCacheAttraction;
        public bool HasShownAttractionFeedback => _attractionFeedbackSent;

        public void Initialize(SurvivorsTemplateController controller, SurvivorsPickupKind kind, int amount, float attractRange, float attractionSpeed, float collectRadius)
        {
            _controller = controller;
            Kind = kind;
            Amount = Mathf.Max(1, amount);
            UpdateAttractionSettings(attractRange, attractionSpeed, collectRadius);
            _globalRecall = false;
            _rewardCacheAttraction = false;
            _attractionFeedbackSent = false;
            _recallSpeedMultiplier = 1f;
            _currentSpeed = 0f;
            _pulseSeconds = 0f;
            IsActive = true;
            _baseScale = ResolvePickupBaseScale(kind);
            transform.localScale = _baseScale;
        }

        public void UpdateAttractionSettings(float attractRange, float attractionSpeed, float collectRadius)
        {
            _attractRange = Mathf.Max(0.1f, attractRange);
            _attractionSpeed = Mathf.Max(0.1f, attractionSpeed);
            _collectRadius = Mathf.Max(0.1f, collectRadius);
        }

        public void StartGlobalRecall(float speedMultiplier)
        {
            if (Kind != SurvivorsPickupKind.Experience)
            {
                return;
            }

            _globalRecall = true;
            _recallSpeedMultiplier = Mathf.Max(1f, speedMultiplier);
            _currentSpeed = Mathf.Max(_currentSpeed, _attractionSpeed * 1.5f);
            BeginAttractionFeedback();
        }

        public bool StartRewardCacheAttraction(float speedMultiplier)
        {
            if (!IsActive)
            {
                return false;
            }

            _rewardCacheAttraction = true;
            _recallSpeedMultiplier = Mathf.Max(_recallSpeedMultiplier, speedMultiplier);
            _currentSpeed = Mathf.Max(_currentSpeed, _attractionSpeed * 1.25f);
            BeginAttractionFeedback();
            return true;
        }

        private static Vector3 ResolvePickupBaseScale(SurvivorsPickupKind kind)
        {
            switch (kind)
            {
                case SurvivorsPickupKind.Magnet:
                    return Vector3.one * 0.58f;
                case SurvivorsPickupKind.Health:
                    return Vector3.one * 0.44f;
                case SurvivorsPickupKind.BloodShard:
                    return Vector3.one * 0.4f;
                default:
                    return Vector3.one * 0.34f;
            }
        }

        public void Simulate(float deltaTime)
        {
            if (!IsActive || _controller == null)
            {
                return;
            }

            Vector3 playerPosition = _controller.PlayerPosition;
            Vector3 offset = playerPosition - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            bool forcedAttraction = _globalRecall || _rewardCacheAttraction;
            bool shouldAttract = forcedAttraction || distance <= _attractRange;
            if (shouldAttract && distance > 0.001f)
            {
                BeginAttractionFeedback();
                float targetSpeed = _attractionSpeed * (forcedAttraction ? _recallSpeedMultiplier * Mathf.Clamp(1f + distance * 0.18f, 1f, 10f) : 1f);
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, targetSpeed * 8f * deltaTime);
                float travel = Mathf.Min(distance, _currentSpeed * deltaTime);
                transform.position += offset.normalized * travel;
                distance = Vector3.Distance(transform.position, playerPosition);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _attractionSpeed * 5f * deltaTime);
            }

            if (forcedAttraction)
            {
                transform.Rotate(0f, 420f * deltaTime, 0f, Space.Self);
            }

            TickPickupPresentation(deltaTime, shouldAttract);

            if (distance <= _collectRadius)
            {
                IsActive = false;
                _controller.CollectPickup(this);
            }
        }

        private void BeginAttractionFeedback()
        {
            if (_attractionFeedbackSent || _controller == null)
            {
                return;
            }

            _attractionFeedbackSent = true;
            _controller.RecordPickupAttractionFeedback(Kind, transform.position);
        }

        private void TickPickupPresentation(float deltaTime, bool attracting)
        {
            if (_baseScale == Vector3.zero)
            {
                _baseScale = transform.localScale == Vector3.zero ? Vector3.one * 0.34f : transform.localScale;
            }

            _pulseSeconds += Mathf.Max(0f, deltaTime);
            float scale = 1f;
            if (_globalRecall || _rewardCacheAttraction)
            {
                scale = 1.28f + Mathf.Sin(_pulseSeconds * 18f) * 0.16f;
            }
            else if (attracting)
            {
                scale = 1.12f + Mathf.Sin(_pulseSeconds * 12f) * 0.08f;
            }

            transform.localScale = _baseScale * Mathf.Max(0.5f, scale);
        }

        public void OnWorldSpawned(WorldSpawnContext context)
        {
            InstanceId = context.InstanceId;
        }

        public void OnWorldDespawned(DespawnReason reason)
        {
            _controller = null;
            IsActive = false;
            _rewardCacheAttraction = false;
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            IsActive = false;
            InstanceId = default;
            _globalRecall = false;
            _rewardCacheAttraction = false;
            _attractionFeedbackSent = false;
            _currentSpeed = 0f;
            _pulseSeconds = 0f;
        }
    }

    public sealed class SurvivorsSpawnPoseResolver : ISpawnPoseResolver
    {
        private readonly SurvivorsTemplateController _controller;
        private readonly Dictionary<long, Vector3> _explicitPoses = new Dictionary<long, Vector3>();

        public SurvivorsSpawnPoseResolver(SurvivorsTemplateController controller)
        {
            _controller = controller;
        }

        public void RegisterExplicitPose(long sequence, Vector3 position)
        {
            _explicitPoses[sequence] = position;
        }

        public SpawnPoseResult TryResolvePose(WorldSpawnRequest request)
        {
            if (_explicitPoses.TryGetValue(request.Sequence, out Vector3 explicitPosition))
            {
                _explicitPoses.Remove(request.Sequence);
                return SpawnPoseResult.Success(new SpawnPose(explicitPosition, Quaternion.identity));
            }

            if (!request.ChannelId.Equals(BasicSurvivorsGame.RadialSpawnChannelId))
            {
                return SpawnPoseResult.Failure("Unknown Survivors spawn channel: " + request.ChannelId);
            }

            if (_controller == null)
            {
                float angle = (request.Sequence * 137.50777f) * Mathf.Deg2Rad;
                Vector3 fallbackPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 12f;
                return SpawnPoseResult.Success(new SpawnPose(fallbackPosition, Quaternion.identity));
            }

            Vector3 resolvedPosition = _controller.ResolveRuntimeEnemySpawnPositionForResolver(request.Sequence);
            return SpawnPoseResult.Success(new SpawnPose(resolvedPosition, Quaternion.identity));
        }
    }
}
