using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Common;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsWaystoneCompassPresenter
    {
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly SurvivorsArenaPrimitives _primitives;
        private Transform _waystoneCompassRoot;
        private SurvivorsUiTheme ActiveUiTheme => _theme();
        public SurvivorsWaystoneCompassPresenter(Func<SurvivorsUiTheme> theme, SurvivorsArenaPrimitives primitives)
        {
            _theme = theme;
            _primitives = primitives;
        }
        public bool Visible => _waystoneCompassRoot != null && _waystoneCompassRoot.gameObject.activeSelf;
        public Vector3 Forward => _waystoneCompassRoot == null ? Vector3.zero : _waystoneCompassRoot.forward;
        public void ForgetHierarchy() => _waystoneCompassRoot = null;
        public void Build(Transform parent)
        {
            GameObject root = new GameObject("Waystone Compass Arrow");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;
            _waystoneCompassRoot = root.transform;

            _primitives.Create(
                "Waystone Compass Shaft",
                PrimitiveType.Cube,
                new Vector3(0f, 0.18f, 3.08f),
                new Vector3(0.16f, 0.08f, 1.36f),
                ActiveUiTheme.GetArenaAccentColor(new Color(0.32f, 0.94f, 1f)),
                _waystoneCompassRoot);

            GameObject head = _primitives.Create(
                "Waystone Compass Head",
                PrimitiveType.Cube,
                new Vector3(0f, 0.2f, 3.88f),
                new Vector3(0.62f, 0.1f, 0.62f),
                ActiveUiTheme.GetFeedbackAccentColor(new Color(0.92f, 1f, 0.42f)),
                _waystoneCompassRoot);
            head.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);

            _primitives.Create(
                "Waystone Compass Pulse",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.13f, 3.88f),
                new Vector3(1.05f, 0.025f, 1.05f),
                ActiveUiTheme.GetArenaGridColor(new Color(0.12f, 0.58f, 0.72f)),
                _waystoneCompassRoot);

            root.SetActive(false);
        }

        public void Update(bool visible, float distance, Vector3 delta, float discoveryRadius, float runTime)
        {
            if (_waystoneCompassRoot == null)
            {
                return;
            }

            if (!visible || delta.sqrMagnitude <= 0.0001f)
            {
                _waystoneCompassRoot.gameObject.SetActive(false);
                return;
            }

            _waystoneCompassRoot.gameObject.SetActive(true);
            float headingDegrees = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            _waystoneCompassRoot.localRotation = Quaternion.Euler(0f, headingDegrees, 0f);

            float nearDistance = Mathf.Max(1f, discoveryRadius * 3.5f);
            float proximityPulse = Mathf.Clamp01(1f - distance / nearDistance) * 0.16f;
            float timePulse = Mathf.Sin(runTime * 7.5f) * 0.035f;
            float scale = Mathf.Max(0.86f, 1f + proximityPulse + timePulse);
            _waystoneCompassRoot.localScale = new Vector3(scale, scale, scale);
        }

        public void ApplyTheme()
        {
            if (_waystoneCompassRoot != null)
            {
                Renderer[] compassRenderers = _waystoneCompassRoot.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < compassRenderers.Length; i++)
                {
                    Renderer renderer = compassRenderers[i];
                    Color color = renderer != null && renderer.gameObject.name.IndexOf("Head", StringComparison.Ordinal) >= 0
                        ? ActiveUiTheme.GetFeedbackAccentColor(new Color(0.92f, 1f, 0.42f))
                        : renderer != null && renderer.gameObject.name.IndexOf("Pulse", StringComparison.Ordinal) >= 0
                            ? ActiveUiTheme.GetArenaGridColor(new Color(0.12f, 0.58f, 0.72f))
                            : ActiveUiTheme.GetArenaAccentColor(new Color(0.32f, 0.94f, 1f));
                    SurvivorsPrimitivePresentation.SetRendererColor(renderer, color);
                }
            }

        }
    }
}
