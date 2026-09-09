using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRunWeaponsTests
    {
        [Test]
        public void BeforeLoadoutQueriesAreEmptyAndCommandsDoNotCreateDefinitions()
        {
            var port = new WeaponPort();
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.ActiveWeaponCount, Is.Zero);
            Assert.That(weapons.ActiveWeaponIds, Is.Empty);
            Assert.That(weapons.ActiveOrbitBladeCount, Is.Zero);
            Assert.That(weapons.ContainsWeapon("a"), Is.False);
            Assert.That(weapons.FireForTest(SurvivorsWeaponArchetype.Projectile), Is.False);
            Assert.That(weapons.TryAddWeaponToLoadout("a"), Is.False);
            weapons.Tick(-2f);
            weapons.DisposeLoadout();
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void FallbackDefinitionUsesItsInitializationSnapshotAndCooldownReadsLiveTuning()
        {
            var port = new WeaponPort();
            var weapons = new SurvivorsRunWeapons(port);
            port.Damage = new SurvivorsWeaponBonusValues(upgrade: 2f);
            Assert.That(weapons.ResolveDisplayedWeaponDamage(), Is.EqualTo(2f));
            port.Tuning.ProjectileDamage = 7f;
            weapons.InitializeFallbackDefinition(port.Tuning);
            port.Tuning.ProjectileDamage = 99f;
            Assert.That(weapons.ResolveDisplayedWeaponDamage(), Is.EqualTo(9f));
            port.Tuning.WeaponCooldownSeconds = 2f;
            Assert.That(weapons.ResolveDisplayedWeaponCooldownSeconds(), Is.EqualTo(2f));
            port.Tuning.WeaponCooldownSeconds = 3f;
            Assert.That(weapons.ResolveDisplayedWeaponCooldownSeconds(), Is.EqualTo(3f));
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void PrimaryUsesFirstActiveIdAndFirstOrdinalMatchingDefinition()
        {
            var first = Definition("B", 12f, 2f);
            var later = Definition("B", 99f, 9f);
            var port = new WeaponPort { Definitions = new[] { Definition("a"), first, later }, StartingWeaponIds = new[] { "B", "a" } };
            var weapons = new SurvivorsRunWeapons(port);
            weapons.BuildLoadout(port.Tuning);
            Assert.That(weapons.ResolvePrimaryWeaponDefinitionForDisplay(), Is.SameAs(first));
            Assert.That(weapons.ResolveDisplayedWeaponDamage(), Is.EqualTo(12f));
            Assert.That(weapons.ResolveDisplayedWeaponCooldownSeconds(), Is.EqualTo(2f));
            port.Session.Ids[0] = "b";
            Assert.That(weapons.ResolvePrimaryWeaponDefinitionForDisplay(), Is.Null);
        }

        [Test]
        public void MissingPrimaryUsesFallbackWithoutLoadingAnotherCatalog()
        {
            var port = new WeaponPort { Definitions = new[] { Definition("a") } };
            var weapons = new SurvivorsRunWeapons(port);
            port.Tuning.ProjectileDamage = 4f;
            weapons.InitializeFallbackDefinition(port.Tuning);
            weapons.BuildLoadout(port.Tuning);
            port.Session.Ids[0] = "missing";
            port.Events.Clear();
            Assert.That(weapons.ResolveDisplayedWeaponDamage(), Is.EqualTo(4f));
            Assert.That(weapons.ResolveDisplayedWeaponCooldownSeconds(), Is.EqualTo(port.Tuning.WeaponCooldownSeconds));
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void NullDamageDoesNotReadBonusesAndNullCooldownResolvesPrimary()
        {
            var port = new WeaponPort { Definitions = new[] { Definition("a", 4f, 2f) } };
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.ResolveWeaponDamage(null), Is.Zero);
            Assert.That(port.DamageReads, Is.Zero);
            weapons.BuildLoadout(port.Tuning);
            port.Cooldown = new SurvivorsWeaponBonusValues(upgrade: 0.5f);
            Assert.That(weapons.ResolveWeaponCooldownSeconds(null), Is.EqualTo(3f));
            Assert.That(port.CooldownReads, Is.EqualTo(1));
        }

        [Test]
        public void DamageAddsBaseBeforeBonusesWithoutReassociation()
        {
            var port = new WeaponPort { Damage = new SurvivorsWeaponBonusValues(upgrade: -16777216f, streak: 1f) };
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.ResolveWeaponDamage(Definition("large", 16777216f)), Is.EqualTo(1f));
        }

        [Test]
        public void AllThirteenSourcesContributeAndDamageCooldownReadsRemainIndependent()
        {
            var port = new WeaponPort { Damage = new SurvivorsWeaponBonusValues(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13) };
            var weapons = new SurvivorsRunWeapons(port);
            var definition = Definition("a", 10f, 2f);
            Assert.That(weapons.ResolveWeaponDamage(definition), Is.EqualTo(101f));
            Assert.That(port.CooldownReads, Is.Zero);
            port.Cooldown = new SurvivorsWeaponBonusValues(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);
            Assert.That(weapons.ResolveWeaponCooldownSeconds(definition), Is.EqualTo(184f));
            Assert.That(port.DamageReads, Is.EqualTo(1));
        }

        [Test]
        public void DamageAndCooldownReadChangedBonusesOnEveryResolution()
        {
            var port = new WeaponPort();
            var weapons = new SurvivorsRunWeapons(port);
            var definition = Definition("a", 10f, 2f);
            Assert.That(weapons.ResolveWeaponDamage(definition), Is.EqualTo(10f));
            Assert.That(weapons.ResolveWeaponCooldownSeconds(definition), Is.EqualTo(2f));
            port.Damage = new SurvivorsWeaponBonusValues(endless: 3f);
            port.Cooldown = new SurvivorsWeaponBonusValues(endless: -0.5f);
            Assert.That(weapons.ResolveWeaponDamage(definition), Is.EqualTo(13f));
            Assert.That(weapons.ResolveWeaponCooldownSeconds(definition), Is.EqualTo(1f));
        }

        [Test]
        public void DamageZeroMultiplierFloorAndDistinctDisplayedCooldownFloorArePreserved()
        {
            var port = new WeaponPort { Damage = new SurvivorsWeaponBonusValues(upgrade: -20f), Cooldown = new SurvivorsWeaponBonusValues(upgrade: -20f) };
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.ResolveWeaponDamage(Definition("a", 10f)), Is.Zero);
            Assert.That(weapons.ResolveWeaponCooldownSeconds(Definition("a", cooldown: 1f)), Is.EqualTo(0.2f));
            Assert.That(weapons.ResolveWeaponCooldownSeconds(Definition("b", cooldown: 0.1f)), Is.EqualTo(0.08f));
            port.Tuning.WeaponCooldownSeconds = 0.1f;
            Assert.That(weapons.ResolveDisplayedWeaponCooldownSeconds(), Is.EqualTo(0.12f));
        }

        [Test]
        public void StartingSelectionRetainsClassOrderDuplicatesAndFirstDefinitionMatches()
        {
            var a = Definition("A"); var b = Definition("b"); var duplicate = Definition("b", 99f);
            var port = new WeaponPort { StartingWeaponIds = new[] { "b", "A", "b", "missing", "a" }, MaximumSlots = 1 };
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.ResolveStartingWeaponDefinitions(new[] { a, b, duplicate }), Is.EqualTo(new[] { b, a, b }));
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void MissingClassAndUnknownStartingIdsUseFirstEntryEvenWhenNull()
        {
            var port = new WeaponPort();
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.ResolveStartingWeaponDefinitions(null), Is.Empty);
            Assert.That(weapons.ResolveStartingWeaponDefinitions(Array.Empty<SurvivorsWeaponArchetypeDefinition>()), Is.Empty);
            var definitions = new[] { null, Definition("a") };
            Assert.That(weapons.ResolveStartingWeaponDefinitions(definitions), Is.EqualTo(new SurvivorsWeaponArchetypeDefinition[] { null }));
            port.StartingWeaponIds = new[] { "unknown" };
            Assert.That(weapons.ResolveStartingWeaponDefinitions(definitions), Is.EqualTo(new SurvivorsWeaponArchetypeDefinition[] { null }));
        }

        [Test]
        public void DefinitionLookupCachesCatalogAndObservesItsLiveMembership()
        {
            var a = Definition("a"); var replacement = Definition("a", 99f);
            var list = new List<SurvivorsWeaponArchetypeDefinition> { null, a, Definition("a", 33f) };
            var port = new WeaponPort { Definitions = list };
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.FindWeaponDefinition(null), Is.Null);
            Assert.That(port.Events, Is.Empty);
            Assert.That(weapons.FindWeaponDefinition("a"), Is.SameAs(a));
            Assert.That(weapons.FindWeaponDefinition("A"), Is.Null);
            list[1] = replacement;
            Assert.That(weapons.FindWeaponDefinition("a"), Is.SameAs(replacement));
            Assert.That(port.Events, Is.EqualTo(new[] { "definitions" }));
        }

        [Test]
        public void EmptyCatalogRetriesLookupAndBuildRefreshesPreviouslyCachedDefinitions()
        {
            var port = new WeaponPort();
            var weapons = new SurvivorsRunWeapons(port);
            Assert.That(weapons.FindWeaponDefinition("a"), Is.Null);
            Assert.That(weapons.FindWeaponDefinition("a"), Is.Null);
            var a = Definition("a");
            port.Definitions = new[] { a };
            Assert.That(weapons.FindWeaponDefinition("a"), Is.SameAs(a));
            var b = Definition("b");
            port.Definitions = new[] { b };
            weapons.BuildLoadout(port.Tuning);
            Assert.That(weapons.ActiveWeaponIds, Is.EqualTo(new[] { "b" }));
            Assert.That(port.Events, Is.EqualTo(new[] { "definitions", "definitions", "definitions", "definitions", "build" }));
        }

        [Test]
        public void FullAndInvalidLoadoutRequestsStopBeforeContainsOrFeedback()
        {
            var port = new WeaponPort { Definitions = new[] { Definition("a"), Definition("b") }, MaximumSlots = 1 };
            var weapons = new SurvivorsRunWeapons(port);
            weapons.BuildLoadout(port.Tuning); port.Events.Clear();
            Assert.That(weapons.TryAddWeaponToLoadout(null), Is.False);
            Assert.That(weapons.TryAddWeaponToLoadout("b"), Is.False);
            Assert.That(port.Events, Is.Empty);
        }

        [Test]
        public void DuplicateMissingAndRejectedDefinitionsDoNotProduceSuccessfulAddFeedback()
        {
            var port = new WeaponPort { Definitions = new[] { Definition("a"), Definition("b") } };
            var weapons = new SurvivorsRunWeapons(port);
            weapons.BuildLoadout(port.Tuning); port.Events.Clear();
            Assert.That(weapons.TryAddWeaponToLoadout("a"), Is.False);
            Assert.That(weapons.TryAddWeaponToLoadout("unknown"), Is.False);
            port.Session.AcceptAdds = false;
            Assert.That(weapons.TryAddWeaponToLoadout("b"), Is.False);
            Assert.That(port.Events, Is.EqualTo(new[] { "contains:a", "contains:unknown", "contains:b", "add:b" }));
            Assert.That(weapons.ActiveWeaponCount, Is.EqualTo(1));
        }

        [Test]
        public void SuccessfulAdditionPublishesTheUpdatedLoadoutBeforeFeedback()
        {
            var port = new WeaponPort { Definitions = new[] { Definition("a"), Definition("b") } };
            var weapons = new SurvivorsRunWeapons(port);
            weapons.BuildLoadout(port.Tuning); port.Events.Clear();
            port.OnAdded = definition => Assert.That(weapons.ActiveWeaponIds, Is.EqualTo(new[] { "a", definition.Id }));
            Assert.That(weapons.TryAddWeaponToLoadout("b"), Is.True);
            Assert.That(port.Events, Is.EqualTo(new[] { "contains:b", "add:b", "feedback:b" }));
        }

        [Test]
        public void DisposeIsIdempotentAndRetainsDefinitionCacheAndFallbackUntilNextInitialization()
        {
            var a = Definition("a");
            var port = new WeaponPort { Definitions = new[] { a } };
            var weapons = new SurvivorsRunWeapons(port);
            port.Tuning.ProjectileDamage = 4f;
            weapons.InitializeFallbackDefinition(port.Tuning);
            weapons.BuildLoadout(port.Tuning); port.Events.Clear();
            weapons.DisposeLoadout(); weapons.DisposeLoadout();
            Assert.That(weapons.ActiveWeaponIds, Is.Empty);
            Assert.That(weapons.FindWeaponDefinition("a"), Is.SameAs(a));
            Assert.That(weapons.ResolveDisplayedWeaponDamage(), Is.EqualTo(4f));
            Assert.That(port.Events, Is.EqualTo(new[] { "dispose" }));
            port.Tuning.ProjectileDamage = 9f;
            weapons.InitializeFallbackDefinition(port.Tuning);
            Assert.That(weapons.ResolveDisplayedWeaponDamage(), Is.EqualTo(9f));
        }

        [Test]
        public void SessionReadsAndCommandsForwardWithoutOwningAnotherWeaponList()
        {
            var port = new WeaponPort { Definitions = new[] { Definition("a") } };
            var weapons = new SurvivorsRunWeapons(port);
            weapons.BuildLoadout(port.Tuning); port.Events.Clear();
            Assert.That(weapons.ActiveWeaponIds, Is.SameAs(port.Session.Ids));
            Assert.That(weapons.ActiveOrbitBladeCount, Is.EqualTo(3));
            weapons.Tick(-2f);
            Assert.That(weapons.FireForTest(SurvivorsWeaponArchetype.Orbit), Is.True);
            Assert.That(weapons.ContainsWeapon("a"), Is.True);
            Assert.That(port.Session.LastDeltaTime, Is.EqualTo(-2f));
            Assert.That(port.Events, Is.EqualTo(new[] { "tick", "fire:Orbit", "contains:a" }));
        }

        private static SurvivorsWeaponArchetypeDefinition Definition(string id, float damage = 5f, float cooldown = 1f)
            => new SurvivorsWeaponArchetypeDefinition(id, id, SurvivorsWeaponArchetype.Projectile, cooldown, damage, 10f, Color.white);

        private sealed class WeaponPort : ISurvivorsRunWeaponPort
        {
            internal readonly List<string> Events = new List<string>();
            internal IReadOnlyList<SurvivorsWeaponArchetypeDefinition> Definitions = Array.Empty<SurvivorsWeaponArchetypeDefinition>();
            internal WeaponSession Session;
            internal SurvivorsWeaponBonusValues Damage, Cooldown;
            internal int DamageReads, CooldownReads;
            internal Action<SurvivorsWeaponArchetypeDefinition> OnAdded;
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning();
            public IReadOnlyList<string> StartingWeaponIds { get; set; }
            public int MaximumSlots { get; set; } = 6;
            public SurvivorsWeaponBonusValues DamageBonuses { get { DamageReads++; return Damage; } }
            public SurvivorsWeaponBonusValues CooldownBonuses { get { CooldownReads++; return Cooldown; } }
            public IReadOnlyList<SurvivorsWeaponArchetypeDefinition> CreateDefinitions(SurvivorsTemplateTuning tuning)
            { Events.Add("definitions"); Assert.That(tuning, Is.SameAs(Tuning)); return Definitions; }
            public ISurvivorsWeaponLoadoutSession CreateLoadout(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions)
            { Events.Add("build"); Session = new WeaponSession(Events, definitions); return Session; }
            public void WeaponAdded(SurvivorsWeaponArchetypeDefinition definition)
            { Events.Add("feedback:" + definition.Id); OnAdded?.Invoke(definition); }
        }

        private sealed class WeaponSession : ISurvivorsWeaponLoadoutSession
        {
            private readonly List<string> _events;
            internal readonly List<string> Ids = new List<string>();
            internal bool AcceptAdds = true;
            internal float LastDeltaTime;
            internal WeaponSession(List<string> events, IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions)
            { _events = events; foreach (var definition in definitions) if (definition != null) Ids.Add(definition.Id); }
            public int WeaponCount => Ids.Count;
            public IReadOnlyList<string> WeaponIds => Ids;
            public int ActiveOrbitBladeCount => 3;
            public bool ContainsWeapon(string id) { _events.Add("contains:" + id); return Ids.Contains(id); }
            public bool TryAddWeapon(SurvivorsWeaponArchetypeDefinition definition)
            { _events.Add("add:" + definition.Id); if (!AcceptAdds) return false; Ids.Add(definition.Id); return true; }
            public void Tick(float deltaTime) { LastDeltaTime = deltaTime; _events.Add("tick"); }
            public bool FireForTest(SurvivorsWeaponArchetype archetype) { _events.Add("fire:" + archetype); return true; }
            public void Dispose() { _events.Add("dispose"); Ids.Clear(); }
        }
    }
}
