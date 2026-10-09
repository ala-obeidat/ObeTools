using System;
using System.Text.RegularExpressions;

namespace ObeTools.Model
{
    /// <summary>
    /// Log Information Model
    /// </summary>
    /// <remarks>
    /// The text produced by <see cref="ToString"/> is a public contract: log scanners parse it.
    /// Do not change the field order, the separators (" |  ") or the time format.
    /// New fields are only ever appended at the end, and only when opted in.
    /// </remarks>
    public class LogModel
    {
        private static readonly Regex ForgedEntry = new Regex(
            @"(\r\n|\r|\n)([ \t]*)StringTime:",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        /// <summary>
        /// Set Time of Exception
        /// </summary>
        public LogModel() : this(DateTime.Now)
        {
        }

        /// <summary>
        /// Set Time of entry from a given moment (the same moment is used to pick the daily file).
        /// </summary>
        internal LogModel(DateTime now)
        {
            StringTime = now.ToString("HH:mm:ss:ff", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Method (function) name.
        /// </summary>
        public string Method { get; set; }


        /// <summary>
        /// Information data.
        /// </summary>
        public string Data { get; set; }


        /// <summary>
        /// time in "HH:mm:ss:ff" to log in text file.
        /// </summary>
        public readonly string StringTime;

        /// <summary>
        /// Caption message for exception.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Line number where exception happend in file.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// File name where exception happend.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Full exception text including the stack trace. Only filled when <see cref="Logger.IncludeStackTrace"/> is on;
        /// when set it is written after LineNumber.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// One log entry: a leading line break, then
        /// <c>StringTime: HH:mm:ss:ff |  Caption: … |  Message: … |  Method: … |  FileName: … |  LineNumber: …</c>
        /// </summary>
        public override string ToString()
        {
            var text = $"\nStringTime: {StringTime} |  Caption: {Neutralize(Caption)} |  Message: {Neutralize(Data)} |  Method: {Neutralize(Method)} |  FileName: {Neutralize(FileName)} |  LineNumber: {LineNumber}";
            if (StackTrace != null)
            {
                text += $" |  StackTrace: {Neutralize(StackTrace)}";
            }
            return text;
        }

        /// <summary>
        /// Stops a value (often an exception message carrying user input) from starting a fake log entry.
        /// </summary>
        internal static string Neutralize(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf("StringTime:", StringComparison.Ordinal) < 0)
            {
                return value;
            }
            return ForgedEntry.Replace(value, "$1$2(StringTime):");
        }
    }
}
