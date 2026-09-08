namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsNormalMilestoneTimes
    {
        public SurvivorsNormalMilestoneTimes(
            bool hasDefinition,
            float eliteTime,
            float dreadEliteTime,
            float minibossTime,
            float bossTime,
            float victoryTime)
        {
            HasDefinition = hasDefinition;
            EliteTime = eliteTime;
            DreadEliteTime = dreadEliteTime;
            MinibossTime = minibossTime;
            BossTime = bossTime;
            VictoryTime = victoryTime;
        }
        public bool HasDefinition { get; }
        public float EliteTime { get; }
        public float DreadEliteTime { get; }
        public float MinibossTime { get; }
        public float BossTime { get; }
        public float VictoryTime { get; }
    }

    internal readonly struct SurvivorsEndlessMilestoneTimes
    {
        public SurvivorsEndlessMilestoneTimes(
            SurvivorsEnemyRole eliteRole,
            float eliteTime,
            float minibossTime,
            float bossTime)
        {
            EliteRole = eliteRole;
            EliteTime = eliteTime;
            MinibossTime = minibossTime;
            BossTime = bossTime;
        }
        public SurvivorsEnemyRole EliteRole { get; }
        public float EliteTime { get; }
        public float MinibossTime { get; }
        public float BossTime { get; }
    }

    internal readonly struct SurvivorsRunMilestoneValues
    {
        public SurvivorsRunMilestoneValues(
            bool started,
            SurvivorsRunState state,
            float runTimeSeconds,
            bool isEndlessRun,
            float hordeTime,
            SurvivorsNormalMilestoneTimes normal,
            SurvivorsEndlessMilestoneTimes endless)
        {
            Started = started;
            State = state;
            RunTimeSeconds = runTimeSeconds;
            IsEndlessRun = isEndlessRun;
            HordeTime = hordeTime;
            Normal = normal;
            Endless = endless;
        }
        public bool Started { get; }
        public SurvivorsRunState State { get; }
        public float RunTimeSeconds { get; }
        public bool IsEndlessRun { get; }
        public float HordeTime { get; }
        public SurvivorsNormalMilestoneTimes Normal { get; }
        public SurvivorsEndlessMilestoneTimes Endless { get; }
    }
}
