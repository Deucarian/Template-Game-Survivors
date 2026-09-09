using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsFeedbackPulsesTests
    {
        [Test]
        public void BuildRetainsHierarchyOrderAndEveryEmitterSettingWithoutCreatingAudio()
        {
            var parent = new GameObject("feedback-parent");
            parent.transform.position = new Vector3(2f, 3f, 4f);
            int audioCalls = 0;
            var pulses = new SurvivorsFeedbackPulses(_ => audioCalls++, (_, __, ___) => { audioCalls++; return true; });
            try
            {
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                Assert.That(pulses.Root.name, Is.EqualTo("Survivors Feedback Presentation"));
                Assert.That(pulses.Root.parent, Is.EqualTo(parent.transform));
                Assert.That(pulses.Root.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(pulses.Root.childCount, Is.EqualTo(6));
                Assert.That(pulses.Root.GetComponentsInChildren<AudioSource>(), Is.Empty);
                Assert.That(audioCalls, Is.Zero);
                AssertEmitter(pulses.Spawn, 0, "Survivors Spawn Pulse", 0.2f, 2.2f, 0.5f);
                AssertEmitter(pulses.Fire, 1, "Survivors Weapon Fire Pulse", 0.16f, 2.8f, 0.35f);
                AssertEmitter(pulses.Kill, 2, "Survivors Kill Burst", 0.22f, 3.2f, 0.45f);
                AssertEmitter(pulses.Pickup, 3, "Survivors Pickup Pulse", 0.14f, 2.4f, 0.35f);
                AssertEmitter(pulses.LevelUp, 4, "Survivors Level Up Pulse", 0.28f, 2f, 0.7f);
                AssertEmitter(pulses.Boss, 5, "Survivors Boss Cue Pulse", 0.32f, 3.6f, 0.65f);
                AssertColors(pulses, new Color(0.82f, 0.4f, 1f), new Color(0.2f, 0.82f, 1f),
                    new Color(1f, 0.85f, 0.24f), new Color(1f, 0.3f, 0.82f));
            }
            finally { pulses.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void ApplyingAuthoredThemeRecolorsExistingEmittersWithoutChangingResourcesOrMotion()
        {
            var parent = new GameObject("feedback-parent");
            var pulses = QuietPulses();
            try
            {
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                ParticleSystem oldFire = pulses.Fire;
                Transform oldRoot = pulses.Root;
                oldFire.transform.position = new Vector3(4f, 2f, 7f);
                Assert.That(SurvivorsUiTheme.TryFromJson("{\"worldPresentation\":{\"authored\":true,\"feedbackAccentColor\":\"#FF0000\",\"arenaAccentColor\":\"#00FF00\",\"eliteThreatColor\":\"#0000FF\",\"bossThreatColor\":\"#FFFFFF\"}}", out SurvivorsUiTheme theme, out _), Is.True);
                pulses.ApplyTheme(theme);
                AssertColors(pulses, Color.red, Color.green, Color.blue, Color.white);
                Assert.That(pulses.Root, Is.SameAs(oldRoot));
                Assert.That(pulses.Fire, Is.SameAs(oldFire));
                Assert.That(pulses.Fire.transform.position, Is.EqualTo(new Vector3(4f, 2f, 7f)));
                Assert.That(pulses.Fire.main.startLifetime.constant, Is.EqualTo(0.35f));
                Assert.That(parent.transform.childCount, Is.EqualTo(1));
            }
            finally { pulses.Dispose(); Object.DestroyImmediate(parent); }
        }

        [TestCase(-4)]
        [TestCase(0)]
        [TestCase(7)]
        public void DirectAudioRunsAfterPositionAndMinimumBurstAreApplied(int requested)
        {
            var parent = new GameObject("feedback-parent");
            var clip = AudioClip.Create("borrowed-feedback-clip", 1, 1, 22050, false);
            SurvivorsFeedbackPulses pulses = null;
            int calls = 0;
            pulses = new SurvivorsFeedbackPulses(actual =>
            {
                calls++;
                Assert.That(actual, Is.SameAs(clip));
                Assert.That(pulses.Fire.transform.position, Is.EqualTo(new Vector3(3f, 4.28f, 5f)));
                Assert.That(pulses.Fire.particleCount, Is.EqualTo(Mathf.Max(1, requested)));
            }, (_, __, ___) => throw new AssertionException("Direct playback must not dispatch an event."));
            try
            {
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                pulses.Fire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pulses.Play(pulses.Fire, new Vector3(3f, 4f, 5f), requested, clip, "  ");
                Assert.That(calls, Is.EqualTo(1));
            }
            finally { pulses.Dispose(); Object.DestroyImmediate(parent); Object.DestroyImmediate(clip); }
        }

        [Test]
        public void RejectedEventPreservesArgumentsAndNeverFallsThroughToDirectAudio()
        {
            var parent = new GameObject("feedback-parent");
            var clip = AudioClip.Create("borrowed-feedback-clip", 1, 1, 22050, false);
            SurvivorsFeedbackPulses pulses = null;
            int calls = 0;
            pulses = new SurvivorsFeedbackPulses(_ => Assert.Fail("A rejected or throttled event must not replay its clip."),
                (id, actual, throttle) =>
                {
                    calls++;
                    Assert.That(id, Is.EqualTo("  authored.event  "));
                    Assert.That(actual, Is.SameAs(clip));
                    Assert.That(throttle, Is.EqualTo(0.4f));
                    Assert.That(pulses.Boss.particleCount, Is.EqualTo(3));
                    return false;
                });
            try
            {
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                pulses.Boss.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pulses.Play(pulses.Boss, Vector3.zero, 3, clip, "  authored.event  ", 0.4f);
                Assert.That(calls, Is.EqualTo(1));
            }
            finally { pulses.Dispose(); Object.DestroyImmediate(parent); Object.DestroyImmediate(clip); }
        }

        [Test]
        public void AudioCommandsRemainUsableWithoutBuiltOrSurvivingParticles()
        {
            int direct = 0;
            int events = 0;
            var pulses = new SurvivorsFeedbackPulses(_ => direct++, (_, __, ___) => { events++; return true; });
            var parent = new GameObject("feedback-parent");
            try
            {
                pulses.Play(null, Vector3.zero, 2, null);
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                ParticleSystem destroyed = pulses.Spawn;
                Object.DestroyImmediate(parent);
                pulses.Play(destroyed, Vector3.zero, 2, null, "event");
                pulses.Dispose();
                pulses.Play(pulses.Pickup, Vector3.zero, 2, null);
                Assert.That(direct, Is.EqualTo(2));
                Assert.That(events, Is.EqualTo(1));
                Assert.That(pulses.Root, Is.Null);
            }
            finally { pulses.Dispose(); if (parent != null) Object.DestroyImmediate(parent); }
        }

        [Test]
        public void RebuildAndRepeatedDisposalReleaseOnlyOwnedHierarchy()
        {
            var parent = new GameObject("feedback-parent");
            var sibling = new GameObject("borrowed-sibling");
            sibling.transform.SetParent(parent.transform, false);
            var clip = AudioClip.Create("borrowed-feedback-clip", 1, 1, 22050, false);
            var material = new Material(Shader.Find("Sprites/Default"));
            var pulses = QuietPulses();
            try
            {
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                Transform firstRoot = pulses.Root;
                pulses.Fire.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                Assert.That(firstRoot == null, Is.True);
                Assert.That(parent.transform.childCount, Is.EqualTo(2));
                pulses.Fire.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
                Transform finalRoot = pulses.Root;
                pulses.Dispose();
                pulses.Dispose();
                Assert.That(finalRoot == null, Is.True);
                Assert.That(parent != null && sibling != null && material != null && clip != null, Is.True);
                Assert.That(parent.transform.childCount, Is.EqualTo(1));
                Assert.That(pulses.Root, Is.Null);
                Assert.That(pulses.Spawn, Is.Null);
                Assert.That(pulses.Fire, Is.Null);
                Assert.That(pulses.Kill, Is.Null);
                Assert.That(pulses.Pickup, Is.Null);
                Assert.That(pulses.LevelUp, Is.Null);
                Assert.That(pulses.Boss, Is.Null);
            }
            finally { pulses.Dispose(); Object.DestroyImmediate(parent); Object.DestroyImmediate(material); Object.DestroyImmediate(clip); }
        }

        [Test]
        public void InvalidThemeDoesNotReplaceAnExistingHierarchy()
        {
            var parent = new GameObject("feedback-parent");
            var pulses = QuietPulses();
            try
            {
                pulses.Build(parent.transform, SurvivorsUiTheme.CreateDefault());
                Transform root = pulses.Root;
                Assert.Throws<ArgumentNullException>(() => pulses.Build(parent.transform, null));
                Assert.Throws<ArgumentNullException>(() => pulses.ApplyTheme(null));
                Assert.That(pulses.Root, Is.SameAs(root));
                Assert.That(parent.transform.childCount, Is.EqualTo(1));
            }
            finally { pulses.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void UnbuiltAndDisposedThemeApplicationDoesNotAllocateSceneResources()
        {
            var pulses = QuietPulses();
            pulses.ApplyTheme(SurvivorsUiTheme.CreateDefault());
            pulses.Dispose();
            pulses.ApplyTheme(SurvivorsUiTheme.CreateDefault());
            Assert.That(pulses.Root, Is.Null);
            Assert.That(pulses.Fire, Is.Null);
        }

        private static SurvivorsFeedbackPulses QuietPulses() => new SurvivorsFeedbackPulses(_ => { }, (_, __, ___) => false);

        private static void AssertEmitter(ParticleSystem particles, int index, string name, float size, float speed, float lifetime)
        {
            Assert.That(particles.name, Is.EqualTo(name));
            Assert.That(particles.transform.GetSiblingIndex(), Is.EqualTo(index));
            Assert.That(particles.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(particles.main.loop, Is.False);
            Assert.That(particles.main.playOnAwake, Is.False);
            Assert.That(particles.main.startSize.constant, Is.EqualTo(size));
            Assert.That(particles.main.startSpeed.constant, Is.EqualTo(speed));
            Assert.That(particles.main.startLifetime.constant, Is.EqualTo(lifetime));
            Assert.That(particles.main.maxParticles, Is.EqualTo(120));
            Assert.That(particles.emission.enabled, Is.False);
            Assert.That(particles.shape.shapeType, Is.EqualTo(ParticleSystemShapeType.Sphere));
            Assert.That(particles.shape.radius, Is.EqualTo(0.35f));
        }

        private static void AssertColors(SurvivorsFeedbackPulses pulses, Color accent, Color arena, Color elite, Color boss)
        {
            Assert.That((Color32)pulses.Spawn.main.startColor.color, Is.EqualTo((Color32)Color.Lerp(boss, arena, 0.25f)));
            Assert.That((Color32)pulses.Fire.main.startColor.color, Is.EqualTo((Color32)accent));
            Assert.That((Color32)pulses.Kill.main.startColor.color, Is.EqualTo((Color32)Color.Lerp(arena, accent, 0.25f)));
            Assert.That((Color32)pulses.Pickup.main.startColor.color, Is.EqualTo((Color32)arena));
            Assert.That((Color32)pulses.LevelUp.main.startColor.color, Is.EqualTo((Color32)elite));
            Assert.That((Color32)pulses.Boss.main.startColor.color, Is.EqualTo((Color32)boss));
        }
    }
}
