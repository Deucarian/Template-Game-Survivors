using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsEvolutionAnnouncementsTests
    {
        [Test]
        public void MissingCatalogDoesNotCreateBuildStateOrPublishFeedback()
        {
            var port = new BuildPort();
            var build = new SurvivorsRunBuildState(port);
            var owner = Owner(build, port, (_, __) => Assert.Fail("No goal without a catalog."),
                _ => Assert.Fail("No readiness without a catalog."));
            owner.Refresh();
            owner.Reset();
            owner.Refresh();
            Assert.That(build.Catalog, Is.Null);
            Assert.That(build.State, Is.Null);
        }

        [Test]
        public void GoalAndReadyAreEachAnnouncedOnceWithoutSelectingOrAcquiringAnything()
        {
            var port = new BuildPort();
            SurvivorsRunBuildState build = Build(port, true, "evolution");
            var calls = new List<string>();
            var owner = Owner(build, port, (evolution, passive) => calls.Add("goal:" + evolution.Id + ":" + passive.Id),
                evolution => calls.Add("ready:" + evolution.Id));
            owner.Refresh();
            Assert.That(calls, Is.Empty, "A missing required weapon rank is not a passive goal yet.");
            Select(build, "rank");
            owner.Refresh();
            owner.Refresh();
            Assert.That(calls, Is.EqualTo(new[] { "goal:evolution:passive" }));
            Assert.That(build.State.GetRank(new RunUpgradeId("passive")), Is.Zero);
            Select(build, "passive");
            owner.Refresh();
            owner.Refresh();
            Assert.That(calls, Is.EqualTo(new[] { "goal:evolution:passive", "ready:evolution" }));
            Assert.That(build.State.GetRank(new RunUpgradeId("evolution")), Is.Zero);
            Assert.That(build.EvolutionIds, Is.Empty);
            Assert.That(port.SelectionCallbacks, Is.Zero);
        }

        [Test]
        public void ReadyOrderFollowsCatalogAndDistinctIdsHaveIndependentHistory()
        {
            var port = new BuildPort();
            SurvivorsRunBuildState build = Build(port, false, "z.evolution", "a.evolution-2", "a.evolution");
            var calls = new List<string>();
            var owner = Owner(build, port, (_, __) => Assert.Fail("These evolutions are ready."), definition => calls.Add(definition.Id.Value));
            owner.Refresh();
            owner.Refresh();
            Assert.That(calls, Is.EqualTo(new[] { "a.evolution", "a.evolution-2", "z.evolution" }));
        }

        [Test]
        public void AlreadyAcquiredEvolutionIsNotAnnouncedEvenWhenItsRankStillLooksEligible()
        {
            var port = new BuildPort();
            SurvivorsRunBuildState build = Build(port, false, "owned", "unowned");
            Assert.That(build.TryGetRunUpgrade("owned", out RunUpgradeDefinition owned), Is.True);
            build.RecordRunBuildSelection(owned);
            Assert.That(build.IsUpgradeEligibleForCurrentBuild(owned), Is.True);
            var calls = new List<string>();
            var owner = Owner(build, port, (_, __) => Assert.Fail("No goals expected."), definition => calls.Add(definition.Id.Value));
            owner.Refresh();
            Assert.That(calls, Is.EqualTo(new[] { "unowned" }));
        }

        [Test]
        public void ResetClearsBothHistoriesAndDoesNotResetTheBorrowedBuild()
        {
            var port = new BuildPort();
            SurvivorsRunBuildState build = Build(port, true, "evolution");
            Select(build, "rank");
            int goals = 0;
            int ready = 0;
            var owner = Owner(build, port, (_, __) => goals++, _ => ready++);
            owner.Refresh();
            owner.Reset();
            owner.Refresh();
            Assert.That(goals, Is.EqualTo(2));
            Select(build, "passive");
            owner.Refresh();
            owner.Reset();
            owner.Refresh();
            Assert.That(ready, Is.EqualTo(2));
            Assert.That(build.State.GetRank(new RunUpgradeId("rank")), Is.EqualTo(1));
            Assert.That(build.State.GetRank(new RunUpgradeId("passive")), Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void HistoryIsRecordedBeforeThrowingCallbacksSoRetryDoesNotRepeatThem(bool ready)
        {
            var port = new BuildPort();
            SurvivorsRunBuildState build = Build(port, !ready, "evolution");
            if (!ready) Select(build, "rank");
            int calls = 0;
            var owner = Owner(build, port,
                (_, __) => { calls++; throw new InvalidOperationException("goal failure"); },
                _ => { calls++; throw new InvalidOperationException("ready failure"); });
            Assert.Throws<InvalidOperationException>(owner.Refresh);
            Assert.DoesNotThrow(owner.Refresh);
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void ReentrantReadyCallbackSeesItsIdAlreadyRecorded()
        {
            var port = new BuildPort();
            SurvivorsRunBuildState build = Build(port, false, "evolution");
            int calls = 0;
            SurvivorsEvolutionAnnouncements owner = null;
            owner = Owner(build, port, (_, __) => Assert.Fail("No goals expected."), _ =>
            {
                calls++;
                Assert.That(calls, Is.EqualTo(1));
                owner.Refresh();
            });
            owner.Refresh();
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void EachLaterEntryObservesBuildChangesMadeByAnEarlierGoalCallback()
        {
            var port = new BuildPort();
            SurvivorsRunBuildState build = Build(port, true, "a.evolution", "b.evolution");
            Select(build, "rank");
            var calls = new List<string>();
            var owner = Owner(build, port, (evolution, passive) =>
            {
                calls.Add("goal:" + evolution.Id);
                Select(build, passive.Id.Value);
            }, evolution => calls.Add("ready:" + evolution.Id));
            owner.Refresh();
            Assert.That(calls, Is.EqualTo(new[] { "goal:a.evolution", "ready:b.evolution" }));
            owner.Refresh();
            Assert.That(calls, Is.EqualTo(new[] { "goal:a.evolution", "ready:b.evolution", "ready:a.evolution" }));
        }

        private static SurvivorsEvolutionAnnouncements Owner(SurvivorsRunBuildState build, BuildPort port,
            Action<RunUpgradeDefinition, RunUpgradeDefinition> goal, Action<RunUpgradeDefinition> ready)
            => new SurvivorsEvolutionAnnouncements(build, new SurvivorsDraftCatalogPolicy(build,
                new SurvivorsDraftRarityPolicy(() => port.Tuning, () => 0f), () => port.Tuning), goal, ready);

        private static SurvivorsRunBuildState Build(BuildPort port, bool requirePassive, params string[] evolutions)
        {
            var definitions = new List<RunUpgradeDefinition> { Upgrade("rank"), Upgrade("passive"), Upgrade("ordinary") };
            var metadata = new List<SurvivorsRunUpgradeMetadata>
            {
                new SurvivorsRunUpgradeMetadata("passive", "Authored Passive", SurvivorsRunUpgradeCategory.Passive,
                    SurvivorsRunBuildSlotKind.Passive, "weapon", "passive")
            };
            foreach (string id in evolutions)
            {
                definitions.Add(Upgrade(id));
                metadata.Add(new SurvivorsRunUpgradeMetadata(id, id, SurvivorsRunUpgradeCategory.Evolution,
                    SurvivorsRunBuildSlotKind.None, "weapon", "evolution",
                    requiredUpgradeId: requirePassive ? "rank" : null, requiredUpgradeRank: 1,
                    requiredPassiveUpgradeId: requirePassive ? "passive" : null));
            }
            var build = new SurvivorsRunBuildState(port);
            build.Initialize(new RunUpgradeCatalog(definitions), metadata, null, null);
            return build;
        }

        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id),
            RunUpgradeRarity.Common, 1, 3, new[]
            {
                new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1)
            });
        private static void Select(SurvivorsRunBuildState build, string id)
            => Assert.That(build.State.Select(build.Catalog, new RunUpgradeId(id)).Succeeded, Is.True);
        private sealed class BuildPort : ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning();
            internal int SelectionCallbacks;
            public int WeaponCount => 0;
            public bool HasWeapon(string id) => true;
            public void AddWeapon(string id) => SelectionCallbacks++;
            public void PassiveAdded(RunUpgradeDefinition upgrade) => SelectionCallbacks++;
            public void RecordEvolutionTime() => SelectionCallbacks++;
            public void EvolutionAdded(RunUpgradeDefinition upgrade) => SelectionCallbacks++;
        }
    }
}
