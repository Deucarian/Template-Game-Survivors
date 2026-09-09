using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Read-only catalog-ordered evolution goals and eligible reward hints.</summary>
    internal sealed class SurvivorsEvolutionHudModel
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly SurvivorsDraftCatalogPolicy _catalogs;
        public SurvivorsEvolutionHudModel(SurvivorsRunBuildState build, SurvivorsDraftCatalogPolicy catalogs)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _catalogs = catalogs ?? throw new ArgumentNullException(nameof(catalogs));
        }

        public string Goal()
        {
            if (_build.Catalog == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition evolution = _build.Catalog.Definitions[i];
                if (_catalogs.TryResolveEvolutionMissingPassive(evolution, out RunUpgradeDefinition passive))
                {
                    return $"Goal {_build.ResolveUpgradeDisplayName(passive.Id)} -> {_build.ResolveUpgradeDisplayName(evolution.Id)}";
                }
            }

            return string.Empty;
        }

        public string Ready()
        {
            if (_build.Catalog == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition evolution = _build.Catalog.Definitions[i];
                if (evolution != null && _catalogs.IsEvolutionUpgrade(evolution) && _build.IsUpgradeEligibleForCurrentBuild(evolution))
                {
                    return $"Ready {_build.ResolveUpgradeDisplayName(evolution.Id)} -> elite/boss reward";
                }
            }

            return string.Empty;
        }

        public string Objective()
        {
            string goal = Goal();
            return string.IsNullOrWhiteSpace(goal) ? Ready() : goal;
        }
    }
}
