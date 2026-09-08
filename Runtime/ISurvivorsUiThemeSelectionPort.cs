namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsUiThemeSelectionPort
    {
        SurvivorsUiTheme SerializedTheme { get; set; }
        void ResetHudStyles();
        void ApplyWorldPresentation();
        void PlayThemeSelectionAudio();
    }
}
