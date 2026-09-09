using System;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Unity audio resource ownership and theme presentation, independent from gameplay.</summary>
    internal sealed class SurvivorsAudioPresenter : IDisposable
    {
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly Func<float> _clock;
        private AudioSource _source;

        public SurvivorsAudioPresenter(Func<SurvivorsUiTheme> theme, Func<float> clock)
        {
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public SurvivorsAudioEventRouter Events { get; } = new SurvivorsAudioEventRouter();
        public AudioClip SpawnClip { get; private set; }
        public AudioClip FireClip { get; private set; }
        public AudioClip KillClip { get; private set; }
        public AudioClip PickupClip { get; private set; }
        public AudioClip LevelUpClip { get; private set; }
        public AudioClip BossClip { get; private set; }
        public AudioClip DangerClip { get; private set; }

        public void Build(Transform parent)
        {
            ReleaseResources();
            try
            {
                var audioObject = new GameObject("Survivors Feedback Audio");
                audioObject.transform.SetParent(parent, false);
                _source = audioObject.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.spatialBlend = 0f;
                _source.volume = 0.28f;
                SpawnClip = CreateTone("survivors-spawn", 170f, 0.12f, 0.16f);
                FireClip = CreateTone("survivors-fire", 540f, 0.07f, 0.13f);
                KillClip = CreateTone("survivors-kill", 760f, 0.1f, 0.18f);
                PickupClip = CreateTone("survivors-pickup", 1040f, 0.07f, 0.16f);
                LevelUpClip = CreateTone("survivors-level-up", 880f, 0.2f, 0.2f);
                BossClip = CreateTone("survivors-boss", 92f, 0.28f, 0.24f);
                DangerClip = CreateTone("survivors-danger", 130f, 0.16f, 0.2f);
            }
            catch
            {
                ReleaseResources();
                throw;
            }
        }

        public void Play(AudioClip clip)
        {
            if (!Events.Muted && _source != null && clip != null)
            {
                _source.PlayOneShot(clip);
            }
        }

        public bool PlayEvent(string eventId, AudioClip fallbackClip, float fallbackThrottleSeconds)
        {
            if (string.IsNullOrWhiteSpace(eventId) || Events.Muted) return false;
            SurvivorsUiTheme theme = _theme();
            string normalized = eventId.Trim();
            float throttle = theme.GetAudioEventThrottleSeconds(normalized, fallbackThrottleSeconds);
            if (!Events.TryDispatch(normalized, _clock(), throttle)) return false;

            if (_source != null && fallbackClip != null)
            {
                float oldVolume = _source.volume;
                try
                {
                    _source.volume = theme.GetAudioEventVolume(normalized, oldVolume);
                    _source.PlayOneShot(fallbackClip);
                }
                finally
                {
                    _source.volume = oldVolume;
                }
            }

            return true;
        }

        public void ReleaseResources()
        {
            if (_source != null) UnityObjectUtility.DestroySafely(_source.gameObject);
            _source = null;
            UnityObjectUtility.DestroySafely(SpawnClip);
            UnityObjectUtility.DestroySafely(FireClip);
            UnityObjectUtility.DestroySafely(KillClip);
            UnityObjectUtility.DestroySafely(PickupClip);
            UnityObjectUtility.DestroySafely(LevelUpClip);
            UnityObjectUtility.DestroySafely(BossClip);
            UnityObjectUtility.DestroySafely(DangerClip);
            SpawnClip = FireClip = KillClip = PickupClip = LevelUpClip = BossClip = DangerClip = null;
        }

        public void Dispose() => ReleaseResources();

        private static AudioClip CreateTone(string name, float frequency, float durationSeconds, float volume)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * durationSeconds));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float fade = Mathf.Clamp01(1f - i / (float)sampleCount);
                samples[i] = Mathf.Sin(Mathf.PI * 2f * frequency * t) * volume * fade;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
