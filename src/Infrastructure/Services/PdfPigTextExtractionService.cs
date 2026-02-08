using Application.Interfaces;
using Application.Models;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace Infrastructure.Services;

/// <summary>
/// PDF text extraction service using PdfPig library.
/// </summary>
public sealed class PdfPigTextExtractionService : IPdfTextExtractionService
{
    private readonly ILogger<PdfPigTextExtractionService> _logger;

    public PdfPigTextExtractionService(ILogger<PdfPigTextExtractionService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PdfTextContent> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("PDF file not found.", filePath);
        }

        _logger.LogInformation("Extracting text from PDF: {FilePath}", filePath);

        try
        {
            // PdfPig operations are synchronous, run on thread pool
            return await Task.Run(() =>
            {
                using var document = PdfDocument.Open(filePath);
                var pages = new List<PageText>();
                var fullTextBuilder = new System.Text.StringBuilder();

                foreach (var page in document.GetPages())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var pageText = page.Text;
                    pages.Add(new PageText
                    {
                        PageNumber = page.Number,
                        Text = pageText
                    });

                    fullTextBuilder.AppendLine(pageText);
                }

                _logger.LogInformation("Successfully extracted text from {PageCount} pages.", pages.Count);

                return new PdfTextContent
                {
                    FullText = fullTextBuilder.ToString(),
                    Pages = pages
                };
            }, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to extract text from PDF: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract text from PDF: {ex.Message}", ex);
        }
    }
}
