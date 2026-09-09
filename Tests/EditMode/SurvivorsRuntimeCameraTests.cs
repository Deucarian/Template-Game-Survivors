using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRuntimeCameraTests
    {
        [Test]
        public void BorrowedCameraAndExistingListenerSurviveRepeatedOwnerDisposal()
        {
            var borrowed = new GameObject("borrowed-camera-test");
            var camera = borrowed.AddComponent<Camera>();
            var listener = borrowed.AddComponent<AudioListener>();
            var owner = new SurvivorsRuntimeCamera();
            try
            {
                owner.Ensure(Vector3.right, camera);
                Assert.That(owner.Camera, Is.SameAs(camera));
                Assert.That(camera.orthographic, Is.True);
                Assert.That(camera.orthographicSize, Is.EqualTo(8.2f));
                owner.Ensure(Vector3.left, camera);
                Assert.That(borrowed.GetComponents<AudioListener>().Length, Is.EqualTo(1));
                owner.Dispose();
                owner.Dispose();
                Assert.That(camera != null && listener != null, Is.True);
            }
            finally { owner.Dispose(); Object.DestroyImmediate(borrowed); }
        }

        [Test]
        public void BorrowedCameraLosesOnlyTheListenerAddedByThisOwner()
        {
            var borrowed = new GameObject("borrowed-camera-test");
            var camera = borrowed.AddComponent<Camera>();
            var owner = new SurvivorsRuntimeCamera();
            try
            {
                owner.Ensure(Vector3.zero, camera);
                var added = borrowed.GetComponent<AudioListener>();
                Assert.That(added, Is.Not.Null);
                owner.Ensure(Vector3.zero, camera);
                Assert.That(borrowed.GetComponents<AudioListener>().Length, Is.EqualTo(1));
                owner.Dispose();
                Assert.That(added == null, Is.True);
                Assert.That(camera != null, Is.True);
            }
            finally { owner.Dispose(); Object.DestroyImmediate(borrowed); }
        }

        [Test]
        public void GeneratedCameraCanBeReusedAcrossRunsThenReleasedOnFinalDisposal()
        {
            var owner = new SurvivorsRuntimeCamera();
            GameObject cameraObject = null;
            try
            {
                owner.Ensure(Vector3.zero, null);
                Camera camera = owner.Camera;
                cameraObject = camera.gameObject;
                Assert.That(cameraObject.name, Is.EqualTo("Main Camera"));
                Assert.That(cameraObject.CompareTag("MainCamera"), Is.True);
                Assert.That(cameraObject.GetComponent<AudioListener>(), Is.Not.Null);
                owner.Ensure(Vector3.forward * 5f, camera);
                Assert.That(owner.Camera, Is.SameAs(camera));
                Assert.That(camera.transform.position, Is.EqualTo(new Vector3(0f, 13.5f, -4.5f)));
                Assert.That(cameraObject.GetComponents<AudioListener>().Length, Is.EqualTo(1));
                owner.Dispose();
                Assert.That(cameraObject == null, Is.True);
            }
            finally { owner.Dispose(); if (cameraObject != null) Object.DestroyImmediate(cameraObject); }
        }

        [Test]
        public void FollowRetainsLerpRateClampingAndMissingPlayerGuard()
        {
            var cameraObject = new GameObject("borrowed-camera-test");
            var camera = cameraObject.AddComponent<Camera>();
            var player = new GameObject("camera-target-test");
            var owner = new SurvivorsRuntimeCamera();
            try
            {
                owner.Ensure(Vector3.zero, camera);
                Vector3 start = camera.transform.position;
                player.transform.position = Vector3.right * 10f;
                owner.Follow(player.transform, 0.025f);
                Assert.That(camera.transform.position.x, Is.EqualTo(3.5f).Within(0.00001f));
                Assert.That(camera.transform.position.y, Is.EqualTo(start.y));
                Assert.That(camera.transform.position.z, Is.EqualTo(start.z));
                owner.Follow(player.transform, -1f);
                Assert.That(camera.transform.position.x, Is.EqualTo(3.5f).Within(0.00001f));
                owner.Follow(null, 1f);
                Assert.That(camera.transform.position.x, Is.EqualTo(3.5f).Within(0.00001f));
                owner.Follow(player.transform, 1f);
                Assert.That(camera.transform.position.x, Is.EqualTo(10f));
                Assert.That(Quaternion.Angle(camera.transform.rotation, Quaternion.Euler(58f, 0f, 0f)), Is.LessThan(0.001f));
            }
            finally { owner.Dispose(); Object.DestroyImmediate(player); Object.DestroyImmediate(cameraObject); }
        }
    }
}
