namespace Application.Models;

/// <summary>
/// Summary result from contract extraction and analysis.
/// </summary>
public sealed record ExtractionSummary
{
    /// <summary>
    /// Contract ID that was processed.
    /// </summary>
    public required Guid ContractId { get; init; }

    /// <summary>
    /// Document ID that was analyzed.
    /// </summary>
    public required Guid DocumentId { get; init; }

    /// <summary>
    /// Number of pages extracted.
    /// </summary>
    public required int PageCount { get; init; }

    /// <summary>
    /// Total number of clauses detected.
    /// </summary>
    public required int TotalClauses { get; init; }

    /// <summary>
    /// Breakdown of clause counts by type.
    /// </summary>
    public required IReadOnlyDictionary<string, int> ClausesByType { get; init; }

    /// <summary>
    /// Updated risk score for the contract.
    /// </summary>
    public required decimal RiskScore { get; init; }
}
