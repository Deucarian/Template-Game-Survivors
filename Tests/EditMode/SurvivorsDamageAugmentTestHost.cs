using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    internal sealed class SurvivorsDamageAugmentTestHost : ISurvivorsDamageAugmentPort, ISurvivorsDamageAugmentTarget
    {
        public readonly List<string> Events = new List<string>();
        public readonly List<float> StatusAmounts = new List<float>();
        public readonly List<float> StatusDurations = new List<float>();
        public readonly List<string> StatusSources = new List<string>();
        public readonly HashSet<string> Evolutions = new HashSet<string>();
        public SurvivorsDamageAugmentValues Values { get; set; }
        public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning
        {
            StatusPoisonDurationSeconds = 4f,
            StatusBleedDurationSeconds = 2f,
            StatusBurnDurationSeconds = 3f
        };
        public bool IsPlayerBound { get; set; } = true;
        public bool IsAlive { get; set; } = true;
        public float HealthFraction { get; set; } = 0.2f;
        public string DisplayName => "Target";
        public float Healed;
        public float Barrier;
        public bool AcceptSlow = true;
        public float SlowMultiplier;
        public float SlowDuration;

        public void HealPlayer(float amount) { Healed += amount; Events.Add("heal"); }
        public void RestoreBarrier(float amount) { Barrier += amount; Events.Add("barrier"); }
        public bool IsEvolutionActive(string id) => Evolutions.Contains(id);
        public string ResolveUpgradeName(string id) => "Authored evolution " + id;
        public string ResolveWeaponName(string id) => "Authored weapon " + id;
        public void ApplyDamageOverTime(float amount, float duration, string statusId, string source)
        {
            Events.Add(statusId);
            StatusAmounts.Add(amount);
            StatusDurations.Add(duration);
            StatusSources.Add(source);
        }
        public bool ApplyMovementSlow(float multiplier, float duration)
        {
            SlowMultiplier = multiplier;
            SlowDuration = duration;
            Events.Add("slow");
            return AcceptSlow;
        }
        public void ExecuteFromAugment(string source) { Events.Add("execute:" + source); IsAlive = false; }
    }
}
