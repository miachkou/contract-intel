using Application.Interfaces;
using Application.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Infrastructure.Services;

public class PdfPigTextExtractionService : IPdfTextExtractionService
{
    private readonly ILogger<PdfPigTextExtractionService> _logger;

    public PdfPigTextExtractionService(ILogger<PdfPigTextExtractionService> logger)
    {
        _logger = logger;
    }

    public async Task<PdfTextContent> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        if (pdfStream == null || !pdfStream.CanRead)
        {
            _logger.LogWarning("Invalid PDF stream provided");
            return new PdfTextContent();
        }

        try
        {
            return await Task.Run(() => ExtractTextInternal(pdfStream), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error extracting text from PDF");
            return new PdfTextContent();
        }
    }

    private PdfTextContent ExtractTextInternal(Stream pdfStream)
    {
        try
        {
            using var document = PdfDocument.Open(pdfStream);

            var pages = new List<PageText>();
            var fullTextBuilder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                var pageText = ExtractPageText(page);

                pages.Add(new PageText
                {
                    PageNumber = page.Number,
                    Text = pageText
                });

                fullTextBuilder.AppendLine(pageText);
            }

            return new PdfTextContent
            {
                FullText = NormalizeText(fullTextBuilder.ToString()),
                Pages = pages
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open or read PDF document");
            return new PdfTextContent();
        }
    }

    private string ExtractPageText(Page page)
    {
        try
        {
            var text = page.Text;
            return NormalizeText(text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract text from page {PageNumber}", page.Number);
            return string.Empty;
        }
    }

    private string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize line breaks
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Replace multiple spaces with single space
        text = System.Text.RegularExpressions.Regex.Replace(text, @" +", " ");

        // Replace multiple newlines with double newline (paragraph breaks)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\n{3,}", "\n\n");

        return text.Trim();
    }
}
