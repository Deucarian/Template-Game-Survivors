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
    // Live vitals, motion, dash and derived stat bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private SurvivorsPlayerStats PlayerStats => _playerStats ?? (_playerStats = new SurvivorsPlayerStats(this));

        private SurvivorsHealthPickupDrops HealthPickupDrops => _healthPickupDrops ?? (_healthPickupDrops = new SurvivorsHealthPickupDrops(this));

        SurvivorsTemplateTuning ISurvivorsPlayerStatReadPort.Tuning => CurrentTuning;

        SurvivorsUpgradeModifiers ISurvivorsPlayerStatReadPort.Modifiers => UpgradeModifiers;

        SurvivorsRunState ISurvivorsPlayerStatReadPort.State => State;

        float ISurvivorsPlayerStatReadPort.MaxHealth => MaxHealth;

        float ISurvivorsPlayerStatReadPort.CurrentHealth => CurrentHealth;

        SurvivorsPlayerSurgeValues ISurvivorsPlayerStatReadPort.MovementSurges => new SurvivorsPlayerSurgeValues(
            StreakSurgeMoveSpeedBonus,
            RoamingCacheSurgeMoveSpeedBonus,
            ArenaShrineSurgeMoveSpeedBonus,
            WaystoneFocusMoveSpeedBonus,
            WaystoneChainSurgeMoveSpeedBonus,
            HordeRushClearSurgeMoveSpeedBonus,
            WeaponLoadoutSurgeMoveSpeedBonus,
            PassiveLoadoutSurgeMoveSpeedBonus,
            BossRelicSurgeMoveSpeedBonus,
            GemRushMoveSpeedBonus,
            EvolutionChainSurgeMoveSpeedBonus,
            EndlessSurgeMoveSpeedBonus);

        SurvivorsPlayerSurgeValues ISurvivorsPlayerStatReadPort.PickupSurges => new SurvivorsPlayerSurgeValues(
            StreakSurgePickupRangeBonus,
            RoamingCacheSurgePickupRangeBonus,
            ArenaShrineSurgePickupRangeBonus,
            WaystoneFocusPickupRangeBonus,
            WaystoneChainSurgePickupRangeBonus,
            HordeRushClearSurgePickupRangeBonus,
            WeaponLoadoutSurgePickupRangeBonus,
            PassiveLoadoutSurgePickupRangeBonus,
            BossRelicSurgePickupRangeBonus,
            GemRushPickupRangeBonus,
            EvolutionChainSurgePickupRangeBonus,
            EndlessSurgePickupRangeBonus);

        bool ISurvivorsHealthPickupDropPort.IsHealthBound => PlayerVitals.IsBound;

        int ISurvivorsHealthPickupDropPort.HealAmount => CurrentTuning.HealthPickupHealAmount;

        float ISurvivorsHealthPickupDropPort.CurrentHealth => CurrentHealth;

        float ISurvivorsHealthPickupDropPort.MaxHealth => MaxHealth;

        bool ISurvivorsHealthPickupDropPort.SpawnHealthPickup(Vector3 position, int amount)
            => SpawnPickup(SurvivorsPickupKind.Health, position, amount) != null;

        private SurvivorsPlayerVitals PlayerVitals => _playerVitals ?? (_playerVitals = new SurvivorsPlayerVitals(this));

        private SurvivorsPlayerMotion PlayerMotion => _playerMotion ?? (_playerMotion = new SurvivorsPlayerMotion(this));

        public void ApplyDamageToPlayer(float amount, string source) => PlayerVitals.ApplyDamageToPlayer(amount, source);

        SurvivorsTemplateTuning ISurvivorsPlayerDamagePort.Tuning => CurrentTuning;

        int ISurvivorsPlayerDamagePort.DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) =>
            DamageNonMajorEnemies(position, radius, damage, source);

        void ISurvivorsPlayerDamagePort.ShowBlockedDamage(bool invulnerable)
        {
            if (invulnerable) PlayFeedback(_pickupPulse, PlayerPosition, 4, null);
            else PlayFeedback(_bossPulse, PlayerPosition, 8, _dangerClip);
        }

        void ISurvivorsPlayerDamagePort.RecordDamage(DamageResult damage, Vector3 position) => RecordPlayerDamageFeedback(damage, position);

        void ISurvivorsPlayerDamagePort.Defeat()
        {
            GrantRunRewards(victory: false);
            _runSession.Defeat();
            ClearRewardDrafts();
            PlayFeedback(_bossPulse, PlayerPosition, 34, _dangerClip, AudioEventDefeat, 0.5f);
        }

        void ISurvivorsPlayerDamagePort.ShowClutch(string label, int hitCount)
        {
            RecordStreakRewardFeedback(label, new Color(1f, 0.32f, 0.42f));
            PlayFeedback(_bossPulse, PlayerPosition, Mathf.Clamp(24 + hitCount * 5, 30, 72), _dangerClip, AudioEventLowHealthWarning, 0.5f);
        }

        void ISurvivorsPlayerDamagePort.ShowHurt() => PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip);

        SurvivorsTemplateTuning ISurvivorsPlayerMotionPort.Tuning => CurrentTuning;

        bool ISurvivorsPlayerMotionPort.HasPlayer => _playerObject != null;

        Vector3 ISurvivorsPlayerMotionPort.Position { get => PlayerPosition; set => _playerObject.transform.position = value; }

        Vector3 ISurvivorsPlayerMotionPort.Forward { get => PlayerForward; set => _playerObject.transform.forward = value; }

        float ISurvivorsPlayerMotionPort.MoveSpeed => PlayerMoveSpeed;

        void ISurvivorsPlayerMotionPort.RecordTravel(Vector3 delta) => RecordRoamingArenaTravel(delta);

        void ISurvivorsPlayerMotionPort.ExtendSafety(float seconds) => PlayerVitals.ExtendSafety(seconds);

        int ISurvivorsPlayerMotionPort.ApplyDashPressure(Vector3 start, Vector3 end, Vector3 direction, Action onDamageHit) =>
            SurvivorsDashPressure.Apply(CurrentTuning, _enemies, start, end, direction, onDamageHit);

        void ISurvivorsPlayerMotionPort.ShowDash(Vector3 position, string label, int shoved)
        {
            RecordStreakRewardFeedback(label, new Color(0.54f, 0.84f, 1f));
            PlayFeedback(_pickupPulse, position, Mathf.Clamp(18 + shoved * 4, 18, 54), _pickupClip);
        }

        public int LowHealthClutchPulseCount => PlayerVitals.LowHealthClutchPulseCount;

        public int LowHealthClutchPulseHitCount => PlayerVitals.LowHealthClutchPulseHitCount;

        public string LastLowHealthClutchPulseFeedbackLabel => PlayerVitals.LastLowHealthClutchPulseFeedbackLabel;

        public int DashUseCount => PlayerMotion.DashUseCount;

        public int DashEnemyShoveCount => PlayerMotion.DashEnemyShoveCount;

        public int DashDamageHitCount => PlayerMotion.DashDamageHitCount;

        public string LastDashFeedbackLabel => PlayerMotion.LastDashFeedbackLabel;

        public int HealthPickupCollectedCount => PlayerVitals.HealthPickupCollectedCount;

        public float HealthRestoredByPickups => PlayerVitals.HealthRestoredByPickups;

        public float BarrierValue => PlayerVitals.BarrierValue;

        public float PlayerMoveSpeed => PlayerStats.PlayerMoveSpeed;

        public float DashCooldownRemainingSeconds => PlayerMotion.CooldownRemaining;

        public float PlayerSafetyRemainingSeconds => PlayerVitals.SafetyRemaining;

        public bool IsPlayerSafetyActive => PlayerVitals.SafetyRemaining > 0f;

        public float CurrentPickupAttractRange => PlayerStats.CurrentPickupAttractRange;

        public float CurrentPickupAttractionSpeed => PlayerStats.CurrentPickupAttractionSpeed;

        public float CriticalChanceNormalized => PlayerStats.CriticalChanceNormalized;

        public float CriticalDamageMultiplier => PlayerStats.CriticalDamageMultiplier;

        public float DeathNovaDamage => PlayerStats.DeathNovaDamage;

        public float DeathNovaRadius => PlayerStats.DeathNovaRadius;

        public float CurrentHealth => PlayerVitals.CurrentHealth;

        public float MaxHealth => PlayerVitals.MaxHealth;

        public float BarrierCapacity => PlayerStats.BarrierCapacity;

        public Vector3 PlayerPosition => _playerObject == null ? transform.position : _playerObject.transform.position;

        public Vector3 PlayerForward => _playerObject == null ? Vector3.forward : _playerObject.transform.forward;

        public bool IsLowHealthWarningActive => PlayerStats.IsLowHealthWarningActive;

        public float DamageTakenThisRun => PlayerVitals.DamageTaken;

        public SurvivorsPickupActor SpawnHealthForTest(Vector3 position, int amount)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.Health, position, amount);
        }

        public bool DashForTest(Vector2 directionInput)
        {
            EnsureRunStartedForTest();
            return PlayerMotion.TryDash(directionInput);
        }

        public void KillPlayerForTest()
        {
            ApplyDamageToPlayer(MaxHealth + 1000f, "test.kill");
        }

        private bool TryDropHealthPickup(Vector3 position) => HealthPickupDrops.TryDropHealthPickup(position);

        private void DrawLowHealthWarning() => SurvivorsStatusHudPresenter.DrawLowHealth(IsLowHealthWarningActive, _lowHealthStyle);
    }
}
