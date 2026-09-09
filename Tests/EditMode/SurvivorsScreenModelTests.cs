using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsScreenModelTests
    {
        [Test]
        public void WideDraftFitsAllCardsAndReservesUpgradeActionSpace()
        {
            Rect panel = new Rect(100, 100, 1120, 690);
            var layout = new SurvivorsDraftScreenLayout(panel, true, 3);
            Assert.IsTrue(layout.Horizontal);
            Assert.AreEqual(130f, layout.Card(0).x);
            Assert.AreEqual(panel.xMax - 30f, layout.Card(2).xMax, 0.001f);
            Assert.AreEqual(16f, layout.Card(1).xMin - layout.Card(0).xMax, 0.001f);
            Assert.AreEqual(panel.yMax - 86f, layout.Card(2).yMax);
            Assert.AreEqual(layout.Card(0).yMax - 48f, layout.Selection(layout.Card(0)).yMax);
            Assert.AreEqual(Rect.zero, layout.View);
        }

        [Test]
        public void NarrowDraftStacksCardsInsideTheScrollContentAndRelicsUseTheWholeCard()
        {
            var layout = new SurvivorsDraftScreenLayout(new Rect(20, 20, 340, 690), false, 3);
            Assert.IsFalse(layout.Horizontal);
            Assert.AreEqual(330f, layout.Card(0).height);
            Assert.AreEqual(262f, layout.Card(0).width);
            Assert.AreEqual(1022f, layout.Content.height);
            Assert.AreEqual(layout.Content.yMax, layout.Card(2).yMax);
            Assert.AreEqual(50f, layout.View.x);
            Assert.AreEqual(120f, layout.View.y);
            Assert.AreEqual(layout.Card(1), layout.Selection(layout.Card(1)));
        }

        [Test]
        public void DraftOrientationKeepsTheExactBreakpointAndShortViewportMinimums()
        {
            Assert.IsFalse(new SurvivorsDraftScreenLayout(new Rect(0, 0, 759, 420), true, 2).Horizontal);
            Assert.IsTrue(new SurvivorsDraftScreenLayout(new Rect(0, 0, 760, 420), true, 2).Horizontal);
            var tiny = new SurvivorsDraftScreenLayout(new Rect(0, 0, 320, 100), true, 2);
            Assert.AreEqual(1f, tiny.View.height);
            Assert.AreEqual(220f, tiny.Card(0).height);
            Assert.Greater(tiny.Content.height, tiny.View.height);
        }

        [Test]
        public void ModeCardsCopyAuthoredTextAndPreserveTheStandardTitleOverride()
        {
            var tuning = new SurvivorsTemplateTuning { RunModeDisplayName = "Authored Sprint", RunModeDurationLabel = "7 min", RunModeDescription = "Custom arc" };
            var card = new SurvivorsRunModeCardView(SurvivorsPacingProfile.SprintRun, tuning, "Boss 02:00   Victory 07:00");
            tuning.RunModeDisplayName = "Changed";
            tuning.RunModeDurationLabel = "99 min";
            tuning.RunModeDescription = "Changed arc";
            Assert.AreEqual("Authored Sprint", card.Title);
            Assert.AreEqual("7 min", card.Duration);
            Assert.AreEqual("Custom arc", card.Description);
            Assert.AreEqual("Boss 02:00   Victory 07:00", card.Milestones);
            Assert.AreEqual("Standard / Human Playtest", SurvivorsRunModeCardView.ResolveTitle(SurvivorsPacingProfile.HumanPlaytest, tuning));
            tuning.RunModeDisplayName = " ";
            Assert.AreEqual(BasicSurvivorsGame.GetPacingProfileDisplayName(SurvivorsPacingProfile.SprintRun),
                SurvivorsRunModeCardView.ResolveTitle(SurvivorsPacingProfile.SprintRun, tuning));
        }
    }
}
