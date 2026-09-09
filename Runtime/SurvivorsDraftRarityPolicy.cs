using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    internal enum DraftRarityProfile
    {
        NormalEarly = 0,
        NormalMid = 1,
        NormalLate = 2,
        Elite = 3,
        Boss = 4
    }

    /// <summary>Owns authored rarity profiles, luck weighting and weighted catalog construction.</summary>
    internal sealed class SurvivorsDraftRarityPolicy
    {
        private readonly Func<SurvivorsTemplateTuning> _tuning;
        private readonly Func<float> _luck;
        private SurvivorsTemplateTuning CurrentTuning => _tuning();
        private float DraftLuckBonus => _luck();
        public SurvivorsDraftRarityPolicy(Func<SurvivorsTemplateTuning> tuning, Func<float> luck)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _luck = luck ?? throw new ArgumentNullException(nameof(luck));
        }
        public DraftRarityProfile ResolveNormalDraftRarityProfile(int level)
        {
            if (level >= Mathf.Max(1, CurrentTuning.DraftLateRarityLevel))
            {
                return DraftRarityProfile.NormalLate;
            }

            if (level >= Mathf.Max(1, CurrentTuning.DraftMidRarityLevel))
            {
                return DraftRarityProfile.NormalMid;
            }

            return DraftRarityProfile.NormalEarly;
        }

        public RunUpgradeCatalog CreateWeightedDraftCatalog(IReadOnlyList<RunUpgradeDefinition> definitions, DraftRarityProfile profile)
        {
            if (definitions == null || definitions.Count == 0)
            {
                return null;
            }

            var weighted = new List<RunUpgradeDefinition>(definitions.Count);
            for (int i = 0; i < definitions.Count; i++)
            {
                RunUpgradeDefinition definition = definitions[i];
                if (definition == null)
                {
                    continue;
                }

                int rarityWeight = ResolveDraftRarityWeight(profile, definition.Rarity);
                if (rarityWeight <= 0)
                {
                    continue;
                }

                weighted.Add(CloneUpgradeWithDraftWeight(definition, ResolveWeightedDraftWeight(definition.Weight, rarityWeight)));
            }

            return weighted.Count == 0 ? null : new RunUpgradeCatalog(weighted);
        }

        private int ResolveWeightedDraftWeight(int baseWeight, int rarityWeight)
        {
            long resolved = (long)Mathf.Max(1, baseWeight) * Mathf.Max(0, rarityWeight);
            resolved = Mathf.Max(1, Mathf.RoundToInt(resolved / 100f));
            return resolved > int.MaxValue ? int.MaxValue : (int)resolved;
        }

        private RunUpgradeDefinition CloneUpgradeWithDraftWeight(RunUpgradeDefinition definition, int weight)
        {
            return new RunUpgradeDefinition(
                definition.Id,
                definition.Rarity,
                Mathf.Max(1, weight),
                definition.MaxRank,
                definition.Effects,
                definition.Prerequisites,
                definition.Exclusions);
        }

        public int ResolveDraftRarityWeight(DraftRarityProfile profile, RunUpgradeRarity rarity)
        {
            return ApplyDraftLuckToRarityWeight(ResolveBaseDraftRarityWeight(profile, rarity), rarity);
        }

        private int ResolveBaseDraftRarityWeight(DraftRarityProfile profile, RunUpgradeRarity rarity)
        {
            switch (profile)
            {
                case DraftRarityProfile.NormalEarly:
                    return ResolveNormalEarlyRarityWeight(rarity);
                case DraftRarityProfile.NormalMid:
                    return ResolveNormalMidRarityWeight(rarity);
                case DraftRarityProfile.NormalLate:
                    return ResolveNormalLateRarityWeight(rarity);
                case DraftRarityProfile.Elite:
                    return ResolveEliteRarityWeight(rarity);
                case DraftRarityProfile.Boss:
                    return ResolveBossRarityWeight(rarity);
                default:
                    return 100;
            }
        }

        private int ApplyDraftLuckToRarityWeight(int baseWeight, RunUpgradeRarity rarity)
        {
            if (baseWeight <= 0 || DraftLuckBonus <= 0f)
            {
                return baseWeight;
            }

            float luck = Mathf.Max(0f, DraftLuckBonus);
            float multiplier;
            switch (rarity)
            {
                case RunUpgradeRarity.Common:
                    multiplier = Mathf.Max(0.35f, 1f - luck * 0.35f);
                    break;
                case RunUpgradeRarity.Uncommon:
                    multiplier = 1f + luck * 0.15f;
                    break;
                case RunUpgradeRarity.Rare:
                    multiplier = 1f + luck;
                    break;
                case RunUpgradeRarity.Epic:
                    multiplier = 1f + luck * 1.6f;
                    break;
                case RunUpgradeRarity.Legendary:
                    multiplier = 1f + luck * 2.2f;
                    break;
                default:
                    multiplier = 1f;
                    break;
            }

            return Mathf.Max(1, Mathf.RoundToInt(baseWeight * multiplier));
        }

        private int ResolveNormalEarlyRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.NormalEarlyCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.NormalEarlyUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.NormalEarlyRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.NormalEarlyEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.NormalEarlyLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveNormalMidRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.NormalMidCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.NormalMidUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.NormalMidRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.NormalMidEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.NormalMidLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveNormalLateRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.NormalLateCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.NormalLateUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.NormalLateRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.NormalLateEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.NormalLateLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveEliteRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.EliteCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.EliteUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.EliteRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.EliteEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.EliteLegendaryWeight;
                default: return 0;
            }
        }

        private int ResolveBossRarityWeight(RunUpgradeRarity rarity)
        {
            switch (rarity)
            {
                case RunUpgradeRarity.Common: return CurrentTuning.BossCommonWeight;
                case RunUpgradeRarity.Uncommon: return CurrentTuning.BossUncommonWeight;
                case RunUpgradeRarity.Rare: return CurrentTuning.BossRareWeight;
                case RunUpgradeRarity.Epic: return CurrentTuning.BossEpicWeight;
                case RunUpgradeRarity.Legendary: return CurrentTuning.BossLegendaryWeight;
                default: return 0;
            }
        }
    }
}
