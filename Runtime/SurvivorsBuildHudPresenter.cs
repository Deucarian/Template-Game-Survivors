using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsBuildHudPanelLayout
    {
        public SurvivorsBuildHudPanelLayout(int screenWidth, float panelWidth, int lineCount)
        {
            VisibleLineCount = Mathf.Min(18, lineCount);
            HiddenLineCount = lineCount - VisibleLineCount;
            float height = 42f + (VisibleLineCount * 19f) + (HiddenLineCount > 0 ? 19f : 0f);
            Panel = new Rect(screenWidth - panelWidth - 12f, 12f, panelWidth, height);
        }
        public Rect Panel { get; }
        public int VisibleLineCount { get; }
        public int HiddenLineCount { get; }
    }

    /// <summary>Owns the compact build panel's visibility, row clipping and drawing over prepared text.</summary>
    internal sealed class SurvivorsBuildHudPresenter
    {
        private readonly Func<IReadOnlyList<string>> _lines;
        private readonly SurvivorsHudStyles _styles;
        public SurvivorsBuildHudPresenter(Func<IReadOnlyList<string>> lines, SurvivorsHudStyles styles)
        {
            _lines = lines ?? throw new ArgumentNullException(nameof(lines));
            _styles = styles ?? throw new ArgumentNullException(nameof(styles));
        }

        public bool TryPrepare(int screenWidth, out SurvivorsBuildHudPanelLayout layout, out IReadOnlyList<string> lines)
        {
            layout = default;
            lines = null;
            float panelWidth = Mathf.Min(382f, screenWidth - 392f);
            if (panelWidth < 300f)
            {
                return false;
            }

            lines = _lines();
            layout = new SurvivorsBuildHudPanelLayout(screenWidth, panelWidth, lines.Count);
            return true;
        }

        public void Draw()
        {
            if (!TryPrepare(Screen.width, out SurvivorsBuildHudPanelLayout layout, out IReadOnlyList<string> lines))
            {
                return;
            }

            Rect panel = layout.Panel;
            GUI.Box(panel, string.Empty);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 22f), "Current Build", _styles.HudTitleStyle);
            float y = panel.y + 34f;
            for (int i = 0; i < layout.VisibleLineCount; i++)
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 19f), lines[i], _styles.HudSmallStyle);
                y += 19f;
            }

            if (layout.HiddenLineCount > 0)
            {
                GUI.Label(new Rect(panel.x + 12f, y, panel.width - 24f, 19f), "+" + layout.HiddenLineCount.ToString() + " more", _styles.HudSmallStyle);
            }
        }
    }
}
