namespace WebApi.Models;

public record CreateContractRequest
{
    public required string Title { get; init; }
    public required string Vendor { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public DateTime? RenewalDate { get; init; }
    public string? Status { get; init; }
}

public record ContractResponse
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Vendor { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public DateTime? RenewalDate { get; init; }
    public decimal? RiskScore { get; init; }
    public string? Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

public record DocumentResponse
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public string? MimeType { get; init; }
    public required DateTime UploadedAt { get; init; }
}

public record ClauseResponse
{
    public required Guid Id { get; init; }
    public required string ClauseType { get; init; }
    public required string Excerpt { get; init; }
    public decimal? Confidence { get; init; }
    public int? PageNumber { get; init; }
    public required DateTime ExtractedAt { get; init; }
}
