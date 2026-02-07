using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ContractIntelDbContext : DbContext
{
    public ContractIntelDbContext(DbContextOptions<ContractIntelDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Clause> Clauses => Set<Clause>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Contract configuration
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Vendor).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.RiskScore).HasPrecision(5, 2);

            // Indexes for common queries
            entity.HasIndex(e => e.RenewalDate).HasDatabaseName("IX_Contracts_RenewalDate");
            entity.HasIndex(e => e.RiskScore).HasDatabaseName("IX_Contracts_RiskScore");
            entity.HasIndex(e => e.Vendor).HasDatabaseName("IX_Contracts_Vendor");
        });

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.MimeType).HasMaxLength(100);

            entity.HasOne(e => e.Contract)
                .WithMany(c => c.Documents)
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ContractId).HasDatabaseName("IX_Documents_ContractId");
        });

        // Clause configuration
        modelBuilder.Entity<Clause>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClauseType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Excerpt).IsRequired();
            entity.Property(e => e.Confidence).HasPrecision(5, 4);

            entity.HasOne(e => e.Contract)
                .WithMany(c => c.Clauses)
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Clauses)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Composite index for common clause queries
            entity.HasIndex(e => new { e.ContractId, e.ClauseType })
                .HasDatabaseName("IX_Clauses_ContractId_ClauseType");
        });
    }
}
