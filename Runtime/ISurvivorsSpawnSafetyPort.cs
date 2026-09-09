using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsSpawnSafetyPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        Vector3 PlayerPosition { get; }
        bool TryResolveCameraGroundRect(float padding, out Rect rect);
    }
}
