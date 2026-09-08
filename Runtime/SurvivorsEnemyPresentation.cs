using System;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns an enemy's transient material, hit flash and status tint; observes status only.</summary>
    internal sealed class SurvivorsEnemyPresentation : IDisposable
    {
        private readonly Transform _transform;
        private readonly Func<bool> _isSlowed;
        private readonly Func<bool> _isBurning;
        private Renderer _renderer;
        private Material _runtimeMaterial;
        private Color _baseTint;
        private Vector3 _baseScale = Vector3.one;
        private float _hitFlashTimer;
        private float _hitFlashDuration;
        private bool _hitFlashCritical;

        public SurvivorsEnemyPresentation(Transform transform, Func<bool> isSlowed, Func<bool> isBurning)
        {
            _transform = transform;
            _isSlowed = isSlowed ?? throw new ArgumentNullException(nameof(isSlowed));
            _isBurning = isBurning ?? throw new ArgumentNullException(nameof(isBurning));
        }

        public bool IsHitFlashActive => _hitFlashTimer > 0f;

        public void Initialize(float radius, Color tint)
        {
            _transform.localScale = Vector3.one * (radius * 2f);
            _baseScale = _transform.localScale;
            ResetForPool();
            ApplyTint(tint);
        }

        public void ResetForPool()
        {
            _hitFlashTimer = 0f;
            _hitFlashDuration = 0f;
            _hitFlashCritical = false;
            if (_runtimeMaterial != null) _runtimeMaterial.color = _baseTint;
            _transform.localScale = _baseScale;
        }

        public void Refresh()
        {
            ApplyHitFlashPresentation(_hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(_hitFlashTimer / _hitFlashDuration));
        }

        public void Dispose()
        {
            UnityObjectUtility.DestroySafely(_runtimeMaterial);
            _runtimeMaterial = null;
        }

        public void TriggerHitFlash(bool critical, float durationSeconds)
        {
            _hitFlashCritical = critical;
            _hitFlashDuration = Mathf.Max(0.01f, durationSeconds);
            _hitFlashTimer = _hitFlashDuration;
            ApplyHitFlashPresentation(1f);
        }

        public void Tick(float deltaTime)
        {
            if (_hitFlashTimer <= 0f)
            {
                return;
            }

            _hitFlashTimer = Mathf.Max(0f, _hitFlashTimer - Mathf.Max(0f, deltaTime));
            float intensity = _hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(_hitFlashTimer / _hitFlashDuration);
            ApplyHitFlashPresentation(intensity);
        }

        private void ApplyHitFlashPresentation(float intensity)
        {
            if (_runtimeMaterial != null)
            {
                Color baseTint = ResolvePresentationBaseTint();
                Color flash = _hitFlashCritical
                    ? new Color(1f, 0.86f, 0.25f)
                    : Color.white;
                _runtimeMaterial.color = Color.Lerp(baseTint, flash, Mathf.Clamp01(intensity));
            }

            _transform.localScale = _baseScale * (1f + 0.18f * Mathf.Clamp01(intensity));
        }

        private Color ResolvePresentationBaseTint()
        {
            Color tint = _isSlowed()
                ? Color.Lerp(_baseTint, new Color(0.52f, 0.88f, 1f), 0.48f)
                : _baseTint;
            return _isBurning()
                ? Color.Lerp(tint, new Color(1f, 0.42f, 0.12f), 0.42f)
                : tint;
        }

        private void ApplyTint(Color tint)
        {
            Renderer renderer = _transform.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _renderer = renderer;
            _baseTint = tint;
            if (_runtimeMaterial == null || _runtimeMaterial.shader != shader)
            {
                UnityObjectUtility.DestroySafely(_runtimeMaterial);
                _runtimeMaterial = new Material(shader);
            }

            _runtimeMaterial.color = tint;
            _renderer.sharedMaterial = _runtimeMaterial;
        }
    }
}
