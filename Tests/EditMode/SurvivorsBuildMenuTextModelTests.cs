using System.Collections.Generic;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsBuildMenuTextModelTests
    {
        [Test]
        public void StatsRowsKeepAllDisplayPrecisionOrderingAndSeparateDamageTotals()
        {
            var values = new SurvivorsBuildMenuStatsValues(
                damageBonusTotal: 6.6f, surgeDamageBonus: 4.4f, weaponCooldownSeconds: 0.456f, playerMoveSpeed: 3.26f,
                currentHealth: 15.4f, maxHealth: 20.4f, barrierValue: 1.2f, barrierCapacity: 4.5f,
                contactInvulnerabilitySeconds: 0.127f, currentPickupAttractRange: 2.3f, currentPickupAttractionSpeed: 4.4f,
                experienceGainBonus: 0.25f, draftLuckBonus: 0.15f, areaRadiusBonus: 1.2f, orbitRadiusBonus: 2.3f,
                deathNovaDamage: 4.5f, deathNovaRadius: 6.7f, payloadExplosionRadiusBonus: 3.4f, payloadTriggerRadiusBonus: 4.5f,
                poisonDamageRatio: 0.1f, bleedDamageRatio: 0.2f, executeThresholdNormalized: 0.3f, lifestealRatio: 0.4f,
                criticalChanceNormalized: 0.25f, criticalDamageMultiplier: 1.5f,
                projectileFanBonus: 1, projectilePierceBonus: 2, projectileChainBonus: 3, projectileForkBonus: 4,
                projectileReturnBonus: 5, payloadCountBonus: 2, pickupPulseLabel: "00:09");
            IReadOnlyList<string> first = SurvivorsBuildMenuTextModel.Stats(values);
            CollectionAssert.AreEqual(new[]
            {
                "Damage +6.6   Surge +4.4",
                "Cooldown 0.46s   Move speed 3.3",
                "Health 15/20   Barrier 1.2/4.5   Armor: contact safety 0.13s",
                "Pickup radius 2.3   Magnet range 2.3   Magnet speed 4.4   Pulse 00:09",
                "XP gain +25%   Draft luck +15%",
                "Area/radius +1.2   Orbit +2.3   Death nova 4.5/6.7",
                "Projectiles fan +1   pierce +2   chain +3   fork +4   return +5",
                "Payloads +2   payload radius +3.4   trigger +4.5",
                "Status poison 10%   bleed 20%   execute 30%   lifesteal 40%",
                "Crit 25% x1.5"
            }, first);
            Assert.AreEqual("Damage +12   Surge +1", SurvivorsBuildMenuTextModel.Stats(
                new SurvivorsBuildMenuStatsValues(damageBonusTotal: 12f, surgeDamageBonus: 1f))[0]);
            Assert.AreEqual("Damage +6.6   Surge +4.4", first[0]);
        }

        [TestCase(false, "00:00")]
        [TestCase(true, "Endless")]
        public void RunInfoPreservesAuthoredLabelsLongBalancesAndNormalVersusEndlessRemainingTime(bool endless, string remaining)
        {
            var values = new SurvivorsBuildMenuRunInfoValues(
                isEndlessRun: endless, currentRunModeDisplayName: "Authored Sprint", pacingProfileLabel: "Quick Pace",
                runTimeSeconds: 65.9f, survivalVictoryTimeSeconds: 20f, currentRunMilestoneHudLabel: "Next Horde", phaseLabel: "Endless",
                runEscalationLevel: 4, level: 7, killedCount: 10, activeEnemyCount: 11, currentEnemyMaximumAlive: 40,
                activeEliteCount: 2, activeMinibossCount: 3, activeBossCount: 1, currencyEarned: 13, progressionEarned: 17,
                draftRerollsRemaining: 2, draftBanishesRemaining: 3, draftSkipBloodShards: 5,
                waystoneDiscoveryCount: 6, roamingCacheDropCount: 7, arenaShrineTrialCount: 8,
                metaBloodShards: 5000000000L, lifetimeLegacyExperience: 6000000000L,
                currencyDisplayName: "Shard Bank", progressionDisplayName: "Legacy Bank", currencyRewardLabel: "Shards");
            CollectionAssert.AreEqual(new[]
            {
                "Mode: Authored Sprint (Quick Pace)",
                "Elapsed 01:05   Remaining " + remaining,
                "Milestone: Next Horde",
                "Phase Endless +4   Level 7   Kills 10",
                "Enemies 11/40   Elites 2   Minibosses 3   Bosses 1",
                "Run rewards: 13 Shard Bank, 17 Legacy Bank",
                "Meta bank: 5000000000 Shard Bank   Legacy Bank 6000000000",
                "Rerolls 2   Banishes 3   Skip reward +5 Shards",
                "Waystones 6   Roaming caches 7   Arena trials 8"
            }, SurvivorsBuildMenuTextModel.RunInfo(values));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("  ", false)]
        [TestCase("Ready Radiance", true)]
        public void PlayerHudKeepsOptionalEvolutionBetweenCollectorAndBuildMenuHint(string evolution, bool shown)
        {
            var values = new SurvivorsPlayerHudValues("Next Horde", "Build W 2/6", "Wand, Frost", 2.3f, 4.5f, "not yet", evolution);
            var expected = new List<string>
            {
                "Next Horde", "Build W 2/6", "Weapons: Wand, Frost", "Pickup 2.3   Pull 4.5   Pulse not yet"
            };
            if (shown) expected.Add("Ready Radiance");
            expected.Add("Tab/B Build Menu");
            CollectionAssert.AreEqual(expected, SurvivorsPlayerHudTextModel.BuildLines(values));
        }

        [Test]
        public void ControlsKeepLegacyCopyAndCurrentBuildRowsAreCopiedWithEmptyFallback()
        {
            CollectionAssert.AreEqual(new[]
            {
                "Move: WASD or arrow keys",
                "Arc Step: Space",
                "Draft choice: mouse or 1/2/3",
                "Draft tools: R reroll, S skip, Shift+1/2/3 banish",
                "Build menu: Esc, Tab, or B opens and closes",
                "Build menu tabs: 1 Current Build, 2 Stats, 3 Run Info, 4 Controls",
                "Debug overlay: F1",
                "Victory: C continues Standard into endless if available",
                "Result screen: Restart Same or Change Mode buttons"
            }, SurvivorsBuildMenuTextModel.Controls());
            var source = new List<string> { "Weapons 1/6", "  Wand" };
            IReadOnlyList<string> rows = SurvivorsBuildMenuTextModel.CurrentBuild(source);
            source.Add("Passives 0/6");
            CollectionAssert.AreEqual(new[] { "Weapons 1/6", "  Wand" }, rows);
            CollectionAssert.AreEqual(new[] { "No build data yet." }, SurvivorsBuildMenuTextModel.CurrentBuild(new string[0]));
        }
    }
}
