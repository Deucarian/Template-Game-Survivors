using Deucarian.TemplateGameSurvivors;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.BasicSurvivorsGame
{
    public sealed class BasicSurvivorsGameBootstrap : MonoBehaviour
    {
        [SerializeField]
        private SurvivorsTemplateController controller;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<SurvivorsTemplateController>();
            }

            if (controller == null)
            {
                controller = gameObject.AddComponent<SurvivorsTemplateController>();
            }
        }
    }
}
