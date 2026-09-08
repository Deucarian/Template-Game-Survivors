using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsMajorRewardPickupCachePort
    {
        SurvivorsTemplateTuning Tuning { get; }
        int RunEscalationLevel { get; }
        bool IsMajorRewardRole(SurvivorsEnemyRole role);
        SurvivorsPickupActor SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount);
        string ResolveMajorRewardDropLabel(SurvivorsEnemyRole role);
        Color ResolveMajorRewardDropColor(SurvivorsEnemyRole role);
        void RecordStreakRewardFeedback(string label, Color color);
    }
}
