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
            GeneratePDF(converter, outputPath, htmlInput, null, null);
        }
        public static void CreatePdf(string outputPath, string htmlInput, string headrImageUrl, string footerImageUrl)
        {

            var converter = GetConverter();
            // footer settings
            converter.Options.DisplayFooter = true;
            GeneratePDF(converter, outputPath, htmlInput, headrImageUrl, footerImageUrl);
        }
        public static void CreatePdf(string outputPath, string htmlInput, string headrImageUrl, string footerImageUrl, HtmlToPdfOptions options)
        {

            var converter = GetConverter(options);
            // footer settings
            converter.Options.DisplayFooter = options.DisplayFooter;
            GeneratePDF(converter, outputPath, htmlInput, headrImageUrl, footerImageUrl);
        }
        #endregion

        #region Helper
        private static HtmlToPdf GetConverter(HtmlToPdfOptions options = null)
        {
            var converter = new HtmlToPdf();
            if (options != null)
            {
                converter.Options.PdfPageSize = options.PdfPageSize;
                converter.Options.PdfPageOrientation = options.PdfPageOrientation;
                converter.Options.WebPageWidth = options.WebPageWidth;
                converter.Options.WebPageHeight = options.WebPageHeight;
                converter.Options.MarginTop = options.MarginTop;
            }
            else
            {
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.WebPageWidth = 1024;
                converter.Options.WebPageHeight = 0;
                converter.Options.MarginTop = 20;
            }
            return converter;

        }
        private static void GeneratePDF(HtmlToPdf converter, string outputPath, string htmlInput, string headrImageUrl, string footerImageUrl)
        {
            converter.Footer.DisplayOnFirstPage = true;
            converter.Footer.DisplayOnOddPages = true;
            converter.Footer.DisplayOnEvenPages = true;
            converter.Footer.Height = 75;
            if (string.IsNullOrEmpty(headrImageUrl))
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
                var headerImage = $"<img height='100' width='100%' src='{headrImageUrl}' />";
                // add some html content to the header
                var headerHtml = new PdfHtmlSection(headerImage, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerHtml);
            }

            if (!string.IsNullOrEmpty(footerImageUrl))
            {
                var footerImage = $"<img height='100' width='100%' src='{footerImageUrl}' />";
                // add some html content to the footer
                PdfHtmlSection footerHtml = new PdfHtmlSection(footerImage, "");
                footerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Footer.Add(footerHtml);
            }

            // page numbers can be added using a PdfTextSection object
            var text = new PdfTextSection(10, 10, "Page: {page_number} of {total_pages}  ", new System.Drawing.Font("Arial", 8));
            text.HorizontalAlign = PdfTextHorizontalAlign.Right;
            converter.Footer.Add(text);

            PdfDocument doc = converter.ConvertHtmlString(htmlInput, "");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            // save pdf document
            doc.Save(outputPath);
            // close pdf document
            doc.Close();
        }
        #endregion



    }
}
