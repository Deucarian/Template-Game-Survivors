using System;
using Deucarian.Persistence;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRunCompositionTests
    {
        [Test]
        public void RunClockPausesForDraftAndResultsAndContinuesWithoutResetting()
        {
            var run = new SurvivorsRunSession();
            run.Tick(10f);
            Assert.AreEqual(0f, run.ElapsedSeconds);
            run.Start();
            run.Tick(2f);
            run.OpenRewardSelection();
            run.Tick(10f);
            Assert.AreEqual(2f, run.ElapsedSeconds);
            run.ResumePlaying();
            run.Tick(-3f);
            run.Tick(1f);
            run.Win();
            Assert.IsFalse(run.ContinueAfterVictory(false));
            run.Tick(10f);
            Assert.AreEqual(3f, run.ElapsedSeconds);
            Assert.IsTrue(run.ContinueAfterVictory(true));
            Assert.IsTrue(run.HasClearedVictory);
            run.Tick(4f);
            Assert.AreEqual(7f, run.ElapsedSeconds);
            run.Defeat();
            Assert.IsFalse(run.ContinueAfterVictory(true));
            run.Reset();
            Assert.AreEqual(SurvivorsRunState.Booting, run.State);
            Assert.IsFalse(run.Started);
            Assert.IsFalse(run.HasClearedVictory);
            Assert.AreEqual(0f, run.ElapsedSeconds);
        }

        [Test]
        public void DraftQueueCapsStoredExperienceAndCooldownDoesNotAccumulateHiddenLevels()
        {
            var xp = new SurvivorsExperienceProgression();
            var tuning = CreateTuning();
            Assert.AreEqual(50, xp.Gain(50, 0f, tuning));
            Assert.AreEqual(2, xp.Level);
            Assert.AreEqual(1, xp.PendingLevelUps);
            Assert.AreEqual(14, xp.Experience);
            Assert.AreEqual(26, xp.ThrottledExperienceOverflow);
            xp.BeginDraft(2f);
            xp.ConsumeLevelUp();
            xp.Gain(20, 0f, tuning);
            Assert.AreEqual(2, xp.Level);
            Assert.AreEqual(46, xp.ThrottledExperienceOverflow);
            xp.Tick(1f, tuning);
            Assert.AreEqual(1f, xp.DraftCooldownRemaining);
            xp.Tick(1f, tuning);
            xp.Gain(1, 0f, tuning);
            Assert.AreEqual(3, xp.Level);
            Assert.AreEqual(1, xp.PendingLevelUps);
            Assert.AreEqual(71, xp.ExperienceCollected);
        }

        [Test]
        public void UnthrottledExperienceKeepsOverflowAndRestartClearsAllProgression()
        {
            var xp = new SurvivorsExperienceProgression();
            var tuning = CreateTuning();
            tuning.LevelUpDraftCooldownSeconds = 0f;
            tuning.MaximumQueuedLevelUps = 2;
            xp.Gain(100, 0f, tuning);
            Assert.AreEqual(3, xp.Level);
            Assert.AreEqual(75, xp.Experience);
            xp.ConsumeLevelUp();
            xp.ResolveBudget(tuning);
            Assert.AreEqual(4, xp.Level);
            Assert.AreEqual(55, xp.Experience);
            xp.Reset();
            Assert.AreEqual(1, xp.Level);
            Assert.AreEqual(0, xp.Experience);
            Assert.AreEqual(0, xp.PendingLevelUps);
            Assert.AreEqual(0, xp.ExperienceCollected);
        }

        [Test]
        public void AudioEventsNormalizeIdsAndHaveIndependentThrottleAndReset()
        {
            var events = new SurvivorsAudioEventRouter();
            Assert.IsTrue(events.TryDispatch(" hit ", 10f, 2f));
            Assert.IsFalse(events.TryDispatch("hit", 11f, 2f));
            Assert.IsTrue(events.TryDispatch("pickup", 11f, 2f));
            Assert.IsTrue(events.TryDispatch("hit", 12f, 2f));
            Assert.AreEqual(3, events.DispatchCount);
            events.SetMuted(true);
            Assert.IsFalse(events.TryDispatch("hit", 20f, 2f));
            events.Reset();
            Assert.IsTrue(events.Muted);
            events.SetMuted(false);
            Assert.IsFalse(events.TryDispatch("  ", 0f, 0f));
            Assert.IsTrue(events.TryDispatch("hit", 0f, 2f));
            Assert.AreEqual("hit", events.LastEventId);
            Assert.AreEqual(1, events.DispatchCount);
        }

        [Test]
        public void BorrowedPersistenceSurvivesReleaseAndReloadRetainsProfile()
        {
            using (var persistence = new PersistenceService(new InMemoryTextStorage()))
            using (var session = new SurvivorsProfileSession(() => throw new InvalidOperationException("Must not create storage")))
            {
                session.ConfigureBorrowedPersistence(persistence, new SaveSlotId("composition-borrowed"));
                var first = session.EnsureLoaded(null);
                Assert.IsTrue(first.GrantBloodShardsForDebug(20).Succeeded);
                Assert.AreSame(first, session.EnsureLoaded(null));
                session.Release();
                session.Release();
                var second = session.EnsureLoaded(null);
                Assert.AreNotSame(first, second);
                Assert.AreEqual(20, second.UnspentBloodShards);
                Assert.DoesNotThrow(() => first.Load());
            }
        }

        [Test]
        public void OwnedProfileIsDisposedOnceAndRebindingDoesNotDisposeBorrowedStorage()
        {
            int creations = 0;
            using (var borrowed = new PersistenceService(new InMemoryTextStorage()))
            using (var session = new SurvivorsProfileSession(() =>
            {
                creations++;
                return new PersistenceService(new InMemoryTextStorage());
            }))
            {
                var owned = session.EnsureLoaded(null);
                session.ConfigureBorrowedPersistence(borrowed, new SaveSlotId("composition-rebound"));
                Assert.Throws<ObjectDisposedException>(() => owned.Load());
                var rebound = session.EnsureLoaded(null);
                session.Dispose();
                session.Dispose();
                Assert.DoesNotThrow(() => rebound.Load());
                Assert.AreEqual(1, creations);
            }
        }

        private static SurvivorsTemplateTuning CreateTuning()
        {
            return new SurvivorsTemplateTuning
            {
                ExperienceRequiredBase = 10,
                ExperienceRequiredPerLevel = 5,
                MaximumQueuedLevelUps = 1,
                LevelUpDraftCooldownSeconds = 2f
            };
        }
    }
}
