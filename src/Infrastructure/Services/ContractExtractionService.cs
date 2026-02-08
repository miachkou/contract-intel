using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Orchestrates contract document extraction and analysis pipeline.
/// </summary>
public sealed class ContractExtractionService : IContractExtractionService
{
    private readonly ContractIntelDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IPdfTextExtractionService _textExtractor;
    private readonly IClauseDetectionService _clauseDetector;
    private readonly IRiskScoringService _riskScoring;
    private readonly ILogger<ContractExtractionService> _logger;

    public ContractExtractionService(
        ContractIntelDbContext context,
        IFileStorageService fileStorage,
        IPdfTextExtractionService textExtractor,
        IClauseDetectionService clauseDetector,
        IRiskScoringService riskScoring,
        ILogger<ContractExtractionService> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _textExtractor = textExtractor;
        _clauseDetector = clauseDetector;
        _riskScoring = riskScoring;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ExtractionSummary> ProcessContractAsync(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting extraction pipeline for contract {ContractId}", contractId);

        // 1. Load contract with documents
        var contract = await _context.Contracts
            .Include(c => c.Documents)
            .Include(c => c.Clauses)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found.");
        }

        // 2. Get latest document
        var latestDocument = contract.Documents
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefault();

        if (latestDocument == null)
        {
            throw new InvalidOperationException($"No documents found for contract {contractId}.");
        }

        _logger.LogInformation(
            "Processing document {DocumentId} ({FileName}) for contract {ContractId}",
            latestDocument.Id,
            latestDocument.FileName,
            contractId);

        // 3. Extract text from file
        PdfTextContent textContent;
        try
        {
            if (!await _fileStorage.FileExistsAsync(latestDocument.FilePath, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"File not found: {latestDocument.FilePath}");
            }

            textContent = await _textExtractor.ExtractTextAsync(
                latestDocument.FilePath,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(textContent.FullText))
            {
                throw new InvalidOperationException(
                    $"No text extracted from document {latestDocument.FileName}. " +
                    "File may be empty, corrupted, or image-based PDF.");
            }

            _logger.LogInformation(
                "Extracted {CharCount} characters from {PageCount} pages",
                textContent.FullText.Length,
                textContent.Pages.Count);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to extract text from {FilePath}", latestDocument.FilePath);
            throw new InvalidOperationException(
                $"Failed to extract text from document: {ex.Message}",
                ex);
        }

        // 4. Detect clauses
        var detectionResult = await _clauseDetector.DetectClausesAsync(
            textContent.FullText,
            textContent.Pages,
            cancellationToken);

        _logger.LogInformation(
            "Detected {ClauseCount} clauses",
            detectionResult.Clauses.Count);

        // 5. Replace stored clauses in transaction
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Remove old clauses for this contract
            var oldClauses = contract.Clauses.ToList();
            _context.Clauses.RemoveRange(oldClauses);

            // Add new detected clauses
            var newClauses = detectionResult.Clauses.Select(dc => new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contractId,
                DocumentId = latestDocument.Id,
                ClauseType = dc.ClauseType,
                Excerpt = dc.Excerpt,
                Confidence = dc.Confidence,
                PageNumber = dc.PageNumber,
                ExtractedAt = DateTime.UtcNow
            }).ToList();

            _context.Clauses.AddRange(newClauses);

            // 6. Calculate and update risk score
            var riskScore = _riskScoring.CalculateRiskScore(newClauses);
            contract.RiskScore = riskScore;
            contract.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Updated risk score to {RiskScore} for contract {ContractId}",
                riskScore,
                contractId);

            // Save all changes
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Extraction pipeline completed for contract {ContractId}",
                contractId);

            // 7. Build summary
            var clausesByType = newClauses
                .GroupBy(c => c.ClauseType)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ExtractionSummary
            {
                ContractId = contractId,
                DocumentId = latestDocument.Id,
                PageCount = textContent.Pages.Count,
                TotalClauses = newClauses.Count,
                ClausesByType = clausesByType,
                RiskScore = riskScore
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
