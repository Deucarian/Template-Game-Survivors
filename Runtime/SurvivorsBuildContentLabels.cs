using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Reads authored build names in catalog order and preserves the template's compact fallback labels.</summary>
    internal sealed class SurvivorsBuildContentLabels
    {
        private readonly SurvivorsRunBuildState _build;
        public SurvivorsBuildContentLabels(SurvivorsRunBuildState build) =>
            _build = build ?? throw new ArgumentNullException(nameof(build));

        public string ShortWeaponName(string weaponId)
        {
            if (TryResolveWeaponUpgradeDisplayName(weaponId, out string authoredName, out RunUpgradeId upgradeId) &&
                !string.Equals(authoredName, BasicSurvivorsGame.GetUpgradeDisplayName(upgradeId), StringComparison.Ordinal))
            {
                return authoredName;
            }

            if (weaponId == BasicSurvivorsGame.ArcaneWandWeaponContentId) return "Wand";
            if (weaponId == BasicSurvivorsGame.FrostFanWeaponContentId) return "Frost";
            if (weaponId == BasicSurvivorsGame.OrbitWardWeaponContentId) return "Orbit";
            if (weaponId == BasicSurvivorsGame.ThornHaloWeaponContentId) return "Halo";
            if (weaponId == BasicSurvivorsGame.MoonSlashWeaponContentId) return "Slash";
            if (weaponId == BasicSurvivorsGame.StarNovaWeaponContentId) return "Nova";
            if (weaponId == BasicSurvivorsGame.StarBeamWeaponContentId) return "Beam";
            if (weaponId == BasicSurvivorsGame.GravityGrenadeWeaponContentId) return "Grenade";
            if (weaponId == BasicSurvivorsGame.RuneTrapWeaponContentId) return "Trap";
            if (weaponId == BasicSurvivorsGame.AetherMineWeaponContentId) return "Mine";
            if (weaponId == BasicSurvivorsGame.PlayerTarget.Value) return "Player";
            if (weaponId == BasicSurvivorsGame.PickupTarget.Value) return "Pickups";
            if (weaponId == BasicSurvivorsGame.StatusTarget.Value) return "Status";
            if (weaponId == BasicSurvivorsGame.BarrierTarget.Value) return "Barrier";
            if (weaponId == BasicSurvivorsGame.PayloadWeaponTarget.Value) return "Payloads";
            if (weaponId == BasicSurvivorsGame.ExperienceTarget.Value) return "XP";
            if (weaponId == BasicSurvivorsGame.AreaTarget.Value) return "Area";
            return string.IsNullOrWhiteSpace(weaponId) ? "unknown" : weaponId;
        }

        public bool TryResolveWeaponUpgradeDisplayName(string weaponId, out string displayName, out RunUpgradeId upgradeId)
        {
            displayName = string.Empty;
            upgradeId = default;
            if (_build.Catalog == null)
            {
                return false;
            }

            for (int i = 0; i < _build.Catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = _build.Catalog.Definitions[i];
                if (definition == null ||
                    !_build.TryGetUpgradeMetadata(definition.Id.Value, out SurvivorsRunUpgradeMetadata metadata) ||
                    metadata.Category != SurvivorsRunUpgradeCategory.Weapon ||
                    !string.Equals(metadata.AffectedContentId, weaponId, StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(metadata.DisplayName))
                {
                    continue;
                }

                displayName = metadata.DisplayName;
                upgradeId = definition.Id;
                return true;
            }

            return false;
        }

        public string ResolveWeaponBuildDisplayName(string weaponId)
        {
            return TryResolveWeaponUpgradeDisplayName(weaponId, out string displayName, out _)
                ? displayName
                : ShortWeaponName(weaponId);
        }

        public string FormatBuildRankFragment(RunUpgradeDefinition definition, int rank)
        {
            if (definition == null)
            {
                return "Missing";
            }

            return $"{_build.ResolveUpgradeDisplayName(definition.Id)} {rank}/{Mathf.Max(1, definition.MaxRank)}";
        }

        public string FormatPassiveBuildHudLine(RunUpgradeDefinition definition, SurvivorsRunUpgradeMetadata metadata, int rank)
        {
            string line = FormatBuildRankFragment(definition, rank);
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.AffectedContentId))
            {
                return line;
            }

            return line + " - " + ShortWeaponName(metadata.AffectedContentId);
        }
    }
}
