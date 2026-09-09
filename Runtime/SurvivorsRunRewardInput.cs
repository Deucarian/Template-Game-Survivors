namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsRunRewardInput
    {
        public SurvivorsRunRewardInput(float duration, int level, int minibossKills, int bossKills, float multiplier)
        {
            Duration = duration;
            Level = level;
            MinibossKills = minibossKills;
            BossKills = bossKills;
            Multiplier = multiplier;
        }

        public float Duration { get; }
        public int Level { get; }
        public int MinibossKills { get; }
        public int BossKills { get; }
        public float Multiplier { get; }
    }
}
