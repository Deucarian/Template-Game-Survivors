using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Pure layout rules over viewport size and already projected positions.</summary>
    internal static class SurvivorsThreatHudLayout
    {
        public static Rect BossPanel(Vector2 viewport, int index, bool majorWarning, bool hordeWarning)
        {
            float width = Mathf.Min(420f, Mathf.Max(260f, viewport.x - 48f));
            float x = Mathf.Max(24f, viewport.x - width - 24f);
            float y = 24f + (majorWarning ? 60f : 0f) + (hordeWarning ? 52f : 0f);
            return new Rect(x, y + index * 50f, width, 46f);
        }

        public static Rect OverheadPanel(Vector2 viewport, Vector3 screen, SurvivorsEnemyRole role)
        {
            float width = role == SurvivorsEnemyRole.Miniboss ? 190f : 156f;
            const float height = 28f;
            float x = Mathf.Clamp(screen.x - width * 0.5f, 12f, Mathf.Max(12f, viewport.x - width - 12f));
            float y = Mathf.Clamp(viewport.y - screen.y, 58f, Mathf.Max(58f, viewport.y - height - 12f));
            return new Rect(x, y, width, height);
        }

        public static bool TryMarkerPanel(Vector2 viewport, Vector3 delta, out Rect panel)
        {
            panel = default;
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.0001f) return false;
            Vector2 direction = new Vector2(delta.x, -delta.z).normalized;
            if (direction.sqrMagnitude <= 0.0001f) return false;

            const float width = 248f;
            const float height = 38f;
            const float margin = 18f;
            float halfWidth = viewport.x * 0.5f;
            float halfHeight = viewport.y * 0.5f;
            float xLimit = Mathf.Max(1f, halfWidth - width * 0.5f - margin);
            float yLimit = Mathf.Max(1f, halfHeight - height * 0.5f - margin);
            float xScale = Mathf.Abs(direction.x) <= 0.001f ? float.MaxValue : xLimit / Mathf.Abs(direction.x);
            float yScale = Mathf.Abs(direction.y) <= 0.001f ? float.MaxValue : yLimit / Mathf.Abs(direction.y);
            Vector2 center = new Vector2(halfWidth, halfHeight) + direction * Mathf.Min(xScale, yScale);
            center.x = Mathf.Clamp(center.x, margin + width * 0.5f, viewport.x - margin - width * 0.5f);
            center.y = Mathf.Clamp(center.y, 96f + height * 0.5f, viewport.y - margin - height * 0.5f);
            panel = new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
            return true;
        }
    }
}
