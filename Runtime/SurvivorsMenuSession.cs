using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal enum BuildMenuTab
    {
        CurrentBuild = 0,
        Stats = 1,
        RunInfo = 2,
        Controls = 3
    }

    internal interface ISurvivorsTutorialPort
    {
        bool HasProfile { get; }
        bool TutorialSeen { get; }
        void EnsureProfile();
        void ResetTutorialSeen();
        void MarkTutorialSeen();
        void PlaySelect();
    }
    /// <summary>Owns menu visibility, build tab navigation and the tutorial's persisted progression policy.</summary>
    internal sealed class SurvivorsMenuSession
    {
        private readonly ISurvivorsTutorialPort _port;
        public bool ModeSelectionOpen { get; set; }
        public bool BuildOpen { get; set; }
        public BuildMenuTab BuildTab { get; set; }
        public bool TutorialOpen { get; set; }
        public int TutorialIndex { get; set; }
        public SurvivorsMenuSession(ISurvivorsTutorialPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        public bool HandleBuildInput(SurvivorsRunState state, bool toggle, int selectedTab)
        {
            if (state != SurvivorsRunState.Playing) { BuildOpen = false; return false; }
            if (toggle)
            {
                BuildOpen = !BuildOpen;
                if (BuildOpen) BuildTab = BuildMenuTab.CurrentBuild;
                return BuildOpen;
            }
            if (!BuildOpen) return false;
            if (selectedTab >= 0) BuildTab = ClampBuildMenuTab(selectedTab);
            return true;
        }
        public void TryOpenFirstRunTutorial()
        {
            _port.EnsureProfile();
            if (_port.HasProfile && !_port.TutorialSeen)
            {
                OpenTutorialOverlay(markUnseen: true);
            }
        }

        public bool OpenTutorialOverlay(bool markUnseen)
        {
            _port.EnsureProfile();
            TutorialOpen = true;
            TutorialIndex = 0;
            BuildOpen = false;
            if (markUnseen && _port.HasProfile && _port.TutorialSeen)
            {
                _port.ResetTutorialSeen();
            }

            _port.PlaySelect();
            return true;
        }

        public bool CloseTutorialOverlay(bool markSeen)
        {
            if (!TutorialOpen && !markSeen)
            {
                return false;
            }

            TutorialOpen = false;
            if (markSeen)
            {
                _port.EnsureProfile();
                _port.MarkTutorialSeen();
            }

            _port.PlaySelect();
            return true;
        }

        public bool AdvanceTutorialStep()
        {
            int max = SurvivorsTutorialContent.StepCount - 1;
            if (TutorialIndex >= max)
            {
                return CloseTutorialOverlay(markSeen: true);
            }

            TutorialIndex = Mathf.Min(max, TutorialIndex + 1);
            _port.PlaySelect();
            return true;
        }

        public bool BackTutorialStep()
        {
            if (!TutorialOpen || TutorialIndex <= 0)
            {
                return false;
            }

            TutorialIndex--;
            _port.PlaySelect();
            return true;
        }
        public static BuildMenuTab ClampBuildMenuTab(int tabIndex)
        {
            if (tabIndex <= 0) return BuildMenuTab.CurrentBuild;
            if (tabIndex == 1) return BuildMenuTab.Stats;
            if (tabIndex == 2) return BuildMenuTab.RunInfo;
            return BuildMenuTab.Controls;
        }
        public static string FormatBuildMenuTabLabel(BuildMenuTab tab)
        {
            switch (tab)
            {
                case BuildMenuTab.Stats:
                    return "Stats";
                case BuildMenuTab.RunInfo:
                    return "Run Info";
                case BuildMenuTab.Controls:
                    return "Controls";
                default:
                    return "Current Build";
            }
        }
    }
}
