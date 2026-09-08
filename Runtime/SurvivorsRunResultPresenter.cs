using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsRunResultPort
    {
        SurvivorsRunResultView ReadSummary();
        IReadOnlyList<SurvivorsResultClassChoice> ReadClasses();
        SurvivorsResultMetaView ReadMeta();
        void SelectClass(int index);
        void PurchaseMeta(int index);
        void Continue();
        void Restart();
        void ChangeMode();
        void PlaySelect();
    }
    /// <summary>Renders copied result text/options and sends explicit next-run or purchase commands.</summary>
    internal sealed class SurvivorsRunResultPresenter
    {
        private readonly ISurvivorsRunResultPort _port;
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly SurvivorsHudStyles _styles;
        private Vector2 _scroll;
        public SurvivorsRunResultPresenter(ISurvivorsRunResultPort port, Func<SurvivorsUiTheme> theme, SurvivorsHudStyles styles)
        { _port = port; _theme = theme; _styles = styles; }
        public void ResetScroll() => _scroll = Vector2.zero;
        public void Draw(bool victory)
        {
            SurvivorsRunResultView view = _port.ReadSummary();
            SurvivorsScreenLayout.DrawDimOverlay(0.72f);
            Rect rect = SurvivorsScreenLayout.ResolveCenteredPanelRect(760f, 700f, 320f, 400f, 20f);
            Color accent = _theme().GetHudAccentColor(new Color(0.2f, 0.78f, 1f));
            SurvivorsScreenLayout.DrawSolidRect(rect, new Color(0.014f, 0.018f, 0.027f, 0.97f));
            SurvivorsScreenLayout.DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 4f), new Color(accent.r, accent.g, accent.b, 0.95f));
            string title = string.IsNullOrWhiteSpace(view.Title)
                ? _theme().runSummaryTitle + " - " + (victory ? "Victory" : "Defeat")
                : view.Title;
            GUI.Label(new Rect(rect.x + 24f, rect.y + 18f, rect.width - 48f, 34f), title, _styles.MenuTitleStyle);

            IReadOnlyList<string> lines = view.Lines;
            int lineCount = lines == null ? 0 : lines.Count;
            Rect viewRect = new Rect(rect.x + 24f, rect.y + 64f, rect.width - 48f, rect.height - 130f);
            float lineHeight = 22f;
            float summaryHeight = Mathf.Max(58f, Mathf.Max(1, lineCount) * lineHeight + 12f);
            float contentWidth = Mathf.Max(1f, viewRect.width - 18f);
            float classOptionsY = summaryHeight + 8f;
            float metaOptionsY = classOptionsY + 82f;
            float contentHeight = Mathf.Max(viewRect.height, metaOptionsY + 132f);
            _scroll = GUI.BeginScrollView(viewRect, _scroll, new Rect(0f, 0f, contentWidth, contentHeight));
            if (lineCount == 0)
            {
                GUI.Label(new Rect(0f, 0f, contentWidth, 22f), view.Rewards, _styles.HudLabelStyle);
            }
            else
            {
                for (int i = 0; i < lineCount; i++)
                {
                    GUI.Label(new Rect(0f, i * lineHeight, contentWidth, lineHeight), lines[i], _styles.HudSmallStyle);
                }
            }

            DrawResultClassOptions(new Rect(0f, classOptionsY, contentWidth, 66f));
            DrawResultMetaUpgradeOptions(new Rect(0f, metaOptionsY, contentWidth, 96f));
            GUI.EndScrollView();

            float buttonY = rect.yMax - 48f;
            if (victory)
            {
                int buttonCount = view.EndlessEnabled ? 3 : 2;
                float gap = 10f;
                float buttonWidth = (rect.width - 48f - gap * (buttonCount - 1)) / buttonCount;
                float x = rect.x + 24f;
                if (view.EndlessEnabled && GUI.Button(new Rect(x, buttonY, buttonWidth, 36f), _theme().continueButtonLabel))
                {
                    _port.PlaySelect();
                    _port.Continue();
                }

                if (view.EndlessEnabled)
                {
                    x += buttonWidth + gap;
                }

                if (GUI.Button(new Rect(x, buttonY, buttonWidth, 36f), _theme().restartSameButtonLabel))
                {
                    _port.PlaySelect();
                    _port.Restart();
                }

                x += buttonWidth + gap;
                if (GUI.Button(new Rect(x, buttonY, buttonWidth, 36f), _theme().changeModeButtonLabel))
                {
                    _port.PlaySelect();
                    _port.ChangeMode();
                }
            }
            else
            {
                float gap = 10f;
                float buttonWidth = (rect.width - 48f - gap) * 0.5f;
                if (GUI.Button(new Rect(rect.x + 24f, buttonY, buttonWidth, 36f), _theme().restartSameButtonLabel))
                {
                    _port.PlaySelect();
                    _port.Restart();
                }

                if (GUI.Button(new Rect(rect.x + 24f + buttonWidth + gap, buttonY, buttonWidth, 36f), _theme().changeModeButtonLabel))
                {
                    _port.PlaySelect();
                    _port.ChangeMode();
                }
            }
        }

        private void DrawResultClassOptions(Rect rect)
        {
            IReadOnlyList<SurvivorsResultClassChoice> options = _port.ReadClasses();
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20f), "Next Run Class", _styles.HudLabelStyle);
            if (options.Count == 0)
            {
                GUI.Label(new Rect(rect.x, rect.y + 24f, rect.width, 20f), "No class definitions found.", _styles.HudSmallStyle);
                return;
            }

            float gap = 8f;
            float buttonWidth = (rect.width - gap * (options.Count - 1)) / options.Count;
            for (int i = 0; i < options.Count; i++)
            {
                SurvivorsResultClassChoice option = options[i];
                bool unlocked = option.Unlocked;
                bool selected = option.Selected;
                Rect buttonRect = new Rect(rect.x + i * (buttonWidth + gap), rect.y + 24f, buttonWidth, 42f);

                bool previousEnabled = GUI.enabled;
                Color previousBackgroundColor = GUI.backgroundColor;
                GUI.enabled = previousEnabled && unlocked && !selected;
                GUI.backgroundColor = selected
                    ? new Color(0.48f, 0.84f, 0.62f)
                    : (unlocked ? new Color(0.8f, 0.58f, 1f) : new Color(0.38f, 0.38f, 0.38f));

                if (GUI.Button(buttonRect, option.Label))
                {
                    _port.SelectClass(i);
                    options = _port.ReadClasses();
                }

                GUI.backgroundColor = previousBackgroundColor;
                GUI.enabled = previousEnabled;
            }
        }

        private void DrawResultMetaUpgradeOptions(Rect rect)
        {
            SurvivorsResultMetaView view = _port.ReadMeta();
            IReadOnlyList<string> options = view.Labels;
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20f), view.Title, _styles.HudLabelStyle);
            if (options.Count == 0)
            {
                GUI.Label(new Rect(rect.x, rect.y + 24f, rect.width, 20f), view.EmptyLabel, _styles.HudSmallStyle);
                return;
            }

            for (int i = 0; i < options.Count; i++)
            {
                Rect buttonRect = new Rect(rect.x, rect.y + 24f + i * 28f, rect.width, 24f);
                if (GUI.Button(buttonRect, options[i]))
                {
                    _port.PurchaseMeta(i);
                    return;
                }
            }
        }
    }
}
