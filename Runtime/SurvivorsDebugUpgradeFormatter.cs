using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Formats live ranks and authored upgrade/relic labels without applying selections.</summary>
    internal sealed class SurvivorsDebugUpgradeFormatter
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly SurvivorsBuildContentLabels _labels;
        private readonly SurvivorsDraftCardFactory _cards;
        public SurvivorsDebugUpgradeFormatter(SurvivorsRunBuildState build, SurvivorsBuildContentLabels labels, SurvivorsDraftCardFactory cards)
        { _build = build; _labels = labels; _cards = cards; }

        public void AppendSelectedUpgradeRankLines(List<string> lines)
        {
            if (lines == null || _build.Catalog == null || _build.State == null)
            {
                return;
            }

            bool addedHeader = false;
            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _build.Catalog.Definitions[i];
                int rank = definition == null ? 0 : _build.State.GetRank(definition.Id);
                if (rank <= 0)
                {
                    continue;
                }

                if (!addedHeader)
                {
                    lines.Add("Ranks");
                    addedHeader = true;
                }

                lines.Add("  " + FormatDebugRankLine(definition, rank));
            }
        }

        public string FormatDebugRankLine(RunUpgradeDefinition definition, int rank)
        {
            if (definition == null)
            {
                return "Missing upgrade";
            }

            string name = _build.ResolveUpgradeDisplayName(definition.Id);
            SurvivorsRunUpgradeCategory category = _build.ResolveCurrentUpgradeCategory(definition);
            string affected = _build.TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata)
                ? _labels.ShortWeaponName(metadata.AffectedContentId)
                : "Build";
            return $"{name} [{category}] rank {rank}/{definition.MaxRank} ({definition.Rarity}) - {affected}";
        }

        public string FormatDebugUpgradeLine(int index, RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return index >= 0 ? $"{index + 1}. Missing upgrade" : "Missing upgrade";
            }

            string prefix = index >= 0 ? $"{index + 1}. " : string.Empty;
            string name = _build.ResolveUpgradeDisplayName(definition.Id);
            SurvivorsRunUpgradeCategory category = _build.ResolveCurrentUpgradeCategory(definition);
            int currentRank = _build.State == null ? 0 : _build.State.GetRank(definition.Id);
            int nextRank = Mathf.Min(definition.MaxRank, currentRank + 1);
            string description = _build.TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata)
                ? metadata.Description
                : name;
            string affected = metadata == null ? "Build" : _labels.ShortWeaponName(metadata.AffectedContentId);
            return $"{prefix}{name} [{SurvivorsDraftCardFactory.FormatUpgradeCategoryLabel(category)}/{definition.Rarity}] rank {currentRank}->{nextRank}/{definition.MaxRank} - {affected}: {description}";
        }

        public string FormatUpgradeChoiceLabel(int index, RunUpgradeDefinition choice)
        {
            if (choice == null)
            {
                return (index + 1).ToString() + ". Missing Choice";
            }

            SurvivorsRunUpgradeCategory category = _build.ResolveCurrentUpgradeCategory(choice);
            string name = _build.ResolveUpgradeDisplayName(choice.Id);
            string affected = _cards.ResolveUpgradeAffectedLabel(choice);
            string description = _build.TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata)
                ? metadata.Description
                : name;
            int currentRank = _build.State == null ? 0 : _build.State.GetRank(choice.Id);
            int nextRank = Mathf.Min(choice.MaxRank, currentRank + 1);
            return $"{index + 1}. {choice.Rarity} {SurvivorsDraftCardFactory.FormatUpgradeCategoryLabel(category)}: {name}\n{affected}  Rank {currentRank}->{nextRank}/{choice.MaxRank} - {description}";
        }

        public string FormatRelicChoiceLabel(int index, SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return (index + 1).ToString() + ". Missing Relic";
            }

            return $"{index + 1}. Boss Relic: {relic.DisplayName}\n{_cards.FormatRelicEffectSummary(relic)}";
        }
    }
}
