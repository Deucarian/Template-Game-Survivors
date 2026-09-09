using System;
using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Retains the latest streak observation independently of banner expiry.</summary>
    internal sealed class SurvivorsStreakFeedbackHistory
    {
        private const float StreakRewardFeedbackDurationSeconds = 1.8f;
        private readonly Action<string, float, Color> _show;
        public SurvivorsStreakFeedbackHistory(Action<string, float, Color> show) { _show = show; }
        public int StreakRewardFeedbackCount { get; private set; }
        public string LastStreakRewardFeedbackLabel { get; private set; } = string.Empty;
        public void Reset() { StreakRewardFeedbackCount = 0; LastStreakRewardFeedbackLabel = string.Empty; }

        public void RecordStreakRewardFeedback(string label, Color color)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return;
            }

            StreakRewardFeedbackCount++;
            LastStreakRewardFeedbackLabel = label;
            _show(label, StreakRewardFeedbackDurationSeconds, color);
        }
    }
}
