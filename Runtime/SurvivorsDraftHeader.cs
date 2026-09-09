using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsDraftHeaderReadPort
    {
        bool IsRelicChoiceOpen { get; }
        RunUpgradeDraft CurrentDraft { get; }
        SurvivorsRunUpgradeCategory ResolveCategory(RunUpgradeDefinition choice);
        SurvivorsUiTheme ActiveTheme { get; }
    }

    /// <summary>Projects the current draft header while retaining live category and authored-theme reads.</summary>
    internal sealed class SurvivorsDraftHeader
    {
        private readonly ISurvivorsDraftHeaderReadPort _port;
        internal SurvivorsDraftHeader(ISurvivorsDraftHeaderReadPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal Color ResolveAccent()
        {
            if (_port.IsRelicChoiceOpen)
            {
                return _port.ActiveTheme.GetRarityAccentColor("Relic", new Color(1f, 0.84f, 0.42f));
            }

            if (_port.CurrentDraft == null || _port.CurrentDraft.Choices.Count == 0)
            {
                return _port.ActiveTheme.GetRarityAccentColor("Common", Color.white);
            }

            for (int i = 0; i < _port.CurrentDraft.Choices.Count; i++)
            {
                RunUpgradeDefinition choice = _port.CurrentDraft.Choices[i];
                if (choice != null && _port.ResolveCategory(choice) == SurvivorsRunUpgradeCategory.Evolution)
                {
                    return _port.ActiveTheme.GetRarityAccentColor("Evolution", new Color(1f, 0.38f, 0.56f));
                }
            }

            RunUpgradeRarity rarity = SurvivorsRewardFeedbackHistory.ResolveHighestRarity(_port.CurrentDraft.Choices);
            return _port.ActiveTheme.GetRarityAccentColor(rarity, SurvivorsDraftCardFactory.ResolveRarityAccentColor(rarity));
        }

        internal static string Title(SurvivorsRewardSelectionKind kind)
        {
            if (kind == SurvivorsRewardSelectionKind.BossRelic) return "Choose a Boss Relic";
            if (kind == SurvivorsRewardSelectionKind.EliteUpgrade) return "Elite Reward";
            if (kind == SurvivorsRewardSelectionKind.BossUpgrade) return "Boss Evolution Reward";
            return "Level Up";
        }
    }
}
