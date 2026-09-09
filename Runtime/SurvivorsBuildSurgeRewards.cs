using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsBuildSurgePort
    {
        SurvivorsTemplateTuning Tuning { get; }
        Vector3 PlayerPosition { get; }
        int WeaponCount { get; }
        int DamageNonMajor(Vector3 position, float radius, float damage, string source);
        int RecallGems();
        Color RelicAccent(SurvivorsRelicDefinition relic);
        void ShowFeedback(string label, Color color);
        void PlayPulse(int count, bool boss);
    }
    /// <summary>Owns acquisition milestone payoffs, one-time full-slot rewards and their timed build bonuses.</summary>
    internal sealed class SurvivorsBuildSurgeRewards
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly ISurvivorsBuildSurgePort _port;
        public SurvivorsBuildSurgeRewards(SurvivorsRunBuildState build, ISurvivorsBuildSurgePort port)
        { _build = build ?? throw new ArgumentNullException(nameof(build)); _port = port ?? throw new ArgumentNullException(nameof(port)); }
        private float _weaponLoadoutSurgeTimer;
        private float _passiveLoadoutSurgeTimer;
        private float _bossRelicSurgeTimer;
        private float _evolutionChainSurgeTimer;
        private bool _weaponLoadoutSurgeUsed;
        private bool _passiveLoadoutSurgeUsed;
        public int BossRelicSurgeCount { get; private set; }
        public int BossRelicSurgeHitCount { get; private set; }
        public string LastBossRelicSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int WeaponLoadoutSurgeActivationCount { get; private set; }
        public int WeaponLoadoutSurgePulseHitCount { get; private set; }
        public string LastWeaponLoadoutSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int PassiveLoadoutSurgeActivationCount { get; private set; }
        public int PassiveLoadoutSurgePulseHitCount { get; private set; }
        public string LastPassiveLoadoutSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int WeaponEvolutionSurgeCount { get; private set; }
        public int WeaponEvolutionSurgeHitCount { get; private set; }
        public int EvolutionMagnetRecallCount { get; private set; }
        public int EvolutionMagnetRecallGemCount { get; private set; }
        public int EvolutionChainSurgeActivationCount { get; private set; }
        public int EvolutionChainSurgePulseHitCount { get; private set; }
        public string LastWeaponEvolutionSurgeFeedbackLabel { get; private set; } = string.Empty;
        public string LastEvolutionMagnetRecallFeedbackLabel { get; private set; } = string.Empty;
        public string LastEvolutionChainSurgeFeedbackLabel { get; private set; } = string.Empty;
        public bool IsWeaponLoadoutSurgeActive => _weaponLoadoutSurgeTimer > 0f;
        public float WeaponLoadoutSurgeRemainingSeconds => Mathf.Max(0f, _weaponLoadoutSurgeTimer);
        public float WeaponLoadoutSurgeDamageBonus => IsWeaponLoadoutSurgeActive ? Mathf.Max(0f, _port.Tuning.WeaponLoadoutSurgeDamageBonus) : 0f;
        public float WeaponLoadoutSurgeMoveSpeedBonus => IsWeaponLoadoutSurgeActive ? Mathf.Max(0f, _port.Tuning.WeaponLoadoutSurgeMoveSpeedBonus) : 0f;
        public float WeaponLoadoutSurgeCooldownMultiplierBonus => IsWeaponLoadoutSurgeActive ? Mathf.Min(0f, _port.Tuning.WeaponLoadoutSurgeCooldownMultiplierBonus) : 0f;
        public float WeaponLoadoutSurgePickupRangeBonus => IsWeaponLoadoutSurgeActive ? Mathf.Max(0f, _port.Tuning.WeaponLoadoutSurgePickupRangeBonus) : 0f;
        public bool IsPassiveLoadoutSurgeActive => _passiveLoadoutSurgeTimer > 0f;
        public float PassiveLoadoutSurgeRemainingSeconds => Mathf.Max(0f, _passiveLoadoutSurgeTimer);
        public float PassiveLoadoutSurgeDamageBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, _port.Tuning.PassiveLoadoutSurgeDamageBonus) : 0f;
        public float PassiveLoadoutSurgeMoveSpeedBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, _port.Tuning.PassiveLoadoutSurgeMoveSpeedBonus) : 0f;
        public float PassiveLoadoutSurgeCooldownMultiplierBonus => IsPassiveLoadoutSurgeActive ? Mathf.Min(0f, _port.Tuning.PassiveLoadoutSurgeCooldownMultiplierBonus) : 0f;
        public float PassiveLoadoutSurgePickupRangeBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, _port.Tuning.PassiveLoadoutSurgePickupRangeBonus) : 0f;
        public float PassiveLoadoutSurgeExperienceGainMultiplierBonus => IsPassiveLoadoutSurgeActive ? Mathf.Max(0f, _port.Tuning.PassiveLoadoutSurgeExperienceGainMultiplierBonus) : 0f;
        public bool IsBossRelicSurgeActive => _bossRelicSurgeTimer > 0f;
        public float BossRelicSurgeRemainingSeconds => Mathf.Max(0f, _bossRelicSurgeTimer);
        public float BossRelicSurgeDamageBonus => IsBossRelicSurgeActive ? Mathf.Max(0f, _port.Tuning.BossRelicSurgeDamageBonus) : 0f;
        public float BossRelicSurgeMoveSpeedBonus => IsBossRelicSurgeActive ? Mathf.Max(0f, _port.Tuning.BossRelicSurgeMoveSpeedBonus) : 0f;
        public float BossRelicSurgeCooldownMultiplierBonus => IsBossRelicSurgeActive ? Mathf.Min(0f, _port.Tuning.BossRelicSurgeCooldownMultiplierBonus) : 0f;
        public float BossRelicSurgePickupRangeBonus => IsBossRelicSurgeActive ? Mathf.Max(0f, _port.Tuning.BossRelicSurgePickupRangeBonus) : 0f;
        public bool IsEvolutionChainSurgeActive => _evolutionChainSurgeTimer > 0f;
        public float EvolutionChainSurgeRemainingSeconds => Mathf.Max(0f, _evolutionChainSurgeTimer);
        public float EvolutionChainSurgeDamageBonus => IsEvolutionChainSurgeActive ? Mathf.Max(0f, _port.Tuning.EvolutionChainSurgeDamageBonus) : 0f;
        public float EvolutionChainSurgeMoveSpeedBonus => IsEvolutionChainSurgeActive ? Mathf.Max(0f, _port.Tuning.EvolutionChainSurgeMoveSpeedBonus) : 0f;
        public float EvolutionChainSurgeCooldownMultiplierBonus => IsEvolutionChainSurgeActive ? Mathf.Min(0f, _port.Tuning.EvolutionChainSurgeCooldownMultiplierBonus) : 0f;
        public float EvolutionChainSurgePickupRangeBonus => IsEvolutionChainSurgeActive ? Mathf.Max(0f, _port.Tuning.EvolutionChainSurgePickupRangeBonus) : 0f;
        public void Reset()
        {
            BossRelicSurgeCount = 0;
            BossRelicSurgeHitCount = 0;
            LastBossRelicSurgeFeedbackLabel = string.Empty;
            WeaponLoadoutSurgeActivationCount = 0;
            WeaponLoadoutSurgePulseHitCount = 0;
            LastWeaponLoadoutSurgeFeedbackLabel = string.Empty;
            PassiveLoadoutSurgeActivationCount = 0;
            PassiveLoadoutSurgePulseHitCount = 0;
            LastPassiveLoadoutSurgeFeedbackLabel = string.Empty;
            WeaponEvolutionSurgeCount = 0;
            WeaponEvolutionSurgeHitCount = 0;
            EvolutionMagnetRecallCount = 0;
            EvolutionMagnetRecallGemCount = 0;
            EvolutionChainSurgeActivationCount = 0;
            EvolutionChainSurgePulseHitCount = 0;
            LastWeaponEvolutionSurgeFeedbackLabel = string.Empty;
            LastEvolutionMagnetRecallFeedbackLabel = string.Empty;
            LastEvolutionChainSurgeFeedbackLabel = string.Empty;
            _weaponLoadoutSurgeTimer = 0f;
            _passiveLoadoutSurgeTimer = 0f;
            _bossRelicSurgeTimer = 0f;
            _evolutionChainSurgeTimer = 0f;
            _weaponLoadoutSurgeUsed = false;
            _passiveLoadoutSurgeUsed = false;
        }
        public void TriggerWeaponEvolutionSurge(RunUpgradeDefinition upgrade)
        {
            float radius = Mathf.Max(0f, _port.Tuning.EvolutionSurgeRadius);
            float damage = Mathf.Max(0f, _port.Tuning.EvolutionSurgeDamage);
            string name = upgrade == null ? "Evolution" : _build.ResolveUpgradeDisplayName(upgrade.Id);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(_port.PlayerPosition, radius, damage, "survivors.evolution.surge") : 0;

            int recalledGemCount = _port.RecallGems();
            if (recalledGemCount > 0)
            {
                EvolutionMagnetRecallCount++;
                EvolutionMagnetRecallGemCount += recalledGemCount;
                LastEvolutionMagnetRecallFeedbackLabel = $"{name} Recall: {recalledGemCount} XP gems pulled";
            }

            WeaponEvolutionSurgeCount++;
            WeaponEvolutionSurgeHitCount += hitCount;
            LastWeaponEvolutionSurgeFeedbackLabel = $"{name} Surge: {hitCount} enemies hit";
            if (recalledGemCount > 0)
            {
                LastWeaponEvolutionSurgeFeedbackLabel += $", {recalledGemCount} XP recalled";
            }

            _port.ShowFeedback(LastWeaponEvolutionSurgeFeedbackLabel, new Color(0.72f, 0.42f, 1f));
            _port.PlayPulse(Mathf.Clamp(46 + hitCount * 5, 54, 96), false);
        }

        public void TriggerEvolutionChainSurge(RunUpgradeDefinition upgrade)
        {
            int evolutionCount = _build.EvolutionIds.Count;
            if (evolutionCount < Mathf.Max(2, _port.Tuning.EvolutionChainSurgeMinimumEvolutions))
            {
                return;
            }

            float duration = Mathf.Max(0f, _port.Tuning.EvolutionChainSurgeDurationSeconds);
            if (duration > 0f)
            {
                _evolutionChainSurgeTimer = Mathf.Max(_evolutionChainSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, _port.Tuning.EvolutionChainSurgePulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.EvolutionChainSurgePulseDamage);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(_port.PlayerPosition, radius, damage, "survivors.evolution.legend-surge") : 0;

            string name = upgrade == null ? "Evolution" : _build.ResolveUpgradeDisplayName(upgrade.Id);
            EvolutionChainSurgeActivationCount++;
            EvolutionChainSurgePulseHitCount += hitCount;
            LastEvolutionChainSurgeFeedbackLabel = $"{name} Legend Surge: {evolutionCount} evolutions, {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastEvolutionChainSurgeFeedbackLabel += $", rush {EvolutionChainSurgeRemainingSeconds:0.#}s";
            }

            _port.ShowFeedback(LastEvolutionChainSurgeFeedbackLabel, new Color(1f, 0.66f, 0.2f));
            _port.PlayPulse(Mathf.Clamp(58 + hitCount * 5, 64, 112), false);
        }

        public void TryTriggerWeaponLoadoutSurge(SurvivorsWeaponArchetypeDefinition addedWeapon)
        {
            if (_weaponLoadoutSurgeUsed || _port.WeaponCount < _build.MaxWeaponSlots)
            {
                return;
            }

            _weaponLoadoutSurgeUsed = true;

            float duration = Mathf.Max(0f, _port.Tuning.WeaponLoadoutSurgeDurationSeconds);
            if (duration > 0f)
            {
                _weaponLoadoutSurgeTimer = Mathf.Max(_weaponLoadoutSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, _port.Tuning.WeaponLoadoutSurgePulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.WeaponLoadoutSurgePulseDamage);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(_port.PlayerPosition, radius, damage, "survivors.weapon-loadout.arsenal-surge") : 0;

            string name = addedWeapon == null ? "Weapon" : addedWeapon.DisplayName;
            WeaponLoadoutSurgeActivationCount++;
            WeaponLoadoutSurgePulseHitCount += hitCount;
            LastWeaponLoadoutSurgeFeedbackLabel = $"{name} Arsenal Surge: {_port.WeaponCount}/{_build.MaxWeaponSlots} weapons, {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastWeaponLoadoutSurgeFeedbackLabel += $", rush {WeaponLoadoutSurgeRemainingSeconds:0.#}s";
            }

            _port.ShowFeedback(LastWeaponLoadoutSurgeFeedbackLabel, new Color(0.45f, 1f, 0.62f));
            _port.PlayPulse(Mathf.Clamp(34 + hitCount * 5, 42, 86), false);
        }

        public void TryTriggerPassiveLoadoutSurge(RunUpgradeDefinition addedPassive)
        {
            if (_passiveLoadoutSurgeUsed || _build.ActivePassiveCount < _build.MaxPassiveSlots)
            {
                return;
            }

            _passiveLoadoutSurgeUsed = true;

            float duration = Mathf.Max(0f, _port.Tuning.PassiveLoadoutSurgeDurationSeconds);
            if (duration > 0f)
            {
                _passiveLoadoutSurgeTimer = Mathf.Max(_passiveLoadoutSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, _port.Tuning.PassiveLoadoutSurgePulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.PassiveLoadoutSurgePulseDamage);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(_port.PlayerPosition, radius, damage, "survivors.passive-loadout.harmony-surge") : 0;

            string name = addedPassive == null ? "Passive" : _build.ResolveUpgradeDisplayName(addedPassive.Id);
            PassiveLoadoutSurgeActivationCount++;
            PassiveLoadoutSurgePulseHitCount += hitCount;
            LastPassiveLoadoutSurgeFeedbackLabel = $"{name} Harmony Surge: {_build.ActivePassiveCount}/{_build.MaxPassiveSlots} passives, {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastPassiveLoadoutSurgeFeedbackLabel += $", rush {PassiveLoadoutSurgeRemainingSeconds:0.#}s";
            }

            _port.ShowFeedback(LastPassiveLoadoutSurgeFeedbackLabel, new Color(0.58f, 0.92f, 1f));
            _port.PlayPulse(Mathf.Clamp(32 + hitCount * 5, 40, 82), false);
        }

        public void TriggerBossRelicSurge(SurvivorsRelicDefinition relic)
        {
            float duration = Mathf.Max(0f, _port.Tuning.BossRelicSurgeDurationSeconds);
            if (duration > 0f)
            {
                _bossRelicSurgeTimer = Mathf.Max(_bossRelicSurgeTimer, duration);
            }

            float radius = Mathf.Max(0f, _port.Tuning.BossRelicSurgeRadius);
            float damage = Mathf.Max(0f, _port.Tuning.BossRelicSurgeDamage);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(_port.PlayerPosition, radius, damage, "survivors.relic.surge") : 0;

            BossRelicSurgeCount++;
            BossRelicSurgeHitCount += hitCount;
            string name = relic == null || string.IsNullOrWhiteSpace(relic.DisplayName) ? "Boss Relic" : relic.DisplayName;
            LastBossRelicSurgeFeedbackLabel = $"{name} Surge: {hitCount} enemies hit";
            if (duration > 0f)
            {
                LastBossRelicSurgeFeedbackLabel += $", rush {BossRelicSurgeRemainingSeconds:0.#}s";
            }

            _port.ShowFeedback(LastBossRelicSurgeFeedbackLabel, _port.RelicAccent(relic));
            _port.PlayPulse(Mathf.Clamp(42 + hitCount * 4, 48, 88), true);
        }

        public void TickWeaponLoadoutSurge(float deltaTime)
        {
            if (_weaponLoadoutSurgeTimer <= 0f)
            {
                return;
            }

            _weaponLoadoutSurgeTimer = Mathf.Max(0f, _weaponLoadoutSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        public void TickPassiveLoadoutSurge(float deltaTime)
        {
            if (_passiveLoadoutSurgeTimer <= 0f)
            {
                return;
            }

            _passiveLoadoutSurgeTimer = Mathf.Max(0f, _passiveLoadoutSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        public void TickBossRelicSurge(float deltaTime)
        {
            if (_bossRelicSurgeTimer <= 0f)
            {
                return;
            }

            _bossRelicSurgeTimer = Mathf.Max(0f, _bossRelicSurgeTimer - Mathf.Max(0f, deltaTime));
        }

        public void TickEvolutionChainSurge(float deltaTime)
        {
            if (_evolutionChainSurgeTimer <= 0f)
            {
                return;
            }

            _evolutionChainSurgeTimer = Mathf.Max(0f, _evolutionChainSurgeTimer - Mathf.Max(0f, deltaTime));
        }
    }
}
