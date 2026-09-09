using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Projects the live build into ordered HUD rows without owning ranks, loadout or relics.</summary>
    internal sealed class SurvivorsBuildHudModel
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly SurvivorsBuildContentLabels _labels;
        private readonly Func<string> _evolutionObjective;

        public SurvivorsBuildHudModel(SurvivorsRunBuildState build, SurvivorsBuildContentLabels labels, Func<string> evolutionObjective)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _labels = labels ?? throw new ArgumentNullException(nameof(labels));
            _evolutionObjective = evolutionObjective ?? throw new ArgumentNullException(nameof(evolutionObjective));
        }

        public IReadOnlyList<string> BuildLines(in SurvivorsBuildHudValues values)
        {
            var lines = new List<string>(18)
            {
                $"Weapons {values.ActiveWeaponCount}/{_build.MaxWeaponSlots}"
            };

            AppendWeaponBuildHudLines(lines, values.ActiveWeaponIds);
            lines.Add($"Passives {_build.ActivePassiveCount}/{_build.MaxPassiveSlots}");
            AppendPassiveBuildHudLines(lines);
            lines.Add($"Evolutions {_build.EvolutionIds.Count}");
            AppendEvolutionBuildHudLines(lines);
            lines.Add($"Pickup range {values.CurrentPickupAttractRange:0.#}   Pull {values.CurrentPickupAttractionSpeed:0.#}   Pulse {values.PickupPulseLabel}");
            lines.Add("Relics " + values.SelectedRelicLabel);
            return lines;
        }

        private void AppendWeaponBuildHudLines(List<string> lines, IReadOnlyList<string> weaponIds)
        {
            if (lines == null)
            {
                return;
            }

            if (weaponIds.Count == 0)
            {
                lines.Add("  none");
                return;
            }

            for (int i = 0; i < weaponIds.Count; i++)
            {
                string weaponId = weaponIds[i];
                var fragments = new List<string>(4);
                int hidden = AppendBuildRankFragments(
                    fragments,
                    metadata => string.Equals(metadata.AffectedContentId, weaponId, StringComparison.Ordinal) &&
                        (metadata.Category == SurvivorsRunUpgradeCategory.Weapon ||
                            metadata.Category == SurvivorsRunUpgradeCategory.WeaponUpgrade ||
                            metadata.Category == SurvivorsRunUpgradeCategory.Mutation),
                    3);

                if (hidden > 0)
                {
                    fragments.Add("+" + hidden.ToString());
                }

                string weaponName = _labels.ResolveWeaponBuildDisplayName(weaponId);
                lines.Add(fragments.Count == 0
                    ? "  " + weaponName
                    : "  " + weaponName + ": " + string.Join(", ", fragments));
            }
        }

        private void AppendPassiveBuildHudLines(List<string> lines)
        {
            if (lines == null)
            {
                return;
            }

            int added = 0;
            AppendSelectedBuildRanks(
                (definition, metadata, rank) =>
                {
                    if (metadata.Category != SurvivorsRunUpgradeCategory.Passive)
                    {
                        return;
                    }

                    added++;
                    lines.Add("  " + _labels.FormatPassiveBuildHudLine(definition, metadata, rank));
                });

            if (added == 0)
            {
                lines.Add("  none");
            }
        }

        private void AppendEvolutionBuildHudLines(List<string> lines)
        {
            if (lines == null)
            {
                return;
            }

            int added = 0;
            AppendSelectedBuildRanks(
                (definition, metadata, rank) =>
                {
                    if (metadata.Category != SurvivorsRunUpgradeCategory.Evolution)
                    {
                        return;
                    }

                    added++;
                    lines.Add("  " + _labels.FormatBuildRankFragment(definition, rank));
                });

            if (added > 0)
            {
                return;
            }

            string objective = _evolutionObjective();
            lines.Add(string.IsNullOrWhiteSpace(objective) ? "  none" : "  " + objective);
        }

        private int AppendBuildRankFragments(List<string> fragments, Func<SurvivorsRunUpgradeMetadata, bool> matches, int maxFragments)
        {
            int hidden = 0;
            AppendSelectedBuildRanks(
                (definition, metadata, rank) =>
                {
                    if (matches == null || !matches(metadata))
                    {
                        return;
                    }

                    if (fragments != null && fragments.Count < maxFragments)
                    {
                        fragments.Add(_labels.FormatBuildRankFragment(definition, rank));
                    }
                    else
                    {
                        hidden++;
                    }
                });

            return hidden;
        }

        private void AppendSelectedBuildRanks(Action<RunUpgradeDefinition, SurvivorsRunUpgradeMetadata, int> append)
        {
            if (append == null || _build.Catalog == null || _build.State == null)
            {
                return;
            }

            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _build.Catalog.Definitions[i];
                int rank = definition == null ? 0 : _build.State.GetRank(definition.Id);
                if (rank <= 0 || !_build.TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
                {
                    continue;
                }

                append(definition, metadata, rank);
            }
        }
    }
}
