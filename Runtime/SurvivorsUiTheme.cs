using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    [Serializable]
    public sealed class SurvivorsUiTheme
    {
        public string themeName = "Deucarian Survivors";
        public string cardBackgroundToken = "obsidian-panel";
        public string cardFrameToken = "rarity-frame";
        public string standardModeDisplayName = "Standard / Human Playtest";
        public string sprintModeDisplayName = "Sprint Run";
        public string chooseRewardTitle = "Choose Your Reward";
        public string levelUpTitle = "Level Up";
        public string eliteRewardTitle = "Elite Reward";
        public string bossRewardTitle = "Boss Reward";
        public string bossRelicTitle = "Boss Relic";
        public string rerollButtonLabel = "Reroll";
        public string skipButtonLabel = "Skip";
        public string banishButtonLabel = "Banish";
        public string restartSameButtonLabel = "Restart Same";
        public string changeModeButtonLabel = "Change Mode";
        public string continueButtonLabel = "Continue";
        public string buildMenuTitle = "Build";
        public string runSummaryTitle = "Run Summary";
        public string tutorialTitle = "Survivor Primer";
        public string themeSelectorTitle = "Theme";
        public string showTutorialButtonLabel = "Show Tutorial";
        public string debugToggleLabel = "Debug Overlay";
        public string iconPlaceholderPrefix = "Icon";
        public string hudAccentColor = "#33C7FF";
        public string buttonStyleToken = "solid";
        public RarityStyleToken[] rarityStyles;
        public CategoryStyleToken[] categoryStyles;
        public TutorialStepStyleToken[] tutorialSteps;
        public AudioEventStyleToken[] audioEvents;

        public static SurvivorsUiTheme CreateDefault()
        {
            var theme = new SurvivorsUiTheme();
            theme.ApplyFallbacks();
            return theme;
        }

        public static bool TryFromJson(string json, out SurvivorsUiTheme theme, out string error)
        {
            theme = CreateDefault();
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            try
            {
                SurvivorsUiTheme parsed = JsonUtility.FromJson<SurvivorsUiTheme>(json);
                if (parsed == null)
                {
                    error = "Theme JSON did not contain a Survivors UI theme object.";
                    return false;
                }

                parsed.ApplyFallbacks();
                theme = parsed;
                return true;
            }
            catch (ArgumentException exception)
            {
                error = exception.Message;
                return false;
            }
        }

        public string GetRarityDisplayName(RunUpgradeRarity rarity)
        {
            return GetRarityDisplayName(rarity.ToString());
        }

        public string GetRarityDisplayName(string rarityId)
        {
            RarityStyleToken token = FindRarityStyle(rarityId);
            return token == null || string.IsNullOrWhiteSpace(token.displayName) ? rarityId : token.displayName;
        }

        public string GetRarityStyleToken(RunUpgradeRarity rarity)
        {
            return GetRarityStyleToken(rarity.ToString());
        }

        public string GetRarityStyleToken(string rarityId)
        {
            RarityStyleToken token = FindRarityStyle(rarityId);
            return token == null || string.IsNullOrWhiteSpace(token.styleToken) ? rarityId : token.styleToken;
        }

        public string GetCategoryDisplayName(string categoryId, string fallback)
        {
            CategoryStyleToken token = FindCategoryStyle(categoryId);
            return token == null || string.IsNullOrWhiteSpace(token.displayName) ? fallback : token.displayName;
        }

        public string GetCategoryIconId(string categoryId, string fallback)
        {
            CategoryStyleToken token = FindCategoryStyle(categoryId);
            return token == null || string.IsNullOrWhiteSpace(token.iconId) ? fallback : token.iconId;
        }

        public Color GetRarityAccentColor(RunUpgradeRarity rarity, Color fallback)
        {
            return GetRarityAccentColor(rarity.ToString(), fallback);
        }

        public Color GetRarityAccentColor(string rarityId, Color fallback)
        {
            RarityStyleToken token = FindRarityStyle(rarityId);
            if (token == null || string.IsNullOrWhiteSpace(token.accentColor))
            {
                return fallback;
            }

            return ColorUtility.TryParseHtmlString(token.accentColor, out Color color) ? color : fallback;
        }

        public string GetModeDisplayName(SurvivorsPacingProfile profile, string fallback)
        {
            if (profile == SurvivorsPacingProfile.SprintRun && !string.IsNullOrWhiteSpace(sprintModeDisplayName))
            {
                return sprintModeDisplayName;
            }

            if (profile == SurvivorsPacingProfile.HumanPlaytest && !string.IsNullOrWhiteSpace(standardModeDisplayName))
            {
                return standardModeDisplayName;
            }

            return fallback;
        }

        public AudioEventStyleToken GetAudioEvent(string eventId)
        {
            if (audioEvents == null || string.IsNullOrWhiteSpace(eventId))
            {
                return null;
            }

            for (int i = 0; i < audioEvents.Length; i++)
            {
                AudioEventStyleToken token = audioEvents[i];
                if (token != null && string.Equals(token.id, eventId, StringComparison.OrdinalIgnoreCase))
                {
                    return token;
                }
            }

            return null;
        }

        public float GetAudioEventVolume(string eventId, float fallback)
        {
            AudioEventStyleToken token = GetAudioEvent(eventId);
            if (token == null || token.volume < 0f)
            {
                return Mathf.Clamp01(fallback);
            }

            return Mathf.Clamp01(token.volume);
        }

        public float GetAudioEventThrottleSeconds(string eventId, float fallback)
        {
            AudioEventStyleToken token = GetAudioEvent(eventId);
            if (token == null || token.throttleSeconds < 0f)
            {
                return Mathf.Max(0f, fallback);
            }

            return Mathf.Max(0f, token.throttleSeconds);
        }

        public string GetTutorialStepTitle(int stepIndex, string fallback)
        {
            TutorialStepStyleToken token = GetTutorialStep(stepIndex);
            return token == null || string.IsNullOrWhiteSpace(token.title) ? fallback : token.title;
        }

        public IReadOnlyList<string> GetTutorialStepLines(int stepIndex, IReadOnlyList<string> fallback)
        {
            TutorialStepStyleToken token = GetTutorialStep(stepIndex);
            return token == null || token.lines == null || token.lines.Length == 0 ? fallback : token.lines;
        }

        public Color GetHudAccentColor(Color fallback)
        {
            return ColorUtility.TryParseHtmlString(hudAccentColor, out Color color) ? color : fallback;
        }

        private void ApplyFallbacks()
        {
            themeName = string.IsNullOrWhiteSpace(themeName) ? "Deucarian Survivors" : themeName;
            cardBackgroundToken = string.IsNullOrWhiteSpace(cardBackgroundToken) ? "obsidian-panel" : cardBackgroundToken;
            cardFrameToken = string.IsNullOrWhiteSpace(cardFrameToken) ? "rarity-frame" : cardFrameToken;
            standardModeDisplayName = string.IsNullOrWhiteSpace(standardModeDisplayName) ? "Standard / Human Playtest" : standardModeDisplayName;
            sprintModeDisplayName = string.IsNullOrWhiteSpace(sprintModeDisplayName) ? "Sprint Run" : sprintModeDisplayName;
            chooseRewardTitle = string.IsNullOrWhiteSpace(chooseRewardTitle) ? "Choose Your Reward" : chooseRewardTitle;
            levelUpTitle = string.IsNullOrWhiteSpace(levelUpTitle) ? "Level Up" : levelUpTitle;
            eliteRewardTitle = string.IsNullOrWhiteSpace(eliteRewardTitle) ? "Elite Reward" : eliteRewardTitle;
            bossRewardTitle = string.IsNullOrWhiteSpace(bossRewardTitle) ? "Boss Reward" : bossRewardTitle;
            bossRelicTitle = string.IsNullOrWhiteSpace(bossRelicTitle) ? "Boss Relic" : bossRelicTitle;
            rerollButtonLabel = string.IsNullOrWhiteSpace(rerollButtonLabel) ? "Reroll" : rerollButtonLabel;
            skipButtonLabel = string.IsNullOrWhiteSpace(skipButtonLabel) ? "Skip" : skipButtonLabel;
            banishButtonLabel = string.IsNullOrWhiteSpace(banishButtonLabel) ? "Banish" : banishButtonLabel;
            restartSameButtonLabel = string.IsNullOrWhiteSpace(restartSameButtonLabel) ? "Restart Same" : restartSameButtonLabel;
            changeModeButtonLabel = string.IsNullOrWhiteSpace(changeModeButtonLabel) ? "Change Mode" : changeModeButtonLabel;
            continueButtonLabel = string.IsNullOrWhiteSpace(continueButtonLabel) ? "Continue" : continueButtonLabel;
            buildMenuTitle = string.IsNullOrWhiteSpace(buildMenuTitle) ? "Build" : buildMenuTitle;
            runSummaryTitle = string.IsNullOrWhiteSpace(runSummaryTitle) ? "Run Summary" : runSummaryTitle;
            tutorialTitle = string.IsNullOrWhiteSpace(tutorialTitle) ? "Survivor Primer" : tutorialTitle;
            themeSelectorTitle = string.IsNullOrWhiteSpace(themeSelectorTitle) ? "Theme" : themeSelectorTitle;
            showTutorialButtonLabel = string.IsNullOrWhiteSpace(showTutorialButtonLabel) ? "Show Tutorial" : showTutorialButtonLabel;
            debugToggleLabel = string.IsNullOrWhiteSpace(debugToggleLabel) ? "Debug Overlay" : debugToggleLabel;
            iconPlaceholderPrefix = string.IsNullOrWhiteSpace(iconPlaceholderPrefix) ? "Icon" : iconPlaceholderPrefix;
            hudAccentColor = string.IsNullOrWhiteSpace(hudAccentColor) ? "#33C7FF" : hudAccentColor;
            buttonStyleToken = string.IsNullOrWhiteSpace(buttonStyleToken) ? "solid" : buttonStyleToken;
            rarityStyles = MergeRarityFallbacks(rarityStyles);
            categoryStyles = MergeCategoryFallbacks(categoryStyles);
            tutorialSteps = MergeTutorialFallbacks(tutorialSteps);
            audioEvents = MergeAudioFallbacks(audioEvents);
        }

        private RarityStyleToken FindRarityStyle(string rarityId)
        {
            if (rarityStyles == null || string.IsNullOrWhiteSpace(rarityId))
            {
                return null;
            }

            for (int i = 0; i < rarityStyles.Length; i++)
            {
                RarityStyleToken token = rarityStyles[i];
                if (token != null && string.Equals(token.id, rarityId, StringComparison.OrdinalIgnoreCase))
                {
                    return token;
                }
            }

            return null;
        }

        private CategoryStyleToken FindCategoryStyle(string categoryId)
        {
            if (categoryStyles == null || string.IsNullOrWhiteSpace(categoryId))
            {
                return null;
            }

            for (int i = 0; i < categoryStyles.Length; i++)
            {
                CategoryStyleToken token = categoryStyles[i];
                if (token != null && string.Equals(token.id, categoryId, StringComparison.OrdinalIgnoreCase))
                {
                    return token;
                }
            }

            return null;
        }

        private static RarityStyleToken[] MergeRarityFallbacks(RarityStyleToken[] configured)
        {
            RarityStyleToken[] fallback =
            {
                new RarityStyleToken("Common", "Common", "common-frame", "#C8D5DC", "dot"),
                new RarityStyleToken("Uncommon", "Uncommon", "uncommon-frame", "#58F08F", "leaf"),
                new RarityStyleToken("Rare", "Rare", "rare-frame", "#56B3FF", "star"),
                new RarityStyleToken("Epic", "Epic", "epic-frame", "#E875FF", "diamond"),
                new RarityStyleToken("Legendary", "Legendary", "legendary-frame", "#FFC347", "crown"),
                new RarityStyleToken("Evolution", "Evolution", "evolution-banner", "#FF5F8F", "sigil"),
                new RarityStyleToken("Relic", "Relic", "relic-frame", "#FFD56A", "relic")
            };

            return MergeRarityTokens(configured, fallback);
        }

        private static CategoryStyleToken[] MergeCategoryFallbacks(CategoryStyleToken[] configured)
        {
            CategoryStyleToken[] fallback =
            {
                new CategoryStyleToken("NewWeapon", "New Weapon", "weapon-new"),
                new CategoryStyleToken("WeaponUpgrade", "Weapon Upgrade", "weapon-rank"),
                new CategoryStyleToken("Passive", "Passive", "passive"),
                new CategoryStyleToken("PassiveUpgrade", "Passive Upgrade", "passive-rank"),
                new CategoryStyleToken("Mutation", "Mutation", "mutation"),
                new CategoryStyleToken("Evolution", "Evolution", "evolution"),
                new CategoryStyleToken("Relic", "Relic", "relic"),
                new CategoryStyleToken("PickupMagnet", "Pickup/Magnet", "magnet"),
                new CategoryStyleToken("MetaReward", "Meta/Reward", "reward")
            };

            return MergeCategoryTokens(configured, fallback);
        }

        private static AudioEventStyleToken[] MergeAudioFallbacks(AudioEventStyleToken[] configured)
        {
            AudioEventStyleToken[] fallback =
            {
                new AudioEventStyleToken("ui.hover", "UI", 0.16f, 0.08f),
                new AudioEventStyleToken("ui.select", "UI", 0.22f, 0.04f),
                new AudioEventStyleToken("mode.selected", "UI", 0.28f, 0.05f),
                new AudioEventStyleToken("draft.opened", "Rewards", 0.34f, 0.1f),
                new AudioEventStyleToken("draft.choice.selected", "Rewards", 0.36f, 0.06f),
                new AudioEventStyleToken("draft.reroll", "UI", 0.24f, 0.08f),
                new AudioEventStyleToken("draft.banish", "UI", 0.24f, 0.08f),
                new AudioEventStyleToken("draft.skip", "UI", 0.2f, 0.08f),
                new AudioEventStyleToken("level.up", "Rewards", 0.38f, 0.12f),
                new AudioEventStyleToken("pickup.xp", "Rewards", 0.16f, 0.12f),
                new AudioEventStyleToken("pickup.magnet_pulse", "Rewards", 0.26f, 0.4f),
                new AudioEventStyleToken("combat.hit", "Combat", 0.12f, 0.08f),
                new AudioEventStyleToken("combat.enemy_death", "Combat", 0.18f, 0.08f),
                new AudioEventStyleToken("warning.elite", "Warnings", 0.28f, 0.35f),
                new AudioEventStyleToken("warning.boss", "Warnings", 0.38f, 0.35f),
                new AudioEventStyleToken("warning.low_health", "Warnings", 0.3f, 0.5f),
                new AudioEventStyleToken("reward.evolution", "Rewards", 0.42f, 0.08f),
                new AudioEventStyleToken("reward.relic", "Rewards", 0.38f, 0.08f),
                new AudioEventStyleToken("run.victory", "Rewards", 0.45f, 0.5f),
                new AudioEventStyleToken("run.defeat", "Warnings", 0.38f, 0.5f),
                new AudioEventStyleToken("run.summary.opened", "UI", 0.32f, 0.25f)
            };

            var merged = new AudioEventStyleToken[fallback.Length];
            for (int i = 0; i < fallback.Length; i++)
            {
                merged[i] = CopyAudioToken(FindConfiguredAudio(configured, fallback[i].id) ?? fallback[i], fallback[i]);
            }

            return merged;
        }

        private static TutorialStepStyleToken[] MergeTutorialFallbacks(TutorialStepStyleToken[] configured)
        {
            TutorialStepStyleToken[] fallback =
            {
                new TutorialStepStyleToken("combat", "Move And Survive", new[]
                {
                    "Move with WASD or the left stick. Your weapons fire automatically at nearby enemies.",
                    "Use Arc Step to dash through pressure, shove enemies back, and buy a short safety window.",
                    "The goal is not to stand still. Kite, collect, and keep the horde just barely under control."
                }),
                new TutorialStepStyleToken("experience", "Collect XP Gems", new[]
                {
                    "Enemies drop blue XP gems. Move near them to pull them in and fill the level bar.",
                    "Magnet pickups and pickup-radius upgrades help recover loose gems without flooding drafts.",
                    "Streak rewards, horde clears, and waystones can add extra pickups when you play actively."
                }),
                new TutorialStepStyleToken("drafts", "Choose A Build", new[]
                {
                    "Level-ups pause the run and offer draft cards. Pick weapons, passives, mutations, and evolutions.",
                    "Reroll changes the offered cards, Banish removes a card for the run, and Skip grants blood shards.",
                    "Open the Build panel to compare current weapons, passives, relics, run info, and controls."
                }),
                new TutorialStepStyleToken("threats", "Elites, Bosses, Rewards", new[]
                {
                    "Elites, dread elites, minibosses, and bosses keep their health, show bars, and stay tracked offscreen.",
                    "Major threats drop recoverable reward caches, relic drafts, upgrade drafts, or class unlock rewards.",
                    "Warnings, slam markers, and support-call banners tell you when the arena is about to spike."
                }),
                new TutorialStepStyleToken("evolutions", "Evolve Weapons", new[]
                {
                    "Evolutions need a ranked weapon path plus its matching passive. Ready banners call out missing pieces.",
                    "Evolution picks trigger big payoff surges, XP recall, and stronger weapon behavior.",
                    "Multiple evolutions can stack into a late-run Legend Surge."
                }),
                new TutorialStepStyleToken("modes", "Pick A Run Mode", new[]
                {
                    "Standard / Human Playtest is the full 30-minute arc with victory, boss rewards, and endless continuation.",
                    "Sprint Run compresses the game into 5 minutes with quicker XP, early elites, a faster boss climax, and fast restart.",
                    "After victory or defeat you can restart the same mode or return to mode selection."
                })
            };

            var merged = new TutorialStepStyleToken[fallback.Length];
            for (int i = 0; i < fallback.Length; i++)
            {
                merged[i] = CopyTutorialToken(FindConfiguredTutorial(configured, fallback[i].id) ?? fallback[i], fallback[i]);
            }

            return merged;
        }

        private static RarityStyleToken[] MergeRarityTokens(RarityStyleToken[] configured, RarityStyleToken[] fallback)
        {
            var merged = new RarityStyleToken[fallback.Length];
            for (int i = 0; i < fallback.Length; i++)
            {
                merged[i] = CopyRarityToken(FindConfiguredRarity(configured, fallback[i].id) ?? fallback[i], fallback[i]);
            }

            return merged;
        }

        private static CategoryStyleToken[] MergeCategoryTokens(CategoryStyleToken[] configured, CategoryStyleToken[] fallback)
        {
            var merged = new CategoryStyleToken[fallback.Length];
            for (int i = 0; i < fallback.Length; i++)
            {
                merged[i] = CopyCategoryToken(FindConfiguredCategory(configured, fallback[i].id) ?? fallback[i], fallback[i]);
            }

            return merged;
        }

        private static RarityStyleToken FindConfiguredRarity(RarityStyleToken[] configured, string id)
        {
            if (configured == null)
            {
                return null;
            }

            for (int i = 0; i < configured.Length; i++)
            {
                if (configured[i] != null && string.Equals(configured[i].id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return configured[i];
                }
            }

            return null;
        }

        private static CategoryStyleToken FindConfiguredCategory(CategoryStyleToken[] configured, string id)
        {
            if (configured == null)
            {
                return null;
            }

            for (int i = 0; i < configured.Length; i++)
            {
                if (configured[i] != null && string.Equals(configured[i].id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return configured[i];
                }
            }

            return null;
        }

        private static AudioEventStyleToken FindConfiguredAudio(AudioEventStyleToken[] configured, string id)
        {
            if (configured == null)
            {
                return null;
            }

            for (int i = 0; i < configured.Length; i++)
            {
                if (configured[i] != null && string.Equals(configured[i].id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return configured[i];
                }
            }

            return null;
        }

        private TutorialStepStyleToken GetTutorialStep(int stepIndex)
        {
            if (tutorialSteps == null || stepIndex < 0 || stepIndex >= tutorialSteps.Length)
            {
                return null;
            }

            return tutorialSteps[stepIndex];
        }

        private static TutorialStepStyleToken FindConfiguredTutorial(TutorialStepStyleToken[] configured, string id)
        {
            if (configured == null)
            {
                return null;
            }

            for (int i = 0; i < configured.Length; i++)
            {
                if (configured[i] != null && string.Equals(configured[i].id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return configured[i];
                }
            }

            return null;
        }

        private static RarityStyleToken CopyRarityToken(RarityStyleToken source, RarityStyleToken fallback)
        {
            return new RarityStyleToken(
                string.IsNullOrWhiteSpace(source.id) ? fallback.id : source.id,
                string.IsNullOrWhiteSpace(source.displayName) ? fallback.displayName : source.displayName,
                string.IsNullOrWhiteSpace(source.styleToken) ? fallback.styleToken : source.styleToken,
                string.IsNullOrWhiteSpace(source.accentColor) ? fallback.accentColor : source.accentColor,
                string.IsNullOrWhiteSpace(source.iconId) ? fallback.iconId : source.iconId);
        }

        private static CategoryStyleToken CopyCategoryToken(CategoryStyleToken source, CategoryStyleToken fallback)
        {
            return new CategoryStyleToken(
                string.IsNullOrWhiteSpace(source.id) ? fallback.id : source.id,
                string.IsNullOrWhiteSpace(source.displayName) ? fallback.displayName : source.displayName,
                string.IsNullOrWhiteSpace(source.iconId) ? fallback.iconId : source.iconId);
        }

        private static AudioEventStyleToken CopyAudioToken(AudioEventStyleToken source, AudioEventStyleToken fallback)
        {
            return new AudioEventStyleToken(
                string.IsNullOrWhiteSpace(source.id) ? fallback.id : source.id,
                string.IsNullOrWhiteSpace(source.category) ? fallback.category : source.category,
                source.volume < 0f ? fallback.volume : source.volume,
                source.throttleSeconds < 0f ? fallback.throttleSeconds : source.throttleSeconds);
        }

        private static TutorialStepStyleToken CopyTutorialToken(TutorialStepStyleToken source, TutorialStepStyleToken fallback)
        {
            return new TutorialStepStyleToken(
                string.IsNullOrWhiteSpace(source.id) ? fallback.id : source.id,
                string.IsNullOrWhiteSpace(source.title) ? fallback.title : source.title,
                source.lines == null || source.lines.Length == 0 ? fallback.lines : source.lines);
        }
    }

    [Serializable]
    public sealed class RarityStyleToken
    {
        public string id;
        public string displayName;
        public string styleToken;
        public string accentColor;
        public string iconId;

        public RarityStyleToken()
        {
        }

        public RarityStyleToken(string id, string displayName, string styleToken, string accentColor, string iconId)
        {
            this.id = id;
            this.displayName = displayName;
            this.styleToken = styleToken;
            this.accentColor = accentColor;
            this.iconId = iconId;
        }
    }

    [Serializable]
    public sealed class CategoryStyleToken
    {
        public string id;
        public string displayName;
        public string iconId;

        public CategoryStyleToken()
        {
        }

        public CategoryStyleToken(string id, string displayName, string iconId)
        {
            this.id = id;
            this.displayName = displayName;
            this.iconId = iconId;
        }
    }

    [Serializable]
    public sealed class AudioEventStyleToken
    {
        public string id;
        public string category;
        public float volume;
        public float throttleSeconds;

        public AudioEventStyleToken()
        {
            volume = -1f;
            throttleSeconds = -1f;
        }

        public AudioEventStyleToken(string id, string category, float volume, float throttleSeconds)
        {
            this.id = id;
            this.category = category;
            this.volume = volume;
            this.throttleSeconds = throttleSeconds;
        }
    }

    [Serializable]
    public sealed class TutorialStepStyleToken
    {
        public string id;
        public string title;
        public string[] lines;

        public TutorialStepStyleToken()
        {
        }

        public TutorialStepStyleToken(string id, string title, string[] lines)
        {
            this.id = id;
            this.title = title;
            this.lines = lines ?? Array.Empty<string>();
        }
    }
}
