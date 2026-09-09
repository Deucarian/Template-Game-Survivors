using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Common;
using static Deucarian.TemplateGameSurvivors.SurvivorsPrimitivePresentation;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns bounded transient feedback, animation and Unity material cleanup.</summary>
    internal sealed class SurvivorsRewardDropPresenter : IDisposable
    {
        private readonly Func<Transform> _root;
        private Transform _feedbackRoot => _root();
        private readonly Func<SurvivorsUiTheme> _theme;
        private SurvivorsUiTheme ActiveUiTheme => _theme();

        public SurvivorsRewardDropPresenter(Func<Transform> root, Func<SurvivorsUiTheme> theme)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        }

        private const float MajorRewardDropLifetimeSeconds = 1.25f;
        private const int MajorRewardDropEffectLimit = 8;
        private readonly List<SurvivorsRewardDropFeedbackEffect> _rewardDropFeedbackEffects = new List<SurvivorsRewardDropFeedbackEffect>(MajorRewardDropEffectLimit);
        public int MajorRewardDropFeedbackCount { get; private set; }
        public int ActiveMajorRewardDropFeedbackCount => _rewardDropFeedbackEffects.Count;
        public string LastMajorRewardDropFeedbackLabel { get; private set; } = string.Empty;

        public void ResetMetrics()
        {
            MajorRewardDropFeedbackCount = 0;
            LastMajorRewardDropFeedbackLabel = string.Empty;
        }

        public void Dispose()
        {
            while (_rewardDropFeedbackEffects.Count > 0) ReleaseMajorRewardDropFeedbackEffect(_rewardDropFeedbackEffects.Count - 1);
        }

        public void RecordMajorRewardDropFeedback(Vector3 position, SurvivorsEnemyRole role, float radius)
        {
            if (_feedbackRoot == null)
            {
                return;
            }

            while (_rewardDropFeedbackEffects.Count >= MajorRewardDropEffectLimit)
            {
                ReleaseMajorRewardDropFeedbackEffect(0);
            }

            string label = ResolveMajorRewardDropLabel(role);
            Color color = ResolveMajorRewardDropColor(role);
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "Survivors " + label;
            instance.transform.SetParent(_feedbackRoot, false);
            Vector3 basePosition = position + Vector3.up * Mathf.Max(0.56f, radius * 1.1f);
            instance.transform.position = basePosition;
            instance.transform.rotation = Quaternion.Euler(12f, 45f, 18f);
            float size = role == SurvivorsEnemyRole.Boss
                ? Mathf.Max(0.9f, radius * 0.95f)
                : Mathf.Max(0.56f, radius * 0.82f);
            Vector3 baseScale = new Vector3(size, size, size);
            instance.transform.localScale = baseScale;

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                UnityObjectUtility.DestroySafely(collider);
            }

            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            Material material = ApplyColor(renderer, color);
            _rewardDropFeedbackEffects.Add(new SurvivorsRewardDropFeedbackEffect(instance, material, color, basePosition, baseScale));
            MajorRewardDropFeedbackCount++;
            LastMajorRewardDropFeedbackLabel = label;
        }

        public void TickMajorRewardDropFeedbackEffects(float deltaTime)
        {
            if (_rewardDropFeedbackEffects.Count == 0)
            {
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _rewardDropFeedbackEffects.Count - 1; i >= 0; i--)
            {
                SurvivorsRewardDropFeedbackEffect effect = _rewardDropFeedbackEffects[i];
                effect.ElapsedSeconds += dt;
                if (effect.Instance == null || effect.ElapsedSeconds >= MajorRewardDropLifetimeSeconds)
                {
                    ReleaseMajorRewardDropFeedbackEffect(i);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(effect.ElapsedSeconds / MajorRewardDropLifetimeSeconds);
                float pulse = Mathf.Sin(normalizedAge * Mathf.PI);
                effect.Instance.transform.position = effect.BasePosition + Vector3.up * (0.58f * pulse);
                effect.Instance.transform.localScale = effect.BaseScale * Mathf.Lerp(1f, 1.48f, pulse);
                effect.Instance.transform.Rotate(0f, 260f * dt, 0f, Space.World);
                if (effect.Material != null)
                {
                    Color color = effect.Color;
                    color.a = Mathf.Lerp(0.95f, 0.1f, normalizedAge);
                    effect.Material.color = color;
                }

                _rewardDropFeedbackEffects[i] = effect;
            }
        }

        private void ReleaseMajorRewardDropFeedbackEffect(int index)
        {
            if (index < 0 || index >= _rewardDropFeedbackEffects.Count)
            {
                return;
            }

            SurvivorsRewardDropFeedbackEffect effect = _rewardDropFeedbackEffects[index];
            _rewardDropFeedbackEffects.RemoveAt(index);
            if (effect.Material != null)
            {
                UnityObjectUtility.DestroySafely(effect.Material);
            }

            if (effect.Instance != null)
            {
                UnityObjectUtility.DestroySafely(effect.Instance);
            }
        }

        public Color ResolveMajorRewardDropColor(SurvivorsEnemyRole role)
        {
            Color elite = ActiveUiTheme.GetEliteThreatColor(new Color(1f, 0.76f, 0.2f));
            Color boss = ActiveUiTheme.GetBossThreatColor(new Color(1f, 0.2f, 0.45f));
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return WithAlpha(boss, 0.95f);
                case SurvivorsEnemyRole.Miniboss:
                    return WithAlpha(Color.Lerp(elite, boss, 0.62f), 0.95f);
                case SurvivorsEnemyRole.DreadElite:
                    return WithAlpha(Color.Lerp(elite, ActiveUiTheme.GetArenaAccentColor(new Color(0.38f, 0.9f, 1f)), 0.58f), 0.92f);
                default:
                    return WithAlpha(elite, 0.92f);
            }
        }

        public static string ResolveMajorRewardDropLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return "Boss Reward Cache";
                case SurvivorsEnemyRole.Miniboss:
                    return "Miniboss Reward Cache";
                case SurvivorsEnemyRole.DreadElite:
                    return "Dread Elite Reward Cache";
                default:
                    return "Elite Reward Cache";
            }
        }

        private struct SurvivorsRewardDropFeedbackEffect
        {
            public SurvivorsRewardDropFeedbackEffect(GameObject instance, Material material, Color color, Vector3 basePosition, Vector3 baseScale)
            {
                Instance = instance;
                Material = material;
                Color = color;
                BasePosition = basePosition;
                BaseScale = baseScale;
                ElapsedSeconds = 0f;
            }

            public GameObject Instance { get; }
            public Material Material { get; }
            public Color Color { get; }
            public Vector3 BasePosition { get; }
            public Vector3 BaseScale { get; }
            public float ElapsedSeconds;
        }

    }
}
