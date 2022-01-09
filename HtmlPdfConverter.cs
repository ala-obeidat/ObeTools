﻿using ObeTools.Model;

using SelectPdf;

using System.IO;

namespace ObeTools
{
    public static class HtmlPdfConverter
    {
        #region Methods
        public static void CreatePdf(string outputPath, string htmlInput)
        {

            var converter = GetConverter();
            converter.Options.DisplayFooter = false;
            var doc = GetPDF(converter, htmlInput, null, null);
            SavePdf(doc, outputPath);
        }
        public static void CreatePdf(string outputPath, string htmlInput, string header, string footer)
        {

            var converter = GetConverter();
            // footer settings
            converter.Options.DisplayFooter = true;
            var doc = GetPDF(converter, htmlInput, header, footer);
            SavePdf(doc, outputPath);
        }
        public static void CreatePdf(string outputPath, string htmlInput, string header, string footer, ConverterOptionModel options)
        {

            var converter = GetConverter(options);
            // footer settings
            converter.Options.DisplayFooter = options.DisplayFooter;
            var doc = GetPDF(converter, htmlInput, header, footer);
            SavePdf(doc, outputPath);
        }


        public static void CreatePdf(Stream outputStream, string htmlInput, bool leaveStreamOpen = false)
        {

            var converter = GetConverter();
            converter.Options.DisplayFooter = false;
            var doc = GetPDF(converter, htmlInput, null, null);
            SavePdf(doc, outputStream, leaveStreamOpen);
        }
        public static void CreatePdf(Stream outputStream, string htmlInput, string header, string footer, bool leaveStreamOpen = false)
        {

            var converter = GetConverter();
            // footer settings
            converter.Options.DisplayFooter = true;
            var doc = GetPDF(converter, htmlInput, header, footer);
            SavePdf(doc, outputStream, leaveStreamOpen);
        }
        public static void CreatePdf(Stream outputStream, string htmlInput, string header, string footer, ConverterOptionModel options, bool leaveStreamOpen = false)
        {

            var converter = GetConverter(options);
            // footer settings
            converter.Options.DisplayFooter = options.DisplayFooter;
            var doc = GetPDF(converter, htmlInput, header, footer);
            SavePdf(doc, outputStream, leaveStreamOpen);
        }

        #endregion

        #region Helper
        private static HtmlToPdf GetConverter(ConverterOptionModel options = null)
        {
            var converter = new HtmlToPdf();
            if (options != null)
            {
                converter.Options.PdfPageSize = options.PdfPageSize;
                converter.Options.PdfPageOrientation = options.PdfPageOrientation;
                converter.Options.WebPageWidth = options.WebPageWidth;
                converter.Options.WebPageHeight = options.WebPageHeight;
                converter.Options.MarginTop = options.MarginTop;
                converter.Options.MarginBottom = options.MarginBottom;
                converter.Options.MarginLeft = options.MarginLeft;
                converter.Options.MarginRight = options.MarginRight;
            }
            else
            {
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.WebPageWidth = 1024;
                converter.Options.WebPageHeight = 0;
                converter.Options.MarginTop = 20;
                converter.Options.MarginBottom = 20;
                converter.Options.MarginLeft = 50;
                converter.Options.MarginRight = 50;
            }
            return converter;

        }

        private static void SavePdf(PdfDocument doc, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            // save pdf document
            doc.Save(outputPath);
            // close pdf document
            doc.Close();
        }
        private static void SavePdf(PdfDocument doc, Stream outputStream, bool leaveStreamOpen)
        {

            // save pdf document
            doc.Save(outputStream);

            // close pdf document
            if (leaveStreamOpen)
            {
                doc.DetachStream();
            }
            else
            {
                doc.Close();
            }
        }
        private static PdfDocument GetPDF(HtmlToPdf converter, string htmlInput, string header, string footer)
        {
            converter.Footer.DisplayOnFirstPage = true;
            converter.Footer.DisplayOnOddPages = true;
            converter.Footer.DisplayOnEvenPages = true;
            converter.Footer.Height = 75;
            if (string.IsNullOrEmpty(header))
            {
                converter.Options.DisplayHeader = false;
            }
            else
            {
                // header settings
                converter.Options.DisplayHeader = true;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.Height = 75;
                // add some html content to the header
                var headerHtml = new PdfHtmlSection(header, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerHtml);
            }

            if (!string.IsNullOrEmpty(footer))
            {
                // add some html content to the footer
                PdfHtmlSection footerHtml = new PdfHtmlSection(footer, "");
                footerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Footer.Add(footerHtml);
            }

            // page numbers can be added using a PdfTextSection object
            var text = new PdfTextSection(10, 10, "Page: {page_number} of {total_pages}  ", new System.Drawing.Font("Arial", 8))
            {
                HorizontalAlign = PdfTextHorizontalAlign.Right
            };
            converter.Footer.Add(text);

            return converter.ConvertHtmlString(htmlInput, "");
        }

        #endregion



    }
}
