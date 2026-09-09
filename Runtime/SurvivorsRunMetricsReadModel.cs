using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Formats live run observations into one reused list; it never starts or changes the run.</summary>
    internal sealed class SurvivorsRunMetricsReadModel
    {
        private readonly ISurvivorsRunMetricsReadPort _port;
        private readonly List<string> _runMetricsLines = new List<string>(16);
        internal SurvivorsRunMetricsReadModel(ISurvivorsRunMetricsReadPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));
        internal void Clear() => _runMetricsLines.Clear();

        internal IReadOnlyList<string> Describe()
        {
            _runMetricsLines.Clear();
            _runMetricsLines.Add($"Mode {_port.ModeName} ({BasicSurvivorsGame.GetPacingProfileDisplayName(_port.PacingProfile)})");
            _runMetricsLines.Add($"Target {SurvivorsRunText.FormatMetricTime(_port.TargetDuration)} - boss {SurvivorsRunText.FormatMetricTime(_port.BossSpawnTime)} - victory {SurvivorsRunText.FormatMetricTime(_port.VictoryTime)}");
            if (!_port.Started)
            {
                _runMetricsLines.Add(_port.ModeSelectionOpen ? "Run mode selection open" : "Run not started");
                return _runMetricsLines;
            }

            ISurvivorsActiveRunMetricsReadPort active = _port.Active;
            _runMetricsLines.Add($"Runtime {SurvivorsRunText.FormatMetricTime(active.RunTimeSeconds)} - state {active.State}");
            AppendMetricTime(_runMetricsLines, "First kill", active.Telemetry.FirstKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First XP pickup", active.Telemetry.FirstExperiencePickupTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First level-up draft", active.Telemetry.FirstLevelUpDraftTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First elite spawn", active.Telemetry.FirstEliteSpawnTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First elite kill", active.Telemetry.FirstEliteKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First miniboss spawn", active.Telemetry.FirstMinibossSpawnTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First miniboss kill", active.Telemetry.FirstMinibossKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First boss spawn", active.Telemetry.FirstBossSpawnTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First boss kill", active.Telemetry.FirstBossKillTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First evolution ready", active.Telemetry.FirstEvolutionEligibilityTimeSeconds);
            AppendMetricTime(_runMetricsLines, "First evolution acquired", active.Telemetry.FirstEvolutionAcquiredTimeSeconds);
            _runMetricsLines.Add($"Levels 1m {FormatMetricLevel(active.Telemetry.LevelAtOneMinute)}, 2m {FormatMetricLevel(active.Telemetry.LevelAtTwoMinutes)}, 3m {FormatMetricLevel(active.Telemetry.LevelAtThreeMinutes)}, 4m {FormatMetricLevel(active.Telemetry.LevelAtFourMinutes)}, 5m {FormatMetricLevel(active.Telemetry.LevelAtFiveMinutes)}");
            SurvivorsRunMetricsDraftValues drafts = active.CaptureDrafts();
            _runMetricsLines.Add($"Drafts level {drafts.LevelDrafts}, total {drafts.TotalDrafts}, pending {drafts.PendingLevels}, weapons {drafts.Weapons}/{drafts.WeaponSlots}, passives {drafts.Passives}/{drafts.PassiveSlots}, evolutions {drafts.Evolutions}");
            SurvivorsRunMetricsCombatValues combat = active.CaptureCombat();
            _runMetricsLines.Add($"Kills {combat.Kills}, XP {combat.CollectedExperience}, stored {combat.StoredExperience}/{combat.NextLevelExperience}, overflow {combat.Overflow}, damage taken {combat.DamageTaken:0.#}");
            SurvivorsRunMetricsPickupValues pickups = active.CapturePickups();
            _runMetricsLines.Add($"Pickup range {pickups.Range:0.#}, pull {pickups.Speed:0.#}, pulse {SurvivorsRunText.FormatMetricTime(pickups.PulseInterval)}, markers {pickups.Markers}, recycles {pickups.NormalRecycles}, major repositions {pickups.MajorRepositions}");
            return _runMetricsLines;
        }

        private static string FormatMetricLevel(int level)
        {
            return level > 0 ? level.ToString() : "not yet";
        }

        private static void AppendMetricTime(List<string> lines, string label, float seconds)
        {
            lines.Add(label + ": " + SurvivorsRunText.FormatMetricTime(seconds));
        }
    }
}
