namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Scene effects triggered in authored order after the corresponding modifier changes.</summary>
    internal interface ISurvivorsUpgradeEffectSink
    {
        void IncreaseMaximumHealth(double amount);
        void RestoreBarrier(float amount);
        void ScheduleMagnetPulse();
    }
}
