using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Renders prepared cards and delegates only selection commands to the draft session.</summary>
    internal sealed class SurvivorsDraftScreenPresenter
    {
        private readonly SurvivorsDraftSession _session;
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly SurvivorsHudStyles _styles;
        private readonly Func<int, SurvivorsDraftCard> _readCard;
        private readonly Func<string> _title;
        private readonly Func<Color> _accent;
        private readonly Func<int> _skipReward;
        private Vector2 _scroll;
        private bool IsRelicChoiceOpen => _session.Kind == SurvivorsRewardSelectionKind.BossRelic;
        public SurvivorsDraftScreenPresenter(SurvivorsDraftSession session, Func<SurvivorsUiTheme> theme, SurvivorsHudStyles styles,
            Func<int, SurvivorsDraftCard> readCard, Func<string> title, Func<Color> accent, Func<int> skipReward)
        { _session = session; _theme = theme; _styles = styles; _readCard = readCard; _title = title; _accent = accent; _skipReward = skipReward; }
        public void ResetScroll() => _scroll = Vector2.zero;
        public void Draw()
        {
            bool upgradeDraftOpen = !IsRelicChoiceOpen;
            int choiceCount = IsRelicChoiceOpen ? (_session.CurrentRelicDraft?.Choices.Count ?? 0) : (_session.CurrentDraft?.Choices.Count ?? 0);
            SurvivorsScreenLayout.DrawDimOverlay(0.72f);
            Rect rect = SurvivorsScreenLayout.ResolveCenteredPanelRect(1120f, 690f, 320f, 420f, 20f);
            SurvivorsScreenLayout.DrawSolidRect(rect, new Color(0.014f, 0.018f, 0.027f, 0.96f));
            SurvivorsScreenLayout.DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 4f), _accent());
            string title = _title();
            GUI.Label(new Rect(rect.x + 30f, rect.y + 20f, rect.width - 60f, 38f), title, _styles.DraftTitleStyle);
            GUI.Label(
                new Rect(rect.x + 30f, rect.y + 58f, rect.width - 60f, 22f),
                _session.RemainingSeconds > 0f ? $"Auto-pick in {_session.RemainingSeconds:0}s" : "Choose one card. Mouse, 1/2/3, R reroll, Shift+number banish, S skip.",
                _styles.HudSmallStyle);

            if (choiceCount <= 0)
            {
                GUI.Label(new Rect(rect.x + 30f, rect.y + 110f, rect.width - 60f, 28f), "No rewards available.", _styles.HudLabelStyle);
                return;
            }

            var layout = new SurvivorsDraftScreenLayout(rect, upgradeDraftOpen, choiceCount);
            if (!layout.Horizontal)
            {
                _scroll = GUI.BeginScrollView(layout.View, _scroll, layout.Content);
            }

            for (int i = 0; i < choiceCount; i++)
            {
                Rect cardRect = layout.Card(i);
                SurvivorsDraftCard card = _readCard(i);
                bool hover = cardRect.Contains(Event.current.mousePosition);
                SurvivorsDraftCardPresenter.Draw(cardRect, card, hover, _theme(), _styles);
                Rect selectRect = layout.Selection(cardRect);
                if (GUI.Button(selectRect, GUIContent.none, _styles.TransparentButtonStyle))
                {
                    if (_session.Select(i))
                    {
                        return;
                    }
                }

                if (upgradeDraftOpen)
                {
                    bool previousEnabled = GUI.enabled;
                    GUI.enabled = previousEnabled && _session.CanBanish;
                    Rect banishRect = new Rect(cardRect.x + 14f, cardRect.yMax - 40f, Mathf.Min(112f, cardRect.width - 28f), 28f);
                    if (GUI.Button(banishRect, _theme().banishButtonLabel))
                    {
                        if (_session.Banish(i))
                        {
                            GUI.enabled = previousEnabled;
                            return;
                        }
                    }

                    GUI.enabled = previousEnabled;
                }
            }

            if (!layout.Horizontal)
            {
                GUI.EndScrollView();
            }

            if (upgradeDraftOpen)
            {
                float footerY = rect.yMax - 58f;
                float footerX = rect.x + 30f;
                float footerWidth = rect.width - 60f;
                float footerGap = 10f;
                bool compactFooter = footerWidth < 560f;
                float buttonWidth = compactFooter ? (footerWidth - footerGap) * 0.5f : 170f;
                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && _session.CanReroll;
                if (GUI.Button(new Rect(footerX, footerY, buttonWidth, 34f), $"{_theme().rerollButtonLabel} ({_session.RerollsRemaining})"))
                {
                    if (_session.Reroll())
                    {
                        GUI.enabled = previousEnabled;
                        return;
                    }
                }

                GUI.enabled = previousEnabled && _session.CanSkip;
                float skipX = footerX + buttonWidth + footerGap;
                float skipWidth = compactFooter ? buttonWidth : 210f;
                if (GUI.Button(new Rect(skipX, footerY, skipWidth, 34f), $"{_theme().skipButtonLabel} (+{_skipReward()})"))
                {
                    if (_session.Skip())
                    {
                        GUI.enabled = previousEnabled;
                        return;
                    }
                }

                GUI.enabled = previousEnabled;
                if (!compactFooter)
                {
                    GUI.Label(
                        new Rect(rect.x + 438f, footerY + 7f, rect.width - 468f, 20f),
                        $"Banishes {_session.BanishesRemaining}",
                        _styles.HudSmallStyle);
                }
            }
        }
    }
}
