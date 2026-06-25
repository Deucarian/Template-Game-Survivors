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
            IReadOnlyList<SurvivorsClassStatModifierDefinition> startingStatModifiers,
            IReadOnlyList<string> startingWeaponIds = null)
        {
            Id = id ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
            StartingWeaponId = startingWeaponId ?? string.Empty;
            StartingWeaponIds = CopyWeaponIds(startingWeaponIds, StartingWeaponId);
            IsUnlockedByDefault = isUnlockedByDefault;
            UnlockRewardId = unlockRewardId ?? string.Empty;
            StartingStatModifiers = startingStatModifiers == null ? Array.Empty<SurvivorsClassStatModifierDefinition>() : Copy(startingStatModifiers);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string StartingWeaponId { get; }
        public IReadOnlyList<string> StartingWeaponIds { get; }
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

        private static string[] CopyWeaponIds(IReadOnlyList<string> source, string fallback)
        {
            var ids = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (source != null)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    AddWeaponId(ids, seen, source[i]);
                }
            }

            AddWeaponId(ids, seen, fallback);
            return ids.ToArray();
        }

        private static void AddWeaponId(List<string> ids, HashSet<string> seen, string value)
        {
            string normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            if (string.IsNullOrWhiteSpace(normalized) || !seen.Add(normalized))
            {
                return;
            }

            ids.Add(normalized);
        }
    }

    public sealed class SurvivorsClassLibraryDefinition
    {
        public SurvivorsClassLibraryDefinition(IReadOnlyList<SurvivorsClassDefinition> classes, string defaultClassId = null)
        {
            Classes = classes == null ? Array.Empty<SurvivorsClassDefinition>() : Copy(classes);
            DefaultClassId = defaultClassId ?? string.Empty;
        }

        public IReadOnlyList<SurvivorsClassDefinition> Classes { get; }
        public string DefaultClassId { get; }

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
            if (!string.IsNullOrWhiteSpace(DefaultClassId) && TryGetClass(DefaultClassId, out SurvivorsClassDefinition defaultClass) && defaultClass.IsUnlockedByDefault)
            {
                return defaultClass;
            }

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

    public sealed class SurvivorsClassUpgradeGateDefinition
    {
        public SurvivorsClassUpgradeGateDefinition(string upgradeId, IReadOnlyList<string> allowedClassIds)
        {
            UpgradeId = upgradeId ?? string.Empty;
            AllowedClassIds = CopyAllowedClassIds(allowedClassIds);
        }

        public string UpgradeId { get; }
        public IReadOnlyList<string> AllowedClassIds { get; }

        public bool IsAvailableToClass(SurvivorsClassDefinition selectedClass)
        {
            if (AllowedClassIds.Count == 0)
            {
                return true;
            }

            if (selectedClass == null || string.IsNullOrWhiteSpace(selectedClass.Id))
            {
                return false;
            }

            for (int i = 0; i < AllowedClassIds.Count; i++)
            {
                if (string.Equals(AllowedClassIds[i], selectedClass.Id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] CopyAllowedClassIds(IReadOnlyList<string> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<string>();
            }

            var ids = new List<string>(source.Count);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < source.Count; i++)
            {
                string normalized = string.IsNullOrWhiteSpace(source[i]) ? string.Empty : source[i].Trim();
                if (string.IsNullOrWhiteSpace(normalized) || !seen.Add(normalized))
                {
                    continue;
                }

                ids.Add(normalized);
            }

            return ids.ToArray();
        }
    }
}
