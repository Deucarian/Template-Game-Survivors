using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsContentBindingPort
    {
        bool RunStarted { get; }
        void RefreshConfiguredTuning();
        void ReleaseProfile();
        bool ConfigureUiThemes(TextAsset defaultTheme, TextAsset alternateTheme);
    }
}
