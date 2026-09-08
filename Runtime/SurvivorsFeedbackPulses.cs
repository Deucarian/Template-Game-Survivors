using System;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns the run's particle hierarchy; audio commands and materials remain borrowed.</summary>
    internal sealed class SurvivorsFeedbackPulses : IDisposable
    {
        private readonly Action<AudioClip> _playClip;
        private readonly Func<string, AudioClip, float, bool> _playEvent;

        internal SurvivorsFeedbackPulses(Action<AudioClip> playClip, Func<string, AudioClip, float, bool> playEvent)
        {
            _playClip = playClip ?? throw new ArgumentNullException(nameof(playClip));
            _playEvent = playEvent ?? throw new ArgumentNullException(nameof(playEvent));
        }

        internal Transform Root { get; private set; }
        internal ParticleSystem Spawn { get; private set; }
        internal ParticleSystem Fire { get; private set; }
        internal ParticleSystem Kill { get; private set; }
        internal ParticleSystem Pickup { get; private set; }
        internal ParticleSystem LevelUp { get; private set; }
        internal ParticleSystem Boss { get; private set; }

        internal void Build(Transform parent, SurvivorsUiTheme theme)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            Dispose();
            try
            {
                Root = new GameObject("Survivors Feedback Presentation").transform;
                Root.SetParent(parent, false);
                Color accent = theme.GetFeedbackAccentColor(new Color(0.82f, 0.4f, 1f));
                Color arenaAccent = theme.GetArenaAccentColor(new Color(0.2f, 0.82f, 1f));
                Color elite = theme.GetEliteThreatColor(new Color(1f, 0.85f, 0.24f));
                Color boss = theme.GetBossThreatColor(new Color(1f, 0.3f, 0.82f));
                Spawn = CreatePulse("Survivors Spawn Pulse", Color.Lerp(boss, arenaAccent, 0.25f), 0.2f, 2.2f, 0.5f);
                Fire = CreatePulse("Survivors Weapon Fire Pulse", accent, 0.16f, 2.8f, 0.35f);
                Kill = CreatePulse("Survivors Kill Burst", Color.Lerp(arenaAccent, accent, 0.25f), 0.22f, 3.2f, 0.45f);
                Pickup = CreatePulse("Survivors Pickup Pulse", arenaAccent, 0.14f, 2.4f, 0.35f);
                LevelUp = CreatePulse("Survivors Level Up Pulse", elite, 0.28f, 2.0f, 0.7f);
                Boss = CreatePulse("Survivors Boss Cue Pulse", boss, 0.32f, 3.6f, 0.65f);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal void ApplyTheme(SurvivorsUiTheme theme)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            Color accent = theme.GetFeedbackAccentColor(new Color(0.82f, 0.4f, 1f));
            Color arenaAccent = theme.GetArenaAccentColor(new Color(0.2f, 0.82f, 1f));
            Color elite = theme.GetEliteThreatColor(new Color(1f, 0.85f, 0.24f));
            Color boss = theme.GetBossThreatColor(new Color(1f, 0.3f, 0.82f));
            SetColor(Spawn, Color.Lerp(boss, arenaAccent, 0.25f));
            SetColor(Fire, accent);
            SetColor(Kill, Color.Lerp(arenaAccent, accent, 0.25f));
            SetColor(Pickup, arenaAccent);
            SetColor(LevelUp, elite);
            SetColor(Boss, boss);
        }

        internal void Play(ParticleSystem particles, Vector3 position, int count, AudioClip clip,
            string audioEventId = null, float audioThrottleSeconds = 0f)
        {
            if (particles != null)
            {
                particles.transform.position = position + Vector3.up * 0.28f;
                particles.Emit(Mathf.Max(1, count));
            }

            if (!string.IsNullOrWhiteSpace(audioEventId))
            {
                _playEvent(audioEventId, clip, audioThrottleSeconds);
                return;
            }

            _playClip(clip);
        }

        public void Dispose()
        {
            if (Root != null) UnityObjectUtility.DestroySafely(Root.gameObject);
            Root = null;
            Spawn = Fire = Kill = Pickup = LevelUp = Boss = null;
        }

        private ParticleSystem CreatePulse(string name, Color color, float startSize, float startSpeed, float lifetime)
        {
            GameObject instance = new GameObject(name);
            instance.transform.SetParent(Root, false);
            ParticleSystem particles = instance.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = color;
            main.maxParticles = 120;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;
            return particles;
        }

        private static void SetColor(ParticleSystem particles, Color color)
        {
            if (particles == null) return;
            ParticleSystem.MainModule main = particles.main;
            main.startColor = color;
        }
    }
}
