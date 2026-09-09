using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRunSimulationTests
    {
        [Test]
        public void StoppedTutorialAndPlayingBuildPauseBeforeAllClocks()
        {
            var host = new Host();
            host.Simulation.Simulate(1, Vector2.one);
            host.Session.Start(); host.Menus.TutorialOpen = true;
            host.Simulation.Simulate(1, Vector2.one);
            host.Menus.TutorialOpen = false; host.Menus.BuildOpen = true;
            host.Simulation.Simulate(1, Vector2.one);
            Assert.IsEmpty(host.Events);
            Assert.AreEqual(0, host.Session.ElapsedSeconds);
        }

        [Test]
        public void DraftPresentationAndTimeoutUseClampedDeltaThenStopEvenWhenTimeoutResumes()
        {
            var host = new Host(); host.Session.Start(); host.Session.OpenRewardSelection();
            host.ResumeOnTimeout = true;
            host.Simulation.Simulate(-1, Vector2.one);
            CollectionAssert.AreEqual(new[] { "presentation:0", "timeout:0" }, host.Events);
            Assert.AreEqual(SurvivorsRunState.Playing, host.Session.State);
            Assert.AreEqual(0, host.Session.ElapsedSeconds);
        }

        [Test]
        public void FlowTransitionStopsPlayingPhaseAfterClockAndProgressCommands()
        {
            var host = new Host(); host.Session.Start(); host.WinDuringProgress = true;
            host.Simulation.Simulate(2, Vector2.one);
            CollectionAssert.AreEqual(new[] { "presentation:2", "progress:2", "flow" }, host.Events);
            Assert.AreEqual(2, host.Session.ElapsedSeconds);
            Assert.AreEqual(SurvivorsRunState.Victory, host.Session.State);
        }

        [Test]
        public void PlayingCommandsRetainTheirOrderAndFinishTheFrameAfterAnActorDefeat()
        {
            var host = new Host(); host.Session.Start(); host.DefeatDuringWorld = true;
            host.Simulation.Simulate(0.5f, new Vector2(0.2f, 0.7f));
            CollectionAssert.AreEqual(new[] { "presentation:0.5", "progress:0.5", "flow", "world", "markers" }, host.Events);
            Assert.AreEqual(new Vector2(0.2f, 0.7f), host.Movement);
            host.Events.Clear(); host.Simulation.Simulate(2, Vector2.zero);
            CollectionAssert.AreEqual(new[] { "presentation:2" }, host.Events);
            Assert.AreEqual(0.5f, host.Session.ElapsedSeconds);
        }

        [Test]
        public void ActorSimulationPrunesTheBorrowedListsAndLeashesLiveEnemiesInReverseOrder()
        {
            var objects = new List<GameObject>();
            try
            {
                var a = Enemy("a", objects); var b = Enemy("b", objects);
                var projectileObject = new GameObject("inactive projectile"); objects.Add(projectileObject);
                var pickupObject = new GameObject("inactive pickup"); objects.Add(pickupObject);
                var enemies = new List<SurvivorsEnemyActor> { a, null, b };
                var projectiles = new List<SurvivorsProjectileActor> { null, projectileObject.AddComponent<SurvivorsProjectileActor>() };
                var pickups = new List<SurvivorsPickupActor> { pickupObject.AddComponent<SurvivorsPickupActor>(), null };
                var order = new List<string>(); int pickupReads = 0;
                var simulation = new SurvivorsActorSimulation(enemies, projectiles, pickups, (actor, dt) => order.Add(actor.name),
                    () => { pickupReads++; return default; });
                simulation.TickEnemies(0.1f); simulation.TickProjectiles(0.1f); simulation.TickPickups(0.1f);
                CollectionAssert.AreEqual(new[] { "b", "a" }, order);
                Assert.AreEqual(2, enemies.Count);
                Assert.IsEmpty(projectiles); Assert.IsEmpty(pickups);
                Assert.AreEqual(0, pickupReads);
            }
            finally { foreach (var value in objects) UnityEngine.Object.DestroyImmediate(value); }
        }
        private static SurvivorsEnemyActor Enemy(string name, List<GameObject> objects)
        {
            var value = new GameObject(name); objects.Add(value);
            var actor = value.AddComponent<SurvivorsEnemyActor>();
            actor.Initialize(null, BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm, new SurvivorsTemplateTuning()));
            return actor;
        }
        private sealed class Host : ISurvivorsTutorialPort
        {
            public readonly SurvivorsRunSession Session = new SurvivorsRunSession();
            public readonly SurvivorsMenuSession Menus;
            public readonly SurvivorsRunSimulation Simulation;
            public readonly List<string> Events = new List<string>();
            public bool ResumeOnTimeout, WinDuringProgress, DefeatDuringWorld;
            public Vector2 Movement;
            public Host()
            {
                Menus = new SurvivorsMenuSession(this);
                Simulation = new SurvivorsRunSimulation(Session, Menus, dt => Events.Add("presentation:" + dt),
                    dt => { Events.Add("timeout:" + dt); if (ResumeOnTimeout) Session.ResumePlaying(); },
                    new Action<SurvivorsSimulationFrame>[] {
                        frame => Events.Add("progress:" + Session.ElapsedSeconds),
                        frame => { Events.Add("flow"); if (WinDuringProgress) Session.Win(); } },
                    new Action<SurvivorsSimulationFrame>[] {
                        frame => { Movement = frame.Movement; Events.Add("world"); if (DefeatDuringWorld) Session.Defeat(); },
                        frame => Events.Add("markers") });
            }
            public bool HasProfile => false;
            public bool TutorialSeen => false;
            public void EnsureProfile() { }
            public void ResetTutorialSeen() { }
            public void MarkTutorialSeen() { }
            public void PlaySelect() { }
        }
    }
}
