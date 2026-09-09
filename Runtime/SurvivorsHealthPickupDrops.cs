using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Checks current recovery eligibility before requesting a health pickup.</summary>
    internal sealed class SurvivorsHealthPickupDrops
    {
        private readonly ISurvivorsHealthPickupDropPort _port;
        internal SurvivorsHealthPickupDrops(ISurvivorsHealthPickupDropPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal bool TryDropHealthPickup(Vector3 position)
        {
            if (!_port.IsHealthBound || _port.HealAmount <= 0 || _port.CurrentHealth >= _port.MaxHealth - 0.01f)
            {
                return false;
            }

            return _port.SpawnHealthPickup(position, _port.HealAmount);
        }
    }
}
