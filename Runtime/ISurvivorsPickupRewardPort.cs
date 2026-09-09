using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsPickupRewardPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        Vector3 PlayerPosition { get; }
        string CurrencyLabel { get; }
        int DamageNonMajor(Vector3 position, float radius, float damage, string source);
        bool SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount, bool attract);
        void ShowFeedback(string label, Color color);
        void PlayPulse(Vector3 position, int count, bool boss, bool pickupAudio);
    }
}
