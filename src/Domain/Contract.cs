namespace Domain.Entities;

public class Contract
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Vendor { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public decimal? RiskScore { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Clause> Clauses { get; set; } = new List<Clause>();
}

public class Document
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public DateTime UploadedAt { get; set; }

    public Contract Contract { get; set; } = null!;
    public ICollection<Clause> Clauses { get; set; } = new List<Clause>();
}

public class Clause
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid? DocumentId { get; set; }
    public required string ClauseType { get; set; }
    public required string Excerpt { get; set; }
    public decimal? Confidence { get; set; }
    public int? PageNumber { get; set; }
    public string? Analysis { get; set; }
    public DateTime ExtractedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Contract Contract { get; set; } = null!;
    public Document? Document { get; set; }
}
