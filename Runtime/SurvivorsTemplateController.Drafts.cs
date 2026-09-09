using System;

using System.Collections.Generic;

using Deucarian.Common;

using Deucarian.Combat;

using Deucarian.GameplayFoundation;

using Deucarian.Persistence;

using Deucarian.Persistence.Unity;

using Deucarian.Projectiles;

using Deucarian.RunUpgrades;

using Deucarian.WeaponSystems;

using Deucarian.WorldSpawning;

using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    // Draft choice, selection and reward header bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private SurvivorsDraftFeedback DraftFeedback => _draftFeedback ?? (_draftFeedback = new SurvivorsDraftFeedback(this));

        void ISurvivorsDraftFeedbackPort.RecordSelection(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition selected) => RecordRewardSelectionFeedback(kind, selected);

        void ISurvivorsDraftFeedbackPort.PlayChoiceAudio() => PlayAudioEvent(AudioEventDraftChoiceSelected, _levelUpClip, 0.06f);

        bool ISurvivorsDraftFeedbackPort.IsEvolution(RunUpgradeDefinition selected) => DraftOffers.Catalogs.IsEvolutionUpgrade(selected);

        void ISurvivorsDraftFeedbackPort.PlayEvolutionAudio() => PlayAudioEvent(AudioEventEvolution, _levelUpClip, 0.08f);

        void ISurvivorsDraftFeedbackPort.TriggerLevelPulse(RunUpgradeDefinition selected) => SelectionRewards.TriggerLevelUpPulse(selected);

        bool ISurvivorsDraftFeedbackPort.IsRewardUpgradeKind(SurvivorsRewardSelectionKind kind) => IsRewardUpgradeSelectionKind(kind);

        void ISurvivorsDraftFeedbackPort.TriggerJackpot(RunUpgradeDefinition selected, SurvivorsRewardSelectionKind kind) => SelectionRewards.TriggerRewardJackpot(selected, kind);

        void ISurvivorsDraftFeedbackPort.TriggerSurge(RunUpgradeDefinition selected, SurvivorsRewardSelectionKind kind) => SelectionRewards.TriggerRewardUpgradeSurge(selected, kind);

        void ISurvivorsDraftFeedbackPort.RecordFirstLevelUpDraft() => Telemetry.Record(SurvivorsRunMetric.FirstLevelUpDraft, RunTimeSeconds);

        void ISurvivorsDraftFeedbackPort.RecordRelicCards(SurvivorsRelicDraft draft) => RecordRewardCardPresentation(draft);

        void ISurvivorsDraftFeedbackPort.RecordUpgradeCards(SurvivorsRewardSelectionKind kind, RunUpgradeDraft draft) => RecordRewardCardPresentation(kind, draft);

        void ISurvivorsDraftFeedbackPort.PlayLevelUpAudio() => PlayAudioEvent(AudioEventLevelUp, _levelUpClip, 0.12f);

        void ISurvivorsDraftFeedbackPort.PlayOpeningPulse(bool levelUp, int count) => PlayFeedback(levelUp ? _levelUpPulse : _bossPulse, PlayerPosition, count, levelUp ? _levelUpClip : _bossClip, AudioEventDraftOpened, 0.1f);

        private SurvivorsDraftHeader DraftHeader => _draftHeader ?? (_draftHeader = new SurvivorsDraftHeader(this));

        bool ISurvivorsDraftHeaderReadPort.IsRelicChoiceOpen => IsRelicChoiceOpen;

        RunUpgradeDraft ISurvivorsDraftHeaderReadPort.CurrentDraft => DraftSession.CurrentDraft;

        SurvivorsRunUpgradeCategory ISurvivorsDraftHeaderReadPort.ResolveCategory(RunUpgradeDefinition choice) => RunBuild.ResolveCurrentUpgradeCategory(choice);

        SurvivorsUiTheme ISurvivorsDraftHeaderReadPort.ActiveTheme => ActiveUiTheme;

        private SurvivorsDraftCardFactory DraftCards => _draftCardFactory ?? (_draftCardFactory = new SurvivorsDraftCardFactory(RunBuild, ShortWeaponName));

        private SurvivorsDraftCard CreateUpgradeDraftCard(int index, RunUpgradeDefinition choice) =>
            DraftCards.CreateUpgradeCard(index, choice, ActiveUiTheme, choice == null ? default : CaptureDraftPreviewValues());

        private SurvivorsDraftCard CreateRelicDraftCard(int index, SurvivorsRelicDefinition relic) =>
            DraftCards.CreateRelicCard(index, relic, ActiveUiTheme);

        private SurvivorsDraftPreviewValues CaptureDraftPreviewValues() => new SurvivorsDraftPreviewValues
        {
            ProjectileDamage = ProjectileDamage,
            WeaponCooldownSeconds = WeaponCooldownSeconds,
            PlayerMoveSpeed = PlayerMoveSpeed,
            CurrentPickupAttractRange = CurrentPickupAttractRange,
            CurrentPickupAttractionSpeed = CurrentPickupAttractionSpeed,
            CurrentPickupMagnetPulseIntervalSeconds = CurrentPickupMagnetPulseIntervalSeconds,
            MaxHealth = MaxHealth,
            OrbitRadiusBonus = OrbitRadiusBonus,
            PayloadExplosionRadiusBonus = PayloadExplosionRadiusBonus,
            PayloadTriggerRadiusBonus = PayloadTriggerRadiusBonus,
            PoisonDamageRatio = PoisonDamageRatio,
            BleedDamageRatio = BleedDamageRatio,
            ExecuteThresholdNormalized = ExecuteThresholdNormalized,
            CriticalChanceNormalized = CriticalChanceNormalized,
            CriticalDamageMultiplier = CriticalDamageMultiplier,
            DraftLuckBonus = DraftLuckBonus,
            DeathNovaDamage = DeathNovaDamage,
            DeathNovaRadius = DeathNovaRadius,
            LifestealRatio = LifestealRatio,
            BarrierCapacity = BarrierCapacity,
            BarrierRegenPerSecondBonus = BarrierRegenPerSecondBonus,
            BarrierOnDamageRatio = BarrierOnDamageRatio,
            ExperienceGainMultiplierBonus = ExperienceGainMultiplierBonus,
            AreaRadiusBonus = AreaRadiusBonus,
            OrbitBladeBonus = OrbitBladeBonus,
            MeleeTargetBonus = MeleeTargetBonus,
            BurstCountBonus = BurstCountBonus,
            BurstEchoBonus = BurstEchoBonus,
            TargetedBurstSigilBonus = TargetedBurstSigilBonus,
            ProjectileFanBonus = ProjectileFanBonus,
            ProjectilePierceBonus = ProjectilePierceBonus,
            ProjectileChainBonus = ProjectileChainBonus,
            ProjectileForkBonus = ProjectileForkBonus,
            ProjectileReturnBonus = ProjectileReturnBonus,
            HitscanPierceBonus = HitscanPierceBonus,
            PayloadCountBonus = PayloadCountBonus,
            PickupMagnetPulseBaseIntervalSeconds = CurrentTuning.PickupMagnetPulseBaseIntervalSeconds,
            PickupMagnetPulseMinimumIntervalSeconds = CurrentTuning.PickupMagnetPulseMinimumIntervalSeconds,
        };

        private SurvivorsRelicInventory RelicInventory => _relicInventory ?? (_relicInventory = new SurvivorsRelicInventory(
            () => { EnsureClassLibraryLoaded(); return _relicDefinitions; }, ApplyRelic,
            selected => { RecordRelicSelectionFeedback(selected); BuildSurges.TriggerBossRelicSurge(selected); }));

        private SurvivorsDraftSession DraftSession => _draftSession ?? (_draftSession = new SurvivorsDraftSession(
            RunBuild, DraftOffers, RelicInventory, _runSession, _experienceProgression, this));

        public bool ApplyUpgradeByIdForTest(string id) { EnsureRunStartedForTest(); return DraftSession.ApplyById(id); }

        public bool SelectUpgrade(int index) => DraftSession.Select(index);

        public bool RerollCurrentDraft() => DraftSession.Reroll();

        public bool SkipCurrentDraft() => DraftSession.Skip();

        public bool BanishDraftChoice(int index) => DraftSession.Banish(index);

        private void TickRewardSelectionTimeout(float deltaTime) => DraftSession.TickTimeout(deltaTime);

        SurvivorsTemplateTuning ISurvivorsDraftSessionPort.Tuning => CurrentTuning;

        int ISurvivorsDraftSessionPort.RerollCharges => TotalDraftRerollCharges;

        int ISurvivorsDraftSessionPort.RelicSeed => CurrentTuning.RunSeed + MinibossKilledCount + SelectedRelicCount + 97;

        void ISurvivorsDraftSessionPort.ResetScroll() => DraftScreen.ResetScroll();

        void ISurvivorsDraftSessionPort.ApplyUpgrade(RunUpgradeDefinition upgrade) => ApplyUpgrade(upgrade);

        void ISurvivorsDraftSessionPort.RecordDirectUpgrade(RunUpgradeDefinition upgrade) => RecordBestRewardMoment(upgrade);

        void ISurvivorsDraftSessionPort.PresentSelectedUpgrade(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition selected) => DraftFeedback.PresentSelectedUpgrade(kind, selected);

        void ISurvivorsDraftSessionPort.PresentDraft(SurvivorsRewardSelectionKind kind, RunUpgradeDraft upgradeDraft, SurvivorsRelicDraft relicDraft, bool opening) => DraftFeedback.PresentDraft(kind, upgradeDraft, relicDraft, opening);

        void ISurvivorsDraftSessionPort.PlayDraftOpened(SurvivorsRewardSelectionKind kind, SurvivorsEnemyRole role) => DraftFeedback.PlayDraftOpened(kind, role);

        void ISurvivorsDraftSessionPort.PlayReroll() => PlayFeedback(_levelUpPulse, PlayerPosition, 18, _levelUpClip, AudioEventDraftReroll, 0.08f);

        void ISurvivorsDraftSessionPort.PlayBanish() => PlayFeedback(_bossPulse, PlayerPosition, 12, _dangerClip, AudioEventDraftBanish, 0.08f);

        void ISurvivorsDraftSessionPort.GrantSkipReward(SurvivorsRewardSelectionKind kind)
        {
            RunRewards.AddBloodShards(DraftSkipBloodShards);
            RecordRewardSkipFeedback(kind);
        }

        void ISurvivorsDraftSessionPort.EnterVictory() => EnterVictory();

        private SurvivorsDraftOfferGenerator DraftOffers => _draftOffers ?? (_draftOffers = new SurvivorsDraftOfferGenerator(
            RunBuild, DraftRarity, () => CurrentTuning,
            () => new SurvivorsDraftProgress(CurrentTuning.RunSeed, Level, SelectedUpgradeCount, MinibossKilledCount, BossKilledCount)));

        private SurvivorsDraftRarityPolicy DraftRarity => _draftRarity ?? (_draftRarity = new SurvivorsDraftRarityPolicy(() => CurrentTuning, () => DraftLuckBonus));

        public int BossRelicDraftOpenCount => DraftSession.RelicOpenCount;

        public int EliteUpgradeDraftOpenCount => DraftSession.EliteOpenCount;

        public int BossUpgradeDraftOpenCount => DraftSession.BossOpenCount;

        public int LevelUpDraftOpenCount => DraftSession.LevelUpOpenCount;

        public int SelectedRelicCount => RelicInventory.Count;

        public int SelectedRewardUpgradeCount => DraftSession.SelectedRewardUpgradeCount;

        public int RewardAutoSelectCount => DraftSession.AutoSelectCount;

        public int DraftRerollCount => DraftSession.RerollCount;

        public int DraftBanishCount => DraftSession.BanishCount;

        public int DraftSkipCount => DraftSession.SkipCount;

        public int SelectedUpgradeCount => DraftSession.SelectedUpgradeCount;

        public float LevelUpDraftCooldownRemainingSeconds => Mathf.Max(0f, _experienceProgression.DraftCooldownRemaining);

        public bool IsPlayerFacingDraftOverlayVisible => State == SurvivorsRunState.LevelUp && (DraftSession.CurrentDraft != null || DraftSession.CurrentRelicDraft != null);

        public int CurrentDraftCardCountForTest => IsRelicChoiceOpen ? CurrentRelicChoices.Count : CurrentDraftChoices.Count;

        public string CurrentDraftOverlayTitleForTest => ResolveRewardOverlayTitle();

        public IReadOnlyList<RunUpgradeDefinition> CurrentDraftChoices => DraftSession.CurrentDraft == null ? EmptyChoices : DraftSession.CurrentDraft.Choices;

        public IReadOnlyList<SurvivorsRelicDefinition> CurrentRelicChoices => DraftSession.CurrentRelicDraft == null ? EmptyRelicChoices : DraftSession.CurrentRelicDraft.Choices;

        public float RewardSelectionRemainingSeconds => Mathf.Max(0f, DraftSession.RemainingSeconds);

        public bool IsRunUpgradeDraftOpen => State == SurvivorsRunState.LevelUp && DraftSession.Kind == SurvivorsRewardSelectionKind.LevelUp;

        public bool IsRelicChoiceOpen => State == SurvivorsRunState.LevelUp && DraftSession.Kind == SurvivorsRewardSelectionKind.BossRelic;

        public bool IsUpgradeRewardChoiceOpen => State == SurvivorsRunState.LevelUp && IsRewardUpgradeSelectionKind(DraftSession.Kind);

        public int TotalDraftRerollCharges => Mathf.Max(0, CurrentTuning.DraftRerollCharges + PersistentDraftRerollBonus);

        public int DraftRerollsRemaining => DraftSession.RerollsRemaining;

        public int DraftBanishesRemaining => DraftSession.BanishesRemaining;

        public int DraftSkipBloodShards => Mathf.Max(0, CurrentTuning.DraftSkipBloodShards);

        public float FirstLevelUpDraftTimeSeconds => Telemetry.FirstLevelUpDraftTimeSeconds;

        public int DraftOpenCount => DraftSession.OpenCount;

        public string GetCurrentDraftChoiceLabelForTest(int index)
        {
            EnsureRunStartedForTest();
            if (DraftSession.CurrentDraft == null || index < 0 || index >= DraftSession.CurrentDraft.Choices.Count)
            {
                return string.Empty;
            }

            return FormatUpgradeChoiceLabel(index, DraftSession.CurrentDraft.Choices[index]);
        }

        public string GetCurrentDraftCardSummaryForTest(int index)
        {
            EnsureRunStartedForTest();
            if (IsRelicChoiceOpen)
            {
                if (DraftSession.CurrentRelicDraft == null || index < 0 || index >= DraftSession.CurrentRelicDraft.Choices.Count)
                {
                    return string.Empty;
                }

                return CreateRelicDraftCard(index, DraftSession.CurrentRelicDraft.Choices[index]).Summary;
            }

            if (DraftSession.CurrentDraft == null || index < 0 || index >= DraftSession.CurrentDraft.Choices.Count)
            {
                return string.Empty;
            }

            return CreateUpgradeDraftCard(index, DraftSession.CurrentDraft.Choices[index]).Summary;
        }

        public int GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity rarity)
        {
            EnsureRunStartedForTest();
            return DraftRarity.ResolveDraftRarityWeight(DraftRarityProfile.NormalMid, rarity);
        }

        private void TickLevelUpDraftCooldown(float deltaTime)
        {
            _experienceProgression.Tick(deltaTime, CurrentTuning);
            DraftSession.TryOpenPending();
        }

        public bool OpenBossRelicDraftForTest()
        {
            EnsureRunStartedForTest();
            return DraftSession.OpenRelic();
        }

        public bool SelectRelicForTest(int index)
        {
            return DraftSession.SelectRelic(index);
        }

        public bool SelectDraftHotkeyForTest(int hotkeyNumber)
        {
            return SelectUpgrade(hotkeyNumber - 1);
        }

        private static bool IsRewardUpgradeSelectionKind(SurvivorsRewardSelectionKind selectionKind)
        {
            return selectionKind == SurvivorsRewardSelectionKind.EliteUpgrade ||
                selectionKind == SurvivorsRewardSelectionKind.BossUpgrade;
        }

        private Color ResolveRewardTitleAccentColor() => DraftHeader.ResolveAccent();

        private string ResolveRewardOverlayTitle() => SurvivorsDraftHeader.Title(DraftSession.Kind);

        private int ResolveTotalRelicCount()
        {
            return _relicDefinitions == null ? 0 : _relicDefinitions.Count;
        }
    }
}
