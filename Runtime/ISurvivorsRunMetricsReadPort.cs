namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsRunMetricsReadPort
    {
        string ModeName { get; }
        SurvivorsPacingProfile PacingProfile { get; }
        float TargetDuration { get; }
        float BossSpawnTime { get; }
        float VictoryTime { get; }
        bool Started { get; }
        bool ModeSelectionOpen { get; }
        ISurvivorsActiveRunMetricsReadPort Active { get; }
    }

    internal interface ISurvivorsActiveRunMetricsReadPort
    {
        float RunTimeSeconds { get; }
        SurvivorsRunState State { get; }
        SurvivorsRunTelemetry Telemetry { get; }
        SurvivorsRunMetricsDraftValues CaptureDrafts();
        SurvivorsRunMetricsCombatValues CaptureCombat();
        SurvivorsRunMetricsPickupValues CapturePickups();
    }
}
