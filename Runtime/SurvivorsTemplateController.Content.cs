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
    // Authored content, pacing and theme configuration compatibility bindings.
    public sealed partial class SurvivorsTemplateController
    {
        public bool ConfigureAuthoredContent(TextAsset enemyLibrary, TextAsset runFlowLibrary, TextAsset rewardLibrary) => ContentBinding.ConfigureAuthoredContent(enemyLibrary, runFlowLibrary, rewardLibrary);

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary) => ContentBinding.ConfigureAuthoredContent(weaponLibrary, upgradeLibrary, relicLibrary, classLibrary, progressionLibrary, enemyLibrary, runFlowLibrary, rewardLibrary);

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy) => ContentBinding.ConfigureAuthoredContent(weaponLibrary, upgradeLibrary, relicLibrary, classLibrary, progressionLibrary, enemyLibrary, runFlowLibrary, rewardLibrary, bindingPolicy);

        public bool ConfigureAuthoredContentJson(string enemyJson, string runFlowJson, string rewardJson) => ContentBinding.ConfigureAuthoredContentJson(enemyJson, runFlowJson, rewardJson);

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson) => ContentBinding.ConfigureAuthoredContentJson(weaponJson, upgradeJson, relicJson, classJson, progressionJson, enemyJson, runFlowJson, rewardJson);

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy) => ContentBinding.ConfigureAuthoredContentJson(weaponJson, upgradeJson, relicJson, classJson, progressionJson, enemyJson, runFlowJson, rewardJson, bindingPolicy);

        public bool ConfigureStrictSampleContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset pickupLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary,
            TextAsset defaultThemeLibrary,
            TextAsset alternateThemeLibrary) => ContentBinding.ConfigureStrictSampleContent(weaponLibrary, upgradeLibrary, relicLibrary, classLibrary, progressionLibrary, enemyLibrary, pickupLibrary, runFlowLibrary, rewardLibrary, defaultThemeLibrary, alternateThemeLibrary);

        private SurvivorsMetaProgressionDefinition ResolveMetaProgressionDefinition() => RuntimeContent.ResolveMetaProgressionDefinition();

        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateWeaponArchetypeDefinitions(SurvivorsTemplateTuning resolved) => RuntimeContent.CreateWeaponArchetypeDefinitions(resolved);

        private IReadOnlyList<SurvivorsRelicDefinition> CreateRelicDefinitions() => RuntimeContent.CreateRelicDefinitions();

        private SurvivorsClassLibraryDefinition CreateClassLibraryDefinition() => RuntimeContent.CreateClassLibraryDefinition();

        private IReadOnlyList<SurvivorsClassUpgradeGateDefinition> CreateClassUpgradeGates() => RuntimeContent.CreateClassUpgradeGates();

        private RunUpgradeCatalog CreateBaseRunUpgradeCatalog() => RuntimeContent.CreateBaseRunUpgradeCatalog();

        private IReadOnlyList<SurvivorsRunUpgradeMetadata> CreateRunUpgradeMetadata() => RuntimeContent.CreateRunUpgradeMetadata();

        private SurvivorsRunFlowDefinition CreateRunFlowDefinition(SurvivorsTemplateTuning resolved) => RuntimeContent.CreateRunFlowDefinition(resolved);

        private SurvivorsTemplateTuning CreateConfiguredTuning(SurvivorsPacingProfile profile) => RuntimeContent.CreateConfiguredTuning(profile);

        private SurvivorsContentBinding ContentBinding => _contentBinding ?? (_contentBinding = new SurvivorsContentBinding(this));

        private SurvivorsRuntimeContentResolver RuntimeContent => _runtimeContent ?? (_runtimeContent = new SurvivorsRuntimeContentResolver(ContentBinding));

        bool ISurvivorsContentBindingPort.RunStarted => _runSession.Started;

        void ISurvivorsContentBindingPort.RefreshConfiguredTuning() => tuning = CreateConfiguredTuning(pacingProfile);

        void ISurvivorsContentBindingPort.ReleaseProfile() => ReleaseMetaProgressionService();

        bool ISurvivorsContentBindingPort.ConfigureUiThemes(TextAsset primary, TextAsset alternate) => ConfigureUiThemes(primary, alternate);

        public bool ConfigureUiTheme(TextAsset themeLibrary) => UiThemeSelection.ConfigureUiTheme(themeLibrary);

        public bool ConfigureUiThemeJson(string themeJson) => UiThemeSelection.ConfigureUiThemeJson(themeJson);

        public bool ConfigureUiThemes(TextAsset defaultThemeLibrary, TextAsset alternateThemeLibrary) => UiThemeSelection.ConfigureUiThemes(defaultThemeLibrary, alternateThemeLibrary);

        public bool ConfigureAdditionalUiThemeJson(string themeJson) => UiThemeSelection.ConfigureAdditionalUiThemeJson(themeJson);

        private bool SelectUiTheme(int index) => UiThemeSelection.SelectUiTheme(index);

        private void EnsureUiTheme() => UiThemeSelection.EnsureUiTheme();

        private SurvivorsUiThemeSelection UiThemeSelection => _uiThemeSelection ?? (_uiThemeSelection = new SurvivorsUiThemeSelection(this));

        SurvivorsUiTheme ISurvivorsUiThemeSelectionPort.SerializedTheme { get => uiTheme; set => uiTheme = value; }

        void ISurvivorsUiThemeSelectionPort.ResetHudStyles() => ResetHudStyles();

        void ISurvivorsUiThemeSelectionPort.ApplyWorldPresentation() => ApplyWorldPresentation();

        void ISurvivorsUiThemeSelectionPort.PlayThemeSelectionAudio() => PlayAudioEvent(AudioEventUiSelect, _pickupClip, 0.05f);

        public bool IsHumanPlaytestPacing => CurrentPacingProfile == SurvivorsPacingProfile.HumanPlaytest;

        public string CurrentUiThemeName => ActiveUiTheme.themeName;

        public IReadOnlyList<SurvivorsUiTheme> AvailableUiThemesForTest => UiThemeSelection.AvailableThemes;

        public int SelectedUiThemeIndex => UiThemeSelection.SelectedIndex;

        public SurvivorsTemplateTuning CurrentTuning => tuning ?? (tuning = CreateConfiguredTuning(pacingProfile));

        public bool IsAuthoredContentBound => ContentBinding.IsAuthoredContentBound;

        public bool IsStrictAuthoredSample => ContentBinding.IsStrictAuthoredSample;

        public bool IsFallbackContentActive => ContentBinding.IsFallbackContentActive;

        public bool CanStartConfiguredRun => ContentBinding.CanStartConfiguredRun;

        public bool IsUsingAuthoredRunFlow => RuntimeContent.IsUsingAuthoredRunFlow;

        public string AuthoredContentStatus => ContentBinding.AuthoredContentStatus;

        public bool SelectUiThemeForTest(int index)
        {
            return SelectUiTheme(index);
        }

        public void ConfigureMetaPersistenceForTest(IPersistenceService persistence, SaveSlotId slotId)
        {
            _profileSession.ConfigureBorrowedPersistence(persistence, slotId);
        }

        private void ApplyPacingProfile(SurvivorsPacingProfile profile, bool restartRun)
        {
            tuning = CreateConfiguredTuning(profile);
            pacingProfile = tuning.PacingProfile;
            if (restartRun)
            {
                RestartRun();
            }
        }

        private SurvivorsUiTheme ActiveUiTheme => UiThemeSelection.ActiveTheme;
    }
}
