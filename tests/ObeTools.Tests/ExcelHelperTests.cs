using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ObeTools.Model;

using OfficeOpenXml;
using OfficeOpenXml.Style;

using Xunit;

namespace ObeTools.Tests
{
    [CollectionDefinition("Excel", DisableParallelization = true)]
    public class ExcelCollection
    {
    }

    [Collection("Excel")]
    public class ExcelHelperTests
    {
        private static readonly List<string> Headers = new List<string> { "Name", "Amount" };

        private static List<string[]> Data(int rows) =>
            Enumerable.Range(1, rows).Select(i => new[] { $"n{i}", i.ToString() }).ToList();

        private static ExcelPackage Open(Stream stream)
        {
            stream.Position = 0;
            return new ExcelPackage(stream);
        }

        [Fact]
        public void DataDesign_StylesEveryDataRow()
        {
            using var stream = new MemoryStream();
            var design = new ExcelDesign { HorizontalAlignment = ExcelHorizontalAlignment.Center };

            ExcelHelper.CreateExcel(stream, "S", Headers, Data(5), dataDesign: design);

            using var package = Open(stream);
            var sheet = package.Workbook.Worksheets["S"];
            for (int row = 2; row <= 6; row++)
            {
                Assert.Equal(ExcelHorizontalAlignment.Center, sheet.Cells[row, 2].Style.HorizontalAlignment);
            }
        }

        [Fact]
        public void ZeroRows_WithDataDesign_DoesNotThrow()
        {
            using var stream = new MemoryStream();
            ExcelHelper.CreateExcel(stream, "S", Headers, new List<string[]>(), dataDesign: new ExcelDesign());
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public void NoHeaders_DoesNotThrow()
        {
            using var stream = new MemoryStream();
            ExcelHelper.CreateExcel(stream, "S", new List<string>(), Data(2));
            Assert.Equal(2, ExcelHelper.GetCells(stream).Count);
        }

        [Fact]
        public void CreateThenRead_RoundTrips_AndLeavesTheStreamOpen()
        {
            using var stream = new MemoryStream();
            ExcelHelper.CreateExcel(stream, "S", Headers, Data(3));

            Assert.True(stream.CanRead);
            var cells = ExcelHelper.GetCells(stream, skipFirstRow: true);
            Assert.True(stream.CanRead);
            Assert.Equal(new[] { "n3", "3" }, cells[2]);
        }

        [Fact]
        public void GetCells_EmptySheet_ReturnsEmpty()
        {
            using var stream = new MemoryStream();
            ExcelHelper.WriteExcel(stream, new List<ExcelSheetModel> { new ExcelSheetModel { SheetName = "Empty" } });

            Assert.Empty(ExcelHelper.GetCells(stream));
        }

        [Fact]
        public void TypedRows_KeepNumbersAndDates()
        {
            using var stream = new MemoryStream();
            var date = new DateTime(2026, 10, 9);
            ExcelHelper.WriteExcel(stream, "S", Headers, new List<object[]> { new object[] { "a", 12.5m, date, true } });

            using var package = Open(stream);
            var sheet = package.Workbook.Worksheets["S"];
            Assert.Equal(12.5, Convert.ToDouble(sheet.Cells[2, 2].Value));
            Assert.IsNotType<string>(sheet.Cells[2, 2].Value);
            Assert.Equal("yyyy-mm-dd", sheet.Cells[2, 3].Style.Numberformat.Format);
            Assert.Equal(true, sheet.Cells[2, 4].Value);
        }

        [Fact]
        public void TextRows_AreNotRichText()
        {
            using var stream = new MemoryStream();
            ExcelHelper.CreateExcel(stream, "S", Headers, Data(1));

            using var package = Open(stream);
            Assert.False(package.Workbook.Worksheets["S"].Cells[2, 1].IsRichText);
        }

        [Fact]
        public void DefaultProperties_CarryNoPersonalMetadata()
        {
            using var stream = new MemoryStream();
            ExcelHelper.CreateExcel(stream, "Orders", Headers, Data(1));

            using var package = Open(stream);
            var props = package.Workbook.Properties;
            Assert.Equal("Orders", props.Title);
            Assert.True(string.IsNullOrEmpty(props.Author));
            Assert.True(string.IsNullOrEmpty(props.Company));
        }

        [Fact]
        public void Properties_CanBeSet()
        {
            using var stream = new MemoryStream();
            ExcelHelper.WriteExcel(stream, new List<ExcelSheetModel> { new ExcelSheetModel { SheetName = "A", Headers = Headers, Data = Data(1) } },
                new ExcelWriteOptions
                {
                    Properties = new ExcelDocumentProperties
                    {
                        Author = "Finance",
                        Company = "Ertikaz",
                        Custom = new Dictionary<string, string> { { "Batch", "42" } }
                    }
                });

            using var package = Open(stream);
            Assert.Equal("Finance", package.Workbook.Properties.Author);
            Assert.Equal("Ertikaz", package.Workbook.Properties.Company);
            Assert.Equal("42", package.Workbook.Properties.GetCustomPropertyValue("Batch"));
        }

        [Fact]
        public void MultipleSheets()
        {
            using var stream = new MemoryStream();
            ExcelHelper.CreateExcel(stream, new List<ExcelSheetModel>
            {
                new ExcelSheetModel { SheetName = "A", Headers = Headers, Data = Data(1) },
                new ExcelSheetModel { SheetName = "B", Headers = Headers, Data = Data(2) }
            }, rightToLeft: true);

            using var package = Open(stream);
            Assert.Equal(2, package.Workbook.Worksheets.Count);
            Assert.True(package.Workbook.Worksheets["B"].View.RightToLeft);
            Assert.Equal("A | B", package.Workbook.Properties.Title);
        }

        [Fact]
        public void HostLicence_IsNotOverwritten()
        {
            var previous = ExcelPackage.LicenseContext;
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                using var stream = new MemoryStream();
                ExcelHelper.CreateExcel(stream, "S", Headers, Data(1));
                Assert.Equal(LicenseContext.Commercial, ExcelPackage.LicenseContext);
            }
            finally
            {
                ExcelPackage.LicenseContext = previous;
            }
        }
    }
}
