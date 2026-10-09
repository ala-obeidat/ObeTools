using System;

namespace ObeTools.Model
{
    /// <summary>
    /// File information
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// File array of bytes
        /// </summary>
        public byte[] FileBytes { get; set; }

        /// <summary>
        /// File extension without the dot (for example "png"), or empty when unknown.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// File extenstion. Misspelled alias of <see cref="Extension"/>.
        /// </summary>
        [Obsolete("Use Extension.")]
        public string Extenstion
        {
            get => Extension;
            set => Extension = value;
        }

        /// <summary>
        /// MIME type (for example "image/png"), or "application/octet-stream" when unknown.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// File category
        /// </summary>
        public FileInfoType Type { get; set; }

        /// <summary>
        /// Size of file in bytes (same as <see cref="SizeInBytes"/>; kept as double for compatibility).
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// Size of file in bytes.
        /// </summary>
        public long SizeInBytes { get; set; }
    }

    /// <summary>
    /// File category
    /// </summary>
    public enum FileInfoType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Misspelled alias of <see cref="Unknown"/>.
        /// </summary>
        [Obsolete("Use Unknown.")]
        Unkown = 0,

        /// <summary>
        /// Image file
        /// </summary>
        Image = 1,

        /// <summary>
        /// PDF file
        /// </summary>
        Pdf = 2,

        /// <summary>
        /// Word file
        /// </summary>
        Word = 3,

        /// <summary>
        /// Audio file
        /// </summary>
        Audio = 4,

        /// <summary>
        /// Video file
        /// </summary>
        Video = 5,

        /// <summary>
        /// Spreadsheet (xlsx)
        /// </summary>
        Spreadsheet = 6,

        /// <summary>
        /// Presentation (pptx)
        /// </summary>
        Presentation = 7,

        /// <summary>
        /// Archive (zip, apk and other containers)
        /// </summary>
        Archive = 8,
    }
}
