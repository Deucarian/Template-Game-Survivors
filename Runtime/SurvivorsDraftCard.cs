using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal struct SurvivorsDraftCard
    {
        public int Index;
        public string Hotkey;
        public string Name;
        public string RarityLabel;
        public string CategoryId;
        public string CategoryLabel;
        public string AffectedLabel;
        public string RankLabel;
        public string Description;
        public string EffectPreview;
        public string RequirementHint;
        public string IconId;
        public string StyleToken;
        public bool IsEvolution;
        public Color AccentColor;

        public string Summary
        {
            get
            {
                return $"{Hotkey} | {Name} | {RarityLabel} | {CategoryLabel} | {AffectedLabel} | {RankLabel} | {Description} | Preview: {EffectPreview} | {RequirementHint}";
            }
        }
    }
}
