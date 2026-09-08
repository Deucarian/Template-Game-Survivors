using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsOrbitKnockbackPort
    {
        bool CrimsonAegisActive { get; }
        float OrbitKnockbackDistance { get; }
        float CrimsonAegisOrbitKnockbackDistance { get; }
        Vector3 PlayerPosition { get; }
        Vector3 PlayerForward { get; }
    }

    /// <summary>Applies orbit knockback policy to borrowed enemies and records successful movement.</summary>
    internal sealed class SurvivorsOrbitKnockback
    {
        private readonly ISurvivorsOrbitKnockbackPort _port;
        private readonly SurvivorsWeaponDiagnostics _diagnostics;
        internal SurvivorsOrbitKnockback(ISurvivorsOrbitKnockbackPort port, SurvivorsWeaponDiagnostics diagnostics)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        internal bool Apply(SurvivorsEnemyActor enemy, SurvivorsWeaponArchetypeDefinition definition)
        {
            if (enemy == null || definition == null || !enemy.IsAlive || SurvivorsEnemyRosterQueries.IsMajorRewardRole(enemy.Role))
                return false;

            bool crimsonAegis = IsCrimsonAegisOrbitDefinition(definition) && _port.CrimsonAegisActive;
            float distance = crimsonAegis
                ? Mathf.Max(_port.OrbitKnockbackDistance, _port.CrimsonAegisOrbitKnockbackDistance)
                : _port.OrbitKnockbackDistance;
            distance = Mathf.Max(0f, distance);
            if (distance <= 0f) return false;

            Vector3 enemyPosition = enemy.transform.position;
            Vector3 away = enemyPosition - _port.PlayerPosition;
            away.y = 0f;
            if (away.sqrMagnitude <= 0.0001f) away = _port.PlayerForward;
            if (away.sqrMagnitude <= 0.0001f) return false;

            Vector3 direction = away.normalized;
            enemy.transform.position += direction * distance;
            enemy.transform.forward = direction;
            _diagnostics.RecordOrbitKnockback(crimsonAegis, definition.DisplayName, enemy.DisplayName);
            return true;
        }

        private static bool IsCrimsonAegisOrbitDefinition(SurvivorsWeaponArchetypeDefinition definition)
        {
            return definition != null &&
                (string.Equals(definition.Id, BasicSurvivorsGame.OrbitWardWeaponContentId, StringComparison.Ordinal) ||
                 string.Equals(definition.Id, BasicSurvivorsGame.ThornHaloWeaponContentId, StringComparison.Ordinal));
        }
    }
}
