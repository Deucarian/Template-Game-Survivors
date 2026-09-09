using System;
using System.Collections.Generic;
using Deucarian.Persistence;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    internal sealed class SurvivorsProgressionOwnerTestHost : ISurvivorsPersistentProgressionPort, ISurvivorsRunRewardPort, IDisposable
    {
        private readonly PersistenceService _persistence = new PersistenceService(new InMemoryTextStorage());
        private readonly SurvivorsProfileSession _profile;
        public readonly List<string> Events = new List<string>();
        public SurvivorsRunRewards Rewards;
        public SurvivorsPersistentProgression Persistent;
        public SurvivorsClassDefinition Selected;
        public int ClassReads;
        public int ProfileReads;
        public int BonusApplications;
        public SurvivorsClassLibraryDefinition Classes = BasicSurvivorsGame.CreateClassLibraryDefinition();
        public SurvivorsMetaProgressionDefinition Definition = BasicSurvivorsGame.CreateMetaProgressionDefinition();

        public SurvivorsProgressionOwnerTestHost()
        {
            _profile = new SurvivorsProfileSession(() => throw new InvalidOperationException("Borrowed storage only"));
            _profile.ConfigureBorrowedPersistence(_persistence, new SaveSlotId("progression-owner"));
            Rewards = new SurvivorsRunRewards(this);
            Persistent = new SurvivorsPersistentProgression(this);
        }

        public SurvivorsMetaProgressionDefinition ProgressionDefinition => Definition;
        public SurvivorsMetaProgressionService EnsureProgression()
        {
            ProfileReads++;
            return _profile.EnsureLoaded(Definition);
        }

        public SurvivorsClassLibraryDefinition EnsureClasses()
        {
            ClassReads++;
            _profile.Current?.EnsureDefaultClassUnlocks(Classes);
            return Classes;
        }

        public void ApplyPersistentBonuses()
        {
            BonusApplications++;
            Assert.AreEqual(1, _profile.Current.GetPersistentUpgradeRank(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));
            Events.Add("apply");
        }

        public void SetSelectedClass(SurvivorsClassDefinition selected)
        {
            Selected = selected;
            Events.Add("selected");
        }

        public void ShowMetaPurchase(string id)
        {
            Assert.Greater(Persistent.MetaUpgradePurchaseCount, 0);
            Events.Add("purchase:" + id);
        }

        public void ShowClassSelection(SurvivorsClassDefinition selected)
        {
            Assert.AreSame(selected, Selected);
            Assert.Greater(Persistent.ResultClassSelectionCount, 0);
            Events.Add("class:" + selected.Id);
        }

        public void ShowClassUnlock()
        {
            Assert.IsTrue(Rewards.Granted);
            Assert.AreEqual(1, Rewards.ClassUnlockRewardCount);
            Assert.IsTrue(_profile.Current.IsClassUnlocked(BasicSurvivorsGame.EmberVanguardClassId, Classes));
            Events.Add("unlock");
        }

        public void ShowRunSummary(bool victory)
        {
            Assert.IsTrue(Rewards.Granted);
            Assert.AreEqual(Rewards.LastRunResult.BloodShardsEarned, Rewards.BloodShardsEarned);
            Events.Add(victory ? "victory" : "defeat");
        }

        public void Dispose()
        {
            _profile.Dispose();
            _persistence.Dispose();
        }
    }
}
