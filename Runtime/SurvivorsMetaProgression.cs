using System;
using System.Collections.Generic;
using Deucarian.Persistence;
using Deucarian.Progression;

namespace Deucarian.TemplateGameSurvivors
{
    public sealed class SurvivorsRunRewardSummary
    {
        public float RunDurationSeconds;
        public int LevelReached;
        public int MinibossKills;
        public int BossKills;
        public bool Victory;
        public int BloodShardsEarned;
        public int LegacyExperienceEarned;
    }

    public static class SurvivorsRunRewardCalculator
    {
        public static SurvivorsRunRewardSummary Calculate(
            float runDurationSeconds,
            int levelReached,
            int minibossKills,
            int bossKills,
            bool victory,
            int bonusBloodShards,
            int bonusLegacyExperience)
        {
            int durationBloodShards = Math.Max(0, (int)Math.Floor(runDurationSeconds / 20f));
            int levelBloodShards = Math.Max(0, levelReached - 1);
            int minibossBloodShards = Math.Max(0, minibossKills) * 3;

            int durationLegacyExperience = Math.Max(0, (int)Math.Floor(runDurationSeconds / 15f));
            int levelLegacyExperience = Math.Max(0, levelReached - 1) * 3;
            int bossLegacyExperience = Math.Max(0, bossKills) * 20;
            int victoryLegacyExperience = victory ? 75 : 0;

            return new SurvivorsRunRewardSummary
            {
                RunDurationSeconds = Math.Max(0f, runDurationSeconds),
                LevelReached = Math.Max(1, levelReached),
                MinibossKills = Math.Max(0, minibossKills),
                BossKills = Math.Max(0, bossKills),
                Victory = victory,
                BloodShardsEarned = Math.Max(0, durationBloodShards + levelBloodShards + minibossBloodShards + bonusBloodShards),
                LegacyExperienceEarned = Math.Max(0, durationLegacyExperience + levelLegacyExperience + bossLegacyExperience + victoryLegacyExperience + bonusLegacyExperience)
            };
        }
    }

    public sealed class SurvivorsMetaProgressionDefinition
    {
        public SurvivorsMetaProgressionDefinition(
            CurrencyId bloodShardsCurrencyId,
            TrackId legacyExperienceTrackId,
            IReadOnlyList<SurvivorsPersistentUpgradeDefinition> persistentUpgrades,
            IReadOnlyList<SurvivorsRewardDefinition> rewards,
            string currencyDisplayName = "Blood Shards",
            string legacyExperienceDisplayName = "Legacy XP")
        {
            BloodShardsCurrencyId = bloodShardsCurrencyId;
            LegacyExperienceTrackId = legacyExperienceTrackId;
            CurrencyDisplayName = string.IsNullOrWhiteSpace(currencyDisplayName) ? "Blood Shards" : currencyDisplayName;
            LegacyExperienceDisplayName = string.IsNullOrWhiteSpace(legacyExperienceDisplayName) ? "Legacy XP" : legacyExperienceDisplayName;
            PersistentUpgrades = persistentUpgrades == null ? Array.Empty<SurvivorsPersistentUpgradeDefinition>() : Copy(persistentUpgrades);
            Rewards = rewards == null ? Array.Empty<SurvivorsRewardDefinition>() : Copy(rewards);
            ProgressionCatalog = CreateCatalog();
        }

        public CurrencyId BloodShardsCurrencyId { get; }
        public TrackId LegacyExperienceTrackId { get; }
        public string CurrencyDisplayName { get; }
        public string LegacyExperienceDisplayName { get; }
        public IReadOnlyList<SurvivorsPersistentUpgradeDefinition> PersistentUpgrades { get; }
        public IReadOnlyList<SurvivorsRewardDefinition> Rewards { get; }
        public ProgressionCatalog ProgressionCatalog { get; }

        public bool TryGetPersistentUpgrade(string id, out SurvivorsPersistentUpgradeDefinition definition)
        {
            for (int i = 0; i < PersistentUpgrades.Count; i++)
            {
                SurvivorsPersistentUpgradeDefinition candidate = PersistentUpgrades[i];
                if (candidate != null && string.Equals(candidate.Id.Value, id, StringComparison.Ordinal))
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public bool TryGetReward(string id, out SurvivorsRewardDefinition definition)
        {
            for (int i = 0; i < Rewards.Count; i++)
            {
                SurvivorsRewardDefinition candidate = Rewards[i];
                if (candidate != null && string.Equals(candidate.Id, id, StringComparison.Ordinal))
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        private ProgressionCatalog CreateCatalog()
        {
            var research = new ResearchNodeDefinition[PersistentUpgrades.Count];
            for (int i = 0; i < PersistentUpgrades.Count; i++)
            {
                research[i] = PersistentUpgrades[i].CreateResearchDefinition(BloodShardsCurrencyId);
            }

            return new ProgressionCatalog(
                new[] { new CurrencyDefinition(BloodShardsCurrencyId, ProgressionAmount.Max) },
                new[]
                {
                    new ProgressionTrackDefinition(
                        LegacyExperienceTrackId,
                        startingLevel: 1,
                        new[]
                        {
                            new ProgressionAmount(50),
                            new ProgressionAmount(125),
                            new ProgressionAmount(250),
                            new ProgressionAmount(500)
                        })
                },
                research);
        }

        private static T[] Copy<T>(IReadOnlyList<T> source)
        {
            T[] copy = new T[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    public sealed class SurvivorsPersistentUpgradeDefinition
    {
        public SurvivorsPersistentUpgradeDefinition(
            ResearchNodeId id,
            string displayName,
            string targetId,
            string effectId,
            int maxRank,
            IReadOnlyList<int> rankCosts,
            float amountPerRank)
        {
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id.Value : displayName;
            TargetId = targetId ?? string.Empty;
            EffectId = effectId ?? string.Empty;
            MaxRank = Math.Max(1, maxRank);
            RankCosts = rankCosts == null ? Array.Empty<int>() : Copy(rankCosts);
            AmountPerRank = Math.Max(0f, amountPerRank);
        }

        public ResearchNodeId Id { get; }
        public string DisplayName { get; }
        public string TargetId { get; }
        public string EffectId { get; }
        public int MaxRank { get; }
        public IReadOnlyList<int> RankCosts { get; }
        public float AmountPerRank { get; }
        public float DamageBonusPerRank => string.Equals(EffectId, BasicSurvivorsGame.MetaDamageEffectId, StringComparison.Ordinal) ? AmountPerRank : 0f;

        public ResearchNodeDefinition CreateResearchDefinition(CurrencyId currencyId)
        {
            var costs = new CurrencyLine[RankCosts.Count];
            for (int i = 0; i < RankCosts.Count; i++)
            {
                costs[i] = new CurrencyLine(currencyId, new ProgressionAmount(Math.Max(0, RankCosts[i])), isCredit: false);
            }

            return new ResearchNodeDefinition(Id, MaxRank, costs);
        }

        private static int[] Copy(IReadOnlyList<int> source)
        {
            int[] copy = new int[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    public sealed class SurvivorsRewardDefinition
    {
        public SurvivorsRewardDefinition(string id, CurrencyId currencyId, int currencyAmount, TrackId trackId, int trackAmount)
        {
            Id = id ?? string.Empty;
            CurrencyId = currencyId;
            CurrencyAmount = Math.Max(0, currencyAmount);
            TrackId = trackId;
            TrackAmount = Math.Max(0, trackAmount);
        }

        public string Id { get; }
        public CurrencyId CurrencyId { get; }
        public int CurrencyAmount { get; }
        public TrackId TrackId { get; }
        public int TrackAmount { get; }
    }

    public sealed class SurvivorsMetaProfileDocument
    {
        public const int CurrentSchemaVersion = 4;

        public long LifetimeBloodShards;
        public long UnspentBloodShards;
        public long LifetimeLegacyExperience;
        public int HighestLevelReached;
        public float BestRunDurationSeconds;
        public int CompletedRuns;
        public int BossVictories;
        public bool TutorialSeen;
        public List<SurvivorsPersistentUpgradeRankRecord> PersistentUpgradeRanks = new List<SurvivorsPersistentUpgradeRankRecord>();
        public string SelectedClassId;
        public List<string> UnlockedClassIds = new List<string>();
    }

    public sealed class SurvivorsPersistentUpgradeRankRecord
    {
        public string Id;
        public int Rank;
    }

    public sealed class SurvivorsMetaProgressionService : IDisposable
    {
        public static readonly DocumentId ProfileDocumentId = new DocumentId("survivors-meta-profile");
        private static readonly SaveSlotId DefaultSlotId = new SaveSlotId("survivors-template");

        private readonly IPersistenceService _persistence;
        private readonly SaveSlotId _slotId;
        private readonly SurvivorsMetaProgressionDefinition _definition;
        private ProgressionState _state;
        private SurvivorsMetaProfileDocument _profile;
        private int _debugGrantSequence;
        private bool _disposed;

        public SurvivorsMetaProgressionService(
            IPersistenceService persistence,
            SaveSlotId slotId,
            SurvivorsMetaProgressionDefinition definition = null)
        {
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _slotId = slotId;
            _definition = definition ?? BasicSurvivorsGame.CreateMetaProgressionDefinition();
            _profile = new SurvivorsMetaProfileDocument();
            _state = BuildState(_profile);
        }

        public SurvivorsMetaProfileDocument Profile => _profile;
        public long UnspentBloodShards => _profile == null ? 0 : _profile.UnspentBloodShards;
        public long LifetimeBloodShards => _profile == null ? 0 : _profile.LifetimeBloodShards;
        public long LifetimeLegacyExperience => _profile == null ? 0 : _profile.LifetimeLegacyExperience;
        public int CompletedRuns => _profile == null ? 0 : _profile.CompletedRuns;
        public int BossVictories => _profile == null ? 0 : _profile.BossVictories;
        public int HighestLevelReached => _profile == null ? 0 : _profile.HighestLevelReached;
        public float BestRunDurationSeconds => _profile == null ? 0f : _profile.BestRunDurationSeconds;
        public bool TutorialSeen => _profile != null && _profile.TutorialSeen;

        public static DocumentDefinition<SurvivorsMetaProfileDocument> CreateDocumentDefinition()
        {
            return new DocumentDefinition<SurvivorsMetaProfileDocument>(
                ProfileDocumentId,
                new SchemaVersion(SurvivorsMetaProfileDocument.CurrentSchemaVersion),
                () => new SurvivorsMetaProfileDocument(),
                new DelegateDocumentValidator<SurvivorsMetaProfileDocument>(ValidateProfileDocument),
                new DocumentMigrationSet(new[]
                {
                    new DelegateDocumentMigration(
                        ProfileDocumentId,
                        new SchemaVersion(1),
                        new SchemaVersion(2),
                        MigrateV1ToV2),
                    new DelegateDocumentMigration(
                        ProfileDocumentId,
                        new SchemaVersion(2),
                        new SchemaVersion(3),
                        MigrateV2ToV3),
                    new DelegateDocumentMigration(
                        ProfileDocumentId,
                        new SchemaVersion(3),
                        new SchemaVersion(4),
                        MigrateV3ToV4)
                }));
        }

        public LoadResult<SurvivorsMetaProfileDocument> Load()
        {
            ThrowIfDisposed();
            LoadResult<SurvivorsMetaProfileDocument> result = _persistence.LoadAsync(CreateDocumentDefinition(), _slotId).GetAwaiter().GetResult();
            _profile = result.Succeeded && result.Document != null ? result.Document : new SurvivorsMetaProfileDocument();
            NormalizeProfile(_profile);
            _state = BuildState(_profile);
            return result;
        }

        public WriteResult Save()
        {
            ThrowIfDisposed();
            NormalizeProfile(_profile);
            return _persistence.SaveAsync(CreateDocumentDefinition(), _profile, _slotId).GetAwaiter().GetResult();
        }

        public WriteResult Reset()
        {
            ThrowIfDisposed();
            _profile = new SurvivorsMetaProfileDocument();
            _state = BuildState(_profile);
            WriteResult deleteResult = _persistence.DeleteAsync(new DocumentLocation(ProfileDocumentId, _slotId)).GetAwaiter().GetResult();
            WriteResult saveResult = Save();
            return saveResult.Succeeded ? saveResult : deleteResult;
        }

        public WriteResult MarkTutorialSeen()
        {
            ThrowIfDisposed();
            if (_profile.TutorialSeen)
            {
                return Save();
            }

            _profile.TutorialSeen = true;
            return Save();
        }

        public WriteResult ResetTutorialSeen()
        {
            ThrowIfDisposed();
            _profile.TutorialSeen = false;
            return Save();
        }

        public ProgressionResult GrantRunRewards(SurvivorsRunRewardSummary summary)
        {
            ThrowIfDisposed();
            if (summary == null)
            {
                return ProgressionResult.Fail(ProgressionStatus.InvalidDefinition, default);
            }

            int runNumber = Math.Max(1, _profile.CompletedRuns + 1);
            var reward = new RewardBundle(
                new[]
                {
                    new CurrencyLine(_definition.BloodShardsCurrencyId, new ProgressionAmount(summary.BloodShardsEarned), isCredit: true)
                },
                new[]
                {
                    new XpGrant(_definition.LegacyExperienceTrackId, new ProgressionAmount(summary.LegacyExperienceEarned))
                });
            ProgressionResult result = _state.ApplyReward(_definition.ProgressionCatalog, new ProgressionOperationId("op.survivors.run." + runNumber + ".result"), reward);
            if (!result.Succeeded)
            {
                return result;
            }

            _profile.CompletedRuns++;
            _profile.LifetimeBloodShards += summary.BloodShardsEarned;
            _profile.LifetimeLegacyExperience = _state.GetTrackTotal(_definition.LegacyExperienceTrackId).Value;
            _profile.UnspentBloodShards = _state.GetBalance(_definition.BloodShardsCurrencyId).Value;
            _profile.HighestLevelReached = Math.Max(_profile.HighestLevelReached, summary.LevelReached);
            _profile.BestRunDurationSeconds = Math.Max(_profile.BestRunDurationSeconds, summary.RunDurationSeconds);
            if (summary.Victory)
            {
                _profile.BossVictories++;
            }

            Save();
            return result;
        }

        public ProgressionResult GrantBloodShardsForDebug(int amount)
        {
            ThrowIfDisposed();
            int grant = Math.Max(0, amount);
            if (grant <= 0)
            {
                return ProgressionResult.Fail(ProgressionStatus.InvalidAmount, default);
            }

            ProgressionResult result = _state.ApplyReward(
                _definition.ProgressionCatalog,
                new ProgressionOperationId("op.survivors.debug.blood-shards." + (++_debugGrantSequence)),
                new RewardBundle(
                    new[] { new CurrencyLine(_definition.BloodShardsCurrencyId, new ProgressionAmount(grant), isCredit: true) }));
            if (!result.Succeeded)
            {
                return result;
            }

            _profile.LifetimeBloodShards += grant;
            _profile.UnspentBloodShards = _state.GetBalance(_definition.BloodShardsCurrencyId).Value;
            Save();
            return result;
        }

        public bool TryPurchasePersistentUpgrade(string id)
        {
            ThrowIfDisposed();
            if (!_definition.TryGetPersistentUpgrade(id, out SurvivorsPersistentUpgradeDefinition upgrade))
            {
                return false;
            }

            int nextRank = GetPersistentUpgradeRank(id) + 1;
            ProgressionResult result = _state.PurchaseResearch(
                _definition.ProgressionCatalog,
                new ProgressionOperationId("op.survivors.meta." + id.Replace('.', '-') + ".rank" + nextRank),
                upgrade.Id);
            if (!result.Succeeded)
            {
                return false;
            }

            SetPersistentUpgradeRank(id, nextRank);
            _profile.UnspentBloodShards = _state.GetBalance(_definition.BloodShardsCurrencyId).Value;
            Save();
            return true;
        }

        public int GetPersistentUpgradeRank(string id)
        {
            if (_profile == null || _profile.PersistentUpgradeRanks == null)
            {
                return 0;
            }

            for (int i = 0; i < _profile.PersistentUpgradeRanks.Count; i++)
            {
                SurvivorsPersistentUpgradeRankRecord record = _profile.PersistentUpgradeRanks[i];
                if (record != null && string.Equals(record.Id, id, StringComparison.Ordinal))
                {
                    return Math.Max(0, record.Rank);
                }
            }

            return 0;
        }

        public float GetPersistentDamageBonus(string targetId)
        {
            return GetPersistentUpgradeBonus(BasicSurvivorsGame.MetaDamageEffectId, targetId);
        }

        public float GetPersistentUpgradeBonus(string effectId, string targetId)
        {
            if (_profile == null)
            {
                return 0f;
            }

            float bonus = 0f;
            for (int i = 0; i < _definition.PersistentUpgrades.Count; i++)
            {
                SurvivorsPersistentUpgradeDefinition upgrade = _definition.PersistentUpgrades[i];
                if (upgrade == null ||
                    !string.Equals(upgrade.EffectId, effectId, StringComparison.Ordinal) ||
                    !string.Equals(upgrade.TargetId, targetId, StringComparison.Ordinal))
                {
                    continue;
                }

                bonus += GetPersistentUpgradeRank(upgrade.Id.Value) * upgrade.AmountPerRank;
            }

            return bonus;
        }

        public IReadOnlyList<string> UnlockedClassIds => _profile == null || _profile.UnlockedClassIds == null
            ? Array.Empty<string>()
            : _profile.UnlockedClassIds;

        public string SelectedClassId => _profile == null ? string.Empty : (_profile.SelectedClassId ?? string.Empty);

        public bool EnsureDefaultClassUnlocks(SurvivorsClassLibraryDefinition classLibrary)
        {
            ThrowIfDisposed();
            bool changed = false;
            _profile.UnlockedClassIds ??= new List<string>();
            if (classLibrary != null)
            {
                for (int i = 0; i < classLibrary.Classes.Count; i++)
                {
                    SurvivorsClassDefinition definition = classLibrary.Classes[i];
                    if (definition == null || !definition.IsUnlockedByDefault || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    if (!ContainsClassId(_profile.UnlockedClassIds, definition.Id))
                    {
                        _profile.UnlockedClassIds.Add(definition.Id);
                        changed = true;
                    }
                }

                SurvivorsClassDefinition selected = ResolveSelectedClassWithoutMutation(classLibrary);
                if (selected != null && !string.Equals(_profile.SelectedClassId, selected.Id, StringComparison.Ordinal))
                {
                    _profile.SelectedClassId = selected.Id;
                    changed = true;
                }
            }

            if (changed)
            {
                NormalizeProfile(_profile);
                Save();
            }

            return changed;
        }

        public bool IsClassUnlocked(string classId, SurvivorsClassLibraryDefinition classLibrary)
        {
            if (classLibrary == null || string.IsNullOrWhiteSpace(classId) || !classLibrary.TryGetClass(classId, out SurvivorsClassDefinition definition))
            {
                return false;
            }

            return definition.IsUnlockedByDefault || ContainsClassId(_profile.UnlockedClassIds, definition.Id);
        }

        public bool UnlockClass(string classId, SurvivorsClassLibraryDefinition classLibrary)
        {
            ThrowIfDisposed();
            if (classLibrary == null || string.IsNullOrWhiteSpace(classId) || !classLibrary.TryGetClass(classId, out SurvivorsClassDefinition definition))
            {
                return false;
            }

            _profile.UnlockedClassIds ??= new List<string>();
            if (ContainsClassId(_profile.UnlockedClassIds, definition.Id))
            {
                return false;
            }

            _profile.UnlockedClassIds.Add(definition.Id);
            if (string.IsNullOrWhiteSpace(_profile.SelectedClassId))
            {
                _profile.SelectedClassId = definition.Id;
            }

            NormalizeProfile(_profile);
            Save();
            return true;
        }

        public bool TrySetSelectedClass(string classId, SurvivorsClassLibraryDefinition classLibrary)
        {
            ThrowIfDisposed();
            if (!IsClassUnlocked(classId, classLibrary))
            {
                return false;
            }

            if (string.Equals(_profile.SelectedClassId, classId, StringComparison.Ordinal))
            {
                return true;
            }

            _profile.SelectedClassId = classId;
            NormalizeProfile(_profile);
            Save();
            return true;
        }

        public SurvivorsClassDefinition ResolveSelectedClass(SurvivorsClassLibraryDefinition classLibrary)
        {
            ThrowIfDisposed();
            EnsureDefaultClassUnlocks(classLibrary);
            return ResolveSelectedClassWithoutMutation(classLibrary);
        }

        public RewardBundle CreateRewardBundle(SurvivorsRewardDefinition reward)
        {
            return new RewardBundle(
                new[] { new CurrencyLine(reward.CurrencyId, new ProgressionAmount(reward.CurrencyAmount), isCredit: true) },
                new[] { new XpGrant(reward.TrackId, new ProgressionAmount(reward.TrackAmount)) });
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _persistence.Dispose();
            _disposed = true;
        }

        private ProgressionState BuildState(SurvivorsMetaProfileDocument profile)
        {
            NormalizeProfile(profile);
            ProgressionState state = new ProgressionState();
            long spent = CalculateSpentBloodShards(profile);
            long seedBalance = Math.Max(0, profile.UnspentBloodShards + spent);
            if (seedBalance > 0 || profile.LifetimeLegacyExperience > 0)
            {
                state.ApplyReward(
                    _definition.ProgressionCatalog,
                    new ProgressionOperationId("op.survivors.profile.seed"),
                    new RewardBundle(
                        new[] { new CurrencyLine(_definition.BloodShardsCurrencyId, new ProgressionAmount(seedBalance), isCredit: true) },
                        new[] { new XpGrant(_definition.LegacyExperienceTrackId, new ProgressionAmount(profile.LifetimeLegacyExperience)) }));
            }

            for (int i = 0; i < profile.PersistentUpgradeRanks.Count; i++)
            {
                SurvivorsPersistentUpgradeRankRecord record = profile.PersistentUpgradeRanks[i];
                if (record == null || !_definition.TryGetPersistentUpgrade(record.Id, out SurvivorsPersistentUpgradeDefinition upgrade))
                {
                    continue;
                }

                int rank = Math.Min(Math.Max(0, record.Rank), upgrade.MaxRank);
                for (int purchase = 0; purchase < rank; purchase++)
                {
                    state.PurchaseResearch(
                        _definition.ProgressionCatalog,
                        new ProgressionOperationId("op.survivors.profile.restore." + record.Id.Replace('.', '-') + "." + purchase),
                        upgrade.Id);
                }
            }

            profile.UnspentBloodShards = state.GetBalance(_definition.BloodShardsCurrencyId).Value;
            profile.LifetimeLegacyExperience = state.GetTrackTotal(_definition.LegacyExperienceTrackId).Value;
            return state;
        }

        private long CalculateSpentBloodShards(SurvivorsMetaProfileDocument profile)
        {
            long spent = 0;
            if (profile.PersistentUpgradeRanks == null)
            {
                return spent;
            }

            for (int i = 0; i < profile.PersistentUpgradeRanks.Count; i++)
            {
                SurvivorsPersistentUpgradeRankRecord record = profile.PersistentUpgradeRanks[i];
                if (record == null || !_definition.TryGetPersistentUpgrade(record.Id, out SurvivorsPersistentUpgradeDefinition upgrade))
                {
                    continue;
                }

                int rank = Math.Min(Math.Max(0, record.Rank), Math.Min(upgrade.MaxRank, upgrade.RankCosts.Count));
                for (int costIndex = 0; costIndex < rank; costIndex++)
                {
                    spent += Math.Max(0, upgrade.RankCosts[costIndex]);
                }
            }

            return spent;
        }

        private void SetPersistentUpgradeRank(string id, int rank)
        {
            _profile.PersistentUpgradeRanks ??= new List<SurvivorsPersistentUpgradeRankRecord>();
            for (int i = 0; i < _profile.PersistentUpgradeRanks.Count; i++)
            {
                SurvivorsPersistentUpgradeRankRecord record = _profile.PersistentUpgradeRanks[i];
                if (record != null && string.Equals(record.Id, id, StringComparison.Ordinal))
                {
                    record.Rank = Math.Max(0, rank);
                    return;
                }
            }

            _profile.PersistentUpgradeRanks.Add(new SurvivorsPersistentUpgradeRankRecord { Id = id, Rank = Math.Max(0, rank) });
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SurvivorsMetaProgressionService));
            }
        }

        private static ValidationResult ValidateProfileDocument(SurvivorsMetaProfileDocument profile)
        {
            if (profile == null)
            {
                return ValidationResult.Failure("Survivors meta profile cannot be null.");
            }

            if (profile.LifetimeBloodShards < 0 || profile.UnspentBloodShards < 0 || profile.LifetimeLegacyExperience < 0)
            {
                return ValidationResult.Failure("Survivors meta profile currency and XP values cannot be negative.");
            }

            if (profile.UnspentBloodShards > profile.LifetimeBloodShards)
            {
                return ValidationResult.Failure("Survivors meta profile unspent blood shards cannot exceed lifetime blood shards.");
            }

            if (profile.HighestLevelReached < 0 || profile.BestRunDurationSeconds < 0f || profile.CompletedRuns < 0 || profile.BossVictories < 0)
            {
                return ValidationResult.Failure("Survivors meta profile run summary values cannot be negative.");
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (profile.PersistentUpgradeRanks != null)
            {
                for (int i = 0; i < profile.PersistentUpgradeRanks.Count; i++)
                {
                    SurvivorsPersistentUpgradeRankRecord record = profile.PersistentUpgradeRanks[i];
                    if (record == null || string.IsNullOrWhiteSpace(record.Id))
                    {
                        return ValidationResult.Failure("Survivors meta profile upgrade rank entries require ids.");
                    }

                    if (!seen.Add(record.Id))
                    {
                        return ValidationResult.Failure("Survivors meta profile contains duplicate upgrade rank id: " + record.Id);
                    }

                    if (record.Rank < 0)
                    {
                        return ValidationResult.Failure("Survivors meta profile upgrade ranks cannot be negative.");
                    }
                }
            }

            if (profile.UnlockedClassIds != null)
            {
                seen.Clear();
                for (int i = 0; i < profile.UnlockedClassIds.Count; i++)
                {
                    string id = profile.UnlockedClassIds[i];
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        return ValidationResult.Failure("Survivors meta profile unlocked class entries require ids.");
                    }

                    if (!seen.Add(id))
                    {
                        return ValidationResult.Failure("Survivors meta profile contains duplicate unlocked class id: " + id);
                    }
                }
            }

            if (profile.SelectedClassId != null && profile.SelectedClassId.Length > 0 && string.IsNullOrWhiteSpace(profile.SelectedClassId))
            {
                return ValidationResult.Failure("Survivors meta profile selected class id cannot be whitespace.");
            }

            return ValidationResult.Success();
        }

        private static void NormalizeProfile(SurvivorsMetaProfileDocument profile)
        {
            profile.PersistentUpgradeRanks ??= new List<SurvivorsPersistentUpgradeRankRecord>();
            profile.UnlockedClassIds ??= new List<string>();
            profile.SelectedClassId = string.IsNullOrWhiteSpace(profile.SelectedClassId) ? string.Empty : profile.SelectedClassId.Trim();
            NormalizeClassIdList(profile.UnlockedClassIds);
            profile.LifetimeBloodShards = Math.Max(0, profile.LifetimeBloodShards);
            profile.UnspentBloodShards = Math.Max(0, Math.Min(profile.UnspentBloodShards, profile.LifetimeBloodShards));
            profile.LifetimeLegacyExperience = Math.Max(0, profile.LifetimeLegacyExperience);
            profile.HighestLevelReached = Math.Max(0, profile.HighestLevelReached);
            profile.BestRunDurationSeconds = Math.Max(0f, profile.BestRunDurationSeconds);
            profile.CompletedRuns = Math.Max(0, profile.CompletedRuns);
            profile.BossVictories = Math.Max(0, profile.BossVictories);
        }

        private static string MigrateV1ToV2(string payloadJson, IPersistenceSerializer serializer)
        {
            SurvivorsMetaProfileDocumentV1 old = serializer.Deserialize<SurvivorsMetaProfileDocumentV1>(payloadJson) ?? new SurvivorsMetaProfileDocumentV1();
            var migrated = new SurvivorsMetaProfileDocument
            {
                LifetimeBloodShards = Math.Max(0, old.BloodShards),
                UnspentBloodShards = Math.Max(0, old.BloodShards),
                LifetimeLegacyExperience = Math.Max(0, old.LegacyExperience),
                HighestLevelReached = Math.Max(0, old.HighestLevelReached),
                BestRunDurationSeconds = Math.Max(0f, old.BestRunDurationSeconds),
                CompletedRuns = Math.Max(0, old.CompletedRuns),
                BossVictories = Math.Max(0, old.BossVictories),
                TutorialSeen = false,
                PersistentUpgradeRanks = old.PersistentUpgradeRanks ?? new List<SurvivorsPersistentUpgradeRankRecord>(),
                SelectedClassId = string.Empty,
                UnlockedClassIds = new List<string>()
            };
            return serializer.Serialize(migrated);
        }

        private static string MigrateV2ToV3(string payloadJson, IPersistenceSerializer serializer)
        {
            SurvivorsMetaProfileDocumentV2 old = serializer.Deserialize<SurvivorsMetaProfileDocumentV2>(payloadJson) ?? new SurvivorsMetaProfileDocumentV2();
            var migrated = new SurvivorsMetaProfileDocument
            {
                LifetimeBloodShards = Math.Max(0, old.LifetimeBloodShards),
                UnspentBloodShards = Math.Max(0, old.UnspentBloodShards),
                LifetimeLegacyExperience = Math.Max(0, old.LifetimeLegacyExperience),
                HighestLevelReached = Math.Max(0, old.HighestLevelReached),
                BestRunDurationSeconds = Math.Max(0f, old.BestRunDurationSeconds),
                CompletedRuns = Math.Max(0, old.CompletedRuns),
                BossVictories = Math.Max(0, old.BossVictories),
                TutorialSeen = false,
                PersistentUpgradeRanks = old.PersistentUpgradeRanks ?? new List<SurvivorsPersistentUpgradeRankRecord>(),
                SelectedClassId = string.Empty,
                UnlockedClassIds = new List<string>()
            };
            NormalizeProfile(migrated);
            return serializer.Serialize(migrated);
        }

        private static string MigrateV3ToV4(string payloadJson, IPersistenceSerializer serializer)
        {
            SurvivorsMetaProfileDocumentV3 old = serializer.Deserialize<SurvivorsMetaProfileDocumentV3>(payloadJson) ?? new SurvivorsMetaProfileDocumentV3();
            var migrated = new SurvivorsMetaProfileDocument
            {
                LifetimeBloodShards = Math.Max(0, old.LifetimeBloodShards),
                UnspentBloodShards = Math.Max(0, old.UnspentBloodShards),
                LifetimeLegacyExperience = Math.Max(0, old.LifetimeLegacyExperience),
                HighestLevelReached = Math.Max(0, old.HighestLevelReached),
                BestRunDurationSeconds = Math.Max(0f, old.BestRunDurationSeconds),
                CompletedRuns = Math.Max(0, old.CompletedRuns),
                BossVictories = Math.Max(0, old.BossVictories),
                TutorialSeen = old.TutorialSeen || old.CompletedRuns > 0 || old.BossVictories > 0 || old.HighestLevelReached > 0,
                PersistentUpgradeRanks = old.PersistentUpgradeRanks ?? new List<SurvivorsPersistentUpgradeRankRecord>(),
                SelectedClassId = string.IsNullOrWhiteSpace(old.SelectedClassId) ? string.Empty : old.SelectedClassId,
                UnlockedClassIds = old.UnlockedClassIds ?? new List<string>()
            };
            NormalizeProfile(migrated);
            return serializer.Serialize(migrated);
        }

        private SurvivorsClassDefinition ResolveSelectedClassWithoutMutation(SurvivorsClassLibraryDefinition classLibrary)
        {
            if (classLibrary == null || classLibrary.Classes.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(_profile.SelectedClassId) &&
                classLibrary.TryGetClass(_profile.SelectedClassId, out SurvivorsClassDefinition selected) &&
                IsClassUnlocked(selected.Id, classLibrary))
            {
                return selected;
            }

            for (int i = 0; i < classLibrary.Classes.Count; i++)
            {
                SurvivorsClassDefinition definition = classLibrary.Classes[i];
                if (definition != null && IsClassUnlocked(definition.Id, classLibrary))
                {
                    return definition;
                }
            }

            return classLibrary.FirstDefaultUnlocked();
        }

        private static bool ContainsClassId(List<string> classIds, string classId)
        {
            if (classIds == null || string.IsNullOrWhiteSpace(classId))
            {
                return false;
            }

            for (int i = 0; i < classIds.Count; i++)
            {
                if (string.Equals(classIds[i], classId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void NormalizeClassIdList(List<string> classIds)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = classIds.Count - 1; i >= 0; i--)
            {
                string normalized = string.IsNullOrWhiteSpace(classIds[i]) ? string.Empty : classIds[i].Trim();
                if (string.IsNullOrWhiteSpace(normalized) || !seen.Add(normalized))
                {
                    classIds.RemoveAt(i);
                    continue;
                }

                classIds[i] = normalized;
            }
        }

        private sealed class SurvivorsMetaProfileDocumentV1
        {
            public long BloodShards;
            public long LegacyExperience;
            public int HighestLevelReached;
            public float BestRunDurationSeconds;
            public int CompletedRuns;
            public int BossVictories;
            public List<SurvivorsPersistentUpgradeRankRecord> PersistentUpgradeRanks = new List<SurvivorsPersistentUpgradeRankRecord>();
        }

        private sealed class SurvivorsMetaProfileDocumentV2
        {
            public long LifetimeBloodShards;
            public long UnspentBloodShards;
            public long LifetimeLegacyExperience;
            public int HighestLevelReached;
            public float BestRunDurationSeconds;
            public int CompletedRuns;
            public int BossVictories;
            public List<SurvivorsPersistentUpgradeRankRecord> PersistentUpgradeRanks = new List<SurvivorsPersistentUpgradeRankRecord>();
        }

        private sealed class SurvivorsMetaProfileDocumentV3
        {
            public long LifetimeBloodShards;
            public long UnspentBloodShards;
            public long LifetimeLegacyExperience;
            public int HighestLevelReached;
            public float BestRunDurationSeconds;
            public int CompletedRuns;
            public int BossVictories;
            public bool TutorialSeen;
            public List<SurvivorsPersistentUpgradeRankRecord> PersistentUpgradeRanks = new List<SurvivorsPersistentUpgradeRankRecord>();
            public string SelectedClassId;
            public List<string> UnlockedClassIds = new List<string>();
        }
    }
}
