using Deucarian.Editor;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    public sealed class SurvivorsRuntimeDebugWindow : EditorWindow
    {
        private DeucarianEditorPageSession navigation;
        public static void Open() => DeucarianEditorWindowPages.ShowStandalone<SurvivorsRuntimeDebugWindow>(
            "Survivors runtime", new Vector2(560, 520));
        public static IDeucarianEditorPage CreatePage() => new SurvivorsRuntimeWorkspace().Page;
        private void CreateGUI()
        {
            navigation?.Dispose();
            navigation = new DeucarianEditorPageSession(this, "survivors-runtime-home", _ => { });
            navigation.Navigate("deucarian.template.survivors.debugger");
        }
        private void OnDisable() { navigation?.Dispose(); navigation = null; }
    }
}
