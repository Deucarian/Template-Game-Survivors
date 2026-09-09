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
    // Diagnostic read models and explicit test scenario bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private void DrawDebugOverlay() => SurvivorsDebugHudPresenter.Draw(
            Screen.height, CaptureDebugHudValues(), _hudStyles);

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

        private void AppendSelectedUpgradeRankLines(List<string> lines) => DebugUpgradeFormatter.AppendSelectedUpgradeRankLines(lines);

        private string FormatDebugRankLine(RunUpgradeDefinition definition, int rank) => DebugUpgradeFormatter.FormatDebugRankLine(definition, rank);

        private string FormatDebugUpgradeLine(int index, RunUpgradeDefinition definition) => DebugUpgradeFormatter.FormatDebugUpgradeLine(index, definition);

        private string FormatUpgradeChoiceLabel(int index, RunUpgradeDefinition choice) => DebugUpgradeFormatter.FormatUpgradeChoiceLabel(index, choice);

        private string FormatRelicChoiceLabel(int index, SurvivorsRelicDefinition relic) => DebugUpgradeFormatter.FormatRelicChoiceLabel(index, relic);

        public IReadOnlyList<string> DebugDescribeEligibleEvolutionPool() { EnsureRunStartedForTest(); return DebugDraftModel.DebugDescribeEligibleEvolutionPool(); }

        public IReadOnlyList<string> DebugDescribeCurrentDraftPool() { EnsureRunStartedForTest(); return DebugDraftModel.DebugDescribeCurrentDraftPool(); }

        private SurvivorsDebugUpgradeFormatter DebugUpgradeFormatter => _debugUpgradeFormatter ?? (_debugUpgradeFormatter = new SurvivorsDebugUpgradeFormatter(RunBuild, BuildContentLabels, DraftCards));

        private SurvivorsDebugDraftModel DebugDraftModel => _debugDraftModel ?? (_debugDraftModel = new SurvivorsDebugDraftModel(RunBuild, DraftOffers.Catalogs, DebugUpgradeFormatter, BuildContentLabels, this));

        RunUpgradeDraft ISurvivorsDebugDraftPort.CurrentDraft => DraftSession.CurrentDraft;

        SurvivorsRelicDraft ISurvivorsDebugDraftPort.CurrentRelicDraft => DraftSession.CurrentRelicDraft;

        string ISurvivorsDebugDraftPort.RewardOverlayTitle => ResolveRewardOverlayTitle();

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

        public int LevelAtOneMinute => Telemetry.LevelAtOneMinute;

        public int LevelAtTwoMinutes => Telemetry.LevelAtTwoMinutes;

        public int LevelAtThreeMinutes => Telemetry.LevelAtThreeMinutes;

        public int LevelAtFourMinutes => Telemetry.LevelAtFourMinutes;

        public int LevelAtFiveMinutes => Telemetry.LevelAtFiveMinutes;

        public bool IsDebugFastPacing => CurrentPacingProfile == SurvivorsPacingProfile.DebugFast;

        public bool IsDebugOverlayVisible => _debugOverlayVisible;

        public float FirstKillTimeSeconds => Telemetry.FirstKillTimeSeconds;

        public float FirstBossKillTimeSeconds => Telemetry.FirstBossKillTimeSeconds;

        public void ToggleDebugOverlayForTest()
        {
            _debugOverlayVisible = !_debugOverlayVisible;
        }

        private void ResetRunMetrics()
        {
            DraftSession.ResetOpenCount();
            Telemetry.Reset();
            PlayerVitals.ResetDamageTaken();
            _runMetricsReadModel?.Clear();
        }
    }
}
