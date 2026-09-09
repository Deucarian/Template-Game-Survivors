using System;

namespace Deucarian.TemplateGameSurvivors
{
    internal enum SurvivorsRunMetric
    {
        FirstKill,
        FirstExperiencePickup,
        FirstLevelUpDraft,
        FirstEliteSpawn,
        FirstEliteKill,
        FirstMinibossSpawn,
        FirstMinibossKill,
        FirstBossSpawn,
        FirstBossKill,
        FirstEvolutionEligibility,
        FirstEvolutionAcquired
    }

    /// <summary>Owns first-event times and observed minute checkpoints; it does not own the run clock.</summary>
    internal sealed class SurvivorsRunTelemetry
    {
        // Preserve the component's pre-start zero values; Reset starts a new run with unseen markers.
        private readonly float[] _firstTimes = new float[11];
        private readonly int[] _minuteLevels = new int[5];

        public float FirstKillTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstKill];
        public float FirstExperiencePickupTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstExperiencePickup];
        public float FirstLevelUpDraftTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstLevelUpDraft];
        public float FirstEliteSpawnTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstEliteSpawn];
        public float FirstEliteKillTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstEliteKill];
        public float FirstMinibossSpawnTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstMinibossSpawn];
        public float FirstMinibossKillTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstMinibossKill];
        public float FirstBossSpawnTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstBossSpawn];
        public float FirstBossKillTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstBossKill];
        public float FirstEvolutionEligibilityTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstEvolutionEligibility];
        public float FirstEvolutionAcquiredTimeSeconds => _firstTimes[(int)SurvivorsRunMetric.FirstEvolutionAcquired];
        public int LevelAtOneMinute => _minuteLevels[0];
        public int LevelAtTwoMinutes => _minuteLevels[1];
        public int LevelAtThreeMinutes => _minuteLevels[2];
        public int LevelAtFourMinutes => _minuteLevels[3];
        public int LevelAtFiveMinutes => _minuteLevels[4];

        public void Reset()
        {
            for (int i = 0; i < _firstTimes.Length; i++) _firstTimes[i] = -1f;
            for (int i = 0; i < _minuteLevels.Length; i++) _minuteLevels[i] = -1;
        }

        public void Record(SurvivorsRunMetric metric, float runTimeSeconds)
        {
            int index = (int)metric;
            if (index < 0 || index >= _firstTimes.Length) throw new ArgumentOutOfRangeException(nameof(metric));
            if (_firstTimes[index] < 0f) _firstTimes[index] = runTimeSeconds;
        }

        public void RecordLevelCheckpoints(float runTimeSeconds, int level)
        {
            for (int i = 0; i < _minuteLevels.Length; i++)
            {
                if (_minuteLevels[i] < 0 && runTimeSeconds >= (i + 1) * 60f) _minuteLevels[i] = level;
            }
        }
    }
}
