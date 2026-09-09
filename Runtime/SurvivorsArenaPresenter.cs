using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Common;
using static Deucarian.TemplateGameSurvivors.SurvivorsArenaGeometry;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsArenaPresenter : IDisposable
    {
        private readonly List<Transform> _arenaTiles = new List<Transform>(25);
        private readonly List<Transform> _arenaLandmarks = new List<Transform>(InfiniteArenaLandmarkCount);
        private readonly List<long> _arenaLandmarkKeys = new List<long>(InfiniteArenaLandmarkCount);
        private readonly SurvivorsArenaPrimitives _primitives = new SurvivorsArenaPrimitives();
        private readonly SurvivorsWaystoneCompassPresenter _compass;
        private readonly Func<SurvivorsUiTheme> _theme;
        private readonly Func<long, bool> _isDiscovered;
        private Transform _arenaTileRoot;
        private Transform _arenaFollowRoot;
        private SurvivorsUiTheme ActiveUiTheme => _theme();
        public SurvivorsArenaPresenter(Func<SurvivorsUiTheme> theme, Func<long, bool> isDiscovered)
        {
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _isDiscovered = isDiscovered ?? throw new ArgumentNullException(nameof(isDiscovered));
            _compass = new SurvivorsWaystoneCompassPresenter(theme, _primitives);
        }
        public int TileCount => _arenaTiles.Count;
        public int LandmarkCount => _arenaLandmarks.Count;
        public Vector3 Center => _arenaFollowRoot == null ? Vector3.zero : _arenaFollowRoot.position;
        public Vector3 FirstTilePosition => _arenaTiles.Count == 0 || _arenaTiles[0] == null ? Vector3.zero : _arenaTiles[0].position;
        public Vector3 FirstLandmarkPosition => _arenaLandmarks.Count == 0 || _arenaLandmarks[0] == null ? Vector3.zero : _arenaLandmarks[0].position;
        public bool CompassVisible => _compass.Visible;
        public Vector3 CompassForward => _compass.Forward;
        public bool TryGetLandmark(int index, out long key, out Vector3 position)
        {
            key = 0L;
            position = Vector3.zero;
            if (index < 0 || index >= _arenaLandmarks.Count || index >= _arenaLandmarkKeys.Count || _arenaLandmarks[index] == null) return false;
            key = _arenaLandmarkKeys[index];
            position = _arenaLandmarks[index].position;
            return true;
        }

        public void Build(Transform worldRoot)
        {
            Dispose();

            GameObject tileRoot = new GameObject("Survivors Infinite Arena Tiles");
            tileRoot.transform.SetParent(worldRoot, false);
            _arenaTileRoot = tileRoot.transform;
            BuildInfiniteArenaTiles();
            BuildInfiniteArenaLandmarks();

            GameObject followRoot = new GameObject("Survivors Moving Arena Readability");
            followRoot.transform.SetParent(worldRoot, false);
            _arenaFollowRoot = followRoot.transform;

            _compass.Build(_arenaFollowRoot);
        }

        private void BuildInfiniteArenaTiles()
        {
            for (int x = -InfiniteArenaGridRadius; x <= InfiniteArenaGridRadius; x++)
            {
                for (int z = -InfiniteArenaGridRadius; z <= InfiniteArenaGridRadius; z++)
                {
                    bool alternate = ((x + z) & 1) == 0;
                    Color tileColor = alternate
                        ? ActiveUiTheme.GetArenaFloorColor(new Color(0.045f, 0.055f, 0.058f))
                        : ActiveUiTheme.GetArenaGridColor(new Color(0.055f, 0.065f, 0.05f));
                    GameObject tile = _primitives.Create(
                        "Infinite Arena Tile " + x.ToString() + "," + z.ToString(),
                        PrimitiveType.Cube,
                        new Vector3(x * InfiniteArenaTileSize, -0.08f, z * InfiniteArenaTileSize),
                        new Vector3(InfiniteArenaTileSize, 0.035f, InfiniteArenaTileSize),
                        tileColor,
                        _arenaTileRoot);
                    _arenaTiles.Add(tile.transform);
                }
            }
        }

        private void BuildInfiniteArenaLandmarks()
        {
            for (int i = 0; i < InfiniteArenaLandmarkCount; i++)
            {
                GameObject landmark = _primitives.Create(
                    "Infinite Arena Waystone " + (i + 1).ToString(),
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(0.42f, 0.52f, 0.42f),
                    ResolveArenaLandmarkColor(i),
                    _arenaTileRoot);
                _arenaLandmarks.Add(landmark.transform);
                _arenaLandmarkKeys.Add(0L);
            }
        }

        public void Update(Vector3 player, float discoveryRadius, float runTime)
        {
            Vector3 playerGround = new Vector3(player.x, 0f, player.z);
            if (_arenaFollowRoot != null)
            {
                _arenaFollowRoot.position = playerGround;
            }

            if (_arenaTiles.Count == 0)
            {
                return;
            }

            float anchorX = ResolveArenaPresentationAnchor(player.x);
            float anchorZ = ResolveArenaPresentationAnchor(player.z);
            int tileIndex = 0;
            for (int x = -InfiniteArenaGridRadius; x <= InfiniteArenaGridRadius; x++)
            {
                for (int z = -InfiniteArenaGridRadius; z <= InfiniteArenaGridRadius; z++)
                {
                    if (tileIndex >= _arenaTiles.Count)
                    {
                        return;
                    }

                    Transform tile = _arenaTiles[tileIndex++];
                    if (tile != null)
                    {
                        tile.position = new Vector3(anchorX + x * InfiniteArenaTileSize, -0.08f, anchorZ + z * InfiniteArenaTileSize);
                    }
                }
            }

            UpdateArenaLandmarks(anchorX, anchorZ);
            float distance = 0f;
            Vector3 delta = Vector3.zero;
            bool visible = discoveryRadius > 0f && TryResolveClosestArenaLandmark(player, true, out _, out distance, out delta);
            _compass.Update(visible, distance, delta, discoveryRadius, runTime);
        }

        private void UpdateArenaLandmarks(float anchorX, float anchorZ)
        {
            if (_arenaLandmarks.Count == 0)
            {
                return;
            }

            int anchorCellX = Mathf.FloorToInt(anchorX / InfiniteArenaTileSize);
            int anchorCellZ = Mathf.FloorToInt(anchorZ / InfiniteArenaTileSize);
            for (int i = 0; i < _arenaLandmarks.Count; i++)
            {
                Transform landmark = _arenaLandmarks[i];
                if (landmark == null)
                {
                    continue;
                }

                ResolveArenaLandmarkOffset(i, out int offsetX, out int offsetZ);
                int cellX = anchorCellX + offsetX;
                int cellZ = anchorCellZ + offsetZ;
                long key = ResolveArenaLandmarkKey(cellX, cellZ);
                if (i < _arenaLandmarkKeys.Count)
                {
                    _arenaLandmarkKeys[i] = key;
                }
                else
                {
                    _arenaLandmarkKeys.Add(key);
                }

                float jitterX = ResolveArenaLandmarkJitter(cellX, cellZ, i * 2 + 1) * InfiniteArenaLandmarkJitterRadius;
                float jitterZ = ResolveArenaLandmarkJitter(cellX, cellZ, i * 2 + 2) * InfiniteArenaLandmarkJitterRadius;
                landmark.position = new Vector3(
                    cellX * InfiniteArenaTileSize + InfiniteArenaTileSize * 0.5f + jitterX,
                    0.38f,
                    cellZ * InfiniteArenaTileSize + InfiniteArenaTileSize * 0.5f + jitterZ);
                landmark.rotation = Quaternion.Euler(0f, (cellX * 37f + cellZ * 19f + i * 31f) % 360f, 0f);
            }
        }

        private Color ResolveArenaLandmarkColor(int index)
        {
            switch (index % 4)
            {
                case 0:
                    return ActiveUiTheme.GetArenaAccentColor(new Color(0.28f, 0.9f, 1f));
                case 1:
                    return ActiveUiTheme.GetBossThreatColor(new Color(0.95f, 0.38f, 0.56f));
                case 2:
                    return ActiveUiTheme.GetEliteThreatColor(new Color(0.98f, 0.82f, 0.24f));
                default:
                    return ActiveUiTheme.GetFeedbackAccentColor(new Color(0.48f, 0.92f, 0.42f));
            }
        }

        public bool TryResolveClosestArenaLandmark(Vector3 player, bool ignoreDiscovered, out Vector3 closest, out float closestDistance, out Vector3 closestDelta)
        {
            closest = Vector3.zero;
            closestDistance = 0f;
            closestDelta = Vector3.zero;
            if (_arenaLandmarks.Count == 0)
            {
                return false;
            }

            float closestDistanceSquared = float.MaxValue;
            for (int i = 0; i < _arenaLandmarks.Count; i++)
            {
                Transform landmark = _arenaLandmarks[i];
                if (landmark == null)
                {
                    continue;
                }

                if (ignoreDiscovered &&
                    i < _arenaLandmarkKeys.Count &&
                    _isDiscovered(_arenaLandmarkKeys[i]))
                {
                    continue;
                }

                Vector3 delta = landmark.position - player;
                delta.y = 0f;
                float distanceSquared = delta.sqrMagnitude;
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closest = landmark.position;
                    closestDelta = delta;
                }
            }

            if (closestDistanceSquared >= float.MaxValue)
            {
                return false;
            }

            closestDistance = Mathf.Sqrt(closestDistanceSquared);
            return true;
        }

        public void ApplyTheme()
        {
            for (int i = 0; i < _arenaTiles.Count; i++)
            {
                Color color = (i & 1) == 0
                    ? ActiveUiTheme.GetArenaFloorColor(new Color(0.045f, 0.055f, 0.058f))
                    : ActiveUiTheme.GetArenaGridColor(new Color(0.055f, 0.065f, 0.05f));
                SetTransformColor(_arenaTiles[i], color);
            }

            for (int i = 0; i < _arenaLandmarks.Count; i++)
            {
                SetTransformColor(_arenaLandmarks[i], ResolveArenaLandmarkColor(i));
            }

            _compass.ApplyTheme();
        }

        private static void SetTransformColor(Transform target, Color color)
        {
            SurvivorsPrimitivePresentation.SetRendererColor(target == null ? null : target.GetComponentInChildren<Renderer>(), color);
        }

        public void Dispose()
        {
            _arenaTiles.Clear();
            _arenaLandmarks.Clear();
            _arenaLandmarkKeys.Clear();
            _compass.ForgetHierarchy();
            if (_arenaTileRoot != null) UnityObjectUtility.DestroySafely(_arenaTileRoot.gameObject);
            if (_arenaFollowRoot != null) UnityObjectUtility.DestroySafely(_arenaFollowRoot.gameObject);
            _arenaTileRoot = null;
            _arenaFollowRoot = null;
            _primitives.Dispose();
        }
    }
}
