using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsEnemyNavigationPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        Vector3 PlayerPosition { get; }
        bool IsMajorRewardRole(SurvivorsEnemyRole role);
        Vector3 ResolveSafeOffscreenPosition(Vector3 center, float minimumDistance, float maximumDistance, long seed, float padding, float bandDepth);
        float ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string reason);
        void RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string reason);
        void RecordMajorThreatReentry(SurvivorsEnemyRole role, string displayName, Vector3 playerToEnemy);
    }
}
