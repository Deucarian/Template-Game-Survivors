using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Common;
using static Deucarian.TemplateGameSurvivors.SurvivorsPrimitivePresentation;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns bounded transient feedback, animation and Unity material cleanup.</summary>
    internal sealed class SurvivorsThreatTelegraphPresenter : IDisposable
    {
        private readonly Func<Transform> _root;
        private Transform _feedbackRoot => _root();

        public SurvivorsThreatTelegraphPresenter(Func<Transform> root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        private const int MajorThreatSlamTelegraphEffectLimit = 12;
        private const float MajorThreatSlamTelegraphFadePaddingSeconds = 0.12f;
        private const int IncomingThreatTelegraphEffectLimit = 6;
        private const float IncomingThreatTelegraphFadePaddingSeconds = 0.18f;
        private readonly List<SurvivorsMajorThreatSlamTelegraphEffect> _majorThreatSlamTelegraphEffects = new List<SurvivorsMajorThreatSlamTelegraphEffect>(MajorThreatSlamTelegraphEffectLimit);
        public int MajorThreatSlamTelegraphEffectCount { get; private set; }
        public int ActiveMajorThreatSlamTelegraphEffectCount => _majorThreatSlamTelegraphEffects.Count;
        private readonly List<SurvivorsIncomingThreatTelegraphEffect> _incomingThreatTelegraphEffects = new List<SurvivorsIncomingThreatTelegraphEffect>(IncomingThreatTelegraphEffectLimit);
        public int IncomingThreatTelegraphEffectCount { get; private set; }
        public int ActiveIncomingThreatTelegraphEffectCount => _incomingThreatTelegraphEffects.Count;
        public string LastIncomingThreatTelegraphLabel { get; private set; } = string.Empty;

        public void ResetMetrics()
        {
            MajorThreatSlamTelegraphEffectCount = 0;
            IncomingThreatTelegraphEffectCount = 0;
            LastIncomingThreatTelegraphLabel = string.Empty;
        }

        public void Dispose()
        {
            while (_majorThreatSlamTelegraphEffects.Count > 0) ReleaseMajorThreatSlamTelegraphEffect(_majorThreatSlamTelegraphEffects.Count - 1);
            while (_incomingThreatTelegraphEffects.Count > 0) ReleaseIncomingThreatTelegraphEffect(_incomingThreatTelegraphEffects.Count - 1);
        }

        public void RecordMajorThreatSlamTelegraphEffect(Vector3 position, SurvivorsEnemyRole role, float radius, float durationSeconds)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_majorThreatSlamTelegraphEffects.Count >= MajorThreatSlamTelegraphEffectLimit)
            {
                ReleaseMajorThreatSlamTelegraphEffect(0);
            }

            Color color = ResolveMajorThreatSlamTelegraphColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            instance.name = "Survivors Major Threat Slam Telegraph";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = new Vector3(position.x, 0.115f, position.z);
            float diameter = Mathf.Max(0.4f, radius * 2f);
            Vector3 baseScale = new Vector3(diameter, 0.028f, diameter);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                UnityObjectUtility.DestroySafely(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            float duration = Mathf.Max(0.08f, durationSeconds) + MajorThreatSlamTelegraphFadePaddingSeconds;
            _majorThreatSlamTelegraphEffects.Add(new SurvivorsMajorThreatSlamTelegraphEffect(instance, material, color, baseScale, duration));
            MajorThreatSlamTelegraphEffectCount++;
        }

        public void TickMajorThreatSlamTelegraphEffects(float deltaTime)
        {
            if (_majorThreatSlamTelegraphEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _majorThreatSlamTelegraphEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsMajorThreatSlamTelegraphEffect effect = _majorThreatSlamTelegraphEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= effect.DurationSeconds)
                {
                    ReleaseMajorThreatSlamTelegraphEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / effect.DurationSeconds);
                float charge = Mathf.Lerp(0.58f, 1.05f, normalizedAge);
                float pulse = 1f + Mathf.Sin(effect.ElapsedSeconds * 26f) * 0.06f;
                effect.Instance.transform.localScale = effect.BaseScale * Mathf.Max(0.45f, charge * pulse);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.82f, 0.22f, normalizedAge);
                    effect.Material.color = color;
                }

                _majorThreatSlamTelegraphEffects[i] = effect;
            }
        }

        private void ReleaseMajorThreatSlamTelegraphEffect(int index)
        {
            if (index < 0 || index >= _majorThreatSlamTelegraphEffects.Count)
            {
                return;
            }

            SurvivorsMajorThreatSlamTelegraphEffect effect = _majorThreatSlamTelegraphEffects[index];
            _majorThreatSlamTelegraphEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                UnityObjectUtility.DestroySafely(effect.Material);
            }

            if (effect.Instance != null)
            {
                UnityObjectUtility.DestroySafely(effect.Instance);
            }
        }

        public void RecordIncomingThreatTelegraph(Vector3 position, SurvivorsEnemyRole role, string label, float radius, float durationSeconds)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_incomingThreatTelegraphEffects.Count >= IncomingThreatTelegraphEffectLimit)
            {
                ReleaseIncomingThreatTelegraphEffect(0);
            }

            Color color = ResolveIncomingThreatTelegraphColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            instance.name = "Survivors Incoming Threat Telegraph";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = new Vector3(position.x, 0.095f, position.z);
            float diameter = Mathf.Max(1.2f, radius * 2f);
            Vector3 baseScale = new Vector3(diameter, 0.024f, diameter);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                UnityObjectUtility.DestroySafely(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            float duration = Mathf.Max(0.12f, durationSeconds) + IncomingThreatTelegraphFadePaddingSeconds;
            _incomingThreatTelegraphEffects.Add(new SurvivorsIncomingThreatTelegraphEffect(instance, material, color, baseScale, duration));
            IncomingThreatTelegraphEffectCount++;
            LastIncomingThreatTelegraphLabel = string.IsNullOrWhiteSpace(label) ? SurvivorsTimedEncounterDirector.ResolveMajorThreatWarningLabel(role) : label;
        }

        public void TickIncomingThreatTelegraphEffects(float deltaTime)
        {
            if (_incomingThreatTelegraphEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _incomingThreatTelegraphEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsIncomingThreatTelegraphEffect effect = _incomingThreatTelegraphEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= effect.DurationSeconds)
                {
                    ReleaseIncomingThreatTelegraphEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / effect.DurationSeconds);
                float pulse = 1f + Mathf.Sin(effect.ElapsedSeconds * 18f) * 0.08f;
                float charge = Mathf.Lerp(0.88f, 1.16f, normalizedAge);
                effect.Instance.transform.localScale = effect.BaseScale * Mathf.Max(0.42f, charge * pulse);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.66f, 0.08f, normalizedAge);
                    effect.Material.color = color;
                }

                _incomingThreatTelegraphEffects[i] = effect;
            }
        }

        private void ReleaseIncomingThreatTelegraphEffect(int index)
        {
            if (index < 0 || index >= _incomingThreatTelegraphEffects.Count)
            {
                return;
            }

            SurvivorsIncomingThreatTelegraphEffect effect = _incomingThreatTelegraphEffects[index];
            _incomingThreatTelegraphEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                UnityObjectUtility.DestroySafely(effect.Material);
            }

            if (effect.Instance != null)
            {
                UnityObjectUtility.DestroySafely(effect.Instance);
            }
        }

        private static Color ResolveIncomingThreatTelegraphColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.12f, 0.48f, 0.66f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(1f, 0.36f, 0.12f, 0.64f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.38f, 0.86f, 1f, 0.62f);
                default:
                    return new Color(1f, 0.72f, 0.16f, 0.6f);
            }
        }

        public static float ResolveIncomingThreatTelegraphRadius(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return 9.5f;
                case SurvivorsEnemyRole.Miniboss:
                    return 8f;
                case SurvivorsEnemyRole.DreadElite:
                    return 6.8f;
                default:
                    return 5.8f;
            }
        }

        private static Color ResolveMajorThreatSlamTelegraphColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.1f, 0.5f, 0.82f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(1f, 0.38f, 0.14f, 0.8f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.42f, 0.88f, 1f, 0.78f);
                default:
                    return new Color(1f, 0.62f, 0.18f, 0.76f);
            }
        }

        private struct SurvivorsMajorThreatSlamTelegraphEffect
        {
            public SurvivorsMajorThreatSlamTelegraphEffect(GameObject instance, Material material, Color color, Vector3 baseScale, float durationSeconds)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                DurationSeconds = Mathf.Max(0.08f, durationSeconds);
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float DurationSeconds { get; }
            public float ElapsedSeconds;
        }

        private struct SurvivorsIncomingThreatTelegraphEffect
        {
            public SurvivorsIncomingThreatTelegraphEffect(GameObject instance, Material material, Color color, Vector3 baseScale, float durationSeconds)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                DurationSeconds = Mathf.Max(0.12f, durationSeconds);
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float DurationSeconds { get; }
            public float ElapsedSeconds;
        }

    }
}
