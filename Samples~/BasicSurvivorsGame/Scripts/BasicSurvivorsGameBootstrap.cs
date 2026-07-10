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
        private TextAsset weaponLibrary;

        [SerializeField]
        private TextAsset upgradeLibrary;

        [SerializeField]
        private TextAsset relicLibrary;

        [SerializeField]
        private TextAsset classLibrary;

        [SerializeField]
        private TextAsset progressionLibrary;

        [SerializeField]
        private TextAsset enemyLibrary;

        [SerializeField]
        private TextAsset pickupLibrary;

        [SerializeField]
        private TextAsset runFlowLibrary;

        [SerializeField]
        private TextAsset rewardLibrary;

        [SerializeField]
        private TextAsset uiThemeLibrary;

        [SerializeField]
        private TextAsset neonArcanaThemeLibrary;

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

            controller.ConfigureStrictSampleContent(
                weaponLibrary,
                upgradeLibrary,
                relicLibrary,
                classLibrary,
                progressionLibrary,
                enemyLibrary,
                pickupLibrary,
                runFlowLibrary,
                rewardLibrary,
                uiThemeLibrary,
                neonArcanaThemeLibrary);
            controller.ConfigureRunModeSelection(showRunModeSelection);
        }
    }
}
