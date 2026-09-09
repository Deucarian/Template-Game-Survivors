using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Template tutorial fallback copy and step indexing; themes may replace this copy.</summary>
    internal static class SurvivorsTutorialContent
    {
        public static int StepCount => Enum.GetValues(typeof(TutorialStep)).Length;
        private enum TutorialStep
        {
            Movement = 0,
            Combat = 1,
            Experience = 2,
            Drafts = 3,
            ElitesBosses = 4,
            Evolutions = 5,
            Modes = 6
        }

        public static int ClampTutorialStepIndex(int index)
        {
            int max = Enum.GetValues(typeof(TutorialStep)).Length - 1;
            return Mathf.Clamp(index, 0, Mathf.Max(0, max));
        }

        public static string ResolveDefaultTutorialStepTitle(int step)
        {
            switch ((TutorialStep)ClampTutorialStepIndex(step))
            {
                case TutorialStep.Combat:
                    return "Move And Survive";
                case TutorialStep.Experience:
                    return "Collect XP Gems";
                case TutorialStep.Drafts:
                    return "Choose A Build";
                case TutorialStep.ElitesBosses:
                    return "Elites, Bosses, Rewards";
                case TutorialStep.Evolutions:
                    return "Evolve Weapons";
                case TutorialStep.Modes:
                    return "Pick A Run Mode";
                default:
                    return "Move To Survive";
            }
        }

        public static IReadOnlyList<string> ResolveDefaultTutorialStepLines(int step)
        {
            switch ((TutorialStep)ClampTutorialStepIndex(step))
            {
                case TutorialStep.Combat:
                    return new[] { "Move with WASD or the left stick. Your weapons fire automatically at nearby enemies.", "Use Arc Step to dash through pressure, shove enemies back, and buy a short safety window.", "The goal is not to stand still. Kite, collect, and keep the horde just barely under control." };
                case TutorialStep.Experience:
                    return new[] { "Enemies drop blue XP gems. Move near them to pull them in and fill the level bar.", "Magnet pickups and pickup-radius upgrades help recover loose gems without flooding drafts.", "Streak rewards, horde clears, and waystones can add extra pickups when you play actively." };
                case TutorialStep.Drafts:
                    return new[] { "Level-ups pause the run and offer draft cards. Pick weapons, passives, mutations, and evolutions.", "Reroll changes the offered cards, Banish removes a card for the run, and Skip grants blood shards.", "Open the Build panel to compare current weapons, passives, relics, run info, and controls." };
                case TutorialStep.ElitesBosses:
                    return new[] { "Elites, dread elites, minibosses, and bosses keep their health, show bars, and stay tracked offscreen.", "Major threats drop recoverable reward caches, relic drafts, upgrade drafts, or class unlock rewards.", "Warnings, slam markers, and support-call banners tell you when the arena is about to spike." };
                case TutorialStep.Evolutions:
                    return new[] { "Evolutions need a ranked weapon path plus its matching passive. Ready banners call out missing pieces.", "Evolution picks trigger big payoff surges, XP recall, and stronger weapon behavior.", "Multiple evolutions can stack into a late-run Legend Surge." };
                case TutorialStep.Modes:
                    return new[] { "Standard / Human Playtest is the full 30-minute arc with victory, boss rewards, and endless continuation.", "Sprint Run compresses the game into 5 minutes with quicker XP, early elites, a faster boss climax, and fast restart.", "After victory or defeat you can restart the same mode or return to mode selection." };
                default:
                    return new[] { "Move to survive. Standing still becomes dangerous unless your build is already overpowering the arena.", "Arc Step with Space can buy room when enemies get close." };
            }
        }
    }
}
