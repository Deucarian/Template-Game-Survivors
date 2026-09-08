using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Responsive draft geometry, including the distinct relic and upgrade selection areas.</summary>
    internal readonly struct SurvivorsDraftScreenLayout
    {
        private const float Gap = 16f;
        private readonly Rect _panel;
        private readonly float _top, _width, _height;
        private readonly bool _upgrade;
        public readonly bool Horizontal;
        public readonly Rect View, Content;
        public SurvivorsDraftScreenLayout(Rect panel, bool upgrade, int choiceCount)
        {
            _panel = panel;
            _upgrade = upgrade;
            Horizontal = panel.width >= 760f;
            _top = panel.y + 100f;
            float bottom = upgrade ? panel.yMax - 86f : panel.yMax - 34f;
            _width = Horizontal ? (panel.width - 60f - Gap * Mathf.Max(0, choiceCount - 1)) / choiceCount : panel.width - 60f;
            _height = Horizontal ? bottom - _top : Mathf.Max(220f, Mathf.Min(330f, bottom - _top - 12f));
            View = Horizontal ? Rect.zero : new Rect(panel.x + 30f, _top, _width, Mathf.Max(1f, bottom - _top));
            Content = Horizontal ? Rect.zero : new Rect(0f, 0f, _width - 18f, choiceCount * _height + Mathf.Max(0, choiceCount - 1) * Gap);
        }
        public Rect Card(int index) => Horizontal
            ? new Rect(_panel.x + 30f + index * (_width + Gap), _top, _width, _height)
            : new Rect(0f, index * (_height + Gap), Content.width, _height);
        public Rect Selection(Rect card) => _upgrade ? new Rect(card.x, card.y, card.width, Mathf.Max(24f, card.height - 48f)) : card;
    }
}
