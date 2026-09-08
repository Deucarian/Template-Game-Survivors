using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Template run clock, phase, dash and compact build labels over copied observations.</summary>
    internal static class SurvivorsRunText
    {
        public static string FormatRunTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        public static string FormatMetricTime(float seconds)
        {
            return seconds >= 0f ? FormatRunTime(seconds) : "not yet";
        }

        public static string FormatRewardTimeout(float seconds)
        {
            return seconds > 0f ? seconds.ToString("0.#") + "s" : "Off";
        }

        public static string FormatPhase(bool isEndlessRun, SurvivorsRunPhase phase)
        {
            return isEndlessRun ? "Endless" : phase.ToString();
        }

        public static string FormatDash(float cooldownRemainingSeconds, bool isSafetyActive, float safetyRemainingSeconds)
        {
            string cooldown = cooldownRemainingSeconds <= 0.01f
                ? "Ready"
                : cooldownRemainingSeconds.ToString("0.0") + "s";
            string safety = isSafetyActive ? "   Safe " + safetyRemainingSeconds.ToString("0.0") + "s" : string.Empty;
            return "Arc Step " + cooldown + safety;
        }

        public static string FormatBuildSlots(int weaponCount, int maxWeapons, int passiveCount, int maxPassives, int evolutions, int relics, int totalRelics)
        {
            return $"Build W {weaponCount}/{maxWeapons}   P {passiveCount}/{maxPassives}   Evo {evolutions}   Relic {relics}/{totalRelics}";
        }
    }
}
