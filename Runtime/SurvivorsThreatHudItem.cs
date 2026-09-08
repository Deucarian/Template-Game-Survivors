using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>A copied threat observation with no actor, transform or gameplay command reference.</summary>
    internal readonly struct SurvivorsThreatHudItem
    {
        public SurvivorsThreatHudItem(SurvivorsEnemyRole role, string name, float healthFraction,
            Vector3 position, float radius, bool alive, bool bossBar, bool overheadBar, bool offscreenMarker)
        {
            Role = role;
            Name = string.IsNullOrWhiteSpace(name) ? SurvivorsThreatHudModel.ResolveFallbackLabel(role) : name;
            HealthFraction = healthFraction;
            Position = position;
            Radius = radius;
            IsAlive = alive;
            ShowBossLifeBar = bossBar;
            ShowOverheadLifeBar = overheadBar;
            ShowOffscreenMarker = offscreenMarker;
        }

        public SurvivorsEnemyRole Role { get; }
        public string Name { get; }
        public float HealthFraction { get; }
        public Vector3 Position { get; }
        public float Radius { get; }
        public bool IsAlive { get; }
        public bool ShowBossLifeBar { get; }
        public bool ShowOverheadLifeBar { get; }
        public bool ShowOffscreenMarker { get; }
        public bool HasLifeBar => IsAlive && (ShowBossLifeBar || ShowOverheadLifeBar);
    }
}
