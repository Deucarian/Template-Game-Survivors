using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal enum SurvivorsDebugEncounter { Horde, RoamingCache, Shrine }
    internal interface ISurvivorsDebugWorldPort
    {
        void EnsureRunStarted();
        bool Started { get; }
        SurvivorsPacingProfile PacingProfile { get; }
        void ApplyPacing(SurvivorsPacingProfile profile, bool restart);
        void StartRun();
        SurvivorsTemplateTuning Tuning { get; }
        Vector3 PlayerPosition { get; }
        Vector3 PlayerForward { get; }
        int ActiveEnemyCount { get; }
        SurvivorsEnemyActor SpawnEnemy(Vector3 position, SurvivorsEnemyRole role);
        int ActiveMembers(SurvivorsDebugEncounter encounter);
        IEnumerable<long> Members(SurvivorsDebugEncounter encounter);
        bool DamageMember(long id, float damage, string source);
    }

    /// <summary>Owns bounded debug world scenarios; actual actor and run commands stay behind the scene adapter.</summary>
    internal sealed class SurvivorsDebugWorldCommands
    {
        private readonly ISurvivorsDebugWorldPort _port;
        public SurvivorsDebugWorldCommands(ISurvivorsDebugWorldPort port) => _port = port;
        public SurvivorsEnemyActor SpawnWithHealth(Vector3 position, SurvivorsEnemyRole role, float healthOverride)
        {
            _port.EnsureRunStarted();
            SurvivorsEnemyActor enemy = _port.SpawnEnemy(position, role);
            if (enemy != null && healthOverride > 0f) enemy.OverrideHealthForTest(healthOverride);
            return enemy;
        }

        public int ClearEncounter(SurvivorsDebugEncounter encounter)
        {
            _port.EnsureRunStarted();
            if (_port.ActiveMembers(encounter) == 0) return 0;
            var ids = new List<long>(_port.Members(encounter));
            string source = encounter == SurvivorsDebugEncounter.Horde ? "test.horde-rush-clear" :
                encounter == SurvivorsDebugEncounter.RoamingCache ? "test.roaming-cache-ambush-clear" : "test.arena-shrine-clear";
            int killed = 0;
            for (int i = 0; i < ids.Count; i++)
                if (_port.DamageMember(ids[i], 10000f, source)) killed++;
            return killed;
        }

        public int SpawnBurst(SurvivorsEnemyRole role, int count, float radius)
        {
            _port.EnsureRunStarted();
            int spawned = 0;
            int resolvedCount = Mathf.Clamp(count, 1, 256);
            float resolvedRadius = Mathf.Max(0.5f, radius);
            for (int index = 0; index < resolvedCount; index++)
            {
                float angle = (index / (float)resolvedCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * resolvedRadius;
                if (_port.SpawnEnemy(_port.PlayerPosition + offset, role) != null) spawned++;
            }
            return spawned;
        }

        public SurvivorsEnemyActor SpawnMajor(SurvivorsEnemyRole role, float radius)
        {
            _port.EnsureRunStarted();
            SurvivorsEnemyRole resolvedRole = SurvivorsEnemyRosterQueries.ResolveDebugMajorEnemyRole(role);
            Vector3 forward = _port.PlayerForward;
            if (forward.sqrMagnitude <= 0.001f) forward = Vector3.forward;
            float resolvedRadius = Mathf.Clamp(radius, 2f, 40f);
            return _port.SpawnEnemy(_port.PlayerPosition + forward.normalized * resolvedRadius, resolvedRole);
        }

        public SurvivorsEnemyActor SpawnSprintBoss(float radius)
        {
            if (_port.PacingProfile != SurvivorsPacingProfile.SprintRun)
                _port.ApplyPacing(SurvivorsPacingProfile.SprintRun, _port.Started);
            if (!_port.Started) _port.StartRun();
            return SpawnMajor(SurvivorsEnemyRole.Boss, radius);
        }

        public int FillArena(SurvivorsEnemyRole role, int targetAlive, float radius)
        {
            _port.EnsureRunStarted();
            int target = Mathf.Clamp(targetAlive, 1, 512);
            int needed = Mathf.Max(0, target - _port.ActiveEnemyCount);
            return needed <= 0 ? 0 : SpawnBurst(role, needed, radius);
        }

        public void ApplyStress(int targetAlive)
        {
            int target = Mathf.Clamp(targetAlive, 50, 512);
            _port.Tuning.EnemyMaximumAlive = target;
            _port.Tuning.EnemySpawnIntervalSeconds = Mathf.Min(_port.Tuning.EnemySpawnIntervalSeconds, target >= 250 ? 0.18f : 0.28f);
            FillArena(SurvivorsEnemyRole.Swarm, Mathf.Min(target, 160), _port.Tuning.EnemySpawnRadius);
        }
    }
}
