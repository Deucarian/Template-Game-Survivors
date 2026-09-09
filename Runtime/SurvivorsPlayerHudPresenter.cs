using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsPlayerHudLayout
    {
        internal SurvivorsPlayerHudLayout(int screenWidth, int lineCount)
        {
            VisibleLineCount = Mathf.Min(4, lineCount);
            float width = Mathf.Min(340f, Mathf.Max(270f, screenWidth - 32f));
            Panel = new Rect(12f, 58f, width, 126f + VisibleLineCount * 19f);
        }
        internal Rect Panel { get; }
        internal int VisibleLineCount { get; }
        internal Rect Row(int index) => new Rect(Panel.x + 14f, Panel.y + 110f + index * 19f, Panel.width - 28f, 19f);
    }

    /// <summary>Draws the player panel from copied bars and existing player-facing rows.</summary>
    internal static class SurvivorsPlayerHudPresenter
    {
        internal static void Draw(int screenWidth, IReadOnlyList<string> lines, string modeName,
            in SurvivorsHudVitals vitals, SurvivorsHudStyles styles)
        {
            var layout = new SurvivorsPlayerHudLayout(screenWidth, lines.Count);
            Rect panel = layout.Panel;
            SurvivorsScreenLayout.DrawSolidRect(panel, new Color(0.015f, 0.02f, 0.026f, 0.74f));
            SurvivorsScreenLayout.DrawSolidRect(new Rect(panel.x, panel.y, 4f, panel.height), new Color(0.2f, 0.78f, 1f, 0.9f));
            GUI.Label(new Rect(panel.x + 14f, panel.y + 8f, panel.width - 28f, 22f), modeName, styles.HudTitleStyle);
            SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 14f, panel.y + 36f, panel.width - 28f, 18f),
                "Health", vitals.HealthRatio, new Color(0.9f, 0.22f, 0.24f), styles.HudSmallStyle);
            if (vitals.ShowPlayerBarrier)
            {
                SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 14f, panel.y + 60f, panel.width - 28f, 18f),
                    "Barrier", vitals.BarrierRatio, new Color(0.42f, 0.8f, 1f), styles.HudSmallStyle);
            }
            SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 14f, panel.y + 84f, panel.width - 28f, 18f),
                $"XP L{vitals.Level}", vitals.ExperienceRatio, new Color(0.2f, 0.78f, 1f), styles.HudSmallStyle);
            for (int i = 0; i < layout.VisibleLineCount; i++) GUI.Label(layout.Row(i), lines[i], styles.HudSmallStyle);
        }
    }
}
