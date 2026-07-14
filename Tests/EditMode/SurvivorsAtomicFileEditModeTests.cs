using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deucarian.TemplateGameSurvivors.Editor;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsAtomicFileEditModeTests
    {
        private sealed class Win32IOException : IOException
        {
            public Win32IOException(int win32ErrorCode, string message = null)
                : base(message ?? ("Simulated Win32 error " + win32ErrorCode + "."))
            {
                HResult = unchecked((int)(0x80070000u | (uint)win32ErrorCode));
            }
        }

        private string _directory;

        [SetUp]
        public void SetUp()
        {
            SurvivorsAtomicFile.ResetProbeForTests();
            _directory = Path.Combine(
                Path.GetTempPath(),
                "Deucarian-Survivors-Atomic-Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            SurvivorsAtomicFile.ResetProbeForTests();
            try
            {
                if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
            }
            catch
            {
            }
        }

        [TestCase(32)]
        [TestCase(33)]
        [TestCase(1175)]
        public void TryReplace_RetriesOnlyKnownTransientWin32Codes(int win32ErrorCode)
        {
            byte[] original = { 1, 2, 3 };
            byte[] proposed = { 4, 5, 6 };
            string destination = CreateDestination(original);
            int calls = 0;
            int preconditions = 0;
            var delays = new List<int>();
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (replacement, currentDestination) =>
                {
                    calls++;
                    if (calls == 1) throw new Win32IOException(win32ErrorCode);
                    File.Replace(replacement, currentDestination, null);
                },
                DelayMilliseconds = delays.Add
            };
            SurvivorsAtomicReplaceRequest request = CreateRequest(
                destination,
                original,
                proposed,
                operations,
                () =>
                {
                    preconditions++;
                    return SurvivorsAtomicRetryPreconditionResult.Current();
                });

            bool succeeded = SurvivorsAtomicFile.TryReplace(request, out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.True, result.Message);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.SucceededAfterRetry));
            Assert.That(result.AttemptCount, Is.EqualTo(2));
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(preconditions, Is.EqualTo(1));
            Assert.That(delays, Is.EqualTo(new[] { 25 }));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(proposed));
            Assert.That(result.Failures.Count, Is.EqualTo(1));
            Assert.That(result.Failures[0].OperationStage, Is.EqualTo("AtomicTest.Replace"));
            Assert.That(result.Failures[0].DestinationPath, Is.EqualTo("AtomicTests/destination.json"));
            Assert.That(result.Failures[0].Attempt, Is.EqualTo(1));
            Assert.That(result.Failures[0].MaximumAttempts, Is.EqualTo(4));
            Assert.That(result.Failures[0].ExceptionType, Does.EndWith("Win32IOException"));
            Assert.That(result.Failures[0].HResult, Is.EqualTo(unchecked((int)(0x80070000u | (uint)win32ErrorCode))));
            Assert.That(result.Failures[0].Win32ErrorCode, Is.EqualTo(win32ErrorCode));
            Assert.That(result.Failures[0].Disposition, Is.EqualTo(SurvivorsAtomicReplaceFailureDisposition.RetryScheduled));
            Assert.That(result.Message, Does.Contain("HResult=0x"));
            Assert.That(result.Message, Does.Contain("Win32=" + win32ErrorCode));
            Assert.That(Directory.GetFiles(_directory, "*.deucarian-tmp"), Is.Empty);
        }

        [Test]
        public void TryReplace_UsesExactBackoffBeforeFourthAttemptSuccess()
        {
            byte[] original = { 10 };
            byte[] proposed = { 20 };
            string destination = CreateDestination(original);
            int calls = 0;
            var delays = new List<int>();
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (replacement, currentDestination) =>
                {
                    calls++;
                    if (calls < 4) throw new Win32IOException(32);
                    File.Replace(replacement, currentDestination, null);
                },
                DelayMilliseconds = delays.Add
            };

            bool succeeded = SurvivorsAtomicFile.TryReplace(
                CreateRequest(destination, original, proposed, operations),
                out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.True, result.Message);
            Assert.That(calls, Is.EqualTo(4));
            Assert.That(result.AttemptCount, Is.EqualTo(4));
            Assert.That(result.Failures.Count, Is.EqualTo(3));
            Assert.That(result.Failures.All(failure =>
                failure.Disposition == SurvivorsAtomicReplaceFailureDisposition.RetryScheduled), Is.True);
            Assert.That(delays, Is.EqualTo(new[] { 25, 75, 200 }));
            Assert.That(delays.Sum(), Is.EqualTo(300));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(proposed));
        }

        [Test]
        public void TryReplace_ExhaustsAfterFourAttemptsWithoutChangingDestination()
        {
            byte[] original = { 30 };
            byte[] proposed = { 40 };
            string destination = CreateDestination(original);
            int calls = 0;
            var delays = new List<int>();
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (_, __) =>
                {
                    calls++;
                    throw new Win32IOException(1175);
                },
                DelayMilliseconds = delays.Add
            };

            bool succeeded = SurvivorsAtomicFile.TryReplace(
                CreateRequest(destination, original, proposed, operations),
                out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.False);
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.RetryExhausted));
            Assert.That(result.AttemptCount, Is.EqualTo(4));
            Assert.That(calls, Is.EqualTo(4));
            Assert.That(delays, Is.EqualTo(new[] { 25, 75, 200 }));
            Assert.That(result.Failures.Count, Is.EqualTo(4));
            Assert.That(result.Failures[3].Disposition, Is.EqualTo(SurvivorsAtomicReplaceFailureDisposition.RetryExhausted));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(original));
            Assert.That(Directory.GetFiles(_directory, "*.deucarian-tmp"), Is.Empty);
        }

        [TestCase(5)]
        [TestCase(1176)]
        [TestCase(1177)]
        [TestCase(999)]
        public void TryReplace_DoesNotRetryNonTransientWin32Codes(int win32ErrorCode)
        {
            byte[] original = { 50 };
            string destination = CreateDestination(original);
            int calls = 0;
            var delays = new List<int>();
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (_, __) =>
                {
                    calls++;
                    throw new Win32IOException(win32ErrorCode);
                },
                DelayMilliseconds = delays.Add
            };

            bool succeeded = SurvivorsAtomicFile.TryReplace(
                CreateRequest(destination, original, new byte[] { 51 }, operations),
                out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(delays, Is.Empty);
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure));
            Assert.That(result.Failures.Single().Win32ErrorCode, Is.EqualTo(win32ErrorCode));
            Assert.That(result.Failures.Single().Retryable, Is.False);
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(original));
        }

        [Test]
        public void TryReplace_DoesNotRetryUnauthorizedAccessException()
        {
            byte[] original = { 60 };
            string destination = CreateDestination(original);
            int calls = 0;
            var delays = new List<int>();
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (_, __) =>
                {
                    calls++;
                    throw new UnauthorizedAccessException("simulated access denial");
                },
                DelayMilliseconds = delays.Add
            };

            bool succeeded = SurvivorsAtomicFile.TryReplace(
                CreateRequest(destination, original, new byte[] { 61 }, operations),
                out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(delays, Is.Empty);
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure));
            Assert.That(result.Failures.Single().ExceptionType, Is.EqualTo(typeof(UnauthorizedAccessException).FullName));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(original));
        }

        [Test]
        public void TryReplace_StopsWhenDestinationChangesBeforeRetry()
        {
            byte[] original = { 70 };
            byte[] external = { 71 };
            string destination = CreateDestination(original);
            int calls = 0;
            int preconditions = 0;
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (_, __) =>
                {
                    calls++;
                    throw new Win32IOException(32);
                },
                DelayMilliseconds = _ => File.WriteAllBytes(destination, external)
            };

            bool succeeded = SurvivorsAtomicFile.TryReplace(
                CreateRequest(
                    destination,
                    original,
                    new byte[] { 72 },
                    operations,
                    () =>
                    {
                        preconditions++;
                        return SurvivorsAtomicRetryPreconditionResult.Current();
                    }),
                out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(preconditions, Is.Zero);
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.RetryPreconditionFailed));
            Assert.That(result.FinalReason, Does.Contain("destination hash changed"));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(external));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TryReplace_StopsWhenPreparedReplacementChangesOrDisappears(bool deletePreparedFile)
        {
            byte[] original = { 80 };
            string destination = CreateDestination(original);
            string prepared = null;
            int calls = 0;
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (replacement, _) =>
                {
                    prepared = replacement;
                    calls++;
                    throw new Win32IOException(33);
                },
                DelayMilliseconds = _ =>
                {
                    if (deletePreparedFile) File.Delete(prepared);
                    else File.WriteAllBytes(prepared, new byte[] { 99 });
                }
            };

            bool succeeded = SurvivorsAtomicFile.TryReplace(
                CreateRequest(destination, original, new byte[] { 81 }, operations),
                out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.RetryPreconditionFailed));
            Assert.That(result.FinalReason, Does.Contain(deletePreparedFile ? "no longer exists" : "bytes changed"));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(original));
        }

        [Test]
        public void TryReplace_StopsWhenSessionRevisionPreconditionFails()
        {
            byte[] original = { 90 };
            string destination = CreateDestination(original);
            int calls = 0;
            int preconditions = 0;
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (_, __) =>
                {
                    calls++;
                    throw new Win32IOException(32);
                },
                DelayMilliseconds = _ => { }
            };

            bool succeeded = SurvivorsAtomicFile.TryReplace(
                CreateRequest(
                    destination,
                    original,
                    new byte[] { 91 },
                    operations,
                    () =>
                    {
                        preconditions++;
                        return SurvivorsAtomicRetryPreconditionResult.Failure("simulated source revision change");
                    }),
                out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(preconditions, Is.EqualTo(1));
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.RetryPreconditionFailed));
            Assert.That(result.FinalReason, Does.Contain("source revision change"));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(original));
        }

        [Test]
        public void TryReplace_StopsWithoutDelayWhenTransactionIsAborted()
        {
            byte[] original = { 100 };
            string destination = CreateDestination(original);
            int calls = 0;
            bool aborted = false;
            var delays = new List<int>();
            var operations = new SurvivorsAtomicFileOperations
            {
                ReplacementOperation = (_, __) =>
                {
                    calls++;
                    aborted = true;
                    throw new Win32IOException(32);
                },
                DelayMilliseconds = delays.Add
            };
            SurvivorsAtomicReplaceRequest request = CreateRequest(
                destination,
                original,
                new byte[] { 101 },
                operations,
                isAborted: () => aborted);

            bool succeeded = SurvivorsAtomicFile.TryReplace(request, out SurvivorsAtomicReplaceResult result);

            Assert.That(succeeded, Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(delays, Is.Empty);
            Assert.That(result.FinalDisposition, Is.EqualTo(SurvivorsAtomicReplaceFinalDisposition.Aborted));
            Assert.That(File.ReadAllBytes(destination), Is.EqualTo(original));
        }

        [Test]
        public void AtomicSupportProbe_TransientFailureUsesCooldownThenRecoversAndCachesSuccess()
        {
            DateTime now = new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc);
            bool fail = true;
            int calls = 0;
            var delays = new List<int>();
            var operations = new SurvivorsAtomicFileOperations
            {
                ProbeDirectory = Path.Combine(_directory, "probe"),
                UtcNow = () => now,
                DelayMilliseconds = delays.Add,
                ReplacementOperation = (replacement, destination) =>
                {
                    calls++;
                    if (fail) throw new Win32IOException(32);
                    File.Replace(replacement, destination, null);
                }
            };

            Assert.That(SurvivorsAtomicFile.TryConfirmSupport(operations, out string transientReason), Is.False);
            Assert.That(calls, Is.EqualTo(4));
            Assert.That(delays, Is.EqualTo(new[] { 25, 75, 200 }));
            Assert.That(transientReason, Does.Contain("RetryExhausted"));
            Assert.That(transientReason, Does.Contain("Win32=32"));

            fail = false;
            Assert.That(SurvivorsAtomicFile.TryConfirmSupport(operations, out string cooldownReason), Is.False);
            Assert.That(calls, Is.EqualTo(4));
            Assert.That(cooldownReason, Does.Contain("one-second transient cooldown"));

            now = now.AddMilliseconds(SurvivorsAtomicFile.ProbeTransientCooldownMilliseconds + 1);
            Assert.That(SurvivorsAtomicFile.TryConfirmSupport(operations, out string recoveredReason), Is.True, recoveredReason);
            Assert.That(calls, Is.EqualTo(5));
            Assert.That(SurvivorsAtomicFile.TryConfirmSupport(operations, out string cachedReason), Is.True, cachedReason);
            Assert.That(calls, Is.EqualTo(5));
        }

        [Test]
        public void AtomicSupportProbe_PermanentFailureIsCached()
        {
            int calls = 0;
            bool deny = true;
            var operations = new SurvivorsAtomicFileOperations
            {
                ProbeDirectory = Path.Combine(_directory, "probe"),
                DelayMilliseconds = _ => { },
                ReplacementOperation = (replacement, destination) =>
                {
                    calls++;
                    if (deny) throw new UnauthorizedAccessException("simulated permanent denial");
                    File.Replace(replacement, destination, null);
                }
            };

            Assert.That(SurvivorsAtomicFile.TryConfirmSupport(operations, out string firstReason), Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(firstReason, Does.Contain(typeof(UnauthorizedAccessException).FullName));
            Assert.That(firstReason, Does.Contain("NonRetryableFailure"));

            deny = false;
            Assert.That(SurvivorsAtomicFile.TryConfirmSupport(operations, out string cachedReason), Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(cachedReason, Is.EqualTo(firstReason));
        }

        private string CreateDestination(byte[] bytes)
        {
            string destination = Path.Combine(_directory, "destination.json");
            SurvivorsAtomicFile.WriteNew(destination, bytes);
            return destination;
        }

        private static SurvivorsAtomicReplaceRequest CreateRequest(
            string destination,
            byte[] original,
            byte[] proposed,
            SurvivorsAtomicFileOperations operations,
            Func<SurvivorsAtomicRetryPreconditionResult> retryPrecondition = null,
            Func<bool> isAborted = null)
        {
            return new SurvivorsAtomicReplaceRequest(
                destination,
                "AtomicTests/destination.json",
                proposed,
                SurvivorsContentEditHash.Sha256(original),
                "AtomicTest",
                retryPrecondition: retryPrecondition,
                isAborted: isAborted,
                operations: operations);
        }
    }
}
