using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>One owner for run lifecycle and elapsed simulation time; scene work stays in the host.</summary>
    internal sealed class SurvivorsRunSession
    {
        public SurvivorsRunState State { get; private set; } = SurvivorsRunState.Booting;
        public bool Started { get; private set; }
        public bool HasClearedVictory { get; private set; }
        public float ElapsedSeconds { get; private set; }

        public void Reset()
        {
            ElapsedSeconds = 0f;
            HasClearedVictory = false;
            Started = false;
            State = SurvivorsRunState.Booting;
        }

        public void Start()
        {
            Started = true;
            State = SurvivorsRunState.Playing;
        }

        public void Stop() => Started = false;
        public void OpenModeSelection() => State = SurvivorsRunState.Booting;
        public void OpenRewardSelection() => State = SurvivorsRunState.LevelUp;
        public void ResumePlaying() => State = SurvivorsRunState.Playing;
        public void Defeat() => State = SurvivorsRunState.GameOver;

        public void Win()
        {
            HasClearedVictory = true;
            State = SurvivorsRunState.Victory;
        }

        public bool ContinueAfterVictory(bool enabled)
        {
            if (State != SurvivorsRunState.Victory || !Started || !enabled)
            {
                return false;
            }

            HasClearedVictory = true;
            State = SurvivorsRunState.Playing;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (Started && State == SurvivorsRunState.Playing)
            {
                ElapsedSeconds += Mathf.Max(0f, deltaTime);
            }
        }
    }
}
