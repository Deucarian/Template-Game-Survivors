using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Renders a completed card projection without access to the draft command/state owner.</summary>
    internal static class SurvivorsDraftCardPresenter
    {
        public static void Draw(Rect rect, SurvivorsDraftCard card, bool hover, SurvivorsUiTheme theme, SurvivorsHudStyles styles)
        {
            float pulse = 0.72f + Mathf.Sin(Time.unscaledTime * (card.IsEvolution ? 5.5f : 3.2f)) * 0.14f;
            Color accent = card.AccentColor;
            DrawSolidRect(rect, new Color(0.022f, 0.027f, 0.038f, 0.96f));
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, rect.height), new Color(accent.r, accent.g, accent.b, hover ? 0.18f : 0.1f));
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 8f), new Color(accent.r, accent.g, accent.b, hover ? 1f : 0.82f));
            DrawSolidRect(new Rect(rect.x, rect.y, 4f, rect.height), new Color(accent.r, accent.g, accent.b, card.IsEvolution ? 0.94f : 0.78f));
            DrawSolidRect(new Rect(rect.xMax - 4f, rect.y, 4f, rect.height), new Color(accent.r, accent.g, accent.b, hover ? 0.74f : 0.38f));
            if (card.IsEvolution || hover)
            {
                DrawSolidRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, 2f), new Color(accent.r, accent.g, accent.b, 0.22f + pulse * 0.18f));
            }

            Rect hotkey = new Rect(rect.xMax - 54f, rect.y + 18f, 34f, 30f);
            DrawSolidRect(hotkey, new Color(accent.r, accent.g, accent.b, 0.84f));
            GUI.Label(hotkey, card.Hotkey, styles.DraftCardHotkeyStyle);
            Rect icon = new Rect(rect.x + 18f, rect.y + 22f, 58f, 58f);
            DrawSolidRect(icon, new Color(0.04f, 0.05f, 0.065f, 0.92f));
            DrawSolidRect(new Rect(icon.x, icon.yMax - 3f, icon.width, 3f), new Color(accent.r, accent.g, accent.b, 0.86f));
            GUI.Label(icon, theme.iconPlaceholderPrefix + "\n" + card.IconId, styles.DraftCardMetaStyle);

            float x = rect.x + 90f;
            float width = rect.width - 116f;
            GUI.Label(new Rect(x, rect.y + 20f, width - 42f, 20f), card.RarityLabel + "  |  " + card.CategoryLabel, styles.DraftCardMetaStyle);
            GUI.Label(new Rect(x, rect.y + 44f, width - 42f, 42f), card.Name, styles.DraftCardNameStyle);
            if (card.IsEvolution)
            {
                DrawSolidRect(new Rect(rect.x + 18f, rect.y + 92f, rect.width - 36f, 24f), new Color(accent.r, accent.g, accent.b, 0.22f));
                GUI.Label(new Rect(rect.x + 24f, rect.y + 94f, rect.width - 48f, 20f), "EVOLUTION REWARD", styles.DraftCardMetaStyle);
            }

            float y = card.IsEvolution ? rect.y + 124f : rect.y + 94f;
            GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, 22f), card.AffectedLabel + "   " + card.RankLabel, styles.HudSmallStyle);
            y += 28f;
            GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, Mathf.Max(72f, rect.height * 0.25f)), card.Description, styles.DraftCardDescriptionStyle);
            y += Mathf.Max(82f, rect.height * 0.27f);
            GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, 48f), "Effect: " + card.EffectPreview, styles.HudSmallStyle);
            y += 54f;
            if (!string.IsNullOrWhiteSpace(card.RequirementHint))
            {
                GUI.Label(new Rect(rect.x + 20f, y, rect.width - 40f, 44f), card.RequirementHint, styles.HudSmallStyle);
            }
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}
