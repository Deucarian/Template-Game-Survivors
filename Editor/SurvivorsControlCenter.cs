using System.Collections.Generic;
using System.Linq;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    [InitializeOnLoad]
    internal static class SurvivorsControlCenter
    {
        private const string PackageId =
            "com.deucarian.template.game.survivors";
        private const string ToolId = "deucarian.template.survivors.authoring";

        static SurvivorsControlCenter()
        {
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor(
                ToolId,
                "Survivors Content",
                "Open the Survivors content packs in Game Content Authoring.",
                DeucarianControlCenterArea.Authoring,
                GameContentAuthoringWindow.Open,
                PackageId,
                searchTerms: new[] { "survivors", "content", "template" },
                order: 240));
            DeucarianControlCenterRegistry.RegisterCardProvider(new Provider());
        }

        private sealed class Provider : IDeucarianControlCenterCardProvider
        {
            public string Id => PackageId + ".control-center";

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                bool packStatusAvailable = TryGetImportedPackCount(
                    out int packCount);
                yield return new DeucarianControlCenterCard(
                    PackageId + ".authoring",
                    DeucarianControlCenterArea.Authoring,
                    "Survivors Content",
                    "Open and validate the template-owned content packs.",
                    PackageId,
                    !packStatusAvailable
                        ? DeucarianControlCenterStatus.Error
                        : packCount > 0
                            ? DeucarianControlCenterStatus.Success
                            : DeucarianControlCenterStatus.Info,
                    !packStatusAvailable
                        ? "Content pack status unavailable"
                        : packCount > 0
                            ? packCount + " imported pack(s)"
                            : "Sample content is not imported",
                    order: 240,
                    details: new[]
                    {
                        "Only the imported pack count is summarized; record payloads stay in authoring."
                    },
                    actions: new[]
                    {
                        new DeucarianControlCenterAction(
                            "open-authoring",
                            "Open Authoring",
                            GameContentAuthoringWindow.Open),
                        new DeucarianControlCenterAction(
                            "validate",
                            "Validate Content",
                            SurvivorsEditorContentValidation.ValidateContent)
                    },
                    searchTerms: new[] { "survivors", "packs", "validate" });

                yield return new DeucarianControlCenterCard(
                    PackageId + ".developer",
                    DeucarianControlCenterArea.Developer,
                    "Survivors Runtime Debugger",
                    "Open the standalone play-mode tuning and stress controls.",
                    PackageId,
                    DeucarianControlCenterStatus.Info,
                    "Standalone debugger available",
                    order: 240,
                    actions: new[]
                    {
                        new DeucarianControlCenterAction(
                            "open-debugger",
                            "Open Runtime Debugger",
                            SurvivorsRuntimeDebugWindow.Open)
                    },
                    searchTerms: new[] { "survivors", "debugger", "playmode" });
            }
        }

        internal static int CountImportedPacks(
            IEnumerable<GameContentPackDescriptor> packs)
        {
            return packs == null
                ? 0
                : packs.Count(pack => pack != null &&
                    pack.SourceState != GameContentPackSourceState.SampleNotImported);
        }

        internal static bool TryGetImportedPackCount(out int packCount)
        {
            try
            {
                packCount = CountImportedPacks(
                    SurvivorsContentPackProvider.Instance.GetContentPacks());
                return true;
            }
            catch
            {
                packCount = 0;
                return false;
            }
        }
    }
}
