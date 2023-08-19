using System;

namespace ObeTools.Model
{
    /// <summary>
    /// Log Information Model
    /// </summary>
    public class LogModel
    {
        /// <summary>
        /// Set Time of Exception
        /// </summary>
        public LogModel()
        {
            StringTime = DateTime.Now.ToString("HH:mm:ss:ff");
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
        /// time in "yyyy-MM-dd HH:mm:ss:ff" to log in text file.
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
        public override string ToString()
        {
            return $"\nStringTime: {StringTime} |  Caption: {Caption} |  Message: {Data} |  Method: {Method} |  FileName: {FileName} |  LineNumber: {LineNumber}";
        }
    }
}
