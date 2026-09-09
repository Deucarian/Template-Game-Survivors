using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns acquired relic identity and order; authored definitions remain with content binding.</summary>
    internal sealed class SurvivorsRelicInventory
    {
        private readonly HashSet<string> _ids = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<SurvivorsRelicDefinition> _selected = new List<SurvivorsRelicDefinition>(8);
        private readonly Func<IReadOnlyList<SurvivorsRelicDefinition>> _definitions;
        private readonly Action<SurvivorsRelicDefinition> _apply;
        private readonly Action<SurvivorsRelicDefinition> _acquired;
        public IReadOnlyList<SurvivorsRelicDefinition> Selected => _selected;
        public int Count { get; private set; }
        public SurvivorsRelicInventory(Func<IReadOnlyList<SurvivorsRelicDefinition>> definitions,
            Action<SurvivorsRelicDefinition> apply, Action<SurvivorsRelicDefinition> acquired)
        {
            _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            _apply = apply ?? throw new ArgumentNullException(nameof(apply));
            _acquired = acquired ?? throw new ArgumentNullException(nameof(acquired));
        }
        public void Reset()
        {
            _ids.Clear();
            _selected.Clear();
            Count = 0;
        }
        public IReadOnlyList<SurvivorsRelicDefinition> Available()
        {
            IReadOnlyList<SurvivorsRelicDefinition> definitions = _definitions();
            if (definitions == null || definitions.Count == 0) return Array.Empty<SurvivorsRelicDefinition>();
            if (_ids.Count == 0) return definitions;
            var available = new List<SurvivorsRelicDefinition>(definitions.Count);
            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorsRelicDefinition relic = definitions[i];
                if (relic != null && !string.IsNullOrWhiteSpace(relic.Id) && !_ids.Contains(relic.Id)) available.Add(relic);
            }
            return available.Count == 0 ? Array.Empty<SurvivorsRelicDefinition>() : available;
        }
        public bool Select(SurvivorsRelicDefinition relic)
        {
            if (relic == null || string.IsNullOrWhiteSpace(relic.Id) || !_ids.Add(relic.Id)) return false;
            _apply(relic);
            Count++;
            _selected.Add(relic);
            _acquired(relic);
            return true;
        }
    }
}
