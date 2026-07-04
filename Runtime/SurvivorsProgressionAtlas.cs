using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsProgressionTrackKind
    {
        PassiveAtlas = 0,
        WeaponSkillTrack = 1
    }

    public enum SurvivorsProgressionNodeKind
    {
        Passive = 0,
        WeaponRank = 1,
        WeaponMutation = 2,
        WeaponUnlock = 3,
        Evolution = 4
    }

    public sealed class SurvivorsProgressionNodeDefinition
    {
        public SurvivorsProgressionNodeDefinition(
            string id,
            string displayName,
            string upgradeId,
            SurvivorsProgressionNodeKind kind,
            int tier,
            int pointCost,
            int maxRank)
        {
            Id = id ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
            UpgradeId = upgradeId ?? string.Empty;
            Kind = kind;
            Tier = tier;
            PointCost = pointCost;
            MaxRank = maxRank;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string UpgradeId { get; }
        public SurvivorsProgressionNodeKind Kind { get; }
        public int Tier { get; }
        public int PointCost { get; }
        public int MaxRank { get; }
    }

    public sealed class SurvivorsProgressionTrackDefinition
    {
        public SurvivorsProgressionTrackDefinition(
            string id,
            string displayName,
            SurvivorsProgressionTrackKind kind,
            string classId,
            string targetWeaponId,
            IReadOnlyList<SurvivorsProgressionNodeDefinition> nodes)
        {
            Id = id ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
            Kind = kind;
            ClassId = classId ?? string.Empty;
            TargetWeaponId = targetWeaponId ?? string.Empty;
            Nodes = nodes == null ? Array.Empty<SurvivorsProgressionNodeDefinition>() : Copy(nodes);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public SurvivorsProgressionTrackKind Kind { get; }
        public string ClassId { get; }
        public string TargetWeaponId { get; }
        public IReadOnlyList<SurvivorsProgressionNodeDefinition> Nodes { get; }
        public bool IsClassSpecific => !string.IsNullOrWhiteSpace(ClassId);

        public bool ContainsUpgrade(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return false;
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                SurvivorsProgressionNodeDefinition node = Nodes[i];
                if (node != null && string.Equals(node.UpgradeId, upgradeId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static SurvivorsProgressionNodeDefinition[] Copy(IReadOnlyList<SurvivorsProgressionNodeDefinition> source)
        {
            var copy = new SurvivorsProgressionNodeDefinition[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }
}
