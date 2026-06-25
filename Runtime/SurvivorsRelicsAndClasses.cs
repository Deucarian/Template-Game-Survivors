using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    internal enum SurvivorsRewardSelectionKind
    {
        None = 0,
        LevelUp = 1,
        BossRelic = 2
    }

    public enum SurvivorsRelicEffectKind
    {
        DamageBonus = 0,
        CooldownMultiplier = 1,
        PickupRange = 2
    }

    public sealed class SurvivorsRelicDefinition
    {
        public SurvivorsRelicDefinition(
            string id,
            string displayName,
            string targetId,
            string effectId,
            SurvivorsRelicEffectKind effectKind,
            float amount,
            int weight)
        {
            Id = id ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
            TargetId = targetId ?? string.Empty;
            EffectId = effectId ?? string.Empty;
            EffectKind = effectKind;
            Amount = amount;
            Weight = Math.Max(1, weight);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string TargetId { get; }
        public string EffectId { get; }
        public SurvivorsRelicEffectKind EffectKind { get; }
        public float Amount { get; }
        public int Weight { get; }
    }

    public sealed class SurvivorsRelicDraft
    {
        public SurvivorsRelicDraft(IReadOnlyList<SurvivorsRelicDefinition> choices)
        {
            Choices = choices == null ? Array.Empty<SurvivorsRelicDefinition>() : Copy(choices);
        }

        public IReadOnlyList<SurvivorsRelicDefinition> Choices { get; }

        private static SurvivorsRelicDefinition[] Copy(IReadOnlyList<SurvivorsRelicDefinition> source)
        {
            var copy = new SurvivorsRelicDefinition[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    public static class SurvivorsRelicDraftService
    {
        public static SurvivorsRelicDraft Generate(IReadOnlyList<SurvivorsRelicDefinition> relics, int choiceCount, int seed)
        {
            if (relics == null || relics.Count == 0 || choiceCount <= 0)
            {
                return new SurvivorsRelicDraft(Array.Empty<SurvivorsRelicDefinition>());
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var candidates = new List<WeightedRelicCandidate>(relics.Count);
            for (int i = 0; i < relics.Count; i++)
            {
                SurvivorsRelicDefinition relic = relics[i];
                if (relic == null || string.IsNullOrWhiteSpace(relic.Id) || !seen.Add(relic.Id))
                {
                    continue;
                }

                double score = DeterministicScore(seed, relic.Id) / Math.Max(1, relic.Weight);
                candidates.Add(new WeightedRelicCandidate(relic, score));
            }

            candidates.Sort((left, right) =>
            {
                int scoreComparison = left.Score.CompareTo(right.Score);
                return scoreComparison != 0
                    ? scoreComparison
                    : string.Compare(left.Definition.Id, right.Definition.Id, StringComparison.Ordinal);
            });

            int resolvedCount = Math.Min(choiceCount, candidates.Count);
            var choices = new SurvivorsRelicDefinition[resolvedCount];
            for (int i = 0; i < resolvedCount; i++)
            {
                choices[i] = candidates[i].Definition;
            }

            return new SurvivorsRelicDraft(choices);
        }

        private static double DeterministicScore(int seed, string id)
        {
            unchecked
            {
                uint hash = 2166136261u;
                hash = (hash ^ (uint)seed) * 16777619u;
                for (int i = 0; i < id.Length; i++)
                {
                    hash = (hash ^ id[i]) * 16777619u;
                }

                return hash / (double)uint.MaxValue;
            }
        }

        private readonly struct WeightedRelicCandidate
        {
            public WeightedRelicCandidate(SurvivorsRelicDefinition definition, double score)
            {
                Definition = definition;
                Score = score;
            }

            public SurvivorsRelicDefinition Definition { get; }
            public double Score { get; }
        }
    }

    public enum SurvivorsClassStatKind
    {
        MoveSpeed = 0,
        Damage = 1,
        MaxHealth = 2
    }

    public sealed class SurvivorsClassStatModifierDefinition
    {
        public SurvivorsClassStatModifierDefinition(SurvivorsClassStatKind statKind, float amount)
        {
            StatKind = statKind;
            Amount = amount;
        }

        public SurvivorsClassStatKind StatKind { get; }
        public float Amount { get; }
    }

    public sealed class SurvivorsClassDefinition
    {
        public SurvivorsClassDefinition(
            string id,
            string displayName,
            string startingWeaponId,
            bool isUnlockedByDefault,
            string unlockRewardId,
            IReadOnlyList<SurvivorsClassStatModifierDefinition> startingStatModifiers)
        {
            Id = id ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
            StartingWeaponId = startingWeaponId ?? string.Empty;
            IsUnlockedByDefault = isUnlockedByDefault;
            UnlockRewardId = unlockRewardId ?? string.Empty;
            StartingStatModifiers = startingStatModifiers == null ? Array.Empty<SurvivorsClassStatModifierDefinition>() : Copy(startingStatModifiers);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string StartingWeaponId { get; }
        public bool IsUnlockedByDefault { get; }
        public string UnlockRewardId { get; }
        public IReadOnlyList<SurvivorsClassStatModifierDefinition> StartingStatModifiers { get; }

        private static SurvivorsClassStatModifierDefinition[] Copy(IReadOnlyList<SurvivorsClassStatModifierDefinition> source)
        {
            var copy = new SurvivorsClassStatModifierDefinition[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    public sealed class SurvivorsClassLibraryDefinition
    {
        public SurvivorsClassLibraryDefinition(IReadOnlyList<SurvivorsClassDefinition> classes)
        {
            Classes = classes == null ? Array.Empty<SurvivorsClassDefinition>() : Copy(classes);
        }

        public IReadOnlyList<SurvivorsClassDefinition> Classes { get; }

        public bool TryGetClass(string id, out SurvivorsClassDefinition definition)
        {
            for (int i = 0; i < Classes.Count; i++)
            {
                SurvivorsClassDefinition candidate = Classes[i];
                if (candidate != null && string.Equals(candidate.Id, id, StringComparison.Ordinal))
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public SurvivorsClassDefinition FirstDefaultUnlocked()
        {
            for (int i = 0; i < Classes.Count; i++)
            {
                SurvivorsClassDefinition definition = Classes[i];
                if (definition != null && definition.IsUnlockedByDefault)
                {
                    return definition;
                }
            }

            return Classes.Count == 0 ? null : Classes[0];
        }

        private static SurvivorsClassDefinition[] Copy(IReadOnlyList<SurvivorsClassDefinition> source)
        {
            var copy = new SurvivorsClassDefinition[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }
}
