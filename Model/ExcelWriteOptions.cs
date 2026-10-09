using System.Collections.Generic;

namespace ObeTools.Model
{
    /// <summary>
    /// Options for <see cref="ExcelHelper.WriteExcel(System.IO.Stream, List{ExcelSheetModel}, ExcelWriteOptions)"/>.
    /// </summary>
    public class ExcelWriteOptions
    {
        /// <summary>
        /// Header row design; null uses the default (bold white text on dark blue, thick borders).
        /// </summary>
        public ExcelDesign TitleDesign { get; set; }

        /// <summary>
        /// Data rows design; null leaves the data unstyled.
        /// </summary>
        public ExcelDesign DataDesign { get; set; }

        /// <summary>
        /// Right to left layout.
        /// </summary>
        public bool RightToLeft { get; set; }

        /// <summary>
        /// Document properties (author, company …). Null writes only the title (the sheet names).
        /// </summary>
        public ExcelDocumentProperties Properties { get; set; }

        /// <summary>
        /// Number format for <see cref="System.DateTime"/> values without a time part. Default "yyyy-mm-dd".
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-mm-dd";

        /// <summary>
        /// Number format for <see cref="System.DateTime"/> values with a time part. Default "yyyy-mm-dd hh:mm:ss".
        /// </summary>
        public string DateTimeFormat { get; set; } = "yyyy-mm-dd hh:mm:ss";
    }

    /// <summary>
    /// Excel document properties. Every value is optional.
    /// </summary>
    public class ExcelDocumentProperties
    {
        /// <summary>
        /// Title; when null the sheet names are used.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Company
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Application name
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// Custom properties
        /// </summary>
        public Dictionary<string, string> Custom { get; set; }
    }
}
