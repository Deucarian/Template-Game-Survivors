using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Resolves the currently bound content without caching another copy of its definitions.</summary>
    internal sealed class SurvivorsRuntimeContentResolver
    {
        private readonly SurvivorsContentBinding _binding;

        public SurvivorsRuntimeContentResolver(SurvivorsContentBinding binding)
        {
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
        }

        private SurvivorsAuthoredContentDefinition Content => _binding.Definition;
        public bool IsUsingAuthoredRunFlow { get; private set; }

        public SurvivorsMetaProgressionDefinition ResolveMetaProgressionDefinition()
        {
            return Content != null && Content.MetaProgressionDefinition != null
                ? Content.MetaProgressionDefinition
                : BasicSurvivorsGame.CreateMetaProgressionDefinition();
        }

        public IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateWeaponArchetypeDefinitions(SurvivorsTemplateTuning resolved)
        {
            return Content != null && Content.HasWeaponDefinitions
                ? Content.WeaponDefinitions
                : BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(resolved);
        }

        public IReadOnlyList<SurvivorsRelicDefinition> CreateRelicDefinitions()
        {
            return Content != null && Content.HasRelicDefinitions
                ? Content.RelicDefinitions
                : BasicSurvivorsGame.CreateRelicDefinitions();
        }

        public SurvivorsClassLibraryDefinition CreateClassLibraryDefinition()
        {
            return Content != null && Content.HasClassLibrary
                ? Content.ClassLibrary
                : BasicSurvivorsGame.CreateClassLibraryDefinition();
        }

        public IReadOnlyList<SurvivorsClassUpgradeGateDefinition> CreateClassUpgradeGates()
        {
            return Content != null && Content.HasClassUpgradeGates
                ? Content.ClassUpgradeGates
                : BasicSurvivorsGame.CreateClassUpgradeGates();
        }

        public RunUpgradeCatalog CreateBaseRunUpgradeCatalog()
        {
            return Content != null && Content.HasRunUpgradeCatalog
                ? Content.RunUpgradeCatalog
                : BasicSurvivorsGame.CreateRunUpgradeCatalog();
        }

        public IReadOnlyList<SurvivorsRunUpgradeMetadata> CreateRunUpgradeMetadata()
        {
            return Content != null && Content.HasRunUpgradeMetadata
                ? Content.RunUpgradeMetadata
                : BasicSurvivorsGame.CreateRunUpgradeMetadata();
        }

        public SurvivorsRunFlowDefinition CreateRunFlowDefinition(SurvivorsTemplateTuning resolved)
        {
            if (Content != null)
            {
                IsUsingAuthoredRunFlow = true;
                return Content.CreateRunFlowDefinition(resolved);
            }

            IsUsingAuthoredRunFlow = false;
            return BasicSurvivorsGame.CreateRunFlowDefinition(resolved);
        }

        public SurvivorsTemplateTuning CreateConfiguredTuning(SurvivorsPacingProfile profile)
        {
            SurvivorsTemplateTuning configured = Content == null
                ? BasicSurvivorsGame.CreateTuning(profile)
                : Content.CreateTuning(profile);
            configured.PacingProfile = profile;
            return configured;
        }
    }
}
