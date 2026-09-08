namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Borrows profile and authored content; storage lifetime stays with ProfileSession.</summary>
    internal interface ISurvivorsProgressionContentPort
    {
        SurvivorsMetaProgressionService EnsureProgression();
        SurvivorsMetaProgressionDefinition ProgressionDefinition { get; }
        SurvivorsClassLibraryDefinition EnsureClasses();
    }

    internal interface ISurvivorsPersistentProgressionPort : ISurvivorsProgressionContentPort
    {
        void ApplyPersistentBonuses();
        void SetSelectedClass(SurvivorsClassDefinition selected);
        void ShowMetaPurchase(string id);
        void ShowClassSelection(SurvivorsClassDefinition selected);
    }

    internal interface ISurvivorsRunRewardPort : ISurvivorsProgressionContentPort
    {
        void ShowClassUnlock();
        void ShowRunSummary(bool victory);
    }
}
