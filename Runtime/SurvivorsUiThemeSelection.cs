using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns available themes and selection while the component retains its serialized active theme.</summary>
    internal sealed class SurvivorsUiThemeSelection
    {
        private readonly ISurvivorsUiThemeSelectionPort _port;
        private readonly List<SurvivorsUiTheme> _availableUiThemes = new List<SurvivorsUiTheme>(2);
        private int _selectedUiThemeIndex;
        internal IReadOnlyList<SurvivorsUiTheme> AvailableThemes => _availableUiThemes;
        internal int SelectedIndex => _selectedUiThemeIndex;
        internal SurvivorsUiTheme ActiveTheme
        {
            get { EnsureUiTheme(); return _port.SerializedTheme; }
        }

        internal SurvivorsUiThemeSelection(ISurvivorsUiThemeSelectionPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal bool SelectUiTheme(int index)
        {
            EnsureUiTheme();
            if (index < 0 || index >= _availableUiThemes.Count || _availableUiThemes[index] == null)
            {
                return false;
            }

            _selectedUiThemeIndex = index;
            _port.SerializedTheme = _availableUiThemes[index];
            _port.ResetHudStyles();
            _port.ApplyWorldPresentation();
            _port.PlayThemeSelectionAudio();
            return true;
        }

        internal bool ConfigureUiTheme(TextAsset themeLibrary)
        {
            return ConfigureUiThemeJson(themeLibrary == null ? null : themeLibrary.text);
        }

        internal bool ConfigureUiThemeJson(string themeJson)
        {
            if (!SurvivorsUiTheme.TryFromJson(themeJson, out SurvivorsUiTheme parsed, out _))
            {
                _port.SerializedTheme = SurvivorsUiTheme.CreateDefault();
                _availableUiThemes.Clear();
                _availableUiThemes.Add(_port.SerializedTheme);
                _selectedUiThemeIndex = 0;
                _port.ResetHudStyles();
                _port.ApplyWorldPresentation();
                return false;
            }

            _port.SerializedTheme = parsed;
            _availableUiThemes.Clear();
            _availableUiThemes.Add(_port.SerializedTheme);
            _selectedUiThemeIndex = 0;
            _port.ResetHudStyles();
            _port.ApplyWorldPresentation();
            return true;
        }

        internal bool ConfigureUiThemes(TextAsset defaultThemeLibrary, TextAsset alternateThemeLibrary)
        {
            bool configuredDefault = ConfigureUiThemeJson(defaultThemeLibrary == null ? null : defaultThemeLibrary.text);
            if (alternateThemeLibrary == null)
            {
                return configuredDefault;
            }

            return ConfigureAdditionalUiThemeJson(alternateThemeLibrary.text) && configuredDefault;
        }

        internal bool ConfigureAdditionalUiThemeJson(string themeJson)
        {
            if (!SurvivorsUiTheme.TryFromJson(themeJson, out SurvivorsUiTheme parsed, out _))
            {
                EnsureUiTheme();
                return false;
            }

            EnsureUiTheme();
            if (_availableUiThemes.Count == 0)
            {
                _availableUiThemes.Add(_port.SerializedTheme);
            }

            _availableUiThemes.Add(parsed);
            return true;
        }

        internal void EnsureUiTheme()
        {
            if (_port.SerializedTheme == null)
            {
                _port.SerializedTheme = SurvivorsUiTheme.CreateDefault();
            }

            if (_availableUiThemes.Count == 0)
            {
                _availableUiThemes.Add(_port.SerializedTheme);
                _selectedUiThemeIndex = 0;
            }
            else if (_selectedUiThemeIndex < 0 || _selectedUiThemeIndex >= _availableUiThemes.Count)
            {
                _selectedUiThemeIndex = 0;
                _port.SerializedTheme = _availableUiThemes[0] ?? SurvivorsUiTheme.CreateDefault();
            }
        }
    }
}
