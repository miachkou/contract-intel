using Application.Models;

namespace Application.Interfaces;

public interface IClauseDetectionService
{
    /// <summary>
    /// Detects clauses in the provided text content.
    /// </summary>
    /// <param name="fullText">The full text to analyze.</param>
    /// <param name="pageTexts">Optional per-page text for page number attribution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detection results with found clauses.</returns>
    Task<ClauseDetectionResult> DetectClausesAsync(
        string fullText,
        IReadOnlyList<PageText>? pageTexts = null,
        CancellationToken cancellationToken = default);
}
