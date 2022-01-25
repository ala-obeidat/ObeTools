using ObeTools.Model;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ObeTools
{
    public static class Logger
    {
        #region Method
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
        public static void LogInfo<T>(string logName, string logDirectory, string caption, T data, string method)
        {
            try
            {
                var model = new LogModel()
                {
                    Data = JsonSerializer.Serialize(data),
                    Caption = caption,
                    Method = method,
                };
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
                Directory.CreateDirectory(logDirectory);
            }
            var logFilePath = $"{logName} {DateTime.Now:yyyy-MM-dd} - {logType}.txt";
            return Path.Combine(logDirectory, logFilePath);
        }
        private static string GetExeptionLogLine(Exception e, string caption)
        {
            var model = new LogModel();
            var message = new StringBuilder();
            Exception temp = e;
            var st = new StackTrace(temp, true);
            do
            {
                if (string.IsNullOrEmpty(temp.Source))
                {
                    message.Append(temp.Message);
                }
                else
                {
                    message.Append($"{temp.Message} ({temp.Source})");
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
            model.Caption = caption;
            return model.ToString();
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
