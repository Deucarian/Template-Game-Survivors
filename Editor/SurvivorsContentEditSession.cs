using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    internal sealed class SurvivorsContentEditSession : IGameContentEditSession
    {
        private readonly SurvivorsEditableSource _source;
        private readonly byte[] _originalBytes;
        private readonly List<Dictionary<string, GameContentFieldValue>> _history =
            new List<Dictionary<string, GameContentFieldValue>>();
        private readonly Action _refreshProvider;
        private readonly SurvivorsEditTransactionHooks _hooks;
        private int _historyIndex;
        private bool _disposed;
        private GameContentSourceRevision _committedRevision;
        private string _proposedHash = string.Empty;
        private SurvivorsRecoveryHandle _recovery;

        public SurvivorsContentEditSession(
            SurvivorsEditableSource source,
            Action refreshProvider,
            SurvivorsEditTransactionHooks hooks = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _originalBytes = (byte[])source.ExactBytes.Clone();
            _refreshProvider = refreshProvider;
            _hooks = hooks;
            BackendId = SurvivorsContentPackProvider.StableProviderId;
            RecordKey = source.Record.CanonicalKey;
            SourceTarget = source.SourceTarget;
            OriginalRevision = source.Revision;
            Fields = source.Definition.Fields.ToArray();
            var baseline = Copy(source.Definition.Values);
            _history.Add(baseline);
            Snapshot = new GameContentEditSnapshot(
                RecordKey,
                SourceTarget,
                OriginalRevision,
                baseline,
                DateTime.UtcNow,
                SurvivorsContentSourceRevision.BackendSchemaVersion);
            State = GameContentEditSessionState.Clean;

            GameContentStaleCheckResult initialCheck = CheckStale();
            if (initialCheck.IsStale)
                throw new InvalidOperationException(initialCheck.Message);
        }

        public string BackendId { get; }
        public GameContentRecordKey RecordKey { get; }
        public GameContentSourceTarget SourceTarget { get; }
        public GameContentSourceRevision OriginalRevision { get; }
        public GameContentEditSessionState State { get; private set; }
        public GameContentEditSnapshot Snapshot { get; }
        public IReadOnlyList<GameContentFieldDescriptor> Fields { get; }
        public IReadOnlyList<GameContentProposedChange> Changes => BuildChanges();
        public bool CanUndo => !_disposed && IsStagingState && _historyIndex > 0;
        public bool CanRedo => !_disposed && IsStagingState && _historyIndex < _history.Count - 1;

        private bool IsStagingState => State == GameContentEditSessionState.Clean ||
                                       State == GameContentEditSessionState.Dirty;
        private Dictionary<string, GameContentFieldValue> Current => _history[_historyIndex];

        public GameContentEditOperationResult Apply(string fieldId, GameContentFieldValue value)
        {
            if (_disposed) return GameContentEditOperationResult.Failure("The edit session is disposed.");
            if (!IsStagingState) return GameContentEditOperationResult.Failure("The edit session is not accepting staged changes in its current state.");
            GameContentFieldDescriptor field = Fields.FirstOrDefault(candidate =>
                string.Equals(candidate.FieldId, fieldId, StringComparison.Ordinal));
            if (field == null) return GameContentEditOperationResult.Failure("Field '" + fieldId + "' is not exposed by this record.");
            if (!field.Accepts(value, out string reason)) return GameContentEditOperationResult.Failure(reason);
            if (Current[field.FieldId].Equals(value)) return GameContentEditOperationResult.Success("The proposed value is unchanged.");

            if (_historyIndex < _history.Count - 1)
                _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
            var next = Copy(Current);
            next[field.FieldId] = value;
            _history.Add(next);
            _historyIndex++;
            RefreshDirtyState();
            return GameContentEditOperationResult.Success("Staged " + field.DisplayName + ".");
        }

        public GameContentEditOperationResult Undo()
        {
            if (!CanUndo) return GameContentEditOperationResult.Failure("There is no staged change to undo.");
            _historyIndex--;
            RefreshDirtyState();
            return GameContentEditOperationResult.Success("Undid the latest staged scalar change.");
        }

        public GameContentEditOperationResult Redo()
        {
            if (!CanRedo) return GameContentEditOperationResult.Failure("There is no staged change to redo.");
            _historyIndex++;
            RefreshDirtyState();
            return GameContentEditOperationResult.Success("Restored the latest staged scalar change.");
        }

        public GameContentValidationPreview Preview()
        {
            if (_disposed) return GameContentValidationPreview.Error("Editing", "The edit session is disposed.");
            if (!IsStagingState) return GameContentValidationPreview.Error("Editing", "Validation preview is unavailable in the current session state.");
            GameContentStaleCheckResult stale = CheckStale();
            if (stale.IsStale) return GameContentValidationPreview.Error("Source", stale.Message);

            var issues = new List<GameContentAuthoringValidationIssue>();
            foreach (GameContentFieldDescriptor field in Fields.Where(candidate => !candidate.IsReadOnly))
            {
                if (!field.Accepts(Current[field.FieldId], out string reason))
                    issues.Add(GameContentAuthoringValidationIssue.Error(field.DisplayName, reason));
            }
            if (issues.Any(issue => issue.Severity == GameContentAuthoringValidationSeverity.Error))
                return new GameContentValidationPreview(issues, false);

            if (!TryBuildProposed(out string proposedText, out _, out string patchError))
                return GameContentValidationPreview.Error("JSON", patchError);
            GameContentAuthoringValidationResult validation = SurvivorsContentPackIndex.ValidateSelectedSources(
                _source.Manifest,
                _source.SourceId,
                proposedText);
            issues.AddRange(validation.Issues);
            return new GameContentValidationPreview(issues);
        }

        public GameContentStaleCheckResult CheckStale()
        {
            if (_disposed)
            {
                var disposedRevision = new GameContentSourceRevision("disposed::" + OriginalRevision.Token);
                return GameContentStaleCheckResult.Stale("The edit session is disposed.", disposedRevision);
            }

            GameContentSourceRevision expected = _committedRevision ?? OriginalRevision;
            if (State == GameContentEditSessionState.RolledBack) expected = OriginalRevision;
            if (!SurvivorsEditableSource.TryCreate(
                    _source.Manifest,
                    _source.Record,
                    true,
                    out SurvivorsEditableSource current,
                    out string error))
            {
                State = GameContentEditSessionState.Stale;
                var unresolved = new GameContentSourceRevision("unresolved::" + SurvivorsContentEditHash.Sha256(error));
                return GameContentStaleCheckResult.Stale(
                    "The authored source can no longer be resolved safely: " + error,
                    unresolved);
            }

            if (!current.SourceTarget.Equals(SourceTarget) || !current.Revision.Equals(expected))
            {
                State = GameContentEditSessionState.Stale;
                return GameContentStaleCheckResult.Stale(
                    "The JSON bytes, source path/GUID, manifest source list, pack, or canonical record changed after this session began.",
                    current.Revision);
            }

            if (State == GameContentEditSessionState.Stale) RefreshDirtyState();
            return GameContentStaleCheckResult.Current(current.Revision);
        }

        public GameContentCommitResult Commit(bool confirmWarnings)
        {
            if (_disposed) return GameContentCommitResult.Failure("The edit session is disposed.", OriginalRevision);
            if (State != GameContentEditSessionState.Dirty)
                return GameContentCommitResult.Failure("Only a dirty edit session can be committed.", OriginalRevision);

            GameContentStaleCheckResult stale = CheckStale();
            if (stale.IsStale) return GameContentCommitResult.Failure(stale.Message, OriginalRevision);
            if (!TryBuildProposed(out string proposedText, out byte[] proposedBytes, out string patchError))
                return GameContentCommitResult.Failure(patchError, OriginalRevision);

            GameContentValidationPreview preview = Preview();
            if (!preview.CanCommit)
                return GameContentCommitResult.Failure("Strict proposed-pack validation failed. No source bytes were written.", OriginalRevision);
            if (preview.RequiresWarningConfirmation && !confirmWarnings)
                return GameContentCommitResult.Failure("Confirm the strict validation warnings before committing.", OriginalRevision);

            if (!SurvivorsEditableSource.TryCreate(
                    _source.Manifest,
                    _source.Record,
                    true,
                    out SurvivorsEditableSource current,
                    out string sourceError))
                return GameContentCommitResult.Failure("The writable source failed its final safety check: " + sourceError, OriginalRevision);
            if (!current.SourceTarget.Equals(SourceTarget) || !current.Revision.Equals(OriginalRevision))
            {
                State = GameContentEditSessionState.Stale;
                return GameContentCommitResult.Failure("The source became stale immediately before commit.", OriginalRevision);
            }

            _proposedHash = SurvivorsContentEditHash.Sha256(proposedBytes);
            GameContentSourceRevision expectedRevision = current.CreateRevision(proposedBytes);
            if (!SurvivorsRecoveryStore.TryPrepare(current, _proposedHash, out _recovery, out string recoveryError))
                return GameContentCommitResult.Failure(recoveryError, OriginalRevision);

            State = GameContentEditSessionState.Committing;
            if (!SurvivorsAtomicFile.TryReplace(current.SourcePath.FullPath, proposedBytes, _hooks, out string writeError))
                return HandleCommitFailure(writeError, expectedRevision);

            if (!TryImportAndVerify(
                    proposedBytes,
                    expectedRevision,
                    Current,
                    proposedText,
                    out SurvivorsEditableSource committed,
                    out string verifyError))
                return HandleCommitFailure(verifyError, expectedRevision);

            _committedRevision = committed.Revision;
            SurvivorsRecoveryStore.Update(
                _recovery,
                "Verified",
                "Atomic replacement, import, token verification, strict pack validation, and reindex completed.");
            SurvivorsRecoveryStore.PruneResolved(_recovery);
            State = GameContentEditSessionState.Committed;
            return new GameContentCommitResult(
                true,
                "Committed the selected scalar tokens and reindexed " + _source.Manifest.DisplayName + ".",
                OriginalRevision,
                _committedRevision,
                true,
                true,
                false);
        }

        public GameContentRollbackResult Rollback()
        {
            if (_disposed) return GameContentRollbackResult.Failure("The edit session is disposed.", OriginalRevision);
            if (State == GameContentEditSessionState.Committing)
                return GameContentRollbackResult.Failure("Rollback is disabled while the source transaction is committing.", OriginalRevision);

            if (_committedRevision == null && State != GameContentEditSessionState.RecoveryRequired)
            {
                State = GameContentEditSessionState.RolledBack;
                return new GameContentRollbackResult(
                    true,
                    "Cancelled the edit session without changing source bytes.",
                    OriginalRevision);
            }
            if (State == GameContentEditSessionState.RolledBack)
                return new GameContentRollbackResult(true, "The exact original bytes are already restored.", OriginalRevision);

            if (!TryReadCurrentHash(out string currentHash, out string readError))
                return RollbackRecoveryRequired("Current source bytes could not be verified before rollback: " + readError, _committedRevision);

            string originalHash = SurvivorsContentEditHash.Sha256(_originalBytes);
            if (string.Equals(currentHash, originalHash, StringComparison.Ordinal))
            {
                string originalText = _source.Document.Text;
                if (!TryImportAndVerify(
                        _originalBytes,
                        OriginalRevision,
                        Snapshot.FieldValues,
                        originalText,
                        out _,
                        out string existingVerifyError))
                    return RollbackRecoveryRequired(existingVerifyError, OriginalRevision);
                return CompleteRollback("Verified the already-restored exact original bytes.");
            }

            if (string.IsNullOrWhiteSpace(_proposedHash) || !string.Equals(currentHash, _proposedHash, StringComparison.Ordinal))
            {
                State = GameContentEditSessionState.Stale;
                return GameContentRollbackResult.Failure(
                    "Rollback refused because the source changed after commit. The later bytes were not overwritten.",
                    new GameContentSourceRevision("external::" + currentHash));
            }

            if (!SurvivorsEditableSource.TryCreate(
                    _source.Manifest,
                    _source.Record,
                    true,
                    out SurvivorsEditableSource current,
                    out string sourceError))
                return RollbackRecoveryRequired("The committed source failed its rollback safety check: " + sourceError, _committedRevision);
            if (_committedRevision != null && !current.Revision.Equals(_committedRevision))
            {
                State = GameContentEditSessionState.Stale;
                return GameContentRollbackResult.Failure(
                    "Rollback refused because the committed source revision is no longer current.",
                    current.Revision);
            }

            SurvivorsRecoveryStore.Update(_recovery, "RollbackPrepared", "Restoring exact pre-commit bytes after a revision check.");
            if (!SurvivorsAtomicFile.TryReplace(current.SourcePath.FullPath, _originalBytes, _hooks, out string replaceError))
            {
                if (TryReadCurrentHash(out string hashAfterFailure, out _) &&
                    string.Equals(hashAfterFailure, _proposedHash, StringComparison.Ordinal))
                {
                    State = GameContentEditSessionState.Committed;
                    SurvivorsRecoveryStore.Update(_recovery, "Verified", "Rollback replacement failed before changing the committed source: " + replaceError);
                    return GameContentRollbackResult.Failure(replaceError, _committedRevision);
                }
                if (!string.Equals(hashAfterFailure, originalHash, StringComparison.Ordinal))
                    return RollbackRecoveryRequired(replaceError, _committedRevision);
            }

            if (!TryImportAndVerify(
                    _originalBytes,
                    OriginalRevision,
                    Snapshot.FieldValues,
                    _source.Document.Text,
                    out _,
                    out string verifyError))
                return RollbackRecoveryRequired(verifyError, OriginalRevision);
            return CompleteRollback("Restored the exact pre-commit JSON bytes and reindexed the pack.");
        }

        public void Dispose()
        {
            _disposed = true;
        }

        private GameContentCommitResult HandleCommitFailure(
            string failure,
            GameContentSourceRevision proposedRevision)
        {
            if (!TryReadCurrentHash(out string currentHash, out string readError))
                return CommitRecoveryRequired(failure + " Final source bytes could not be read: " + readError, proposedRevision);

            string originalHash = SurvivorsContentEditHash.Sha256(_originalBytes);
            if (string.Equals(currentHash, originalHash, StringComparison.Ordinal))
            {
                SurvivorsRecoveryStore.DeletePrepared(_recovery);
                _recovery = null;
                State = GameContentEditSessionState.Dirty;
                return GameContentCommitResult.Failure(failure + " The exact original bytes remain in place.", OriginalRevision);
            }
            if (!string.Equals(currentHash, _proposedHash, StringComparison.Ordinal))
                return CommitRecoveryRequired(failure + " The final source hash is neither the original nor the proposed hash.", proposedRevision);

            if (!SurvivorsAtomicFile.TryReplace(_source.SourcePath.FullPath, _originalBytes, _hooks, out string restoreError))
                return CommitRecoveryRequired(failure + " Automatic restoration failed: " + restoreError, proposedRevision);
            if (!TryImportAndVerify(
                    _originalBytes,
                    OriginalRevision,
                    Snapshot.FieldValues,
                    _source.Document.Text,
                    out _,
                    out string verifyError))
                return CommitRecoveryRequired(failure + " Original bytes were written but restoration verification failed: " + verifyError, proposedRevision);

            SurvivorsRecoveryStore.Update(
                _recovery,
                "RolledBackAfterCommitFailure",
                "Commit failed after source replacement; exact original bytes were atomically restored and verified.");
            SurvivorsRecoveryStore.PruneResolved(_recovery);
            State = GameContentEditSessionState.RolledBack;
            return GameContentCommitResult.Failure(
                failure + " The transaction restored and verified the exact original bytes.",
                OriginalRevision);
        }

        private GameContentCommitResult CommitRecoveryRequired(
            string message,
            GameContentSourceRevision currentRevision)
        {
            State = GameContentEditSessionState.RecoveryRequired;
            SurvivorsRecoveryStore.Update(_recovery, "RecoveryRequired", message);
            GameContentRecoveryRecord recovery = SurvivorsRecoveryStore.ToRecoveryRecord(
                _recovery,
                OriginalRevision,
                currentRevision,
                "Commit recovery required",
                message);
            return GameContentCommitResult.Failure(message, OriginalRevision, recovery);
        }

        private GameContentRollbackResult CompleteRollback(string message)
        {
            SurvivorsRecoveryStore.Update(_recovery, "RolledBack", message);
            SurvivorsRecoveryStore.PruneResolved(_recovery);
            State = GameContentEditSessionState.RolledBack;
            return new GameContentRollbackResult(true, message, OriginalRevision);
        }

        private GameContentRollbackResult RollbackRecoveryRequired(
            string message,
            GameContentSourceRevision currentRevision)
        {
            State = GameContentEditSessionState.RecoveryRequired;
            SurvivorsRecoveryStore.Update(_recovery, "RecoveryRequired", message);
            GameContentRecoveryRecord recovery = SurvivorsRecoveryStore.ToRecoveryRecord(
                _recovery,
                _committedRevision ?? OriginalRevision,
                currentRevision,
                "Rollback recovery required",
                message);
            return GameContentRollbackResult.Failure(message, currentRevision ?? OriginalRevision, recovery);
        }

        private bool TryImportAndVerify(
            byte[] expectedBytes,
            GameContentSourceRevision expectedRevision,
            IReadOnlyDictionary<string, GameContentFieldValue> expectedValues,
            string proposedSourceText,
            out SurvivorsEditableSource current,
            out string error)
        {
            current = null;
            error = string.Empty;
            try
            {
                _hooks?.BeforeImport?.Invoke();
                AssetDatabase.ImportAsset(
                    _source.SourcePath.AssetPath,
                    ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                _hooks?.AfterImport?.Invoke();
            }
            catch (Exception exception)
            {
                error = "AssetDatabase reimport failed: " + exception.GetBaseException().Message;
                return false;
            }

            if (!SurvivorsEditableSource.TryCreate(
                    _source.Manifest,
                    _source.Record,
                    true,
                    out current,
                    out error))
            {
                error = "The reimported source could not be resolved: " + error;
                return false;
            }
            if (!current.SourceTarget.Equals(SourceTarget))
            {
                error = "The reimported source resolved to a different physical target.";
                return false;
            }
            if (!current.ExactBytes.SequenceEqual(expectedBytes))
            {
                error = "The reimported source bytes do not match the expected transaction bytes.";
                return false;
            }
            if (!current.Revision.Equals(expectedRevision))
            {
                error = "The source revision changed during reimport or verification.";
                return false;
            }

            foreach (KeyValuePair<string, GameContentFieldValue> expected in expectedValues)
            {
                if (!current.Definition.Values.TryGetValue(expected.Key, out GameContentFieldValue actual) ||
                    !actual.Equals(expected.Value))
                {
                    error = "Edited scalar token verification failed for field '" + expected.Key + "'.";
                    return false;
                }
            }

            GameContentAuthoringValidationResult validation = SurvivorsContentPackIndex.ValidateSelectedSources(
                _source.Manifest,
                _source.SourceId,
                proposedSourceText);
            if (validation.ErrorCount > 0)
            {
                error = "Strict pack validation failed after reimport: " + string.Join(
                    " | ",
                    validation.Issues
                        .Where(issue => issue.Severity == GameContentAuthoringValidationSeverity.Error)
                        .Select(issue => issue.Path + ": " + issue.Message));
                return false;
            }

            try
            {
                _refreshProvider?.Invoke();
            }
            catch (Exception exception)
            {
                error = "The selected pack could not be reindexed: " + exception.GetBaseException().Message;
                return false;
            }
            return true;
        }

        private bool TryBuildProposed(out string proposedText, out byte[] proposedBytes, out string error)
        {
            var changedValues = BuildChanges().ToDictionary(
                change => change.FieldId,
                change => change.ProposedValue,
                StringComparer.Ordinal);
            return SurvivorsLosslessJsonPatcher.TryPatch(
                _source.Document,
                _source.Definition.Tokens,
                changedValues,
                out proposedText,
                out proposedBytes,
                out error);
        }

        private bool TryReadCurrentHash(out string hash, out string error)
        {
            hash = string.Empty;
            error = string.Empty;
            try
            {
                hash = SurvivorsContentEditHash.Sha256(File.ReadAllBytes(_source.SourcePath.FullPath));
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = exception.GetBaseException().Message;
                return false;
            }
        }

        private IReadOnlyList<GameContentProposedChange> BuildChanges()
        {
            var changes = new List<GameContentProposedChange>();
            foreach (GameContentFieldDescriptor field in Fields
                         .OrderBy(candidate => candidate.Order)
                         .ThenBy(candidate => candidate.FieldId, StringComparer.Ordinal))
            {
                GameContentFieldValue original = Snapshot.FieldValues[field.FieldId];
                GameContentFieldValue proposed = Current[field.FieldId];
                if (original.Equals(proposed)) continue;
                changes.Add(new GameContentProposedChange(
                    field.FieldId,
                    original,
                    proposed,
                    field.DisplayName,
                    field.Group,
                    field.Order));
            }
            return changes;
        }

        private void RefreshDirtyState()
        {
            State = BuildChanges().Count == 0
                ? GameContentEditSessionState.Clean
                : GameContentEditSessionState.Dirty;
        }

        private static Dictionary<string, GameContentFieldValue> Copy(
            IReadOnlyDictionary<string, GameContentFieldValue> values)
        {
            return values.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }
    }
}
