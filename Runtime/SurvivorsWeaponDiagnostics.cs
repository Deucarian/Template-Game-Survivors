namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Retained weapon hit/trigger observations, reset only at the corresponding new-run phases.</summary>
    internal sealed class SurvivorsWeaponDiagnostics
    {
        internal int OrbitHitCount { get; private set; }
        internal int MeleeSwingCount { get; private set; }
        internal int MeleeHitCount { get; private set; }
        internal int BurstPulseCount { get; private set; }
        internal int BurstHitCount { get; private set; }
        internal int HitscanFireCount { get; private set; }
        internal int HitscanHitCount { get; private set; }
        internal int TempestPrismArcHitCount { get; private set; }
        internal string LastTempestPrismArcFeedbackLabel { get; private set; } = string.Empty;
        internal int ProjectilePierceHitCount { get; private set; }
        internal int ProjectileChainHitCount { get; private set; }
        internal int ProjectileForkSpawnCount { get; private set; }
        internal int ProjectileReturnStartCount { get; private set; }
        internal int OrbitKnockbackCount { get; private set; }
        internal string LastOrbitKnockbackFeedbackLabel { get; private set; } = string.Empty;
        internal int PayloadThrowCount { get; private set; }
        internal int PayloadPlacedCount { get; private set; }
        internal int PayloadDetonationCount { get; private set; }
        internal int PayloadExplosionHitCount { get; private set; }

        internal void RecordOrbitHit() => OrbitHitCount++;
        internal void RecordMeleeSwing() => MeleeSwingCount++;
        internal void RecordMeleeHit() => MeleeHitCount++;
        internal void RecordBurstPulse() => BurstPulseCount++;
        internal void RecordBurstHit() => BurstHitCount++;
        internal void RecordHitscanFire() => HitscanFireCount++;
        internal void RecordHitscanHit() => HitscanHitCount++;
        internal void RecordProjectilePierceHit() => ProjectilePierceHitCount++;
        internal void RecordProjectileChainHit() => ProjectileChainHitCount++;
        internal void RecordProjectileForkSpawn() => ProjectileForkSpawnCount++;
        internal void RecordProjectileReturnStart() => ProjectileReturnStartCount++;
        internal void RecordPayloadThrow() => PayloadThrowCount++;
        internal void RecordPayloadPlaced() => PayloadPlacedCount++;
        internal void RecordPayloadDetonation() => PayloadDetonationCount++;
        internal void RecordPayloadExplosionHit() => PayloadExplosionHitCount++;

        internal void RecordTempestPrismArcHit(string source, string target)
        {
            TempestPrismArcHitCount++;
            string sourceName = string.IsNullOrWhiteSpace(source) ? "target" : source;
            string targetName = string.IsNullOrWhiteSpace(target) ? "nearby enemy" : target;
            LastTempestPrismArcFeedbackLabel = $"Tempest Prism arced from {sourceName} to {targetName}";
        }

        internal void RecordOrbitKnockback(bool crimsonAegis, string weaponName, string enemyName)
        {
            OrbitKnockbackCount++;
            string name = string.IsNullOrWhiteSpace(weaponName) ? "Orbit" : weaponName;
            LastOrbitKnockbackFeedbackLabel = crimsonAegis
                ? $"Crimson Aegis pushed {enemyName}"
                : $"{name} pushed {enemyName}";
        }

        internal void ResetHitDiagnostics()
        {
            OrbitHitCount = 0;
            MeleeSwingCount = 0;
            MeleeHitCount = 0;
            BurstPulseCount = 0;
            BurstHitCount = 0;
            HitscanFireCount = 0;
            HitscanHitCount = 0;
            TempestPrismArcHitCount = 0;
            LastTempestPrismArcFeedbackLabel = string.Empty;
            ProjectilePierceHitCount = 0;
            ProjectileChainHitCount = 0;
            ProjectileForkSpawnCount = 0;
            ProjectileReturnStartCount = 0;
            OrbitKnockbackCount = 0;
            LastOrbitKnockbackFeedbackLabel = string.Empty;
        }

        internal void ResetPayloadDiagnostics()
        {
            PayloadThrowCount = 0;
            PayloadPlacedCount = 0;
            PayloadDetonationCount = 0;
            PayloadExplosionHitCount = 0;
        }
    }
}
