using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Threat selection, authored visibility and player-facing labels over read-only observations.</summary>
    internal sealed class SurvivorsThreatHudModel
    {
        public SurvivorsThreatHudModel(IReadOnlyList<SurvivorsThreatHudItem> items)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public IReadOnlyList<SurvivorsThreatHudItem> Items { get; }
        public string LastMarkerLabel { get; private set; } = string.Empty;
        public void Reset() => LastMarkerLabel = string.Empty;
        public void RecordMarkerLabel(string label) => LastMarkerLabel = label;

        public int CountLifeBars(bool boss)
        {
            int count = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                SurvivorsThreatHudItem item = Items[i];
                if (item.IsAlive && (boss ? item.ShowBossLifeBar : item.ShowOverheadLifeBar)) count++;
            }
            return count;
        }

        public string LifeBarSummary()
        {
            var labels = new List<string>();
            for (int i = 0; i < Items.Count; i++)
            {
                SurvivorsThreatHudItem item = Items[i];
                if (item.HasLifeBar) labels.Add($"{item.Name} {Mathf.RoundToInt(item.HealthFraction * 100f)}%");
            }
            return string.Join(", ", labels);
        }

        public SurvivorsThreatHudItem? SelectHealthThreat()
        {
            SurvivorsThreatHudItem? selected = null;
            int priority = -1;
            float health = 2f;
            for (int i = 0; i < Items.Count; i++)
            {
                SurvivorsThreatHudItem item = Items[i];
                if (!item.HasLifeBar) continue;
                int candidatePriority = ResolvePriority(item.Role);
                float candidateHealth = Mathf.Clamp01(item.HealthFraction);
                if (candidatePriority > priority || (candidatePriority == priority && candidateHealth < health))
                {
                    selected = item;
                    priority = candidatePriority;
                    health = candidateHealth;
                }
            }
            return selected;
        }

        public int CountMarkers(Vector3 player, float markerDistance)
        {
            int count = 0;
            for (int i = 0; i < Items.Count; i++) if (IsMarkerVisible(Items[i], player, markerDistance)) count++;
            return count;
        }

        public SurvivorsThreatHudItem? SelectMarker(Vector3 player, float markerDistance)
        {
            SurvivorsThreatHudItem? selected = null;
            int priority = -1;
            float distance = 0f;
            for (int i = 0; i < Items.Count; i++)
            {
                SurvivorsThreatHudItem item = Items[i];
                if (!IsMarkerVisible(item, player, markerDistance)) continue;
                int candidatePriority = ResolvePriority(item.Role);
                float candidateDistance = Vector3.Distance(player, item.Position);
                if (candidatePriority > priority || (candidatePriority == priority && candidateDistance > distance))
                {
                    selected = item;
                    priority = candidatePriority;
                    distance = candidateDistance;
                }
            }
            return selected;
        }

        public string MarkerLabel(Vector3 player, float markerDistance)
        {
            SurvivorsThreatHudItem? selected = SelectMarker(player, markerDistance);
            return selected.HasValue ? FormatMarkerLabel(selected.Value, player) : string.Empty;
        }

        public void UpdateLastMarker(Vector3 player, float markerDistance)
        {
            string label = MarkerLabel(player, markerDistance);
            if (!string.IsNullOrWhiteSpace(label)) LastMarkerLabel = label;
        }

        public static bool IsMarkerVisible(SurvivorsThreatHudItem item, Vector3 player, float markerDistance)
        {
            if (!item.IsAlive || !item.ShowOffscreenMarker) return false;
            Vector3 offset = item.Position - player;
            offset.y = 0f;
            float distance = Mathf.Max(0.1f, markerDistance);
            return offset.sqrMagnitude >= distance * distance;
        }

        public static string FormatMarkerLabel(SurvivorsThreatHudItem item, Vector3 player)
        {
            Vector3 delta = item.Position - player;
            delta.y = 0f;
            return $"{item.Name} {CompassDirection(delta)} {delta.magnitude:0}m";
        }

        public static int ResolvePriority(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss: return 4;
                case SurvivorsEnemyRole.Miniboss: return 3;
                case SurvivorsEnemyRole.DreadElite: return 2;
                case SurvivorsEnemyRole.Elite: return 1;
                default: return 0;
            }
        }

        public static string ResolveFallbackLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss: return "Final Boss";
                case SurvivorsEnemyRole.Miniboss: return "Miniboss";
                case SurvivorsEnemyRole.DreadElite: return "Dread Elite";
                default: return "Elite";
            }
        }

        public static string CompassDirection(Vector3 delta)
        {
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.0001f) return "here";
            float absX = Mathf.Abs(delta.x);
            float absZ = Mathf.Abs(delta.z);
            string horizontal = delta.x >= 0f ? "E" : "W";
            string vertical = delta.z >= 0f ? "N" : "S";
            if (absX > absZ * 1.85f) return horizontal;
            if (absZ > absX * 1.85f) return vertical;
            return vertical + horizontal;
        }
    }
}
