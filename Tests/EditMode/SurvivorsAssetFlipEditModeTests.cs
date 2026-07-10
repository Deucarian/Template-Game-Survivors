using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsAssetFlipEditModeTests
    {
        private const string BootstrapScriptGuid = "d473dc41f0b44b63aef3a6d21f0a6872";

        [Test]
        public void BasicAndNeonArcanaValidateAsIndependentStrictContentSets()
        {
            Assert.AreEqual(0, Editor.SurvivorsEditorContentValidation.BuildBasicSampleReport().ErrorCount);
            Assert.AreEqual(0, Editor.SurvivorsEditorContentValidation.BuildNeonArcanaSampleReport().ErrorCount);

            SurvivorsContentValidationResult neonValidation = ValidatePack("NeonArcana");
            Assert.IsTrue(neonValidation.Succeeded, string.Join(Environment.NewLine, neonValidation.Errors));

            string sampleRoot = GetSampleRoot();
            string neonRoot = Path.Combine(sampleRoot, "Content", "NeonArcana");
            Assert.AreEqual(11, Directory.GetFiles(neonRoot, "*.json", SearchOption.AllDirectories).Length);
            Assert.AreEqual(0, Directory.GetFiles(neonRoot, "*.cs", SearchOption.AllDirectories).Length);
            Assert.That(File.ReadAllText(Path.Combine(neonRoot, "Weapons", "weapons.json")), Does.Not.Contain("DefaultWeapons"));
        }

        [Test]
        public void NeonArcanaStrictBindingUsesDistinctContentWithoutFallbackCatalogs()
        {
            SurvivorsAuthoredContentDefinition basic = CreateStrictDefinition("Default");
            SurvivorsAuthoredContentDefinition neon = CreateStrictDefinition("NeonArcana");

            Assert.IsTrue(neon.IsStrictSample);
            Assert.IsFalse(neon.UsesBuiltInFallbacks);
            Assert.AreEqual("Arc Bolt", FindWeapon(basic, BasicSurvivorsGame.ArcaneWandWeaponContentId).DisplayName);
            Assert.AreEqual("Neon Pulse", FindWeapon(neon, BasicSurvivorsGame.ArcaneWandWeaponContentId).DisplayName);
            Assert.AreEqual(new Color32(0, 229, 255, 255), (Color32)FindWeapon(neon, BasicSurvivorsGame.ArcaneWandWeaponContentId).Tint);
            Assert.AreEqual("Swarm Thrall", FindEnemy(basic, SurvivorsEnemyRole.Swarm).DisplayName);
            Assert.AreEqual("Static Wisp", FindEnemy(neon, SurvivorsEnemyRole.Swarm).DisplayName);
            Assert.AreEqual("Blacklight Sovereign", FindEnemy(neon, SurvivorsEnemyRole.Boss).DisplayName);
            Assert.AreEqual("Pulse Runner", neon.ClassLibrary.Classes[0].DisplayName);
            Assert.AreEqual(1, neon.ClassLibrary.Classes[0].StartingWeaponIds.Count);
            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponContentId, neon.ClassLibrary.Classes[0].StartingWeaponIds[0]);
            Assert.That(neon.RunUpgradeCatalog.Definitions.Count, Is.GreaterThanOrEqualTo(60));
            Assert.That(neon.RunUpgradeMetadata.Any(item => item.DisplayName == "Data Vacuum"));
            Assert.That(neon.RunUpgradeMetadata.Any(item => item.DisplayName == "Neon Tempest"));
            Assert.That(neon.ProgressionTracks.Any(track => track.DisplayName == "Neon Pulse Skill Track"));
            Assert.AreEqual("Neon Fragments", neon.MetaProgressionDefinition.CurrencyDisplayName);
            Assert.AreEqual("Signal Memory", neon.MetaProgressionDefinition.LegacyExperienceDisplayName);
            Assert.AreEqual("Pulse Legacy", neon.MetaProgressionDefinition.PersistentUpgrades[0].DisplayName);

            SurvivorsTemplateTuning standard = neon.CreateTuning(SurvivorsPacingProfile.HumanPlaytest);
            SurvivorsTemplateTuning sprint = neon.CreateTuning(SurvivorsPacingProfile.SprintRun);
            Assert.AreEqual("Neon Arcana Standard", standard.RunModeDisplayName);
            Assert.AreEqual(1800f, standard.SurvivalVictoryTimeSeconds);
            Assert.AreEqual("Neon Arcana Sprint", sprint.RunModeDisplayName);
            Assert.AreEqual(300f, sprint.SurvivalVictoryTimeSeconds);
        }

        [Test]
        public void NeonArcanaThemeOwnsWorldPaletteTutorialAndAudioPresentation()
        {
            string defaultJson = ReadDefault("DefaultUiTheme", "ui-theme.json");
            string neonJson = ReadNeon("Themes", "NeonArcana", "ui-theme.json");
            Assert.IsTrue(SurvivorsUiTheme.TryFromJson(defaultJson, out SurvivorsUiTheme basic, out string basicError), basicError);
            Assert.IsTrue(SurvivorsUiTheme.TryFromJson(neonJson, out SurvivorsUiTheme neon, out string neonError), neonError);

            Assert.IsFalse(basic.HasAuthoredWorldPresentation);
            Assert.IsTrue(neon.HasAuthoredWorldPresentation);
            Assert.AreEqual("Neon Arcana", neon.themeName);
            Assert.AreEqual("Arcana Run Report", neon.runSummaryTitle);
            Assert.AreEqual("Run The Circuit", neon.GetTutorialStepTitle(0, string.Empty));
            Assert.That(neon.GetTutorialStepLines(0, Array.Empty<string>())[0], Does.Contain("Neon Pulse"));
            Assert.AreNotEqual(basic.GetAudioEventVolume("ui.select", 0f), neon.GetAudioEventVolume("ui.select", 0f));
            Assert.AreEqual(new Color32(92, 255, 241, 255), (Color32)neon.GetPlayerColor(Color.black));
            Assert.AreEqual(new Color32(0, 229, 255, 255), (Color32)neon.GetArenaAccentColor(Color.black));

            string invalidTheme = neonJson.Replace("#5CFFF1", "not-a-color");
            SurvivorsContentValidationResult invalid = ValidatePack("NeonArcana", invalidTheme);
            Assert.IsFalse(invalid.Succeeded);
            StringAssert.Contains("player presentation requires an authored HTML color value", string.Join(Environment.NewLine, invalid.Errors));
        }

        [Test]
        public void NeonArcanaWeaponMutationChangesAlternateRuntimeWithoutChangingBasic()
        {
            SurvivorsAuthoredContentDefinition basic = CreateStrictDefinition("Default");
            string mutatedWeapons = ReadNeon("Weapons", "weapons.json")
                .Replace("\"cooldownSeconds\":  0.52", "\"cooldownSeconds\":  0.87")
                .Replace("\"damage\":  7", "\"damage\":  11.5");
            SurvivorsAuthoredContentDefinition neon = CreateStrictDefinition("NeonArcana", mutatedWeapons);

            SurvivorsWeaponArchetypeDefinition basicStarter = FindWeapon(basic, BasicSurvivorsGame.ArcaneWandWeaponContentId);
            SurvivorsWeaponArchetypeDefinition neonStarter = FindWeapon(neon, BasicSurvivorsGame.ArcaneWandWeaponContentId);
            Assert.AreEqual(0.52f, basicStarter.CooldownSeconds, 0.001f);
            Assert.AreEqual(7f, basicStarter.Damage, 0.001f);
            Assert.AreEqual(0.87f, neonStarter.CooldownSeconds, 0.001f);
            Assert.AreEqual(11.5f, neonStarter.Damage, 0.001f);
        }

        [Test]
        public void NeonArcanaSceneReusesBootstrapAndReferencesOnlyAlternateLibraries()
        {
            string scenesRoot = Path.Combine(GetSampleRoot(), "Scenes");
            string basicScene = File.ReadAllText(Path.Combine(scenesRoot, "BasicSurvivorsGame.unity"));
            string neonScene = File.ReadAllText(Path.Combine(scenesRoot, "NeonArcana.unity"));
            Assert.That(basicScene, Does.Contain(BootstrapScriptGuid));
            Assert.That(neonScene, Does.Contain(BootstrapScriptGuid));
            Assert.That(neonScene, Does.Contain("PLAYTEST_THIS_SCENE_NEON_ARCANA"));

            string[] neonGuids =
            {
                "f7ca5f6c80e54abc8d0782ccb96bd8e7",
                "26e52917c56548158f986ab1c9d51c68",
                "64de018571244e7e8ec65271e9712b3f",
                "0f134f2a156f4ddfa5033d4d5b758ace",
                "788f500c96294d5d9536c86d7b425b80",
                "4edcbbf4cecb4462b097ed81bff7033b",
                "587298a0e84946d4b64fdfa97d85b810",
                "90484a0310f8460ca445e832eba71141",
                "3dfae122d854407c9f851fa854ee0115",
                "51ec99e21e9448a1b43aeb8a8ab81140",
                "48be1741f5be423782e1dd5095895238"
            };
            string[] basicGuids =
            {
                "187d6c522814452b99f0f219dafe05a9",
                "3429c782a95147ceb622a80808ae96a1",
                "de71d061c47e4c4fbb4c8dcc517a0097",
                "71e25d10b1094f108be2a788aa16debd",
                "01618825803244d2ad45877c34813a24",
                "9252b55c5f4b4019a0a80b5cb14d3654",
                "27d54548ed0b4ab9a811e163c0d471cf",
                "ffe384a93f9c4415ad18e3859b8cb082",
                "69e9189e1bee4ca799e72143e48a60f4",
                "772a0e4c2de84c8e9a33618f68c830cb",
                "3803b80ec2954fbcb0491fcbdecd11ea"
            };

            foreach (string guid in neonGuids)
            {
                Assert.That(neonScene, Does.Contain(guid));
            }

            foreach (string guid in basicGuids)
            {
                Assert.That(neonScene, Does.Not.Contain(guid));
            }
        }

        [Test]
        public void AssetFlipDocumentationRecordsLaunchBoundariesAndExtractionReadiness()
        {
            string path = Path.Combine(GetPackageRoot(), "Documentation~", "neon-arcana-asset-flip.md");
            Assert.IsTrue(File.Exists(path), path);
            string documentation = File.ReadAllText(path);
            StringAssert.Contains("Scenes/BasicSurvivorsGame.unity", documentation);
            StringAssert.Contains("Scenes/NeonArcana.unity", documentation);
            StringAssert.Contains("Runtime And Authored Boundaries", documentation);
            StringAssert.Contains("Runtime primitive fallback", documentation);
            StringAssert.Contains("Ready for second-template evaluation", documentation);
            StringAssert.Contains("Still needs Idle Auto Defense", documentation);
            StringAssert.Contains("No Game Content Authoring package change was needed", documentation);
        }

        private static SurvivorsContentValidationResult ValidatePack(string pack, string primaryThemeOverride = null)
        {
            bool neon = string.Equals(pack, "NeonArcana", StringComparison.Ordinal);
            return SurvivorsContentValidator.ValidateSampleJson(
                neon ? ReadNeon("Weapons", "weapons.json") : ReadDefault("DefaultWeapons", "weapons.json"),
                neon ? ReadNeon("Upgrades", "upgrades.json") : ReadDefault("DefaultUpgrades", "upgrades.json"),
                neon ? ReadNeon("Enemies", "enemies.json") : ReadDefault("DefaultEnemies", "enemies.json"),
                neon ? ReadNeon("Rewards", "rewards.json") : ReadDefault("DefaultRewards", "rewards.json"),
                neon ? ReadNeon("Relics", "relics.json") : ReadDefault("DefaultRelics", "relics.json"),
                neon ? ReadNeon("Classes", "classes.json") : ReadDefault("DefaultClasses", "classes.json"),
                neon ? ReadNeon("Progression", "progression.json") : ReadDefault("DefaultProgression", "progression.json"),
                neon ? ReadNeon("Pickups", "pickups.json") : ReadDefault("DefaultPickups", "pickups.json"),
                neon ? ReadNeon("RunFlow", "run-flow.json") : ReadDefault("DefaultRunFlow", "run-flow.json"),
                primaryThemeOverride ?? (neon ? ReadNeon("Themes", "NeonArcana", "ui-theme.json") : ReadDefault("DefaultUiTheme", "ui-theme.json")),
                neon ? ReadNeon("Themes", "Afterglow", "ui-theme.json") : ReadDefault("NeonArcanaUiTheme", "ui-theme.json"));
        }

        private static SurvivorsAuthoredContentDefinition CreateStrictDefinition(string pack, string weaponOverride = null)
        {
            bool neon = string.Equals(pack, "NeonArcana", StringComparison.Ordinal);
            bool created = SurvivorsAuthoredContentDefinition.TryCreate(
                weaponOverride ?? (neon ? ReadNeon("Weapons", "weapons.json") : ReadDefault("DefaultWeapons", "weapons.json")),
                neon ? ReadNeon("Upgrades", "upgrades.json") : ReadDefault("DefaultUpgrades", "upgrades.json"),
                neon ? ReadNeon("Relics", "relics.json") : ReadDefault("DefaultRelics", "relics.json"),
                neon ? ReadNeon("Classes", "classes.json") : ReadDefault("DefaultClasses", "classes.json"),
                neon ? ReadNeon("Progression", "progression.json") : ReadDefault("DefaultProgression", "progression.json"),
                neon ? ReadNeon("Enemies", "enemies.json") : ReadDefault("DefaultEnemies", "enemies.json"),
                neon ? ReadNeon("RunFlow", "run-flow.json") : ReadDefault("DefaultRunFlow", "run-flow.json"),
                neon ? ReadNeon("Rewards", "rewards.json") : ReadDefault("DefaultRewards", "rewards.json"),
                SurvivorsAuthoredContentBindingPolicy.StrictSample,
                out SurvivorsAuthoredContentDefinition definition,
                out string error);
            Assert.IsTrue(created, error);
            return definition;
        }

        private static SurvivorsWeaponArchetypeDefinition FindWeapon(SurvivorsAuthoredContentDefinition content, string id)
        {
            SurvivorsWeaponArchetypeDefinition definition = content.WeaponDefinitions.FirstOrDefault(item => item.Id == id);
            Assert.IsNotNull(definition, id);
            return definition;
        }

        private static SurvivorsEnemyProfile FindEnemy(SurvivorsAuthoredContentDefinition content, SurvivorsEnemyRole role)
        {
            Assert.IsTrue(content.TryGetEnemyProfile(role, out SurvivorsEnemyProfile profile), role.ToString());
            return profile;
        }

        private static string ReadDefault(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { GetSampleRoot(), "Content" }.Concat(parts).ToArray()));
        }

        private static string ReadNeon(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { GetSampleRoot(), "Content", "NeonArcana" }.Concat(parts).ToArray()));
        }

        private static string GetSampleRoot()
        {
            string root = Path.Combine(GetPackageRoot(), "Samples~", "BasicSurvivorsGame");
            Assert.IsTrue(Directory.Exists(root), root);
            return root;
        }

        private static string GetPackageRoot()
        {
            PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            Assert.IsNotNull(packageInfo);
            Assert.IsTrue(Directory.Exists(packageInfo.resolvedPath), packageInfo.resolvedPath);
            return packageInfo.resolvedPath;
        }
    }
}
