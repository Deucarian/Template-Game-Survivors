using System.Collections.Generic;
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
        private int _bloodShardAmount = 25;
        private float _spawnRadius = 10f;
        private float _majorEnemyRadius = 8f;
        private SurvivorsPacingProfile _pacingProfile = SurvivorsPacingProfile.HumanPlaytest;
        private Vector2 _scroll;

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

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSnapshot(controller);
            EditorGUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Standard Run"))
            {
                controller.SelectStandardRun();
            }

            if (GUILayout.Button("Start Sprint Run"))
            {
                controller.SelectSprintRun();
            }

            if (GUILayout.Button("Choose Run Mode"))
            {
                controller.OpenRunModeSelection();
            }

            EditorGUILayout.EndHorizontal();
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

            _bloodShardAmount = EditorGUILayout.IntSlider("Grant Blood Shards", _bloodShardAmount, 1, 500);
            if (GUILayout.Button("Grant Blood Shards"))
            {
                controller.DebugGrantBloodShards(_bloodShardAmount);
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

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Trigger Horde Rush"))
            {
                controller.DebugTriggerHordeRush();
            }

            if (GUILayout.Button("Clear Horde Rush"))
            {
                controller.DebugClearActiveHordeRush();
            }

            EditorGUILayout.EndHorizontal();
            _majorEnemyRadius = EditorGUILayout.Slider("Major Enemy Radius", _majorEnemyRadius, 2f, 24f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Elite"))
            {
                controller.DebugSpawnElite(_majorEnemyRadius);
            }

            if (GUILayout.Button("Force Dread Elite"))
            {
                controller.DebugSpawnDreadElite(_majorEnemyRadius);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Miniboss"))
            {
                controller.DebugSpawnMiniboss(_majorEnemyRadius);
            }

            if (GUILayout.Button("Force Boss"))
            {
                controller.DebugSpawnBoss(_majorEnemyRadius);
            }

            if (GUILayout.Button("Force Sprint Boss"))
            {
                controller.DebugSpawnSprintBoss(_majorEnemyRadius);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8f);
            _stressTarget = EditorGUILayout.IntSlider("Stress Target", _stressTarget, 50, 512);
            if (GUILayout.Button("Apply Stress Profile"))
            {
                controller.DebugApplyStressProfile(_stressTarget);
            }

            EditorGUILayout.Space(8f);
            _pacingProfile = (SurvivorsPacingProfile)EditorGUILayout.EnumPopup("Pacing Profile", _pacingProfile);
            if (GUILayout.Button("Apply Pacing Profile And Restart Current Run"))
            {
                controller.DebugApplyPacingProfile(_pacingProfile);
            }

            if (GUILayout.Button("Trigger Magnet Recall"))
            {
                controller.TriggerMagnetRecall();
            }

            if (GUILayout.Button("Explicitly Reset Save / Progress"))
            {
                controller.DebugResetMetaProgression();
            }

            EditorGUILayout.Space(8f);
            DrawDebugLines("Run Metrics", controller.DebugDescribeRunMetrics());
            if (controller.IsRunStarted)
            {
                DrawDebugLines("Current Build", controller.DebugDescribeCurrentBuild());
                DrawDebugLines("Eligible Evolutions", controller.DebugDescribeEligibleEvolutionPool());
                DrawDebugLines("Current Draft Pool", controller.DebugDescribeCurrentDraftPool());
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawSnapshot(SurvivorsTemplateController controller)
        {
            string rewardTimeout = controller.CurrentTuning.RewardSelectionTimeoutSeconds > 0f
                ? controller.CurrentTuning.RewardSelectionTimeoutSeconds.ToString("0.#") + "s"
                : "Off";
            EditorGUILayout.LabelField("Run Mode", controller.CurrentRunModeDisplayName);
            EditorGUILayout.LabelField("Pacing Profile", BasicSurvivorsGame.GetPacingProfileDisplayName(controller.CurrentPacingProfile));
            EditorGUILayout.LabelField("Target Duration", controller.CurrentTuning.TargetDurationSeconds.ToString("0") + "s");
            EditorGUILayout.LabelField("Boss / Victory", $"{controller.CurrentTuning.BossSpawnTimeSeconds:0}s / {controller.CurrentTuning.SurvivalVictoryTimeSeconds:0}s");
            EditorGUILayout.LabelField("Time Scale", Time.timeScale.ToString("0.##"));
            EditorGUILayout.LabelField("Run Timer", $"{controller.State}  {controller.RunTimeSeconds:0}s  Level {controller.Level}");
            EditorGUILayout.LabelField("Next Milestone", controller.CurrentRunMilestoneHudLabel);
            EditorGUILayout.LabelField("Spawn Interval", controller.CurrentEnemySpawnIntervalSeconds.ToString("0.00") + "s");
            EditorGUILayout.LabelField("Spawn Pack", controller.CurrentEnemySpawnPackSize.ToString());
            EditorGUILayout.LabelField("Max Alive", controller.CurrentEnemyMaximumAlive.ToString());
            EditorGUILayout.LabelField("Alive Count", controller.ActiveEnemyCount.ToString());
            EditorGUILayout.LabelField("Enemy Speed Multiplier", controller.CurrentEnemySpeedMultiplier.ToString("0.##"));
            EditorGUILayout.LabelField("Reward Timeout", rewardTimeout);
            EditorGUILayout.LabelField("Draft Tools", $"Rerolls {controller.DraftRerollsRemaining}, Banishes {controller.DraftBanishesRemaining}, Skips {controller.DraftSkipCount}");
            EditorGUILayout.LabelField("Enemies", $"{controller.ActiveEnemyCount} alive, {controller.KilledCount} killed, {controller.ActiveEliteCount} elites");
            EditorGUILayout.LabelField("Major Threat", controller.IsMajorThreatHealthVisible
                ? $"{controller.CurrentMajorThreatHealthLabel} {controller.CurrentMajorThreatHealthFraction:P0}"
                : "None");
            EditorGUILayout.LabelField("Threat Enrage", $"{controller.MajorThreatEnrageCount} events, {controller.MajorThreatEnrageSupportSpawnCount} support");
            EditorGUILayout.LabelField("Horde Rush", $"{controller.ActiveHordeRushEnemyCount} tracked, {controller.HordeRushSpawnCount} spawned, {controller.HordeRushClearRewardCount} cleared");
            EditorGUILayout.LabelField("Build", $"Weapons {controller.ActiveWeaponCount}, Upgrades {controller.SelectedUpgradeCount}, Relics {controller.SelectedRelicCount}");
            EditorGUILayout.LabelField("Survivability", $"Health {controller.CurrentHealth:0}/{controller.MaxHealth:0}, Barrier {controller.BarrierValue:0}/{controller.BarrierCapacity:0}");
            EditorGUILayout.LabelField("Pools", $"Projectiles {controller.ActiveProjectileCount}, Pickups {controller.ActivePickupCount}");
        }

        private static void DrawDebugLines(string title, IReadOnlyList<string> lines)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (lines == null || lines.Count == 0)
            {
                EditorGUILayout.LabelField("None");
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                EditorGUILayout.LabelField(lines[i], EditorStyles.wordWrappedLabel);
            }
        }

        private static SurvivorsTemplateController FindController()
        {
            return Object.FindFirstObjectByType<SurvivorsTemplateController>();
        }
    }
}
