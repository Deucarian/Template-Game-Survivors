using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns guaranteed rarity, primer-passive and evolution choices, preserving priority and seed salts.</summary>
    internal sealed class SurvivorsDraftGuaranteePolicy
    {
        private const int NormalDraftRarityLockSeedSalt = 7919;
        private const int EarlyPassiveDraftLockSeedSalt = 12289;
        private const int EvolutionPrimerPassiveDraftLockSeedSalt = 17443;
        private const int RewardDraftRarityLockSeedSalt = 23831;
        private readonly SurvivorsRunBuildState RunBuild;
        private readonly SurvivorsDraftRarityPolicy DraftRarity;
        private readonly SurvivorsDraftCatalogPolicy Catalogs;
        private readonly Func<SurvivorsTemplateTuning> _tuning;
        private readonly Func<SurvivorsDraftProgress> _progress;
        private SurvivorsTemplateTuning CurrentTuning => _tuning();
        public SurvivorsDraftGuaranteePolicy(SurvivorsRunBuildState build, SurvivorsDraftRarityPolicy rarity,
            SurvivorsDraftCatalogPolicy catalogs, Func<SurvivorsTemplateTuning> tuning, Func<SurvivorsDraftProgress> progress)
        {
            RunBuild = build;
            DraftRarity = rarity;
            Catalogs = catalogs;
            _tuning = tuning;
            _progress = progress;
        }
        private IReadOnlyList<RunUpgradeId> CreateNormalDraftRarityLocks(
            DraftRarityProfile profile,
            int rerollIndex)
        {
            if (!TryResolveNormalDraftGuaranteedMinimumRarity(profile, out RunUpgradeRarity minimumRarity))
            {
                return Array.Empty<RunUpgradeId>();
            }

            RunUpgradeCatalog highRarityCatalog = Catalogs.CreateEligibleDraftCatalog(
                profile,
                minimumRarity,
                requireMinimumRarity: true);
            if (highRarityCatalog == null || highRarityCatalog.Definitions.Count == 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                highRarityCatalog,
                RunBuild.State,
                new RunUpgradeDraftRequest(
                    1,
                    _progress().ResolveSeed(SurvivorsRewardSelectionKind.LevelUp) + NormalDraftRarityLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            return new[] { lockDraft.Choices[0].Id };
        }

        public IReadOnlyList<RunUpgradeId> CreateNormalDraftChoiceLocks(
            DraftRarityProfile profile,
            int rerollIndex)
        {
            int choiceCount = Mathf.Max(0, CurrentTuning.DraftChoiceCount);
            if (choiceCount <= 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            var locks = new List<RunUpgradeId>(choiceCount);
            IReadOnlyList<RunUpgradeId> rarityLocks = CreateNormalDraftRarityLocks(profile, rerollIndex);
            if (rarityLocks != null)
            {
                for (int i = 0; i < rarityLocks.Count && locks.Count < choiceCount; i++)
                {
                    AddUniqueDraftLock(locks, rarityLocks[i], choiceCount);
                }
            }

            if (TryCreateEvolutionPrimerPassiveChoiceLock(profile, rerollIndex, out RunUpgradeId evolutionPassiveLock))
            {
                AddUniqueDraftLock(locks, evolutionPassiveLock, choiceCount);
            }

            if (TryCreateEarlyPassiveChoiceLock(profile, rerollIndex, out RunUpgradeId passiveLock))
            {
                AddUniqueDraftLock(locks, passiveLock, choiceCount);
            }

            return locks.Count == 0 ? Array.Empty<RunUpgradeId>() : locks.ToArray();
        }

        private bool TryCreateEvolutionPrimerPassiveChoiceLock(
            DraftRarityProfile profile,
            int rerollIndex,
            out RunUpgradeId passiveLock)
        {
            passiveLock = default;
            if (RunBuild.Catalog == null ||
                RunBuild.State == null ||
                RunBuild.ActivePassiveCount >= RunBuild.MaxPassiveSlots ||
                CurrentTuning.DraftChoiceCount <= 0)
            {
                return false;
            }

            var candidates = new List<RunUpgradeDefinition>(RunBuild.Catalog.Definitions.Count);
            var seenPassiveIds = new HashSet<RunUpgradeId>();
            for (int i = 0; i < RunBuild.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition evolution = RunBuild.Catalog.Definitions[i];
                if (!Catalogs.TryResolveEvolutionMissingPassive(evolution, out RunUpgradeDefinition passive) ||
                    !seenPassiveIds.Add(passive.Id))
                {
                    continue;
                }

                candidates.Add(passive);
            }

            RunUpgradeCatalog passiveCatalog = DraftRarity.CreateWeightedDraftCatalog(candidates, profile);
            if (passiveCatalog == null || passiveCatalog.Definitions.Count == 0)
            {
                return false;
            }

            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                passiveCatalog,
                RunBuild.State,
                new RunUpgradeDraftRequest(
                    1,
                    _progress().ResolveSeed(SurvivorsRewardSelectionKind.LevelUp) + EvolutionPrimerPassiveDraftLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return false;
            }

            passiveLock = lockDraft.Choices[0].Id;
            return true;
        }

        private bool TryCreateEarlyPassiveChoiceLock(
            DraftRarityProfile profile,
            int rerollIndex,
            out RunUpgradeId passiveLock)
        {
            passiveLock = default;
            if (RunBuild.Catalog == null ||
                RunBuild.ActivePassiveCount > 0 ||
                RunBuild.ActivePassiveCount >= RunBuild.MaxPassiveSlots ||
                CurrentTuning.DraftChoiceCount <= 0)
            {
                return false;
            }

            var candidates = new List<RunUpgradeDefinition>(RunBuild.Catalog.Definitions.Count);
            for (int i = 0; i < RunBuild.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = RunBuild.Catalog.Definitions[i];
                if (definition == null ||
                    definition.Rarity > RunUpgradeRarity.Uncommon ||
                    !RunBuild.IsUpgradeEligibleForCurrentBuild(definition) ||
                    !RunBuild.TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata) ||
                    metadata.Category != SurvivorsRunUpgradeCategory.Passive ||
                    !metadata.UsesPassiveSlot ||
                    RunBuild.State.GetRank(definition.Id) > 0)
                {
                    continue;
                }

                candidates.Add(definition);
            }

            RunUpgradeCatalog passiveCatalog = DraftRarity.CreateWeightedDraftCatalog(candidates, profile);
            if (passiveCatalog == null || passiveCatalog.Definitions.Count == 0)
            {
                return false;
            }

            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                passiveCatalog,
                RunBuild.State,
                new RunUpgradeDraftRequest(
                    1,
                    _progress().ResolveSeed(SurvivorsRewardSelectionKind.LevelUp) + EarlyPassiveDraftLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return false;
            }

            passiveLock = lockDraft.Choices[0].Id;
            return true;
        }

        private static void AddUniqueDraftLock(List<RunUpgradeId> locks, RunUpgradeId candidate, int maxCount)
        {
            if (locks == null || locks.Count >= maxCount)
            {
                return;
            }

            for (int i = 0; i < locks.Count; i++)
            {
                if (locks[i].Equals(candidate))
                {
                    return;
                }
            }

            locks.Add(candidate);
        }

        private static bool TryResolveNormalDraftGuaranteedMinimumRarity(
            DraftRarityProfile profile,
            out RunUpgradeRarity minimumRarity)
        {
            switch (profile)
            {
                case DraftRarityProfile.NormalMid:
                    minimumRarity = RunUpgradeRarity.Rare;
                    return true;
                case DraftRarityProfile.NormalLate:
                    minimumRarity = RunUpgradeRarity.Epic;
                    return true;
                default:
                    minimumRarity = RunUpgradeRarity.Common;
                    return false;
            }
        }

        public IReadOnlyList<RunUpgradeId> CreateEligibleEvolutionChoiceLocks(int maxCount)
        {
            if (RunBuild.Catalog == null || maxCount <= 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            var locks = new List<RunUpgradeId>(Mathf.Min(maxCount, RunBuild.Catalog.Definitions.Count));
            for (int i = 0; i < RunBuild.Catalog.Definitions.Count && locks.Count < maxCount; i++)
            {
                RunUpgradeDefinition definition = RunBuild.Catalog.Definitions[i];
                if (definition != null && Catalogs.IsEvolutionUpgrade(definition) && RunBuild.IsUpgradeEligibleForCurrentBuild(definition))
                {
                    locks.Add(definition.Id);
                }
            }

            return locks;
        }

        public IReadOnlyList<RunUpgradeId> CreateRewardDraftChoiceLocks(
            SurvivorsEnemyRole role,
            int maxCount,
            int rerollIndex)
        {
            if (maxCount <= 0)
            {
                return Array.Empty<RunUpgradeId>();
            }

            IReadOnlyList<RunUpgradeId> evolutionLocks = CreateEligibleEvolutionChoiceLocks(maxCount);
            if (evolutionLocks != null && evolutionLocks.Count > 0)
            {
                return evolutionLocks;
            }

            return TryCreateRewardRarityChoiceLock(role, rerollIndex, out RunUpgradeId rarityLock)
                ? new[] { rarityLock }
                : Array.Empty<RunUpgradeId>();
        }

        private bool TryCreateRewardRarityChoiceLock(
            SurvivorsEnemyRole role,
            int rerollIndex,
            out RunUpgradeId rarityLock)
        {
            rarityLock = default;
            if (!TryResolveRewardDraftGuaranteedMinimumRarity(role, out RunUpgradeRarity minimumRarity))
            {
                return false;
            }

            DraftRarityProfile profile = role == SurvivorsEnemyRole.Boss ? DraftRarityProfile.Boss : DraftRarityProfile.Elite;
            RunUpgradeCatalog highRarityCatalog = Catalogs.CreateEligibleDraftCatalog(
                profile,
                minimumRarity,
                requireMinimumRarity: true);
            if (highRarityCatalog == null || highRarityCatalog.Definitions.Count == 0)
            {
                return false;
            }

            SurvivorsRewardSelectionKind selectionKind = role == SurvivorsEnemyRole.Boss
                ? SurvivorsRewardSelectionKind.BossUpgrade
                : SurvivorsRewardSelectionKind.EliteUpgrade;
            RunUpgradeDraft lockDraft = RunUpgradeDraftService.Generate(
                highRarityCatalog,
                RunBuild.State,
                new RunUpgradeDraftRequest(
                    1,
                    _progress().ResolveSeed(selectionKind) + RewardDraftRarityLockSeedSalt,
                    Mathf.Max(0, rerollIndex)));
            if (lockDraft == null || lockDraft.Choices.Count == 0)
            {
                return false;
            }

            rarityLock = lockDraft.Choices[0].Id;
            return true;
        }

        private static bool TryResolveRewardDraftGuaranteedMinimumRarity(
            SurvivorsEnemyRole role,
            out RunUpgradeRarity minimumRarity)
        {
            if (role == SurvivorsEnemyRole.Boss)
            {
                minimumRarity = RunUpgradeRarity.Rare;
                return true;
            }

            minimumRarity = RunUpgradeRarity.Uncommon;
            return role == SurvivorsEnemyRole.Elite ||
                role == SurvivorsEnemyRole.DreadElite ||
                role == SurvivorsEnemyRole.Miniboss;
        }
    }
}
