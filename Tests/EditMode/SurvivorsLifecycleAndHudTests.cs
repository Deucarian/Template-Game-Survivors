using System.Collections.Generic;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsLifecycleAndHudTests
    {
        [Test]
        public void InvalidContentReturnsToModeSelectionWithoutReleasingTheLiveRun()
        {
            var h = new Host(); h.Session.Start(); h.CanStart = false;
            h.Lifecycle.StartRun();
            Assert.IsFalse(h.Lifecycle.SelectMode(SurvivorsPacingProfile.SprintRun));
            Assert.IsTrue(h.Session.Started);
            Assert.AreEqual(SurvivorsRunState.Booting, h.Session.State);
            Assert.IsTrue(h.Menus.ModeSelectionOpen);
            Assert.IsEmpty(h.Events);
        }

        [Test]
        public void NewRunReleasesBeforeInitializationAndOpensTutorialOnlyAfterPlaying()
        {
            var h = new Host(); h.Menus.ModeSelectionOpen = true; h.Menus.BuildOpen = true;
            h.Menus.TutorialOpen = true; h.Menus.TutorialIndex = 4;
            h.Lifecycle.StartRun();
            CollectionAssert.AreEqual(new[] { "theme", "time", "release", "debug-reset", "scroll-reset", "initialize", "profile:Playing" }, h.Events);
            Assert.IsTrue(h.Session.Started);
            Assert.IsFalse(h.Menus.BuildOpen);
            Assert.IsFalse(h.Menus.ModeSelectionOpen);
            Assert.IsFalse(h.Menus.TutorialOpen);
            Assert.AreEqual(0, h.Menus.TutorialIndex);
        }

        [Test]
        public void ModeSelectionRechecksAdmissionAfterApplyingPacingAndRetainsItsAudioResult()
        {
            var h = new Host { RejectAfterPacing = true };
            Assert.IsTrue(h.Lifecycle.SelectMode(SurvivorsPacingProfile.SprintRun));
            CollectionAssert.AreEqual(new[] { "pacing", "mode-audio" }, h.Events);
            Assert.IsTrue(h.Menus.ModeSelectionOpen);
            Assert.IsFalse(h.Session.Started);
        }

        [Test]
        public void OpeningModeSelectionReleasesOnlyStartedRunsAndResetsMenusInOrder()
        {
            var h = new Host(); h.Session.Start(); h.Session.Win();
            h.Lifecycle.OpenModeSelection();
            CollectionAssert.AreEqual(new[] { "release", "draft-clear", "flags:True:False", "debug-reset", "scroll-reset" }, h.Events);
            Assert.IsFalse(h.Session.Started);
            Assert.AreEqual(SurvivorsRunState.Booting, h.Session.State);
            h.Events.Clear(); h.Lifecycle.OpenModeSelection();
            Assert.AreEqual("draft-clear", h.Events[0]);
        }

        [Test]
        public void AutomaticModePromptTakesPriorityAndConfigureDoesNotChangeALiveSession()
        {
            var h = new Host(); h.Lifecycle.StartAutomatically(true, true);
            CollectionAssert.AreEqual(new[] { "disable-auto" }, h.Events);
            Assert.IsTrue(h.Menus.ModeSelectionOpen);
            h.Session.Start(); h.Menus.ModeSelectionOpen = false; h.Events.Clear();
            h.Lifecycle.ConfigureModeSelection(true);
            CollectionAssert.AreEqual(new[] { "flags:True:False" }, h.Events);
            Assert.IsFalse(h.Menus.ModeSelectionOpen);
            Assert.AreEqual(SurvivorsRunState.Playing, h.Session.State);
        }

        [Test]
        public void VictoryIsIdempotentAndContinuationSchedulesAfterThePlayingTransition()
        {
            var h = new Host(); h.Session.Start(); h.Session.Tick(23);
            h.Lifecycle.EnterVictory(); h.Lifecycle.EnterVictory();
            CollectionAssert.AreEqual(new[] { "consume", "rewards:Playing", "draft-clear", "victory:Victory" }, h.Events);
            h.Events.Clear(); h.EndlessEnabled = false;
            Assert.IsFalse(h.Lifecycle.ContinueAfterVictory()); Assert.IsEmpty(h.Events);
            h.EndlessEnabled = true; h.Menus.TutorialOpen = true;
            Assert.IsTrue(h.Lifecycle.ContinueAfterVictory());
            CollectionAssert.AreEqual(new[] { "draft-clear", "schedule:Playing", "continue-audio" }, h.Events);
            Assert.AreEqual(23, h.Session.ElapsedSeconds);
            Assert.IsFalse(h.Menus.TutorialOpen);
        }

        [Test]
        public void StoppedScreensPreserveModeThenTutorialAndSeparateStyleInitialization()
        {
            var h = new Host(); h.Menus.ModeSelectionOpen = true; h.Menus.TutorialOpen = true;
            h.Hud.Draw();
            CollectionAssert.AreEqual(new[] { "styles", "mode", "styles", "tutorial" }, h.Events);
        }

        [Test]
        public void TutorialOverlaysAllRunFeedbackAndSuppressesDraftAndBuildPanels()
        {
            var h = new Host { DebugVisible = true }; h.Session.Start(); h.Session.OpenRewardSelection();
            h.Menus.TutorialOpen = true; h.Menus.BuildOpen = true; h.Hud.Draw();
            CollectionAssert.AreEqual(new[] { "styles", "timer", "player", "debug", "build-hud", "feedback", "tutorial" }, h.Events);
        }

        [TestCase(SurvivorsRunState.Playing, "build")]
        [TestCase(SurvivorsRunState.LevelUp, "draft")]
        [TestCase(SurvivorsRunState.GameOver, "result:False")]
        [TestCase(SurvivorsRunState.Victory, "result:True")]
        public void PhaseSelectsExactlyOneFinalPanel(SurvivorsRunState state, string panel)
        {
            var h = new Host(); h.Session.Start(); h.Menus.BuildOpen = true;
            if (state == SurvivorsRunState.LevelUp) h.Session.OpenRewardSelection();
            if (state == SurvivorsRunState.GameOver) h.Session.Defeat();
            if (state == SurvivorsRunState.Victory) h.Session.Win();
            h.Hud.Draw();
            CollectionAssert.AreEqual(new[] { "styles", "timer", "player", "feedback", panel }, h.Events);
        }

        private sealed class Host : ISurvivorsRunLifecyclePort, ISurvivorsHudRenderPort, ISurvivorsTutorialPort
        {
            public readonly List<string> Events = new List<string>();
            public readonly SurvivorsRunSession Session = new SurvivorsRunSession();
            public readonly SurvivorsMenuSession Menus;
            public readonly SurvivorsRunLifecycle Lifecycle;
            public readonly SurvivorsHudDispatch Hud;
            public bool CanStart { get; set; } = true;
            public bool EndlessEnabled { get; set; } = true;
            public bool DebugVisible { get; set; }
            public bool RejectAfterPacing;
            public Host() { Menus = new SurvivorsMenuSession(this); Lifecycle = new SurvivorsRunLifecycle(Session, Menus, this); Hud = new SurvivorsHudDispatch(Session, Menus, this); }
            public void SetStartupFlags(bool modeSelection, bool autoStart) => Events.Add($"flags:{modeSelection}:{autoStart}");
            public void DisableAutoStart() => Events.Add("disable-auto");
            public void ResetDebugVisibility() => Events.Add("debug-reset");
            public void ResetScreenScrolls() => Events.Add("scroll-reset");
            public void EnsureTheme() => Events.Add("theme");
            public void RestoreTimeScale() => Events.Add("time");
            public void ReleaseRun() { Events.Add("release"); Session.Stop(); }
            public void InitializeRun() { Events.Add("initialize"); Session.Reset(); }
            public void ClearDrafts() => Events.Add("draft-clear");
            public void ApplyPacing(SurvivorsPacingProfile profile) { Events.Add("pacing"); if (RejectAfterPacing) CanStart = false; }
            public void PlayModeSelected() => Events.Add("mode-audio");
            public void ScheduleContinuation() => Events.Add("schedule:" + Session.State);
            public void PlayContinuation() => Events.Add("continue-audio");
            public void ConsumeBossVictory() => Events.Add("consume");
            public void GrantVictoryRewards() => Events.Add("rewards:" + Session.State);
            public void PlayVictory() => Events.Add("victory:" + Session.State);
            public void EnsureStyles() => Events.Add("styles");
            public void DrawModeSelection() => Events.Add("mode");
            public void DrawTutorial() => Events.Add("tutorial");
            public void DrawTimer() => Events.Add("timer");
            public void DrawPlayer() => Events.Add("player");
            public void DrawDebug() => Events.Add("debug");
            public void DrawBuildHud() => Events.Add("build-hud");
            public void DrawRunFeedback() => Events.Add("feedback");
            public void DrawDraft() => Events.Add("draft");
            public void DrawResult(bool victory) => Events.Add("result:" + victory);
            public void DrawBuildMenu() => Events.Add("build");
            public bool HasProfile => true;
            public bool TutorialSeen => true;
            public void EnsureProfile() => Events.Add("profile:" + Session.State);
            public void ResetTutorialSeen() { }
            public void MarkTutorialSeen() { }
            public void PlaySelect() { }
        }
    }
}
