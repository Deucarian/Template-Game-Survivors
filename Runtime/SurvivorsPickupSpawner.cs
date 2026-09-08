using System;
using System.Collections.Generic;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns pickup request selection and initialization without owning the shared actor collection.</summary>
    internal sealed class SurvivorsPickupSpawner
    {
        private readonly ISurvivorsPickupSpawnPort _port;
        private readonly SurvivorsSpawnSequence _sequence;
        public SurvivorsPickupSpawner(ISurvivorsPickupSpawnPort port, SurvivorsSpawnSequence sequence)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        }

        public SurvivorsPickupActor SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount)
        {
            long sequence = _sequence.Next();
            WorldSpawnableId spawnable = ResolvePickupSpawnableId(kind);
            _port.RegisterExplicitPose(sequence, position);
            SpawnResult result = _port.Spawn(new WorldSpawnRequest(
                spawnable,
                BasicSurvivorsGame.ExplicitSpawnChannelId,
                sequence,
                new WorldSpawnRequestContext("SurvivorsTemplate", groupId: kind.ToString())));
            if (!result.Succeeded || result.Instance == null)
            {
                return null;
            }

            SurvivorsPickupActor pickup = result.Instance.GetComponent<SurvivorsPickupActor>();
            _port.InitializePickup(pickup, kind, Mathf.Max(1, amount), _port.CurrentPickupAttractRange, _port.CurrentPickupAttractionSpeed, _port.Tuning.PickupCollectRadius);
            _port.RegisterPickup(pickup);
            return pickup;
        }

        public static WorldSpawnableId ResolvePickupSpawnableId(SurvivorsPickupKind kind)
        {
            switch (kind)
            {
                case SurvivorsPickupKind.Magnet:
                    return BasicSurvivorsGame.MagnetPickupSpawnableId;
                case SurvivorsPickupKind.Health:
                    return BasicSurvivorsGame.HealthPickupSpawnableId;
                case SurvivorsPickupKind.BloodShard:
                    return BasicSurvivorsGame.BloodShardPickupSpawnableId;
                default:
                    return BasicSurvivorsGame.ExperiencePickupSpawnableId;
            }
        }

    }
}
