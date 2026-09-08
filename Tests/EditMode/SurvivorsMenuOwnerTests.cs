using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsMenuOwnerTests
    {
        [Test]
        public void BuildToggleResetsItsTabAndRunStateClosesItBeforeOtherInput()
        {
            var menu = new SurvivorsMenuSession(new TutorialPort());
            Assert.IsFalse(menu.HandleBuildInput(SurvivorsRunState.Playing, false, 2));
            Assert.IsTrue(menu.HandleBuildInput(SurvivorsRunState.Playing, true, 2));
            Assert.AreEqual(BuildMenuTab.CurrentBuild, menu.BuildTab);
            Assert.IsTrue(menu.HandleBuildInput(SurvivorsRunState.Playing, false, 2));
            Assert.AreEqual(BuildMenuTab.RunInfo, menu.BuildTab);
            Assert.IsFalse(menu.HandleBuildInput(SurvivorsRunState.LevelUp, true, 1));
            Assert.IsFalse(menu.BuildOpen);
            Assert.AreEqual(BuildMenuTab.RunInfo, menu.BuildTab);
        }

        [Test]
        public void OpeningTutorialClosesBuildBeforePersistenceAndResetsTheStep()
        {
            var port = new TutorialPort { TutorialSeen = true };
            var menu = new SurvivorsMenuSession(port) { BuildOpen = true, TutorialIndex = 4 };
            port.OnReset = () => {
                Assert.IsTrue(menu.TutorialOpen);
                Assert.IsFalse(menu.BuildOpen);
                Assert.AreEqual(0, menu.TutorialIndex);
            };
            Assert.IsTrue(menu.OpenTutorialOverlay(true));
            CollectionAssert.AreEqual(new[] { "ensure", "reset", "select" }, port.Events);
            Assert.IsFalse(port.TutorialSeen);
        }

        [Test]
        public void ClosingAnAlreadyClosedTutorialMayStillPersistSeenWhenRequested()
        {
            var port = new TutorialPort();
            var menu = new SurvivorsMenuSession(port);
            Assert.IsFalse(menu.CloseTutorialOverlay(false));
            Assert.IsEmpty(port.Events);
            Assert.IsTrue(menu.CloseTutorialOverlay(true));
            CollectionAssert.AreEqual(new[] { "ensure", "seen", "select" }, port.Events);
            Assert.IsTrue(port.TutorialSeen);
        }

        [Test]
        public void BackRequiresAnOpenTutorialAndFinishMarksItSeen()
        {
            var port = new TutorialPort();
            var menu = new SurvivorsMenuSession(port) { TutorialIndex = 2 };
            Assert.IsFalse(menu.BackTutorialStep());
            Assert.IsTrue(menu.AdvanceTutorialStep(), "The existing advance command also works while hidden.");
            Assert.AreEqual(3, menu.TutorialIndex);
            menu.OpenTutorialOverlay(false);
            Assert.IsFalse(menu.BackTutorialStep());
            menu.AdvanceTutorialStep();
            Assert.IsTrue(menu.BackTutorialStep());
            menu.TutorialIndex = SurvivorsTutorialContent.StepCount - 1;
            Assert.IsTrue(menu.AdvanceTutorialStep());
            Assert.IsFalse(menu.TutorialOpen);
            Assert.IsTrue(port.TutorialSeen);
        }

        [Test]
        public void FirstRunRequiresAnAvailableUnseenProfile()
        {
            var port = new TutorialPort { HasProfile = false };
            var menu = new SurvivorsMenuSession(port);
            menu.TryOpenFirstRunTutorial();
            Assert.IsFalse(menu.TutorialOpen);
            port.HasProfile = true;
            port.TutorialSeen = true;
            menu.TryOpenFirstRunTutorial();
            Assert.IsFalse(menu.TutorialOpen);
            port.TutorialSeen = false;
            menu.TryOpenFirstRunTutorial();
            Assert.IsTrue(menu.TutorialOpen);
            Assert.IsFalse(port.Events.Contains("reset"));
        }

        [Test]
        public void TutorialCopyAndBuildTabsRetainTheirBoundedIndexFallbacks()
        {
            Assert.AreEqual(7, SurvivorsTutorialContent.StepCount);
            Assert.AreEqual(0, SurvivorsTutorialContent.ClampTutorialStepIndex(-10));
            Assert.AreEqual(6, SurvivorsTutorialContent.ClampTutorialStepIndex(99));
            Assert.AreEqual("Move To Survive", SurvivorsTutorialContent.ResolveDefaultTutorialStepTitle(-1));
            Assert.AreEqual("Pick A Run Mode", SurvivorsTutorialContent.ResolveDefaultTutorialStepTitle(99));
            for (int i = 0; i < SurvivorsTutorialContent.StepCount; i++) Assert.IsNotEmpty(SurvivorsTutorialContent.ResolveDefaultTutorialStepLines(i));
            Assert.AreEqual(BuildMenuTab.CurrentBuild, SurvivorsMenuSession.ClampBuildMenuTab(-1));
            Assert.AreEqual(BuildMenuTab.Controls, SurvivorsMenuSession.ClampBuildMenuTab(99));
            Assert.AreEqual("Run Info", SurvivorsMenuSession.FormatBuildMenuTabLabel(BuildMenuTab.RunInfo));
        }

        [TestCase(1920, 1080, 530, 230, 860, 620)]
        [TestCase(400, 500, 24, 24, 352, 452)]
        [TestCase(100, 100, 24, 24, 220, 220)]
        public void PanelGeometryPreservesWideNarrowAndVerySmallViewportRules(float width, float height, float x, float y, float panelWidth, float panelHeight)
        {
            Rect panel = SurvivorsScreenLayout.ResolveCenteredPanel(width, height, 860, 620, 360, 360, 24);
            Assert.AreEqual(new Rect(x, y, panelWidth, panelHeight), panel);
        }

        private sealed class TutorialPort : ISurvivorsTutorialPort
        {
            public bool HasProfile { get; set; } = true;
            public bool TutorialSeen { get; set; }
            public readonly List<string> Events = new List<string>();
            public Action OnReset;
            public void EnsureProfile() => Events.Add("ensure");
            public void ResetTutorialSeen() { Events.Add("reset"); TutorialSeen = false; OnReset?.Invoke(); }
            public void MarkTutorialSeen() { Events.Add("seen"); TutorialSeen = true; }
            public void PlaySelect() => Events.Add("select");
        }
    }
}
