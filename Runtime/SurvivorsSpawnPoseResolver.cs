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
    public sealed class SurvivorsSpawnPoseResolver : ISpawnPoseResolver
    {
        private readonly SurvivorsTemplateController _controller;
        private readonly Dictionary<long, Vector3> _explicitPoses = new Dictionary<long, Vector3>();

        public SurvivorsSpawnPoseResolver(SurvivorsTemplateController controller)
        {
            _controller = controller;
        }

        public void RegisterExplicitPose(long sequence, Vector3 position)
        {
            _explicitPoses[sequence] = position;
        }

        public SpawnPoseResult TryResolvePose(WorldSpawnRequest request)
        {
            if (_explicitPoses.TryGetValue(request.Sequence, out Vector3 explicitPosition))
            {
                _explicitPoses.Remove(request.Sequence);
                return SpawnPoseResult.Success(new SpawnPose(explicitPosition, Quaternion.identity));
            }

            if (!request.ChannelId.Equals(BasicSurvivorsGame.RadialSpawnChannelId))
            {
                return SpawnPoseResult.Failure("Unknown Survivors spawn channel: " + request.ChannelId);
            }

            if (_controller == null)
            {
                float angle = (request.Sequence * 137.50777f) * Mathf.Deg2Rad;
                Vector3 fallbackPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 12f;
                return SpawnPoseResult.Success(new SpawnPose(fallbackPosition, Quaternion.identity));
            }

            Vector3 resolvedPosition = _controller.ResolveRuntimeEnemySpawnPositionForResolver(request.Sequence);
            return SpawnPoseResult.Success(new SpawnPose(resolvedPosition, Quaternion.identity));
        }
    }
}
