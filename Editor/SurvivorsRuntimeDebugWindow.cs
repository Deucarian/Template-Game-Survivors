using System.Collections.Generic;
using Deucarian.Editor;
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

        public static void Open()
        {
            SurvivorsRuntimeDebugWindow window =
                DeucarianEditorWindowPages.GetStandalone<SurvivorsRuntimeDebugWindow>("Survivors Debug");
            window.minSize = new Vector2(560f, 520f);
            window.Show();
        }

        public static IDeucarianEditorPage CreatePage() =>
            DeucarianEditorImGuiPage.Create<SurvivorsRuntimeDebugWindow>(
                "deucarian.template.survivors.debugger", window => window.OnGUI());

        private void OnGUI()
        {
            using (DeucarianEditorWorkbenchPanelScope page =
                   DeucarianEditorWorkbenchGUI.BeginSettingsPage(this, GUILayout.ExpandHeight(true)))
            {
                DeucarianEditorChrome.DrawPackageHeader(this,
                    "bug",
                    "Survivors Runtime Debugger",
                    "Inspect and deliberately exercise the active template run.");
                SurvivorsTemplateController controller = FindController();
                DeucarianEditorChrome.DrawSectionHeader("Runtime Controls");
                DeucarianEditorChrome.BeginSection();
                if (!Application.isPlaying)
                {
                    DeucarianEditorWorkbenchGUI.DrawStatusIconRow(
                        "circle-info",
                        "Enter Play Mode with the Basic Survivors Game scene open to use runtime controls.",
                        DeucarianEditorStatus.Info);
                    DeucarianEditorChrome.EndSection();
                    DeucarianEditorChrome.DrawFooterVersion(this,
                        "com.deucarian.template.game.survivors");
                    return;
                }

                if (controller == null)
                {
                    DeucarianEditorWorkbenchGUI.DrawStatusIconRow(
                        "circle-alert",
                        "No active SurvivorsTemplateController was found in the open scene.",
                        DeucarianEditorStatus.Warning);
                    DeucarianEditorChrome.EndSection();
                    DeucarianEditorChrome.DrawFooterVersion(this,
                        "com.deucarian.template.game.survivors");
                    return;
                }

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                DrawSnapshot(controller);
                EditorGUILayout.Space(8f);
                EditorGUILayout.BeginHorizontal();
                if (DeucarianEditorActionGUI.Button("Start Standard Run"))
                {
                    controller.SelectStandardRun();
                }

                if (DeucarianEditorActionGUI.Button("Start Sprint Run"))
                {
                    controller.SelectSprintRun();
                }

                if (DeucarianEditorActionGUI.Button("Choose Run Mode"))
                {
                    controller.OpenRunModeSelection();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(8f);
                _experienceAmount = DeucarianEditorInputGUI.IntSlider("Grant XP", _experienceAmount, 1, 250);
                if (DeucarianEditorActionGUI.Button("Grant XP"))
                {
                    controller.DebugGrantExperience(_experienceAmount);
                }

                if (DeucarianEditorActionGUI.Button("Force Level-Up"))
                {
                    controller.ForceLevelUp();
                }

                _bloodShardAmount = DeucarianEditorInputGUI.IntSlider("Grant Blood Shards", _bloodShardAmount, 1, 500);
                if (DeucarianEditorActionGUI.Button("Grant Blood Shards"))
                {
                    controller.DebugGrantBloodShards(_bloodShardAmount);
                }

                EditorGUILayout.Space(8f);
                _spawnRole = (SurvivorsEnemyRole)DeucarianEditorInputGUI.EnumPopup("Enemy Role", _spawnRole);
                _burstCount = DeucarianEditorInputGUI.IntSlider("Burst Count", _burstCount, 1, 128);
                _spawnRadius = DeucarianEditorInputGUI.Slider("Spawn Radius", _spawnRadius, 2f, 24f);
                if (DeucarianEditorActionGUI.Button("Spawn Enemy Burst"))
                {
                    controller.DebugSpawnEnemyBurst(_spawnRole, _burstCount, _spawnRadius);
                }

                _fillTarget = DeucarianEditorInputGUI.IntSlider("Fill Target", _fillTarget, 1, 512);
                if (DeucarianEditorActionGUI.Button("Fill Arena To Target"))
                {
                    controller.DebugFillArenaToTarget(_spawnRole, _fillTarget, _spawnRadius);
                }

                EditorGUILayout.BeginHorizontal();
                if (DeucarianEditorActionGUI.Button("Trigger Horde Rush"))
                {
                    controller.DebugTriggerHordeRush();
                }

                if (DeucarianEditorActionGUI.Button("Clear Horde Rush"))
                {
                    controller.DebugClearActiveHordeRush();
                }

                EditorGUILayout.EndHorizontal();
                _majorEnemyRadius = DeucarianEditorInputGUI.Slider("Major Enemy Radius", _majorEnemyRadius, 2f, 24f);
                EditorGUILayout.BeginHorizontal();
                if (DeucarianEditorActionGUI.Button("Force Elite"))
                {
                    controller.DebugSpawnElite(_majorEnemyRadius);
                }

                if (DeucarianEditorActionGUI.Button("Force Dread Elite"))
                {
                    controller.DebugSpawnDreadElite(_majorEnemyRadius);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (DeucarianEditorActionGUI.Button("Force Miniboss"))
                {
                    controller.DebugSpawnMiniboss(_majorEnemyRadius);
                }

                if (DeucarianEditorActionGUI.Button("Force Boss"))
                {
                    controller.DebugSpawnBoss(_majorEnemyRadius);
                }

                if (DeucarianEditorActionGUI.Button("Force Sprint Boss"))
                {
                    controller.DebugSpawnSprintBoss(_majorEnemyRadius);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(8f);
                _stressTarget = DeucarianEditorInputGUI.IntSlider("Stress Target", _stressTarget, 50, 512);
                if (DeucarianEditorActionGUI.Button("Apply Stress Profile"))
                {
                    controller.DebugApplyStressProfile(_stressTarget);
                }

                EditorGUILayout.Space(8f);
                _pacingProfile = (SurvivorsPacingProfile)DeucarianEditorInputGUI.EnumPopup("Pacing Profile", _pacingProfile);
                if (DeucarianEditorActionGUI.Button("Apply Pacing Profile And Restart Current Run"))
                {
                    controller.DebugApplyPacingProfile(_pacingProfile);
                }

                if (DeucarianEditorActionGUI.Button("Trigger Magnet Recall"))
                {
                    controller.TriggerMagnetRecall();
                }

                if (DeucarianEditorActionGUI.Button("Explicitly Reset Save / Progress"))
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
                DeucarianEditorChrome.EndSection();
                DeucarianEditorChrome.DrawFooterVersion(this,
                    "com.deucarian.template.game.survivors");
            }
        }

        private static void DrawSnapshot(SurvivorsTemplateController controller)
        {
            string rewardTimeout = controller.CurrentTuning.RewardSelectionTimeoutSeconds > 0f
                ? controller.CurrentTuning.RewardSelectionTimeoutSeconds.ToString("0.#") + "s"
                : "Off";
            DeucarianEditorTextGUI.LabelField("Run Mode", controller.CurrentRunModeDisplayName);
            DeucarianEditorTextGUI.LabelField("Pacing Profile", BasicSurvivorsGame.GetPacingProfileDisplayName(controller.CurrentPacingProfile));
            DeucarianEditorTextGUI.LabelField("Target Duration", controller.CurrentTuning.TargetDurationSeconds.ToString("0") + "s");
            DeucarianEditorTextGUI.LabelField("Boss / Victory", $"{controller.CurrentTuning.BossSpawnTimeSeconds:0}s / {controller.CurrentTuning.SurvivalVictoryTimeSeconds:0}s");
            DeucarianEditorTextGUI.LabelField("Time Scale", Time.timeScale.ToString("0.##"));
            DeucarianEditorTextGUI.LabelField("Run Timer", $"{controller.State}  {controller.RunTimeSeconds:0}s  Level {controller.Level}");
            DeucarianEditorTextGUI.LabelField("Next Milestone", controller.CurrentRunMilestoneHudLabel);
            DeucarianEditorTextGUI.LabelField("Spawn Interval", controller.CurrentEnemySpawnIntervalSeconds.ToString("0.00") + "s");
            DeucarianEditorTextGUI.LabelField("Spawn Pack", controller.CurrentEnemySpawnPackSize.ToString());
            DeucarianEditorTextGUI.LabelField("Max Alive", controller.CurrentEnemyMaximumAlive.ToString());
            DeucarianEditorTextGUI.LabelField("Alive Count", controller.ActiveEnemyCount.ToString());
            DeucarianEditorTextGUI.LabelField("Enemy Speed Multiplier", controller.CurrentEnemySpeedMultiplier.ToString("0.##"));
            DeucarianEditorTextGUI.LabelField("Reward Timeout", rewardTimeout);
            DeucarianEditorTextGUI.LabelField("Draft Tools", $"Rerolls {controller.DraftRerollsRemaining}, Banishes {controller.DraftBanishesRemaining}, Skips {controller.DraftSkipCount}");
            DeucarianEditorTextGUI.LabelField("Enemies", $"{controller.ActiveEnemyCount} alive, {controller.KilledCount} killed, {controller.ActiveEliteCount} elites");
            DeucarianEditorTextGUI.LabelField("Major Threat", controller.IsMajorThreatHealthVisible
                ? $"{controller.CurrentMajorThreatHealthLabel} {controller.CurrentMajorThreatHealthFraction:P0}"
                : "None");
            DeucarianEditorTextGUI.LabelField("Threat Enrage", $"{controller.MajorThreatEnrageCount} events, {controller.MajorThreatEnrageSupportSpawnCount} support");
            DeucarianEditorTextGUI.LabelField("Horde Rush", $"{controller.ActiveHordeRushEnemyCount} tracked, {controller.HordeRushSpawnCount} spawned, {controller.HordeRushClearRewardCount} cleared");
            DeucarianEditorTextGUI.LabelField("Build", $"Weapons {controller.ActiveWeaponCount}, Upgrades {controller.SelectedUpgradeCount}, Relics {controller.SelectedRelicCount}");
            DeucarianEditorTextGUI.LabelField("Survivability", $"Health {controller.CurrentHealth:0}/{controller.MaxHealth:0}, Barrier {controller.BarrierValue:0}/{controller.BarrierCapacity:0}");
            DeucarianEditorTextGUI.LabelField("Pools", $"Projectiles {controller.ActiveProjectileCount}, Pickups {controller.ActivePickupCount}");
        }

        private static void DrawDebugLines(string title, IReadOnlyList<string> lines)
        {
            DeucarianEditorTextGUI.LabelField(title, DeucarianEditorWorkbenchGUI.BoldLabelStyle);
            if (lines == null || lines.Count == 0)
            {
                DeucarianEditorTextGUI.LabelField("None");
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                DeucarianEditorTextGUI.LabelField(lines[i], DeucarianEditorWorkbenchGUI.LabelStyle);
            }
        }

        private static SurvivorsTemplateController FindController()
        {
            return Object.FindFirstObjectByType<SurvivorsTemplateController>();
        }
    }
}
