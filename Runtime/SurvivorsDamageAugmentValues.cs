namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>A fresh value read; UpgradeModifiers remains the authoritative build state.</summary>
    internal readonly struct SurvivorsDamageAugmentValues
    {
        public SurvivorsDamageAugmentValues(float lifesteal, float barrier, float poison, float bleed, float execute)
        {
            Lifesteal = lifesteal;
            Barrier = barrier;
            Poison = poison;
            Bleed = bleed;
            Execute = execute;
        }
        public float Lifesteal { get; }
        public float Barrier { get; }
        public float Poison { get; }
        public float Bleed { get; }
        public float Execute { get; }
    }
}
