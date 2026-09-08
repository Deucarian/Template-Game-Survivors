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
    // Enemy damage, defeat, area effects and status command bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private SurvivorsEnemyDefeatFlow Defeats => _defeats ?? (_defeats = new SurvivorsEnemyDefeatFlow(this, _runSession));

        internal void HandleEnemyKilled(SurvivorsEnemyActor enemy, string source, bool applyAugments)
        { if (enemy != null) Defeats.HandleEnemyKilled(enemy, source, applyAugments); }

        void ISurvivorsEnemyDefeatPort.RecordKillMetric(SurvivorsEnemyRole role)
        {
            Telemetry.Record(SurvivorsRunMetric.FirstKill, RunTimeSeconds);
            if (IsEliteRole(role)) Telemetry.Record(SurvivorsRunMetric.FirstEliteKill, RunTimeSeconds);
            else if (role == SurvivorsEnemyRole.Miniboss) Telemetry.Record(SurvivorsRunMetric.FirstMinibossKill, RunTimeSeconds);
            else if (role == SurvivorsEnemyRole.Boss) Telemetry.Record(SurvivorsRunMetric.FirstBossKill, RunTimeSeconds);
        }

        void ISurvivorsEnemyDefeatPort.SpawnExperience(Vector3 position, int amount) => SpawnPickup(SurvivorsPickupKind.Experience, position, amount);

        void ISurvivorsEnemyDefeatPort.RegisterStreak(Vector3 position) => RegisterKillStreak(position);

        void ISurvivorsEnemyDefeatPort.ShowDeath(Vector3 position, SurvivorsEnemyRole role, float radius) => RecordEnemyDeathEffect(position, role, radius);

        void ISurvivorsEnemyDefeatPort.TriggerDeathNova(Vector3 position, string source, bool applyAugments) => TryTriggerDeathNova(position, source, applyAugments);

        void ISurvivorsEnemyDefeatPort.SpawnMajorRewards(Vector3 position, SurvivorsEnemyRole role, float radius, int xp)
        {
            RecordMajorRewardDropFeedback(position, role, radius);
            SpawnMajorRewardPickupCache(position, role, radius);
            TryDropHealthPickup(position + new Vector3(radius * 0.7f, 0f, radius * 0.35f));
            TryActivateEndlessSurge(role, position, xp);
        }

        void ISurvivorsEnemyDefeatPort.PlayDeath(Vector3 position, int burst) => PlayFeedback(_killPulse, position, burst, _killClip, AudioEventEnemyDeath, 0.08f);

        void ISurvivorsEnemyDefeatPort.SpawnSplitterChildren(Vector3 position, string displayName) => SpawnSplitterChildren(position, displayName);

        void ISurvivorsEnemyDefeatPort.GrantMajorEnemyReward(SurvivorsEnemyRole role) => GrantMajorEnemyReward(role);

        bool ISurvivorsEnemyDefeatPort.OpenUpgradeRewardDraft(SurvivorsEnemyRole role) => OpenUpgradeRewardDraft(role, requireEvolutionChoice: false);

        void ISurvivorsEnemyDefeatPort.OpenBossRelicDraft() => OpenBossRelicDraft();

        void ISurvivorsEnemyDefeatPort.EnterVictory() => EnterVictory();

        void ISurvivorsEnemyDefeatPort.RewardHordeClear(Vector3 position) => HordeRush.SpawnHordeRushClearReward(position);

        void ISurvivorsEnemyDefeatPort.RewardCacheClear(Vector3 position) => RoamingCaches.SpawnRoamingCacheAmbushClearReward(position);

        void ISurvivorsEnemyDefeatPort.RewardShrineClear(Vector3 position) => ShrineTrials.SpawnArenaShrineClearReward(position);

        internal DamageResolutionResult ResolveEnemyDamage(HealthState health, float amount, string source, bool applyAugments) => EnemyDamage.ResolveEnemyDamage(health, amount, source, applyAugments);

        private static bool CanApplyDamageAugments(string source) => SurvivorsEnemyDamage.CanApplyDamageAugments(source);

        private SurvivorsDamageAugments DamageAugments => _damageAugments ?? (_damageAugments = new SurvivorsDamageAugments(this));

        private SurvivorsEnemyDamage EnemyDamage => _enemyDamage ?? (_enemyDamage = new SurvivorsEnemyDamage(() => CriticalChanceNormalized, () => CriticalDamageMultiplier));

        SurvivorsDamageAugmentValues ISurvivorsDamageAugmentPort.Values => new SurvivorsDamageAugmentValues(LifestealRatio, BarrierOnDamageRatio, PoisonDamageRatio, BleedDamageRatio, ExecuteThresholdNormalized);

        SurvivorsTemplateTuning ISurvivorsDamageAugmentPort.Tuning => CurrentTuning;

        bool ISurvivorsDamageAugmentPort.IsPlayerBound => PlayerVitals.IsBound;

        void ISurvivorsDamageAugmentPort.HealPlayer(float amount) => PlayerVitals.Heal(amount);

        void ISurvivorsDamageAugmentPort.RestoreBarrier(float amount) => PlayerVitals.RestoreBarrier(amount);

        bool ISurvivorsDamageAugmentPort.IsEvolutionActive(string id) => IsEvolutionActive(id);

        string ISurvivorsDamageAugmentPort.ResolveUpgradeName(string id) => ResolveUpgradeDisplayName(new RunUpgradeId(id));

        string ISurvivorsDamageAugmentPort.ResolveWeaponName(string id) => ResolveWeaponBuildDisplayName(id);

        internal void ApplyDamageAugmentsToEnemy(SurvivorsEnemyActor enemy, DamageResult damage, string source)
        { if (enemy != null) DamageAugments.ApplyDamageAugmentsToEnemy(enemy, damage, source); }

        internal void ApplyWeaponStatusEffectsToEnemy(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition, DamageResult damage)
        { if (enemy != null) DamageAugments.ApplyWeaponStatusEffectsToEnemy(enemy, definition, damage); }

        private SurvivorsPayloadHazardRewards PayloadHazards => _payloadHazards ?? (_payloadHazards = new SurvivorsPayloadHazardRewards(this));

        private SurvivorsDeathNova DeathNova => _deathNova ?? (_deathNova = new SurvivorsDeathNova(_enemies, () => DeathNovaDamage, () => DeathNovaRadius, (position, count) => PlayFeedback(_killPulse, position, count, _killClip)));

        internal void RecordPayloadHazardTick() => PayloadHazards.RecordPayloadHazardTick();

        internal void RecordPayloadHazardSnare(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition, Vector3 origin)
        { if (enemy != null) PayloadHazards.RecordPayloadHazardSnare(enemy.DisplayName, definition, origin); }

        private void TickPayloadHazardChain(float deltaTime) => PayloadHazards.TickPayloadHazardChain(deltaTime);

        private void TryTriggerDeathNova(Vector3 position, string source, bool applyAugments) => DeathNova.TryTriggerDeathNova(position, source, applyAugments);

        private void RecordPlayerDamageFeedback(DamageResult damage, Vector3 position) => DamageFeedback.RecordPlayerDamageFeedback(damage, position);

        private void RecordDamagePopup(Vector3 worldPosition, float amount, bool playerDamage, bool critical) => DamageFeedback.RecordDamagePopup(worldPosition, amount, playerDamage, critical);

        void ISurvivorsDamageFeedbackPort.RecordPopup(Vector3 position, float amount, bool playerDamage, bool critical) => _damageFeedback.Record(position, amount, playerDamage, critical);

        void ISurvivorsDamageFeedbackPort.PlayCombatHitAudio() => PlayAudioEvent(AudioEventCombatHit, _fireClip, 0.08f);

        void ISurvivorsDamageFeedbackPort.TryEnrage(ISurvivorsFeedbackEnemy enemy) => TryTriggerMajorThreatEnrage((SurvivorsEnemyActor)enemy);

        SurvivorsTemplateTuning ISurvivorsRangedDodgePort.Tuning => CurrentTuning;

        Vector3 ISurvivorsRangedDodgePort.PlayerPosition => PlayerPosition;

        bool ISurvivorsRangedDodgePort.TrySpawnExperience(Vector3 position, int amount) => SpawnPickup(SurvivorsPickupKind.Experience, position, amount) != null;

        void ISurvivorsRangedDodgePort.ShowStreakFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        void ISurvivorsRangedDodgePort.PlayDodgePulse(Vector3 position, int count) => PlayFeedback(_pickupPulse, position, count, _pickupClip);

        SurvivorsEncounterClears ISurvivorsEnemyDefeatPort.ReleaseKilledEnemy(ISurvivorsDefeatTarget target) => ActorMembership.ReleaseKilledEnemy((SurvivorsEnemyActor)target);

        private int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) => AreaDamage.DamageNonMajorEnemies(position, radius, damage, source);

        private SurvivorsAreaDamage AreaDamage => _areaDamage ?? (_areaDamage = new SurvivorsAreaDamage(EnemySpatialQueries, (enemy, damage, source) => enemy.ApplyDamage(damage, source)));

        public int KilledCount => Defeats.KilledCount;

        public int FrostFanSlowApplicationCount => DamageAugments.FrostFanSlowApplicationCount;

        public string LastFrostFanSlowFeedbackLabel => DamageAugments.LastFrostFanSlowFeedbackLabel;

        public int CinderBurnApplicationCount => DamageAugments.CinderBurnApplicationCount;

        public string LastCinderBurnFeedbackLabel => DamageAugments.LastCinderBurnFeedbackLabel;

        public int PayloadHazardTickCount => PayloadHazards.PayloadHazardTickCount;

        public int PayloadHazardSnareCount => PayloadHazards.PayloadHazardSnareCount;

        public string LastPayloadHazardSnareFeedbackLabel => PayloadHazards.LastPayloadHazardSnareFeedbackLabel;

        public int PayloadHazardChainActivationCount => PayloadHazards.PayloadHazardChainActivationCount;

        public int PayloadHazardChainPulseHitCount => PayloadHazards.PayloadHazardChainPulseHitCount;

        public int PayloadHazardChainExperienceGemDropCount => PayloadHazards.PayloadHazardChainExperienceGemDropCount;

        public string LastPayloadHazardChainFeedbackLabel => PayloadHazards.LastPayloadHazardChainFeedbackLabel;

        public int EliteKilledCount => Defeats.EliteKilledCount;

        public int MinibossKilledCount => Defeats.MinibossKilledCount;

        public int BossKilledCount => Defeats.BossKilledCount;

        public int PlayerDamageFeedbackCount => DamageFeedback.PlayerDamageFeedbackCount;

        public int EnemyHitFlashFeedbackCount => DamageFeedback.EnemyHitFlashFeedbackCount;

        public int CriticalHitFeedbackCount => DamageFeedback.CriticalHitFeedbackCount;

        public int DeathNovaTriggerCount => DeathNova.DeathNovaTriggerCount;

        public int DeathNovaHitCount => DeathNova.DeathNovaHitCount;

        public CombatCatalog CombatCatalog => EnemyDamage.Catalog;
    }
}
