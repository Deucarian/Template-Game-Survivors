using System.Collections.Generic;
using Deucarian.WorldSpawning;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsActorMembershipPort
    {
        bool RemoveHordeMember(long id);
        bool RemoveCacheMember(long id);
        bool RemoveShrineMember(long id);
        void ForgetMajorThreat(SurvivorsEnemyActor enemy);
        bool HasSpawnService { get; }
        void Despawn(SpawnInstanceId id, DespawnReason reason);
    }

    /// <summary>One actor-list owner and ordered membership-to-pool release boundary.</summary>
    internal sealed class SurvivorsActorMembership
    {
        private readonly ISurvivorsActorMembershipPort _port;
        public readonly List<SurvivorsEnemyActor> Enemies = new List<SurvivorsEnemyActor>(64);
        public readonly List<SurvivorsPickupActor> Pickups = new List<SurvivorsPickupActor>(128);
        public readonly List<SurvivorsProjectileActor> Projectiles = new List<SurvivorsProjectileActor>(64);
        public SurvivorsActorMembership(ISurvivorsActorMembershipPort port) => _port = port;

        public SurvivorsEncounterClears ReleaseKilledEnemy(SurvivorsEnemyActor enemy) => ReleaseEnemyMembership(enemy, DespawnReason.Killed);
        public void ReleaseEnemy(SurvivorsEnemyActor enemy, DespawnReason reason)
        {
            if (enemy == null) return;
            ReleaseEnemyMembership(enemy, reason);
        }

        private SurvivorsEncounterClears ReleaseEnemyMembership(SurvivorsEnemyActor enemy, DespawnReason reason)
        {
            Enemies.Remove(enemy);
            bool horde = _port.RemoveHordeMember(enemy.InstanceId.Value);
            bool cache = _port.RemoveCacheMember(enemy.InstanceId.Value);
            bool shrine = _port.RemoveShrineMember(enemy.InstanceId.Value);
            _port.ForgetMajorThreat(enemy);
            if (_port.HasSpawnService && enemy.InstanceId.Value > 0) _port.Despawn(enemy.InstanceId, reason);
            return new SurvivorsEncounterClears(horde, cache, shrine);
        }

        public void ReleaseProjectile(SurvivorsProjectileActor projectile, DespawnReason reason)
        {
            if (projectile == null) return;
            Projectiles.Remove(projectile);
            if (_port.HasSpawnService && projectile.InstanceId.Value > 0) _port.Despawn(projectile.InstanceId, reason);
        }
    }
}
