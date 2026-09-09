namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsHudRenderPort
    {
        bool DebugVisible { get; }
        void EnsureStyles();
        void DrawModeSelection();
        void DrawTutorial();
        void DrawTimer();
        void DrawPlayer();
        void DrawDebug();
        void DrawBuildHud();
        void DrawRunFeedback();
        void DrawDraft();
        void DrawResult(bool victory);
        void DrawBuildMenu();
    }

    /// <summary>Owns screen visibility and overlay priority; renderers and menu state remain borrowed.</summary>
    internal sealed class SurvivorsHudDispatch
    {
        private readonly SurvivorsRunSession _session;
        private readonly SurvivorsMenuSession _menus;
        private readonly ISurvivorsHudRenderPort _port;
        public SurvivorsHudDispatch(SurvivorsRunSession session, SurvivorsMenuSession menus, ISurvivorsHudRenderPort port)
        { _session = session; _menus = menus; _port = port; }

        public void Draw()
        {
            if (!_session.Started)
            {
                if (_menus.ModeSelectionOpen) { _port.EnsureStyles(); _port.DrawModeSelection(); }
                if (_menus.TutorialOpen) { _port.EnsureStyles(); _port.DrawTutorial(); }
                return;
            }
            _port.EnsureStyles();
            _port.DrawTimer();
            _port.DrawPlayer();
            if (_port.DebugVisible) { _port.DrawDebug(); _port.DrawBuildHud(); }
            _port.DrawRunFeedback();
            if (_menus.TutorialOpen) { _port.DrawTutorial(); return; }
            if (_session.State == SurvivorsRunState.LevelUp) _port.DrawDraft();
            else if (_session.State == SurvivorsRunState.GameOver) _port.DrawResult(false);
            else if (_session.State == SurvivorsRunState.Victory) _port.DrawResult(true);
            if (_menus.BuildOpen && _session.State == SurvivorsRunState.Playing) _port.DrawBuildMenu();
        }
    }
}
