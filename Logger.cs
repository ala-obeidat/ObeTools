using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using ObeTools.Model;

namespace ObeTools
{
    /// <summary>
    /// Nice and simple thread safe text logger
    /// </summary>
    public static class Logger
    {
        #region Public Methods
        /// <summary>
        /// Log exception to text file (with date and time)
        /// </summary>
        /// <param name="logName">Log file name</param>
        /// <param name="logDirectory">Log file path</param>
        /// <param name="e">Exception to be logged</param>
        /// <param name="caption">Optional title for this exception</param>
        /// <returns>Exception message</returns>
        public static string LogException(
            string logName,
            string logDirectory,
            Exception e,
            string caption = "")
        {
            try
            {
                var text = GetExceptionLogLine(e, caption);
                WriteToFile(logName, logDirectory, "Exception", text);
                return text;
            }
            catch
            {
                return string.Empty;
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
                return GetExceptionModel(e);
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
        /// <param name="data">object data to be serilized</param>
        /// <param name="method">Method name</param>
        public static void LogInfo<T>(
            string logName,
            string logDirectory,
            string caption,
            T data,
            string method)
        {
            try
            {
                var model = new LogModel
                {
                    Caption = caption,
                    Method = method
                };

                if (data != null)
                {
                    try
                    {
                        model.Data = JsonSerializer.Serialize(
                            data,
                            new JsonSerializerOptions
                            {
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                WriteIndented = true
                            });
                    }
                    catch { }
                }

                WriteToFile(logName, logDirectory, "Info", model.ToString());
            }
            catch { }
        }

        #endregion

        #region Core Logic

        private static string GetExceptionLogLine(Exception e, string caption)
        {
            var model = GetExceptionModel(e);
            model.Caption = caption;
            return model.ToString();
        }

        private static LogModel GetExceptionModel(Exception e)
        {
            var model = new LogModel();
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

            // 🔹 Append DB info only if provider exists
            AppendDatabaseDetailsIfAny(message, e);

            model.Data = message.ToString();

            GetMethodInfo(model, stackTrace);

            return model;
        }

        #endregion

        #region Database Detection (Reflection-based)

        private static void AppendDatabaseDetailsIfAny(
            StringBuilder sb,
            Exception ex)
        {
            Exception temp = ex;

            while (temp != null)
            {
                var type = temp.GetType();
                var typeName = type.FullName ?? string.Empty;

                // PostgreSQL (Npgsql.PostgresException)
                if (typeName == "Npgsql.PostgresException")
                {
                    AppendProperties(sb, temp,
                        "PostgreSQL",
                        ("SqlState", "SQL State"),
                        ("MessageText", "Message"),
                        ("Detail", "Detail"),
                        ("TableName", "Table"),
                        ("ColumnName", "Column"),
                        ("ConstraintName", "Constraint"));
                    return;
                }

                // SQL Server (System.Data.SqlClient.SqlException or Microsoft.Data.SqlClient.SqlException)
                if (typeName == "System.Data.SqlClient.SqlException" ||
                    typeName == "Microsoft.Data.SqlClient.SqlException")
                {
                    AppendProperties(sb, temp,
                        "SQL Server",
                        ("Number", "Number"),
                        ("State", "State"),
                        ("Procedure", "Procedure"),
                        ("LineNumber", "Line"),
                        ("Server", "Server"));
                    return;
                }

                temp = temp.InnerException;
            }
        }

        private static void AppendProperties(
            StringBuilder sb,
            object exception,
            string providerName,
            params (string Property, string Label)[] props)
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

        private static void WriteToFile(
            string logName,
            string logDirectory,
            string logType,
            string text)
        {
            try
            {
                var filePath = GetLogFilePath(logName, logDirectory, logType);
                using StreamWriter writer = File.AppendText(filePath);
                writer.WriteLine(text);
            }
            catch { }
        }

        private static string GetLogFilePath(
            string logName,
            string logDirectory,
            string logType)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var logFilePath =
                $"{logName} {DateTime.Now:yyyy-MM-dd} - {logType}.txt";

            return Path.Combine(logDirectory, logFilePath);
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

                model.FileName = string.Join(
                    " => ",
                    frame.GetFileName().Split('\\').Skip(2));

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

        #endregion
    }
}
