using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Bounded, transient damage feedback. Never mutates combat or run state.</summary>
    internal sealed class SurvivorsDamagePopupPresenter
    {
        public const int Capacity = 72;
        public const float LifetimeSeconds = 0.9f;
        private const float RiseHeight = 1.25f;
        private readonly List<Popup> _popups = new List<Popup>(Capacity);
        private GUIStyle _damageStyle;
        private GUIStyle _playerDamageStyle;

        public int ActiveCount => _popups.Count;
        public int SpawnCount { get; private set; }

        public void Reset()
        {
            _popups.Clear();
            SpawnCount = 0;
        }

        public void ResetStyles()
        {
            _damageStyle = null;
            _playerDamageStyle = null;
        }

        public void Record(Vector3 worldPosition, float amount, bool playerDamage, bool critical)
        {
            if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return;
            if (_popups.Count >= Capacity) _popups.RemoveAt(0);
            int sequence = SpawnCount++;
            float lane = ((sequence % 5) - 2) * 0.18f;
            float lift = 1.1f + (sequence % 3) * 0.12f;
            string label = (playerDamage ? "-" : string.Empty) + Mathf.CeilToInt(amount);
            Color color = playerDamage
                ? new Color(1f, 0.24f, 0.18f, 1f)
                : critical ? new Color(1f, 0.68f, 0.12f, 1f) : new Color(1f, 0.92f, 0.42f, 1f);
            _popups.Add(new Popup(label, worldPosition + new Vector3(lane, lift, 0f), color, playerDamage));
        }

        public void Tick(float deltaTime)
        {
            float dt = Mathf.Max(0f, deltaTime);
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                Popup popup = _popups[i];
                popup.ElapsedSeconds += dt;
                if (popup.ElapsedSeconds >= LifetimeSeconds) _popups.RemoveAt(i);
                else _popups[i] = popup;
            }
        }

        public void Draw(Camera camera)
        {
            if (_popups.Count == 0 || camera == null) return;
            EnsureStyles();
            for (int i = 0; i < _popups.Count; i++)
            {
                Popup popup = _popups[i];
                float age = Mathf.Clamp01(popup.ElapsedSeconds / LifetimeSeconds);
                Vector3 world = popup.WorldPosition + Vector3.up * (RiseHeight * age);
                Vector3 screen = camera.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;
                GUIStyle style = popup.PlayerDamage ? _playerDamageStyle : _damageStyle;
                Color color = popup.Color;
                color.a *= 1f - age;
                style.normal.textColor = color;
                GUI.Label(new Rect(screen.x - 40f, Screen.height - screen.y - 14f, 80f, 24f), popup.Label, style);
            }
        }

        private void EnsureStyles()
        {
            if (_damageStyle != null) return;
            _damageStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.9f, 0.35f) }
            };
            _playerDamageStyle = new GUIStyle(_damageStyle)
            {
                fontSize = 17,
                normal = { textColor = new Color(1f, 0.24f, 0.18f) }
            };
        }

        private struct Popup
        {
            public Popup(string label, Vector3 position, Color color, bool playerDamage)
            {
                Label = label;
                WorldPosition = position;
                Color = color;
                PlayerDamage = playerDamage;
                ElapsedSeconds = 0f;
            }

            public string Label { get; }
            public Vector3 WorldPosition { get; }
            public Color Color { get; }
            public bool PlayerDamage { get; }
            public float ElapsedSeconds;
        }
    }
}
