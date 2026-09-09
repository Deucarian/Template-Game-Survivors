using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Result/menu choice copy over borrowed definitions and copied progression observations.</summary>
    internal static class SurvivorsProgressionChoiceLabels
    {
        internal static string PersistentUpgradeOption(int index, SurvivorsPersistentUpgradeDefinition upgrade,
            int currentRank, int nextCost, string currencyLabel)
        {
            if (upgrade == null) return string.Empty;
            return $"{index + 1}. {upgrade.DisplayName} rank {currentRank}->{Mathf.Min(upgrade.MaxRank, currentRank + 1)}/{upgrade.MaxRank} ({nextCost} {currencyLabel}) - {PersistentUpgradeEffect(upgrade)}";
        }

        internal static string ClassOption(int index, SurvivorsClassDefinition definition, bool unlocked, string selectedClassId)
        {
            if (definition == null) return string.Empty;
            bool selected = string.Equals(selectedClassId, definition.Id, StringComparison.Ordinal);
            string state = selected ? "Selected" : (unlocked ? "Unlocked" : "Locked");
            return $"{index + 1}. {definition.DisplayName} [{state}]\n{ClassStats(definition)} | {ClassStartingWeapons(definition)}";
        }

        internal static string ClassStats(SurvivorsClassDefinition definition)
        {
            if (definition == null || definition.StartingStatModifiers.Count == 0) return "Balanced";
            var labels = new List<string>(definition.StartingStatModifiers.Count);
            for (int i = 0; i < definition.StartingStatModifiers.Count; i++)
            {
                SurvivorsClassStatModifierDefinition modifier = definition.StartingStatModifiers[i];
                if (modifier == null) continue;
                if (modifier.StatKind == SurvivorsClassStatKind.MoveSpeed) labels.Add($"Move +{modifier.Amount:0.##}");
                else if (modifier.StatKind == SurvivorsClassStatKind.Damage) labels.Add($"Dmg +{modifier.Amount:0.##}");
                else if (modifier.StatKind == SurvivorsClassStatKind.MaxHealth) labels.Add($"HP +{modifier.Amount:0.#}");
            }
            return labels.Count == 0 ? "Balanced" : string.Join(", ", labels);
        }

        internal static string ClassStartingWeapons(SurvivorsClassDefinition definition)
        {
            if (definition == null || definition.StartingWeaponIds.Count == 0) return "0 weapons";
            return definition.StartingWeaponIds.Count == 1 ? "1 weapon" : definition.StartingWeaponIds.Count.ToString() + " weapons";
        }

        internal static string PersistentUpgradeEffect(SurvivorsPersistentUpgradeDefinition upgrade)
        {
            if (upgrade == null) return string.Empty;
            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaDamageEffectId, StringComparison.Ordinal))
                return $"+{upgrade.AmountPerRank:0.#} starting damage";
            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaMaxHealthEffectId, StringComparison.Ordinal))
                return $"+{upgrade.AmountPerRank:0.#} max health";
            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaPickupRangeEffectId, StringComparison.Ordinal))
                return $"+{upgrade.AmountPerRank:0.#} pickup range";
            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaExperienceGainEffectId, StringComparison.Ordinal))
                return $"+{upgrade.AmountPerRank:P0} XP gain";
            if (string.Equals(upgrade.EffectId, BasicSurvivorsGame.MetaDraftRerollEffectId, StringComparison.Ordinal))
                return $"+{upgrade.AmountPerRank:0} draft reroll";
            return upgrade.EffectId;
        }

        internal static string ClassDisplayName(SurvivorsClassLibraryDefinition library, string classId, string fallback)
        {
            return library != null && library.TryGetClass(classId, out SurvivorsClassDefinition definition) &&
                !string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.DisplayName : fallback;
        }
    }
}
