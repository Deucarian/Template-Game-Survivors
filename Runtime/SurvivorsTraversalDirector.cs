using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsTraversalPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        int ActiveShrineEnemyCount { get; }
        void SpawnShrine(Vector3 direction);
        void SpawnCache(Vector3 direction, int sequenceOffset);
    }

    /// <summary>Owns travel budgets and preserves shrine-before-cache reward ordering.</summary>
    internal sealed class SurvivorsTraversalDirector
    {
        private readonly ISurvivorsTraversalPort _port;
        private float _cacheTravel;
        private float _shrineTravel;

        public SurvivorsTraversalDirector(ISurvivorsTraversalPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public void Reset()
        {
            _cacheTravel = 0f;
            _shrineTravel = 0f;
        }

        public void RecordTravel(Vector3 delta, bool playing)
        {
            if (!playing || delta.sqrMagnitude <= 0.0001f) return;
            float distance = delta.magnitude;
            _cacheTravel += distance;
            RecordShrineTravel(delta, distance);
            float interval = Mathf.Max(1f, _port.Tuning.RoamingCacheTravelInterval);
            if (_cacheTravel < interval) return;
            int count = Mathf.Min(3, Mathf.FloorToInt(_cacheTravel / interval));
            _cacheTravel = Mathf.Max(0f, _cacheTravel - count * interval);
            Vector3 direction = delta.normalized;
            for (int i = 0; i < count; i++) _port.SpawnCache(direction, i);
        }

        private void RecordShrineTravel(Vector3 delta, float distance)
        {
            float interval = Mathf.Max(0f, _port.Tuning.ArenaShrineTravelInterval);
            if (interval <= 0f || distance <= 0f || _port.ActiveShrineEnemyCount > 0) return;
            _shrineTravel += distance;
            if (_shrineTravel < interval) return;
            _shrineTravel = Mathf.Max(0f, _shrineTravel - interval);
            _port.SpawnShrine(delta);
        }
    }
}
