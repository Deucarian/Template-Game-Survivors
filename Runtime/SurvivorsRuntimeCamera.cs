using System;
using System.Collections.Generic;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns camera configuration/following and only the cameras/listeners this template creates.</summary>
    internal sealed class SurvivorsRuntimeCamera : IDisposable
    {
        private readonly List<GameObject> _createdCameras = new List<GameObject>();
        private readonly List<AudioListener> _addedListeners = new List<AudioListener>();
        internal Camera Camera { get; private set; }

        // The composition supplies Camera.main; explicit input keeps borrowed ownership testable.
        internal void Ensure(Vector3 playerPosition, Camera sceneCamera)
        {
            Camera = sceneCamera;
            if (Camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                _createdCameras.Add(cameraObject);
                Camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }
            else if (Camera.GetComponent<AudioListener>() == null)
            {
                _addedListeners.Add(Camera.gameObject.AddComponent<AudioListener>());
            }

            Camera.orthographic = true;
            Camera.orthographicSize = 8.2f;
            Camera.transform.position = playerPosition + new Vector3(0f, 13.5f, -9.5f);
            Camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }

        internal void Follow(Transform player, float deltaTime)
        {
            if (Camera == null || player == null) return;
            Vector3 target = player.position + new Vector3(0f, 13.5f, -9.5f);
            Camera.transform.position = Vector3.Lerp(Camera.transform.position, target, 14f * deltaTime);
            Camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }

        public void Dispose()
        {
            foreach (AudioListener listener in _addedListeners) UnityObjectUtility.DestroySafely(listener);
            _addedListeners.Clear();
            foreach (GameObject camera in _createdCameras) UnityObjectUtility.DestroySafely(camera);
            _createdCameras.Clear();
            Camera = null;
        }
    }
}
