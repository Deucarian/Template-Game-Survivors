using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsEnemySupportSpawnPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        SurvivorsRunState State { get; }
        int MaximumAlive { get; }
        int EnemyCount { get; }
        long SpawnSequence { get; }
        SurvivorsEnemyActor SpawnGameplayEnemyOffscreen(SurvivorsEnemyRole role, long seed, float minimumDistance, float maximumDistance, string spawnSource);
        void RecordStreakRewardFeedback(string label, Color color);
        void PlaySupportSpawnFeedback(Vector3 position, int burstCount);
    }
}
