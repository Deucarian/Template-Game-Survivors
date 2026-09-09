using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsContentBindingTests
    {
        [Test]
        public void UnboundHostAllowsFallbackWithoutLoadingProfileOrCreatingAuthoredFlow()
        {
            using (var host = new SurvivorsContentBindingTestHost())
            {
                Assert.IsFalse(host.Binding.IsAuthoredContentBound);
                Assert.IsFalse(host.Binding.IsStrictAuthoredSample);
                Assert.IsTrue(host.Binding.IsFallbackContentActive);
                Assert.IsTrue(host.Binding.CanStartConfiguredRun);
                StringAssert.Contains("unbound host", host.Binding.AuthoredContentStatus);
                var tuning = host.Resolver.CreateConfiguredTuning(SurvivorsPacingProfile.SprintRun);
                Assert.AreEqual(SurvivorsPacingProfile.SprintRun, tuning.PacingProfile);
                Assert.IsNotNull(host.Resolver.CreateRunFlowDefinition(tuning));
                Assert.IsFalse(host.Resolver.IsUsingAuthoredRunFlow);
                Assert.IsNotNull(host.Resolver.ResolveMetaProgressionDefinition());
                Assert.IsNotEmpty(host.Resolver.CreateWeaponArchetypeDefinitions(tuning));
                Assert.IsNotEmpty(host.Resolver.CreateRelicDefinitions());
                Assert.IsNotNull(host.Resolver.CreateClassLibraryDefinition());
                Assert.IsNotEmpty(host.Resolver.CreateClassUpgradeGates());
                Assert.IsNotNull(host.Resolver.CreateBaseRunUpgradeCatalog());
                Assert.IsNotEmpty(host.Resolver.CreateRunUpgradeMetadata());
                Assert.IsEmpty(host.Events);
            }
        }

        [TestCase(0)]
        [TestCase(6)]
        [TestCase(10)]
        public void StrictRequiredSlotsBlockRunAndRefreshBeforeProfileRelease(int missing)
        {
            using (var host = new SurvivorsContentBindingTestHost())
            {
                var assets = host.LoadPack();
                assets[missing] = null;
                Assert.IsFalse(host.BindStrict(assets));
                Assert.IsTrue(host.Binding.IsStrictAuthoredSample);
                Assert.IsFalse(host.Binding.IsFallbackContentActive);
                Assert.IsFalse(host.Binding.CanStartConfiguredRun);
                Assert.IsNull(host.Binding.Definition);
                Assert.AreEqual($"Strict authored sample is missing required TextAsset at slot {missing}.", host.Binding.AuthoredContentStatus);
                CollectionAssert.AreEqual(new[] { "refresh", "release" }, host.Events);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void StrictBasicAndNeonBindingBorrowsDefinitionsAndBindsThemesLast(bool neon)
        {
            using (var host = new SurvivorsContentBindingTestHost())
            {
                var assets = host.LoadPack(neon);
                Assert.IsTrue(host.BindStrict(assets), host.Binding.AuthoredContentStatus);
                Assert.IsTrue(host.Binding.CanStartConfiguredRun);
                Assert.IsFalse(host.Binding.IsFallbackContentActive);
                Assert.AreSame(host.Binding.Definition, host.DefinitionAtRefresh);
                Assert.AreSame(assets[9], host.PrimaryTheme);
                Assert.AreSame(assets[10], host.AlternateTheme);
                CollectionAssert.AreEqual(new[] { "refresh", "release", "themes" }, host.Events);
                Assert.AreSame(host.Binding.Definition.MetaProgressionDefinition, host.Resolver.ResolveMetaProgressionDefinition());
                Assert.AreSame(host.Binding.Definition.WeaponDefinitions, host.Resolver.CreateWeaponArchetypeDefinitions(host.RefreshedTuning));
                Assert.AreSame(host.Binding.Definition.RelicDefinitions, host.Resolver.CreateRelicDefinitions());
                Assert.AreSame(host.Binding.Definition.ClassLibrary, host.Resolver.CreateClassLibraryDefinition());
                Assert.AreSame(host.Binding.Definition.ClassUpgradeGates, host.Resolver.CreateClassUpgradeGates());
                Assert.AreSame(host.Binding.Definition.RunUpgradeCatalog, host.Resolver.CreateBaseRunUpgradeCatalog());
                Assert.AreSame(host.Binding.Definition.RunUpgradeMetadata, host.Resolver.CreateRunUpgradeMetadata());
                Assert.IsFalse(host.Resolver.IsUsingAuthoredRunFlow, "Binding alone must not change the last constructed run-flow observation.");
                host.Resolver.CreateRunFlowDefinition(host.RefreshedTuning);
                Assert.IsTrue(host.Resolver.IsUsingAuthoredRunFlow);
                if (neon) Assert.AreEqual("Neon Fragments", host.Resolver.ResolveMetaProgressionDefinition().CurrencyDisplayName);
            }
        }

        [Test]
        public void ThemeFailureClearsBoundContentAndReleasesProfileAgainAfterFallbackRefresh()
        {
            using (var host = new SurvivorsContentBindingTestHost())
            {
                host.ThemesSucceed = false;
                Assert.IsFalse(host.BindStrict(host.LoadPack()));
                Assert.IsNull(host.Binding.Definition);
                Assert.IsFalse(host.Binding.CanStartConfiguredRun);
                Assert.IsFalse(host.Binding.IsFallbackContentActive);
                Assert.AreEqual("Strict authored sample UI themes failed to bind.", host.Binding.AuthoredContentStatus);
                CollectionAssert.AreEqual(new[] { "refresh", "release", "themes", "refresh", "release" }, host.Events);
                Assert.IsNull(host.DefinitionAtRefresh);
            }
        }

        [Test]
        public void FullValidationFailureStopsBeforeThemeBindingAndClearsPriorDefinition()
        {
            using (var host = new SurvivorsContentBindingTestHost())
            {
                var assets = host.LoadPack();
                Assert.IsTrue(host.BindStrict(assets));
                host.Events.Clear();
                assets[6] = host.CreateAsset("{}");
                Assert.IsFalse(host.BindStrict(assets));
                Assert.IsNull(host.Binding.Definition);
                Assert.IsFalse(host.Binding.CanStartConfiguredRun);
                StringAssert.StartsWith("Strict authored sample validation failed: ", host.Binding.AuthoredContentStatus);
                CollectionAssert.AreEqual(new[] { "refresh", "release" }, host.Events);
            }
        }

        [Test]
        public void PartialBindingUsesFallbackPolicyAndNeverRefreshesAnActiveRun()
        {
            using (var host = new SurvivorsContentBindingTestHost())
            {
                var assets = host.LoadPack();
                host.RunStarted = true;
                Assert.IsTrue(host.Binding.ConfigureAuthoredContent(assets[5], assets[7], assets[8]));
                Assert.IsFalse(host.Binding.IsStrictAuthoredSample);
                Assert.IsTrue(host.Binding.IsFallbackContentActive);
                Assert.IsTrue(host.Binding.CanStartConfiguredRun);
                Assert.IsNull(host.RefreshedTuning);
                CollectionAssert.AreEqual(new[] { "release" }, host.Events);
                host.Events.Clear();
                Assert.IsFalse(host.Binding.ConfigureAuthoredContentJson(null, null, null));
                Assert.IsNull(host.Binding.Definition);
                Assert.IsTrue(host.Binding.CanStartConfiguredRun);
                StringAssert.StartsWith("Fallback policy active after", host.Binding.AuthoredContentStatus);
                CollectionAssert.AreEqual(new[] { "release" }, host.Events);
            }
        }

        [Test]
        public void ResolverTracksRebindingAndFlowObservationChangesOnlyWhenNewFlowIsCreated()
        {
            using (var host = new SurvivorsContentBindingTestHost())
            {
                Assert.IsTrue(host.BindFullJson(host.LoadPack(), SurvivorsAuthoredContentBindingPolicy.StrictSample));
                var first = host.Resolver.ResolveMetaProgressionDefinition();
                host.Resolver.CreateRunFlowDefinition(host.RefreshedTuning);
                Assert.IsTrue(host.Resolver.IsUsingAuthoredRunFlow);
                Assert.IsTrue(host.BindFullJson(host.LoadPack(true), SurvivorsAuthoredContentBindingPolicy.StrictSample));
                Assert.AreNotSame(first, host.Resolver.ResolveMetaProgressionDefinition());
                Assert.AreEqual("Neon Fragments", host.Resolver.ResolveMetaProgressionDefinition().CurrencyDisplayName);
                Assert.IsFalse(host.Binding.ConfigureAuthoredContentJson(null, null, null));
                Assert.IsTrue(host.Resolver.IsUsingAuthoredRunFlow, "Last constructed flow survives a binding failure.");
                host.Resolver.CreateRunFlowDefinition(host.RefreshedTuning);
                Assert.IsFalse(host.Resolver.IsUsingAuthoredRunFlow);
                Assert.AreEqual("Blood Shards", host.Resolver.ResolveMetaProgressionDefinition().CurrencyDisplayName);
            }
        }
    }
}
