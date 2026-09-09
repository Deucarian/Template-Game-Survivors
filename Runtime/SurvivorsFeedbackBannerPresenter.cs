using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal enum SurvivorsFeedbackBannerKind { Reward, Streak, ClassUnlock, Experience, Evolution }

    /// <summary>Owns a transient HUD banner. Gameplay supplies finished copy and never drives expiry through rendering.</summary>
    internal sealed class SurvivorsFeedbackBannerPresenter
    {
        private readonly SurvivorsFeedbackBannerKind _kind;
        public SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind kind) => _kind = kind;
        public string Label { get; private set; } = string.Empty;
        public Color Accent { get; private set; } = Color.white;
        public float RemainingSeconds { get; private set; }

        public void Show(string label, float seconds, Color accent)
        {
            Label = label;
            RemainingSeconds = seconds;
            Accent = accent;
        }

        public void Reset()
        {
            Label = string.Empty;
            RemainingSeconds = 0f;
            Accent = Color.white;
        }

        public void Tick(float deltaTime)
        {
            if (RemainingSeconds <= 0f) return;
            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Mathf.Max(0f, deltaTime));
            if (RemainingSeconds <= 0f) Label = string.Empty;
        }

        public void Draw(GUIStyle style)
        {
            switch (_kind)
            {
                case SurvivorsFeedbackBannerKind.Reward: DrawReward(style); break;
                case SurvivorsFeedbackBannerKind.Streak: DrawStreak(style); break;
                case SurvivorsFeedbackBannerKind.ClassUnlock: DrawClassUnlock(style); break;
                case SurvivorsFeedbackBannerKind.Experience: DrawExperience(style); break;
                case SurvivorsFeedbackBannerKind.Evolution: DrawEvolution(style); break;
            }
        }

        private void DrawReward(GUIStyle style)
        {
            if (RemainingSeconds <= 0f || string.IsNullOrWhiteSpace(Label))
            {
                return;
            }

            float width = Mathf.Min(500f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 9f) * 0.28f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 84f, width, 46f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.015f, 0.02f, 0.028f, 0.74f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(Accent.r, Accent.g, Accent.b, 0.2f + 0.14f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(Accent.r, Accent.g, Accent.b, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), Label, style);
            GUI.color = oldColor;
        }

        private void DrawStreak(GUIStyle style)
        {
            if (RemainingSeconds <= 0f || string.IsNullOrWhiteSpace(Label))
            {
                return;
            }

            float width = Mathf.Min(360f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 10f) * 0.28f;
            float x = Mathf.Max(16f, Screen.width - width - 18f);
            float y = Mathf.Min(144f, Mathf.Max(16f, Screen.height - 58f));
            Rect panel = new Rect(x, y, width, 44f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.02f, 0.018f, 0.024f, 0.72f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(Accent.r, Accent.g, Accent.b, 0.22f + 0.12f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(Accent.r, Accent.g, Accent.b, 0.95f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, 5f, panel.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), Label, style);
            GUI.color = oldColor;
        }

        private void DrawClassUnlock(GUIStyle style)
        {
            if (RemainingSeconds <= 0f || string.IsNullOrWhiteSpace(Label))
            {
                return;
            }

            float width = Mathf.Min(500f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.7f + Mathf.Sin(Time.unscaledTime * 8f) * 0.3f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 188f, width, 44f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.032f, 0.018f, 0.012f, 0.78f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.62f, 0.24f, 0.22f + 0.16f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.78f, 0.36f, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panel.x, panel.y + panel.height - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), Label, style);
            GUI.color = oldColor;
        }

        private void DrawExperience(GUIStyle style)
        {
            if (RemainingSeconds <= 0f || string.IsNullOrWhiteSpace(Label))
            {
                return;
            }

            float width = Mathf.Min(420f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.74f + Mathf.Sin(Time.unscaledTime * 11f) * 0.26f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 136f, width, 42f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.018f, 0.024f, 0.03f, 0.7f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.22f, 0.9f, 1f, 0.2f + 0.16f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.22f, 0.9f, 1f, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y + panel.height - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 5f, panel.width - 24f, panel.height - 10f), Label, style);
            GUI.color = oldColor;
        }

        private void DrawEvolution(GUIStyle style)
        {
            if (RemainingSeconds <= 0f || string.IsNullOrWhiteSpace(Label))
            {
                return;
            }

            float width = Mathf.Min(460f, Mathf.Max(0f, Screen.width - 32f));
            if (width <= 0f)
            {
                return;
            }

            float pulse = 0.7f + Mathf.Sin(Time.unscaledTime * 9f) * 0.3f;
            Rect panel = new Rect(Screen.width * 0.5f - width * 0.5f, 88f, width, 44f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.03f, 0.018f, 0.04f, 0.76f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.78f, 0.22f, 0.2f + 0.16f * pulse);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.78f, 0.22f, 0.96f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panel.x, panel.y + panel.height - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 6f, panel.width - 24f, panel.height - 12f), Label, style);
            GUI.color = oldColor;
        }
    }
}
