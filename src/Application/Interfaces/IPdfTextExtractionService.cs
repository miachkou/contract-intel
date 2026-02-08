using Application.Models;

namespace Application.Interfaces;

public interface IPdfTextExtractionService
{
    /// <summary>
    /// Extracts text from a PDF stream, returning full text and per-page text.
    /// </summary>
    /// <param name="pdfStream">The PDF file stream to read from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>PDF text content with full text and page-by-page breakdown.</returns>
    Task<PdfTextContent> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default);
}
