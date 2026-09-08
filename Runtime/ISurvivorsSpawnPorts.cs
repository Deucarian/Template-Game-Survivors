using System.Collections.Generic;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsSpawnBackend
    {
        bool HasSpawnService { get; }
        void RegisterExplicitPose(long sequence, Vector3 position);
        SpawnResult Spawn(WorldSpawnRequest request);
    }

    internal interface ISurvivorsEnemySpawnPort : ISurvivorsSpawnBackend
    {
        SurvivorsTemplateTuning Tuning { get; }
        SurvivorsRunFlowRuntime RunFlow { get; }
        Vector3 PlayerPosition { get; }
        float ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole role, float requested);
        float ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole role, float minimum, float maximum);
        float ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string source);
        Vector3 ResolveSafeOffscreenPosition(Vector3 center, float minimum, float maximum, long seed, float padding, float bandDepth);
        void InitializeEnemy(SurvivorsEnemyActor enemy, SurvivorsEnemyProfile profile);
        void RegisterEnemy(SurvivorsEnemyActor enemy);
        void RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string source);
        void RecordSpawnMetric(SurvivorsEnemyRole role);
        void ShowEnemySpawnFeedback(bool major, Vector3 position, int burst);
    }

    internal interface ISurvivorsPickupSpawnPort : ISurvivorsSpawnBackend
    {
        SurvivorsTemplateTuning Tuning { get; }
        float CurrentPickupAttractRange { get; }
        float CurrentPickupAttractionSpeed { get; }
        void InitializePickup(SurvivorsPickupActor pickup, SurvivorsPickupKind kind, int amount, float range, float speed, float radius);
        void RegisterPickup(SurvivorsPickupActor pickup);
    }

    internal interface ISurvivorsProjectileLaunchPort : ISurvivorsSpawnBackend
    {
        Vector3 PlayerPosition { get; }
        int ProjectileChainBonus { get; }
        int ProjectilePierceBonus { get; }
        int ProjectileForkBonus { get; }
        int ProjectileReturnBonus { get; }
        float ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition);
        void InitializeProjectile(SurvivorsProjectileActor projectile, SurvivorsWeaponArchetypeDefinition definition, SurvivorsProjectileLaunchValues values);
        void RegisterProjectile(SurvivorsProjectileActor projectile);
        void ShowProjectileLaunchFeedback(Vector3 origin);
    }
}
