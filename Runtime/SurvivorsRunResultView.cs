using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsRunResultView
    {
        public readonly string Title, Rewards;
        public readonly IReadOnlyList<string> Lines;
        public readonly bool EndlessEnabled;
        public SurvivorsRunResultView(string title, string rewards, IReadOnlyList<string> lines, bool endlessEnabled)
        { Title = title; Rewards = rewards; Lines = lines; EndlessEnabled = endlessEnabled; }
    }
    internal readonly struct SurvivorsResultClassChoice
    {
        public readonly string Label;
        public readonly bool Unlocked, Selected;
        public SurvivorsResultClassChoice(string label, bool unlocked, bool selected)
        { Label = label; Unlocked = unlocked; Selected = selected; }
    }
    internal readonly struct SurvivorsResultMetaView
    {
        public readonly string Title, EmptyLabel;
        public readonly IReadOnlyList<string> Labels;
        public SurvivorsResultMetaView(string title, string emptyLabel, IReadOnlyList<string> labels)
        { Title = title; EmptyLabel = emptyLabel; Labels = labels; }
    }
}
