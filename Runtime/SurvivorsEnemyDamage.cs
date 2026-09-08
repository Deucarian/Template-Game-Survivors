using System;
using Deucarian.Combat;
using Deucarian.GameplayFoundation;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns the seeded critical stream and borrows live critical values from the run build.</summary>
    internal sealed class SurvivorsEnemyDamage
    {
        private readonly Func<float> _criticalChance;
        private readonly Func<float> _criticalMultiplier;
        private DeterministicRandom _combatRandom;
        public CombatCatalog Catalog { get; private set; }

        public SurvivorsEnemyDamage(Func<float> criticalChance, Func<float> criticalMultiplier)
        {
            _criticalChance = criticalChance ?? throw new ArgumentNullException(nameof(criticalChance));
            _criticalMultiplier = criticalMultiplier ?? throw new ArgumentNullException(nameof(criticalMultiplier));
        }

        public void Reset(int runSeed)
        {
            Catalog = BasicSurvivorsGame.CreateCombatCatalog();
            _combatRandom = new DeterministicRandom(runSeed + 4049);
        }

        public DamageResolutionResult ResolveEnemyDamage(HealthState health, float amount, string source, bool applyAugments)
        {
            if (health == null)
            {
                return null;
            }

            CombatSourceSnapshot combatSource = CreateEnemyDamageSourceSnapshot(source, applyAugments);
            bool allowCritical = combatSource != null;
            DamageRequest request = new DamageRequest(
                health.Id,
                new[] { new DamageComponent(BasicSurvivorsGame.ArcaneDamageType, amount) },
                source: combatSource,
                sourceId: new CombatantId(string.IsNullOrWhiteSpace(source) ? "combatant.survivors.player" : source),
                preResolvedCritical: allowCritical ? (bool?)null : false);
            return CombatDamageResolver.Resolve(Catalog, health, null, request, allowCritical ? _combatRandom : null);
        }

        private CombatSourceSnapshot CreateEnemyDamageSourceSnapshot(string source, bool applyAugments)
        {
            if (!applyAugments || !CanApplyDamageAugments(source) || _criticalChance() <= 0f)
            {
                return null;
            }

            return new CombatSourceSnapshot(_criticalChance(), _criticalMultiplier());
        }

        public static bool CanApplyDamageAugments(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return true;
            }

            return source.IndexOf(".status.", StringComparison.Ordinal) < 0 &&
                source.IndexOf(".augment.", StringComparison.Ordinal) < 0 &&
                source.IndexOf("combatant.survivors.enemy", StringComparison.Ordinal) < 0;
        }

    }
}
