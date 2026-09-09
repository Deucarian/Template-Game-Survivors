using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns keyboard shortcut priority, debug visibility and input routing across run/menu phases.</summary>
    internal sealed class SurvivorsFrameInput
    {
        private readonly SurvivorsRunSession _session;
        private readonly SurvivorsMenuSession _menus;
        private readonly ISurvivorsKeyboard _keys;
        private readonly ISurvivorsFrameInputPort _commands;
        private SurvivorsRunState State => _session.State;
        public bool DebugVisible { get; set; }
        public SurvivorsFrameInput(SurvivorsRunSession session, SurvivorsMenuSession menus, ISurvivorsKeyboard keys, ISurvivorsFrameInputPort commands)
        { _session = session; _menus = menus; _keys = keys; _commands = commands; }
        private bool HandleBuildMenuInput() => _menus.HandleBuildInput(State,
            _keys.Pressed(KeyCode.Escape) || _keys.Pressed(KeyCode.Tab) || _keys.Pressed(KeyCode.B),
            _keys.Pressed(KeyCode.Alpha1) ? 0 : _keys.Pressed(KeyCode.Alpha2) ? 1 : _keys.Pressed(KeyCode.Alpha3) ? 2 : _keys.Pressed(KeyCode.Alpha4) ? 3 : -1);
        public void Tick(float deltaTime)
        {
            if (_keys.Pressed(KeyCode.F1))
            {
                DebugVisible = !DebugVisible;
            }

            if (!_session.Started)
            {
                if (_menus.TutorialOpen)
                {
                    HandleTutorialInput();
                }
                else if (_menus.ModeSelectionOpen)
                {
                    HandleRunModeSelectionInput();
                }

                return;
            }

            if (_menus.TutorialOpen)
            {
                HandleTutorialInput();
                return;
            }

            if (HandleBuildMenuInput())
            {
                return;
            }

            if (State == SurvivorsRunState.LevelUp)
            {
                _commands.TickPresentation(deltaTime);
                _commands.TickRewardSelectionTimeout(deltaTime);
                HandleLevelUpInput();
                return;
            }

            if (State == SurvivorsRunState.GameOver || State == SurvivorsRunState.Victory)
            {
                _commands.TickPresentation(deltaTime);
                if (HandleResultMetaUpgradeInput())
                {
                    return;
                }

                if (State == SurvivorsRunState.Victory && _keys.Pressed(KeyCode.C))
                {
                    _commands.ContinueAfterVictory();
                    return;
                }

                if (_keys.Pressed(KeyCode.R))
                {
                    _commands.RestartRun();
                }
                return;
            }

            Vector2 movement = ReadMovementInput();
            if (_keys.Pressed(KeyCode.Space))
            {
                _commands.Dash(movement);
            }

            _commands.Simulate(deltaTime, movement);

            if (_keys.Pressed(KeyCode.M))
            {
                _commands.TriggerMagnetRecall();
            }
        }

        private void HandleRunModeSelectionInput()
        {
            if (_keys.Pressed(KeyCode.Alpha1) || _keys.Pressed(KeyCode.Return))
            {
                _commands.SelectMode(SurvivorsPacingProfile.HumanPlaytest);
            }
            else if (_keys.Pressed(KeyCode.Alpha2) || _keys.Pressed(KeyCode.S))
            {
                _commands.SelectMode(SurvivorsPacingProfile.SprintRun);
            }
            else if (_keys.Pressed(KeyCode.T))
            {
                _menus.OpenTutorialOverlay(markUnseen: false);
            }
        }

        private void HandleTutorialInput()
        {
            if (_keys.Pressed(KeyCode.Escape) || _keys.Pressed(KeyCode.S))
            {
                _menus.CloseTutorialOverlay(markSeen: true);
            }
            else if (_keys.Pressed(KeyCode.RightArrow) || _keys.Pressed(KeyCode.Space) || _keys.Pressed(KeyCode.Return))
            {
                _menus.AdvanceTutorialStep();
            }
            else if (_keys.Pressed(KeyCode.LeftArrow) || _keys.Pressed(KeyCode.Backspace))
            {
                _menus.BackTutorialStep();
            }
        }

        private void HandleLevelUpInput()
        {
            bool banishModifier = _keys.Held(KeyCode.LeftShift) || _keys.Held(KeyCode.RightShift);
            if (banishModifier && _keys.Pressed(KeyCode.Alpha1))
            {
                _commands.BanishDraftChoice(0);
            }
            else if (banishModifier && _keys.Pressed(KeyCode.Alpha2))
            {
                _commands.BanishDraftChoice(1);
            }
            else if (banishModifier && _keys.Pressed(KeyCode.Alpha3))
            {
                _commands.BanishDraftChoice(2);
            }
            else if (_keys.Pressed(KeyCode.R))
            {
                _commands.RerollCurrentDraft();
            }
            else if (_keys.Pressed(KeyCode.S))
            {
                _commands.SkipCurrentDraft();
            }
            else if (_keys.Pressed(KeyCode.Alpha1))
            {
                _commands.SelectUpgrade(0);
            }
            else if (_keys.Pressed(KeyCode.Alpha2))
            {
                _commands.SelectUpgrade(1);
            }
            else if (_keys.Pressed(KeyCode.Alpha3))
            {
                _commands.SelectUpgrade(2);
            }
        }

        private bool HandleResultMetaUpgradeInput()
        {
            if (_keys.Pressed(KeyCode.Alpha1))
            {
                return _commands.TryPurchaseResultMetaUpgrade(0);
            }

            if (_keys.Pressed(KeyCode.Alpha2))
            {
                return _commands.TryPurchaseResultMetaUpgrade(1);
            }

            if (_keys.Pressed(KeyCode.Alpha3))
            {
                return _commands.TryPurchaseResultMetaUpgrade(2);
            }

            return false;
        }

        private Vector2 ReadMovementInput()
        {
            float x = 0f;
            float y = 0f;
            if (_keys.Held(KeyCode.A) || _keys.Held(KeyCode.LeftArrow)) x -= 1f;
            if (_keys.Held(KeyCode.D) || _keys.Held(KeyCode.RightArrow)) x += 1f;
            if (_keys.Held(KeyCode.S) || _keys.Held(KeyCode.DownArrow)) y -= 1f;
            if (_keys.Held(KeyCode.W) || _keys.Held(KeyCode.UpArrow)) y += 1f;
            return new Vector2(x, y);
        }
    }
}
