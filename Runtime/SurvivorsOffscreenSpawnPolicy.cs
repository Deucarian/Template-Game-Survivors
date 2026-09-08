using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Deterministic radial spawn geometry over a supplied visible-ground rectangle.</summary>
    internal static class SurvivorsOffscreenSpawnPolicy
    {
        public static Vector3 Resolve(
            Vector3 center,
            float minimumDistance,
            float maximumDistance,
            long seed,
            float bandDepth,
            Rect? cameraGroundRect)
        {
            float min = Mathf.Max(1f, minimumDistance);
            float max = Mathf.Max(min + 0.1f, maximumDistance);
            float resolvedSeed = Mathf.Abs((float)(seed % 100000L));
            float angle = Mathf.Repeat(resolvedSeed * 137.508f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
            float span = Mathf.Max(0.1f, max - min);
            float radialDistance = min + Mathf.Repeat(resolvedSeed * 0.381966f, 1f) * span;
            float resolvedBandDepth = Mathf.Max(0.25f, bandDepth);
            if (cameraGroundRect.HasValue)
            {
                Rect visibleRect = cameraGroundRect.Value;
                float exitDistance = ResolveDistanceToExitRect(center, direction, visibleRect);
                float bandOffset = 0.15f + Mathf.Repeat(resolvedSeed * 0.754877f, 1f) * resolvedBandDepth;
                float distance = Mathf.Max(radialDistance, exitDistance + bandOffset);
                Vector3 candidate = center + direction * distance;
                if (ContainsGroundPoint(visibleRect, candidate))
                {
                    candidate = center + direction * (exitDistance + resolvedBandDepth + 0.5f);
                }

                candidate.y = center.y;
                return candidate;
            }

            Vector3 fallback = center + direction * radialDistance;
            fallback.y = center.y;
            return fallback;
        }

        private static float ResolveDistanceToExitRect(Vector3 center, Vector3 direction, Rect rect)
        {
            const float Epsilon = 0.0001f;
            float exit = float.PositiveInfinity;
            if (Mathf.Abs(direction.x) > Epsilon)
            {
                float boundary = direction.x > 0f ? rect.xMax : rect.xMin;
                exit = Mathf.Min(exit, (boundary - center.x) / direction.x);
            }

            if (Mathf.Abs(direction.z) > Epsilon)
            {
                float boundary = direction.z > 0f ? rect.yMax : rect.yMin;
                exit = Mathf.Min(exit, (boundary - center.z) / direction.z);
            }

            return float.IsInfinity(exit) ? 0f : Mathf.Max(0f, exit);
        }

        public static bool ContainsGroundPoint(Rect rect, Vector3 position)
        {
            return position.x >= rect.xMin &&
                position.x <= rect.xMax &&
                position.z >= rect.yMin &&
                position.z <= rect.yMax;
        }
    }
}
