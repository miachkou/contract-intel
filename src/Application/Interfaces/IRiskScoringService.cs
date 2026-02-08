using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Service for calculating contract risk scores based on detected clauses.
/// </summary>
public interface IRiskScoringService
{
    /// <summary>
    /// Calculates a normalized risk score (0-100) for a contract based on its clauses.
    /// Higher scores indicate higher risk.
    /// </summary>
    /// <param name="clauses">The detected clauses for the contract.</param>
    /// <returns>Risk score from 0 (low risk) to 100 (high risk).</returns>
    decimal CalculateRiskScore(IEnumerable<Clause> clauses);
}
