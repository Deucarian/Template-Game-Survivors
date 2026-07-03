using System;
using System.IO;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.GameplayFoundation;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    public static class SurvivorsEditorContentValidation
    {
        public const string MenuPath = "Tools/Deucarian/Templates/Survivors/Validate Content";
        private const string ReportTitle = "Survivors Template Content Validation";
        private const string SampleName = "BasicSurvivorsGame";

        [MenuItem(MenuPath, priority = 330)]
        public static void ValidateContent()
        {
            ContentValidationReport report = BuildBasicSampleReport();
            string summary = GameContentAuthoringValidationReports.BuildSummary(report);
            string markdown = BuildMarkdownReport(report);

            if (report.ErrorCount > 0)
            {
                Debug.LogError(summary + Environment.NewLine + markdown);
            }
            else if (report.WarningCount > 0)
            {
                Debug.LogWarning(summary + Environment.NewLine + markdown);
            }
            else
            {
                Debug.Log(summary + Environment.NewLine + markdown);
            }
        }

        public static ContentValidationReport BuildBasicSampleReport()
        {
            var report = new ContentValidationReport();
            string sampleRoot = ResolveSampleRoot(report);
            if (string.IsNullOrWhiteSpace(sampleRoot))
            {
                return report;
            }

            string weaponJson = ReadRequiredText(sampleRoot, "Content/DefaultWeapons/weapons.json", report);
            string upgradeJson = ReadRequiredText(sampleRoot, "Content/DefaultUpgrades/upgrades.json", report);
            string enemyJson = ReadRequiredText(sampleRoot, "Content/DefaultEnemies/enemies.json", report);
            string pickupJson = ReadRequiredText(sampleRoot, "Content/DefaultPickups/pickups.json", report);
            string rewardJson = ReadRequiredText(sampleRoot, "Content/DefaultRewards/rewards.json", report);
            string relicJson = ReadRequiredText(sampleRoot, "Content/DefaultRelics/relics.json", report);
            string classJson = ReadRequiredText(sampleRoot, "Content/DefaultClasses/classes.json", report);
            string progressionJson = ReadRequiredText(sampleRoot, "Content/DefaultProgression/progression.json", report);
            string runFlowJson = ReadRequiredText(sampleRoot, "Content/DefaultRunFlow/run-flow.json", report);

            if (report.ErrorCount > 0)
            {
                return report;
            }

            Merge(report, ValidateJsonContent(weaponJson, upgradeJson, enemyJson, rewardJson, relicJson, classJson, progressionJson, pickupJson, runFlowJson));
            return report;
        }

        public static ContentValidationReport ValidateJsonContent(
            string weaponJson,
            string upgradeJson,
            string enemyJson = null,
            string rewardJson = null,
            string relicJson = null,
            string classJson = null,
            string progressionJson = null,
            string pickupJson = null,
            string runFlowJson = null)
        {
            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(
                weaponJson,
                upgradeJson,
                enemyJson,
                rewardJson,
                relicJson,
                classJson,
                progressionJson,
                pickupJson,
                runFlowJson);

            var report = new ContentValidationReport();
            for (int index = 0; index < result.Errors.Count; index++)
            {
                report.AddError(result.Errors[index], "Survivors.SampleContent");
            }

            return report;
        }

        public static string BuildMarkdownReport(ContentValidationReport report)
        {
            return GameContentAuthoringValidationReports.ToMarkdown(report, ReportTitle);
        }

        private static string ResolveSampleRoot(ContentValidationReport report)
        {
            UnityEditor.PackageManager.PackageInfo packageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            if (packageInfo == null || string.IsNullOrWhiteSpace(packageInfo.resolvedPath))
            {
                report.AddError("Could not resolve the Survivors template package path.", SampleName);
                return null;
            }

            string sampleRoot = Path.Combine(packageInfo.resolvedPath, "Samples~", SampleName);
            if (!Directory.Exists(sampleRoot))
            {
                report.AddError("Could not find the BasicSurvivorsGame sample at " + sampleRoot + ".", SampleName);
                return null;
            }

            return sampleRoot;
        }

        private static string ReadRequiredText(string sampleRoot, string relativePath, ContentValidationReport report)
        {
            string fullPath = Path.Combine(sampleRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                report.AddError("Required sample content file is missing: " + relativePath + ".", relativePath);
                return null;
            }

            try
            {
                return File.ReadAllText(fullPath);
            }
            catch (IOException exception)
            {
                report.AddError("Could not read sample content file " + relativePath + ": " + exception.Message, relativePath);
                return null;
            }
            catch (UnauthorizedAccessException exception)
            {
                report.AddError("Could not read sample content file " + relativePath + ": " + exception.Message, relativePath);
                return null;
            }
        }

        private static void Merge(ContentValidationReport target, ContentValidationReport source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Issues.Count; index++)
            {
                target.AddIssue(source.Issues[index]);
            }
        }
    }
}
