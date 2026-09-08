using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsKeyboard
    {
        bool Pressed(KeyCode key);
        bool Held(KeyCode key);
    }
    internal sealed class SurvivorsUnityKeyboard : ISurvivorsKeyboard
    {
        public bool Pressed(KeyCode key) => Input.GetKeyDown(key);
        public bool Held(KeyCode key) => Input.GetKey(key);
    }
    internal interface ISurvivorsFrameInputPort
    {
        void TickPresentation(float deltaTime);
        void TickRewardSelectionTimeout(float deltaTime);
        void SelectMode(SurvivorsPacingProfile profile);
        void ContinueAfterVictory();
        void RestartRun();
        void TriggerMagnetRecall();
        void BanishDraftChoice(int index);
        void RerollCurrentDraft();
        void SkipCurrentDraft();
        void SelectUpgrade(int index);
        bool TryPurchaseResultMetaUpgrade(int index);
        void Dash(Vector2 movement);
        void Simulate(float deltaTime, Vector2 movement);
    }
}
