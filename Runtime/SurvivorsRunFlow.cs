using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsEnemyRole
    {
        Swarm = 0,
        Miniboss = 1,
        Boss = 2
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
            Color tint)
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
            float survivalVictoryTimeSeconds)
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

        public SurvivorsEnemyProfile ResolveSwarmProfile(SurvivorsTemplateTuning tuning)
        {
            SurvivorsTemplateTuning resolved = tuning ?? BasicSurvivorsGame.CreateDefaultTuning();
            float healthMultiplier = ResolveLinearMultiplier(Definition == null ? 0f : Definition.EnemyHealthMultiplierPerEscalation);
            float speedMultiplier = ResolveLinearMultiplier(Definition == null ? 0f : Definition.EnemyMoveSpeedMultiplierPerEscalation);
            float experienceMultiplier = ResolveLinearMultiplier(Definition == null ? 0f : Definition.EnemyExperienceMultiplierPerEscalation);
            return new SurvivorsEnemyProfile(
                SurvivorsEnemyRole.Swarm,
                BasicSurvivorsGame.SwarmEnemySpawnableId.Value,
                "Swarm Thrall",
                resolved.EnemyMaxHealth * healthMultiplier,
                resolved.EnemyMoveSpeed * speedMultiplier,
                resolved.EnemyRadius,
                resolved.EnemyContactDamage,
                resolved.EnemyContactIntervalSeconds,
                Mathf.Max(1, Mathf.RoundToInt(resolved.EnemyExperienceReward * experienceMultiplier)),
                new Color(0.88f, 0.22f, 0.32f));
        }

        private float ResolveLinearMultiplier(float step)
        {
            return Mathf.Max(0.1f, 1f + (step * EscalationLevel));
        }
    }
}
