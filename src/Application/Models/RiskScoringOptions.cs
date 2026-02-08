namespace Application.Models;

/// <summary>
/// Configuration options for risk scoring calculation.
/// </summary>
public sealed class RiskScoringOptions
{
    public const string SectionName = "RiskScoring";

    /// <summary>
    /// Penalty points for each missing required clause (default: 15).
    /// </summary>
    public int MissingRequiredClauseWeight { get; set; } = 15;

    /// <summary>
    /// Extra penalty when auto-renewal exists with short notice period (default: 25).
    /// </summary>
    public int AutoRenewShortNoticeWeight { get; set; } = 25;

    /// <summary>
    /// Threshold in days to consider notice period as "short" (default: 30).
    /// </summary>
    public int ShortNoticeDays { get; set; } = 30;

    /// <summary>
    /// Clauses required to be present in a contract.
    /// </summary>
    public string[] RequiredClauses { get; set; } =
    {
        "renewal",
        "termination",
        "data_protection",
        "liability_cap",
        "governing_law"
    };
}
