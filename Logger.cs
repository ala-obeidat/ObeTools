using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;

using ObeTools.Model;

namespace ObeTools
{
    /// <summary>
    /// Nice and simple thread safe text logger.
    /// </summary>
    /// <remarks>
    /// Every entry is written with a single write while holding a per-file lock, so threads in one process never
    /// lose lines. When another process (or a reader without write-share) holds the file, the write is retried
    /// with back-off for up to <see cref="WriteTimeoutMs"/>. Writers always open the file with <see cref="FileShare.Read"/>:
    /// two writers that both allow write-share can overwrite each other's bytes.
    /// </remarks>
    public static class Logger
    {
        #region Settings

        private static readonly ConcurrentDictionary<string, object> FileGates =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, Mutex> FileMutexes =
            new ConcurrentDictionary<string, Mutex>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, byte> KnownDirectories =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, DateTime> LastRetentionRun =
            new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        // Same bytes File.AppendText wrote in 1.6.x (UTF-8, no BOM).
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private static readonly JsonSerializerOptions DataJsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        [ThreadStatic]
        private static Random _jitter;

        /// <summary>
        /// Max time (milliseconds) one entry waits for another writer (other process or a reader holding the file). Default 2000.
        /// </summary>
        public static int WriteTimeoutMs { get; set; } = 2000;

        /// <summary>
        /// Also serialise writers of other processes with a named mutex per file.
        /// Default: false on Windows, where the exclusive file open plus retry already keeps processes apart
        /// (turn it on when many processes share one file, to avoid spinning); true elsewhere, because on Linux
        /// and macOS file sharing is advisory and two processes could otherwise overwrite each other's lines.
        /// If the mutex cannot be created or opened (for example an IIS app pool under another identity owns it)
        /// the logger silently falls back to retrying.
        /// </summary>
        public static bool UseCrossProcessMutex { get; set; } = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Append the full exception text (all inner exceptions and stack traces) as <c>|  StackTrace: …</c>
        /// after LineNumber. Default false, which keeps the 1.6 line format.
        /// </summary>
        public static bool IncludeStackTrace { get; set; }

        /// <summary>
        /// Include the PostgreSQL <c>Detail</c> field in exception logs. Default true.
        /// Note: with <c>Include Error Detail=true</c> in the connection string, Detail contains row values,
        /// which may be customer data.
        /// </summary>
        public static bool IncludeDatabaseDetail { get; set; } = true;

        /// <summary>
        /// When set, daily files <c>"{logName} yyyy-MM-dd - *.txt"</c> older than this many days are deleted
        /// (checked at most once per hour per log name and directory). Default null: never delete.
        /// </summary>
        public static int? RetentionDays { get; set; }

        /// <summary>
        /// Raised when an entry could not be written (file path, exception). Never throws into the caller.
        /// </summary>
        public static event Action<string, Exception> WriteFailed;

        #endregion

        #region Public Methods
        /// <summary>
        /// Log exception to text file (with date and time)
        /// </summary>
        /// <param name="logName">Log file name</param>
        /// <param name="logDirectory">Log file path</param>
        /// <param name="e">Exception to be logged</param>
        /// <param name="caption">Optional title for this exception</param>
        /// <returns>Exception log text (returned even when the file could not be written); empty when it could not be built</returns>
        public static string LogException(
            string logName,
            string logDirectory,
            Exception e,
            string caption = "")
        {
            TryLogException(logName, logDirectory, e, out var text, caption);
            return text;
        }

        /// <summary>
        /// Log exception to text file and report whether the entry reached the file.
        /// </summary>
        /// <param name="logName">Log file name</param>
        /// <param name="logDirectory">Log file path</param>
        /// <param name="e">Exception to be logged</param>
        /// <param name="text">Exception log text; empty when it could not be built</param>
        /// <param name="caption">Optional title for this exception</param>
        /// <returns>true when the entry was written</returns>
        public static bool TryLogException(
            string logName,
            string logDirectory,
            Exception e,
            out string text,
            string caption = "")
        {
            text = string.Empty;
            try
            {
                var now = DateTime.Now;
                var model = GetExceptionModel(e, now);
                model.Caption = caption;
                text = model.ToString();
                return WriteToFile(logName, logDirectory, "Exception", text, now);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get Exception Information
        /// </summary>
        /// <param name="e">Input Exception</param>
        /// <returns>Log Model Data</returns>
        public static LogModel GetExceptionInfo(Exception e)
        {
            try
            {
                return GetExceptionModel(e, DateTime.Now);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Log information to text file (with date and time)
        /// </summary>
        /// <typeparam name="T">Type of object data to be logged</typeparam>
        /// <param name="logName">Log file name</param>
        /// <param name="logDirectory">Log file path</param>
        /// <param name="caption">title for this information</param>
        /// <param name="data">object data to be serialized</param>
        /// <param name="method">Method name</param>
        public static void LogInfo<T>(
            string logName,
            string logDirectory,
            string caption,
            T data,
            string method)
        {
            TryLogInfo(logName, logDirectory, caption, data, method);
        }

        /// <summary>
        /// Log information to text file and report whether the entry reached the file.
        /// </summary>
        /// <typeparam name="T">Type of object data to be logged</typeparam>
        /// <param name="logName">Log file name</param>
        /// <param name="logDirectory">Log file path</param>
        /// <param name="caption">title for this information</param>
        /// <param name="data">object data to be serialized</param>
        /// <param name="method">Method name</param>
        /// <returns>true when the entry was written</returns>
        public static bool TryLogInfo<T>(
            string logName,
            string logDirectory,
            string caption,
            T data,
            string method)
        {
            try
            {
                var now = DateTime.Now;
                var model = new LogModel(now)
                {
                    Caption = caption,
                    Method = method
                };

                if (data != null)
                {
                    try
                    {
                        model.Data = JsonSerializer.Serialize(data, DataJsonOptions);
                    }
                    catch { }
                }

                return WriteToFile(logName, logDirectory, "Info", model.ToString(), now);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Core Logic

        private static LogModel GetExceptionModel(Exception e, DateTime now)
        {
            var model = new LogModel(now);
            var message = new StringBuilder();
            var stackTrace = new StackTrace(e, true);

            Exception temp = e;
            while (temp != null)
            {
                if (string.IsNullOrEmpty(temp.Source))
                    message.AppendLine(temp.Message);
                else
                    message.AppendLine($"{temp.Message} ({temp.Source})");

                temp = temp.InnerException;
            }

            // Append DB info only if provider exists
            AppendDatabaseDetailsIfAny(message, e);

            model.Data = message.ToString();

            GetMethodInfo(model, stackTrace);

            if (IncludeStackTrace)
            {
                model.StackTrace = e.ToString();
            }

            return model;
        }

        #endregion

        #region Database Detection (Reflection-based)

        private static void AppendDatabaseDetailsIfAny(
            StringBuilder sb,
            Exception ex)
        {
            foreach (var temp in Flatten(ex))
            {
                var typeName = temp.GetType().FullName ?? string.Empty;

                // PostgreSQL (Npgsql.PostgresException)
                if (typeName == "Npgsql.PostgresException")
                {
                    var props = new List<(string Property, string Label)>
                    {
                        ("SqlState", "SQL State"),
                        ("MessageText", "Message")
                    };
                    if (IncludeDatabaseDetail)
                    {
                        props.Add(("Detail", "Detail"));
                    }
                    props.Add(("TableName", "Table"));
                    props.Add(("ColumnName", "Column"));
                    props.Add(("ConstraintName", "Constraint"));

                    AppendProperties(sb, temp, "PostgreSQL", props);
                    return;
                }

                // SQL Server (System.Data.SqlClient.SqlException or Microsoft.Data.SqlClient.SqlException)
                if (typeName == "System.Data.SqlClient.SqlException" ||
                    typeName == "Microsoft.Data.SqlClient.SqlException")
                {
                    AppendProperties(sb, temp,
                        "SQL Server",
                        new List<(string Property, string Label)>
                        {
                            ("Number", "Number"),
                            ("State", "State"),
                            ("Procedure", "Procedure"),
                            ("LineNumber", "Line"),
                            ("Server", "Server")
                        });
                    return;
                }
            }
        }

        /// <summary>
        /// The exception, its InnerException chain and every AggregateException branch, depth first.
        /// </summary>
        private static IEnumerable<Exception> Flatten(Exception root)
        {
            var stack = new Stack<Exception>();
            var seen = new HashSet<Exception>();
            if (root != null)
            {
                stack.Push(root);
            }

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!seen.Add(current))
                {
                    continue;
                }
                yield return current;

                if (current is AggregateException aggregate)
                {
                    for (int i = aggregate.InnerExceptions.Count - 1; i >= 0; i--)
                    {
                        stack.Push(aggregate.InnerExceptions[i]);
                    }
                }
                else if (current.InnerException != null)
                {
                    stack.Push(current.InnerException);
                }
            }
        }

        private static void AppendProperties(
            StringBuilder sb,
            object exception,
            string providerName,
            List<(string Property, string Label)> props)
        {
            sb.AppendLine();
            sb.AppendLine($"---- {providerName} Details ----");

            var type = exception.GetType();

            foreach (var (property, label) in props)
            {
                try
                {
                    PropertyInfo pi = type.GetProperty(property);
                    var value = pi?.GetValue(exception);
                    if (value != null)
                        sb.AppendLine($"{label,-11}: {value}");
                }
                catch { }
            }
        }

        #endregion

        #region File & Stack Helpers

        private static bool WriteToFile(
            string logName,
            string logDirectory,
            string logType,
            string text,
            DateTime now)
        {
            string path = null;
            try
            {
                path = GetLogFilePath(logName, logDirectory, logType, now);
                byte[] bytes = Utf8NoBom.GetBytes(text + Environment.NewLine); // ONE write per entry
                object gate = FileGates.GetOrAdd(path, _ => new object());

                lock (gate) // threads in this process queue here
                {
                    Mutex mutex = null;
                    bool ownsMutex = false;
                    try
                    {
                        if (UseCrossProcessMutex)
                        {
                            ownsMutex = TryAcquireMutex(path, out mutex);
                        }
                        AppendWithRetry(path, logDirectory, bytes);
                    }
                    finally
                    {
                        if (ownsMutex)
                        {
                            try { mutex.ReleaseMutex(); } catch { }
                        }
                    }
                }

                ApplyRetention(logName, logDirectory, now);
                return true;
            }
            catch (Exception e)
            {
                try { WriteFailed?.Invoke(path, e); } catch { }
                try { Trace.WriteLine($"ObeTools.Logger write failed: {path} ({e.GetType().Name})"); } catch { }
                return false;
            }
        }

        private static void AppendWithRetry(string path, string logDirectory, byte[] bytes)
        {
            int start = Environment.TickCount;
            int delay = 1;
            bool recreatedDirectory = false;
            while (true)
            {
                try
                {
                    using var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                    fs.Write(bytes, 0, bytes.Length);
                    return;
                }
                catch (DirectoryNotFoundException) when (!recreatedDirectory)
                {
                    // The directory was removed after we cached it.
                    recreatedDirectory = true;
                    KnownDirectories.TryRemove(logDirectory, out _);
                    EnsureDirectory(logDirectory);
                }
                catch (IOException e) when (IsSharingViolation(e) && unchecked(Environment.TickCount - start) < WriteTimeoutMs)
                {
                    // Another PROCESS (or a reader without write-share) holds the file.
                    var jitter = _jitter ??= new Random(Guid.NewGuid().GetHashCode());
                    Thread.Sleep(delay + jitter.Next(0, 3));
                    delay = Math.Min(delay * 2, 50);
                }
            }
        }

        private static bool TryAcquireMutex(string path, out Mutex mutex)
        {
            mutex = null;
            try
            {
                mutex = FileMutexes.GetOrAdd(path, p => new Mutex(false, GetMutexName(p)));
                try
                {
                    return mutex.WaitOne(WriteTimeoutMs);
                }
                catch (AbandonedMutexException)
                {
                    return true; // the previous owner died; we own it now
                }
            }
            catch
            {
                return false; // no access to the mutex: rely on retry
            }
        }

        private static string GetMutexName(string path)
        {
            using var sha = SHA256.Create();
            var full = Path.GetFullPath(path).ToUpperInvariant();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(full));
            return @"Global\ObeLog-" + BitConverter.ToString(hash).Replace("-", "");
        }

        private static bool IsSharingViolation(IOException e)
        {
            if (e is FileNotFoundException || e is DirectoryNotFoundException || e is PathTooLongException)
            {
                return false;
            }
            int code = e.HResult & 0xFFFF;
            // ERROR_SHARING_VIOLATION, ERROR_LOCK_VIOLATION on Windows. On Unix .NET reports a held lock
            // with the raw errno (EWOULDBLOCK/EAGAIN = 11).
            return code == 32 || code == 33 || code == 11;
        }

        private static void EnsureDirectory(string logDirectory)
        {
            if (KnownDirectories.ContainsKey(logDirectory))
            {
                return;
            }
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            KnownDirectories.TryAdd(logDirectory, 0);
        }

        private static string GetLogFilePath(
            string logName,
            string logDirectory,
            string logType,
            DateTime now)
        {
            EnsureDirectory(logDirectory);

            var logFilePath =
                $"{logName} {now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {logType}.txt";

            return Path.Combine(logDirectory, logFilePath);
        }

        private static void ApplyRetention(string logName, string logDirectory, DateTime now)
        {
            var days = RetentionDays;
            if (days == null || days.Value < 1)
            {
                return;
            }

            var key = logDirectory + "|" + logName;
            var last = LastRetentionRun.GetOrAdd(key, DateTime.MinValue);
            if (now - last < TimeSpan.FromHours(1) || !LastRetentionRun.TryUpdate(key, now, last))
            {
                return; // ran recently, or another thread is running it now
            }

            try
            {
                var cutoff = now.Date.AddDays(-days.Value);
                var prefix = logName + " ";
                foreach (var file in Directory.EnumerateFiles(logDirectory, prefix + "*.txt"))
                {
                    var name = Path.GetFileName(file);
                    if (name.Length < prefix.Length + 10)
                    {
                        continue;
                    }
                    var datePart = name.Substring(prefix.Length, 10);
                    if (name.IndexOf(" - ", prefix.Length + 10, StringComparison.Ordinal) != prefix.Length + 10)
                    {
                        continue;
                    }
                    if (DateTime.TryParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                        && date < cutoff)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch { }
        }

        private static void GetMethodInfo(
            LogModel model,
            StackTrace stackTrace)
        {
            if (stackTrace == null || stackTrace.FrameCount == 0)
            {
                model.FileName = string.Empty;
                model.Method = string.Empty;
                model.LineNumber = -1;
                return;
            }

            foreach (var frame in stackTrace.GetFrames())
            {
                if (frame?.GetFileName() == null)
                    continue;

                model.FileName = FormatFileName(frame.GetFileName());

                var method = frame.GetMethod();
                if (method != null)
                {
                    if (method.Name == "MoveNext" &&
                        method.ReflectedType != null)
                    {
                        model.Method = method.ReflectedType.Name;
                        if (model.Method.Contains('>'))
                        {
                            model.Method =
                                model.Method[1..model.Method.IndexOf('>')];
                        }
                    }
                    else
                    {
                        model.Method = method.Name;
                    }
                }

                model.LineNumber = frame.GetFileLineNumber();
                break;
            }
        }

        /// <summary>
        /// "D:\Code\Back\Api\File.cs" => "Back => Api => File.cs" (the 1.6 format: drive and first folder dropped).
        /// Unix paths and deterministic build paths ("/_/src/File.cs") are handled the same way instead of coming out empty.
        /// </summary>
        internal static string FormatFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            string[] parts;
            if (fileName.Contains('\\'))
            {
                parts = fileName.Split('\\');
            }
            else
            {
                parts = fileName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1 && parts[0] == "_")
                {
                    // Deterministic build: "/_/" is the repository root.
                    return string.Join(" => ", parts.Skip(1));
                }
            }

            return string.Join(" => ", parts.Skip(Math.Min(2, Math.Max(parts.Length - 1, 0))));
        }

        #endregion
    }
}
