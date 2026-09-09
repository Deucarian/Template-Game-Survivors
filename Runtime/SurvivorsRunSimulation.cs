using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsSimulationFrame
    {
        public readonly float DeltaTime;
        public readonly Vector2 Movement;
        public SurvivorsSimulationFrame(float deltaTime, Vector2 movement) { DeltaTime = deltaTime; Movement = movement; }
    }
    /// <summary>Owns simulation phase gates and clock advancement over explicitly ordered, owner-bound commands.</summary>
    internal sealed class SurvivorsRunSimulation
    {
        private readonly SurvivorsRunSession _session;
        private readonly SurvivorsMenuSession _menus;
        private readonly Action<float> _presentation, _draftTimeout;
        private readonly IReadOnlyList<Action<SurvivorsSimulationFrame>> _progress, _playing;
        public SurvivorsRunSimulation(SurvivorsRunSession session, SurvivorsMenuSession menus, Action<float> presentation,
            Action<float> draftTimeout, IReadOnlyList<Action<SurvivorsSimulationFrame>> progress, IReadOnlyList<Action<SurvivorsSimulationFrame>> playing)
        { _session = session; _menus = menus; _presentation = presentation; _draftTimeout = draftTimeout; _progress = progress; _playing = playing; }
        public void Simulate(float deltaTime, Vector2 movement)
        {
            if (!_session.Started || (_menus.BuildOpen && _session.State == SurvivorsRunState.Playing) || _menus.TutorialOpen) return;
            float dt = Mathf.Max(0f, deltaTime);
            _presentation(dt);
            if (_session.State == SurvivorsRunState.LevelUp) { _draftTimeout(dt); return; }
            if (_session.State != SurvivorsRunState.Playing) return;
            _session.Tick(dt);
            var frame = new SurvivorsSimulationFrame(dt, movement);
            for (int i = 0; i < _progress.Count; i++) _progress[i](frame);
            if (_session.State != SurvivorsRunState.Playing) return;
            for (int i = 0; i < _playing.Count; i++) _playing[i](frame);
        }
    }
}
