using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsPickupCollectionPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        float PickupMagnetPulseIntervalReductionBonus { get; }
        Vector3 PlayerPosition { get; }
        void RecordFirstExperiencePickupTime();
        int GainExperience(int amount);
        void RecordExperienceCombo(int gained);
        void RestoreHealthFromPickup(int amount);
        void AddBloodShards(int amount);
        void PlayCollectionFeedback(Vector3 position, SurvivorsPickupKind kind, int burstCount);
        void PlayAttractionFeedback(Vector3 position);
        void PlayMagnetRecallFeedback(Vector3 position, int burstCount);
        void RecordStreakRewardFeedback(string label, Color color);
        void DespawnCompletedPickup(SpawnInstanceId instanceId);
    }
}
