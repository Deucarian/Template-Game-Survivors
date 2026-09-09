using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Common;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsArenaPrimitives : IDisposable
    {
        private readonly List<Material> _materials = new List<Material>();
        public GameObject Create(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Transform parent)
        {
            GameObject instance = GameObject.CreatePrimitive(primitive);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localScale = scale;
            _materials.Add(SurvivorsPrimitivePresentation.ApplyColor(instance.GetComponentInChildren<Renderer>(), color));
            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            return instance;
        }

        public void Dispose()
        {
            foreach (Material material in _materials)
            {
                if (material != null) UnityObjectUtility.DestroySafely(material);
            }
            _materials.Clear();
        }
    }
}
