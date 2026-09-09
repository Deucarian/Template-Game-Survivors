using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns authored binding and strict readiness; parsers, UI and storage retain their own lifetimes.</summary>
    internal sealed class SurvivorsContentBinding
    {
        private readonly ISurvivorsContentBindingPort _port;
        private SurvivorsAuthoredContentDefinition _authoredContent;
        private SurvivorsAuthoredContentBindingPolicy _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.AllowFallbacks;
        private bool _strictSampleContentReady;
        private string _authoredContentStatus = "Fallback content active for an unbound host.";

        public SurvivorsContentBinding(ISurvivorsContentBindingPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public SurvivorsAuthoredContentDefinition Definition => _authoredContent;
        public bool IsAuthoredContentBound => _authoredContent != null;
        public bool IsStrictAuthoredSample => _authoredBindingPolicy == SurvivorsAuthoredContentBindingPolicy.StrictSample;
        public bool IsFallbackContentActive => !IsStrictAuthoredSample && (_authoredContent == null || _authoredContent.UsesBuiltInFallbacks);
        public bool CanStartConfiguredRun => !IsStrictAuthoredSample || (_strictSampleContentReady && _authoredContent != null);
        public string AuthoredContentStatus => _authoredContentStatus;

        public bool ConfigureAuthoredContent(TextAsset enemyLibrary, TextAsset runFlowLibrary, TextAsset rewardLibrary)
        {
            _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.AllowFallbacks;
            _strictSampleContentReady = false;
            return ConfigureAuthoredContentJson(
                enemyLibrary == null ? null : enemyLibrary.text,
                runFlowLibrary == null ? null : runFlowLibrary.text,
                rewardLibrary == null ? null : rewardLibrary.text);
        }

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary)
        {
            return ConfigureAuthoredContent(
                weaponLibrary,
                upgradeLibrary,
                relicLibrary,
                classLibrary,
                progressionLibrary,
                enemyLibrary,
                runFlowLibrary,
                rewardLibrary,
                SurvivorsAuthoredContentBindingPolicy.StrictSample);
        }

        public bool ConfigureAuthoredContent(
            TextAsset weaponLibrary,
            TextAsset upgradeLibrary,
            TextAsset relicLibrary,
            TextAsset classLibrary,
            TextAsset progressionLibrary,
            TextAsset enemyLibrary,
            TextAsset runFlowLibrary,
            TextAsset rewardLibrary,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy)
        {
            return ConfigureAuthoredContentJson(
                weaponLibrary == null ? null : weaponLibrary.text,
                upgradeLibrary == null ? null : upgradeLibrary.text,
                relicLibrary == null ? null : relicLibrary.text,
                classLibrary == null ? null : classLibrary.text,
                progressionLibrary == null ? null : progressionLibrary.text,
                enemyLibrary == null ? null : enemyLibrary.text,
                runFlowLibrary == null ? null : runFlowLibrary.text,
                rewardLibrary == null ? null : rewardLibrary.text,
                bindingPolicy);
        }

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
            TextAsset alternateThemeLibrary)
        {
            _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.StrictSample;
            _strictSampleContentReady = false;
            TextAsset[] requiredAssets =
            {
                weaponLibrary,
                upgradeLibrary,
                relicLibrary,
                classLibrary,
                progressionLibrary,
                enemyLibrary,
                pickupLibrary,
                runFlowLibrary,
                rewardLibrary,
                defaultThemeLibrary,
                alternateThemeLibrary
            };
            for (int i = 0; i < requiredAssets.Length; i++)
            {
                if (requiredAssets[i] == null)
                {
                    return SetAuthoredBindingFailure($"Strict authored sample is missing required TextAsset at slot {i}.");
                }
            }

            SurvivorsContentValidationResult validation = SurvivorsContentValidator.ValidateSampleJson(
                weaponLibrary.text,
                upgradeLibrary.text,
                enemyLibrary.text,
                rewardLibrary.text,
                relicLibrary.text,
                classLibrary.text,
                progressionLibrary.text,
                pickupLibrary.text,
                runFlowLibrary.text,
                defaultThemeLibrary.text,
                alternateThemeLibrary.text);
            if (!validation.Succeeded)
            {
                return SetAuthoredBindingFailure("Strict authored sample validation failed: " + string.Join("; ", validation.Errors));
            }

            if (!ConfigureAuthoredContent(
                weaponLibrary,
                upgradeLibrary,
                relicLibrary,
                classLibrary,
                progressionLibrary,
                enemyLibrary,
                runFlowLibrary,
                rewardLibrary,
                SurvivorsAuthoredContentBindingPolicy.StrictSample))
            {
                return false;
            }

            if (!_port.ConfigureUiThemes(defaultThemeLibrary, alternateThemeLibrary))
            {
                return SetAuthoredBindingFailure("Strict authored sample UI themes failed to bind.");
            }

            _strictSampleContentReady = true;
            _authoredContentStatus = "Strict authored Survivors sample bound: " + _authoredContent.SourceSummary;
            return true;
        }

        public bool ConfigureAuthoredContentJson(string enemyJson, string runFlowJson, string rewardJson)
        {
            _authoredBindingPolicy = SurvivorsAuthoredContentBindingPolicy.AllowFallbacks;
            _strictSampleContentReady = false;
            if (!SurvivorsAuthoredContentDefinition.TryCreate(
                enemyJson,
                runFlowJson,
                rewardJson,
                out SurvivorsAuthoredContentDefinition definition,
                out string error))
            {
                _authoredContent = null;
                _authoredContentStatus = string.IsNullOrWhiteSpace(error)
                    ? "Fallback policy active after authored Survivors content failed to bind."
                    : "Fallback policy active after authored Survivors content failed to bind: " + error;
                if (!_port.RunStarted)
                {
                    _port.RefreshConfiguredTuning();
                }

                _port.ReleaseProfile();
                return false;
            }

            _authoredContent = definition;
            _authoredContentStatus = "Authored Survivors content bound with fallback policy active: " + definition.SourceSummary;
            if (!_port.RunStarted)
            {
                _port.RefreshConfiguredTuning();
            }

            _port.ReleaseProfile();
            return true;
        }

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson)
        {
            return ConfigureAuthoredContentJson(
                weaponJson,
                upgradeJson,
                relicJson,
                classJson,
                progressionJson,
                enemyJson,
                runFlowJson,
                rewardJson,
                SurvivorsAuthoredContentBindingPolicy.StrictSample);
        }

        public bool ConfigureAuthoredContentJson(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy)
        {
            _authoredBindingPolicy = bindingPolicy;
            _strictSampleContentReady = false;
            if (!SurvivorsAuthoredContentDefinition.TryCreate(
                weaponJson,
                upgradeJson,
                relicJson,
                classJson,
                progressionJson,
                enemyJson,
                runFlowJson,
                rewardJson,
                bindingPolicy,
                out SurvivorsAuthoredContentDefinition definition,
                out string error))
            {
                _authoredContent = null;
                _authoredContentStatus = string.IsNullOrWhiteSpace(error)
                    ? "Authored Survivors content failed to bind."
                    : "Authored Survivors content failed to bind: " + error;
                if (!_port.RunStarted)
                {
                    _port.RefreshConfiguredTuning();
                }

                _port.ReleaseProfile();
                return false;
            }

            _authoredContent = definition;
            _strictSampleContentReady = bindingPolicy == SurvivorsAuthoredContentBindingPolicy.StrictSample;
            _authoredContentStatus = definition.IsStrictSample
                ? "Strict authored Survivors content bound: " + definition.SourceSummary
                : "Authored Survivors content bound with fallback policy: " + definition.SourceSummary;
            if (!_port.RunStarted)
            {
                _port.RefreshConfiguredTuning();
            }

            _port.ReleaseProfile();
            return true;
        }

        private bool SetAuthoredBindingFailure(string message)
        {
            _authoredContent = null;
            _strictSampleContentReady = false;
            _authoredContentStatus = string.IsNullOrWhiteSpace(message)
                ? "Strict authored Survivors content failed to bind."
                : message;
            if (!_port.RunStarted)
            {
                _port.RefreshConfiguredTuning();
            }

            _port.ReleaseProfile();
            return false;
        }

    }
}
