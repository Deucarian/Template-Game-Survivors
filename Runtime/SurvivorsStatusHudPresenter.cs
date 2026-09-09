using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Draws status snapshots; no gameplay commands or persistent state.</summary>
    internal static class SurvivorsStatusHudPresenter
    {
        public static void DrawLowHealth(bool active, GUIStyle style)
        {
            if (!active)
            {
                return;
            }

            float pulse = 0.75f + Mathf.Sin(Time.unscaledTime * 7f) * 0.25f;
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 0.04f, 0.04f, 0.12f + 0.08f * pulse);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, 12f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, Screen.height - 12f, Screen.width, 12f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, 0f, 12f, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - 12f, 0f, 12f, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.95f);
            GUI.Label(new Rect(24, 370, 318, 22), "LOW HEALTH", style);
            GUI.color = oldColor;
        }

        public static void DrawMajorThreat(bool active, bool timerVisible, string label, float remaining, GUIStyle style)
        {
            if (!active)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 8f) * 0.28f;
            float y = timerVisible ? 58f : 24f;
            Rect panel = new Rect(Screen.width * 0.5f - 184f, y, 368f, 52f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.22f, 0.05f, 0.04f, 0.56f + 0.18f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            GUI.Label(panel, $"{label}  {Mathf.CeilToInt(remaining)}s", style);
            GUI.color = oldColor;
        }

        public static void DrawHordeRush(bool active, bool timerVisible, bool majorWarningActive, string label, float remaining, GUIStyle style)
        {
            if (!active)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 8.5f) * 0.28f;
            float baseY = timerVisible ? 58f : 24f;
            float y = majorWarningActive ? baseY + 60f : baseY;
            Rect panel = new Rect(Screen.width * 0.5f - 170f, y, 340f, 46f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.28f, 0.08f, 0.02f, 0.52f + 0.18f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            GUI.Label(panel, $"{label}  {Mathf.CeilToInt(remaining)}s", style);
            GUI.color = oldColor;
        }

        public static void DrawBar(Rect rect, string label, float value, Color fill, GUIStyle style)
        {
            GUI.Box(rect, GUIContent.none);
            Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(value), rect.height - 4f);
            Color oldColor = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUI.Label(rect, label + " " + Mathf.RoundToInt(Mathf.Clamp01(value) * 100f).ToString() + "%", style);
        }
    }
}
