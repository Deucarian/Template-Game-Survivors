using System;
using System.Collections.Generic;
using Deucarian.Projectiles;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns run weapon definitions, loadout lifetime and live weapon-stat resolution.</summary>
    internal sealed class SurvivorsRunWeapons
    {
        private readonly ISurvivorsRunWeaponPort _port;
        private ProjectileDefinition _projectileDefinition;
        private ISurvivorsWeaponLoadoutSession _weaponLoadout;
        private IReadOnlyList<SurvivorsWeaponArchetypeDefinition> _weaponArchetypeDefinitions = Array.Empty<SurvivorsWeaponArchetypeDefinition>();

        internal SurvivorsRunWeapons(ISurvivorsRunWeaponPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal int ActiveWeaponCount => _weaponLoadout == null ? 0 : _weaponLoadout.WeaponCount;
        internal IReadOnlyList<string> ActiveWeaponIds => _weaponLoadout == null ? Array.Empty<string>() : _weaponLoadout.WeaponIds;
        internal int ActiveOrbitBladeCount => _weaponLoadout == null ? 0 : _weaponLoadout.ActiveOrbitBladeCount;

        // These two initialization phases stay on opposite sides of world construction.
        internal void InitializeFallbackDefinition(SurvivorsTemplateTuning tuning)
            => _projectileDefinition = BasicSurvivorsGame.CreateProjectileDefinition(tuning);

        internal void BuildLoadout(SurvivorsTemplateTuning tuning)
        {
            _weaponArchetypeDefinitions = _port.CreateDefinitions(tuning);
            _weaponLoadout = _port.CreateLoadout(ResolveStartingWeaponDefinitions(_weaponArchetypeDefinitions));
        }

        internal void DisposeLoadout()
        {
            if (_weaponLoadout == null) return;
            _weaponLoadout.Dispose();
            _weaponLoadout = null;
        }

        internal void Tick(float deltaTime) => _weaponLoadout?.Tick(deltaTime);
        internal bool FireForTest(SurvivorsWeaponArchetype archetype) => _weaponLoadout != null && _weaponLoadout.FireForTest(archetype);
        internal bool ContainsWeapon(string id) => _weaponLoadout != null && _weaponLoadout.ContainsWeapon(id);

        internal float ResolveDisplayedWeaponDamage()
        {
            SurvivorsWeaponArchetypeDefinition definition = ResolvePrimaryWeaponDefinitionForDisplay();
            if (definition != null)
            {
                return ResolveWeaponDamage(definition);
            }

            float baseDamage = _projectileDefinition == null ? 0f : (float)_projectileDefinition.BaseDamage;
            return Mathf.Max(0f, _port.DamageBonuses.AddTo(baseDamage));
        }

        internal float ResolveDisplayedWeaponCooldownSeconds()
        {
            SurvivorsWeaponArchetypeDefinition definition = ResolvePrimaryWeaponDefinitionForDisplay();
            if (definition != null)
            {
                return ResolveWeaponCooldownSeconds(definition);
            }

            return Mathf.Max(0.12f, _port.Tuning.WeaponCooldownSeconds * Mathf.Max(0.2f, _port.CooldownBonuses.AddTo(1f)));
        }

        internal SurvivorsWeaponArchetypeDefinition ResolvePrimaryWeaponDefinitionForDisplay()
        {
            IReadOnlyList<string> weaponIds = _weaponLoadout == null ? null : _weaponLoadout.WeaponIds;
            IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions = _weaponArchetypeDefinitions;
            if (weaponIds == null || weaponIds.Count == 0 || definitions == null)
            {
                return null;
            }

            string primaryWeaponId = weaponIds[0];
            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition definition = definitions[i];
                if (definition != null && string.Equals(definition.Id, primaryWeaponId, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        internal float ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, _port.DamageBonuses.AddTo(definition.Damage));
        }

        internal float ResolveWeaponCooldownSeconds(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null)
            {
                return ResolveDisplayedWeaponCooldownSeconds();
            }

            return Mathf.Max(0.08f, definition.CooldownSeconds * Mathf.Max(0.2f, _port.CooldownBonuses.AddTo(1f)));
        }

        internal IReadOnlyList<SurvivorsWeaponArchetypeDefinition> ResolveStartingWeaponDefinitions(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> allDefinitions)
        {
            if (allDefinitions == null || allDefinitions.Count == 0)
            {
                return Array.Empty<SurvivorsWeaponArchetypeDefinition>();
            }

            if (_port.StartingWeaponIds == null || _port.StartingWeaponIds.Count == 0)
            {
                return new[] { allDefinitions[0] };
            }

            var selected = new List<SurvivorsWeaponArchetypeDefinition>(_port.StartingWeaponIds.Count);
            for (int classWeaponIndex = 0; classWeaponIndex < _port.StartingWeaponIds.Count; classWeaponIndex++)
            {
                string weaponId = _port.StartingWeaponIds[classWeaponIndex];
                for (int definitionIndex = 0; definitionIndex < allDefinitions.Count; definitionIndex++)
                {
                    SurvivorsWeaponArchetypeDefinition definition = allDefinitions[definitionIndex];
                    if (definition != null && string.Equals(definition.Id, weaponId, StringComparison.Ordinal))
                    {
                        selected.Add(definition);
                        break;
                    }
                }
            }

            return selected.Count == 0 ? new[] { allDefinitions[0] } : selected;
        }

        internal bool TryAddWeaponToLoadout(string weaponId)
        {
            if (_weaponLoadout == null || string.IsNullOrWhiteSpace(weaponId) || ActiveWeaponCount >= _port.MaximumSlots)
            {
                return false;
            }

            if (_weaponLoadout.ContainsWeapon(weaponId))
            {
                return false;
            }

            SurvivorsWeaponArchetypeDefinition definition = FindWeaponDefinition(weaponId);
            if (definition == null || !_weaponLoadout.TryAddWeapon(definition))
            {
                return false;
            }

            _port.WeaponAdded(definition);
            return true;
        }

        internal SurvivorsWeaponArchetypeDefinition FindWeaponDefinition(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions = _weaponArchetypeDefinitions;
            if (definitions == null || definitions.Count == 0)
            {
                definitions = _port.CreateDefinitions(_port.Tuning);
                _weaponArchetypeDefinitions = definitions;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition definition = definitions[i];
                if (definition != null && string.Equals(definition.Id, weaponId, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
