using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    internal sealed class SurvivorsContentBindingTestHost : ISurvivorsContentBindingPort, IDisposable
    {
        private readonly List<TextAsset> _ownedAssets = new List<TextAsset>();
        public readonly List<string> Events = new List<string>();
        public readonly SurvivorsContentBinding Binding;
        public readonly SurvivorsRuntimeContentResolver Resolver;
        public bool RunStarted { get; set; }
        public bool ThemesSucceed = true;
        public SurvivorsTemplateTuning RefreshedTuning;
        public SurvivorsAuthoredContentDefinition DefinitionAtRefresh;
        public TextAsset PrimaryTheme;
        public TextAsset AlternateTheme;

        public SurvivorsContentBindingTestHost()
        {
            Binding = new SurvivorsContentBinding(this);
            Resolver = new SurvivorsRuntimeContentResolver(Binding);
        }

        public void RefreshConfiguredTuning()
        {
            DefinitionAtRefresh = Binding.Definition;
            RefreshedTuning = Resolver.CreateConfiguredTuning(SurvivorsPacingProfile.HumanPlaytest);
            Events.Add("refresh");
        }

        public void ReleaseProfile() => Events.Add("release");

        public bool ConfigureUiThemes(TextAsset defaultTheme, TextAsset alternateTheme)
        {
            Assert.IsTrue(Binding.IsAuthoredContentBound, "Content binds before themes.");
            PrimaryTheme = defaultTheme;
            AlternateTheme = alternateTheme;
            Events.Add("themes");
            return ThemesSucceed;
        }

        public TextAsset[] LoadPack(bool neon = false)
        {
            string[] paths = neon ? new[]
            {
                "NeonArcana/Weapons/weapons.json", "NeonArcana/Upgrades/upgrades.json",
                "NeonArcana/Relics/relics.json", "NeonArcana/Classes/classes.json",
                "NeonArcana/Progression/progression.json", "NeonArcana/Enemies/enemies.json",
                "NeonArcana/Pickups/pickups.json", "NeonArcana/RunFlow/run-flow.json",
                "NeonArcana/Rewards/rewards.json", "NeonArcana/Themes/NeonArcana/ui-theme.json",
                "NeonArcana/Themes/Afterglow/ui-theme.json"
            } : new[]
            {
                "DefaultWeapons/weapons.json", "DefaultUpgrades/upgrades.json",
                "DefaultRelics/relics.json", "DefaultClasses/classes.json",
                "DefaultProgression/progression.json", "DefaultEnemies/enemies.json",
                "DefaultPickups/pickups.json", "DefaultRunFlow/run-flow.json",
                "DefaultRewards/rewards.json", "DefaultUiTheme/ui-theme.json", "NeonArcanaUiTheme/ui-theme.json"
            };
            string root = PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly).resolvedPath;
            var assets = new TextAsset[paths.Length];
            for (int i = 0; i < paths.Length; i++)
                assets[i] = CreateAsset(File.ReadAllText(Path.Combine(root, "Samples~", "BasicSurvivorsGame", "Content", paths[i])));
            return assets;
        }

        public TextAsset CreateAsset(string json)
        {
            var asset = new TextAsset(json);
            _ownedAssets.Add(asset);
            return asset;
        }

        public bool BindStrict(TextAsset[] assets) => Binding.ConfigureStrictSampleContent(
            assets[0], assets[1], assets[2], assets[3], assets[4], assets[5], assets[6],
            assets[7], assets[8], assets[9], assets[10]);

        public bool BindFullJson(TextAsset[] assets, SurvivorsAuthoredContentBindingPolicy policy) =>
            Binding.ConfigureAuthoredContentJson(assets[0].text, assets[1].text, assets[2].text, assets[3].text,
                assets[4].text, assets[5].text, assets[7].text, assets[8].text, policy);

        public void Dispose()
        {
            foreach (TextAsset asset in _ownedAssets) UnityEngine.Object.DestroyImmediate(asset);
        }
    }
}
