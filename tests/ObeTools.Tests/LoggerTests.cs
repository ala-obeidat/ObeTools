using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using ObeTools.Model;

using Xunit;

namespace ObeTools.Tests
{
    /// <summary>
    /// Logger settings are static, so every test that touches them runs in this non-parallel collection.
    /// </summary>
    [CollectionDefinition("Logger", DisableParallelization = true)]
    public class LoggerCollection
    {
    }

    [Collection("Logger")]
    public sealed class LoggerTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "obetools-tests", Guid.NewGuid().ToString("N"));

        public LoggerTests()
        {
            Logger.IncludeStackTrace = false;
            Logger.IncludeDatabaseDetail = true;
            Logger.RetentionDays = null;
            Logger.WriteTimeoutMs = 2000;
        }

        public void Dispose()
        {
            Logger.IncludeStackTrace = false;
            Logger.IncludeDatabaseDetail = true;
            Logger.RetentionDays = null;
            try { Directory.Delete(_directory, true); } catch { }
        }

        private string InfoFile(string logName) =>
            Path.Combine(_directory, $"{logName} {DateTime.Now:yyyy-MM-dd} - Info.txt");

        private static int CountEntries(string path) =>
            File.ReadLines(path).Count(l => l.StartsWith("StringTime: ", StringComparison.Ordinal));

        [Fact]
        public void ConcurrentWritersInOneProcess_KeepEveryLine()
        {
            const int threads = 8, perThread = 2000;

            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, t =>
            {
                for (int i = 0; i < perThread; i++)
                {
                    Logger.LogInfo("concurrent", _directory, $"t{t}-{i}", (object)null, "Test");
                }
            });

            Assert.Equal(threads * perThread, CountEntries(InfoFile("concurrent")));
        }

        [Fact]
        public void TryLogInfo_ReportsSuccess()
        {
            Assert.True(Logger.TryLogInfo("status", _directory, "c", new { A = 1 }, "M"));
            Assert.Equal(1, CountEntries(InfoFile("status")));
        }

        [Fact]
        public void TryLogInfo_ReportsFailure_AndRaisesWriteFailed()
        {
            // A file where the directory should be: the directory can never be created.
            Directory.CreateDirectory(_directory);
            var blocker = Path.Combine(_directory, "not-a-directory");
            File.WriteAllText(blocker, "x");

            string failedPath = null;
            Exception failure = null;
            void Handler(string path, Exception e) { failedPath = path; failure = e; }
            Logger.WriteFailed += Handler;
            try
            {
                Assert.False(Logger.TryLogInfo("fail", blocker, "c", (object)null, "M"));
                Assert.NotNull(failure);
            }
            finally
            {
                Logger.WriteFailed -= Handler;
            }
        }

        [Fact]
        public void LogException_ReturnsText_AndWritesIt()
        {
            var text = Logger.LogException("exc", _directory, Throw(() => throw new InvalidOperationException("boom")), "caption");

            Assert.Contains("Caption: caption", text);
            Assert.Contains("Message: boom", text);
            var file = Path.Combine(_directory, $"exc {DateTime.Now:yyyy-MM-dd} - Exception.txt");
            Assert.Contains("Message: boom", File.ReadAllText(file));
        }

        [Fact]
        public void LogException_ReturnsText_EvenWhenWriteFails()
        {
            Directory.CreateDirectory(_directory);
            var blocker = Path.Combine(_directory, "not-a-directory");
            File.WriteAllText(blocker, "x");

            var text = Logger.LogException("exc", blocker, Throw(() => throw new InvalidOperationException("boom")));

            Assert.Contains("Message: boom", text);
        }

        [Fact]
        public async Task ReaderHoldingTheFile_WriteWaitsAndSucceeds()
        {
            Assert.True(Logger.TryLogInfo("held", _directory, "first", (object)null, "M"));
            var path = InfoFile("held");

            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var write = Task.Run(() => Logger.TryLogInfo("held", _directory, "second", (object)null, "M"));
                await Task.Delay(300);
                Assert.False(write.IsCompleted); // waiting for the reader, not dropped
                reader.Dispose();
                Assert.True(await write.WaitAsync(TimeSpan.FromSeconds(5)));
            }

            Assert.Equal(2, CountEntries(path));
        }

        [Fact]
        public void CaptionWithFakeEntry_IsNeutralised()
        {
            Logger.LogInfo("forge", _directory, "real\nStringTime: 00:00:00:00 |  Caption: fake", (object)null, "M");

            var lines = File.ReadAllLines(InfoFile("forge"));
            Assert.Equal(1, lines.Count(l => l.StartsWith("StringTime:", StringComparison.Ordinal)));
            Assert.Contains(lines, l => l.StartsWith("(StringTime): 00:00:00:00", StringComparison.Ordinal));
        }

        [Fact]
        public void ExceptionMessageWithFakeEntry_IsNeutralised()
        {
            var text = Logger.LogException("forge", _directory,
                Throw(() => throw new Exception("user input\r\n  StringTime: 01:02:03:04 |  Caption: injected")));

            Assert.DoesNotMatch(new Regex(@"[\r\n][ \t]*StringTime:"), text.Substring(1));
        }

        [Fact]
        public void IncludeStackTrace_AppendsAfterLineNumber()
        {
            Logger.IncludeStackTrace = true;
            var text = Logger.LogException("stack", _directory, Throw(() => throw new InvalidOperationException("outer", new ArgumentException("inner"))));

            var lineNumber = text.IndexOf("|  LineNumber: ", StringComparison.Ordinal);
            var stack = text.IndexOf("|  StackTrace: ", StringComparison.Ordinal);
            Assert.True(lineNumber > 0 && stack > lineNumber);
            Assert.Contains("ArgumentException: inner", text);
            Assert.Contains(nameof(IncludeStackTrace_AppendsAfterLineNumber), text);
        }

        [Fact]
        public void StackTraceIsOffByDefault()
        {
            var text = Logger.LogException("stack", _directory, Throw(() => throw new InvalidOperationException("x")));
            Assert.DoesNotContain("StackTrace:", text);
        }

        [Fact]
        public void ExceptionLocation_UsesTheFirstFrameWithAFile()
        {
            var model = Logger.GetExceptionInfo(Throw(() => throw new InvalidOperationException("x")));

            Assert.EndsWith("LoggerTests.cs", model.FileName);
            Assert.True(model.LineNumber > 0);
        }

        [Theory]
        [InlineData(@"D:\Code\Back\Api\Controllers\File.cs", "Back => Api => Controllers => File.cs")] // 1.6 format
        [InlineData(@"D:\File.cs", "File.cs")]
        [InlineData("/home/user/src/Api/File.cs", "src => Api => File.cs")]
        [InlineData("/_/src/Api/File.cs", "src => Api => File.cs")]
        [InlineData("File.cs", "File.cs")]
        [InlineData("", "")]
        public void FormatFileName(string path, string expected)
        {
            Assert.Equal(expected, Logger.FormatFileName(path));
        }

        [Fact]
        public void AggregateException_DatabaseDetailsAreFoundInAnyBranch()
        {
            var aggregate = new AggregateException(new InvalidOperationException("first"), new Npgsql.PostgresException());

            var text = Logger.GetExceptionInfo(aggregate).Data;

            Assert.Contains("---- PostgreSQL Details ----", text);
            Assert.Contains("SQL State  : 23505", text);
            Assert.Contains("Detail     : Key (id)=(42) already exists.", text);
        }

        [Fact]
        public void IncludeDatabaseDetail_False_OmitsDetail()
        {
            Logger.IncludeDatabaseDetail = false;

            var text = Logger.GetExceptionInfo(new Npgsql.PostgresException()).Data;

            Assert.Contains("SQL State  : 23505", text);
            Assert.DoesNotContain("Detail     :", text);
            Assert.DoesNotContain("already exists", text);
        }

        [Fact]
        public void Retention_DeletesOnlyOldFilesOfThisLog()
        {
            Directory.CreateDirectory(_directory);
            var old = Path.Combine(_directory, $"ret {DateTime.Now.AddDays(-40):yyyy-MM-dd} - Info.txt");
            var recent = Path.Combine(_directory, $"ret {DateTime.Now.AddDays(-2):yyyy-MM-dd} - Exception.txt");
            var otherLog = Path.Combine(_directory, $"other {DateTime.Now.AddDays(-40):yyyy-MM-dd} - Info.txt");
            var unrelated = Path.Combine(_directory, "ret notes.txt");
            foreach (var f in new[] { old, recent, otherLog, unrelated })
            {
                File.WriteAllText(f, "x");
            }

            Logger.RetentionDays = 30;
            Logger.LogInfo("ret", _directory, "c", (object)null, "M");

            Assert.False(File.Exists(old));
            Assert.True(File.Exists(recent));
            Assert.True(File.Exists(otherLog));
            Assert.True(File.Exists(unrelated));
        }

        [Fact]
        public void DeletedDirectory_IsRecreated()
        {
            Logger.LogInfo("gone", _directory, "1", (object)null, "M");
            Directory.Delete(_directory, true);

            Assert.True(Logger.TryLogInfo("gone", _directory, "2", (object)null, "M"));
        }

        [Fact]
        public void TwoProcesses_KeepEveryLine()
        {
            const int perProcess = 1500;
            var writer = Path.Combine(AppContext.BaseDirectory, "ObeTools.LogWriter.dll");
            var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
            var start = DateTime.UtcNow.AddSeconds(2).Ticks.ToString();
            var useMutex = Logger.UseCrossProcessMutex.ToString(); // the platform default

            var processes = new[] { "A", "B" }.Select(marker => Process.Start(new ProcessStartInfo
            {
                FileName = dotnet,
                ArgumentList = { writer, _directory, "multi", marker, perProcess.ToString(), useMutex, start },
                RedirectStandardOutput = true,
                UseShellExecute = false
            })).ToList();

            foreach (var p in processes)
            {
                Assert.True(p.WaitForExit(120_000), "writer process timed out");
                Assert.True(p.ExitCode == 0, p.StandardOutput.ReadToEnd());
            }

            var lines = File.ReadAllLines(InfoFile("multi"));
            var captions = new HashSet<string>(lines
                .Where(l => l.StartsWith("StringTime: ", StringComparison.Ordinal))
                .Select(l => Regex.Match(l, @"Caption: (\S+) \|").Groups[1].Value));

            Assert.Equal(2 * perProcess, lines.Count(l => l.StartsWith("StringTime: ", StringComparison.Ordinal)));
            for (int i = 0; i < perProcess; i++)
            {
                Assert.Contains($"A-{i}", captions);
                Assert.Contains($"B-{i}", captions);
            }
        }

        internal static Exception Throw(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                return e;
            }
            throw new InvalidOperationException("Expected an exception.");
        }
    }
}

namespace Npgsql
{
    // Stand-in with the same full type name the Logger looks for.
    public class PostgresException : Exception
    {
        public PostgresException() : base("duplicate key value violates unique constraint") { }
        public string SqlState => "23505";
        public string MessageText => "duplicate key value";
        public string Detail => "Key (id)=(42) already exists.";
        public string TableName => "orders";
        public string ColumnName => null;
        public string ConstraintName => "orders_pkey";
    }
}
