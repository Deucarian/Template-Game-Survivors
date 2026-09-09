using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsTimedEncounterPort
    {
        SurvivorsRunFlowRuntime RunFlow { get; }
        SurvivorsTemplateTuning Tuning { get; }
        float RunTime { get; }
        bool IsEndlessPlaying { get; }
        bool TrySpawn(SurvivorsEnemyRole role, string source);
        void EnterVictory();
        void ShowWarning(SurvivorsEnemyRole role, string label, float remainingSeconds);
    }

    /// <summary>Sequences timed threats, warning windows and post-victory retries through scene ports.</summary>
    internal sealed class SurvivorsTimedEncounterDirector
    {
        private readonly ISurvivorsTimedEncounterPort _port;
        private SurvivorsRunFlowRuntime _runFlow => _port.RunFlow;
        private SurvivorsTemplateTuning CurrentTuning => _port.Tuning;
        private float RunTimeSeconds => _port.RunTime;
        private bool _firstEliteWarningShown;
        private bool _firstDreadEliteWarningShown;
        private float _timedEliteWarningTargetTimeSeconds;
        private float _timedDreadEliteWarningTargetTimeSeconds;
        private bool _minibossWarningShown;
        private bool _bossWarningShown;
        private bool _endlessEliteWarningShown;
        private bool _endlessMinibossWarningShown;
        private bool _endlessBossWarningShown;
        private float _nextEndlessEliteSpawnTimeSeconds;
        private float _nextEndlessMinibossSpawnTimeSeconds;
        private float _nextEndlessBossSpawnTimeSeconds;
        private int _endlessEliteSpawnSequence;
        private string _majorThreatWarningLabel = string.Empty;
        private float _majorThreatWarningTargetTimeSeconds;

        public SurvivorsTimedEncounterDirector(ISurvivorsTimedEncounterPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public int MajorThreatWarningCount { get; private set; }
        public int EndlessThreatSpawnCount { get; private set; }
        public string WarningLabel => _majorThreatWarningLabel;
        public float WarningTargetTime => _majorThreatWarningTargetTimeSeconds;
        public float NextEliteTime => _nextEndlessEliteSpawnTimeSeconds;
        public float NextMinibossTime => _nextEndlessMinibossSpawnTimeSeconds;
        public float NextBossTime => _nextEndlessBossSpawnTimeSeconds;

        public void Reset()
        {
            _firstEliteWarningShown = _firstDreadEliteWarningShown = _minibossWarningShown = _bossWarningShown = false;
            _timedEliteWarningTargetTimeSeconds = _timedDreadEliteWarningTargetTimeSeconds = 0f;
            _majorThreatWarningLabel = string.Empty;
            _majorThreatWarningTargetTimeSeconds = 0f;
            MajorThreatWarningCount = EndlessThreatSpawnCount = 0;
            ResetEndlessThreatSchedule();
        }

        public void TickRunFlow()
        {
            if (_runFlow == null)
            {
                return;
            }

            _runFlow.Tick(RunTimeSeconds);
            while (_runFlow.TryConsumeTimedEliteSpawn(RunTimeSeconds, out SurvivorsEnemyRole timedEliteRole))
            {
                _port.TrySpawn(timedEliteRole, "timed-elite");
            }

            if (_runFlow.TryConsumeMinibossSpawn(RunTimeSeconds))
            {
                _port.TrySpawn(SurvivorsEnemyRole.Miniboss, "timed-miniboss");
            }

            if (_runFlow.TryConsumeBossSpawn(RunTimeSeconds))
            {
                _port.TrySpawn(SurvivorsEnemyRole.Boss, "timed-boss");
            }

            if (_runFlow.TryConsumeSurvivalVictory(RunTimeSeconds))
            {
                _port.EnterVictory();
            }

            TickEndlessThreatSpawns();
        }

        public void TickMajorThreatWarnings()
        {
            if (_runFlow == null || _runFlow.Definition == null)
            {
                return;
            }

            SurvivorsRunFlowDefinition definition = _runFlow.Definition;
            TryBeginRecurringMajorThreatWarning(ref _firstEliteWarningShown, ref _timedEliteWarningTargetTimeSeconds, _runFlow.NextEliteSpawnTimeSeconds, SurvivorsEnemyRole.Elite);
            TryBeginRecurringMajorThreatWarning(ref _firstDreadEliteWarningShown, ref _timedDreadEliteWarningTargetTimeSeconds, _runFlow.NextDreadEliteSpawnTimeSeconds, SurvivorsEnemyRole.DreadElite);
            TryBeginMajorThreatWarning(ref _minibossWarningShown, definition.MinibossSpawnTimeSeconds, SurvivorsEnemyRole.Miniboss);
            TryBeginMajorThreatWarning(ref _bossWarningShown, definition.BossSpawnTimeSeconds, SurvivorsEnemyRole.Boss);
            if (_port.IsEndlessPlaying)
            {
                TryBeginMajorThreatWarning(ref _endlessEliteWarningShown, _nextEndlessEliteSpawnTimeSeconds, ResolveNextEndlessEliteRole());
                TryBeginMajorThreatWarning(ref _endlessMinibossWarningShown, _nextEndlessMinibossSpawnTimeSeconds, SurvivorsEnemyRole.Miniboss);
                TryBeginMajorThreatWarning(ref _endlessBossWarningShown, _nextEndlessBossSpawnTimeSeconds, SurvivorsEnemyRole.Boss);
            }
        }

        private void TryBeginRecurringMajorThreatWarning(ref bool warningShown, ref float warningTargetTimeSeconds, float targetTimeSeconds, SurvivorsEnemyRole role)
        {
            if (targetTimeSeconds <= 0f)
            {
                warningShown = false;
                warningTargetTimeSeconds = 0f;
                return;
            }

            if (!Mathf.Approximately(warningTargetTimeSeconds, targetTimeSeconds))
            {
                warningShown = false;
                warningTargetTimeSeconds = targetTimeSeconds;
            }

            TryBeginMajorThreatWarning(ref warningShown, targetTimeSeconds, role);
        }

        private void TryBeginMajorThreatWarning(ref bool warningShown, float targetTimeSeconds, SurvivorsEnemyRole role)
        {
            float leadSeconds = Mathf.Max(0f, CurrentTuning.MajorThreatWarningLeadSeconds);
            if (warningShown || leadSeconds <= 0f || targetTimeSeconds <= 0f)
            {
                return;
            }

            float warningTime = Mathf.Max(0f, targetTimeSeconds - leadSeconds);
            if (RunTimeSeconds < warningTime || RunTimeSeconds >= targetTimeSeconds)
            {
                return;
            }

            warningShown = true;
            _majorThreatWarningLabel = ResolveMajorThreatWarningLabel(role);
            _majorThreatWarningTargetTimeSeconds = targetTimeSeconds;
            MajorThreatWarningCount++;
            _port.ShowWarning(role, _majorThreatWarningLabel, targetTimeSeconds - RunTimeSeconds);
        }

        public static string ResolveMajorThreatWarningLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.DreadElite:
                    return "DREAD ELITE INCOMING";
                case SurvivorsEnemyRole.Miniboss:
                    return "MINIBOSS INCOMING";
                case SurvivorsEnemyRole.Boss:
                    return "FINAL BOSS INCOMING";
                default:
                    return "ELITE INCOMING";
            }
        }

        public void ResetEndlessThreatSchedule()
        {
            _endlessEliteWarningShown = false;
            _endlessMinibossWarningShown = false;
            _endlessBossWarningShown = false;
            _nextEndlessEliteSpawnTimeSeconds = 0f;
            _nextEndlessMinibossSpawnTimeSeconds = 0f;
            _nextEndlessBossSpawnTimeSeconds = 0f;
            _endlessEliteSpawnSequence = 0;
        }

        public void ScheduleEndlessThreats(float startTimeSeconds)
        {
            _endlessEliteSpawnSequence = 0;
            ScheduleNextEndlessThreat(
                ref _nextEndlessEliteSpawnTimeSeconds,
                ref _endlessEliteWarningShown,
                startTimeSeconds,
                CurrentTuning.EndlessEliteSpawnIntervalSeconds);
            ScheduleNextEndlessThreat(
                ref _nextEndlessMinibossSpawnTimeSeconds,
                ref _endlessMinibossWarningShown,
                startTimeSeconds,
                CurrentTuning.EndlessMinibossSpawnIntervalSeconds);
            ScheduleNextEndlessThreat(
                ref _nextEndlessBossSpawnTimeSeconds,
                ref _endlessBossWarningShown,
                startTimeSeconds,
                CurrentTuning.EndlessBossSpawnIntervalSeconds);
        }

        private static void ScheduleNextEndlessThreat(
            ref float targetTimeSeconds,
            ref bool warningShown,
            float startTimeSeconds,
            float intervalSeconds)
        {
            warningShown = false;
            targetTimeSeconds = intervalSeconds <= 0f
                ? 0f
                : Mathf.Max(0f, startTimeSeconds) + Mathf.Max(0.1f, intervalSeconds);
        }

        private void TickEndlessThreatSpawns()
        {
            if (!_port.IsEndlessPlaying)
            {
                return;
            }

            SurvivorsEnemyRole eliteRole = ResolveNextEndlessEliteRole();
            if (TrySpawnEndlessThreat(eliteRole, _nextEndlessEliteSpawnTimeSeconds))
            {
                _endlessEliteSpawnSequence++;
                ScheduleNextEndlessThreat(
                    ref _nextEndlessEliteSpawnTimeSeconds,
                    ref _endlessEliteWarningShown,
                    RunTimeSeconds,
                    CurrentTuning.EndlessEliteSpawnIntervalSeconds);
            }

            if (TrySpawnEndlessThreat(SurvivorsEnemyRole.Miniboss, _nextEndlessMinibossSpawnTimeSeconds))
            {
                ScheduleNextEndlessThreat(
                    ref _nextEndlessMinibossSpawnTimeSeconds,
                    ref _endlessMinibossWarningShown,
                    RunTimeSeconds,
                    CurrentTuning.EndlessMinibossSpawnIntervalSeconds);
            }

            if (TrySpawnEndlessThreat(SurvivorsEnemyRole.Boss, _nextEndlessBossSpawnTimeSeconds))
            {
                ScheduleNextEndlessThreat(
                    ref _nextEndlessBossSpawnTimeSeconds,
                    ref _endlessBossWarningShown,
                    RunTimeSeconds,
                    CurrentTuning.EndlessBossSpawnIntervalSeconds);
            }
        }

        private bool TrySpawnEndlessThreat(SurvivorsEnemyRole role, float targetTimeSeconds)
        {
            if (targetTimeSeconds <= 0f || RunTimeSeconds < targetTimeSeconds)
            {
                return false;
            }

            if (!_port.TrySpawn(role, "endless-threat"))
            {
                return false;
            }

            EndlessThreatSpawnCount++;
            return true;
        }

        public SurvivorsEnemyRole ResolveNextEndlessEliteRole()
        {
            return (_endlessEliteSpawnSequence % 2) == 0
                ? SurvivorsEnemyRole.Elite
                : SurvivorsEnemyRole.DreadElite;
        }
    }
}
