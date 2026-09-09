using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsDraftProgress
    {
        public readonly int RunSeed, Level, SelectedUpgradeCount, MinibossKilledCount, BossKilledCount;
        public SurvivorsDraftProgress(int runSeed, int level, int selectedUpgradeCount, int minibossKilledCount, int bossKilledCount)
        {
            RunSeed = runSeed; Level = level; SelectedUpgradeCount = selectedUpgradeCount;
            MinibossKilledCount = minibossKilledCount; BossKilledCount = bossKilledCount;
        }
        public int ResolveSeed(SurvivorsRewardSelectionKind kind) => RunSeed + Level + SelectedUpgradeCount +
            MinibossKilledCount + BossKilledCount * 11 + (kind == SurvivorsRewardSelectionKind.EliteUpgrade ? 173 :
                kind == SurvivorsRewardSelectionKind.BossUpgrade ? 313 : 0);
    }

    /// <summary>Composes eligible pools and guarantees into deterministic offers; it never selects or spends ranks.</summary>
    internal sealed class SurvivorsDraftOfferGenerator
    {
        private readonly SurvivorsRunBuildState RunBuild;
        private readonly SurvivorsDraftRarityPolicy DraftRarity;
        private readonly Func<SurvivorsTemplateTuning> _tuning;
        private readonly Func<SurvivorsDraftProgress> _progress;
        private SurvivorsTemplateTuning CurrentTuning => _tuning();
        public SurvivorsDraftCatalogPolicy Catalogs { get; }
        public SurvivorsDraftGuaranteePolicy Guarantees { get; }
        public SurvivorsDraftOfferGenerator(SurvivorsRunBuildState build, SurvivorsDraftRarityPolicy rarity,
            Func<SurvivorsTemplateTuning> tuning, Func<SurvivorsDraftProgress> progress)
        {
            RunBuild = build ?? throw new ArgumentNullException(nameof(build));
            DraftRarity = rarity ?? throw new ArgumentNullException(nameof(rarity));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            Catalogs = new SurvivorsDraftCatalogPolicy(build, rarity, tuning);
            Guarantees = new SurvivorsDraftGuaranteePolicy(build, rarity, Catalogs, tuning, progress);
        }
        public bool TryGenerate(
            SurvivorsRewardSelectionKind selectionKind,
            int rerollIndex,
            IReadOnlyList<RunUpgradeId> lockedChoices,
            out RunUpgradeDraft draft)
        {
            draft = null;
            RunUpgradeCatalog draftCatalog;
            IReadOnlyList<RunUpgradeId> resolvedLocks = lockedChoices;
            bool requireEvolutionChoice = false;
            switch (selectionKind)
            {
                case SurvivorsRewardSelectionKind.LevelUp:
                    DraftRarityProfile normalProfile = DraftRarity.ResolveNormalDraftRarityProfile(_progress().Level);
                    draftCatalog = Catalogs.CreateEligibleDraftCatalog(normalProfile);
                    if (resolvedLocks == null || resolvedLocks.Count == 0)
                    {
                        resolvedLocks = Guarantees.CreateNormalDraftChoiceLocks(normalProfile, rerollIndex);
                    }
                    break;
                case SurvivorsRewardSelectionKind.EliteUpgrade:
                    draftCatalog = Catalogs.CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Miniboss, requireEvolutionChoice: false);
                    resolvedLocks = Guarantees.CreateRewardDraftChoiceLocks(SurvivorsEnemyRole.Miniboss, CurrentTuning.DraftChoiceCount, rerollIndex);
                    break;
                case SurvivorsRewardSelectionKind.BossUpgrade:
                    draftCatalog = Catalogs.CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Boss, requireEvolutionChoice: false);
                    resolvedLocks = Guarantees.CreateRewardDraftChoiceLocks(SurvivorsEnemyRole.Boss, CurrentTuning.DraftChoiceCount, rerollIndex);
                    break;
                default:
                    return false;
            }

            if (draftCatalog == null)
            {
                return false;
            }

            int choiceCount = Mathf.Max(0, CurrentTuning.DraftChoiceCount);
            if (choiceCount <= 0)
            {
                return false;
            }

            draft = RunUpgradeDraftService.Generate(
                draftCatalog,
                RunBuild.State,
                new RunUpgradeDraftRequest(
                    choiceCount,
                    _progress().ResolveSeed(selectionKind),
                    Mathf.Max(0, rerollIndex),
                    resolvedLocks == null || resolvedLocks.Count == 0 ? null : resolvedLocks));
            if (draft == null || draft.Choices.Count == 0)
            {
                draft = null;
                return false;
            }

            if (requireEvolutionChoice && !Catalogs.DraftContainsEvolution(draft))
            {
                draft = null;
                return false;
            }

            return true;
        }
    }
}
