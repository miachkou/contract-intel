namespace Application.Models;

/// <summary>
/// Represents text extracted from a PDF with per-page breakdown.
/// </summary>
public sealed record PdfTextContent
{
    /// <summary>
    /// The full concatenated text of the entire document.
    /// </summary>
    public required string FullText { get; init; }

    /// <summary>
    /// Per-page text breakdown.
    /// </summary>
    public required IReadOnlyList<PageText> Pages { get; init; }
}

/// <summary>
/// Represents text extracted from a single PDF page.
/// </summary>
public sealed record PageText
{
    /// <summary>
    /// 1-based page number.
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// The text content of this page.
    /// </summary>
    public required string Text { get; init; }
}
