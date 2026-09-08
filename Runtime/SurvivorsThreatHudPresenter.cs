using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Draws threat observations; projection/color/style dependencies grant no gameplay mutation.</summary>
    internal static class SurvivorsThreatHudPresenter
    {
        public static void DrawLifeBars(SurvivorsThreatHudModel model, Camera camera,
            bool majorWarning, bool hordeWarning, Func<SurvivorsEnemyRole, Color> color, GUIStyle style)
        {
            var viewport = new Vector2(Screen.width, Screen.height);
            int drawn = 0;
            for (int i = 0; i < model.Items.Count; i++)
            {
                SurvivorsThreatHudItem item = model.Items[i];
                if (!item.IsAlive || !item.ShowBossLifeBar) continue;
                Rect panel = SurvivorsThreatHudLayout.BossPanel(viewport, drawn++, majorWarning, hordeWarning);
                DrawBackground(panel, new Color(0.03f, 0.02f, 0.02f, 0.66f));
                SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 10f, panel.y + 13f, panel.width - 20f, 20f),
                    item.Name + " HP", item.HealthFraction, color(item.Role), style);
            }

            if (camera == null) return;
            for (int i = 0; i < model.Items.Count; i++)
            {
                SurvivorsThreatHudItem item = model.Items[i];
                if (!item.IsAlive || !item.ShowOverheadLifeBar) continue;
                Vector3 world = item.Position + Vector3.up * Mathf.Max(1.2f, item.Radius + 0.95f);
                Vector3 screen = camera.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;
                Rect panel = SurvivorsThreatHudLayout.OverheadPanel(viewport, screen, item.Role);
                DrawBackground(panel, new Color(0.025f, 0.018f, 0.02f, 0.72f));
                SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 6f, panel.y + 6f, panel.width - 12f, 16f),
                    item.Name, item.HealthFraction, color(item.Role), style);
            }
        }

        public static void DrawMarker(SurvivorsThreatHudItem? selected, Vector3 player, Color color, GUIStyle style)
        {
            if (!selected.HasValue) return;
            SurvivorsThreatHudItem item = selected.Value;
            if (!SurvivorsThreatHudLayout.TryMarkerPanel(new Vector2(Screen.width, Screen.height),
                item.Position - player, out Rect panel)) return;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.02f, 0.015f, 0.02f, 0.76f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(color.r, color.g, color.b, 0.92f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - 3f, panel.width, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 10f, panel.y + 7f, panel.width - 20f, panel.height - 12f),
                SurvivorsThreatHudModel.FormatMarkerLabel(item, player), style);
            GUI.color = oldColor;
        }

        private static void DrawBackground(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
