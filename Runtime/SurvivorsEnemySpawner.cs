using System;
using System.Collections.Generic;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns enemy request/profile selection, successful-spawn diagnostics and ordered registration feedback.</summary>
    internal sealed class SurvivorsEnemySpawner
    {
        private readonly ISurvivorsEnemySpawnPort _port;
        private readonly SurvivorsSpawnSequence _sequence;
        public SurvivorsEnemySpawner(ISurvivorsEnemySpawnPort port, SurvivorsSpawnSequence sequence)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        }

        public int SpawnedCount { get; private set; }
        public int MinibossSpawnCount { get; private set; }
        public int BossSpawnCount { get; private set; }
        public void ResetDiagnostics() { SpawnedCount = 0; MinibossSpawnCount = 0; BossSpawnCount = 0; }

        public SurvivorsEnemyProfile ResolveEnemyProfile(SurvivorsEnemyRole role)
        {
            if (_port.RunFlow != null && _port.RunFlow.Definition != null)
            {
                if (role == SurvivorsEnemyRole.Miniboss)
                {
                    return _port.RunFlow.Definition.Miniboss;
                }

                if (role == SurvivorsEnemyRole.Boss)
                {
                    return _port.RunFlow.Definition.Boss;
                }

                return _port.RunFlow.ResolveSwarmProfile(_port.Tuning, role);
            }

            return BasicSurvivorsGame.CreateEnemyProfile(role, _port.Tuning);
        }

        public static WorldSpawnableId ResolveEnemySpawnableId(SurvivorsEnemyRole role)
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

        public static string ResolveEnemyGroupId(SurvivorsEnemyRole role)
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

        public SurvivorsEnemyActor SpawnGameplayEnemyOffscreen(
            SurvivorsEnemyRole role,
            long seed,
            float minimumDistance,
            float maximumDistance,
            string spawnSource)
        {
            Vector3 position = _port.ResolveSafeOffscreenPosition(
                _port.PlayerPosition,
                _port.ResolveGameplaySpawnMinimumDistance(role, minimumDistance),
                _port.ResolveGameplaySpawnMaximumDistance(role, minimumDistance, maximumDistance),
                seed,
                _port.ResolveOffscreenSpawnPadding(role, spawnSource),
                _port.Tuning.SpawnBandDepth);
            return SpawnEnemy(position, explicitPosition: true, role, gameplaySpawn: true, spawnSource: spawnSource);
        }

        public SurvivorsEnemyActor SpawnEnemy(Vector3 position, bool explicitPosition, SurvivorsEnemyRole role, bool gameplaySpawn = false, string spawnSource = null)
        {
            long sequence = _sequence.Next();
            bool trackSpawnSafety = gameplaySpawn || !explicitPosition;
            if (!explicitPosition)
            {
                position = _port.ResolveSafeOffscreenPosition(
                    _port.PlayerPosition,
                    _port.ResolveGameplaySpawnMinimumDistance(role, _port.Tuning.EnemySpawnRadius),
                    _port.ResolveGameplaySpawnMaximumDistance(role, _port.Tuning.EnemySpawnRadius, _port.Tuning.EnemySpawnRadius + _port.Tuning.SpawnBandDepth),
                    sequence,
                    _port.ResolveOffscreenSpawnPadding(role, spawnSource),
                    _port.Tuning.SpawnBandDepth);
                explicitPosition = true;
            }

            WorldSpawnChannelId channel = explicitPosition ? BasicSurvivorsGame.ExplicitSpawnChannelId : BasicSurvivorsGame.RadialSpawnChannelId;
            if (explicitPosition)
            {
                _port.RegisterExplicitPose(sequence, position);
            }

            SurvivorsEnemyProfile profile = ResolveEnemyProfile(role);
            SpawnResult result = _port.Spawn(new WorldSpawnRequest(
                ResolveEnemySpawnableId(role),
                channel,
                sequence,
                new WorldSpawnRequestContext("SurvivorsTemplate", waveId: "wave.survivors.opening-ring", groupId: ResolveEnemyGroupId(role))));
            if (!result.Succeeded || result.Instance == null)
            {
                return null;
            }

            SurvivorsEnemyActor enemy = result.Instance.GetComponent<SurvivorsEnemyActor>();
            _port.InitializeEnemy(enemy, profile);
            _port.RegisterEnemy(enemy);
            SpawnedCount++;
            if (trackSpawnSafety)
            {
                _port.RecordGameplaySpawnSafety(role, enemy.transform.position, spawnSource);
            }

            if (role == SurvivorsEnemyRole.Miniboss)
            {
                _port.RecordSpawnMetric(SurvivorsEnemyRole.Miniboss);
                MinibossSpawnCount++;
                _port.ShowEnemySpawnFeedback(true, enemy.transform.position, 42);
            }
            else if (role == SurvivorsEnemyRole.Boss)
            {
                _port.RecordSpawnMetric(SurvivorsEnemyRole.Boss);
                BossSpawnCount++;
                _port.ShowEnemySpawnFeedback(true, enemy.transform.position, 58);
            }
            else if ((role == SurvivorsEnemyRole.Elite || role == SurvivorsEnemyRole.DreadElite))
            {
                _port.RecordSpawnMetric(SurvivorsEnemyRole.Elite);
                _port.ShowEnemySpawnFeedback(true, enemy.transform.position, 30);
            }
            else
            {
                _port.ShowEnemySpawnFeedback(false, enemy.transform.position, 10);
            }

            return enemy;
        }

    }
}
