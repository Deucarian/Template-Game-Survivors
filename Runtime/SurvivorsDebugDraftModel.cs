using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsDebugDraftPort
    {
        RunUpgradeDraft CurrentDraft { get; }
        SurvivorsRelicDraft CurrentRelicDraft { get; }
        string RewardOverlayTitle { get; }
    }

    /// <summary>Describes available drafts over the existing build and draft authorities.</summary>
    internal sealed class SurvivorsDebugDraftModel
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly SurvivorsDraftCatalogPolicy _catalogs;
        private readonly SurvivorsDebugUpgradeFormatter _formatter;
        private readonly SurvivorsBuildContentLabels _labels;
        private readonly ISurvivorsDebugDraftPort _port;
        public SurvivorsDebugDraftModel(SurvivorsRunBuildState build, SurvivorsDraftCatalogPolicy catalogs,
            SurvivorsDebugUpgradeFormatter formatter, SurvivorsBuildContentLabels labels, ISurvivorsDebugDraftPort port)
        { _build = build; _catalogs = catalogs; _formatter = formatter; _labels = labels; _port = port; }

        public IReadOnlyList<string> DebugDescribeEligibleEvolutionPool()
        {
            if (_build.Catalog == null)
            {
                return Array.Empty<string>();
            }

            var lines = new List<string>();
            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _build.Catalog.Definitions[i];
                if (definition != null && _catalogs.IsEvolutionUpgrade(definition) && _build.IsUpgradeEligibleForCurrentBuild(definition))
                {
                    lines.Add(_formatter.FormatDebugUpgradeLine(-1, definition));
                }
            }

            if (lines.Count == 0)
            {
                lines.Add("No eligible evolutions yet. Max a weapon path and own its matching passive.");
            }

            return lines;
        }

        public IReadOnlyList<string> DebugDescribeCurrentDraftPool()
        {
            var lines = new List<string>();
            if (_port.CurrentDraft != null && _port.CurrentDraft.Choices.Count > 0)
            {
                lines.Add(_port.RewardOverlayTitle);
                for (int i = 0; i < _port.CurrentDraft.Choices.Count; i++)
                {
                    lines.Add(_formatter.FormatDebugUpgradeLine(i, _port.CurrentDraft.Choices[i]));
                }
            }

            if (_port.CurrentRelicDraft != null && _port.CurrentRelicDraft.Choices.Count > 0)
            {
                lines.Add("Boss Relics");
                for (int i = 0; i < _port.CurrentRelicDraft.Choices.Count; i++)
                {
                    SurvivorsRelicDefinition relic = _port.CurrentRelicDraft.Choices[i];
                    if (relic == null)
                    {
                        lines.Add($"{i + 1}. Missing relic");
                    }
                    else
                    {
                        lines.Add($"{i + 1}. {relic.DisplayName} [{relic.EffectKind}] +{relic.Amount:0.##} {_labels.ShortWeaponName(relic.TargetId)}");
                    }
                }
            }

            if (lines.Count == 0)
            {
                lines.Add("No draft or reward pool is currently open.");
            }

            return lines;
        }
    }
}
