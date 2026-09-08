using System;

using System.Collections.Generic;

using Deucarian.Common;

using Deucarian.Combat;

using Deucarian.GameplayFoundation;

using Deucarian.Persistence;

using Deucarian.Persistence.Unity;

using Deucarian.Projectiles;

using Deucarian.RunUpgrades;

using Deucarian.WeaponSystems;

using Deucarian.WorldSpawning;

using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    // Swarm, timed threat, horde and support ability bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private string ResolveRunPhaseHudLabel() => SurvivorsRunText.FormatPhase(IsEndlessRun, RunPhase);

        internal static bool IsMajorThreatSlamRole(SurvivorsEnemyRole role) => SurvivorsMajorThreatAbilities.IsMajorThreatSlamRole(role);

        internal void RecordMajorThreatSlamTelegraph(SurvivorsEnemyActor enemy) => MajorThreatAbilities.RecordMajorThreatSlamTelegraph(enemy);

        internal void ResolveMajorThreatSlam(SurvivorsEnemyActor enemy) => MajorThreatAbilities.ResolveMajorThreatSlam(enemy);

        private void TryTriggerMajorThreatEnrage(SurvivorsEnemyActor enemy) => MajorThreatAbilities.TryTriggerMajorThreatEnrage(enemy);

        private void SpawnSplitterChildren(Vector3 position, string splitterName) => EnemySupportSpawning.SpawnSplitterChildren(position, splitterName);

        internal int SpawnSummonerSupport(SurvivorsEnemyActor summoner) => EnemySupportSpawning.SpawnSummonerSupport(summoner);

        private SurvivorsEnemySupportSpawning EnemySupportSpawning => _enemySupportSpawning ?? (_enemySupportSpawning = new SurvivorsEnemySupportSpawning(this));

        private SurvivorsMajorThreatAbilities MajorThreatAbilities => _majorThreatAbilities ?? (_majorThreatAbilities = new SurvivorsMajorThreatAbilities(this, EnemySupportSpawning));

        SurvivorsTemplateTuning ISurvivorsEnemySupportSpawnPort.Tuning => CurrentTuning;

        SurvivorsRunState ISurvivorsEnemySupportSpawnPort.State => State;

        int ISurvivorsEnemySupportSpawnPort.MaximumAlive => ResolveEnemyMaximumAlive();

        int ISurvivorsEnemySupportSpawnPort.EnemyCount => _enemies.Count;

        long ISurvivorsEnemySupportSpawnPort.SpawnSequence => _spawnSequence;

        SurvivorsEnemyActor ISurvivorsEnemySupportSpawnPort.SpawnGameplayEnemyOffscreen(SurvivorsEnemyRole role, long seed, float minimumDistance, float maximumDistance, string spawnSource) => SpawnGameplayEnemyOffscreen(role, seed, minimumDistance, maximumDistance, spawnSource);

        void ISurvivorsEnemySupportSpawnPort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        void ISurvivorsEnemySupportSpawnPort.PlaySupportSpawnFeedback(Vector3 position, int burstCount) => PlayFeedback(_spawnPulse, position, burstCount, _spawnClip);

        SurvivorsTemplateTuning ISurvivorsMajorThreatAbilityPort.Tuning => CurrentTuning;

        SurvivorsRunState ISurvivorsMajorThreatAbilityPort.State => State;

        Vector3 ISurvivorsMajorThreatAbilityPort.PlayerPosition => PlayerPosition;

        float ISurvivorsMajorThreatAbilityPort.CurrentHealth => CurrentHealth;

        float ISurvivorsMajorThreatAbilityPort.BarrierValue => BarrierValue;

        bool ISurvivorsMajorThreatAbilityPort.IsMajorRewardRole(SurvivorsEnemyRole role) => IsMajorRewardRole(role);

        string ISurvivorsMajorThreatAbilityPort.ResolveMajorThreatHealthFallbackLabel(SurvivorsEnemyRole role) => ResolveMajorThreatHealthFallbackLabel(role);

        void ISurvivorsMajorThreatAbilityPort.ApplyDamageToPlayer(float amount, string source) => ApplyDamageToPlayer(amount, source);

        void ISurvivorsMajorThreatAbilityPort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        void ISurvivorsMajorThreatAbilityPort.RecordMajorThreatSlamTelegraphEffect(Vector3 position, SurvivorsEnemyRole role, float radius, float durationSeconds) => RecordMajorThreatSlamTelegraphEffect(position, role, radius, durationSeconds);

        void ISurvivorsMajorThreatAbilityPort.PlayBossFeedback(Vector3 position, int burstCount) => PlayFeedback(_bossPulse, position, burstCount, _dangerClip);

        private int CountEliteEnemies() => EnemyRosterQueries.CountEliteEnemies();

        private static bool IsEliteRole(SurvivorsEnemyRole role) => SurvivorsEnemyRosterQueries.IsEliteRole(role);

        private SurvivorsHordeRushEncounter HordeRush => _hordeRush ?? (_hordeRush = new SurvivorsHordeRushEncounter(this));

        SurvivorsTemplateTuning ISurvivorsHordeRushPort.Tuning => CurrentTuning;

        float ISurvivorsHordeRushPort.RunTime => RunTimeSeconds;

        int ISurvivorsHordeRushPort.Escalation => RunEscalationLevel;

        int ISurvivorsHordeRushPort.MaximumAlive => ResolveEnemyMaximumAlive();

        long ISurvivorsHordeRushPort.SpawnSequence => _spawnSequence;

        long ISurvivorsHordeRushPort.SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum) =>
            SpawnGameplayEnemyOffscreen(role, seed, minimum, maximum, "horde-rush")?.InstanceId.Value ?? 0;

        bool ISurvivorsHordeRushPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount) != null;

        int ISurvivorsHordeRushPort.DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) =>
            DamageNonMajorEnemies(position, radius, damage, source);

        void ISurvivorsHordeRushPort.ShowWarning(string label, float radius, float remaining)
        {
            RecordIncomingThreatTelegraph(PlayerPosition, SurvivorsEnemyRole.Runner, label, radius, remaining);
            PlayFeedback(_bossPulse, PlayerPosition, 22, _dangerClip);
        }

        void ISurvivorsHordeRushPort.ShowBurst(string label)
        {
            RecordStreakRewardFeedback(label, new Color(1f, 0.42f, 0.18f));
            PlayFeedback(_bossPulse, PlayerPosition, 28, _dangerClip);
        }

        void ISurvivorsHordeRushPort.ShowClear(Vector3 position, string label, int hitCount)
        {
            RecordStreakRewardFeedback(label, new Color(1f, 0.74f, 0.24f));
            PlayFeedback(_levelUpPulse, position, Mathf.Clamp(24 + hitCount * 5, 28, 78), _pickupClip);
        }

        private SurvivorsTimedEncounterDirector TimedEncounters => _timedEncounters ??
            (_timedEncounters = new SurvivorsTimedEncounterDirector(this));

        SurvivorsRunFlowRuntime ISurvivorsTimedEncounterPort.RunFlow => _runFlow;

        SurvivorsTemplateTuning ISurvivorsTimedEncounterPort.Tuning => CurrentTuning;

        float ISurvivorsTimedEncounterPort.RunTime => RunTimeSeconds;

        bool ISurvivorsTimedEncounterPort.IsEndlessPlaying => IsEndlessRun;

        bool ISurvivorsTimedEncounterPort.TrySpawn(SurvivorsEnemyRole role, string source) =>
            SpawnEnemy(Vector3.zero, explicitPosition: false, role, gameplaySpawn: true, spawnSource: source) != null;

        void ISurvivorsTimedEncounterPort.EnterVictory() => EnterVictory();

        void ISurvivorsTimedEncounterPort.ShowWarning(SurvivorsEnemyRole role, string label, float remainingSeconds)
        {
            RecordIncomingThreatTelegraph(PlayerPosition, role, label, ResolveIncomingThreatTelegraphRadius(role), remainingSeconds);
            PlayFeedback(_bossPulse, PlayerPosition, role == SurvivorsEnemyRole.Boss ? 44 : 28, _dangerClip,
                role == SurvivorsEnemyRole.Boss ? AudioEventBossWarning : AudioEventEliteWarning, 0.35f);
        }

        private SurvivorsTraversalDirector Traversal => _traversal ?? (_traversal = new SurvivorsTraversalDirector(this));

        SurvivorsTemplateTuning ISurvivorsTraversalPort.Tuning => CurrentTuning;

        int ISurvivorsTraversalPort.ActiveShrineEnemyCount => ShrineTrials.ActiveCount;

        void ISurvivorsTraversalPort.SpawnShrine(Vector3 direction) => ShrineTrials.SpawnArenaShrineTrial(direction);

        void ISurvivorsTraversalPort.SpawnCache(Vector3 direction, int sequenceOffset) => RoamingCaches.SpawnRoamingArenaCache(direction, sequenceOffset);

        public int SplitterChildSpawnCount => EnemySupportSpawning.SplitterChildSpawnCount;

        public int SplitterSplitFeedbackCount => EnemySupportSpawning.SplitterSplitFeedbackCount;

        public string LastSplitterSplitFeedbackLabel => EnemySupportSpawning.LastSplitterSplitFeedbackLabel;

        public int SummonerSupportSpawnCount => EnemySupportSpawning.SummonerSupportSpawnCount;

        public int SummonerSupportFeedbackCount => EnemySupportSpawning.SummonerSupportFeedbackCount;

        public string LastSummonerSupportFeedbackLabel => EnemySupportSpawning.LastSummonerSupportFeedbackLabel;

        public int EndlessThreatSpawnCount => TimedEncounters.EndlessThreatSpawnCount;

        public int HordeRushSpawnCount => HordeRush.HordeRushSpawnCount;

        public int HordeRushEnemySpawnCount => HordeRush.HordeRushEnemySpawnCount;

        public int HordeRushWarningCount => HordeRush.HordeRushWarningCount;

        public int HordeRushClearRewardCount => HordeRush.HordeRushClearRewardCount;

        public int HordeRushClearExperienceGemDropCount => HordeRush.HordeRushClearExperienceGemDropCount;

        public int HordeRushClearSpecialDropCount => HordeRush.HordeRushClearSpecialDropCount;

        public int HordeRushClearPulseCount => HordeRush.HordeRushClearPulseCount;

        public int HordeRushClearPulseHitCount => HordeRush.HordeRushClearPulseHitCount;

        public int HordeRushClearSurgeActivationCount => HordeRush.HordeRushClearSurgeActivationCount;

        public string LastHordeRushFeedbackLabel => HordeRush.LastHordeRushFeedbackLabel;

        public string LastHordeRushClearFeedbackLabel => HordeRush.LastHordeRushClearFeedbackLabel;

        public string LastHordeRushClearPulseFeedbackLabel => HordeRush.LastHordeRushClearPulseFeedbackLabel;

        public int MajorThreatWarningCount => TimedEncounters.MajorThreatWarningCount;

        public int MajorThreatEnrageCount => MajorThreatAbilities.MajorThreatEnrageCount;

        public int MajorThreatEnrageSupportSpawnCount => MajorThreatAbilities.MajorThreatEnrageSupportSpawnCount;

        public string LastMajorThreatEnrageFeedbackLabel => MajorThreatAbilities.LastMajorThreatEnrageFeedbackLabel;

        public int MajorThreatSlamWarningCount => MajorThreatAbilities.MajorThreatSlamWarningCount;

        public int MajorThreatSlamCastCount => MajorThreatAbilities.MajorThreatSlamCastCount;

        public int MajorThreatSlamHitCount => MajorThreatAbilities.MajorThreatSlamHitCount;

        public string LastMajorThreatSlamFeedbackLabel => MajorThreatAbilities.LastMajorThreatSlamFeedbackLabel;

        public bool IsHordeRushClearSurgeActive => HordeRush.IsHordeRushClearSurgeActive;

        public float HordeRushClearSurgeRemainingSeconds => HordeRush.HordeRushClearSurgeRemainingSeconds;

        public float HordeRushClearSurgeDamageBonus => HordeRush.HordeRushClearSurgeDamageBonus;

        public float HordeRushClearSurgeMoveSpeedBonus => HordeRush.HordeRushClearSurgeMoveSpeedBonus;

        public float HordeRushClearSurgeCooldownMultiplierBonus => HordeRush.HordeRushClearSurgeCooldownMultiplierBonus;

        public float HordeRushClearSurgePickupRangeBonus => HordeRush.HordeRushClearSurgePickupRangeBonus;

        public int ActiveSplitterCount => CountEnemiesByRole(SurvivorsEnemyRole.Splitter);

        public int ActiveSummonerCount => CountEnemiesByRole(SurvivorsEnemyRole.Summoner);

        public int ActiveEliteCount => CountEliteEnemies();

        public int ActiveDreadEliteCount => CountEnemiesByRole(SurvivorsEnemyRole.DreadElite);

        public int ActiveMinibossCount => CountEnemiesByRole(SurvivorsEnemyRole.Miniboss);

        public int ActiveMajorThreatLifeBarCount => ActiveBossLifeBarCount + ActiveOverheadLifeBarCount;

        public string ActiveMajorThreatLifeBarSummary => ResolveActiveMajorThreatLifeBarSummary();

        public float CurrentMajorThreatHealthFraction => Mathf.Clamp01(ThreatHud.SelectHealthThreat()?.HealthFraction ?? 0f);

        public int ActiveHordeRushEnemyCount => HordeRush.ActiveCount;

        public SurvivorsRunFlowDefinition CurrentRunFlowDefinition => _runFlow == null ? null : _runFlow.Definition;

        public float CurrentEnemySpeedMultiplier => _runFlow == null ? 1f : _runFlow.ResolveEnemySpeedMultiplier();

        public SurvivorsRunPhase RunPhase => _runFlow == null ? SurvivorsRunPhase.Opening : _runFlow.Phase;

        public int RunEscalationLevel => _runFlow == null ? 0 : _runFlow.EscalationLevel;

        public bool IsMajorThreatWarningActive => !string.IsNullOrEmpty(TimedEncounters.WarningLabel) && RunTimeSeconds < TimedEncounters.WarningTargetTime;

        public string CurrentMajorThreatWarningLabel => IsMajorThreatWarningActive ? TimedEncounters.WarningLabel : string.Empty;

        public float MajorThreatWarningRemainingSeconds => IsMajorThreatWarningActive ? Mathf.Max(0f, TimedEncounters.WarningTargetTime - RunTimeSeconds) : 0f;

        public bool IsHordeRushWarningActive => HordeRush.WarningActive;

        public string CurrentHordeRushWarningLabel => HordeRush.WarningLabel;

        public float HordeRushWarningRemainingSeconds => HordeRush.WarningRemaining;

        public float NextHordeRushTimeSecondsForTest => HordeRush.NextTime;

        public float FirstEliteSpawnTimeSeconds => Telemetry.FirstEliteSpawnTimeSeconds;

        public float FirstEliteKillTimeSeconds => Telemetry.FirstEliteKillTimeSeconds;

        public float FirstMinibossSpawnTimeSeconds => Telemetry.FirstMinibossSpawnTimeSeconds;

        public float FirstMinibossKillTimeSeconds => Telemetry.FirstMinibossKillTimeSeconds;

        public float FirstBossSpawnTimeSeconds => Telemetry.FirstBossSpawnTimeSeconds;

        public int KillActiveHordeRushEnemiesForTest()
        {
            return DebugClearActiveHordeRush();
        }

        public SurvivorsEnemyActor DebugSpawnElite(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.Elite, radius);
        }

        public SurvivorsEnemyActor DebugSpawnDreadElite(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.DreadElite, radius);
        }

        public SurvivorsEnemyActor DebugSpawnMiniboss(float radius)
        {
            return DebugSpawnMajorEnemy(SurvivorsEnemyRole.Miniboss, radius);
        }

        public int DebugTriggerHordeRush()
        {
            EnsureRunStartedForTest();
            return HordeRush.Trigger();
        }

        private static string ResolveMajorThreatHealthFallbackLabel(SurvivorsEnemyRole role) => SurvivorsThreatHudModel.ResolveFallbackLabel(role);

        private void DrawMajorThreatWarning() => SurvivorsStatusHudPresenter.DrawMajorThreat(
            IsMajorThreatWarningActive, IsTopCenterTimerVisible, CurrentMajorThreatWarningLabel, MajorThreatWarningRemainingSeconds, _majorThreatWarningStyle);

        private void DrawHordeRushWarning() => SurvivorsStatusHudPresenter.DrawHordeRush(
            IsHordeRushWarningActive, IsTopCenterTimerVisible, IsMajorThreatWarningActive, CurrentHordeRushWarningLabel, HordeRushWarningRemainingSeconds, _majorThreatWarningStyle);

        private void DrawMajorThreatHealthBar() => SurvivorsThreatHudPresenter.DrawLifeBars(ThreatHud, _camera,
            IsMajorThreatWarningActive, IsHordeRushWarningActive, ResolveMajorRewardDropColor, _hudSmallStyle);
    }
}
