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
    // Weapon loadout, projectile launch and weapon observation bindings.
    public sealed partial class SurvivorsTemplateController
    {
        internal bool LaunchProjectile(SurvivorsWeaponArchetypeDefinition definition, Vector3 direction) => ProjectileLauncher.LaunchProjectile(definition, direction);

        internal bool LaunchProjectileFrom(
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 origin,
            Vector3 direction,
            int remainingChains,
            int remainingPierces,
            int remainingForks,
            int remainingReturns,
            HashSet<int> ignoredEnemyIds) => ProjectileLauncher.LaunchProjectileFrom(definition, origin, direction, remainingChains, remainingPierces, remainingForks, remainingReturns, ignoredEnemyIds);

        private SurvivorsProjectileLauncher ProjectileLauncher => _projectileLauncher ?? (_projectileLauncher = new SurvivorsProjectileLauncher(this, SpawnSequence));

        Vector3 ISurvivorsProjectileLaunchPort.PlayerPosition => PlayerPosition;

        int ISurvivorsProjectileLaunchPort.ProjectileChainBonus => ProjectileChainBonus;

        int ISurvivorsProjectileLaunchPort.ProjectilePierceBonus => ProjectilePierceBonus;

        int ISurvivorsProjectileLaunchPort.ProjectileForkBonus => ProjectileForkBonus;

        int ISurvivorsProjectileLaunchPort.ProjectileReturnBonus => ProjectileReturnBonus;

        float ISurvivorsProjectileLaunchPort.ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition) => ResolveWeaponDamage(definition);

        void ISurvivorsProjectileLaunchPort.InitializeProjectile(SurvivorsProjectileActor projectile, SurvivorsWeaponArchetypeDefinition definition, SurvivorsProjectileLaunchValues values) =>
            projectile.Initialize(this, definition, values.Direction, values.Speed, values.Damage, values.Radius, values.Lifetime,
                values.Chains, values.Pierces, values.Forks, values.Returns, values.IgnoredEnemyIds);

        void ISurvivorsProjectileLaunchPort.RegisterProjectile(SurvivorsProjectileActor projectile) => _projectiles.Add(projectile);

        void ISurvivorsProjectileLaunchPort.ShowProjectileLaunchFeedback(Vector3 origin) => PlayFeedback(_firePulse, origin, 8, _fireClip);

        private string ResolveWeaponHudLabel() => CompactHudLabels.Weapons(ActiveWeaponIds);

        private float ResolveDisplayedWeaponDamage() => RunWeapons.ResolveDisplayedWeaponDamage();

        private float ResolveDisplayedWeaponCooldownSeconds() => RunWeapons.ResolveDisplayedWeaponCooldownSeconds();

        private SurvivorsWeaponArchetypeDefinition ResolvePrimaryWeaponDefinitionForDisplay() => RunWeapons.ResolvePrimaryWeaponDefinitionForDisplay();

        internal float ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition) => RunWeapons.ResolveWeaponDamage(definition);

        internal float ResolveWeaponCooldownSeconds(SurvivorsWeaponArchetypeDefinition definition) => RunWeapons.ResolveWeaponCooldownSeconds(definition);

        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> ResolveStartingWeaponDefinitions(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> allDefinitions) => RunWeapons.ResolveStartingWeaponDefinitions(allDefinitions);

        private bool TryAddWeaponToLoadout(string weaponId) => RunWeapons.TryAddWeaponToLoadout(weaponId);

        private SurvivorsWeaponArchetypeDefinition FindWeaponDefinition(string weaponId) => RunWeapons.FindWeaponDefinition(weaponId);

        private SurvivorsRunWeapons RunWeapons => _runWeapons ?? (_runWeapons = new SurvivorsRunWeapons(this));

        SurvivorsTemplateTuning ISurvivorsRunWeaponPort.Tuning => CurrentTuning;

        IReadOnlyList<string> ISurvivorsRunWeaponPort.StartingWeaponIds => _selectedClass == null ? null : _selectedClass.StartingWeaponIds;

        int ISurvivorsRunWeaponPort.MaximumSlots => MaxWeaponSlots;

        IReadOnlyList<SurvivorsWeaponArchetypeDefinition> ISurvivorsRunWeaponPort.CreateDefinitions(SurvivorsTemplateTuning resolved)
            => CreateWeaponArchetypeDefinitions(resolved);

        ISurvivorsWeaponLoadoutSession ISurvivorsRunWeaponPort.CreateLoadout(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions)
            => new SurvivorsWeaponLoadoutSession(new SurvivorsWeaponLoadoutRuntime(this, definitions));

        void ISurvivorsRunWeaponPort.WeaponAdded(SurvivorsWeaponArchetypeDefinition definition)
            => BuildSurges.TryTriggerWeaponLoadoutSurge(definition);

        SurvivorsWeaponBonusValues ISurvivorsRunWeaponPort.DamageBonuses => new SurvivorsWeaponBonusValues(
            DamageBonus,
            StreakSurgeDamageBonus,
            RoamingCacheSurgeDamageBonus,
            ArenaShrineSurgeDamageBonus,
            WaystoneFocusDamageBonus,
            WaystoneChainSurgeDamageBonus,
            HordeRushClearSurgeDamageBonus,
            WeaponLoadoutSurgeDamageBonus,
            PassiveLoadoutSurgeDamageBonus,
            BossRelicSurgeDamageBonus,
            GemRushDamageBonus,
            EvolutionChainSurgeDamageBonus,
            EndlessSurgeDamageBonus);

        SurvivorsWeaponBonusValues ISurvivorsRunWeaponPort.CooldownBonuses => new SurvivorsWeaponBonusValues(
            WeaponCooldownMultiplierBonus,
            StreakSurgeCooldownMultiplierBonus,
            RoamingCacheSurgeCooldownMultiplierBonus,
            ArenaShrineSurgeCooldownMultiplierBonus,
            WaystoneFocusCooldownMultiplierBonus,
            WaystoneChainSurgeCooldownMultiplierBonus,
            HordeRushClearSurgeCooldownMultiplierBonus,
            WeaponLoadoutSurgeCooldownMultiplierBonus,
            PassiveLoadoutSurgeCooldownMultiplierBonus,
            BossRelicSurgeCooldownMultiplierBonus,
            GemRushCooldownMultiplierBonus,
            EvolutionChainSurgeCooldownMultiplierBonus,
            EndlessSurgeCooldownMultiplierBonus);

        internal void RecordOrbitHit() => _weaponDiagnostics.RecordOrbitHit();

        internal void RecordMeleeSwing() => _weaponDiagnostics.RecordMeleeSwing();

        internal void RecordMeleeHit() => _weaponDiagnostics.RecordMeleeHit();

        internal void RecordBurstPulse() => _weaponDiagnostics.RecordBurstPulse();

        internal void RecordBurstHit() => _weaponDiagnostics.RecordBurstHit();

        internal void RecordHitscanFire() => _weaponDiagnostics.RecordHitscanFire();

        internal void RecordHitscanHit() => _weaponDiagnostics.RecordHitscanHit();

        internal void RecordProjectilePierceHit() => _weaponDiagnostics.RecordProjectilePierceHit();

        internal void RecordProjectileChainHit() => _weaponDiagnostics.RecordProjectileChainHit();

        internal void RecordProjectileForkSpawn() => _weaponDiagnostics.RecordProjectileForkSpawn();

        internal void RecordProjectileReturnStart() => _weaponDiagnostics.RecordProjectileReturnStart();

        internal void RecordPayloadThrow() => _weaponDiagnostics.RecordPayloadThrow();

        internal void RecordPayloadPlaced() => _weaponDiagnostics.RecordPayloadPlaced();

        internal void RecordPayloadDetonation() => _weaponDiagnostics.RecordPayloadDetonation();

        internal void RecordPayloadExplosionHit() => _weaponDiagnostics.RecordPayloadExplosionHit();

        private SurvivorsOrbitKnockback OrbitKnockback => _orbitKnockback ??
            (_orbitKnockback = new SurvivorsOrbitKnockback(this, _weaponDiagnostics));

        bool ISurvivorsOrbitKnockbackPort.CrimsonAegisActive
            => IsEvolutionActive(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId);

        float ISurvivorsOrbitKnockbackPort.OrbitKnockbackDistance => CurrentTuning.OrbitKnockbackDistance;

        float ISurvivorsOrbitKnockbackPort.CrimsonAegisOrbitKnockbackDistance => CurrentTuning.CrimsonAegisOrbitKnockbackDistance;

        Vector3 ISurvivorsOrbitKnockbackPort.PlayerPosition => PlayerPosition;

        Vector3 ISurvivorsOrbitKnockbackPort.PlayerForward => PlayerForward;

        internal bool ApplyOrbitKnockback(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition)
            => OrbitKnockback.Apply(enemy, definition);

        internal void RecordTempestPrismArcHit(SurvivorsEnemyActor source, SurvivorsEnemyActor target)
            => _weaponDiagnostics.RecordTempestPrismArcHit(
                source == null ? null : source.DisplayName,
                target == null ? null : target.DisplayName);

        public int ProjectileLaunchCount => ProjectileLauncher.ProjectileLaunchCount;

        public int OrbitHitCount => _weaponDiagnostics.OrbitHitCount;

        public int MeleeSwingCount => _weaponDiagnostics.MeleeSwingCount;

        public int MeleeHitCount => _weaponDiagnostics.MeleeHitCount;

        public int BurstPulseCount => _weaponDiagnostics.BurstPulseCount;

        public int BurstHitCount => _weaponDiagnostics.BurstHitCount;

        public int HitscanFireCount => _weaponDiagnostics.HitscanFireCount;

        public int HitscanHitCount => _weaponDiagnostics.HitscanHitCount;

        public int TempestPrismArcHitCount => _weaponDiagnostics.TempestPrismArcHitCount;

        public string LastTempestPrismArcFeedbackLabel => _weaponDiagnostics.LastTempestPrismArcFeedbackLabel;

        public int ProjectilePierceHitCount => _weaponDiagnostics.ProjectilePierceHitCount;

        public int ProjectileChainHitCount => _weaponDiagnostics.ProjectileChainHitCount;

        public int ProjectileForkSpawnCount => _weaponDiagnostics.ProjectileForkSpawnCount;

        public int ProjectileReturnStartCount => _weaponDiagnostics.ProjectileReturnStartCount;

        public int OrbitKnockbackCount => _weaponDiagnostics.OrbitKnockbackCount;

        public string LastOrbitKnockbackFeedbackLabel => _weaponDiagnostics.LastOrbitKnockbackFeedbackLabel;

        public int PayloadThrowCount => _weaponDiagnostics.PayloadThrowCount;

        public int PayloadPlacedCount => _weaponDiagnostics.PayloadPlacedCount;

        public int PayloadDetonationCount => _weaponDiagnostics.PayloadDetonationCount;

        public int PayloadExplosionHitCount => _weaponDiagnostics.PayloadExplosionHitCount;

        public int ActiveProjectileCount => _projectiles.Count;

        public int ActiveWeaponCount => RunWeapons.ActiveWeaponCount;

        public IReadOnlyList<string> ActiveWeaponIds => RunWeapons.ActiveWeaponIds;

        public int ActiveOrbitBladeCount => RunWeapons.ActiveOrbitBladeCount;

        public float ProjectileDamage => ResolveDisplayedWeaponDamage();

        public float WeaponCooldownSeconds => ResolveDisplayedWeaponCooldownSeconds();

        public void FireWeaponForTest()
        {
            EnsureRunStartedForTest();
            RunWeapons.FireForTest(SurvivorsWeaponArchetype.Projectile);
        }

        public bool FireWeaponForTest(SurvivorsWeaponArchetype archetype)
        {
            EnsureRunStartedForTest();
            return RunWeapons.FireForTest(archetype);
        }

        public bool HasWeaponInLoadoutForTest(string weaponId)
        {
            EnsureRunStartedForTest();
            return RunWeapons.ContainsWeapon(weaponId);
        }

        internal Color ResolveProjectileFallbackColor()
        {
            return ActiveUiTheme.GetFeedbackAccentColor(new Color(0.78f, 0.42f, 1f));
        }

        internal Color ResolveProjectileTrailEndColor()
        {
            return WithAlpha(ActiveUiTheme.GetArenaAccentColor(new Color(0.18f, 0.86f, 1f)), 0f);
        }

        private void TickWeapon(float deltaTime)
        {
            RunWeapons.Tick(deltaTime);
        }
    }
}
