using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsFeedbackEnemy
    {
        Vector3 Position { get; }
        string DisplayName { get; }
        SurvivorsEnemyRole Role { get; }
        void TriggerHitFlash(bool critical, float durationSeconds);
    }

    internal interface ISurvivorsDamageFeedbackPort
    {
        void RecordPopup(Vector3 position, float amount, bool playerDamage, bool critical);
        void PlayCombatHitAudio();
        void TryEnrage(ISurvivorsFeedbackEnemy enemy);
    }

    internal interface ISurvivorsRangedDodgePort
    {
        SurvivorsTemplateTuning Tuning { get; }
        Vector3 PlayerPosition { get; }
        bool TrySpawnExperience(Vector3 position, int amount);
        void ShowStreakFeedback(string label, Color color);
        void PlayDodgePulse(Vector3 position, int count);
    }
}
