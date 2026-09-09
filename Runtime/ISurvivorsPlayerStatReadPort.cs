using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsPlayerStatReadPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        SurvivorsUpgradeModifiers Modifiers { get; }
        SurvivorsPlayerSurgeValues MovementSurges { get; }
        SurvivorsPlayerSurgeValues PickupSurges { get; }
        SurvivorsRunState State { get; }
        float MaxHealth { get; }
        float CurrentHealth { get; }
    }

    internal interface ISurvivorsHealthPickupDropPort
    {
        bool IsHealthBound { get; }
        int HealAmount { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool SpawnHealthPickup(Vector3 position, int amount);
    }
}
