using System;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsDefeatTarget
    {
        Vector3 Position { get; }
        SurvivorsEnemyRole Role { get; }
        int ExperienceReward { get; }
        float Radius { get; }
        string DisplayName { get; }
        SpawnInstanceId InstanceId { get; }
    }
    internal readonly struct SurvivorsEncounterClears
    {
        public readonly bool Horde, Cache, Shrine;
        public SurvivorsEncounterClears(bool horde, bool cache, bool shrine) { Horde = horde; Cache = cache; Shrine = shrine; }
    }
    internal interface ISurvivorsEnemyDefeatPort
    {
        void RecordKillMetric(SurvivorsEnemyRole role);
        SurvivorsEncounterClears ReleaseKilledEnemy(ISurvivorsDefeatTarget enemy);
        void SpawnExperience(Vector3 position, int amount);
        void RegisterStreak(Vector3 position);
        void ShowDeath(Vector3 position, SurvivorsEnemyRole role, float radius);
        void TriggerDeathNova(Vector3 position, string source, bool applyAugments);
        void SpawnMajorRewards(Vector3 position, SurvivorsEnemyRole role, float radius, int experience);
        void PlayDeath(Vector3 position, int burst);
        void SpawnSplitterChildren(Vector3 position, string displayName);
        void GrantMajorEnemyReward(SurvivorsEnemyRole role);
        bool OpenUpgradeRewardDraft(SurvivorsEnemyRole role);
        void OpenBossRelicDraft();
        void EnterVictory();
        void RewardHordeClear(Vector3 position);
        void RewardCacheClear(Vector3 position);
        void RewardShrineClear(Vector3 position);
    }
}
