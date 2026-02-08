using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

/// <summary>
/// Calculates contract risk scores based on clause analysis.
/// </summary>
public sealed class RiskScoringService : IRiskScoringService
{
    private readonly RiskScoringOptions _options;
    private readonly ILogger<RiskScoringService> _logger;

    // Regex to extract notice period in days from termination/auto-renewal clauses
    private static readonly Regex NoticePeriodRegex = new(
        @"(\d+)[\s-]*days?\s+notice",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public RiskScoringService(
        IOptions<RiskScoringOptions> options,
        ILogger<RiskScoringService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public decimal CalculateRiskScore(IEnumerable<Clause> clauses)
    {
        var clauseList = clauses.ToList();
        var clauseTypes = new HashSet<string>(
            clauseList.Select(c => c.ClauseType.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

        decimal riskScore = 0;

        // Penalty for missing required clauses
        var missingRequired = _options.RequiredClauses
            .Where(required => !clauseTypes.Contains(required))
            .ToList();

        if (missingRequired.Any())
        {
            var penalty = missingRequired.Count * _options.MissingRequiredClauseWeight;
            riskScore += penalty;
            _logger.LogDebug(
                "Missing required clauses: {MissingClauses}. Penalty: {Penalty}",
                string.Join(", ", missingRequired),
                penalty);
        }

        // Extra penalty for auto-renewal with short notice period
        if (clauseTypes.Contains("auto_renewal"))
        {
            var autoRenewalClause = clauseList
                .FirstOrDefault(c => c.ClauseType.Equals("auto_renewal", StringComparison.OrdinalIgnoreCase));

            if (autoRenewalClause != null && HasShortNoticePeriod(autoRenewalClause.Excerpt))
            {
                riskScore += _options.AutoRenewShortNoticeWeight;
                _logger.LogDebug(
                    "Auto-renewal with short notice period detected. Penalty: {Penalty}",
                    _options.AutoRenewShortNoticeWeight);
            }
        }

        // Normalize to 0-100 range
        var normalizedScore = Math.Min(100, Math.Max(0, riskScore));

        _logger.LogInformation(
            "Calculated risk score: {Score} for {ClauseCount} clauses",
            normalizedScore,
            clauseList.Count);

        return normalizedScore;
    }

    private bool HasShortNoticePeriod(string excerpt)
    {
        var match = NoticePeriodRegex.Match(excerpt);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var days))
        {
            return days < _options.ShortNoticeDays;
        }

        // If no explicit notice period found, assume it's concerning
        return false;
    }
}
