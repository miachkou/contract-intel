using Application.Models;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContractIntel.Tests;

public class ClauseDetectionServiceTests
{
    private readonly RegexClauseDetectionService _service;

    public ClauseDetectionServiceTests()
    {
        _service = new RegexClauseDetectionService(NullLogger<RegexClauseDetectionService>.Instance);
    }

    [Fact]
    public async Task DetectClauses_RenewalClause_IsDetected()
    {
        // Arrange
        var text = "The renewal period is set at 12 months from the expiration date.";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Clauses);
        Assert.Equal("renewal", result.Clauses[0].ClauseType);
        Assert.Contains("renewal", result.Clauses[0].Excerpt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.75m, result.Clauses[0].Confidence);
    }

    [Fact]
    public async Task DetectClauses_AutoRenewalClause_IsDetected()
    {
        // Arrange
        var text = "The contract will auto-renew unless proper notice is given.";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Clauses);
        Assert.Equal("auto_renewal", result.Clauses[0].ClauseType);
        Assert.Contains("auto-renew", result.Clauses[0].Excerpt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.80m, result.Clauses[0].Confidence);
    }

    [Fact]
    public async Task DetectClauses_TerminationClause_IsDetected()
    {
        // Arrange
        var text = "The termination clause requires 30 days notice period.";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Clauses);
        Assert.Equal("termination", result.Clauses[0].ClauseType);
        Assert.Contains("termination", result.Clauses[0].Excerpt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.75m, result.Clauses[0].Confidence);
    }

    [Fact]
    public async Task DetectClauses_DataProtectionClause_IsDetected()
    {
        // Arrange
        var text = "The parties shall comply with all data protection regulations including GDPR requirements.";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Clauses);
        Assert.Equal("data_protection", result.Clauses[0].ClauseType);
        Assert.Contains("data protection", result.Clauses[0].Excerpt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.70m, result.Clauses[0].Confidence);
    }

    [Fact]
    public async Task DetectClauses_LiabilityCapClause_IsDetected()
    {
        // Arrange
        var text = "The liability cap is set at $100,000 for this agreement.";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Clauses);
        Assert.Equal("liability_cap", result.Clauses[0].ClauseType);
        Assert.Contains("liability", result.Clauses[0].Excerpt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.80m, result.Clauses[0].Confidence);
    }

    [Fact]
    public async Task DetectClauses_GoverningLawClause_IsDetected()
    {
        // Arrange
        var text = "This agreement shall be governed by the laws of New York.";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Clauses);
        Assert.Equal("governing_law", result.Clauses[0].ClauseType);
        Assert.Contains("governed", result.Clauses[0].Excerpt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0.75m, result.Clauses[0].Confidence);
    }

    [Fact]
    public async Task DetectClauses_MultipleClausesInText_AllDetected()
    {
        // Arrange
        var text = @"
            This agreement shall be governed by the laws of California.
            The termination clause requires 60 days notice.
            The liability cap is set at $500,000.
        ";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Clauses.Count);
        Assert.Contains(result.Clauses, c => c.ClauseType == "governing_law");
        Assert.Contains(result.Clauses, c => c.ClauseType == "termination");
        Assert.Contains(result.Clauses, c => c.ClauseType == "liability_cap");
    }

    [Fact]
    public async Task DetectClauses_EmptyText_ReturnsEmptyResult()
    {
        // Arrange
        var text = "";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Clauses);
    }

    [Fact]
    public async Task DetectClauses_NullText_ReturnsEmptyResult()
    {
        // Arrange
        string? text = null;

        // Act
        var result = await _service.DetectClausesAsync(text!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Clauses);
    }

    [Fact]
    public async Task DetectClauses_NoMatchingClauses_ReturnsEmptyResult()
    {
        // Arrange
        var text = "This is a simple contract with no special provisions.";

        // Act
        var result = await _service.DetectClausesAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Clauses);
    }
}
