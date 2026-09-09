using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsDraftSessionTests
    {
        [Test]
        public void PendingDraftConsumesOneLevelOnlyAfterAValidSelectionAndPreservesEffectOrder()
        {
            var f = new Fixture();
            f.Experience.QueueDebugLevelUp();
            Assert.IsTrue(f.Session.TryOpenPending());
            Assert.AreEqual(1, f.Experience.PendingLevelUps);
            Assert.AreEqual(1, f.Session.LevelUpOpenCount);
            Assert.AreEqual(1, f.Session.OpenCount);
            Assert.AreEqual(5f, f.Session.RemainingSeconds);
            Assert.IsFalse(f.Session.Select(-1));
            RunUpgradeId id = f.Session.CurrentDraft.Choices[0].Id;
            f.Port.Events.Clear();
            Assert.IsTrue(f.Session.Select(0));
            Assert.AreEqual(1, f.Build.State.GetRank(id));
            CollectionAssert.AreEqual(new[] { "apply", "selected" }, f.Port.Events);
            Assert.AreEqual(1, f.Session.SelectedUpgradeCount);
            Assert.AreEqual(0, f.Experience.PendingLevelUps);
            Assert.AreEqual(SurvivorsRunState.Playing, f.Run.State);
            Assert.IsNull(f.Session.CurrentDraft);
        }

        [Test]
        public void PendingDraftHonorsRunStateAndCooldownAndClearPreservesDiagnostics()
        {
            var f = new Fixture();
            f.Experience.QueueDebugLevelUp();
            f.Run.Defeat();
            Assert.IsFalse(f.Session.TryOpenPending());
            f.Run.ResumePlaying();
            f.Experience.BeginDraft(2f);
            Assert.IsFalse(f.Session.TryOpenPending());
            f.Experience.Tick(2f, f.Port.Tuning);
            Assert.IsTrue(f.Session.TryOpenPending());
            f.Session.Clear();
            Assert.AreEqual(1, f.Session.OpenCount);
            Assert.AreEqual(SurvivorsRewardSelectionKind.None, f.Session.Kind);
            f.Session.Reset();
            Assert.AreEqual(0, f.Session.OpenCount);
            Assert.AreEqual(0, f.Session.LevelUpOpenCount);
        }

        [Test]
        public void FailedRerollKeepsOfferAndChargeWhileExhaustingBanishCompletesWithoutSkipReward()
        {
            var f = new Fixture();
            f.Experience.QueueDebugLevelUp();
            f.Session.OpenLevelUp();
            RunUpgradeDraft before = f.Session.CurrentDraft;
            f.Port.Tuning.NormalEarlyCommonWeight = 0;
            Assert.IsFalse(f.Session.Reroll());
            Assert.AreSame(before, f.Session.CurrentDraft);
            Assert.AreEqual(0, f.Session.RerollCount);
            Assert.IsTrue(f.Session.Banish(0));
            Assert.AreEqual(1, f.Session.BanishCount);
            Assert.AreEqual(0, f.Session.SkipCount);
            Assert.AreEqual(SurvivorsRunState.Playing, f.Run.State);
            Assert.AreEqual("banish", f.Port.Events[f.Port.Events.Count - 1]);
            Assert.IsFalse(f.Port.Events.Contains("skip"));
        }

        [Test]
        public void SuccessfulRerollRestartsTimeoutAndChargesOnlyAfterOfferGeneration()
        {
            var f = new Fixture();
            f.Session.OpenLevelUp();
            f.Session.TickTimeout(2f);
            Assert.AreEqual(3f, f.Session.RemainingSeconds);
            f.Port.Events.Clear();
            Assert.IsTrue(f.Session.Reroll());
            Assert.AreEqual(1, f.Session.RerollCount);
            Assert.AreEqual(0, f.Session.RerollsRemaining);
            Assert.AreEqual(5f, f.Session.RemainingSeconds);
            CollectionAssert.AreEqual(new[] { "scroll", "present", "reroll" }, f.Port.Events);
            Assert.IsFalse(f.Session.Reroll());
        }

        [Test]
        public void TimeoutCountsItsAttemptEvenWhenTheFirstChoiceBecameUnavailable()
        {
            var f = new Fixture();
            f.Session.OpenLevelUp();
            f.Build.State.Banish(f.Session.CurrentDraft.Choices[0].Id);
            f.Session.TickTimeout(-3f);
            Assert.AreEqual(5f, f.Session.RemainingSeconds);
            f.Run.ResumePlaying();
            f.Session.TickTimeout(99f);
            Assert.AreEqual(5f, f.Session.RemainingSeconds);
            f.Run.OpenRewardSelection();
            f.Session.TickTimeout(5f);
            Assert.AreEqual(1, f.Session.AutoSelectCount);
            Assert.AreEqual(0, f.Session.SelectedUpgradeCount);
            Assert.AreEqual(0f, f.Session.RemainingSeconds);
            Assert.AreEqual(SurvivorsRunState.LevelUp, f.Run.State);
            f.Session.TickTimeout(99f);
            Assert.AreEqual(1, f.Session.AutoSelectCount);
        }

        [Test]
        public void FirstBossSkipCompletesVictoryAndEndlessBossSelectionResumesTheRun()
        {
            var f = new Fixture();
            Assert.IsTrue(f.Session.OpenReward(SurvivorsEnemyRole.Boss, false));
            Assert.IsFalse(f.Session.CanBanish);
            Assert.IsTrue(f.Session.Skip());
            Assert.AreEqual(1, f.Port.Victories);
            Assert.AreEqual(SurvivorsRunState.Victory, f.Run.State);
            Assert.AreEqual(1, f.Session.SkipCount);
            Assert.AreEqual(0, f.Session.SelectedRewardUpgradeCount);
            Assert.IsTrue(f.Run.ContinueAfterVictory(true));
            Assert.IsTrue(f.Session.OpenReward(SurvivorsEnemyRole.Boss, false));
            Assert.IsTrue(f.Session.Select(0));
            Assert.AreEqual(1, f.Port.Victories);
            Assert.AreEqual(1, f.Session.SelectedRewardUpgradeCount);
            Assert.AreEqual(SurvivorsRunState.Playing, f.Run.State);
        }

        [Test]
        public void MinibossChainsRelicBeforeTheQueuedLevelUp()
        {
            var f = new Fixture(true);
            f.Experience.QueueDebugLevelUp();
            Assert.IsTrue(f.Session.OpenReward(SurvivorsEnemyRole.Miniboss, false));
            Assert.IsTrue(f.Session.Select(0));
            Assert.AreEqual(SurvivorsRewardSelectionKind.BossRelic, f.Session.Kind);
            Assert.AreEqual(1, f.Experience.PendingLevelUps);
            Assert.IsTrue(f.Session.SelectRelic(0));
            Assert.AreEqual(1, f.Relics.Count);
            Assert.AreEqual(SurvivorsRewardSelectionKind.LevelUp, f.Session.Kind);
            Assert.AreEqual(3, f.Session.OpenCount);
        }

        [Test]
        public void EmptyRelicFallbackResumesBeforeTheNextPendingDraftAttempt()
        {
            var f = new Fixture();
            f.Experience.QueueDebugLevelUp();
            Assert.IsTrue(f.Session.OpenReward(SurvivorsEnemyRole.Miniboss, false));
            Assert.IsTrue(f.Session.Select(0));
            Assert.AreEqual(SurvivorsRunState.Playing, f.Run.State);
            Assert.AreEqual(SurvivorsRewardSelectionKind.None, f.Session.Kind);
            Assert.AreEqual(1, f.Experience.PendingLevelUps);
            Assert.IsTrue(f.Session.TryOpenPending());
        }

        [Test]
        public void EmptyNormalPoolConsumesTheQueuedLevelAndGrantsOneSkipReward()
        {
            var f = new Fixture();
            f.Experience.QueueDebugLevelUp();
            f.Port.Tuning.NormalEarlyCommonWeight = 0;
            f.Session.OpenLevelUp();
            Assert.AreEqual(1, f.Session.SkipCount);
            Assert.AreEqual(0, f.Experience.PendingLevelUps);
            Assert.AreEqual(0, f.Session.OpenCount);
            Assert.AreEqual(SurvivorsRunState.Playing, f.Run.State);
            CollectionAssert.AreEqual(new[] { "skip" }, f.Port.Events);
        }

        [Test]
        public void RelicInventoryReservesIdentityBeforeEffectsAndPublishesAcquiredOrderAfterEffects()
        {
            IReadOnlyList<SurvivorsRelicDefinition> definitions = BasicSurvivorsGame.CreateRelicDefinitions();
            SurvivorsRelicInventory inventory = null;
            int calls = 0;
            inventory = new SurvivorsRelicInventory(() => definitions, relic => {
                calls++;
                Assert.AreEqual(0, inventory.Count);
                Assert.IsFalse(inventory.Select(relic));
            }, relic => {
                Assert.AreEqual(1, inventory.Count);
                Assert.AreSame(relic, inventory.Selected[0]);
            });
            Assert.AreSame(definitions, inventory.Available());
            Assert.IsTrue(inventory.Select(definitions[0]));
            Assert.IsFalse(inventory.Select(definitions[0]));
            Assert.AreEqual(1, calls);
            foreach (var relic in inventory.Available()) Assert.AreNotEqual(definitions[0].Id, relic.Id);
            inventory.Reset();
            Assert.AreEqual(0, inventory.Count);
            Assert.IsEmpty(inventory.Selected);
            Assert.AreSame(definitions, inventory.Available());
        }

        private sealed class Fixture
        {
            public readonly SurvivorsRunSession Run = new SurvivorsRunSession();
            public readonly SurvivorsExperienceProgression Experience = new SurvivorsExperienceProgression();
            public readonly Port Port = new Port();
            public readonly SurvivorsRunBuildState Build;
            public readonly SurvivorsRelicInventory Relics;
            public readonly SurvivorsDraftSession Session;
            public Fixture(bool withRelics = false)
            {
                Run.Start();
                Build = new SurvivorsRunBuildState(Port);
                var upgrade = new RunUpgradeDefinition(new RunUpgradeId("test"), RunUpgradeRarity.Common, 100, 8,
                    new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1d) });
                Build.Initialize(new RunUpgradeCatalog(new[] { upgrade }), Array.Empty<SurvivorsRunUpgradeMetadata>(), null, null);
                var rarity = new SurvivorsDraftRarityPolicy(() => Port.Tuning, () => 0f);
                var offers = new SurvivorsDraftOfferGenerator(Build, rarity, () => Port.Tuning, () => new SurvivorsDraftProgress(1, 1, 0, 0, 0));
                Relics = new SurvivorsRelicInventory(() => withRelics ? BasicSurvivorsGame.CreateRelicDefinitions() : Array.Empty<SurvivorsRelicDefinition>(), _ => { }, _ => { });
                Session = new SurvivorsDraftSession(Build, offers, Relics, Run, Experience, Port);
                Port.OnVictory = Run.Win;
                Port.OnSelected = () => Assert.AreEqual(1, Session.SelectedUpgradeCount);
            }
        }
        private sealed class Port : ISurvivorsDraftSessionPort, ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning {
                DraftChoiceCount = 1, DraftBanishCharges = 1, RewardSelectionTimeoutSeconds = 5f,
                NormalEarlyCommonWeight = 100, BossCommonWeight = 100, EliteCommonWeight = 100, LevelUpDraftCooldownSeconds = 0f };
            public readonly List<string> Events = new List<string>();
            public Action OnVictory, OnSelected;
            public int Victories;
            public int RerollCharges => 1;
            public int RelicSeed => 99;
            public int WeaponCount => 0;
            public bool HasWeapon(string id) => false;
            public void AddWeapon(string id) { }
            public void PassiveAdded(RunUpgradeDefinition upgrade) { }
            public void RecordEvolutionTime() { }
            public void EvolutionAdded(RunUpgradeDefinition upgrade) { }
            public void ResetScroll() => Events.Add("scroll");
            public void ApplyUpgrade(RunUpgradeDefinition upgrade) => Events.Add("apply");
            public void RecordDirectUpgrade(RunUpgradeDefinition upgrade) => Events.Add("direct");
            public void PresentSelectedUpgrade(SurvivorsRewardSelectionKind kind, RunUpgradeDefinition upgrade) { Events.Add("selected"); OnSelected?.Invoke(); }
            public void PresentDraft(SurvivorsRewardSelectionKind kind, RunUpgradeDraft draft, SurvivorsRelicDraft relicDraft, bool opening) => Events.Add("present");
            public void PlayDraftOpened(SurvivorsRewardSelectionKind kind, SurvivorsEnemyRole role) => Events.Add("opened");
            public void PlayReroll() => Events.Add("reroll");
            public void PlayBanish() => Events.Add("banish");
            public void GrantSkipReward(SurvivorsRewardSelectionKind kind) => Events.Add("skip");
            public void EnterVictory() { Victories++; OnVictory?.Invoke(); }
        }
    }
}
