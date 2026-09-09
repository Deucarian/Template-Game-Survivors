using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsEnemyRosterQueriesTests
    {
        [Test]
        public void CountsObserveExistingMembershipAndExcludeDeadDestroyedAndMissingActors()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var queries = new SurvivorsEnemyRosterQueries(host.Enemies);
            host.Add(Vector3.zero, SurvivorsEnemyRole.Elite);
            host.Add(Vector3.zero, SurvivorsEnemyRole.DreadElite);
            host.Add(Vector3.zero, SurvivorsEnemyRole.Boss);
            var dead = host.Add(Vector3.zero, SurvivorsEnemyRole.Elite);
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(dead, 0f);
            var destroyed = host.Add(Vector3.zero, SurvivorsEnemyRole.Elite);
            Object.DestroyImmediate(destroyed.gameObject);
            host.Enemies.Add(null);
            Assert.That(queries.CountEliteEnemies(), Is.EqualTo(2));
            Assert.That(queries.CountEnemiesByRole(SurvivorsEnemyRole.Elite), Is.EqualTo(1));
            Assert.That(queries.CountEnemiesByRole(SurvivorsEnemyRole.Boss), Is.EqualTo(1));
            host.Enemies.Clear();
            Assert.That(queries.CountEliteEnemies(), Is.Zero);
            host.Add(Vector3.zero, SurvivorsEnemyRole.Runner);
            Assert.That(queries.CountEnemiesByRole(SurvivorsEnemyRole.Runner), Is.EqualTo(1));
        }

        [TestCase(SurvivorsEnemyRole.Elite, true, true, SurvivorsEnemyRole.Elite)]
        [TestCase(SurvivorsEnemyRole.DreadElite, true, true, SurvivorsEnemyRole.DreadElite)]
        [TestCase(SurvivorsEnemyRole.Miniboss, false, true, SurvivorsEnemyRole.Miniboss)]
        [TestCase(SurvivorsEnemyRole.Boss, false, true, SurvivorsEnemyRole.Boss)]
        [TestCase(SurvivorsEnemyRole.Summoner, false, false, SurvivorsEnemyRole.Elite)]
        [TestCase(SurvivorsEnemyRole.Swarm, false, false, SurvivorsEnemyRole.Elite)]
        public void RolePredicatesAndDebugFallbackRetainTheirDifferentCategories(
            SurvivorsEnemyRole role, bool elite, bool major, SurvivorsEnemyRole debug)
        {
            Assert.That(SurvivorsEnemyRosterQueries.IsEliteRole(role), Is.EqualTo(elite));
            Assert.That(SurvivorsEnemyRosterQueries.IsMajorRewardRole(role), Is.EqualTo(major));
            Assert.That(SurvivorsEnemyRosterQueries.ResolveDebugMajorEnemyRole(role), Is.EqualTo(debug));
        }
    }
}
