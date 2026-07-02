using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    public sealed class SurvivorsRuntimeDebugWindow : EditorWindow
    {
        private SurvivorsEnemyRole _spawnRole = SurvivorsEnemyRole.Swarm;
        private int _experienceAmount = 12;
        private int _burstCount = 24;
        private int _fillTarget = 120;
        private int _stressTarget = 250;
        private float _spawnRadius = 10f;
        private SurvivorsPacingProfile _pacingProfile = SurvivorsPacingProfile.DebugFast;

        [MenuItem("Tools/Deucarian/Templates/Survivors/Runtime Debugger", priority = 340)]
        public static void Open()
        {
            GetWindow<SurvivorsRuntimeDebugWindow>("Survivors Debug");
        }

        private void OnGUI()
        {
            SurvivorsTemplateController controller = FindController();
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode with the Basic Survivors Game scene open to use runtime controls.", MessageType.Info);
                return;
            }

            if (controller == null)
            {
                EditorGUILayout.HelpBox("No active SurvivorsTemplateController was found in the open scene.", MessageType.Warning);
                return;
            }

            DrawSnapshot(controller);
            EditorGUILayout.Space(8f);
            _experienceAmount = EditorGUILayout.IntSlider("Grant XP", _experienceAmount, 1, 250);
            if (GUILayout.Button("Grant XP"))
            {
                controller.DebugGrantExperience(_experienceAmount);
            }

            if (GUILayout.Button("Force Level-Up"))
            {
                controller.ForceLevelUp();
            }

            EditorGUILayout.Space(8f);
            _spawnRole = (SurvivorsEnemyRole)EditorGUILayout.EnumPopup("Enemy Role", _spawnRole);
            _burstCount = EditorGUILayout.IntSlider("Burst Count", _burstCount, 1, 128);
            _spawnRadius = EditorGUILayout.Slider("Spawn Radius", _spawnRadius, 2f, 24f);
            if (GUILayout.Button("Spawn Enemy Burst"))
            {
                controller.DebugSpawnEnemyBurst(_spawnRole, _burstCount, _spawnRadius);
            }

            _fillTarget = EditorGUILayout.IntSlider("Fill Target", _fillTarget, 1, 512);
            if (GUILayout.Button("Fill Arena To Target"))
            {
                controller.DebugFillArenaToTarget(_spawnRole, _fillTarget, _spawnRadius);
            }

            EditorGUILayout.Space(8f);
            _stressTarget = EditorGUILayout.IntSlider("Stress Target", _stressTarget, 50, 512);
            if (GUILayout.Button("Apply Stress Profile"))
            {
                controller.DebugApplyStressProfile(_stressTarget);
            }

            EditorGUILayout.Space(8f);
            _pacingProfile = (SurvivorsPacingProfile)EditorGUILayout.EnumPopup("Pacing Profile", _pacingProfile);
            if (GUILayout.Button("Apply Pacing Profile And Restart"))
            {
                controller.DebugApplyPacingProfile(_pacingProfile);
            }

            if (GUILayout.Button("Trigger Magnet Recall"))
            {
                controller.TriggerMagnetRecall();
            }

            if (GUILayout.Button("Reset Save / Meta Progression"))
            {
                controller.DebugResetMetaProgression();
            }
        }

        private static void DrawSnapshot(SurvivorsTemplateController controller)
        {
            EditorGUILayout.LabelField("Run", $"{controller.State}  {controller.RunTimeSeconds:0}s  Level {controller.Level}  {controller.CurrentPacingProfile}");
            EditorGUILayout.LabelField("Enemies", $"{controller.ActiveEnemyCount} alive, {controller.KilledCount} killed, {controller.ActiveEliteCount} elites");
            EditorGUILayout.LabelField("Build", $"Weapons {controller.ActiveWeaponCount}, Upgrades {controller.SelectedUpgradeCount}, Relics {controller.SelectedRelicCount}");
            EditorGUILayout.LabelField("Survivability", $"Health {controller.CurrentHealth:0}/{controller.MaxHealth:0}, Barrier {controller.BarrierValue:0}/{controller.BarrierCapacity:0}");
            EditorGUILayout.LabelField("Pools", $"Projectiles {controller.ActiveProjectileCount}, Pickups {controller.ActivePickupCount}");
        }

        private static SurvivorsTemplateController FindController()
        {
            return Object.FindFirstObjectByType<SurvivorsTemplateController>();
        }
    }
}
