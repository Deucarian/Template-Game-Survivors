using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsBuildMenuPresenter
    {
        private readonly SurvivorsMenuSession _menu;
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly SurvivorsHudStyles _styles;
        private readonly Func<BuildMenuTab, IReadOnlyList<string>> _lines;
        private readonly Action _playSelect;
        private Vector2 _scroll;
        public SurvivorsBuildMenuPresenter(SurvivorsMenuSession menu, Func<SurvivorsUiTheme> theme, SurvivorsHudStyles styles,
            Func<BuildMenuTab, IReadOnlyList<string>> lines, Action playSelect)
        { _menu = menu; _theme = theme; _styles = styles; _lines = lines; _playSelect = playSelect; }
        public void ResetScroll() => _scroll = Vector2.zero;
        public void Draw()
        {
            SurvivorsScreenLayout.DrawDimOverlay(0.62f);
            Rect panel = SurvivorsScreenLayout.ResolveCenteredPanelRect(860f, 620f, 360f, 360f, 24f);
            SurvivorsScreenLayout.DrawSolidRect(panel, new Color(0.018f, 0.023f, 0.032f, 0.96f));
            Color accent = _theme().GetHudAccentColor(new Color(0.2f, 0.78f, 1f));
            SurvivorsScreenLayout.DrawSolidRect(new Rect(panel.x, panel.y, panel.width, 4f), new Color(accent.r, accent.g, accent.b, 0.9f));
            GUI.Label(new Rect(panel.x + 24f, panel.y + 18f, panel.width - 48f, 32f), _theme().buildMenuTitle, _styles.MenuTitleStyle);
            if (GUI.Button(new Rect(panel.xMax - 94f, panel.y + 18f, 70f, 28f), "Close"))
            {
                _playSelect();
                _menu.BuildOpen = false;
                return;
            }

            DrawBuildMenuTabs(new Rect(panel.x + 24f, panel.y + 62f, panel.width - 48f, 34f));
            IReadOnlyList<string> lines = _lines(_menu.BuildTab);
            Rect viewRect = new Rect(panel.x + 24f, panel.y + 110f, panel.width - 48f, panel.height - 132f);
            float lineHeight = 22f;
            Rect contentRect = new Rect(0f, 0f, Mathf.Max(1f, viewRect.width - 18f), Mathf.Max(viewRect.height, lines.Count * lineHeight + 10f));
            _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);
            float y = 0f;
            for (int i = 0; i < lines.Count; i++)
            {
                GUI.Label(new Rect(0f, y, contentRect.width, lineHeight), lines[i], _styles.HudLabelStyle);
                y += lineHeight;
            }
            GUI.EndScrollView();
        }

        private void DrawBuildMenuTabs(Rect rect)
        {
            float gap = 8f;
            float width = (rect.width - gap * 3f) / 4f;
            DrawBuildMenuTabButton(new Rect(rect.x, rect.y, width, rect.height), BuildMenuTab.CurrentBuild, "1 Current Build");
            DrawBuildMenuTabButton(new Rect(rect.x + (width + gap), rect.y, width, rect.height), BuildMenuTab.Stats, "2 Stats");
            DrawBuildMenuTabButton(new Rect(rect.x + (width + gap) * 2f, rect.y, width, rect.height), BuildMenuTab.RunInfo, "3 Run Info");
            DrawBuildMenuTabButton(new Rect(rect.x + (width + gap) * 3f, rect.y, width, rect.height), BuildMenuTab.Controls, "4 Controls");
        }

        private void DrawBuildMenuTabButton(Rect rect, BuildMenuTab tab, string label)
        {
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = _menu.BuildTab == tab ? new Color(0.35f, 0.76f, 1f) : new Color(0.42f, 0.48f, 0.56f);
            if (GUI.Button(rect, label, _styles.MenuTabStyle))
            {
                _menu.BuildTab = tab;
            }

            GUI.backgroundColor = oldColor;
        }
    }
}
