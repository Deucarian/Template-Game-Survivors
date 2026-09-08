using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal static class SurvivorsPrimitivePresentation
    {
        public static Material ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return null;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader) { color = color };
            renderer.sharedMaterial = material;
            return material;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
