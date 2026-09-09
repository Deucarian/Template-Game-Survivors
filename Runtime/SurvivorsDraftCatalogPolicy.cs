using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Read-only eligible pools and evolution-primer queries over the authoritative run build.</summary>
    internal sealed class SurvivorsDraftCatalogPolicy
    {
        private readonly SurvivorsRunBuildState RunBuild;
        private readonly SurvivorsDraftRarityPolicy DraftRarity;
        private readonly Func<SurvivorsTemplateTuning> _tuning;
        private SurvivorsTemplateTuning CurrentTuning => _tuning();
        public SurvivorsDraftCatalogPolicy(SurvivorsRunBuildState build, SurvivorsDraftRarityPolicy rarity, Func<SurvivorsTemplateTuning> tuning)
        {
            RunBuild = build ?? throw new ArgumentNullException(nameof(build));
            DraftRarity = rarity ?? throw new ArgumentNullException(nameof(rarity));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        }
        public RunUpgradeCatalog CreateEligibleDraftCatalog(DraftRarityProfile profile)
        {
            return CreateEligibleDraftCatalog(profile, RunUpgradeRarity.Common, requireMinimumRarity: false);
        }

        public RunUpgradeCatalog CreateEligibleDraftCatalog(
            DraftRarityProfile profile,
            RunUpgradeRarity minimumRarity,
            bool requireMinimumRarity)
        {
            if (RunBuild.Catalog == null)
            {
                return null;
            }

            var eligible = new List<RunUpgradeDefinition>(RunBuild.Catalog.Definitions.Count);
            for (int i = 0; i < RunBuild.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = RunBuild.Catalog.Definitions[i];
                if (definition != null &&
                    (!requireMinimumRarity || definition.Rarity >= minimumRarity) &&
                    RunBuild.IsUpgradeEligibleForCurrentBuild(definition))
                {
                    eligible.Add(definition);
                }
            }

            return DraftRarity.CreateWeightedDraftCatalog(eligible, profile);
        }

        public RunUpgradeCatalog CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole role, bool requireEvolutionChoice)
        {
            if (RunBuild.Catalog == null)
            {
                return null;
            }

            RunUpgradeRarity minimumRarity = role == SurvivorsEnemyRole.Boss ? RunUpgradeRarity.Rare : RunUpgradeRarity.Uncommon;
            var allEligible = new List<RunUpgradeDefinition>(RunBuild.Catalog.Definitions.Count);
            var preferred = new List<RunUpgradeDefinition>(RunBuild.Catalog.Definitions.Count);
            for (int i = 0; i < RunBuild.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = RunBuild.Catalog.Definitions[i];
                if (definition == null || !RunBuild.IsUpgradeEligibleForCurrentBuild(definition))
                {
                    continue;
                }

                allEligible.Add(definition);
                if (definition.Rarity >= minimumRarity || IsEvolutionUpgrade(definition))
                {
                    preferred.Add(definition);
                }
            }

            if (requireEvolutionChoice && CountEvolutionChoices(preferred) <= 0)
            {
                return null;
            }

            List<RunUpgradeDefinition> selected = preferred.Count >= CurrentTuning.DraftChoiceCount ? preferred : allEligible;
            DraftRarityProfile profile = role == SurvivorsEnemyRole.Boss ? DraftRarityProfile.Boss : DraftRarityProfile.Elite;
            return DraftRarity.CreateWeightedDraftCatalog(selected, profile);
        }

        public bool IsEvolutionUpgrade(RunUpgradeDefinition definition)
        {
            return definition != null &&
                RunBuild.TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata) &&
                metadata.IsEvolution;
        }

        public bool TryResolveEvolutionMissingPassive(RunUpgradeDefinition evolution, out RunUpgradeDefinition passive)
        {
            passive = null;
            if (evolution == null ||
                RunBuild.State == null ||
                !RunBuild.TryGetUpgradeMetadata(evolution.Id.Value, out SurvivorsRunUpgradeMetadata evolutionMetadata) ||
                !evolutionMetadata.IsEvolution ||
                string.IsNullOrWhiteSpace(evolutionMetadata.RequiredUpgradeId) ||
                string.IsNullOrWhiteSpace(evolutionMetadata.RequiredPassiveUpgradeId) ||
                RunBuild.State.GetRank(evolution.Id) > 0)
            {
                return false;
            }

            int requiredRank = RunBuild.ResolveRequiredUpgradeRank(evolutionMetadata);
            if (RunBuild.State.GetRank(new RunUpgradeId(evolutionMetadata.RequiredUpgradeId)) < requiredRank)
            {
                return false;
            }

            var requiredPassiveId = new RunUpgradeId(evolutionMetadata.RequiredPassiveUpgradeId);
            if (RunBuild.State.GetRank(requiredPassiveId) > 0 ||
                !RunBuild.TryGetRunUpgrade(evolutionMetadata.RequiredPassiveUpgradeId, out passive) ||
                !RunBuild.IsUpgradeEligibleForCurrentBuild(passive) ||
                !RunBuild.TryGetUpgradeMetadata(passive.Id.Value, out SurvivorsRunUpgradeMetadata passiveMetadata) ||
                passiveMetadata.Category != SurvivorsRunUpgradeCategory.Passive ||
                !passiveMetadata.UsesPassiveSlot)
            {
                passive = null;
                return false;
            }

            return true;
        }

        public int CountEvolutionChoices(IReadOnlyList<RunUpgradeDefinition> definitions)
        {
            if (definitions == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < definitions.Count; i++)
            {
                if (IsEvolutionUpgrade(definitions[i]))
                {
                    count++;
                }
            }

            return count;
        }

        public bool DraftContainsEvolution(RunUpgradeDraft draft)
        {
            return draft != null && CountEvolutionChoices(draft.Choices) > 0;
        }
    }
}
