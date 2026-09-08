using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns goal/ready announcement history while eligibility remains in the live run build.</summary>
    internal sealed class SurvivorsEvolutionAnnouncements
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly SurvivorsDraftCatalogPolicy _catalogs;
        private readonly Action<RunUpgradeDefinition, RunUpgradeDefinition> _showGoal;
        private readonly Action<RunUpgradeDefinition> _showReady;
        private readonly HashSet<string> _goals = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _ready = new HashSet<string>(StringComparer.Ordinal);

        internal SurvivorsEvolutionAnnouncements(SurvivorsRunBuildState build, SurvivorsDraftCatalogPolicy catalogs,
            Action<RunUpgradeDefinition, RunUpgradeDefinition> showGoal, Action<RunUpgradeDefinition> showReady)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _catalogs = catalogs ?? throw new ArgumentNullException(nameof(catalogs));
            _showGoal = showGoal ?? throw new ArgumentNullException(nameof(showGoal));
            _showReady = showReady ?? throw new ArgumentNullException(nameof(showReady));
        }

        internal void Refresh()
        {
            if (_build.Catalog == null) return;
            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _build.Catalog.Definitions[i];
                if (definition == null || !_catalogs.IsEvolutionUpgrade(definition)) continue;

                string upgradeId = definition.Id.Value;
                if (_build.HasEvolution(upgradeId) || _ready.Contains(upgradeId)) continue;

                if (!_build.IsUpgradeEligibleForCurrentBuild(definition))
                {
                    if (!_goals.Contains(upgradeId) &&
                        _catalogs.TryResolveEvolutionMissingPassive(definition, out RunUpgradeDefinition missingPassive))
                    {
                        _goals.Add(upgradeId);
                        _showGoal(definition, missingPassive);
                    }
                    continue;
                }

                _ready.Add(upgradeId);
                _showReady(definition);
            }
        }

        internal void Reset()
        {
            _goals.Clear();
            _ready.Clear();
        }
    }
}
