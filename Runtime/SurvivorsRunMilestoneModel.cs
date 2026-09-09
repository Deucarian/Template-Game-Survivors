using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Selects the next displayed objective from copied schedules without advancing encounters.</summary>
    internal static class SurvivorsRunMilestoneModel
    {
        public static bool TryResolve(in SurvivorsRunMilestoneValues values, out string name, out float targetTimeSeconds, out float remainingSeconds)
        {
            name = string.Empty;
            targetTimeSeconds = 0f;
            remainingSeconds = 0f;

            if (!values.Started)
            {
                return false;
            }

            if (values.State == SurvivorsRunState.Victory)
            {
                name = "Victory Clear";
                return true;
            }

            if (values.State == SurvivorsRunState.GameOver)
            {
                name = "Run Ended";
                return true;
            }

            float bestTargetTimeSeconds = float.MaxValue;
            string bestName = string.Empty;
            ConsiderRunMilestone(values.RunTimeSeconds, "Horde Rush", values.HordeTime, ref bestName, ref bestTargetTimeSeconds);

            if (values.IsEndlessRun)
            {
                ConsiderRunMilestone(values.RunTimeSeconds, ResolveEndlessMilestoneName(values.Endless.EliteRole), values.Endless.EliteTime, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone(values.RunTimeSeconds, "Endless Miniboss", values.Endless.MinibossTime, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone(values.RunTimeSeconds, "Endless Boss", values.Endless.BossTime, ref bestName, ref bestTargetTimeSeconds);
            }
            else if (values.Normal.HasDefinition)
            {
                ConsiderRunMilestone(values.RunTimeSeconds, "Elite", values.Normal.EliteTime, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone(values.RunTimeSeconds, "Dread Elite", values.Normal.DreadEliteTime, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone(values.RunTimeSeconds, "Miniboss", values.Normal.MinibossTime, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone(values.RunTimeSeconds, "Final Boss", values.Normal.BossTime, ref bestName, ref bestTargetTimeSeconds);
                ConsiderRunMilestone(values.RunTimeSeconds, "Victory", values.Normal.VictoryTime, ref bestName, ref bestTargetTimeSeconds);
            }

            if (string.IsNullOrWhiteSpace(bestName) || bestTargetTimeSeconds == float.MaxValue)
            {
                return false;
            }

            name = bestName;
            targetTimeSeconds = bestTargetTimeSeconds;
            remainingSeconds = Mathf.Max(0f, bestTargetTimeSeconds - values.RunTimeSeconds);
            return true;
        }

        private static void ConsiderRunMilestone(float runTimeSeconds, string candidateName, float candidateTimeSeconds, ref string bestName, ref float bestTargetTimeSeconds)
        {
            if (string.IsNullOrWhiteSpace(candidateName) || candidateTimeSeconds <= 0f || candidateTimeSeconds <= runTimeSeconds)
            {
                return;
            }

            if (candidateTimeSeconds < bestTargetTimeSeconds - 0.001f)
            {
                bestName = candidateName;
                bestTargetTimeSeconds = candidateTimeSeconds;
            }
        }

        private static string ResolveEndlessMilestoneName(SurvivorsEnemyRole role)
        {
            return role == SurvivorsEnemyRole.DreadElite
                ? "Endless Dread Elite"
                : "Endless Elite";
        }

        public static string Label(in SurvivorsRunMilestoneValues values)
        {
            if (!TryResolve(values, out string milestoneName, out _, out float remainingSeconds))
            {
                return "Next Objective: survive";
            }

            if (values.State == SurvivorsRunState.Victory)
            {
                return "Victory Clear - continue or restart";
            }

            if (values.State == SurvivorsRunState.GameOver)
            {
                return "Run Ended - restart to try again";
            }

            return "Next " + milestoneName + " in " + SurvivorsRunText.FormatRunTime(remainingSeconds);
        }
    }
}
