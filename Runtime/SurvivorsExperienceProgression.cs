using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns the template's XP budget and bounded draft queue, without a scene or UI.</summary>
    internal sealed class SurvivorsExperienceProgression
    {
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int PendingLevelUps { get; private set; }
        public int ExperienceCollected { get; private set; }
        public int ThrottledExperienceOverflow { get; private set; }
        public float DraftCooldownRemaining { get; private set; }

        public void Reset()
        {
            Level = 1;
            Experience = 0;
            PendingLevelUps = 0;
            ExperienceCollected = 0;
            ThrottledExperienceOverflow = 0;
            DraftCooldownRemaining = 0f;
        }

        public int RequiredExperience(SurvivorsTemplateTuning tuning)
        {
            return Mathf.Max(1, tuning.ExperienceRequiredBase + (Level - 1) * tuning.ExperienceRequiredPerLevel);
        }

        public int Gain(int amount, float multiplierBonus, SurvivorsTemplateTuning tuning)
        {
            int gained = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, amount) * Mathf.Max(0.1f, 1f + multiplierBonus)));
            ExperienceCollected += gained;
            Experience += gained;
            ResolveBudget(tuning);
            return gained;
        }

        public void QueueDebugLevelUp() => PendingLevelUps++;

        public void ConsumeLevelUp() => PendingLevelUps = Mathf.Max(0, PendingLevelUps - 1);

        public void BeginDraft(float cooldownSeconds)
        {
            DraftCooldownRemaining = Mathf.Max(0f, cooldownSeconds);
        }

        public void Tick(float deltaTime, SurvivorsTemplateTuning tuning)
        {
            if (DraftCooldownRemaining > 0f)
            {
                DraftCooldownRemaining = Mathf.Max(0f, DraftCooldownRemaining - Mathf.Max(0f, deltaTime));
            }

            ResolveBudget(tuning);
        }

        public void ResolveBudget(SurvivorsTemplateTuning tuning)
        {
            int queueLimit = Mathf.Max(1, tuning.MaximumQueuedLevelUps);
            if (tuning.LevelUpDraftCooldownSeconds > 0f && DraftCooldownRemaining > 0f && PendingLevelUps <= 0)
            {
                CapStoredExperience(tuning);
                return;
            }

            int guard = 0;
            while (Experience >= RequiredExperience(tuning) && PendingLevelUps < queueLimit && guard++ < 256)
            {
                Experience -= RequiredExperience(tuning);
                Level++;
                PendingLevelUps++;
            }

            if (tuning.LevelUpDraftCooldownSeconds > 0f && PendingLevelUps >= queueLimit)
            {
                CapStoredExperience(tuning);
            }
        }

        private void CapStoredExperience(SurvivorsTemplateTuning tuning)
        {
            int cap = Mathf.Max(0, RequiredExperience(tuning) - 1);
            if (Experience > cap)
            {
                ThrottledExperienceOverflow += Experience - cap;
                Experience = cap;
            }
        }
    }
}
