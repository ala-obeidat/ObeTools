using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        #region Method
        /// <summary>
        /// Log exception to text file (with date and time)
        /// </summary>
        /// <param name="logName">Log file name</param>
        /// <param name="logDirectory">Log file path</param>
        /// <param name="e">Exception to be logged</param>
        /// <param name="caption">Optional title for this exception</param>
        /// <returns>Exception message</returns>
        public static string LogException(string logName, string logDirectory, Exception e, string caption = "")
        {
            try
            {
                var text = GetExeptionLogLine(e, caption);
                WriteToFile(logName, logDirectory, "Exception", text);
                return text;
            }
            catch { return string.Empty; }
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
                return GetExeptionModel(e);
            }
            catch { return null; }
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
        public static void LogInfo<T>(string logName, string logDirectory, string caption, T data, string method)
        {
            try
            {
                var model = new LogModel()
                {

                    Caption = caption,
                    Method = method,
                };
                if (data != null)
                {
                    try
                    {
                        model.Data = JsonSerializer.Serialize(data, new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            WriteIndented = true,
                        });
                    }
                    catch { }
                }
                WriteToFile(logName, logDirectory, "Info", model.ToString());
            }
            catch { }
        }
        #endregion

        #region Helper
        private static void WriteToFile(string logName, string logDirectory, string logType, string text)
        {
            try
            {
                var filePath = GetLogFilePath(logName, logDirectory, logType);
                using StreamWriter writer = File.AppendText(filePath);
                writer.WriteLine(text);
            }
            catch { }

        }
        private static string GetLogFilePath(string logName, string logDirectory, string logType)
        {
            if (!Directory.Exists(logDirectory))
            {
                _ = Directory.CreateDirectory(logDirectory);
            }
            var logFilePath = $"{logName} {DateTime.Now:yyyy-MM-dd} - {logType}.txt";
            return Path.Combine(logDirectory, logFilePath);
        }
        private static string GetExeptionLogLine(Exception e, string caption)
        {
            var model = GetExeptionModel(e);
            model.Caption = caption;
            return model.ToString();
        }
        private static LogModel GetExeptionModel(Exception e)
        {
            var model = new LogModel();
            var message = new StringBuilder();
            Exception temp = e;
            var st = new StackTrace(temp, true);
            do
            {
                if (string.IsNullOrEmpty(temp.Source))
                {
                    _ = message.Append(temp.Message);
                }
                else
                {
                    _ = message.Append($"{temp.Message} ({temp.Source})");
                }

                if (temp.InnerException == null)
                {
                    // Create a StackDebug that captures
                    // filename, line number, and column
                    // information for the current thread.
                    GetMethodInfo(model, temp, st);

                    break;
                }
                temp = temp.InnerException;

            } while (temp != null);
            if (string.IsNullOrEmpty(model.Method))
            {
                GetMethodInfo(model, temp, st);
            }
            model.Data = message.ToString();
            return model;
        }
        private static void GetMethodInfo(LogModel model, Exception temp, StackTrace st)
        {
            if (st.FrameCount == 0)
            {
                model.FileName = "";
                model.Method = "";
                model.LineNumber = -1;
                return;
            }
            for (int i = 0; i < st.FrameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                if (sf != null && sf.GetFileName() != null)
                {
                    model.FileName = string.Join(" => ", sf.GetFileName().Split('\\').Skip(2));
                    System.Reflection.MethodBase method = sf.GetMethod();
                    if (method.Name == "MoveNext")
                    {

                        model.Method = method.ReflectedType.Name;
                        if (model.Method.Contains('>'))
                        {
                            model.Method = model.Method[1..model.Method.IndexOf('>')];
                        }
                    }
                    else
                    {
                        model.Method = method.Name;
                    }
                    model.LineNumber = sf.GetFileLineNumber();
                    break;
                }
            }
        }
        #endregion
    }
}
