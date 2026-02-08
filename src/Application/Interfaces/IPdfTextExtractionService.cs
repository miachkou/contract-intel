using Application.Models;

namespace Application.Interfaces;

/// <summary>
/// Service for extracting text content from PDF files.
/// </summary>
public interface IPdfTextExtractionService
{
    /// <summary>
    /// Extracts text from a PDF file with per-page breakdown.
    /// </summary>
    /// <param name="filePath">The absolute path to the PDF file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The extracted text content with page information.</returns>
    Task<PdfTextContent> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);
}
