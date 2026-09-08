using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns gem combo budget, feedback and its separately ticking Gem Rush gameplay reward.</summary>
    internal sealed class SurvivorsExperienceComboRewards
    {
        private readonly Func<SurvivorsTemplateTuning> _tuning;
        public readonly SurvivorsFeedbackBannerPresenter Banner = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Experience);
        public SurvivorsExperienceComboRewards(Func<SurvivorsTemplateTuning> tuning) => _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        private const float ExperienceComboWindowSeconds = 0.85f;
        private const float ExperienceComboFeedbackDurationSeconds = 1.35f;
        private const int ExperienceComboMinimumPickupCount = 3;
        private float _experienceComboTimer;
        private float _gemRushTimer;
        private int _experienceComboPickupCount;
        private int _experienceComboAmount;
        private bool _gemRushActivatedForCurrentCombo;
        public int ExperienceComboFeedbackCount { get; private set; }
        public int GemRushActivationCount { get; private set; }
        public string LastExperienceComboFeedbackLabel { get; private set; } = string.Empty;
        public string LastGemRushFeedbackLabel { get; private set; } = string.Empty;
        public bool IsGemRushActive => _gemRushTimer > 0f;
        public float GemRushRemainingSeconds => Mathf.Max(0f, _gemRushTimer);
        public float GemRushDamageBonus => IsGemRushActive ? Mathf.Max(0f, _tuning().GemRushDamageBonus) : 0f;
        public float GemRushMoveSpeedBonus => IsGemRushActive ? Mathf.Max(0f, _tuning().GemRushMoveSpeedBonus) : 0f;
        public float GemRushCooldownMultiplierBonus => IsGemRushActive ? Mathf.Min(0f, _tuning().GemRushCooldownMultiplierBonus) : 0f;
        public float GemRushPickupRangeBonus => IsGemRushActive ? Mathf.Max(0f, _tuning().GemRushPickupRangeBonus) : 0f;
        public string ActiveExperienceComboFeedbackLabel => Banner.RemainingSeconds > 0f ? Banner.Label : string.Empty;
        public float ExperienceComboFeedbackRemainingSeconds => Mathf.Max(0f, Banner.RemainingSeconds);
        public int CurrentExperienceComboPickupCount => _experienceComboTimer > 0f ? _experienceComboPickupCount : 0;
        public int CurrentExperienceComboAmount => _experienceComboTimer > 0f ? _experienceComboAmount : 0;
        public void Reset()
        {
            Banner.Reset();
            _experienceComboTimer = _gemRushTimer = 0f;
            _experienceComboPickupCount = _experienceComboAmount = ExperienceComboFeedbackCount = GemRushActivationCount = 0;
            _gemRushActivatedForCurrentCombo = false;
            LastExperienceComboFeedbackLabel = LastGemRushFeedbackLabel = string.Empty;
        }
        public void RecordExperienceCombo(int gained)
        {
            if (gained <= 0)
            {
                return;
            }

            if (_experienceComboTimer <= 0f)
            {
                _experienceComboPickupCount = 0;
                _experienceComboAmount = 0;
                _gemRushActivatedForCurrentCombo = false;
            }

            _experienceComboPickupCount++;
            _experienceComboAmount += gained;
            _experienceComboTimer = ExperienceComboWindowSeconds;

            if (_experienceComboPickupCount < ExperienceComboMinimumPickupCount)
            {
                return;
            }

            ExperienceComboFeedbackCount++;
            ActivateGemRush();
            Banner.Show($"{_experienceComboPickupCount} Gem Rush: +{_experienceComboAmount} XP, rush {GemRushRemainingSeconds:0.#}s", ExperienceComboFeedbackDurationSeconds, Color.white);
            LastExperienceComboFeedbackLabel = Banner.Label;
        }

        private void ActivateGemRush()
        {
            float duration = Mathf.Max(0f, _tuning().GemRushDurationSeconds);
            if (duration <= 0f)
            {
                return;
            }

            _gemRushTimer = Mathf.Max(_gemRushTimer, duration);
            if (!_gemRushActivatedForCurrentCombo)
            {
                GemRushActivationCount++;
                _gemRushActivatedForCurrentCombo = true;
            }

            LastGemRushFeedbackLabel = $"Gem Rush: damage +{GemRushDamageBonus:0.#}, cooldown {GemRushCooldownMultiplierBonus:P0}, pickup +{GemRushPickupRangeBonus:0.#}";
        }

        public void TickGemRush(float deltaTime)
        {
            if (_gemRushTimer <= 0f)
            {
                return;
            }

            _gemRushTimer = Mathf.Max(0f, _gemRushTimer - Mathf.Max(0f, deltaTime));
        }

        public void TickExperienceComboFeedback(float deltaTime)
        {
            float dt = Mathf.Max(0f, deltaTime);
            if (_experienceComboTimer > 0f)
            {
                _experienceComboTimer = Mathf.Max(0f, _experienceComboTimer - dt);
                if (_experienceComboTimer <= 0f)
                {
                    _experienceComboPickupCount = 0;
                    _experienceComboAmount = 0;
                }
            }
            Banner.Tick(dt);
        }
    }
}
