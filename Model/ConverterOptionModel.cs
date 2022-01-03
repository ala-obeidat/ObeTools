using SelectPdf;

namespace ObeTools.Model
{
    public class ConverterOptionModel
    {
        //
        // Summary:
        //     The left margin of the pdf document.
        //
        // Remarks:
        //     The margin is specified in points. 1 point is 1/72 inch.
        public int MarginLeft { get; set; }

        //
        // Summary:
        //     The right margin of the pdf document.
        //
        // Remarks:
        //     The margin is specified in points. 1 point is 1/72 inch.
        public int MarginRight { get; set; }

        //
        // Summary:
        //     The top margin of the pdf document.
        //
        // Remarks:
        //     The margin is specified in points. 1 point is 1/72 inch.
        public int MarginTop { get; set; }

        //
        // Summary:
        //     The bottom margin of the pdf document.
        //
        // Remarks:
        //     The margin is specified in points. 1 point is 1/72 inch.
        public int MarginBottom { get; set; }

        //
        // Summary:
        //     This property controls the size of the generated document pages.
        //
        // Remarks:
        //     The default size of the pdf document pages is SelectPdf.PdfPageSize.A4. When
        //     this property is set to SelectPdf.PdfPageSize.Custom, a custom size can be specified
        //     for the generated pdf document pages using the SelectPdf.HtmlToPdfOptions.PdfPageCustomSize
        //     property.
        public PdfPageSize PdfPageSize { get; set; }

        //
        // Summary:
        //     This property controls the page orientation of the generated pdf document pages.
        //
        // Remarks:
        //     The default pdf page orientation is SelectPdf.PdfPageOrientation.Portrait.
        public PdfPageOrientation PdfPageOrientation { get; set; }

        //
        // Summary:
        //     Gets or sets the width of the converted web page as it would appear in the internal
        //     browser used to render the html.
        //
        // Remarks:
        //     The web page width is specified in pixels and the default value is 1024px. The
        //     page width is only an indication of the minimum page width recommended for conversion.
        //     If the content does not fit this width, the converter will automatically resize
        //     the internal browser to fit the whole html content. To avoid this, the SelectPdf.HtmlToPdfOptions.WebPageFixedSize
        //     property needs to be set to true. When SelectPdf.HtmlToPdfOptions.WebPageFixedSize
        //     is true, the web page will be rendered with the specified SelectPdf.HtmlToPdfOptions.WebPageWidth
        //     and SelectPdf.HtmlToPdfOptions.WebPageHeight even though the content might be
        //     truncated.
        //     If SelectPdf.HtmlToPdfOptions.WebPageWidth is set to 0, the converter will automatically
        //     determine the page width, finding the width that will fit the html content.
        //     This property can also be set directly in the constructor of SelectPdf.HtmlToPdf
        //     class.
        public int WebPageWidth { get; set; }


        //
        // Summary:
        //     Gets or sets the height of the converted web page as it would appear in the internal
        //     browser used to render the html.
        //
        // Remarks:
        //     The web page height is specified in pixels and the default value is 0px. This
        //     means that the converter will automatically calculate the page height.
        //     Generally this property does not need to be changed, but there are situations
        //     when the converter cannot calculate correctly the web page height (for example
        //     for web pages with frames) and in that case, SelectPdf.HtmlToPdfOptions.WebPageHeight
        //     needs to be set, otherwise no content might appear in the generated pdf.
        //     Note: If the SelectPdf.HtmlToPdfOptions.WebPageHeight is set, the content that
        //     exceeds this page height is truncated and will not appear in the generated pdf
        //     document. Only using the default 0 value will allow the whole page content to
        //     be rendered all the time in the generated pdf document.
        //     This property can also be set directly in the constructor of SelectPdf.HtmlToPdf
        //     class.
        public int WebPageHeight { get; set; }

        //
        // Summary:
        //     Controls if a custom footer is displayed in the generated pdf document.
        //
        // Remarks:
        //     The footer properties can be customized using the SelectPdf.PdfFooter object
        //     exposed by SelectPdf.HtmlToPdf.Footer property of the SelectPdf.HtmlToPdf converter
        //     class.
        //     Note: The default value of this property is false and the generated pdf document
        //     will not have a custom footer.
        public bool DisplayFooter { get; set; }
    }
}
