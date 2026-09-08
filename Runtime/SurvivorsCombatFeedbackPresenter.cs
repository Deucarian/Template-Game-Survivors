using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Common;
using static Deucarian.TemplateGameSurvivors.SurvivorsPrimitivePresentation;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns bounded transient feedback, animation and Unity material cleanup.</summary>
    internal sealed class SurvivorsCombatFeedbackPresenter : IDisposable
    {
        private readonly Func<Transform> _root;
        private Transform _feedbackRoot => _root();

        public SurvivorsCombatFeedbackPresenter(Func<Transform> root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        private const float EnemyDeathEffectLifetimeSeconds = 0.42f;
        private const int EnemyDeathEffectLimit = 56;
        private const float EnemyRangedAttackFeedbackLifetimeSeconds = 0.34f;
        private const int EnemyRangedAttackFeedbackLimit = 28;
        private readonly List<SurvivorsWorldFeedbackEffect> _worldFeedbackEffects = new List<SurvivorsWorldFeedbackEffect>(EnemyDeathEffectLimit);
        public int EnemyDeathEffectCount { get; private set; }
        public int ActiveEnemyDeathEffectCount => _worldFeedbackEffects.Count;
        private readonly List<SurvivorsEnemyRangedAttackFeedbackEffect> _enemyRangedAttackFeedbackEffects = new List<SurvivorsEnemyRangedAttackFeedbackEffect>(EnemyRangedAttackFeedbackLimit);
        public int EnemyRangedAttackFeedbackCount { get; private set; }
        public int ActiveEnemyRangedAttackFeedbackCount => _enemyRangedAttackFeedbackEffects.Count;

        public void ResetMetrics()
        {
            EnemyDeathEffectCount = 0;
            EnemyRangedAttackFeedbackCount = 0;
        }

        public void Dispose()
        {
            while (_worldFeedbackEffects.Count > 0) ReleaseWorldFeedbackEffect(_worldFeedbackEffects.Count - 1);
            while (_enemyRangedAttackFeedbackEffects.Count > 0) ReleaseEnemyRangedAttackFeedbackEffect(_enemyRangedAttackFeedbackEffects.Count - 1);
        }

        public void RecordEnemyDeathEffect(Vector3 position, SurvivorsEnemyRole role, float radius)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_worldFeedbackEffects.Count >= EnemyDeathEffectLimit)
            {
                ReleaseWorldFeedbackEffect(0);
            }

            Color color = ResolveEnemyDeathEffectColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            instance.name = "Survivors Enemy Death Burst";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = position + Vector3.up * 0.16f;
            Vector3 baseScale = Vector3.one * Mathf.Max(0.35f, radius * 1.7f);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                UnityObjectUtility.DestroySafely(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            _worldFeedbackEffects.Add(new SurvivorsWorldFeedbackEffect(instance, renderer, material, color, baseScale));
            EnemyDeathEffectCount++;
        }

        public void TickWorldFeedbackEffects(float deltaTime)
        {
            if (_worldFeedbackEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _worldFeedbackEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsWorldFeedbackEffect effect = _worldFeedbackEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= EnemyDeathEffectLifetimeSeconds)
                {
                    ReleaseWorldFeedbackEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / EnemyDeathEffectLifetimeSeconds);
                float scale = Mathf.Lerp(1f, 2.65f, normalizedAge);
                effect.Instance.transform.localScale = effect.BaseScale * scale;
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.72f, 0.04f, normalizedAge);
                    effect.Material.color = color;
                }

                _worldFeedbackEffects[i] = effect;
            }
        }

        private void ReleaseWorldFeedbackEffect(int index)
        {
            if (index < 0 || index >= _worldFeedbackEffects.Count)
            {
                return;
            }

            SurvivorsWorldFeedbackEffect effect = _worldFeedbackEffects[index];
            _worldFeedbackEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                UnityObjectUtility.DestroySafely(effect.Material);
            }

            if (effect.Instance != null)
            {
                UnityObjectUtility.DestroySafely(effect.Instance);
            }
        }

        public void RecordEnemyRangedAttackFeedback(Vector3 origin, Vector3 target, SurvivorsEnemyRole role)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            Vector3 start = origin + Vector3.up * 0.42f;
            Vector3 end = target + Vector3.up * 0.36f;
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            if (distance <= 0.05f)
            {
                return;
            }

            while (_enemyRangedAttackFeedbackEffects.Count >= EnemyRangedAttackFeedbackLimit)
            {
                ReleaseEnemyRangedAttackFeedbackEffect(0);
            }

            Color color = ResolveEnemyRangedAttackColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "Survivors Enemy Ranged Attack Cue";
            instance.transform.SetParent(_feedbackRoot, false);
            instance.transform.position = (start + end) * 0.5f;
            instance.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Vector3 baseScale = new Vector3(0.1f, 0.1f, distance);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                UnityObjectUtility.DestroySafely(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            _enemyRangedAttackFeedbackEffects.Add(new SurvivorsEnemyRangedAttackFeedbackEffect(instance, material, color, baseScale));
            EnemyRangedAttackFeedbackCount++;
        }

        public void TickEnemyRangedAttackFeedbackEffects(float deltaTime)
        {
            if (_enemyRangedAttackFeedbackEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _enemyRangedAttackFeedbackEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsEnemyRangedAttackFeedbackEffect effect = _enemyRangedAttackFeedbackEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= EnemyRangedAttackFeedbackLifetimeSeconds)
                {
                    ReleaseEnemyRangedAttackFeedbackEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / EnemyRangedAttackFeedbackLifetimeSeconds);
                float width = Mathf.Lerp(1.35f, 0.25f, normalizedAge);
                effect.Instance.transform.localScale = new Vector3(effect.BaseScale.x * width, effect.BaseScale.y * width, effect.BaseScale.z);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.86f, 0.05f, normalizedAge);
                    effect.Material.color = color;
                }

                _enemyRangedAttackFeedbackEffects[i] = effect;
            }
        }

        private void ReleaseEnemyRangedAttackFeedbackEffect(int index)
        {
            if (index < 0 || index >= _enemyRangedAttackFeedbackEffects.Count)
            {
                return;
            }

            SurvivorsEnemyRangedAttackFeedbackEffect effect = _enemyRangedAttackFeedbackEffects[index];
            _enemyRangedAttackFeedbackEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                UnityObjectUtility.DestroySafely(effect.Material);
            }

            if (effect.Instance != null)
            {
                UnityObjectUtility.DestroySafely(effect.Instance);
            }
        }

        private static Color ResolveEnemyRangedAttackColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.18f, 0.52f, 0.86f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.42f, 0.86f, 1f, 0.82f);
                case SurvivorsEnemyRole.Miniboss:
                case SurvivorsEnemyRole.Elite:
                    return new Color(1f, 0.56f, 0.2f, 0.84f);
                default:
                    return new Color(0.54f, 1f, 0.32f, 0.78f);
            }
        }

        private static Color ResolveEnemyDeathEffectColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Elite:
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(1f, 0.62f, 0.18f, 0.72f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(1f, 0.3f, 0.74f, 0.72f);
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.15f, 0.22f, 0.76f);
                default:
                    return new Color(0.35f, 0.95f, 1f, 0.68f);
            }
        }

        private struct SurvivorsWorldFeedbackEffect
        {
            public SurvivorsWorldFeedbackEffect(GameObject instance, Renderer renderer, Material material, Color color, Vector3 baseScale)
            {
                Instance = instance;
                Renderer = renderer;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Renderer Renderer { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float ElapsedSeconds;
        }

        private struct SurvivorsEnemyRangedAttackFeedbackEffect
        {
            public SurvivorsEnemyRangedAttackFeedbackEffect(GameObject instance, Material material, Color color, Vector3 baseScale)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BaseScale = baseScale;
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BaseScale { get; }
            public float ElapsedSeconds;
        }

    }
}
