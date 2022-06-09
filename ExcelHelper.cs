using OfficeOpenXml;
using OfficeOpenXml.Style;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ObeTools
{
    /// <summary>
    /// Write/read data from/to excel file
    /// </summary>
    public static class ExcelHelper
    {

        #region Methods

        /// <summary>
        /// Create modren excel file (xlsx)
        /// </summary>
        /// <param name="excelFileStream">Output stream to write excel file on it</param>
        /// <param name="sheetName">Excel sheet name</param>
        /// <param name="headers">Title first row</param>
        /// <param name="data">Rows Data</param>
        public static void CreateExcel(Stream excelFileStream, string sheetName, List<string> headers, List<string[]> data)
        {
            var excelPackage = GetPackage(excelFileStream);

            // add a new worksheet to the empty workbook
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            //Add the headers
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            //Add some items...
            for (int i = 0; i < data.Count; i++)
            {
                for (int q = 0; q < data[i].Length; q++)
                {
                    worksheet.Cells[i + 2, q + 1].Value = CheckData(data[i][q]);
                }
            }

            //Ok now format the values;
            using (var range = worksheet.Cells[1, 1, 1, headers.Count])
            {
                range.Style.Font.Bold = true;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thick;
                range.Style.Border.Top.Style = ExcelBorderStyle.Thick;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                range.Style.Font.Color.SetColor(Color.White);

                //Create an autofilter for the range
                range.AutoFitColumns(0);
            }
            worksheet.HeaderFooter.OddHeader.CenteredText = "&24&U&\"Arial,Regular Bold\"" + sheetName;
            // add the page number to the footer plus the total number of pages
            worksheet.HeaderFooter.OddFooter.RightAlignedText =
            string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            // add the sheet name to the footer
            worksheet.HeaderFooter.OddFooter.CenteredText = ExcelHeaderFooter.SheetName;

            // set some document properties
            excelPackage.Workbook.Properties.Title = sheetName;
            excelPackage.Workbook.Properties.Application = "Obe Tools";
            excelPackage.Workbook.Properties.Author = "Ala Obeidat";
            excelPackage.Workbook.Properties.Comments = "This excel file from Obe tools";
            // set some extended property values
            excelPackage.Workbook.Properties.Company = "JITBS";
            // set some custom property values
            excelPackage.Workbook.Properties.SetCustomPropertyValue("Checked by", "Ala Obeidat");
            excelPackage.Workbook.Properties.SetCustomPropertyValue("AssemblyName", "EPPlus");
            //save our new workbook and we are done!
            excelPackage.Save();
        }

        /// <summary>
        /// Get Cells from excel file 
        /// </summary>
        /// <param name="excelFileStream"> must be Read/Write stream and still open</param>
        /// <param name="skipFirstRow">first row is title [default is false]</param>
        /// <param name="removeEmptyRows">remove empty rows from result cells [default is true]</param>
        /// <returns> List of string array, each item in list is row and it's array is the coulmn for every cell </returns>
        public static List<string[]> GetCells(Stream excelFileStream, bool skipFirstRow = false, bool removeEmptyRows = true)
        {
            var excelPackage = GetPackage(excelFileStream);
            List<string[]> pageStrings = new List<string[]>();
            var osheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
            if (osheet != null)
            {
                var totalRowsXLSX = osheet.Dimension.End.Row;
                int totalcols = osheet.Dimension.End.Column;
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
            }
            return pageStrings;
        }

        public static string GetStringValue(string[] data, int index)
        {
            if (data.Length > index)
            {
                return data[index];
            }

            return null;
        }
        public static int? GetIntValue(string[] data, int index)
        {
            if (data.Length > index)
            {
                if (int.TryParse(data[index], out int result))
                {
                    return result;
                }
            }

            return null;
        }
        public static DateTime GetDateValue(string[] data, int index)
        {
            DateTime estimatedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(data[index]))
            {
                DateTime testDate = DateTime.MinValue;
                try
                {
                    if (!DateTime.TryParse(data[index], out testDate))
                    {
                        string[] dateInfo = Regex.Replace(data[index], @"\s", string.Empty).Split('/');
                        if (dateInfo[1].Length == 1)
                        {
                            dateInfo[1] = "0" + dateInfo[1];
                        }

                        if (dateInfo[0].Length == 1)
                        {
                            dateInfo[0] = "0" + dateInfo[0];
                        }

                        int day = Convert.ToInt32(dateInfo[0]);
                        int month = Convert.ToInt32(dateInfo[1]);
                        if (month > 12)
                        {
                            month = Convert.ToInt32(dateInfo[0]);
                            day = Convert.ToInt32(dateInfo[1]);
                        }
                        testDate = new DateTime(Convert.ToInt32(dateInfo[2][..4]), month, day);
                    }
                }
                catch { }
                if (testDate != DateTime.MinValue)
                {
                    estimatedDate = testDate;
                }
            }
            return estimatedDate;
        }
        public static int GetIntEnum(string[] data, int index, Type type)
        {
            if (data.Length > index)
            {
                if (Enum.TryParse(type, Regex.Replace(data[index], @"\s", string.Empty), out object result))
                {
                    if (result != null)
                    {
                        return (int)result;
                    }
                }
            }

            return 0;
        }
        #endregion

        #region Helper
        private static object CheckData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            if (double.TryParse(data, out double number))
            {
                return number;
            }

            return data;
        }
        private static ExcelPackage GetPackage(Stream excelFileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(excelFileStream);
        }
        #endregion

    }
}
