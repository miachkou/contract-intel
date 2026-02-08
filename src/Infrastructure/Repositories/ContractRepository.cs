using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ContractRepository : Repository<Contract>, IContractRepository
{
    public ContractRepository(ContractIntelDbContext context) : base(context)
    {
    }

    public async Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.Documents)
            .Include(c => c.Clauses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Contract>> SearchAsync(
        string? vendorContains = null,
        decimal? minRiskScore = null,
        DateTime? renewalBefore = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(vendorContains))
        {
            query = query.Where(c => c.Vendor.Contains(vendorContains));
        }

        if (minRiskScore.HasValue)
        {
            query = query.Where(c => c.RiskScore >= minRiskScore.Value);
        }

        if (renewalBefore.HasValue)
        {
            query = query.Where(c => c.RenewalDate <= renewalBefore.Value);
        }

        return await query
            .OrderBy(c => c.RenewalDate)
            .ThenByDescending(c => (double?)c.RiskScore)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Contract>> GetUpcomingRenewalsAsync(
        int daysAhead = 90,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        var today = DateTime.UtcNow;

        return await _dbSet
            .AsNoTracking()
            .Where(c => c.RenewalDate.HasValue
                     && c.RenewalDate >= today
                     && c.RenewalDate <= cutoffDate)
            .OrderBy(c => c.RenewalDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
