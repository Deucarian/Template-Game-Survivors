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
    // Owned world and camera lifetime plus arena presentation bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private SurvivorsRuntimeCamera RuntimeCamera => _runtimeCamera ?? (_runtimeCamera = new SurvivorsRuntimeCamera());

        private void BuildRuntimeWorld()
        {
            _runtimeWorld = new SurvivorsRuntimeWorld(transform, SurvivorsRuntimeWorldPalette.Capture(ActiveUiTheme));
            _poseResolver = new SurvivorsSpawnPoseResolver(this);
            _runtimeWorld.BuildSpawning(_poseResolver, CurrentTuning.EnemyMaximumAlive);
            if (buildVisualsOnAwake)
            {
                Arena.Build(_worldRoot);
                UpdateArenaPresentation();
            }
            BuildFeedbackPresentation();
            EnsureCamera();
        }

        private void EnsureCamera() => RuntimeCamera.Ensure(PlayerPosition, Camera.main);

        private void ApplyWorldPresentation()
        {
            if (_worldRoot == null) return;
            _runtimeWorld.ApplyPalette(SurvivorsRuntimeWorldPalette.Capture(ActiveUiTheme));
            Arena.ApplyTheme();
            _feedbackPulses?.ApplyTheme(ActiveUiTheme);
        }

        private SurvivorsArenaPresenter Arena => _arena ?? (_arena = new SurvivorsArenaPresenter(() => ActiveUiTheme, key => Waystones.IsDiscovered(key)));

        private bool TryResolveClosestArenaLandmark(bool ignoreDiscovered, out Vector3 closest, out float distance, out Vector3 delta) =>
            Arena.TryResolveClosestArenaLandmark(PlayerPosition, ignoreDiscovered, out closest, out distance, out delta);

        private void UpdateArenaPresentation()
        {
            if (_playerObject != null) Arena.Update(PlayerPosition, CurrentTuning.WaystoneDiscoveryRadius, RunTimeSeconds);
        }

        private void TickArenaWaystoneDiscoveries()
        {
            for (int i = 0; i < Arena.LandmarkCount; i++)
            {
                if (Arena.TryGetLandmark(i, out long key, out Vector3 position)) Waystones.TryDiscover(key, position);
            }
        }

        private Transform _worldRoot => _runtimeWorld?.Root;

        private GameObject _playerObject => _runtimeWorld?.Player;

        private Camera _camera => _runtimeCamera?.Camera;

        private WorldSpawnService _spawnService => _runtimeWorld?.Spawning;

        public int InfiniteArenaTileCountForTest => Arena.TileCount;

        public int InfiniteArenaLandmarkCountForTest => Arena.LandmarkCount;

        public Vector3 ArenaPresentationCenterForTest => Arena.Center;

        public Vector3 FirstInfiniteArenaTilePositionForTest => Arena.FirstTilePosition;

        public Vector3 FirstInfiniteArenaLandmarkPositionForTest => Arena.FirstLandmarkPosition;

        public Vector3 ClosestInfiniteArenaLandmarkPositionForTest => ResolveClosestArenaLandmarkPositionForTest();

        public bool IsWaystoneCompassArrowVisibleForTest => Arena.CompassVisible;

        public Vector3 WaystoneCompassArrowForwardForTest => Arena.CompassForward;

        private Vector3 ResolveClosestArenaLandmarkPositionForTest()
        {
            return TryResolveClosestArenaLandmark(ignoreDiscovered: false, out Vector3 closest, out _, out _)
                ? closest
                : Vector3.zero;
        }

        internal Transform RuntimeWorldRoot => _worldRoot;

        private void RecordRoamingArenaTravel(Vector3 delta) => Traversal.RecordTravel(delta, State == SurvivorsRunState.Playing);

        private bool TryResolveCameraGroundRect(float padding, out Rect rect)
        {
            return SurvivorsCameraGroundProjection.TryResolve(_camera != null ? _camera : Camera.main, PlayerPosition.y, padding, out rect);
        }

        private static string ResolveCompassDirectionLabel(Vector3 delta) => SurvivorsThreatHudModel.CompassDirection(delta);
    }
}
