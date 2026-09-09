using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsRunWeaponPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        IReadOnlyList<string> StartingWeaponIds { get; }
        int MaximumSlots { get; }
        SurvivorsWeaponBonusValues DamageBonuses { get; }
        SurvivorsWeaponBonusValues CooldownBonuses { get; }
        IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateDefinitions(SurvivorsTemplateTuning tuning);
        ISurvivorsWeaponLoadoutSession CreateLoadout(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions);
        void WeaponAdded(SurvivorsWeaponArchetypeDefinition definition);
    }
}
