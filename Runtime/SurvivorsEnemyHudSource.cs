using System.Collections;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Reads current actor values without caching another authoritative enemy collection.</summary>
    internal sealed class SurvivorsEnemyHudSource : IReadOnlyList<SurvivorsThreatHudItem>
    {
        private readonly IReadOnlyList<SurvivorsEnemyActor> _enemies;
        public SurvivorsEnemyHudSource(IReadOnlyList<SurvivorsEnemyActor> enemies) => _enemies = enemies;
        public int Count => _enemies.Count;
        public SurvivorsThreatHudItem this[int index]
        {
            get
            {
                SurvivorsEnemyActor enemy = _enemies[index];
                return enemy == null ? default : new SurvivorsThreatHudItem(
                    enemy.Role, enemy.DisplayName, enemy.HealthFraction, enemy.transform.position,
                    enemy.Radius, enemy.IsAlive, enemy.ShowBossLifeBar, enemy.ShowOverheadLifeBar, enemy.ShowOffscreenMarker);
            }
        }

        public IEnumerator<SurvivorsThreatHudItem> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
