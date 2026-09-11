using System;
using System.Linq;
using System.Collections.Generic;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Controls = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    internal sealed class SurvivorsRuntimeWorkspace
    {
        private readonly DeucarianEditorWorkspace workspace;
        private readonly DeucarianEditorWorkspaceForm scope, snapshot, tuning;
        private readonly Label state, sessionTime, sessionEnemies;
        private readonly VisualElement controls;
        private SurvivorsTemplateController controller;
        private SurvivorsEnemyRole role = SurvivorsEnemyRole.Swarm;
        private SurvivorsPacingProfile pacing = SurvivorsPacingProfile.HumanPlaytest;
        private int experience = 12, burst = 24, fill = 120, stress = 250, shards = 25;
        private float radius = 10, majorRadius = 8;
        private double nextUpdate;
        internal IDeucarianEditorPage Page { get; }
        private bool Connected => Application.isPlaying && controller != null && controller.gameObject.scene.IsValid()
            && controller.gameObject.scene.isLoaded && !EditorUtility.IsPersistent(controller);

        internal SurvivorsRuntimeWorkspace()
        {
            var root = new VisualElement();
            workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "Survivors runtime";
            workspace.Subtitle.text = "Inspect a run without losing the overview.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, "deucarian.template.survivors.debugger");
            var scroll = Controls.Scroll("survivors-runtime"); workspace.Content.Add(scroll);
            var session = Controls.Panel("survivors-session", "Session"); scroll.Add(session);
            var sessionRow = Controls.Region(null, "dw-summary-row"); session.Add(sessionRow);
            state = Controls.Label(string.Empty, "dw-readonly"); state.name = "survivors-run-state"; sessionRow.Add(state);
            sessionTime = Controls.Label(string.Empty, "dw-readonly"); sessionRow.Add(sessionTime);
            sessionEnemies = Controls.Label(string.Empty, "dw-readonly"); sessionRow.Add(sessionEnemies);
            scope = new DeucarianEditorWorkspaceForm(session).Section("Choose controller", true);
            var target = scope.Asset("survivors-controller", "Controller", typeof(SurvivorsTemplateController),
                () => controller, value => { controller = value as SurvivorsTemplateController; Refresh(); });
            target.allowSceneObjects = true;
            var values = Controls.Panel("survivors-snapshot"); scroll.Add(values);
            snapshot = new DeucarianEditorWorkspaceForm(values).Section("Run snapshot", true);
            snapshot.ReadOnly("survivors-run-mode", "Run mode", () => Connected ? controller.CurrentRunModeDisplayName : "No active run");
            snapshot.ReadOnly("survivors-run-time", "Run timer", () => Connected ? controller.RunTimeSeconds.ToString("0") + " s" : "—");
            snapshot.ReadOnly("survivors-health", "Health", () => Connected ? $"{controller.CurrentHealth:0} / {controller.MaxHealth:0}" : "—");
            snapshot.ReadOnly("survivors-level", "Level", () => Connected ? controller.Level.ToString() : "—");
            var details = snapshot.Section("Run details", true);
            details.ReadOnly(null, "Pacing", () => Connected ? BasicSurvivorsGame.GetPacingProfileDisplayName(controller.CurrentPacingProfile) : "—");
            details.ReadOnly(null, "Duration", () => Connected ? controller.CurrentTuning.TargetDurationSeconds.ToString("0") + " s" : "—");
            details.ReadOnly(null, "Boss / victory", () => Connected ? $"{controller.CurrentTuning.BossSpawnTimeSeconds:0} s / {controller.CurrentTuning.SurvivalVictoryTimeSeconds:0} s" : "—");
            details.ReadOnly(null, "Time scale", () => Connected ? Time.timeScale.ToString("0.##") : "—");
            details.ReadOnly(null, "Next milestone", () => Connected ? controller.CurrentRunMilestoneHudLabel : "—");
            details.ReadOnly(null, "Spawn interval / pack", () => Connected ? $"{controller.CurrentEnemySpawnIntervalSeconds:0.00} s / {controller.CurrentEnemySpawnPackSize}" : "—");
            details.ReadOnly(null, "Alive / maximum", () => Connected ? $"{controller.ActiveEnemyCount} / {controller.CurrentEnemyMaximumAlive}" : "—");
            details.ReadOnly(null, "Enemy speed", () => Connected ? controller.CurrentEnemySpeedMultiplier.ToString("0.##") : "—");
            details.ReadOnly(null, "Reward timeout", () => Connected ? controller.CurrentTuning.RewardSelectionTimeoutSeconds.ToString("0.#") + " s (0 = off)" : "—");
            details.ReadOnly(null, "Draft tools", () => Connected ? $"{controller.DraftRerollsRemaining} rerolls · {controller.DraftBanishesRemaining} banishes · {controller.DraftSkipCount} skips" : "—");
            details.ReadOnly(null, "Enemies", () => Connected ? $"{controller.KilledCount} killed · {controller.ActiveEliteCount} elites" : "—");
            details.ReadOnly(null, "Major threat", () => Connected && controller.IsMajorThreatHealthVisible ? $"{controller.CurrentMajorThreatHealthLabel} {controller.CurrentMajorThreatHealthFraction:P0}" : "None");
            details.ReadOnly(null, "Enrage", () => Connected ? $"{controller.MajorThreatEnrageCount} events · {controller.MajorThreatEnrageSupportSpawnCount} support" : "—");
            details.ReadOnly(null, "Horde rush", () => Connected ? $"{controller.ActiveHordeRushEnemyCount} tracked · {controller.HordeRushSpawnCount} spawned · {controller.HordeRushClearRewardCount} cleared" : "—");
            details.ReadOnly(null, "Build", () => Connected ? $"{controller.ActiveWeaponCount} weapons · {controller.SelectedUpgradeCount} upgrades · {controller.SelectedRelicCount} relics" : "—");
            details.ReadOnly(null, "Barrier", () => Connected ? $"{controller.BarrierValue:0} / {controller.BarrierCapacity:0}" : "—");
            details.ReadOnly(null, "Pools", () => Connected ? $"{controller.ActiveProjectileCount} projectiles · {controller.ActivePickupCount} pickups" : "—");
            controls = Controls.Panel("survivors-tuning", "Tuning"); scroll.Add(controls);
            tuning = new DeucarianEditorWorkspaceForm(controls);
            tuning.IntegerSlider("survivors-experience", "Grant XP", 1, 250, () => experience, value => experience = value);
            tuning.IntegerSlider("survivors-burst", "Enemy burst", 1, 128, () => burst, value => burst = value);
            controls.Add(Controls.Divider());
            tuning.Root.Add(Controls.EndActions(
                Controls.Button("Force level-up", () => Execute(value => value.ForceLevelUp())),
                Controls.Button("Spawn enemy burst", () => Execute(value => value.DebugSpawnEnemyBurst(role, burst, radius))),
                Controls.Button("Grant XP", () => Execute(value => value.DebugGrantExperience(experience)), true)));
            var run = tuning.Section("Run controls", true);
            run.Root.Add(Controls.Actions(
                Controls.Button("Start standard run", () => Execute(value => value.SelectStandardRun())),
                Controls.Button("Start sprint run", () => Execute(value => value.SelectSprintRun())),
                Controls.Button("Choose run mode", () => Execute(value => value.OpenRunModeSelection()))));
            var enemies = tuning.Section("Enemies & threats", true);
            EnumChoice(enemies, "Enemy role", () => role, value => role = value);
            enemies.Slider("survivors-radius", "Spawn radius", 2, 24, () => radius, value => radius = value);
            enemies.IntegerSlider("survivors-fill", "Fill target", 1, 512, () => fill, value => fill = value);
            enemies.Action(null, "Fill arena to target", () => Execute(value => value.DebugFillArenaToTarget(role, fill, radius)));
            enemies.Root.Add(Controls.Actions(Controls.Button("Trigger horde rush", () => Execute(value => value.DebugTriggerHordeRush())),
                Controls.Button("Clear horde rush", () => Execute(value => value.DebugClearActiveHordeRush()))));
            enemies.Slider("survivors-major-radius", "Major enemy radius", 2, 24, () => majorRadius, value => majorRadius = value);
            enemies.Root.Add(Controls.Actions(Controls.Button("Force elite", () => Execute(value => value.DebugSpawnElite(majorRadius))),
                Controls.Button("Force dread elite", () => Execute(value => value.DebugSpawnDreadElite(majorRadius)))));
            enemies.Root.Add(Controls.Actions(Controls.Button("Force miniboss", () => Execute(value => value.DebugSpawnMiniboss(majorRadius))),
                Controls.Button("Force boss", () => Execute(value => value.DebugSpawnBoss(majorRadius))),
                Controls.Button("Force sprint boss", () => Execute(value => value.DebugSpawnSprintBoss(majorRadius)))));
            var advanced = tuning.Section("Advanced testing", true);
            advanced.IntegerSlider("survivors-shards", "Blood shards", 1, 500, () => shards, value => shards = value);
            advanced.Action(null, "Grant blood shards", () => Execute(value => value.DebugGrantBloodShards(shards)));
            advanced.IntegerSlider("survivors-stress", "Stress target", 50, 512, () => stress, value => stress = value);
            advanced.Action(null, "Apply stress profile", () => Execute(value => value.DebugApplyStressProfile(stress)));
            EnumChoice(advanced, "Pacing profile", () => pacing, value => pacing = value);
            advanced.Action(null, "Apply pacing and restart", () => Execute(value => value.DebugApplyPacingProfile(pacing)));
            advanced.Action(null, "Trigger magnet recall", () => Execute(value => value.TriggerMagnetRecall()));
            advanced.Root.Add(Controls.Button("Reset save / progress", () =>
            {
                if (Connected && EditorUtility.DisplayDialog("Reset Survivors progress?", "This removes this template's saved progression.", "Reset progress", "Cancel"))
                    Execute(value => value.DebugResetMetaProgression());
            }, DeucarianEditorButtonRole.Destructive));
            var reports = snapshot.Section("Runtime reports", true);
            reports.ReadOnly(null, "Run metrics", () => Describe(value => value.DebugDescribeRunMetrics()));
            reports.ReadOnly(null, "Current build", () => Describe(value => value.DebugDescribeCurrentBuild(), true));
            reports.ReadOnly(null, "Eligible evolutions", () => Describe(value => value.DebugDescribeEligibleEvolutionPool(), true));
            reports.ReadOnly(null, "Draft pool", () => Describe(value => value.DebugDescribeCurrentDraftPool(), true));
            values.BringToFront();
            Page = new DeucarianEditorPage(root, activate: _ => Refresh(), update: _ =>
            {
                if (EditorApplication.timeSinceStartup < nextUpdate) return;
                nextUpdate = EditorApplication.timeSinceStartup + .25; Refresh();
            }, dispose: workspace.Dispose);
            Refresh();
        }
        private void Refresh()
        {
            if (controller == null && Application.isPlaying)
            {
                var candidates = Resources.FindObjectsOfTypeAll<SurvivorsTemplateController>()
                    .Where(value => value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded && !EditorUtility.IsPersistent(value)).ToArray();
                if (candidates.Length == 1) controller = candidates[0];
            }
            state.text = Connected ? "Run " + controller.State.ToString().ToLowerInvariant() : "No active run";
            state.tooltip = Connected ? controller.gameObject.name : Application.isPlaying
                ? "Choose a scene controller." : "Start Play Mode with a Survivors scene.";
            sessionTime.text = Connected ? "Time " + controller.RunTimeSeconds.ToString("0") + " s" : "Start Play Mode with a Survivors scene.";
            sessionEnemies.text = Connected ? "Enemies " + controller.ActiveEnemyCount : string.Empty;
            scope.Refresh(); snapshot.Refresh(); tuning.Refresh(); controls.SetEnabled(Connected);
        }
        private void Execute(Action<SurvivorsTemplateController> action) { if (Connected) action(controller); Refresh(); }
        private string Describe(Func<SurvivorsTemplateController, IReadOnlyList<string>> capture, bool needsRun = false) =>
            !Connected || needsRun && !controller.IsRunStarted ? "No active run" : string.Join("\n", capture(controller) ?? Array.Empty<string>());
        private static void EnumChoice<T>(DeucarianEditorWorkspaceForm form, string label, Func<T> read, Action<T> write) where T : struct, Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            form.Choice(null, label, values.Select(value => ObjectNames.NicifyVariableName(value.ToString())).ToArray(),
                () => Array.IndexOf(values, read()), index => write(values[index]));
        }
    }
}
