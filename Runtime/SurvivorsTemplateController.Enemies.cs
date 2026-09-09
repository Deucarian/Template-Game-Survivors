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
    // Authoritative actor membership, spawn commands, safety and spatial query bindings.
    public sealed partial class SurvivorsTemplateController
    {
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

        private SurvivorsEnemySpawner EnemySpawner => _enemySpawner ?? (_enemySpawner = new SurvivorsEnemySpawner(this, SpawnSequence));

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

        private static bool IsMajorRewardRole(SurvivorsEnemyRole role) => SurvivorsEnemyRosterQueries.IsMajorRewardRole(role);

        private static SurvivorsEnemyRole ResolveDebugMajorEnemyRole(SurvivorsEnemyRole role) => SurvivorsEnemyRosterQueries.ResolveDebugMajorEnemyRole(role);

        private SurvivorsSpawnSafety SpawnSafety => _spawnSafety ?? (_spawnSafety = new SurvivorsSpawnSafety(this));

        private SurvivorsEnemyRosterQueries EnemyRosterQueries => _enemyRosterQueries ?? (_enemyRosterQueries = new SurvivorsEnemyRosterQueries(_enemies));

        SurvivorsTemplateTuning ISurvivorsSpawnSafetyPort.Tuning => CurrentTuning;

        Vector3 ISurvivorsSpawnSafetyPort.PlayerPosition => PlayerPosition;

        bool ISurvivorsSpawnSafetyPort.TryResolveCameraGroundRect(float padding, out Rect rect) => TryResolveCameraGroundRect(padding, out rect);

        internal void ReleaseEnemy(SurvivorsEnemyActor enemy, DespawnReason reason) => ActorMembership.ReleaseEnemy(enemy, reason);

        internal void ReleaseProjectile(SurvivorsProjectileActor projectile, DespawnReason reason) => ActorMembership.ReleaseProjectile(projectile, reason);

        private SurvivorsActorMembership ActorMembership => _actorMembership ?? (_actorMembership = new SurvivorsActorMembership(this));

        bool ISurvivorsActorMembershipPort.RemoveHordeMember(long id) => HordeRush.RemoveEnemy(id);

        bool ISurvivorsActorMembershipPort.RemoveCacheMember(long id) => RoamingCaches.RemoveEnemy(id);

        bool ISurvivorsActorMembershipPort.RemoveShrineMember(long id) => ShrineTrials.RemoveEnemy(id);

        void ISurvivorsActorMembershipPort.ForgetMajorThreat(SurvivorsEnemyActor enemy) => MajorThreatAbilities.ForgetEnemy(enemy);

        bool ISurvivorsActorMembershipPort.HasSpawnService => _spawnService != null;

        void ISurvivorsActorMembershipPort.Despawn(SpawnInstanceId id, DespawnReason reason) => _spawnService.Despawn(id, reason);

        private List<SurvivorsEnemyActor> _enemies => ActorMembership.Enemies;

        private List<SurvivorsPickupActor> _pickups => ActorMembership.Pickups;

        private List<SurvivorsProjectileActor> _projectiles => ActorMembership.Projectiles;

        private SurvivorsEnemySpatialQueries EnemySpatialQueries => _enemySpatialQueries ?? (_enemySpatialQueries = new SurvivorsEnemySpatialQueries(_enemies, () => CurrentTuning));

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

        private SurvivorsSwarmSpawnCoordinator SwarmSpawning => _swarmSpawning ??
            (_swarmSpawning = new SurvivorsSwarmSpawnCoordinator(this));

        long ISurvivorsSwarmSpawnPort.SpawnSequence => _spawnSequence;

        bool ISurvivorsSwarmSpawnPort.TrySpawn(SurvivorsEnemyRole role) =>
            SpawnEnemy(Vector3.zero, explicitPosition: false, role, gameplaySpawn: true, spawnSource: "normal-pack") != null;

        private long _spawnSequence => SpawnSequence.Current;

        public int SpawnedCount => EnemySpawner.SpawnedCount;

        public int MinibossSpawnCount => EnemySpawner.MinibossSpawnCount;

        public int BossSpawnCount => EnemySpawner.BossSpawnCount;

        public int ActiveEnemyCount => _enemies.Count;

        public int ActiveRunnerCount => CountEnemiesByRole(SurvivorsEnemyRole.Runner);

        public int ActiveBruiserCount => CountEnemiesByRole(SurvivorsEnemyRole.Bruiser);

        public int ActiveSpitterCount => CountEnemiesByRole(SurvivorsEnemyRole.Spitter);

        public int ActiveBossCount => CountEnemiesByRole(SurvivorsEnemyRole.Boss);

        public int ActiveBossLifeBarCount => CountAuthoredThreatLifeBars(showBossLifeBar: true);

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

        public float CurrentEnemySpawnIntervalSeconds => ResolveEnemySpawnIntervalSeconds();

        public int CurrentEnemySpawnPackSize => ResolveEnemySpawnPackSize();

        public int CurrentEnemyMaximumAlive => ResolveEnemyMaximumAlive();

        public SurvivorsEnemyActor DebugSpawnBoss(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.Boss, radius);
        }

        internal IReadOnlyList<SurvivorsEnemyActor> ActiveEnemies => _enemies;

        private float ResolveEnemySpawnIntervalSeconds() =>
            SurvivorsSwarmSpawnCoordinator.ResolveInterval(CurrentTuning, _runFlow, _runSession.HasClearedVictory);

        private int ResolveEnemyMaximumAlive() =>
            SurvivorsSwarmSpawnCoordinator.ResolveMaximumAlive(CurrentTuning, _runFlow, _runSession.HasClearedVictory);

        private int ResolveEnemySpawnPackSize() => SurvivorsSwarmSpawnCoordinator.ResolvePackSize(CurrentTuning, _runFlow);

        private void TickEnemySpawning(float deltaTime)
        {
            SwarmSpawning.Tick(deltaTime, RunTimeSeconds, CurrentTuning, _runFlow, _runSession.HasClearedVictory);
        }
    }
}
