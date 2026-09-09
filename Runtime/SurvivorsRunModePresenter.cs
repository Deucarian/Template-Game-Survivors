using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsRunModePort
    {
        bool CanStart { get; }
        bool StrictAuthored { get; }
        string AuthoredStatus { get; }
        IReadOnlyList<SurvivorsUiTheme> Themes { get; }
        int SelectedThemeIndex { get; }
        SurvivorsRunModeCardView ReadCard(SurvivorsPacingProfile profile);
        void Start(SurvivorsPacingProfile profile);
        void OpenTutorial();
        void EnsureThemes();
        void SelectTheme(int index);
        void Hover();
        void Select();
    }
    /// <summary>Owns mode cards, theme-selection rendering and scroll state over bounded reads and commands.</summary>
    internal sealed class SurvivorsRunModePresenter
    {
        private readonly ISurvivorsRunModePort _port;
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly SurvivorsHudStyles _styles;
        private Vector2 _scroll;
        public SurvivorsRunModePresenter(ISurvivorsRunModePort port, Func<SurvivorsUiTheme> theme, SurvivorsHudStyles styles)
        { _port = port; _theme = theme; _styles = styles; }
        public void ResetScroll() => _scroll = Vector2.zero;
        public void Draw()
        {
            Rect rect = SurvivorsScreenLayout.ResolveCenteredPanelRect(820f, 430f, 340f, 360f, 20f);
            float width = rect.width;
            GUI.Box(rect, "Choose Run Mode");
            GUI.Label(new Rect(rect.x + 28f, rect.y + 34f, width - 56f, 28f), "Deucarian Survivors Run", _styles.HudTitleStyle);
            GUI.Label(new Rect(rect.x + 28f, rect.y + 64f, width - 56f, 22f), "Pick Standard / Human Playtest for the full 30-minute arc or Sprint Run for the compact five-minute loop.", _styles.HudLabelStyle);

            float viewOffset = 98f;
            float viewBottomPadding = 154f;
            if (_port.StrictAuthored && !_port.CanStart)
            {
                GUI.Label(new Rect(rect.x + 28f, rect.y + 86f, width - 56f, 34f), _port.AuthoredStatus, _styles.HudSmallStyle);
                viewOffset = 122f;
                viewBottomPadding = 178f;
            }

            Rect viewRect = new Rect(rect.x + 28f, rect.y + viewOffset, width - 56f, rect.height - viewBottomPadding);
            float contentWidth = Mathf.Max(1f, viewRect.width - 18f);
            bool narrow = contentWidth < 700f;
            float cardGap = 16f;
            float cardWidth = narrow ? contentWidth : (contentWidth - cardGap) * 0.5f;
            float cardHeight = narrow ? 132f : 190f;
            float selectorY = narrow ? cardHeight * 2f + cardGap + 12f : cardHeight + 14f;
            float selectorHeight = contentWidth < 360f ? 64f : 46f;
            float contentHeight = Mathf.Max(viewRect.height, selectorY + selectorHeight);

            _scroll = GUI.BeginScrollView(viewRect, _scroll, new Rect(0f, 0f, contentWidth, contentHeight));
            Rect standardRect = new Rect(0f, 0f, cardWidth, cardHeight);
            Rect sprintRect = narrow
                ? new Rect(0f, standardRect.yMax + cardGap, cardWidth, cardHeight)
                : new Rect(standardRect.xMax + cardGap, 0f, cardWidth, cardHeight);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && _port.CanStart;
            if (DrawRunModeCard(standardRect, SurvivorsPacingProfile.HumanPlaytest, "Start Standard"))
            {
                _port.Start(SurvivorsPacingProfile.HumanPlaytest);
            }

            if (DrawRunModeCard(sprintRect, SurvivorsPacingProfile.SprintRun, "Start Sprint"))
            {
                _port.Start(SurvivorsPacingProfile.SprintRun);
            }
            GUI.enabled = previousEnabled;

            DrawThemeSelector(new Rect(0f, selectorY, contentWidth, 42f));
            GUI.EndScrollView();

            if (GUI.Button(new Rect(rect.x + 28f, rect.yMax - 42f, Mathf.Min(180f, width - 56f), 30f), _theme().showTutorialButtonLabel + " (T)"))
            {
                _port.OpenTutorial();
            }
        }

        private bool DrawRunModeCard(Rect rect, SurvivorsPacingProfile profile, string buttonLabel)
        {
            SurvivorsRunModeCardView preview = _port.ReadCard(profile);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 16f, rect.width - 32f, 24f), preview.Title, _styles.HudLabelStyle);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 44f, rect.width - 32f, 20f), preview.Duration, _styles.HudSmallStyle);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 70f, rect.width - 32f, 42f), preview.Description, _styles.HudSmallStyle);
            if (rect.height > 150f)
            {
                GUI.Label(new Rect(rect.x + 16f, rect.y + 118f, rect.width - 32f, 20f), preview.Milestones, _styles.HudSmallStyle);
                GUI.Label(new Rect(rect.x + 16f, rect.y + 140f, rect.width - 32f, 20f), preview.Profile, _styles.HudSmallStyle);
            }

            bool hovered = rect.Contains(Event.current.mousePosition);
            if (hovered)
            {
                _port.Hover();
            }

            bool clicked = GUI.Button(new Rect(rect.x + 16f, rect.yMax - 38f, rect.width - 32f, 30f), buttonLabel);
            if (clicked)
            {
                _port.Select();
            }

            return clicked;
        }

        private void DrawThemeSelector(Rect rect)
        {
            _port.EnsureThemes();
            bool stacked = rect.width < 360f;
            GUI.Label(new Rect(rect.x, rect.y, stacked ? rect.width : Mathf.Min(96f, rect.width), 24f), _theme().themeSelectorTitle, _styles.HudLabelStyle);
            float x = stacked ? rect.x : rect.x + 104f;
            float y = stacked ? rect.y + 24f : rect.y;
            float availableWidth = Mathf.Max(1f, rect.xMax - x);
            float width = Mathf.Max(96f, Mathf.Min(180f, (availableWidth - 8f) / Mathf.Max(1, _port.Themes.Count)));
            for (int i = 0; i < _port.Themes.Count; i++)
            {
                SurvivorsUiTheme option = _port.Themes[i];
                if (option == null)
                {
                    continue;
                }

                Rect button = new Rect(x + i * (width + 8f), y, width, 32f);
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = i == _port.SelectedThemeIndex
                    ? _theme().GetHudAccentColor(new Color(0.2f, 0.78f, 1f))
                    : new Color(0.72f, 0.72f, 0.76f);
                if (GUI.Button(button, option.themeName))
                {
                    _port.SelectTheme(i);
                }

                GUI.backgroundColor = oldColor;
            }
        }
    }
}
