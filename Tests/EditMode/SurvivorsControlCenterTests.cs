using System.Linq;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.TemplateGameSurvivors.Editor;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsControlCenterTests
    {
        [Test]
        public void MissingSampleSentinelDoesNotCountAsAnImportedPack()
        {
            GameContentPackDescriptor sentinel =
                SurvivorsContentPackProvider.CreateSampleNotImportedDescriptor();

            Assert.That(
                SurvivorsControlCenter.CountImportedPacks(new[] { sentinel }),
                Is.Zero);
        }

        [Test]
        public void ContributionRegistersStableAuthoringAndDeveloperActions()
        {
            SurvivorsControlCenter.CountImportedPacks(null);
            DeucarianControlCenterSnapshot snapshot =
                DeucarianControlCenterSnapshotBuilder.Capture();
            DeucarianControlCenterCard providerFailure = snapshot.Cards
                .FirstOrDefault(candidate => candidate.Id ==
                    "deucarian.provider-failure.com.deucarian.template.game.survivors.control-center");
            Assert.That(
                providerFailure,
                Is.Null,
                providerFailure == null ? string.Empty : providerFailure.Description);
            DeucarianToolDescriptor tool = snapshot.Tools.Single(candidate =>
                candidate.Id == "deucarian.template.survivors.authoring");
            DeucarianControlCenterCard authoring = snapshot
                .GetCards(DeucarianControlCenterArea.Authoring)
                .Single(candidate =>
                    candidate.Id == "com.deucarian.template.game.survivors.authoring");
            DeucarianControlCenterCard developer = snapshot
                .GetCards(DeucarianControlCenterArea.Developer)
                .Single(candidate =>
                    candidate.Id == "com.deucarian.template.game.survivors.developer");
            bool packStatusAvailable =
                SurvivorsControlCenter.TryGetImportedPackCount(
                    out int importedCount);

            Assert.That(tool.Area, Is.EqualTo(DeucarianControlCenterArea.Authoring));
            Assert.That(authoring.Area, Is.EqualTo(DeucarianControlCenterArea.Authoring));
            Assert.That(developer.Area, Is.EqualTo(DeucarianControlCenterArea.Developer));
            Assert.That(
                authoring.Status,
                Is.EqualTo(!packStatusAvailable
                    ? DeucarianControlCenterStatus.Error
                    : importedCount > 0
                        ? DeucarianControlCenterStatus.Success
                        : DeucarianControlCenterStatus.Info));
            CollectionAssert.AreEqual(
                new[] { "open-authoring", "validate" },
                authoring.Actions.Select(action => action.Id).ToArray());
            CollectionAssert.AreEqual(
                new[] { "open-debugger" },
                developer.Actions.Select(action => action.Id).ToArray());
        }
    }
}