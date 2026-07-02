using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Combat;
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
        private Transform _worldRoot;
        private Transform _prefabRoot;
        private GameObject _playerObject;
        private Renderer _playerRenderer;
        private Camera _camera;
        private GameObject _enemyPrefab;
        private GameObject _experiencePickupPrefab;
        private GameObject _magnetPickupPrefab;
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
        private SurvivorsSpawnPoseResolver _poseResolver;
        private WorldSpawnService _spawnService;
        private HealthState _playerHealth;
        private RunUpgradeCatalog _upgradeCatalog;
        private RunUpgradeState _upgradeState;
        private RunUpgradeDraft _currentDraft;
        private SurvivorsRelicDraft _currentRelicDraft;
        private SurvivorsRewardSelectionKind _rewardSelectionKind;
        private CombatCatalog _combatCatalog;
        private WeaponDefinition _weaponDefinition;
        private ProjectileDefinition _projectileDefinition;
        private SurvivorsWeaponLoadoutRuntime _weaponLoadout;
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
        private float _rewardSelectionTimer;
        private long _spawnSequence;
        private bool _runStarted;
        private bool _ownsMetaProgressionService;
        private bool _metaProfileLoaded;
        private bool _runRewardsGranted;
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
        public int MinibossSpawnCount { get; private set; }
        public int BossSpawnCount { get; private set; }
        public int MinibossKilledCount { get; private set; }
        public int BossKilledCount { get; private set; }
        public int MinibossRewardGrantCount { get; private set; }
        public int BossRewardGrantCount { get; private set; }
        public int BossRelicDraftOpenCount { get; private set; }
        public int SelectedRelicCount { get; private set; }
        public int RewardAutoSelectCount { get; private set; }
        public int ClassUnlockRewardCount { get; private set; }
        public int ExperienceCollected { get; private set; }
        public int SelectedUpgradeCount { get; private set; }
        public int MagnetRecallCount { get; private set; }
        public int BonusBloodShardsEarnedThisRun => _bonusBloodShardsEarnedThisRun;
        public int BonusLegacyExperienceEarnedThisRun => _bonusLegacyExperienceEarnedThisRun;
        public int BloodShardsEarnedThisRun { get; private set; }
        public int LegacyExperienceEarnedThisRun { get; private set; }
        public SurvivorsRunRewardSummary LastRunResult { get; private set; }
        public float RunTimeSeconds { get; private set; }
        public float MoveSpeedBonus { get; private set; }
        public float DamageBonus { get; private set; }
        public float PersistentDamageBonus { get; private set; }
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
        public float LifestealRatio { get; private set; }
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
        public int ActiveEliteCount => CountEnemiesByRole(SurvivorsEnemyRole.Elite);
        public int ActiveMinibossCount => CountEnemiesByRole(SurvivorsEnemyRole.Miniboss);
        public int ActiveBossCount => CountEnemiesByRole(SurvivorsEnemyRole.Boss);
        public int ActivePickupCount => _pickups.Count;
        public int ActiveProjectileCount => _projectiles.Count;
        public int ActiveWeaponCount => _weaponLoadout == null ? 0 : _weaponLoadout.WeaponCount;
        public IReadOnlyList<string> ActiveWeaponIds => _weaponLoadout == null ? EmptyWeaponIds : _weaponLoadout.WeaponIds;
        public int ActiveOrbitBladeCount => _weaponLoadout == null ? 0 : _weaponLoadout.ActiveOrbitBladeCount;
        public float PlayerMoveSpeed => CurrentTuning.PlayerMoveSpeed + MoveSpeedBonus;
        public float ProjectileDamage => Mathf.Max(0f, (float)_projectileDefinition.BaseDamage + DamageBonus);
        public float WeaponCooldownSeconds => Mathf.Max(0.12f, CurrentTuning.WeaponCooldownSeconds * Mathf.Max(0.2f, 1f + WeaponCooldownMultiplierBonus));
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
        public bool IsGameOver => State == SurvivorsRunState.GameOver;
        public bool IsVictory => State == SurvivorsRunState.Victory;
        public int RequiredExperienceForNextLevel => Mathf.Max(1, CurrentTuning.ExperienceRequiredBase + ((Level - 1) * CurrentTuning.ExperienceRequiredPerLevel));

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
                TickRewardSelectionTimeout(Time.deltaTime);
                HandleLevelUpInput();
                return;
            }

            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartRun();
                }
                return;
            }

            Vector2 movement = ReadMovementInput();
            Simulate(Time.deltaTime, movement);

            if (Input.GetKeyDown(KeyCode.M))
            {
                TriggerMagnetRecall();
            }
        }

        private void LateUpdate()
        {
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
            GUI.Box(new Rect(12, 12, 356, 330), string.Empty);
            GUI.Label(new Rect(24, 22, 300, 22), "Deucarian Survivors Run", _hudTitleStyle);
            DrawHudBar(new Rect(24, 50, 318, 18), "Health", MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth, new Color(0.9f, 0.22f, 0.24f));
            DrawHudBar(new Rect(24, 74, 318, 18), "Barrier", BarrierCapacity <= 0f ? 0f : BarrierValue / BarrierCapacity, new Color(0.42f, 0.8f, 1f));
            DrawHudBar(new Rect(24, 98, 318, 18), "XP", Experience / (float)RequiredExperienceForNextLevel, new Color(0.2f, 0.78f, 1f));
            DrawHudBar(new Rect(24, 122, 318, 18), "Run", Mathf.Clamp01(RunTimeSeconds / Mathf.Max(1f, CurrentTuning.SurvivalVictoryTimeSeconds)), new Color(0.72f, 0.44f, 1f));
            GUI.Label(new Rect(24, 148, 318, 22), $"LV {Level}   Time {FormatRunTime(RunTimeSeconds)}   Phase {RunPhase} +{RunEscalationLevel}", _hudLabelStyle);
            GUI.Label(new Rect(24, 170, 318, 22), $"Enemies {ActiveEnemyCount}/{CurrentEnemyMaximumAlive}   Kills {KilledCount}   Elite {ActiveEliteCount}", _hudLabelStyle);
            GUI.Label(new Rect(24, 192, 318, 22), $"Miniboss {ActiveMinibossCount}   Boss {ActiveBossCount}   Shards {MetaBloodShards}", _hudLabelStyle);
            GUI.Label(new Rect(24, 214, 318, 22), $"Poison {PoisonDamageRatio:0.##}   Bleed {BleedDamageRatio:0.##}   Execute {ExecuteThresholdNormalized:P0}", _hudSmallStyle);
            GUI.Label(new Rect(24, 236, 318, 22), "Weapons: " + ResolveWeaponHudLabel(), _hudSmallStyle);
            GUI.Label(new Rect(24, 258, 318, 22), $"Profile {BasicSurvivorsGame.GetPacingProfileDisplayName(CurrentPacingProfile)}   TimeScale {Time.timeScale:0.##}", _hudSmallStyle);
            GUI.Label(new Rect(24, 280, 318, 22), $"Spawn {CurrentEnemySpawnIntervalSeconds:0.00}s   Enemy Speed x{CurrentEnemySpeedMultiplier:0.##}", _hudSmallStyle);
            GUI.Label(new Rect(24, 302, 318, 22), $"Reward Timeout {FormatRewardTimeout(CurrentTuning.RewardSelectionTimeoutSeconds)}", _hudSmallStyle);

            if (State == SurvivorsRunState.LevelUp)
            {
                DrawLevelUpOverlay();
            }
            else if (State == SurvivorsRunState.GameOver)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 82f, 300f, 164f), "Game Over");
                GUI.Label(new Rect(Screen.width * 0.5f - 116f, Screen.height * 0.5f - 28f, 232f, 22f), $"Rewards {BloodShardsEarnedThisRun} shards / {LegacyExperienceEarnedThisRun} XP");
                if (GUI.Button(new Rect(Screen.width * 0.5f - 70f, Screen.height * 0.5f + 18f, 140f, 34f), "Restart"))
                {
                    RestartRun();
                }
            }
            else if (State == SurvivorsRunState.Victory)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 82f, 300f, 164f), "Victory");
                GUI.Label(new Rect(Screen.width * 0.5f - 116f, Screen.height * 0.5f - 28f, 232f, 22f), $"Run cleared in {RunTimeSeconds:0}s");
                GUI.Label(new Rect(Screen.width * 0.5f - 116f, Screen.height * 0.5f - 6f, 232f, 22f), $"Rewards {BloodShardsEarnedThisRun} shards / {LegacyExperienceEarnedThisRun} XP");
                if (GUI.Button(new Rect(Screen.width * 0.5f - 70f, Screen.height * 0.5f + 30f, 140f, 34f), "Restart"))
                {
                    RestartRun();
                }
            }
        }

        public void StartRun()
        {
            Time.timeScale = 1f;
            ClearRun();
            SurvivorsTemplateTuning resolved = CurrentTuning;
            _combatCatalog = BasicSurvivorsGame.CreateCombatCatalog();
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
            MinibossSpawnCount = 0;
            BossSpawnCount = 0;
            MinibossKilledCount = 0;
            BossKilledCount = 0;
            MinibossRewardGrantCount = 0;
            BossRewardGrantCount = 0;
            BossRelicDraftOpenCount = 0;
            SelectedRelicCount = 0;
            RewardAutoSelectCount = 0;
            ClassUnlockRewardCount = 0;
            ExperienceCollected = 0;
            SelectedUpgradeCount = 0;
            MagnetRecallCount = 0;
            BloodShardsEarnedThisRun = 0;
            LegacyExperienceEarnedThisRun = 0;
            LastRunResult = null;
            RunTimeSeconds = 0f;
            MoveSpeedBonus = 0f;
            DamageBonus = 0f;
            PersistentDamageBonus = 0f;
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
            LifestealRatio = 0f;
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
            _bonusBloodShardsEarnedThisRun = 0;
            _bonusLegacyExperienceEarnedThisRun = 0;
            _enemySpawnTimer = 0f;
            _playerInvulnerabilityTimer = 0f;
            _spawnSequence = 0;
            _currentDraft = null;
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
            ApplyPersistentMetaBonuses();
            ApplySelectedClassBonuses();
            BarrierValue = BarrierCapacity;
            BuildRuntimeWorld();
            _runFlow = new SurvivorsRunFlowRuntime(BasicSurvivorsGame.CreateRunFlowDefinition(resolved));
            _weaponLoadout = new SurvivorsWeaponLoadoutRuntime(this, ResolveStartingWeaponDefinitions(BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(resolved)));
            State = SurvivorsRunState.Playing;
            _runStarted = true;
        }

        public void RestartRun()
        {
            StartRun();
        }

        public void Simulate(float deltaTime, Vector2 movementInput = default)
        {
            if (!_runStarted)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
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
            TickRunFlow();
            if (State != SurvivorsRunState.Playing)
            {
                return;
            }

            _playerInvulnerabilityTimer = Mathf.Max(0f, _playerInvulnerabilityTimer - dt);
            TickBarrier(dt);
            MovePlayer(movementInput, dt);
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

        public int DebugFillArenaToTarget(SurvivorsEnemyRole role, int targetAlive, float radius)
        {
            EnsureRunStartedForTest();
            int target = Mathf.Clamp(targetAlive, 1, 512);
            int needed = Mathf.Max(0, target - ActiveEnemyCount);
            return needed <= 0 ? 0 : DebugSpawnEnemyBurst(role, needed, radius);
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

        public bool TryPurchasePersistentUpgrade(string id)
        {
            EnsureMetaProgressionLoaded();
            bool purchased = _metaProgression.TryPurchasePersistentUpgrade(id);
            if (purchased && _runStarted)
            {
                ApplyPersistentMetaBonuses();
            }

            return purchased;
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
            CombatDamageResolver.Resolve(_combatCatalog, _playerHealth, null, request);
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

            if (State != SurvivorsRunState.LevelUp || _rewardSelectionKind != SurvivorsRewardSelectionKind.LevelUp || _currentDraft == null || index < 0 || index >= _currentDraft.Choices.Count)
            {
                return false;
            }

            RunUpgradeDefinition selected = _currentDraft.Choices[index];
            RunUpgradeSelectionResult selection = _upgradeState.Select(_upgradeCatalog, selected.Id);
            if (!selection.Succeeded)
            {
                return false;
            }

            ApplyUpgrade(selected);
            SelectedUpgradeCount++;
            PendingLevelUps = Mathf.Max(0, PendingLevelUps - 1);
            _currentDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
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

        public void TriggerMagnetRecall()
        {
            MagnetRecallCount++;
            for (int i = 0; i < _pickups.Count; i++)
            {
                SurvivorsPickupActor pickup = _pickups[i];
                if (pickup != null && pickup.Kind == SurvivorsPickupKind.Experience)
                {
                    pickup.StartGlobalRecall(CurrentTuning.MagnetRecallSpeedMultiplier);
                }
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

        internal void HandleEnemyKilled(SurvivorsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return;
            }

            KilledCount++;
            Vector3 position = enemy.transform.position;
            SurvivorsEnemyRole role = enemy.Role;
            int xp = Mathf.Max(1, enemy.ExperienceReward);
            _enemies.Remove(enemy);
            if (_spawnService != null && enemy.InstanceId.Value > 0)
            {
                _spawnService.Despawn(enemy.InstanceId, DespawnReason.Killed);
            }

            SpawnPickup(SurvivorsPickupKind.Experience, position, xp);
            PlayFeedback(_killPulse, position, role == SurvivorsEnemyRole.Swarm ? 18 : 34, _killClip);
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                MinibossKilledCount++;
                GrantBossReward(SurvivorsEnemyRole.Miniboss);
                OpenBossRelicDraft();
            }
            else if (role == SurvivorsEnemyRole.Boss)
            {
                BossKilledCount++;
                GrantBossReward(SurvivorsEnemyRole.Boss);
                EnterVictory();
            }
        }

        internal void ReleaseEnemy(SurvivorsEnemyActor enemy, DespawnReason reason)
        {
            if (enemy == null)
            {
                return;
            }

            _enemies.Remove(enemy);
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
            else
            {
                GainExperience(Mathf.Max(1, pickup.Amount));
            }

            PlayFeedback(_pickupPulse, pickup.transform.position, pickup.Kind == SurvivorsPickupKind.Magnet ? 28 : 10, _pickupClip);
            _pickups.Remove(pickup);
            if (_spawnService != null && pickup.InstanceId.Value > 0)
            {
                _spawnService.Despawn(pickup.InstanceId, DespawnReason.Completed);
            }
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

            return Mathf.Max(0f, definition.Damage + DamageBonus);
        }

        internal float ResolveWeaponCooldownSeconds(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null)
            {
                return WeaponCooldownSeconds;
            }

            return Mathf.Max(0.08f, definition.CooldownSeconds * Mathf.Max(0.2f, 1f + WeaponCooldownMultiplierBonus));
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
            _projectilePrefab = CreatePrimitivePrefab("Survivors Arcane Bolt Prefab", PrimitiveType.Sphere, new Color(0.78f, 0.42f, 1f), typeof(SurvivorsProjectileActor));

            _poseResolver = new SurvivorsSpawnPoseResolver(this);
            var spawnables = new[]
            {
                new SpawnableDefinition(BasicSurvivorsGame.SwarmEnemySpawnableId, new GameObjectPrefabProvider(_enemyPrefab), 24, Mathf.Max(CurrentTuning.EnemyMaximumAlive + 96, 256), "survivors-enemy-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.MinibossEnemySpawnableId, new GameObjectPrefabProvider(_enemyPrefab), 1, 8, "survivors-miniboss-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.BossEnemySpawnableId, new GameObjectPrefabProvider(_enemyPrefab), 1, 4, "survivors-boss-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.ExperiencePickupSpawnableId, new GameObjectPrefabProvider(_experiencePickupPrefab), 16, 256, "survivors-xp-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.MagnetPickupSpawnableId, new GameObjectPrefabProvider(_magnetPickupPrefab), 2, 16, "survivors-magnet-pool"),
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

        private void BuildArenaVisuals()
        {
            CreateArenaPrimitive("Survivors Arena Ring", PrimitiveType.Cylinder, Vector3.zero, new Vector3(17.5f, 0.035f, 17.5f), new Color(0.07f, 0.09f, 0.12f));
            CreateArenaPrimitive("Inner XP Magnet Ring", PrimitiveType.Cylinder, Vector3.zero, new Vector3(6.4f, 0.04f, 6.4f), new Color(0.09f, 0.16f, 0.2f));
            CreateArenaPrimitive("Player Safe Readability Ring", PrimitiveType.Cylinder, Vector3.zero, new Vector3(2.1f, 0.05f, 2.1f), new Color(0.12f, 0.19f, 0.28f));

            CreateArenaPrimitive("North Spawn Warning", PrimitiveType.Cube, new Vector3(0f, 0.09f, 8.8f), new Vector3(4.4f, 0.08f, 0.28f), new Color(0.62f, 0.12f, 0.18f));
            CreateArenaPrimitive("East Spawn Warning", PrimitiveType.Cube, new Vector3(8.8f, 0.09f, 0f), new Vector3(0.28f, 0.08f, 4.4f), new Color(0.62f, 0.12f, 0.18f));
            CreateArenaPrimitive("South Spawn Warning", PrimitiveType.Cube, new Vector3(0f, 0.09f, -8.8f), new Vector3(4.4f, 0.08f, 0.28f), new Color(0.62f, 0.12f, 0.18f));
            CreateArenaPrimitive("West Spawn Warning", PrimitiveType.Cube, new Vector3(-8.8f, 0.09f, 0f), new Vector3(0.28f, 0.08f, 4.4f), new Color(0.62f, 0.12f, 0.18f));

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 5.4f, 0.12f, Mathf.Sin(angle) * 5.4f);
                Vector3 scale = i % 2 == 0 ? new Vector3(0.9f, 0.18f, 0.32f) : new Vector3(0.32f, 0.18f, 0.9f);
                CreateArenaPrimitive("Rune Lane Marker " + (i + 1).ToString(), PrimitiveType.Cube, position, scale, new Color(0.18f, 0.2f, 0.32f));
            }
        }

        private GameObject CreateArenaPrimitive(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            GameObject instance = GameObject.CreatePrimitive(primitive);
            instance.name = name;
            instance.transform.SetParent(_worldRoot, false);
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
            _pickups.Clear();
            _projectiles.Clear();
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

        private void ApplyPersistentMetaBonuses()
        {
            EnsureMetaProgressionLoaded();
            float previousBonus = PersistentDamageBonus;
            PersistentDamageBonus = _metaProgression.GetPersistentDamageBonus(BasicSurvivorsGame.WeaponTarget.Value);
            DamageBonus += PersistentDamageBonus - previousBonus;
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

        private void GrantBossReward(SurvivorsEnemyRole role)
        {
            string rewardId = role == SurvivorsEnemyRole.Boss ? BasicSurvivorsGame.BossRewardId : BasicSurvivorsGame.MinibossRewardId;
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
            }
        }

        private void TickRunFlow()
        {
            if (_runFlow == null)
            {
                return;
            }

            _runFlow.Tick(RunTimeSeconds);
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
            State = SurvivorsRunState.Victory;
            PlayFeedback(_levelUpPulse, PlayerPosition, 42, _levelUpClip);
        }

        private float ResolveEnemySpawnIntervalSeconds()
        {
            return _runFlow == null
                ? Mathf.Max(0.05f, CurrentTuning.EnemySpawnIntervalSeconds)
                : _runFlow.ResolveSpawnInterval(CurrentTuning.EnemySpawnIntervalSeconds);
        }

        private int ResolveEnemyMaximumAlive()
        {
            return _runFlow == null
                ? Mathf.Max(1, CurrentTuning.EnemyMaximumAlive)
                : _runFlow.ResolveMaximumAlive(CurrentTuning.EnemyMaximumAlive);
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

            if (role == SurvivorsEnemyRole.Elite)
            {
                return "group.survivors.elites";
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
            if (_enemySpawnTimer > 0f || _enemies.Count >= ResolveEnemyMaximumAlive())
            {
                return;
            }

            SurvivorsEnemyRole role = _runFlow == null
                ? SurvivorsEnemyRole.Swarm
                : _runFlow.ResolveNextSwarmRole(RunTimeSeconds, _spawnSequence + 1);
            SpawnEnemy(Vector3.zero, explicitPosition: false, role);
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
            else
            {
                PlayFeedback(_spawnPulse, enemy.transform.position, 10, _spawnClip);
            }

            return enemy;
        }

        private SurvivorsPickupActor SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount)
        {
            long sequence = ++_spawnSequence;
            WorldSpawnableId spawnable = kind == SurvivorsPickupKind.Magnet
                ? BasicSurvivorsGame.MagnetPickupSpawnableId
                : BasicSurvivorsGame.ExperiencePickupSpawnableId;
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
            pickup.Initialize(this, kind, Mathf.Max(1, amount), CurrentTuning.PickupAttractRange + PickupRangeBonus, CurrentTuning.PickupAttractionSpeed, CurrentTuning.PickupCollectRadius);
            _pickups.Add(pickup);
            return pickup;
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

                pickup.Simulate(deltaTime);
            }
        }

        private void ClearRewardDrafts()
        {
            _currentDraft = null;
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
        }

        private void GainExperience(int amount)
        {
            ExperienceCollected += Mathf.Max(1, amount);
            Experience += Mathf.Max(1, amount);
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
        }

        private void OpenLevelUpDraft(IReadOnlyList<RunUpgradeId> lockedChoices = null)
        {
            if (_rewardSelectionKind == SurvivorsRewardSelectionKind.BossRelic)
            {
                return;
            }

            _currentDraft = RunUpgradeDraftService.Generate(
                _upgradeCatalog,
                _upgradeState,
                new RunUpgradeDraftRequest(CurrentTuning.DraftChoiceCount, CurrentTuning.RunSeed + Level + SelectedUpgradeCount, lockedChoices: lockedChoices));
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.LevelUp;
            State = SurvivorsRunState.LevelUp;
            BeginRewardSelectionTimeout();
            PlayFeedback(_levelUpPulse, PlayerPosition, 34, _levelUpClip);
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
            _currentRelicDraft = SurvivorsRelicDraftService.Generate(
                _relicDefinitions,
                CurrentTuning.DraftChoiceCount,
                CurrentTuning.RunSeed + MinibossKilledCount + SelectedRelicCount + 97);
            if (_currentRelicDraft == null || _currentRelicDraft.Choices.Count == 0)
            {
                _currentRelicDraft = null;
                return false;
            }

            _currentDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.BossRelic;
            BossRelicDraftOpenCount++;
            State = SurvivorsRunState.LevelUp;
            BeginRewardSelectionTimeout();
            PlayFeedback(_bossPulse, PlayerPosition, 44, _bossClip);
            return true;
        }

        private void BeginRewardSelectionTimeout()
        {
            float timeout = CurrentTuning.RewardSelectionTimeoutSeconds;
            _rewardSelectionTimer = timeout > 0f ? timeout : 0f;
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
            else if (_rewardSelectionKind == SurvivorsRewardSelectionKind.LevelUp && _currentDraft != null && _currentDraft.Choices.Count > 0)
            {
                RewardAutoSelectCount++;
                SelectUpgrade(0);
            }
        }

        private bool SelectRelic(int index)
        {
            if (State != SurvivorsRunState.LevelUp || _rewardSelectionKind != SurvivorsRewardSelectionKind.BossRelic || _currentRelicDraft == null || index < 0 || index >= _currentRelicDraft.Choices.Count)
            {
                return false;
            }

            ApplyRelic(_currentRelicDraft.Choices[index]);
            SelectedRelicCount++;
            _currentRelicDraft = null;
            _rewardSelectionKind = SurvivorsRewardSelectionKind.None;
            _rewardSelectionTimer = 0f;
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
            }
        }

        private void DrawLevelUpOverlay()
        {
            float width = 420f;
            int choiceCount = IsRelicChoiceOpen ? CurrentRelicChoices.Count : CurrentDraftChoices.Count;
            float height = 112f + choiceCount * 48f;
            Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
            GUI.Box(rect, IsRelicChoiceOpen ? "Choose a Boss Relic" : "Level Up");
            GUI.Label(
                new Rect(rect.x + 24f, rect.y + 28f, width - 48f, 18f),
                RewardSelectionRemainingSeconds > 0f ? $"Auto-pick in {RewardSelectionRemainingSeconds:0}s" : "Choose a reward",
                _hudSmallStyle);
            for (int i = 0; i < choiceCount; i++)
            {
                Rect buttonRect = new Rect(rect.x + 24f, rect.y + 68f + i * 48f, width - 48f, 34f);
                string label;
                if (IsRelicChoiceOpen)
                {
                    SurvivorsRelicDefinition relic = CurrentRelicChoices[i];
                    label = $"{i + 1}. {relic.DisplayName}";
                }
                else
                {
                    RunUpgradeDefinition choice = CurrentDraftChoices[i];
                    label = $"{i + 1}. {BasicSurvivorsGame.GetUpgradeDisplayName(choice.Id)} ({choice.Rarity})";
                }

                if (GUI.Button(buttonRect, label))
                {
                    SelectUpgrade(i);
                }
            }
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

        private static string ShortWeaponName(string weaponId)
        {
            if (weaponId == BasicSurvivorsGame.ArcaneWandWeaponContentId) return "Wand";
            if (weaponId == BasicSurvivorsGame.OrbitWardWeaponContentId) return "Orbit";
            if (weaponId == BasicSurvivorsGame.MoonSlashWeaponContentId) return "Slash";
            if (weaponId == BasicSurvivorsGame.StarNovaWeaponContentId) return "Nova";
            if (weaponId == BasicSurvivorsGame.StarBeamWeaponContentId) return "Beam";
            if (weaponId == BasicSurvivorsGame.GravityGrenadeWeaponContentId) return "Grenade";
            if (weaponId == BasicSurvivorsGame.RuneTrapWeaponContentId) return "Trap";
            if (weaponId == BasicSurvivorsGame.AetherMineWeaponContentId) return "Mine";
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

        private void HandleLevelUpInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
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

        private static void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            renderer.sharedMaterial = new Material(shader);
            renderer.sharedMaterial.color = color;
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
        private float _preferredRange;
        private float _rangedAttackCooldown;
        private float _poisonDamagePerSecond;
        private float _poisonRemainingSeconds;
        private float _bleedDamagePerSecond;
        private float _bleedRemainingSeconds;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsAlive => _health != null && _health.IsAlive;
        public SurvivorsEnemyRole Role { get; private set; }
        public string ProfileId { get; private set; }
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
            _moveSpeed = Mathf.Max(0f, profile.MoveSpeed);
            Radius = Mathf.Max(0.05f, profile.Radius);
            _contactDamage = Mathf.Max(0f, profile.ContactDamage);
            _contactInterval = Mathf.Max(0.05f, profile.ContactIntervalSeconds);
            _contactCooldown = 0f;
            _rangedAttackRange = Mathf.Max(0f, profile.RangedAttackRange);
            _rangedAttackDamage = Mathf.Max(0f, profile.RangedAttackDamage);
            _rangedAttackInterval = Mathf.Max(0.05f, profile.RangedAttackIntervalSeconds);
            _preferredRange = Mathf.Max(0f, profile.PreferredRange);
            _rangedAttackCooldown = Mathf.Min(0.75f, _rangedAttackInterval);
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            ExperienceReward = Mathf.Max(1, profile.ExperienceReward);
            string id = InstanceId.Value > 0 ? "combatant.survivors.enemy." + InstanceId.Value : "combatant.survivors.enemy.pending";
            float maxHealth = Mathf.Max(1f, profile.MaxHealth);
            _health = new HealthState(new CombatantId(id), maxHealth, maxHealth);
            transform.localScale = Vector3.one * (Radius * 2f);
            ApplyTint(profile.Tint);
        }

        public void Simulate(float deltaTime)
        {
            if (!IsAlive || _controller == null || !_controller.IsPlaying)
            {
                return;
            }

            TickDamageOverTime(deltaTime);
            if (!IsAlive)
            {
                return;
            }

            Vector3 direction = _controller.PlayerPosition - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance > 0.001f)
            {
                Vector3 normalized = direction / distance;
                Vector3 moveDirection = ResolveMoveDirection(normalized, distance);
                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    transform.position += moveDirection.normalized * (_moveSpeed * deltaTime);
                    transform.forward = moveDirection.normalized;
                }
                else
                {
                    transform.forward = normalized;
                }
            }

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

            DamageRequest request = new DamageRequest(
                _health.Id,
                new[] { new DamageComponent(BasicSurvivorsGame.ArcaneDamageType, amount) },
                sourceId: new CombatantId(string.IsNullOrWhiteSpace(source) ? "combatant.survivors.player" : source),
                preResolvedCritical: false);
            DamageResolutionResult result = CombatDamageResolver.Resolve(_controller.CombatCatalog, _health, null, request);
            if (applyAugments && result != null && result.Damage != null && IsAlive)
            {
                _controller.ApplyDamageAugmentsToEnemy(this, result.Damage, source);
            }

            if (_health != null && !_health.IsAlive)
            {
                _controller.HandleEnemyKilled(this);
            }

            return result.Damage;
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

        private void TickRangedAttack(float deltaTime, float distance)
        {
            if (_controller == null || _rangedAttackRange <= 0f || _rangedAttackDamage <= 0f)
            {
                return;
            }

            _rangedAttackCooldown -= deltaTime;
            if (_rangedAttackCooldown > 0f || distance > _rangedAttackRange)
            {
                return;
            }

            _controller.ApplyDamageToPlayer(_rangedAttackDamage, "combatant.survivors.enemy.ranged." + InstanceId.Value);
            _rangedAttackCooldown = _rangedAttackInterval;
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
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            _health = null;
            _contactCooldown = 0f;
            _rangedAttackRange = 0f;
            _rangedAttackDamage = 0f;
            _rangedAttackInterval = 0f;
            _preferredRange = 0f;
            _rangedAttackCooldown = 0f;
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            Role = SurvivorsEnemyRole.Swarm;
            ProfileId = null;
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
            renderer.material = new Material(shader) { color = tint };
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

            transform.position += _direction * (_speed * deltaTime);
            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
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

                float hitRange = _radius + enemy.Radius;
                if ((enemy.transform.position - transform.position).sqrMagnitude <= hitRange * hitRange)
                {
                    _hitEnemyIds.Add(enemyId);
                    enemy.ApplyDamage(_damage, _definition == null ? "combatant.survivors.player" : _definition.Id);
                    SpawnForkProjectiles(enemy);
                    if (_remainingPierces > 0)
                    {
                        _remainingPierces--;
                        _controller.RecordProjectilePierceHit();
                        transform.position += _direction * Mathf.Max(0.08f, hitRange);
                        return;
                    }

                    if (_remainingChains > 0 && TryRetargetFrom(enemy))
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
        private float _recallSpeedMultiplier;
        private float _currentSpeed;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsActive { get; private set; }
        public SurvivorsPickupKind Kind { get; private set; }
        public int Amount { get; private set; }

        public void Initialize(SurvivorsTemplateController controller, SurvivorsPickupKind kind, int amount, float attractRange, float attractionSpeed, float collectRadius)
        {
            _controller = controller;
            Kind = kind;
            Amount = Mathf.Max(1, amount);
            _attractRange = Mathf.Max(0.1f, attractRange);
            _attractionSpeed = Mathf.Max(0.1f, attractionSpeed);
            _collectRadius = Mathf.Max(0.1f, collectRadius);
            _globalRecall = false;
            _recallSpeedMultiplier = 1f;
            _currentSpeed = 0f;
            IsActive = true;
            transform.localScale = Vector3.one * (kind == SurvivorsPickupKind.Magnet ? 0.58f : 0.34f);
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
            bool shouldAttract = _globalRecall || distance <= _attractRange;
            if (shouldAttract && distance > 0.001f)
            {
                float targetSpeed = _attractionSpeed * (_globalRecall ? _recallSpeedMultiplier * Mathf.Clamp(1f + distance * 0.18f, 1f, 10f) : 1f);
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, targetSpeed * 8f * deltaTime);
                float travel = Mathf.Min(distance, _currentSpeed * deltaTime);
                transform.position += offset.normalized * travel;
                distance = Vector3.Distance(transform.position, playerPosition);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _attractionSpeed * 5f * deltaTime);
            }

            if (_globalRecall)
            {
                transform.Rotate(0f, 420f * deltaTime, 0f, Space.Self);
            }

            if (distance <= _collectRadius)
            {
                IsActive = false;
                _controller.CollectPickup(this);
            }
        }

        public void OnWorldSpawned(WorldSpawnContext context)
        {
            InstanceId = context.InstanceId;
        }

        public void OnWorldDespawned(DespawnReason reason)
        {
            _controller = null;
            IsActive = false;
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            IsActive = false;
            InstanceId = default;
            _globalRecall = false;
            _currentSpeed = 0f;
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
