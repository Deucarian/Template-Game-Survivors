using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns the shared latest exploration message and its presentation dispatch.</summary>
    internal sealed class SurvivorsExplorationFeedback
    {
        private readonly Action<string, Color> _record;
        private readonly Action<Vector3, int, bool> _pulse;
        public SurvivorsExplorationFeedback(Action<string, Color> record, Action<Vector3, int, bool> pulse)
        {
            _record = record ?? throw new ArgumentNullException(nameof(record));
            _pulse = pulse ?? throw new ArgumentNullException(nameof(pulse));
        }
        public string LastLabel { get; private set; } = string.Empty;
        public void Reset() => LastLabel = string.Empty;
        public void Record(string label, Color color)
        {
            LastLabel = label;
            _record(label, color);
        }
        public void Pulse(Vector3 position, int particles, bool danger = false) => _pulse(position, particles, danger);
    }
}
