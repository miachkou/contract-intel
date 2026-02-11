using Application.Models;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ContractIntel.Tests;

public class RiskScoringServiceTests
{
    private readonly RiskScoringService _service;
    private readonly RiskScoringOptions _options;

    public RiskScoringServiceTests()
    {
        _options = new RiskScoringOptions
        {
            MissingRequiredClauseWeight = 15,
            AutoRenewShortNoticeWeight = 25,
            ShortNoticeDays = 30,
            RequiredClauses = new[] { "renewal", "termination", "data_protection", "liability_cap", "governing_law" }
        };
        _service = new RiskScoringService(Options.Create(_options), NullLogger<RiskScoringService>.Instance);
    }

    [Fact]
    public void CalculateRiskScore_AllRequiredClausesPresent_ReturnsZero()
    {
        // Arrange
        var clauses = new List<Clause>
        {
            new() { ClauseType = "renewal", Excerpt = "Renewal term is 12 months." },
            new() { ClauseType = "termination", Excerpt = "Termination notice is 60 days." },
            new() { ClauseType = "data_protection", Excerpt = "GDPR compliance required." },
            new() { ClauseType = "liability_cap", Excerpt = "Liability capped at $100,000." },
            new() { ClauseType = "governing_law", Excerpt = "Governed by laws of California." }
        };

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void CalculateRiskScore_OneMissingRequiredClause_ReturnsPenalty()
    {
        // Arrange
        var clauses = new List<Clause>
        {
            new() { ClauseType = "renewal", Excerpt = "Renewal term is 12 months." },
            new() { ClauseType = "termination", Excerpt = "Termination notice is 60 days." },
            new() { ClauseType = "data_protection", Excerpt = "GDPR compliance required." },
            new() { ClauseType = "governing_law", Excerpt = "Governed by laws of California." }
            // Missing liability_cap
        };

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        Assert.Equal(15, score); // 1 missing clause * 15 points
    }

    [Fact]
    public void CalculateRiskScore_MultipleMissingRequiredClauses_ReturnsHigherPenalty()
    {
        // Arrange
        var clauses = new List<Clause>
        {
            new() { ClauseType = "renewal", Excerpt = "Renewal term is 12 months." },
            new() { ClauseType = "termination", Excerpt = "Termination notice is 60 days." }
            // Missing data_protection, liability_cap, governing_law
        };

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        Assert.Equal(45, score); // 3 missing clauses * 15 points
    }

    [Fact]
    public void CalculateRiskScore_AutoRenewalWithShortNotice_AddsExtraPenalty()
    {
        // Arrange
        var clauses = new List<Clause>
        {
            new() { ClauseType = "auto_renewal", Excerpt = "Auto-renews with 15 days notice." },
            new() { ClauseType = "termination", Excerpt = "Termination clause present." },
            new() { ClauseType = "data_protection", Excerpt = "GDPR compliance required." },
            new() { ClauseType = "liability_cap", Excerpt = "Liability capped at $100,000." },
            new() { ClauseType = "governing_law", Excerpt = "Governed by laws of California." }
            // Missing renewal (has auto_renewal instead)
        };

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        // 1 missing clause (renewal) + auto-renewal with short notice (< 30 days)
        Assert.Equal(40, score); // 15 + 25
    }

    [Fact]
    public void CalculateRiskScore_AutoRenewalWithAdequateNotice_NoExtraPenalty()
    {
        // Arrange
        var clauses = new List<Clause>
        {
            new() { ClauseType = "auto_renewal", Excerpt = "Auto-renews with 60 days notice." },
            new() { ClauseType = "termination", Excerpt = "Termination clause present." },
            new() { ClauseType = "data_protection", Excerpt = "GDPR compliance required." },
            new() { ClauseType = "liability_cap", Excerpt = "Liability capped at $100,000." },
            new() { ClauseType = "governing_law", Excerpt = "Governed by laws of California." }
            // Missing renewal (has auto_renewal instead)
        };

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        // Only penalty for missing renewal clause
        Assert.Equal(15, score);
    }

    [Fact]
    public void CalculateRiskScore_AutoRenewalWithoutNoticePeriod_NoExtraPenalty()
    {
        // Arrange
        var clauses = new List<Clause>
        {
            new() { ClauseType = "auto_renewal", Excerpt = "This contract auto-renews annually." },
            new() { ClauseType = "termination", Excerpt = "Termination clause present." },
            new() { ClauseType = "data_protection", Excerpt = "GDPR compliance required." },
            new() { ClauseType = "liability_cap", Excerpt = "Liability capped at $100,000." },
            new() { ClauseType = "governing_law", Excerpt = "Governed by laws of California." }
        };

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        // Only penalty for missing renewal clause; no notice period = no extra penalty
        Assert.Equal(15, score);
    }

    [Fact]
    public void CalculateRiskScore_EmptyClausesList_ReturnsMaxPenalty()
    {
        // Arrange
        var clauses = new List<Clause>();

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        Assert.Equal(75, score); // 5 missing required clauses * 15 points
    }

    [Fact]
    public void CalculateRiskScore_ScoreNormalizedToMax100()
    {
        // Arrange - create scenario with very high score
        var customOptions = new RiskScoringOptions
        {
            MissingRequiredClauseWeight = 50,
            AutoRenewShortNoticeWeight = 60,
            ShortNoticeDays = 30,
            RequiredClauses = new[] { "renewal", "termination", "data_protection" }
        };
        var customService = new RiskScoringService(Options.Create(customOptions), NullLogger<RiskScoringService>.Instance);
        
        var clauses = new List<Clause>
        {
            new() { ClauseType = "auto_renewal", Excerpt = "Auto-renews with 10 days notice." }
            // Missing all 3 required clauses
        };

        // Act
        var score = customService.CalculateRiskScore(clauses);

        // Assert
        // Would be 3 * 50 + 60 = 210, but normalized to 100
        Assert.Equal(100, score);
    }

    [Fact]
    public void CalculateRiskScore_CaseInsensitiveClauseTypes()
    {
        // Arrange
        var clauses = new List<Clause>
        {
            new() { ClauseType = "RENEWAL", Excerpt = "Renewal term is 12 months." },
            new() { ClauseType = "Termination", Excerpt = "Termination notice is 60 days." },
            new() { ClauseType = "Data_Protection", Excerpt = "GDPR compliance required." },
            new() { ClauseType = "liability_CAP", Excerpt = "Liability capped at $100,000." },
            new() { ClauseType = "Governing_Law", Excerpt = "Governed by laws of California." }
        };

        // Act
        var score = _service.CalculateRiskScore(clauses);

        // Assert
        Assert.Equal(0, score);
    }
}
