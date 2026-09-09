using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsTutorialPresenter
    {
        private readonly SurvivorsMenuSession _menu;
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly SurvivorsHudStyles _styles;
        private readonly Func<int, string> _title;
        private readonly Func<int, IReadOnlyList<string>> _lines;
        public SurvivorsTutorialPresenter(SurvivorsMenuSession menu, Func<SurvivorsUiTheme> theme, SurvivorsHudStyles styles,
            Func<int, string> title, Func<int, IReadOnlyList<string>> lines)
        { _menu = menu; _theme = theme; _styles = styles; _title = title; _lines = lines; }
        public void Draw()
        {
            SurvivorsScreenLayout.DrawDimOverlay(0.7f);
            Rect panel = SurvivorsScreenLayout.ResolveCenteredPanelRect(720f, 420f, 320f, 330f, 22f);
            Color accent = _theme().GetHudAccentColor(new Color(0.2f, 0.78f, 1f));
            SurvivorsScreenLayout.DrawSolidRect(panel, new Color(0.016f, 0.02f, 0.03f, 0.97f));
            SurvivorsScreenLayout.DrawSolidRect(new Rect(panel.x, panel.y, panel.width, 4f), new Color(accent.r, accent.g, accent.b, 0.94f));
            int step = SurvivorsTutorialContent.ClampTutorialStepIndex(_menu.TutorialIndex);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 20f, panel.width - 48f, 32f), _theme().tutorialTitle, _styles.MenuTitleStyle);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 56f, panel.width - 48f, 28f), $"{step + 1}/{SurvivorsTutorialContent.StepCount}  {_title(step)}", _styles.HudLabelStyle);

            IReadOnlyList<string> lines = _lines(step);
            float y = panel.y + 104f;
            for (int i = 0; i < lines.Count; i++)
            {
                GUI.Label(new Rect(panel.x + 28f, y, panel.width - 56f, 34f), lines[i], _styles.DraftCardDescriptionStyle);
                y += 42f;
            }

            float buttonY = panel.yMax - 52f;
            if (GUI.Button(new Rect(panel.x + 24f, buttonY, 108f, 34f), "Back"))
            {
                _menu.BackTutorialStep();
            }

            if (GUI.Button(new Rect(panel.x + 144f, buttonY, 120f, 34f), "Skip"))
            {
                _menu.CloseTutorialOverlay(markSeen: true);
            }

            string nextLabel = step >= SurvivorsTutorialContent.StepCount - 1 ? "Finish" : "Next";
            if (GUI.Button(new Rect(panel.xMax - 152f, buttonY, 128f, 34f), nextLabel))
            {
                _menu.AdvanceTutorialStep();
            }
        }
    }
}
