using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsEnemyRole
    {
        Swarm = 0,
        Runner = 1,
        Bruiser = 2,
        Spitter = 3,
        Elite = 4,
        Miniboss = 5,
        Boss = 6
    }

    public enum SurvivorsRunPhase
    {
        Opening = 0,
        Escalating = 1,
        Miniboss = 2,
        Boss = 3,
        Victory = 4
    }

    public struct SurvivorsEnemyProfile
    {
        public SurvivorsEnemyProfile(
            SurvivorsEnemyRole role,
            string id,
            string displayName,
            float maxHealth,
            float moveSpeed,
            float radius,
            float contactDamage,
            float contactIntervalSeconds,
            int experienceReward,
            Color tint,
            float rangedAttackRange = 0f,
            float rangedAttackDamage = 0f,
            float rangedAttackIntervalSeconds = 0f,
            float preferredRange = 0f)
        {
            Role = role;
            Id = id;
            DisplayName = displayName;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            Radius = radius;
            ContactDamage = contactDamage;
            ContactIntervalSeconds = contactIntervalSeconds;
            ExperienceReward = experienceReward;
            Tint = tint;
            RangedAttackRange = rangedAttackRange;
            RangedAttackDamage = rangedAttackDamage;
            RangedAttackIntervalSeconds = rangedAttackIntervalSeconds;
            PreferredRange = preferredRange;
        }

        public SurvivorsEnemyRole Role { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float Radius { get; }
        public float ContactDamage { get; }
        public float ContactIntervalSeconds { get; }
        public int ExperienceReward { get; }
        public Color Tint { get; }
        public float RangedAttackRange { get; }
        public float RangedAttackDamage { get; }
        public float RangedAttackIntervalSeconds { get; }
        public float PreferredRange { get; }
    }

    public sealed class SurvivorsRunFlowDefinition
    {
        public SurvivorsRunFlowDefinition(
            float escalationIntervalSeconds,
            float minimumEnemySpawnIntervalSeconds,
            float enemySpawnIntervalReductionPerEscalation,
            int enemyMaximumAliveIncreasePerEscalation,
            float enemyHealthMultiplierPerEscalation,
            float enemyMoveSpeedMultiplierPerEscalation,
            float enemyExperienceMultiplierPerEscalation,
            float minibossSpawnTimeSeconds,
            SurvivorsEnemyProfile miniboss,
            float bossSpawnTimeSeconds,
            SurvivorsEnemyProfile boss,
            float survivalVictoryTimeSeconds,
            IReadOnlyList<SurvivorsEnemyProfile> swarmProfiles = null)
        {
            EscalationIntervalSeconds = escalationIntervalSeconds;
            MinimumEnemySpawnIntervalSeconds = minimumEnemySpawnIntervalSeconds;
            EnemySpawnIntervalReductionPerEscalation = enemySpawnIntervalReductionPerEscalation;
            EnemyMaximumAliveIncreasePerEscalation = enemyMaximumAliveIncreasePerEscalation;
            EnemyHealthMultiplierPerEscalation = enemyHealthMultiplierPerEscalation;
            EnemyMoveSpeedMultiplierPerEscalation = enemyMoveSpeedMultiplierPerEscalation;
            EnemyExperienceMultiplierPerEscalation = enemyExperienceMultiplierPerEscalation;
            MinibossSpawnTimeSeconds = minibossSpawnTimeSeconds;
            Miniboss = miniboss;
            BossSpawnTimeSeconds = bossSpawnTimeSeconds;
            Boss = boss;
            SurvivalVictoryTimeSeconds = survivalVictoryTimeSeconds;
            SwarmProfiles = swarmProfiles == null ? Array.Empty<SurvivorsEnemyProfile>() : CopyProfiles(swarmProfiles);
        }

        public float EscalationIntervalSeconds { get; }
        public float MinimumEnemySpawnIntervalSeconds { get; }
        public float EnemySpawnIntervalReductionPerEscalation { get; }
        public int EnemyMaximumAliveIncreasePerEscalation { get; }
        public float EnemyHealthMultiplierPerEscalation { get; }
        public float EnemyMoveSpeedMultiplierPerEscalation { get; }
        public float EnemyExperienceMultiplierPerEscalation { get; }
        public float MinibossSpawnTimeSeconds { get; }
        public SurvivorsEnemyProfile Miniboss { get; }
        public float BossSpawnTimeSeconds { get; }
        public SurvivorsEnemyProfile Boss { get; }
        public float SurvivalVictoryTimeSeconds { get; }
        public IReadOnlyList<SurvivorsEnemyProfile> SwarmProfiles { get; }

        private static SurvivorsEnemyProfile[] CopyProfiles(IReadOnlyList<SurvivorsEnemyProfile> source)
        {
            SurvivorsEnemyProfile[] copy = new SurvivorsEnemyProfile[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return copy;
        }
    }

    public sealed class SurvivorsRunFlowRuntime
    {
        private bool _minibossSpawned;
        private bool _bossSpawned;
        private bool _victoryTriggered;

        public SurvivorsRunFlowRuntime(SurvivorsRunFlowDefinition definition)
        {
            Definition = definition;
        }

        public SurvivorsRunFlowDefinition Definition { get; }
        public int EscalationLevel { get; private set; }
        public SurvivorsRunPhase Phase { get; private set; } = SurvivorsRunPhase.Opening;

        public void Tick(float elapsedTimeSeconds)
        {
            float interval = Definition == null ? 0f : Definition.EscalationIntervalSeconds;
            EscalationLevel = interval <= 0f ? 0 : Mathf.Max(0, Mathf.FloorToInt(elapsedTimeSeconds / interval));

            if (_victoryTriggered)
            {
                Phase = SurvivorsRunPhase.Victory;
            }
            else if (Definition != null && elapsedTimeSeconds >= Definition.BossSpawnTimeSeconds)
            {
                Phase = SurvivorsRunPhase.Boss;
            }
            else if (Definition != null && elapsedTimeSeconds >= Definition.MinibossSpawnTimeSeconds)
            {
                Phase = SurvivorsRunPhase.Miniboss;
            }
            else if (EscalationLevel > 0)
            {
                Phase = SurvivorsRunPhase.Escalating;
            }
            else
            {
                Phase = SurvivorsRunPhase.Opening;
            }
        }

        public bool TryConsumeMinibossSpawn(float elapsedTimeSeconds)
        {
            if (_minibossSpawned || Definition == null || elapsedTimeSeconds < Definition.MinibossSpawnTimeSeconds)
            {
                return false;
            }

            _minibossSpawned = true;
            Phase = SurvivorsRunPhase.Miniboss;
            return true;
        }

        public bool TryConsumeBossSpawn(float elapsedTimeSeconds)
        {
            if (_bossSpawned || Definition == null || elapsedTimeSeconds < Definition.BossSpawnTimeSeconds)
            {
                return false;
            }

            _bossSpawned = true;
            Phase = SurvivorsRunPhase.Boss;
            return true;
        }

        public bool TryConsumeSurvivalVictory(float elapsedTimeSeconds)
        {
            if (_victoryTriggered || Definition == null || elapsedTimeSeconds < Definition.SurvivalVictoryTimeSeconds)
            {
                return false;
            }

            _victoryTriggered = true;
            Phase = SurvivorsRunPhase.Victory;
            return true;
        }

        public bool TryConsumeBossVictory()
        {
            if (_victoryTriggered)
            {
                return false;
            }

            _victoryTriggered = true;
            Phase = SurvivorsRunPhase.Victory;
            return true;
        }

        public float ResolveSpawnInterval(float baseIntervalSeconds)
        {
            if (Definition == null)
            {
                return Mathf.Max(0.05f, baseIntervalSeconds);
            }

            float interval = baseIntervalSeconds - (Definition.EnemySpawnIntervalReductionPerEscalation * EscalationLevel);
            return Mathf.Max(Definition.MinimumEnemySpawnIntervalSeconds, interval);
        }

        public int ResolveMaximumAlive(int baseMaximumAlive)
        {
            if (Definition == null)
            {
                return Mathf.Max(1, baseMaximumAlive);
            }

            return Mathf.Max(1, baseMaximumAlive + (Definition.EnemyMaximumAliveIncreasePerEscalation * EscalationLevel));
        }

        public float ResolveEnemySpeedMultiplier()
        {
            return ResolveLinearMultiplier(Definition == null ? 0f : Definition.EnemyMoveSpeedMultiplierPerEscalation);
        }

        public SurvivorsEnemyRole ResolveNextSwarmRole(float elapsedTimeSeconds, long spawnSequence)
        {
            if (Definition == null || Definition.SwarmProfiles.Count == 0)
            {
                return SurvivorsEnemyRole.Swarm;
            }

            int totalWeight = 0;
            SurvivorsEnemyRole[] roles = new SurvivorsEnemyRole[5];
            int[] weights = new int[5];
            int candidateCount = 0;
            AddCandidate(SurvivorsEnemyRole.Swarm, 60, ref candidateCount, roles, weights, ref totalWeight);
            if (elapsedTimeSeconds >= 35f)
            {
                AddCandidate(SurvivorsEnemyRole.Runner, 18 + Math.Min(18, EscalationLevel * 2), ref candidateCount, roles, weights, ref totalWeight);
            }

            if (elapsedTimeSeconds >= 90f)
            {
                AddCandidate(SurvivorsEnemyRole.Bruiser, 14 + Math.Min(16, EscalationLevel * 2), ref candidateCount, roles, weights, ref totalWeight);
            }

            if (elapsedTimeSeconds >= 150f)
            {
                AddCandidate(SurvivorsEnemyRole.Spitter, 10 + Math.Min(18, EscalationLevel * 2), ref candidateCount, roles, weights, ref totalWeight);
            }

            if (elapsedTimeSeconds >= 300f)
            {
                AddCandidate(SurvivorsEnemyRole.Elite, 4 + Math.Min(12, EscalationLevel), ref candidateCount, roles, weights, ref totalWeight);
            }

            if (totalWeight <= 0 || candidateCount <= 0)
            {
                return SurvivorsEnemyRole.Swarm;
            }

            uint hash = unchecked((uint)((spawnSequence * 1103515245L) + (EscalationLevel * 97L)));
            int roll = (int)(hash % (uint)totalWeight);
            for (int index = 0; index < candidateCount; index++)
            {
                roll -= weights[index];
                if (roll < 0)
                {
                    return roles[index];
                }
            }

            return SurvivorsEnemyRole.Swarm;
        }

        public SurvivorsEnemyProfile ResolveSwarmProfile(SurvivorsTemplateTuning tuning, float elapsedTimeSeconds = 0f, long spawnSequence = 0L)
        {
            SurvivorsEnemyRole role = ResolveNextSwarmRole(elapsedTimeSeconds, spawnSequence);
            return ResolveSwarmProfile(tuning, role);
        }

        public SurvivorsEnemyProfile ResolveSwarmProfile(SurvivorsTemplateTuning tuning, SurvivorsEnemyRole role)
        {
            SurvivorsTemplateTuning resolved = tuning ?? BasicSurvivorsGame.CreateDefaultTuning();
            float healthMultiplier = ResolveLinearMultiplier(Definition == null ? 0f : Definition.EnemyHealthMultiplierPerEscalation);
            float speedMultiplier = ResolveLinearMultiplier(Definition == null ? 0f : Definition.EnemyMoveSpeedMultiplierPerEscalation);
            float experienceMultiplier = ResolveLinearMultiplier(Definition == null ? 0f : Definition.EnemyExperienceMultiplierPerEscalation);
            SurvivorsEnemyProfile baseProfile = FindSwarmProfile(role);
            return new SurvivorsEnemyProfile(
                baseProfile.Role,
                baseProfile.Id,
                baseProfile.DisplayName,
                baseProfile.MaxHealth * healthMultiplier,
                baseProfile.MoveSpeed * speedMultiplier,
                baseProfile.Radius,
                baseProfile.ContactDamage,
                baseProfile.ContactIntervalSeconds,
                Mathf.Max(1, Mathf.RoundToInt(baseProfile.ExperienceReward * experienceMultiplier)),
                baseProfile.Tint,
                baseProfile.RangedAttackRange,
                baseProfile.RangedAttackDamage * healthMultiplier,
                baseProfile.RangedAttackIntervalSeconds,
                baseProfile.PreferredRange);
        }

        private SurvivorsEnemyProfile FindSwarmProfile(SurvivorsEnemyRole role)
        {
            if (Definition != null)
            {
                for (int index = 0; index < Definition.SwarmProfiles.Count; index++)
                {
                    SurvivorsEnemyProfile candidate = Definition.SwarmProfiles[index];
                    if (candidate.Role == role)
                    {
                        return candidate;
                    }
                }
            }

            return BasicSurvivorsGame.CreateEnemyProfile(role);
        }

        private float ResolveLinearMultiplier(float step)
        {
            return Mathf.Max(0.1f, 1f + (step * EscalationLevel));
        }

        private static void AddCandidate(
            SurvivorsEnemyRole role,
            int weight,
            ref int candidateCount,
            SurvivorsEnemyRole[] roles,
            int[] weights,
            ref int totalWeight)
        {
            if (candidateCount >= roles.Length || weight <= 0)
            {
                return;
            }

            roles[candidateCount] = role;
            weights[candidateCount] = weight;
            totalWeight += weight;
            candidateCount++;
        }
    }
}
