using System;
using System.Collections.Generic;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns projectile request options, shared sequence consumption and successful launch ordering.</summary>
    internal sealed class SurvivorsProjectileLauncher
    {
        private readonly ISurvivorsProjectileLaunchPort _port;
        private readonly SurvivorsSpawnSequence _sequence;
        public SurvivorsProjectileLauncher(ISurvivorsProjectileLaunchPort port, SurvivorsSpawnSequence sequence)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        }

        public int ProjectileLaunchCount { get; private set; }
        public void ResetDiagnostics() => ProjectileLaunchCount = 0;

        public bool LaunchProjectile(SurvivorsWeaponArchetypeDefinition definition, Vector3 direction)
        {
            if (definition == null)
            {
                return false;
            }

            return LaunchProjectileFrom(
                definition,
                _port.PlayerPosition + Vector3.up * 0.4f,
                direction,
                definition.ProjectileChainCount + _port.ProjectileChainBonus,
                definition.ProjectilePierceCount + _port.ProjectilePierceBonus,
                definition.ProjectileForkCount + _port.ProjectileForkBonus,
                definition.ProjectileReturnCount + _port.ProjectileReturnBonus,
                null);
        }

        public bool LaunchProjectileFrom(
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 origin,
            Vector3 direction,
            int remainingChains,
            int remainingPierces,
            int remainingForks,
            int remainingReturns,
            HashSet<int> ignoredEnemyIds)
        {
            if (definition == null || !_port.HasSpawnService)
            {
                return false;
            }

            Vector3 resolvedDirection = direction.sqrMagnitude <= 0.001f ? Vector3.forward : direction.normalized;
            long sequence = _sequence.Next();
            _port.RegisterExplicitPose(sequence, origin + resolvedDirection * 0.55f);
            SpawnResult result = _port.Spawn(new WorldSpawnRequest(
                BasicSurvivorsGame.ProjectileSpawnableId,
                BasicSurvivorsGame.ExplicitSpawnChannelId,
                sequence,
                new WorldSpawnRequestContext("SurvivorsTemplate", groupId: definition.Id)));
            if (!result.Succeeded || result.Instance == null)
            {
                return false;
            }

            SurvivorsProjectileActor projectile = result.Instance.GetComponent<SurvivorsProjectileActor>();
            _port.InitializeProjectile(projectile, definition, new SurvivorsProjectileLaunchValues(
                resolvedDirection,
                definition.ProjectileSpeed,
                _port.ResolveWeaponDamage(definition),
                definition.ProjectileRadius,
                definition.ProjectileLifetimeSeconds,
                remainingChains,
                remainingPierces,
                remainingForks,
                remainingReturns,
                ignoredEnemyIds));
            _port.RegisterProjectile(projectile);
            ProjectileLaunchCount++;
            _port.ShowProjectileLaunchFeedback(origin);
            return true;
        }

    }
}
