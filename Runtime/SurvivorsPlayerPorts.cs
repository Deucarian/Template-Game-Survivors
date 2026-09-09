using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Combat;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsPlayerDamagePort
    {
        SurvivorsTemplateTuning Tuning { get; }
        SurvivorsRunState State { get; }
        Vector3 PlayerPosition { get; }
        CombatCatalog CombatCatalog { get; }
        float BarrierCapacity { get; }
        float BarrierRegenPerSecondBonus { get; }
        int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source);
        void ShowBlockedDamage(bool invulnerable);
        void RecordDamage(DamageResult damage, Vector3 position);
        void Defeat();
        void ShowClutch(string label, int hitCount);
        void ShowHurt();
    }

    internal interface ISurvivorsPlayerMotionPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        SurvivorsRunState State { get; }
        bool HasPlayer { get; }
        Vector3 Position { get; set; }
        Vector3 Forward { get; set; }
        float MoveSpeed { get; }
        void RecordTravel(Vector3 delta);
        void ExtendSafety(float seconds);
        int ApplyDashPressure(Vector3 start, Vector3 end, Vector3 direction, Action onDamageHit);
        void ShowDash(Vector3 position, string label, int shoved);
    }
}
