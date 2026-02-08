namespace Application.Models;

public class DetectedClause
{
    public string ClauseType { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public int? PageNumber { get; set; }
}

public class ClauseDetectionResult
{
    public IReadOnlyList<DetectedClause> Clauses { get; set; } = Array.Empty<DetectedClause>();
    public int TotalClauses => Clauses.Count;
}
