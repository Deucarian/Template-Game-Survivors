using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Coordinates camera-aware spawn policy and owns its last-result diagnostics.</summary>
    internal sealed class SurvivorsSpawnSafety
    {
        private readonly ISurvivorsSpawnSafetyPort _port;
        private string _lastGameplaySpawnSafetyFailure = string.Empty;
        private Vector3 _lastGameplaySpawnPosition = Vector3.zero;
        private float _lastGameplaySpawnPadding;
        private bool _lastGameplaySpawnWasInsideCameraViewport;
        internal int GameplayEnemySpawnSafetyCheckCountForTest { get; private set; }
        internal int GameplaySpawnInsideCameraViewViolationCountForTest { get; private set; }
        internal string LastGameplaySpawnSafetyFailureForTest => _lastGameplaySpawnSafetyFailure;
        internal Vector3 LastGameplaySpawnPositionForTest => _lastGameplaySpawnPosition;
        internal float LastGameplaySpawnPaddingForTest => _lastGameplaySpawnPadding;
        internal bool LastGameplaySpawnWasInsideCameraViewportForTest => _lastGameplaySpawnWasInsideCameraViewport;

        internal SurvivorsSpawnSafety(ISurvivorsSpawnSafetyPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal void ResetDiagnostics()
        {
            GameplayEnemySpawnSafetyCheckCountForTest = 0;
            GameplaySpawnInsideCameraViewViolationCountForTest = 0;
            _lastGameplaySpawnSafetyFailure = string.Empty;
            _lastGameplaySpawnPosition = Vector3.zero;
            _lastGameplaySpawnPadding = 0f;
            _lastGameplaySpawnWasInsideCameraViewport = false;
        }

        internal Vector3 ResolveRuntimeEnemySpawnPositionForResolver(long seed)
        {
            return ResolveSafeOffscreenPosition(
                _port.PlayerPosition,
                ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole.Swarm, _port.Tuning.EnemySpawnRadius),
                ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole.Swarm, _port.Tuning.EnemySpawnRadius, _port.Tuning.EnemySpawnRadius + _port.Tuning.SpawnBandDepth),
                seed,
                ResolveOffscreenSpawnPadding(SurvivorsEnemyRole.Swarm, "radial-resolver"),
                _port.Tuning.SpawnBandDepth);
        }

        internal bool IsWorldPositionInsideCameraViewportForTest(Vector3 position, float padding)
        {
            return _port.TryResolveCameraGroundRect(Mathf.Max(0f, padding), out Rect visibleRect) &&
                SurvivorsOffscreenSpawnPolicy.ContainsGroundPoint(visibleRect, position);
        }

        internal Vector3 ResolveSafeOffscreenPosition(Vector3 center, float minimumDistance, float maximumDistance, long seed)
        {
            return ResolveSafeOffscreenPosition(
                center,
                minimumDistance,
                maximumDistance,
                seed,
                _port.Tuning.OffscreenSpawnPadding,
                _port.Tuning.SpawnBandDepth);
        }

        internal Vector3 ResolveSafeOffscreenPosition(
            Vector3 center, float minimumDistance, float maximumDistance, long seed, float padding, float bandDepth)
        {
            Rect? visible = _port.TryResolveCameraGroundRect(Mathf.Max(0f, padding), out Rect rect) ? rect : (Rect?)null;
            return SurvivorsOffscreenSpawnPolicy.Resolve(center, minimumDistance, maximumDistance, seed, bandDepth, visible);
        }

        internal float ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole role, float requested)
        {
            float minimum = Mathf.Max(1f, requested);
            if (SurvivorsEnemyRosterQueries.IsMajorRewardRole(role))
            {
                minimum = Mathf.Max(minimum, _port.Tuning.MajorThreatCatchUpRadius);
            }

            return minimum;
        }

        internal float ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole role, float minimumDistance, float requestedMaximum)
        {
            float band = Mathf.Max(0.25f, _port.Tuning.SpawnBandDepth);
            float maximum = Mathf.Max(minimumDistance + band, requestedMaximum);
            if (SurvivorsEnemyRosterQueries.IsMajorRewardRole(role))
            {
                maximum = Mathf.Max(maximum, _port.Tuning.MajorThreatCatchUpRadius + band);
            }

            return maximum;
        }

        internal float ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string spawnSource)
        {
            if (string.Equals(spawnSource, "normal-recycle", StringComparison.Ordinal))
            {
                return Mathf.Max(0.1f, _port.Tuning.RecycledEnemyOffscreenSpawnPadding);
            }

            return SurvivorsEnemyRosterQueries.IsMajorRewardRole(role)
                ? Mathf.Max(0.1f, _port.Tuning.MajorThreatOffscreenSpawnPadding)
                : Mathf.Max(0.1f, _port.Tuning.OffscreenSpawnPadding);
        }

        internal void RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string spawnSource)
        {
            float padding = ResolveOffscreenSpawnPadding(role, spawnSource);
            GameplayEnemySpawnSafetyCheckCountForTest++;
            _lastGameplaySpawnPosition = position;
            _lastGameplaySpawnPadding = padding;
            _lastGameplaySpawnWasInsideCameraViewport = IsWorldPositionInsideCameraViewportForTest(position, padding);
            if (!_lastGameplaySpawnWasInsideCameraViewport)
            {
                return;
            }

            GameplaySpawnInsideCameraViewViolationCountForTest++;
            string source = string.IsNullOrWhiteSpace(spawnSource) ? "runtime" : spawnSource;
            _lastGameplaySpawnSafetyFailure = $"{source} spawned {role} inside camera viewport at {position}";
        }
    }
}
