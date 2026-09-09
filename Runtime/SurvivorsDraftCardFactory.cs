using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Projects the live build and authored theme into cards without selecting or applying rewards.</summary>
    internal sealed class SurvivorsDraftCardFactory
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly Func<string, string> _shortWeaponName;
        private readonly SurvivorsUpgradePreviewFormatter _preview;

        public SurvivorsDraftCardFactory(SurvivorsRunBuildState build, Func<string, string> shortWeaponName)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _shortWeaponName = shortWeaponName ?? throw new ArgumentNullException(nameof(shortWeaponName));
            _preview = new SurvivorsUpgradePreviewFormatter(build, shortWeaponName);
        }

        public SurvivorsDraftCard CreateUpgradeCard(int index, RunUpgradeDefinition choice, SurvivorsUiTheme theme, in SurvivorsDraftPreviewValues values)
        {
            if (choice == null)
            {
                return new SurvivorsDraftCard
                {
                    Index = index,
                    Hotkey = (index + 1).ToString(),
                    Name = "Missing Choice",
                    RarityLabel = "Missing",
                    CategoryId = "MetaReward",
                    CategoryLabel = theme.GetCategoryDisplayName("MetaReward", "Meta/Reward"),
                    AffectedLabel = "Affects Build",
                    RankLabel = "No rank",
                    Description = "Missing upgrade definition.",
                    EffectPreview = "No effect preview.",
                    RequirementHint = string.Empty,
                    IconId = theme.GetCategoryIconId("MetaReward", "reward"),
                    StyleToken = theme.GetRarityStyleToken("Common"),
                    AccentColor = Color.white
                };
            }

            _build.TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata);
            SurvivorsRunUpgradeCategory category = _build.ResolveCurrentUpgradeCategory(choice);
            string categoryId = ResolvePlayerUpgradeCategoryId(choice, category, metadata);
            bool isEvolution = category == SurvivorsRunUpgradeCategory.Evolution;
            string rarityId = isEvolution ? "Evolution" : choice.Rarity.ToString();
            Color fallbackAccent = isEvolution
                ? new Color(1f, 0.38f, 0.56f)
                : ResolveRarityAccentColor(choice.Rarity);
            int currentRank = _build.State == null ? 0 : _build.State.GetRank(choice.Id);
            int nextRank = Mathf.Min(choice.MaxRank, currentRank + 1);
            return new SurvivorsDraftCard
            {
                Index = index,
                Hotkey = (index + 1).ToString(),
                Name = _build.ResolveUpgradeDisplayName(choice.Id),
                RarityLabel = theme.GetRarityDisplayName(rarityId),
                CategoryId = categoryId,
                CategoryLabel = theme.GetCategoryDisplayName(categoryId, FormatUpgradeCategoryLabel(category)),
                AffectedLabel = ResolveUpgradeAffectedLabel(choice),
                RankLabel = choice.MaxRank <= 1 ? "One-time unlock" : $"Rank {currentRank}->{nextRank}/{choice.MaxRank}",
                Description = metadata == null ? _build.ResolveUpgradeDisplayName(choice.Id) : metadata.Description,
                EffectPreview = _preview.ResolveUpgradeEffectPreview(choice, values),
                RequirementHint = ResolveUpgradeRequirementHint(choice, metadata, category),
                IconId = theme.GetCategoryIconId(categoryId, categoryId),
                StyleToken = theme.GetRarityStyleToken(rarityId),
                IsEvolution = isEvolution,
                AccentColor = theme.GetRarityAccentColor(rarityId, fallbackAccent)
            };
        }

        public SurvivorsDraftCard CreateRelicCard(int index, SurvivorsRelicDefinition relic, SurvivorsUiTheme theme)
        {
            Color accent = theme.GetRarityAccentColor("Relic", ResolveRelicAccentColor(relic));
            return new SurvivorsDraftCard
            {
                Index = index,
                Hotkey = (index + 1).ToString(),
                Name = relic == null || string.IsNullOrWhiteSpace(relic.DisplayName) ? "Boss Relic" : relic.DisplayName,
                RarityLabel = theme.GetRarityDisplayName("Relic"),
                CategoryId = "Relic",
                CategoryLabel = theme.GetCategoryDisplayName("Relic", "Relic"),
                AffectedLabel = relic == null ? "Affects Build" : "Affects " + _shortWeaponName(relic.TargetId),
                RankLabel = "Run relic",
                Description = relic == null ? "Missing relic definition." : FormatRelicEffectSummary(relic),
                EffectPreview = relic == null ? "No effect preview." : _preview.ResolveRelicEffectPreview(relic),
                RequirementHint = "Unique boss relic for this run.",
                IconId = theme.GetCategoryIconId("Relic", "relic"),
                StyleToken = theme.GetRarityStyleToken("Relic"),
                AccentColor = accent
            };
        }

        private string ResolvePlayerUpgradeCategoryId(RunUpgradeDefinition choice, SurvivorsRunUpgradeCategory category, SurvivorsRunUpgradeMetadata metadata)
        {
            if (choice != null && SurvivorsUpgradePreviewFormatter.IsPickupMagnetUpgrade(choice))
            {
                return "PickupMagnet";
            }

            if (category == SurvivorsRunUpgradeCategory.Weapon)
            {
                return "NewWeapon";
            }

            return category.ToString();
        }

        private string ResolveUpgradeRequirementHint(RunUpgradeDefinition choice, SurvivorsRunUpgradeMetadata metadata, SurvivorsRunUpgradeCategory category)
        {
            if (choice == null || metadata == null)
            {
                return string.Empty;
            }

            if (category == SurvivorsRunUpgradeCategory.Evolution)
            {
                string weapon = _shortWeaponName(metadata.AffectedContentId);
                string rank = string.IsNullOrWhiteSpace(metadata.RequiredUpgradeId)
                    ? "max weapon rank"
                    : _build.ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredUpgradeId)) + " rank " + _build.ResolveRequiredUpgradeRank(metadata).ToString();
                string passive = string.IsNullOrWhiteSpace(metadata.RequiredPassiveUpgradeId)
                    ? "matching passive"
                    : _build.ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredPassiveUpgradeId));
                return "Evolution path: " + weapon + " needs " + rank + " plus " + passive + ".";
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredUpgradeId))
            {
                return "Requires " + _build.ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredUpgradeId)) + " rank " + _build.ResolveRequiredUpgradeRank(metadata).ToString() + ".";
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredPassiveUpgradeId))
            {
                return "Requires " + _build.ResolveUpgradeDisplayName(new RunUpgradeId(metadata.RequiredPassiveUpgradeId)) + ".";
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredOwnedWeaponId))
            {
                return "Requires " + _shortWeaponName(metadata.RequiredOwnedWeaponId) + ".";
            }

            if (SurvivorsUpgradePreviewFormatter.IsPickupMagnetUpgrade(choice))
            {
                return "Pickup build: improves gem reach without bypassing draft pacing.";
            }

            return string.Empty;
        }

        public string ResolveUpgradeAffectedLabel(RunUpgradeDefinition choice)
        {
            if (choice == null || !_build.TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return "Build";
            }

            if (string.IsNullOrWhiteSpace(metadata.AffectedContentId))
            {
                return "Build";
            }

            return "Affects " + _shortWeaponName(metadata.AffectedContentId);
        }

        public static string FormatUpgradeCategoryLabel(SurvivorsRunUpgradeCategory category)
        {
            switch (category)
            {
                case SurvivorsRunUpgradeCategory.Weapon:
                    return "Weapon";
                case SurvivorsRunUpgradeCategory.WeaponUpgrade:
                    return "Weapon Upgrade";
                case SurvivorsRunUpgradeCategory.Passive:
                    return "Passive";
                case SurvivorsRunUpgradeCategory.PassiveUpgrade:
                    return "Passive Upgrade";
                case SurvivorsRunUpgradeCategory.Mutation:
                    return "Mutation";
                case SurvivorsRunUpgradeCategory.Evolution:
                    return "Evolution";
                default:
                    return category.ToString();
            }
        }

        public static Color ResolveRarityAccentColor(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common:
                    return new Color(0.78f, 0.84f, 0.88f);
                case RunUpgradeRarity.Uncommon:
                    return new Color(0.34f, 0.94f, 0.56f);
                case RunUpgradeRarity.Rare:
                    return new Color(0.34f, 0.7f, 1f);
                case RunUpgradeRarity.Epic:
                    return new Color(0.9f, 0.46f, 1f);
                case RunUpgradeRarity.Legendary:
                    return new Color(1f, 0.76f, 0.2f);
                default:
                    return Color.white;
            }
        }

        public static Color ResolveRelicAccentColor(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return Color.white;
            }

            switch (relic.EffectKind)
            {
                case SurvivorsRelicEffectKind.DamageBonus:
                    return new Color(1f, 0.45f, 0.28f);
                case SurvivorsRelicEffectKind.CooldownMultiplier:
                    return new Color(0.34f, 0.88f, 1f);
                case SurvivorsRelicEffectKind.PickupRange:
                    return new Color(0.42f, 1f, 0.6f);
                default:
                    return new Color(1f, 0.76f, 0.2f);
            }
        }

        public string FormatRelicEffectSummary(SurvivorsRelicDefinition relic) => _preview.FormatRelicEffectSummary(relic);
    }
}
