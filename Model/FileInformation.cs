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
        /// File extenstion
        /// </summary>
        public string Extenstion { get; set; }

        /// <summary>
        /// File category
        /// </summary>
        public FileInfoType Type { get; set; }

        /// <summary>
        /// Size of file in bytes
        /// </summary>
        public double Size { get; set; }
    }

    /// <summary>
    /// File category
    /// </summary>
    public enum FileInfoType
    {
        /// <summary>
        /// Unkown
        /// </summary>
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
    }
}
