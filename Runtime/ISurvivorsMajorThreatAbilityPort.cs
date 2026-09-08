using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsMajorThreatAbilityPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        SurvivorsRunState State { get; }
        Vector3 PlayerPosition { get; }
        float CurrentHealth { get; }
        float BarrierValue { get; }
        bool IsMajorRewardRole(SurvivorsEnemyRole role);
        string ResolveMajorThreatHealthFallbackLabel(SurvivorsEnemyRole role);
        void ApplyDamageToPlayer(float amount, string source);
        void RecordStreakRewardFeedback(string label, Color color);
        void RecordMajorThreatSlamTelegraphEffect(Vector3 position, SurvivorsEnemyRole role, float radius, float durationSeconds);
        void PlayBossFeedback(Vector3 position, int burstCount);
    }
}
