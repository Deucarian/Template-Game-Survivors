using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsWeaponLoadoutSession : IDisposable
    {
        int WeaponCount { get; }
        IReadOnlyList<string> WeaponIds { get; }
        int ActiveOrbitBladeCount { get; }
        bool ContainsWeapon(string id);
        bool TryAddWeapon(SurvivorsWeaponArchetypeDefinition definition);
        void Tick(float deltaTime);
        bool FireForTest(SurvivorsWeaponArchetype archetype);
    }

    /// <summary>Adapts the existing public runtime without copying its weapon collections.</summary>
    internal sealed class SurvivorsWeaponLoadoutSession : ISurvivorsWeaponLoadoutSession
    {
        private readonly SurvivorsWeaponLoadoutRuntime _runtime;
        internal SurvivorsWeaponLoadoutSession(SurvivorsWeaponLoadoutRuntime runtime)
            => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public int WeaponCount => _runtime.WeaponCount;
        public IReadOnlyList<string> WeaponIds => _runtime.WeaponIds;
        public int ActiveOrbitBladeCount => _runtime.ActiveOrbitBladeCount;
        public bool ContainsWeapon(string id) => _runtime.ContainsWeapon(id);
        public bool TryAddWeapon(SurvivorsWeaponArchetypeDefinition definition) => _runtime.TryAddWeapon(definition);
        public void Tick(float deltaTime) => _runtime.Tick(deltaTime);
        public bool FireForTest(SurvivorsWeaponArchetype archetype) => _runtime.FireForTest(archetype);
        public void Dispose() => _runtime.Dispose();
    }
}
