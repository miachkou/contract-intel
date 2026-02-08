using Domain.Entities;

namespace Application.Interfaces;

public interface IContractRepository : IRepository<Contract>
{
    Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Contract>> SearchAsync(
        string? vendorContains = null,
        decimal? minRiskScore = null,
        DateTime? renewalBefore = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Contract>> GetUpcomingRenewalsAsync(
        int daysAhead = 90,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);
}
