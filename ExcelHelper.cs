using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using ObeTools.Model;

using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ObeTools
{
    /// <summary>
    /// Write/read data from/to excel file
    /// </summary>
    /// <remarks>
    /// Licensing: EPPlus 5 and later is free for non-commercial use only. The HOST application should declare its
    /// licence (<c>ExcelPackage.LicenseContext = LicenseContext.Commercial</c>, or the EPPlusLicenseContext
    /// environment variable / appsettings key) before calling these methods. ObeTools only falls back to
    /// NonCommercial when nothing was declared and <see cref="UseNonCommercialLicenseWhenUnset"/> is true;
    /// that fallback will be removed in 2.0.
    /// </remarks>
    public static class ExcelHelper
    {
        /// <summary>
        /// When the host declared no EPPlus licence, use NonCommercial (the 1.6 behaviour). Default true.
        /// Set to false to make EPPlus raise its licence error instead. A licence the host set is never overwritten.
        /// </summary>
        public static bool UseNonCommercialLicenseWhenUnset { get; set; } = true;

        #region Methods

        /// <summary>
        /// Create modern excel file (xlsx). Values are written as text; use
        /// <see cref="WriteExcel(Stream, string, List{string}, List{object[]}, ExcelWriteOptions)"/> to keep numbers and dates typed.
        /// </summary>
        /// <param name="excelFileStream">Output stream to write excel file on it (left open)</param>
        /// <param name="sheetName">Excel sheet name</param>
        /// <param name="headers">Title first row</param>
        /// <param name="data">Rows Data</param>
        /// <param name="titleDesign">[Optinal parameter] to design title(header)
        /// with default value:
        ///  Font.Bold = true;
        ///  Font.Color.SetColor(Color.White);
        ///  Border.Right.Style = ExcelBorderStyle.Thick;
        ///  Border.Top.Style = ExcelBorderStyle.Thick;
        ///  Border.Left.Style = ExcelBorderStyle.Thick;
        ///  Border.Bottom.Style = ExcelBorderStyle.Thick;
        ///  Fill.PatternType = ExcelFillStyle.Solid;
        ///  Fill.BackgroundColor.SetColor(Color.DarkBlue);
        /// </param>
        /// <param name="dataDesign">[Optinal parameter] to design body(data)</param>
        /// <param name="rightToLeft">[Default is false] Right to left layout</param>
        public static void CreateExcel(Stream excelFileStream, string sheetName, List<string> headers, List<string[]> data, ExcelDesign titleDesign = null, ExcelDesign dataDesign = null, bool rightToLeft = false)
        {
            WriteExcel(excelFileStream,
                new List<ExcelSheetModel> { new ExcelSheetModel { SheetName = sheetName, Headers = headers, Data = data } },
                new ExcelWriteOptions { TitleDesign = titleDesign, DataDesign = dataDesign, RightToLeft = rightToLeft });
        }

        /// <summary>
        /// Create modern excel file (xlsx) with multiple sheet
        /// </summary>
        /// <param name="excelFileStream">Output stream to write excel file on it (left open)</param>
        /// <param name="sheetData">list of Excel sheet name with rows data and headers</param>
        /// <param name="titleDesign">[Optinal parameter] to design title(header)
        /// with default value:
        ///  Font.Bold = true;
        ///  Font.Color.SetColor(Color.White);
        ///  Border.Right.Style = ExcelBorderStyle.Thick;
        ///  Border.Top.Style = ExcelBorderStyle.Thick;
        ///  Border.Left.Style = ExcelBorderStyle.Thick;
        ///  Border.Bottom.Style = ExcelBorderStyle.Thick;
        ///  Fill.PatternType = ExcelFillStyle.Solid;
        ///  Fill.BackgroundColor.SetColor(Color.DarkBlue);
        /// </param>
        /// <param name="dataDesign">[Optinal parameter] to design body(data)</param>
        /// <param name="rightToLeft">[Default is false] Right to left layout</param>
        public static void CreateExcel(Stream excelFileStream, List<ExcelSheetModel> sheetData, ExcelDesign titleDesign = null, ExcelDesign dataDesign = null, bool rightToLeft = false)
        {
            WriteExcel(excelFileStream, sheetData,
                new ExcelWriteOptions { TitleDesign = titleDesign, DataDesign = dataDesign, RightToLeft = rightToLeft });
        }

        /// <summary>
        /// Create an xlsx file with one sheet of typed rows: numbers, dates and booleans keep their type.
        /// </summary>
        /// <param name="excelFileStream">Output stream to write excel file on it (left open)</param>
        /// <param name="sheetName">Excel sheet name</param>
        /// <param name="headers">Title first row</param>
        /// <param name="rows">Rows; each value is written with its own type</param>
        /// <param name="options">designs, layout and document properties (optional)</param>
        public static void WriteExcel(Stream excelFileStream, string sheetName, List<string> headers, List<object[]> rows, ExcelWriteOptions options = null)
        {
            WriteExcel(excelFileStream,
                new List<ExcelSheetModel> { new ExcelSheetModel { SheetName = sheetName, Headers = headers, Rows = rows } },
                options);
        }

        /// <summary>
        /// Create an xlsx file with one or more sheets.
        /// </summary>
        /// <param name="excelFileStream">Output stream to write excel file on it (left open)</param>
        /// <param name="sheetData">sheets; each uses <see cref="ExcelSheetModel.Rows"/> when set, otherwise <see cref="ExcelSheetModel.Data"/></param>
        /// <param name="options">designs, layout and document properties (optional)</param>
        public static void WriteExcel(Stream excelFileStream, List<ExcelSheetModel> sheetData, ExcelWriteOptions options = null)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }
            if (sheetData == null)
            {
                throw new ArgumentNullException(nameof(sheetData));
            }
            options ??= new ExcelWriteOptions();

            using var excelPackage = GetPackage(excelFileStream);
            foreach (var item in sheetData)
            {
                WriteSheetDataToExcelPackage(excelPackage, item, options);
            }

            var title = string.Join(" | ", sheetData.Select(s => s.SheetName));
            if (title.Length > 200)
            {
                title = title.Substring(0, 200) + " ...";
            }
            ApplyProperties(excelPackage, options.Properties, title);
            excelPackage.Save();
        }


        /// <summary>
        /// Get Cells from excel file
        /// </summary>
        /// <param name="excelFileStream"> must be Read/Write stream and still open (left open)</param>
        /// <param name="skipFirstRow">first row is title [default is false]</param>
        /// <param name="removeEmptyRows">remove empty rows from result cells [default is true]</param>
        /// <returns> List of string array, each item in list is row and it's array is the coulmn for every cell; empty for an empty sheet </returns>
        public static List<string[]> GetCells(Stream excelFileStream, bool skipFirstRow = false, bool removeEmptyRows = true)
        {
            using var excelPackage = GetPackage(excelFileStream);
            List<string[]> pageStrings = new List<string[]>();
            var osheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
            var end = osheet?.Dimension?.End;
            if (end == null)
            {
                return pageStrings;
            }

            var totalRowsXLSX = end.Row;
            int totalcols = end.Column;
            for (int i = skipFirstRow ? 2 : 1; i <= totalRowsXLSX; i++)
            {
                string[] subStrings = new string[totalcols];
                for (int j = 1; j <= totalcols; j++)
                {
                    subStrings[j - 1] = (osheet.Cells[i, j].Value == null) ? string.Empty : osheet.Cells[i, j].Value.ToString();
                }
                if (!removeEmptyRows || subStrings.Any(x => !string.IsNullOrEmpty(x)))
                {
                    pageStrings.Add(subStrings);
                }
            }
            return pageStrings;
        }

        #endregion

        #region Helper

        private static ExcelPackage GetPackage(Stream excelFileStream)
        {
            if (ExcelPackage.LicenseContext == null
                && UseNonCommercialLicenseWhenUnset
                && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("EPPlusLicenseContext")))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            }
            return new ExcelPackage(excelFileStream);
        }

        private static void ApplyProperties(ExcelPackage excelPackage, ExcelDocumentProperties properties, string defaultTitle)
        {
            var target = excelPackage.Workbook.Properties;
            target.Title = properties?.Title ?? defaultTitle;
            if (properties == null)
            {
                return;
            }
            if (properties.Author != null) target.Author = properties.Author;
            if (properties.Company != null) target.Company = properties.Company;
            if (properties.Comments != null) target.Comments = properties.Comments;
            if (properties.Application != null) target.Application = properties.Application;
            if (properties.Custom != null)
            {
                foreach (var pair in properties.Custom)
                {
                    target.SetCustomPropertyValue(pair.Key, pair.Value);
                }
            }
        }

        private static void WriteSheetDataToExcelPackage(ExcelPackage excelPackage, ExcelSheetModel sheet, ExcelWriteOptions options)
        {
            var sheetName = sheet.SheetName;
            var headers = sheet.Headers ?? new List<string>();
            IReadOnlyList<object[]> rows = sheet.Rows
                ?? (IReadOnlyList<object[]>)(sheet.Data ?? new List<string[]>()).Select(r => r?.Cast<object>().ToArray()).ToList();

            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            //Add the headers
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            int columnCount = headers.Count;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row == null)
                {
                    continue;
                }
                columnCount = Math.Max(columnCount, row.Length);
                for (int q = 0; q < row.Length; q++)
                {
                    SetCellValue(worksheet.Cells[i + 2, q + 1], row[q], options);
                }
            }

            //Ok now format the header
            if (headers.Count > 0)
            {
                using var range = worksheet.Cells[1, 1, 1, headers.Count];
                if (options.TitleDesign is null)
                {
                    range.Style.Font.Bold = true;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thick;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thick;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                }
                else
                {
                    ApplyDesign(range, options.TitleDesign);
                }

                range.AutoFitColumns(0);
            }

            // Format every data row (rows 2 .. Count + 1)
            if (options.DataDesign != null && rows.Count > 0 && columnCount > 0)
            {
                using var range = worksheet.Cells[2, 1, rows.Count + 1, columnCount];
                ApplyDesign(range, options.DataDesign);
                range.AutoFitColumns(0);
            }

            worksheet.View.RightToLeft = options.RightToLeft;
            worksheet.HeaderFooter.OddHeader.CenteredText = "&24&U&\"Arial,Regular Bold\"" + sheetName;
            // add the page number to the footer plus the total number of pages
            worksheet.HeaderFooter.OddFooter.RightAlignedText =
            string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            // add the sheet name to the footer
            worksheet.HeaderFooter.OddFooter.CenteredText = ExcelHeaderFooter.SheetName;
        }

        private static void SetCellValue(ExcelRange cell, object value, ExcelWriteOptions options)
        {
            if (value is DateTimeOffset offset)
            {
                value = offset.DateTime;
            }

            cell.Value = value;
            if (value is DateTime date)
            {
                // EPPlus stores dates as numbers; without a format Excel shows e.g. 45574.5
                cell.Style.Numberformat.Format = date.TimeOfDay == TimeSpan.Zero ? options.DateFormat : options.DateTimeFormat;
            }
        }

        private static void ApplyDesign(ExcelRange range, ExcelDesign design)
        {
            if (design.Font != null)
            {
                range.Style.Font = design.Font;
            }
            if (design.Border != null)
            {
                range.Style.Border = design.Border;
            }
            if (design.Fill != null)
            {
                range.Style.Fill = design.Fill;
            }
            if (design.Numberformat != null)
            {
                range.Style.Numberformat = design.Numberformat;
            }
            range.Style.QuotePrefix = design.QuotePrefix;
            range.Style.TextRotation = design.TextRotation;
            range.Style.Hidden = design.Hidden;
            range.Style.HorizontalAlignment = design.HorizontalAlignment;
            range.Style.Indent = design.Indent;
            range.Style.Locked = design.Locked;
            range.Style.ReadingOrder = design.ReadingOrder;
            range.Style.ShrinkToFit = design.ShrinkToFit;
            range.Style.VerticalAlignment = design.VerticalAlignment;
        }

        #endregion

    }
}
