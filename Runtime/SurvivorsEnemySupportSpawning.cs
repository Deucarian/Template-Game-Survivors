using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Coordinates deterministic support spawns through the existing safe offscreen adapter.</summary>
    internal sealed class SurvivorsEnemySupportSpawning
    {
        private readonly ISurvivorsEnemySupportSpawnPort _port;
        internal int SplitterChildSpawnCount { get; private set; }
        internal int SplitterSplitFeedbackCount { get; private set; }
        internal string LastSplitterSplitFeedbackLabel { get; private set; } = string.Empty;
        internal int SummonerSupportSpawnCount { get; private set; }
        internal int SummonerSupportFeedbackCount { get; private set; }
        internal string LastSummonerSupportFeedbackLabel { get; private set; } = string.Empty;

        internal SurvivorsEnemySupportSpawning(ISurvivorsEnemySupportSpawnPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal void ResetDiagnostics()
        {
            SplitterChildSpawnCount = 0;
            SplitterSplitFeedbackCount = 0;
            LastSplitterSplitFeedbackLabel = string.Empty;
            SummonerSupportSpawnCount = 0;
            SummonerSupportFeedbackCount = 0;
            LastSummonerSupportFeedbackLabel = string.Empty;
        }

        internal int SpawnMajorThreatEnrageSupport(SurvivorsEnemyActor enemy, int enrageCount)
        {
            int requested = ResolveMajorThreatEnrageSupportCount(enemy.Role);
            if (requested <= 0)
            {
                return 0;
            }

            int available = Mathf.Max(
                0,
                _port.MaximumAlive + Mathf.Max(0, _port.Tuning.MajorThreatEnrageExtraAliveAllowance) - _port.EnemyCount);
            int targetCount = Mathf.Min(requested, available);
            if (targetCount <= 0)
            {
                return 0;
            }

            Vector3 center = enemy.transform.position;
            float radius = Mathf.Max(enemy.Radius + 1.35f, _port.Tuning.MajorThreatEnrageSupportRadius);
            int spawned = 0;
            for (int i = 0; i < targetCount; i++)
            {
                float laneRadius = radius + ((i & 1) == 0 ? 0f : 1.15f);
                SurvivorsEnemyRole supportRole = ResolveMajorThreatEnrageSupportRole(enemy.Role, i);
                if (_port.SpawnGameplayEnemyOffscreen(
                    supportRole,
                    _port.SpawnSequence + i + enrageCount * 31 + 701,
                    radius,
                    radius + _port.Tuning.SpawnBandDepth,
                    "major-threat-enrage") != null)
                {
                    spawned++;
                }
            }

            return spawned;
        }

        internal int ResolveMajorThreatEnrageSupportCount(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return Mathf.Max(0, _port.Tuning.MajorThreatEnrageBossSupportCount);
                case SurvivorsEnemyRole.Miniboss:
                    return Mathf.Max(0, _port.Tuning.MajorThreatEnrageMinibossSupportCount);
                case SurvivorsEnemyRole.DreadElite:
                    return Mathf.Max(0, _port.Tuning.MajorThreatEnrageEliteSupportCount + 2);
                default:
                    return Mathf.Max(0, _port.Tuning.MajorThreatEnrageEliteSupportCount);
            }
        }

        internal static SurvivorsEnemyRole ResolveMajorThreatEnrageSupportRole(SurvivorsEnemyRole majorRole, int index)
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

        internal void SpawnSplitterChildren(Vector3 position, string splitterName)
        {
            if (_port.State == SurvivorsRunState.GameOver || _port.State == SurvivorsRunState.Victory)
            {
                return;
            }

            int childCount = Mathf.Clamp(_port.Tuning.SplitterChildCount, 1, 8);
            float radius = _port.Tuning.SplitterChildSpawnRadius > 0f
                ? _port.Tuning.SplitterChildSpawnRadius
                : Mathf.Max(0.65f, _port.Tuning.EnemyRadius * 1.5f);
            int spawned = 0;
            for (int index = 0; index < childCount; index++)
            {
                if (_port.SpawnGameplayEnemyOffscreen(
                    SurvivorsEnemyRole.Swarm,
                    _port.SpawnSequence + index + SplitterChildSpawnCount * 53 + 991,
                    radius,
                    radius + _port.Tuning.SpawnBandDepth,
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

        internal void RecordSplitterSplitFeedback(string splitterName, Vector3 position, int spawned)
        {
            string name = string.IsNullOrWhiteSpace(splitterName) ? "Splitter" : splitterName;
            LastSplitterSplitFeedbackLabel = $"{name}: +{spawned} fragments";
            SplitterSplitFeedbackCount++;
            _port.RecordStreakRewardFeedback(LastSplitterSplitFeedbackLabel, new Color(0.78f, 0.58f, 1f));
            _port.PlaySupportSpawnFeedback(position, Mathf.Clamp(16 + spawned * 5, 24, 64));
        }

        internal int SpawnSummonerSupport(SurvivorsEnemyActor summoner)
        {
            if (_port.State == SurvivorsRunState.GameOver || _port.State == SurvivorsRunState.Victory || summoner == null || !summoner.IsAlive)
            {
                return 0;
            }

            int requested = Mathf.Max(0, _port.Tuning.SummonerSupportCount);
            int available = Mathf.Max(
                0,
                _port.MaximumAlive + Mathf.Max(0, _port.Tuning.SummonerSupportExtraAliveAllowance) - _port.EnemyCount);
            int count = Mathf.Min(requested, available);
            if (count <= 0)
            {
                return 0;
            }

            Vector3 center = summoner.transform.position;
            float radius = Mathf.Max(summoner.Radius + 0.85f, _port.Tuning.SummonerSupportRadius);
            int spawned = 0;
            int sequenceOffset = SummonerSupportSpawnCount;
            for (int index = 0; index < count; index++)
            {
                SurvivorsEnemyRole role = ResolveSummonerSupportRole(sequenceOffset + index);
                if (_port.SpawnGameplayEnemyOffscreen(
                    role,
                    _port.SpawnSequence + index + sequenceOffset * 59 + 1031,
                    radius,
                    radius + _port.Tuning.SpawnBandDepth,
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

        internal void RecordSummonerSupportFeedback(SurvivorsEnemyActor summoner, int spawned)
        {
            if (summoner == null || spawned <= 0)
            {
                return;
            }

            string name = string.IsNullOrWhiteSpace(summoner.DisplayName) ? "Rift Caller" : summoner.DisplayName;
            LastSummonerSupportFeedbackLabel = $"{name}: +{spawned} support";
            SummonerSupportFeedbackCount++;
            _port.RecordStreakRewardFeedback(LastSummonerSupportFeedbackLabel, new Color(0.56f, 0.72f, 1f));
            _port.PlaySupportSpawnFeedback(summoner.transform.position, Mathf.Clamp(18 + spawned * 6, 24, 60));
        }

        internal static SurvivorsEnemyRole ResolveSummonerSupportRole(int sequence)
        {
            return sequence % 4 == 3 ? SurvivorsEnemyRole.Runner : SurvivorsEnemyRole.Swarm;
        }
    }
}
