using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsTopTimerValues
    {
        internal SurvivorsTopTimerValues(bool started, bool endless, float targetSeconds, float runTimeSeconds,
            string modeName, SurvivorsPacingProfile pacingProfile)
        {
            Started = started;
            Endless = endless;
            TargetSeconds = targetSeconds;
            RunTimeSeconds = runTimeSeconds;
            ModeName = modeName;
            PacingProfile = pacingProfile;
        }
        internal bool Started { get; }
        internal bool Endless { get; }
        internal float TargetSeconds { get; }
        internal float RunTimeSeconds { get; }
        internal string ModeName { get; }
        internal SurvivorsPacingProfile PacingProfile { get; }
    }

    /// <summary>Owns the top timer's display copy and geometry over a captured run clock.</summary>
    internal static class SurvivorsTopTimerHud
    {
        internal static Rect Panel(int screenWidth)
        {
            float width = Mathf.Min(360f, Mathf.Max(240f, screenWidth - 32f));
            return new Rect(screenWidth * 0.5f - width * 0.5f, 12f, width, 36f);
        }

        internal static string Label(in SurvivorsTopTimerValues values)
        {
            if (!values.Started) return string.Empty;
            float target = Mathf.Max(0f, values.TargetSeconds);
            string elapsed = SurvivorsRunText.FormatRunTime(values.RunTimeSeconds);
            string mode = string.IsNullOrWhiteSpace(values.ModeName)
                ? BasicSurvivorsGame.GetPacingProfileDisplayName(values.PacingProfile)
                : values.ModeName;
            if (values.Endless || target <= 0f) return mode + "  TIME " + elapsed + "  ENDLESS";
            return mode + "  TIME " + elapsed + "  LEFT " + SurvivorsRunText.FormatRunTime(Mathf.Max(0f, target - values.RunTimeSeconds));
        }

        internal static void Draw(int screenWidth, in SurvivorsTopTimerValues values, GUIStyle style)
        {
            Rect panel = Panel(screenWidth);
            string label = Label(values);
            if (string.IsNullOrWhiteSpace(label)) return;
            Color oldColor = GUI.color;
            GUI.color = new Color(0.015f, 0.02f, 0.026f, 0.78f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.78f, 1f, 0.92f);
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(panel, label, style);
            GUI.color = oldColor;
        }
    }
}
