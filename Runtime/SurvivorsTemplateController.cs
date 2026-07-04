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
        private const string DirectionalLightName = "Survivors Directional Light";
        private const float InfiniteArenaTileSize = 12f;
        private const int InfiniteArenaGridRadius = 2;
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
        private const int SplitterChildCount = 2;
        private const float DamagePopupLifetimeSeconds = 0.9f;
        private const float DamagePopupRiseHeight = 1.25f;
        private const int DamagePopupLimit = 72;
        private const float LowHealthWarningThreshold = 0.3f;
        private const float RewardFeedbackDurationSeconds = 2.35f;
        private const float StreakRewardFeedbackDurationSeconds = 1.8f;
        private const float EvolutionReadyFeedbackDurationSeconds = 2.4f;
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
        private const float EnemyRangedAttackFeedbackLifetimeSeconds = 0.34f;
        private const int EnemyRangedAttackFeedbackLimit = 28;
        private const float EndlessSpawnIntervalMultiplier = 0.82f;
        private const int EndlessEnemyAliveBonus = 24;
        private const int NormalDraftRarityLockSeedSalt = 7919;
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

        private static readonly RunUpgradeDefinition[] EmptyChoices = Array.Empty<RunUpgradeDefinition>();
        private static readonly SurvivorsRelicDefinition[] EmptyRelicChoices = Array.Empty<SurvivorsRelicDefinition>();
        private static readonly string[] EmptyWeaponIds = Array.Empty<string>();

        [SerializeField]
        private bool autoStart = true;

        [SerializeField]
        private bool buildVisualsOnAwake = true;

        [SerializeField]
        private SurvivorsPacingProfile pacingProfile = SurvivorsPacingProfile.HumanPlaytest;

        [SerializeField]
        private SurvivorsTemplateTuning tuning;

        private readonly List<SurvivorsEnemyActor> _enemies = new List<SurvivorsEnemyActor>(64);
        private readonly List<SurvivorsPickupActor> _pickups = new List<SurvivorsPickupActor>(128);
        private readonly List<SurvivorsProjectileActor> _projectiles = new List<SurvivorsProjectileActor>(64);
        private readonly List<Transform> _arenaTiles = new List<Transform>(25);
        private readonly List<SurvivorsDamagePopup> _damagePopups = new List<SurvivorsDamagePopup>(DamagePopupLimit);
        private readonly List<SurvivorsWorldFeedbackEffect> _worldFeedbackEffects = new List<SurvivorsWorldFeedbackEffect>(EnemyDeathEffectLimit);
        private readonly List<SurvivorsRewardDropFeedbackEffect> _rewardDropFeedbackEffects = new List<SurvivorsRewardDropFeedbackEffect>(MajorRewardDropEffectLimit);
        private readonly List<SurvivorsMajorThreatSlamTelegraphEffect> _majorThreatSlamTelegraphEffects = new List<SurvivorsMajorThreatSlamTelegraphEffect>(MajorThreatSlamTelegraphEffectLimit);
        private readonly List<SurvivorsEnemyRangedAttackFeedbackEffect> _enemyRangedAttackFeedbackEffects = new List<SurvivorsEnemyRangedAttackFeedbackEffect>(EnemyRangedAttackFeedbackLimit);
        private readonly List<string> _lastRunSummaryLines = new List<string>(8);
        private readonly List<SurvivorsPersistentUpgradeDefinition> _resultMetaUpgradeOptions = new List<SurvivorsPersistentUpgradeDefinition>(ResultMetaUpgradeOptionCount);
        private readonly List<SurvivorsClassDefinition> _resultClassOptions = new List<SurvivorsClassDefinition>(ResultClassOptionCount);
        private readonly HashSet<SurvivorsEnemyActor> _activeHordeRushEnemies = new HashSet<SurvivorsEnemyActor>();
        private readonly HashSet<SurvivorsEnemyActor> _activeRoamingCacheAmbushEnemies = new HashSet<SurvivorsEnemyActor>();
        private readonly HashSet<SurvivorsEnemyActor> _enragedMajorThreats = new HashSet<SurvivorsEnemyActor>();
        private readonly Dictionary<string, SurvivorsRunUpgradeMetadata> _upgradeMetadataById = new Dictionary<string, SurvivorsRunUpgradeMetadata>(StringComparer.Ordinal);
        private readonly HashSet<string> _ownedPassiveUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _ownedEvolutionUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _announcedEvolutionReadyUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _selectedRelicIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<SurvivorsRelicDefinition> _selectedRelics = new List<SurvivorsRelicDefinition>(8);
        private Transform _worldRoot;
        private Transform _prefabRoot;
        private Transform _arenaTileRoot;
        private Transform _arenaFollowRoot;
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
        private long _spawnSequence;
        private int _killStreakCount;
        private int _currentDraftRerollIndex;
        private float _streakSurgeTimer;
        private bool _runStarted;
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
        private string _evolutionReadyFeedbackLabel = string.Empty;
        private float _evolutionReadyFeedbackTimer;
        private string _experienceComboFeedbackLabel = string.Empty;
        private float _experienceComboTimer;
        private float _experienceComboFeedbackTimer;
        private int _experienceComboPickupCount;
        private int _experienceComboAmount;
        private int _bonusBloodShardsEarnedThisRun;
        private int _bonusLegacyExperienceEarnedThisRun;

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
        public int ProjectilePierceHitCount { get; private set; }
        public int ProjectileChainHitCount { get; private set; }
        public int ProjectileForkSpawnCount { get; private set; }
        public int ProjectileReturnStartCount { get; private set; }
        public int PayloadThrowCount { get; private set; }
        public int PayloadPlacedCount { get; private set; }
        public int PayloadDetonationCount { get; private set; }
        public int PayloadExplosionHitCount { get; private set; }
        public int PayloadHazardTickCount { get; private set; }
        public int SplitterChildSpawnCount { get; private set; }
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
        public int SelectedRelicCount { get; private set; }
        public int SelectedRewardUpgradeCount { get; private set; }
        public int RewardAutoSelectCount { get; private set; }
        public int EndlessThreatSpawnCount { get; private set; }
        public int HordeRushSpawnCount { get; private set; }
        public int HordeRushEnemySpawnCount { get; private set; }
        public int HordeRushWarningCount { get; private set; }
        public int HordeRushClearRewardCount { get; private set; }
        public int HordeRushClearExperienceGemDropCount { get; private set; }
        public int HordeRushClearSpecialDropCount { get; private set; }
        public string LastHordeRushFeedbackLabel { get; private set; } = string.Empty;
        public string LastHordeRushClearFeedbackLabel { get; private set; } = string.Empty;
        public int DraftRerollCount { get; private set; }
        public int DraftBanishCount { get; private set; }
        public int DraftSkipCount { get; private set; }
        public int ClassUnlockRewardCount { get; private set; }
        public int DamagePopupSpawnCount { get; private set; }
        public int PlayerDamageFeedbackCount { get; private set; }
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
        public int MajorRewardDropFeedbackCount { get; private set; }
        public int MajorRewardCacheDropCount { get; private set; }
        public int MajorRewardCacheExperienceGemDropCount { get; private set; }
        public int MajorRewardCacheSpecialDropCount { get; private set; }
        public int MajorRewardCacheAttractedPickupCount { get; private set; }
        public int WeaponEvolutionFeedbackCount { get; private set; }
        public int WeaponEvolutionSurgeCount { get; private set; }
        public int WeaponEvolutionSurgeHitCount { get; private set; }
        public string LastWeaponEvolutionSurgeFeedbackLabel { get; private set; } = string.Empty;
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
        public string LastMajorThreatSlamFeedbackLabel { get; private set; } = string.Empty;
        public int ExperiencePickupFeedbackCount { get; private set; }
        public int ExperienceComboFeedbackCount { get; private set; }
        public string LastExperienceComboFeedbackLabel { get; private set; } = string.Empty;
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
        public string LastRoamingCacheFeedbackLabel { get; private set; } = string.Empty;
        public string LastRoamingCacheAmbushClearFeedbackLabel { get; private set; } = string.Empty;
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
        public int ActiveEnemyCount => _enemies.Count;
        public int ActiveRunnerCount => CountEnemiesByRole(SurvivorsEnemyRole.Runner);
        public int ActiveBruiserCount => CountEnemiesByRole(SurvivorsEnemyRole.Bruiser);
        public int ActiveSpitterCount => CountEnemiesByRole(SurvivorsEnemyRole.Spitter);
        public int ActiveSplitterCount => CountEnemiesByRole(SurvivorsEnemyRole.Splitter);
        public int ActiveEliteCount => CountEliteEnemies();
        public int ActiveDreadEliteCount => CountEnemiesByRole(SurvivorsEnemyRole.DreadElite);
        public int ActiveMinibossCount => CountEnemiesByRole(SurvivorsEnemyRole.Miniboss);
        public int ActiveBossCount => CountEnemiesByRole(SurvivorsEnemyRole.Boss);
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
        public int ActiveProjectileCount => _projectiles.Count;
        public int ActiveDamagePopupCount => _damagePopups.Count;
        public int ActiveEnemyDeathEffectCount => _worldFeedbackEffects.Count;
        public int ActiveEnemyRangedAttackFeedbackCount => _enemyRangedAttackFeedbackEffects.Count;
        public int ActiveMajorRewardDropFeedbackCount => _rewardDropFeedbackEffects.Count;
        public int ActiveMajorThreatSlamTelegraphEffectCount => _majorThreatSlamTelegraphEffects.Count;
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
        public Vector3 ArenaPresentationCenterForTest => _arenaFollowRoot == null ? Vector3.zero : _arenaFollowRoot.position;
        public Vector3 FirstInfiniteArenaTilePositionForTest => _arenaTiles.Count == 0 || _arenaTiles[0] == null ? Vector3.zero : _arenaTiles[0].position;
        public IReadOnlyList<string> ActiveWeaponIds => _weaponLoadout == null ? EmptyWeaponIds : _weaponLoadout.WeaponIds;
        public int ActiveOrbitBladeCount => _weaponLoadout == null ? 0 : _weaponLoadout.ActiveOrbitBladeCount;
        public float PlayerMoveSpeed => CurrentTuning.PlayerMoveSpeed + MoveSpeedBonus + StreakSurgeMoveSpeedBonus;
        public float DashCooldownRemainingSeconds => Mathf.Max(0f, _dashCooldownTimer);
        public float PlayerSafetyRemainingSeconds => Mathf.Max(0f, _playerInvulnerabilityTimer);
        public bool IsPlayerSafetyActive => _playerInvulnerabilityTimer > 0f;
        public float ProjectileDamage => Mathf.Max(0f, (float)_projectileDefinition.BaseDamage + DamageBonus + StreakSurgeDamageBonus);
        public float WeaponCooldownSeconds => Mathf.Max(0.12f, CurrentTuning.WeaponCooldownSeconds * Mathf.Max(0.2f, 1f + WeaponCooldownMultiplierBonus + StreakSurgeCooldownMultiplierBonus));
        public float CurrentPickupAttractRange => Mathf.Max(0f, CurrentTuning.PickupAttractRange + PickupRangeBonus + StreakSurgePickupRangeBonus);
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
        public SurvivorsTemplateTuning CurrentTuning => tuning ?? (tuning = BasicSurvivorsGame.CreateTuning(pacingProfile));
        public SurvivorsRunFlowDefinition CurrentRunFlowDefinition => _runFlow == null ? null : _runFlow.Definition;
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
        public string ActiveRewardFeedbackLabel => _rewardFeedbackTimer > 0f ? _rewardFeedbackLabel : string.Empty;
        public float RewardFeedbackRemainingSeconds => Mathf.Max(0f, _rewardFeedbackTimer);
        public string ActiveStreakRewardFeedbackLabel => _streakRewardFeedbackTimer > 0f ? _streakRewardFeedbackLabel : string.Empty;
        public float StreakRewardFeedbackRemainingSeconds => Mathf.Max(0f, _streakRewardFeedbackTimer);
        public string ActiveEvolutionReadyFeedbackLabel => _evolutionReadyFeedbackTimer > 0f ? _evolutionReadyFeedbackLabel : string.Empty;
        public float EvolutionReadyFeedbackRemainingSeconds => Mathf.Max(0f, _evolutionReadyFeedbackTimer);
        public string ActiveExperienceComboFeedbackLabel => _experienceComboFeedbackTimer > 0f ? _experienceComboFeedbackLabel : string.Empty;
        public float ExperienceComboFeedbackRemainingSeconds => Mathf.Max(0f, _experienceComboFeedbackTimer);
        public int CurrentExperienceComboPickupCount => _experienceComboTimer > 0f ? _experienceComboPickupCount : 0;
        public int CurrentExperienceComboAmount => _experienceComboTimer > 0f ? _experienceComboAmount : 0;
        public int RequiredExperienceForNextLevel => Mathf.Max(1, CurrentTuning.ExperienceRequiredBase + ((Level - 1) * CurrentTuning.ExperienceRequiredPerLevel));
        public int TotalDraftRerollCharges => Mathf.Max(0, CurrentTuning.DraftRerollCharges + PersistentDraftRerollBonus);
        public int DraftRerollsRemaining => Mathf.Max(0, TotalDraftRerollCharges - DraftRerollCount);
        public int DraftBanishesRemaining => Mathf.Max(0, CurrentTuning.DraftBanishCharges - DraftBanishCount);
        public int DraftSkipBloodShards => Mathf.Max(0, CurrentTuning.DraftSkipBloodShards);

        private void Awake()
        {
            if (tuning == null)
            {
                tuning = BasicSurvivorsGame.CreateTuning(pacingProfile);
            }
            else
            {
                pacingProfile = tuning.PacingProfile;
            }
        }

        private void Start()
        {
            if (autoStart && !_runStarted)
            {
                StartRun();
            }
        }

        private void Update()
        {
            if (!_runStarted)
            {
                return;
            }

            if (State == SurvivorsRunState.LevelUp)
            {
                TickDamagePopups(Time.deltaTime);
                TickWorldFeedbackEffects(Time.deltaTime);
                TickEnemyRangedAttackFeedbackEffects(Time.deltaTime);
                TickMajorThreatSlamTelegraphEffects(Time.deltaTime);
                TickMajorRewardDropFeedbackEffects(Time.deltaTime);
                TickRewardFeedback(Time.deltaTime);
                TickStreakRewardFeedback(Time.deltaTime);
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
                TickMajorRewardDropFeedbackEffects(Time.deltaTime);
                TickRewardFeedback(Time.deltaTime);
                TickStreakRewardFeedback(Time.deltaTime);
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
                return;
            }

            EnsureHudStyles();
            GUI.Box(new Rect(12, 12, 356, 400), string.Empty);
            GUI.Label(new Rect(24, 22, 300, 22), "Deucarian Survivors Run", _hudTitleStyle);
            DrawHudBar(new Rect(24, 50, 318, 18), "Health", MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth, new Color(0.9f, 0.22f, 0.24f));
            DrawHudBar(new Rect(24, 74, 318, 18), "Barrier", BarrierCapacity <= 0f ? 0f : BarrierValue / BarrierCapacity, new Color(0.42f, 0.8f, 1f));
            DrawHudBar(new Rect(24, 98, 318, 18), "XP", Experience / (float)RequiredExperienceForNextLevel, new Color(0.2f, 0.78f, 1f));
            DrawHudBar(new Rect(24, 122, 318, 18), "Run", Mathf.Clamp01(RunTimeSeconds / Mathf.Max(1f, CurrentTuning.SurvivalVictoryTimeSeconds)), new Color(0.72f, 0.44f, 1f));
            GUI.Label(new Rect(24, 148, 318, 22), $"LV {Level}   Time {FormatRunTime(RunTimeSeconds)}   Phase {ResolveRunPhaseHudLabel()} +{RunEscalationLevel}", _hudLabelStyle);
            GUI.Label(new Rect(24, 170, 318, 22), $"Enemies {ActiveEnemyCount}/{CurrentEnemyMaximumAlive}   Kills {KilledCount}", _hudLabelStyle);
            GUI.Label(new Rect(24, 192, 318, 22), $"Split {ActiveSplitterCount}   Elite {ActiveEliteCount}   Miniboss {ActiveMinibossCount}   Boss {ActiveBossCount}", _hudLabelStyle);
            GUI.Label(new Rect(24, 214, 318, 22), $"Shards {MetaBloodShards}   Poison {PoisonDamageRatio:0.##}   Bleed {BleedDamageRatio:0.##}   Execute {ExecuteThresholdNormalized:P0}", _hudSmallStyle);
            GUI.Label(new Rect(24, 236, 318, 22), "Weapons: " + ResolveWeaponHudLabel(), _hudSmallStyle);
            GUI.Label(new Rect(24, 258, 318, 22), $"Profile {BasicSurvivorsGame.GetPacingProfileDisplayName(CurrentPacingProfile)}   TimeScale {Time.timeScale:0.##}", _hudSmallStyle);
            GUI.Label(new Rect(24, 280, 318, 22), $"Spawn {CurrentEnemySpawnIntervalSeconds:0.00}s   Enemy Speed x{CurrentEnemySpeedMultiplier:0.##}", _hudSmallStyle);
            string surgeHud = IsStreakSurgeActive ? $"   Surge T{StreakSurgeTier} {StreakSurgeRemainingSeconds:0.0}s" : string.Empty;
            GUI.Label(new Rect(24, 302, 318, 22), $"Streak {CurrentKillStreak}   Best {BestKillStreak}   Bonus Drops {StreakBonusDropCount}{surgeHud}", _hudSmallStyle);
            GUI.Label(new Rect(24, 324, 318, 22), $"Reward Timeout {FormatRewardTimeout(CurrentTuning.RewardSelectionTimeoutSeconds)}   Reroll {DraftRerollsRemaining}   Banish {DraftBanishesRemaining}", _hudSmallStyle);
            GUI.Label(new Rect(24, 346, 318, 22), ResolveBuildSlotHudLabel(), _hudSmallStyle);
            GUI.Label(new Rect(24, 368, 318, 22), ResolveDashHudLabel(), _hudSmallStyle);
            DrawLowHealthWarning();
            DrawMajorThreatWarning();
            DrawHordeRushWarning();
            DrawMajorThreatHealthBar();
            DrawRewardSelectionFeedback();
            DrawStreakRewardFeedback();
            DrawEvolutionReadyFeedback();
            DrawExperienceComboFeedback();
            DrawDamagePopups();

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
        }

        public void StartRun()
        {
            Time.timeScale = 1f;
            ClearRun();
            SurvivorsTemplateTuning resolved = CurrentTuning;
            _combatCatalog = BasicSurvivorsGame.CreateCombatCatalog();
            _combatRandom = new DeterministicRandom(resolved.RunSeed + 4049);
            _weaponDefinition = BasicSurvivorsGame.CreateWeaponDefinition();
            _projectileDefinition = BasicSurvivorsGame.CreateProjectileDefinition(resolved);
            _upgradeState = new RunUpgradeState();
            _relicDefinitions = BasicSurvivorsGame.CreateRelicDefinitions();
            _upgradeClassGates = BasicSurvivorsGame.CreateClassUpgradeGates();
            _classLibrary = BasicSurvivorsGame.CreateClassLibraryDefinition();
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
            ProjectilePierceHitCount = 0;
            ProjectileChainHitCount = 0;
            ProjectileForkSpawnCount = 0;
            ProjectileReturnStartCount = 0;
            PayloadThrowCount = 0;
            PayloadPlacedCount = 0;
            PayloadDetonationCount = 0;
            PayloadExplosionHitCount = 0;
            PayloadHazardTickCount = 0;
            SplitterChildSpawnCount = 0;
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
            LastWeaponEvolutionSurgeFeedbackLabel = string.Empty;
            MetaUpgradePurchaseCount = 0;
            LastMetaUpgradePurchaseFeedbackLabel = string.Empty;
            ResultClassSelectionCount = 0;
            LastResultClassSelectionFeedbackLabel = string.Empty;
            EvolutionReadyFeedbackCount = 0;
            LastEvolutionReadyFeedbackLabel = string.Empty;
            BossRelicDraftOpenCount = 0;
            EliteUpgradeDraftOpenCount = 0;
            BossUpgradeDraftOpenCount = 0;
            SelectedRelicCount = 0;
            SelectedRewardUpgradeCount = 0;
            RewardAutoSelectCount = 0;
            EndlessThreatSpawnCount = 0;
            HordeRushSpawnCount = 0;
            HordeRushEnemySpawnCount = 0;
            HordeRushWarningCount = 0;
            HordeRushClearRewardCount = 0;
            HordeRushClearExperienceGemDropCount = 0;
            HordeRushClearSpecialDropCount = 0;
            LastHordeRushFeedbackLabel = string.Empty;
            LastHordeRushClearFeedbackLabel = string.Empty;
            DraftRerollCount = 0;
            DraftBanishCount = 0;
            DraftSkipCount = 0;
            ClassUnlockRewardCount = 0;
            DamagePopupSpawnCount = 0;
            PlayerDamageFeedbackCount = 0;
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
            LastMajorThreatSlamFeedbackLabel = string.Empty;
            ExperiencePickupFeedbackCount = 0;
            ExperienceComboFeedbackCount = 0;
            LastExperienceComboFeedbackLabel = string.Empty;
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
            LastRoamingCacheFeedbackLabel = string.Empty;
            LastRoamingCacheAmbushClearFeedbackLabel = string.Empty;
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
            _experienceComboPickupCount = 0;
            _experienceComboAmount = 0;
            _bonusBloodShardsEarnedThisRun = 0;
            _bonusLegacyExperienceEarnedThisRun = 0;
            _enemySpawnTimer = 0f;
            _playerInvulnerabilityTimer = 0f;
            _dashCooldownTimer = 0f;
            _killStreakTimer = 0f;
            _roamingCacheTravelDistance = 0f;
            _killStreakCount = 0;
            _streakSurgeTimer = 0f;
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
            _runFlow = new SurvivorsRunFlowRuntime(BasicSurvivorsGame.CreateRunFlowDefinition(resolved));
            _weaponArchetypeDefinitions = BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(resolved);
            _weaponLoadout = new SurvivorsWeaponLoadoutRuntime(this, ResolveStartingWeaponDefinitions(_weaponArchetypeDefinitions));
            State = SurvivorsRunState.Playing;
            _runStarted = true;
        }

        public void RestartRun()
        {
            StartRun();
        }

        public bool ContinueAfterVictory()
        {
            if (State != SurvivorsRunState.Victory || !_runStarted)
            {
                return false;
            }

            _victoryClearedThisRun = true;
            ClearRewardDrafts();
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

            float dt = Mathf.Max(0f, deltaTime);
            TickDamagePopups(dt);
            TickWorldFeedbackEffects(dt);
            TickEnemyRangedAttackFeedbackEffects(dt);
            TickMajorThreatSlamTelegraphEffects(dt);
            TickMajorRewardDropFeedbackEffects(dt);
            TickRewardFeedback(dt);
            TickStreakRewardFeedback(dt);
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
            TickBarrier(dt);
            MovePlayer(movementInput, dt);
            UpdateArenaPresentation();
            TickEnemySpawning(dt);
            TickWeapon(dt);
            TickEnemies(dt);
            TickProjectiles(dt);
            TickPickups(dt);
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
            SurvivorsMetaProgressionDefinition definition = BasicSurvivorsGame.CreateMetaProgressionDefinition();
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
            return $"{index + 1}. {upgrade.DisplayName} rank {currentRank}->{Mathf.Min(upgrade.MaxRank, currentRank + 1)}/{upgrade.MaxRank} ({nextCost} shards) - {FormatPersistentUpgradeEffectLabel(upgrade)}";
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

            return definition.StartingWeaponIds.Count.ToString() + " weapons";
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
            SurvivorsMetaProgressionDefinition definition = BasicSurvivorsGame.CreateMetaProgressionDefinition();
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
                : BasicSurvivorsGame.GetUpgradeDisplayName(new RunUpgradeId(upgradeId));
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
                $"Stats: damage +{DamageBonus:0.#} surge +{StreakSurgeDamageBonus:0.#}, crit {CriticalChanceNormalized:P0} x{CriticalDamageMultiplier:0.0}, luck +{DraftLuckBonus:P0}, cooldown {WeaponCooldownSeconds:0.00}s, move {PlayerMoveSpeed:0.0}, pickup {CurrentPickupAttractRange:0.#}, XP +{ExperienceGainMultiplierBonus:P0}",
                $"Projectiles: fan +{ProjectileFanBonus}, pierce +{ProjectilePierceBonus}, chain +{ProjectileChainBonus}, fork +{ProjectileForkBonus}, return +{ProjectileReturnBonus}",
                $"Area: global +{AreaRadiusBonus:0.#}, orbit +{OrbitRadiusBonus:0.#}, burst +{BurstCountBonus}, echoes +{BurstEchoBonus}, payload +{PayloadCountBonus}, death nova {DeathNovaDamage:0.#}/{DeathNovaRadius:0.#}",
                $"Status: poison {PoisonDamageRatio:P0}, bleed {BleedDamageRatio:P0}, execute {ExecuteThresholdNormalized:P0}, lifesteal {LifestealRatio:P0}"
            };

            AppendSelectedUpgradeRankLines(lines);
            return lines;
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

            DamageRequest request = new DamageRequest(
                _playerHealth.Id,
                new[] { new DamageComponent(BasicSurvivorsGame.ArcaneDamageType, incoming) },
                sourceId: new CombatantId(string.IsNullOrWhiteSpace(source) ? "combatant.survivors.enemy" : source),
                preResolvedCritical: false);
            DamageResolutionResult result = CombatDamageResolver.Resolve(_combatCatalog, _playerHealth, null, request);
            RecordPlayerDamageFeedback(result == null ? null : result.Damage, PlayerPosition);
            if (!_playerHealth.IsAlive)
            {
                GrantRunRewards(victory: false);
                State = SurvivorsRunState.GameOver;
                ClearRewardDrafts();
                PlayFeedback(_bossPulse, PlayerPosition, 34, _dangerClip);
            }
            else
            {
                PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip);
            }
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
            CompleteUpgradeDraftSelection(
                selectionKind,
                consumeLevelUp: selectionKind == SurvivorsRewardSelectionKind.LevelUp,
                selectedRewardUpgrade: selectionKind != SurvivorsRewardSelectionKind.LevelUp);

            return true;
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
            DraftRerollCount++;
            RecordRewardCardPresentation(_rewardSelectionKind, _currentDraft);
            BeginRewardSelectionTimeout();
            PlayFeedback(_levelUpPulse, PlayerPosition, 18, _levelUpClip);
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

            PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip);
            return true;
        }

        public void TriggerMagnetRecall()
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
                PlayFeedback(_pickupPulse, PlayerPosition, Mathf.Clamp(12 + recalled * 3, 18, 72), _pickupClip);
            }
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
            int xp = Mathf.Max(1, enemy.ExperienceReward);
            float radius = enemy.Radius;
            _enemies.Remove(enemy);
            bool clearedHordeRushEnemy = _activeHordeRushEnemies.Remove(enemy) && _activeHordeRushEnemies.Count == 0;
            bool clearedRoamingCacheAmbushEnemy = _activeRoamingCacheAmbushEnemies.Remove(enemy) && _activeRoamingCacheAmbushEnemies.Count == 0;
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
            }

            PlayFeedback(_killPulse, position, role == SurvivorsEnemyRole.Swarm ? 18 : 34, _killClip);
            if (role == SurvivorsEnemyRole.Splitter)
            {
                SpawnSplitterChildren(position);
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
                int gained = GainExperience(Mathf.Max(1, pickup.Amount));
                RecordExperienceCombo(gained);
                ExperiencePickupFeedbackCount++;
            }

            PlayFeedback(_pickupPulse, pickup.transform.position, ResolvePickupFeedbackBurstCount(pickup.Kind), _pickupClip);
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

        internal float ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, definition.Damage + DamageBonus + StreakSurgeDamageBonus);
        }

        internal float ResolveWeaponCooldownSeconds(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null)
            {
                return WeaponCooldownSeconds;
            }

            return Mathf.Max(0.08f, definition.CooldownSeconds * Mathf.Max(0.2f, 1f + WeaponCooldownMultiplierBonus + StreakSurgeCooldownMultiplierBonus));
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

        private SurvivorsEnemyActor ResolveCurrentMajorThreatForHud()
        {
            SurvivorsEnemyActor selected = null;
            int selectedPriority = -1;
            float selectedHealthFraction = 2f;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive || !IsMajorRewardRole(enemy.Role))
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
                float angle = ((i + 0.29f + MajorThreatEnrageCount * 0.17f) / targetCount) * Mathf.PI * 2f;
                float laneRadius = radius + ((i & 1) == 0 ? 0f : 1.15f);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * laneRadius;
                SurvivorsEnemyRole supportRole = ResolveMajorThreatEnrageSupportRole(enemy.Role, i);
                if (SpawnEnemy(center + offset, explicitPosition: true, supportRole) != null)
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
            ApplyColor(_playerRenderer, new Color(0.78f, 0.91f, 1f));

            _enemyPrefab = CreatePrimitivePrefab("Survivors Swarm Enemy Prefab", PrimitiveType.Capsule, new Color(0.88f, 0.22f, 0.32f), typeof(SurvivorsEnemyActor));
            _experiencePickupPrefab = CreatePrimitivePrefab("Survivors XP Gem Prefab", PrimitiveType.Sphere, new Color(0.22f, 0.83f, 1f), typeof(SurvivorsPickupActor));
            _magnetPickupPrefab = CreatePrimitivePrefab("Survivors Magnet Prefab", PrimitiveType.Sphere, new Color(1f, 0.86f, 0.18f), typeof(SurvivorsPickupActor));
            _healthPickupPrefab = CreatePrimitivePrefab("Survivors Vital Shard Prefab", PrimitiveType.Sphere, new Color(1f, 0.24f, 0.42f), typeof(SurvivorsPickupActor));
            _bloodShardPickupPrefab = CreatePrimitivePrefab("Survivors Blood Shard Prefab", PrimitiveType.Sphere, new Color(0.96f, 0.08f, 0.18f), typeof(SurvivorsPickupActor));
            _projectilePrefab = CreatePrimitivePrefab("Survivors Arcane Bolt Prefab", PrimitiveType.Sphere, new Color(0.78f, 0.42f, 1f), typeof(SurvivorsProjectileActor));

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
                trail.startColor = new Color(0.86f, 0.48f, 1f, 0.95f);
                trail.endColor = new Color(0.18f, 0.86f, 1f, 0f);
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

            GameObject tileRoot = new GameObject("Survivors Infinite Arena Tiles");
            tileRoot.transform.SetParent(_worldRoot, false);
            _arenaTileRoot = tileRoot.transform;
            BuildInfiniteArenaTiles();

            GameObject followRoot = new GameObject("Survivors Moving Arena Readability");
            followRoot.transform.SetParent(_worldRoot, false);
            _arenaFollowRoot = followRoot.transform;

            CreateArenaPrimitive("Survivors Local Arena Ring", PrimitiveType.Cylinder, Vector3.zero, new Vector3(17.5f, 0.035f, 17.5f), new Color(0.07f, 0.09f, 0.12f), _arenaFollowRoot);
            CreateArenaPrimitive("Inner XP Magnet Ring", PrimitiveType.Cylinder, Vector3.zero, new Vector3(6.4f, 0.04f, 6.4f), new Color(0.09f, 0.16f, 0.2f), _arenaFollowRoot);
            CreateArenaPrimitive("Player Safe Readability Ring", PrimitiveType.Cylinder, Vector3.zero, new Vector3(2.1f, 0.05f, 2.1f), new Color(0.12f, 0.19f, 0.28f), _arenaFollowRoot);

            float warningOffset = Mathf.Max(7.5f, CurrentTuning.EnemySpawnRadius * 0.68f);
            CreateArenaPrimitive("North Spawn Warning", PrimitiveType.Cube, new Vector3(0f, 0.09f, warningOffset), new Vector3(4.4f, 0.08f, 0.28f), new Color(0.62f, 0.12f, 0.18f), _arenaFollowRoot);
            CreateArenaPrimitive("East Spawn Warning", PrimitiveType.Cube, new Vector3(warningOffset, 0.09f, 0f), new Vector3(0.28f, 0.08f, 4.4f), new Color(0.62f, 0.12f, 0.18f), _arenaFollowRoot);
            CreateArenaPrimitive("South Spawn Warning", PrimitiveType.Cube, new Vector3(0f, 0.09f, -warningOffset), new Vector3(4.4f, 0.08f, 0.28f), new Color(0.62f, 0.12f, 0.18f), _arenaFollowRoot);
            CreateArenaPrimitive("West Spawn Warning", PrimitiveType.Cube, new Vector3(-warningOffset, 0.09f, 0f), new Vector3(0.28f, 0.08f, 4.4f), new Color(0.62f, 0.12f, 0.18f), _arenaFollowRoot);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 5.4f, 0.12f, Mathf.Sin(angle) * 5.4f);
                Vector3 scale = i % 2 == 0 ? new Vector3(0.9f, 0.18f, 0.32f) : new Vector3(0.32f, 0.18f, 0.9f);
                CreateArenaPrimitive("Rune Lane Marker " + (i + 1).ToString(), PrimitiveType.Cube, position, scale, new Color(0.18f, 0.2f, 0.32f), _arenaFollowRoot);
            }

            UpdateArenaPresentation();
        }

        private void BuildInfiniteArenaTiles()
        {
            for (int x = -InfiniteArenaGridRadius; x <= InfiniteArenaGridRadius; x++)
            {
                for (int z = -InfiniteArenaGridRadius; z <= InfiniteArenaGridRadius; z++)
                {
                    bool alternate = ((x + z) & 1) == 0;
                    Color tileColor = alternate ? new Color(0.045f, 0.055f, 0.058f) : new Color(0.055f, 0.065f, 0.05f);
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
                    RecordStreakRewardFeedback($"{_killStreakCount} Streak: Blood Shards +{amount}", new Color(1f, 0.34f, 0.42f));
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
            _spawnPulse = CreateFeedbackPulse(SpawnPulseName, new Color(1f, 0.3f, 0.28f), 0.2f, 2.2f, 0.5f);
            _firePulse = CreateFeedbackPulse(FirePulseName, new Color(0.82f, 0.4f, 1f), 0.16f, 2.8f, 0.35f);
            _killPulse = CreateFeedbackPulse(KillPulseName, new Color(0.4f, 0.95f, 1f), 0.22f, 3.2f, 0.45f);
            _pickupPulse = CreateFeedbackPulse(PickupPulseName, new Color(0.2f, 0.82f, 1f), 0.14f, 2.4f, 0.35f);
            _levelUpPulse = CreateFeedbackPulse(LevelUpPulseName, new Color(1f, 0.85f, 0.24f), 0.28f, 2.0f, 0.7f);
            _bossPulse = CreateFeedbackPulse(BossPulseName, new Color(1f, 0.3f, 0.82f), 0.32f, 3.6f, 0.65f);

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

        private void PlayFeedback(ParticleSystem particles, Vector3 position, int count, AudioClip clip)
        {
            if (particles != null)
            {
                particles.transform.position = position + Vector3.up * 0.28f;
                particles.Emit(Mathf.Max(1, count));
            }

            if (_feedbackAudio != null && clip != null)
            {
                _feedbackAudio.PlayOneShot(clip);
            }
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

        private static Color ResolveMajorRewardDropColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.2f, 0.45f, 0.95f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(0.78f, 0.35f, 1f, 0.95f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.38f, 0.9f, 1f, 0.92f);
                default:
                    return new Color(1f, 0.76f, 0.2f, 0.92f);
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

            while (_rewardDropFeedbackEffects.Count > 0)
            {
                ReleaseMajorRewardDropFeedbackEffect(_rewardDropFeedbackEffects.Count - 1);
            }

            _arenaTiles.Clear();
            _ownedPassiveUpgradeIds.Clear();
            _ownedEvolutionUpgradeIds.Clear();
            _arenaTileRoot = null;
            _arenaFollowRoot = null;
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
                    BasicSurvivorsGame.CreateMetaProgressionDefinition());
                _metaProfileLoaded = false;
            }

            if (!_metaProfileLoaded)
            {
                _metaProgression.Load();
                _metaProfileLoaded = true;
            }
        }

        private void EnsureClassLibraryLoaded()
        {
            _classLibrary ??= BasicSurvivorsGame.CreateClassLibraryDefinition();
            _relicDefinitions ??= BasicSurvivorsGame.CreateRelicDefinitions();
            _upgradeClassGates ??= BasicSurvivorsGame.CreateClassUpgradeGates();
            if (_metaProgression != null)
            {
                _metaProgression.EnsureDefaultClassUnlocks(_classLibrary);
            }
        }

        private RunUpgradeCatalog CreateRunUpgradeCatalogForSelectedClass()
        {
            RunUpgradeCatalog fullCatalog = BasicSurvivorsGame.CreateRunUpgradeCatalog();
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
            IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata = BasicSurvivorsGame.CreateRunUpgradeMetadata();
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

        private bool IsEvolutionUpgrade(RunUpgradeDefinition definition)
        {
            return definition != null &&
                TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata) &&
                metadata.IsEvolution;
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
                        resolvedLocks = CreateNormalDraftRarityLocks(normalProfile, rerollIndex);
                    }
                    break;
                case SurvivorsRewardSelectionKind.EliteUpgrade:
                    draftCatalog = CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Miniboss, requireEvolutionChoice: false);
                    resolvedLocks = CreateEligibleEvolutionChoiceLocks(CurrentTuning.DraftChoiceCount);
                    break;
                case SurvivorsRewardSelectionKind.BossUpgrade:
                    draftCatalog = CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Boss, requireEvolutionChoice: false);
                    resolvedLocks = CreateEligibleEvolutionChoiceLocks(CurrentTuning.DraftChoiceCount);
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
                int requiredRank = Mathf.Max(1, metadata.RequiredUpgradeRank);
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
                _ownedPassiveUpgradeIds.Add(upgrade.Id.Value);
            }

            if (metadata.UsesWeaponSlot)
            {
                TryAddWeaponToLoadout(metadata.AffectedContentId);
            }

            if (metadata.IsEvolution)
            {
                _ownedEvolutionUpgradeIds.Add(upgrade.Id.Value);
                WeaponEvolutionFeedbackCount++;
                TriggerWeaponEvolutionSurge(upgrade);
            }
        }

        private void TriggerWeaponEvolutionSurge(RunUpgradeDefinition upgrade)
        {
            float radius = Mathf.Max(0f, CurrentTuning.EvolutionSurgeRadius);
            float damage = Mathf.Max(0f, CurrentTuning.EvolutionSurgeDamage);
            string name = upgrade == null ? "Evolution" : BasicSurvivorsGame.GetUpgradeDisplayName(upgrade.Id);
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

            WeaponEvolutionSurgeCount++;
            WeaponEvolutionSurgeHitCount += hitCount;
            LastWeaponEvolutionSurgeFeedbackLabel = $"{name} Surge: {hitCount} enemies hit";
            RecordStreakRewardFeedback(LastWeaponEvolutionSurgeFeedbackLabel, new Color(0.72f, 0.42f, 1f));
            PlayFeedback(_levelUpPulse, PlayerPosition, Mathf.Clamp(46 + hitCount * 5, 54, 96), _levelUpClip);
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
                    continue;
                }

                _announcedEvolutionReadyUpgradeIds.Add(upgradeId);
                RecordEvolutionReadyFeedback(definition);
            }
        }

        private void RecordEvolutionReadyFeedback(RunUpgradeDefinition evolution)
        {
            string name = BasicSurvivorsGame.GetUpgradeDisplayName(evolution.Id);
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
                return allDefinitions;
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
            return definition != null && _weaponLoadout.TryAddWeapon(definition);
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
                definitions = BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(CurrentTuning);
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

            SurvivorsMetaProgressionDefinition definition = BasicSurvivorsGame.CreateMetaProgressionDefinition();
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
            LastRunResult = SurvivorsRunRewardCalculator.Calculate(
                RunTimeSeconds,
                Level,
                MinibossKilledCount,
                BossKilledCount,
                victory,
                _bonusBloodShardsEarnedThisRun,
                _bonusLegacyExperienceEarnedThisRun);
            BloodShardsEarnedThisRun = LastRunResult.BloodShardsEarned;
            LegacyExperienceEarnedThisRun = LastRunResult.LegacyExperienceEarned;
            if (_metaProgression.GrantRunRewards(LastRunResult).Succeeded)
            {
                _runRewardsGranted = true;
                if (victory)
                {
                    GrantVictoryClassUnlockReward();
                }
            }

            RebuildLastRunSummaryLines(victory);
        }

        private void RebuildLastRunSummaryLines(bool victory)
        {
            _lastRunSummaryLines.Clear();
            string result = victory ? "Victory" : "Defeat";
            _lastRunSummaryLines.Add($"{result} - {FormatRunTime(RunTimeSeconds)} - Level {Level}");
            _lastRunSummaryLines.Add($"Kills {KilledCount} - Best Streak {BestKillStreak} - Bosses {BossKilledCount}");
            _lastRunSummaryLines.Add($"Build {ActiveWeaponCount}/{MaxWeaponSlots} weapons, {ActivePassiveCount}/{MaxPassiveSlots} passives, {EvolvedWeaponCount} evolutions, {SelectedRelicCount}/{ResolveTotalRelicCount()} relics");
            _lastRunSummaryLines.Add($"Rewards +{BloodShardsEarnedThisRun} shards, +{LegacyExperienceEarnedThisRun} XP");
            _lastRunSummaryLines.Add($"Meta {MetaBloodShards} shards banked, {LifetimeLegacyExperience} lifetime XP");
            if (ClassUnlockRewardCount > 0)
            {
                _lastRunSummaryLines.Add("Class unlocked: Ember Vanguard");
            }
        }

        private void GrantVictoryClassUnlockReward()
        {
            EnsureClassLibraryLoaded();
            if (_metaProgression != null && _metaProgression.UnlockClass(BasicSurvivorsGame.EmberVanguardClassId, _classLibrary))
            {
                ClassUnlockRewardCount++;
            }
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

            _roamingCacheTravelDistance += delta.magnitude;
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
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * (1f + RunEscalationLevel * 0.08f)));
            int gemCount = Mathf.Max(1, CurrentTuning.RoamingCacheExperienceGemCount);
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
                if (SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, CurrentTuning.BloodShardPickupAmount) != null)
                {
                    spawnedBloodShard = true;
                    RoamingCacheBloodShardDropCount++;
                }
            }

            int ambushSpawned = SpawnRoamingArenaCacheAmbush(origin, forward, side, nextCacheNumber);
            if (spawnedExperience <= 0 && !spawnedMagnet && !spawnedBloodShard && ambushSpawned <= 0)
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

            LastRoamingCacheFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(0.35f, 0.9f, 1f));
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
            int targetCount = Mathf.Clamp(baseCount + distancePressureBonus + escalationBonus, 1, maxCount);
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
                float angle = ((i + 0.5f) / targetCount) * Mathf.PI * 2f;
                Vector3 offset = side * (Mathf.Cos(angle) * radius) + forward * (Mathf.Sin(angle) * radius);
                SurvivorsEnemyRole role = ResolveRoamingCacheAmbushRole(i, cacheNumber);
                SurvivorsEnemyActor enemy = SpawnEnemy(center + offset, explicitPosition: true, role);
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
            int gemCount = Mathf.Max(1, CurrentTuning.RoamingCacheExperienceGemCount + 1);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(CurrentTuning.EnemyExperienceReward * (1.35f + RunEscalationLevel * 0.08f)));
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

            RoamingCacheAmbushClearRewardCount++;
            string label = $"Roaming Ambush Cleared: +{spawnedExperience} XP";
            LastRoamingCacheAmbushClearFeedbackLabel = label;
            LastRoamingCacheFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(0.5f, 1f, 0.68f));
            PlayFeedback(_levelUpPulse, position, 18, _pickupClip);
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

            float anchorX = Mathf.Floor(player.x / InfiniteArenaTileSize) * InfiniteArenaTileSize;
            float anchorZ = Mathf.Floor(player.z / InfiniteArenaTileSize) * InfiniteArenaTileSize;
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
                SpawnEnemy(Vector3.zero, explicitPosition: false, timedEliteRole);
            }

            if (_runFlow.TryConsumeMinibossSpawn(RunTimeSeconds))
            {
                SpawnEnemy(Vector3.zero, explicitPosition: false, SurvivorsEnemyRole.Miniboss);
            }

            if (_runFlow.TryConsumeBossSpawn(RunTimeSeconds))
            {
                SpawnEnemy(Vector3.zero, explicitPosition: false, SurvivorsEnemyRole.Boss);
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
            PlayFeedback(_bossPulse, PlayerPosition, role == SurvivorsEnemyRole.Boss ? 44 : 28, _dangerClip);
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

            if (SpawnEnemy(Vector3.zero, explicitPosition: false, role) == null)
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
                float angle = ((i + 0.37f + _hordeRushSequence * 0.19f) / targetCount) * Mathf.PI * 2f;
                float laneRadius = radius + ((i & 1) == 0 ? 0f : 1.35f);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * laneRadius;
                SurvivorsEnemyRole role = ResolveHordeRushRole(i);
                SurvivorsEnemyActor enemy = SpawnEnemy(PlayerPosition + offset, explicitPosition: true, role);
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
            string specialLabel = specialDropCount > 0 ? $" + {specialDropCount} special" : string.Empty;
            string label = $"Horde Rush Cleared: +{spawnedExperience} XP{specialLabel}";
            LastHordeRushClearFeedbackLabel = label;
            LastHordeRushFeedbackLabel = label;
            RecordStreakRewardFeedback(label, new Color(1f, 0.74f, 0.24f));
            PlayFeedback(_levelUpPulse, position, 24, _pickupClip);
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
            PlayFeedback(_levelUpPulse, PlayerPosition, 42, _levelUpClip);
        }

        private float ResolveEnemySpawnIntervalSeconds()
        {
            float interval = _runFlow == null
                ? Mathf.Max(0.05f, CurrentTuning.EnemySpawnIntervalSeconds)
                : _runFlow.ResolveSpawnInterval(CurrentTuning.EnemySpawnIntervalSeconds);
            return ResolveEndlessSpawnInterval(interval);
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
                if (SpawnEnemy(Vector3.zero, explicitPosition: false, role) == null)
                {
                    break;
                }
            }

            _enemySpawnTimer = ResolveEnemySpawnIntervalSeconds();
        }

        private SurvivorsEnemyActor SpawnEnemy(Vector3 position, bool explicitPosition, SurvivorsEnemyRole role)
        {
            long sequence = ++_spawnSequence;
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
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                MinibossSpawnCount++;
                PlayFeedback(_bossPulse, enemy.transform.position, 42, _bossClip);
            }
            else if (role == SurvivorsEnemyRole.Boss)
            {
                BossSpawnCount++;
                PlayFeedback(_bossPulse, enemy.transform.position, 58, _bossClip);
            }
            else if (IsEliteRole(role))
            {
                PlayFeedback(_bossPulse, enemy.transform.position, 30, _bossClip);
            }
            else
            {
                PlayFeedback(_spawnPulse, enemy.transform.position, 10, _spawnClip);
            }

            return enemy;
        }

        private void SpawnSplitterChildren(Vector3 position)
        {
            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                return;
            }

            float radius = Mathf.Max(0.65f, CurrentTuning.EnemyRadius * 1.5f);
            for (int index = 0; index < SplitterChildCount; index++)
            {
                float angle = ((index + 0.25f) / SplitterChildCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (SpawnEnemy(position + offset, explicitPosition: true, SurvivorsEnemyRole.Swarm) != null)
                {
                    SplitterChildSpawnCount++;
                }
            }
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
            pickup.Initialize(this, kind, Mathf.Max(1, amount), CurrentPickupAttractRange, CurrentTuning.PickupAttractionSpeed, CurrentTuning.PickupCollectRadius);
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

                enemy.Simulate(deltaTime);
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

                pickup.UpdateAttractionSettings(CurrentPickupAttractRange, CurrentTuning.PickupAttractionSpeed, CurrentTuning.PickupCollectRadius);
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
            int gained = Mathf.Max(1, Mathf.RoundToInt(baseAmount * Mathf.Max(0.1f, 1f + ExperienceGainMultiplierBonus)));
            ExperienceCollected += gained;
            Experience += gained;
            while (Experience >= RequiredExperienceForNextLevel)
            {
                Experience -= RequiredExperienceForNextLevel;
                Level++;
                PendingLevelUps++;
            }

            if (PendingLevelUps > 0 && State == SurvivorsRunState.Playing)
            {
                OpenLevelUpDraft();
            }

            return gained;
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
            }

            _experienceComboPickupCount++;
            _experienceComboAmount += gained;
            _experienceComboTimer = ExperienceComboWindowSeconds;

            if (_experienceComboPickupCount < ExperienceComboMinimumPickupCount)
            {
                return;
            }

            ExperienceComboFeedbackCount++;
            _experienceComboFeedbackLabel = $"{_experienceComboPickupCount} Gem Rush: +{_experienceComboAmount} XP";
            LastExperienceComboFeedbackLabel = _experienceComboFeedbackLabel;
            _experienceComboFeedbackTimer = ExperienceComboFeedbackDurationSeconds;
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
            RecordRewardCardPresentation(_rewardSelectionKind, _currentDraft);
            BeginRewardSelectionTimeout();
            PlayFeedback(_levelUpPulse, PlayerPosition, 34, _levelUpClip);
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
            if (role == SurvivorsEnemyRole.Boss)
            {
                BossUpgradeDraftOpenCount++;
            }
            else
            {
                EliteUpgradeDraftOpenCount++;
            }

            State = SurvivorsRunState.LevelUp;
            RecordRewardCardPresentation(selectionKind, _currentDraft);
            BeginRewardSelectionTimeout();
            PlayFeedback(_bossPulse, PlayerPosition, role == SurvivorsEnemyRole.Boss ? 72 : 54, _bossClip);
            return true;
        }

        private void ApplyPacingProfile(SurvivorsPacingProfile profile, bool restartRun)
        {
            tuning = BasicSurvivorsGame.CreateTuning(profile);
            pacingProfile = tuning.PacingProfile;
            if (restartRun)
            {
                RestartRun();
            }
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
            RecordRewardCardPresentation(_currentRelicDraft);
            BeginRewardSelectionTimeout();
            PlayFeedback(_bossPulse, PlayerPosition, 44, _bossClip);
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

            string name = BasicSurvivorsGame.GetUpgradeDisplayName(selected.Id);
            string category = ResolveCurrentUpgradeCategory(selected).ToString();
            string affected = ResolveUpgradeAffectedLabel(selected);
            LastRewardSelectionFeedbackLabel = $"{ResolveRewardKindLabel(selectionKind)}: {selected.Rarity} {category} - {name} ({affected})";
            RewardSelectionFeedbackCount++;
            _rewardFeedbackLabel = LastRewardSelectionFeedbackLabel;
            _rewardFeedbackColor = ResolveRarityAccentColor(selected.Rarity);
            _rewardFeedbackTimer = RewardFeedbackDurationSeconds;
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
        }

        private void RecordRewardSkipFeedback(SurvivorsRewardSelectionKind selectionKind)
        {
            LastRewardSelectionFeedbackLabel = $"{ResolveRewardKindLabel(selectionKind)} skipped +{DraftSkipBloodShards} shards";
            RewardSelectionFeedbackCount++;
            _rewardFeedbackLabel = LastRewardSelectionFeedbackLabel;
            _rewardFeedbackColor = new Color(0.72f, 0.84f, 0.9f);
            _rewardFeedbackTimer = RewardFeedbackDurationSeconds;
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

                if (PendingLevelUps > 0)
                {
                    OpenLevelUpDraft();
                }
                else
                {
                    State = SurvivorsRunState.Playing;
                }
            }
            else if (PendingLevelUps > 0)
            {
                OpenLevelUpDraft();
            }
            else
            {
                State = SurvivorsRunState.Playing;
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
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
            _pendingVictoryAfterRewardDraft = false;
            _pendingBossRelicAfterRewardDraft = false;
            _currentDraftRerollIndex = 0;
            if (PendingLevelUps > 0)
            {
                OpenLevelUpDraft();
            }
            else
            {
                State = SurvivorsRunState.Playing;
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
            float width = upgradeDraftOpen ? 660f : 560f;
            int choiceCount = IsRelicChoiceOpen ? CurrentRelicChoices.Count : CurrentDraftChoices.Count;
            float rowHeight = IsRelicChoiceOpen ? 60f : 78f;
            float footerHeight = upgradeDraftOpen ? 48f : 0f;
            float height = 112f + choiceCount * rowHeight + footerHeight;
            Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
            GUI.Box(rect, ResolveRewardOverlayTitle());
            GUI.Label(
                new Rect(rect.x + 24f, rect.y + 28f, width - 48f, 18f),
                RewardSelectionRemainingSeconds > 0f ? $"Auto-pick in {RewardSelectionRemainingSeconds:0}s" : "Choose a reward",
                _hudSmallStyle);
            for (int i = 0; i < choiceCount; i++)
            {
                Rect rowRect = new Rect(rect.x + 24f, rect.y + 68f + i * rowHeight, width - 48f, rowHeight - 12f);
                Rect buttonRect = upgradeDraftOpen
                    ? new Rect(rowRect.x, rowRect.y, rowRect.width - 88f, rowRect.height)
                    : rowRect;
                string label;
                Color cardAccent;
                if (IsRelicChoiceOpen)
                {
                    SurvivorsRelicDefinition relic = CurrentRelicChoices[i];
                    label = FormatRelicChoiceLabel(i, relic);
                    cardAccent = ResolveRelicAccentColor(relic);
                }
                else
                {
                    RunUpgradeDefinition choice = CurrentDraftChoices[i];
                    label = FormatUpgradeChoiceLabel(i, choice);
                    cardAccent = choice == null ? Color.white : ResolveRarityAccentColor(choice.Rarity);
                }

                DrawRewardChoiceCard(rowRect, cardAccent);
                Color previousBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = ResolveRewardButtonBackgroundColor(cardAccent);
                if (GUI.Button(buttonRect, label))
                {
                    GUI.backgroundColor = previousBackgroundColor;
                    if (SelectUpgrade(i))
                    {
                        return;
                    }
                }
                GUI.backgroundColor = previousBackgroundColor;

                if (upgradeDraftOpen)
                {
                    bool previousEnabled = GUI.enabled;
                    GUI.enabled = previousEnabled && CanBanishCurrentDraft();
                    Rect banishRect = new Rect(rowRect.xMax - 76f, rowRect.y, 76f, rowRect.height);
                    if (GUI.Button(banishRect, "Banish"))
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

            if (upgradeDraftOpen)
            {
                float footerY = rect.y + 72f + choiceCount * rowHeight;
                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && CanRerollCurrentDraft();
                if (GUI.Button(new Rect(rect.x + 24f, footerY, 160f, 30f), $"Reroll ({DraftRerollsRemaining})"))
                {
                    if (RerollCurrentDraft())
                    {
                        GUI.enabled = previousEnabled;
                        return;
                    }
                }

                GUI.enabled = previousEnabled && CanSkipCurrentDraft();
                if (GUI.Button(new Rect(rect.x + 194f, footerY, 180f, 30f), $"Skip (+{DraftSkipBloodShards} shards)"))
                {
                    if (SkipCurrentDraft())
                    {
                        GUI.enabled = previousEnabled;
                        return;
                    }
                }

                GUI.enabled = previousEnabled;
                GUI.Label(
                    new Rect(rect.x + 390f, footerY + 6f, width - 414f, 18f),
                    $"Banishes {DraftBanishesRemaining}",
                    _hudSmallStyle);
            }
        }

        private void DrawRunResultOverlay(bool victory)
        {
            const float width = 560f;
            float height = victory ? 420f : 390f;
            Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
            GUI.Box(rect, victory ? "Victory" : "Game Over");
            IReadOnlyList<string> lines = LastRunSummaryLines;
            int shown = Mathf.Min(lines == null ? 0 : lines.Count, 6);
            if (shown == 0)
            {
                GUI.Label(new Rect(rect.x + 24f, rect.y + 34f, width - 48f, 22f), $"Rewards {BloodShardsEarnedThisRun} shards / {LegacyExperienceEarnedThisRun} XP", _hudLabelStyle);
            }
            else
            {
                for (int i = 0; i < shown; i++)
                {
                    GUI.Label(new Rect(rect.x + 24f, rect.y + 34f + i * 22f, width - 48f, 22f), lines[i], _hudSmallStyle);
                }
            }

            DrawResultClassOptions(new Rect(rect.x + 24f, rect.y + 166f, width - 48f, 66f));
            DrawResultMetaUpgradeOptions(new Rect(rect.x + 24f, rect.y + 246f, width - 48f, 96f));

            float buttonY = rect.yMax - 48f;
            if (victory)
            {
                if (GUI.Button(new Rect(rect.x + 160f, buttonY, 106f, 34f), "Continue"))
                {
                    ContinueAfterVictory();
                }

                if (GUI.Button(new Rect(rect.x + width - 266f, buttonY, 106f, 34f), "Restart"))
                {
                    RestartRun();
                }
            }
            else if (GUI.Button(new Rect(rect.x + width * 0.5f - 70f, buttonY, 140f, 34f), "Restart"))
            {
                RestartRun();
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
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20f), $"Meta Upgrades - {MetaBloodShards} shards banked", _hudLabelStyle);
            if (options.Count == 0)
            {
                GUI.Label(new Rect(rect.x, rect.y + 24f, rect.width, 20f), "No affordable meta upgrades yet. Bank more shards from runs, elites, bosses, or skips.", _hudSmallStyle);
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

            string name = BasicSurvivorsGame.GetUpgradeDisplayName(definition.Id);
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
            string name = BasicSurvivorsGame.GetUpgradeDisplayName(definition.Id);
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
            string name = BasicSurvivorsGame.GetUpgradeDisplayName(choice.Id);
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
            Rect panel = new Rect(Screen.width * 0.5f - 184f, 24f, 368f, 52f);
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
            float y = IsMajorThreatWarningActive ? 84f : 24f;
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
            SurvivorsEnemyActor enemy = ResolveCurrentMajorThreatForHud();
            if (enemy == null)
            {
                return;
            }

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

            Rect panel = new Rect(x, y, width, 46f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.03f, 0.02f, 0.02f, 0.66f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = oldColor;
            string label = (string.IsNullOrWhiteSpace(enemy.DisplayName) ? ResolveMajorThreatHealthFallbackLabel(enemy.Role) : enemy.DisplayName) + " HP";
            DrawHudBar(new Rect(panel.x + 10f, panel.y + 13f, panel.width - 20f, 20f), label, enemy.HealthFraction, ResolveMajorRewardDropColor(enemy.Role));
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

        private static string ShortWeaponName(string weaponId)
        {
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
        private float _majorThreatSlamCooldown;
        private float _majorThreatSlamTelegraphTimer;
        private bool _majorThreatSlamTelegraphing;
        private float _poisonDamagePerSecond;
        private float _poisonRemainingSeconds;
        private float _bleedDamagePerSecond;
        private float _bleedRemainingSeconds;
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
            _majorThreatSlamCooldown = ResolveInitialMajorThreatSlamCooldown(profile.Role, controller == null ? null : controller.CurrentTuning);
            _majorThreatSlamTelegraphTimer = 0f;
            _majorThreatSlamTelegraphing = false;
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            ExperienceReward = Mathf.Max(1, profile.ExperienceReward);
            string id = InstanceId.Value > 0 ? "combatant.survivors.enemy." + InstanceId.Value : "combatant.survivors.enemy.pending";
            float maxHealth = Mathf.Max(1f, profile.MaxHealth);
            _health = new HealthState(new CombatantId(id), maxHealth, maxHealth);
            transform.localScale = Vector3.one * (Radius * 2f);
            _baseScale = transform.localScale;
            _hitFlashTimer = 0f;
            _hitFlashDuration = 0f;
            _hitFlashCritical = false;
            ApplyTint(profile.Tint);
        }

        public void Simulate(float deltaTime)
        {
            if (!IsAlive || _controller == null || !_controller.IsPlaying)
            {
                return;
            }

            TickHitFlash(deltaTime);
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
                transform.position += resolvedMove * (_moveSpeed * deltaTime);
                transform.forward = resolvedMove;
            }
            else if (distance > 0.001f)
            {
                transform.forward = facingDirection;
            }

            TickMajorThreatSlam(deltaTime, distance);
            TickRangedAttack(deltaTime, distance);
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

            if (string.Equals(statusId, "status.survivors.bleed", StringComparison.Ordinal))
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
                Color flash = _hitFlashCritical
                    ? new Color(1f, 0.86f, 0.25f)
                    : Color.white;
                _runtimeMaterial.color = Color.Lerp(_baseTint, flash, Mathf.Clamp01(intensity));
            }

            transform.localScale = _baseScale * (1f + 0.18f * Mathf.Clamp01(intensity));
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
            _rangedAttackWindupTimer = 0f;
            _rangedAttackTelegraphing = false;
            _hitFlashTimer = 0f;
            _hitFlashDuration = 0f;
            _hitFlashCritical = false;
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
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            _hitFlashTimer = 0f;
            _hitFlashDuration = 0f;
            _hitFlashCritical = false;
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
                hitEnemy.ApplyDamage(_damage, _definition == null ? "combatant.survivors.player" : _definition.Id);
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

            float angle = (request.Sequence * 137.50777f) * Mathf.Deg2Rad;
            float radius = _controller == null ? 12f : _controller.CurrentTuning.EnemySpawnRadius;
            Vector3 center = _controller == null ? Vector3.zero : _controller.PlayerPosition;
            Vector3 position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            return SpawnPoseResult.Success(new SpawnPose(position, Quaternion.identity));
        }
    }
}
