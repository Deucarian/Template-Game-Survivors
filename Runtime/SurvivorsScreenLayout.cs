using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal static class SurvivorsScreenLayout
    {
        public static void DrawDimOverlay(float alpha)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        public static Rect ResolveCenteredPanelRect(float maxWidth, float maxHeight, float minWidth, float minHeight, float margin)
        {
            return ResolveCenteredPanel(Screen.width, Screen.height, maxWidth, maxHeight, minWidth, minHeight, margin);
        }

        public static Rect ResolveCenteredPanel(float screenWidth, float screenHeight, float maxWidth, float maxHeight, float minWidth, float minHeight, float margin)
        {
            float safeMargin = Mathf.Max(8f, margin);
            float availableWidth = Mathf.Max(220f, screenWidth - safeMargin * 2f);
            float availableHeight = Mathf.Max(220f, screenHeight - safeMargin * 2f);
            float width = Mathf.Min(Mathf.Max(1f, maxWidth), availableWidth);
            float height = Mathf.Min(Mathf.Max(1f, maxHeight), availableHeight);
            if (width < minWidth)
            {
                width = availableWidth;
            }

            if (height < minHeight)
            {
                height = availableHeight;
            }

            return new Rect(
                Mathf.Max(safeMargin, screenWidth * 0.5f - width * 0.5f),
                Mathf.Max(safeMargin, screenHeight * 0.5f - height * 0.5f),
                width,
                height);
        }

        public static void DrawSolidRect(Rect rect, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}
