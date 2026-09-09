using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Scene boundary needed by the swarm cadence policy.</summary>
    internal interface ISurvivorsSwarmSpawnPort
    {
        int ActiveEnemyCount { get; }
        long SpawnSequence { get; }
        bool TrySpawn(SurvivorsEnemyRole role);
    }

    /// <summary>Owns regular-pack cadence, capacity and escalation policy; World Spawning owns instances.</summary>
    internal sealed class SurvivorsSwarmSpawnCoordinator
    {
        private readonly ISurvivorsSwarmSpawnPort _spawning;
        private float _remaining;

        public SurvivorsSwarmSpawnCoordinator(ISurvivorsSwarmSpawnPort spawning)
        {
            _spawning = spawning ?? throw new ArgumentNullException(nameof(spawning));
        }

        public void Reset() => _remaining = 0f;

        public void Tick(float deltaTime, float runTime, SurvivorsTemplateTuning tuning, SurvivorsRunFlowRuntime flow, bool clearedVictory)
        {
            _remaining -= deltaTime;
            int maximum = ResolveMaximumAlive(tuning, flow, clearedVictory);
            if (_remaining > 0f || _spawning.ActiveEnemyCount >= maximum) return;
            int packCount = Mathf.Min(ResolvePackSize(tuning, flow), maximum - _spawning.ActiveEnemyCount);
            for (int i = 0; i < packCount; i++)
            {
                // Read the port sequence after each spawn, preserving the original role-seed progression.
                SurvivorsEnemyRole role = flow == null
                    ? SurvivorsEnemyRole.Swarm
                    : flow.ResolveNextSwarmRole(runTime, _spawning.SpawnSequence + 1 + i);
                if (!_spawning.TrySpawn(role)) break;
            }

            _remaining = ResolveInterval(tuning, flow, clearedVictory);
        }

        public static float ResolveInterval(SurvivorsTemplateTuning tuning, SurvivorsRunFlowRuntime flow, bool clearedVictory)
        {
            float interval = flow == null
                ? Mathf.Max(0.05f, tuning.EnemySpawnIntervalSeconds)
                : flow.ResolveSpawnInterval(tuning.EnemySpawnIntervalSeconds);
            float resolved = Mathf.Max(0.05f, interval);
            if (!clearedVictory) return resolved;
            float minimum = flow == null || flow.Definition == null ? 0.05f : flow.Definition.MinimumEnemySpawnIntervalSeconds;
            return Mathf.Max(minimum, resolved * 0.82f);
        }

        public static int ResolveMaximumAlive(SurvivorsTemplateTuning tuning, SurvivorsRunFlowRuntime flow, bool clearedVictory)
        {
            int maximum = flow == null ? Mathf.Max(1, tuning.EnemyMaximumAlive) : flow.ResolveMaximumAlive(tuning.EnemyMaximumAlive);
            return clearedVictory ? maximum + 24 : maximum;
        }

        public static int ResolvePackSize(SurvivorsTemplateTuning tuning, SurvivorsRunFlowRuntime flow)
        {
            int baseCount = Mathf.Max(1, tuning.EnemySpawnPackBaseCount);
            int maxCount = Mathf.Max(baseCount, tuning.EnemySpawnPackMaxCount);
            int step = Mathf.Max(1, tuning.EnemySpawnPackIncreaseEveryEscalations);
            int bonus = Mathf.Max(0, flow == null ? 0 : flow.EscalationLevel) / step;
            return Mathf.Clamp(baseCount + bonus, 1, maxCount);
        }
    }
}
