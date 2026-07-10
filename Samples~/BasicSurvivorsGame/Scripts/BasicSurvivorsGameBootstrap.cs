using Deucarian.TemplateGameSurvivors;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.BasicSurvivorsGame
{
    public sealed class BasicSurvivorsGameBootstrap : MonoBehaviour
    {
        [SerializeField]
        private SurvivorsTemplateController controller;

        [SerializeField]
        private bool showRunModeSelection = true;

        [SerializeField]
        private TextAsset enemyLibrary;

        [SerializeField]
        private TextAsset runFlowLibrary;

        [SerializeField]
        private TextAsset rewardLibrary;

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

            controller.ConfigureAuthoredContent(enemyLibrary, runFlowLibrary, rewardLibrary);
            controller.ConfigureRunModeSelection(showRunModeSelection);
        }
    }
}
