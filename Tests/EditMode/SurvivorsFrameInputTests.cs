using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsFrameInputTests
    {
        [Test]
        public void TutorialInterceptsGameplayButDebugToggleStillRunsFirst()
        {
            var host = new Host(); host.Session.Start(); host.Menus.TutorialOpen = true;
            host.Down.UnionWith(new[] { KeyCode.F1, KeyCode.B, KeyCode.Escape, KeyCode.Space });
            host.Router.Tick(0.1f);
            Assert.IsTrue(host.Router.DebugVisible);
            Assert.IsFalse(host.Menus.TutorialOpen);
            Assert.IsFalse(host.Menus.BuildOpen);
            Assert.IsEmpty(host.Events);
        }

        [Test]
        public void OpeningBuildConsumesTheFrameButClosingItAllowsDashAndSimulation()
        {
            var host = new Host(); host.Session.Start();
            host.Down.UnionWith(new[] { KeyCode.B, KeyCode.Space });
            host.Router.Tick(0.1f);
            Assert.IsTrue(host.Menus.BuildOpen);
            Assert.IsEmpty(host.Events);
            host.Router.Tick(0.1f);
            Assert.IsFalse(host.Menus.BuildOpen);
            CollectionAssert.AreEqual(new[] { "dash", "simulate" }, host.Events);
        }

        [Test]
        public void DraftTimeoutPrecedesShortcutPriorityEvenWhenItChangesTheRunPhase()
        {
            var host = new Host(); host.Session.Start(); host.Session.OpenRewardSelection();
            host.Down.UnionWith(new[] { KeyCode.Alpha1, KeyCode.R, KeyCode.S });
            host.HeldKeys.Add(KeyCode.LeftShift);
            host.ResumeOnTimeout = true;
            host.Router.Tick(0.1f);
            CollectionAssert.AreEqual(new[] { "presentation", "timeout", "banish:0" }, host.Events);
            Assert.AreEqual(SurvivorsRunState.Playing, host.Session.State);
        }

        [Test]
        public void SuccessfulResultPurchaseConsumesContinueAndRestartButFailureAllowsContinue()
        {
            var host = new Host(); host.Session.Start(); host.Session.Win();
            host.Down.UnionWith(new[] { KeyCode.Alpha2, KeyCode.C, KeyCode.R });
            host.PurchaseSucceeds = true;
            host.Router.Tick(0.1f);
            CollectionAssert.AreEqual(new[] { "presentation", "purchase:1" }, host.Events);
            host.Events.Clear(); host.PurchaseSucceeds = false;
            host.Router.Tick(0.1f);
            CollectionAssert.AreEqual(new[] { "presentation", "purchase:1", "continue" }, host.Events);
        }

        [Test]
        public void PlayingReadsHeldMovementAndOrdersDashBeforeSimulationThenMagnet()
        {
            var host = new Host(); host.Session.Start();
            host.Down.UnionWith(new[] { KeyCode.Space, KeyCode.M });
            host.HeldKeys.UnionWith(new[] { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.LeftArrow });
            host.DefeatOnSimulate = true;
            host.Router.Tick(0.25f);
            CollectionAssert.AreEqual(new[] { "dash", "simulate", "magnet" }, host.Events);
            Assert.AreEqual(new Vector2(0, 1), host.Movement, "A/LeftArrow form one direction; D cancels it.");
            Assert.AreEqual(0.25f, host.Delta);
            Assert.AreEqual(SurvivorsRunState.GameOver, host.Session.State);
        }

        [Test]
        public void ModeSelectionPrefersStandardOverSprintAndTutorialInTheSameFrame()
        {
            var host = new Host(); host.Menus.ModeSelectionOpen = true;
            host.Down.UnionWith(new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.T });
            host.Router.Tick(0.1f);
            CollectionAssert.AreEqual(new[] { "mode:HumanPlaytest" }, host.Events);
            Assert.IsFalse(host.Menus.TutorialOpen);
        }

        private sealed class Host : ISurvivorsKeyboard, ISurvivorsFrameInputPort, ISurvivorsTutorialPort
        {
            public readonly SurvivorsRunSession Session = new SurvivorsRunSession();
            public readonly SurvivorsMenuSession Menus;
            public readonly SurvivorsFrameInput Router;
            public readonly HashSet<KeyCode> Down = new HashSet<KeyCode>(), HeldKeys = new HashSet<KeyCode>();
            public readonly List<string> Events = new List<string>();
            public bool ResumeOnTimeout, PurchaseSucceeds, DefeatOnSimulate;
            public Vector2 Movement;
            public float Delta;
            public Host() { Menus = new SurvivorsMenuSession(this); Router = new SurvivorsFrameInput(Session, Menus, this, this); }
            public bool Pressed(KeyCode key) => Down.Contains(key);
            public bool Held(KeyCode key) => HeldKeys.Contains(key);
            public void TickPresentation(float dt) => Events.Add("presentation");
            public void TickRewardSelectionTimeout(float dt) { Events.Add("timeout"); if (ResumeOnTimeout) Session.ResumePlaying(); }
            public void SelectMode(SurvivorsPacingProfile profile) => Events.Add("mode:" + profile);
            public void ContinueAfterVictory() => Events.Add("continue");
            public void RestartRun() => Events.Add("restart");
            public void TriggerMagnetRecall() => Events.Add("magnet");
            public void BanishDraftChoice(int index) => Events.Add("banish:" + index);
            public void RerollCurrentDraft() => Events.Add("reroll");
            public void SkipCurrentDraft() => Events.Add("skip");
            public void SelectUpgrade(int index) => Events.Add("select:" + index);
            public bool TryPurchaseResultMetaUpgrade(int index) { Events.Add("purchase:" + index); return PurchaseSucceeds; }
            public void Dash(Vector2 movement) { Movement = movement; Events.Add("dash"); }
            public void Simulate(float dt, Vector2 movement) { Delta = dt; Movement = movement; Events.Add("simulate"); if (DefeatOnSimulate) Session.Defeat(); }
            public bool HasProfile => false;
            public bool TutorialSeen => false;
            public void EnsureProfile() { }
            public void ResetTutorialSeen() { }
            public void MarkTutorialSeen() { }
            public void PlaySelect() { }
        }
    }
}
