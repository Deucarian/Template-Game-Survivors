using System;
using System.IO;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.GameplayFoundation;
using Deucarian.TemplateGameSurvivors.Editor;
using NUnit.Framework;
using UnityEditor.PackageManager;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsEditorContentValidationTests
    {
        [Test]
        public void EditorValidationRunnerBuildsReportForSampleContent()
        {
            ContentValidationReport report = SurvivorsEditorContentValidation.BuildBasicSampleReport();
            string markdown = SurvivorsEditorContentValidation.BuildMarkdownReport(report);

            Assert.IsNotNull(report);
            StringAssert.Contains("Survivors Template Content Validation", markdown);
        }

        [Test]
        public void EditorValidationRunnerReportsNoSampleContentErrors()
        {
            ContentValidationReport report = SurvivorsEditorContentValidation.BuildBasicSampleReport();

            Assert.AreEqual(0, report.ErrorCount, string.Join(Environment.NewLine, report.GetMessages()));
        }

        [Test]
        public void EditorValidationRunnerReportsInvalidSampleContentThroughAuthoringAdapter()
        {
            ContentValidationReport report = SurvivorsEditorContentValidation.ValidateJsonContent(string.Empty, string.Empty);
            GameContentAuthoringValidationResult authoringResult =
                GameContentAuthoringValidationReports.ToAuthoringResult(report);
            string markdown = GameContentAuthoringValidationReports.ToMarkdown(report, "Invalid Survivors Content");

            Assert.That(report.ErrorCount, Is.GreaterThan(0));
            Assert.AreEqual(report.ErrorCount, authoringResult.ErrorCount);
            StringAssert.Contains("## Errors", markdown);
            StringAssert.Contains("Sample weapon library JSON is empty.", markdown);
            StringAssert.Contains("Survivors.SampleContent", markdown);
        }

        [Test]
        public void RuntimeAssemblyDoesNotReferenceEditorOnlyAuthoringPackages()
        {
            PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            Assert.IsNotNull(packageInfo);
            string runtimeAsmdefPath = Path.Combine(packageInfo.resolvedPath, "Runtime", "Deucarian.TemplateGameSurvivors.asmdef");
            string runtimeAsmdef = File.ReadAllText(runtimeAsmdefPath);

            StringAssert.DoesNotContain("GameContentAuthoring", runtimeAsmdef);
            StringAssert.DoesNotContain("Deucarian.TemplateGameSurvivors.Editor", runtimeAsmdef);
        }
    }
}
