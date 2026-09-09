using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.PlayModeTests
{
    public sealed class SurvivorsProjectileMaterialLifetimeTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly List<Material> _materials = new List<Material>();

        [UnityTest]
        public IEnumerator DestroyedActorReleasesOwnedCopyAndPreservesBorrowedSource()
        {
            SurvivorsProjectileActor actor = CreateActor(out Renderer renderer, out Material borrowed);
            GameObject other = Track(new GameObject("Borrowed projectile material consumer"));
            Renderer otherRenderer = other.AddComponent<MeshRenderer>();
            otherRenderer.sharedMaterial = borrowed;
            Initialize(actor);
            Material owned = Track(renderer.sharedMaterial);
            Assert.That(owned, Is.Not.SameAs(borrowed));

            Object.Destroy(actor.gameObject);
            yield return null;
            yield return null;

            Assert.That(owned == null, Is.True, "The actor-created material must be released with its actor.");
            Assert.That(borrowed != null, Is.True, "The assigned source material is borrowed.");
            Assert.That(otherRenderer.sharedMaterial, Is.SameAs(borrowed));
        }

        [UnityTest]
        public IEnumerator PooledResetAndReinitializeReuseOwnedMaterial()
        {
            SurvivorsProjectileActor actor = CreateActor(out Renderer renderer, out Material borrowed);
            Initialize(actor);
            Material owned = Track(renderer.sharedMaterial);
            actor.gameObject.SetActive(false);
            actor.ResetForWorldSpawn();
            yield return null;

            Assert.That(owned != null, Is.True, "Pooling must keep the reusable material alive.");
            actor.gameObject.SetActive(true);
            Initialize(actor);
            Assert.That(renderer.sharedMaterial, Is.SameAs(owned));
            Assert.That(borrowed != null, Is.True);

            Object.Destroy(actor.gameObject);
            yield return null;
            yield return null;
            Assert.That(owned == null, Is.True, "The reused material must still be released on final destruction.");
            Assert.That(borrowed != null, Is.True);
        }

        [UnityTest]
        public IEnumerator ShaderReplacementReleasesOnlySupersededOwnedMaterial()
        {
            SurvivorsProjectileActor actor = CreateActor(out Renderer renderer, out Material borrowed);
            Initialize(actor);
            Material first = Track(renderer.sharedMaterial);
            Shader alternate = Shader.Find("Sprites/Default") ?? Shader.Find("UI/Default");
            Assert.That(alternate, Is.Not.Null, "A built-in alternate shader is required for this lifecycle regression.");
            Assert.That(alternate, Is.Not.SameAs(first.shader));
            first.shader = alternate;

            Initialize(actor);
            Material replacement = Track(renderer.sharedMaterial);
            Assert.That(replacement, Is.Not.SameAs(first));
            Assert.That(replacement, Is.Not.SameAs(borrowed));
            yield return null;
            yield return null;

            Assert.That(first == null, Is.True, "Changing shader must release the superseded actor-created material.");
            Assert.That(replacement != null, Is.True);
            Assert.That(borrowed != null, Is.True);
            Object.Destroy(actor.gameObject);
            yield return null;
            yield return null;
            Assert.That(replacement == null, Is.True);
            Assert.That(borrowed != null, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject item in _objects)
            {
                if (item != null) Object.Destroy(item);
            }

            yield return null;
            yield return null;
            // Also clean leaked copies after failed assertions so later fixtures remain isolated.
            foreach (Material item in _materials)
            {
                if (item != null) Object.Destroy(item);
            }

            yield return null;
            _objects.Clear();
            _materials.Clear();
        }

        private SurvivorsProjectileActor CreateActor(out Renderer renderer, out Material borrowed)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Assert.That(shader, Is.Not.Null);
            borrowed = Track(new Material(shader));
            GameObject root = Track(new GameObject("Projectile material lifetime regression"));
            renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = borrowed;
            return root.AddComponent<SurvivorsProjectileActor>();
        }

        private static void Initialize(SurvivorsProjectileActor actor)
        {
            actor.Initialize(null, null, Vector3.forward, 1f, 1f, 0.2f, 2f, 0, 0, 0, 0);
        }

        private GameObject Track(GameObject item)
        {
            _objects.Add(item);
            return item;
        }

        private Material Track(Material item)
        {
            _materials.Add(item);
            return item;
        }
    }
}
