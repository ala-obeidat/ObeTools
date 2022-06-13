using OfficeOpenXml.Style;

namespace ObeTools.Model
{
    /// <summary>
    /// Excel design.
    /// </summary>
    public class ExcelDesign
    {
        /// <summary>
        /// 
        /// </summary>
        public ExcelNumberFormat Numberformat { get; set; }

        /// <summary>
        /// Font style
        /// </summary>
        public ExcelFont Font { get; set; }


        /// <summary>
        /// Fill Styling
        /// </summary>
        public ExcelFill Fill { get; set; }

        /// <summary>
        /// Border style
        /// </summary>
        public Border Border { get; set; }


        /// <summary>
        /// The horizontal alignment in the cell
        /// </summary>
        public ExcelHorizontalAlignment HorizontalAlignment
        { get; set; }

        /// <summary>
        /// The vertical alignment in the cell
        /// </summary>
        public ExcelVerticalAlignment VerticalAlignment
        { get; set; }

        /// <summary>
        /// Wrap the text
        /// </summary>
        public bool WrapText
        { get; set; }

        /// <summary>
        /// Readingorder RTL or LTR
        /// </summary>
        public ExcelReadingOrder ReadingOrder
        { get; set; }

        /// <summary>
        /// Shrink the text to fit
        /// </summary>
        public bool ShrinkToFit
        { get; set; }

        /// <summary>
        /// The margin between the border and the text
        /// </summary>
        public int Indent
        { get; set; }

        /// <summary>
        /// Text orientation in degrees. Values range from 0 to 180 or 255. Setting the rotation
        /// to 255 will align text vertically.
        /// </summary>
        public int TextRotation
        { get; set; }
        /// <summary>
        /// If true the cell is locked for editing when the sheet is protected OfficeOpenXml.ExcelWorksheet.Protection
        /// </summary>
        public bool Locked
        { get; set; }

        /// <summary>
        /// If true the formula is hidden when the sheet is protected. OfficeOpenXml.ExcelWorksheet.Protection
        /// </summary>
        public bool Hidden
        { get; set; }

        /// <summary>
        /// If true the cell has a quote prefix, which indicates the value of the cell is
        /// text.
        /// </summary>
        public bool QuotePrefix
        { get; set; }
    }
}
