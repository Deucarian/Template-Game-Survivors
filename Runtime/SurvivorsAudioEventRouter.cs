using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Deterministic event throttling. The Unity presenter supplies the unscaled clock.</summary>
    internal sealed class SurvivorsAudioEventRouter
    {
        private readonly Dictionary<string, float> _nextAllowed = new Dictionary<string, float>(StringComparer.Ordinal);
        public bool Muted { get; private set; }
        public int DispatchCount { get; private set; }
        public string LastEventId { get; private set; } = string.Empty;

        public void SetMuted(bool muted) => Muted = muted;

        public void Reset()
        {
            _nextAllowed.Clear();
            DispatchCount = 0;
            LastEventId = string.Empty;
        }

        public bool TryDispatch(string eventId, float now, float throttleSeconds)
        {
            if (Muted || string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            string normalized = eventId.Trim();
            if (throttleSeconds > 0f && _nextAllowed.TryGetValue(normalized, out float next) && now < next)
            {
                return false;
            }

            if (throttleSeconds > 0f)
            {
                _nextAllowed[normalized] = now + throttleSeconds;
            }

            DispatchCount++;
            LastEventId = normalized;
            return true;
        }
    }
}
