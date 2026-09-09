using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsDebugHudLayout
    {
        internal SurvivorsDebugHudLayout(int screenHeight, string evolutionObjective)
        {
            float height = string.IsNullOrWhiteSpace(evolutionObjective) ? 424f : 448f;
            Panel = new Rect(12f, 58f + Mathf.Min(206f, screenHeight * 0.22f), 356f, height);
        }
        internal Rect Panel { get; }
        internal Rect Row(int index) => new Rect(Panel.x + 12f, Panel.y + 136f + index * 22f, 318f, 22f);
        internal static bool UsesLabelStyle(int index) => index < 4;
    }

    /// <summary>Draws debug observations with the existing bar, row and optional-evolution geometry.</summary>
    internal static class SurvivorsDebugHudPresenter
    {
        internal static void Draw(int screenHeight, in SurvivorsDebugHudValues values, SurvivorsHudStyles styles)
        {
            var layout = new SurvivorsDebugHudLayout(screenHeight, values.EvolutionObjective);
            IReadOnlyList<string> lines = SurvivorsDebugHudModel.BuildRows(values);
            Rect panel = layout.Panel;
            SurvivorsScreenLayout.DrawSolidRect(panel, new Color(0.02f, 0.024f, 0.03f, 0.86f));
            SurvivorsScreenLayout.DrawSolidRect(new Rect(panel.x, panel.y, 4f, panel.height), new Color(1f, 0.72f, 0.22f, 0.86f));
            GUI.Label(new Rect(panel.x + 12f, panel.y + 10f, 300f, 22f), "Survivors Debug Overlay", styles.HudTitleStyle);
            SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 12f, panel.y + 38f, 318f, 18f),
                "Health", values.Vitals.HealthRatio, new Color(0.9f, 0.22f, 0.24f), styles.HudSmallStyle);
            SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 12f, panel.y + 62f, 318f, 18f),
                "Barrier", values.Vitals.BarrierRatio, new Color(0.42f, 0.8f, 1f), styles.HudSmallStyle);
            SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 12f, panel.y + 86f, 318f, 18f),
                "XP", values.Vitals.ExperienceRatio, new Color(0.2f, 0.78f, 1f), styles.HudSmallStyle);
            SurvivorsStatusHudPresenter.DrawBar(new Rect(panel.x + 12f, panel.y + 110f, 318f, 18f),
                "Run", SurvivorsDebugHudModel.RunRatio(values), new Color(0.72f, 0.44f, 1f), styles.HudSmallStyle);
            for (int i = 0; i < lines.Count; i++)
                GUI.Label(layout.Row(i), lines[i], SurvivorsDebugHudLayout.UsesLabelStyle(i) ? styles.HudLabelStyle : styles.HudSmallStyle);
        }
    }
}
