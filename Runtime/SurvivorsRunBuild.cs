using System;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsRunUpgradeCategory
    {
        Weapon = 0,
        WeaponUpgrade = 1,
        Passive = 2,
        PassiveUpgrade = 3,
        Mutation = 4,
        Evolution = 5
    }

    public enum SurvivorsRunBuildSlotKind
    {
        None = 0,
        Weapon = 1,
        Passive = 2
    }

    public sealed class SurvivorsRunUpgradeMetadata
    {
        public SurvivorsRunUpgradeMetadata(
            string upgradeId,
            string displayName,
            SurvivorsRunUpgradeCategory category,
            SurvivorsRunBuildSlotKind slotKind,
            string affectedContentId,
            string description,
            string requiredOwnedWeaponId = null,
            string requiredUpgradeId = null,
            int requiredUpgradeRank = 0,
            string requiredPassiveUpgradeId = null)
        {
            UpgradeId = upgradeId ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? UpgradeId : displayName;
            Category = category;
            SlotKind = slotKind;
            AffectedContentId = affectedContentId ?? string.Empty;
            Description = string.IsNullOrWhiteSpace(description) ? DisplayName : description;
            RequiredOwnedWeaponId = requiredOwnedWeaponId ?? string.Empty;
            RequiredUpgradeId = requiredUpgradeId ?? string.Empty;
            RequiredUpgradeRank = Math.Max(0, requiredUpgradeRank);
            RequiredPassiveUpgradeId = requiredPassiveUpgradeId ?? string.Empty;
        }

        public string UpgradeId { get; }
        public string DisplayName { get; }
        public SurvivorsRunUpgradeCategory Category { get; }
        public SurvivorsRunBuildSlotKind SlotKind { get; }
        public string AffectedContentId { get; }
        public string Description { get; }
        public string RequiredOwnedWeaponId { get; }
        public string RequiredUpgradeId { get; }
        public int RequiredUpgradeRank { get; }
        public string RequiredPassiveUpgradeId { get; }
        public bool IsEvolution => Category == SurvivorsRunUpgradeCategory.Evolution;
        public bool UsesPassiveSlot => SlotKind == SurvivorsRunBuildSlotKind.Passive;
        public bool UsesWeaponSlot => SlotKind == SurvivorsRunBuildSlotKind.Weapon;
    }
}
