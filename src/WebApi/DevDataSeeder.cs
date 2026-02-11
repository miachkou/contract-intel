using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

/// <summary>
/// Development-only data seeder for testing and demo purposes.
/// </summary>
public static class DevDataSeeder
{
    public static async Task SeedAsync(ContractIntelDbContext dbContext, ILogger logger)
    {
        // Check if data already exists (idempotency)
        if (await dbContext.Contracts.AnyAsync())
        {
            logger.LogInformation("Seed data already exists. Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding development data...");

        var now = DateTime.UtcNow;
        var contracts = new List<Contract>();

        // Contract 1: Upcoming renewal, low risk
        var contract1 = new Contract
        {
            Id = Guid.NewGuid(),
            Title = "Cloud Infrastructure Services Agreement",
            Vendor = "Acme Cloud Services",
            StartDate = now.AddYears(-2),
            EndDate = now.AddMonths(3),
            RenewalDate = now.AddDays(60), // 60 days from now
            RiskScore = 15.5m,
            Status = "Active",
            CreatedAt = now.AddYears(-2),
            UpdatedAt = now
        };
        contracts.Add(contract1);

        // Contract 2: Recent renewal, medium risk
        var contract2 = new Contract
        {
            Id = Guid.NewGuid(),
            Title = "Software License and Support",
            Vendor = "TechVendor Inc",
            StartDate = now.AddYears(-1),
            EndDate = now.AddYears(2),
            RenewalDate = now.AddYears(1),
            RiskScore = 42.0m,
            Status = "Active",
            CreatedAt = now.AddYears(-1),
            UpdatedAt = now
        };
        contracts.Add(contract2);

        // Contract 3: Expiring soon, high risk
        var contract3 = new Contract
        {
            Id = Guid.NewGuid(),
            Title = "Professional Services Master Agreement",
            Vendor = "Global Consulting Partners",
            StartDate = now.AddYears(-3),
            EndDate = now.AddDays(30),
            RenewalDate = now.AddDays(15), // 15 days from now
            RiskScore = 68.5m,
            Status = "Active",
            CreatedAt = now.AddYears(-3),
            UpdatedAt = now
        };
        contracts.Add(contract3);

        // Contract 4: Long-term contract, no immediate renewal
        var contract4 = new Contract
        {
            Id = Guid.NewGuid(),
            Title = "Data Center Lease Agreement",
            Vendor = "Prime Real Estate Holdings",
            StartDate = now.AddYears(-1),
            EndDate = now.AddYears(4),
            RenewalDate = now.AddYears(3).AddMonths(6),
            RiskScore = 22.0m,
            Status = "Active",
            CreatedAt = now.AddYears(-1),
            UpdatedAt = now
        };
        contracts.Add(contract4);

        // Contract 5: Recently signed, minimal risk
        var contract5 = new Contract
        {
            Id = Guid.NewGuid(),
            Title = "Marketing Services Agreement",
            Vendor = "Digital Marketing Solutions",
            StartDate = now.AddMonths(-2),
            EndDate = now.AddMonths(10),
            RenewalDate = now.AddMonths(9),
            RiskScore = 8.0m,
            Status = "Active",
            CreatedAt = now.AddMonths(-2),
            UpdatedAt = now
        };
        contracts.Add(contract5);

        await dbContext.Contracts.AddRangeAsync(contracts);

        // Add sample clauses to contracts
        var clauses = new List<Clause>();

        // Clauses for Contract 1
        clauses.AddRange(new[]
        {
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract1.Id,
                ClauseType = "Termination",
                Excerpt = "Either party may terminate this agreement with 90 days written notice.",
                Confidence = 0.95m,
                Analysis = "Standard termination clause with reasonable notice period.",
                ExtractedAt = now.AddDays(-5),
                ApprovedBy = "System",
                ApprovedAt = now.AddDays(-4)
            },
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract1.Id,
                ClauseType = "Liability Cap",
                Excerpt = "Provider's liability shall not exceed the fees paid in the twelve months preceding the claim.",
                Confidence = 0.88m,
                Analysis = "Limited liability clause may pose risk for critical services.",
                ExtractedAt = now.AddDays(-5)
            }
        });

        // Clauses for Contract 2
        clauses.AddRange(new[]
        {
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract2.Id,
                ClauseType = "Auto-Renewal",
                Excerpt = "This agreement automatically renews for successive one-year terms unless terminated with 60 days notice.",
                Confidence = 0.92m,
                Analysis = "Auto-renewal clause requires attention to avoid unintended renewal.",
                ExtractedAt = now.AddDays(-180)
            },
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract2.Id,
                ClauseType = "Price Increase",
                Excerpt = "Vendor reserves the right to increase prices by up to 10% annually upon renewal.",
                Confidence = 0.85m,
                Analysis = "Uncapped price increases may impact budget planning.",
                ExtractedAt = now.AddDays(-180)
            },
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract2.Id,
                ClauseType = "Data Ownership",
                Excerpt = "Customer retains all rights to data. Vendor may use anonymized data for product improvements.",
                Confidence = 0.90m,
                Analysis = "Data ownership clearly defined with reasonable anonymization clause.",
                ExtractedAt = now.AddDays(-180),
                ApprovedBy = "System",
                ApprovedAt = now.AddDays(-179)
            }
        });

        // Clauses for Contract 3
        clauses.AddRange(new[]
        {
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract3.Id,
                ClauseType = "Indemnification",
                Excerpt = "Customer agrees to indemnify and hold harmless Provider from any third-party claims arising from Customer's use of services.",
                Confidence = 0.78m,
                Analysis = "Broad indemnification clause may expose organization to significant liability.",
                ExtractedAt = now.AddDays(-800)
            },
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract3.Id,
                ClauseType = "Non-Compete",
                Excerpt = "Customer agrees not to engage competitors of Provider for similar services during the term and for 12 months thereafter.",
                Confidence = 0.82m,
                Analysis = "Post-term non-compete may restrict future vendor selection.",
                ExtractedAt = now.AddDays(-800)
            }
        });

        // Clauses for Contract 4
        clauses.AddRange(new[]
        {
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract4.Id,
                ClauseType = "Force Majeure",
                Excerpt = "Neither party shall be liable for failure to perform due to causes beyond reasonable control.",
                Confidence = 0.94m,
                Analysis = "Standard force majeure clause provides balanced protection.",
                ExtractedAt = now.AddDays(-200),
                ApprovedBy = "System",
                ApprovedAt = now.AddDays(-199)
            }
        });

        // Clauses for Contract 5
        clauses.AddRange(new[]
        {
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract5.Id,
                ClauseType = "Confidentiality",
                Excerpt = "Both parties agree to maintain confidentiality of proprietary information for 3 years post-termination.",
                Confidence = 0.91m,
                Analysis = "Standard confidentiality terms with reasonable duration.",
                ExtractedAt = now.AddDays(-50),
                ApprovedBy = "System",
                ApprovedAt = now.AddDays(-49)
            },
            new Clause
            {
                Id = Guid.NewGuid(),
                ContractId = contract5.Id,
                ClauseType = "Payment Terms",
                Excerpt = "Invoices are due within 30 days of receipt. Late payments subject to 1.5% monthly interest.",
                Confidence = 0.96m,
                Analysis = "Standard payment terms with typical late payment penalty.",
                ExtractedAt = now.AddDays(-50)
            }
        });

        await dbContext.Clauses.AddRangeAsync(clauses);

        // Add sample document metadata (no actual files)
        var documents = new List<Document>
        {
            new Document
            {
                Id = Guid.NewGuid(),
                ContractId = contract1.Id,
                FileName = "acme-cloud-services-2024.pdf",
                FilePath = "/dev/uploads/acme-cloud-services-2024.pdf",
                FileSize = 245760,
                MimeType = "application/pdf",
                UploadedAt = now.AddDays(-5)
            },
            new Document
            {
                Id = Guid.NewGuid(),
                ContractId = contract2.Id,
                FileName = "techvendor-license-agreement.pdf",
                FilePath = "/dev/uploads/techvendor-license-agreement.pdf",
                FileSize = 512000,
                MimeType = "application/pdf",
                UploadedAt = now.AddDays(-180)
            },
            new Document
            {
                Id = Guid.NewGuid(),
                ContractId = contract3.Id,
                FileName = "global-consulting-master-agreement.pdf",
                FilePath = "/dev/uploads/global-consulting-master-agreement.pdf",
                FileSize = 1048576,
                MimeType = "application/pdf",
                UploadedAt = now.AddDays(-800)
            }
        };

        await dbContext.Documents.AddRangeAsync(documents);

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "✅ Seed data inserted: {ContractCount} contracts, {ClauseCount} clauses, {DocumentCount} documents",
            contracts.Count,
            clauses.Count,
            documents.Count);
    }
}
