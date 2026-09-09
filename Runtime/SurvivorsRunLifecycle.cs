namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsRunLifecyclePort
    {
        bool CanStart { get; }
        bool EndlessEnabled { get; }
        void SetStartupFlags(bool modeSelection, bool autoStart);
        void DisableAutoStart();
        void ResetDebugVisibility();
        void ResetScreenScrolls();
        void EnsureTheme();
        void RestoreTimeScale();
        void ReleaseRun();
        void InitializeRun();
        void ClearDrafts();
        void ApplyPacing(SurvivorsPacingProfile profile);
        void PlayModeSelected();
        void ScheduleContinuation();
        void PlayContinuation();
        void ConsumeBossVictory();
        void GrantVictoryRewards();
        void PlayVictory();
    }

    /// <summary>Owns run admission and lifecycle transitions; borrowed owners retain their state and resources.</summary>
    internal sealed class SurvivorsRunLifecycle
    {
        private readonly SurvivorsRunSession _session;
        private readonly SurvivorsMenuSession _menus;
        private readonly ISurvivorsRunLifecyclePort _port;
        public SurvivorsRunLifecycle(SurvivorsRunSession session, SurvivorsMenuSession menus, ISurvivorsRunLifecyclePort port)
        { _session = session; _menus = menus; _port = port; }

        public void StartAutomatically(bool showModeSelection, bool autoStart)
        {
            if (showModeSelection && !_session.Started)
            {
                _menus.ModeSelectionOpen = true;
                _port.DisableAutoStart();
                return;
            }
            if (autoStart && !_session.Started) StartRun();
        }

        public void ConfigureModeSelection(bool enabled)
        {
            _port.SetStartupFlags(enabled, !enabled);
            if (!_session.Started)
            {
                _menus.ModeSelectionOpen = enabled;
                _session.OpenModeSelection();
            }
        }

        public void OpenModeSelection()
        {
            if (_session.Started) _port.ReleaseRun();
            _port.ClearDrafts();
            _port.SetStartupFlags(true, false);
            ResetScreens();
            _menus.ModeSelectionOpen = true;
            _session.OpenModeSelection();
        }

        public bool SelectMode(SurvivorsPacingProfile profile)
        {
            if (!AdmitRun()) return false;
            _port.ApplyPacing(profile);
            _menus.ModeSelectionOpen = false;
            StartRun();
            _port.PlayModeSelected();
            return true;
        }

        public void StartRun()
        {
            if (!AdmitRun()) return;
            _port.EnsureTheme();
            _port.RestoreTimeScale();
            _port.ReleaseRun();
            _menus.ModeSelectionOpen = false;
            ResetScreens();
            _port.InitializeRun();
            _session.Start();
            _menus.TryOpenFirstRunTutorial();
        }

        public bool ContinueAfterVictory()
        {
            if (!_session.ContinueAfterVictory(_port.EndlessEnabled)) return false;
            _port.ClearDrafts();
            _menus.TutorialOpen = false;
            _session.ResumePlaying();
            _port.ScheduleContinuation();
            _port.PlayContinuation();
            return true;
        }

        public void EnterVictory()
        {
            if (_session.State == SurvivorsRunState.Victory || _session.State == SurvivorsRunState.GameOver) return;
            _port.ConsumeBossVictory();
            _port.GrantVictoryRewards();
            _port.ClearDrafts();
            _session.Win();
            _port.PlayVictory();
        }

        private bool AdmitRun()
        {
            if (_port.CanStart) return true;
            _menus.ModeSelectionOpen = true;
            _session.OpenModeSelection();
            return false;
        }

        private void ResetScreens()
        {
            _port.ResetDebugVisibility();
            _menus.BuildOpen = false;
            _menus.BuildTab = BuildMenuTab.CurrentBuild;
            _port.ResetScreenScrolls();
            _menus.TutorialOpen = false;
            _menus.TutorialIndex = 0;
        }
    }
}
