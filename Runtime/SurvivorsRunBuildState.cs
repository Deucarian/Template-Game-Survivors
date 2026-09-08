using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsRunBuildPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        int WeaponCount { get; }
        bool HasWeapon(string id);
        void AddWeapon(string id);
        void PassiveAdded(RunUpgradeDefinition upgrade);
        void RecordEvolutionTime();
        void EvolutionAdded(RunUpgradeDefinition upgrade);
    }

    /// <summary>Owns the active run catalog, rank state, metadata and acquired build slots.</summary>
    internal sealed class SurvivorsRunBuildState
    {
        private readonly ISurvivorsRunBuildPort _port;
        private readonly Dictionary<string, SurvivorsRunUpgradeMetadata> _upgradeMetadataById = new Dictionary<string, SurvivorsRunUpgradeMetadata>(StringComparer.Ordinal);
        private readonly HashSet<string> _ownedPassiveUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _ownedEvolutionUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
        public SurvivorsRunBuildState(ISurvivorsRunBuildPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        public RunUpgradeCatalog Catalog { get; private set; }
        public RunUpgradeState State { get; private set; }
        public IReadOnlyCollection<string> PassiveIds => _ownedPassiveUpgradeIds;
        public IReadOnlyCollection<string> EvolutionIds => _ownedEvolutionUpgradeIds;
        public int ActivePassiveCount => _ownedPassiveUpgradeIds.Count;
        public int WeaponEvolutionFeedbackCount { get; private set; }
        public int MaxWeaponSlots => _port.Tuning.MaxWeaponSlots > 0 ? _port.Tuning.MaxWeaponSlots : 6;
        public int MaxPassiveSlots => _port.Tuning.MaxPassiveSlots > 0 ? _port.Tuning.MaxPassiveSlots : 6;
        public bool HasEvolution(string id) => !string.IsNullOrWhiteSpace(id) && _ownedEvolutionUpgradeIds.Contains(id);
        public bool HasPassive(string id) => _ownedPassiveUpgradeIds.Contains(id);
        public void Initialize(RunUpgradeCatalog catalog, IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata, SurvivorsClassDefinition selectedClass, IReadOnlyList<SurvivorsClassUpgradeGateDefinition> gates)
        {
            State = new RunUpgradeState();
            Catalog = FilterForClass(catalog, selectedClass, gates);
            BuildUpgradeMetadataIndex(metadata);
            ClearOwnedSelections();
            WeaponEvolutionFeedbackCount = 0;
        }
        public void ClearOwnedSelections()
        {
            _ownedPassiveUpgradeIds.Clear();
            _ownedEvolutionUpgradeIds.Clear();
        }

        private static RunUpgradeCatalog FilterForClass(RunUpgradeCatalog fullCatalog, SurvivorsClassDefinition selectedClass, IReadOnlyList<SurvivorsClassUpgradeGateDefinition> gates)
        {
            var definitions = new List<RunUpgradeDefinition>(fullCatalog.Definitions.Count);
            for (int i = 0; i < fullCatalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = fullCatalog.Definitions[i];
                if (definition != null && IsUpgradeAllowedForSelectedClass(definition.Id.Value, selectedClass, gates))
                {
                    definitions.Add(definition);
                }
            }

            return definitions.Count == 0 ? fullCatalog : new RunUpgradeCatalog(definitions);
        }

        private static bool IsUpgradeAllowedForSelectedClass(string upgradeId, SurvivorsClassDefinition selectedClass, IReadOnlyList<SurvivorsClassUpgradeGateDefinition> gates)
        {
            if (string.IsNullOrWhiteSpace(upgradeId) || gates == null)
            {
                return true;
            }

            for (int i = 0; i < gates.Count; i++)
            {
                SurvivorsClassUpgradeGateDefinition gate = gates[i];
                if (gate != null && string.Equals(gate.UpgradeId, upgradeId, StringComparison.Ordinal))
                {
                    return gate.IsAvailableToClass(selectedClass);
                }
            }

            return true;
        }

        private void BuildUpgradeMetadataIndex(IReadOnlyList<SurvivorsRunUpgradeMetadata> metadataSource)
        {
            _upgradeMetadataById.Clear();
            IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata = metadataSource;
            for (int i = 0; i < metadata.Count; i++)
            {
                SurvivorsRunUpgradeMetadata entry = metadata[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.UpgradeId) || _upgradeMetadataById.ContainsKey(entry.UpgradeId))
                {
                    continue;
                }

                _upgradeMetadataById.Add(entry.UpgradeId, entry);
            }
        }

        public bool TryGetRunUpgrade(string upgradeId, out RunUpgradeDefinition definition)
        {
            definition = null;
            return Catalog != null &&
                !string.IsNullOrWhiteSpace(upgradeId) &&
                Catalog.TryGet(new RunUpgradeId(upgradeId), out definition);
        }

        public bool TryGetUpgradeMetadata(string upgradeId, out SurvivorsRunUpgradeMetadata metadata)
        {
            metadata = null;
            return !string.IsNullOrWhiteSpace(upgradeId) && _upgradeMetadataById.TryGetValue(upgradeId, out metadata);
        }

        public string ResolveUpgradeDisplayName(RunUpgradeId upgradeId)
        {
            return TryGetUpgradeMetadata(upgradeId.Value, out SurvivorsRunUpgradeMetadata metadata) &&
                !string.IsNullOrWhiteSpace(metadata.DisplayName)
                    ? metadata.DisplayName
                    : BasicSurvivorsGame.GetUpgradeDisplayName(upgradeId);
        }

        public int ResolveRequiredUpgradeRank(SurvivorsRunUpgradeMetadata metadata)
        {
            if (metadata == null)
            {
                return 1;
            }

            int requiredRank = Mathf.Max(1, metadata.RequiredUpgradeRank);
            if (metadata.IsEvolution)
            {
                requiredRank = Mathf.Max(1, requiredRank - Mathf.Max(0, _port.Tuning.EvolutionRequiredRankReduction));
            }

            return requiredRank;
        }

        public bool IsUpgradeEligibleForCurrentBuild(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null || Catalog == null || State == null)
            {
                return false;
            }

            if (RunUpgradeDraftService.GetAvailability(Catalog, State, upgrade) != RunUpgradeSelectionStatus.Selected)
            {
                return false;
            }

            if (!TryGetUpgradeMetadata(upgrade.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return true;
            }

            if (metadata.UsesPassiveSlot && State.GetRank(upgrade.Id) <= 0 && ActivePassiveCount >= MaxPassiveSlots)
            {
                return false;
            }

            if (metadata.UsesWeaponSlot && State.GetRank(upgrade.Id) <= 0)
            {
                if (_port.WeaponCount >= MaxWeaponSlots)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(metadata.AffectedContentId) && _port.HasWeapon(metadata.AffectedContentId))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredOwnedWeaponId) && !_port.HasWeapon(metadata.RequiredOwnedWeaponId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredUpgradeId))
            {
                int requiredRank = ResolveRequiredUpgradeRank(metadata);
                if (State.GetRank(new RunUpgradeId(metadata.RequiredUpgradeId)) < requiredRank)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(metadata.RequiredPassiveUpgradeId) &&
                State.GetRank(new RunUpgradeId(metadata.RequiredPassiveUpgradeId)) <= 0)
            {
                return false;
            }

            return true;
        }

        public SurvivorsRunUpgradeCategory ResolveCurrentUpgradeCategory(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null || !TryGetUpgradeMetadata(upgrade.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return SurvivorsRunUpgradeCategory.WeaponUpgrade;
            }

            if (metadata.Category == SurvivorsRunUpgradeCategory.Passive && State.GetRank(upgrade.Id) > 0)
            {
                return SurvivorsRunUpgradeCategory.PassiveUpgrade;
            }

            if (metadata.Category == SurvivorsRunUpgradeCategory.Weapon && State.GetRank(upgrade.Id) > 0)
            {
                return SurvivorsRunUpgradeCategory.WeaponUpgrade;
            }

            return metadata.Category;
        }

        public void RecordRunBuildSelection(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null || !TryGetUpgradeMetadata(upgrade.Id.Value, out SurvivorsRunUpgradeMetadata metadata))
            {
                return;
            }

            if (metadata.UsesPassiveSlot)
            {
                bool addedPassive = _ownedPassiveUpgradeIds.Add(upgrade.Id.Value);
                if (addedPassive)
                {
                    _port.PassiveAdded(upgrade);
                }
            }

            if (metadata.UsesWeaponSlot)
            {
                _port.AddWeapon(metadata.AffectedContentId);
            }

            if (metadata.IsEvolution)
            {
                _port.RecordEvolutionTime();
                _ownedEvolutionUpgradeIds.Add(upgrade.Id.Value);
                WeaponEvolutionFeedbackCount++;
                _port.EvolutionAdded(upgrade);
            }
        }
    }
}
