using System.Collections.Generic;

namespace ObeTools.Model
{
    /// <summary>
    /// Excel Sheet Model.
    /// </summary>
    public class ExcelSheetModel
    {
        /// <summary>
        /// Excel sheet name
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// Rows Data for each sheet, written as text. Ignored when <see cref="Rows"/> is set.
        /// </summary>
        public List<string[]> Data { get; set; }

        /// <summary>
        /// Typed rows: numbers, dates and booleans keep their type in Excel (no "number stored as text").
        /// Takes precedence over <see cref="Data"/>.
        /// </summary>
        public List<object[]> Rows { get; set; }

        /// <summary>
        /// Title first row
        /// </summary>
        public List<string> Headers { get; set; }
    }
}
