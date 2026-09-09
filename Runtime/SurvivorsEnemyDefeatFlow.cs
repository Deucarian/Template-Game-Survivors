using System;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns kill diagnostics and ordered removal, payoff, draft and encounter-clear continuation.</summary>
    internal sealed class SurvivorsEnemyDefeatFlow
    {
        private readonly ISurvivorsEnemyDefeatPort _port;
        private readonly SurvivorsRunSession _runSession;
        public SurvivorsEnemyDefeatFlow(ISurvivorsEnemyDefeatPort port, SurvivorsRunSession runSession)
        { _port = port ?? throw new ArgumentNullException(nameof(port)); _runSession = runSession ?? throw new ArgumentNullException(nameof(runSession)); }
        public int KilledCount { get; private set; }
        public int EliteKilledCount { get; private set; }
        public int MinibossKilledCount { get; private set; }
        public int BossKilledCount { get; private set; }
        public void Reset() { KilledCount = 0; EliteKilledCount = 0; MinibossKilledCount = 0; BossKilledCount = 0; }
        private static bool IsEliteRole(SurvivorsEnemyRole role) => role == SurvivorsEnemyRole.Elite || role == SurvivorsEnemyRole.DreadElite;
        private static bool IsMajorRewardRole(SurvivorsEnemyRole role) => IsEliteRole(role) || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;
        public void HandleEnemyKilled(ISurvivorsDefeatTarget enemy, string source, bool applyAugments)
        {
            if (enemy == null)
            {
                return;
            }

            KilledCount++;
            Vector3 position = enemy.Position;
            SurvivorsEnemyRole role = enemy.Role;
            _port.RecordKillMetric(role);

            int xp = Mathf.Max(1, enemy.ExperienceReward);
            float radius = enemy.Radius;
            SurvivorsEncounterClears clears = _port.ReleaseKilledEnemy(enemy);

            _port.SpawnExperience(position, xp);
            _port.RegisterStreak(position);
            _port.ShowDeath(position, role, radius);
            _port.TriggerDeathNova(position, source, applyAugments);
            if (IsMajorRewardRole(role))
            {
                _port.SpawnMajorRewards(position, role, radius, xp);
            }

            _port.PlayDeath(position, role == SurvivorsEnemyRole.Swarm ? 18 : 34);
            if (role == SurvivorsEnemyRole.Splitter)
            {
                _port.SpawnSplitterChildren(position, enemy.DisplayName);
            }
            else if (IsEliteRole(role))
            {
                EliteKilledCount++;
                _port.GrantMajorEnemyReward(role);
                _port.OpenUpgradeRewardDraft(role);
            }
            else if (role == SurvivorsEnemyRole.Miniboss)
            {
                MinibossKilledCount++;
                _port.GrantMajorEnemyReward(SurvivorsEnemyRole.Miniboss);
                if (!_port.OpenUpgradeRewardDraft(SurvivorsEnemyRole.Miniboss))
                {
                    _port.OpenBossRelicDraft();
                }
            }
            else if (role == SurvivorsEnemyRole.Boss)
            {
                BossKilledCount++;
                _port.GrantMajorEnemyReward(SurvivorsEnemyRole.Boss);
                if (!_port.OpenUpgradeRewardDraft(SurvivorsEnemyRole.Boss) && !_runSession.HasClearedVictory)
                {
                    _port.EnterVictory();
                }
            }

            if (clears.Horde)
            {
                _port.RewardHordeClear(position);
            }

            if (clears.Cache)
            {
                _port.RewardCacheClear(position);
            }

            if (clears.Shrine)
            {
                _port.RewardShrineClear(position);
            }
        }
    }
}
