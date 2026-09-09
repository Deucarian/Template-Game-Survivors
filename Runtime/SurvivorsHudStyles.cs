using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Template-owned IMGUI style cache shared by its composed presenters.</summary>
    internal sealed class SurvivorsHudStyles
    {
        public GUIStyle HudTitleStyle { get; private set; }
        public GUIStyle HudLabelStyle { get; private set; }
        public GUIStyle HudSmallStyle { get; private set; }
        public GUIStyle LowHealthStyle { get; private set; }
        public GUIStyle MajorThreatWarningStyle { get; private set; }
        public GUIStyle RewardFeedbackStyle { get; private set; }
        public GUIStyle DraftTitleStyle { get; private set; }
        public GUIStyle DraftCardNameStyle { get; private set; }
        public GUIStyle DraftCardMetaStyle { get; private set; }
        public GUIStyle DraftCardDescriptionStyle { get; private set; }
        public GUIStyle DraftCardHotkeyStyle { get; private set; }
        public GUIStyle MenuTitleStyle { get; private set; }
        public GUIStyle MenuTabStyle { get; private set; }
        public GUIStyle TransparentButtonStyle { get; private set; }

        public void Reset()
        {
            HudTitleStyle = null;
            HudLabelStyle = null;
            HudSmallStyle = null;
            LowHealthStyle = null;
            MajorThreatWarningStyle = null;
            RewardFeedbackStyle = null;
            DraftTitleStyle = null;
            DraftCardNameStyle = null;
            DraftCardMetaStyle = null;
            DraftCardDescriptionStyle = null;
            DraftCardHotkeyStyle = null;
            MenuTitleStyle = null;
            MenuTabStyle = null;
            TransparentButtonStyle = null;
        }

        public void Ensure()
        {
            if (HudTitleStyle != null)
            {
                return;
            }

            HudTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.94f, 1f) }
            };
            HudLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            HudSmallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.78f, 0.88f, 0.95f) },
                wordWrap = true
            };
            LowHealthStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.3f, 0.24f) }
            };
            MajorThreatWarningStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.78f, 0.24f) }
            };
            RewardFeedbackStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            DraftTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            DraftCardNameStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            DraftCardMetaStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.92f, 1f) },
                wordWrap = true
            };
            DraftCardDescriptionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                normal = { textColor = new Color(0.88f, 0.93f, 0.97f) },
                wordWrap = true
            };
            DraftCardHotkeyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.black }
            };
            MenuTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            MenuTabStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            TransparentButtonStyle = new GUIStyle(GUIStyle.none);
        }
    }
}
