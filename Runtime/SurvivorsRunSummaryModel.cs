using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns rendered result history and authored build summary text, never reward transactions.</summary>
    internal sealed class SurvivorsRunSummaryModel
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly SurvivorsBuildContentLabels _labels;
        private readonly List<string> _lines = new List<string>(8);
        public SurvivorsRunSummaryModel(SurvivorsRunBuildState build, SurvivorsBuildContentLabels labels)
        { _build = build ?? throw new ArgumentNullException(nameof(build)); _labels = labels ?? throw new ArgumentNullException(nameof(labels)); }
        public IReadOnlyList<string> Lines => _lines;
        public string Title { get; private set; } = string.Empty;
        public void Clear() { _lines.Clear(); Title = string.Empty; }

        public void Rebuild(in SurvivorsRunSummaryValues values)
        {
            _lines.Clear();
            string result = values.Outcome.Victory
                ? (values.Outcome.EndlessContinuationEnabled ? "Victory - Endless continuation available" : "Victory")
                : "Defeat";
            Title = values.Outcome.ThemeTitle + " - " + (values.Outcome.Victory ? "Victory" : "Defeat");
            _lines.Add("Run mode: " + values.Outcome.ModeDisplayName);
            _lines.Add("Result: " + result);
            _lines.Add($"Run time: {SurvivorsRunText.FormatRunTime(values.Outcome.RunTimeSeconds)} / target {SurvivorsRunText.FormatRunTime(values.Outcome.SurvivalVictoryTimeSeconds)}");
            _lines.Add($"Level reached: {values.Outcome.Level}   XP collected: {values.Outcome.ExperienceCollected}   Stored XP: {values.Outcome.Experience}/{values.Outcome.RequiredExperienceForNextLevel}");
            _lines.Add($"Kills: {values.Combat.KilledCount}   Elites: {values.Combat.EliteKilledCount}   Minibosses: {values.Combat.MinibossKilledCount}   Bosses: {values.Combat.BossKilledCount}");
            _lines.Add($"Rewards earned: +{values.Rewards.BloodShardsEarnedThisRun} {values.Rewards.CurrencyRewardLabel}, +{values.Rewards.LegacyExperienceEarnedThisRun} {values.Rewards.ProgressionRewardLabel}");
            _lines.Add($"Reward profile: target {SurvivorsRunText.FormatRunTime(values.Outcome.TargetDurationSeconds)}, multiplier x{values.Outcome.RunRewardMultiplier:0.##}");
            _lines.Add($"Meta bank: {values.Rewards.MetaBloodShards} {values.Rewards.CurrencyDisplayName}, {values.Rewards.LifetimeLegacyExperience} {values.Rewards.ProgressionDisplayName}");
            _lines.Add($"Damage taken: {values.Combat.DamageTakenThisRun:0.#}   Final health: {values.Combat.CurrentHealth:0.#}/{values.Combat.MaxHealth:0.#}");
            _lines.Add($"Weapons {values.Collection.ActiveWeaponCount}/{_build.MaxWeaponSlots}: {FormatActiveWeaponList(values.Collection.ActiveWeaponIds)}");
            _lines.Add($"Passives {_build.ActivePassiveCount}/{_build.MaxPassiveSlots}: {FormatRunSummaryUpgradeList(_build.PassiveIds, includeRanks: true)}");
            _lines.Add($"Evolutions {_build.EvolutionIds.Count}: {FormatRunSummaryUpgradeList(_build.EvolutionIds, includeRanks: false)}");
            _lines.Add($"Relics {values.Collection.SelectedRelicCount}/{values.Collection.TotalRelicCount}: {values.Collection.SelectedRelicLabel}");
            _lines.Add($"Pickup build: radius {values.Collection.CurrentPickupAttractRange:0.#}, magnet range {values.Collection.CurrentPickupAttractRange:0.#}, pull speed {values.Collection.CurrentPickupAttractionSpeed:0.#}, pulse {SurvivorsRunText.FormatMetricTime(values.Collection.CurrentPickupMagnetPulseIntervalSeconds)}, recalls {values.Collection.MagnetRecallCount}");
            _lines.Add("Top weapon by damage: not tracked yet");
            _lines.Add("Top weapon by kills: not tracked yet");
            _lines.Add("Best moment: " + (string.IsNullOrWhiteSpace(values.Combat.BestMomentLabel) ? ResolveFallbackBestMomentLabel(values) : values.Combat.BestMomentLabel));
            if (values.Collection.ClassUnlockRewardCount > 0)
            {
                _lines.Add("Class unlocked: " + values.Collection.UnlockedClassDisplayName);
            }
        }

        private static string ResolveFallbackBestMomentLabel(in SurvivorsRunSummaryValues values)
        {
            if (values.Combat.FirstEvolutionAcquiredTimeSeconds >= 0f)
            {
                return "First evolution at " + SurvivorsRunText.FormatRunTime(values.Combat.FirstEvolutionAcquiredTimeSeconds);
            }

            if (!string.IsNullOrWhiteSpace(values.Combat.HighestChosenRarityLabel))
            {
                return "Highest rarity chosen: " + values.Combat.HighestChosenRarityLabel;
            }

            if (values.Combat.BestKillStreak > 0)
            {
                return "Best streak " + values.Combat.BestKillStreak.ToString();
            }

            return "First run data captured";
        }

        public string FormatActiveWeaponList(IReadOnlyList<string> weaponIds)
        {
            if (weaponIds == null || weaponIds.Count == 0)
            {
                return "None";
            }

            var labels = new List<string>(weaponIds.Count);
            for (int i = 0; i < weaponIds.Count; i++)
            {
                labels.Add(_labels.ShortWeaponName(weaponIds[i]));
            }

            return string.Join(", ", labels);
        }

        public string FormatRunSummaryUpgradeList(IReadOnlyCollection<string> upgradeIds, bool includeRanks)
        {
            if (upgradeIds == null || upgradeIds.Count == 0 || _build.Catalog == null)
            {
                return "None yet";
            }

            var labels = new List<string>(upgradeIds.Count);
            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _build.Catalog.Definitions[i];
                if (definition == null || !System.Linq.Enumerable.Contains(upgradeIds, definition.Id.Value, StringComparer.Ordinal))
                {
                    continue;
                }

                string label = _build.ResolveUpgradeDisplayName(definition.Id);
                if (includeRanks && _build.State != null)
                {
                    int rank = Mathf.Max(1, _build.State.GetRank(definition.Id));
                    label += " " + rank.ToString() + "/" + definition.MaxRank.ToString();
                }

                labels.Add(label);
            }

            return labels.Count == 0 ? "None yet" : string.Join(", ", labels);
        }
    }
}
