using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns result choices and persistent purchase/selection transactions, without GUI state.</summary>
    internal sealed class SurvivorsPersistentProgression
    {
        private readonly ISurvivorsPersistentProgressionPort _port;
        private readonly List<SurvivorsPersistentUpgradeDefinition> _upgradeOptions = new List<SurvivorsPersistentUpgradeDefinition>();
        private readonly List<SurvivorsClassDefinition> _classOptions = new List<SurvivorsClassDefinition>();

        public SurvivorsPersistentProgression(ISurvivorsPersistentProgressionPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public int MetaUpgradePurchaseCount { get; private set; }
        public int ResultClassSelectionCount { get; private set; }

        public void ResetRunDiagnostics()
        {
            MetaUpgradePurchaseCount = 0;
            ResultClassSelectionCount = 0;
        }

        public bool TryPurchasePersistentUpgrade(string id, bool runStarted)
        {
            bool purchased = _port.EnsureProgression().TryPurchasePersistentUpgrade(id);
            if (purchased && runStarted)
            {
                _port.ApplyPersistentBonuses();
            }

            if (purchased)
            {
                MetaUpgradePurchaseCount++;
                _port.ShowMetaPurchase(id);
            }

            return purchased;
        }

        public bool TryPurchaseResultMetaUpgrade(int index, int limit, bool runStarted)
        {
            IReadOnlyList<SurvivorsPersistentUpgradeDefinition> options = ResolveResultMetaUpgradeOptions(limit);
            if (index < 0 || index >= options.Count) return false;
            return TryPurchasePersistentUpgrade(options[index].Id.Value, runStarted);
        }

        public IReadOnlyList<SurvivorsPersistentUpgradeDefinition> ResolveResultMetaUpgradeOptions(int limit)
        {
            _upgradeOptions.Clear();
            if (limit <= 0) return _upgradeOptions;

            SurvivorsMetaProgressionService progression = _port.EnsureProgression();
            SurvivorsMetaProgressionDefinition definition = _port.ProgressionDefinition;
            for (int i = 0; i < definition.PersistentUpgrades.Count && _upgradeOptions.Count < limit; i++)
            {
                SurvivorsPersistentUpgradeDefinition upgrade = definition.PersistentUpgrades[i];
                if (upgrade == null) continue;
                int currentRank = progression.GetPersistentUpgradeRank(upgrade.Id.Value);
                int nextCost = ResolveNextPersistentUpgradeCost(upgrade, currentRank);
                if (currentRank < upgrade.MaxRank && nextCost > 0 && progression.UnspentBloodShards >= nextCost)
                {
                    _upgradeOptions.Add(upgrade);
                }
            }

            return _upgradeOptions;
        }

        public IReadOnlyList<SurvivorsClassDefinition> ResolveResultClassOptions(int limit)
        {
            _classOptions.Clear();
            if (limit <= 0) return _classOptions;
            _port.EnsureProgression();
            SurvivorsClassLibraryDefinition library = _port.EnsureClasses();
            if (library == null) return _classOptions;
            for (int i = 0; i < library.Classes.Count && _classOptions.Count < limit; i++)
            {
                SurvivorsClassDefinition definition = library.Classes[i];
                if (definition != null) _classOptions.Add(definition);
            }

            return _classOptions;
        }

        public bool TrySelectResultClass(int index, int limit, bool runStarted, SurvivorsRunState state)
        {
            if (runStarted && state != SurvivorsRunState.GameOver && state != SurvivorsRunState.Victory) return false;
            IReadOnlyList<SurvivorsClassDefinition> options = ResolveResultClassOptions(limit);
            if (index < 0 || index >= options.Count) return false;
            SurvivorsClassDefinition selected = options[index];
            if (!IsResultClassUnlocked(selected)) return false;

            SurvivorsMetaProgressionService progression = _port.EnsureProgression();
            SurvivorsClassLibraryDefinition library = _port.EnsureClasses();
            bool changed = progression.TrySetSelectedClass(selected.Id, library);
            _port.SetSelectedClass(progression.ResolveSelectedClass(library));
            if (changed)
            {
                ResultClassSelectionCount++;
                _port.ShowClassSelection(selected);
            }

            return changed;
        }

        public bool IsResultClassUnlocked(SurvivorsClassDefinition definition)
        {
            if (definition == null) return false;
            SurvivorsMetaProgressionService progression = _port.EnsureProgression();
            SurvivorsClassLibraryDefinition library = _port.EnsureClasses();
            return progression != null && progression.IsClassUnlocked(definition.Id, library);
        }

        public static int ResolveNextPersistentUpgradeCost(SurvivorsPersistentUpgradeDefinition upgrade, int currentRank)
        {
            if (upgrade == null || currentRank < 0 || currentRank >= upgrade.MaxRank || upgrade.RankCosts.Count == 0) return 0;
            int costIndex = Mathf.Clamp(currentRank, 0, upgrade.RankCosts.Count - 1);
            return Mathf.Max(0, upgrade.RankCosts[costIndex]);
        }
    }
}
