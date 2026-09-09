using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsExplorationPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        int Escalation { get; }
        Vector3 PlayerPosition { get; }
        Vector3 PlayerForward { get; }
        int MaximumAlive { get; }
        int ActiveEnemyCount { get; }
        long SpawnSequence { get; }
        SurvivorsExplorationBonuses Endless { get; }
        string CurrencyRewardLabel { get; }
        SurvivorsExplorationFeedback Feedback { get; }
        long SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum, string source);
        bool SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount);
        int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source);
    }
}
