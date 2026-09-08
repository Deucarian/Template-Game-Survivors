using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Unity camera projection used by the template spawn policy and marker presentation.</summary>
    internal static class SurvivorsCameraGroundProjection
    {
        public static bool TryResolve(Camera camera, float playerHeight, float padding, out Rect rect)
        {
            rect = default;
            if (camera == null)
            {
                return false;
            }

            var ground = new Plane(Vector3.up, new Vector3(0f, playerHeight, 0f));
            Vector3[] corners =
            {
                ResolveViewportGroundPoint(camera, ground, new Vector3(0f, 0f, 0f)),
                ResolveViewportGroundPoint(camera, ground, new Vector3(0f, 1f, 0f)),
                ResolveViewportGroundPoint(camera, ground, new Vector3(1f, 0f, 0f)),
                ResolveViewportGroundPoint(camera, ground, new Vector3(1f, 1f, 0f))
            };

            float minX = corners[0].x;
            float maxX = corners[0].x;
            float minZ = corners[0].z;
            float maxZ = corners[0].z;
            for (int i = 1; i < corners.Length; i++)
            {
                minX = Mathf.Min(minX, corners[i].x);
                maxX = Mathf.Max(maxX, corners[i].x);
                minZ = Mathf.Min(minZ, corners[i].z);
                maxZ = Mathf.Max(maxZ, corners[i].z);
            }

            float resolvedPadding = Mathf.Max(0f, padding);
            rect = Rect.MinMaxRect(minX - resolvedPadding, minZ - resolvedPadding, maxX + resolvedPadding, maxZ + resolvedPadding);
            return rect.width > 0.01f && rect.height > 0.01f;
        }

        private static Vector3 ResolveViewportGroundPoint(Camera camera, Plane ground, Vector3 viewportPoint)
        {
            Ray ray = camera.ViewportPointToRay(viewportPoint);
            if (ground.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            Vector3 fallback = camera.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, Mathf.Max(1f, camera.nearClipPlane)));
            fallback.y = 0f;
            return fallback;
        }
    }
}
