using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsUiThemeSelectionTests
    {
        [Test]
        public void ExistingSerializedThemeRemainsAuthoritativeAndEnsureDoesNotReplayCallbacks()
        {
            var original = new SurvivorsUiTheme { themeName = "Inspector" };
            var port = new ThemePort { StoredTheme = original };
            var selection = new SurvivorsUiThemeSelection(port);
            Assert.That(selection.AvailableThemes, Is.Empty);
            Assert.That(selection.ActiveTheme, Is.SameAs(original));
            Assert.That(selection.AvailableThemes[0], Is.SameAs(original));
            selection.EnsureUiTheme();
            Assert.That(port.Events, Is.Empty);
            var external = new SurvivorsUiTheme { themeName = "External" };
            port.StoredTheme = external;
            Assert.That(selection.ActiveTheme, Is.SameAs(external));
            Assert.That(selection.AvailableThemes[0], Is.SameAs(original));
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void ValidConfigurationReplacesAlternatesBeforeResetAndWorldCallbacksWithoutAudio()
        {
            var port = new ThemePort();
            var selection = new SurvivorsUiThemeSelection(port);
            selection.ConfigureAdditionalUiThemeJson("{\"themeName\":\"Old alternate\"}");
            port.Events.Clear();
            port.OnReset = () =>
            {
                Assert.That(selection.SelectedIndex, Is.Zero);
                Assert.That(selection.AvailableThemes.Count, Is.EqualTo(1));
                Assert.That(selection.ActiveTheme.themeName, Is.EqualTo("Primary"));
            };
            Assert.That(selection.ConfigureUiThemeJson("{\"themeName\":\"Primary\"}"), Is.True);
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Primary", "reset", "world" }));
        }

        [Test]
        public void InvalidPrimaryInstallsDefaultAndStillAppliesPresentation()
        {
            var port = new ThemePort { StoredTheme = new SurvivorsUiTheme { themeName = "Old" } };
            var selection = new SurvivorsUiThemeSelection(port);
            Assert.That(selection.ConfigureUiThemeJson("not-json"), Is.False);
            Assert.That(selection.ActiveTheme.themeName, Is.EqualTo("Deucarian Survivors"));
            Assert.That(selection.AvailableThemes.Count, Is.EqualTo(1));
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Deucarian Survivors", "reset", "world" }));
        }

        [Test]
        public void ValidAlternateAppendsDespiteFailedPrimaryAndOverallResultRemainsFalse()
        {
            var port = new ThemePort();
            var selection = new SurvivorsUiThemeSelection(port);
            var invalidPrimary = new TextAsset("not-json");
            var alternate = new TextAsset("{\"themeName\":\"Alternate\"}");
            try
            {
                Assert.That(selection.ConfigureUiThemes(invalidPrimary, alternate), Is.False);
                Assert.That(selection.AvailableThemes.Count, Is.EqualTo(2));
                Assert.That(selection.AvailableThemes[1].themeName, Is.EqualTo("Alternate"));
                Assert.That(selection.SelectedIndex, Is.Zero);
                Assert.That(port.Events, Is.EqualTo(new[] { "set:Deucarian Survivors", "reset", "world" }));
            }
            finally { Object.DestroyImmediate(invalidPrimary); Object.DestroyImmediate(alternate); }
        }

        [Test]
        public void InvalidAdditionalOnlyEnsuresFallbackWhileValidDuplicatesRemainSeparateOptions()
        {
            var port = new ThemePort();
            var selection = new SurvivorsUiThemeSelection(port);
            Assert.That(selection.ConfigureAdditionalUiThemeJson("not-json"), Is.False);
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Deucarian Survivors" }));
            port.Events.Clear();
            Assert.That(selection.ConfigureAdditionalUiThemeJson("{\"themeName\":\"Same\"}"), Is.True);
            Assert.That(selection.ConfigureAdditionalUiThemeJson("{\"themeName\":\"Same\"}"), Is.True);
            Assert.That(selection.AvailableThemes.Count, Is.EqualTo(3));
            Assert.That(selection.AvailableThemes[1], Is.Not.SameAs(selection.AvailableThemes[2]));
            Assert.That(selection.SelectedIndex, Is.Zero);
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void EmptyConfigurationIsAnAcceptedDefaultAndEmptyAlternateAppendsAnotherDefault()
        {
            var port = new ThemePort();
            var selection = new SurvivorsUiThemeSelection(port);
            Assert.That(selection.ConfigureUiTheme(null), Is.True);
            Assert.That(selection.ActiveTheme.themeName, Is.EqualTo("Deucarian Survivors"));
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Deucarian Survivors", "reset", "world" }));
            port.Events.Clear();
            Assert.That(selection.ConfigureAdditionalUiThemeJson(" \t"), Is.True);
            Assert.That(selection.AvailableThemes.Count, Is.EqualTo(2));
            Assert.That(selection.AvailableThemes[1].themeName, Is.EqualTo("Deucarian Survivors"));
            Assert.That(selection.AvailableThemes[1], Is.Not.SameAs(selection.AvailableThemes[0]));
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void SelectionSetsIndexAndSerializedValueBeforeResetWorldAndAudio()
        {
            var port = new ThemePort();
            var selection = new SurvivorsUiThemeSelection(port);
            selection.ConfigureUiThemeJson("{\"themeName\":\"Primary\"}");
            selection.ConfigureAdditionalUiThemeJson("{\"themeName\":\"Alternate\"}");
            port.Events.Clear();
            port.OnSet = () => Assert.That(selection.SelectedIndex, Is.EqualTo(1));
            port.OnReset = () => Assert.That(selection.ActiveTheme, Is.SameAs(selection.AvailableThemes[1]));
            Assert.That(selection.SelectUiTheme(1), Is.True);
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Alternate", "reset", "world", "audio" }));
            port.Events.Clear();
            Assert.That(selection.SelectUiTheme(1), Is.True, "Reselecting the same index retains the original callbacks.");
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Alternate", "reset", "world", "audio" }));
        }

        [Test]
        public void InvalidIndexEnsuresInitialThemeButDoesNotApplyOrPlaySelection()
        {
            var port = new ThemePort();
            var selection = new SurvivorsUiThemeSelection(port);
            Assert.That(selection.SelectUiTheme(-1), Is.False);
            Assert.That(selection.SelectUiTheme(1), Is.False);
            Assert.That(selection.SelectedIndex, Is.Zero);
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Deucarian Survivors" }));
        }

        [Test]
        public void NullSerializedThemeRebuildsFallbackWithoutReplacingExistingAvailableOptions()
        {
            var port = new ThemePort();
            var selection = new SurvivorsUiThemeSelection(port);
            selection.ConfigureUiThemeJson("{\"themeName\":\"Primary\"}");
            selection.ConfigureAdditionalUiThemeJson("{\"themeName\":\"Alternate\"}");
            selection.SelectUiTheme(1);
            port.StoredTheme = null;
            port.Events.Clear();
            Assert.That(selection.ActiveTheme.themeName, Is.EqualTo("Deucarian Survivors"));
            Assert.That(selection.SelectedIndex, Is.EqualTo(1));
            Assert.That(selection.AvailableThemes[1].themeName, Is.EqualTo("Alternate"));
            Assert.That(port.Events, Is.EqualTo(new[] { "set:Deucarian Survivors" }));
        }

        private sealed class ThemePort : ISurvivorsUiThemeSelectionPort
        {
            internal readonly List<string> Events = new List<string>();
            internal SurvivorsUiTheme StoredTheme;
            internal Action OnSet;
            internal Action OnReset;
            public SurvivorsUiTheme SerializedTheme
            {
                get => StoredTheme;
                set { StoredTheme = value; Events.Add("set:" + value.themeName); OnSet?.Invoke(); }
            }
            public void ResetHudStyles() { Events.Add("reset"); OnReset?.Invoke(); }
            public void ApplyWorldPresentation() => Events.Add("world");
            public void PlayThemeSelectionAudio() => Events.Add("audio");
        }
    }
}
