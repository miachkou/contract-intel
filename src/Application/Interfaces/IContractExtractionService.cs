using Application.Models;

namespace Application.Interfaces;

/// <summary>
/// Orchestrates the full extraction and analysis pipeline for contract documents.
/// </summary>
public interface IContractExtractionService
{
    /// <summary>
    /// Processes the latest document for a contract: extracts text, detects clauses,
    /// calculates risk, and updates the database.
    /// </summary>
    /// <param name="contractId">The contract ID to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of extraction results including clause counts and risk score.</returns>
    /// <exception cref="InvalidOperationException">When no documents found or file missing.</exception>
    Task<ExtractionSummary> ProcessContractAsync(Guid contractId, CancellationToken cancellationToken = default);
}
