using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace Deucarian.TemplateGameSurvivors.PlayModeTests
{
    internal enum SurvivorsImportedSampleSceneKind
    {
        Basic = 0,
        NeonArcana = 1
    }

    internal static class SurvivorsImportedSampleSceneResolver
    {
        public static string ResolveOrIgnore(SurvivorsImportedSampleSceneKind kind)
        {
            if (TryResolve(kind, out string scenePath, out string diagnostic, out bool ambiguous)) return scenePath;
            if (ambiguous) Assert.Fail(diagnostic);
            Assert.Ignore(diagnostic);
            return string.Empty;
        }

        public static bool TryResolve(
            SurvivorsImportedSampleSceneKind kind,
            out string scenePath,
            out string diagnostic,
            out bool ambiguous)
        {
            string fileName = kind == SurvivorsImportedSampleSceneKind.NeonArcana
                ? "NeonArcana.unity"
                : "BasicSurvivorsGame.unity";
            string marker = kind == SurvivorsImportedSampleSceneKind.NeonArcana
                ? "PLAYTEST_THIS_SCENE_NEON_ARCANA"
                : "PLAYTEST_THIS_SCENE_OPEN_ME";
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            UnityEditor.PackageManager.PackageInfo package =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            string currentVersion = package == null ? string.Empty : package.version;

            Candidate[] candidates = AssetDatabase.FindAssets(baseName + " t:Scene", new[] { "Assets/Samples" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase))
                .Where(path => IsValidImportedScene(path, marker))
                .Select(path => new Candidate(path, Score(path, currentVersion)))
                .OrderByDescending(candidate => candidate.Score)
                .ThenBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (candidates.Length == 0)
            {
                scenePath = string.Empty;
                ambiguous = false;
                diagnostic = "No valid imported " + baseName + " scene was found under Assets/Samples. Import the Basic Survivors Game sample through Package Installer.";
                return false;
            }

            int bestScore = candidates[0].Score;
            Candidate[] preferred = candidates.Where(candidate => candidate.Score == bestScore).ToArray();
            if (preferred.Length > 1)
            {
                scenePath = string.Empty;
                ambiguous = true;
                diagnostic = "Multiple equally current imported " + baseName + " scenes were found: " +
                             string.Join(", ", preferred.Select(candidate => candidate.Path)) +
                             ". Remove or move stale duplicate sample imports.";
                return false;
            }

            scenePath = preferred[0].Path;
            ambiguous = false;
            diagnostic = "Resolved imported scene: " + scenePath;
            return true;
        }

        private static bool IsValidImportedScene(string assetPath, string marker)
        {
            if (string.IsNullOrWhiteSpace(assetPath) ||
                !assetPath.Replace('\\', '/').StartsWith("Assets/Samples/", StringComparison.OrdinalIgnoreCase)) return false;
            string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
            if (!File.Exists(fullPath)) return false;
            try
            {
                return File.ReadAllText(fullPath).IndexOf(marker, StringComparison.Ordinal) >= 0;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static int Score(string assetPath, string currentVersion)
        {
            string normalized = assetPath.Replace('\\', '/');
            int score = 0;
            if (!string.IsNullOrWhiteSpace(currentVersion) &&
                normalized.IndexOf("/" + currentVersion + "/", StringComparison.OrdinalIgnoreCase) >= 0) score += 100;
            if (normalized.IndexOf("/com.deucarian.template.game.survivors/", StringComparison.OrdinalIgnoreCase) >= 0) score += 20;
            if (normalized.IndexOf("/Deucarian Template Game - Survivors/", StringComparison.OrdinalIgnoreCase) >= 0) score += 10;
            if (normalized.IndexOf("/Basic Survivors Game/Scenes/", StringComparison.OrdinalIgnoreCase) >= 0) score += 5;
            return score;
        }

        private readonly struct Candidate
        {
            public Candidate(string path, int score)
            {
                Path = path;
                Score = score;
            }

            public string Path { get; }
            public int Score { get; }
        }
    }
}
